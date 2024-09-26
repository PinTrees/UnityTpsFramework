using UnityEngine;

public class PlayerInputController : MonoBehaviour
{
    // Runtime Value
    private PlayerCharacterActorBase ownerCharacter;

    public void Init(PlayerCharacterActorBase owner)
    {
        ownerCharacter = owner;
    }
}
