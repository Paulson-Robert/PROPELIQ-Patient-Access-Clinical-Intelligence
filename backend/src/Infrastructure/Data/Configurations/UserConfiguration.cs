using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.UserId);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.PasswordHash)
            .HasMaxLength(512);

        builder.Property(u => u.MfaMethod)
            .IsRequired();

        // TOTP seed — encryption converter applied in ApplicationDbContext (AC-02)
        builder.Property(u => u.MfaSecret)
            .HasMaxLength(512); // Max covers base64-encoded ciphertext

        builder.Property(u => u.MfaPhoneNumber)
            .HasMaxLength(512); // Max covers encrypted contact data

        builder.Property(u => u.Role)
            .IsRequired();

        builder.Property(u => u.AuthProvider)
            .IsRequired();

        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.Property(u => u.UpdatedAt)
            .IsRequired();

        // One-to-one: User → PatientProfile
        builder.HasOne(u => u.PatientProfile)
            .WithOne(p => p.User)
            .HasForeignKey<PatientProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // One-to-many: User (as patient) → Appointments
        builder.HasMany(u => u.PatientAppointments)
            .WithOne(a => a.Patient)
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        // One-to-many: User (as creator) → Appointments
        builder.HasMany(u => u.CreatedAppointments)
            .WithOne(a => a.CreatedByUser)
            .HasForeignKey(a => a.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // One-to-many: User → AuditLogs
        // Restrict prevents hard-deleting a user who has audit entries, preserving
        // the immutable audit history (ADD-8, NFR-005). Use soft-delete (IsActive=false) instead.
        builder.HasMany(u => u.AuditLogs)
            .WithOne(l => l.ActorUser)
            .HasForeignKey(l => l.ActorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // One-to-many: User → CalendarSyncs
        builder.HasMany(u => u.CalendarSyncs)
            .WithOne(c => c.User)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(u => u.CreatedAt);
    }
}
