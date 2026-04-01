using LateralGroup.Application.Abstractions.Persistence;
using LateralGroup.Application.Abstractions.Services;
using LateralGroup.Infrastructure.Persistence;
using LateralGroup.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace LateralGroup.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {

        services.AddDbContext<CmsWriteDbContext>(options =>
        {
            options.UseSqlite(configuration.GetConnectionString("CmsWriteDatabase"));
        });

        services.AddDbContext<CmsReadDbContext>(options =>
        {
            options.UseSqlite(configuration.GetConnectionString("CmsReadDatabase"));
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });

        services.AddScoped<ICmsWriteDbContext>(sp => sp.GetRequiredService<CmsWriteDbContext>());
        services.AddScoped<ICmsReadDbContext>(sp => sp.GetRequiredService<CmsReadDbContext>());

        services.AddScoped<IClock, SystemClockService>();

        return services;
    }
}
