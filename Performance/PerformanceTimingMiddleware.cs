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
    var total = Stopwatch.StartNew();

    var middlewareBefore = Stopwatch.StartNew();
    var controller = new Stopwatch();
    var responsePipeline = new Stopwatch();

    // Start controller timing only while executing the next delegate
    middlewareBefore.Stop();
    controller.Start();

    await _next(context);

    controller.Stop();

    // Everything after controller counts as response pipeline
    responsePipeline.Start();
    responsePipeline.Stop();

    total.Stop();

    var trace = new ApiTraceResult
    {
        Path = context.Request.Path,
        Method = context.Request.Method,
        TotalMs = total.ElapsedMilliseconds,
        MiddlewareOverheadMs = middlewareBefore.ElapsedMilliseconds,
        ControllerMs = controller.ElapsedMilliseconds,
        ResponsePipelineMs = responsePipeline.ElapsedMilliseconds,
        TimestampUtc = DateTime.UtcNow
    };

    Console.WriteLine($@"
================ API PERFORMANCE TRACE ================
Path: {trace.Method} {trace.Path}

Total: {trace.TotalMs} ms
 ├─ Middleware Overhead: {trace.MiddlewareOverheadMs} ms
 ├─ Controller Execution: {trace.ControllerMs} ms
 └─ Response Pipeline: {trace.ResponsePipelineMs} ms
=======================================================
");

    TraceStore.Add(trace);
}

}
