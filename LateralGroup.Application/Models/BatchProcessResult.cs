namespace LateralGroup.Application.Models;

public sealed class BatchProcessResult
{
    public int Received { get; init; }
    public int Processed { get; init; }
    public int Ignored { get; init; }
    public int Failed { get; init; }
}
