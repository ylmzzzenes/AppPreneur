using UniFlow.Entity.Common;

namespace UniFlow.Entity.Entities;

public sealed class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public ICollection<Course> Courses { get; set; } = new List<Course>();
}
