namespace App2.Domain.Abstractions;

public interface IAuditable
{
    DateTimeOffset CreatedAt { get; set; }
    DateTimeOffset ModifiedAt { get; set; }
    string? CreatedBy { get; set; }
    string? ModifiedBy { get; set; }
    string? CreatedByIp { get; set; }
    string? ModifiedByIp { get; set; }
}
