﻿// Copyright (c) Maila. All rights reserved.
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
        private static readonly ConcurrentDictionary<string, bool> saving = new();
        private static int savingCount;

        internal static bool SavingData => savingCount > 0;

        public static readonly string DataPath = AppDomain.CurrentDomain.BaseDirectory + "/data/";

        public static async void SaveData(string name, object? obj)
        {
            if (obj is null)
            {
                return;
            }
            if (saving.GetOrAdd(name, false))
            {
                needSave[name] = true;
            }
            else
            {
                saving[name] = true;
                Interlocked.Increment(ref savingCount);
                string path = $@"{DataPath}{name}.json";
                if (!Directory.Exists(Path.GetDirectoryName(path)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                }
                await File.WriteAllTextAsync(path, JsonConvert.SerializeObject(obj, Formatting.Indented));
                while (needSave.GetOrAdd(name, false))
                {
                    needSave[name] = false;
                    await File.WriteAllTextAsync(path, JsonConvert.SerializeObject(obj, Formatting.Indented));
                }

                saving[name] = false;
                Interlocked.Decrement(ref savingCount);
            }
        }

        public static async Task<T?> LoadData<T>(string name)
        {
            while (needSave.GetOrAdd(name, false) || saving.GetOrAdd(name, false))
            {
                await Task.Delay(10);
            }
            return File.Exists($@"{DataPath}{name}.json")
                ? JsonConvert.DeserializeObject<T>(await File.ReadAllTextAsync($@"{DataPath}{name}.json"))
                : default;
        }

        public static async Task<object?> LoadData(string name, Type type)
        {
            while (needSave.GetOrAdd(name, false) || saving.GetOrAdd(name, false))
            {
                await Task.Delay(10);
            }
            return File.Exists($@"{DataPath}{name}.json")
                ? JsonConvert.DeserializeObject(await File.ReadAllTextAsync($@"{DataPath}{name}.json"), type)
                : default;
        }

        private class HostingInfo
        {
            private readonly FieldInfo field;
            private readonly object? instance;
            private readonly string fileName;
            private readonly string filePath;
            private string last = string.Empty;
            private DateTime lastSave = DateTime.MinValue;

            public HostingInfo(FieldInfo field, object? instance, string fileName)
            {
                this.field = field;
                this.instance = instance;
                this.fileName = fileName;
                filePath = $"{DataPath}{fileName}.json";
            }

            private int _lock;

            public async Task Sync()
            {
                if (Interlocked.Exchange(ref _lock, 1) == 1)
                {
                    return;
                }
                string now = JsonConvert.SerializeObject(field.GetValue(instance));
                if (!File.Exists(filePath))
                {
                    SaveData(fileName, field.GetValue(instance));
                    last = now;
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
                else if (last != now)
                {
                    SaveData(fileName, field.GetValue(instance));
                    last = now;
                }

                lastSave = DateTime.UtcNow;
                _lock = 0;
            }
        }

        private static readonly List<HostingInfo> hostingInfos = new();
        private static CancellationTokenSource? _hosting;
        private static bool stopHosting;

        internal static void AddHosting(FieldInfo field, object? instance, string name)
        {
            hostingInfos.Add(new(field, instance, name));
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
                catch (TaskCanceledException) { }
            }

            stopHosting = false;
        }

        internal static async Task StopHosting()
        {
            if (_hosting is null)
            {
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