// Copyright (c) Maila. All rights reserved.
// Licensed under the GNU AGPLv3

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Maila.Cocoa.Beans.Models.Messages;
using Maila.Cocoa.Framework.Support;

namespace Maila.Cocoa.Framework.Models.Route
{
    internal class AtRouteInfo : RouteInfo
    {
        private readonly string[] texts;
        private readonly bool[] ignoreCases;
        private readonly BotModuleBase module;

        private readonly int srcIndex;
        private readonly int msgIndex;
        private readonly int argCount;
        private readonly List<(int argIndex, int bindingType, string name, Type type)> autoDataIndex;

        public AtRouteInfo(BotModuleBase module, MethodInfo route, string[] texts, bool[] ignoreCases, Func<MessageSource, bool> pred) : base(module, route, pred)
        {
            this.texts = texts;
            this.ignoreCases = ignoreCases;
            this.module = module;

            ParameterInfo[] parameters = route.GetParameters();
            argCount = parameters.Length;
            autoDataIndex = new();
            srcIndex = -1;
            msgIndex = -1;

            for (int i = 0; i < argCount; i++)
            {
                ParameterInfo para = parameters[i];
                if (para.GetCustomAttribute<DisabledAttribute>() is not null)
                {
                    continue;
                }

                Type paraType = para.ParameterType;
                if (srcIndex == -1 && paraType == typeof(MessageSource))
                {
                    srcIndex = i;
                }
                else if (msgIndex == -1 && paraType == typeof(QMessage))
                {
                    msgIndex = i;
                }
                else if (paraType.IsGenericType)
                {
                    Type typeDefinition = paraType.GetGenericTypeDefinition();
                    if (typeDefinition == UserAutoDataType)
                    {
                        autoDataIndex.Add((i, 0 + (para.GetCustomAttribute<MemoryOnlyAttribute>() is null ? 0 : 3),
                                           $"{para.Name} {paraType.GenericTypeArguments[0].FullName!.CalculateCRC16():X}",
                                           paraType.GenericTypeArguments[0]));
                    }
                    else if (typeDefinition == GroupAutoDataType)
                    {
                        autoDataIndex.Add((i, 1 + (para.GetCustomAttribute<MemoryOnlyAttribute>() is null ? 0 : 3),
                                           $"{para.Name} {paraType.GenericTypeArguments[0].FullName!.CalculateCRC16():X}",
                                           paraType.GenericTypeArguments[0]));
                    }
                    else if (typeDefinition == SourceAutoDataType)
                    {
                        autoDataIndex.Add((i, 2 + (para.GetCustomAttribute<MemoryOnlyAttribute>() is null ? 0 : 3),
                                           $"{para.Name} {paraType.GenericTypeArguments[0].FullName!.CalculateCRC16():X}",
                                           paraType.GenericTypeArguments[0]));
                    }
                }
            }
        }

        protected override object?[]? Check(MessageSource src, QMessage msg)
        {
            string text = msg.PlainText;
            text = text.StartsWith(' ') ? text[1..] : text;

            if (!msg.GetSubMessages<AtMessage>().Any(at => at.Target == BotAPI.BotQQ)
             || !texts.Where((t, i) => ignoreCases[i]
                                       ? string.Equals(text, t, StringComparison.CurrentCultureIgnoreCase)
                                       : text == t)
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
            foreach ((int argIndex, int bindingType, string name, Type type) in autoDataIndex)
            {
                args[argIndex] = bindingType switch
                {
                    0 => module.GetUserAutoData(src, name, type),
                    1 => module.GetGroupAutoData(src, name, type),
                    2 => module.GetSourceAutoData(src, name, type),
                    3 => module.GetUserTempData(src, name, type),
                    4 => module.GetGroupTempData(src, name, type),
                    5 => module.GetSourceTempData(src, name, type),
                    _ => null
                };
            }

            return args;
        }
    }
}
