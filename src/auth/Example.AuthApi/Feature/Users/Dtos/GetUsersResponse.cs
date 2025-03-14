using Example.AuthApi.Feature.Group.Dtos;

namespace Example.AuthApi.Feature.Users.Dtos;

public sealed record GetUsersResponse(List<UserDto> Users);
