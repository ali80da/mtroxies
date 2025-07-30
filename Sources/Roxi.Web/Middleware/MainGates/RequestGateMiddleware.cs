using System.Text;

namespace Roxi.Web.Middleware.MainGates;

public class RequestGateMiddleware(RequestDelegate Next, ILogger<RequestGateMiddleware> Logger)
{

    private readonly RequestDelegate Next = Next;
    private readonly ILogger<RequestGateMiddleware> Logger = Logger;

    #region Middleware Execution (v1)

    /// <summary>
    /// Processes incoming HTTP requests, logs comprehensive details, and validates headers.
    /// </summary>
    /// <param name="context">The HTTP context of the request.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        var requestId = Guid.NewGuid().ToString();
        var request = context.Request;
        var logBuilder = new StringBuilder();

        #region Request Information Logging

        logBuilder.AppendLine($"Request ID: {requestId}");
        logBuilder.AppendLine($"Timestamp: {DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}");
        logBuilder.AppendLine($"Method: {request.Method}");
        logBuilder.AppendLine($"Path: {request.Path}");
        logBuilder.AppendLine($"Query String: {request.QueryString}");
        logBuilder.AppendLine($"Remote IP: {context.Connection.RemoteIpAddress?.ToString() ?? "Unknown"}");
        logBuilder.AppendLine($"User-Agent: {request.Headers["User-Agent"]}");

        // Log headers
        logBuilder.AppendLine("Headers:");
        foreach (var header in request.Headers)
        {
            logBuilder.AppendLine($"  {header.Key}: {header.Value}");
        }

        // Validate specific headers (e.g., Authorization)
        if (!request.Headers.ContainsKey("Authorization") && request.Path.StartsWithSegments("/roxi/v-one"))
        {
            logBuilder.AppendLine("Warning: Authorization header is missing.");
            Logger.LogWarning(logBuilder.ToString());
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Authorization header is required.");
            return;
        }

        // Log query parameters
        if (request.Query.Any())
        {
            logBuilder.AppendLine("Query Parameters:");
            foreach (var param in request.Query)
            {
                logBuilder.AppendLine($"  {param.Key}: {param.Value}");
            }
        }

        // Log request body (if applicable)
        string body = null;
        if (request.Body.CanRead && (request.Method == "POST" || request.Method == "PUT"))
        {
            request.EnableBuffering();
            using var reader = new StreamReader(
                request.Body,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                leaveOpen: true);
            body = await reader.ReadToEndAsync();
            logBuilder.AppendLine($"Body: {body}");
            request.Body.Position = 0;
        }

        #endregion

        // Log the request details
        Logger.LogInformation(logBuilder.ToString());

        // Add Request ID to response headers
        // context.Response.Headers.Add("X-Request-Id", requestId);
        context.Response.Headers.Append("X-Request-Id", requestId);

        await Next(context);
    }

    #endregion


}