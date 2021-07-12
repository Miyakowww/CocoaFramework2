# 复读机

## 重复全部消息
```C#
using Maila.Cocoa.Framework;

[BotModule("Repeater")]
public class Repeater : BotModuleBase
{
    protected override bool OnMessage(MessageSource src, QMessage msg)
    {
        src.Send(msg.PlainText);
        return true;
    }
}

// <= abc
// => abc
```

<br>

## 实现 echo 指令
```C#
using Maila.Cocoa.Framework;

[BotModule("Repeater")]
public class Repeater : BotModuleBase
{
    // 手动发送：
    [RegexRoute("^/echo (?<content>.+)")]
    public static void Echo(MessageSource src, string content)
    {
        src.Send(content);
    }

    // 自动发送：
    // [RegexRoute("^/echo (?<content>.+)")]
    // public static string Echo(string content)
    // {
    //     return content;
    // }
    
    // 简化版自动发送：
    // [RegexRoute("^/echo (?<content>.+)")]
    // public static string Echo(string content) => content;
}

// <= /echo abcd
// => abcd
```