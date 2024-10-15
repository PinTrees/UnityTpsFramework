using UnityEngine;

public class VfxFire : VfxObject
{
    public ParticleSystem debriParticleSystem;
    public ParticleSystem flameParticleSystem;

    public void SetFireSetting(MeshRenderer debriShape, MeshRenderer flameShape)
    {
        var devriParticleShape = debriParticleSystem.shape;
        devriParticleShape.mesh = debriShape.GetComponent<MeshFilter>().sharedMesh;

        var flameParticleShape = flameParticleSystem.shape;
        flameParticleShape.mesh = flameShape.GetComponent<MeshFilter>().sharedMesh;
    }
}
