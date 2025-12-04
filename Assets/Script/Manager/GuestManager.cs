using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using Foundation;
using UnityEngine;

[DefaultExecutionOrder(-50)]
public partial class GuestManager : MonoBehaviour
{
    private static readonly ILogger log = Debug.unityLogger;

    Dictionary<int, Line> lines = new();

    public float SumRemainingPatienceTime()
    {
        float sum = 0;
        foreach (var l in lines.Values)
        {
            sum += l.RemainingPatienceTime();
        }
        return sum;
    }

    void Start()
    {
        var mg = MasterManager.Get<StageMaster>();
        var stage = mg.GetByIndex(1);
        if (null == stage)
        {
            log.LogError("", $"스테이지 마스터 없음 {1}");
            return;
        }

        var lineset = stage.lines.Count;
        log.Log($"대기줄은 {lineset}개로 구성되어 있는 스테이지 {stage.tid}");

        var sm = GetComponent<StageManager>();
        foreach (var l in stage.lines)
        {
            var q = sm.GetLineQueue(lineset, l.Key);
            if (null == q)
            {
                log.LogError("", $"StageManager에서 손님 줄 서는 위치 얻기 실패 {lineset}셋 - {l.Key}줄");
                continue;
            }

            lines[l.Key] = new Line(this, l.Value, q);
        }
    }

    void Update()
    {
        foreach (var l in lines.Values)
        {
            l.Update();
        }
    }

    public void StartSpawn()
    {
        foreach (var l in lines.Values)
        {
            l.Start();
        }
    }
}

