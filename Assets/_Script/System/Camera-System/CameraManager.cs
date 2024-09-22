using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public enum CameraUpdateType
{
    Update,
    FixedUpdate,
    LateUpdate,
}

[Serializable]
public class CameraShakeData
{
    public float positionIntensity = 0.5f;  // ��ġ ����ũ ����
    public float rotationIntensity = 1f;    // ȸ�� ����ũ ����
    public float zoomInAmount  = 0.5f;      // ���� ����
    public float frequency = 5;             // �ֱ�, �ʴ� �� ȸ
}

public class CameraManager : Singleton<CameraManager>
{
    // base setting data
    private const float DEFAULT_TRANSITION_DURATION = 2.5f;
    public CameraUpdateType updateType;

    [Space]
    // runtima value
    public List<CameraBase> cameras = new();
    public CameraBase currentActiveCamera;

    // component
    private Camera mainCamera;
    private Coroutine transitionCorutine;


    protected override void Awake()
    {
        base.Awake();
    }


    public override void Init()
    {
        base.Init();

        mainCamera = Camera.main;

        cameras = Object.FindObjectsByType<CameraBase>(FindObjectsSortMode.None).ToList();
        cameras.ForEach(e => e.Init());
        cameras.ForEach(e => e.TurnOffCamera());

        ChangeCamera(cameras.First());
    }

    public void UpdateCameraTransform()
    {
        if (transitionCorutine != null)
            return;

        if (currentActiveCamera != null)
        {
            mainCamera.transform.position = currentActiveCamera.transform.position;
            mainCamera.transform.rotation = currentActiveCamera.transform.rotation;

            mainCamera.fieldOfView = currentActiveCamera.fieldOfView;

            // ����ũ ������ ����
            foreach (var shakeData in shakeOffsetMap)
            {
                Vector3 eulerAngleOffset = shakeData.Value.Item1;
                Vector3 positionOffset = shakeData.Value.Item2;
                float distanceOffset = shakeData.Value.Item3;

                // ȸ�� �����°� ��ġ �������� ī�޶� ����
                mainCamera.transform.position += positionOffset;
                mainCamera.transform.rotation *= Quaternion.Euler(eulerAngleOffset);

                mainCamera.transform.position += currentActiveCamera.transform.forward * distanceOffset;
            }
        }
    }

    Dictionary<string, (Vector3, Vector3, float)> shakeOffsetMap = new();
    public async UniTaskVoid ShakeCamera(CameraShakeData shakeData, float duration)
    {
        string guid = System.Guid.NewGuid().ToString();

        Vector3 eulerAngleOffset = Vector3.zero;
        Vector3 positionOffset = Vector3.zero;
        float zoomInOffset = 0.0f;
        shakeOffsetMap[guid] = (eulerAngleOffset, positionOffset, zoomInOffset);

        float elapsed = 0f;

        // Perlin noise seed �� ����
        float noiseSeedX = Random.Range(0f, 100f);
        float noiseSeedY = Random.Range(0f, 100f);
        float noiseSeedZ = Random.Range(0f, 100f);
        float noiseSeedRotation = Random.Range(0f, 100f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // ������ �������� ����ũ ���� ���
            float shakeIntensity = Mathf.Lerp(1f, 0f, elapsed / duration);
            float t = elapsed / duration;
            float zoomIntensity = 4 * t * (1 - t);

            // �ֱ�(frequency)�� ������� Perlin Noise ���
            float frequencyFactor = elapsed * shakeData.frequency;

            // �ε巯�� ������ ��� (Perlin Noise ���, �ֱ� ����)
            float noiseX = (Mathf.PerlinNoise(noiseSeedX + frequencyFactor, elapsed) - 0.5f) * 2f;
            float noiseY = (Mathf.PerlinNoise(noiseSeedY + frequencyFactor, elapsed) - 0.5f) * 2f;
            float noiseZ = (Mathf.PerlinNoise(noiseSeedZ + frequencyFactor, elapsed) - 0.5f) * 2f;
            float noiseRotation = (Mathf.PerlinNoise(noiseSeedRotation + frequencyFactor, elapsed) - 0.5f) * 2f;

            // �������� �ε巴�� ����
            eulerAngleOffset = new Vector3(
                noiseRotation * shakeData.rotationIntensity * shakeIntensity,
                noiseRotation * shakeData.rotationIntensity * shakeIntensity,
                noiseRotation * shakeData.rotationIntensity * shakeIntensity
            );

            positionOffset = new Vector3(
                noiseX * shakeData.positionIntensity * shakeIntensity,
                noiseY * shakeData.positionIntensity * shakeIntensity,
                noiseZ * shakeData.positionIntensity * shakeIntensity
            );

            zoomInOffset = Mathf.Lerp(0, shakeData.zoomInAmount, zoomIntensity);

            // ����ũ ������ ����
            shakeOffsetMap[guid] = (eulerAngleOffset, positionOffset, zoomInOffset);

            // ���� �����ӱ��� ���
            await UniTask.Yield();
        }

        // �ڿ������� 0���� �������
        float remainingTime = 0.2f;
        while (remainingTime > 0f)
        {
            remainingTime -= Time.deltaTime;
            float t = remainingTime / 0.2f;

            shakeOffsetMap[guid] = (
                Vector3.Lerp(Vector3.zero, eulerAngleOffset, t),
                Vector3.Lerp(Vector3.zero, positionOffset, t),
                0);

            await UniTask.Yield();
        }

        // ������ ����
        shakeOffsetMap.Remove(guid);
    }

