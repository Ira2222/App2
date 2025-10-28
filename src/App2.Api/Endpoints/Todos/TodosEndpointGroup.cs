using App2.Api.Constants;
using App2.Application.Features.Todos.Commands;
using App2.Application.Features.Todos.Dtos;
using App2.Application.Features.Todos.Queries;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OutputCaching;

namespace App2.Api.Endpoints.Todos;

public static class TodosEndpointGroup
{
    public static IEndpointRouteBuilder MapTodosEndpoints(this IEndpointRouteBuilder builder, bool requireAuthorization)
    {
        var group = builder.MapGroup(AppConstants.Routes.TodosBase)
            .WithTags("Todos");

        var getEndpoint = group.MapGet("/", GetTodosAsync)
            .RequireRateLimiting(AppConstants.RateLimiting.FixedPolicy)
            .CacheOutput(AppConstants.Cache.TodosPolicy)
            .Produces<IReadOnlyList<TodoDto>>(StatusCodes.Status200OK);

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
            .ProducesProblem(StatusCodes.Status400BadRequest);

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
            .CacheOutput(AppConstants.Cache.TodosPolicy)
            .Produces<TodoDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

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
            .ProducesProblem(StatusCodes.Status400BadRequest);

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

    private static async Task<Results<Created<TodoDto>, ValidationProblem>> CreateTodoAsync(
        CreateTodoCommand command,
        IMediator mediator,
        IOutputCacheStore cache,
        CancellationToken cancellationToken)
    {
        try
        {
            var created = await mediator.Send(command, cancellationToken);
            await cache.EvictByTagAsync(AppConstants.Cache.TodosTag, cancellationToken);
            return TypedResults.Created($"{AppConstants.Routes.TodosBase}/{created.Id}", created);
        }
        catch (FluentValidation.ValidationException ex)
        {
            return TypedResults.ValidationProblem(ex.Errors.ToDictionary(
                failure => failure.PropertyName,
                failure => new[] { failure.ErrorMessage }));
        }
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

    private static async Task<Results<Ok<TodoDto>, NotFound, ValidationProblem>> UpdateTodoAsync(
        int id,
        UpdateTodoCommand command,
        IMediator mediator,
        IOutputCacheStore cache,
        CancellationToken cancellationToken)
    {
        try
        {
            // Ensure the ID from the route matches the command
            var commandWithId = command with { Id = id };
            var updated = await mediator.Send(commandWithId, cancellationToken);

            if (updated is null)
            {
                return TypedResults.NotFound();
            }

            await cache.EvictByTagAsync(AppConstants.Cache.TodosTag, cancellationToken);
            return TypedResults.Ok(updated);
        }
        catch (FluentValidation.ValidationException ex)
        {
            return TypedResults.ValidationProblem(ex.Errors.ToDictionary(
                failure => failure.PropertyName,
                failure => new[] { failure.ErrorMessage }));
        }
    }

    private static async Task<Results<NoContent, NotFound>> DeleteTodoAsync(
        int id,
        IMediator mediator,
        IOutputCacheStore cache,
        CancellationToken cancellationToken)
    {
        var deleted = await mediator.Send(new DeleteTodoCommand(id), cancellationToken);
        if (!deleted)
        {
            return TypedResults.NotFound();
        }

        await cache.EvictByTagAsync(AppConstants.Cache.TodosTag, cancellationToken);
        return TypedResults.NoContent();
    }
}
