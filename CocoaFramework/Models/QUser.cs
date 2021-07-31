// Copyright (c) Maila. All rights reserved.
// Licensed under the GNU AGPLv3

using System.Threading.Tasks;
using Maila.Cocoa.Beans.API;
using Maila.Cocoa.Beans.Models.Messages;
using Maila.Cocoa.Framework.Support;

namespace Maila.Cocoa.Framework.Models
{
    public class QUser
    {
        public long Id { get; }

        public UserIdentity Identity => BotAuth.GetIdentity(Id);
        public bool IsFriend => BotInfo.HasFriend(Id);

        public bool IsOwner => Identity.Fit(UserIdentity.Owner);
        public bool IsAdmin => Identity.Fit(UserIdentity.Admin);
        public bool IsDeveloper => Identity.Fit(UserIdentity.Developer);
        public bool IsDebugger => Identity.Fit(UserIdentity.Debugger);
        public bool IsOperator => Identity.Fit(UserIdentity.Operator);
        public bool IsStaff => Identity.Fit(UserIdentity.Staff);

        public QUser(long id)
        {
            Id = id;
        }
        public override bool Equals(object? obj)
            => obj is QUser user && user.Id == Id;
        public override int GetHashCode()
            => Id.GetHashCode();

        public int SendMessage(string message)
            => SendMessageAsync(message).Result;

        public int SendMessage(params IMessage[] chain)
            => SendMessageAsync(chain).Result;

        public Task<int> SendMessageAsync(string message)
        {
            return BotAPI.SendPrivateMessage(Id, new PlainMessage(message));
        }

        public Task<int> SendMessageAsync(params IMessage[] chain)
            => BotAPI.SendPrivateMessage(Id, chain);

        public int SendImage(string path)
            => SendImageAsync(path).Result;

        public async Task<int> SendImageAsync(string path)
        {
            var image = await BotAPI.UploadImage(IsFriend ? UploadType.Friend : UploadType.Temp, path);
            return await BotAPI.SendPrivateMessage(Id, image);
        }

        public int SendVoice(string path)
            => SendVoiceAsync(path).Result;

        public async Task<int> SendVoiceAsync(string path)
        {
            var voice = await BotAPI.UploadVoice(path);
            return await BotAPI.SendPrivateMessage(Id, voice);
        }
    }
}
