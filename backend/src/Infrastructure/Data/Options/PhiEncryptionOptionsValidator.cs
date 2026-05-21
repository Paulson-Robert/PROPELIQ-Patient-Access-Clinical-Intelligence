using Microsoft.Extensions.Options;

namespace Infrastructure.Data.Options;

/// <summary>
/// Validates PhiEncryptionOptions at host startup via ValidateOnStart.
/// The app refuses to start if the key is absent, malformed, or the wrong length.
/// Design-time tools (dotnet ef) never call IHost.StartAsync, so migrations are unaffected.
/// </summary>
internal sealed class PhiEncryptionOptionsValidator : IValidateOptions<PhiEncryptionOptions>
{
    public ValidateOptionsResult Validate(string? name, PhiEncryptionOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Key))
            return ValidateOptionsResult.Fail(
                "PhiEncryption:Key is required. " +
                "Provide a Base64-encoded 32-byte AES key via the PhiEncryption__Key " +
                "environment variable or a secrets manager. " +
                "The application will not start without it to prevent PHI being stored in plaintext.");

        byte[] keyBytes;
        try
        {
            keyBytes = Convert.FromBase64String(options.Key);
        }
        catch (FormatException)
        {
            return ValidateOptionsResult.Fail(
                "PhiEncryption:Key is not valid Base64. " +
                "Generate a key with: openssl rand -base64 32");
        }

        if (keyBytes.Length != 32)
            return ValidateOptionsResult.Fail(
                $"PhiEncryption:Key must decode to exactly 32 bytes (256-bit AES); " +
                $"got {keyBytes.Length} byte(s). " +
                $"Generate a key with: openssl rand -base64 32");

        // Validate PreviousKey only when provided (optional — used during key rotation)
        if (!string.IsNullOrWhiteSpace(options.PreviousKey))
        {
            byte[] previousKeyBytes;
            try
            {
                previousKeyBytes = Convert.FromBase64String(options.PreviousKey);
            }
            catch (FormatException)
            {
                return ValidateOptionsResult.Fail(
                    "PhiEncryption:PreviousKey is not valid Base64. " +
                    "Generate a key with: openssl rand -base64 32");
            }

            if (previousKeyBytes.Length != 32)
                return ValidateOptionsResult.Fail(
                    $"PhiEncryption:PreviousKey must decode to exactly 32 bytes (256-bit AES); " +
                    $"got {previousKeyBytes.Length} byte(s).");
        }

        return ValidateOptionsResult.Success;
    }
}
