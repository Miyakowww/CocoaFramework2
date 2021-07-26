# AutoData

AutoData 是一种自动数据托管方案，可以根据消息来源提供不同的数据。AutoData 包括三种类型，即 UserAutoData<>、GroupAutoData<>、SourceAutoData<>，分别以用户、群和消息来源（相当于 QQ 中的窗口）作为区分数据的依据。在路由方法的参数列表中添加这些类型的参数，Cocoa Framework 便会提供对应的实例，处理过程中可以对这些实例的 Value 属性进行自由读写。更改后的 Value 会被自动保存至硬盘，以保证程序重启后数据仍然存在。如果不需要保存至硬盘，可以为对应的参数添加 MemoryOnlyAttribute 特性。

<br>

## 示例
```C#
[RegexRoute("笔记 (?<notes>.+)")]
public static string TakeNotes(string notes, UserAutoData<string?> paper)
{
    paper.Value = notes;
    return "写好了！";
}

[TextRoute("查看笔记")]
public static string ViewNotes(UserAutoData<string?> paper)
{
    if(paper.Value is null)
    {
        return "您没有记下任何内容";
    }
    return paper.Value;
}

// user1 <= 查看笔记
// => 您没有记下任何内容
// user1 <= 笔记 测试
// => 写好了！
// user1 <= 查看笔记
// => 测试
// user2 <= 笔记 test
// => 写好了！
// user2 <= 查看笔记
// => test
// user1 <= 查看笔记
// => 测试
```

<br>

## 注解
- 作用域
    - AutoData 目前仅可用于路由方法
- AutoData 之间的数据互通性
    - 同一 Module 之内、参数名和参数类型一致、生命周期（是否保存至硬盘）一致的 AutoData 之间数据互通
- 性能
    - 本功能依赖于 DataManager 的数据托管功能，因此不推荐用于存储过于庞大的数据。不存储至硬盘的 AutoData 没有此限制