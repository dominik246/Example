using Microsoft.EntityFrameworkCore;

using Example.AuthApi.Database;
using Example.AuthApi.Feature.Group.Dtos;

namespace Example.AuthApi.Feature.Group.Handlers;

public sealed record GetByIdCommand(Guid Id) : ICommand<GetByIdResponse?>;
public sealed class GetByIdCommandHandler(AuthDbContext db) : CommandHandler<GetByIdCommand, GetByIdResponse?>
{
    public override async Task<GetByIdResponse?> ExecuteAsync(GetByIdCommand command, CancellationToken ct = default)
    {
        var dbEntry = await db.UserGroups.Where(p => p.GroupId == command.Id).Include(p => p.User).Include(p => p.Group).ToListAsync(ct);

        if (dbEntry is { Count: 0 })
        {
            AddError("Entry with the provided Id does not exist.");
            return null;
        }

        var mapped = dbEntry.ConvertAll(p => new UserDto(p.User!.Id, p.User.Email, p.User.IsDisabled, p.User.EmailConfirmed));
        
        var group = dbEntry[0].Group!;
        return new GetByIdResponse(new(group.Id, group.Name, group.IsAdminGroup), mapped);
    }
}