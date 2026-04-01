using LateralGroup.Application.Abstractions.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace LateralGroup.Infrastructure.Services;

public class SystemClockService : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    public DateTimeOffset Now => DateTimeOffset.Now;

}
