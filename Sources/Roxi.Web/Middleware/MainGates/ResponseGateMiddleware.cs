using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Roxi.Web.Middleware.MainGates;

public class ResponseGateMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ResponseGateMiddleware> _logger;
    private const int MaxBodySize = 1024 * 1024; // 1MB limit for response body

    public ResponseGateMiddleware(RequestDelegate next, ILogger<ResponseGateMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Processes outgoing HTTP responses and logs details including request ID, status code, headers, and body.
    /// </summary>
    /// <param name="context">The HTTP context of the response.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        var requestId = context.Response.Headers["X-Request-Id"].ToString() ?? "Unknown";
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await _next(context);

        responseBody.Seek(0, SeekOrigin.Begin);
        var responseText = await ReadResponseBodyAsync(responseBody);
        responseBody.Seek(0, SeekOrigin.Begin);

        // Log response details
        var logBuilder = new StringBuilder(512); // Initial capacity to reduce allocations
        AppendResponseInfo(logBuilder, context, requestId, responseText);

        // Log based on status code
        if (context.Response.StatusCode >= 400)
        {
            _logger.LogWarning("{ResponseDetails}", logBuilder.ToString());
        }
        else
        {
            _logger.LogInformation("{ResponseDetails}", logBuilder.ToString());
        }

        await responseBody.CopyToAsync(originalBodyStream);
    }

    private static void AppendResponseInfo(StringBuilder logBuilder, HttpContext context, string requestId, string? responseText)
    {
        logBuilder.AppendLine($"Response for Request ID: {requestId}");
        logBuilder.AppendLine($"Timestamp: {DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}");
        logBuilder.AppendLine($"Request Path: {context.Request.Path}{context.Request.QueryString}");
        logBuilder.AppendLine($"API Version: {(context.Request.Path.StartsWithSegments("/roxi/v-one") ? "1.0" : "Unknown")}");
        logBuilder.AppendLine($"Status Code: {context.Response.StatusCode}");
        logBuilder.AppendLine("Headers:");
        foreach (var header in context.Response.Headers)
        {
            logBuilder.AppendLine($"  {header.Key}: {header.Value}");
        }
        if (!string.IsNullOrEmpty(responseText))
        {
            logBuilder.AppendLine($"Body: {responseText}");
        }
    }

    private static async Task<string?> ReadResponseBodyAsync(MemoryStream responseBody)
    {
        try
        {
            using var reader = new StreamReader(responseBody, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 1024, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            return body.Length > MaxBodySize ? body[..MaxBodySize] : body;
        }
        catch (Exception)
        {
            return null;
        }
    }
}