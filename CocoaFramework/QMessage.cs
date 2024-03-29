﻿// Copyright (c) Maila. All rights reserved.
// Licensed under the GNU AGPLv3

using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Maila.Cocoa.Beans.Models.Messages;
using Maila.Cocoa.Framework.Support;

namespace Maila.Cocoa.Framework
{
    public class QMessage
    {
        public ImmutableArray<IMessage> Chain { get; }
        public int Id { get; }
        public DateTime Time { get; }
        public string PlainText { get; }

        public QMessage(IMessage[] chain)
        {
            if (chain is null || chain.Length < 2 || chain[0] is not SourceMessage sm)
            {
                throw new ArgumentException("Invalid message chain.");
            }

            Id = sm.Id;
            Time = DateTimeOffset.FromUnixTimeSeconds(sm.Time).LocalDateTime;
            Chain = ImmutableArray.Create(chain, 1, chain.Length - 1);
            PlainText = string.Concat(chain.Select(m => (m as PlainMessage)?.Text));
        }

        public T[] GetSubMessages<T>() where T : IMessage
            => Chain.OfType<T>()
                    .ToArray();

        public override string ToString()
            => PlainText;

        [return: NotNullIfNotNull("msg")]
        public static implicit operator string?(QMessage? msg)
            => msg?.PlainText;

        public void Recall()
            => RecallAsync();

        public Task RecallAsync()
            => BotAPI.Recall(Id);

        public void SetEssence()
            => SetEssenceAsync();

        public Task SetEssenceAsync()
            => BotAPI.SetEssence(Id);
    }
}
