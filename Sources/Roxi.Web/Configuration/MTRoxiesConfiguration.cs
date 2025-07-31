namespace Roxi.Web.Configuration;

public class MTroxiesMiddlewareSettings
{
    public long MaxRequestBodySize { get; set; } = 1024 * 1024; // Default: 1MB
    public bool EnableRequestLogging { get; set; } = true;
    public bool EnableResponseLogging { get; set; } = true;
    public bool EnableSecurityHeaders { get; set; } = true;

}