    IEnumerator TransitionCamera(float duration=DEFAULT_TRANSITION_DURATION)
    {
        if (currentActiveCamera == null)
        {
            transitionCorutine = null;
            yield break;
        }

        float startTransitionTime = Time.unscaledTime;

        while (Time.unscaledTime <= (startTransitionTime + duration))
        {
            if (currentActiveCamera == null)
            {
                transitionCorutine = null;
                yield break;
            }

            float lerp = (Time.unscaledTime - startTransitionTime) / duration;
            SetupCameraLerp(mainCamera, currentActiveCamera, lerp);

            yield return null;
        }

        // force setting
        SetupCameraLerp(mainCamera, currentActiveCamera, lerp: 1f);

        transitionCorutine = null;
    }

    public void ChangeCamera(string tag, float duration=DEFAULT_TRANSITION_DURATION)
    {
        var camera = cameras.Where(e => e.cameraTag == tag).FirstOrDefault();
        if (camera == null)
            return;

        if (currentActiveCamera != null)
        {
            currentActiveCamera.TurnOffCamera();
            if (camera is ThirdPersonCamera tpc && currentActiveCamera is ThirdPersonCamera activeTpc)
            {
                tpc.transform.position = activeTpc.transform.position;
                tpc.lookDirection = activeTpc.lookDirection;
                tpc.currentRotationX = activeTpc.currentRotationX;
                tpc.currentRotationY = activeTpc.currentRotationY;
            }
        }

        camera.TurnOnCamera();
        currentActiveCamera = camera;

        if (transitionCorutine != null) StopCoroutine(transitionCorutine);
        transitionCorutine = StartCoroutine(TransitionCamera(duration));
    }

    public void ChangeCamera(CameraBase camera)
    {
        if (currentActiveCamera != null)
        {
            currentActiveCamera.TurnOffCamera();
        }

        camera.TurnOnCamera();
        currentActiveCamera = camera;

        if (transitionCorutine != null) StopCoroutine(transitionCorutine);
        transitionCorutine = StartCoroutine(TransitionCamera());
    }

    public T ChangeCameraOnly<T>(string cameraTag = "") where T : CameraBase
    {
        if (currentActiveCamera != null)
        {
            currentActiveCamera.TurnOffCamera();
        }

        var targetCamera = GetCamera<T>(cameraTag);
        if (targetCamera != null)
        {
            targetCamera.TurnOnCamera();
            if (currentActiveCamera != null)
            {
            }
        }

        currentActiveCamera = targetCamera;

        if (transitionCorutine != null) StopCoroutine(transitionCorutine);
        transitionCorutine = StartCoroutine(TransitionCamera());

        return targetCamera;
    }

    public T GetCamera<T>(string cameraTag = "") where T : CameraBase
    {
        if (cameraTag == "")
        {
            cameraTag = typeof(T).FullName;
        }

        Debug.Log(cameraTag);

        var find = cameras.FirstOrDefault(e => e.cameraTag == cameraTag);

        if (find == null)
            return null;

        return find as T;
    }

    private void SetupCameraLerp(Camera targetCamera, CameraBase setting, float lerp)
    {
        targetCamera.transform.position = Vector3.Lerp(targetCamera.transform.position, setting.transform.position, lerp);
        targetCamera.transform.rotation = Quaternion.Lerp(targetCamera.transform.rotation, setting.transform.rotation, lerp);

        targetCamera.fieldOfView = Mathf.Lerp(targetCamera.fieldOfView, setting.fieldOfView, lerp);
        targetCamera.nearClipPlane = Mathf.Lerp(targetCamera.nearClipPlane, setting.nearClipPlane, lerp);
        targetCamera.farClipPlane = Mathf.Lerp(targetCamera.farClipPlane, setting.farClipPlane, lerp);
    }


    protected void Update()
    {
        if (updateType == CameraUpdateType.Update)
            UpdateCameraTransform();
    }

    protected void FixedUpdate()
    {
        if (updateType == CameraUpdateType.FixedUpdate)
            UpdateCameraTransform();
    }

    protected void LateUpdate()
    {
        if (updateType == CameraUpdateType.LateUpdate)
            UpdateCameraTransform();
    }
}
