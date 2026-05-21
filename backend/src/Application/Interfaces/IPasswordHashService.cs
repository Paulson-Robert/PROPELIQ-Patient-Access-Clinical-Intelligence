namespace Application.Interfaces;

public interface IPasswordHashService
{
    string HashPassword(string password);

    bool VerifyPassword(string password, string hash);

    bool VerifyAgainstTimingSafeHash(string password);
}
