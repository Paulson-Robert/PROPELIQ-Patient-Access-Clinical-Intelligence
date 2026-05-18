using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

/// <summary>
/// AuditLog is append-only (ADD-8, NFR-005).
/// The database-level immutability trigger is applied in the migration.
/// </summary>
public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(l => l.AuditLogId);

        builder.Property(l => l.AuditLogId)
            .UseIdentityAlwaysColumn(); // PostgreSQL GENERATED ALWAYS AS IDENTITY

        builder.Property(l => l.Timestamp)
            .IsRequired();

        builder.Property(l => l.ActorRole)
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(l => l.ActionType)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(l => l.ResourceType)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(l => l.ResourceId)
            .IsRequired()
            .HasMaxLength(128);

        // JSONB column for arbitrary action details
        builder.Property(l => l.Details)
            .HasColumnType("jsonb");

        builder.Property(l => l.IpAddress)
            .HasMaxLength(45); // IPv6 max length

        builder.HasIndex(l => l.ActorUserId);
        builder.HasIndex(l => l.Timestamp);
        builder.HasIndex(l => l.ActionType);
        builder.HasIndex(l => new { l.ResourceType, l.ResourceId });
    }
}
