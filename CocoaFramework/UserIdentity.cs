// Copyright (c) Maila. All rights reserved.
// Licensed under the GNU AGPLv3

using System;

namespace Maila.Cocoa.Framework
{
    [Flags]
    public enum UserIdentity
    {
        User
            = 0b0,
        Admin
            = 0b1,
        Owner
            = 0b10,
        Developer
            = 0b100,
        Debugger
            = 0b1000,
        Operator
            = 0b10000,
        Staff
            = 0b100000,
        Custom1
            = 0b1000000,
        Custom2
            = 0b10000000,
        Custom3
            = 0b100000000,
        Custom4
            = 0b1000000000,
        Custom5
            = 0b10000000000,
        Custom6
            = 0b100000000000,
        Custom7
            = 0b1000000000000,
        Custom8
            = 0b10000000000000,
        Custom9
            = 0b100000000000000,

        SU
            = 0b111111
    }
}
