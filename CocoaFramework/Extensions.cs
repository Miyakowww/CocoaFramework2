// Copyright (c) Maila. All rights reserved.
// Licensed under the GNU AGPLv3

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Maila.Cocoa.Beans.API;
using Maila.Cocoa.Beans.Models.Events;
using Maila.Cocoa.Framework.Support;

namespace Maila.Cocoa.Framework
{
    public static partial class Extensions
    {
        #region === User Identity ===

        public static bool Fit(this UserIdentity identity, UserIdentity requirements)
        {
            return (identity & requirements) == requirements;
        }

        #endregion

        #region === Request Event ===

        public static Task Response(this NewFriendRequestEvent e, NewFriendRequestOperate operate, string message = "")
            => BotAPI.NewFriendRequestResp(e, operate, message);

        public static Task Response(this MemberJoinRequestEvent e, MemberJoinRequestOperate operate, string message = "")
            => BotAPI.MemberJoinRequestResp(e, operate, message);

        public static Task Response(this BotInvitedJoinGroupRequestEvent e, BotInvitedJoinGroupRequestOperate operate, string message = "")
            => BotAPI.BotInvitedJoinGroupRequestResp(e, operate, message);

        #endregion

        #region === Attribute ===

        public static bool TryGetAttribute<T>(this MemberInfo member, [NotNullWhen(true)] out T? attribute) where T : Attribute
        {
            attribute = member?.GetCustomAttribute<T>();
            return attribute != null;
        }

        public static bool HasAttribute<T>(this MemberInfo member) where T : Attribute
        {
            return member?.IsDefined(typeof(T), true) ?? false;
        }

        public static bool TryGetAttribute<T>(this ParameterInfo parameter, [NotNullWhen(true)] out T? attribute) where T : Attribute
        {
            attribute = parameter?.GetCustomAttribute<T>();
            return attribute != null;
        }

        public static bool HasAttribute<T>(this ParameterInfo parameter) where T : Attribute
        {
            return parameter?.IsDefined(typeof(T), true) ?? false;
        }

        #endregion

        internal static ushort CalculateCRC16(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return 0;
            }

            byte[] data = Encoding.UTF8.GetBytes(str);
            uint crc = 0xFFFF;
            foreach (var b in data)
            {
                crc ^= b;
                for (int i = 0; i < 8; i++)
                {
                    crc = (crc & 1) != 0 ? (crc >> 1) ^ 0xA001 : crc >> 1;
                    crc &= 0xFFFF;
                }
            }
            return (ushort)crc;
        }
    }
}
