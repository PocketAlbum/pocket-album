using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using PocketAlbum.Server.Services;

namespace PocketAlbum.Server.Controllers;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(
        this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth");

        group.MapGet("/", GetServerInfo);
        group.MapPost("/pair", Pair);

        return app;
    }

    private static IResult GetServerInfo(
        IAuthService authService,
        IServer server)
    {
        var addressesFeature =
            server.Features.Get<IServerAddressesFeature>();

        if (addressesFeature == null ||
            !addressesFeature.Addresses.Any())
        {
            return Results.NotFound(
                "No listening addresses could be found.");
        }

        return Results.Ok(
            authService.GetServerInfo(addressesFeature.Addresses));
    }

    private static IResult Pair(
        TokenRequest tokenRequest,
        IAuthService authService)
    {
        try
        {
            return Results.Ok(
                authService.RequestToken(tokenRequest));
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(ex.Message);
        }
        catch
        {
            return Results.Unauthorized();
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
