# 权限管理

Cocoa Framework 采了基于身份的权限管理机制。

<br>

## 用户身份
每个用户可同时拥有多个身份，在程序内使用 enum UserIdentity 记录。UserIdentity 带有 Flags 特性，意味着您可以使用按位或运算符（|）叠加多个身份。

<br>

## 身份授予和检验
BotAuth 提供了身份相关的功能，这些是较为常用的方法：
- public static UserIdentity GetIdentity(long qqId);  
    > 获取用户身份
- public static void SetIdentity(long qqId, UserIdentity identity);  
    > 设置用户身份
- public static UserIdentity AddIdentity(long qqId, UserIdentity identity);  
    > 追加用户身份
- public static UserIdentity RemoveIdentity(long qqId, UserIdentity identity);  
    > 移除用户身份

<br>

## 群成员身份
enum GroupPermission 记录用户的群成员身份，包括成员（MEMBER)、管理员（ADMINISTRATOR）和群主（OWNER）。可以从 MessageSource.Permission 获取。

<br>

## Cocoa Framework 内置的身份校验功能
可以在 Module 对应的类和消息处理方法前添加 IdentityRequirementsAttribute 特性以限制对相关功能的访问。
```C#
using Maila.Cocoa.Framework;
using Maila.Cocoa.Beans.Models;

// 仅 Owner 可使用 RunA 和 RunB
[BotModule]
[IdentityRequirements(UserIdentity.Owner)]
public class Demo1 : BotModuleBase
{
    [TextRoute("a")]
    public static void RunA()
    {
        // ...
    }
    [TextRoute("b")]
    public static void RunB()
    {
        // ...
    }
}

[BotModule]
public class Demo2 : BotModuleBase
{
    // 所有人均可使用
    [TextRoute("a")]
    public static void RunA()
    {
        // ...
    }

    // 仅 Owner 可使用
    [TextRoute("b")]
    [IdentityRequirements(UserIdentity.Owner)]
    public static void RunB()
    {
        // ...
    }

    // 仅 Owner 和 Admin 可使用
    [TextRoute("c")]
    [IdentityRequirements(UserIdentity.Owner)]
    [IdentityRequirements(UserIdentity.Admin)]
    public static void RunC()
    {
        // ...
    }

    // 仅同时为 Owner 和 Admin 的用户可使用
    [TextRoute("d")]
    [IdentityRequirements(UserIdentity.Owner | UserIdentity.Admin)]
    public static void RunD()
    {
        // ...
    }

    // 仅群管理员及以上（群主）可使用
    [TextRoute("e")]
    [IdentityRequirements(GroupPermission.ADMINISTRATOR)]
    public static void RunE()
    {
        // ...
    }
}
```