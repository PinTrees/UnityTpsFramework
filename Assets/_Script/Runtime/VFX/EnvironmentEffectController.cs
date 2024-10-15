using UnityEngine;


public class EnvironmentEffectController : MonoBehaviour
{
    public GameObject baseObject;

    public void SetFire()
    {
        var fireEffect = VfxObject.Create<VfxFire>("");
        fireEffect.transform.position = baseObject.transform.position;
        fireEffect.transform.rotation = baseObject.transform.rotation;

        fireEffect.SetFireSetting(null, null);
    }
}
