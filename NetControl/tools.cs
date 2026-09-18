using System;
using System.Net;

namespace NetControl
{
    public static class Tools
    {
        public static IPAddress ParseIpAddress(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip)) return IPAddress.None;
            if (IPAddress.TryParse(ip.Trim(), out var parsed))
            {
                return parsed;
            }
            return IPAddress.None;
        }

        public static bool AreValuesEqual(byte[] a, byte[] b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null || b == null) return false;
            return a.AsSpan().SequenceEqual(b);
        }

        public static bool AreValuesEqual(ReadOnlySpan<byte> a, ReadOnlySpan<byte> b)
        {
            return a.SequenceEqual(b);
        }
    }
}
