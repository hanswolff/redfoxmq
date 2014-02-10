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

using NUnit.Framework;
using System;

namespace RedFoxMQ.Tests
{
    [TestFixture]
    public class TypeMappedResponderWorkerFactoryTests
    {
        [Test]
        public void GetWorkerFor_constructor_passed_function_is_called()
        {
            var factory = new TypeMappedResponderWorkerFactory
            {
                DefaultFunc = m => { throw new InvalidOperationException(); }
            };

            Assert.Throws<InvalidOperationException>(() => factory.GetWorkerFor(new TestMessage()));
        }

        [Test]
        public void Map_untyped_ResponderWorker()
        {
            Func<IMessage, IMessage> echoFunc = m => new TestMessage("response");
            var factory = new TypeMappedResponderWorkerFactory();

            factory.Map<TestMessage>(new ResponderWorker(echoFunc));

            var requestMessage = new TestMessage();
            var response = factory.GetWorkerFor(requestMessage).GetResponse(requestMessage, null);

            Assert.AreEqual("response", ((TestMessage)response).Text);
        }

        [Test]
        public void Map_typed_ResponderWorker()
        {
            Func<TestMessage, IMessage> echoFunc = m => new TestMessage("response");
            var factory = new TypeMappedResponderWorkerFactory();

            factory.Map(new ResponderWorker<TestMessage>(echoFunc));

            var requestMessage = new TestMessage();
            var response = factory.GetWorkerFor(requestMessage).GetResponse(requestMessage, null);

            Assert.AreEqual("response", ((TestMessage)response).Text);
        }

        [Test]
        public void Map_typed_request_message()
        {
            Func<TestMessage, IMessage> echoFunc = m => new TestMessage("response");
            var factory = new TypeMappedResponderWorkerFactory();

            factory.Map(echoFunc);

            var requestMessage = new TestMessage();
            var response = factory.GetWorkerFor(requestMessage).GetResponse(requestMessage, null);

            Assert.AreEqual("response", ((TestMessage)response).Text);
        }
    }
}
