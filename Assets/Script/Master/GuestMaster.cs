using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class GuestMaster : MasterBase
{    
    private static readonly ILogger log = Debug.unityLogger;
    
    [Serializable]
    public class Guest
    {
        public string tid;
        public string desc;
        public string prefab;
    }

    public Dictionary<string, Guest> Guests = new();

    public Guest Get(string tid)
    {
        if (Guests.TryGetValue(tid, out var g))
            return g;
        return null;
    }

    public override void Load()
    {
        var json = MasterStorage.Load("Foundation/Guest");
        //log.Log(json);
        var l = JsonConvert.DeserializeObject<List<Guest>>(json);
        foreach (var g in l)
        {
            Guests[g.tid] = g;
        }
    }
}