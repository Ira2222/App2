using App2.Application.Features.Todos.Dtos;
using App2.Domain.Repositories;
using MediatR;

namespace App2.Application.Features.Todos.Queries;

public sealed class GetTodosHandler : IRequestHandler<GetTodosQuery, IReadOnlyList<TodoDto>>
{
    private readonly ITodoRepository _todoRepository;

    public GetTodosHandler(ITodoRepository todoRepository)
    {
        _todoRepository = todoRepository;
    }

    public async Task<IReadOnlyList<TodoDto>> Handle(GetTodosQuery request, CancellationToken cancellationToken)
    {
        var todos = await _todoRepository.GetAllAsync(cancellationToken);
        return todos.Select(TodoDto.FromEntity).ToList();
    }
}
