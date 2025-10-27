using App2.Application.Features.Todos.Dtos;
using App2.Domain.Entities;
using App2.Domain.Repositories;
using MediatR;

namespace App2.Application.Features.Todos.Commands;

public sealed class CreateTodoHandler : IRequestHandler<CreateTodoCommand, TodoDto>
{
    private readonly ITodoRepository _todoRepository;

    public CreateTodoHandler(ITodoRepository todoRepository)
    {
        _todoRepository = todoRepository;
    }

    public async Task<TodoDto> Handle(CreateTodoCommand request, CancellationToken cancellationToken)
    {
        var todo = new Todo
        {
            Title = request.Title,
            Description = request.Description,
            IsCompleted = false
        };

        var created = await _todoRepository.AddAsync(todo, cancellationToken);
        return TodoDto.FromEntity(created);
    }
}
