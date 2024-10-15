using Cysharp.Threading.Tasks;
using UnityEngine;

public class Initializer_InGame : MonoBehaviour
{
    void Start()
    {
        Init().Forget();
    }

    // 절차적 초기화
    protected async UniTaskVoid Init()
    {
        PostProcessingManager.Instance.Init();

        DetecteSystem.Init();
        InputManager.Instance.Init();
        GraphicManager.Instance.Init();
        TaskSystem.Instance.Init();
        GizmosSystem.Instance.Init();
        //await UniTask.Yield();

        ObjectPoolManager.Instance.Init();
        VfxObjectManager.Instance.Init();

        LegsAnimatorEx.Init();
        FLookAnimatorEx.Init();
        AnimatorLayerWeightEx.Init();

        CameraManager.Instance.Init();

        try
        {
            NavigationSystem.Instance.Init();
        }
        catch { }

        // Character Create
        PlayerManager.Instance.Init();
        NpcManager.Instance.Init();
    }
}
