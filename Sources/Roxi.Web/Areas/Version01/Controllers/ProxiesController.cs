using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Roxi.Core.Models.V01.Common;
using Roxi.Core.Models.V01.Proxies;
using Roxi.Core.Services.V01.Proxie;

namespace Roxi.Web.Areas.Version01.Controllers;

/// <summary>
/// Controller 4 MTProto Proxies (Version 01).
/// </summary>
public class ProxiesController : SharedV01Controller
{

    private readonly IProxiService _proxiService;

    public ProxiesController(IProxiService proxiService)
    {
        _proxiService = proxiService;
    }




    [HttpGet("roxies")]
    public IActionResult Proxies()
    {
        return Ok(new
        {
            Success = true,
            Data = new List<Proxi>
            {
                new Proxi
                {
                    Id = "",
                    Port = 8080,
                    SponsorChannel = "@exampleChannel",
                    Secret = "",
                    FakeDomain = "example.com",
                    Send2Channel = "@sendChannel",
                }
            },
            Message = "Proxies retrieved successfully."
        });
    }




    /// <summary>
    /// Creates a new MTProto proxy.
    /// </summary>
    /// <param name="request">The request containing sponsor channel, fake domain, and tags.</param>
    /// <returns>A ResultConditions containing the created proxy details or an error.</returns>
    /// <response code="200">Proxy created successfully.</response>
    /// <response code="400">Invalid request data.</response>
    /// <response code="409">Duplicate proxy detected.</response>
    /// <response code="500">Server error occurred.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ResultConditions<Proxi>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultConditions<Proxi>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResultConditions<Proxi>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ResultConditions<Proxi>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CreateAndUpdateProxiRequest request)
    {
        var result = await _proxiService.CreateProxiAsync(request);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    /// <summary>
    /// Updates an existing MTProto proxy.
    /// </summary>
    /// <param name="port">The port of the proxy to update.</param>
    /// <param name="request">The request containing new sponsor channel, fake domain, and tags.</param>
    /// <returns>A ResultConditions containing the updated proxy details or an error.</returns>
    /// <response code="200">Proxy updated successfully.</response>
    /// <response code="400">Invalid request data.</response>
    /// <response code="404">Proxy not found.</response>
    /// <response code="409">Duplicate proxy detected.</response>
    /// <response code="500">Server error occurred.</response>
    [HttpPut("{port}")]
    [ProducesResponseType(typeof(ResultConditions<Proxi>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultConditions<Proxi>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResultConditions<Proxi>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ResultConditions<Proxi>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ResultConditions<Proxi>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(int port, [FromBody] CreateAndUpdateProxiRequest request)
    {
        var result = await _proxiService.UpdateProxiAsync(port, request);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    /// <summary>
    /// Deletes an MTProto proxy by port.
    /// </summary>
    /// <param name="port">The port of the proxy to delete.</param>
    /// <returns>A ResultConditions indicating the deletion result.</returns>
    /// <response code="200">Proxy deleted successfully.</response>
    /// <response code="404">Proxy not found.</response>
    /// <response code="500">Server error occurred.</response>
    [HttpDelete("{port}")]
    [ProducesResponseType(typeof(ResultConditions<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultConditions<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ResultConditions<bool>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(int port)
    {
        var result = await _proxiService.DeleteProxiAsync(port);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    /// <summary>
    /// Retrieves the list of all active MTProto proxies.
    /// </summary>
    /// <returns>A ResultConditions containing the list of proxies.</returns>
    /// <response code="200">Proxies retrieved successfully.</response>
    /// <response code="500">Server error occurred.</response>
    [HttpGet("mtroxies")]
    [ProducesResponseType(typeof(ResultConditions<List<Proxi>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultConditions<List<Proxi>>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProxies()
    {
        var result = await _proxiService.GetProxiesAsync();
        return StatusCode((int)result.HttpStatusCode, result);
    }






}