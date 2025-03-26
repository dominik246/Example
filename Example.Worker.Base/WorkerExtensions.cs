using EntityFramework.Exceptions.PostgreSQL;

using Example.Database.Base.Interceptors;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Example.Worker.Base;

public static class WorkerExtensions
{
    public static void ConfigureDb<TContext>(this IHostApplicationBuilder builder, string serverName, string dbName, bool useInterceptors = false)
        where TContext : DbContext
    {
        var connectionString = builder.Configuration.GetConnectionString(dbName);

        builder.AddNpgsqlDbContext<TContext>(
            serverName,
            x => x.ConnectionString = connectionString,
            x =>
            {
                if (useInterceptors)
                {
                    x.AddInterceptors(new SoftDeleteInterceptor(), new AddOrModifyInterceptor(), new ConcurrentInterceptor());
                }

                x.UseNpgsql();
                x.UseExceptionProcessor();
            });
    }
}
