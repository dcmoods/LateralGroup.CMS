namespace LateralGroup.API.Authentication;

public sealed class BasicAuthOptions
{
    public const string SectionName = "BasicAuth";

    public BasicAuthUserOptions Cms { get; init; } = new();
    public BasicAuthUserOptions Consumer { get; init; } = new();
    public BasicAuthUserOptions Admin { get; init; } = new();
}
