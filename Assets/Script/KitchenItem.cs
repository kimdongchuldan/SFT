using System.Collections.Generic;
using Foundation;
using UnityEngine;
using UnityEngine.UI;

public class KitchenItem : MonoBehaviour
{
    private static readonly ILogger log = Debug.unityLogger;
    public string TID;
    KitchenItemMaster.KitchenItem master;

    public Cooking Cooking;

    //Dictionary<Cooking.Ingredient, GameObject> cooking = new();

    Dictionary<string, GameObject> components = new();

    Slider gauge;

    public KitchenItemMaster.KitchenItem Master
    {
        get
        {
            return master;
        }
    }

    void Start()
    {
        var m = MasterManager.Get<KitchenItemMaster>();
        master =  m.Get(TID);
        if (null == master)
        {
            log.LogError("", $"음식 마스터 찾기 실패 {GetType().Name} - {TID}");
        }
        
        foreach (Transform c in transform)
        {
            if ("Canvas" == c.gameObject.name)
                continue;

            components[c.gameObject.name] = c.gameObject;
        }

        foreach (var c in components)
        {
            if (null == Cooking.Get(c.Key))
            {
                log.LogError("", $"Cooking 초기화중, 재료가 누락되어 있습니다 {TID} - {c.Key}");
            }
        }
        
        gauge = GetComponentInChildren<Slider>();
        if (null != gauge)
        {
            gauge.gameObject.SetActive(false);
        }
    }

    void SetActiveGauge(bool a)
    {
        if (null != gauge)
        {
            gauge.gameObject.SetActive(a);
        }
    }


    void Update()
    {
        foreach (var c in components)
        {
            var i = Cooking.Get(c.Key);
            var o = c.Value;

            if (i.IsFilled)
            {
                o.SetActive(true);
            }
            else
            {
                if (i.IsCandidate)
                {
                    o.SetActive(true);
                }
                else
                {
                    o.SetActive(false);
                }
            }
        }

        if (null != Cooking)
        {
            if (Cooking.Prepared)
            {
                if (Cooking.Paused)
                {
                    SetActiveGauge(false);
                }
                else
                {
                    var q = Cooking.Quality;
                    if (q == 1)
                    {
                        SetActiveGauge(false);
                    }
                    else
                    {
                        SetActiveGauge(true);
                    }

                    if (null != gauge)
                    {
                        gauge.value = q;
                    }
                }
            }
            else
            {
                SetActiveGauge(false);
            }
        }
    }
}