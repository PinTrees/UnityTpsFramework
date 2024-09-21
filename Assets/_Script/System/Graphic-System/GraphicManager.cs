using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;

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

        SetDynamicResolution(true);
        SetRenderScale(renderScale); 
    }

    private void SetDynamicResolution(bool enable)
    {
        HDRenderPipelineAsset hdrpAsset = (HDRenderPipelineAsset)GraphicsSettings.defaultRenderPipeline;
        if (hdrpAsset != null)
        {
            var dynamicResolutionSettings = hdrpAsset.currentPlatformRenderPipelineSettings.dynamicResolutionSettings;
            dynamicResolutionSettings.enabled = enable;
            Debug.Log($"Dynamic Resolution enabled: {enable}");
        }
        else
        {
            Debug.LogError("HDRP Asset not found!");
        }
    }

    private void SetRenderScale(float scale)
    {
        if (scale < 0.5f || scale > 1.0f)
        {
            Debug.LogError("Render scale should be between 0.5 and 1.0");
            return;
        }

        ScalableBufferManager.ResizeBuffers(scale, scale);
        Debug.Log($"Render Scale set to {scale}");
    }
}
