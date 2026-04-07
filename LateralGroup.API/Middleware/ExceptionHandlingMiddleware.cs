using System.Net;
using System.Text.Json;
using LateralGroup.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace LateralGroup.API.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, detail) = MapException(exception);

        _logger.LogError(
            exception,
            "Unhandled exception for {Method} {Path}. Returning {StatusCode}.",
            context.Request.Method,
            context.Request.Path,
            statusCode);

        if (context.Response.HasStarted)
        {
            _logger.LogWarning(
                "The response has already started, so the exception middleware will not modify the response.");
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Title = title,
            Status = statusCode,
            Detail = detail,
            Instance = context.Request.Path.ToString()
        };

        problem.Extensions["traceId"] = context.TraceIdentifier;

        if (exception is ValidationException validationException)
        {
            problem.Extensions["errors"] = validationException.Errors;
        }

        if (_environment.IsDevelopment())
        {
            problem.Extensions["exception"] = exception.GetType().Name;
        }

        await context.Response.WriteAsJsonAsync(problem);
    }

    private (int StatusCode, string Title, string? Detail) MapException(Exception exception)
    {
        return exception switch
        {

            ValidationException ex => (
                StatusCodes.Status400BadRequest,
                "Validation failed.",
                ex.Message),

            ArgumentNullException ex => (
                (int)HttpStatusCode.BadRequest,
                "Invalid request.",
                ex.Message),

            ArgumentException ex => (
                (int)HttpStatusCode.BadRequest,
                "Invalid request.",
                ex.Message),

            JsonException => (
                (int)HttpStatusCode.BadRequest,
                "Malformed JSON.",
                "The request body could not be parsed."),

            UnauthorizedAccessException => (
                (int)HttpStatusCode.Forbidden,
                "Forbidden.",
                "You do not have access to this resource."),

            _ => (
                (int)HttpStatusCode.InternalServerError,
                "An unexpected error occurred.",
                _environment.IsDevelopment() ? exception.Message : null)
        };
    }
}
