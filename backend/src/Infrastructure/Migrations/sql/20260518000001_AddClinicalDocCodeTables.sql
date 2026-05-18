-- =============================================================================
-- Migration: 20260518000001_AddClinicalDocCodeTables
-- Description:
--   1. Converts ClinicalDocuments.FileFormat from varchar(32) to integer
--      (DocumentFormat enum: Pdf=0, Docx=1, Jpg=2, Png=3, Dicom=4, Hl7Fhir=5)
--   2. Adds ScanningStartedAt and ProcessingStartedAt stage timestamp columns
--   3. Creates Icd10Codes reference table with composite unique index
--   4. Creates CptCodes reference table with composite unique index
-- Idempotency: All steps guarded by IF NOT EXISTS / DO blocks.
-- Target: PostgreSQL 16, applied via EF Core migration runner or directly.
-- =============================================================================

BEGIN;

-- ---------------------------------------------------------------------------
-- 1. FileFormat: varchar(32) → integer
--    Only executes if the column is still character varying type.
-- ---------------------------------------------------------------------------
DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_name = 'ClinicalDocuments'
          AND column_name = 'FileFormat'
          AND data_type = 'character varying'
    ) THEN
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
    END IF;
END $$;

-- ---------------------------------------------------------------------------
-- 2. Stage timestamp columns: ScanningStartedAt, ProcessingStartedAt
-- ---------------------------------------------------------------------------
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'ClinicalDocuments' AND column_name = 'ScanningStartedAt'
    ) THEN
        ALTER TABLE "ClinicalDocuments"
        ADD COLUMN "ScanningStartedAt" timestamp with time zone NULL;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'ClinicalDocuments' AND column_name = 'ProcessingStartedAt'
    ) THEN
        ALTER TABLE "ClinicalDocuments"
        ADD COLUMN "ProcessingStartedAt" timestamp with time zone NULL;
    END IF;
END $$;

-- ---------------------------------------------------------------------------
-- 3. ICD-10-CM reference table
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS "Icd10Codes" (
    "Icd10CodeId"    uuid                NOT NULL,
    "CodeValue"      character varying(16)  NOT NULL,
    "Description"    character varying(512) NOT NULL,
    "Category"       character varying(128) NOT NULL,
    "CodeSetVersion" character varying(16)  NOT NULL,
    CONSTRAINT "PK_Icd10Codes" PRIMARY KEY ("Icd10CodeId")
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_Icd10Codes_CodeValue_CodeSetVersion"
    ON "Icd10Codes" ("CodeValue", "CodeSetVersion");

CREATE INDEX IF NOT EXISTS "IX_Icd10Codes_CodeSetVersion"
    ON "Icd10Codes" ("CodeSetVersion");

-- ---------------------------------------------------------------------------
-- 4. CPT-4 reference table
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS "CptCodes" (
    "CptCodeId"      uuid                NOT NULL,
    "CodeValue"      character varying(16)  NOT NULL,
    "Description"    character varying(512) NOT NULL,
    "Category"       character varying(128) NOT NULL,
    "CodeSetVersion" character varying(16)  NOT NULL,
    CONSTRAINT "PK_CptCodes" PRIMARY KEY ("CptCodeId")
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_CptCodes_CodeValue_CodeSetVersion"
    ON "CptCodes" ("CodeValue", "CodeSetVersion");

CREATE INDEX IF NOT EXISTS "IX_CptCodes_CodeSetVersion"
    ON "CptCodes" ("CodeSetVersion");

-- ---------------------------------------------------------------------------
-- 5. Record migration in EF Core history table
-- ---------------------------------------------------------------------------
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
SELECT '20260518000001_AddClinicalDocCodeTables', '9.0.5'
WHERE NOT EXISTS (
    SELECT 1 FROM "__EFMigrationsHistory"
    WHERE "MigrationId" = '20260518000001_AddClinicalDocCodeTables'
);

COMMIT;
