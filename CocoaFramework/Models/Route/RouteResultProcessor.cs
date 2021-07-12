// Copyright (c) Maila. All rights reserved.
// Licensed under the GNU AGPLv3

using System;
using System.Collections;
using System.Text;

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
            src.Send(res);
            return true;
        }

        private static bool StringBuilder(MessageSource src, object? result)
        {
            if (result is not StringBuilder res || res.Length <= 0)
            {
                return false;
            }
            src.Send(res.ToString());
            return true;
        }
    }
}
