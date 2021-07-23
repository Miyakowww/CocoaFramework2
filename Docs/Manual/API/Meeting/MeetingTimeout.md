# MeetingTimeout 类
命名空间：Maila.Cocoa.Framework.Models.Processing

<br>

用于设置超时时长。
```C#
public class MeetingTimeout
```

<br>

## 属性
- Off
    > 表示关闭超时
    > ```C#
    > public static MeetingTimeout Off { get; }
    > ```

<br>

## 方法
- FromTimeSpan
    > 根据给定的 TimeSpan 设置超时时长
    > ```C#
    > public static MeetingTimeout FromTimeSpan(TimeSpan time);
    > ```
    >
    > ### 参数
    > `time` TimeSpan  
    > 超时时长
- FromMinutes
    > 根据给定的分钟数设置超时时长
    > ```C#
    > public static MeetingTimeout FromMinutes(double minutes);
    > ```
    >
    > ### 参数
    > `minutes` double  
    > 超时时长（分钟）
- FromSeconds
    > 根据给定的秒钟数设置超时时长
    > ```C#
    > public static MeetingTimeout FromSeconds(double seconds);
    > ```
    >
    > ### 参数
    > `seconds` double  
    > 超时时长（秒钟）