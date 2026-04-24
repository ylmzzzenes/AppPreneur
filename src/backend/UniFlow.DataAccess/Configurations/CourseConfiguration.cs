using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniFlow.Entity.Entities;

namespace UniFlow.DataAccess.Configurations;

internal sealed class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("Courses");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Code)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(e => e.Title)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(4000);

        builder.HasMany(e => e.Syllabi)
            .WithOne(e => e.Course)
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
