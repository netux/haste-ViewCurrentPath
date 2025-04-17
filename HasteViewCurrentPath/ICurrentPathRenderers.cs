using UnityEngine;

namespace HasteViewCurrentPath;

public interface ICurrentPathRenderers
{
    public void Setup(RectTransform parentTransform);

    public bool CanBeUsed();

    public void Render(IEnumerable<PathAggregator.PathNode> aggregatedNodes, bool showEllipsis);
}
