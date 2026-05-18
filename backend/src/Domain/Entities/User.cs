using Domain.Enums;

namespace Domain.Entities;

public class User
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public AuthProvider AuthProvider { get; set; }
    public UserRole Role { get; set; }
    public bool MfaEnabled { get; set; }
    public string? MfaSecret { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public PatientProfile? PatientProfile { get; set; }
    public ICollection<Appointment> CreatedAppointments { get; set; } = [];
    public ICollection<Appointment> PatientAppointments { get; set; } = [];
    public ICollection<AuditLog> AuditLogs { get; set; } = [];
    public ICollection<CalendarSync> CalendarSyncs { get; set; } = [];
}
