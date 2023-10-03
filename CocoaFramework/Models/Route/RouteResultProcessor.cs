// Copyright (c) Maila. All rights reserved.
// Licensed under the GNU AGPLv3

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Maila.Cocoa.Framework.Models.Route
{
    public static class RouteResultProcessor
    {
        internal delegate bool Processor(MessageSource src, QMessage msg, object? returnValue);
        public delegate bool Processor<T>(MessageSource src, QMessage msg, T returnValue);

        private static readonly Dictionary<Type, Processor> processors = new();

        static RouteResultProcessor()
        {
            processors[typeof(void)] = static (_, _, _) => true;
            processors[typeof(Task)] = static (_, _, _) => true;

            RegistProcessor<bool>(static (_, _, result) => result);
            RegistProcessor<string>(static (src, _, result) =>
            {
                if (string.IsNullOrEmpty(result))
                {
                    return false;
                }
                else
                {
                    src.Send(result);
                    return true;
                }
            });
            RegistProcessor<IEnumerator>(static (src, _, result) =>
            {
                Meeting.Start(src, result);
                return true;
            });
            RegistProcessor<IEnumerable>(static (src, _, result) =>
            {
                Meeting.Start(src, result);
                return true;
            });
            RegistProcessor<StringBuilder>(static (src, _, result) =>
            {
                if (result.Length > 0)
                {
                    src.SendAsync(result.ToString());
                    return true;
                }
                else
                {
                    return false;
                }
            });
            RegistProcessor<MessageBuilder>(static (src, _, result) =>
            {
                src.Send(result);
                return true;
            });
        }

        private static Processor Wrap<T>(Processor<T> func)
            => (src, msg, returnVal) => returnVal is T result && func(src, msg, result);

        public static void RegistProcessor<T>(Processor<T> func)
        {
            processors[typeof(T)] = Wrap(func);
            processors[typeof(Task<T>)] = Wrap<Task<T>>((src, msg, task) =>
            {
                Task.Run(async () => func(src, msg, await task));
                return true;
            });
        }

        internal static Processor GetProcessor(Type returnType)
        {
            if (processors.TryGetValue(returnType, out var processor))
            {
                return processor;
            }

            if (returnType.IsValueType)
            {
                return (_, _, returnValue) =>
                {
                    try
                    {
                        var defaultValue = Activator.CreateInstance(returnType);
                        var isDefaultValue = returnValue?.Equals(defaultValue) ?? false;
                        return !isDefaultValue;
                    }
                    catch
                    {
                        return false;
                    }
                };
            }
            else
            {
                return static (_, _, returnValue) => returnValue != null;
            }
        }
    }
}
