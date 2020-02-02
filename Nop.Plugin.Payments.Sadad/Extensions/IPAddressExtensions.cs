using System.Net;

namespace Nop.Plugin.Payments.Sadad.Extensions
{
    public static class IPAddressExtensions
    {
        public static bool TryParseIPAddress(string ip, out IPAddress ipAddress)
        {
            if (ip.Contains(".") && ip.Contains(":"))
            {
                string ipAdd = ip.Substring(0, ip.IndexOf(':'));
                return IPAddress.TryParse(ipAdd, out ipAddress);
            }
            else
            {
                ipAddress = null;
                return false;
            }
        }

        public static string ToStringIPAddress(this byte[] value)
        {
            return (new IPAddress(value)).ToString();
        }
    }
}
