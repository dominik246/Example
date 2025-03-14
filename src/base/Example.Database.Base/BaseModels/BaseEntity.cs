using System.ComponentModel.DataAnnotations;

namespace Example.Database.Base.BaseModels;

public abstract class BaseEntity<T> : BaseEntity where T : unmanaged
{
    [Key]
    public T Id { get; init; }
}

public abstract class BaseEntity : ITimeTrackedEntity, IConcurrentEntity
{
    public DateTimeOffset DateCreated { get; set; }
    public DateTimeOffset? DateModified { get; set; }

    [Timestamp]
    public uint RowVersion { get; set; }

    internal void Create()
    {
        DateCreated = DateTimeOffset.UtcNow;
    }

    internal void Modify()
    {
        DateModified = DateTimeOffset.UtcNow;
    }
}
