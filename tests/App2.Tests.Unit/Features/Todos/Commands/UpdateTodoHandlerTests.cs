using App2.Application.Features.Todos.Commands;
using App2.Application.Features.Todos.Dtos;
using App2.Domain.Entities;
using App2.Domain.Repositories;
using FluentAssertions;
using Moq;
using Xunit;

namespace App2.Tests.Unit.Features.Todos.Commands;

public class UpdateTodoHandlerTests
{
    private readonly Mock<ITodoRepository> _repositoryMock = new();
    private readonly UpdateTodoHandler _handler;

    public UpdateTodoHandlerTests()
    {
        _handler = new UpdateTodoHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsNull_WhenTodoNotFound()
    {
        _repositoryMock
            .Setup(repo => repo.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Todo?)null);

        var result = await _handler.Handle(new UpdateTodoCommand { Id = 1, Title = "Title", Description = null, IsCompleted = false }, CancellationToken.None);

        result.Should().BeNull();
        _repositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Todo>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_SetsCompletedAt_WhenMarkingAsCompleted()
    {
        // Arrange
        var existing = new Todo
        {
            Id = 5,
            Title = "Old",
            Description = "Desc",
            IsCompleted = false,
            CreatedAt = DateTimeOffset.UtcNow,
            ModifiedAt = DateTimeOffset.UtcNow
        };

        _repositoryMock
            .Setup(repo => repo.GetByIdAsync(existing.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        _repositoryMock
            .Setup(repo => repo.UpdateAsync(It.IsAny<Todo>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Todo todo, CancellationToken _) => todo);

        var command = new UpdateTodoCommand { Id = existing.Id, Title = "New Title", Description = "New Desc", IsCompleted = true };

        // Act
        TodoDto? result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.IsCompleted.Should().BeTrue();
        result.CompletedAt.Should().NotBeNull();
        existing.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ClearsCompletedAt_WhenMarkingAsIncomplete()
    {
        // Arrange
        var existing = new Todo
        {
            Id = 6,
            Title = "Completed",
            Description = "Desc",
            IsCompleted = true,
            CompletedAt = DateTimeOffset.UtcNow.AddDays(-1),
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-2),
            ModifiedAt = DateTimeOffset.UtcNow.AddDays(-1)
        };

        _repositoryMock
            .Setup(repo => repo.GetByIdAsync(existing.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        _repositoryMock
            .Setup(repo => repo.UpdateAsync(It.IsAny<Todo>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Todo todo, CancellationToken _) => todo);

        var command = new UpdateTodoCommand { Id = existing.Id, Title = "Completed", Description = "Still here", IsCompleted = false };

        // Act
        TodoDto? result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.IsCompleted.Should().BeFalse();
        result.CompletedAt.Should().BeNull();
        existing.CompletedAt.Should().BeNull();
    }
}
