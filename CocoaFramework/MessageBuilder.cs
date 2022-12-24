// Copyright (c) Maila. All rights reserved.
// Licensed under the GNU AGPLv3

using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Maila.Cocoa.Beans.API;
using Maila.Cocoa.Beans.Models.Messages;
using Maila.Cocoa.Framework.Support;

namespace Maila.Cocoa.Framework
{
    public class MessageBuilder : IEnumerable<IMessage>
    {
        private readonly List<IMessage> chain = new();

        public MessageBuilder Add(IMessage message)
        {
            chain.Add(message);
            return this;
        }

        public MessageBuilder AddAll(IEnumerable<IMessage> messages)
        {
            chain.AddRange(messages);
            return this;
        }

        public MessageBuilder Insert(int index, IMessage message)
        {
            chain.Insert(index, message);
            return this;
        }

        public MessageBuilder RemoveAt(int index)
        {
            chain.RemoveAt(index);
            return this;
        }

        public MessageBuilder RemoveAll<T>() where T : IMessage
        {
            for (int i = chain.Count - 1; i >= 0; i--)
            {
                if (chain[i] is T)
                {
                    chain.RemoveAt(i);
                }
            }
            return this;
        }

        public MessageBuilder Clear()
        {
            chain.Clear();
            return this;
        }

        public IMessage[] ToMessageChain()
        {
            return chain.ToArray();
        }

        #region === Messages ===

        public MessageBuilder At(long target)
        {
            chain.Add(new AtMessage(target));
            return this;
        }

        public MessageBuilder AtAll()
        {
            chain.Add(AtAllMessage.Instance);
            return this;
        }

        public MessageBuilder Face(int id)
        {
            chain.Add(new FaceMessage(id));
            return this;
        }

        public MessageBuilder Plain(string text)
        {
            chain.Add(new PlainMessage(text));
            return this;
        }

        public MessageBuilder MiraiCode(string code)
        {
            chain.Add(new MiraiCodeMessage(code));
            return this;
        }


        public static Task<IImageMessage> Image(UploadType type, string path)
            => BotAPI.UploadImage(type, path);

        public static async Task<IFlashImageMessage> FlashImage(UploadType type, string path)
            => ((ImageMessage)await BotAPI.UploadImage(type, path)).ToFlashImage();

        public static Task<IVoiceMessage> Voice(string path)
            => BotAPI.UploadVoice(path);

        public static IXmlMessage Xml(string xml)
            => new XmlMessage(xml);

        public static IJsonMessage Json(string json)
            => new JsonMessage(json);

        public static IAppMessage App(string content)
            => new AppMessage(content);

        public static IPokeMessage Poke(PokeType type)
            => new PokeMessage(type.ToString());

        public static IDiceMessage Dice(int value)
            => new DiceMessage(value);

        public static IMusicShareMessage MusicShare(MusicShareKind kind, string title, string summary, string jumpUrl, string pictureUrl, string musicUrl, string brief)
            => new MusicShareMessage(kind.ToString(), title, summary, jumpUrl, pictureUrl, musicUrl, brief);

        #endregion

        IEnumerator<IMessage> IEnumerable<IMessage>.GetEnumerator()
        {
            return chain.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return chain.GetEnumerator();
        }
    }

    public enum PokeType
    {
        Poke,
        ShowLove,
        Like,
        Heartbroken,
        SixSixSix,
        FangDaZhao,
    }

    public enum MusicShareKind
    {
        NeteaseCloudMusic,
        QQMusic,
        MiguMusic,
        KugouMusic,
        KuwoMusic,
    }
}
