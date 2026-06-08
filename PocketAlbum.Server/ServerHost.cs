using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PocketAlbum.Models;
using PocketAlbum.Server.Controllers;
using PocketAlbum.Server.Services;
using PocketAlbum.SQLite;
using System.Text;
using System.Text.Json.Serialization;

namespace PocketAlbum.Server;

public class ServerHost(string[] args, List<IAlbum>? preloadedAlbums = null)
{
    WebApplication? app;
    Task? appTask;

    public async Task<WebApplication> Start()
    {
        var builder = WebApplication.CreateSlimBuilder(args);

        var albums = new Dictionary<Guid, IAlbum>();
        foreach (var album in preloadedAlbums ?? [])
        {
            var metadata = await album.GetMetadata();
            albums.TryAdd(metadata.Id, album);
        }

        var sources = builder.Configuration.GetSection("Albums").Get<string[]>();
        foreach (var source in sources ?? [])
        {
            try
            {
                var album = await LoadAlbumFromSource(source);
                var metadata = await album.GetMetadata();
                albums.TryAdd(metadata.Id, album);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to load album from {source}: {ex.Message}");
            }
        }

        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
        });

        var jwtSettings = builder.Configuration.GetSection("Jwt");
        var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

        builder.Services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero // Removes the default 5-minute grace period
                };
            });
        builder.Services.AddSingleton<IAuthService, AuthService>();
        builder.Services.AddSingleton(new AlbumService(albums));
        builder.Services.AddAuthorization();

        app = builder.Build();

        app.UseAuthentication();
        app.UseAuthorization();
        app.MapAlbumEndpoints();
        app.MapAuthEndpoints();

        appTask = app.RunAsync("http://0.0.0.0:0");

        return app;
    }

    public void AwaitShutdown()
    {
        appTask?.Wait();
    }

    public async Task Stop()
    {
        if (app != null)
        {
            await app.StopAsync();
        }
        app = null;
    }

    private static async Task<IAlbum> LoadAlbumFromSource(string source)
    {
        if (File.Exists(source))
        {
            if (source.EndsWith(".sqlite"))
            {
                return await SQLiteAlbum.Open(source);
            }
            else throw new IOException("Only SQLITE albums supported");
        }
        else throw new IOException("File doesn't exist");
    }

}

[JsonSerializable(typeof(TokenRequest))]
[JsonSerializable(typeof(ServerInfo))]
[JsonSerializable(typeof(List<MetadataModel>))]
[JsonSerializable(typeof(List<YearIndex>))]
[JsonSerializable(typeof(List<ImageInfo>))]
[JsonSerializable(typeof(byte[]))]
[JsonSerializable(typeof(AlbumInfo))]
[JsonSerializable(typeof(FilterModel))]
public partial class AppJsonSerializerContext : JsonSerializerContext
{
}
