# 使用路由

通过本教程，您将学会如何使用路由

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
咕咕咕