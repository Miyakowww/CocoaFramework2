# Meeting

Meeting 是对一段连续的消息处理过程的抽象，可用于实现对话功能。支持单人对话（如复杂功能的分步式引导）和多人会话（如多人游戏）场景。

<br>

## 示例
本示例实现最简单的加法计算分步式引导。为了方便理解，本代码不包含非法输入判断。

```C#
[TextRoute("加法")]
[TextRoute("add")]
public IEnumerator Proc(MessageSource src)
{
    MessageReceiver receiver = new();
    yield return receiver;

    src.Send("请输入第一个数");
    yield return null;
    int a = int.Parse(receiver.Message.PlainText);

    src.Send("请输入第二个数");
    yield return null;
    int b = int.Parse(receiver.Message.PlainText);

    // yield return "请输入第一个数";
    // int a = int.Parse(receiver.Message.PlainText);
    // 
    // yield return "请输入第二个数";
    // int b = int.Parse(receiver.Message.PlainText);

    src.Send($"结果是 {a + b}");
}

// <= 加法  
// => 请输入第一个数
// <= 1  
// => 请输入第二个数
// <= 2  
// => 结果是 3
```