using App2.Application.Features.Todos.Commands;
using App2.Application.Features.Todos.Validators;
using FluentAssertions;
using Xunit;

namespace App2.Tests.Unit.Features.Todos.Validators;

public class CreateTodoValidatorTests
{
    private readonly CreateTodoValidator _validator = new();

    [Fact]
    public void Validate_ShouldFail_WhenTitleEmpty()
    {
        var command = new CreateTodoCommand(string.Empty, null);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_ShouldFail_WhenDescriptionTooLong()
    {
        var command = new CreateTodoCommand("Valid title", new string('a', 1200));

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_ShouldPass_ForValidPayload()
    {
        var command = new CreateTodoCommand("Valid", "desc");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }
}
