// Copyright (c) Maila. All rights reserved.
// Licensed under the GNU AGPLv3

using System;
using System.Collections.Concurrent;
using Newtonsoft.Json.Linq;

namespace Maila.Cocoa.Framework
{
    public class UserAutoData<T>
    {
        private readonly Func<T?> getter;
        private readonly Action<T?> setter;

        public T? Value { get => getter(); set => setter(value); }

        internal UserAutoData(ConcurrentDictionary<long, ConcurrentDictionary<string, object?>> data, long key1, string key2)
        {
            var userData = data[key1];
            getter = () =>
            {
                object? val = userData[key2];
                if (val is T t)
                {
                    return t;
                }
                if (val is JObject jobj)
                {
                    T? newVal = jobj.ToObject<T>();
                    userData[key2] = newVal;
                    return newVal;
                }
                return default;
            };
            setter = val => userData[key2] = val;
        }
    }

    public class GroupAutoData<T>
    {
        private readonly Func<T?> getter;
        private readonly Action<T?> setter;

        public T? Value { get => getter(); set => setter(value); }

        internal GroupAutoData(ConcurrentDictionary<long, ConcurrentDictionary<string, object?>> data, long key1, string key2)
        {
            var groupData = data[key1];
            getter = () =>
            {
                object? val = groupData[key2];
                if (val is T t)
                {
                    return t;
                }
                if (val is JObject jobj)
                {
                    T? newVal = jobj.ToObject<T>();
                    groupData[key2] = newVal;
                    return newVal;
                }
                return default;
            };
            setter = val => groupData[key2] = val;
        }
    }

    public class SourceAutoData<T>
    {
        private readonly Func<T?> getter;
        private readonly Action<T?> setter;

        public T? Value { get => getter(); set => setter(value); }

        internal SourceAutoData(ConcurrentDictionary<(long?, long), ConcurrentDictionary<string, object?>> data, (long?, long) key1, string key2)
        {
            var sourceData = data[key1];
            getter = () =>
            {
                object? val = sourceData[key2];
                if (val is T t)
                {
                    return t;
                }
                if (val is JObject jobj)
                {
                    T? newVal = jobj.ToObject<T>();
                    sourceData[key2] = newVal;
                    return newVal;
                }
                return default;
            };
            setter = val => sourceData[key2] = val;
        }
    }
}
