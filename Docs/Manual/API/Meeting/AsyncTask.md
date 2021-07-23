# AsyncTask 类
命名空间：Maila.Cocoa.Framework.Models.Processing

<br>

用于在 Meeting 中执行异步任务。请注意，异步任务执行过程中不会阻塞消息，因此在 Meeting 等待异步任务的过程中可能由用户误操作导致重复建立 Meeting，请自行避免。
```C#
public class AsyncTask
```

<br>

## 方法
- Wait
    > 暂停执行
    > ```C#
    > public static AsyncTask Wait(int milliseconds);
    > public static AsyncTask Wait(int milliseconds, CancellationToken cancellationToken);
    > public static AsyncTask Wait(TimeSpan delay);
    > public static AsyncTask Wait(TimeSpan delay, CancellationToken cancellationToken);
    > ```
    >
    > ### 参数
    > `milliseconds` int  
    > 暂停时长（毫秒）  
    > `delay` TimeSpan  
    > 暂停时长  
    > `cancellationToken` CancellationToken  
    > 用于取消执行的 Token
- WaitUntil
    > 在指定时间之前暂停执行
    > ```C#
    > public static AsyncTask WaitUntil(DateTime time);
    > public static AsyncTask WaitUntil(DateTime time, CancellationToken cancellationToken);
    > ```
    >
    > ### 参数
    > `time` DateTime  
    > 结束时间  
    > `cancellationToken` CancellationToken  
    > 用于取消执行的 Token
- Run
    > 执行指定的异步任务
    > ```C#
    > public static AsyncTask Run(Action action);
    > public static AsyncTask Run(Action action, CancellationToken cancellationToken);
    > public static AsyncTask Run(Func<Task?> function);
    > public static AsyncTask Run(Func<Task?> function, CancellationToken cancellationToken);
    > public static AsyncTask Run<T>(Func<T> function, out GetValue<T> result);
    > public static AsyncTask Run<T>(Func<Task<T>> func, out GetValue<T> result);
    > public static AsyncTask Run<T>(Func<Task<T>> func, out GetValue<T> result, CancellationToken cancellationToken);
    > ```
    >
    > ### 参数
    > `action` Action  
    > 要异步执行的同步任务  
    > `function` Func\<Task?>  
    > 要执行的异步任务  
    > `function` Func\<T>  
    > 要异步执行的同步函数  
    > `function` Func\<Task\<T>>  
    > 要执行的异步函数  
    > `result` out GetValue\<T>  
    > 用于获取函数返回值的对象  
    > `cancellationToken` CancellationToken  
    > 用于取消执行的 Token