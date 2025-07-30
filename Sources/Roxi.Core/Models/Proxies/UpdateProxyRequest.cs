namespace Roxi.Core.Models.Proxies;

/// <summary>
/// Represents the request model for updating an existing proxy.
/// </summary>
public class UpdateProxyRequest
{
    /// <summary>
    /// The new Telegram sponsor channel (e.g., @MyChannel).
    /// </summary>
    public string SponsorChannel { get; set; } = string.Empty;

    /// <summary>
    /// Optional new fake domain for anti-filtering (e.g., domain.com).
    /// </summary>
    public string FakeDomain { get; set; } = string.Empty;
}

