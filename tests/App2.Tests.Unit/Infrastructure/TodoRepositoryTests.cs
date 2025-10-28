using App2.Domain.Entities;
using App2.Infrastructure.Data;
using App2.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace App2.Tests.Unit.Infrastructure;

public class TodoRepositoryTests
{
    [Fact]
    public async Task AddAsync_PersistsEntityWithAuditing()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDbContext(options);
        var repository = new TodoRepository(context);

        var todo = new Todo { Title = "unit-test", Description = "audit" };

        var created = await repository.AddAsync(todo, CancellationToken.None);

        created.Id.Should().BeGreaterThan(0);
        created.CreatedAt.Should().NotBe(default);
        created.ModifiedAt.Should().NotBe(default);
        created.CreatedBy.Should().Be("system");
        created.ModifiedBy.Should().Be("system");

        var all = await repository.GetAllAsync(CancellationToken.None);
        all.Should().ContainSingle(t => t.Title == "unit-test");
    }
}
