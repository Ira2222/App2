using System;
using App2.Api.Constants;
using App2.Application.Features.Todos.Commands;
using App2.Application.Features.Todos.Dtos;
using App2.Application.Features.Todos.Queries;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace App2.Api.Endpoints.Todos;

public static class TodosEndpointGroup
{
    public static IEndpointRouteBuilder MapTodosEndpoints(this IEndpointRouteBuilder builder, bool requireAuthorization, bool enableOutputCache)
    {
        var group = builder.MapGroup(AppConstants.Routes.TodosBase)
            .WithTags("Todos");

        var getEndpoint = group.MapGet("/", GetTodosAsync)
            .RequireRateLimiting(AppConstants.RateLimiting.FixedPolicy)
            .Produces<IReadOnlyList<TodoDto>>(StatusCodes.Status200OK);

        getEndpoint = ConfigureCaching(getEndpoint, enableOutputCache);

        if (requireAuthorization)
        {
            getEndpoint.RequireAuthorization("Todo.Read");
        }
        else
        {
            getEndpoint.AllowAnonymous();
        }

        var postEndpoint = group.MapPost("/", CreateTodoAsync)
            .RequireRateLimiting(AppConstants.RateLimiting.FixedPolicy)
            .Produces<TodoDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        if (requireAuthorization)
        {
            postEndpoint.RequireAuthorization("Todo.Write");
        }
        else
        {
            postEndpoint.AllowAnonymous();
        }

        var getByIdEndpoint = group.MapGet("/{id:int}", GetTodoByIdAsync)
            .RequireRateLimiting(AppConstants.RateLimiting.FixedPolicy)
            .Produces<TodoDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        getByIdEndpoint = ConfigureCaching(getByIdEndpoint, enableOutputCache);

        if (requireAuthorization)
        {
            getByIdEndpoint.RequireAuthorization("Todo.Read");
        }
        else
        {
            getByIdEndpoint.AllowAnonymous();
        }

        var putEndpoint = group.MapPut("/{id:int}", UpdateTodoAsync)
            .RequireRateLimiting(AppConstants.RateLimiting.FixedPolicy)
            .Produces<TodoDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        if (requireAuthorization)
        {
            putEndpoint.RequireAuthorization("Todo.Write");
        }
        else
        {
            putEndpoint.AllowAnonymous();
        }

        var deleteEndpoint = group.MapDelete("/{id:int}", DeleteTodoAsync)
            .RequireRateLimiting(AppConstants.RateLimiting.FixedPolicy)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        if (requireAuthorization)
        {
            deleteEndpoint.RequireAuthorization("Todo.Write");
        }
        else
        {
            deleteEndpoint.AllowAnonymous();
        }

        return builder;
    }

    private static async Task<Ok<IReadOnlyList<TodoDto>>> GetTodosAsync(IMediator mediator, CancellationToken cancellationToken)
    {
        var todos = await mediator.Send(new GetTodosQuery(), cancellationToken);
        return TypedResults.Ok(todos);
    }

    private static async Task<Created<TodoDto>> CreateTodoAsync(
        CreateTodoCommand command,
        IMediator mediator,
        IServiceProvider services,
        CancellationToken cancellationToken)
    {
        var created = await mediator.Send(command, cancellationToken);
        await EvictTodosCacheAsync(services, cancellationToken);
        return TypedResults.Created($"{AppConstants.Routes.TodosBase}/{created.Id}", created);
    }

    private static async Task<Results<Ok<TodoDto>, NotFound>> GetTodoByIdAsync(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var todo = await mediator.Send(new GetTodoByIdQuery(id), cancellationToken);
        return todo is not null
            ? TypedResults.Ok(todo)
            : TypedResults.NotFound();
    }

    private static async Task<Results<Ok<TodoDto>, NotFound>> UpdateTodoAsync(
        int id,
        UpdateTodoCommand command,
        IMediator mediator,
        IServiceProvider services,
        CancellationToken cancellationToken)
    {
        // Ensure the ID from the route matches the command
        var commandWithId = command with { Id = id };
        var updated = await mediator.Send(commandWithId, cancellationToken);

        if (updated is null)
        {
            return TypedResults.NotFound();
        }

        await EvictTodosCacheAsync(services, cancellationToken);
        return TypedResults.Ok(updated);
    }

    private static async Task<Results<NoContent, NotFound>> DeleteTodoAsync(
        int id,
        IMediator mediator,
        IServiceProvider services,
        CancellationToken cancellationToken)
    {
        var deleted = await mediator.Send(new DeleteTodoCommand(id), cancellationToken);
        if (!deleted)
        {
            return TypedResults.NotFound();
        }

        await EvictTodosCacheAsync(services, cancellationToken);
        return TypedResults.NoContent();
    }

    private static RouteHandlerBuilder ConfigureCaching(RouteHandlerBuilder builder, bool enableOutputCache)
        => enableOutputCache ? builder.CacheOutput(AppConstants.Cache.TodosPolicy) : builder;

    private static async Task EvictTodosCacheAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        var cache = services.GetService<IOutputCacheStore>();
        if (cache is null)
        {
            return;
        }

        await cache.EvictByTagAsync(AppConstants.Cache.TodosTag, cancellationToken);
    }
}
