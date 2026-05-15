using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniFlow.Entity.Entities;

namespace UniFlow.DataAccess.Configurations;

internal sealed class SyllabusScanSessionConfiguration : IEntityTypeConfiguration<SyllabusScanSession>
{
    private const int MaxPreviewJsonLength = 512_000;

    public void Configure(EntityTypeBuilder<SyllabusScanSession> builder)
    {
        builder.ToTable("SyllabusScanSessions");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.CourseCode)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.CourseTitle)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.SourceTextHash)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(e => e.PreviewJson)
            .HasMaxLength(MaxPreviewJsonLength)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.ExpiresAt)
            .IsRequired();

        builder.HasIndex(e => new { e.UserId, e.ExpiresAt });

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
