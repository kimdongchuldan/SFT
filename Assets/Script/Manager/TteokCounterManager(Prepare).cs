using Foundation;
using UnityEngine;

public partial class TteokCounterManager
{
    enum PhaseType
    {
        Standby,
        Prepare,
        Ready
    }

    FSM<PhaseType> phase = new();

    void StandbyState()
    {
        FSM<PhaseType>.Handler b = delegate ()
        {
            //log.Log($"{GetType().Name}");
            
        };

        FSM<PhaseType>.Handler u = delegate ()
        {
        };

        FSM<PhaseType>.Handler e = delegate ()
        {
        };

        phase.Add(PhaseType.Standby, b, u, e);
    }

    void PrepareState()
    {
        FSM<PhaseType>.Handler b = delegate ()
        {
            //log.Log($"{GetType().Name}");
            
        };

        FSM<PhaseType>.Handler u = delegate ()
        {
            phase.Transit(PhaseType.Ready);
        };

        FSM<PhaseType>.Handler e = delegate ()
        {
        };

        phase.Add(PhaseType.Prepare, b, u, e);
    }

    void ReadyState()
    {
        FSM<PhaseType>.Handler b = delegate ()
        {
            //log.Log($"{GetType().Name}");
            
        };

        FSM<PhaseType>.Handler u = delegate ()
        {
        };

        FSM<PhaseType>.Handler e = delegate ()
        {
        };

        phase.Add(PhaseType.Ready, b, u, e);
    }


    void InitPhaseState()
    {
        StandbyState();
        PrepareState();
        ReadyState();

        phase.Transit(PhaseType.Standby);
    }

    public void Prepare()
    {
        phase.Transit(PhaseType.Prepare);
    }

    public bool IsReady()
    {
        if (phase.Current() == PhaseType.Ready)
            return true;
        return false;
    }
}
