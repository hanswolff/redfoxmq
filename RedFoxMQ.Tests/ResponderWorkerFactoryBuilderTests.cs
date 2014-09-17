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

namespace RedFoxMQ.Tests
{
    [TestFixture]
    public class ResponderWorkerFactoryBuilderTests
    {
        [Test]
        public void Create()
        {
            var builder = new ResponderWorkerFactoryBuilder();
            var responderFactory = builder.Create(new TestResponderHub());

            var worker = responderFactory.GetWorkerFor(new TestMessage());
            var response = (TestMessage)worker.GetResponse(null, null);
            Assert.AreEqual("hub", response.Text);
        }

        public class TestResponderHub
        {
            public IMessage Respond(TestMessage message)
            {
                return new TestMessage("hub");
            }
        }
    }
}
