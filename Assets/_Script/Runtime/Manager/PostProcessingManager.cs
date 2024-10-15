using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class PostProcessingManager : Singleton<PostProcessingManager>
{
    public Volume globalVolume;
    public DepthOfField depthOfField;
    public ColorAdjustments colorAdjustments;
    public Bloom bloom;
    
    public ChromaticAberration ChromaticAberration;
    public Vignette Vignette;

    public override void Init()
    {
        base.Init();

        globalVolume = GameObject.FindAnyObjectByType<Volume>();
        VolumeProfile profile = globalVolume.profile;

        profile.TryGet(out depthOfField);
        profile.TryGet(out Vignette);
        profile.TryGet(out ChromaticAberration);

        SetDepthOfField(new BlurSetting());
        SetVignette(new VignetteSetting());
    }


    public void SetActive(bool active)
    {
        globalVolume.enabled = active;
    }

    public class BlurSetting
    {
        public bool active = false;
    }
    public void SetDepthOfField(BlurSetting setting)
    {
        depthOfField.active = setting.active;
    }

    public class VignetteSetting
    {
        public bool actice = false;
    }
    public void SetVignette(VignetteSetting setting)
    {
        Vignette.active = setting.actice;
    }

    public class ColorAdjustmentSetting
    {
        public bool active = false;
        public float postExposure = 0.0f; 
        public float contrast = 0.0f; 
        public Color colorFilter = Color.white;
        public float hueShift = 0.0f;
        public float saturation = 0.0f; 
    }
    public void SetColorAdjustments(ColorAdjustmentSetting setting)
    {
        if (colorAdjustments != null)
        {
            colorAdjustments.active = setting.active;
        }
    }

    public class BloomSetting
    {
        public bool active = false;
        public float intensity = 0.0f; 
        public float threshold = 1.0f; 
    }

    public void SetBloom(BloomSetting setting)
    {
        if (bloom != null)
        {
            bloom.active = setting.active;
        }
    }

    public void SetChromaticAberration(bool active)
    {
        ChromaticAberration.active = active;
    }
}
