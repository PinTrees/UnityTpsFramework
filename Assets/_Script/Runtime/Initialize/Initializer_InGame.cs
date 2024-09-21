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
        InputManager.Instance.Init();
        GraphicManager.Instance.Init();
        //await UniTask.Yield();

        ObjectPoolManager.Instance.Init();
        VfxObjectManager.Instance.Init();

        LegsAnimatorEx.Init();
        FLookAnimatorEx.Init();
        AnimatorLayerWeightEx.Init();

        CameraManager.Instance.Init();
        CameraManager.Instance.ChangeCamera("PlayerMainCamera");

        // Character Create
        NpcManager.Instance.Init();
        PlayerManager.Instance.Init();
    }
}
