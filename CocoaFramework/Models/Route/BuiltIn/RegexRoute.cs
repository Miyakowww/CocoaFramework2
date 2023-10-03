// Copyright (c) Maila. All rights reserved.
// Licensed under the GNU AGPLv3

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Maila.Cocoa.Beans.Models.Messages;
using Maila.Cocoa.Framework.Support;

namespace Maila.Cocoa.Framework.Models.Route.BuiltIn
{
    internal class RegexRoute : RouteInfo
    {
        private readonly Regex regex;
        private readonly bool atRequired;
        private readonly List<RegexGroupIndex> groupIndexes = new();

        public RegexRoute(BotModuleBase module, MethodInfo route, Regex regex, bool atRequired) : base(module, route)
        {
            this.regex = regex;
            this.atRequired = atRequired;

            var groupNames = regex.GetGroupNames();
            for (int i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                if (parameter.HasAttribute<DisabledAttribute>())
                {
                    continue;
                }

                var name = parameter.Name!;
                if (parameter.TryGetAttribute<GroupNameAttribute>(out var groupNameInfo))
                {
                    name = groupNameInfo.Name;
                }

                var parameterType = GetParameterType(parameters[i].ParameterType);
                if (parameterType != ParameterType.Unknown && groupNames.Contains(name))
                {
                    groupIndexes.Add(new()
                    {
                        groupNumber = regex.GroupNumberFromName(name),
                        parameterIndex = i,
                        parameterType = parameterType,
                    });
                }
            }
        }

        protected override bool IsMatch(MessageSource src, QMessage msg)
        {
            var msgText = msg.PlainText;
            if (atRequired)
            {
                if (!msg.GetSubMessages<AtMessage>().Any(at => at.Target == BotAPI.BotQQ))
                {
                    return false;
                }

                msgText = msgText.StartsWith(' ') ? msgText[1..] : msgText;
            }

            return regex.IsMatch(msgText);
        }

        protected override void FillArguments(MessageSource src, QMessage msg, object?[] args)
        {
            var msgText = msg.PlainText;
            if (atRequired)
            {
                msgText = msgText.StartsWith(' ') ? msgText[1..] : msgText;
            }

            var match = regex.Match(msgText);
            foreach (var index in groupIndexes)
            {
                index.Fill(args, match);
            }
        }

        private static ParameterType GetParameterType(Type type)
        {
            if (type == typeof(string))
            {
                return ParameterType.String;
            }

            if (type == typeof(string[]))
            {
                return ParameterType.StringArray;
            }

            if (type == typeof(List<string>))
            {
                return ParameterType.StringList;
            }

            return ParameterType.Unknown;
        }

        private enum ParameterType
        {
            String,
            StringArray,
            StringList,
            Unknown
        }

        private class RegexGroupIndex
        {
            public int groupNumber;
            public int parameterIndex;
            public ParameterType parameterType;

            public void Fill(object?[] args, Match match)
            {
                var group = match.Groups[groupNumber];
                args[parameterIndex] = group.Success
                    ? parameterType switch
                    {
                        ParameterType.String => group.Value,
                        ParameterType.StringArray => group.Captures.Select(c => c.Value).ToArray(),
                        ParameterType.StringList => group.Captures.Select(c => c.Value).ToList(),
                        _ => null
                    }
                    : parameterType switch
                    {
                        ParameterType.String => null,
                        ParameterType.StringArray => Array.Empty<string>(),
                        ParameterType.StringList => new List<string>(),
                        _ => null
                    };
            }
        }
    }
}
