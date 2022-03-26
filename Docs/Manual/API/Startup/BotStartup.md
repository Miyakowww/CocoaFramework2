# BotStartup 类
命名空间：Maila.Cocoa.Framework

<br>

提供运行状态管理的相关方法。
```C#
public static class BotStartup
```

<br>

## 属性
- Connected
    > 表示是否已连接到 Mirai
    > ```C#
    > public static bool Connected { get; }
    > ```
  
<br>

## 方法
- ConnectAndInit
    > 连接 Mirai 并初始化
    > ```C#
    > public static Task<bool> ConnectAndInit(BotStartupConfig config);
    > ```
    > 
    > ### 参数
    > `config` [BotStartupConfig](./BotStartupConfig.md)  
    > 启动信息
    > 
    > ### 返回值
    > bool
    > 表示是否初始化成功
- DisconnectAndSaveData
    > 断开连接并保存数据
    > ```C#
    > public static Task DisconnectAndSaveData();
    > ```
- Reconnect
    > 重新连接并重新加载模块
    > ```C#
    > public static Task Reconnect();
    > ```