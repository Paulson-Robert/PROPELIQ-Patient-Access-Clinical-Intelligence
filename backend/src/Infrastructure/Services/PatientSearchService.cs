using Application.Interfaces;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

/// <summary>
/// EF Core implementation for walk-in patient search by demographics.
/// </summary>
public sealed class PatientSearchService : IPatientSearchService
{
    private readonly ApplicationDbContext _db;

    public PatientSearchService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<PatientSearchResultDto>> SearchAsync(
        string name,
        DateOnly? dateOfBirth,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return [];
        }

        var term = name.Trim().ToLowerInvariant();

        var query = _db.PatientProfiles
            .AsNoTracking()
            .Where(p =>
                p.User.Role == UserRole.Patient
                && p.User.IsActive
                && (p.FirstName.ToLower().Contains(term)
                    || p.LastName.ToLower().Contains(term)
                    || (p.FirstName + " " + p.LastName).ToLower().Contains(term)
                    || p.User.Email.ToLower().Contains(term)
                    || (p.Phone != null && p.Phone.ToLower().Contains(term))));

        if (dateOfBirth.HasValue)
        {
            query = query.Where(p => p.DateOfBirth == dateOfBirth.Value);
        }

        return await query
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .Select(p => new PatientSearchResultDto(
                p.UserId,
                p.FirstName,
                p.LastName,
                p.DateOfBirth,
                p.User.Email,
                p.Phone))
            .Take(25)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
