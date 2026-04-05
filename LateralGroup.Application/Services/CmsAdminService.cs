using LateralGroup.Application.Abstractions.Persistence;
using LateralGroup.Application.Abstractions.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace LateralGroup.Application.Services;

public class CmsAdminService : ICmsAdminService
{
    private readonly ICmsWriteDbContext _writeDbContext;
    private readonly ILogger<CmsAdminService> _logger;

    public CmsAdminService(ICmsWriteDbContext writeDbContext, ILogger<CmsAdminService> logger)
    {
        _writeDbContext = writeDbContext;
        _logger = logger;
    }

    public async Task DisableAsync(string id, CancellationToken cancellationToken = default)
    {
      throw new NotImplementedException();

    }

    public Task EnableAsync(string id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
