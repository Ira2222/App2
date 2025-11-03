using System.ComponentModel.DataAnnotations;

namespace App2.Api.Options;

public class AzureAdOptions
{
    public const string SectionName = "AzureAd";

    [Required]
    public string Instance { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^(?!.*TODO).*$", ErrorMessage = "TenantId must be configured (contains 'TODO')")]
    public string TenantId { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^(?!.*TODO).*$", ErrorMessage = "ClientId must be configured (contains 'TODO')")]
    public string ClientId { get; set; } = string.Empty;

    public string? Audience { get; set; }
}
