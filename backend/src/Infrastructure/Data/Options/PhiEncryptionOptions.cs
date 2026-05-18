namespace Infrastructure.Data.Options;

public class PhiEncryptionOptions
{
    public const string SectionName = "PhiEncryption";

    /// <summary>
    /// Base64-encoded 32-byte (256-bit) AES key used for PHI field-level encryption.
    /// Must be set via environment variable or secrets manager — never hardcoded.
    /// </summary>
    public string Key { get; set; } = string.Empty;
}
