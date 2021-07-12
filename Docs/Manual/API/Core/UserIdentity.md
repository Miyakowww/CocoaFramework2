# UserIdentity 枚举
命名空间：Maila.Cocoa.Framework

<br>

标识用户身份。  
此枚举有一个 FlagsAttribute 特性，允许按位组合成员值。
```C#
[System.Flags]
public enum UserIdentity

```

## 字段
|           |       |                                     |
| -         | -     | -                                   |
| User      | 0     |                                     |
| Admin     | 1     |                                     |
| Owner     | 2     |                                     |
| Developer | 4     |                                     |
| Debugger  | 8     |                                     |
| Operator  | 16    |                                     |
| Staff     | 32    |                                     |
| Custom1   | 64    |                                     |
| Custom2   | 128   |                                     |
| Custom3   | 256   |                                     |
| Custom4   | 512   |                                     |
| Custom5   | 1024  |                                     |
| Custom6   | 2048  |                                     |
| Custom7   | 4096  |                                     |
| Custom8   | 8192  |                                     |
| Custom9   | 16384 |                                     |
| SU        | 63    | 超级用户，包含自定义身份以外的全部身份 |
