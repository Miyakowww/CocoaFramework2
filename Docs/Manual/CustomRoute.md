# 自定义路由

Cocoa Framework 支持自定义路由，以便于定制特殊的消息处理逻辑。

<br>

## 示例
本示例实现限定于特定群聊的路由。相较于普通的 TextRoute，添加了对群聊的判断。为便于理解，本示例不支持 TextRoute 的 IgnoreCase 与 AtRequired 功能，且仅支持设置一个群。

```C#
// 继承 RouteInfo 类，实现对消息的处理
public class GroupSpecifiedTextRoute : RouteInfo
{
    private readonly string text;
    private readonly long groupId;

    public GroupSpecifiedTextRoute(BotModuleBase module, MethodInfo route, string text, long groupId) : base(module, route)
    {
        this.text = text;
        this.groupId = groupId;
    }

    protected override bool IsMatch(MessageSource src, QMessage msg)
    {
        return src.Group?.Id == groupId && msg.PlainText == text;
    }
}

// 继承 RouteAttribute 类，实现对应的路由特性
public sealed class GroupSpecifiedTextRouteAttribute : RouteAttribute
{
    public string Text { get; }
    public long GroupId { get; }

    public GroupSpecifiedTextRouteAttribute(string text, long groupId)
    {
        Text = text;
        GroupId = groupId;
    }

    public override RouteInfo GetRouteInfo(BotModuleBase module, MethodInfo route)
    {
        return new GroupSpecifiedTextRoute(module, route, Text, GroupId);
    }
}

// 使用自定义路由
[BotModule]
public class Demo : BotModuleBase
{
    // 在群 123456 中收到“ping”时回复“pong”
    [GroupSpecifiedTextRoute("ping", 123456)]
    public static void Run(MessageSource src)
    {
        src.Send("pong");
    }
}
```