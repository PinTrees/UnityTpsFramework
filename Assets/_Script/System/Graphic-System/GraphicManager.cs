using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine;

public class GraphicManager : Singleton<GraphicManager>
{
    public int targetFrameRate = 60;
    [Range(0.5f, 1)]
    public float renderScale = 0.5f;

    public override void Init()
    {
        base.Init();

        Application.targetFrameRate = targetFrameRate;
        QualitySettings.vSyncCount = 0;
    }
}
