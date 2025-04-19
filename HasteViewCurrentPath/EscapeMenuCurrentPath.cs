using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;

namespace HasteViewCurrentPath;

public class EscapeMenuCurrentPath : MonoBehaviour
{
    public static int MAX_NODES = 5;

    public ICurrentPathRenderer? Renderer { get => ViewCurrentPath.Renderer; }

    public void OnPageEnter()
    {
        if (Renderer == null)
        {
            throw new AssertionException("Renderer == null", "No Current Path Renderer has been configured");
        }

        if (Renderer.NeedsSetup())
        {
            // FAILSAFE: for some reason, objects created by the renderers might be removed after on page enter?
            Debug.Log($"{MethodBase.GetCurrentMethod().DeclaringType.Name}: Failsafe 1 triggered!");
            Renderer.Setup((RectTransform) transform);
        }

        try
        {
            Render();
        }
        catch (Exception)
        {
            // FAILSAFE: and even after that, maybe it needs _yet another_ setup?
            Debug.Log($"{MethodBase.GetCurrentMethod().DeclaringType.Name}: Failsafe 2 triggered!");

            Renderer.Setup((RectTransform)transform);
            Render();
        }
    }

    public void Render()
    {
        Render(RunHandler.RunData.QueuedNodes);
    }

    public void Render(IEnumerable<LevelSelectionNode.Data> queuedNodes)
    {
        if (Renderer == null)
        {
            throw new AssertionException("Renderer == null", "No Current Path Renderer has been configured");
        }

        var aggregateResult = PathAggregator.Aggregate(queuedNodes, MAX_NODES);

        Renderer.Render(aggregateResult.Nodes, showEllipsis: aggregateResult.HasMore);
    }
}
