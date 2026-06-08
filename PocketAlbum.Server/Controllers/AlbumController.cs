using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PocketAlbum.Models;
using PocketAlbum.Server.Services;

namespace PocketAlbum.Server.Controllers;

[ApiController]
[Authorize]
public class AlbumController(AlbumService service) : ControllerBase
{
    private readonly AlbumService service = service;

    [AllowAnonymous]
    [HttpGet("/api/albums")]
    public async Task<IActionResult> GetAlbums()
    {
        var metaList = service.Albums.Values
            .Select(async a => await a.GetMetadata())
            .Select(m => m.Result)
            .ToList();
        return Ok(metaList);
    }

    [HttpGet("/api/albums/{albumId}/index")]
    public async Task<IActionResult> GetAlbumIndex([FromRoute] Guid albumId)
    {
        if (!service.Albums.TryGetValue(albumId, out var album))
        {
            return NotFound($"Album with id {albumId} not found");
        }
        return Ok(await album.GetYearIndex());
    }

    [HttpGet("/api/albums/{albumId}/list")]
    public async Task<IActionResult> ListImages(
        [FromRoute] Guid albumId,
        [FromQuery] FilterModel filter,
        [FromQuery] int index = 0,
        [FromQuery] int page = 100)
    {
        if (!service.Albums.TryGetValue(albumId, out var album))
        {
            return NotFound($"Album with id {albumId} not found");
        }
        var paging = new Interval(index * page, ((index + 1) * page) - 1);
        return Ok(await album.List(filter, paging));
    }

    [HttpGet("/api/albums/{albumId}/images/{imageId}")]
    public async Task<IActionResult> GetImage(
        [FromRoute] Guid albumId,
        [FromRoute] string imageId)
    {
        if (!service.Albums.TryGetValue(albumId, out var album))
        {
            return NotFound($"Album with id {albumId} not found");
        }
        return File(await album.GetImageData(imageId), "image/jpeg");
    }


    [HttpGet("/api/albums/{albumId}/thumbnails/{imageId}")]
    public async Task<IActionResult> GetThumbnail(
        [FromRoute] Guid albumId,
        [FromRoute] string imageId)
    {
        if (!service.Albums.TryGetValue(albumId, out var album))
        {
            return NotFound($"Album with id {albumId} not found");
        }
        return File(await album.GetImageThumbnail(imageId), "image/jpeg");
    }

    [HttpGet("/api/albums/{albumId}/info")]
    public async Task<IActionResult> GetInfo(
        [FromRoute] Guid albumId,
        [FromQuery] FilterModel filter)
    {
        if (!service.Albums.TryGetValue(albumId, out var album))
        {
            return NotFound($"Album with id {albumId} not found");
        }
        return Ok(await album.GetInfo(filter));
    }
}
