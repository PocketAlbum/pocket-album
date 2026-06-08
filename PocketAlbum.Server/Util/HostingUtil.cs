using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace PocketAlbum.Server.Util;

public static class HostingUtil
{
    public static List<string> GetEndpoints(ICollection<string> addresses)
    {
        var resolvedAddresses = new List<string>();
        var localIpAddresses = GetLocalIpAddresses();

        foreach (var address in addresses)
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

    private static List<IPAddress> GetLocalIpAddresses()
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
