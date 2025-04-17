using Landfall.Modding;
using TMPro;
using UnityEngine;
using Zorro.Core.CLI;

namespace HasteViewCurrentPath;

[LandfallPlugin]
public class ViewCurrentPath
{
    public static EscapeMenuCurrentPath? CurrentPathComponent;

    static ViewCurrentPath()
    {
        On.EscapeMenuMainPage.OnPageEnter += static (original, escapeMenuMainPage) =>
        {
            original(escapeMenuMainPage);

            if (CurrentPathComponent == null || CurrentPathComponent.gameObject == null)
            {
                CurrentPathComponent = CreateEscapeMenuCurrentPathComponent((RectTransform)escapeMenuMainPage.transform);
            }

            CurrentPathComponent?.OnPageEnter();
        };
    }

    static EscapeMenuCurrentPath CreateEscapeMenuCurrentPathComponent(RectTransform escapeMenuMainPageTransform)
    {
        var currentPathGameObject = new GameObject("CurrentPath", [typeof(RectTransform)]);
        var currentPathTransform = currentPathGameObject.GetComponent<RectTransform>();

        var cancelPathButton = escapeMenuMainPageTransform.Find("Buttons/CancelPath");
        var cancelPathButtonTransform = (RectTransform) cancelPathButton.transform;

        currentPathTransform.SetParent(escapeMenuMainPageTransform.Find("Buttons"), worldPositionStays: false);
        currentPathTransform.SetSiblingIndex(Math.Max(cancelPathButton.GetSiblingIndex() - 1, 0));

        currentPathTransform.sizeDelta = cancelPathButtonTransform.sizeDelta;
        currentPathTransform.localScale = Vector3.one;

        var currentPathComponent = currentPathGameObject.AddComponent<EscapeMenuCurrentPath>();
        return currentPathComponent;
    }
}

public class EscapeMenuCurrentPath : MonoBehaviour
{
    public static int MAX_NODES = 5;

    public ICurrentPathRenderers[] renderers = [
        new CurrentPathTextRenderer(),
    ];

    public void Start()
    {
        SetupRenderers();
    }

    private void SetupRenderers()
    {
        foreach (var renderer in renderers)
        {
            renderer.Setup((RectTransform) this.gameObject.transform);
        }
    }

    public void OnPageEnter()
    {
        try
        {
            Render();
        }
        catch (Exception)
        {
            // FAILSAFE: for some reason, any objects created by the renderers might be removed after on page enter?

            Debug.Log($"{typeof(EscapeMenuCurrentPath).Name}: Failsafe triggered!");

            SetupRenderers();
            Render();
        }
    }

    [ConsoleCommand]
    public void Render()
    {
        Render(RunHandler.RunData.QueuedNodes);
    }

    public void Render(IEnumerable<LevelSelectionNode.Data> queuedNodes)
    {
        var aggregateResult = PathAggregator.Aggregate(queuedNodes, MAX_NODES);

        var firstAvailableRenderer = renderers.FirstOrDefault(renderer => renderer.CanBeUsed());

        if (firstAvailableRenderer == null)
        {
            throw new Exception("No renderer available to render current node path");
        }

        firstAvailableRenderer.Render(aggregateResult.Nodes, showEllipsis: aggregateResult.HasMore);
    }
}
