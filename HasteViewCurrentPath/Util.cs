using TMPro;
using UnityEngine;

namespace HasteViewCurrentPath;

internal static class Util
{
    private static TextMeshProUGUI? referenceTMP;

    static internal TextMeshProUGUI GetReferenceTextMeshPro(Transform escapeMenuMainMenuButtonsTransform)
    {
        if (referenceTMP == null) {
            referenceTMP = escapeMenuMainMenuButtonsTransform.Find("CancelPath/Text").GetComponent<TextMeshProUGUI>();
        }

        return referenceTMP;
    }


    // Thanks https://stackoverflow.com/a/44734346 !
    static internal Texture2D DuplicateTexture2DAsReadable(Texture2D originalTexture)
    {
        RenderTexture renderTexture = RenderTexture.GetTemporary(
            originalTexture.width,
            originalTexture.height,
            depthBuffer: 0,
            format: RenderTextureFormat.Default,
            readWrite: RenderTextureReadWrite.Linear
        );
        UnityEngine.Graphics.Blit(originalTexture, renderTexture);

        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTexture;

        Texture2D readableTexture = new(originalTexture.width, originalTexture.height);
        readableTexture.ReadPixels(
            source: new Rect(0, 0, renderTexture.width, renderTexture.height),
            destX: 0,
            destY: 0
        );
        readableTexture.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTexture);

        return readableTexture;
    }

    static internal System.Drawing.Image ConvertTexture2DToImage(Texture2D texture)
    {
        byte[] pngData = texture.isReadable
            ? texture.EncodeToPNG()
            : DuplicateTexture2DAsReadable(texture).EncodeToPNG();

        using MemoryStream memoryStream = new(pngData);
        return System.Drawing.Image.FromStream(memoryStream);
    }

    static internal Texture2D ConvertImageToTexture2D(System.Drawing.Image image)
    {
        using MemoryStream memoryStream = new();

        image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);

        memoryStream.Position = 0;

        Texture2D result = new(2, 2);
        result.LoadImage(memoryStream.ToArray());

        return result;
    }
}
