using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json.Serialization;

namespace Roxi.Core.Models.V01.Common;

/// <summary>
/// Represents the status of an API response.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ResultStatus
{
    Success,
    Error,
    Warning
}

/// <summary>
/// Represents a detailed error in an API response.
/// </summary>
public class ErrorDetail(string title, string message, string? resolution = null, string? target = null)
{
    /// <summary>
    /// The error code identifying the specific issue.
    /// </summary>
    public string Title { get; set; } = title;

    /// <summary>
    /// A human-readable error message.
    /// </summary>
    public string Message { get; set; } = message;

    /// <summary>
    /// Suggested resolution for the error.
    /// </summary>
    public string Resolution { get; set; } = resolution!;

    /// <summary>
    /// Optional field indicating the property or parameter causing the error.
    /// </summary>
    public string Target { get; set; } = target!;
}

/// <summary>
/// Represents metadata for the API response, such as pagination or API version.
/// </summary>
public class ResultMetadata
{
    /// <summary>
    /// The API version used for the response.
    /// </summary>
    public string SourceVersion { get; set; } = "1.0";

    /// <summary>
    /// Optional pagination information (e.g., total items, page number).
    /// </summary>
    public Dictionary<string, object> Pagination { get; set; } = new Dictionary<string, object>();

    /// <summary>
    /// Additional metadata for custom use cases.
    /// </summary>
    public Dictionary<string, object> Custom { get; set; } = new Dictionary<string, object>();
}

/// <summary>
/// Represents a standardized API response structure.
/// </summary>
/// <typeparam name="T">The type of data included in the response.</typeparam>
public class ResultConditions<T>(T data, ResultStatus status, HttpStatusCode httpStatusCode, string message, string? description = null, List<string>? recommendedActions = null, List<ErrorDetail>? errors = null, ResultMetadata? metadata = null)
{
    /// <summary>
    /// The status of the response (Success, Error, Warning).
    /// </summary>
    public ResultStatus Status { get; set; } = status;

    /// <summary>
    /// The HTTP status code corresponding to the response.
    /// </summary>
    public HttpStatusCode HttpStatusCode { get; set; } = httpStatusCode;

    /// <summary>
    /// The primary data returned by the API.
    /// </summary>
    public T Data { get; set; } = data;

    /// <summary>
    /// A human-readable message describing the response.
    /// </summary>
    public string Message { get; set; } = message;

    /// <summary>
    /// A detailed description of the response, providing additional context or information.
    /// </summary>
    public string Description { get; set; } = description ?? "No additional description provided.";

    /// <summary>
    /// A list of recommended actions for the user or developer to resolve issues or optimize usage.
    /// </summary>
    public List<string> RecommendedActions { get; set; } = recommendedActions ?? new List<string>();

    /// <summary>
    /// A list of detailed errors, if any.
    /// </summary>
    public List<ErrorDetail> Errors { get; set; } = errors ?? new List<ErrorDetail>();

    /// <summary>
    /// Metadata providing additional context (e.g., pagination, API version).
    /// </summary>
    public ResultMetadata Metadata { get; set; } = metadata ?? new ResultMetadata();

    /// <summary>
    /// The timestamp when the response was generated.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    #region Factory Methods (v1)

    /// <summary>
    /// Creates a successful API response.
    /// </summary>
    /// <param name="data">The data to include in the response.</param>
    /// <param name="message">A human-readable success message.</param>
    /// <param name="code">The success code.</param>
    /// <param name="description">A detailed description of the response.</param>
    /// <param name="recommendedActions">Optional list of recommended actions for the user.</param>
    /// <param name="metadata">Optional metadata for the response.</param>
    /// <returns>A ResultConditions indicating success.</returns>
    public static ResultConditions<T> Success(T data, string message, string code, string? description = null, List<string>? recommendedActions = null, ResultMetadata? metadata = null)
    {
        return new ResultConditions<T>(
            data,
            ResultStatus.Success,
            HttpStatusCode.OK,
            message,
            description ?? "Operation completed successfully.",
            recommendedActions ?? new List<string> { "No further actions required." },
            new List<ErrorDetail> { new ErrorDetail(code, message) },
            metadata
        );
    }

    /// <summary>
    /// Creates an error API response.
    /// </summary>
    /// <param name="message">A human-readable error message.</param>
    /// <param name="code">The error code.</param>
    /// <param name="description">A detailed description of the error.</param>
    /// <param name="recommendedActions">Optional list of recommended actions to resolve the error.</param>
    /// <param name="resolution">Suggested resolution for the error.</param>
    /// <param name="target">Optional field indicating the property or parameter causing the error.</param>
    /// <param name="httpStatusCode">The HTTP status code (default: 400 BadRequest).</param>
    /// <returns>A ResultConditions indicating an error.</returns>
    public static ResultConditions<T> Error(string message, string code, string? description = null, List<string>? recommendedActions = null, string? resolution = null, string? target = null, HttpStatusCode httpStatusCode = HttpStatusCode.BadRequest)
    {
        return new ResultConditions<T>(
            default!,
            ResultStatus.Error,
            httpStatusCode,
            message,
            description ?? "An error occurred during the operation.",
            recommendedActions ?? new List<string> { "Please review the error details and try again." },
            new List<ErrorDetail> { new ErrorDetail(code, message, resolution, target) }
        );
    }

    /// <summary>
    /// Creates a warning API response with partial success.
    /// </summary>
    /// <param name="data">The data to include in the response.</param>
    /// <param name="message">A human-readable warning message.</param>
    /// <param name="code">The warning code.</param>
    /// <param name="description">A detailed description of the warning.</param>
    /// <param name="recommendedActions">Optional list of recommended actions to address the warning.</param>
    /// <param name="resolution">Suggested resolution for the warning.</param>
    /// <param name="target">Optional field indicating the property or parameter causing the warning.</param>
    /// <param name="httpStatusCode">The HTTP status code (default: 200 OK).</param>
    /// <returns>A ResultConditions indicating a warning.</returns>
    public static ResultConditions<T> Warning(T data, string message, string code, string? description = null, List<string>? recommendedActions = null, string? resolution = null, string? target = null, HttpStatusCode httpStatusCode = HttpStatusCode.OK)
    {
        return new ResultConditions<T>(
            data,
            ResultStatus.Warning,
            httpStatusCode,
            message,
            description ?? "Operation completed with warnings.",
            recommendedActions ?? new List<string> { "Please review the warning details and take appropriate actions." },
            new List<ErrorDetail> { new ErrorDetail(code, message, resolution, target) }
        );
    }

    #endregion
}