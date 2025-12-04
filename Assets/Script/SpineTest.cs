using UnityEngine;
using Spine;
using Spine.Unity;

public class SpineTest : MonoBehaviour
{
    private static readonly ILogger logger = Debug.unityLogger;

    SkeletonAnimation skeletonAnimation;

    public string skinName = "default";   // 스파인에서 만든 스킨 이름
    public string animationName = "c_play"; // 스파인에서 만든 애니메이션 이름
    public bool loop = true;

     void OnSpineInitialized(ISkeletonAnimation animated)
    {
        Debug.Log("🔥 Spine 초기화 완료!");
        // 여기부터 skin 변경, animation state 변경 모두 OK
    }

    void ApplySkinAndAnimation()
    {
        if (skeletonAnimation == null || !skeletonAnimation.valid)
            return;

        Skeleton skeleton = skeletonAnimation.Skeleton;

        // --- 스킨 변경 ---
        if (!string.IsNullOrEmpty(skinName))
        {
            Skin newSkin = skeleton.Data.FindSkin(skinName);
            if (newSkin != null)
            {
                skeleton.SetSkin(newSkin);
                logger.Log($"스킨 설정");
            }
            else
            {
                logger.Log($"[SpineInitializer] Skin '{skinName}' 을(를) 찾을 수 없습니다.");
            }
        }

        // 스킨 바꾼 뒤 슬롯을 세팅 포즈로 돌려줘야 눈/장비 등이 정상 표시됨
        skeleton.SetSlotsToSetupPose();

        // --- 애니메이션 변경 ---
        if (!string.IsNullOrEmpty(animationName))
        {
            TrackEntry entry = skeletonAnimation.AnimationState.SetAnimation(0, animationName, loop);
            if (entry == null)
            {
                logger.Log($"[SpineInitializer] Animation '{animationName}' 을(를) 찾을 수 없습니다.");
            }
            else
            {
                logger.Log($"애니 설정");
            }
        }
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        skeletonAnimation = GetComponent<SkeletonAnimation>();

        // Spine 초기화가 안 되어 있으면 강제로 초기화
        if (!skeletonAnimation.valid)
        {
            // true : 초기화 이벤트(Complete, Start 등)도 다시 세팅
            skeletonAnimation.Initialize(true);
        }

        // ApplySkinAndAnimation();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
