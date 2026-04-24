namespace UniFlow.Entity.Common;

/// <summary>
/// Base type for all persisted domain entities.
/// </summary>
public abstract class BaseEntity
{
    public long Id { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
