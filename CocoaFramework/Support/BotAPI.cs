// Copyright (c) Maila. All rights reserved.
// Licensed under the GNU AGPLv3

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Maila.Cocoa.Beans.API;
using Maila.Cocoa.Beans.Exceptions;
using Maila.Cocoa.Beans.Models;
using Maila.Cocoa.Beans.Models.Events;
using Maila.Cocoa.Beans.Models.Files;
using Maila.Cocoa.Beans.Models.Messages;
using Maila.Cocoa.Framework.Core;

namespace Maila.Cocoa.Framework.Support
{
    public static class BotAPI
    {
        public static long? BotQQ => BotCore.BindingQQ;
        public static string? SessionKey => BotCore.SessionKey;

        private static readonly Dictionary<Type, Action<Event>> eventListeners = new();

        private static CancellationTokenSource? source;

        static BotAPI()
        {
            eventListeners[typeof(FriendMessageEvent)] = e =>
            {
                var fm = (FriendMessageEvent)e;
                BotCore.OnMessage(new(fm.Sender.Id), new(fm.MessageChain.ToArray()));
            };

            eventListeners[typeof(GroupMessageEvent)] = e =>
            {
                var gm = (GroupMessageEvent)e;
                BotCore.OnMessage(new(gm.Sender.Group.Id, gm.Sender.Id, gm.Sender.Permission, gm.Sender.MemberName), new(gm.MessageChain.ToArray()));
            };

            eventListeners[typeof(TempMessageEvent)] = e =>
            {
                var tm = (TempMessageEvent)e;
                BotCore.OnMessage(new(tm.Sender.Group.Id, tm.Sender.Id, null, null), new(tm.MessageChain.ToArray()));
            };
        }

        internal static async void Init()
        {
            if (!BotCore.Connected)
            {
                return;
            }

            source = new();
            string? ver = BotCore.host is null ? null : await MiraiAPI.About(BotCore.host);
            if (ver is null)
            {
                _ = BotCore.Disconnect();
                return;
            }

            if (ver.StartsWith('2'))
            {
                MiraiAPI.ListenAllEvent(BotCore.host!, BotCore.SessionKey!, BotCore.verifyKey, BotCore.BindingQQ!.Value,
                        e => eventListeners.GetValueOrDefault(e.GetType())?.Invoke(e),
                        Init, source.Token);
            }
            else
            {
                MiraiAPI.ListenAllEventv1(BotCore.host!, BotCore.SessionKey!,
                        e => eventListeners.GetValueOrDefault(e.GetType())?.Invoke(e),
                        Init, source.Token);
            }
        }

        internal static void Reset()
        {
            source?.Cancel();
            source = null;
            OnDisconnect?.Invoke();
        }

        #region === Events ===

        private static void SetBotEvent<T>(Action? val) where T : BotEvent
        {
            Type type = typeof(T);
            if (val is null)
            {
                if (eventListeners.ContainsKey(type))
                {
                    eventListeners.Remove(type);
                }
            }
            else
            {
                eventListeners[type] = (e) =>
                {
                    if (((T)e).QQ == BotCore.BindingQQ)
                    {
                        val.Invoke();
                    }
                };
            }
        }
        private static void SetEvent<T>(Action<T>? val) where T : Event
        {
            Type type = typeof(T);
            if (val is null)
            {
                if (eventListeners.ContainsKey(type))
                {
                    eventListeners.Remove(type);
                }
            }
            else
            {
                eventListeners[type] = e => val.Invoke((T)e);
            }
        }

        public static Action? OnBotOnline { set => SetBotEvent<BotOnlineEvent>(value); }
        public static Action? OnBotOfflineActive { set => SetBotEvent<BotOfflineEventActive>(value); }
        public static Action? OnBotOfflineForce { set => SetBotEvent<BotOfflineEventForce>(value); }
        public static Action? OnBotOfflineDropped { set => SetBotEvent<BotOfflineEventDropped>(value); }
        public static Action? OnBotRelogin { set => SetBotEvent<BotReloginEvent>(value); }
        public static Action<BotGroupPermissionChangeEvent>? OnBotGroupPermissionChange { set => SetEvent(value); }
        public static Action<BotMuteEvent>? OnBotMute { set => SetEvent(value); }
        public static Action<BotUnmuteEvent>? OnBotUnmute { set => SetEvent(value); }
        public static Action<BotJoinGroupEvent>? OnBotJoinGroup { set => SetEvent(value); }
        public static Action<BotLeaveEventActive>? OnBotLeaveActive { set => SetEvent(value); }
        public static Action<BotLeaveEventKick>? OnBotLeaveKick { set => SetEvent(value); }

