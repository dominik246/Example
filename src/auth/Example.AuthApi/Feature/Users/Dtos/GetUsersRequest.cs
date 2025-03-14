namespace Example.AuthApi.Feature.Users.Dtos;

public sealed record GetUsersRequest(string? SearchString, bool OnlyEnabled = true, bool OnlyDisabled = false);
