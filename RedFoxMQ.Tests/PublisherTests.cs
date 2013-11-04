using NUnit.Framework;
using System;
using RedFoxMQ.Transports;

namespace RedFoxMQ.Tests
{
    [TestFixture]
    public class PublisherTests
    {
        [Test]
        public void publishers_bind_same_endpoint_twice_fails()
        {
            using (var publisher = new Publisher())
            {
                var endpoint = new RedFoxEndpoint("/path");
                publisher.Bind(endpoint);
                Assert.Throws<InvalidOperationException>(() => publisher.Bind(endpoint));
            }
        }

        [Test]
        public void publisher_can_bind_multiple_different_endpoints()
        {
            using (var publisher = new Publisher())
            {
                publisher.Bind(new RedFoxEndpoint("/path1"));
                publisher.Bind(new RedFoxEndpoint("/path2"));
            }
        }

        [Test]
        public void publisher_dispose_unbinds_endpoints()
        {
            using (var publisher = new Publisher())
            {
                publisher.Bind(new RedFoxEndpoint("/path1"));
                publisher.Bind(new RedFoxEndpoint("/path2"));
            }

            using (var publisher = new Publisher())
            {
                publisher.Bind(new RedFoxEndpoint("/path1"));
                publisher.Bind(new RedFoxEndpoint("/path2"));
            }
        }

        [Test]
        public void two_publishers_same_endpoint_fails()
        {
            using (var publisher1 = new Publisher())
            using (var publisher2 = new Publisher())
            {
                var endpoint = new RedFoxEndpoint("/path");
                publisher1.Bind(endpoint);
                Assert.Throws<InvalidOperationException>(() => publisher2.Bind(endpoint));
            }
        }
    }
}
