// Copyright (c) Maila. All rights reserved.
// Licensed under the GNU AGPLv3

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Maila.Cocoa.Framework.Support
{
    public static class DataManager
    {
        public static readonly string DataRoot = "data/";

        internal static bool SavingData => !savingStatus.IsEmpty;

        private static readonly ConcurrentDictionary<string, (bool valueUpdated, object? value)> savingStatus = new();
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> savingStatusLock = new();

        public static async void SaveData(string name, object? obj, bool indented = true)
        {
            var statusLock = savingStatusLock.GetOrAdd(name, _ => new(1));
            await statusLock.WaitAsync();

            if (savingStatus.TryGetValue(name, out var status))
            {
                savingStatus[name] = (true, obj);
                statusLock.Release();
                return;
            }
            else
            {
                savingStatus[name] = (false, null);
            }

            statusLock.Release();

            var path = $"{DataRoot}{name}.json";
            var directory = Path.GetDirectoryName(path);
            if (directory == null)
            {
                // Error: bad save path
                savingStatus.TryRemove(name, out _);
                return;
            }

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var formatting = indented ? Formatting.Indented : Formatting.None;
            await File.WriteAllTextAsync(path, JsonConvert.SerializeObject(obj, formatting));

            await statusLock.WaitAsync();
            while (savingStatus.TryGetValue(name, out status) && status.valueUpdated)
            {
                savingStatus[name] = (false, null);
                statusLock.Release();

                await File.WriteAllTextAsync(path, JsonConvert.SerializeObject(status.value, formatting));

                await statusLock.WaitAsync();
            }

            savingStatus.TryRemove(name, out _);
            statusLock.Release();
        }

        public static async Task<T?> LoadData<T>(string name)
        {
            while (savingStatus.ContainsKey(name))
            {
                await Task.Delay(10);
            }

            return File.Exists($"{DataRoot}{name}.json")
                ? JsonConvert.DeserializeObject<T>(await File.ReadAllTextAsync($"{DataRoot}{name}.json"))
                : default;
        }

        public static async Task<object?> LoadData(string name, Type type)
        {
            while (savingStatus.ContainsKey(name))
            {
                await Task.Delay(10);
            }

            return File.Exists($"{DataRoot}{name}.json")
                ? JsonConvert.DeserializeObject(await File.ReadAllTextAsync($"{DataRoot}{name}.json"), type)
                : null;
        }
    }
}
