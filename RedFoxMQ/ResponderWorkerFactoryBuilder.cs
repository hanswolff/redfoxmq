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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RedFoxMQ
{
    public class ResponderWorkerFactoryBuilder
    {
        public IResponderWorkerFactory Create<T>(T hub) where T : class
        {
            if (hub == null) throw new ArgumentNullException("hub");

            var methods = typeof (T)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => !x.IsGenericMethod)
                .Where(x => typeof (IMessage).IsAssignableFrom(x.ReturnType))
                .Where(x => x.GetParameters().Count() == 1 && typeof (IMessage).IsAssignableFrom(x.GetParameters().Single().ParameterType))
                .ToList();

            if (!methods.Any())
                throw new ArgumentException("Hub instance has no worker methods that can be used to be bound " +
                                            "(make sure methods with signature like Func<IMessage, IMessage> exist)");

            var workerFactory = new TypeMappedResponderWorkerFactory();
            var workerFactoryMapMethod = GetWorkerFactoryMapMethod(workerFactory);

            MethodInfo defaultFuncMethod = null;
            var alreadyMappedTypes = new Dictionary<Type, MethodInfo>();

            foreach (var method in methods)
            {
                var inputType = method.GetParameters().Single().ParameterType;

                var funcType = typeof(Func<,>).MakeGenericType(inputType, typeof(IMessage));
                var func = Delegate.CreateDelegate(funcType, hub, method);

                if (inputType == typeof (IMessage))
                {
                    if (defaultFuncMethod != null)
                        throw new ArgumentException(
                            String.Format("Hub instance has multiple methods that are candidates for being a " +
                                          "default responder ('{0}' and '{1}'). There must be only one such method.",
                                defaultFuncMethod.Name, method.Name));

                    defaultFuncMethod = method;
                    var defaultFunc = (Func<IMessage, IMessage>)func;
                    workerFactory.DefaultFunc = m => new ResponderWorker<IMessage>(defaultFunc);
                }
                else
                {
                    MethodInfo alreadyMappedMethod;
                    if (alreadyMappedTypes.TryGetValue(inputType, out alreadyMappedMethod))
                        throw new ArgumentException(
                            String.Format("Hub instance has multiple methods that are candidates for being " +
                                          "responder for input message type '{0}' (methods are: '{1}' and '{2}'). " +
                                          "There must be only one method per input message type.",
                                inputType, alreadyMappedMethod.Name, method.Name));
                    alreadyMappedTypes[inputType] = method;

                    workerFactoryMapMethod.MakeGenericMethod(inputType).Invoke(workerFactory, new object[] {func});
                }
            }
            return workerFactory;
        }

        private static MethodInfo GetWorkerFactoryMapMethod(TypeMappedResponderWorkerFactory workerFactory)
        {
            return workerFactory.GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Single(x => x.Name == "Map" &&
                             x.IsGenericMethod &&
                             x.GetParameters().Count() == 1 &&
                             x.GetParameters().First().ParameterType.Name == typeof (Func<,>).Name);
        }
    }
}
