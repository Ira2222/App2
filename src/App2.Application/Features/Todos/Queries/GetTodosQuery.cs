using App2.Application.Features.Todos.Dtos;
using MediatR;

namespace App2.Application.Features.Todos.Queries;

public sealed record GetTodosQuery() : IRequest<IReadOnlyList<TodoDto>>;
