using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using Zorro.Core;

namespace HasteViewCurrentPath;

public class CurrentPathIconRenderer : ICurrentPathRenderer
{
    private readonly CurrentPathTextRenderer fallbackTextRenderer = new();

    private readonly Dictionary<Texture2D, Material> textureToMaterialMap = [];

    private GameObject? container;

    public bool NeedsSetup() => !NodeTextures.AllTexturesArePresent() || fallbackTextRenderer.NeedsSetup();

    public void Setup(RectTransform parentTransform)
    {
        fallbackTextRenderer.Setup(parentTransform);

        NodeTextures.FindFromLoadedResources();

        if (container == null)
        {
            container = new GameObject("Graphic Container", [typeof(RectTransform), typeof(HorizontalLayoutGroup)]);
            container.transform.SetParent(parentTransform, worldPositionStays: false);

            var containerHorizontalLayoutGroup = container.GetComponent<HorizontalLayoutGroup>();
            containerHorizontalLayoutGroup.childControlWidth = false;
            containerHorizontalLayoutGroup.childControlHeight = true;
            containerHorizontalLayoutGroup.childForceExpandWidth = true;
            containerHorizontalLayoutGroup.childForceExpandHeight = true;
            containerHorizontalLayoutGroup.childScaleWidth = containerHorizontalLayoutGroup.childScaleHeight = false;
            containerHorizontalLayoutGroup.spacing = 10f;

            var containerTransform = (RectTransform)container.transform;
            containerTransform.localScale = Vector3.one;
            containerTransform.sizeDelta = ((RectTransform)container.transform.parent.transform).sizeDelta;
            containerTransform.position = Vector3.zero;
            containerTransform.offsetMin = Vector3.zero;
            containerTransform.offsetMax = Vector3.zero;
            containerTransform.anchorMin = new Vector2(0, 1);
            containerTransform.anchorMax = new Vector2(0, 1);
        }
    }

    public void Dispose()
    {
        fallbackTextRenderer.Dispose();

        if (container != null)
        {
            GameObject.Destroy(container);
        }

        foreach (var material in textureToMaterialMap.Values)
        {
            GameObject.Destroy(material);
        }
        textureToMaterialMap.Clear();

        NodeTextures.Reset();
    }

    public void Render(IEnumerable<PathAggregator.PathNode> nodes, bool showEllipsis)
    {
        if (container == null)
        {
            throw new AssertionException("container == null", "CurrentPathGraphicRenderer's container is null. Cannot render path.");
        }

        if (!NodeTextures.AllTexturesArePresent())
        {
            Debug.LogWarning("CurrentPathIconRenderer: Not all textures are present, falling back to text renderer.");

            container?.SetActive(false);
            fallbackTextRenderer.Render(nodes, showEllipsis);
            fallbackTextRenderer.text?.gameObject.SetActive(true);
            return;
        }

        container.transform.ClearChildren(); // Thanks Zorro!

        int index = 0;
        foreach (var node in nodes)
        {
            if (index == 0)
            {
                var prefixText = NewTextMeshProWithText("Prefix Text", "UP NEXT:");
                prefixText.margin = new Vector4(0, 0, 10, 0); // right padding to separate it from the path itself
                prefixText.transform.SetParent(container.transform, worldPositionStays: false);
            }
            else
            {
                var transitionText = NewTextMeshProWithText($"Node {index} Transition Text", "→");
                transitionText.verticalAlignment = VerticalAlignmentOptions.Bottom;
                transitionText.transform.SetParent(container.transform, worldPositionStays: false);
            }

            Texture2D? nodeTexture = NodeTextures.GetTextureForNodeType(node.Type);
            if (nodeTexture != null)
            {
                var nodeImage = NewImageWithMaterial($"Node {index} ({node.Type}) Image", GetMaterialForTexture($"{node.Type} Node Material", nodeTexture));
                nodeImage.transform.SetParent(container.transform, worldPositionStays: false);
            }
            else
            {
                // Shouldn't happen as long as Landfall doesn't add a new node type...
                var nodeText = NewTextMeshProWithText($"Node {index} Text", node.Type.ToString());
                nodeText.transform.SetParent(container.transform, worldPositionStays: false);
            }

            if (node.RepeatCount > 0)
            {
                var repeatCountText = NewTextMeshProWithText($"Node {index} Count", $"×{node.RepeatCount + 1}");
                repeatCountText.fontSize = repeatCountText.fontSize * 0.8f;
                repeatCountText.transform.SetParent(container.transform, worldPositionStays: false);
            }

            index++;
        }

        if (showEllipsis)
        {
            var ellipsisText = NewTextMeshProWithText($"Ellipsis Text", "→ ...");
            ellipsisText.verticalAlignment = VerticalAlignmentOptions.Bottom;
            ellipsisText.transform.SetParent(container.transform, worldPositionStays: false);
        }

        fallbackTextRenderer.text?.gameObject.SetActive(false);
        container.SetActive(true);

        GameHandler.Instance.StartCoroutine(MarkContainerLayoutForRebuildASAP());
    }

