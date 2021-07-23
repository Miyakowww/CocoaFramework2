# Meeting

Meeting 是对一段连续的消息处理过程的抽象，可用于实现对话功能。

<br>

## 开始 Meeting
可以使用 Meeting.Start 开启一次 Meeting，也可以通过路由自动启动

<br>

## yield return
yield return 将作为 Meeting 向管理器传递状态的方式
- 返回值类型
    - MessageReceiver：设置接收消息的接收器，此次返回不会中断执行
    - ListeningTarget：设置消息的监听源，此次返回会中断执行，直到有来自监听目标的消息
    - MeetingTimeout：设置 Meeting 的超时时长，超时后枚举器将会收到一次超时消息。设为 MeetingTimeout.Off 以关闭超时。此次返回不会中断执行
    - NotFit：使管理器返回 LockState.Continue 或 LockState.ContinueAndRemove。详见 MessageLock
    - string 或 StringBuilder：此次返回会中断执行，并向来源发送返回的内容
    - AsyncTask：执行异步任务，此次返回会在异步任务完成后继续执行
    - IEnumerator 或 IEnumerable：将给定的枚举器作为子 Meeting，此枚举器的 Next 方法会被立即调用。借助 GetValue 可实现 Meeting 间通信
    - null：此次返回会中断执行，直到有来自监听目标的消息

<br>

## ProcessingModels
- [MessageReceiver](./MessageReceiver.md)
- [ListeningTarget](./ListeningTarget.md)
- [MeetingTimeout](./MeetingTimeout.md)
- [NotFit](./NotFit.md)
- [AsyncTask](./AsyncTask.md)
- [GetValue](./GetValue.md)