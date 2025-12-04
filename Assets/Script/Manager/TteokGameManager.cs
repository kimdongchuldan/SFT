using Foundation;
using UnityEngine;

public partial class TteokGameManager : MonoBehaviour
{
    private static readonly ILogger log = Debug.unityLogger;

    void Start()
    {
        InitStepState();
    }

    void Update()
    {
        step.Update(); 
    }

    void OnDestroy()
    {
        HoverItemManager.Clear();
        DroppableManager.Clear();
        
    }
}
