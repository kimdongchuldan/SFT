using System.Threading;
using Foundation;
using UnityEngine;

public partial class TteokGameManager
{    
    enum StepType
    {
        Ready,
        Open,
        Play,
        Close,
        Finish
    }

    FSM<StepType> step = new();

    StageManager stageManager;
    GuestManager guestManager;
    InGameUIManager inGameUIManager;
    TteokCounterManager tteokCounterManager;
    GuestManager tteokGuestManager;

    void InitStepState()
    {
        stageManager = GetComponent<StageManager>();
        Debug.Assert(null!=stageManager);

        guestManager = GetComponent<GuestManager>();
        Debug.Assert(null!=guestManager);

        inGameUIManager = GetComponent<InGameUIManager>();
        Debug.Assert(null!=inGameUIManager);

        tteokCounterManager = GetComponent<TteokCounterManager>();
        Debug.Assert(null!=tteokCounterManager);

        tteokGuestManager = GetComponent<GuestManager>();
        Debug.Assert(null!=tteokGuestManager);

        Step_ReadyState();
        Step_OpenState();
        Step_PlayState();
        Step_CloseState();
        Step_FinishState();

        step.Transit(StepType.Ready);
    }

    DeltaElapsedTimer sharedTimer = new();

    void Step_ReadyState()
    {
        FSM<StepType>.Handler b = delegate ()
        {
            log.Log($"{GetType().Name} 준비");
            sharedTimer.Start();
        };

        FSM<StepType>.Handler u = delegate ()
        {
            sharedTimer.Past(Time.deltaTime);

            if (sharedTimer.ElapsedSeconds > 1)
            {
                step.Transit(StepType.Open);
                return;
            }
        };

        FSM<StepType>.Handler e = delegate ()
        {
        };

        step.Add(StepType.Ready, b, u, e);
    }

    void Step_OpenState()
    {
        FSM<StepType>.Handler b = delegate ()
        {
            log.Log($"{GetType().Name} 오픈");
            tteokCounterManager.Prepare();

        };

        FSM<StepType>.Handler u = delegate ()
        {
            if (tteokCounterManager.IsReady())
            {
                step.Transit(StepType.Play);
                return;
            }
        };

        FSM<StepType>.Handler e = delegate ()
        {
        };

        step.Add(StepType.Open, b, u, e);
    }

    void Step_PlayState()
    {
        FSM<StepType>.Handler b = delegate ()
        {
            log.Log($"{GetType().Name} 플레이");
            tteokGuestManager.StartSpawn();
        };

        FSM<StepType>.Handler u = delegate ()
        {
        };

        FSM<StepType>.Handler e = delegate ()
        {
        };

        step.Add(StepType.Play, b, u, e);
    }

    void Step_CloseState()
    {
        FSM<StepType>.Handler b = delegate ()
        {
            log.Log($"{GetType().Name} 닫기");
        };

        FSM<StepType>.Handler u = delegate ()
        {
        };

        FSM<StepType>.Handler e = delegate ()
        {
        };

        step.Add(StepType.Close, b, u, e);
    }

    void Step_FinishState()
    {
        FSM<StepType>.Handler b = delegate ()
        {
            log.Log($"{GetType().Name} 종료");
        };

        FSM<StepType>.Handler u = delegate ()
        {
        };

        FSM<StepType>.Handler e = delegate ()
        {
        };

        step.Add(StepType.Finish, b, u, e);
    }
}

