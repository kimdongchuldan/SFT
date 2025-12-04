using UnityEngine;

public partial class TteokCounterManager : MonoBehaviour
{    
    private static readonly ILogger log = Debug.unityLogger;
    
    // 어묵
    public GameObject P_어묵1;
    public GameObject P_어묵2;
    public GameObject P_어묵3;
    public GameObject P_어묵냄비;
    public GameObject A_어묵냄비;
    public GameObject A_어묵받침1;
    public GameObject A_어묵받침2;
    public GameObject A_어묵받침3;

    // 떡뽁이
    public GameObject P_떡볶이1;
    public GameObject P_떡볶이2;
    public GameObject P_떡볶이3;

    public GameObject P_떡볶이재료;
    public GameObject A_떡볶이재료;

    public GameObject P_떡볶이접시;
    public GameObject A_떡볶이접시;

    public GameObject P_떡볶이받침1;
    public GameObject P_떡볶이받침2;
    public GameObject P_떡볶이받침3;
    public GameObject A_떡볶이받침1;
    public GameObject A_떡볶이받침2;
    public GameObject A_떡볶이받침3;

    public GameObject P_떡볶이철판;
    public GameObject A_떡볶이철판1;
    public GameObject A_떡볶이철판2;
    public GameObject A_떡볶이철판3;

    // 핫도그
    public GameObject P_핫도그1;
    public GameObject P_핫도그2;
    public GameObject P_핫도그3;

    public GameObject P_핫도그재료;
    public GameObject A_핫도그재료;

    public GameObject P_핫도그튀김기;
    public GameObject A_핫도그튀김기1;
    public GameObject A_핫도그튀김기2;
    public GameObject A_핫도그튀김기3;

    public GameObject P_핫도그받침;
    public GameObject A_핫도그받침1;
    public GameObject A_핫도그받침2;
    public GameObject A_핫도그받침3;

    // 김밥
    public GameObject P_김밥1;
    public GameObject P_김밥2;
    public GameObject P_김밥3;
    public GameObject P_김밥도마;
    public GameObject A_김밥도마;
    public GameObject A_김밥받침1;
    public GameObject A_김밥받침2;
    public GameObject A_김밥받침3;

    void Start()
    {
        InitPhaseState();
    }

    void Update()
    {
        phase.Update();
    }
}
