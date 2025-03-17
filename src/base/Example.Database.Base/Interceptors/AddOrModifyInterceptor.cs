using Example.Database.Base.BaseModels;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Example.Database.Base.Interceptors;

public class AddOrModifyInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        SaveChangesInternal(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        SaveChangesInternal(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    internal static void SaveChangesInternal(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var addedOrModified = context.ChangeTracker
        .Entries()
        .Where(e => e is { Entity: BaseEntity, State: EntityState.Added or EntityState.Modified });

        if (!addedOrModified.Any())
        {
            return;
        }

        foreach (var item in addedOrModified)
        {
            if (item.Entity is not BaseEntity entity)
            {
                continue;
            }

            if (item.State is EntityState.Added)
            {
                entity.Create();
            }

            if (item.State is EntityState.Modified)
            {
                entity.Modify();
            }
        }
    }
}
