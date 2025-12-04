using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class MasterBase
{
    public virtual IEnumerable<System.Type> Dependencies => Array.Empty<System.Type>();
    public abstract void Load();
}

public static class MasterManager
{
    private static readonly ILogger log = Debug.unityLogger;
    static private readonly Dictionary<System.Type, MasterBase> masters = new();
    static private readonly HashSet<System.Type> initialized = new();
    static private readonly HashSet<Type> loading = new();

    static public T Get<T>() where T : MasterBase
    {
        var t = typeof(T);
        if (!masters.TryGetValue(t, out var master))
            throw new KeyNotFoundException($"Master '{t.Name}' not loaded.");
        return master as T ?? throw new InvalidCastException($"Master '{t.Name}' is not {t.Name}.");
    }

    static public void Initialize()
    {
        if (initialized.Count > 0)
            return;

        var arr = new MasterBase[]
        {
            new GuestMaster(),
            new KitchenItemMaster(),
            new TruckMaster(),
            new StageMaster(),
        };

        foreach (var master in arr)
            masters[master.GetType()] = master;

        foreach (var master in masters.Values)
            Load(master);
    }

    static private void Load(MasterBase master)
    {
        var type = master.GetType();

        if (initialized.Contains(type))
            return;
            
        if (!loading.Add(type))
            throw new InvalidOperationException($"마스터 로드중 순환 참조를 발견했습니다 {type.Name}");

        foreach (var depType in master.Dependencies)
        {
            if (!masters.TryGetValue(depType, out var dependency))
                throw new InvalidOperationException($"의존 대상 마스터가 없습니다 {type} -> {depType}");

            Load(dependency);
        }

        master.Load();
        initialized.Add(type);
        loading.Remove(type);
    }
}