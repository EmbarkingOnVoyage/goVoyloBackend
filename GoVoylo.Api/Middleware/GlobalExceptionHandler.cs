using FluentValidation;
using GoVoylo.Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace GoVoylo.Api.Middleware
{
    // Single place that turns any exception into the { error: { code, message, details? } }
    // envelope from the API conventions doc, so no controller needs its own try/catch.
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var (statusCode, code, message, details) = exception switch
            {
                ValidationException validationException => (
                    StatusCodes.Status400BadRequest,
                    "validation_failed",
                    "One or more fields are invalid.",
                    (object?)validationException.Errors.Select(e => new
                    {
                        field = e.PropertyName,
                        error = e.ErrorMessage
                    })),

                AppException appException => (
                    appException.StatusCode,
                    appException.Code,
                    appException.Message,
                    null),

                _ => (
                    StatusCodes.Status500InternalServerError,
                    "internal_error",
                    "Something went wrong. Please try again.",
                    null)
            };

            if (statusCode == StatusCodes.Status500InternalServerError)
            {
                // Only unexpected exceptions are logged with full detail — AppException
                // subtypes are expected control flow (a 404, a validation failure, ...).
                _logger.LogError(exception, "Unhandled exception on {Path}", httpContext.Request.Path);
            }

            httpContext.Response.StatusCode = statusCode;
            await httpContext.Response.WriteAsJsonAsync(
                new { error = new { code, message, details } },
                cancellationToken);

            return true;
        }
    }
}
