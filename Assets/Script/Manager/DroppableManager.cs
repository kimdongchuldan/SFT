using System.Collections.Generic;
using System.Linq;
using UnityEngine;

static public class DroppableManager
{
    private static readonly ILogger log = Debug.unityLogger;
    static HashSet<GameObject> objs = new();

    static public void Add(GameObject obj)
    {
        objs.Add(obj);
    }

    static public void Remove(GameObject obj)
    {
        objs.Remove(obj);
    }

    static public GameObject SearchOverlap(GameObject exclude, GameObject obj)
    {
        var a = obj.GetComponent<BoxCollider2D>();
        GameObject nearest = null;
        float bestDist = float.MaxValue;   // 최소값 찾기용

        foreach (var o in objs)
        {
            if (o == exclude)
                continue;

            var b = o.GetComponent<BoxCollider2D>();
            if (a == b)
                continue;

            var info = Physics2D.Distance(a, b);
            if (info.isOverlapped)
            {
                float d = Mathf.Abs(info.distance); 
                // 겹치는 경우 distance는 보통 0 또는 음수 → 절대값으로 비교

                if (d < bestDist)
                {
                    bestDist = d;
                    nearest = o;
                }
            }
        }

        return nearest;
    }

    static public List<GameObject> SearchOverlapSorted(GameObject exclude, GameObject obj)
    {
        var a = obj.GetComponent<BoxCollider2D>();
        List<(GameObject go, float dist)> hits = new();

        foreach (var o in objs)
        {
            if (o == exclude)
                continue;

            var b = o.GetComponent<BoxCollider2D>();
            if (a == b)
                continue;

            var info = Physics2D.Distance(a, b);
            if (info.isOverlapped)
            {
                float d = Mathf.Abs(info.distance); 
                hits.Add((o, d));
            }
        }

        // 거리 기준으로 정렬 (가까운 → 먼 순)
        hits.Sort((x, y) => x.dist.CompareTo(y.dist));

        // GameObject만 리스트로 변환
        return hits.Select(x => x.go).ToList();
    }


    static public void Clear()
    {
        objs.Clear();
    }


}