using UnityEngine;

public class Hitable : MonoBehaviour
{
    public CharacterActorBase ownerCharacter;

    [Header("Data")]
    public LayerMask hitboxLayerMask;


    public void Init()
    {

    }
}
