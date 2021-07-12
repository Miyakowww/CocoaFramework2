// Copyright (c) Maila. All rights reserved.
// Licensed under the GNU AGPLv3

namespace Maila.Cocoa.Framework.Models.Processing
{
    public class MessageReceiver
    {
        public MessageSource? Source;
        public QMessage? Message;
        public bool IsTimeout;
    }
}
