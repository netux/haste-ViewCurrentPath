using TMPro;
using UnityEngine;

namespace HasteViewCurrentPath;

public class CurrentPathTextRenderer : ICurrentPathRenderer
{
    public TextMeshProUGUI? text;

    public bool NeedsSetup() => text == null;

    public void Setup(RectTransform parentTransform)
    {
        if (text == null)
        {
            text = new GameObject("Text", [typeof(TextMeshProUGUI)]).GetComponent<TextMeshProUGUI>();
            text.gameObject.transform.SetParent(parentTransform, worldPositionStays: false);

            var referenceText = Util.GetReferenceTextMeshPro(parentTransform.parent);

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
    }

    public void Dispose()
    {
        if (text != null)
        {
            GameObject.Destroy(text.gameObject);
        }
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
        if (pathNode.RepeatCount > 0)
        {
            result += $" x{pathNode.RepeatCount + 1}";
        }
        return result;
    }

    internal string GetNodeTypePrettyName(LevelSelectionNode.NodeType nodeType)
    {
        return nodeType switch
        {
            LevelSelectionNode.NodeType.Default => "Fragment",
            LevelSelectionNode.NodeType.Shop => "Shop",
            LevelSelectionNode.NodeType.Challenge => "Challenge",
            LevelSelectionNode.NodeType.Encounter => "Encounter",
            LevelSelectionNode.NodeType.RestStop => "Rest",
            LevelSelectionNode.NodeType.Boss => "Boss",
            _ => nodeType.ToString(), // Shouldn't happen as long as Landfall doesn't add a new node type...
        };
    }
}