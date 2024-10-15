using UnityEngine;
using UnityEngine.VFX;

public class VfxObject : MonoBehaviour
{
    [Header("Component Setting")]
    public VisualEffect visualEffect;

    [Header("Life Setting")]
    public float lifeDuration = 0.5f;
    public bool useManualRelease = false;

    [Header("Effect Setting")]
    public bool lookCamera;

    [Header("Runtime Value")]
    private float spawnTime;
    private bool isReleased = false;
    private bool isSpawned = false;
    private float emission = 0;

    public static T Create<T>(string tag) where T : VfxObject
    {
        var vfx = ObjectPoolManager.Instance.GetC<VfxObject>(tag);
        vfx.Init();
        vfx.Enter();
        return vfx as T;
    }

    public static VfxObject Create(string tag)
    {
        var vfx = ObjectPoolManager.Instance.GetC<VfxObject>(tag);
        vfx.Init();
        vfx.Enter();
        return vfx; 
    }

    private bool isInit = false;
    public void Init()
    {
        if (isInit)
            return;
        isInit = true;

        OnInit();
    }

    protected virtual void OnInit()
    {
        if(visualEffect != null)
        {
            emission = visualEffect.GetFloat("Emission");
        }
    }

    public virtual void Enter()
    {
        isReleased = false;
        spawnTime = Time.time;

        if (lookCamera)
            transform.LookAt(Camera.main.transform, Vector3.up);

        if(visualEffect)
        {
            visualEffect.SetFloat("Emission", emission);
        }

        gameObject.SetActive(true);

        isSpawned = true;
    }

    public virtual void Release()
    {
        if (isReleased)
            return;
        isReleased = true;
        isSpawned = false;

        ObjectPoolManager.Instance.Release(gameObject.name, this.gameObject); 
    }

    public void Play()
    {
        if (visualEffect == null)
            return;

        visualEffect.Play();
    }

    public void Stop()
    {
        if (visualEffect == null)
            return;

        visualEffect.Stop();
    }

    public void SetFloat(string tag, float v)
    {
        if (visualEffect == null)
            return;

        visualEffect.SetFloat(tag, v);
    }

    public void Update()
    {
        if(isSpawned && spawnTime + lifeDuration < Time.time)
        {
            Stop();
            Release();
        }

        if(lookCamera)
        {
            transform.LookAt(Camera.main.transform, Vector3.up);
        }
    }
}
