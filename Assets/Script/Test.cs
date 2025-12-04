using Spine.Unity;
using UnityEngine;

public class Test : MonoBehaviour
{
    private static readonly ILogger log = Debug.unityLogger;

    SkeletonMecanim con;
    Animator animator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        con = GetComponentInChildren<SkeletonMecanim>();
        animator = con.GetComponent<Animator>();

    }

    enum AniType
    {
        Idle,
        IdleGood1,
        IdleGood2,
        IdleAngry,
        Moving,
        MovingAngry,
        MovingGood
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnAngry()
    {
        animator.SetTrigger("Angry");
    }

    public void OnGood()
    {
        animator.SetTrigger("Good");
    }

    public void OnServed()
    {
        animator.SetTrigger("Served");
    }

    public void OnMovingGood()
    {
        animator.SetInteger("State", (int)AniType.MovingGood);
    }

    public void OnMovingAngry()
    {
        animator.SetInteger("State", (int)AniType.MovingAngry);
    }

    public void OnMoving()
    {
        animator.SetInteger("State", (int)AniType.Moving);
    }

    public void OnIdleAngry()
    {
        animator.SetInteger("State", (int)AniType.IdleAngry);
    }

    public void OnIdleGood1()
    {
        animator.SetInteger("State", (int)AniType.IdleGood1);
    }

    public void OnIdleGood2()
    {
        animator.SetInteger("State", (int)AniType.IdleGood2);
    }


}
