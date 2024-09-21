using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NpcManager : Singleton<NpcManager>
{
    public List<NpcCharacterActorBase> npcCharacters = new();

    public override void Init()
    {
        base.Init();

        npcCharacters = Object.FindObjectsOfType<NpcCharacterActorBase>().ToList();
        npcCharacters.ForEach(e =>
        {
            e.Init();
        });
    }
}