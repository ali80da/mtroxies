using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roxi.Core.Models.Proxies;

/// <summary>
/// Represents the configuration of an MTProto proxy.
/// </summary>
public class ProxyConfig
{
    /// <summary>
    /// The port used by the proxy.
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// The secret key for the proxy.
    /// </summary>
    public string Secret { get; set; } = string.Empty;

    /// <summary>
    /// The Telegram sponsor channel (e.g., @MyChannel).
    /// </summary>
    public string SponsorChannel { get; set; } = string.Empty;

    /// <summary>
    /// The fake domain for anti-filtering (e.g., domain.com).
    /// </summary>
    public string FakeDomain { get; set; } = string.Empty;

    /// <summary>
    /// The timestamp when the proxy was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}