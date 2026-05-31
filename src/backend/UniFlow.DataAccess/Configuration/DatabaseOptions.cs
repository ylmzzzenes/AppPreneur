namespace UniFlow.DataAccess.Configuration;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    /// <summary>
    /// <see cref="DatabaseProviders.SqlServer"/> or <see cref="DatabaseProviders.PostgreSql"/>.
    /// </summary>
    public string Provider { get; set; } = DatabaseProviders.SqlServer;
}

public static class DatabaseProviders
{
    public const string SqlServer = "SqlServer";

    public const string PostgreSql = "PostgreSql";
}
