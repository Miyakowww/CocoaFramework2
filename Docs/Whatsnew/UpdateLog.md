<h1 align="center">更新日志</h1>

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