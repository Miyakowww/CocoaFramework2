# ListeningTarget 类
命名空间：Maila.Cocoa.Framework.Models.Processing

<br>

用于更换监听目标。
```C#
public class ListeningTarget
```

<br>

## 属性
- All
    > 表示监听全部内容
    > ```C#
    > public static ListeningTarget All { get; }
    > ```

<br>

## 方法
- FromGroup
    > 设置监听目标为某个群
    > ```C#
    > public static ListeningTarget FromGroup(long groupId);
    > public static ListeningTarget FromGroup(QGroup group);
    > ```
    >
    > ### 参数
    > `groupId` long  
    > 要监听的群号  
    > `group` QGroup  
    > 要监听的群
- FromUser
    > 设置监听目标为某个用户
    > ```C#
    > public static ListeningTarget FromUser(long userId);
    > public static ListeningTarget FromUser(QUser user);
    > ```
    >
    > ### 参数
    > `userId` long  
    > 要监听的 QQ 号  
    > `user` QUser  
    > 要监听的用户
- FromTarget
    > 指定具体监听目标
    > ```C#
    > public static ListeningTarget FromTarget(long groupId, long userId);
    > public static ListeningTarget FromTarget(MessageSource src);
    > ```
    >
    > ### 参数
    > `groupId` long  
    > 要监听的群号  
    > `userId` long  
    > 要监听的 QQ 号  
    > `src` MessageSource  
    > 要监听的消息源
- CustomTarget
    > 自定义监听目标
    > ```C#
    > public static ListeningTarget CustomTarget(Predicate<MessageSource> pred);
    > ```
    >
    > ### 参数
    > `pred` Predicate\<MessageSource>  
    > 目标的判定规则  
