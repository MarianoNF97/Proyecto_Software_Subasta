using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SubastaYa.Application.Exceptions;

namespace SubastaYa.Api.Middlewares;

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
            _logger.LogError(ex, "Excepción capturada por el Middleware: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var code = HttpStatusCode.InternalServerError;
        string message = "Ocurrió un error interno en el servidor.";

        switch (exception)
        {
            case NotFoundException notFound:
                code = HttpStatusCode.NotFound;
                message = notFound.Message;
                break;

            case BusinessValidationException:
            case InsufficientFundsException:
            case ArgumentException:
                code = HttpStatusCode.BadRequest;
                message = exception.Message;
                break;

            case DbUpdateConcurrencyException:
                code = HttpStatusCode.Conflict;
                message = "Conflicto de concurrencia detectado. La subasta o billetera fue modificada simultáneamente. Intente nuevamente.";
                break;

            case InvalidOperationException invalidOp:
                code = HttpStatusCode.BadRequest;
                message = invalidOp.Message;
                break;
        }

        var response = new
        {
            status = (int)code,
            error = message,
            timestamp = DateTime.UtcNow
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}