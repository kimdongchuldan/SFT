using Foundation;
using Spine;
using Spine.Unity;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public partial class Guest
{
    enum LifecycleStateType
    {
        Standby,
        Spawning,
        Queued,
        Order,
        Eating,
        Leaving,
        Despawned,
    }

    float patienceTime;

    public void StartSpawn()
    {
        lifecycleState.Transit(LifecycleStateType.Spawning);
    }

    public void StartOrder(float pt)
    {
        patienceTime = pt;
        log.Log($"{Master.tid}의 주문 {patienceTime}초");

        if (lifecycleState.Current() == LifecycleStateType.Queued)
            lifecycleState.Transit(LifecycleStateType.Order, false);
    }

    public bool IsQueued
    {
        get
        {
            if (lifecycleState.Current() == LifecycleStateType.Queued)
                return true;
            return false;
        }
    }

    public bool IsLeaving
    {
        get
        {
            if (lifecycleState.Current() == LifecycleStateType.Leaving)
                return true;
            return false;
        }
    }

    FSM<LifecycleStateType> lifecycleState = new();

    void InitLifecycleStates()
    {
        Lifecycle_SpawaningState();
        Lifecycle_QueuedState();
        Lifecycle_OrderState();
        Lifecycle_EatingState();
        Lifecycle_LeavingState();

        lifecycleState.Add(LifecycleStateType.Standby, null, null, null);
        lifecycleState.Add(LifecycleStateType.Despawned, null, null, null);
        
        lifecycleState.Transit(LifecycleStateType.Standby);
    }

    DeltaElapsedTimer sharedTimer = new();

    void Lifecycle_SpawaningState()
    {
        FSM<LifecycleStateType>.Handler b = delegate ()
        {
            //log.Log($"{GetType().Name} 대기");
            sharedTimer.Start();
            SetActiveCollision(false);
            StartCoroutine(FadeIn(GetComponentInChildren<SkeletonMecanim>(), 1.0f));
        };

        FSM<LifecycleStateType>.Handler u = delegate ()
        {
            sharedTimer.Past(Time.deltaTime);
            if (sharedTimer.ElapsedSeconds > 1.0f)
            {
                lifecycleState.Transit(LifecycleStateType.Queued);
                return;
            }
        };

        FSM<LifecycleStateType>.Handler e = delegate ()
        {
        };

        lifecycleState.Add(LifecycleStateType.Spawning, b, u, e);
    }

    void Lifecycle_QueuedState()
    {
        FSM<LifecycleStateType>.Handler b = delegate ()
        {
            //log.Log($"{GetType().Name} 대기");
            SetIdle(AniType.Idle);
        };

        FSM<LifecycleStateType>.Handler u = delegate ()
        {
        };

        FSM<LifecycleStateType>.Handler e = delegate ()
        {
        };

        lifecycleState.Add(LifecycleStateType.Queued, b, u, e);
    }

    DeltaElapsedTimer orderTimer = new();
    Slider slider;

    void React(string name)
    {
        if (null != ani)
        {
            ani.SetTrigger(name);
        }
    }
    
    void Lifecycle_OrderState()
    {
        FSM<LifecycleStateType>.Handler b = delegate ()
        {
            log.Log($"음식주문 시작");
            orderTimer.Start();

            if (Master.orders.Count > 0) // 유령은 오더가 없다
            {
                SetActiveCollision(true);
                var order = transform.Find("Order");
                order.gameObject.SetActive(true);
                var canvas = order.Find("Canvas");
                slider = canvas.GetComponentInChildren<Slider>();
                slider.value = 0;
                slider.minValue = 0;
                slider.maxValue = patienceTime;
            }
        };

        FSM<LifecycleStateType>.Handler u = delegate ()
        {
            orderTimer.Past(Time.deltaTime);
            var elapsed = orderTimer.ElapsedSeconds;
            elapsed = Mathf.Clamp(elapsed, 0, patienceTime);

            if (Master.orders.Count > 0) // 유령은 오더가 없다
            {
                slider.value = elapsed;
            }

            if (elapsed >= patienceTime)
            {
                Leave(AniType.IdleAngry);
                lifecycleState.Transit(LifecycleStateType.Leaving);
            }
        };

        FSM<LifecycleStateType>.Handler e = delegate ()
        {
            if (Master.orders.Count > 0) // 유령은 오더가 없다
            {
                SetActiveCollision(false);
                var order = transform.Find("Order");
                order.gameObject.SetActive(false);
            }
        };

        lifecycleState.Add(LifecycleStateType.Order, b, u, e);
    }

    DeltaElapsedTimer eatingTimer = new();

    void Lifecycle_EatingState()
    {
        FSM<LifecycleStateType>.Handler b = delegate ()
        {
            //log.Log($"{GetType().Name} 대기");
            log.Log($"음식 먹었음");
            Leave(AniType.MovingGood);
            eatingTimer.Start();
        };

        FSM<LifecycleStateType>.Handler u = delegate ()
        {
            eatingTimer.Past(Time.deltaTime);

            if (eatingTimer.ElapsedSeconds > 1)
            {
                lifecycleState.Transit(LifecycleStateType.Leaving);
            }
        };

        FSM<LifecycleStateType>.Handler e = delegate ()
        {
        };

        lifecycleState.Add(LifecycleStateType.Eating, b, u, e);
    }

    DeltaElapsedTimer leavingTimer = new();

    void Lifecycle_LeavingState()
    {
        FSM<LifecycleStateType>.Handler b = delegate ()
        {
            log.Log($"손님 나가기 시작");
            leavingTimer.Start();
        };

        FSM<LifecycleStateType>.Handler u = delegate ()
        {
            leavingTimer.Past(Time.deltaTime);

            if (leavingTimer.ElapsedSeconds > 10)
            {
                lifecycleState.Transit(LifecycleStateType.Despawned);
                return;
            }

            if (false == IsMoving)
            {
                lifecycleState.Transit(LifecycleStateType.Despawned);
                return;
            }            
        };

        FSM<LifecycleStateType>.Handler e = delegate ()
        {
            log.Log($"손님 나가기 완료");
        };

        lifecycleState.Add(LifecycleStateType.Leaving, b, u, e);
    }
}
