using Microsoft.IdentityModel.Tokens;
using PocketAlbum.Server.Controllers;
using PocketAlbum.Server.Util;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace PocketAlbum.Server.Services;

public interface IAuthService
{
    Guid InstanceId { get; }

    ServerInfo GetServerInfo(ICollection<string> urls);
    string RequestToken(TokenRequest request);
}

public class AuthService(IConfiguration config) : IAuthService
{
    public Guid InstanceId { get; } = Guid.NewGuid();
    private readonly IConfiguration config = config;

    public string RequestToken(TokenRequest request)
    {
        if (request.ServerId != InstanceId)
        {
            throw new ArgumentException("Server id doesn't match");
        }

        lock (this)
        {
            var keyBytes = Encoding.ASCII.GetBytes(request.ConnectionId.ToString());
            var codeKey = SHA1.HashData(keyBytes);

            var providedCode = RequestCodeFromConsole(request);
            string expectedCode = TOTP.Generate(codeKey);

            if (providedCode != expectedCode)
            {
                throw new IOException("Provided pairing code incorrect");
            }

            var jwtSettings = config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Define user identity properties (Claims)
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, request.ConnectionId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, request.ClientName)
            };

            var token = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["DurationInMinutes"])),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(token);

            return tokenHandler.WriteToken(securityToken);
        }
    }

    private static string RequestCodeFromConsole(TokenRequest request)
    {
        Console.WriteLine($"New connection request from {request.ClientName}");
        Console.Write("Enter the pairing code: ");
        string? code;
        do
        {
            code = Console.ReadLine();
        } while (code == null);
        return code.Trim();
    }

    public ServerInfo GetServerInfo(ICollection<string> urls)
    {
        return new ServerInfo()
        {
            InstanceId = InstanceId,
            Endpoints = HostingUtil.GetEndpoints(urls)
        };
    }
}
