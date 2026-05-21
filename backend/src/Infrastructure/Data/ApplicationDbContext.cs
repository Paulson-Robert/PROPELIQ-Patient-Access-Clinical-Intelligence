using System.Reflection;
using Domain.Entities;
using Infrastructure.Data.Extensions;
using Infrastructure.Data.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    private readonly byte[]? _encryptionKey;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IOptions<PhiEncryptionOptions>? encryptionOptions = null)
        : base(options)
    {
        var keyValue = encryptionOptions?.Value.Key;
        if (!string.IsNullOrWhiteSpace(keyValue))
            _encryptionKey = Convert.FromBase64String(keyValue);
    }

    // --- DbSets ---
    public DbSet<User> Users => Set<User>();
    public DbSet<PatientProfile> PatientProfiles => Set<PatientProfile>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<AvailabilitySlot> AvailabilitySlots => Set<AvailabilitySlot>();
    public DbSet<PreferredSlotQueue> PreferredSlotQueues => Set<PreferredSlotQueue>();
    public DbSet<ClinicalDocument> ClinicalDocuments => Set<ClinicalDocument>();
    public DbSet<ExtractedDataRecord> ExtractedDataRecords => Set<ExtractedDataRecord>();
    public DbSet<DataConflict> DataConflicts => Set<DataConflict>();
    public DbSet<PatientView> PatientViews => Set<PatientView>();
    public DbSet<MedicalCodeMapping> MedicalCodeMappings => Set<MedicalCodeMapping>();
    public DbSet<IntakeRecord> IntakeRecords => Set<IntakeRecord>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<CalendarSync> CalendarSyncs => Set<CalendarSync>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<NoShowRiskFactor> NoShowRiskFactors => Set<NoShowRiskFactor>();
    public DbSet<InsuranceRecord> InsuranceRecords => Set<InsuranceRecord>();
    public DbSet<Icd10Code> Icd10Codes => Set<Icd10Code>();
    public DbSet<CptCode> CptCodes => Set<CptCode>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // pgcrypto extension required for PHI encryption (DR-001)
        modelBuilder.HasPostgresExtension("pgcrypto");

        // Apply all IEntityTypeConfiguration<T> classes in this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // PHI column encryption via AES-256 value converters (AC-02)
        // Applied after configurations so column constraints are already set
        if (_encryptionKey is { Length: 32 })
        {
            var stringConverter = PgCryptoExtensions.CreateStringConverter(_encryptionKey);
            var nullableStringConverter = PgCryptoExtensions.CreateNullableStringConverter(_encryptionKey);
            var dateConverter = PgCryptoExtensions.CreateNullableDateOnlyConverter(_encryptionKey);

            modelBuilder.Entity<PatientProfile>()
                .Property(p => p.FirstName)
                .HasConversion(stringConverter);

            modelBuilder.Entity<PatientProfile>()
                .Property(p => p.LastName)
                .HasConversion(stringConverter);

            modelBuilder.Entity<PatientProfile>()
                .Property(p => p.DateOfBirth)
                .HasConversion(dateConverter);

            modelBuilder.Entity<PatientProfile>()
                .Property(p => p.Phone)
                .HasConversion(nullableStringConverter);

            modelBuilder.Entity<PatientProfile>()
                .Property(p => p.InsuranceId)
                .HasConversion(nullableStringConverter);

            // TOTP seed — credential material encrypted identically to PHI columns
            modelBuilder.Entity<User>()
                .Property(u => u.MfaSecret)
                .HasConversion(nullableStringConverter);

            modelBuilder.Entity<User>()
                .Property(u => u.MfaPhoneNumber)
                .HasConversion(nullableStringConverter);
        }
    }
}
