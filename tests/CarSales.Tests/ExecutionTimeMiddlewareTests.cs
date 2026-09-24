using CarSales.Api.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace CarSales.Tests;

public class ExecutionTimeMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_ExecutesNextMiddleware()
    {
        var context = CreateContext("GET", "/api/Sale/total");
        var logger = new Mock<ILogger<ExecutionTimeMiddleware>>();
        var nextExecuted = false;
        var middleware = new ExecutionTimeMiddleware(
            _ =>
            {
                nextExecuted = true;
                return Task.CompletedTask;
            },
            logger.Object);

        await middleware.InvokeAsync(context);

        Assert.True(nextExecuted);
    }

    [Fact]
    public async Task InvokeAsync_SuccessfulRequest_LogsExecutionTime()
    {
        var context = CreateContext("GET", "/api/Sale/total");
        var logger = new Mock<ILogger<ExecutionTimeMiddleware>>();
        var middleware = new ExecutionTimeMiddleware(_ => Task.CompletedTask, logger.Object);

        await middleware.InvokeAsync(context);

        VerifyExecutionLog(logger, "GET", "/api/Sale/total");
    }

    [Fact]
    public async Task InvokeAsync_WhenNextThrows_LogsExecutionTimeAndPropagatesException()
    {
        var context = CreateContext("POST", "/api/Sale");
        var logger = new Mock<ILogger<ExecutionTimeMiddleware>>();
        var expectedException = new InvalidOperationException("unexpected failure");
        var middleware = new ExecutionTimeMiddleware(
            _ => throw expectedException,
            logger.Object);

        var actualException = await Assert.ThrowsAsync<InvalidOperationException>(
            () => middleware.InvokeAsync(context));

        Assert.Same(expectedException, actualException);
        VerifyExecutionLog(logger, "POST", "/api/Sale");
    }

    private static DefaultHttpContext CreateContext(string method, string path)
    {
        var context = new DefaultHttpContext();
        context.Request.Method = method;
        context.Request.Path = path;

        return context;
    }

    private static void VerifyExecutionLog(
        Mock<ILogger<ExecutionTimeMiddleware>> logger,
        string method,
        string path)
    {
        logger.Verify(
            mock => mock.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) =>
                    state.ToString()!.Contains(method) &&
                    state.ToString()!.Contains(path) &&
                    state.ToString()!.Contains("executed in") &&
                    state.ToString()!.Contains("ms")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
