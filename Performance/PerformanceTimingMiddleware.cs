using System.Diagnostics;
using System.Text.Json;


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

var trace = new ApiTraceResult
{
    Path = context.Request.Path,
    Method = context.Request.Method,
    TotalMs = totalStopwatch.ElapsedMilliseconds,
    MiddlewareOverheadMs = middlewareOverhead,
    ControllerMs = controllerStopwatch.ElapsedMilliseconds,
    ResponsePipelineMs = responseStopwatch.ElapsedMilliseconds,
    TimestampUtc = DateTime.UtcNow
};

// Console pretty log
Console.WriteLine($@"
================ API PERFORMANCE TRACE ================
Path: {trace.Method} {trace.Path}

Total: {trace.TotalMs} ms
 ├─ Middleware Overhead: {trace.MiddlewareOverheadMs} ms
 ├─ Controller Execution: {trace.ControllerMs} ms
 └─ Response Pipeline: {trace.ResponsePipelineMs} ms
=======================================================
");

// JSON trace output
var json = JsonSerializer.Serialize(trace);
Console.WriteLine(json);

    }
}
