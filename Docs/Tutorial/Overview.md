# Cocoa Framewrok 概述

本教程将介绍 Cocoa Framework 的一些基础概念。

<br>

## 消息的组成
一条消息由来源和内容组成。来源是指发送者的 QQ 号、是否为群聊、群号等信息，它们被封装在 MessageSource 类中。内容被封装在 QMessage 类中，通过消息链表示。由于来源和内容被分开存储，因此在某些时候可以只获取来源或者消息。

<br>

## 消息链
QQ 消息经常出现文字和表情、图片等非文本内容存在于同一气泡内的情况，因此使用 IMessage 数组表示消息的内容和顺序。例如一条文本+表情+文本的消息用消息链表示就是 { PlainMessage, FaceMessage, PlainMessage }。需要注意的是，mirai 原始消息链的第一个项是 SourceMessage，而 QMessage 会自动解析 SourceMessage 中包含的信息，并作为属性提供。因此 QMessage 中的消息链不包含 SourceMessage。

<br>

## Middleware
Cocoa Framework 接收到消息后，会先交由 Middleware 进行预处理。此时 Middleware 可以截断消息，也可以更改消息的发送者和内容。同时发送消息前也会交由 Middleware 进行预处理，此时 Middleware 可以取消发送，也可以更改发送的接收者和内容。

<br>

## Module
消息由 Middleware 处理完后会交由 Module 处理。Module 是机器人具体功能的载体，推荐使用路由进行处理。消息处理完后也会将消息、原始消息、处理情况再次交由 Module 进行后处理。

<br>

## 路由
路由可以自动匹配消息内容、进行简单的消息解析并调用相关处理方法，包含 TextRoute 和 RegexRoute 两种，区别在于前者为普通的文字匹配，后者为基于正则表达式的文本匹配。同时路由还提供自动参数匹配、自动数据等特色功能。