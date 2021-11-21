# 创建 Module

通过本教程，您将学会如何创建一个 Module

<br>

1. 创建类
```C#
public class Demo
{
}
```

<br>

2. 继承父类
```C#
using Maila.Cocoa.Framework;

public class Demo : BotModuleBase
{
}
```

<br>

3. 添加特性
```C#
using Maila.Cocoa.Framework;

[BotModule]
public class Demo : BotModuleBase
{
}
```

<br>

至此，一个没有功能但会被加载的 Module 创建完成。