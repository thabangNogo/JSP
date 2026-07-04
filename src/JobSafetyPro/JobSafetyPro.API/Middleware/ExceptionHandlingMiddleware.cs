using System.Net;
using System.Text.Json;
using FluentValidation;
using JobSafetyPro.Application.Common.Models;
using JobSafetyPro.Domain.Exceptions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace JobSafetyPro.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IWebHostEnvironment environment)
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, response) = exception switch
        {
            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                CreateProblem(validationEx.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()),
                    "Validation failed")),

            ValidationAppException appValidationEx => (
                HttpStatusCode.BadRequest,
                CreateProblem(appValidationEx.Errors, "Validation failed")),

            NotFoundException notFoundEx => (
                HttpStatusCode.NotFound,
                ApiResponse<object>.Fail(notFoundEx.Message)),

            UnauthorizedAppException unauthorizedEx => (
                HttpStatusCode.Unauthorized,
                ApiResponse<object>.Fail(unauthorizedEx.Message)),

            ForbiddenAppException forbiddenEx => (
                HttpStatusCode.Forbidden,
                ApiResponse<object>.Fail(forbiddenEx.Message)),

            _ => (
                HttpStatusCode.InternalServerError,
                _environment.IsDevelopment()
                    ? new
                    {
                        success = false,
                        message = exception.Message,
                        stackTrace = exception.StackTrace
                    }
                    : ApiResponse<object>.Fail("An unexpected error occurred."))
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;
        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private static object CreateProblem(IDictionary<string, string[]> errors, string message) =>
        new { success = false, message, errors };
}
