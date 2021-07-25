# 路由

路由是一种方便消息分类的机制，通过给定路由条件，实现自动解析和自动调用，相比于手动解析更为便捷。

<br>

## 特性
特性用于标记入口，需添加于入口方法前
- TextRouteAttribute
    > 文本路由，如消息与提供的文本一致，则进行调用
- RegexRouteAttribute
    > 正则路由，如消息符合正则表达式，则进行调用，并自动将匹配到的组填充到同名参数中
- GroupNameAttribute
    > 用于指定正则路由填充时参数对应的组名

<br>

## 入口方法

- 参数
  - 参数可以任意填写，除下述情况的参数都将被传入默认值或 null
  - 第一个类型为 MessageSource 的参数将被传入消息的来源
  - 第一个类型为 QMessage 的参数将被传入消息的内容
  - 参数名为正则表达式中的组名且类型为 string 的参数将被传入该组匹配到的字符串，如果该组会进行多次匹配（如 (?\<name>abc)+）则会传入匹配到的最后一个字符串。在 TextRoute 中无效
  - 参数名为正则表达式中的组名且类型为 string[] 或 List\<string> 的参数将被传入该组匹配到的全部字符串。在 TextRoute 中无效
  - 类型为 UserAutoData、GroupAutoData、SourceAutoData 的参数将根据消息来源提供对应的数据。详见 [AutoData](./AutoData.md)

- 返回值
  - 入口方法的返回值可以是任意类型
  - 如果为 void 表示一旦被调用就代表消息被处理
  - 如果为 bool 类型表示消息是否被处理
  - 如果为 string 或 StringBuilder 类型且不为空将自动向来源发送对应文本，否则表示消息未被处理
  - 如果为 IEnumerator 或 IEnumerable 会被自动添加为 [Meeting](./Meeting.md)
  - 如果为其他值类型，返回结果不为默认值将代表消息被处理
  - 如果为其他引用类型，返回结果不为 null 将代表消息被处理

<br>

## 示例
```C#
using Maila.Cocoa.Framework;

[BotModule]
public class Demo : BotModuleBase
{
    // 收到“test1”时回复“ok”
    [TextRoute("test1")]
    public static void Run1(MessageSource src)
    {
        src.Send("ok");
    }

    // 收到“test2”时回复“ok”
    [TextRoute("test2")]
    public static string Run2()
    {
        return "ok";
    }

    // 收到“你好abc”时回复“我不叫abc”
    [RegexRoute("你好(?<name>.+)")]
    public static string Run3(string name)
    {
        return "我不叫" + name;
    }

    // 收到“你好abc”时回复“我不叫abc”的另一种实现方式
    [RegexRoute("你好(?<name>.+)")]
    public static string Run3([GroupName("name")] string wrongName)
    {
        return "我不叫" + wrongName;
    }
}

```