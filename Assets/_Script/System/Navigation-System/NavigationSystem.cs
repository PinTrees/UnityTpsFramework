using Unity.AI.Navigation;
using UnityEngine;

public class NavigationSystem : Singleton<NavigationSystem>
{
    [Header("Default Setting")]
    public NavMeshSurface navMeshSurface;

    public override void Init()
    {
        base.Init();

        navMeshSurface.BuildNavMesh();
    }
}
