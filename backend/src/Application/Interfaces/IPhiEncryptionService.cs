namespace Application.Interfaces;

/// <summary>
/// Encrypts and decrypts PHI strings using AES-256-CBC.
/// Supports zero-downtime key rotation: decryption falls back to the previous key
/// when the current key fails, allowing a dual-key transition period (AC-01).
/// </summary>
public interface IPhiEncryptionService
{
    /// <summary>Encrypts <paramref name="plaintext"/> using the current AES-256 key.</summary>
    string Encrypt(string plaintext);

    /// <summary>
    /// Decrypts <paramref name="ciphertext"/>. Tries the current key first; falls back
    /// to the previous key during a key-rotation dual-key period.
    /// </summary>
    string Decrypt(string ciphertext);
}
