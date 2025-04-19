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
}
