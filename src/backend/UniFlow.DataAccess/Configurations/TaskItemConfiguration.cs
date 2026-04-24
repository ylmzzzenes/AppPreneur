using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniFlow.Entity.Entities;

namespace UniFlow.DataAccess.Configurations;

internal sealed class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.ToTable("TaskItems");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(4000);

        builder.Property(e => e.Category)
            .HasMaxLength(128);
    }
}
