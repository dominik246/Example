using System.ComponentModel.DataAnnotations;

using Example.Database.Base.BaseModels;

namespace Example.AuthApi.Database.Models;

public sealed class User : BaseEntity<Guid>
{
    [EmailAddress, MaxLength(320)]
    public string Email { get; set; } = default!;

    [MaxLength(1024)]
    public string PasswordHash { get; set; } = default!;
    public bool EmailConfirmed { get; set; }

    public bool IsDisabled { get; set; }

    public ICollection<UserGroup>? UserGroups { get; set; }
}
