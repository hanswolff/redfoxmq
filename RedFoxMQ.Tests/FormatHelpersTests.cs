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
using System.Collections.Generic;

namespace RedFoxMQ.Tests
{
    [TestFixture]
    public class FormatHelpersTests
    {
        [Test]
        public void FormatHashSet_null_returns_empty_string()
        {
            Assert.AreEqual("", FormatHelpers.FormatHashSet<int>(null));
        }

        [Test]
        public void FormatHashSet_empty_HashSet_returns_empty_string()
        {
            var nodeTypes = new HashSet<NodeType>();
            Assert.AreEqual("", FormatHelpers.FormatHashSet(nodeTypes));
        }

        [Test]
        public void FormatHashSet_single_item_HashSet_returns_single_item_ToString()
        {
            var nodeTypes = new HashSet<NodeType> { NodeType.Publisher };
            Assert.AreEqual(NodeType.Publisher.ToString(), FormatHelpers.FormatHashSet(nodeTypes));
        }

        [Test]
        public void FormatHashSet_two_items_HashSet_contains_both_items_as_ToString()
        {
            var nodeTypes = new HashSet<NodeType> { NodeType.Publisher, NodeType.Subscriber };
            var formatted = FormatHelpers.FormatHashSet(nodeTypes);

            Assert.IsTrue(formatted.Contains(NodeType.Publisher.ToString()), formatted);
            Assert.IsTrue(formatted.Contains(NodeType.Subscriber.ToString()), formatted);
        }
    }
}
