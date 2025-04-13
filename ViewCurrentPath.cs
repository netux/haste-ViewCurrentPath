using Landfall.Modding;
using TMPro;
using UnityEngine;

namespace HasteViewCurrentPath;

[LandfallPlugin]
public class ViewCurrentPath
{
    public static EscapeMenuCurrentPath? CurrentPathHandler;

    static ViewCurrentPath()
    {
        On.EscapeMenuMainPage.Start += static (original, escapeMenuMainPage) =>
        {
            original(escapeMenuMainPage);

            CurrentPathHandler = CreateEscapeMenuCurrentPathComponent((RectTransform) escapeMenuMainPage.transform);
        };

        On.EscapeMenuMainPage.OnPageEnter += static (original, escapeMenuMainPage) =>
        {
            original(escapeMenuMainPage);

            CurrentPathHandler?.OnPageEnter();
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

        var currentPathText = new GameObject("Text", [typeof(TextMeshProUGUI)]).GetComponent<TextMeshProUGUI>();
        {
            currentPathText.gameObject.transform.SetParent(currentPathTransform, worldPositionStays: false);

            var cancelPathButtonText = cancelPathButton.Find("Text").GetComponent<TextMeshProUGUI>();

            currentPathText.font = cancelPathButtonText.font;
            currentPathText.fontMaterial = cancelPathButtonText.fontMaterial;
            currentPathText.fontSharedMaterial = cancelPathButtonText.fontSharedMaterial;
            currentPathText.enableWordWrapping = false;
            currentPathText.overflowMode = TextOverflowModes.Overflow;
            currentPathText.verticalAlignment = VerticalAlignmentOptions.Middle;

            var textTransform = (RectTransform) currentPathText.gameObject.transform;
            textTransform.localScale = Vector3.one;
            textTransform.position = Vector3.zero;
            textTransform.localPosition = Vector3.zero;
            textTransform.pivot = Vector2.one / 2;
            textTransform.anchorMin = Vector2.zero;
            textTransform.anchorMax = Vector2.one;
            textTransform.sizeDelta = Vector2.zero;
        }

        var currentPathComponent = currentPathGameObject.AddComponent<EscapeMenuCurrentPath>();
        currentPathComponent.text = currentPathText;
        return currentPathComponent;
    }
}

public class EscapeMenuCurrentPath : MonoBehaviour
{
    public TextMeshProUGUI? text;

    public void OnPageEnter()
    {
        Render();
    }

    public void Render()
    {
        Render(RunHandler.RunData.QueuedNodes);
    }

    public void Render(IEnumerable<LevelSelectionNode.Data> queuedNodes)
    {
        if (text == null)
        {
            throw new Exception("EscapeMenuCurrentPath text is null");
        }

        if (queuedNodes.Count() == 0)
        {
            text.gameObject.SetActive(false);
        }
        else
        {
            text.gameObject.SetActive(true);
            text.text = $"UP NEXT: {GenerateText(queuedNodes)}";
        }

        // TODO(netux): graphics!
        //foreach (var queuedNode in RunHandler.RunData.QueuedNodes)
        //{
        //    // M_VFX_Node_Icon_Store
        //    // mapicon_Shop 1.png
        //    //Resources.Load
        //}
    }

    internal string GenerateText(IEnumerable<LevelSelectionNode.Data> queuedNodes)
    {
        List<string> aggregatedQueuedNodeTypes = [];
        bool hasMore = false;

        LevelSelectionNode.NodeType? lastNodeType = null;
        int repeatedNodeTypeCount = 0;

        foreach (var queuedNode in queuedNodes)
        {
            if (lastNodeType.HasValue)
            {
                if (lastNodeType == queuedNode.Type)
                {
                    repeatedNodeTypeCount++;
                }
                else
                {
                    if (aggregatedQueuedNodeTypes.Count >= 5)
                    {
                        hasMore = true;
                        break;
                    }
            
                    aggregatedQueuedNodeTypes.Add(GenerateAggregatedNodeTypeText(lastNodeType.Value, repeatedNodeTypeCount));

                    repeatedNodeTypeCount = 0;
                }
            }

            lastNodeType = queuedNode.Type;
        }

        if (lastNodeType.HasValue && repeatedNodeTypeCount > 0 && !hasMore)
        {
            aggregatedQueuedNodeTypes.Add(GenerateAggregatedNodeTypeText(lastNodeType.Value, repeatedNodeTypeCount));
        }

        var result = string.Join(" → ", aggregatedQueuedNodeTypes);
        if (hasMore)
        {
            result += " → …";
        }
        return result;
    }

    internal string GenerateAggregatedNodeTypeText(LevelSelectionNode.NodeType nodeType, int repeatCount)
    {
        string result = GetNodeTypePrettyName(nodeType);
        if (repeatCount > 0)
        {
            result += $" x{repeatCount + 1}";
        }
        return result;
    }

    internal string GetNodeTypePrettyName(LevelSelectionNode.NodeType nodeType)
    {
        switch (nodeType)
        {
            case LevelSelectionNode.NodeType.Default:
                return "Fragment";
            case LevelSelectionNode.NodeType.Shop:
                return "Shop";
            case LevelSelectionNode.NodeType.Challenge:
            case LevelSelectionNode.NodeType.Encounter:
                return "?";
            case LevelSelectionNode.NodeType.RestStop:
                return "Rest Stop";
            case LevelSelectionNode.NodeType.Boss:
                return "Boss";
            default:
                return nodeType.ToString();
        }
    }
}