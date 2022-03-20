# Attributes

## DisabledAttribute
指示 Cocoa Framework 应忽视此内容。  
可指定于：`类`、`方法`、`字段`、`参数`

<br>

## BotModuleAttribute
指示当前类是一个 Module。  
可指定于：`类`

<br>

## IdentityRequirementsAttribute
指示执行当前 Module 或功能的身份要求。  
可指定于：`类`、`方法`  
可多次指定

<br>

## DisableInGroupAttribute
指示当前 Module 或功能不可用于群聊。  
可指定于：`类`、`方法`  

<br>

## DisableInPrivateAttribute
指示当前 Module 或功能不可用于私聊。  
可指定于：`类`、`方法`  

<br>

## ThreadSafeAttribute
指示当前方法是线程安全的，Cocoa Framework 会对这些方法进行异步调用。  
可指定于：`方法`  

<br>

## HostingAttribute
指示当前字段是托管数据字段。由于平台限制，无法托管静态只读字段。  
可指定于：`字段`

<br>

## TextRouteAttribute
指示当前方法可被文本路由。  
可指定于：`字段`  
可多次指定 

<br>

## RegexRouteAttribute
指示当前方法可被正则路由。  
可指定于：`字段`  
可多次指定

<br>

## GroupNameAttribute
指示当前参数将映射到指定名字的正则组。  
可指定于：`参数` 

<br>

## MemoryOnlyAttribute
指示当前 AutoData 参数为临时自动数据，仅存储于内存中  
可指定于：`参数` 

<br>

## SharedFromAttribute
指示当前 AutoData 将共享来自另一 Module 的数据