        public static Action<FriendInputStatusChangedEvent>? OnFriendInputStatusChanged { set => SetEvent(value); }
        public static Action<FriendNickChangedEvent>? OnFriendNickChanged { set => SetEvent(value); }

        public static Action<GroupRecallEvent>? OnGroupRecall { set => SetEvent(value); }
        public static Action<FriendRecallEvent>? OnFriendRecall { set => SetEvent(value); }

        public static Action<GroupNameChangeEvent>? OnGroupNameChange { set => SetEvent(value); }
        public static Action<GroupEntranceAnnouncementChangeEvent>? OnGroupEntranceAnnouncementChange { set => SetEvent(value); }
        public static Action<GroupMuteAllEvent>? OnGroupMuteAll { set => SetEvent(value); }
        public static Action<GroupAllowAnonymousChatEvent>? OnGroupAllowAnonymousChat { set => SetEvent(value); }
        public static Action<GroupAllowConfessTalkEvent>? OnGroupAllowConfessTalk { set => SetEvent(value); }
        public static Action<GroupAllowMemberInviteEvent>? OnGroupAllowMemberInvite { set => SetEvent(value); }
        public static Action<MemberJoinEvent>? OnMemberJoin { set => SetEvent(value); }
        public static Action<MemberLeaveEventKick>? OnMemberLeaveKick { set => SetEvent(value); }
        public static Action<MemberLeaveEventQuit>? OnMemberLeaveQuit { set => SetEvent(value); }
        public static Action<MemberCardChangeEvent>? OnMemberCardChange { set => SetEvent(value); }
        public static Action<MemberSpecialTitleChangeEvent>? OnMemberSpecialTitleChange { set => SetEvent(value); }
        public static Action<MemberPermissionChangeEvent>? OnMemberPermissionChange { set => SetEvent(value); }
        public static Action<MemberMuteEvent>? OnMemberMute { set => SetEvent(value); }
        public static Action<MemberUnmuteEvent>? OnMemberUnmute { set => SetEvent(value); }

        public static Action<NewFriendRequestEvent>? OnNewFriendRequest { set => SetEvent(value); }
        public static Action<MemberJoinRequestEvent>? OnMemberJoinRequest { set => SetEvent(value); }
        public static Action<BotInvitedJoinGroupRequestEvent>? OnBotInvitedJoinGroupRequest { set => SetEvent(value); }

        public static Action<NudgeEvent>? OnNudge { set => SetEvent(value); }

        public static Action<Exception>? OnException { internal get; set; }

        public static Action? OnDisconnect { private get; set; }

        #endregion

        #region === Essence API ===

        /// <summary>Set group essence message.</summary>
        /// <exception cref="MiraiException" />
        /// <exception cref="NotConnectedException" />
        /// <exception cref="WebException" />
        public static Task SetEssence(int messageId)
        {
            return BotCore.Connected
                ? MiraiAPI.SetEssence(BotCore.host!, BotCore.SessionKey!, messageId)
                : throw new NotConnectedException();
        }

        #endregion

        #region === File API ===

        /// <summary>Get the list of the group files.</summary>
        /// <exception cref="NotConnectedException" />
        /// <exception cref="WebException" />
        public static Task<FileSummary[]> GetGroupFileList(long groupId, string? dir = null)
        {
            return BotCore.Connected
                ? MiraiAPI.GetGroupFileList(BotCore.host!, BotCore.SessionKey!, groupId, dir)
                : throw new NotConnectedException();
        }

        /// <summary>Get the details of a specified file.</summary>
        /// <exception cref="NotConnectedException" />
        /// <exception cref="WebException" />
        public static Task<FileDetails> GetGroupFileInfo(long groupId, string fileId)
        {
            return BotCore.Connected
                ? MiraiAPI.GetGroupFileInfo(BotCore.host!, BotCore.SessionKey!, groupId, fileId)
                : throw new NotConnectedException();
        }

