# 黑名单

## Middleware
```C#
using System;
using System.Collections.Generic;
using Maila.Cocoa.Framework;

public class Blacklist : BotMiddlewareBase
{
    [Hosting]
    public static List<long> blacklist = new();

    protected override void OnMessage(MessageSource src, QMessage msg, Action<MessageSource, QMessage> next)
    {
        if (!blacklist.Contains(src.User.Id))
        {
            next(src, msg);
        }
    }
}
```

<br>

## 启动代码
```C#
/*BotStartupConfig*/ config.AddMiddleware<Blacklist>();
```

<br>

## 将 12345678 添加到黑名单
```C#
Blacklist.blacklist.Add(12345678);
```