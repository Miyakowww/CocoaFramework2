﻿// Copyright (c) Maila. All rights reserved.
// Licensed under the GNU AGPLv3

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Maila.Cocoa.Beans.Models;

namespace Maila.Cocoa.Framework.Support
{
    public static class BotInfo
    {
        private static Dictionary<long, QGroupInfo>? groups;
        private static Dictionary<long, Dictionary<long, QMemberInfo>>? members;
        private static Dictionary<long, QFriendInfo>? friends;

        private static readonly TimeSpan CredibleTime = TimeSpan.FromMinutes(1);
        private static DateTime friendsLastSync = DateTime.MinValue;
        private static DateTime groupsLastSync = DateTime.MinValue;

        public static async Task ReloadAll()
        {
            Task f = ReloadFriends();
            Task g = ReloadAllGroupMembers();

            await f;
            await g;
        }

        internal static void Reset()
        {
            groups = null;
            members = null;
            friends = null;
        }

        public static async Task ReloadAllGroupMembers()
        {
            Dictionary<long, QGroupInfo> groups = new();
            Dictionary<long, Dictionary<long, QMemberInfo>> members = new();
            foreach (var info in await BotAPI.GetGroupList())
            {
                groups[info.Id] = info;
                members[info.Id] = (await BotAPI.GetMemberList(info.Id)).ToDictionary(m => m.Id);
            }

            BotInfo.groups = groups;
            BotInfo.members = members;
            groupsLastSync = DateTime.Now;
        }

        public static async Task<bool> ReloadGroupMembers(long groupId)
        {
            if (groups is null || members is null)
            {
                await ReloadAll();
                return true;
            }

            if (members.ContainsKey(groupId))
            {
                members[groupId] = (await BotAPI.GetMemberList(groupId)).ToDictionary(m => m.Id);
                return true;
            }

            if ((await BotAPI.GetGroupList()).FirstOrDefault(i => i.Id == groupId) is not { } gInfo)
            {
                return false;
            }

            groups[groupId] = gInfo;
            members[groupId] = (await BotAPI.GetMemberList(groupId)).ToDictionary(m => m.Id);
            return true;
        }

        public static async Task ReloadFriends()
        {
            friends = (await BotAPI.GetFriendList()).ToDictionary(f => f.Id);
            friendsLastSync = DateTime.Now;
        }

        public static bool HasGroup(long groupId)
        {
            if (DateTime.Now - groupsLastSync > CredibleTime)
            {
                ReloadAllGroupMembers().Wait();
            }
            return groups?.ContainsKey(groupId) ?? false;
        }

        public static QGroupInfo? GetGroupInfo(long groupId)
        {
            if (DateTime.Now - groupsLastSync > CredibleTime)
            {
                ReloadAllGroupMembers().Wait();
            }
            return groups?.GetValueOrDefault(groupId);
        }

        public static QMemberInfo[]? GetMemberList(long groupId)
        {
            if (DateTime.Now - groupsLastSync > CredibleTime)
            {
                ReloadAllGroupMembers().Wait();
            }
            return members?.GetValueOrDefault(groupId)?.Select(p => p.Value).ToArray();
        }

        public static QMemberInfo? GetMemberInfo(long groupId, long memberId)
        {
            if (DateTime.Now - groupsLastSync > CredibleTime)
            {
                ReloadAllGroupMembers().Wait();
            }
            return members?.GetValueOrDefault(groupId)?.GetValueOrDefault(memberId);
        }

        public static bool HasFriend(long qqId)
        {
            if (DateTime.Now - friendsLastSync > CredibleTime)
            {
                ReloadFriends().Wait();
            }
            return friends?.ContainsKey(qqId) ?? false;
        }

        public static QFriendInfo? GetFriendInfo(long qqId)
        {
            if (DateTime.Now - friendsLastSync > CredibleTime)
            {
                ReloadFriends().Wait();
            }
            return friends?.GetValueOrDefault(qqId);
        }

        public static long[] GetTempPath(long qqId)
        {
            if (DateTime.Now - groupsLastSync > CredibleTime)
            {
                ReloadAllGroupMembers().Wait();
            }
            return members?.Where(p => p.Value.ContainsKey(qqId)).Select(p => p.Key).ToArray() ?? Array.Empty<long>();
        }
    }
}
