namespace Example.AuthApi.Feature.Group.Dtos;

public sealed record UpdateRequest(Guid Id, string Name, bool IsAdminGroup, List<Guid> UserIds);
