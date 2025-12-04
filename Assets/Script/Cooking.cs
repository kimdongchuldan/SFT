using System.Collections.Generic;
using Foundation;
using UnityEngine;

public class Cooking
{
    public class Ingredient
    {
        public KitchenItemMaster.KitchenItem Master { get; }
        public bool IsFilled;
        public bool IsCandidate;

        public Ingredient(KitchenItemMaster.KitchenItem master)
        {
            Master = master;
            IsFilled = false;
            IsCandidate = false;
        }
    }

    public KitchenItemMaster.KitchenItem Master { get; }
    public List<Ingredient> SequentialComponents = new();
    public Dictionary<string, Ingredient> OptionalComponents = new();

    public DeltaElapsedTimer timer = new();

    public float Quality
    {
        get
        {
            // pick-up 시점의 아이템
            var i = Master.pickup_item.putdown_item;

            // decay_time == 0 -> 무제한 신선
            if (i.decay_time == 0)
                return 1f;

            // 경과 시간
            float s = timer.ElapsedSeconds;

            // 1 -> 0 선형 감소
            float q = 1f - (s / i.decay_time);

            // 0~1 범위로 Clamp
            return Mathf.Clamp01(q);
        }
    }

    public bool Paused { get; set; }

    public void Update()
    {
        if (false == Paused)
        {
            timer.Past(Time.deltaTime);
        }
    }

    public Ingredient Get(string tid)
    {
        foreach (var i in SequentialComponents)
        {
            if (i.Master.tid == tid)
                return i;
        }

        if (OptionalComponents.TryGetValue(tid, out var o))
            return o;

        return null;
    }

    public bool Preparing
    {
        get
        {
            foreach (var s in SequentialComponents)
            {
                if (s.IsFilled)
                    return true;
            }
            return false;
        }
    }

    public bool Prepared
    {
        get
        {
            foreach (var s in SequentialComponents)
            {
                if (false == s.IsFilled)
                    return false;
            }
            return true;
        }
    }

    /*public void Pop()
    {
        foreach (var s in SequentialComponents)
        {
            s.IsFilled = false;
        }

        foreach (var s in OptionalComponents.Values)
        {
            s.IsFilled = false;
        }

        timer = new();
    }*/

    public Cooking(KitchenItemMaster.KitchenItem m)
    {
        Master = m;

        // 순차 파츠 초기화
        if (m.sequential_part_items != null)
        {
            foreach (var item in m.sequential_part_items)
            {
                if (item == null) 
                    continue;
                SequentialComponents.Add(new Ingredient(item));
            }
        }

        // 옵션 파츠 초기화 (tid 기준으로 딕셔너리 구성)
        if (m.optional_part_items != null)
        {
            foreach (var item in m.optional_part_items)
            {
                if (item == null) continue;
                if (string.IsNullOrEmpty(item.tid)) continue;

                // 같은 tid가 여러 번 들어와도 한 번만 등록
                if (OptionalComponents.ContainsKey(item.tid)) continue;

                OptionalComponents.Add(item.tid, new Ingredient(item));
            }
        }
    }

    public bool Drop(KitchenItemMaster.KitchenItem m)
    {
        if (m == null) 
            return false;

        if (Prepared)
            return false;

        bool changed = false;

        // 1) Sequential: 아직 안 찬 "다음" 슬롯 하나만 검사
        for (int i = 0; i < SequentialComponents.Count; i++)
        {
            var comp = SequentialComponents[i];
            if (comp.IsFilled) continue;

            // 첫 번째로 비어있는 순차 슬롯이 m을 받을 수 있는지 확인
            if (IsSameItem(comp.Master, m))
            {
                comp.IsFilled = true;
                changed = true;
            }

            // 순차 슬롯은 오직 "다음" 것만 의미 있으니 바로 break
            break;
        }

        // 2) Optional: 아직 아무 변화 없으면 옵션 슬롯도 시도
        if (!changed && !string.IsNullOrEmpty(m.tid))
        {
            if (OptionalComponents.TryGetValue(m.tid, out var optComp) && !optComp.IsFilled)
            {
                optComp.IsFilled = true;
                changed = true;
            }
        }

        if (Prepared)
        {
            timer.Start();
        }

        return changed;
    }

    public bool UpdateCandidate(KitchenItemMaster.KitchenItem m)
    {
        if (m == null) 
            return false;

        bool changed = false;

        // 1) Sequential: 아직 안 찬 "다음" 슬롯 하나만 검사
        for (int i = 0; i < SequentialComponents.Count; i++)
        {
            var comp = SequentialComponents[i];
            if (comp.IsFilled) continue;

            // 첫 번째로 비어있는 순차 슬롯이 m을 받을 수 있는지 확인
            if (IsSameItem(comp.Master, m))
            {
                comp.IsCandidate = true;
                changed = true;
            }

            // 순차 슬롯은 오직 "다음" 것만 의미 있으니 바로 break
            break;
        }

        // 2) Optional: 아직 아무 변화 없으면 옵션 슬롯도 시도
        if (!changed && !string.IsNullOrEmpty(m.tid))
        {
            if (OptionalComponents.TryGetValue(m.tid, out var optComp) && !optComp.IsFilled)
            {
                optComp.IsCandidate = true;
                changed = true;
            }
        }

        return changed;
    }

    public void ClearCandidate()
    {
        foreach (var i in SequentialComponents)
        {
            i.IsCandidate = false;
        }

        foreach (var i in OptionalComponents.Values)
        {
            i.IsCandidate = false;
        }
    }

    public bool HasAccept(KitchenItemMaster.KitchenItem m)
    {
        if (m == null) return false;

        // 1) Sequential: 다음 빈 슬롯이 m을 받을 수 있는가?
        for (int i = 0; i < SequentialComponents.Count; i++)
        {
            var comp = SequentialComponents[i];
            if (comp.IsFilled) continue;

            // 첫 번째 비어있는 순차 슬롯만 의미 있음
            return IsSameItem(comp.Master, m);
        }

        // 2) Optional: 아직 안 찬 동일 tid 슬롯이 있는가?
        if (!string.IsNullOrEmpty(m.tid) &&
            OptionalComponents.TryGetValue(m.tid, out var optComp) &&
            !optComp.IsFilled)
        {
            return true;
        }

        return false;
    }

    private static bool IsSameItem(KitchenItemMaster.KitchenItem a, KitchenItemMaster.KitchenItem b)
    {
        if (ReferenceEquals(a, b)) return true;
        if (a == null || b == null) return false;

        // tid 기준으로 동일 여부 판단 (레퍼런스가 달라도 같은 종류면 OK)
        return a.tid == b.tid;
    }

}