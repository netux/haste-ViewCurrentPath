using Landfall.Modding;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using Zorro.Core.CLI;

namespace HasteViewCurrentPath;

[LandfallPlugin]
public class ViewCurrentPath
{
    public static EscapeMenuCurrentPath? CurrentPathComponent;
    public static bool IsHooked { get => CurrentPathComponent != null; }
    
    public static ICurrentPathRenderer? Renderer { get; private set; }

    static ViewCurrentPath()
    {
        On.EscapeMenuMainPage.OnPageEnter += static (original, escapeMenuMainPage) =>
        {
            original(escapeMenuMainPage);

            if (CurrentPathComponent == null || CurrentPathComponent.gameObject == null)
            {
                CurrentPathComponent = CreateEscapeMenuCurrentPathComponent((RectTransform)escapeMenuMainPage.transform);
            }

            if (Renderer == null)
            {
                var rendererType = GameHandler.Instance.SettingsHandler.GetSetting<Settings.RendererSetting>().SelectedRendererType;
                SetRenderer(rendererType);
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

    private static void SetupRenderer()
    {
        if (CurrentPathComponent == null)
        {
            throw new AssertionException("CurrentPathComponent == null", "Current Path component has not been instantiated");
        }

        if (Renderer == null)
        {
            throw new AssertionException("Renderer == null", "No Current Path Renderer has been configured");
        }

        Renderer.Setup((RectTransform)CurrentPathComponent.transform);
    }

    public static void SetRenderer<T>() where T : ICurrentPathRenderer, new() => SetRenderer(typeof(T));

    public static void SetRenderer(Type rendererType)
    {
        Debug.Log($"{MethodBase.GetCurrentMethod().DeclaringType.Name}: Setting up new renderer: {rendererType.Name}");

        Renderer?.Dispose();
        Renderer = (ICurrentPathRenderer)rendererType.GetConstructor([]).Invoke([]);
        SetupRenderer();
    }

    [ConsoleCommand]
    public static void Render()
    {
        CurrentPathComponent?.Render();
    }
}
