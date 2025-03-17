using Example.Database.Base.BaseModels;

using System.ComponentModel.DataAnnotations;

namespace Example.AuthApi.Database.Models;

public sealed class UserEmailConfirm : BaseEntity<Guid>
{
    public Guid UserId { get; set; }
    public User? User { get; set; }

    [StringLength(32)]
    public string Hash { get; set; } = default!;
}
