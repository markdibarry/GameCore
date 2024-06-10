using System;
using System.Collections.Generic;

namespace GameCore.Utility;

public interface IPoolable<T>
{
    public void Init(T? source);
}

public static class ObjectPool<T> where T : IPoolable<T>, new()
{
    private static readonly List<T> s_pool = [];

    public static T Get()
    {
        lock (s_pool)
        {
            if (s_pool.Count > 0)
            {
                T poolable = s_pool[0];
                s_pool.RemoveAt(0);
                return poolable;
            }
            else
            {
                return new();
            }
        }
    }

    public static void Return(T poolable)
    {
        lock (s_pool)
        {
            GC.ReRegisterForFinalize(poolable);
            poolable.Init(default);
            s_pool.Add(poolable);
        }
    }
}

public static class ListPool<T>
{
    private static readonly List<List<T>> s_pool = [];

    public static List<T> Get()
    {
        lock (s_pool)
        {
            if (s_pool.Count > 0)
            {
                List<T> pooledList = s_pool[0];
                s_pool.RemoveAt(0);
                return pooledList;
            }
            else
            {
                return [];
            }
        }
    }

    public static void Return(List<T> list)
    {
        lock (s_pool)
        {
            list.Clear();
            s_pool.Add(list);
        }
    }
}
