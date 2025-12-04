using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class StageManager : MonoBehaviour
{    
    public Transform 왼쪽_등장1;
    public Transform 왼쪽_등장2;
    public Transform 왼쪽_등장3;
    public Transform 왼쪽_등장4;


    public Transform 오른쪽_등장1;
    public Transform 오른쪽_등장2;
    public Transform 오른쪽_등장3;
    public Transform 오른쪽_등장4;


    public Transform 세트1_1;
    public Transform 세트1_2;
    public Transform 세트1_3;


    public Transform 세트2_1_1;
    public Transform 세트2_1_2;
    public Transform 세트2_1_3;

    public Transform 세트2_2_1;
    public Transform 세트2_2_2;
    public Transform 세트2_2_3;


    public Transform 세트3_1_1;
    public Transform 세트3_1_2;
    public Transform 세트3_1_3;

    public Transform 세트3_2_1;
    public Transform 세트3_2_2;
    public Transform 세트3_2_3;

    public Transform 세트3_3_1;
    public Transform 세트3_3_2;
    public Transform 세트3_3_3;


    public Transform 세트4_1_1;
    public Transform 세트4_1_2;
    public Transform 세트4_1_3;

    public Transform 세트4_2_1;
    public Transform 세트4_2_2;
    public Transform 세트4_2_3;

    public Transform 세트4_3_1;
    public Transform 세트4_3_2;
    public Transform 세트4_3_3;

    public Transform 세트4_4_1;
    public Transform 세트4_4_2;
    public Transform 세트4_4_3;

    Dictionary<string, List<Vector3>> lineset = new();
    
    string MakeKey(int set, int line)
    {
        return $"{set}_{line}";
    }

    void AddLine(int set, int line, params Transform[] points)
    {
        var key = MakeKey(set, line);

        var list = new List<Vector3>();
        foreach (var p in points)
        {
            if (p != null)
                list.Add(p.position);
        }

        if (list.Count > 0)
        {
            lineset[key] = list;
        }
    }

    void Start()
    {
        // 세트 + 라인 조합으로 데이터 구조 만들기
        AddLine(1, 1, 세트1_1, 세트1_2, 세트1_3);

        AddLine(2, 1, 세트2_1_1, 세트2_1_2, 세트2_1_3);
        AddLine(2, 2, 세트2_2_1, 세트2_2_2, 세트2_2_3);

        AddLine(3, 1, 세트3_1_1, 세트3_1_2, 세트3_1_3);
        AddLine(3, 2, 세트3_2_1, 세트3_2_2, 세트3_2_3);
        AddLine(3, 3, 세트3_3_1, 세트3_3_2, 세트3_3_3);

        AddLine(4, 1, 세트4_1_1, 세트4_1_2, 세트4_1_3);
        AddLine(4, 2, 세트4_2_1, 세트4_2_2, 세트4_2_3);
        AddLine(4, 3, 세트4_3_1, 세트4_3_2, 세트4_3_3);
        AddLine(4, 4, 세트4_4_1, 세트4_4_2, 세트4_4_3);
    }

    public List<Vector3> GetLineQueue(int set, int line)
    {
        var key = MakeKey(set, line);
        if (lineset.TryGetValue(key, out var path))
            return path;

        return null; // 없으면 null
    }

    void Update()
    {
        
    }
}
