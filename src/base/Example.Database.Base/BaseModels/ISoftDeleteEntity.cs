namespace Example.Database.Base.BaseModels;

public interface ISoftDeleteEntity
{
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DateDeleted { get; set; }

    internal void Delete()
    {
        IsDeleted = true;
        DateDeleted = DateTimeOffset.UtcNow;
    }
}