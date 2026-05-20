using Domain.Enums;

namespace Infrastructure.Auth;

public sealed class GoogleOAuthService
{
    public Task<SocialAuthResult> ExchangeCodeAsync(
        string? code,
        string? emailHint,
        string? overrideEmail,
        CancellationToken cancellationToken)
    {
        _ = cancellationToken;

        if (string.IsNullOrWhiteSpace(code))
        {
            return Task.FromResult(SocialAuthResult.Failure(
                AuthProvider.Google,
                OAuthErrorCode.InvalidCode,
                "Google OAuth code was not provided."));
        }

        var normalizedCode = code.Trim().ToLowerInvariant();

        if (normalizedCode.Contains("denied", StringComparison.Ordinal))
        {
            return Task.FromResult(SocialAuthResult.Failure(
                AuthProvider.Google,
                OAuthErrorCode.ConsentDenied,
                "Google OAuth consent was denied by the user."));
        }

        if (normalizedCode.Contains("outage", StringComparison.Ordinal))
        {
            return Task.FromResult(SocialAuthResult.Failure(
                AuthProvider.Google,
                OAuthErrorCode.ProviderUnavailable,
                "Google OAuth provider is currently unavailable."));
        }

        var resolvedEmail = ResolveEmail(normalizedCode, overrideEmail);
        if (!string.IsNullOrWhiteSpace(emailHint) &&
            !resolvedEmail.Equals(emailHint.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(SocialAuthResult.Failure(
                AuthProvider.Google,
                OAuthErrorCode.EmailMismatch,
                "Google account email does not match the expected account."));
        }

        return Task.FromResult(SocialAuthResult.Success(AuthProvider.Google, resolvedEmail));
    }

    private static string ResolveEmail(string normalizedCode, string? overrideEmail)
    {
        if (!string.IsNullOrWhiteSpace(overrideEmail))
        {
            return overrideEmail.Trim().ToLowerInvariant();
        }

        var codeHash = Math.Abs(normalizedCode.GetHashCode());
        return $"patient.google.{codeHash}@example.com";
    }
}
