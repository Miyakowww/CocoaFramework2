// Copyright (c) Maila. All rights reserved.
// Licensed under the GNU AGPLv3

using System;
using System.Linq;
using System.Reflection;
using Maila.Cocoa.Beans.Models.Messages;
using Maila.Cocoa.Framework.Support;

namespace Maila.Cocoa.Framework.Models.Route.BuiltIn
{
    internal class TextRoute : RouteInfo
    {
        private readonly string text;
        private readonly bool ignoreCase;
        private readonly bool atRequired;

        public TextRoute(BotModuleBase module, MethodInfo route, string text, bool ignoreCase, bool atRequired) : base(module, route)
        {
            this.text = text;
            this.ignoreCase = ignoreCase;
            this.atRequired = atRequired;
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

            return ignoreCase
                ? string.Equals(msgText, text, StringComparison.CurrentCultureIgnoreCase)
                : msgText == text;
        }
    }
}
