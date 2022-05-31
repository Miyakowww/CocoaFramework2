// Copyright (c) Maila. All rights reserved.
// Licensed under the GNU AGPLv3

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Maila.Cocoa.Beans.Models.Messages;
using Maila.Cocoa.Framework.Core;
using Maila.Cocoa.Framework.Support;

namespace Maila.Cocoa.Framework.Models.Route
{
    internal class RegexRouteInfo : RouteInfo
    {
        private readonly Regex[] regexs;
        private readonly bool[] atRequired;
        private readonly BotModuleBase module;

        private readonly int srcIndex;
        private readonly int msgIndex;
        private readonly int argCount;
        private readonly List<(int gnum, int argIndex, int paraType, object? @default)>[] argsIndex;
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

        private static int GetParaType(Type type)
        {
            if (type == typeof(string))
            {
                return 0;
            }

            if (type == typeof(string[]))
            {
                return 1;
            }

            if (type == typeof(List<string>))
            {
                return 2;
            }
            return -1;
        }

        public RegexRouteInfo(BotModuleBase module, MethodInfo route, Regex[] regexs, bool[] atRequired, Func<MessageSource, bool> pred) : base(module, route, pred)
        {
            this.regexs = regexs;
            this.module = module;
            this.atRequired = atRequired;

            ParameterInfo[] parameters = route.GetParameters();
            argCount = parameters.Length;
            argsIndex = new List<(int gnum, int argIndex, int paraType, object? @default)>[regexs.Length];
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

            for (int reId = 0; reId < regexs.Length; reId++)
            {
                argsIndex[reId] = new();
                string[] gNames = regexs[reId].GetGroupNames();
                for (int paraId = 0; paraId < argCount; paraId++)
                {
                    if (parameters[paraId].GetCustomAttribute<DisabledAttribute>() is not null)
                    {
                        continue;
                    }

                    string paraName = parameters[paraId].Name!;
                    string? _default = null;
                    if (parameters[paraId].GetCustomAttribute<GroupNameAttribute>() is { } gName)
                    {
                        paraName = gName.Name;
                        _default = gName.Default;
                    }
                    int paraType = GetParaType(parameters[paraId].ParameterType);
                    if (paraType != -1 && gNames.Contains(paraName))
                    {
                        argsIndex[reId].Add((regexs[reId].GroupNumberFromName(paraName), paraId, paraType,
                            paraType switch
                            {
                                0 => _default,
                                1 => new[] { _default },
                                2 => new[] { _default }.ToList(),
                                _ => null
                            }));
                    }
                }
            }
        }

        protected override object?[]? Check(MessageSource src, QMessage msg)
        {
            (Match match, int index)? CheckCondition(Regex r, int i)
            {
                string tmpText = msg.PlainText;
                if (atRequired[i])
                {
                    if (!msg.GetSubMessages<AtMessage>().Any(at => at.Target == BotAPI.BotQQ))
                    {
                        return null;
                    }

                    tmpText = tmpText.StartsWith(' ') ? tmpText[1..] : tmpText;
                }

                Match match = r.Match(tmpText);
                return match.Success ? (match, i) : null;
            }

            (Match match, var index) = regexs
                .Select(CheckCondition)
                .Where(t => t is not null)
                .Select(t => (t!.Value.match, argsIndex[t.Value.index]))
                .FirstOrDefault();

            if (match is null || index is null)
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
            foreach ((int gName, int argIndex, int paraType, object? _default) in index)
            {
                args[argIndex] = match.Groups[gName].Success
                    ? paraType switch
                    {
                        0 => match.Groups[gName].Value,
                        1 => match.Groups[gName].Captures.Select(c => c.Value).ToArray(),
                        2 => match.Groups[gName].Captures.Select(c => c.Value).ToList(),
                        _ => null
                    }
                    : _default;
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
