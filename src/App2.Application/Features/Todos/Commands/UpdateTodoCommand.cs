using App2.Application.Features.Todos.Dtos;
using MediatR;

namespace App2.Application.Features.Todos.Commands;

public sealed record UpdateTodoCommand : IRequest<TodoDto?>
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsCompleted { get; init; }
}
