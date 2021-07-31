﻿// Copyright (c) Maila. All rights reserved.
// Licensed under the GNU AGPLv3

using System;
using System.Threading.Tasks;
using Maila.Cocoa.Beans.API;
using Maila.Cocoa.Beans.Models;
using Maila.Cocoa.Beans.Models.Messages;
using Maila.Cocoa.Framework.Support;

namespace Maila.Cocoa.Framework.Models
{
    public class QGroup
    {
        public long Id { get; }

        public QGroup(long id)
        {
            Id = id;
        }

        public override bool Equals(object? obj)
            => obj is QGroup group && group.Id == Id;
        public override int GetHashCode()
            => Id.GetHashCode();

        public int SendMessage(string message)
            => SendMessageAsync(message).Result;

        public int SendMessage(params IMessage[] chain)
            => SendMessageAsync(chain).Result;

        public Task<int> SendMessageAsync(string message)
        {
            return BotAPI.SendGroupMessage(Id, new PlainMessage(message));
        }

        public Task<int> SendMessageAsync(params IMessage[] chain)
            => BotAPI.SendGroupMessage(Id, chain);

        public int SendImage(string path)
            => SendImageAsync(path).Result;

        public async Task<int> SendImageAsync(string path)
        {
            var image = await BotAPI.UploadImage(UploadType.Group, path);
            return await BotAPI.SendGroupMessage(Id, image);
        }

        public int SendVoice(string path)
            => SendVoiceAsync(path).Result;

        public async Task<int> SendVoiceAsync(string path)
        {
            var voice = await BotAPI.UploadVoice(path);
            return await BotAPI.SendGroupMessage(Id, voice);
        }

        public QGroupInfo? GetGroupInfo()
            => BotInfo.GetGroupInfo(Id);

        public QMemberInfo? GetMemberInfo(long qqId)
            => BotInfo.GetMemberInfo(Id, qqId);

        public void Mute(long qqId, int duration)
            => MuteAsync(qqId, duration);

        public Task MuteAsync(long qqId, int duration)
            => BotAPI.Mute(Id, qqId, duration > 259199 ? 259199 : duration < 0 ? 0 : duration);

        public void Mute(long qqId, TimeSpan duration)
            => MuteAsync(qqId, duration);

        public Task MuteAsync(long qqId, TimeSpan duration)
            => BotAPI.Mute(Id, qqId, duration.TotalSeconds > 259199 ? 259199 : duration.TotalSeconds < 0 ? 0 : (int)duration.TotalSeconds);

        public void Unmute(long qqId)
            => UnmuteAsync(qqId);

        public Task UnmuteAsync(long qqId)
            => BotAPI.Unmute(Id, qqId);

        public void MuteAll()
            => MuteAllAsync();

        public Task MuteAllAsync()
            => BotAPI.MuteAll(Id);

        public void UnmuteAll()
            => UnmuteAllAsync();

        public Task UnmuteAllAsync()
            => BotAPI.UnmuteAll(Id);

        public void Kick(long qqId)
            => KickAsync(qqId);

        public Task KickAsync(long qqId)
            => BotAPI.Kick(Id, qqId);

        public void Leave()
            => LeaveAsync();

        public Task LeaveAsync()
            => BotAPI.Quit(Id);
    }
}
