using System;
using System.ComponentModel.DataAnnotations;

namespace App2.Api.Options;

public sealed class DatabaseOptions
{
    public const string SectionName = "Data";

    [Required]
    [RegularExpression("Sqlite|Postgres", ErrorMessage = "Data:Provider must be either 'Sqlite' or 'Postgres'.")]
    public string Provider { get; set; } = "Sqlite";

    [Required]
    public ConnectionStringsOptions ConnectionStrings { get; set; } = new();

    public sealed class ConnectionStringsOptions
    {
        public string? Sqlite { get; set; }

        public string? Postgres { get; set; }
    }

    public string? GetConnectionStringForProvider()
        => Provider.Equals("Postgres", StringComparison.OrdinalIgnoreCase)
            ? ConnectionStrings.Postgres
            : ConnectionStrings.Sqlite;
}
