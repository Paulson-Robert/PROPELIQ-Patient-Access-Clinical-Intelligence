using Infrastructure.Data.Extensions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Persistence;

/// <summary>
/// Named EF Core <see cref="ValueConverter{TModel,TProvider}"/> that applies AES-256-CBC
/// encryption to a non-nullable string PHI column (AC-01).
/// Delegates to <see cref="PgCryptoExtensions"/> so the cryptographic implementation
/// remains in a single place.
/// </summary>
public sealed class EncryptedValueConverter : ValueConverter<string, string>
{
    public EncryptedValueConverter(byte[] key)
        : base(
            plaintext => PgCryptoExtensions.Encrypt(plaintext, key),
            ciphertext => PgCryptoExtensions.Decrypt(ciphertext, key))
    {
    }
}

/// <summary>
/// Named EF Core <see cref="ValueConverter{TModel,TProvider}"/> that applies AES-256-CBC
/// encryption to a nullable string PHI column (AC-01).
/// Null values are stored as NULL in the database (not encrypted).
/// </summary>
public sealed class NullableEncryptedValueConverter : ValueConverter<string?, string?>
{
    public NullableEncryptedValueConverter(byte[] key)
        : base(
            plaintext => plaintext == null ? null : PgCryptoExtensions.Encrypt(plaintext, key),
            ciphertext => ciphertext == null ? null : PgCryptoExtensions.Decrypt(ciphertext, key))
    {
    }
}
