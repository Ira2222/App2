using System.Linq;
using System.Reflection;
using App2.Api.Endpoints.Todos;
using App2.Api.Extensions;
using App2.Application.Common.Behaviors;
using App2.Application.Features.Todos.Commands;
using App2.Application.Features.Todos.Validators;
using App2.Domain.Repositories;
using App2.Infrastructure.Data;
using App2.Infrastructure.Repositories;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Logging
builder.Host.UseSerilog((ctx, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());

// Optional Key Vault bootstrap
EnableKeyVaultIfConfigured(builder);

builder.Services.AddHttpContextAccessor();
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        // Handle FluentValidation exceptions centrally
        if (context.Exception is ValidationException validationException)
        {
            var errors = validationException.Errors
                .GroupBy(failure => failure.PropertyName, failure => failure.ErrorMessage)
                .ToDictionary(group => group.Key, group => group.ToArray());

            context.ProblemDetails = new HttpValidationProblemDetails(errors)
            {
                Type = "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.1",
                Title = "One or more validation errors occurred.",
                Status = StatusCodes.Status400BadRequest,
                Instance = context.HttpContext.Request.Path
            };

            // Set the response status code to ensure the exception handler uses 400 not 500
            context.HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }

        context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
    };
});
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Todo.Read", policy => policy.RequireScope("Todo.Read"));
    options.AddPolicy("Todo.Write", policy => policy.RequireScope("Todo.Write"));
});

builder.Services.AddMediatR(typeof(CreateTodoCommand).Assembly);
builder.Services.AddValidatorsFromAssemblyContaining<CreateTodoValidator>();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddScoped<ITodoRepository, TodoRepository>();

var features = builder.Configuration.GetSection("Features");

builder.Services.AddAppData(builder.Configuration, builder.Environment);
builder.Services.AddOutputCacheWithOptionalRedis(builder.Configuration);

var authenticationEnabled = false;
var devAuthEnabled = false;

if (features.GetValue("Authentication", false))
{
    builder.Services.AddJwtAuthentication(builder.Configuration);
    authenticationEnabled = true;
}
else if (builder.Configuration.GetValue("Security:DevHeader:Enabled", false) && !builder.Environment.IsProduction())
{
    builder.Services.AddDevelopmentAuthFallback(builder.Configuration);
    devAuthEnabled = true;
}

if (features.GetValue("CORS", false))
{
    builder.Services.AddCorsAllowList(builder.Configuration);
}

if (features.GetValue("RateLimiting", false))
{
    builder.Services.AddRequestRateLimiting(builder.Configuration);
}

if (features.GetValue("SecurityHeaders", false))
{
    builder.Services.AddAppSecurityHeaders();
}

if (features.GetValue("OpenTelemetry", false))
{
    builder.Services.AddAppObservability(builder.Configuration);
}

builder.Services.AddHealthChecksWithSplit();

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseHttpsRedirection();

if (features.GetValue("SecurityHeaders", false))
{
    app.UseSecurityHeaders(builder.Configuration);
}

if (features.GetValue("CORS", false))
{
    app.UseCors("AllowedOrigins");
}

if (authenticationEnabled || devAuthEnabled)
{
    app.UseAuthentication();
    app.UseAuthorization();
}

if (features.GetValue("RateLimiting", false))
{
    app.UseRateLimiter();
}

if (features.GetValue("OutputCaching", false))
{
    app.UseOutputCache();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => Results.Ok(new
{
    app = "App2",
    version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0"
}));

app.MapHealthEndpoints();
app.MapTodosEndpoints(authenticationEnabled);
app.MapControllers();

app.ValidateRequiredConfiguration(builder.Configuration, app.Environment);

app.Run();

static void EnableKeyVaultIfConfigured(WebApplicationBuilder builder)
{
    var useKeyVault = Environment.GetEnvironmentVariable("USE_KEYVAULT");
    if (!string.Equals(useKeyVault, "true", StringComparison.OrdinalIgnoreCase))
    {
        return;
    }

    var vaultUri = builder.Configuration["KeyVault:VaultUri"];
    if (string.IsNullOrWhiteSpace(vaultUri))
    {
        return;
    }

    var cloud = builder.Configuration["Hosting:Cloud"] ?? "Public";
    var credentialOptions = new DefaultAzureCredentialOptions
    {
        AuthorityHost = cloud.Equals("USGov", StringComparison.OrdinalIgnoreCase)
            ? AzureAuthorityHosts.AzureGovernment
            : AzureAuthorityHosts.AzurePublicCloud
    };

    var credential = new DefaultAzureCredential(credentialOptions);
    builder.Configuration.AddAzureKeyVault(new Uri(vaultUri), credential, new AzureKeyVaultConfigurationOptions
    {
        ReloadInterval = TimeSpan.FromMinutes(5)
    });
}

internal record CreateTodoRequest(string Title, string? Description);

public partial class Program;
