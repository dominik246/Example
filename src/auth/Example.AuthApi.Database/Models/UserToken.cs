using System.ComponentModel.DataAnnotations;

using Example.Database.Base.BaseModels;

namespace Example.AuthApi.Database.Models;

public sealed class UserToken : BaseEntity<Guid>
{
    public Guid UserId { get; set; }
    public User? User { get; set; }

    [MaxLength(32)]
    public string RefreshToken { get; set; } = default!;

    [MaxLength(512)]
    public string AccessToken { get; set; } = default!;
    public DateTimeOffset AccessExpiry { get; set; }
    public DateTimeOffset RefreshExpiry { get; set; }
}
