# BotEventHandler

BotEventHandler 用于处理事件。

<br>

## 示例

### BotEventHandler
```C#
class BotEventHandler : BotEventHandlerBase
{
    // Bot 事件处理器
    // 方法仅允许包含一个参数，且参数类型对应 Maila.Cocoa.Beans.Models.Events 中的事件类型
    // 方法名没有限制，但建议与事件名相关
    // 可以通过添加 DisabledAttribute 来禁用事件处理器
    void OnMemberJoin(MemberJoinEvent evt)
    {
        BotAPI.SendGroupMessage(evt.Member.Group.Id, "欢迎加入群聊！");
    }

    // 其他事件处理器
    protected override void OnException(Exception e)
    {
        Console.WriteLine(e);
    }
}
```

### 启动代码
```C#
/*BotStartupConfig*/ config.AddEventHandler(new BotEventHandler());
```

### 实现效果
当有成员加入群聊时，OnMemberJoin 会被调用，发送欢迎信息。当发生异常时，OnException 会被调用，输出异常信息。