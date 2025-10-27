using App2.Domain.Entities;

namespace App2.Application.Features.Todos.Dtos;

public sealed record TodoDto(int Id, string Title, string? Description, bool IsCompleted, DateTimeOffset CreatedAt, DateTimeOffset? CompletedAt)
{
    public static TodoDto FromEntity(Todo entity) => new(entity.Id, entity.Title, entity.Description, entity.IsCompleted, entity.CreatedAt, entity.CompletedAt);
}