        /// <summary>Rename a file.</summary>
        /// <exception cref="MiraiException" />
        /// <exception cref="NotConnectedException" />
        /// <exception cref="WebException" />
        public static Task RenameGroupFile(long groupId, string fileId, string newName)
        {
            return BotCore.Connected
                ? MiraiAPI.RenameGroupFile(BotCore.host!, BotCore.SessionKey!, groupId, fileId, newName)
                : throw new NotConnectedException();
        }

        /// <summary>Make a group file directory.</summary>
        /// <exception cref="MiraiException" />
        /// <exception cref="NotConnectedException" />
        /// <exception cref="WebException" />
        public static Task MakeFileDirectory(long groupId, string directoryName)
        {
            return BotCore.Connected
                ? MiraiAPI.MakeFileDirectory(BotCore.host!, BotCore.SessionKey!, groupId, directoryName)
                : throw new NotConnectedException();
        }

        /// <summary>Move specified file to a new directory.</summary>
        /// <exception cref="MiraiException" />
        /// <exception cref="NotConnectedException" />
        /// <exception cref="WebException" />
        public static Task MoveGroupFile(long groupId, string fileId, string newPath)
        {
            return BotCore.Connected
                ? MiraiAPI.MoveGroupFile(BotCore.host!, BotCore.SessionKey!, groupId, fileId, newPath)
                : throw new NotConnectedException();
        }

        /// <summary>Delete a file.</summary>
        /// <exception cref="MiraiException" />
        /// <exception cref="NotConnectedException" />
        /// <exception cref="WebException" />
        public static Task DeleteGroupFile(long groupId, string fileId)
        {
            return BotCore.Connected
                ? MiraiAPI.DeleteGroupFile(BotCore.host!, BotCore.SessionKey!, groupId, fileId)
                : throw new NotConnectedException();
        }

        #endregion

        #region === Manage API ===

        /// <summary>Mute group member.</summary>
        /// <exception cref="MiraiException" />
        /// <exception cref="NotConnectedException" />
        /// <exception cref="WebException" />
        public static Task Mute(long groupId, long memberId, int seconds)
        {
            return BotCore.Connected
                ? MiraiAPI.Mute(BotCore.host!, BotCore.SessionKey!, groupId, memberId, seconds)
                : throw new NotConnectedException();
        }

        /// <summary>Unmute group member.</summary>
        /// <exception cref="MiraiException" />
        /// <exception cref="NotConnectedException" />
        /// <exception cref="WebException" />
        public static Task Unmute(long groupId, long memberId)
        {
            return BotCore.Connected
                ? MiraiAPI.Unmute(BotCore.host!, BotCore.SessionKey!, groupId, memberId)
                : throw new NotConnectedException();
        }

        /// <summary>Kick group member.</summary>
        /// <exception cref="MiraiException" />
        /// <exception cref="NotConnectedException" />
        /// <exception cref="WebException" />
        public static Task Kick(long groupId, long memberId, string? reason = null)
        {
            return BotCore.Connected
                ? MiraiAPI.Kick(BotCore.host!, BotCore.SessionKey!, groupId, memberId, reason)
                : throw new NotConnectedException();
        }

        /// <summary>Quit group.</summary>
        /// <exception cref="MiraiException" />
        /// <exception cref="NotConnectedException" />
        /// <exception cref="WebException" />
        public static Task Quit(long groupId)
        {
            return BotCore.Connected
                ? MiraiAPI.Quit(BotCore.host!, BotCore.SessionKey!, groupId)
                : throw new NotConnectedException();
        }

        /// <summary>Mute group.</summary>
        /// <exception cref="MiraiException" />
        /// <exception cref="NotConnectedException" />
        /// <exception cref="WebException" />
        public static Task MuteAll(long groupId)
        {
            return BotCore.Connected
                ? MiraiAPI.MuteAll(BotCore.host!, BotCore.SessionKey!, groupId)
                : throw new NotConnectedException();
        }

        /// <summary>Unmute group.</summary>
        /// <exception cref="MiraiException" />
        /// <exception cref="NotConnectedException" />
        /// <exception cref="WebException" />
        public static Task UnmuteAll(long groupId)
        {
            return BotCore.Connected
                ? MiraiAPI.UnmuteAll(BotCore.host!, BotCore.SessionKey!, groupId)
                : throw new NotConnectedException();
        }

