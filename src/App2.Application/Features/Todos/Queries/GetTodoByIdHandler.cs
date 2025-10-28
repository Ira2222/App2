using App2.Application.Features.Todos.Dtos;
using App2.Domain.Repositories;
using MediatR;

namespace App2.Application.Features.Todos.Queries;

public sealed class GetTodoByIdHandler : IRequestHandler<GetTodoByIdQuery, TodoDto?>
{
    private readonly ITodoRepository _repository;

    public GetTodoByIdHandler(ITodoRepository repository)
    {
        _repository = repository;
    }

    public async Task<TodoDto?> Handle(GetTodoByIdQuery request, CancellationToken cancellationToken)
    {
        var todo = await _repository.GetByIdAsync(request.Id, cancellationToken);
        return todo is not null ? TodoDto.FromEntity(todo) : null;
    }
}
