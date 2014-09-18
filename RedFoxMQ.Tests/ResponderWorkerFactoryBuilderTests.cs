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
    public class ResponderWorkerFactoryBuilderTests
    {
        [Test]
        public void Create_no_method_throws_ArgumentException()
        {
            var builder = new ResponderWorkerFactoryBuilder();
            Assert.Throws<ArgumentException>(() => builder.Create(new TestHubNoMethod()));
        }

        [Test]
        public void Create_single_method()
        {
            var builder = new ResponderWorkerFactoryBuilder();
            var responderFactory = builder.Create(new TestHubSingleMethod());

            var worker = responderFactory.GetWorkerFor(new TestMessage());
            var response = (TestMessage)worker.GetResponse(null, null);
            Assert.AreEqual("hub", response.Text);
        }

        [Test]
        public void Create_single_default_method()
        {
            var builder = new ResponderWorkerFactoryBuilder();
            var responderFactory = builder.Create(new TestHubSingleDefaultMethod());

            var worker = responderFactory.GetWorkerFor(new TestMessage());
            var response = (TestMessage)worker.GetResponse(null, null);
            Assert.AreEqual("hub", response.Text);
        }

        [Test]
        public void Create_multiple_default_methods_throws_ArgumentException()
        {
            var builder = new ResponderWorkerFactoryBuilder();
            Assert.Throws<ArgumentException>(() => builder.Create(new TestHubMultipleDefaultMethods()));
        }

        [Test]
        public void Create_multiple_methods_same_signature_throws_ArgumentException()
        {
            var builder = new ResponderWorkerFactoryBuilder();
            Assert.Throws<ArgumentException>(() => builder.Create(new TestHubMultipleMethodsSameSignature()));
        }

        #region Test Hubs

        public class TestHubNoMethod
        {
        }

        public class TestHubSingleMethod
        {
            public IMessage SomeMethodName(TestMessage message)
            {
                return new TestMessage("hub");
            }
        }

        public class TestHubSingleDefaultMethod
        {
            public IMessage SomeMethodName(IMessage message)
            {
                return new TestMessage("hub");
            }
        }

        public class TestHubMultipleDefaultMethods
        {
            public IMessage SomeMethodName(IMessage message)
            {
                return new TestMessage("hub");
            }

            public IMessage SomeOtherMethod(IMessage message)
            {
                return SomeMethodName(message);
            }
        }

        public class TestHubMultipleMethodsSameSignature
        {
            public IMessage SomeMethodName(TestMessage message)
            {
                return new TestMessage("hub");
            }

            public IMessage SomeOtherMethod(TestMessage message)
            {
                return SomeMethodName(message);
            }
        }

        #endregion
    }
}
