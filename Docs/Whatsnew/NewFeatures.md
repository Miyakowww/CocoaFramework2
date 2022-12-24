# Cocoa Framework 2 中的新特性

## 基于身份的权限管理系统
---
可以为用户指定身份，然后通过身份管理用户对功能的访问。详见 [权限管理](../Manual/Permission.md)

<br>

## 开放事件侦听接口
---
允许添加对消息事件（群聊消息、好友消息、临时消息）以外事件的处理

<br>

## 新增特性（Attribute）
---
- IdentityRequirementsAttribute  
    > 用于指定身份要求，可以重复添加。每个要求的判断标准为“全部”，即用户要拥有所要求的全部身份；要求间的关系为“任意”，即满足任意要求即可。详见 [权限管理](../Manual/Permission.md)  
    > 可用于类和方法，仅在 Module 类、路由方法和 OnMessage 方法中有效
- DisableInGroupAttribute 和 DisableInPrivateAttribute
    > 用于禁止指定的功能在群或私聊（包括好友消息和临时消息）环境下使用  
    > 可用于类和方法，仅在 Module 类、路由方法和 OnMessage 方法中有效
- GroupNameAttribute
    > 用于指定变量所映射的组。详见 [路由](../Manual/Route.md)  
    > 可用于参数，仅在正则路由方法的参数中有效

<br>

## 数据托管逻辑更改为同步
---
被托管的字段会对应于一个数据文件，之前在程序运行过程中对此数据文件的直接修改都会被覆盖，现在则会将手动修改的内容与字段的数据进行合并。但除了修改静态数据，一般不推荐手动修改数据文件。详见 [数据存储](../Manual/Data.md)

<br>

## 增强 Middleware 处理
---
```C#
// protected virtual bool OnMessage(ref MessageSource src, ref QMessage msg);
protected virtual void OnMessage(MessageSource src, QMessage msg, Action<MessageSource, QMessage> next);
```
这意味着 Middleware 不再受到 ref 的限制，可以使用 async/await 更为方便地进行异步处理

<br>

## AutoData
---
基于消息来源提供不同的数据，可以极大简化对简单数据的管理。详见 [AutoData](../Manual/AutoData.md)

<br>

## AsyncMeeting
用更加自然的方式实现对话。详见 [AsyncMeeting](../Manual/AsyncMeeting.md)
