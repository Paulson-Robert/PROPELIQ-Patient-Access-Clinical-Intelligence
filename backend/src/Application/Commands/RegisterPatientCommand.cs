namespace Application.Commands;

/// <summary>
/// Payload used to register a patient or staff account with email/password credentials.
/// </summary>
public sealed record RegisterPatientCommand(
    string Email,
    string Password,
    string? FullName = null,
    string? Role = null);
