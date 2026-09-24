using CarSales.Application.Exceptions;

namespace CarSales.Api.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (BusinessException exception)
        {
            await WriteErrorResponseAsync(context, StatusCodes.Status400BadRequest, exception.Message);
        }
        catch (Exception)
        {
            await WriteErrorResponseAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "Ocurrió un error interno en el servidor.");
        }
    }

    private static async Task WriteErrorResponseAsync(
        HttpContext context,
        int statusCode,
        string message)
    {
        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(new { message });
    }
}
