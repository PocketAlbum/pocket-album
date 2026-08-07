using PocketAlbum.Models;
using PocketAlbum.Server.Services;

namespace PocketAlbum.Server.Controllers;

public static class AlbumEndpoints
{
    public static IEndpointRouteBuilder MapAlbumEndpoints(
        this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/albums")
            .RequireAuthorization();

        group.MapGet("/", GetAlbums).AllowAnonymous();
        group.MapGet("/{albumId:guid}", GetAlbum).AllowAnonymous();
        group.MapGet("/{albumId:guid}/index", GetAlbumIndex);
        group.MapGet("/{albumId:guid}/list", ListImages);
        group.MapGet("/{albumId:guid}/images/{imageId}", GetImage);
        group.MapGet("/{albumId:guid}/thumbnails/{imageId}", GetThumbnail);
        group.MapGet("/{albumId:guid}/info", GetInfo);

        return app;
    }

    private static async Task<IResult> GetAlbums(
        AlbumService service)
    {
        var metaList = service.Albums.Values
            .Select(async a => await a.GetMetadata())
            .Select(m => m.Result)
            .ToList();

        return Results.Ok(metaList);
    }

    private static async Task<IResult> GetAlbum(
        Guid albumId,
        AlbumService service)
    {
        if (!service.Albums.TryGetValue(albumId, out var album))
        {
            return Results.NotFound($"Album with id {albumId} not found");
        }
        return Results.Ok(await album.GetMetadata());
    }

    private static async Task<IResult> GetAlbumIndex(
        Guid albumId,
        AlbumService service)
    {
        if (!service.Albums.TryGetValue(albumId, out var album))
        {
            return Results.NotFound($"Album with id {albumId} not found");
        }

        return Results.Ok(await album.GetYearIndex());
    }

    private static async Task<IResult> ListImages(
        Guid albumId,
        FilterModel filter,
        int index,
        int page,
        AlbumService service)
    {
        if (!service.Albums.TryGetValue(albumId, out var album))
        {
            return Results.NotFound($"Album with id {albumId} not found");
        }

        var paging = new Interval(index * page, ((index + 1) * page) - 1);

        return Results.Ok(await album.List(filter, paging));
    }

    private static async Task<IResult> GetImage(
        Guid albumId,
        string imageId,
        AlbumService service)
    {
        if (!service.Albums.TryGetValue(albumId, out var album))
        {
            return Results.NotFound($"Album with id {albumId} not found");
        }

        return Results.File(
            await album.GetImageData(imageId),
            "image/jpeg");
    }

    private static async Task<IResult> GetThumbnail(
        Guid albumId,
        string imageId,
        AlbumService service)
    {
        if (!service.Albums.TryGetValue(albumId, out var album))
        {
            return Results.NotFound($"Album with id {albumId} not found");
        }

        return Results.File(
            await album.GetImageThumbnail(imageId),
            "image/jpeg");
    }

    private static async Task<IResult> GetInfo(
        Guid albumId,
        FilterModel filter,
        AlbumService service)
    {
        if (!service.Albums.TryGetValue(albumId, out var album))
        {
            return Results.NotFound($"Album with id {albumId} not found");
        }

        return Results.Ok(await album.GetInfo(filter));
    }
}
