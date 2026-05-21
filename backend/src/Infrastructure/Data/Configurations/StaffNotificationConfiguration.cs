using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class StaffNotificationConfiguration : IEntityTypeConfiguration<StaffNotification>
{
    public void Configure(EntityTypeBuilder<StaffNotification> builder)
    {
        builder.HasKey(n => n.StaffNotificationId);

        builder.Property(n => n.Title)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(n => n.Message)
            .HasMaxLength(2048);

        builder.Property(n => n.Variant)
            .IsRequired();

        builder.Property(n => n.IsRead)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(n => n.CreatedAt)
            .IsRequired();

        builder.HasOne(n => n.Staff)
            .WithMany()
            .HasForeignKey(n => n.StaffUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(n => n.StaffUserId);
        builder.HasIndex(n => n.CreatedAt);
        // Composite index supports the common "unread notifications per user" query
        builder.HasIndex(n => new { n.StaffUserId, n.IsRead });
    }
}
