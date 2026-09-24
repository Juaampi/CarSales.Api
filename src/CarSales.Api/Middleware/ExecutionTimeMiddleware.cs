using System.Diagnostics;

namespace CarSales.Api.Middleware;

public class ExecutionTimeMiddleware(
    RequestDelegate next,
    ILogger<ExecutionTimeMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // El finally garantiza que también midamos requests que terminan con una excepción.
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();

            logger.LogInformation(
                "{Method} {Path} executed in {ElapsedMilliseconds} ms",
                context.Request.Method,
                context.Request.Path,
                stopwatch.ElapsedMilliseconds);
        }
    }
}
