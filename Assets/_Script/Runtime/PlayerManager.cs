using UnityEngine;

public class PlayerManager : Singleton<PlayerManager>
{
    public PlayerCharacterActorBase playerCharacter;

    public override void Init()
    {
        base.Init();

        if(playerCharacter == null)
        {
            playerCharacter = FindFirstObjectByType<PlayerCharacterActorBase>();
        }

        playerCharacter.Init();
    }
}
