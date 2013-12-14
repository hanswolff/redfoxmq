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
using NUnit.Framework;
using RedFoxMQ.Transports;
using System;

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

        [TestCase("")]
        [TestCase("invalidprotocol://hostname:1234/")]
        public void Parse_invalid_endpoint_throws_FormatException(string invalidEndpoint)
        {
            Assert.Throws<FormatException>(() => RedFoxEndpoint.Parse(invalidEndpoint));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("invalidprotocol://hostname:1234/")]
        [TestCase("tcp://invalid host name:1234/")]
        [TestCase("tcp://invalidport:-1/")]
        public void TryParse_invalid_endpoint_returns_false(string invalidEndpoint)
        {
            RedFoxEndpoint endpoint;
            Assert.IsFalse(RedFoxEndpoint.TryParse(invalidEndpoint, out endpoint));
        }

        [TestCase("tcp://hostname:1234", RedFoxTransport.Tcp)]
        [TestCase("inproc://hostname:1234", RedFoxTransport.Inproc)]
        public void TryParse_transport_should_be_parsed(string endpointUri, RedFoxTransport expectedTransport)
        {
            RedFoxEndpoint endpoint;
            Assert.IsTrue(RedFoxEndpoint.TryParse(endpointUri, out endpoint));
            Assert.AreEqual(expectedTransport, endpoint.Transport);
        }

        [Test]
        public void TryParse_hostname_should_be_parsed()
        {
            RedFoxEndpoint endpoint;
            Assert.IsTrue(RedFoxEndpoint.TryParse("tcp://hostname:1234", out endpoint));
            Assert.AreEqual("hostname", endpoint.Host);
        }

        [Test]
        public void TryParse_port_should_be_parsed()
        {
            RedFoxEndpoint endpoint;
            Assert.IsTrue(RedFoxEndpoint.TryParse("tcp://hostname:1234", out endpoint));
            Assert.AreEqual(1234, endpoint.Port);
        }

        [Test]
        public void TryParse_inproc_path_should_be_parsed()
        {
            RedFoxEndpoint endpoint;
            Assert.IsTrue(RedFoxEndpoint.TryParse("inproc://hostname:1234/path?query", out endpoint));
            Assert.AreEqual("/path?query", endpoint.Path);
        }

        [Test]
        public void TryParse_tcp_path_should_not_be_parsed()
        {
            RedFoxEndpoint endpoint;
            Assert.IsTrue(RedFoxEndpoint.TryParse("tcp://hostname:1234/path?query", out endpoint));
            Assert.IsNull(endpoint.Path);
        }
    }
}
