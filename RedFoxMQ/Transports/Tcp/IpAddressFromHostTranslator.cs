// 
// Copyright 2013 Hans Wolff
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 
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
