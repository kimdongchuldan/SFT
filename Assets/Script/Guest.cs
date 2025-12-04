using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class Order
{
    public KitchenItemMaster.KitchenItem Master;
    public int ServedCount;
    public int OrderCount;
}

public partial class Guest : MonoBehaviour, Droppable
{
    private static readonly ILogger log = Debug.unityLogger;

    public StageMaster.Spawn Master;

    Dictionary<KitchenItemMaster.KitchenItem, Order> orders = new();

    public void Drop(KitchenItem ki)
    {
        var i = ki.Master.putdown_item;

        log.Log($"손님에게 떨굼 {i.tid}");

        if (lifecycleState.Current() == LifecycleStateType.Order)
        {
            int modfied = 0;

            if (orders.TryGetValue(i, out var o))
            {
                if (o.OrderCount <= o.ServedCount)
                {
                    log.Log($"이미 다 받음 {i.tid}");
                    return;
                }

                o.ServedCount++;
                
                modfied++;
            }

            bool done = true;
            foreach (var od in orders.Values)
            {
                if (od.OrderCount <= od.ServedCount)
                {
                    // 다 받음
                }
                else
                {
                    done = false;
                    log.Log($"일부 받음 {od.Master.tid} {od.ServedCount}/{od.OrderCount}");

                    if (modfied > 0)
                    {
                        React("Served");
                    }
                    else
                    {
                        React("Angry");
                    }
                }
            }

            if (done)
            {
                log.Log($"다 받음 {i.tid}");
                lifecycleState.Transit(LifecycleStateType.Eating, false);
                return;
            }
        }
    }

    AnimatorStateInfo AniState
    {
        get
        {
            if (null != ani)
            {
                return ani.GetCurrentAnimatorStateInfo(0);
            }
            return default(AnimatorStateInfo);
        }
    }

    public float RemainingPatienceTime
    {
        get
        {
            if (lifecycleState.Current() == LifecycleStateType.Order)
            {
                return Mathf.Clamp(patienceTime - orderTimer.ElapsedSeconds, 0.0f, patienceTime);
            }
            return 0.0f;
        }
    }

    GameObject CreateMenu(string name)
    {
        var path = $"Prefab/Menu/{name}";
        var pf = Resources.Load<GameObject>(path);
        if (null == pf)
        {
            log.LogError("", $"메뉴 프리팹 생성중 실패 {path}");
            return null;
        }
        return Instantiate(pf, Vector3.zero, Quaternion.identity);
    }

    void InitOrders()
    {
        var order = transform.Find("Order").transform;
        order.gameObject.SetActive(false);
        var anchor = order.Find("Anchor").transform;
        var f1 = anchor.Find("F1").transform;
        var f2 = anchor.Find("F2").transform;
        var f3 = anchor.Find("F3").transform;

        Queue<Transform> transforms = new();
        transforms.Enqueue(f1);
        transforms.Enqueue(f2);
        transforms.Enqueue(f3);

        foreach (var o in Master.orders)
        {
            var t = transforms.Dequeue();
            var obj = CreateMenu(o.kitchen_item_tid);
            obj.transform.SetParent(t, worldPositionStays: false);

            var m = obj.GetComponent<Menu>();

            m.Order = new Order() { 
                Master = o.master, 
                OrderCount = o.count 
                };

            orders.Add(o.master, m.Order);
        }
    }

    void Start()
    {
        var mc = GetComponentInChildren<SkeletonMecanim>();
        mc.skeleton.SetColor(new Color(1, 1, 1, 0));
        mc.LateUpdate();

        InitOrders();
        DroppableManager.Add(gameObject);
        InitMovingStates();
        InitLifecycleStates();
    }

    public void LeaveHover()
    {
        
    }

    public void UpdateHover(KitchenItem ki)
    {
        
    }

    void SetActiveCollision(bool flag)
    {
        var c = GetComponent<BoxCollider2D>();
        c.enabled = flag;
    }

    public bool IsAcceptable(KitchenItemMaster.KitchenItem mst)
    {
        return true;
    }

    public bool IsDespawned
    {
        get
        {
            if (lifecycleState.Current() == LifecycleStateType.Despawned)
                return true;
            return false;
        }
    }

    Animator ani;

    public enum AniType
    {
        Idle,
        IdleGood1,
        IdleGood2,
        IdleAngry,
        Moving,
        MovingAngry,
        MovingGood
    }
    

    void Update()
    {
        if (null == ani)
        {
            ani = GetComponentInChildren<Animator>(); // 자식이 나중에 붙는다
        }

        movingState.Update();
        lifecycleState.Update();
    }

    void OnDestroy()
    {
        DroppableManager.Remove(gameObject);
    }
}
