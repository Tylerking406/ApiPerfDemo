using System.Diagnostics;

public class PerformanceTimingMiddleware
{
    private readonly RequestDelegate _next;

    public PerformanceTimingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var totalStopwatch = Stopwatch.StartNew();
        var controllerStopwatch = new Stopwatch();
        var responseStopwatch = new Stopwatch();

        // Track when controller starts executing
        context.Response.OnStarting(() =>
        {
            // Response pipeline timing begins when controller finishes
            responseStopwatch.Start();
            controllerStopwatch.Stop();
            return Task.CompletedTask;
        });

        // Start controller timing
        controllerStopwatch.Start();

        await _next(context);

        // End response time
        responseStopwatch.Stop();
        totalStopwatch.Stop();

        var middlewareOverhead =
            totalStopwatch.ElapsedMilliseconds -
            controllerStopwatch.ElapsedMilliseconds -
            responseStopwatch.ElapsedMilliseconds;

        Console.WriteLine($@"
================ API PERFORMANCE TRACE ================
Path: {context.Request.Method} {context.Request.Path}

Total: {totalStopwatch.ElapsedMilliseconds} ms
 ├─ Middleware Overhead: {middlewareOverhead} ms
 ├─ Controller Execution: {controllerStopwatch.ElapsedMilliseconds} ms
 └─ Response Pipeline: {responseStopwatch.ElapsedMilliseconds} ms
=======================================================
");
    }
}
