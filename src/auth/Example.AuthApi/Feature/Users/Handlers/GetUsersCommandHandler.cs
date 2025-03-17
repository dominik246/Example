using Example.AuthApi.Database;
using Example.AuthApi.Database.Models;
using Example.AuthApi.Feature.Group.Dtos;
using Example.AuthApi.Feature.Users.Dtos;

using Microsoft.EntityFrameworkCore;

namespace Example.AuthApi.Feature.Users.Handlers;

public sealed record GetUsersCommand(string? SearchString, bool OnlyEnabled, bool OnlyDisabled) : ICommand<GetUsersResponse>;
public sealed class GetUsersCommandHandler(AuthDbContext db) : CommandHandler<GetUsersCommand, GetUsersResponse>
{
    public override async Task<GetUsersResponse> ExecuteAsync(GetUsersCommand command, CancellationToken ct)
    {
        IQueryable<User> dbEntries = db.Users;

        if (command.OnlyEnabled)
        {
            dbEntries = dbEntries.Where(p => !p.IsDisabled);
        }

        if (command.OnlyDisabled)
        {
            dbEntries = dbEntries.Where(p => p.IsDisabled);
        }

        if (!string.IsNullOrWhiteSpace(command.SearchString))
        {
            dbEntries = dbEntries.Where(p => EF.Functions.ILike(p.Email, $"%{command.SearchString}%"));
        }

        var result = await dbEntries.ToListAsync(ct);

        if (result is { Count: 0 })
        {
            return new([]);
        }

        var mapped = result.ConvertAll(p => new UserDto(p.Id, p.Email, p.IsDisabled, p.EmailConfirmed));

        return new(mapped);
    }
}