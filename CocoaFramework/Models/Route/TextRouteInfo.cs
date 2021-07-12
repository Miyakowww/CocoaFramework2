// Copyright (c) Maila. All rights reserved.
// Licensed under the GNU AGPLv3

using System;
using System.Linq;
using System.Reflection;

namespace Maila.Cocoa.Framework.Models.Route
{
    internal class TextRouteInfo : RouteInfo
    {
        private readonly string[] texts;
        private readonly bool[] ignoreCases;

        private readonly int srcIndex;
        private readonly int msgIndex;
        private readonly int argCount;

        public TextRouteInfo(BotModuleBase module, MethodInfo route, string[] texts, bool[] ignoreCases, Func<MessageSource, bool> pred) : base(module, route, pred)
        {
            this.texts = texts;
            this.ignoreCases = ignoreCases;

            ParameterInfo[] parameters = route.GetParameters();
            argCount = parameters.Length;
            srcIndex = -1;
            msgIndex = -1;

            for (int i = 0; i < argCount; i++)
            {
                if (parameters[i].GetCustomAttribute<DisabledAttribute>() is not null)
                {
                    continue;
                }

                if (parameters[i].ParameterType == typeof(MessageSource) && srcIndex == -1)
                {
                    srcIndex = i;
                }
                if (parameters[i].ParameterType == typeof(QMessage) && msgIndex == -1)
                {
                    msgIndex = i;
                }
            }
        }

        protected override object?[]? Check(MessageSource src, QMessage msg)
        {
            if (!texts.Where((t, i) => ignoreCases[i]
                                       ? string.Equals(msg.PlainText, t, StringComparison.CurrentCultureIgnoreCase)
                                       : msg.PlainText == t)
                      .Any())
            {
                return null;
            }

            object?[] args = new object?[argCount];
            if (srcIndex != -1)
            {
                args[srcIndex] = src;
            }
            if (msgIndex != -1)
            {
                args[msgIndex] = msg;
            }
            return args;
        }
    }
}
