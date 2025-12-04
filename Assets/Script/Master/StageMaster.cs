using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AI;

public class StageMaster : MasterBase
{
    private static readonly ILogger log = Debug.unityLogger;
    public override IEnumerable<Type> Dependencies => new[] { typeof(TruckMaster), typeof(KitchenItemMaster), typeof(GuestMaster)};


    [Serializable]
    public class Order
    {
        [JsonProperty("m")]
        public string kitchen_item_tid;

        [JsonProperty("c")]
        public int count;
        public KitchenItemMaster.KitchenItem master;

        public float OrderPreparationTime
        {
            get
            {
                return master.measured_time * count;
            }
        }

        internal void Caching()
        {
            var m = MasterManager.Get<KitchenItemMaster>();

            if (count <= 0)
            {
                log.LogError("", $"주문 음식 수량이 0개 {kitchen_item_tid}");
                return;
            }

            master = m.Get(kitchen_item_tid);
            if (null == master)
            {
                log.LogError("", $"주문 음식 마스터가 없습니다 {kitchen_item_tid}");
                return;
            }

            if (master.measured_time <= 0)
            {
                log.LogError("", $"주문 음식의 기본 측정시간이 누락되어 있습니다 {master.tid} - {master.measured_time}");
            }
        }
    }

    [Serializable]
    public class Spawn
    {
        public string tid;
        public string guest_tid;
        public List<Order> orders;
        //[NonSerialized] public List<KitchenItemMaster.KitchenItem> order_items = new();
        public float patience_time;
        public int line;
        [NonSerialized] public GuestMaster.Guest guest;

        public float OrderPreparationTime
        {
            get
            {
                float sum = 0.0f;
                foreach (var o in orders)
                {
                    sum += o.OrderPreparationTime;
                }
                return sum;
            }
        }

        internal void Caching()
        {
            {
                var m = MasterManager.Get<GuestMaster>();
                guest = m.Get(guest_tid);
                if (null == guest)
                {
                    log.LogError("", $"스폰 초기화중 손님 마스터가 없습니다 {tid} - {guest_tid}");
                }
            }

            {
                foreach (var o in orders)
                {
                    o.Caching();
                }
            }
        }
    }

    public class Line
    {
        public List<Spawn> spawns = new();
    }
    
    [Serializable]
    public class Stage
    {
        public string tid;
        public int index;
        public string desc;
        public string prefab;
        public string truck_tid;
        [NonSerialized] public TruckMaster.Truck truck;
        [NonSerialized] public Dictionary<int, Line> lines = new();
        public string line_count;

        internal void Caching()
        {
            var json = MasterStorage.Load($"Spawn/{index}");
            if(null == json)
                return;

            var sps = JsonConvert.DeserializeObject<List<Spawn>>(json);
            foreach (var s in sps)
            {
                if (lines.TryGetValue(s.line, out var l))
                {
                    l.spawns.Add(s);
                }
                else
                {
                    var nl = new Line();
                    nl.spawns.Add(s);
                    lines[s.line] = nl;
                }
                
                s.Caching();
            }

            var m = MasterManager.Get<TruckMaster>();
            truck = m.Get(truck_tid);
            if (null == truck)
            {
                log.LogError("", $"스테이지에 트럭이 없습니다 {tid} - {truck_tid}");
            }
        }
    }

    static public Dictionary<string, Stage> Stages = new();
    static public Dictionary<int, Stage> StagesByIndex = new();

    public Stage GetByIndex(int idx)
    {
        if (StagesByIndex.TryGetValue(idx, out var s))
            return s;
        return null;
    }

    public Stage Get(string tid)
    {
        if (Stages.TryGetValue(tid, out var s))
            return s;
        return null;
    }

    public override void Load()
    {
        var json = MasterStorage.Load("Foundation/Stage");
        var list = JsonConvert.DeserializeObject<List<Stage>>(json);
        foreach (var s in list)
        {
            Stages[s.tid] = s;
            StagesByIndex[s.index] = s;
        }

        foreach (var s in Stages.Values)
        {
            s.Caching();
        }
    }
}