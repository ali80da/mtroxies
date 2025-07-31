using System.Diagnostics;
using System.Text;

namespace Roxi.Web.Middleware.MainGates
{
    public class RequestGateMiddleware(RequestDelegate Next, ILogger<RequestGateMiddleware> Logger, IConfiguration Configuration)
    {
        private readonly RequestDelegate Next = Next ?? throw new ArgumentNullException(nameof(Next));
        private readonly ILogger<RequestGateMiddleware> Logger = Logger ?? throw new ArgumentNullException(nameof(Logger));
        private readonly long MaxRequestBodySize = Configuration.GetValue<long>("RequestGate:MaxRequestBodySize", 1024 * 1024);


        #region Core Middleware Logic
        
        public async Task InvokeAsync(HttpContext context)
        {
            // Generate unique RequestId and retrieve or generate CorrelationId
            var requestId = Guid.NewGuid().ToString();
            var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault() ?? Guid.NewGuid().ToString();
            context.Items["RequestId"] = requestId;
            context.Items["CorrelationId"] = correlationId;

            // Start performance tracking
            var stopwatch = Stopwatch.StartNew();

            try
            {
                #region Request Tracking

                // Add CorrelationId to response headers
                context.Response.Headers.Append("X-Correlation-Id", correlationId);
                #endregion

                #region Request Body Processing
                // Enable buffering to read request body
                context.Request.EnableBuffering();

                // Read request body if present
                string requestBody = string.Empty;
                if (context.Request.Body.CanRead && context.Request.ContentLength.HasValue)
                {
                    if (context.Request.ContentLength > MaxRequestBodySize)
                    {
                        Logger.LogWarning("Request ID {RequestId}: Request body size {ContentLength} exceeds limit {MaxSize}.",
                            requestId, context.Request.ContentLength, MaxRequestBodySize);
                        context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
                        await context.Response.WriteAsync("Request body too large.");
                        return;
                    }

                    using var reader = new StreamReader(
                        context.Request.Body,
                        Encoding.UTF8,
                        detectEncodingFromByteOrderMarks: false,
                        leaveOpen: true);
                    requestBody = await reader.ReadToEndAsync();
                    context.Request.Body.Position = 0; // Reset stream position
                }
                #endregion

                #region Security Checks
                // Validate critical headers
                if (!context.Request.Headers.ContainsKey("Accept"))
                {
                    Logger.LogWarning("Request ID {RequestId}: Missing 'Accept' header.", requestId);
                }
                if (context.Request.ContentType != null && !context.Request.ContentType.Contains("application/json"))
                {
                    Logger.LogWarning("Request ID {RequestId}: Invalid Content-Type {ContentType}. Expected application/json.",
                        requestId, context.Request.ContentType);
                }
                #endregion

                #region Request Logging
                // Log request details
                Logger.LogInformation(
                    "Request ID: {RequestId}, Correlation ID: {CorrelationId}\n" +
                    "Timestamp: {Timestamp}\n" +
                    "Method: {Method}\n" +
                    "Path: {Path}\n" +
                    "Query String: {QueryString}\n" +
                    "Remote IP: {RemoteIp}\n" +
                    "User-Agent: {UserAgent}\n" +
                    "Headers: {Headers}\n" +
                    "Body: {Body}",
                    requestId,
                    correlationId,
                    DateTime.UtcNow.ToString("o"),
                    context.Request.Method,
                    context.Request.Path,
                    context.Request.QueryString,
                    context.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                    context.Request.Headers["User-Agent"].ToString(),
                    string.Join("\n  ", context.Request.Headers.Select(h => $"{h.Key}: {h.Value}")),
                    requestBody);
                #endregion

                #region Pipeline Continuation
                // Continue to next middleware
                await Next(context);
                #endregion

                #region Performance Logging
                // Log request completion and duration
                stopwatch.Stop();
                Logger.LogInformation("Request ID {RequestId}: Completed in {ElapsedMilliseconds}ms with status code {StatusCode}.",
                    requestId, stopwatch.ElapsedMilliseconds, context.Response.StatusCode);
                #endregion
            }
            catch (Exception ex)
            {
                #region Error Handling
                // Log errors and rethrow to allow error handling by subsequent middleware
                Logger.LogError(ex, "Request ID {RequestId}: Error processing request.", requestId);
                throw;
                #endregion
            }
        }
        
        #endregion
    }
}