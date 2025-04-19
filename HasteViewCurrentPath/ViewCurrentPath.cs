using Landfall.Modding;
using UnityEngine;
using UnityEngine.UI;
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
        var currentPathGameObject = new GameObject("CurrentPath", [typeof(RectTransform), typeof(HorizontalLayoutGroup)]);
        var currentPathTransform = currentPathGameObject.GetComponent<RectTransform>();

        var cancelPathButton = escapeMenuMainPageTransform.Find("Buttons/CancelPath");
        var cancelPathButtonTransform = (RectTransform) cancelPathButton.transform;

        currentPathTransform.SetParent(escapeMenuMainPageTransform.Find("Buttons"), worldPositionStays: false);
        currentPathTransform.SetSiblingIndex(Math.Max(cancelPathButton.GetSiblingIndex() - 1, 0));

        currentPathTransform.sizeDelta = cancelPathButtonTransform.sizeDelta;
        currentPathTransform.localScale = Vector3.one;

        var currentPathHorizontalLayoutGroup = currentPathGameObject.GetComponent<HorizontalLayoutGroup>();
        currentPathHorizontalLayoutGroup.childAlignment = TextAnchor.MiddleLeft;
        currentPathHorizontalLayoutGroup.childControlWidth = false;
        currentPathHorizontalLayoutGroup.childControlHeight = true;
        currentPathHorizontalLayoutGroup.childScaleWidth = true;
        currentPathHorizontalLayoutGroup.childScaleHeight = true;
        currentPathHorizontalLayoutGroup.childForceExpandWidth = false;
        currentPathHorizontalLayoutGroup.childForceExpandHeight = true;
        currentPathHorizontalLayoutGroup.spacing = 10f;

        var currentPathComponent = currentPathGameObject.AddComponent<EscapeMenuCurrentPath>();
        return currentPathComponent;
    }
}

public class EscapeMenuCurrentPath : MonoBehaviour
{
    public static int MAX_NODES = 5;

    public ICurrentPathRenderer Renderer { get; private set; } = new CurrentPathIconRenderer();

    public void Start()
    {
        SetupRenderer();
    }

    private void SetupRenderer()
    {
        Renderer.Setup((RectTransform)transform);
    }

    public void SetRenderer<T>() where T : ICurrentPathRenderer, new()
    {
        Renderer?.Dispose();

        Renderer = new T();
        SetupRenderer();
    }

    public void OnPageEnter()
    {
        if (Renderer.NeedsSetup())
        {
            // FAILSAFE: for some reason, objects created by the renderers might be removed after on page enter?
            Debug.Log($"{typeof(EscapeMenuCurrentPath).Name}: Failsafe 1 triggered!");
            Renderer.Setup((RectTransform) transform);
        }

        try
        {
            Render();
        }
        catch (Exception)
        {
            // FAILSAFE: and even after that, maybe it needs _yet another_ setup?
            Debug.Log($"{typeof(EscapeMenuCurrentPath).Name}: Failsafe 2 triggered!");

            Renderer.Setup((RectTransform)transform);
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

        Renderer.Render(aggregateResult.Nodes, showEllipsis: aggregateResult.HasMore);
    }
}
