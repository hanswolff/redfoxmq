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
    public class ResponderWorkerTests
    {
        [Test]
        public void GetResponse_constructor_passed_IMessage_IMessage_function_is_called()
        {
            Func<IMessage, IMessage> func = m => new TestMessage("response");
            var responderWorker = new ResponderWorker(func);

            var response = (TestMessage)responderWorker.GetResponse(new TestMessage(), null);
            Assert.AreEqual("response", response.Text);
        }

        [Test]
        public void GetResponse_constructor_passed_nothing_Echo_function_is_called()
        {
            var responderWorker = new ResponderWorker();

            var requestMessage = new TestMessage();
            var response = (TestMessage)responderWorker.GetResponse(requestMessage, null);
            Assert.AreEqual(requestMessage, response);
        }

        [Test]
        public void GetResponse_typed_constructor_passed_nothing_Echo_function_is_called()
        {
            var responderWorker = new ResponderWorker<TestMessage>();

            var requestMessage = new TestMessage();
            var response = (TestMessage)responderWorker.GetResponse(requestMessage, null);
            Assert.AreEqual(requestMessage, response);
        }

        [Test]
        public void GetResponse_typed_constructor_passed_TestMessage_IMessage_function_is_called()
        {
            Func<TestMessage, IMessage> func = m => new TestMessage("response");
            var responderWorker = new ResponderWorker<TestMessage>(func);

            var response = (TestMessage)responderWorker.GetResponse(new TestMessage(), null);
            Assert.AreEqual("response", response.Text);
        }
    }
}
