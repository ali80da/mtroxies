using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roxi.Core.Models.Proxies;


/// <summary>
/// Represents the request model for creating a new proxy.
/// </summary>
public class CreateProxyRequest
{
    /// <summary>
    /// The Telegram sponsor channel (e.g., @MyChannel).
    /// </summary>
    public string SponsorChannel { get; set; } = string.Empty;

    /// <summary>
    /// Optional fake domain for anti-filtering (e.g., domain.com).
    /// </summary>
    public string FakeDomain { get; set; } = string.Empty;
}


