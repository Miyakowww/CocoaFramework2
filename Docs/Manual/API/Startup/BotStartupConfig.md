# BotStartupConfig 类
命名空间：Maila.Cocoa.Framework

<br>

用于配置启动信息。
```C#
public class BotStartupConfig
```

<br>

## 构造函数
- BotStartupConfig(string, long, string)
    > 初始化 BotStartupConfig 类的新实例，默认端口为 80
    > ```C#
    > public BotStartupConfig(string authKey, long qqId, string host);
    > ```
    > 
    > ### 参数
    > `authKey` string  
    > 连接密钥  
    > `qqId` long  
    > 机器人的 QQ 号  
    > `host` string  
    > mirai-api-http 的地址
- BotStartupConfig(string, long, int)
    > 初始化 BotStartupConfig 类的新实例，默认 mirai-api-http 的地址为 127.0.0.1
    > ```C#
    > public BotStartupConfig(string authKey, long qqId, int port);
    > ```
    > 
    > ### 参数
    > `authKey` string  
    > 连接密钥  
    > `qqId` long  
    > 机器人的 QQ 号  
    > `port` int  
    > 端口
- BotStartupConfig(string, long, string, int)
    > 初始化 BotStartupConfig 类的新实例，默认 mirai-api-http 的地址为 127.0.0.1，端口为 8080
    > ```C#
    > public BotStartupConfig(string authKey, long qqId, string host = "127.0.0.1", int port = 8080);
    > ```
    > 
    > ### 参数
    > `authKey` string  
    > 连接密钥  
    > `qqId` long  
    > 机器人的 QQ 号  
    > `host` string  
    > mirai-api-http 的地址  
    > `port` int  
    > 端口

## 字段
- `host` string
    > mirai-api-http 的地址
- `port` int
    > 端口
- `authKey` string
    > 连接密钥
- `qqId` long
    > 机器人的 QQ 号
- `autoSave` TimeSpan
    > 数据托管的自动保存间隔
  
<br>

## 属性
- Assemblies
    > 包含 Module 的程序集列表
    > ```C#
    > public List<Assembly> Assemblies { get; }
    > ```

<br>

## 方法
- AddMiddleware()
    > 添加 Middleware
    > ```C#
    > public BotStartupConfig AddMiddleware<T>() where T : BotMiddlewareBase;
    > public BotStartupConfig AddMiddleware(Type type);
    > ```
    > 
    > ### 参数
    > `type` Type  
    > Middleware 类
    > 
    > ### 返回值
    > BotStartupConfig  
    > 当前 BotStartupConfig
- AddAssembly
    > 添加包含 Module 的程序集
    > ```C#
    > public BotStartupConfig AddAssembly(Assembly assem);
    > ```
    > 
    > ### 参数
    > `assem` Assembly  
    > 要添加的程序集
    > 
    > ### 返回值
    > BotStartupConfig  
    > 当前 BotStartupConfig