using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FormBuilder.API.ExceptionHandlers;

/// <summary>
/// Handles global exceptions and writes problem details responses.
/// </summary>
public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    IHostEnvironment env)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var statusCode = StatusCodes.Status500InternalServerError;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = "An unexpected error occurred",
            Detail = env.IsDevelopment()
                ? exception.Message
                : "Please contact support"
        };

        if (env.IsDevelopment() && exception.InnerException is not null)
        {
            problemDetails.Extensions["InnerExceptionType"]
                = exception.InnerException.GetType().Name;
            problemDetails.Extensions["InnerExceptionMessage"]
                = exception.InnerException.Message;
        }

        httpContext.Response.StatusCode = statusCode;

        return await problemDetailsService.TryWriteAsync(
            new()
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = problemDetails
            });
    }
}
