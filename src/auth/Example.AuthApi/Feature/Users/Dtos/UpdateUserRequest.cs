namespace Example.AuthApi.Feature.Users.Dtos;

public sealed record UpdateUserRequest(Guid Id, bool IsDisabled, bool IsEmailConfirmed);
