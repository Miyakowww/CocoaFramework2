# 你好，Cocoa！

通过本教程，您将学会如何通过 Cocoa Framework 实现最简单的应答机器人

<br>

## 在此之前
- [启动 Mirai](https://github.com/mamoe/mirai/blob/dev/docs/UserManual.md) 并 [安装 mirai-api-http](https://github.com/mamoe/mirai/blob/dev/docs/UserManual.md#%E5%A6%82%E4%BD%95%E5%AE%89%E8%A3%85%E5%AE%98%E6%96%B9%E6%8F%92%E4%BB%B6)，如果使用的是 mirai-api-http 2.x 版本需手动启用 WebSocket
- 安装 [Visual Studio](https://visualstudio.microsoft.com/zh-hans/) 或其他 IDE

<br>

## 新建项目
1. 启动 Visual Studio，创建新项目
2. 选择控制台应用程序，继续
3. 输入项目名，选择项目存放位置，继续
4. 目标框架选择 .NET 5.0（一般已默认选择），创建

<br>

## 添加引用
1. 在创建的项目上右键，点击“管理 Nuget 包”
2. 进入“浏览”选项卡，搜索 Maila.Cocoa.Framework 或 maila
3. 选中搜索结果中的 Maila.Cocoa.Framework，点击右侧界面中的“安装”

<br>

## 编写启动代码
将默认创建的 Program.cs 中的全部代码删除，替换为以下代码
```C#
using System;
using Maila.Cocoa.Framework;

BotStartupConfig config = new("YourVerifyKey", 12345678); // 启动配置，请将 YourVerifyKey 改为您的 VerifyKey，12345678 改为机器人的 QQ 号
var succeed = await BotStartup.Connect(config); // 连接 Mirai 并初始化
if (succeed) // 如果连接成功
{
    Console.WriteLine("Startup OK"); // 提示连接成功
    while (Console.ReadLine() != "exit"); // 在用户往控制台输入“exit”前持续运行
    await BotStartup.Disconnect(); // 断开连接
}
else // 否则
{
    Console.WriteLine("Failed"); // 提示连接失败
}
```

<br>

## 实现简单应答
新建 Hello.cs 文件，并输入以下代码
```C#
using Maila.Cocoa.Framework;

[BotModule]
public class Hello : BotModuleBase
{
    [TextRoute("hello cocoa")] // 收到“hello cocoa”时调用此方法
    public static void Run(MessageSource src)
    {
        src.Send("Hi!"); // 向消息来源发送“Hi!”
    }
}
```

<br>

## 完成
运行程序，进入 QQ 测试功能
