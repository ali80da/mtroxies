using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Roxi.Core.Models.Common;
using Roxi.Core.Models.Proxies;
using Roxi.Core.Services.Proxies;

namespace Roxi.Web.Areas.Version01.Controllers;

public class ProxiesController : SharedV01Controller
{

    private readonly IProxiService _proxiService;

    public ProxiesController(IProxiService proxiService)
    {
        _proxiService = proxiService;
    }




    [HttpGet("mtroxies")]
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
                    FakeDomain = "example.com"
                }
            },
            Message = "Proxies retrieved successfully."
        });
    }











}