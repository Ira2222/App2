using App2.Application.Features.Todos.Queries;
using App2.Domain.Entities;
using App2.Domain.Repositories;
using FluentAssertions;
using Moq;
using Xunit;

namespace App2.Tests.Unit.Features.Todos;

public class GetTodosHandlerTests
{
    private readonly Mock<ITodoRepository> _todoRepository = new();
    private readonly GetTodosHandler _handler;

    public GetTodosHandlerTests()
    {
        _handler = new GetTodosHandler(_todoRepository.Object);
    }

    [Fact]
    public async Task Handle_WhenRepositoryReturnsEmptyList_ReturnsEmptyDtoSet()
    {
        _todoRepository
            .Setup(repository => repository.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Todo>());

        var result = await _handler.Handle(new GetTodosQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenRepositoryReturnsTodos_ProjectsToDto()
    {
        var todos = new List<Todo>
        {
            new() { Id = 1, Title = "first", Description = "one" },
            new() { Id = 2, Title = "second", Description = "two" }
        };

        _todoRepository
            .Setup(repository => repository.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(todos);

        var result = await _handler.Handle(new GetTodosQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
        result.Should().ContainSingle(dto => dto.Id == 1 && dto.Title == "first");
        result.Should().ContainSingle(dto => dto.Id == 2 && dto.Description == "two");
    }
}
