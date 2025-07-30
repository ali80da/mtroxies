using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Roxi.Common.Models;
using Roxi.Core.Models.Proxies;
using Roxi.Core.Services.Proxies;

namespace Roxi.Web.Areas.Version01.Controllers;

public class ProxiesController : SharedV01Controller
{

    [HttpGet("roxies")]
    public IActionResult Proxies()
    {
        return Ok(new
        {
            Success = true,
            Data = new List<ProxyConfig>
            {
                new ProxyConfig
                {
                    Port = 8080,
                    SponsorChannel = "@exampleChannel",
                    FakeDomain = "example.com"
                }
            },
            Message = "Proxies retrieved successfully."
        });
    }










}