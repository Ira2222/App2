using App2.Application.Features.Todos.Commands;
using FluentValidation;

namespace App2.Application.Features.Todos.Validators;

public sealed class CreateTodoValidator : AbstractValidator<CreateTodoCommand>
{
    public CreateTodoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(x => x.Description)
            .MaximumLength(1000);
    }
}
