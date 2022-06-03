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
    }
}
