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

namespace RedFoxMQ.Tests
{
    [TestFixture]
    public class ServiceQueueTests
    {
        [Test]
        public void AddMessageFrame_single_message_frame_MessageFramesCount_is_one()
        {
            using (var serviceQueue = new ServiceQueue())
            {
                serviceQueue.AddMessageFrame(new MessageFrame());

                Assert.AreEqual(1, serviceQueue.MessageFramesCount);
            }
        }

        [Test]
        public void ServiceQueues_bind_same_endpoint_twice_fails()
        {
            using (var serviceQueue = new ServiceQueue())
            {
                var endpoint = new RedFoxEndpoint("/path");
                serviceQueue.Bind(endpoint);
                Assert.Throws<InvalidOperationException>(() => serviceQueue.Bind(endpoint));
            }
        }

        [Test]
        public void ServiceQueue_can_bind_multiple_different_endpoints()
        {
            using (var serviceQueue = new ServiceQueue())
            {
                serviceQueue.Bind(new RedFoxEndpoint("/path1"));
                serviceQueue.Bind(new RedFoxEndpoint("/path2"));
            }
        }

        [Test]
        public void ServiceQueue_dispose_unbinds_endpoints()
        {
            using (var serviceQueue = new ServiceQueue())
            {
                serviceQueue.Bind(new RedFoxEndpoint("/path1"));
                serviceQueue.Bind(new RedFoxEndpoint("/path2"));
            }

            using (var serviceQueue = new ServiceQueue())
            {
                serviceQueue.Bind(new RedFoxEndpoint("/path1"));
                serviceQueue.Bind(new RedFoxEndpoint("/path2"));
            }
        }

        [Test]
        public void two_ServiceQueues_same_endpoint_fails()
        {
            using (var serviceQueue1 = new ServiceQueue())
            using (var serviceQueue2 = new ServiceQueue())
            {
                var endpoint = new RedFoxEndpoint("/path");
                serviceQueue1.Bind(endpoint);
                Assert.Throws<InvalidOperationException>(() => serviceQueue2.Bind(endpoint));
            }
        }
    }
}
