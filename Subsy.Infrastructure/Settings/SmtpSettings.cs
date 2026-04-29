namespace Subsy.Infrastructure.Settings;

public sealed class SmtpSettings
{
    public string Host { get; init; } = "localhost";
    public int Port { get; init; } = 587;
    public string From { get; init; } = "noreply@subsy.app";
    public string Username { get; init; } = "";
    public string Password { get; init; } = "";
}
