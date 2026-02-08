namespace Subsy.Domain.Options
{
    public class ConnectionStringOption
    {
        public const string Key = "ConnectionStrings";
        public string SQLite { get; set; } = default!;
    }
}
