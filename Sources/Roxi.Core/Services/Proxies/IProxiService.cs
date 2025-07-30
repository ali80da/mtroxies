using System.Net;
using System.Security.Cryptography;
using System.Text;
using Docker.DotNet;
using Docker.DotNet.Models;
using Roxi.Common.Models;
using Roxi.Core.Models.Proxies;

namespace Roxi.Core.Services.Proxies
{
    /// <summary>
    /// Defines the contract for managing MTProto proxies.
    /// </summary>
    public interface IProxiService
    {

        /// <summary>
        /// Creates a new MTProto proxy with the specified sponsor channel and optional fake domain.
        /// </summary>
        /// <param name="sponsorChannel">The Telegram sponsor channel (e.g., @MyChannel).</param>
        /// <param name="fakeDomain">Optional fake domain for anti-filtering.</param>
        /// <returns>A ResultConditions containing the created proxy details or an error message.</returns>
        Task<ResultConditions<ProxyConfig>> CreateProxiAsync(string sponsorChannel, string fakeDomain = null);

        /// <summary>
        /// Updates an existing proxy with the specified port.
        /// </summary>
        /// <param name="port">The port of the proxy to update.</param>
        /// <param name="sponsorChannel">The new Telegram sponsor channel.</param>
        /// <param name="fakeDomain">Optional new fake domain.</param>
        /// <returns>A ResultConditions containing the updated proxy details or an error message.</returns>
        Task<ResultConditions<ProxyConfig>> UpdateProxiAsync(int port, string sponsorChannel, string fakeDomain = null);

        /// <summary>
        /// Deletes a proxy with the specified port.
        /// </summary>
        /// <param name="port">The port of the proxy to delete.</param>
        /// <returns>A ResultConditions indicating the deletion result.</returns>
        Task<ResultConditions<bool>> DeleteProxiAsync(int port);

        /// <summary>
        /// Retrieves the list of all configured proxies.
        /// </summary>
        /// <returns>A ResultConditions containing the list of proxies.</returns>
        Task<ResultConditions<List<ProxyConfig>>> GetProxiesAsync();



    }



}
