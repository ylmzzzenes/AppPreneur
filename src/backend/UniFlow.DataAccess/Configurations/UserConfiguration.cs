using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniFlow.Entity.Entities;
using UniFlow.Entity.Enums;

namespace UniFlow.DataAccess.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Email)
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(e => e.DisplayName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.PasswordHash)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(e => e.PersonalityVibe)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired()
            .HasDefaultValue(PersonalityVibe.Friendly);

        builder.Property(e => e.Major)
            .HasMaxLength(100);

        builder.HasIndex(e => e.Email)
            .IsUnique();

        builder.HasMany(e => e.Courses)
            .WithOne(e => e.User)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