        /// <summary>Get group configurations.</summary>
        /// <exception cref="MiraiException" />
        /// <exception cref="NotConnectedException" />
        /// <exception cref="WebException" />
        public static Task<GroupConfig> GetGroupConfig(long groupId)
        {
            return BotCore.Connected
                ? MiraiAPI.GetGroupConfig(BotCore.host!, BotCore.SessionKey!, groupId)
                : throw new NotConnectedException();
        }

        /// <summary>Set group configurations.</summary>
        /// <exception cref="MiraiException" />
        /// <exception cref="NotConnectedException" />
        /// <exception cref="WebException" />
        public static Task SetGroupConfig(long groupId, GroupConfig config)
        {
            return BotCore.Connected
                ? MiraiAPI.SetGroupConfig(BotCore.host!, BotCore.SessionKey!, groupId, config)
                : throw new NotConnectedException();
        }

        /// <summary>Get group member information.</summary>
        /// <exception cref="NotConnectedException" />
        /// <exception cref="WebException" />
        public static Task<(string nickInGroup, string nick, string specialTitle)?> GetMemberInfo(long groupId, long memberId)
        {
            return BotCore.Connected
                ? MiraiAPI.GetMemberInfo(BotCore.host!, BotCore.SessionKey!, groupId, memberId)
                : throw new NotConnectedException();
        }

        /// <summary>Set group member information.</summary>
        /// <exception cref="MiraiException" />
        /// <exception cref="NotConnectedException" />
        /// <exception cref="WebException" />
        public static Task SetMemberInfo(long groupId, long memberId, string? nickInGroup, string? specialTitle)
        {
            return BotCore.Connected
                ? MiraiAPI.SetMemberInfo(BotCore.host!, BotCore.SessionKey!, groupId, memberId, nickInGroup, specialTitle)
                : throw new NotConnectedException();
        }

        #endregion

        #region === Media API ===

        /// <summary>Upload image files to the server.</summary>
        /// <returns>IImageMessage</returns>
        /// <exception cref="MiraiException" />
        /// <exception cref="NotConnectedException" />
        /// <exception cref="WebException" />
        public static Task<IImageMessage> UploadImage(UploadType type, Stream imgStream)
        {
            return BotCore.Connected
                ? MiraiAPI.UploadImage(BotCore.host!, BotCore.SessionKey!, type, imgStream)
                : throw new NotConnectedException();
        }

        /// <summary>Upload image files to the server.</summary>
        /// <returns>IImageMessage</returns>
        /// <exception cref="MiraiException" />
        /// <exception cref="WebException" />
        public static async Task<IImageMessage> UploadImage(UploadType type, string path)
        {
            await using FileStream fs = new(path, FileMode.Open);
            return await UploadImage(type, fs);
        }

        /// <summary>Upload voice files to the server.</summary>
        /// <returns>IVoiceMessage</returns>
        /// <exception cref="MiraiException" />
        /// <exception cref="NotConnectedException" />
        /// <exception cref="WebException" />
        public static Task<IVoiceMessage> UploadVoice(Stream voiceStream)
        {
            return BotCore.Connected
                ? MiraiAPI.UploadVoice(BotCore.host!, BotCore.SessionKey!, voiceStream)
                : throw new NotConnectedException();
        }

        /// <summary>Upload voice files to the server.</summary>
        /// <returns>IVoiceMessage</returns>
        /// <exception cref="MiraiException" />
        /// <exception cref="WebException" />
        public static async Task<IVoiceMessage> UploadVoice(string path)
        {
            await using FileStream fs = new(path, FileMode.Open);
            return await UploadVoice(fs);
        }

        /// <summary>Upload files to group.</summary>
        /// <returns>FileId</returns>
        /// <exception cref="MiraiException" />
        /// <exception cref="NotConnectedException" />
        /// <exception cref="WebException" />
        public static Task<string> UploadFileAndSend(long groupId, string path, Stream fileStream)
        {
            return BotCore.Connected
                ? MiraiAPI.UploadFileAndSend(BotCore.host!, BotCore.SessionKey!, groupId, path, fileStream)
                : throw new NotConnectedException();
        }

