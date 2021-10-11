// Copyright (c) Maila. All rights reserved.
// Licensed under the GNU AGPLv3

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Maila.Cocoa.Beans.API;
using Maila.Cocoa.Beans.Models;
using Maila.Cocoa.Beans.Models.Messages;
using Maila.Cocoa.Framework.Models;
using Maila.Cocoa.Framework.Support;

namespace Maila.Cocoa.Framework
{
    public class MessageSource
    {
        public QGroup? Group { get; }
        public QUser User { get; }

        public bool IsFriend { get; }
        public bool IsGroup { get; }
        public bool IsTemp { get; }

        public GroupPermission? Permission { get; }
        public string? MemberCard { get; }

        public MessageSource(long qqId)
        {
            if (!BotInfo.HasFriend(qqId))
            {
                _ = BotInfo.ReloadFriends();
            }

            IsFriend = true;
            IsGroup = false;
            IsTemp = false;
            Group = null;
            User = new(qqId);
        }

        public MessageSource(long groupId, long qqId, GroupPermission? permission, string? memberCard)
        {
            IsFriend = BotInfo.HasFriend(qqId);
            IsGroup = permission is not null;
            IsTemp = permission is null;
            Group = new(groupId);
            User = new(qqId);
            Permission = permission;
            MemberCard = memberCard;
        }

        public override bool Equals(object? obj)
            => obj is MessageSource src
                && src.IsGroup == IsGroup
                && src.IsTemp == IsTemp
                && src.Group?.Id == Group?.Id
                && src.User.Id == User.Id;
        public override int GetHashCode()
            => (IsGroup || IsTemp)
                ? Group!.Id.GetHashCode() ^ User.Id.GetHashCode()
                : User.Id.GetHashCode();

        public int Send(string message)
            => SendAsync(message).Result;

        public int Send(params IMessage[] chain)
            => SendAsync(chain).Result;

        public Task<int> SendAsync(string message)
        {
            return SendAsync(new PlainMessage(message));
        }

        public Task<int> SendAsync(params IMessage[] chain)
            => IsGroup
                ? BotAPI.SendGroupMessage(Group!.Id, chain)
                : BotAPI.SendPrivateMessage(User.Id, chain);


        public int SendEx(bool addAtWhenGroup, string? groupDelimiter, string message)
            => SendExAsync(addAtWhenGroup, groupDelimiter, message).Result;

        public int SendEx(bool addAtWhenGroup, string? groupDelimiter, params IMessage[] chain)
            => SendExAsync(addAtWhenGroup, groupDelimiter, chain).Result;

        public Task<int> SendExAsync(bool addAtWhenGroup, string? groupDelimiter, string message)
        {
            return SendExAsync(addAtWhenGroup, groupDelimiter, new PlainMessage(message));
        }

        public Task<int> SendExAsync(bool addAtWhenGroup, string? groupDelimiter, params IMessage[] chain)
        {
            if (!IsGroup)
            {
                return BotAPI.SendPrivateMessage(User.Id, chain);
            }
            List<IMessage> newChain = new(chain.Length + 2);
            if (addAtWhenGroup)
            {
                newChain.Add(new AtMessage(User.Id));
            }
            if (!string.IsNullOrEmpty(groupDelimiter))
            {
                newChain.Add(new PlainMessage(groupDelimiter));
            }
            newChain.AddRange(chain);
            return BotAPI.SendGroupMessage(Group!.Id, newChain.ToArray());
        }


        public int SendReplyEx(QMessage quote, bool addAtWhenGroup, string message)
            => SendReplyExAsync(quote, addAtWhenGroup, message).Result;

        public int SendReplyEx(QMessage quote, bool addAtWhenGroup, params IMessage[] chain)
            => SendReplyExAsync(quote, addAtWhenGroup, chain).Result;

        public Task<int> SendReplyExAsync(QMessage quote, bool addAtWhenGroup, string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return SendReplyExAsync(quote, addAtWhenGroup);
            }
            return SendReplyExAsync(quote, addAtWhenGroup, new PlainMessage(message));
        }

        public Task<int> SendReplyExAsync(QMessage quote, bool addAtWhenGroup, params IMessage[] chain)
        {
            if (IsGroup)
            {
                List<IMessage> newChain = new(chain.Length + 2);
                if (addAtWhenGroup)
                {
                    newChain.Add(new AtMessage(User.Id));
                    newChain.Add(new PlainMessage(" "));
                }

                newChain.AddRange(chain);
                return BotAPI.SendGroupMessage(quote.Id, Group!.Id, newChain.ToArray());
            }

            return BotAPI.SendPrivateMessage(quote.Id, User.Id, chain);
        }


        public int SendPrivate(string message)
            => SendPrivateAsync(message).Result;

        public int SendPrivate(params IMessage[] chain)
            => SendPrivateAsync(chain).Result;

        public Task<int> SendPrivateAsync(string message)
        {
            return SendPrivateAsync(new PlainMessage(message));
        }

        public Task<int> SendPrivateAsync(params IMessage[] chain)
            => BotAPI.SendPrivateMessage(User.Id, chain);


        public int? SendImage(string path)
            => SendImageAsync(path).Result;

        public async Task<int> SendImageAsync(string path)
        {
            var image = await BotAPI.UploadImage(IsGroup ? UploadType.Group : IsFriend ? UploadType.Friend : UploadType.Temp, path);
            return await SendAsync(image);
        }


        public int SendVoice(string path)
            => SendVoiceAsync(path).Result;

        public async Task<int> SendVoiceAsync(string path)
        {
            var voice = await BotAPI.UploadVoice(path);
            return await SendAsync(voice);
        }

        public void Mute(int duration)
            => MuteAsync(duration);

        public Task MuteAsync(int duration)
            => IsGroup
                ? BotAPI.Mute(Group!.Id, User.Id, duration)
                : Task.CompletedTask;

        public void Mute(TimeSpan duration)
            => MuteAsync(duration);

        public Task MuteAsync(TimeSpan duration)
            => IsGroup
                ? Group!.MuteAsync(User.Id, duration)
                : Task.CompletedTask;

        public void Unmute()
            => UnmuteAsync();

        public Task UnmuteAsync()
            => IsGroup
                ? BotAPI.Unmute(Group!.Id, User.Id)
                : Task.CompletedTask;
    }
}
