using UniFlow.Entity.Common;

namespace UniFlow.Entity.Entities;

public sealed class Course : BaseEntity
{
    public long UserId { get; set; }

    public User User { get; set; } = null!;

    public string Code { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public ICollection<Syllabus> Syllabi { get; set; } = new List<Syllabus>();
}
