using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class TruckMaster : MasterBase
{
    private static readonly ILogger log = Debug.unityLogger;
    
    [Serializable]
    public class Truck
    {
        public string tid;
        public string desc;
        public string scene;
    }

    public Dictionary<string, Truck> Trucks = new();

    public Truck Get(string tid)
    {
        if (Trucks.TryGetValue(tid, out var t))
        {
            return t;
        }
        return null;
    }

    public override void Load()
    {
        var json = MasterStorage.Load("Foundation/Truck");
        //log.Log(json);

        var list = JsonConvert.DeserializeObject<List<Truck>>(json);
        foreach (var t in list)
        {
            Trucks[t.tid] = t;
        }
    }
}