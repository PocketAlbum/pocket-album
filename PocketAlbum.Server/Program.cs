using PocketAlbum.Server.Controllers;

namespace PocketAlbum.Server;

public class Program
{
    public static void Main(string[] args)
    {
        ServerHost host = new ServerHost(args, AuthService_ConnectionRequest);
        host.Start().Wait();
        host.AwaitShutdown();
    }

    private static string AuthService_ConnectionRequest(TokenRequest request)
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
}
