using App2.Application.Features.Todos.Commands;
using App2.Application.Features.Todos.Dtos;
using App2.Domain.Entities;
using App2.Domain.Repositories;
using FluentAssertions;
using Moq;
using Xunit;

namespace App2.Tests.Unit.Features.Todos.Commands;

public class CreateTodoHandlerTests
{
    private readonly Mock<ITodoRepository> _todoRepositoryMock = new();
    private readonly CreateTodoHandler _handler;

    public CreateTodoHandlerTests()
    {
        _handler = new CreateTodoHandler(_todoRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldPersistAndReturnTodoDto()
    {
        // Arrange
        var command = new CreateTodoCommand("Test todo", null);
        var savedTodo = new Todo { Id = 1, Title = command.Title, CreatedAt = DateTimeOffset.UtcNow };
        _todoRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Todo>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(savedTodo);

        // Act
        TodoDto result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Title.Should().Be(command.Title);
        result.Id.Should().Be(savedTodo.Id);
        _todoRepositoryMock.Verify(repo => repo.AddAsync(It.Is<Todo>(t => t.Title == command.Title), It.IsAny<CancellationToken>()), Times.Once);
    }
}
