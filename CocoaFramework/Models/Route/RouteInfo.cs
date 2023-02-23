// Copyright (c) Maila. All rights reserved.
// Licensed under the GNU AGPLv3

using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Maila.Cocoa.Framework.Models.Route
{
    internal abstract class RouteInfo
    {
        private readonly BotModuleBase module;
        private readonly MethodInfo route;

        private readonly RouteResultProcessor? processor;

        private readonly Predicate<MessageSource> identityPred;

        private readonly bool groupAvailable;
        private readonly bool privateAvailable;

        private readonly bool isValueType;
        private readonly bool isVoid;
        private readonly bool isThreadSafe;

        private readonly object _lock = new();

        protected static readonly Type UserAutoDataType = typeof(UserAutoData<>);
        protected static readonly Type GroupAutoDataType = typeof(GroupAutoData<>);
        protected static readonly Type SourceAutoDataType = typeof(SourceAutoData<>);

        public RouteInfo(BotModuleBase module, MethodInfo route)
        {
            this.module = module;
            this.route = route;

            var identityReqs = route.GetCustomAttributes<IdentityRequirementsAttribute>()
                              .Select<IdentityRequirementsAttribute, Predicate<MessageSource>>(r => src => r.Check(src.User.Identity, src.Permission))
                              .ToList();
            identityPred = identityReqs.Any()
                ? src => identityReqs.Any(p => p(src))
                : src => true;

            isThreadSafe = route.GetCustomAttribute<ThreadSafeAttribute>() is not null
                        || route.GetCustomAttribute<AsyncStateMachineAttribute>() is not null;
            groupAvailable = route.GetCustomAttribute<DisableInGroupAttribute>() is null;
            privateAvailable = route.GetCustomAttribute<DisableInPrivateAttribute>() is null;

            processor = RouteResultProcessors.GetProcessor(route.ReturnType);

            isVoid = route.ReturnType == typeof(void);
            isValueType = route.ReturnType.IsValueType && !isVoid;
        }

        public bool Run(MessageSource src, QMessage msg)
        {
            if (msg.PlainText is null)
            {
                return false;
            }

            if (!(src.IsGroup ? groupAvailable : privateAvailable))
            {
                return false;
            }

            if (!identityPred(src))
            {
                return false;
            }

            object?[]? args = Check(src, msg);

            if (args is null)
            {
                return false;
            }

            if (isVoid)
            {
                Task.Run(() =>
                {
                    if (isThreadSafe)
                    {
                        lock (_lock)
                        {
                            route.Invoke(module, args);
                        }
                    }
                    else
                    {
                        route.Invoke(module, args);
                    }
                });
                return true;
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
