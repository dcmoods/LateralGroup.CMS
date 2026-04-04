using LateralGroup.Application.Abstractions.Persistence;
using LateralGroup.Application.Abstractions.Services;
using LateralGroup.Infrastructure.Persistence;
using LateralGroup.Infrastructure.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

namespace LateralGroup.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var writeConnectionString = BuildConnectionString(configuration, readOnly: false);
        var readConnectionString = BuildConnectionString(configuration, readOnly: true);

        services.AddDbContext<CmsWriteDbContext>(options =>
        {
            options.UseSqlite(writeConnectionString);
        });

        services.AddDbContext<CmsReadDbContext>(options =>
        {
            options.UseSqlite(readConnectionString);
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });

        services.AddScoped<ICmsWriteDbContext>(sp => sp.GetRequiredService<CmsWriteDbContext>());
        services.AddScoped<ICmsReadDbContext>(sp => sp.GetRequiredService<CmsReadDbContext>());

        services.AddScoped<IClock, SystemClockService>();

        return services;
    }

    private static string BuildConnectionString(IConfiguration configuration, bool readOnly)
    {
        var configuredConnectionString = configuration.GetConnectionString("CmsDatabase")
            ?? throw new InvalidOperationException("Connection string 'CmsDatabase' is not configured.");

        var builder = new SqliteConnectionStringBuilder(configuredConnectionString)
        {
            Cache = SqliteCacheMode.Shared
        };

        var dataSource = builder.DataSource;
        if (string.IsNullOrWhiteSpace(dataSource))
        {
            throw new InvalidOperationException("Connection string 'CmsDatabase' must include a Data Source.");
        }

        if (!Path.IsPathRooted(dataSource))
        {
            dataSource = Path.GetFullPath(dataSource, AppContext.BaseDirectory);
        }

        var directory = Path.GetDirectoryName(dataSource);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        builder.DataSource = dataSource;
        builder.Mode = readOnly ? SqliteOpenMode.ReadOnly : SqliteOpenMode.ReadWriteCreate;

        return builder.ToString();
    }
}
