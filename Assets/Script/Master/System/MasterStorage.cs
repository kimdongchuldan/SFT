using System;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public static class MasterStorage
{
    private static readonly ILogger log = Debug.unityLogger;

    static string GetPersistentPath(string path)
    {
        return Path.Combine(Application.persistentDataPath, "Master", $"{path}.json");
    }

    static string GetDefaultPath(string path)
    {
        // path 가 / 로 시작되면 리눅스에서는 루트 임으로... 상대 경로가 무시되니 참고
        return Path.Combine(Application.streamingAssetsPath, "Master", "Master", $"{path}.json");
    }

    static public string Load(string path)
    {
        var p = PathExtention.Correct(path);

        string defaultJson;
        var dp = GetDefaultPath(p);
 
        #if UNITY_ANDROID
            // Android는 StreamingAssets를 File.ReadAllText로 못 읽음
            var req = UnityEngine.Networking.UnityWebRequest.Get(dp);
            req.SendWebRequest();
            while (!req.isDone);
            defaultJson = req.downloadHandler.text;
        #else
            log.Log($"기본 저장소에서 로드 {dp}");

            if (false == File.Exists(dp))
            {
                log.LogWarning("", $"마스터 파일 미존재 {dp}");
                return null;
            }

            defaultJson = File.ReadAllText(dp);
        #endif

        return defaultJson; // 개발중에는 기본 경로만 사용
    }


/*
    static public string Load(string path)
    {
        var p = PathExtention.Correct(path);

        var pp = GetPersistentPath(p);
        if (File.Exists(pp))
        {
            log.Log($"영속적 저장소에서 로드 {pp}");
            return File.ReadAllText(pp);
        }

        string defaultJson;
        var dp = GetDefaultPath(p);
 
        #if UNITY_ANDROID
            // Android는 StreamingAssets를 File.ReadAllText로 못 읽음
            var req = UnityEngine.Networking.UnityWebRequest.Get(dp);
            req.SendWebRequest();
            while (!req.isDone);
            defaultJson = req.downloadHandler.text;
        #else
            log.Log($"기본 저장소에서 로드 {dp}");

            if (false == File.Exists(dp))
            {
                log.LogWarning("", $"마스터 파일 미존재 {dp}");
                return null;
            }

            defaultJson = File.ReadAllText(dp);
        #endif

        #if UNITY_EDITOR
            return defaultJson; // 개발중에는 기본 경로만 사용
        #else
        
            // 3) PersistentDataPath 에 복사
            var dir = Path.GetDirectoryName(pp);
            log.Log($"마스터 파일 영속적 저장소 경로 {dir}");
            Directory.CreateDirectory(dir);
            File.WriteAllText(pp, defaultJson);
            return defaultJson;
        #endif
    }
    */
}
