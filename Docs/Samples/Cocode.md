# Cocode

通过文本表达非文本内容，类似于 CQCode / MiraiCode，但比它们更简单

示例：  
`@123` => `[艾特123]`  
`Hello, @123!` => `Hello, [艾特123]!`  
`@+` => `[艾特全体成员]`  
`@@` => `@`  
`#i./img.png:` => `[图片，路径为"./img.png"]`  
`#f123` => `[表情，id为123]`  
`##` => `#`

## Middleware
```C#
using Maila.Cocoa.Beans.API;
using Maila.Cocoa.Beans.Models.Messages;
using Maila.Cocoa.Framework;
using Maila.Cocoa.Framework.Support;
using System.Collections.Generic;
using System.Text;

public class Cocode : BotMiddlewareBase
{
    protected override bool OnSendMessage(ref long id, ref bool isGroup, ref IMessage[] chain, ref int? quote)
    {
        if (chain.Length != 1 || chain[0] is not PlainMessage msg)
        {
            return true;
        }

        List<IMessage> newChain = new();
        StringBuilder sb = new();
        for (int i = 0; i < msg.Text.Length; i++)
        {
            switch (msg.Text[i])
            {
                case '@':
                    if (i + 1 == msg.Text.Length || msg.Text[i + 1] == '@')
                    {
                        sb.Append('@');
                        i++;
                        continue;
                    }
                    if (msg.Text[i + 1] == '+')
                    {
                        if (sb.Length > 0)
                        {
                            newChain.Add(new PlainMessage(sb.ToString()));
                            sb.Clear();
                        }
                        newChain.Add(AtAllMessage.Instance);
                        i++;
                        continue;
                    }
                    if (msg.Text[i + 1] is >= '0' and <= '9')
                    {
                        if (sb.Length > 0)
                        {
                            newChain.Add(new PlainMessage(sb.ToString()));
                            sb.Clear();
                        }
                        for (i++; i < msg.Text.Length && msg.Text[i] is >= '0' and <= '9'; i++)
                        {
                            sb.Append(msg.Text[i]);
                        }
                        newChain.Add(new AtMessage(long.Parse(sb.ToString())));
                        sb.Clear();
                        i--;
                    }
                    else
                    {
                        sb.Append('@');
                    }
                    break;
                case '#':
                    if (i + 1 == msg.Text.Length || msg.Text[i + 1] == '#')
                    {
                        sb.Append('#');
                        i++;
                        continue;
                    }
                    if (msg.Text[i + 1] == 'f')
                    {
                        if (sb.Length > 0)
                        {
                            newChain.Add(new PlainMessage(sb.ToString()));
                            sb.Clear();
                        }
                        for (i += 2; i < msg.Text.Length && msg.Text[i] is >= '0' and <= '9'; i++)
                        {
                            sb.Append(msg.Text[i]);
                        }
                        newChain.Add(new FaceMessage(int.Parse(sb.ToString())));
                        sb.Clear();
                        i--;
                        continue;
                    }
                    if (msg.Text[i + 1] == 'i')
                    {
                        if (sb.Length > 0)
                        {
                            newChain.Add(new PlainMessage(sb.ToString()));
                            sb.Clear();
                        }
                        for (i += 2; i < msg.Text.Length && msg.Text[i] != ':'; i++)
                        {
                            sb.Append(msg.Text[i]);
                        }
                        var image = BotAPI.UploadImage(isGroup ? UploadType.Group : 
                                                                (BotInfo.HasFriend(id) ? UploadType.Friend : UploadType.Temp),
                                                       sb.ToString()).Result;
                        newChain.Add(image);
                        sb.Clear();
                    }
                    else
                    {
                        sb.Append('#');
                    }
                    break;
                default:
                    sb.Append(msg.Text[i]);
                    break;
            }
        }

        if (sb.Length > 0)
        {
            newChain.Add(new PlainMessage(sb.ToString()));
        }
        chain = newChain.ToArray();
        return true;
    }
}

```

<br>

## 启动代码
```C#
/*BotStartupConfig*/ config.AddMiddleware<Cocode>();
```

<br>

## 逻辑代码
```C#
[TextRoute("hello cocoa")]
public static string Run(MessageSource src) 
    => $"Hello, @{src.User.Id}#f21";
```