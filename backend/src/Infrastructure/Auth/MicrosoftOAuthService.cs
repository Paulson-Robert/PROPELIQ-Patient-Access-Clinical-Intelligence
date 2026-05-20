using Domain.Enums;

namespace Infrastructure.Auth;

public sealed class MicrosoftOAuthService
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
                AuthProvider.Microsoft,
                OAuthErrorCode.InvalidCode,
                "Microsoft OAuth code was not provided."));
        }

        var normalizedCode = code.Trim().ToLowerInvariant();

        if (normalizedCode.Contains("denied", StringComparison.Ordinal))
        {
            return Task.FromResult(SocialAuthResult.Failure(
                AuthProvider.Microsoft,
                OAuthErrorCode.ConsentDenied,
                "Microsoft OAuth consent was denied by the user."));
        }

        if (normalizedCode.Contains("outage", StringComparison.Ordinal))
        {
            return Task.FromResult(SocialAuthResult.Failure(
                AuthProvider.Microsoft,
                OAuthErrorCode.ProviderUnavailable,
                "Microsoft OAuth provider is currently unavailable."));
        }

        var resolvedEmail = ResolveEmail(normalizedCode, overrideEmail);
        if (!string.IsNullOrWhiteSpace(emailHint) &&
            !resolvedEmail.Equals(emailHint.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(SocialAuthResult.Failure(
                AuthProvider.Microsoft,
                OAuthErrorCode.EmailMismatch,
                "Microsoft account email does not match the expected account."));
        }

        return Task.FromResult(SocialAuthResult.Success(AuthProvider.Microsoft, resolvedEmail));
    }

    private static string ResolveEmail(string normalizedCode, string? overrideEmail)
    {
        if (!string.IsNullOrWhiteSpace(overrideEmail))
        {
            return overrideEmail.Trim().ToLowerInvariant();
        }

        var codeHash = Math.Abs(normalizedCode.GetHashCode());
        return $"patient.microsoft.{codeHash}@example.com";
    }
}
