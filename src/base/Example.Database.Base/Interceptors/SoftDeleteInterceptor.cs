using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

using Example.Database.Base.BaseModels;

namespace Example.Database.Base.Interceptors;

public class SoftDeleteInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        SaveChangesInternal(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        SaveChangesInternal(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    internal static void SaveChangesInternal(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var deletedEntities = context.ChangeTracker
        .Entries()
        .Where(e => e is { Entity: ISoftDeleteEntity, State: EntityState.Deleted });

        if (!deletedEntities.Any())
        {
            return;
        }

        foreach (var entityEntry in deletedEntities)
        {
            if (entityEntry.Entity is not ISoftDeleteEntity entity)
            {
                continue;
            }

            if (entityEntry.State is EntityState.Deleted)
            {
                entity.Delete();
                entityEntry.State = EntityState.Modified;
            }
        }
    }
}