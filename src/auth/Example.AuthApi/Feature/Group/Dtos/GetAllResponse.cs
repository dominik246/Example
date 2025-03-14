namespace Example.AuthApi.Feature.Group.Dtos;

public sealed record GetAllResponse(List<GroupDto> Groups);

public sealed record GroupDto(Guid Id, string Name, bool IsAdminGroup);
