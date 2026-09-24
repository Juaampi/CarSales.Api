using CarSales.Api.Configuration;
using CarSales.Api.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace CarSales.Tests;

public class ApiKeyMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_CorrectApiKey_ExecutesNextMiddleware()
    {
        var context = CreateContext("/api/Sale/total", "development-api-key");
        var nextExecuted = false;
        var middleware = CreateMiddleware(_ =>
        {
            nextExecuted = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        Assert.True(nextExecuted);
        Assert.NotEqual(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_IncorrectApiKey_ReturnsUnauthorizedWithoutExecutingNext()
    {
        var context = CreateContext("/api/Sale/total", "incorrect-key");
        var nextExecuted = false;
        var middleware = CreateMiddleware(_ =>
        {
            nextExecuted = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        Assert.False(nextExecuted);
    }

    [Fact]
    public async Task InvokeAsync_MissingApiKey_ReturnsUnauthorizedWithoutExecutingNext()
    {
        var context = CreateContext("/api/Sale/total");
        var nextExecuted = false;
        var middleware = CreateMiddleware(_ =>
        {
            nextExecuted = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        Assert.False(nextExecuted);
    }

    [Fact]
    public async Task InvokeAsync_EmptyApiKey_ReturnsUnauthorized()
    {
        var context = CreateContext("/api/Sale/total", string.Empty);
        var middleware = CreateMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_OpenApiPath_DoesNotRequireApiKey()
    {
        var context = CreateContext("/openapi/v1.json");
        var nextExecuted = false;
        var middleware = CreateMiddleware(_ =>
        {
            nextExecuted = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        Assert.True(nextExecuted);
        Assert.NotEqual(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
    }

    private static ApiKeyMiddleware CreateMiddleware(RequestDelegate next)
    {
        return new ApiKeyMiddleware(
            next,
            Options.Create(new ApiKeyOptions { Key = "development-api-key" }));
    }

    private static DefaultHttpContext CreateContext(string path, string? apiKey = null)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;

        if (apiKey is not null)
        {
            context.Request.Headers["X-API-Key"] = apiKey;
        }

        return context;
    }
}