using App2.Application.Features.Todos.Dtos;
using MediatR;

namespace App2.Application.Features.Todos.Queries;

public sealed record GetTodoByIdQuery(int Id) : IRequest<TodoDto?>;
