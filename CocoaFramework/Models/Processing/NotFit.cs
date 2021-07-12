// Copyright (c) Maila. All rights reserved.
// Licensed under the GNU AGPLv3

namespace Maila.Cocoa.Framework.Models.Processing
{
    public class NotFit
    {
        private NotFit() { }
        internal bool Remove { get; init; }

        public static NotFit Continue { get; } = new() { Remove = false };
        public static NotFit Stop { get; } = new() { Remove = true };
    }
}
