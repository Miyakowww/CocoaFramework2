# Middleware

Middleware 用于对消息进行进行预处理，执行顺序有严格的要求，因此由开发者手动添加，最终执行顺序与添加顺序一致。

<br>

## MiddlewareCore 类
命名空间：Maila.Cocoa.Framework.Core

<br>

管理 Middleware 的核心类
```C#
public static class MiddlewareCore
```

### 属性
- Middlewares
    > 当前所有被加载的 Middleware
    > ```C#
    > public static ImmutableArray<BotMiddlewareBase> Middlewares { get; }
    > ```

<br>

## BotMiddlewareBase 类
命名空间：Maila.Cocoa.Framework

<br>

Middleware 的基类，所有 Middleware 应派生自此类。
```C#
public abstract class BotMiddlewareBase
```

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
    > protected virtual void OnMessage(MessageSource src, QMessage msg, Action<MessageSource, QMessage> next)
    > ```
    > #### 参数
    > `src` MessageSource  
    > 消息来源  
    > `msg` QMessage  
    > 消息内容  
    > `next` Action\<MessageSource, QMessage>  
    > 下一个 Middleware 的 OnMessage 方法，如果允许消息继续传递需要调用此方法，如需更改消息来源或消息内容可以直接把新的内容作为 `next` 的参数。注意，`next` 在一次执行中最多允许调用一次，建议在调用 `next` 后直接使用 return 结束
- OnSendMessage
    > 发送消息时被调用
    > ```C#
    > protected virtual bool OnSendMessage(ref long id, ref bool isGroup, ref IMessage[] chain, ref int? quote);
    > ```
    > #### 参数
    > `id` ref long  
    > 发送目标  
    > `isGroup` ref bool  
    > 目标为群聊  
    > `chain` ref IMessage[]  
    > 要发送的消息链  
    > `quote` ref int?  
    > 要回复的消息 Id，不回复时为 null  
    > #### 返回值
    > bool  
    > 是否同意发送