// 
// Copyright 2013-2014 Hans Wolff
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

using System.Threading;
using NUnit.Framework;
using RedFoxMQ.Transports;
using System;
using System.Diagnostics;
using System.Linq;

namespace RedFoxMQ.Tests
{
    [TestFixture]
    public class SubscriberTests
    {
        [Test]
        public void IsDisconnected_should_return_true_when_not_connected()
        {
            using (var subscriber = new Subscriber())
            {
                Assert.IsTrue(subscriber.IsDisconnected);
            }
        }

        [Test]
        public void Connect_with_timeout_to_non_existing_endpoint_should_throw_TimeoutException()
        {
            using (var subscriber = new Subscriber())
            {
                var endpoint = TestHelpers.CreateEndpointForTransport(RedFoxTransport.Tcp);

                var sw = Stopwatch.StartNew();

                Assert.Throws<TimeoutException>(() =>
                    subscriber.Connect(endpoint, TimeSpan.FromMilliseconds(100)));

                sw.Stop();
                Assert.GreaterOrEqual(sw.ElapsedMilliseconds, 100);
            }
        }

        [TestCase(RedFoxTransport.Inproc)]
        [TestCase(RedFoxTransport.Tcp)]
        [ExpectedException(typeof(RedFoxProtocolException))]
        public void Subscribe_to_Responder_should_cause_protocol_exception(RedFoxTransport transport)
        {
            using (var responder = TestHelpers.CreateTestResponder())
            using (var subscriber = new TestSubscriber())
            {
                var endpoint = TestHelpers.CreateEndpointForTransport(transport);

                responder.Bind(endpoint);

                try
                {
                    subscriber.Connect(endpoint);
                }
                catch (AggregateException ex)
                {
                    throw ex.InnerExceptions.First();
                }
            }
        }

        [Test]
        public void subscriber_should_ignore_ReceiveTimeout_in_socket_configuration_and_must_not_disconnect_on_timeout()
        {
            using (var publisher = new Publisher())
            using (var subscriber = new Subscriber())
            {
                var endpoint = TestHelpers.CreateEndpointForTransport(RedFoxTransport.Tcp);
                var socketConfiguration = new SocketConfiguration { ReceiveTimeout = TimeSpan.FromMilliseconds(100) };

                var disconnected = new ManualResetEventSlim();
                publisher.ClientDisconnected += s => disconnected.Set();

                publisher.Bind(endpoint);
                subscriber.Connect(endpoint, socketConfiguration);

                Assert.IsFalse(disconnected.Wait(TimeSpan.FromSeconds(1)));
            }
        }
    }
}
