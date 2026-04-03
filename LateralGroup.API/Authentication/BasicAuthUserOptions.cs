namespace LateralGroup.API.Authentication;

public sealed class BasicAuthUserOptions
{
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string[] Roles { get; init; } = [];
}
