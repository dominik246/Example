using Example.Database.Base.BaseModels;

using System.ComponentModel.DataAnnotations;

namespace Example.AuthApi.Database.Models;

public sealed class UserPasswordRestore : BaseEntity<Guid>
{
    public Guid UserId { get; set; }
    public User? User { get; set; }

    [MaxLength(512)]
    public string SecurityCode { get; set; } = default!;
}
