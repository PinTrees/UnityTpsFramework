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
        CameraManager.Instance.ChangeCamera("PlayerMainCamera");

        NavigationSystem.Instance.Init();

        // Character Create
        NpcManager.Instance.Init();
        PlayerManager.Instance.Init();
    }
}
