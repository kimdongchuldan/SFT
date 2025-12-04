using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

public class KitchenItemMaster : MasterBase
{
    private static readonly ILogger log = Debug.unityLogger;

    [Serializable]
    public class KitchenItem
    {
        public string tid;
        public string desc;
        public string prefab;
        public string pickup;
        public string putdown;
        public List<string> sequential_parts;
        public List<string> optional_parts;
        public float decay_time;
        public float measured_time;
        public int lv;
        
        [NonSerialized] 
        public KitchenItem pickup_item;

        [NonSerialized] 
        public KitchenItem putdown_item;

        [NonSerialized] 
        public List<KitchenItem> optional_part_items = new();
        
        [NonSerialized] 
        public List<KitchenItem> sequential_part_items = new();

        internal void Caching()
        {
            var m = MasterManager.Get<KitchenItemMaster>();

            if (string.IsNullOrEmpty(pickup))
            {
                pickup = tid; // 비어 있으면 자기 자신
            }

            {
                var k = m.Get(pickup);
                if (null == k)
                {
                    log.LogError("", $"pickup 키친아이템 찾기 실패 {tid} - {pickup}");
                }
                pickup_item = k;
            }

            if (string.IsNullOrEmpty(putdown))
            {
                putdown = tid; // 비어 있으면 자기 자신
            }

            {
                var k = m.Get(putdown);
                if (null == k)
                {
                    log.LogError("", $"putdown 키친아이템 찾기 실패 {tid} - {putdown}");
                }
                putdown_item = k;
            }

            if (sequential_parts != null)
            {
                foreach (var i in sequential_parts)
                {   
                    var k = m.Get(i);
                    if (null == k)
                    {
                        log.LogError("", $"키친아이템 찾기 실패 {tid} - {i}");
                        continue;
                    }
                    sequential_part_items.Add(k);
                }
            }

            if (optional_parts != null)
            {
                foreach (var i in optional_parts)
                {
                    var k = m.Get(i);
                    if (null == k)
                    {
                        log.LogError("", $"키친아이템 찾기 실패 {tid} - {i}");
                        continue;
                    }
                    optional_part_items.Add(k);
                }
            }
        }
    }

    

    // 전체 아이템
    public static Dictionary<string, KitchenItem> KitchenItems = new();

    // 🔥 미리 만들어두는 “조합 → 결과” 테이블
    // key: BuildKey(merge_with) 결과
    private static readonly Dictionary<string, KitchenItem> MergeTable = new();

    // -----------------------------------
    // GetMerged: 2개 버전 (기존 시그니처 유지)
    // -----------------------------------
    public KitchenItem GetMerged(KitchenItem a, KitchenItem b)
    {
        return GetMerged(new[] { a, b });
    }

    // -----------------------------------
    // GetMerged: N개(2,3,4...) 조합
    // -----------------------------------
    public KitchenItem GetMerged(params KitchenItem[] sourceItems)
    {
        if (sourceItems == null)
            return null;

        var tids = sourceItems
            .Where(i => i != null)
            .Select(i => i.tid);

        var key = BuildKey(tids);
        if (string.IsNullOrEmpty(key))
            return null;

        if (MergeTable.TryGetValue(key, out var result))
        {
            return result;
        }

        return null;
    }

    // 필요하면 string tid들로도 바로 쓸 수 있게
    public KitchenItem GetMergedByTid(params string[] tids)
    {
        var key = BuildKey(tids);
        if (string.IsNullOrEmpty(key))
            return null;

        if (MergeTable.TryGetValue(key, out var result))
            return result;

        return null;
    }

    // -----------------------------------
    // 키 생성 로직 (정렬 + Join)
    // 동일 multiset 이면 항상 같은 키가 나오도록
    // -----------------------------------
    private static string BuildKey(IEnumerable<string> tids)
    {
        if (tids == null)
            return null;

        var list = tids
            .Where(t => !string.IsNullOrEmpty(t))
            .ToList();

        if (list.Count == 0)
            return null;

        // 정렬해서 순서 무시, 중복은 그대로 유지
        list.Sort(StringComparer.Ordinal);
        return string.Join("|", list);
    }

    public KitchenItem Get(string tid)
    {
        if (KitchenItems.TryGetValue(tid, out var k))
            return k;
        return null;
    }

    public override void Load()
    {
        var json = MasterStorage.Load("Foundation/KitchenItem");
        //log.Log(json);

        var items = JsonConvert.DeserializeObject<List<KitchenItem>>(json);
        KitchenItems.Clear();
        MergeTable.Clear();

        foreach (var i in items)
        {
            if (string.IsNullOrEmpty(i.tid))
                continue;
            KitchenItems[i.tid] = i;
        }

        // 참조 캐싱
        foreach (var i in KitchenItems.Values)
        {
            i.Caching();
        }

        // 🔥 레시피 테이블 미리 생성
        foreach (var item in KitchenItems.Values)
        {
            if (null == item.sequential_parts)
                continue;

            List<string> parts = new List<string>(item.sequential_parts);
            //parts.AddRange(item.optional_parts);

            if (parts.Count == 0)
                continue;

            var key = BuildKey(parts);
            if (string.IsNullOrEmpty(key))
                continue;

            if (MergeTable.TryGetValue(key, out var exist))
            {
                // 같은 재료 조합으로 두 개의 결과가 있으면 디버그용 로그
                log.LogError("",
                    $"중복 머지 레시피 감지: key={key}, exist={exist.tid}, new={item.tid}");
                // 필요하면 여기서 덮어쓰거나, 첫 번째만 유지하거나, 빌드 실패 처리 등 선택
                continue;
            }

            MergeTable[key] = item;
        }

        log.Log("KitchenItemMaster", $"Loaded {KitchenItems.Count} items, {MergeTable.Count} merge recipes.");
    }
}
