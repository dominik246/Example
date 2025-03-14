using EntityFramework.Exceptions.PostgreSQL;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

using System.Linq.Expressions;

using System.Reflection;

using Example.Database.Base.BaseModels;

namespace Example.Database.Base;

public static class DbContextExtensions
{
    public static void ConfigureBuilder(DbContextOptionsBuilder optionsBuilder)
    {
        if (EF.IsDesignTime)
        {
            optionsBuilder.UseNpgsql();
        }
    }

    public static void CreateModel(ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            ConfigureSoftDelete(entity, modelBuilder);
        }
    }

    private static readonly MethodInfo EFPropertyMethod = typeof(EF).GetMethod(nameof(EF.Property))!.MakeGenericMethod(typeof(bool));
    private static readonly ConstantExpression IsDeletedConstant = Expression.Constant(nameof(ISoftDeleteEntity.IsDeleted));
    internal static void ConfigureSoftDelete(IMutableEntityType entity, ModelBuilder modelBuilder)
    {
        if (entity.FindProperty(nameof(ISoftDeleteEntity.IsDeleted)) is null)
        {
            return;
        }

        var param = Expression.Parameter(entity.ClrType, "arg");
        var call = Expression.Call(EFPropertyMethod, arguments: [param, IsDeletedConstant]);
        var lambda = Expression.Lambda(Expression.Not(call), param);

        // p => !EF.Property<bool>(p, nameof(ISoftDeleteEntity.IsDeleted))
        modelBuilder.Entity(entity.Name).HasQueryFilter(lambda);
    }
}
