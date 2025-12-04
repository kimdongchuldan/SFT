using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using Foundation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public interface Droppable
{
    public void Drop(KitchenItem ki);
    public bool IsAcceptable(KitchenItemMaster.KitchenItem mst);
    public void UpdateHover(KitchenItem ki);
    public void LeaveHover();
}

public partial class CookingTool : MonoBehaviour, Droppable
{
    private static readonly ILogger log = Debug.unityLogger;
    public bool Opened;
    public string Ingredient;
    KitchenItemMaster.KitchenItem ingredientMaster;
    public string Cooking;
    Cooking cooking;
    GameObject cookingGameObject;

    void RefreshVisual()
    {
        transform.Find("Closed").gameObject.SetActive(!Opened);
        transform.Find("Opened").gameObject.SetActive(Opened);
    }

    #if UNITY_EDITOR
        void OnValidate()
        {
            RefreshVisual();
        }
    #endif

    void Caching()
    {
        var m = MasterManager.Get<KitchenItemMaster>();

        if (false == string.IsNullOrEmpty(Ingredient))
        {
            ingredientMaster = m.Get(Ingredient);
            if (null == ingredientMaster)
            {
                log.LogError("", $"기초 재료 마스터 없다 {Ingredient}");
            }
        }
        else
        {
            var mst = m.Get(Cooking);
            if (null == mst)
            {
                log.LogError("", $"조리대상 키친 아이템 마스터 없다 {Cooking}");
                return;
            }

            {
                cookingGameObject = CreateKitchenItem(mst.prefab, Vector3.zero);
                if (null == cookingGameObject)
                {
                    log.LogError("", $"쿠킹툴 초기화중 쿠킹 생성 실패 {mst.tid}");
                    return;
                }

                var model = transform.Find("Model");
                cookingGameObject.transform.SetParent(model.transform, worldPositionStays: false);

                var c = new Cooking(mst);
                var t = cookingGameObject.GetComponent<KitchenItem>();
                t.Cooking = c;
                
                cooking = c;
            }
        }
    }
        
    public void Drop(KitchenItem ki)
    {
        if (null != ingredientMaster)
        {
            log.LogError("", $"기본 재료에 드롭함");
            return;
        }

        if(null != cooking)
        {
            if (cooking.Prepared)
            {
                // 완료된 상태에서 같은게 들어왔다면 머지 가능
                if (ki.Master.tid == cooking.Master.tid)
                {
                    log.Log("머지");
                    // 블라블라
                    return;
                }
            }

            log.Log($"떨굼 {ki.Master.tid}({ki.Master.putdown_item.tid})");
            
            cooking.Drop(ki.Master.putdown_item);

            if (cooking.Prepared)
            {
                log.Log($"완성됨 {cooking.Master.tid}");
            }
        }
    }

    void Awake()
    {
        RefreshVisual();
    }

    void Start()
    {
        DroppableManager.Add(gameObject);
        Caching();
        InitPickStates();
    }

    void Update()
    {
        pickState.Update();
    }

    void OnDestroy()
    {
        DroppableManager.Remove(gameObject);
    }

    GameObject CreateKitchenItem(string prefab, Vector3 pos)
    {
        try
        {
            var path = $"Prefab/KitchenItem/{prefab}";
            var pf = Resources.Load<GameObject>(path);
            if (null == pf)
            {
                log.LogError("", $"키친아이템 생성중 실패 {path}");
                return null;
            }

            var i = Instantiate(pf, pos, Quaternion.identity);

            var col = i.GetComponent<BoxCollider2D>();
            if (col == null)
            {
                log.LogError("", $"키친아이템 생성중 BoxCollider2D 요소가 없어 실패 {path}");
                return null;
            }
            
            var ki = i.GetComponent<KitchenItem>();
            if (null == ki)
            {
                log.LogError("", $"키친아이템 생성중 KitchenItem 구현이 없어 실패 {path}");
                return null;
            }

            log.Log($"키친 아이템 생성 {path}");

            return i;
        }
        catch(Exception ex)
        {
            log.LogError("", $"키친 아이템 생성 실패 {ex.Message}");
            return null;
        }
    }
}
