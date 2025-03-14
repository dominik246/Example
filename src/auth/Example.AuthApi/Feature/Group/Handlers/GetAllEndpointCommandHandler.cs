using Microsoft.EntityFrameworkCore;

using Example.AuthApi.Database;
using Example.AuthApi.Feature.Group.Dtos;
using Example.ServiceDefaults.Consts;
using FastEndpoints.Security;
using System.Text.Json;

namespace Example.AuthApi.Feature.Group.Handlers;

public sealed record GetAllEndpointCommand(string? SearchString, bool OnlySelf, bool OnlyOwned) : ICommand<GetAllResponse>;
public sealed class GetAllEndpointCommandHandler(AuthDbContext db, IHttpContextAccessor accessor) : CommandHandler<GetAllEndpointCommand, GetAllResponse>
{
    public override async Task<GetAllResponse> ExecuteAsync(GetAllEndpointCommand command, CancellationToken ct)
    {
        IQueryable<Database.Models.Group> query = db.Groups;

        if (command.OnlySelf)
        {
            var groupsFromToken = accessor.HttpContext?.User.ClaimValue(JwtClaimConsts.Groups) ?? "[]";
            var parsedGroupIds = JsonSerializer.Deserialize<List<Guid>>(groupsFromToken);

            query = query.Where(p => parsedGroupIds!.Contains(p.Id));
        }

        if (command.OnlyOwned)
        {
            var userId = accessor.HttpContext?.User.ClaimValue(JwtClaimConsts.UserId);
            if (!Guid.TryParse(userId, out var parsedUserId))
            {
                AddError("Auth Mismatch", "401");
                return new([]);
            }

            query = query.Where(p => p.CreatedBy == parsedUserId);
        }

        if (!string.IsNullOrWhiteSpace(command.SearchString))
        {
            query = query.Where(p => EF.Functions.ILike(p.Name, $"%{command.SearchString}%"));
        }

        var dbResult = await query.ToListAsync(ct);

        if (dbResult.Count is 0)
        {
            return new([]);
        }

        var mapped = dbResult.ConvertAll(p => new GroupDto(p.Id, p.Name, p.IsAdminGroup));
        return new(mapped);
    }
}