using System.Security.Cryptography;
using System.Text;
using CarSales.Api.Configuration;
using Microsoft.Extensions.Options;

namespace CarSales.Api.Middleware;

public class ApiKeyMiddleware(
    RequestDelegate next,
    IOptions<ApiKeyOptions> options)
{
    private const string ApiKeyHeaderName = "X-API-Key";
    private const string UnauthorizedMessage = "API Key inválida o ausente.";

    public async Task InvokeAsync(HttpContext context)
    {
        if (IsPublicPath(context.Request.Path))
        {
            await next(context);
            return;
        }

        var configuredKey = options.Value.Key;
        var providedKey = context.Request.Headers[ApiKeyHeaderName].FirstOrDefault();

        if (!IsValidKey(providedKey, configuredKey))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { message = UnauthorizedMessage });
            return;
        }

        await next(context);
    }

    private static bool IsPublicPath(PathString path)
    {
        return path.StartsWithSegments("/openapi") || path.StartsWithSegments("/swagger");
    }

    private static bool IsValidKey(string? providedKey, string configuredKey)
    {
        if (string.IsNullOrEmpty(providedKey) || string.IsNullOrEmpty(configuredKey))
        {
            return false;
        }

        var providedBytes = Encoding.UTF8.GetBytes(providedKey);
        var configuredBytes = Encoding.UTF8.GetBytes(configuredKey);

        return providedBytes.Length == configuredBytes.Length &&
            CryptographicOperations.FixedTimeEquals(providedBytes, configuredBytes);
    }
}
