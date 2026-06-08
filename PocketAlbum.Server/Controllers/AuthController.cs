using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc;
using PocketAlbum.Server.Services;

namespace PocketAlbum.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService, IServer server) : ControllerBase
{
    private readonly IAuthService authService = authService;
    private readonly IServer server = server;

    public IActionResult GetServerInfo()
    {
        var addressesFeature = server.Features.Get<IServerAddressesFeature>();

        if (addressesFeature == null || !addressesFeature.Addresses.Any())
        {
            return NotFound("No listening addresses could be found.");
        }

        return Ok(authService.GetServerInfo(addressesFeature.Addresses));
    }

    [HttpPost("pair")]
    public IActionResult Pair([FromBody] TokenRequest tokenRequest)
    {
        try
        {
            return Ok(authService.RequestToken(tokenRequest));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return Unauthorized(ex.Message);
        }
    }
}

public class ServerInfo
{
    public Guid InstanceId { get; set; }
    public List<string> Endpoints { get; set; } = [];
}

public class TokenRequest
{
    public Guid ServerId { get; set; }
    public Guid ConnectionId { get; set; }
    public required string ClientName { get; set; }
}
