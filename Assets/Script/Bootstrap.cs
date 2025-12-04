using UnityEngine;

public static class Bootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        Debug.Log("마스터 로드!");
        MasterManager.Initialize();
    }
}

// 폰트 시퀀스 TMP
// 32-126,44032-55203,12593-12643,8200-9900