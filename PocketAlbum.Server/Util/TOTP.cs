using System.Security.Cryptography;

namespace PocketAlbum.Server.Util;

public static class TOTP
{
    public enum HashMode
    {
        SHA1,
        SHA256,
        SHA512
    }

    public static string Generate(
        byte[] secretKey,
        long timestamp = 0,
        int digits = 6,
        int timestep = 30,
        HashMode mode = HashMode.SHA1)
    {
        if (timestamp == 0)
        {
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        long counter = timestamp / timestep;
        byte[] counterBytes = BitConverter.GetBytes(counter);

        // Ensure big-endian (RFC requirement)
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(counterBytes);
        }

        byte[] hash = ComputeHMAC(mode, secretKey, counterBytes);

        // Dynamic truncation (RFC 4226)
        int offset = hash[hash.Length - 1] & 0x0F;

        int binaryCode =
            (hash[offset] & 0x7F) << 24 |
            (hash[offset + 1] & 0xFF) << 16 |
            (hash[offset + 2] & 0xFF) << 8 |
            hash[offset + 3] & 0xFF;

        int otp = binaryCode % (int)Math.Pow(10, digits);

        return otp.ToString(new string('0', digits));
    }

    private static byte[] ComputeHMAC(HashMode mode, byte[] key, byte[] data)
    {
        return mode switch
        {
            HashMode.SHA1 => new HMACSHA1(key).ComputeHash(data),
            HashMode.SHA256 => new HMACSHA256(key).ComputeHash(data),
            HashMode.SHA512 => new HMACSHA512(key).ComputeHash(data),
            _ => throw new ArgumentException("Invalid hash mode")
        };
    }
}
