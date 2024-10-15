using Unity.AI.Navigation;
using UnityEngine;

public class NavigationSystem : Singleton<NavigationSystem>
{
    [Header("Default Setting")]
    public NavMeshSurface navMeshSurface;

    public Transform targetTransform;

    public override void Init()
    {
        base.Init();

        transform.position = targetTransform.position;
        navMeshSurface.BuildNavMesh();
    }
}
