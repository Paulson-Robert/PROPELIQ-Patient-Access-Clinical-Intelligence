using Application.Commands;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Handlers;

/// <summary>
/// Persists the swap preference to the DB and enqueues it in Redis (AC-01).
/// </summary>
public sealed class RegisterSwapPreferenceCommandHandler
    : IRequestHandler<RegisterSwapPreferenceCommand, bool>
{
    private readonly ApplicationDbContext _db;
    private readonly ISwapQueueService _swapQueue;
    private readonly ILogger<RegisterSwapPreferenceCommandHandler> _logger;

    public RegisterSwapPreferenceCommandHandler(
        ApplicationDbContext db,
        ISwapQueueService swapQueue,
        ILogger<RegisterSwapPreferenceCommandHandler> logger)
    {
        _db = db;
        _swapQueue = swapQueue;
        _logger = logger;
    }

    public async Task<bool> Handle(
        RegisterSwapPreferenceCommand request,
        CancellationToken cancellationToken)
    {
        // Guard: appointment must belong to the requesting patient and be active
        var appointment = await _db.Appointments
            .FirstOrDefaultAsync(
                a => a.AppointmentId == request.AppointmentId
                  && a.PatientId == request.PatientUserId
                  && a.Status == AppointmentStatus.Scheduled,
                cancellationToken)
            .ConfigureAwait(false);

        if (appointment is null)
        {
            _logger.LogWarning(
                "RegisterSwapPreference rejected: appointment {AppointmentId} not found or not Scheduled for patient {PatientUserId}.",
                request.AppointmentId, request.PatientUserId);
            return false;
        }

        // Idempotency guard: skip duplicate waiting preference for same appointment + slot
        var alreadyQueued = await _db.PreferredSlotQueues
            .AnyAsync(
                q => q.AppointmentId == request.AppointmentId
                  && q.PreferredSlotId == request.PreferredSlotId
                  && q.Status == QueueStatus.Waiting,
                cancellationToken)
            .ConfigureAwait(false);

        if (alreadyQueued)
        {
            _logger.LogDebug(
                "Duplicate swap preference suppressed. AppointmentId={AppointmentId}, PreferredSlotId={PreferredSlotId}.",
                request.AppointmentId, request.PreferredSlotId);
            return false;
        }

        var requestedAt = DateTime.UtcNow;
        var queueEntry = new PreferredSlotQueue
        {
            QueueId = Guid.NewGuid(),
            AppointmentId = request.AppointmentId,
            PreferredSlotId = request.PreferredSlotId,
            RequestedAt = requestedAt,
            Status = QueueStatus.Waiting,
        };

        _db.PreferredSlotQueues.Add(queueEntry);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await _swapQueue.EnqueueAsync(
            queueEntry.QueueId,
            request.AppointmentId,
            request.PatientUserId,
            request.PreferredSlotId,
            requestedAt,
            cancellationToken)
            .ConfigureAwait(false);

        _logger.LogInformation(
            "Swap preference registered. QueueId={QueueId}, AppointmentId={AppointmentId}, PreferredSlotId={PreferredSlotId}.",
            queueEntry.QueueId, request.AppointmentId, request.PreferredSlotId);

        return true;
    }
}
