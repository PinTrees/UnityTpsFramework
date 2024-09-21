using UnityEngine;

public class HitboxViwer : MonoBehaviour
{
    public CharacterActorBase ownerCharacter;
    public HitboxDataTree hitboxTree;

#if UNITY_EDITOR
    public void OnDrawGizmos() 
    {
        if (ownerCharacter == null)
            return;

        hitboxTree.DrawGizmo(ownerCharacter);
    }
#endif
}
