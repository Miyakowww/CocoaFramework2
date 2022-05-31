// Copyright (c) Maila. All rights reserved.
// Licensed under the GNU AGPLv3

using System.Diagnostics.CodeAnalysis;

namespace Maila.Cocoa.Framework.Models.Processing
{
    public class MessageReceiver
    {
        public MessageSource? Source { get; internal set; }
        public QMessage? Message { get; internal set; }

        [MemberNotNullWhen(false, new[] { nameof(Source), nameof(Message) })]
        public bool IsTimeout { get; internal set; }
    }
}