    private IEnumerator MarkContainerLayoutForRebuildASAP()
    {
        if (container == null)
        {
            throw new AssertionException("container == null after a frame", "Container became null a after a frame of rendering");
        }

        LayoutRebuilder.MarkLayoutForRebuild((RectTransform)container.transform);

        while (!container.activeInHierarchy)
        {
            yield return null;
        }

        LayoutRebuilder.MarkLayoutForRebuild((RectTransform)container.transform);

        yield return null;

        LayoutRebuilder.MarkLayoutForRebuild((RectTransform)container.transform);
    }

    private Material GetMaterialForTexture(string name, Texture2D texture)
    {
        if (!textureToMaterialMap.TryGetValue(texture, out Material material))
        {
            material = NewMaterialWithTexture(name, texture);
            textureToMaterialMap.Add(texture, material);
        }
        return material;
    }

    private Material NewMaterialWithTexture(string name, Texture2D texture)
    {
        return new Material(Shader.Find("UI/Default"))
        {
            name = name,
            mainTexture = texture
        };
    }

    private Image NewImageWithMaterial(string name, Material material)
    {
        var imageGameObject = new GameObject(name, [typeof(RectTransform), typeof(Image), typeof(AspectRatioFitter)]);

        var imageImage = imageGameObject.GetComponent<Image>();
        imageImage.material = material;

        var imageAspectRatioFitter = imageGameObject.GetComponent<AspectRatioFitter>();
        imageAspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;

        var imageTransform = (RectTransform)imageGameObject.transform;
        imageTransform.sizeDelta = new Vector2(imageTransform.sizeDelta.y, imageTransform.sizeDelta.y);

        return imageImage;
    }

    private TextMeshProUGUI NewTextMeshProWithText(string name, string content)
    {
        if (container == null)
        {
            throw new AssertionException("container == null", "Graphics Container not found");
        }

        var referenceTextMeshPro = Util.GetReferenceTextMeshPro(container.transform.parent.parent);

        var text = new GameObject(name, [typeof(TextMeshProUGUI), typeof(ContentSizeFitter)]);

        var textTransform = (RectTransform)text.gameObject.transform;
        textTransform.localScale = Vector3.one;
        textTransform.pivot = Vector2.one / 2;
        textTransform.anchorMin = Vector2.zero;
        textTransform.anchorMax = Vector2.zero;

        var textTextMeshPro = text.GetComponent<TextMeshProUGUI>();
        textTextMeshPro.font = referenceTextMeshPro.font;
        textTextMeshPro.fontMaterial = referenceTextMeshPro.fontMaterial;
        textTextMeshPro.fontSharedMaterial = referenceTextMeshPro.fontSharedMaterial;
        textTextMeshPro.enableWordWrapping = false;
        textTextMeshPro.verticalAlignment = VerticalAlignmentOptions.Middle;
        textTextMeshPro.text = content;

        var textContentSizeFitter = text.GetComponent<ContentSizeFitter>();
        textContentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        textContentSizeFitter.SetLayoutHorizontal();

        return textTextMeshPro;
    }

    private class NodeTextures
    {
        private class GenerationConfig
        {
            internal LevelSelectionNode.NodeType nodeType;
            internal float scale = 1f;
            internal bool mergeWithBase = true;
        }

        private static readonly Dictionary<string, GenerationConfig> FindAndGenerateConfigs = new()
        {
            ["mapicon_Shop 1"] = new GenerationConfig()
            {
                nodeType = LevelSelectionNode.NodeType.Shop,
                scale = 0.5f,
            },
            ["mapicon_EtheralSpikes"] = new GenerationConfig()
            {
                nodeType = LevelSelectionNode.NodeType.Challenge,
                scale = 1.3f,
            },
            ["mapicon_Encounter 1"] = new GenerationConfig()
            {
                nodeType = LevelSelectionNode.NodeType.Encounter,
                scale = 0.5f,
            },
            ["mapicon_Rest 1"] = new GenerationConfig()
            {
                nodeType = LevelSelectionNode.NodeType.RestStop,
                scale = 0.5f,
            },
            ["mapicon_FinalBoss"] = new GenerationConfig()
            {
                nodeType = LevelSelectionNode.NodeType.Boss,
                scale = 0.5f,
                mergeWithBase = false,
            },
        };

        private static Texture2D? _baseTexture;
        internal static Texture2D BaseTexture
        {
            get {
                if (_baseTexture == null)
                {
                    _baseTexture = CreateBaseTexture();
                }

                return _baseTexture;
            }
        }
        
