using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc;
using PocketAlbum.Server.Services;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

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

        return Ok(new ServerInfo()
        {
            InstanceId = authService.InstanceId,
            Endpoints = GetEndpoints(addressesFeature)
        });
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

    private List<string> GetEndpoints(IServerAddressesFeature addressesFeature)
    {
        var resolvedAddresses = new List<string>();
        var localIpAddresses = GetLocalIpAddresses();

        foreach (var address in addressesFeature.Addresses)
        {
            if (!Uri.TryCreate(address, UriKind.Absolute, out var uri)) continue;

            string scheme = uri.Scheme;
            string host = uri.Host;
            int port = uri.Port;

            // Check if the host is a wildcard binding
            if (host == "*" || host == "0.0.0.0" || host == "[::]" || host == "any")
            {
                // Map the port to every actual IP address the machine owns
                foreach (var ip in localIpAddresses)
                {
                    // Format IPv6 properly with brackets
                    string ipString = ip.AddressFamily == AddressFamily.InterNetworkV6
                        ? $"[{ip}]"
                        : ip.ToString();

                    resolvedAddresses.Add($"{scheme}://{ipString}:{port}");
                }
            }
            else
            {
                // Keep literal bindings (like "localhost" or specific IPs) as they are
                resolvedAddresses.Add($"{scheme}://{host}:{port}");
            }
        }

        return resolvedAddresses.Distinct().ToList();
    }

    private List<IPAddress> GetLocalIpAddresses()
    {
        var ips = new List<IPAddress>();

        // Loop through all network interfaces (Ethernet, Wi-Fi, etc.)
        foreach (var netInterface in NetworkInterface.GetAllNetworkInterfaces())
        {
            // Skip down, loopback (127.0.0.1), and virtual/tunnel interfaces
            if (netInterface.OperationalStatus != OperationalStatus.Up ||
                netInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback)
            {
                continue;
            }

            var ipProps = netInterface.GetIPProperties();
            foreach (var addr in ipProps.UnicastAddresses)
            {
                // Filter out link-local IPv6 addresses (fe80::...) to keep clean targets
                if (addr.Address.AddressFamily == AddressFamily.InterNetwork ||
                   (addr.Address.AddressFamily == AddressFamily.InterNetworkV6 &&
                   !addr.Address.IsIPv6LinkLocal))
                {
                    ips.Add(addr.Address);
                }
            }
        }

        return ips;
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
