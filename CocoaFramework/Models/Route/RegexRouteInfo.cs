// Copyright (c) Maila. All rights reserved.
// Licensed under the GNU AGPLv3

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Maila.Cocoa.Framework.Models.Route
{
    internal class RegexRouteInfo : RouteInfo
    {
        private readonly Regex[] regexs;

        private readonly int srcIndex;
        private readonly int msgIndex;
        private readonly int argCount;
        private readonly List<(int gnum, int argIndex, int paraType, object? @default)>[] argsIndex;

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

        public RegexRouteInfo(BotModuleBase module, MethodInfo route, Regex[] regexs, Func<MessageSource, bool> pred) : base(module, route, pred)
        {
            this.regexs = regexs;

            ParameterInfo[] parameters = route.GetParameters();
            argCount = parameters.Length;
            argsIndex = new List<(int gnum, int argIndex, int paraType, object? @default)>[regexs.Length];
            srcIndex = -1;
            msgIndex = -1;

            for (int i = 0; i < argCount; i++)
            {
                if (parameters[i].GetCustomAttribute<DisabledAttribute>() is not null)
                {
                    continue;
                }

                if (srcIndex == -1 && parameters[i].ParameterType == typeof(MessageSource))
                {
                    srcIndex = i;
                }
                else if (msgIndex == -1 && parameters[i].ParameterType == typeof(QMessage))
                {
                    msgIndex = i;
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
                                1 => new[] { _default! },
                                2 => new[] { _default! }.ToList(),
                                _ => null
                            }));
                    }
                }
            }
        }

        protected override object?[]? Check(MessageSource src, QMessage msg)
        {
            (Match match, var index) = regexs
                .Select((r, i) => (match: r.Match(msg.PlainText), index: argsIndex[i]))
                .FirstOrDefault(t => t.match.Success);

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
                        1 => match.Groups[gName].Captures.ToArray(),
                        2 => match.Groups[gName].Captures.ToList(),
                        _ => null
                    }
                    : _default;
            }

            return args;
        }
    }
}
