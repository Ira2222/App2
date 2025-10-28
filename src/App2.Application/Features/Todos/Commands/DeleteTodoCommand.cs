using MediatR;

namespace App2.Application.Features.Todos.Commands;

public sealed record DeleteTodoCommand(int Id) : IRequest<bool>;
