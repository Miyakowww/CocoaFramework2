# Module

Module 进行具体处理，一般情况下应各司其职，使运行结果不受 Module 运行顺序的影响。因此 Module 由 Cocoa Framework 自动搜索和添加。但 Cocoa Framework 也提供了 Module 间的优先级功能，以便应对特殊情况。  

<br>

## ModuleCore 类
命名空间：Maila.Cocoa.Framework.Core

<br>

管理 Module 的核心类
```C#
public static class ModuleCore
```

### 属性
- Modules
    > 当前所有被加载的 Modules
    > ```C#
    > public static ImmutableArray<BotModuleBase> Modules { get; }
    > ```

### 方法
- AddLock
    > 添加消息锁
    > 
    > ```C#
    > public static void AddLock(Func<MessageSource, QMessage, LockState> lockRun);
    > public static void AddLock(Func<MessageSource, QMessage, LockState> lockRun, Predicate<MessageSource> predicate);
    > public static void AddLock(Func<MessageSource, QMessage, LockState> lockRun, ListeningTarget target);
    > public static void AddLock(Func<MessageSource, QMessage, LockState> lockRun, MessageSource src);
    > public static void AddLock(Func<MessageSource, QMessage, LockState> lockRun, Predicate<MessageSource> predicate, TimeSpan timeout, Action? onTimeout = null);
    > public static void AddLock(Func<MessageSource, QMessage, LockState> lockRun, ListeningTarget target, TimeSpan timeout, Action? onTimeout = null);
    > public static void AddLock(Func<MessageSource, QMessage, LockState> lockRun, MessageSource src, TimeSpan timeout, Action? onTimeout = null);
    > ```
    > #### 参数
    > `lockRun` Func\<MessageSource, QMessage, LockState>  
    > 消息锁运行方法  
    > `predicate` Predicate\<MessageSource>  
    > 消息锁运行条件  
    > `target` ListeningTarget  
    > 消息锁监听目标，仅目标符合时消息锁才会被调用  
    > `src` MessageSource  
    > 消息锁监听源，仅消息源一致时消息锁才会被调用  
    > `timeout` TimeSpan  
    > 超时时间，消息锁被添加后经过指定时间仍未被调用称为超时，超时后消息锁会被自动移除
    > `onTimeout` Action  
    > 超时回调，超时后会被调用
    >
    > #### 示例
    > ```C#
    > [TextRoute("你好")]
    > public static void Hello(MessageSource src)
    > {
    >     src.Send("你好！你的名字是？");
    >     ModuleCore.AddLock((_src, _msg) =>
    >     {
    >         _src.Send($"你好，{_msg.PlainText}！");
    >         return LockState.Finished;
    >     }, src);
    > }
    > 
    > // <= 你好
    > // => 你好！你的名字是？
    > // <= Chino
    > // => 你好，Chino！
    > ```

<br>

## BotModuleBase 类
命名空间：Maila.Cocoa.Framework

<br>

Module 的基类，所有 Module 应派生自此类。
```C#
public abstract class BotModuleBase
```

### 属性
- Name
    > Module 名，匿名 Module 的本属性值为 null
    > ```C#
    > public string? Name { get; }
    > ```
- Priority
    > 优先顺序，数值越大越晚被执行
    > ```C#
    > public int Priority { get; }
    > ```
- EnableInGroup
    > 在群聊中是否可用
    > ```C#
    > public bool EnableInGroup { get; }
    > ```
- EnableInPrivate
    > 在私聊时是否可用，私聊包含好友消息和临时消息
    > ```C#
    > public bool EnableInPrivate { get; }
    > ```
- IsAnonymous
    > 是否为匿名 Module
    > ```C#
    > public bool IsAnonymous { get; }
    > ```
- Enabled
    > Module 是否被启用
    > ```C#
    > public bool Enabled { get; set; }
    > ```

### 方法
- Init
    > 初始化时被调用
    > ```C#
    > protected virtual void Init();
    > ```
- Destroy
    > 断开连接时被调用
    > ```C#
    > protected virtual void Destroy();
    > ```
- OnMessage
    > 收到消息时被调用
    > ```C#
    > protected virtual bool OnMessage(MessageSource src, QMessage msg);
    > ```
    > #### 参数
    > `src` MessageSource  
    > 消息来源  
    > `msg` QMessage  
    > 消息内容  
    > 
    > #### 返回值
    > bool  
    > 消息是否被处理
- OnMessageFinished
    > 消息处理完后被调用
    > ```C#
    > protected virtual void OnMessageFinished(MessageSource src, QMessage msg, MessageSource origSrc, QMessage origMsg, bool processed, BotModuleBase? processModule);
    > ```
    > #### 参数
    > `src` MessageSource  
    > 消息来源  
    > `msg` QMessage  
    > 消息内容  
    > `origSrc` MessageSource  
    > 被 Middleware 处理前的消息来源  
    > `origMsg` QMessage  
    > 被 Middleware 处理前的消息内容  
    > `processed` bool  
    > 消息是否被处理  
    > `processModule` BotModuleBase?  
    > 处理该消息的 Module，如果未被处理则为 null
