namespace Application.Commands;

/// <summary>
/// Payload used to register a patient account with email/password credentials.
/// </summary>
public sealed record RegisterPatientCommand(string Email, string Password, string? FullName = null);
