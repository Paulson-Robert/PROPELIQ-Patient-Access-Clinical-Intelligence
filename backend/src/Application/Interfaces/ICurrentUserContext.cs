namespace Application.Interfaces;

/// <summary>
/// Abstracts access to the current authenticated user's identity context so that
/// Application-layer pipeline behaviors remain independent of ASP.NET Core HTTP primitives.
/// </summary>
public interface ICurrentUserContext
{
    /// <summary>
    /// The authenticated user's ID, or <c>null</c> for unauthenticated / background-job callers.
    /// Background jobs should surface <c>null</c> here and set ActorRole to "System".
    /// </summary>
    Guid? ActorUserId { get; }

    /// <summary>
    /// The role of the current caller (e.g. "Admin", "Staff", "Patient", "System").
    /// Never null or empty — defaults to "System" when no principal is present.
    /// </summary>
    string ActorRole { get; }

    /// <summary>
    /// The remote IP address of the originating request.
    /// Null for background jobs or when the address cannot be determined.
    /// </summary>
    string? IpAddress { get; }
}
