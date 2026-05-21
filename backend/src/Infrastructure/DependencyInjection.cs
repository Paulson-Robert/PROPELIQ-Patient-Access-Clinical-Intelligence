using Application.Configuration;
using Application.Interfaces;
using Infrastructure.AI;
using Infrastructure.Caching;
using Infrastructure.Auth;
using Infrastructure.Calendar;
using Infrastructure.Data;
using Infrastructure.Data.Options;
using Infrastructure.Jobs;
using Infrastructure.Locking;
using Infrastructure.ML;
using Infrastructure.Notifications;
using Infrastructure.Parsers;
using Infrastructure.RateLimiting;
using Infrastructure.Security;
using Infrastructure.Services;
using Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Fail fast at startup if the PHI encryption key is absent or wrong length.
        // Design-time tools (dotnet ef) skip IHost.StartAsync so migrations still work.
        services.AddSingleton<IValidateOptions<PhiEncryptionOptions>, PhiEncryptionOptionsValidator>();
        services.AddOptions<PhiEncryptionOptions>()
            .BindConfiguration(PhiEncryptionOptions.SectionName)
            .ValidateOnStart();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' not found. " +
                "Set ConnectionStrings__DefaultConnection in appsettings or environment variables.");

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.SetPostgresVersion(16, 0);
                npgsql.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorCodesToAdd: null);
                npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
            }));

        services.AddScoped<IPatientDataDeletionService, PatientDataDeletionService>();
        services.AddScoped<IPasswordHashService, PasswordHashService>();
        services.AddScoped<IAccountLockoutService, AccountLockoutService>();
        services.AddOptions<EmailDeliverySettings>()
            .BindConfiguration(EmailDeliverySettings.SectionName);

        // In-memory cache — required by RedisCacheService as fallback (AC-05).
        services.AddMemoryCache();

        var redisConnectionString = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redisConnectionString))
        {
            // Use RedisConnectionFactory for Upstash TLS + retry configuration (AC-01).
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("RedisConnectionFactory");
                return RedisConnectionFactory.Create(redisConnectionString, logger);
            });

            // Primary cache: Redis with 15-minute sliding expiry + in-memory fallback (AC-02, AC-05).
            services.AddSingleton<ICacheService>(sp =>
                new RedisCacheService(
                    sp.GetRequiredService<IConnectionMultiplexer>(),
                    sp.GetRequiredService<IMemoryCache>(),
                    sp.GetRequiredService<ILogger<RedisCacheService>>()));
            services.AddScoped<ISessionService, SessionService>();

            // Distributed slot lock: SETNX with 30-second TTL (AC-03).
            services.AddSingleton<IDistributedLockService, RedisDistributedLockService>();
            services.AddSingleton<ISlotLockService, SlotLockService>();

            // Rate-limiting counter: sliding window per client + endpoint (AC-04).
            services.AddOptions<RateLimitOptions>()
                .BindConfiguration(RateLimitOptions.SectionName)
                .Validate(options => options.Limit > 0, "RateLimitOptions.Limit must be greater than 0.")
                .Validate(options => options.WindowSeconds > 0, "RateLimitOptions.WindowSeconds must be greater than 0.")
                .ValidateOnStart();
            services.AddSingleton<RedisSlidingWindowCounter>();

            // Swap engine: Redis sorted set queue + FCFS engine (DR-002, US-019).
            services.AddScoped<ISwapQueueService, SwapQueueService>();
            services.AddScoped<ISwapEngineService, SwapEngineService>();
        }
        else
        {
            // Degraded mode: no Redis configured — serve entirely from in-memory cache (AC-05).
            // Swap engine is unavailable without Redis (DR-002).
            services.AddSingleton<ICacheService, InMemoryCacheService>();
            services.AddScoped<ISessionService, SessionService>();
            services.AddSingleton<ISlotLockService, InMemorySlotLockService>();
        }

        services.AddScoped<ISlotSearchService, SlotSearchService>();
        services.AddScoped<IPatientSearchService, PatientSearchService>();
        services.AddScoped<IBookingConfirmationService, BookingConfirmationService>();
        services.AddScoped<IWalkInBookingService, WalkInBookingService>();
        services.AddScoped<IAppointmentManagementService, AppointmentManagementService>();
        services.AddScoped<IQueueService, QueueService>();

        // Google Calendar integration (US_025)
        services.AddDataProtection();
        services.AddOptions<GoogleCalendarSettings>()
            .BindConfiguration(GoogleCalendarSettings.SectionName);
        services.AddHttpClient<GoogleCalendarService>();
        services.AddScoped<ICalendarService, GoogleCalendarService>();
        services.AddScoped<ICalendarTokenStore, CalendarTokenStore>();
        services.AddScoped<GoogleCalendarSyncJob>();

        // Outlook Calendar integration (US_026)
        services.AddOptions<OutlookCalendarSettings>()
            .BindConfiguration(OutlookCalendarSettings.SectionName);
        services.AddHttpClient<OutlookCalendarService>();
        services.AddScoped<IOutlookCalendarService, OutlookCalendarService>();
        services.AddScoped<IOutlookCalendarTokenStore, OutlookCalendarTokenStore>();
        services.AddScoped<OutlookCalendarSyncJob>();

        // Appointment reminder pipeline (US_027)
        services.AddOptions<NotificationSettings>()
            .BindConfiguration(NotificationSettings.SectionName);
        services.AddSingleton<INotificationDeliveryService, NotificationDeliveryService>();
        services.AddScoped<IReminderPipelineService, ReminderPipelineService>();
        services.AddScoped<ScheduleRemindersJob>();

        // Staff notification persistence (US_028)
        services.AddScoped<IStaffNotificationService, StaffNotificationService>();

        // AI intake orchestration (US_029)
        services.AddOptions<AiIntakeOptions>()
            .BindConfiguration(AiIntakeOptions.SectionName);
        services.AddHttpClient<AiIntakeService>();
        services.AddScoped<IAiIntakeService, AiIntakeService>();
        services.AddScoped<IDeIdentificationService, DeIdentificationService>();
        services.AddScoped<IAiIntakePersistenceService, AiIntakePersistenceService>();

        // Manual intake orchestration (US_030)
        services.AddScoped<IManualIntakeService, ManualIntakeService>();

        // Document upload and storage (US_032)
        services.AddOptions<DocumentStorageOptions>()
            .BindConfiguration(DocumentStorageOptions.SectionName);
        services.AddScoped<IDocumentStorageService, DocumentStorageService>();

        // Malware scanning pipeline (US_033)
        services.AddOptions<ClamAvOptions>()
            .BindConfiguration(ClamAvOptions.SectionName);
        services.AddScoped<IMalwareScanService, MalwareScanService>();
        services.AddScoped<MalwareScanJob>();

        // Document deletion and retention policy enforcement (US_034)
        services.AddScoped<IDocumentDeletionService, DocumentDeletionService>();
        services.AddScoped<PatientViewAggregationJob>();
        services.AddScoped<RetentionCleanupJob>();

        // NER clinical entity extraction pipeline (US_035, TR-010)
        services.AddOptions<NerModelOptions>()
            .BindConfiguration(NerModelOptions.SectionName);
        services.AddSingleton<INerModelService, NerModelService>();
        services.AddScoped<INerTrainingPipeline, NerTrainingPipeline>();

        // Confidence scoring — threshold-based extraction decisions (US_036, AC-01, AC-02, AC-03)
        services.AddOptions<ExtractionThresholdConfig>()
            .BindConfiguration(ExtractionThresholdConfig.SectionName);
        services.AddSingleton<IConfidenceScoringService, ConfidenceScoringService>();

        // NER model versioning and accuracy monitoring (US_037, AC-01, AC-02, AC-03)
        services.AddSingleton<IModelVersioningService, ModelVersioningService>();
        services.AddScoped<IModelMonitoringService, ModelMonitoringService>();
        services.AddScoped<ModelAccuracyCheckJob>();

        // Document parsers — format-specific text extractors for NER pipeline (US_035, NFR-011)
        services.AddOptions<TesseractOptions>()
            .BindConfiguration(TesseractOptions.SectionName);
        services.AddSingleton<IDocumentParser, PdfDocumentParser>();
        services.AddSingleton<IDocumentParser, DocxDocumentParser>();
        services.AddSingleton<IDocumentParser, OcrDocumentParser>();
        services.AddSingleton<IDocumentParser, DicomDocumentParser>();
        services.AddSingleton<IDocumentParser, FhirDocumentParser>();
        services.AddSingleton<IDocumentParserFactory, DocumentParserFactory>();

        return services;
    }
}
