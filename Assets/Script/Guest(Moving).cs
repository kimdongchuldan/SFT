using System.Collections;
using Foundation;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.AI;

public partial class Guest
{
    Vector3 movingTarget;
    float scaleTarget;

    float moveSpeed = 2.0f;
    float arriveDistance = 0.02f;
    //float scaleLerpSpeed = 5.0f;

    /*void SetAnimSortingLayer(string name)
    {
        if (ani)
        {
            var mr = ani.GetComponent<MeshRenderer>();
            if (null != mr)
            {
                mr.sortingLayerName = name;
            }
        }
    }*/

    void SetAnimSortingOrder(int order)
    {
        if (ani)
        {
            var meshRenderer = ani.GetComponent<MeshRenderer>();
            if (null != meshRenderer)
            {
                meshRenderer.sortingOrder = order;

            }
        }
    }

    IEnumerator FadeOut(SkeletonMecanim sk, float duration)
    {
        float t = 0f;
        var startColor = sk.skeleton.GetColor();

        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(1f, 0f, t / duration);
            sk.skeleton.SetColor(new Color(1, 1, 1, a));
            sk.LateUpdate();  // 즉시 반영
            yield return null;
        }
    }

    IEnumerator FadeIn(SkeletonMecanim sk, float duration)
    {
        float t = 0f;
        var startColor = sk.skeleton.GetColor();

        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(0f, 1f, t / duration);
            sk.skeleton.SetColor(new Color(1, 1, 1, a));
            sk.LateUpdate();  // 즉시 반영
            yield return null;
        }
    }

    AniType movingAni;

    public void Move(Vector3 pos, float scale, int sortingOrder)
    {
        movingAni = AniType.Moving;
        SetAnimSortingOrder(sortingOrder);
        movingTarget = pos;
        scaleTarget = scale;
        movingState.Transit(MovingStateType.Walking);
    }

    public void Leave(AniType type)
    {
        StartCoroutine(FadeOut(GetComponentInChildren<SkeletonMecanim>(), 1.0f));
        movingAni = type;
        SetAnimSortingOrder(3);
        movingTarget = new Vector3(-18.67f, -2.59f, 0);
        movingState.Transit(MovingStateType.Walking);
    }

    enum MovingStateType
    {
        Idle,
        Walking
    }

    FSM<MovingStateType> movingState = new();

    void InitMovingStates()
    {
        MovingState();
        movingState.Add(MovingStateType.Idle, null, null, null);
        movingState.Transit(MovingStateType.Idle);
    }

    void SetIdle(AniType at)
    {
        if (null != ani)
        {
            ani.SetInteger("State", (int)at);
        }
    }

    void MovingState()
    {
        FSM<MovingStateType>.Handler b = delegate ()
        {
            //log.Log($"{GetType().Name} 대기");
            SetIdle(movingAni);
        };

        FSM<MovingStateType>.Handler u = delegate ()
        {
            // 줄 기다리는 상태일때만 이동 가능
             // 위치 이동
            transform.position =
                Vector3.MoveTowards(transform.position, movingTarget, moveSpeed * Time.deltaTime);

            // 스케일 보간
            /*var targetScale = new Vector3(scaleTarget, scaleTarget, transform.localScale.z);
            transform.localScale =
                Vector3.Lerp(transform.localScale, targetScale, scaleLerpSpeed * Time.deltaTime);*/

            // 도착 체크
            if ((transform.position - movingTarget).sqrMagnitude <= arriveDistance * arriveDistance)
            {
                // 정확히 도착으로 스냅
                transform.position = movingTarget;
                //transform.localScale = targetScale;

                movingState.Transit(MovingStateType.Idle);
            }
        };

        FSM<MovingStateType>.Handler e = delegate ()
        {
            SetIdle(AniType.Idle);
        };

        movingState.Add(MovingStateType.Walking, b, u, e);
    }

    public bool IsMoving
    {
        get
        {
            if (movingState.Current() == MovingStateType.Walking)
                return true;
            return false;
        }
    }
}
