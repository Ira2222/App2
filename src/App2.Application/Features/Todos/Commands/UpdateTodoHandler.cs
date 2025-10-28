using App2.Application.Features.Todos.Dtos;
using App2.Domain.Repositories;
using MediatR;

namespace App2.Application.Features.Todos.Commands;

public sealed class UpdateTodoHandler : IRequestHandler<UpdateTodoCommand, TodoDto?>
{
    private readonly ITodoRepository _repository;

    public UpdateTodoHandler(ITodoRepository repository)
    {
        _repository = repository;
    }

    public async Task<TodoDto?> Handle(UpdateTodoCommand request, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (existing is null)
        {
            return null;
        }

        existing.Title = request.Title;
        existing.Description = request.Description;
        existing.IsCompleted = request.IsCompleted;

        if (request.IsCompleted && existing.CompletedAt is null)
        {
            existing.CompletedAt = DateTimeOffset.UtcNow;
        }
        else if (!request.IsCompleted)
        {
            existing.CompletedAt = null;
        }

        var updated = await _repository.UpdateAsync(existing, cancellationToken);
        return TodoDto.FromEntity(updated);
    }
}
