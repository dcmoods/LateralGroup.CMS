namespace LateralGroup.Application.Abstractions.Services;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
    DateTimeOffset Now { get; }
}
