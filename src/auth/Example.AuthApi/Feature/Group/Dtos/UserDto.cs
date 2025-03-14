
namespace Example.AuthApi.Feature.Group.Dtos;

public sealed record UserDto(Guid Id, string Email, bool IsDisabled, bool IsEmailConfirmed);
