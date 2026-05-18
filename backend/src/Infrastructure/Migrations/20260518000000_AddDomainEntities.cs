using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDomainEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Ensure pgcrypto extension is available for PHI encryption (AC-02, Edge Case)
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pgcrypto");

            migrationBuilder.CreateTable(
                name: "AvailabilitySlots",
                columns: table => new
                {
                    SlotId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Specialty = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false),
                    LockExpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RecurrencePattern = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvailabilitySlots", x => x.SlotId);
                });

            migrationBuilder.CreateTable(
                name: "InsuranceRecords",
                columns: table => new
                {
                    RecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    InsuranceName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    InsuranceIdPattern = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InsuranceRecords", x => x.RecordId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    AuthProvider = table.Column<int>(type: "integer", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    MfaEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    MfaSecret = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    AuditLogId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActorRole = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ActionType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ResourceType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ResourceId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Details = table.Column<string>(type: "jsonb", nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.AuditLogId);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Users_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CalendarSyncs",
                columns: table => new
                {
                    SyncId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<int>(type: "integer", nullable: false),
                    AccessToken = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false),
                    RefreshToken = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false),
                    TokenExpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LastSyncAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarSyncs", x => x.SyncId);
                    table.ForeignKey(
                        name: "FK_CalendarSyncs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PatientProfiles",
                columns: table => new
                {
                    PatientProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    LastName = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    DateOfBirth = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Phone = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    InsuranceName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    InsuranceId = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    InsuranceValidationStatus = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientProfiles", x => x.PatientProfileId);
                    table.ForeignKey(
                        name: "FK_PatientProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    AppointmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderId = table.Column<Guid>(type: "uuid", nullable: false),
                    SlotId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    NoShowRiskTier = table.Column<int>(type: "integer", nullable: true),
                    NoShowRiskScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    BookingType = table.Column<int>(type: "integer", nullable: false),
                    PreferredSlotId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointments", x => x.AppointmentId);
                    table.ForeignKey(
                        name: "FK_Appointments_AvailabilitySlots_PreferredSlotId",
                        column: x => x.PreferredSlotId,
                        principalTable: "AvailabilitySlots",
                        principalColumn: "SlotId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Appointments_AvailabilitySlots_SlotId",
                        column: x => x.SlotId,
                        principalTable: "AvailabilitySlots",
                        principalColumn: "SlotId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Appointments_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Appointments_Users_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClinicalDocuments",
                columns: table => new
                {
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    FileFormat = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    StoragePath = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    MalwareScanStatus = table.Column<int>(type: "integer", nullable: false),
                    ProcessingStatus = table.Column<int>(type: "integer", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicalDocuments", x => x.DocumentId);
                    table.ForeignKey(
                        name: "FK_ClinicalDocuments_PatientProfiles_PatientProfileId",
                        column: x => x.PatientProfileId,
                        principalTable: "PatientProfiles",
                        principalColumn: "PatientProfileId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NoShowRiskFactors",
                columns: table => new
                {
                    FactorId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    HistoricalNoShowCount = table.Column<int>(type: "integer", nullable: false),
                    LastNoShowDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AverageLeadTimeDays = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: false),
                    PreferredTimeOfDay = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    IsNewPatient = table.Column<bool>(type: "boolean", nullable: false),
                    LastCalculatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoShowRiskFactors", x => x.FactorId);
                    table.ForeignKey(
                        name: "FK_NoShowRiskFactors_PatientProfiles_PatientProfileId",
                        column: x => x.PatientProfileId,
                        principalTable: "PatientProfiles",
                        principalColumn: "PatientProfileId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PatientViews",
                columns: table => new
                {
                    PatientViewId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    AggregatedVitals = table.Column<string>(type: "jsonb", nullable: true),
                    AggregatedMedications = table.Column<string>(type: "jsonb", nullable: true),
                    AggregatedAllergies = table.Column<string>(type: "jsonb", nullable: true),
                    AggregatedDiagnoses = table.Column<string>(type: "jsonb", nullable: true),
                    AggregatedProcedures = table.Column<string>(type: "jsonb", nullable: true),
                    LastAggregatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerificationStatus = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientViews", x => x.PatientViewId);
                    table.ForeignKey(
                        name: "FK_PatientViews_PatientProfiles_PatientProfileId",
                        column: x => x.PatientProfileId,
                        principalTable: "PatientProfiles",
                        principalColumn: "PatientProfileId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IntakeRecords",
                columns: table => new
                {
                    IntakeId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    IntakeMode = table.Column<int>(type: "integer", nullable: false),
                    MedicalHistory = table.Column<string>(type: "jsonb", nullable: true),
                    CurrentSymptoms = table.Column<string>(type: "jsonb", nullable: true),
                    Medications = table.Column<string>(type: "jsonb", nullable: true),
                    Allergies = table.Column<string>(type: "jsonb", nullable: true),
                    ReasonForVisit = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntakeRecords", x => x.IntakeId);
                    table.ForeignKey(
                        name: "FK_IntakeRecords_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "AppointmentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IntakeRecords_PatientProfiles_PatientProfileId",
                        column: x => x.PatientProfileId,
                        principalTable: "PatientProfiles",
                        principalColumn: "PatientProfileId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    NotificationId = table.Column<Guid>(type: "uuid", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    Channel = table.Column<int>(type: "integer", nullable: false),
                    NotificationType = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    LastAttemptAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FailureReason = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.NotificationId);
                    table.ForeignKey(
                        name: "FK_Notifications_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "AppointmentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PreferredSlotQueues",
                columns: table => new
                {
                    QueueId = table.Column<Guid>(type: "uuid", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreferredSlotId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreferredSlotQueues", x => x.QueueId);
                    table.ForeignKey(
                        name: "FK_PreferredSlotQueues_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "AppointmentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PreferredSlotQueues_AvailabilitySlots_PreferredSlotId",
                        column: x => x.PreferredSlotId,
                        principalTable: "AvailabilitySlots",
                        principalColumn: "SlotId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExtractedDataRecords",
                columns: table => new
                {
                    RecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    DataType = table.Column<int>(type: "integer", nullable: false),
                    FieldName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    FieldValue = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    Confidence = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false),
                    VerifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SourceLocation = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExtractedDataRecords", x => x.RecordId);
                    table.ForeignKey(
                        name: "FK_ExtractedDataRecords_ClinicalDocuments_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "ClinicalDocuments",
                        principalColumn: "DocumentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExtractedDataRecords_PatientProfiles_PatientProfileId",
                        column: x => x.PatientProfileId,
                        principalTable: "PatientProfiles",
                        principalColumn: "PatientProfileId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExtractedDataRecords_Users_VerifiedByUserId",
                        column: x => x.VerifiedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DataConflicts",
                columns: table => new
                {
                    ConflictId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConflictType = table.Column<int>(type: "integer", nullable: false),
                    FieldName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Value1 = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    SourceDocumentId1 = table.Column<Guid>(type: "uuid", nullable: false),
                    Value2 = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    SourceDocumentId2 = table.Column<Guid>(type: "uuid", nullable: false),
                    ResolutionStatus = table.Column<int>(type: "integer", nullable: false),
                    ResolvedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResolutionNotes = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataConflicts", x => x.ConflictId);
                    table.ForeignKey(
                        name: "FK_DataConflicts_ClinicalDocuments_SourceDocumentId1",
                        column: x => x.SourceDocumentId1,
                        principalTable: "ClinicalDocuments",
                        principalColumn: "DocumentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DataConflicts_ClinicalDocuments_SourceDocumentId2",
                        column: x => x.SourceDocumentId2,
                        principalTable: "ClinicalDocuments",
                        principalColumn: "DocumentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DataConflicts_PatientProfiles_PatientProfileId",
                        column: x => x.PatientProfileId,
                        principalTable: "PatientProfiles",
                        principalColumn: "PatientProfileId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DataConflicts_Users_ResolvedByUserId",
                        column: x => x.ResolvedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "MedicalCodeMappings",
                columns: table => new
                {
                    MappingId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExtractedRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    CodeType = table.Column<int>(type: "integer", nullable: false),
                    CodeValue = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CodeDescription = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Confidence = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false),
                    VerifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CodeSetVersion = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalCodeMappings", x => x.MappingId);
                    table.ForeignKey(
                        name: "FK_MedicalCodeMappings_ExtractedDataRecords_ExtractedRecordId",
                        column: x => x.ExtractedRecordId,
                        principalTable: "ExtractedDataRecords",
                        principalColumn: "RecordId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MedicalCodeMappings_PatientProfiles_PatientProfileId",
                        column: x => x.PatientProfileId,
                        principalTable: "PatientProfiles",
                        principalColumn: "PatientProfileId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MedicalCodeMappings_Users_VerifiedByUserId",
                        column: x => x.VerifiedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                });

            // --- Indexes ---

            migrationBuilder.CreateIndex(name: "IX_Users_Email", table: "Users", column: "Email", unique: true);
            migrationBuilder.CreateIndex(name: "IX_Users_CreatedAt", table: "Users", column: "CreatedAt");

            migrationBuilder.CreateIndex(name: "IX_PatientProfiles_UserId", table: "PatientProfiles", column: "UserId", unique: true);
            migrationBuilder.CreateIndex(name: "IX_PatientProfiles_CreatedAt", table: "PatientProfiles", column: "CreatedAt");

            migrationBuilder.CreateIndex(name: "IX_Appointments_PatientId", table: "Appointments", column: "PatientId");
            migrationBuilder.CreateIndex(name: "IX_Appointments_Status", table: "Appointments", column: "Status");
            migrationBuilder.CreateIndex(name: "IX_Appointments_CreatedAt", table: "Appointments", column: "CreatedAt");
            migrationBuilder.CreateIndex(name: "IX_Appointments_PatientId_Status", table: "Appointments", columns: new[] { "PatientId", "Status" });
            migrationBuilder.CreateIndex(name: "IX_Appointments_SlotId", table: "Appointments", column: "SlotId");
            migrationBuilder.CreateIndex(name: "IX_Appointments_PreferredSlotId", table: "Appointments", column: "PreferredSlotId");
            migrationBuilder.CreateIndex(name: "IX_Appointments_CreatedByUserId", table: "Appointments", column: "CreatedByUserId");

            migrationBuilder.CreateIndex(name: "IX_AvailabilitySlots_ProviderId", table: "AvailabilitySlots", column: "ProviderId");
            migrationBuilder.CreateIndex(name: "IX_AvailabilitySlots_ProviderId_StartTime", table: "AvailabilitySlots", columns: new[] { "ProviderId", "StartTime" });
            migrationBuilder.CreateIndex(name: "IX_AvailabilitySlots_IsAvailable", table: "AvailabilitySlots", column: "IsAvailable");

            migrationBuilder.CreateIndex(name: "IX_PreferredSlotQueues_AppointmentId", table: "PreferredSlotQueues", column: "AppointmentId");
            migrationBuilder.CreateIndex(name: "IX_PreferredSlotQueues_Status", table: "PreferredSlotQueues", column: "Status");
            migrationBuilder.CreateIndex(name: "IX_PreferredSlotQueues_PreferredSlotId", table: "PreferredSlotQueues", column: "PreferredSlotId");

            migrationBuilder.CreateIndex(name: "IX_ClinicalDocuments_PatientProfileId", table: "ClinicalDocuments", column: "PatientProfileId");
            migrationBuilder.CreateIndex(name: "IX_ClinicalDocuments_ProcessingStatus", table: "ClinicalDocuments", column: "ProcessingStatus");
            migrationBuilder.CreateIndex(name: "IX_ClinicalDocuments_UploadedAt", table: "ClinicalDocuments", column: "UploadedAt");

            migrationBuilder.CreateIndex(name: "IX_ExtractedDataRecords_PatientProfileId", table: "ExtractedDataRecords", column: "PatientProfileId");
            migrationBuilder.CreateIndex(name: "IX_ExtractedDataRecords_DocumentId", table: "ExtractedDataRecords", column: "DocumentId");
            migrationBuilder.CreateIndex(name: "IX_ExtractedDataRecords_DataType", table: "ExtractedDataRecords", column: "DataType");
            migrationBuilder.CreateIndex(name: "IX_ExtractedDataRecords_IsVerified", table: "ExtractedDataRecords", column: "IsVerified");
            migrationBuilder.CreateIndex(name: "IX_ExtractedDataRecords_VerifiedByUserId", table: "ExtractedDataRecords", column: "VerifiedByUserId");

            migrationBuilder.CreateIndex(name: "IX_DataConflicts_PatientProfileId", table: "DataConflicts", column: "PatientProfileId");
            migrationBuilder.CreateIndex(name: "IX_DataConflicts_ResolutionStatus", table: "DataConflicts", column: "ResolutionStatus");
            migrationBuilder.CreateIndex(name: "IX_DataConflicts_PatientProfileId_ResolutionStatus", table: "DataConflicts", columns: new[] { "PatientProfileId", "ResolutionStatus" });
            migrationBuilder.CreateIndex(name: "IX_DataConflicts_SourceDocumentId1", table: "DataConflicts", column: "SourceDocumentId1");
            migrationBuilder.CreateIndex(name: "IX_DataConflicts_SourceDocumentId2", table: "DataConflicts", column: "SourceDocumentId2");
            migrationBuilder.CreateIndex(name: "IX_DataConflicts_ResolvedByUserId", table: "DataConflicts", column: "ResolvedByUserId");

            migrationBuilder.CreateIndex(name: "IX_PatientViews_PatientProfileId", table: "PatientViews", column: "PatientProfileId", unique: true);
            migrationBuilder.CreateIndex(name: "IX_PatientViews_VerificationStatus", table: "PatientViews", column: "VerificationStatus");

            migrationBuilder.CreateIndex(name: "IX_MedicalCodeMappings_PatientProfileId", table: "MedicalCodeMappings", column: "PatientProfileId");
            migrationBuilder.CreateIndex(name: "IX_MedicalCodeMappings_ExtractedRecordId", table: "MedicalCodeMappings", column: "ExtractedRecordId");
            migrationBuilder.CreateIndex(name: "IX_MedicalCodeMappings_CodeType", table: "MedicalCodeMappings", column: "CodeType");
            migrationBuilder.CreateIndex(name: "IX_MedicalCodeMappings_IsVerified", table: "MedicalCodeMappings", column: "IsVerified");
            migrationBuilder.CreateIndex(name: "IX_MedicalCodeMappings_VerifiedByUserId", table: "MedicalCodeMappings", column: "VerifiedByUserId");

            migrationBuilder.CreateIndex(name: "IX_IntakeRecords_PatientProfileId", table: "IntakeRecords", column: "PatientProfileId");
            migrationBuilder.CreateIndex(name: "IX_IntakeRecords_AppointmentId", table: "IntakeRecords", column: "AppointmentId");

            migrationBuilder.CreateIndex(name: "IX_Notifications_AppointmentId", table: "Notifications", column: "AppointmentId");
            migrationBuilder.CreateIndex(name: "IX_Notifications_PatientId", table: "Notifications", column: "PatientId");
            migrationBuilder.CreateIndex(name: "IX_Notifications_Status", table: "Notifications", column: "Status");
            migrationBuilder.CreateIndex(name: "IX_Notifications_CreatedAt", table: "Notifications", column: "CreatedAt");

            migrationBuilder.CreateIndex(name: "IX_CalendarSyncs_UserId", table: "CalendarSyncs", column: "UserId");
            migrationBuilder.CreateIndex(name: "IX_CalendarSyncs_UserId_Provider", table: "CalendarSyncs", columns: new[] { "UserId", "Provider" });

            migrationBuilder.CreateIndex(name: "IX_AuditLogs_ActorUserId", table: "AuditLogs", column: "ActorUserId");
            migrationBuilder.CreateIndex(name: "IX_AuditLogs_Timestamp", table: "AuditLogs", column: "Timestamp");
            migrationBuilder.CreateIndex(name: "IX_AuditLogs_ActionType", table: "AuditLogs", column: "ActionType");
            migrationBuilder.CreateIndex(name: "IX_AuditLogs_ResourceType_ResourceId", table: "AuditLogs", columns: new[] { "ResourceType", "ResourceId" });

            migrationBuilder.CreateIndex(name: "IX_NoShowRiskFactors_PatientProfileId", table: "NoShowRiskFactors", column: "PatientProfileId", unique: true);

            migrationBuilder.CreateIndex(name: "IX_InsuranceRecords_InsuranceName", table: "InsuranceRecords", column: "InsuranceName");
            migrationBuilder.CreateIndex(name: "IX_InsuranceRecords_IsActive", table: "InsuranceRecords", column: "IsActive");

            // Enforce append-only immutability on AuditLogs at the database level (ADD-8, NFR-005).
            // Any UPDATE or DELETE attempt — including from direct DB clients — raises an exception.
            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION fn_prevent_audit_log_mutation()
                RETURNS TRIGGER LANGUAGE plpgsql AS $$
                BEGIN
                    RAISE EXCEPTION 'AuditLog records are immutable: % on AuditLogs is not permitted', TG_OP;
                END;
                $$;

                CREATE TRIGGER trg_audit_logs_immutable
                BEFORE UPDATE OR DELETE ON "AuditLogs"
                FOR EACH ROW EXECUTE FUNCTION fn_prevent_audit_log_mutation();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS trg_audit_logs_immutable ON "AuditLogs";
                DROP FUNCTION IF EXISTS fn_prevent_audit_log_mutation();
                """);

            migrationBuilder.DropTable(name: "MedicalCodeMappings");
            migrationBuilder.DropTable(name: "DataConflicts");
            migrationBuilder.DropTable(name: "ExtractedDataRecords");
            migrationBuilder.DropTable(name: "PatientViews");
            migrationBuilder.DropTable(name: "NoShowRiskFactors");
            migrationBuilder.DropTable(name: "IntakeRecords");
            migrationBuilder.DropTable(name: "Notifications");
            migrationBuilder.DropTable(name: "PreferredSlotQueues");
            migrationBuilder.DropTable(name: "AuditLogs");
            migrationBuilder.DropTable(name: "CalendarSyncs");
            migrationBuilder.DropTable(name: "Appointments");
            migrationBuilder.DropTable(name: "ClinicalDocuments");
            migrationBuilder.DropTable(name: "PatientProfiles");
            migrationBuilder.DropTable(name: "AvailabilitySlots");
            migrationBuilder.DropTable(name: "InsuranceRecords");
            migrationBuilder.DropTable(name: "Users");
        }
    }
}
