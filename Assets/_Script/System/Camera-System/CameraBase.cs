using System.Linq;
using UnityEngine;

// 해당 카메라는 가상 카메라로 카메라가 가져야할 세팅을 설정하는 카메라 인스턴스 컨테이너입니다.
// 런타임에 카메라 매니저를 통해 해당 데이터로 트랜지션합니다.
public class CameraBase : MonoBehaviour
{
    public string cameraTag = "";

    public CameraUpdateType updateType;

    // data
    [Space]
    public float fieldOfView = 60;
    public float nearClipPlane = 0.01f;
    public float farClipPlane = 1000f;
    public float distance = 3.5f;

    [Header("Runtime Value")]
    public Vector3 lookDirection;

    private bool _init = false;


    // 초기화 메서드
    public void Init()
    {
        if (_init)
            return;

        OnInit();

        _init = true;
    }

    // 재정의 가능 초기화 메서드
    protected virtual void OnInit()
    {
    }

    protected virtual void OnUpdate()
    {

    }

    public virtual void TurnOnCamera()
    {
        gameObject.SetActive(true);
        //Debug.Log("Camera TurnOn");
    }

    public virtual void TurnOffCamera()
    {
        gameObject.SetActive(false);
        //Debug.Log("Camera TurnOff");
    }

    private void OnDrawGizmos()
    {
    }

    protected virtual void Update()
    {
        if (updateType == CameraUpdateType.Update)
            OnUpdate();
    }

    protected virtual void LateUpdate()
    {
        if (updateType == CameraUpdateType.LateUpdate)
            OnUpdate();
    }

    protected virtual void FixedUpdate()
    {
        if (updateType == CameraUpdateType.FixedUpdate)
            OnUpdate();
    }
}
