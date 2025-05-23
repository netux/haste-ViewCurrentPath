using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;

namespace HasteViewCurrentPath;

public class EscapeMenuCurrentPath : MonoBehaviour
{
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
            Debug.LogWarning($"{MethodBase.GetCurrentMethod().DeclaringType.Name}: Failsafe 1 triggered!");
            Renderer.Setup((RectTransform) transform);
        }

        try
        {
            Render();
        }
        catch (Exception)
        {
            // FAILSAFE: and even after that, maybe it needs _yet another_ setup?
            Debug.LogWarning($"{MethodBase.GetCurrentMethod().DeclaringType.Name}: Failsafe 2 triggered!");

            try
            {
                Renderer.Setup((RectTransform)transform);
                Render();
            }
            catch (Exception exception)
            {
                Debug.LogError("Gave up trying to render current path. Got an exception:");
                Debug.LogError(exception);
            }
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

        var maxNodes = GameHandler.Instance.SettingsHandler.GetSetting<Settings.MaxNodesRenderedSetting>().Value;
        var aggregateResult = PathAggregator.Aggregate(queuedNodes, maxNodes);

        Renderer.Render(aggregateResult.Nodes, showEllipsis: aggregateResult.HasMore);
    }
}
