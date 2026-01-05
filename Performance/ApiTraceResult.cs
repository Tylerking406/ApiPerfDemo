using System.Text.Json;

public class ApiTraceResult
{
    public string Path { get; set; } = "";
    public string Method { get; set; } = "";
    public long TotalMs { get; set; }
    public long MiddlewareOverheadMs { get; set; }
    public long ControllerMs { get; set; }
    public long ResponsePipelineMs { get; set; }
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

    
}

