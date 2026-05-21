using Application.Interfaces;
using Infrastructure.Data.Extensions;
using Infrastructure.Data.Options;
using Microsoft.Extensions.Options;

namespace Infrastructure.Security;

/// <summary>
/// AES-256-CBC PHI encryption service for application-layer encrypt/decrypt operations
/// (AC-01). Delegates cryptographic work to <see cref="PgCryptoExtensions"/>.
///
/// Key rotation (zero-downtime, dual-key period):
///   - Encryption always uses <see cref="PhiEncryptionOptions.Key"/> (current key).
///   - Decryption tries the current key first; if decryption fails and
///     <see cref="PhiEncryptionOptions.PreviousKey"/> is configured, it retries with
///     the previous key. Remove PreviousKey once all records are re-encrypted.
/// </summary>
public sealed class PhiEncryptionService : IPhiEncryptionService
{
    private readonly byte[] _currentKey;
    private readonly byte[]? _previousKey;

    public PhiEncryptionService(IOptions<PhiEncryptionOptions> options)
    {
        var opts = options.Value;
        _currentKey = Convert.FromBase64String(opts.Key);

        if (!string.IsNullOrWhiteSpace(opts.PreviousKey))
            _previousKey = Convert.FromBase64String(opts.PreviousKey);
    }

    /// <inheritdoc/>
    public string Encrypt(string plaintext) =>
        PgCryptoExtensions.Encrypt(plaintext, _currentKey);

    /// <inheritdoc/>
    public string Decrypt(string ciphertext)
    {
        try
        {
            return PgCryptoExtensions.Decrypt(ciphertext, _currentKey);
        }
        catch (Exception) when (_previousKey is not null)
        {
            // Fallback during key rotation — try previous key before propagating failure.
            return PgCryptoExtensions.Decrypt(ciphertext, _previousKey);
        }
    }
}
