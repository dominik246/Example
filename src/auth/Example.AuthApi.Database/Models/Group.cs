using System.ComponentModel.DataAnnotations;

using Example.Database.Base.BaseModels;

namespace Example.AuthApi.Database.Models;

public sealed class Group : BaseEntity<Guid>
{
    [MaxLength(50)]
    public string Name { get; set; } = default!;
    public bool IsDeletable { get; set; } = true;
    public bool IsAdminGroup { get; set; }
    public Guid CreatedBy { get; set; }

    public ICollection<UserGroup>? UserGroups { get; set; }
}
