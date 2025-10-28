using App2.Application.Features.Todos.Commands;
using App2.Domain.Repositories;
using FluentAssertions;
using Moq;
using Xunit;

namespace App2.Tests.Unit.Features.Todos.Commands;

public class DeleteTodoHandlerTests
{
    private readonly Mock<ITodoRepository> _repositoryMock = new();
    private readonly DeleteTodoHandler _handler;

    public DeleteTodoHandlerTests()
    {
        _handler = new DeleteTodoHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsTrue_WhenRepositoryDeletesTodo()
    {
        _repositoryMock
            .Setup(repo => repo.DeleteAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.Handle(new DeleteTodoCommand(7), CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ReturnsFalse_WhenRepositoryReturnsFalse()
    {
        _repositoryMock
            .Setup(repo => repo.DeleteAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.Handle(new DeleteTodoCommand(3), CancellationToken.None);

        result.Should().BeFalse();
    }
}
