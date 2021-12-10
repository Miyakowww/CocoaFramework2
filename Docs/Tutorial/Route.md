# 使用路由

路由可以自动匹配接收到的消息并调用您指定的方法。通过本教程，您将学会如何使用路由

<br>

## 创建最基础的路由
1. 路由仅在 Module 中可用，因此你需要先创建一个 Module
```C#
using Maila.Cocoa.Framework;

[BotModule]
public class Demo : BotModuleBase
{
}
```

<br>

2. 一个路由包含特性和入口方法
```C#
using Maila.Cocoa.Framework;

[BotModule]
public class Demo : BotModuleBase
{
    [TextRoute("hello")] // 特性
    public void Hello() // 入口方法
    {
    }
}
```
TextRoute 是文本路由。在本例中，如果机器人收到 "hello" 将会调用 Hello() 方法。同时还有 RegexRoute，当机器人收到符合提供的正则表达式的消息时将会调用相应的入口方法。

<br>

3. 参数列表
```C#
using Maila.Cocoa.Framework;

[BotModule]
public class Demo : BotModuleBase
{
    [TextRoute("hello")]
    public void Hello(MessageSource src) // 参数列表
    {
    }
}
```
您可以按照自己的喜好添加参数。参数的顺序可以随意更换，Cocoa Framework 会自动按照参数的类型和名字传入您需要的内容。
MessageSource 包含消息来源，您可以借助它轻松地回复消息。QMessage 包含消息的具体内容。对于其他支持的类型，请参考 [路由](../Manual/Route.md#入口方法)。

<br>

4. 可选的返回值
```C#
using Maila.Cocoa.Framework;

[BotModule]
public class Demo : BotModuleBase
{
    // 手动发送消息
    // [TextRoute("hello")]
    // public void Hello(MessageSource src)
    // {
    //     src.Send("Hello!");
    // }

    // 通过返回值发送消息
    [TextRoute("hello")]
    public string Hello(MessageSource src)
    {
        return "Hello!";
    }
}
```
您可以通过返回值来发送消息。如果您不想发送消息，可以返回 null。当然，Cocoa Framework 也支持其他的返回值类型，请参考 [路由](../Manual/Route.md#入口方法)。

<br>

5. RegexRoute 的参数自动匹配
```C#
using Maila.Cocoa.Framework;

[BotModule]
public class Demo : BotModuleBase
{
    [DisableInGroup]
    [RegexRoute("^hello (?<name>[a-zA-Z]+)$")]
    public string Hello(MessageSource src, string name)
    {
        if (name == "cocoa")
        {
            return "Hello!";
        }
        else
        {
            return $"My name is cocoa, not {name}!";
        }
    }
}
```
RegexRoute 可以根据正则表达式中的组名将对应的内容传入同名参数。在此处的例子中，如果消息内容为 "hello cocoa"，则会将 "cocoa" 传入 name 参数，此时机器人将回复 "Hello!"。但如果消息内容为 "hello coco"，机器人将会生气地回复 "My name is cocoa, not coco!"。  
你应该已经注意到了，这里我额外添加了一个 DisableInGroup 特性，它将禁止此功能在群聊中使用，以防被意外调用。当然，类似的也有 DisableInPrivate 特性禁止在私聊中使用、Disabled 特性禁止在一切时候使用。Disabled 特性同时还能用于模块、重写方法、托管字段和一些属性。