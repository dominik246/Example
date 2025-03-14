using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

using Example.Database.Base.BaseModels;

namespace Example.Database.Base.Interceptors;

public class ConcurrentInterceptor : SaveChangesInterceptor
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

        var modifiedEntities = context.ChangeTracker
        .Entries()
        .Where(e => e is { Entity: IConcurrentEntity, State: EntityState.Modified });

        if (!modifiedEntities.Any())
        {
            return;
        }

        foreach (var entityEntry in modifiedEntities)
        {
            // TODO: test if zero or just remove                       - \/
            if (entityEntry.Entity is not IConcurrentEntity { RowVersion: 0 } entity)
            {
                continue;
            }

            if (entityEntry.State is EntityState.Modified)
            {
                entityEntry.Property(nameof(IConcurrentEntity.RowVersion)).OriginalValue = entity.RowVersion;
            }
        }
    }
}