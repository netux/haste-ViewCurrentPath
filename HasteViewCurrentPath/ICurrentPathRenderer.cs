using UnityEngine;

namespace HasteViewCurrentPath;

public interface ICurrentPathRenderer
{
    public bool NeedsSetup();

    public void Setup(RectTransform parentTransform);

    public void Dispose();

    public void Render(IEnumerable<PathAggregator.PathNode> aggregatedNodes, bool showEllipsis);
}
