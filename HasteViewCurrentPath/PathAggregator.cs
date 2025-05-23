namespace HasteViewCurrentPath;

public static class PathAggregator
{
    public class PathNode
    {
        public LevelSelectionNode.NodeType Type;
        public int RepeatCount { get; internal set; } = 0;

        public void IncreaseRepeatCount() => RepeatCount++;
    }

    public class AggregateResult
    {
        public readonly List<PathNode> Nodes = [];
        public bool HasMore { get; internal set; } = false;
    }

    public static AggregateResult Aggregate(IEnumerable<LevelSelectionNode.Data> nodes, int maxNodes)
    {
        return Aggregate(nodes.Select(node => node.type), maxNodes);
    }

    public static AggregateResult Aggregate(IEnumerable<LevelSelectionNode.NodeType> nodeTypes, int maxNodes)
    {
        var result = new AggregateResult();

        foreach (var nodeType in nodeTypes)
        {
            if (result.Nodes.Count > 0)
            {
                var lastAggregatedNode = result.Nodes.Last();
                if (lastAggregatedNode.Type == nodeType)
                {
                    lastAggregatedNode.IncreaseRepeatCount();
                    continue;
                }
            }

            if (result.Nodes.Count >= maxNodes)
            {
                result.HasMore = true;
                break;
            }

            result.Nodes.Add(new PathNode
            {
                Type = nodeType
            });
        }

        return result;
    }
}
