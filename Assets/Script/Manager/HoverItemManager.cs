using System.Collections.Generic;
using UnityEngine;

static public class HoverItemManager
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

    static public GameObject SearchOverlap(GameObject obj)
    {
        var a = obj.GetComponent<BoxCollider2D>();        
        foreach (var o  in objs)
        {
            var b = o.GetComponent<BoxCollider2D>();
            if (a == b)
                continue;

            var distance = Physics2D.Distance(a, b);
            if (distance.isOverlapped)
                return o;
        }
        return null;
    }

    static public void Clear()
    {
        objs.Clear();
    }
}