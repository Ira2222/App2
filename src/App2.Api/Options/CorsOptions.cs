using System.ComponentModel.DataAnnotations;

namespace App2.Api.Options;

public class CorsOptions
{
    public const string SectionName = "Cors";

    [Required]
    [MinLength(1, ErrorMessage = "At least one allowed origin must be configured in production")]
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();

    public string[]? AllowedMethods { get; set; }

    public string[]? AllowedHeaders { get; set; }
}
