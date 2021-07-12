// Copyright (c) Maila. All rights reserved.
// Licensed under the GNU AGPLv3

using System;
using System.Reflection;

namespace Maila.Cocoa.Framework.Models.Route
{
    internal abstract class RouteInfo
    {
        private readonly BotModuleBase module;
        private readonly MethodInfo route;

        private readonly RouteResultProcessor? processor;

        private readonly Func<MessageSource, bool> pred;

        private readonly bool groupAvailable;
        private readonly bool privateAvailable;

        private readonly bool isValueType;
        private readonly bool isVoid;
        private readonly bool isThreadSafe;

        private readonly object _lock = new();

        public RouteInfo(BotModuleBase module, MethodInfo route, Func<MessageSource, bool> pred)
        {
            this.module = module;
            this.route = route;
            this.pred = pred;

            isThreadSafe = route.GetCustomAttribute<ThreadSafeAttribute>() is not null;
            groupAvailable = route.GetCustomAttribute<DisableInGroupAttribute>() is null;
            privateAvailable = route.GetCustomAttribute<DisableInPrivateAttribute>() is null;

            processor = RouteResultProcessors.GetProcessor(route.ReturnType);

            isVoid = route.ReturnType == typeof(void);
            isValueType = route.ReturnType.IsValueType && !isVoid;
        }

        public bool Run(MessageSource src, QMessage msg)
        {
            if (string.IsNullOrEmpty(msg.PlainText))
            {
                return false;
            }

            if (!(src.IsGroup ? groupAvailable : privateAvailable))
            {
                return false;
            }
            if (!pred(src))
            {
                return false;
            }

            object?[]? args = Check(src, msg);

            if (args is null)
            {
                return false;
            }

            object? result;
            if (isThreadSafe)
            {
                lock (_lock)
                {
                    result = route.Invoke(module, args);
                }
            }
            else
            {
                result = route.Invoke(module, args);
            }

            if (isVoid)
            {
                return true;
            }
            if (processor is not null)
            {
                return processor(src, result);
            }
            if (isValueType)
            {
                return !result?.Equals(Activator.CreateInstance(route.ReturnType)) ?? false;
            }
            return result is not null;
        }

        protected abstract object?[]? Check(MessageSource src, QMessage msg);
    }
}
