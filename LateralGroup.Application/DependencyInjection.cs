using LateralGroup.Application.Abstractions.Services;
using LateralGroup.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace LateralGroup.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {

        services.AddScoped<ICmsEventProcessor, CmsEventProcessor>();
        services.AddScoped<ICmsContentQueryService, CmsContentQueryService>();
        services.AddScoped<ICmsAdminService, CmsAdminService>();

        return services;
    }
}
