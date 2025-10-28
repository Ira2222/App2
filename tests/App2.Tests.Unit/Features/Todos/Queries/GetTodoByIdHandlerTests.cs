using App2.Application.Features.Todos.Dtos;
using App2.Application.Features.Todos.Queries;
using App2.Domain.Entities;
using App2.Domain.Repositories;
using FluentAssertions;
using Moq;
using Xunit;

namespace App2.Tests.Unit.Features.Todos.Queries;

public class GetTodoByIdHandlerTests
{
    private readonly Mock<ITodoRepository> _repositoryMock = new();
    private readonly GetTodoByIdHandler _handler;

    public GetTodoByIdHandlerTests()
    {
        _handler = new GetTodoByIdHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsTodoDto_WhenEntityExists()
    {
        // Arrange
        var entity = new Todo
        {
            Id = 42,
            Title = "Test",
            Description = "Description",
            IsCompleted = false,
            CreatedAt = DateTimeOffset.UtcNow,
            ModifiedAt = DateTimeOffset.UtcNow
        };

        _repositoryMock
            .Setup(repo => repo.GetByIdAsync(entity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        // Act
        TodoDto? result = await _handler.Handle(new GetTodoByIdQuery(entity.Id), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(entity.Id);
        result.Title.Should().Be(entity.Title);
        _repositoryMock.Verify(repo => repo.GetByIdAsync(entity.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ReturnsNull_WhenEntityDoesNotExist()
    {
        // Arrange
        _repositoryMock
            .Setup(repo => repo.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Todo?)null);

        // Act
        var result = await _handler.Handle(new GetTodoByIdQuery(999), CancellationToken.None);

        // Assert
        result.Should().BeNull();
        _repositoryMock.Verify(repo => repo.GetByIdAsync(999, It.IsAny<CancellationToken>()), Times.Once);
    }
}
