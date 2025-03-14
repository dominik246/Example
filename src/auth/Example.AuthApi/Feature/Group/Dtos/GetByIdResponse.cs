namespace Example.AuthApi.Feature.Group.Dtos;

public sealed record GetByIdResponse(GroupDto Group, List<UserDto> Users);
