using System.ComponentModel.DataAnnotations;

namespace App2.Api.Options;

public class RedisOptions
{
    public const string SectionName = "Redis";

    [Required(ErrorMessage = "Redis:ConnectionString is required when Redis features are enabled")]
    public string ConnectionString { get; set; } = string.Empty;

    public string InstanceName { get; set; } = "app2:oc:";
}
