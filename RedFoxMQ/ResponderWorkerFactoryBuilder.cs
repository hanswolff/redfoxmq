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

using System;
using System.Linq;
using System.Reflection;

namespace RedFoxMQ
{
    public class ResponderWorkerFactoryBuilder
    {
        public IResponderWorkerFactory Create<T>(T hub) where T : class
        {
            var methods = typeof (T)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => !x.IsGenericMethod)
                .Where(x => typeof (IMessage).IsAssignableFrom(x.ReturnType))
                .Where(x => x.GetParameters().Count() == 1 && typeof (IMessage).IsAssignableFrom(x.GetParameters().Single().ParameterType));

            var workerFactory = new TypeMappedResponderWorkerFactory();
            var workerFactoryMapMethod = workerFactory.GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Single(x => x.Name == "Map" &&
                             x.IsGenericMethod &&
                             x.GetParameters().Count() == 1 &&
                             x.GetParameters().First().ParameterType.Name == typeof (Func<,>).Name);

            foreach (var method in methods)
            {
                var inputType = method.GetParameters().Single().ParameterType;
                if (!inputType.IsClass) continue;

                var funcType = typeof(Func<,>).MakeGenericType(inputType, typeof(IMessage));
                var func = Delegate.CreateDelegate(funcType, hub, method);

                workerFactoryMapMethod.MakeGenericMethod(inputType).Invoke(workerFactory, new [] { func });
            }
            return workerFactory;
        }
    }
}
