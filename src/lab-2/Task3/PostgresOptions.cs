namespace Task3;

public class PostgresOptions
{
    public string Host { get; set; } = string.Empty;

    public int Port { get; set; }

    public int PageSize { get; set; }

    public string Database { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string ConnectionString => $"Database={Database};Host={Host};Port={Port};Username={Username};Password={Password};Enlist=false";
}