namespace PocketAlbum;

/// <summary>
/// Class providing helful tools for CRC calculation. Code is ported from zlib library:
/// https://github.com/madler/zlib/blob/develop/crc32.c
/// </summary>
internal class CrcUtilities
{
    const uint POLY = 0xedb88320;

    static uint[] x2n_table = [
        0x40000000, 0x20000000, 0x08000000, 0x00800000, 0x00008000,
        0xedb88320, 0xb1e6b092, 0xa06a2517, 0xed627dae, 0x88d14467,
        0xd7bbfe6a, 0xec447f11, 0x8e7ea170, 0x6427800e, 0x4d47bae0,
        0x09fe548f, 0x83852d0f, 0x30362f1a, 0x7b5a9cc3, 0x31fec169,
        0x9fec022a, 0x6c8dedc4, 0x15d6874d, 0x5fde7a4e, 0xbad90e37,
        0x2e4e5eef, 0x4eaba214, 0xa8a472c0, 0x429a969e, 0x148d302a,
        0xc40ba6d0, 0xc4e22c3c
    ];

    /*
    Return a(x) multiplied by b(x) modulo p(x), where p(x) is the CRC polynomial,
    reflected. For speed, this requires that a not be zero.
    */
    static uint multmodp(uint a, uint b)
    {
        uint m = (uint)1 << 31;
        uint p = 0;
        while (true)
        {
            if ((a & m) > 0)
            {
                p ^= b;
                if ((a & (m - 1)) == 0)
                {
                    break;
                }
            }
            m >>= 1;
            b = (b & 1) > 0 ? (b >> 1) ^ POLY : b >> 1;
        }
        return p;
    }

    /*
      Return x^(n * 2^k) modulo p(x). Requires that x2n_table[] has been
      initialized.
     */
    static uint x2nmodp(ulong n, uint k)
    {
        uint p = (uint)1 << 31; // x^0 == 1
        while (n > 0)
        {
            if ((n & 1) > 0)
            {
                p = multmodp(x2n_table[k & 31], p);
            }
            n >>= 1;
            k++;
        }
        return p;
    }

    /// <summary>
    /// Calculate combined CRC32 value from two CRC values and length the of second part.
    /// </summary>
    /// <param name="crc1">CRC32 of the first part</param>
    /// <param name="crc2">CRC32 of the second part</param>
    /// <param name="len2">length of the second part in bytes</param>
    /// <returns></returns>
    public static uint CombineCrc32(uint crc1, uint crc2, ulong len2)
    {
        return multmodp(x2nmodp(len2, 3), crc1) ^ (crc2 & uint.MaxValue);
    }
}
