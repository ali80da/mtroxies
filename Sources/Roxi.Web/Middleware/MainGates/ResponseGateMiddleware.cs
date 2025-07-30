using System.Text;

namespace Roxi.Web.Middleware.MainGates;

public class ResponseGateMiddleware(RequestDelegate Next, ILogger<ResponseGateMiddleware> Logger)
{

    private readonly RequestDelegate Next;
    private readonly ILogger<ResponseGateMiddleware> Logger;

    #region Middleware Execution (v1)

    /// <summary>
    /// Processes outgoing HTTP responses and logs comprehensive details, including request ID, status code, headers, body, description, and recommended actions.
    /// </summary>
    /// <param name="context">The HTTP context of the response.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        var requestId = context.Response.Headers["X-Request-Id"].ToString() ?? "Unknown";
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await Next(context);

        responseBody.Seek(0, SeekOrigin.Begin);
        var responseText = await new StreamReader(responseBody).ReadToEndAsync();
        responseBody.Seek(0, SeekOrigin.Begin);

        #region Response Information Logging

        var logBuilder = new StringBuilder();
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
        logBuilder.AppendLine($"Body: {responseText}");

        #endregion

        // Log based on status code
        if (context.Response.StatusCode >= 400)
        {
            Logger.LogWarning(logBuilder.ToString());
        }
        else
        {
            Logger.LogInformation(logBuilder.ToString());
        }

        await responseBody.CopyToAsync(originalBodyStream);
        context.Response.Body = originalBodyStream;
    }

    #endregion



}