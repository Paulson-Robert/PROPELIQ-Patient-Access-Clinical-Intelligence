namespace Application.Interfaces;

public interface IReminderPipelineService
{
    /// <summary>
    /// Creates reminder notification records for the given appointment and schedules
    /// their Hangfire delivery jobs at 24 h and 2 h before the appointment start time.
    /// High-risk patients receive an additional SMS reminder (AC-03).
    /// </summary>
    Task ScheduleAppointmentRemindersAsync(Guid appointmentId, CancellationToken cancellationToken = default);
}
