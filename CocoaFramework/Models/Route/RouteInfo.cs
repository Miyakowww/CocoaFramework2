// Copyright (c) Maila. All rights reserved.
// Licensed under the GNU AGPLv3

using System.Reflection;
using System.Threading;

namespace Maila.Cocoa.Framework.Models.Route
{
    public abstract class RouteInfo
    {
        protected readonly BotModuleBase module;
        protected readonly MethodInfo route;
        protected readonly ParameterInfo[] parameters;

        internal SemaphoreSlim? processLock;

        private readonly RouteResultProcessor.Processor resultProcessor;

        private readonly int srcIndex = -1;
        private readonly int msgIndex = -1;
        private readonly int meetingIndex = -1;

        private readonly bool disabledInGroup;
        private readonly bool disabledInPrivate;

        protected RouteInfo(BotModuleBase module, MethodInfo route)
        {
            this.module = module;
            this.route = route;

            resultProcessor = RouteResultProcessor.GetProcessor(route.ReturnType);

            parameters = route.GetParameters();
            for (int i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                if (parameter.HasAttribute<DisabledAttribute>())
                {
                    continue;
                }

                var parameterType = parameter.ParameterType;
                if (srcIndex == -1 && parameterType == typeof(MessageSource))
                {
                    srcIndex = i;
                }
                if (msgIndex == -1 && parameterType == typeof(QMessage))
                {
                    msgIndex = i;
                }
                if (meetingIndex == -1 && parameterType == typeof(AsyncMeeting))
                {
                    meetingIndex = i;
                }

                if (srcIndex != -1 && msgIndex != -1 && meetingIndex != -1)
                {
                    break;
                }
            }

            disabledInGroup = route.HasAttribute<DisableInGroupAttribute>();
            disabledInPrivate = route.HasAttribute<DisableInPrivateAttribute>();
        }

        internal bool Run(MessageSource src, QMessage msg)
        {
            if (src.IsGroup ? disabledInGroup : disabledInPrivate)
            {
                return false;
            }

            if (!IsMatch(src, msg))
            {
                return false;
            }

            var args = new object?[parameters.Length];

            if (srcIndex > -1)
            {
                args[srcIndex] = src;
            }
            if (msgIndex > -1)
            {
                args[msgIndex] = msg;
            }
            if (meetingIndex > -1)
            {
                args[meetingIndex] = new AsyncMeeting(src);
            }

            FillArguments(src, msg, args);

            try
            {
                processLock?.Wait();

                var result = route.Invoke(module, args);
                return resultProcessor(src, msg, result);
            }
            finally
            {
                processLock?.Release();
            }
        }

        protected abstract bool IsMatch(MessageSource src, QMessage msg);
        protected virtual void FillArguments(MessageSource src, QMessage msg, object?[] args) { }
    }
}
