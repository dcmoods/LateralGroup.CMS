using LateralGroup.Application.Abstractions.Services;

namespace LateralGroup.Application.Tests;

internal sealed class TestClock(DateTimeOffset utcNow) : IClock
{
    public DateTimeOffset UtcNow { get; set; } = utcNow;
    public DateTimeOffset Now => UtcNow.ToLocalTime();
}