        /// <summary>Upload files to group.</summary>
        /// <returns>FileId</returns>
        /// <exception cref="MiraiException" />
        /// <exception cref="WebException" />
        public static async Task<string?> UploadFileAndSend(long groupId, string targetPath, string filePath)
        {
            await using FileStream fs = new(filePath, FileMode.Open);
            return await UploadFileAndSend(groupId, targetPath, fs);
        }

        #endregion

        #region === Member API ===

        /// <summary>Get friend list.</summary>
        /// <returns>Friend List</returns>
        /// <exception cref="MiraiException" />
        /// <exception cref="NotConnectedException" />
        /// <exception cref="WebException" />
        public static Task<QFriendInfo[]> GetFriendList()
        {
            return BotCore.Connected
                ? MiraiAPI.GetFriendList(BotCore.host!, BotCore.SessionKey!)
                : throw new NotConnectedException();
        }

        /// <summary>Get group list.</summary>
        /// <returns>Group List</returns>
        /// <exception cref="MiraiException" />
        /// <exception cref="NotConnectedException" />
        /// <exception cref="WebException" />
        public static Task<QGroupInfo[]> GetGroupList()
        {
            return BotCore.Connected
                ? MiraiAPI.GetGroupList(BotCore.host!, BotCore.SessionKey!)
                : throw new NotConnectedException();
        }

        /// <summary>Get member list of the specified group.</summary>
        /// <returns>Group Member List</returns>
        /// <exception cref="MiraiException" />
        /// <exception cref="NotConnectedException" />
        /// <exception cref="WebException" />
        public static Task<QMemberInfo[]> GetMemberList(long groupId)
        {
            return BotCore.Connected
                ? MiraiAPI.GetMemberList(BotCore.host!, BotCore.SessionKey!, groupId)
                : throw new NotConnectedException();
        }

        #endregion

        #region === Message API ===

        /// <exception cref="MiraiException" />
        /// <exception cref="NotConnectedException" />
        /// <exception cref="WebException" />
        private static async Task<int> CommonSendMessage(long id, bool isGroup, IMessage[] chain, int? quote)
        {
            if (!BotCore.Connected)
            {
                throw new NotConnectedException();
            }

            //if (!MiddlewareCore.OnSend(ref id, ref isGroup, ref chain, ref quote))
            //{
            //    return 0;
            //}

            if (isGroup)
            {
                return await MiraiAPI.SendGroupMessage(BotCore.host!, BotCore.SessionKey!, id, quote, chain);
            }

            if (BotInfo.HasFriend(id))
            {
                return await MiraiAPI.SendFriendMessage(BotCore.host!, BotCore.SessionKey!, id, quote, chain);
            }

            long[] tempPath = BotInfo.GetTempPath(id);
            foreach (long t in tempPath)
            {
                int? msgid;
                try
                {
                    msgid = await MiraiAPI.SendTempMessage(BotCore.host!, BotCore.SessionKey!, t, id, quote, chain);
                    if (msgid is null)
                    {
                        continue;
                    }
                }
                catch { continue; }

                return msgid.Value;
            }

            throw new MiraiException(5);
        }

        /// <summary>Send message to private chat.</summary>
        /// <exception cref="MiraiException" />
        /// <exception cref="WebException" />
        public static Task<int> SendPrivateMessage(long qqId, string message)
        {
            return SendPrivateMessage(qqId, new PlainMessage(message));
        }

        /// <summary>Send message to private chat.</summary>
        /// <exception cref="MiraiException" />
        /// <exception cref="WebException" />
        public static Task<int> SendPrivateMessage(long qqId, params IMessage[] chain)
        {
            return CommonSendMessage(qqId, false, chain, null);
        }

        /// <summary>Send reply message to private chat.</summary>
        /// <exception cref="MiraiException" />
        /// <exception cref="WebException" />
        public static Task<int> SendPrivateMessage(int? quote, long qqId, string message)
        {
            return SendPrivateMessage(quote, qqId, new PlainMessage(message));
        }

        /// <summary>Send reply message to private chat.</summary>
        /// <exception cref="MiraiException" />
        /// <exception cref="WebException" />
        public static Task<int> SendPrivateMessage(int? quote, long qqId, params IMessage[] chain)
        {
            return CommonSendMessage(qqId, false, chain, quote);
        }

