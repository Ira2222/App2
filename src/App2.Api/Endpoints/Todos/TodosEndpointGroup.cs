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
}
