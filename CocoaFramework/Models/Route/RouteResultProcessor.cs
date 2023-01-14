// Copyright (c) Maila. All rights reserved.
// Licensed under the GNU AGPLv3

using System;
using System.Collections;
using System.Text;
using System.Threading.Tasks;

namespace Maila.Cocoa.Framework.Models.Route
{
    internal delegate bool RouteResultProcessor(MessageSource src, object? result);

    internal static class RouteResultProcessors
    {
        public static RouteResultProcessor? GetProcessor(Type type)
        {
            if (type == typeof(bool))
            {
                return (src, res) => res as bool? ?? false;
            }
            if (type == typeof(IEnumerator))
            {
                return Enumerator;
            }
            if (type == typeof(IEnumerable))
            {
                return Enumerable;
            }
            if (type == typeof(string))
            {
                return String;
            }
            if (type == typeof(StringBuilder))
            {
                return StringBuilder;
            }
            if (type == typeof(MessageBuilder))
            {
                return MessageBuilder;
            }
            if (type.IsGenericType
                && type.GetGenericTypeDefinition() == typeof(Task<>)
                && type.GenericTypeArguments.Length > 0)
            {
                var genericType = type.GenericTypeArguments[0];
                if (genericType == typeof(string))
                {
                    return StringTask;
                }
                if (genericType == typeof(StringBuilder))
                {
                    return StringBuilderTask;
                }
                if (genericType == typeof(MessageBuilder))
                {
                    return MessageBuilderTask;
                }
            }
            return null;
        }

        private static bool Enumerator(MessageSource src, object? result)
        {
            if (result is not IEnumerator meeting)
            {
                return false;
            }

            Meeting.Start(src, meeting);
            return true;
        }

        private static bool Enumerable(MessageSource src, object? result)
        {
            if (result is not IEnumerable meeting)
            {
                return false;
            }

            Meeting.Start(src, meeting);
            return true;
        }

        private static bool String(MessageSource src, object? result)
        {
            string? res = result as string;
            if (string.IsNullOrEmpty(res))
            {
                return false;
            }

            src.SendAsync(res);
            return true;
        }

        private static bool StringBuilder(MessageSource src, object? result)
        {
            if (result is not StringBuilder res || res.Length <= 0)
            {
                return false;
            }

            src.SendAsync(res.ToString());
            return true;
        }

        private static bool MessageBuilder(MessageSource src, object? result)
        {
            if (result is not MessageBuilder builder)
            {
                return false;
            }

            src.SendAsync(builder);
            return true;
        }

        private static bool StringTask(MessageSource src, object? result)
        {
            if (result is not Task<string> task)
            {
                return false;
            }

            Task.Run(async () =>
            {
                var res = await task;
                if (!string.IsNullOrEmpty(res))
                {
                    _ = src.SendAsync(res);
                }
            });

            return true;
        }

        private static bool StringBuilderTask(MessageSource src, object? result)
        {
            if (result is not Task<StringBuilder> task)
            {
                return false;
            }

            Task.Run(async () =>
            {
                var res = await task;
                if (res is { Length: > 0 })
                {
                    _ = src.SendAsync(res.ToString());
                }
            });

            return true;
        }

        private static bool MessageBuilderTask(MessageSource src, object? result)
        {
            if (result is not Task<MessageBuilder> task)
            {
                return false;
            }

            Task.Run(async () =>
            {
                var res = await task;
                if (res is not null)
                {
                    _ = src.SendAsync(res);
                }
            });

            return true;
        }
    }
}
