# NotFit 类
命名空间：Maila.Cocoa.Framework.Models.Processing

<br>

表示接收到的消息不符合要求，消息应继续传递。
```C#
public class NotFit
```

<br>

## 属性
- Continue
    > 表示 Meeting 应继续运行
    > ```C#
    > public static NotFit Continue { get; }
    > ```
- Stop
    > 表示 Meeting 应停止运行
    > ```C#
    > public static NotFit Stop { get; }
    > ```