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
using System;

namespace RedFoxMQ.Tests
{
    [TestFixture]
    public class TimeSpanExtensionsTests
    {
        [Test]
        public void ToMillisOrZero_infinity_should_be_zero()
        {
            Assert.AreEqual(0, TimeSpan.FromMilliseconds(-1).ToMillisOrZero());
        }

        [Test]
        public void ToMillisOrZero_should_be_milliseconds()
        {
            Assert.AreEqual(12345, TimeSpan.FromMilliseconds(12345).ToMillisOrZero());
        }
    }
}
