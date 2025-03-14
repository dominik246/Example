using Example.Database.Base.BaseModels;

namespace Example.AuthApi.Database.Models;

public sealed class UserGroup : BaseEntity
{
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public Guid GroupId { get; set; }
    public Group? Group { get; set; }
}
