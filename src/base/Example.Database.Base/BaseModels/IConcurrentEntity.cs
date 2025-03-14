namespace Example.Database.Base.BaseModels;

public interface IConcurrentEntity
{
    public uint RowVersion { get; internal set; }
}