        private static readonly Dictionary<LevelSelectionNode.NodeType, Texture2D> generatedTextures = [];

        internal static void FindFromLoadedResources()
        {
            foreach (Texture2D texture in Resources.FindObjectsOfTypeAll<Texture2D>())
            {
                if (!FindAndGenerateConfigs.TryGetValue(texture.name, out GenerationConfig generationConfig))
                {
                    continue;
                }

                Debug.Log($"CurrentPathGraphicRenderer.NodeTextures found texture {texture.name} (for node type {generationConfig.nodeType})");

                var generatedTexture = generationConfig.mergeWithBase
                    ? MergeTextureWithBase(texture, generationConfig.scale)
                    : Util.DuplicateTexture2DAsReadable(texture);

                generatedTexture.name = $"escapemenupath_{generationConfig.nodeType}";

                generatedTextures[generationConfig.nodeType] = generatedTexture;
            }
        }

        internal static bool AllTexturesArePresent()
        {
            foreach (var config in FindAndGenerateConfigs.Values)
            {
                if (!generatedTextures.ContainsKey(config.nodeType) || generatedTextures[config.nodeType] == null)
                {
                    Debug.Log($"CurrentPathGraphicRenderer.NodeTextures texture for {config.nodeType} not present!");
                    return false;
                }
            }

            return true;
        }

        internal static Texture2D? GetTextureForNodeType(LevelSelectionNode.NodeType nodeType)
        {
            if (nodeType == LevelSelectionNode.NodeType.Default)
            {
                return BaseTexture;
            }

            return generatedTextures[nodeType];
        }

        internal static void Reset()
        {
            _baseTexture = null;

            foreach (var texture in generatedTextures.Values)
            {
                GameObject.Destroy(texture);
            }
            generatedTextures.Clear();
        }

        private static Texture2D CreateBaseTexture()
        {
            const int SIZE = 64;
            const float RADIUS = SIZE / 2;
            const float LINE_WIDTH = SIZE * (1f - 0.05f);
            const float OUTER_RADIUS_SQ = RADIUS * RADIUS;
            const float INNER_RADIUS_SQ = (RADIUS - LINE_WIDTH) * (RADIUS - LINE_WIDTH);

            var texture = new Texture2D(SIZE, SIZE)
            {
                name = "escapemenupath_BaseCircle"
            };

            Color colorTransparent = new(0, 0, 0, 0);

            for (int x = 0; x < SIZE; x++)
            {
                for (int y = 0; y < SIZE; y++)
                {
                    float distanceSq = (x - RADIUS) * (x - RADIUS) + (y - RADIUS) * (y - RADIUS);

                    Color color = distanceSq >= INNER_RADIUS_SQ && distanceSq <= OUTER_RADIUS_SQ
                        ? Color.white
                        : colorTransparent;

                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply();

            return texture;
        }

        private static Texture2D MergeTextureWithBase(Texture2D overlayTexture, float scale)
        {
            var baseImage = Util.ConvertTexture2DToImage(BaseTexture);
            var overlayImage = Util.ConvertTexture2DToImage(overlayTexture);

            // When scale < 1f, scale overlay down to fit inside base circle.
            // When scale > 1f, scale base down to fit inside overlay.
            // Always use baseImage's original size as the output size.

            System.Drawing.Bitmap resultImage = new(baseImage.Width, baseImage.Height);
            using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(resultImage))
            {
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                var baseNewWidth = scale > 1f ? (int)Math.Floor(baseImage.Width * (1f - (scale - 1f))) : baseImage.Width;
                var baseNewHeight = scale > 1f ? (int)Math.Floor(baseImage.Height * (1f - (scale - 1f))) : baseImage.Height;
                graphics.DrawImage(
                    baseImage,
                    new System.Drawing.Rectangle(
                        x: (int)Math.Floor((baseImage.Width - baseNewWidth) / 2f),
                        y: (int)Math.Floor((baseImage.Height - baseNewHeight) / 2f),
                        width: baseNewWidth,
                        height: baseNewHeight
                    )
                );

                var overlayNewWidth = scale < 1f ? (int)Math.Floor(baseImage.Width * scale) : baseImage.Width;
                var overlayNewHeight = scale < 1f ? (int)Math.Floor(baseImage.Height * scale) : baseImage.Height;
                graphics.DrawImage(
                    overlayImage,
                    new System.Drawing.Rectangle(
                        x: (int)Math.Floor((baseImage.Width - overlayNewWidth) / 2f),
                        y: (int)Math.Floor((baseImage.Height - overlayNewHeight) / 2f),
                        width: overlayNewWidth,
                        height: overlayNewHeight
                    )
                );
            }

            return Util.ConvertImageToTexture2D(resultImage);

        }
    }
}