        /// <summary>Send message to group.</summary>
        /// <exception cref="MiraiException" />
        /// <exception cref="WebException" />
        public static Task<int> SendGroupMessage(long groupId, string message)
        {
            return SendGroupMessage(groupId, new PlainMessage(message));
        }

        /// <summary>Send reply message to group.</summary>
        /// <exception cref="MiraiException" />
        /// <exception cref="WebException" />
        public static Task<int> SendGroupMessage(int? quote, long groupId, string message)
        {
            return SendGroupMessage(quote, groupId, new PlainMessage(message));
        }

        /// <summary>Send message to friend.</summary>
        /// <returns>MessageID</returns>
        /// <exception cref="MiraiException" />
        /// <exception cref="NotConnectedException" />
        /// <exception cref="WebException" />
        public static Task<int> SendFriendMessage(long qqId, params IMessage[] chain)
        {
            return BotCore.Connected
                ? MiraiAPI.SendFriendMessage(BotCore.host!, BotCore.SessionKey!, qqId, chain)
                : throw new NotConnectedException();
        }

        /// <summary>Send reply message to friend.</summary>
        /// <returns>MessageID</returns>
        /// <exception cref="MiraiException" />
        /// <exception cref="NotConnectedException" />
        /// <exception cref="WebException" />
        public static Task<int> SendFriendMessage(int? quote, long qqId, params IMessage[] chain)
        {
            return BotCore.Connected
                ? MiraiAPI.SendFriendMessage(BotCore.host!, BotCore.SessionKey!, qqId, quote, chain)
                : throw new NotConnectedException();
        }

        /// <summary>Send message to group member.</summary>
        /// <returns>MessageID</returns>
        /// <exception cref="MiraiException" />
        /// <exception cref="NotConnectedException" />
        /// <exception cref="WebException" />
        public static Task<int> SendTempMessage(long groupId, long qqId, params IMessage[] chain)
        {
            return BotCore.Connected
                ? MiraiAPI.SendTempMessage(BotCore.host!, BotCore.SessionKey!, groupId, qqId, chain)
                : throw new NotConnectedException();
        }

        /// <summary>Send reply message to group member.</summary>
        /// <returns>MessageID</returns>
        /// <exception cref="MiraiException" />
        /// <exception cref="NotConnectedException" />
        /// <exception cref="WebException" />
        public static Task<int> SendTempMessage(int? quote, long groupId, long qqId, params IMessage[] chain)
        {
            return BotCore.Connected
                ? MiraiAPI.SendTempMessage(BotCore.host!, BotCore.SessionKey!, groupId, qqId, quote, chain)
                : throw new NotConnectedException();
        }

        /// <summary>Send message to group.</summary>
        /// <returns>MessageID</returns>
        /// <exception cref="MiraiException" />
        /// <exception cref="NotConnectedException" />
        /// <exception cref="WebException" />
        public static Task<int> SendGroupMessage(long groupId, params IMessage[] chain)
        {
            return BotCore.Connected
                ? MiraiAPI.SendGroupMessage(BotCore.host!, BotCore.SessionKey!, groupId, chain)
                : throw new NotConnectedException();
        }

        /// <summary>Send reply message to group.</summary>
        /// <returns>MessageID</returns>
        /// <exception cref="MiraiException" />
        /// <exception cref="NotConnectedException" />
        /// <exception cref="WebException" />
        public static Task<int> SendGroupMessage(int? quote, long groupId, params IMessage[] chain)
        {
            return BotCore.Connected
                ? MiraiAPI.SendGroupMessage(BotCore.host!, BotCore.SessionKey!, groupId, quote, chain)
                : throw new NotConnectedException();
        }

        /// <summary>Recall message.</summary>
        /// <exception cref="MiraiException" />
        /// <exception cref="NotConnectedException" />
        /// <exception cref="WebException" />
        public static Task Recall(int messageId)
        {
            return BotCore.Connected
                ? MiraiAPI.Recall(BotCore.host!, BotCore.SessionKey!, messageId)
                : throw new NotConnectedException();
        }

        #endregion

        #region === Nudge API ===

