namespace Application.Commands;

/// <summary>
/// Payload used to verify an email activation token.
/// </summary>
public sealed record VerifyEmailCommand(string Token, string? Email = null);
