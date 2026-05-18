using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniFlow.Entity.Entities;

namespace UniFlow.DataAccess.Configurations;

internal sealed class SyllabusConfiguration : IEntityTypeConfiguration<Syllabus>
{
    public const int MaxSourceTextPreviewLength = 4000;

    public void Configure(EntityTypeBuilder<Syllabus> builder)
    {
        builder.ToTable("Syllabi");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(e => e.SourceTextHash)
            .HasMaxLength(64);

        builder.Property(e => e.SourceTextPreview)
            .HasMaxLength(MaxSourceTextPreviewLength);

        builder.HasMany(e => e.Tasks)
            .WithOne(e => e.Syllabus)
            .HasForeignKey(e => e.SyllabusId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
