# MessageReceiver 类
命名空间：Maila.Cocoa.Framework.Models.Processing

<br>

消息接收器，用于在 Meeting 中接收用户发送的消息。
```C#
public class MessageReceiver
```

<br>

## 字段
- `Source` MessageSource?
    > 消息来源，超时情况下值为 null
- `Message` QMessage?
    > 消息内容，超时情况下值为 null
- `IsTimeout` bool
    > 是否为超时