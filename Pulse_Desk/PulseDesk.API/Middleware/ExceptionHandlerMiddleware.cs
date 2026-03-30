using PulseDesk.Domain.Exceptions;
using PulseDesk.Shared.Wrappers;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace PulseDesk.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var (statusCode, response) = exception switch
            {
                FluentValidation.ValidationException validationEx => (
                 HttpStatusCode.BadRequest,
                 ApiResponse<object>.Fail(
                     validationEx.Errors
                         .Select(e => e.ErrorMessage)
                         .Distinct()
                         .ToList()
                 )
             ),

                ApplicationException appEx => (
                HttpStatusCode.BadRequest, ApiResponse<object>.Fail(appEx.Message)
                ),

                // Resource Not found exception
                KeyNotFoundException keyNotFoundEx => (
                HttpStatusCode.NotFound,
                ApiResponse<Object>.Fail(keyNotFoundEx.Message)),

                // ── Our custom domain exceptions ──────────────────────────
                DomainException domainEx => (
                    HttpStatusCode.BadRequest,
                    ApiResponse<object>.Fail(domainEx.Message)
                ),

                NotFoundException notFoundEx => (
                    HttpStatusCode.NotFound,
                    ApiResponse<object>.Fail(notFoundEx.Message)
                ),

                ForbiddenException forbiddenEx => (
                    HttpStatusCode.Forbidden,
                    ApiResponse<object>.Fail(forbiddenEx.Message)
                ),

                UnauthorizedAccessException unauthorizedEx => (
                    HttpStatusCode.Unauthorized,
                    ApiResponse<object>.Fail(unauthorizedEx.Message)
                ),

                // Everything else
                _ => (
             HttpStatusCode.InternalServerError,
                ApiResponse<object>.Fail(
                    context.RequestServices
                    .GetRequiredService<IWebHostEnvironment>()
                    .IsDevelopment() ? exception.Message :
                    "An unexpected error occurred"))
            };

            if (statusCode == HttpStatusCode.InternalServerError)
                _logger.LogError(exception, "Unhandled Exception on {method} {path}", context.Request.Method, context.Request.Path);
            else
                _logger.LogWarning("Handle Exception [{statusCode}] on {method} {Path}: {Message}", (int)statusCode, context.Request.Method, context.Request.Path, exception.Message);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            });

            await context.Response.WriteAsync(json);
        }
    }
}
