using Domain.Enums;

namespace Infrastructure.Auth;

public enum OAuthErrorCode
{
    None = 0,
    InvalidCode = 1,
    ConsentDenied = 2,
    ProviderUnavailable = 3,
    EmailMismatch = 4,
}

public sealed record SocialAuthResult(
    bool IsSuccess,
    AuthProvider Provider,
    string? Email,
    OAuthErrorCode ErrorCode,
    string? ErrorDescription)
{
    public static SocialAuthResult Success(AuthProvider provider, string email) =>
        new(true, provider, email, OAuthErrorCode.None, null);

    public static SocialAuthResult Failure(
        AuthProvider provider,
        OAuthErrorCode errorCode,
        string errorDescription) =>
        new(false, provider, null, errorCode, errorDescription);
}
