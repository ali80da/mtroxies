using System.ComponentModel.DataAnnotations;

namespace Roxi.Core.Models.V01.Proxies;


/// <summary>
/// Represents the request model for creating a new proxy.
/// </summary>
public record CreateAndUpdateProxiRequest
{
    /// <summary>
    /// The Telegram sponsor channel (e.g., @MyChannel).
    /// </summary>
    [RegularExpression(@"^@[A-Za-z0-9_]{5,}$", ErrorMessage = "SponsorChannel must be a valid Telegram channel (e.g., @Channel).")]
    public required string SponsorChannel { get; set; }

    public required string Send2Channel { get; set; } = string.Empty;

    public string? Region { get; init; } = string.Empty;

    /// <summary>
    /// Optional fake domain for anti-filtering (e.g., domain.com).
    /// </summary>
    [RegularExpression(@"^([a-zA-Z0-9-]+\.)+[a-zA-Z]{2,}$", ErrorMessage = "FakeDomain must be a valid domain (e.g., domain.com).")]
    public string? FakeDomain { get; set; } = string.Empty;

    public List<string> Tags { get; init; } = new List<string>();

}


