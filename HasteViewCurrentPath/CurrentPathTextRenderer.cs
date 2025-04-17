using TMPro;
using UnityEngine;

namespace HasteViewCurrentPath;

public class CurrentPathTextRenderer : ICurrentPathRenderers
{
    public TextMeshProUGUI? text;

    public void Setup(RectTransform parentTransform)
    {
        if (text != null)
        {
            Debug.Log($"CurrentPathTextRenderer Setup: text was not null");
            return;
        }

        text = new GameObject("Text", [typeof(TextMeshProUGUI)]).GetComponent<TextMeshProUGUI>();
        text.gameObject.transform.SetParent(parentTransform, worldPositionStays: false);

        var referenceText = parentTransform.Find("../CancelPath/Text").GetComponent<TextMeshProUGUI>();

        text.font = referenceText.font;
        text.fontMaterial = referenceText.fontMaterial;
        text.fontSharedMaterial = referenceText.fontSharedMaterial;
        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Overflow;
        text.verticalAlignment = VerticalAlignmentOptions.Middle;

        var textTransform = (RectTransform)text.gameObject.transform;
        textTransform.localScale = Vector3.one;
        textTransform.position = Vector3.zero;
        textTransform.localPosition = Vector3.zero;
        textTransform.pivot = Vector2.one / 2;
        textTransform.anchorMin = Vector2.zero;
        textTransform.anchorMax = Vector2.one;
        textTransform.sizeDelta = Vector2.zero;
    }

    public bool CanBeUsed() {
        Debug.Log($"CurrentPathTextRenderer CanBeUsed. text == null? {text == null}");

        return text != null;
    }

    public void Render(IEnumerable<PathAggregator.PathNode> nodes, bool showEllipsis)
    {
        if (text == null)
        {
            throw new Exception("CurrentPathTextRenderer's text is null. Cannot render path.");
        }

        if (nodes.Count() == 0)
        {
            text.gameObject.SetActive(false);
        }
        else
        {
            text.gameObject.SetActive(true);

            var pathString = string.Join(" → ", nodes.Select(GeneratePathNodeText));
            if (showEllipsis)
            {
                pathString += " → …";
            }

            text.text = $"UP NEXT: {pathString}";
        }
    }

    internal string GeneratePathNodeText(PathAggregator.PathNode pathNode)
    {
        string result = GetNodeTypePrettyName(pathNode.Type);
        if (pathNode.Count > 0)
        {
            result += $" x{pathNode.Count + 1}";
        }
        return result;
    }

    internal string GetNodeTypePrettyName(LevelSelectionNode.NodeType nodeType)
    {
        return nodeType switch
        {
            LevelSelectionNode.NodeType.Default => "Fragment",
            LevelSelectionNode.NodeType.Shop => "Shop",
            LevelSelectionNode.NodeType.Challenge or LevelSelectionNode.NodeType.Encounter => "Unknown",
            LevelSelectionNode.NodeType.RestStop => "Rest",
            LevelSelectionNode.NodeType.Boss => "Boss",
            _ => nodeType.ToString(),
        };
    }
}