// Copyright (c) Maila. All rights reserved.
// Licensed under the GNU AGPLv3

using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace Maila.Cocoa.Framework.Support
{
    public static class BotReg
    {
        private static ConcurrentDictionary<string, string> data = new();

        internal static void Init()
        {
            DataHosting.AddOptimizeEnabledHosting(
                typeof(BotReg).GetField("data", BindingFlags.Static | BindingFlags.NonPublic)!,
                null,
                "BotReg");
        }

        internal static void Reset()
        {
            data = new();
        }

        public static bool ContainsKey(string key)
        {
            return data.ContainsKey(key);
        }

        public static string[] GetKeys()
        {
            return data.Keys.ToArray();
        }

        public static string[] GetKeys(string path)
        {
            return data.Keys
                .Where(k => k.StartsWith(path))
                .Select(k => k[(path.EndsWith('/') ? path.Length : path.Length + 1)..])
                .ToArray();
        }

        public static string GetString(string key, string defaultVal = "")
        {
            return data.ContainsKey(key) ? data[key] : defaultVal;
        }

        public static void SetString(string key, string val)
        {
            data[key] = val;
        }

        public static int GetInt(string key, int defaultVal = 0)
        {
            if (data.ContainsKey(key))
            {
                return int.TryParse(data[key], out int val) ? val : defaultVal;
            }

            return defaultVal;
        }

        public static void SetInt(string key, int val)
        {
            data[key] = val.ToString();
        }

        public static long GetLong(string key, long defaultVal = 0)
        {
            if (data.ContainsKey(key))
            {
                return long.TryParse(data[key], out long val) ? val : defaultVal;
            }

            return defaultVal;
        }

        public static void SetLong(string key, long val)
        {
            data[key] = val.ToString();
        }

        public static float GetFloat(string key, float defaultVal = 0)
        {
            if (data.ContainsKey(key))
            {
                return float.TryParse(data[key], out float val) ? val : defaultVal;
            }

            return defaultVal;
        }

        public static void SetFloat(string key, float val)
        {
            data[key] = val.ToString();
        }

        public static double GetDouble(string key, double defaultVal = 0)
        {
            if (data.ContainsKey(key))
            {
                return double.TryParse(data[key], out double val) ? val : defaultVal;
            }

            return defaultVal;
        }

        public static void SetDouble(string key, double val)
        {
            data[key] = val.ToString();
        }

        public static bool GetBool(string key, bool defaultVal = false)
        {
            if (data.ContainsKey(key))
            {
                return bool.TryParse(data[key], out bool val) ? val : defaultVal;
            }

            return defaultVal;
        }

        public static void SetBool(string key, bool val)
        {
            data[key] = val.ToString();
        }

        public static bool Remove(string key)
        {
            return data.TryRemove(key, out _);
        }
    }
}
