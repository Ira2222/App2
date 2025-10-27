using App2.Domain.Entities;
using App2.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace App2.Tests.Integration;

public class ModularFeaturesTests : IClassFixture<TestingWebApplicationFactory>
{
    private readonly TestingWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ModularFeaturesTests(TestingWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("http://localhost")
        });

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();
        if (!db.Todos.Any())
        {
            db.Todos.Add(new Todo { Title = "Seed todo", Description = "integration" });
            db.SaveChanges();
        }
    }

    [Fact]
    public async Task LiveHealth_ReturnsHealthy()
    {
        var response = await _client.GetAsync("/healthz/live");
        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task ReadyHealth_ReturnsHealthy()
    {
        var response = await _client.GetAsync("/healthz/ready");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task TodosEndpoint_IsRateLimited()
    {
        var tasks = Enumerable.Range(0, 120)
            .Select(_ => _client.GetAsync("/api/todos"));

        var results = await Task.WhenAll(tasks);
        Assert.Contains(results, r => r.StatusCode == HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task SecurityHeaders_ArePresent()
    {
        var response = await _client.GetAsync("/api/todos");
        response.EnsureSuccessStatusCode();

        Assert.True(response.Headers.TryGetValues("X-Content-Type-Options", out var nosniff));
        Assert.Contains("nosniff", nosniff, StringComparer.OrdinalIgnoreCase);

        Assert.True(response.Headers.TryGetValues("X-Frame-Options", out var frame));
        Assert.Contains("DENY", frame, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SwaggerUi_AccessibleInDevelopment()
    {
        var response = await _client.GetAsync("/swagger/index.html");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task CreateTodo_ReturnsCreated()
    {
        var request = new { title = "integration todo", description = "test" };
        var response = await _client.PostAsJsonAsync("/api/todos", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var todo = await response.Content.ReadFromJsonAsync<TodoResponse>();
        Assert.NotNull(todo);
        Assert.Equal("integration todo", todo!.Title);
    }

    [Fact]
    public async Task CreateTodo_WithInvalidPayload_ReturnsBadRequest()
    {
        var request = new { title = "", description = new string('a', 2000) };
        var response = await _client.PostAsJsonAsync("/api/todos", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateTodo_ThenGetTodos_ReturnsInsertedItem()
    {
        var title = $"todo-{Guid.NewGuid():N}";
        var createResponse = await _client.PostAsJsonAsync("/api/todos", new { title, description = "from integration" });
        createResponse.EnsureSuccessStatusCode();

        var todosFirst = await _client.GetFromJsonAsync<List<TodoResponse>>("/api/todos");
        Assert.Contains(todosFirst!, t => t.Title == title);

        // second call should hit the output cache and still contain the item
        var todosSecond = await _client.GetFromJsonAsync<List<TodoResponse>>("/api/todos");
        Assert.Contains(todosSecond!, t => t.Title == title);
    }

    private sealed record TodoResponse(int Id, string Title, string? Description, bool IsCompleted, DateTimeOffset CreatedAt, DateTimeOffset? CompletedAt);
}
