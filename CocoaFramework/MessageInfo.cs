// Copyright (c) Maila. All rights reserved.
// Licensed under the GNU AGPLv3

namespace Maila.Cocoa.Framework
{
    public class MessageInfo
    {
        public MessageSource Source { get; private set; }
        public QMessage Message { get; private set; }

        public MessageInfo(MessageSource source, QMessage message)
        {
            Source = source;
            Message = message;
        }
    }
}
