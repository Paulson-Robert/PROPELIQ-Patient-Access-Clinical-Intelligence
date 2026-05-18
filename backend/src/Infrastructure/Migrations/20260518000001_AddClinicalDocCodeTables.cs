using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClinicalDocCodeTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // --- ClinicalDocuments: FileFormat varchar(32) → integer ---
            // Existing rows default to 0 (DocumentFormat.Pdf). No production data exists
            // at this migration point (scaffold phase). (AC-01)
            migrationBuilder.Sql("""
                ALTER TABLE "ClinicalDocuments"
                ALTER COLUMN "FileFormat" TYPE integer
                USING CASE "FileFormat"
                    WHEN 'Pdf'     THEN 0
                    WHEN 'Docx'    THEN 1
                    WHEN 'Jpg'     THEN 2
                    WHEN 'Png'     THEN 3
                    WHEN 'Dicom'   THEN 4
                    WHEN 'Hl7Fhir' THEN 5
                    ELSE 0
                END;
                """);

            // --- ClinicalDocuments: add stage transition timestamp columns (AC-02) ---
            migrationBuilder.AddColumn<DateTime>(
                name: "ScanningStartedAt",
                table: "ClinicalDocuments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProcessingStartedAt",
                table: "ClinicalDocuments",
                type: "timestamp with time zone",
                nullable: true);

            // --- ICD-10-CM reference table (AC-03) ---
            migrationBuilder.CreateTable(
                name: "Icd10Codes",
                columns: table => new
                {
                    Icd10CodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    CodeValue = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Category = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CodeSetVersion = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Icd10Codes", x => x.Icd10CodeId);
                });

            // --- CPT-4 reference table (AC-04) ---
            migrationBuilder.CreateTable(
                name: "CptCodes",
                columns: table => new
                {
                    CptCodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    CodeValue = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Category = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CodeSetVersion = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CptCodes", x => x.CptCodeId);
                });

            // --- Indexes ---

            // Composite unique: same CodeValue may exist in different CodeSetVersions (Edge Case)
            migrationBuilder.CreateIndex(
                name: "IX_Icd10Codes_CodeValue_CodeSetVersion",
                table: "Icd10Codes",
                columns: new[] { "CodeValue", "CodeSetVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Icd10Codes_CodeSetVersion",
                table: "Icd10Codes",
                column: "CodeSetVersion");

            migrationBuilder.CreateIndex(
                name: "IX_CptCodes_CodeValue_CodeSetVersion",
                table: "CptCodes",
                columns: new[] { "CodeValue", "CodeSetVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CptCodes_CodeSetVersion",
                table: "CptCodes",
                column: "CodeSetVersion");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "CptCodes");
            migrationBuilder.DropTable(name: "Icd10Codes");

            migrationBuilder.DropColumn(name: "ProcessingStartedAt", table: "ClinicalDocuments");
            migrationBuilder.DropColumn(name: "ScanningStartedAt", table: "ClinicalDocuments");

            // Revert FileFormat from integer back to varchar(32)
            migrationBuilder.Sql("""
                ALTER TABLE "ClinicalDocuments"
                ALTER COLUMN "FileFormat" TYPE character varying(32)
                USING CASE "FileFormat"
                    WHEN 0 THEN 'Pdf'
                    WHEN 1 THEN 'Docx'
                    WHEN 2 THEN 'Jpg'
                    WHEN 3 THEN 'Png'
                    WHEN 4 THEN 'Dicom'
                    WHEN 5 THEN 'Hl7Fhir'
                    ELSE 'Pdf'
                END;
                """);
        }
    }
}
