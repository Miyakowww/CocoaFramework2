// Copyright (c) Maila. All rights reserved.
// Licensed under the GNU AGPLv3

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Maila.Cocoa.Beans.Models.Messages;
using Maila.Cocoa.Framework.Core;
using Maila.Cocoa.Framework.Support;

namespace Maila.Cocoa.Framework.Models.Route
{
    internal class TextRouteInfo : RouteInfo
    {
        private readonly string[] texts;
        private readonly bool[] ignoreCases;
        private readonly bool[] atRequired;
        private readonly BotModuleBase module;

        private readonly int srcIndex;
        private readonly int msgIndex;
        private readonly int argCount;
        private readonly List<AutoDataIndex> autoDataIndices;

        private class AutoDataIndex
        {
            public int argIndex;
            public int bindingType;
            public string name;
            public Type type;
            public BotModuleBase? sourceModule;
            public Type? sourceModuleType;

            public AutoDataIndex(int argIndex, int bindingType, string name, Type type, BotModuleBase? sourceModule, Type? sourceModuleType)
            {
                this.argIndex = argIndex;
                this.bindingType = bindingType;
                this.name = name;
                this.type = type;
                this.sourceModule = sourceModule;
                this.sourceModuleType = sourceModuleType;
            }
        }

        public TextRouteInfo(BotModuleBase module, MethodInfo route, string[] texts, bool[] ignoreCases, bool[] atRequired, Func<MessageSource, bool> pred) : base(module, route, pred)
        {
            this.texts = texts;
            this.ignoreCases = ignoreCases;
            this.module = module;
            this.atRequired = atRequired;

            ParameterInfo[] parameters = route.GetParameters();
            argCount = parameters.Length;
            autoDataIndices = new();
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
                    BotModuleBase? sourceModule = module;
                    Type? sourceModuleType = null;
                    if (para.GetCustomAttribute<SharedFromAttribute>() is SharedFromAttribute sharedFrom)
                    {
                        sourceModule = null;
                        sourceModuleType = sharedFrom.Type;
                    }

                    Type typeDefinition = paraType.GetGenericTypeDefinition();
                    if (typeDefinition == UserAutoDataType)
                    {
                        autoDataIndices.Add(new AutoDataIndex(
                            i,
                            0 + (para.GetCustomAttribute<MemoryOnlyAttribute>() is null ? 0 : 3),
                            $"{para.Name} {paraType.GenericTypeArguments[0].FullName!.CalculateCRC16():X}",
                            paraType.GenericTypeArguments[0],
                            sourceModule,
                            sourceModuleType
                        ));
                    }
                    else if (typeDefinition == GroupAutoDataType)
                    {
                        autoDataIndices.Add(new AutoDataIndex(
                            i,
                            1 + (para.GetCustomAttribute<MemoryOnlyAttribute>() is null ? 0 : 3),
                            $"{para.Name} {paraType.GenericTypeArguments[0].FullName!.CalculateCRC16():X}",
                            paraType.GenericTypeArguments[0],
                            sourceModule,
                            sourceModuleType
                        ));
                    }
                    else if (typeDefinition == SourceAutoDataType)
                    {
                        autoDataIndices.Add(new AutoDataIndex(
                            i,
                            2 + (para.GetCustomAttribute<MemoryOnlyAttribute>() is null ? 0 : 3),
                            $"{para.Name} {paraType.GenericTypeArguments[0].FullName!.CalculateCRC16():X}",
                            paraType.GenericTypeArguments[0],
                            sourceModule,
                            sourceModuleType
                        ));
                    }
                }
            }
        }

        protected override object?[]? Check(MessageSource src, QMessage msg)
        {
            bool CheckCondition(string t, int i)
            {
                string tmpText = msg.PlainText;
                if (atRequired[i])
                {
                    if (!msg.GetSubMessages<AtMessage>().Any(at => at.Target == BotAPI.BotQQ))
                    {
                        return false;
                    }

                    tmpText = tmpText.StartsWith(' ') ? tmpText[1..] : tmpText;
                }

                return ignoreCases[i]
                           ? string.Equals(tmpText, t, StringComparison.CurrentCultureIgnoreCase)
                           : tmpText == t;
            }

            if (!texts.Where(CheckCondition).Any())
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
            foreach (var autoDataIndex in autoDataIndices)
            {
                if (autoDataIndex.sourceModule == null)
                {
                    autoDataIndex.sourceModule = ModuleCore.Modules.FirstOrDefault(m => m.GetType() == autoDataIndex.sourceModuleType) ?? module;
                }

                args[autoDataIndex.argIndex] = autoDataIndex.bindingType switch
                {
                    0 => autoDataIndex.sourceModule.GetUserAutoData(src, autoDataIndex.name, autoDataIndex.type),
                    1 => autoDataIndex.sourceModule.GetGroupAutoData(src, autoDataIndex.name, autoDataIndex.type),
                    2 => autoDataIndex.sourceModule.GetSourceAutoData(src, autoDataIndex.name, autoDataIndex.type),
                    3 => autoDataIndex.sourceModule.GetUserTempData(src, autoDataIndex.name, autoDataIndex.type),
                    4 => autoDataIndex.sourceModule.GetGroupTempData(src, autoDataIndex.name, autoDataIndex.type),
                    5 => autoDataIndex.sourceModule.GetSourceTempData(src, autoDataIndex.name, autoDataIndex.type),
                    _ => null
                };
            }

            return args;
        }
    }
}
