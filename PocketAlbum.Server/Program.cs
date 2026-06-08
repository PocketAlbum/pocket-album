namespace PocketAlbum.Server;

public class Program
{
    public static void Main(string[] args)
    {
        ServerHost host = new ServerHost(args);
        host.Start().Wait();
        host.AwaitShutdown();
    }
}
