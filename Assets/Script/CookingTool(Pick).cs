using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.UI;

public partial class CookingTool
{
    enum PickStateType
    {
        Standby,
        Picked
    }

    FSM<PickStateType> pickState = new();

    BoxCollider2D inputArea;
    InputManager.Input input;
    GameObject pickedObject;

    void InitPickStates()
    {
        inputArea = GetComponent<BoxCollider2D>();
        Pick_StandbyState();
        Pick_PickedState();
        pickState.Transit(PickStateType.Standby);
    }

    void Pick_StandbyState()
    {
        FSM<PickStateType>.Handler b = delegate ()
        {
        };

        FSM<PickStateType>.Handler u = delegate ()
        {
            input = InputManager.Take(inputArea, true);
            if (null == input)
            {
                if (null != cookingGameObject)
                {               
                    var col = cookingGameObject.GetComponent<BoxCollider2D>(); 
                    input = InputManager.Take(col, true);
                    if (null != input)
                    {
                        pickState.Transit(PickStateType.Picked);
                        return;
                    }
                }

                if (null != cooking) // 쿠킹중이면 업데이트(내부 퀄리티 타이머)
                {
                    cooking.Update();

                    if (cooking.Prepared)
                    {
                        if (cooking.Quality == 0) // 퀄리티가 0이면 삭제
                        {
                            cooking = new(cooking.Master);
                            var d = cookingGameObject.GetComponent<KitchenItem>();
                            d.Cooking = cooking;
                        }
                    }
                }
            }
            else
            {
                pickState.Transit(PickStateType.Picked);
                return;
            }
        };

        FSM<PickStateType>.Handler e = delegate ()
        {
        };

        pickState.Add(PickStateType.Standby, b, u, e);
    }

    public bool IsAcceptable(KitchenItemMaster.KitchenItem mst)
    {
        if (null != ingredientMaster)
            return false;

        if (null == cooking)
            return false;

        return cooking.HasAccept(mst);
    }

    public void LeaveHover()
    {
        if (null != ingredientMaster)
            return;

        if (null == cooking)
            return;

        cooking.ClearCandidate();
    }

    public void UpdateHover(KitchenItem ki)
    {
        if (null != ingredientMaster)
            return;

        if (null == cooking)
            return;
        
        // 이미 준비된게 있으면 무시된다
        if (cooking.Prepared)
        {
            if (ki.Master.putdown_item.tid == cooking.Master.tid)
            {
                log.Log("머지 가능");
            }
            return;
        }

        cooking.UpdateCandidate(ki.Master.putdown_item);


        //log.Log($"호버링 {cooking?.Master.tid}에 {ki.Master.putdown_item.tid}");
    }

    Droppable latestHover;
    KitchenItemMaster.KitchenItem pickMaster;
    
    void Pick_PickedState()
    {
        FSM<PickStateType>.Handler b = delegate ()
        {
            KitchenItemMaster.KitchenItem target = null;
            
            if (null != ingredientMaster)
            {
                target = ingredientMaster;
            }
            else
            {
                if (null == cooking)
                    return;

                if (cooking.Prepared)
                {
                    target = cooking.Master;
                }
            }

            if (null == target)
                return;

            pickMaster = target.pickup_item;

            {
                pickedObject = CreateKitchenItem(target.pickup_item.prefab, input.WorldPoint);

                var ki = pickedObject.GetComponent<KitchenItem>();

                if (null != ingredientMaster)
                {
                    cooking = new(ingredientMaster);
                }
                else
                {
                    ki.Cooking = cooking;
                    ki.Cooking.Paused = true; // 퀄리티에 영향 없게 중지
                    
                    cooking = new(cooking.Master);
                    var d = cookingGameObject.GetComponent<KitchenItem>();
                    d.Cooking = cooking;
                }
                
                HoverItemManager.Add(pickedObject);
            }
        };

        FSM<PickStateType>.Handler u = delegate ()
        {
            if (null == pickedObject)
            {
                log.LogError("", "피킹중인데, 이미 오브젝트가 파괴됨");
                pickState.Transit(PickStateType.Standby);
                return;
            }

            pickedObject.transform.SetPositionAndRotation(input.WorldPoint, Quaternion.identity);

            var item = pickedObject.GetComponent<KitchenItem>();
            if (null == item)
            {
                log.LogWarning("", $"피킹 아이템인데, KitchenItem인터페이스 구현 없어서 피킹 종료 {pickedObject.GetType().Name}");
                pickState.Transit(PickStateType.Standby);
                return;
            }

            var list = DroppableManager.SearchOverlapSorted(gameObject, pickedObject);

            Droppable ct = null;
            {
                // 겹치는 도구중에 들고 있는 아이템이 받기 가능한거 찾기
                foreach(var o in list)
                {
                    var c = o.GetComponent<Droppable>();
                    if (null != c)
                    {
                        if (c.IsAcceptable(pickMaster.putdown_item))
                        {
                            ct = c;
                        }
                    }
                }
            }
            
            if (latestHover != ct)
            {
                if (null != latestHover)
                    latestHover.LeaveHover();
                latestHover = ct;
            }
            
            if (false == input.Pressed)
            {
                if (null != ct)
                {
                    ct.Drop(pickedObject.GetComponent<KitchenItem>());
                }
                else
                {
                    log.LogWarning("", $"허공에 떨굼");
                }
                
                pickState.Transit(PickStateType.Standby);
                return;
            }
            else
            {
                if (null != ct)
                {
                    ct.UpdateHover(item);
                }
            }
        };

        FSM<PickStateType>.Handler e = delegate ()
        {
            log.Log("피킹 상태 종료");
            HoverItemManager.Remove(pickedObject);
            Destroy(pickedObject);
            pickedObject = null;
        };

        pickState.Add(PickStateType.Picked, b, u, e);
    }
}
