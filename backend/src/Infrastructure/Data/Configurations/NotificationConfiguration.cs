using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(n => n.NotificationId);

        builder.Property(n => n.Channel)
            .IsRequired();

        builder.Property(n => n.NotificationType)
            .IsRequired();

        builder.Property(n => n.Status)
            .IsRequired();

        builder.Property(n => n.FailureReason)
            .HasMaxLength(1024);

        builder.Property(n => n.CreatedAt)
            .IsRequired();

        // Many-to-one: Notification → User (patient)
        builder.HasOne(n => n.Patient)
            .WithMany()
            .HasForeignKey(n => n.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(n => n.AppointmentId);
        builder.HasIndex(n => n.PatientId);
        builder.HasIndex(n => n.Status);
        builder.HasIndex(n => n.CreatedAt);
    }
}
