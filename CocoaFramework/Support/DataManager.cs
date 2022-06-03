// Copyright (c) Maila. All rights reserved.
// Licensed under the GNU AGPLv3

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Maila.Cocoa.Framework.Support
{
    public static class DataManager
    {
        private static readonly ConcurrentDictionary<string, bool> needSave = new();
        private static readonly ConcurrentDictionary<string, bool> savingStatus = new();
        private static int savingCount;

        internal static bool SavingData => savingCount > 0;

        public static readonly string DataRoot = AppDomain.CurrentDomain.BaseDirectory + "/data/";

        public static async void SaveData(string name, object? obj)
        {
            if (obj is null)
            {
                return;
            }
            
            if (savingStatus.Exchange(name, true, false))
            {
                return;
            }

            Interlocked.Increment(ref savingCount);
            
            string path = $"{DataRoot}{name}.json";
            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            }
            
            await File.WriteAllTextAsync(path, JsonConvert.SerializeObject(obj, Formatting.Indented));
            while (needSave.Exchange(name, false, false))
            {
                await File.WriteAllTextAsync(path, JsonConvert.SerializeObject(obj, Formatting.Indented));
            }

            savingStatus[name] = false;
            Interlocked.Decrement(ref savingCount);
        }

        public static async Task<T?> LoadData<T>(string name)
        {
            while (needSave.GetOrAdd(name, false) || savingStatus.GetOrAdd(name, false))
            {
                await Task.Delay(10);
            }
            
            return File.Exists($"{DataRoot}{name}.json")
                ? JsonConvert.DeserializeObject<T>(await File.ReadAllTextAsync($"{DataRoot}{name}.json"))
                : default;
        }

        public static async Task<object?> LoadData(string name, Type type)
        {
            while (needSave.GetOrAdd(name, false) || savingStatus.GetOrAdd(name, false))
            {
                await Task.Delay(10);
            }
            
            return File.Exists($"{DataRoot}{name}.json")
                ? JsonConvert.DeserializeObject(await File.ReadAllTextAsync($"{DataRoot}{name}.json"), type)
                : default;
        }

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
                
                filePath = $"{DataRoot}{fileName}.json";
                folderPath = Path.GetDirectoryName(filePath) ?? DataRoot;
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
                        SaveData(fileName, field.GetValue(instance));
                    }
                    
                    last = current;
                }
                else if (File.GetLastWriteTimeUtc(filePath) > lastSave)
                {
                    field.SetValue(instance, await LoadData(fileName, field.FieldType));
                    
                    try
                    {
                        last = JsonConvert.SerializeObject(field.GetValue(instance));
                    }
                    catch
                    {
                        SaveData(fileName, field.GetValue(instance));
                    }
                }
                else if (optim && current == "{}")
                {
                    Optimize();
                }
                else if (current != last)
                {
                    SaveData(fileName, field.GetValue(instance));
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
