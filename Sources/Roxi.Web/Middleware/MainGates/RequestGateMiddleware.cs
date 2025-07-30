using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Roxi.Web.Middleware.MainGates;

public class RequestGateMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestGateMiddleware> _logger;
    private const int MaxBodySize = 1024 * 1024; // 1MB limit for request body

    public RequestGateMiddleware(RequestDelegate next, ILogger<RequestGateMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Processes incoming HTTP requests, logs details, and validates headers.
    /// </summary>
    /// <param name="context">The HTTP context of the request.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        var requestId = Guid.NewGuid().ToString();
        var request = context.Request;
        var logBuilder = new StringBuilder(512); // Initial capacity to reduce allocations

        // Log request details
        AppendRequestInfo(logBuilder, context, requestId);

        // Log headers
        AppendHeaders(logBuilder, request.Headers);

        // Log query parameters
        if (request.Query.Count > 0)
        {
            AppendQueryParameters(logBuilder, request.Query);
        }

        // Log request body for POST/PUT if applicable
        string? body = null;
        if (IsBodyReadable(request))
        {
            body = await ReadRequestBodyAsync(request);
            if (!string.IsNullOrEmpty(body))
            {
                logBuilder.AppendLine($"Body: {body}");
            }
        }

        // Log the request details
        _logger.LogInformation("{RequestDetails}", logBuilder.ToString());

        // Add Request ID to response headers
        context.Response.Headers.Append("X-Request-Id", requestId);

        await _next(context);
    }

    private static void AppendRequestInfo(StringBuilder logBuilder, HttpContext context, string requestId)
    {
        logBuilder.AppendLine($"Request ID: {requestId}");
        logBuilder.AppendLine($"Timestamp: {DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}");
        logBuilder.AppendLine($"Method: {context.Request.Method}");
        logBuilder.AppendLine($"Path: {context.Request.Path}");
        logBuilder.AppendLine($"Query String: {context.Request.QueryString}");
        logBuilder.AppendLine($"Remote IP: {context.Connection.RemoteIpAddress?.ToString() ?? "Unknown"}");
        logBuilder.AppendLine($"User-Agent: {context.Request.Headers["User-Agent"]}");
    }

    private static void AppendHeaders(StringBuilder logBuilder, IHeaderDictionary headers)
    {
        logBuilder.AppendLine("Headers:");
        foreach (var header in headers)
        {
            // Skip sensitive headers (e.g., Authorization) to prevent logging sensitive data
            if (string.Equals(header.Key, "Authorization", StringComparison.OrdinalIgnoreCase))
            {
                logBuilder.AppendLine($"  {header.Key}: [REDACTED]");
            }
            else
            {
                logBuilder.AppendLine($"  {header.Key}: {header.Value}");
            }
        }
    }

    private static void AppendQueryParameters(StringBuilder logBuilder, IQueryCollection query)
    {
        logBuilder.AppendLine("Query Parameters:");
        foreach (var param in query)
        {
            logBuilder.AppendLine($"  {param.Key}: {param.Value}");
        }
    }

    private static bool IsBodyReadable(HttpRequest request)
    {
        return request.Body.CanRead &&
               (string.Equals(request.Method, "POST", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(request.Method, "PUT", StringComparison.OrdinalIgnoreCase)) &&
               request.ContentLength is > 0 and <= MaxBodySize &&
               request.ContentType?.StartsWith("application/json", StringComparison.OrdinalIgnoreCase) == true;
    }

    private static async Task<string?> ReadRequestBodyAsync(HttpRequest request)
    {
        request.EnableBuffering();
        try
        {
            using var reader = new StreamReader(
                request.Body,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                bufferSize: 1024,
                leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            request.Body.Position = 0;
            return body.Length > MaxBodySize ? body[..MaxBodySize] : body;
        }
        catch (Exception)
        {
            return null;
        }
    }
}