        /// <summary>Send nudge to private chat.</summary>
        /// <exception cref="MiraiException" />
        /// <exception cref="NotConnectedException" />
        /// <exception cref="WebException" />
        public static Task SendNudge(long qqId)
        {
            return BotCore.Connected
                ? MiraiAPI.SendNudge(BotCore.host!, BotCore.SessionKey!, qqId)
                : throw new NotConnectedException();
        }

        /// <summary>Send nudge to group.</summary>
        /// <exception cref="MiraiException" />
        /// <exception cref="NotConnectedException" />
        /// <exception cref="WebException" />
        public static Task SendNudge(long groupId, long qqId)
        {
            return BotCore.Connected
                ? MiraiAPI.SendNudge(BotCore.host!, BotCore.SessionKey!, groupId, qqId)
                : throw new NotConnectedException();
        }

        #endregion

        #region === Resp API ===

        /// <summary>Handle new friend request.</summary>
        /// <exception cref="MiraiException" />
        /// <exception cref="NotConnectedException" />
        /// <exception cref="WebException" />
        public static Task NewFriendRequestResp(long eventId, long fromId, long groupId, NewFriendRequestOperate operate, string message = "")
        {
            return BotCore.Connected
                ? MiraiAPI.NewFriendRequestResp(BotCore.host!, BotCore.SessionKey!, eventId, fromId, groupId, operate, message)
                : throw new NotConnectedException();
        }

        /// <summary>Handle new friend request.</summary>
        /// <exception cref="MiraiException" />
        /// <exception cref="NotConnectedException" />
        /// <exception cref="WebException" />
        public static Task NewFriendRequestResp(this NewFriendRequestEvent @event, NewFriendRequestOperate operate, string message = "")
        {
            return BotCore.Connected
                ? MiraiAPI.NewFriendRequestResp(BotCore.host!, BotCore.SessionKey!, @event.EventId, @event.FromId, @event.GroupId, operate, message)
                : throw new NotConnectedException();
        }

        /// <summary>Handle others join group request.</summary>
        /// <exception cref="MiraiException" />
        /// <exception cref="NotConnectedException" />
        /// <exception cref="WebException" />
        public static Task MemberJoinRequestResp(long eventId, long fromId, long groupId, MemberJoinRequestOperate operate, string message = "")
        {
            return BotCore.Connected
                ? MiraiAPI.MemberJoinRequestResp(BotCore.host!, BotCore.SessionKey!, eventId, fromId, groupId, operate, message)
                : throw new NotConnectedException();
        }

        /// <summary>Handle others join group request.</summary>
        /// <exception cref="MiraiException" />
        /// <exception cref="NotConnectedException" />
        /// <exception cref="WebException" />
        public static Task MemberJoinRequestResp(this MemberJoinRequestEvent @event, MemberJoinRequestOperate operate, string message = "")
        {
            return BotCore.Connected
                ? MiraiAPI.MemberJoinRequestResp(BotCore.host!, BotCore.SessionKey!, @event.EventId, @event.FromId, @event.GroupId, operate, message)
                : throw new NotConnectedException();
        }

        /// <summary>Handle group invitation request.</summary>
        /// <exception cref="MiraiException" />
        /// <exception cref="NotConnectedException" />
        /// <exception cref="WebException" />
        public static Task BotInvitedJoinGroupRequestResp(long eventId, long fromId, long groupId, BotInvitedJoinGroupRequestOperate operate, string message = "")
        {
            return BotCore.Connected
                ? MiraiAPI.BotInvitedJoinGroupRequestResp(BotCore.host!, BotCore.SessionKey!, eventId, fromId, groupId, operate, message)
                : throw new NotConnectedException();
        }

        /// <summary>Handle group invitation request.</summary>
        /// <exception cref="MiraiException" />
        /// <exception cref="NotConnectedException" />
        /// <exception cref="WebException" />
        public static Task BotInvitedJoinGroupRequestResp(this BotInvitedJoinGroupRequestEvent @event, BotInvitedJoinGroupRequestOperate operate, string message = "")
        {
            return BotCore.Connected
                ? MiraiAPI.BotInvitedJoinGroupRequestResp(BotCore.host!, BotCore.SessionKey!, @event.EventId, @event.FromId, @event.GroupId, operate, message)
                : throw new NotConnectedException();
        }

        #endregion
    }
}
