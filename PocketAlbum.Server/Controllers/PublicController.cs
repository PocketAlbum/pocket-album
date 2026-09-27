using PocketAlbum.Server.Services;

namespace PocketAlbum.Server.Controllers;

public static class PublicEndpoints
{
    public static IEndpointRouteBuilder MapPublicEndpoints(
        this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/public");

        group.MapGet("/{shareId:guid}", GetSharedContent);

        return app;
    }

    public static async Task<IResult> GetSharedContent(Guid shareId, 
        PublicService publicService)
    {
        return Results.File(publicService.GetContent(shareId), "image/jpeg");
    }
}
