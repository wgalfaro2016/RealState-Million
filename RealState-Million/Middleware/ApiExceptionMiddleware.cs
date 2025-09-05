using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using RealEstate.Application.Common.Exceptions;

namespace RealState_Million.Middleware;

public class ApiExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiExceptionMiddleware> _logger;

    public ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger) {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext ctx) {
        try {
            await _next(ctx);
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Unhandled exception");
            await WriteProblemAsync(ctx, ex);
        }
    }

    private static async Task WriteProblemAsync(HttpContext ctx, Exception ex) {
        var problem = new ProblemDetails {
            Instance = ctx.Request.Path
        };

        switch (ex) {
            case NotFoundException nf:
                ctx.Response.StatusCode = (int)HttpStatusCode.NotFound;
                problem.Title = "Resource not found";
                problem.Detail = nf.Message;
                problem.Status = ctx.Response.StatusCode;
                break;

            case ValidationException ve:
                ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                problem.Title = "Validation error";
                problem.Detail = "One or more validation errors occurred.";
                problem.Status = ctx.Response.StatusCode;
                problem.Extensions["errors"] = ve.Errors
                    .Select(e => new { e.PropertyName, e.ErrorMessage });
                break;

            case UnauthorizedAccessException:
                ctx.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                problem.Title = "Unauthorized";
                problem.Detail = "You are not authorized to perform this action.";
                problem.Status = ctx.Response.StatusCode;
                break;

            case InvalidOperationException ioe:
                ctx.Response.StatusCode = (int)HttpStatusCode.NotFound;
                problem.Title = "Resource not found";
                problem.Detail = ioe.Message;
                problem.Status = ctx.Response.StatusCode;
                break;

            default:
                ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                problem.Title = "Internal server error";
                problem.Detail = ex.Message; 
                problem.Status = ctx.Response.StatusCode;
                break;
        }

        ctx.Response.ContentType = "application/problem+json";
        await ctx.Response.WriteAsJsonAsync(problem);
    }
}
