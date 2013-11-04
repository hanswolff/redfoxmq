using System;
using System.Net;

namespace RedFoxMQ.Transports.Tcp
{
    class IpAddressFromHostTranslator
    {
        public IPAddress GetIpAddressForHost(string host)
        {
            if (host == null) host = "";
            host = host.Trim().ToLowerInvariant();
            if (host == "") return IPAddress.Any;

            IPAddress ipAddress;
            if (IPAddress.TryParse(host, out ipAddress)) return ipAddress;

            if (host == "*")
            {
                return IPAddress.Any;
            }

            if (host == "localhost")
            {
                return IPAddress.Loopback;
            }

            var errorMessage = String.Format("Host must be an IP address or '*' or 'localhost' (provided value: '{0}')", host);
            throw new ArgumentException(errorMessage, "host");
        }
    }
}
