using App2.Domain.Abstractions;

namespace App2.Domain.Entities;

public class Todo : IAuditable
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ModifiedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? ModifiedBy { get; set; }
    public string? CreatedByIp { get; set; }
    public string? ModifiedByIp { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
}
