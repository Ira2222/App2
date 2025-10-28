using App2.Application.Features.Todos.Commands;
using App2.Application.Features.Todos.Validators;
using FluentAssertions;
using Xunit;

namespace App2.Tests.Unit.Features.Todos.Validators;

public class UpdateTodoValidatorTests
{
    private readonly UpdateTodoValidator _validator = new();

    [Fact]
    public void Validate_ShouldFail_WhenIdIsNotPositive()
    {
        var command = new UpdateTodoCommand { Id = 0, Title = "Title", Description = "Desc", IsCompleted = false };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(UpdateTodoCommand.Id));
    }

    [Fact]
    public void Validate_ShouldFail_WhenTitleEmpty()
    {
        var command = new UpdateTodoCommand { Id = 1, Title = string.Empty, Description = null, IsCompleted = false };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateTodoCommand.Title));
    }

    [Fact]
    public void Validate_ShouldFail_WhenTitleTooLong()
    {
        var command = new UpdateTodoCommand { Id = 1, Title = new string('a', 201), Description = null, IsCompleted = false };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_ShouldFail_WhenDescriptionTooLong()
    {
        var command = new UpdateTodoCommand { Id = 1, Title = "Title", Description = new string('a', 1001), IsCompleted = false };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_ShouldPass_ForValidCommand()
    {
        var command = new UpdateTodoCommand { Id = 1, Title = "Valid", Description = "Desc", IsCompleted = true };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }
}
