<h1 align="center">更新日志</h1>

# **2.1.3.3**
*released September 1, 2021*

## 变动
- 调整 ListeningTarget 的判定逻辑

<br>

# **2.1.3.2**
*released August 10, 2021*

## 修复
- 修复 Message Lock 执行完成后不会结束处理的问题

## 变动
- 异步路由和 Module 中的异步覆写方法会被认为是线程安全的，即使没有添加 ThreadSafe 特性

<br>

# **2.1.3.1**
*released August 1, 2021*

## 修复
- 修复 GroupAutoData 无法读取和保存的问题

<br>

# **2.1.3**
*released August 1, 2021*

## 请注意！
此版本对数据存储路径中标识符的计算方式进行了更改，以解决更新后数据丢失的问题。部分数据需要手动进行调整。

涉及到的内容：
- `data/ModuleData` 文件夹
- `data/MiddlewareData` 文件夹
- `data/ModuleData/*/UserAutoData.json` 文件
- `data/ModuleData/*/GroupAutoData.json` 文件
- `data/ModuleData/*/SourceAutoData.json` 文件

例外：
- 未使用 AutoData 功能的 Module 可以忽略以 AutoData.json 结尾的文件
- 未使用数据托管和 AutoData 功能的 Module 和 Middleware 可以忽略对应的数据文件夹

推荐的调整方式：
- 文件夹
    1. 备份并删除相关文件夹
    2. 更新 Cocoa Framework 并至少启动一次机器人程序
    3. 检查文件夹，会发现文件夹名的后缀相较之前出现变化
    4. 将旧文件夹中的文件移动到对应的新文件夹中
    5. 如果您使用过 2.1.2 版本或 2.1.2.1 版本，之前的 Middleware 数据会被错误地放置到 ModuleData 文件夹中。当前版本已经修复了这个问题，但您需要手动把它们恢复到 MiddlewareData 文件夹中。
- 文件
    1. 有些文件内容为空（仅包含一对大括号），这些文件您可以直接忽略
    2. 包含数据的文件需要您自行比较和修改，替换工具可以减轻一部分工作量

## 修复
- 修复 Middleware 的数据被放置到 ModuleData 文件夹中的问题

## 变动
- 发送消息相关方法的返回值和部分参数更改为不可为空

## 新特性
- ListeningTarget 支持自定义监听目标

<br>

# **2.1.2.1**
*released July 26, 2021*

## 修复
- 修复包含 string[] 和 List\<String> 类型参数的 RegexRoute 无法被调用的问题

<br>

# **2.1.2**
*released July 25, 2021*

## 新特性

- 新增 AutoData，用于简化对简单数据的管理

<br>

# **2.1.1**
*released July 24, 2021*

## 变动

- Meeting 中修改超时的方式由返回 TimeSpan 更改为 MeetingTimeout
- 优化 Route 在返回 void 时的执行逻辑

## 新特性

- 新增 AsyncTask，用于在 Meeting 中执行异步任务

<br>

# **2.1.0**
*released July 23, 2021*

## 修复

- 解决 Middleware 无法更改消息内容的一系列相关问题

## 变动

- QMessage 中的 Message 类全部更改为 IMessage 接口，涉及构造函数和成员：
    - QMessage(**IMessage**[] chain)
    - ImmutableArray<**IMessage**> Chain
    - T[] GetSubMessages\<T>() where T : **IMessage**

## 新特性

- 添加对 mirai-api-http 2.x 版本的支持


<br>

# **2.0.0**
*released July 14, 2021*

## 变动

- 更改依赖
    > Mirai-CSharp => CocoaBeans
- 调整命名空间规划
- 移除 Service 和 Component
- 移除 Module 的内置黑白名单，Module 的群聊可用性默认为可用
- Middleware 改由框架进行实例化
- Middleware 的 OnMessage() 方法由返回 bool 更改为调用 next() 方法
- 数据托管功能逻辑由自动保存更改为同步
- 异常更改为由用户处理

## 新特性

- 支持添加多个程序集
- 支持软件内重启
- 支持匿名 Module，简化 BotModule 的构造函数
- 新增基于身份的权限管理系统
- 增加新特性（Attribute）
- 允许正则路由的方法指定变量所映射的组
- 允许添加对消息事件（群聊消息、好友消息、临时消息）以外事件的处理