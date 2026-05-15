using UniFlow.Entity.Common;
using UniFlow.Entity.Enums;

namespace UniFlow.Entity.Entities;

public sealed class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public PersonalityVibe PersonalityVibe { get; set; } = PersonalityVibe.Friendly;

    public string? Major { get; set; }

    public ICollection<Course> Courses { get; set; } = new List<Course>();
}
