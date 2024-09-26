using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct GizmoLineDataContainer
{
    public Vector3 startPos;
    public Vector3 endPos;
}

public class GizmosSystem : Singleton<GizmosSystem>
{
    public GameObject lineRendererContainer;

    public Queue<GizmoLineDataContainer> inGameLineGizmoDataRenderQueue = new();
    public List<LineRenderer> lineRenderers = new();

    public override void Init()
    {
        base.Init();
        lineRendererContainer = new GameObject("GizmoSystem-LineRendererContainer");
        for(int i = 0; i < 50; ++i)
        {
            var lineRenderer = new GameObject("LineRenderer").AddComponent<LineRenderer>();
            lineRenderer.transform.SetParent(lineRendererContainer.transform, true);
            lineRenderers.Add(lineRenderer);
        }
    }

    public void DrawLine(Vector3 startPos, Vector3 endPos)
    {
        inGameLineGizmoDataRenderQueue.Enqueue(new GizmoLineDataContainer()
        {
            startPos = startPos,
            endPos = endPos,
        });
    }

    public void LateUpdate()
    {
        lineRenderers.ForEach(e =>
        {
            if(inGameLineGizmoDataRenderQueue.Count <= 0)
            {
                e.enabled = false;
                return;
            }

            var data = inGameLineGizmoDataRenderQueue.Dequeue();
            e.SetPosition(0, data.startPos);
            e.SetPosition(1, data.endPos);
            e.enabled = true;
        });

        inGameLineGizmoDataRenderQueue.Clear();
    }
}
