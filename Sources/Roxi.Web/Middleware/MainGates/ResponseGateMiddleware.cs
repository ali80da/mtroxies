using System.Diagnostics;
using System.Text;

namespace Roxi.Web.Middleware.MainGates
{
    public class ResponseGateMiddleware(RequestDelegate Next, ILogger<ResponseGateMiddleware> Logger)
    {
        private readonly RequestDelegate Next = Next ?? throw new ArgumentNullException(nameof(Next));
        private readonly ILogger<ResponseGateMiddleware> Logger = Logger ?? throw new ArgumentNullException(nameof(Logger));


        #region Core Middleware Logic

        public async Task InvokeAsync(HttpContext context)
        {
            
            // Start telemetry activity
            using var activity = Activity.Current?.Source.StartActivity("MTroxies.ResponseGate");
            activity?.SetTag("middleware", "ResponseGate");

            var requestId = context.Items["RequestId"]?.ToString() ?? "Unknown";
            var correlationId = context.Items["CorrelationId"]?.ToString() ?? "Unknown";

            // Enable response buffering
            var originalBodyStream = context.Response.Body;
            using var newBodyStream = new MemoryStream();
            context.Response.Body = newBodyStream;

            try
            {
                #region Security Headers
                // Add security headers
                context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
                context.Response.Headers.Append("X-Frame-Options", "DENY");
                context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'");
                #endregion

                #region Pipeline Continuation
                // Continue to next middleware
                await Next(context);
                #endregion

                #region Response Processing
                // Read response body if present
                string responseBody = string.Empty;
                if (newBodyStream.Length > 0 && !context.Response.HasStarted)
                {
                    newBodyStream.Seek(0, SeekOrigin.Begin);
                    responseBody = await new StreamReader(newBodyStream, Encoding.UTF8).ReadToEndAsync();
                    newBodyStream.Seek(0, SeekOrigin.Begin);
                    await newBodyStream.CopyToAsync(originalBodyStream);
                }
                #endregion

                #region Response Logging
                // Log response details
                Logger.LogInformation(
                    "Response ID: {RequestId}, Correlation ID: {CorrelationId}\n" +
                    "Timestamp: {Timestamp}\n" +
                    "Status Code: {StatusCode}\n" +
                    "Headers: {Headers}\n" +
                    "Body: {Body}",
                    requestId,
                    correlationId,
                    DateTime.UtcNow.ToString("o"),
                    context.Response.StatusCode,
                    string.Join("\n  ", context.Response.Headers.Select(h => $"{h.Key}: {h.Value}")),
                    responseBody);
                #endregion

                #region Stream Finalization
                // Ensure response stream is flushed if not started
                if (!context.Response.HasStarted)
                {
                    await context.Response.Body.FlushAsync();
                }
                #endregion
            }
            catch (Exception ex)
            {
                #region Error Handling
                // Log errors and rethrow to allow error handling by subsequent middleware
                Logger.LogError(ex, "Response ID {RequestId}: Error processing response.", requestId);
                throw;
                #endregion
            }
            finally
            {
                #region Stream Cleanup
                // Restore original response stream
                context.Response.Body = originalBodyStream;
                #endregion
            }
        }

        #endregion
    }
}