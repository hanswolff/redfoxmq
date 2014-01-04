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
using System.Threading;

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

        [Test]
        public void unbind_disconnects_client()
        {
            using (var publisher = new Publisher())
            using (var subscriber = new Subscriber())
            {
                var endpoint = new RedFoxEndpoint("/path");

                var connected = new ManualResetEventSlim();
                var disconnected = new ManualResetEventSlim();

                publisher.ClientConnected += (s, c) => connected.Set();
                publisher.ClientDisconnected += s => disconnected.Set();
                
                publisher.Bind(endpoint);
                subscriber.Connect(endpoint);
                
                Assert.IsTrue(connected.Wait(TimeSpan.FromSeconds(1)));

                publisher.Unbind(endpoint);

                Assert.IsTrue(disconnected.Wait(TimeSpan.FromSeconds(1)));
            }
        }
    }
}
