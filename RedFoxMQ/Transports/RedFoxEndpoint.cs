using System;
using System.Collections.Generic;

namespace RedFoxMQ.Transports
{
    public struct RedFoxEndpoint : IEquatable<RedFoxEndpoint>, IEqualityComparer<RedFoxEndpoint>
    {
        public RedFoxTransport Transport;
        public string Host;
        public int Port;
        public string Path;

        public RedFoxEndpoint(string path)
            : this(RedFoxTransport.Inproc, null, 0, path)
        {
        }

        public RedFoxEndpoint(string host, int port)
            : this(host, port, null)
        {
        }

        public RedFoxEndpoint(string host, int port, string path)
            : this(RedFoxTransport.Inproc, host, port, path)
        {
        }

        public RedFoxEndpoint(RedFoxTransport transport, string host, int port, string path)
        {
            Transport = transport;
            Host = host;
            Port = port;
            Path = path;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is RedFoxEndpoint)) return false;
            return Equals(this, (RedFoxEndpoint)obj);
        }

        public bool Equals(RedFoxEndpoint other)
        {
            return Equals(this, other);
        }

        public bool Equals(RedFoxEndpoint x, RedFoxEndpoint y)
        {
            return 
                x.Port == y.Port && 
                x.Host == y.Host && 
                x.Path == y.Path && 
                x.Transport == y.Transport;
        }

        public override int GetHashCode()
        {
            return GetHashCode(this);
        }

        public int GetHashCode(RedFoxEndpoint x)
        {
            unchecked
            {
                var hashCode = (int)Transport;
                hashCode = (hashCode * 397) ^ (Host != null ? Host.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Port;
                hashCode = (hashCode * 397) ^ (Path != null ? Path.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override string ToString()
        {
            return Transport.ToString().ToLowerInvariant() + "://" + Host + ":" + Port + Path;
        }
    }
}
