using System.Text;
using System.Text.Json;
using CarSales.Api.Middleware;
using Microsoft.AspNetCore.Http;

namespace CarSales.Tests;

public class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_UnexpectedException_ReturnsGenericInternalServerError()
    {
        var context = CreateContext();
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new Exception("unexpected details"));

        await middleware.InvokeAsync(context);

        var response = await ReadResponseAsync(context);

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.Contains("Ocurrió un error interno en el servidor.", response);
        Assert.DoesNotContain("unexpected details", response);
    }

    [Fact]
    public async Task InvokeAsync_UnexpectedException_ReturnsInternalServerErrorWithoutDetails()
    {
        var context = CreateContext();
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new Exception("error interno"));

        await middleware.InvokeAsync(context);

        var response = await ReadResponseAsync(context);

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        using var errorDocument = JsonDocument.Parse(response);
        var message = errorDocument.RootElement.GetProperty("message").GetString();

        Assert.Equal("Ocurrió un error interno en el servidor.", message);
        Assert.NotEqual("error interno", message);
    }

    [Fact]
    public async Task InvokeAsync_NoException_PreservesNextResponse()
    {
        var context = CreateContext();
        var middleware = new ExceptionHandlingMiddleware(httpContext =>
        {
            httpContext.Response.StatusCode = StatusCodes.Status204NoContent;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status204NoContent, context.Response.StatusCode);
        Assert.Empty(await ReadResponseAsync(context));
    }

    private static DefaultHttpContext CreateContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        return context;
    }

    private static async Task<string> ReadResponseAsync(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);

        using var reader = new StreamReader(
            context.Response.Body,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true,
            leaveOpen: true);

        return await reader.ReadToEndAsync();
    }
}
