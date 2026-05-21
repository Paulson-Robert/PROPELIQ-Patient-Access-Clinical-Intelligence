using Application.Interfaces;

namespace Infrastructure.Auth;

public sealed class PasswordHashService : IPasswordHashService
{
    private const int WorkFactor = 12;
    private const string TimingSafeHash = "$2a$12$6Y8g1G6AmQIJLqVY68A6w.CzWjV5ok0j6mzK4WzKf2iX8rYI8aJ8e";

    public string HashPassword(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    public bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
            return false;

        return BCrypt.Net.BCrypt.Verify(password, hash);
    }

    public bool VerifyAgainstTimingSafeHash(string password)
    {
        if (string.IsNullOrEmpty(password))
            return false;

        return BCrypt.Net.BCrypt.Verify(password, TimingSafeHash);
    }
}
