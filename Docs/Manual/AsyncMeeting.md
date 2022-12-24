# AsyncMeeting

AsyncMeeting 是 [Meeting](./Meeting.md) 的异步实现形式。

<br>

## 示例
本示例实现最简单的加法计算分步式引导。为了方便理解，本代码不包含非法输入判断。

```C#
[TextRoute("加法")]
[TextRoute("add")]
public static async Task<string> Proc(AsyncMeeting am)
{
    am.Send("请输入第一个数")
    var result = await am.Wait();
    int a = int.Parse(result!.Message); // 未设置超时，result 不为空

    am.Send("请输入第二个数");
    result = await am.Wait();
    int b = int.Parse(result!.Message);

    // var result = await am.SendAndWait("请输入第一个数");
    // int a = int.Parse(result!.Message);
    // 
    // result = await am.SendAndWait("请输入第二个数");
    // int b = int.Parse(result!.Message);

    return $"结果是 {a + b}";
}

// <= 加法  
// => 请输入第一个数
// <= 1
// => 请输入第二个数
// <= 2
// => 结果是 3
```