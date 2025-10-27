using App2.Application.Features.Todos.Dtos;
using MediatR;

namespace App2.Application.Features.Todos.Commands;

public sealed record CreateTodoCommand(string Title, string? Description) : IRequest<TodoDto>;
