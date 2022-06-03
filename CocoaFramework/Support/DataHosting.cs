// Copyright (c) Maila. All rights reserved.
// Licensed under the GNU AGPLv3

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Maila.Cocoa.Framework.Support
{
    public static class DataHosting
    {
        private class HostingInfo
        {
            private readonly FieldInfo field;
            private readonly object? instance;
            private readonly string fileName;
            private readonly string filePath;
            private readonly string folderPath;
            private readonly bool optim;
            private string last = string.Empty;
            private DateTime lastSave = DateTime.MinValue;

            public HostingInfo(FieldInfo field, object? instance, string fileName, bool optim)
            {
                this.field = field;
                this.instance = instance;
                this.fileName = fileName;
                this.optim = optim;

                filePath = $"{DataManager.DataRoot}{fileName}.json";
                folderPath = Path.GetDirectoryName(filePath) ?? DataManager.DataRoot;
            }

            private int _lock;

            public async Task Sync()
            {
                if (Interlocked.Exchange(ref _lock, 1) == 1)
                {
                    return;
                }

                string current = JsonConvert.SerializeObject(field.GetValue(instance));

                if (!File.Exists(filePath))
                {
                    if (optim && current == "{}")
                    {
                        Optimize();
                    }
                    else
                    {
                        DataManager.SaveData(fileName, field.GetValue(instance));
                    }

                    last = current;
                }
                else if (File.GetLastWriteTimeUtc(filePath) > lastSave)
                {
                    field.SetValue(instance, await DataManager.LoadData(fileName, field.FieldType));

                    try
                    {
                        last = JsonConvert.SerializeObject(field.GetValue(instance));
                    }
                    catch
                    {
                        DataManager.SaveData(fileName, field.GetValue(instance));
                    }
                }
                else if (optim && current == "{}")
                {
                    Optimize();
                }
                else if (current != last)
                {
                    DataManager.SaveData(fileName, field.GetValue(instance));
                    last = current;
                }

                lastSave = DateTime.UtcNow;
                _lock = 0;
            }

            private void Optimize()
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                if (Directory.Exists(folderPath)
                 && Directory.GetFiles(folderPath).Length == 0
                 && Directory.GetDirectories(folderPath).Length == 0)
                {
                    Directory.Delete(folderPath);
                }
            }
        }

        private static readonly List<HostingInfo> hostingInfos = new();
        private static CancellationTokenSource? _hosting;
        private static bool stopHosting;

        internal static void AddHosting(FieldInfo field, object? instance, string name)
        {
            hostingInfos.Add(new(field, instance, name, false));
        }

        internal static void AddOptimizeEnabledHosting(FieldInfo field, object? instance, string name)
        {
            hostingInfos.Add(new(field, instance, name, true));
        }

        internal static async Task SyncAll()
        {
            foreach (var h in hostingInfos)
            {
                await h.Sync();
            }
        }

        internal static async void StartHosting(TimeSpan delay)
        {
            if (delay.TotalSeconds < 1)
            {
                await SyncAll();
                return;
            }

            if (Interlocked.CompareExchange(ref _hosting, new(), null) is not null)
            {
                throw new("Duplicated Calling");
            }

            while (!stopHosting)
            {
                await SyncAll();
                try
                {
                    await Task.Delay(delay, _hosting.Token);
                }
                catch (TaskCanceledException)
                {
                    if (!stopHosting)
                    {
                        _hosting = new();
                    }
                }
            }

            stopHosting = false;
        }

        internal static async Task StopHosting()
        {
            if (_hosting is null)
            {
                await SyncAll();
                return;
            }
            stopHosting = true;
            _hosting.Cancel();
            _hosting = null;

            while (stopHosting)
            {
                await Task.Delay(10);
            }
            await SyncAll();
            hostingInfos.Clear();
        }
    }
}
