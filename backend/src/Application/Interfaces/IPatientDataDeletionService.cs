namespace Application.Interfaces;

/// <summary>
/// Request payload for irreversible patient data deletion.
/// </summary>
public sealed record DeletePatientDataRequest(
    Guid PatientProfileId,
    Guid? ActorUserId,
    string ActorRole,
    string? IpAddress = null);

/// <summary>
/// Summary of resources removed by a patient deletion operation.
/// </summary>
public sealed record PatientDataDeletionResult(
    bool PatientFound,
    IReadOnlyDictionary<string, int> DeletedRows,
    long DeletedCacheKeys);

/// <summary>
/// Deletes all patient-scoped data from persistence stores.
/// </summary>
public interface IPatientDataDeletionService
{
    Task<PatientDataDeletionResult> DeletePatientDataAsync(
        DeletePatientDataRequest request,
        CancellationToken cancellationToken = default);
}