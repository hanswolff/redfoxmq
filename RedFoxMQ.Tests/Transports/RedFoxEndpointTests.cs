using NUnit.Framework;
using RedFoxMQ.Transports;

namespace RedFoxMQ.Tests.Transports
{
    [TestFixture]
    public class RedFoxEndpointTests
    {
        [Test]
        public void Constructor_default_values()
        {
            var endpoint = new RedFoxEndpoint();

            Assert.AreEqual(RedFoxTransport.Inproc, endpoint.Transport);
            Assert.AreEqual(null, endpoint.Host);
            Assert.AreEqual(0, endpoint.Port);
            Assert.AreEqual(null, endpoint.Path);
        }

        [Test]
        public void Constructor_Host_Port_setting_fields()
        {
            var endpoint = new RedFoxEndpoint("host", 1234);

            Assert.AreEqual(RedFoxTransport.Inproc, endpoint.Transport);
            Assert.AreEqual("host", endpoint.Host);
            Assert.AreEqual(1234, endpoint.Port);
            Assert.AreEqual(null, endpoint.Path);
        }

        [Test]
        public void Constructor_Host_Port_Path_setting_fields()
        {
            var endpoint = new RedFoxEndpoint("host", 1234, "/path");

            Assert.AreEqual(RedFoxTransport.Inproc, endpoint.Transport);
            Assert.AreEqual("host", endpoint.Host);
            Assert.AreEqual(1234, endpoint.Port);
            Assert.AreEqual("/path", endpoint.Path);
        }

        [Test]
        public void Constructor_Transport_Host_Port_Path_setting_fields()
        {
            var endpoint = new RedFoxEndpoint(RedFoxTransport.Tcp, "host", 1234, "/path");

            Assert.AreEqual(RedFoxTransport.Tcp, endpoint.Transport);
            Assert.AreEqual("host", endpoint.Host);
            Assert.AreEqual(1234, endpoint.Port);
            Assert.AreEqual("/path", endpoint.Path);
        }

        [Test]
        public void Equals_all_fields_math_true()
        {
            var endpoint1 = new RedFoxEndpoint(RedFoxTransport.Tcp, "host", 1234, "/path");
            var endpoint2 = new RedFoxEndpoint(RedFoxTransport.Tcp, "host", 1234, "/path");

            Assert.True(endpoint1.Equals(endpoint2));
            Assert.True(endpoint1.Equals((object)endpoint2));
        }

        [Test]
        public void Equals_different_Transport_false()
        {
            var endpoint = new RedFoxEndpoint { Transport = (RedFoxTransport)255 };

            Assert.False(endpoint.Equals(new RedFoxEndpoint()));
            Assert.False(endpoint.Equals((object)new RedFoxEndpoint()));
        }

        [Test]
        public void Equals_different_Host_false()
        {
            var endpoint = new RedFoxEndpoint { Host = "host" };

            Assert.False(endpoint.Equals(new RedFoxEndpoint()));
            Assert.False(endpoint.Equals((object)new RedFoxEndpoint()));
        }

        [Test]
        public void Equals_different_Port_false()
        {
            var endpoint = new RedFoxEndpoint { Port = 1234 };

            Assert.False(endpoint.Equals(new RedFoxEndpoint()));
            Assert.False(endpoint.Equals((object)new RedFoxEndpoint()));
        }

        [Test]
        public void Equals_different_Path_false()
        {
            var endpoint = new RedFoxEndpoint { Path = "/path" };

            Assert.False(endpoint.Equals(new RedFoxEndpoint()));
            Assert.False(endpoint.Equals((object)new RedFoxEndpoint()));
        }

        [Test]
        public void GetHashCode_same_when_all_fields_match()
        {
            var endpoint1 = new RedFoxEndpoint(RedFoxTransport.Tcp, "host", 1234, "/path");
            var endpoint2 = new RedFoxEndpoint(RedFoxTransport.Tcp, "host", 1234, "/path");

            Assert.AreEqual(endpoint1.GetHashCode(), endpoint2.GetHashCode());
        }

        [Test]
        public void GetHashCode_different_Transport()
        {
            var endpoint = new RedFoxEndpoint { Transport = (RedFoxTransport)255 };

            Assert.AreNotEqual(endpoint.GetHashCode(), new RedFoxEndpoint().GetHashCode());
        }

        [Test]
        public void GetHashCode_different_Host()
        {
            var endpoint = new RedFoxEndpoint { Host = "host" };

            Assert.AreNotEqual(endpoint.GetHashCode(), new RedFoxEndpoint().GetHashCode());
        }

        [Test]
        public void GetHashCode_different_Port()
        {
            var endpoint = new RedFoxEndpoint { Port = 1234 };

            Assert.AreNotEqual(endpoint.GetHashCode(), new RedFoxEndpoint().GetHashCode());
        }

        [Test]
        public void GetHashCode_different_Path()
        {
            var endpoint = new RedFoxEndpoint { Path = "/path" };

            Assert.AreNotEqual(endpoint.GetHashCode(), new RedFoxEndpoint().GetHashCode());
        }
    }
}
