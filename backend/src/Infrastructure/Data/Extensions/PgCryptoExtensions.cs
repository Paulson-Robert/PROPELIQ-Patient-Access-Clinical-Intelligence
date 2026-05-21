using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Data.Extensions;

/// <summary>
/// Factory methods for EF Core value converters that apply AES-256-CBC encryption
/// to PHI columns at rest. Encryption is performed in .NET before values reach the
/// database; the pgcrypto extension is registered for SQL-level operations where needed.
/// </summary>
public static class PgCryptoExtensions
{
    private const int IvLength = 16;

    /// <summary>
    /// Returns a converter that encrypts/decrypts a non-nullable string PHI column.
    /// </summary>
    public static ValueConverter<string, string> CreateStringConverter(byte[] key) =>
        new(
            plaintext => Encrypt(plaintext, key),
            ciphertext => Decrypt(ciphertext, key));

    /// <summary>
    /// Returns a converter that encrypts/decrypts a nullable string PHI column.
    /// Null values are stored as NULL in the database (not encrypted).
    /// </summary>
    public static ValueConverter<string?, string?> CreateNullableStringConverter(byte[] key) =>
        new(
            plaintext => plaintext == null ? null : Encrypt(plaintext, key),
            ciphertext => ciphertext == null ? null : Decrypt(ciphertext, key));

    /// <summary>
    /// Returns a converter that encrypts/decrypts a nullable DateOnly PHI column.
    /// The date is serialised to ISO 8601 (yyyy-MM-dd) before encryption.
    /// </summary>
    public static ValueConverter<DateOnly?, string?> CreateNullableDateOnlyConverter(byte[] key) =>
        new(
            date => date == null ? null : Encrypt(date.Value.ToString("yyyy-MM-dd"), key),
            ciphertext => ciphertext == null ? null : DateOnly.Parse(Decrypt(ciphertext, key)));

    internal static string Encrypt(string plaintext, byte[] key)
    {
        using var aes = Aes.Create();
        aes.Key = key;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var encryptedBytes = encryptor.TransformFinalBlock(plaintextBytes, 0, plaintextBytes.Length);

        // Prepend IV so it can be extracted on decryption
        var result = new byte[IvLength + encryptedBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, IvLength);
        Buffer.BlockCopy(encryptedBytes, 0, result, IvLength, encryptedBytes.Length);
        return Convert.ToBase64String(result);
    }

    internal static string Decrypt(string ciphertext, byte[] key)
    {
        var ciphertextBytes = Convert.FromBase64String(ciphertext);

        using var aes = Aes.Create();
        aes.Key = key;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        var iv = new byte[IvLength];
        Buffer.BlockCopy(ciphertextBytes, 0, iv, 0, IvLength);
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        var encryptedBytes = new byte[ciphertextBytes.Length - IvLength];
        Buffer.BlockCopy(ciphertextBytes, IvLength, encryptedBytes, 0, encryptedBytes.Length);
        var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
        return Encoding.UTF8.GetString(decryptedBytes);
    }
}
