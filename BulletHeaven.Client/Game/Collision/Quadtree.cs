namespace BulletHeaven.Client.Game.Collision;

/// <summary>
/// Spatial partitioning index for broad-phase collision (QUAD-001).
/// Per-frame usage: <see cref="Clear"/> + <see cref="Insert"/> every active entity,
/// then <see cref="Query"/> per projectile and circle-check only the candidates.
/// All nodes come from an internal pool reused across frames, so steady-state
/// operation allocates nothing — the pool only grows when a frame needs more
/// nodes than any frame before it.
/// Items inserted outside the root boundary stay at the root and are returned
/// by every query, so off-screen entities (e.g. freshly spawned enemies) are
/// never missed by edge collisions — only checked slightly more often.
/// </summary>
public sealed class Quadtree<T> where T : class, ICollidable
{
    private const int DefaultNodeCapacity = 8;
    private const int MaxDepth = 6;

    private sealed class Node
    {
        public double MinX, MinY, MaxX, MaxY;
        public int Depth;
        public bool IsSplit;
        public readonly List<T> Items = new(DefaultNodeCapacity + 1);
        public Node? Nw, Ne, Sw, Se;
    }

    private readonly int _capacity;
    private readonly List<Node> _nodePool;
    private int _nodesUsed;
    private Node _root;

    public Quadtree(double x, double y, double width, double height, int capacity = DefaultNodeCapacity)
    {
        _capacity = capacity;
        _nodePool = new List<Node>(64);
        for (var i = 0; i < 64; i++) _nodePool.Add(new Node());
        _root = Rent(x, y, width, height, 0);
    }

    /// <summary>Resets the tree for a new frame. Bounds are re-supplied so a viewport resize is picked up.</summary>
    public void Clear(double x, double y, double width, double height)
    {
        _nodesUsed = 0;
        _root = Rent(x, y, width, height, 0);
    }

    public void Insert(T item) => Insert(_root, item);

    /// <summary>
    /// Broad-phase query: fills <paramref name="results"/> (cleared first) with every
    /// candidate whose node touches the bounding box of the circle (x, y, radius).
    /// Callers must still run the precise circle check on each candidate.
    /// </summary>
    public void Query(double x, double y, double radius, List<T> results)
    {
        results.Clear();
        // The root is scanned unconditionally so out-of-bounds stragglers are never missed.
        CollectItems(_root, results);
        if (!_root.IsSplit) return;
        var minX = x - radius;
        var minY = y - radius;
        var maxX = x + radius;
        var maxY = y + radius;
        QueryNode(_root.Nw!, minX, minY, maxX, maxY, results);
        QueryNode(_root.Ne!, minX, minY, maxX, maxY, results);
        QueryNode(_root.Sw!, minX, minY, maxX, maxY, results);
        QueryNode(_root.Se!, minX, minY, maxX, maxY, results);
    }

    private void Insert(Node node, T item)
    {
        if (node.IsSplit)
        {
            var child = ChildFullyContaining(node, item);
            if (child != null) { Insert(child, item); return; }
            node.Items.Add(item); // straddles a child boundary — keep at this level
            return;
        }

        node.Items.Add(item);
        if (node.Items.Count > _capacity && node.Depth < MaxDepth)
            Split(node);
    }

    private void Split(Node node)
    {
        var midX = (node.MinX + node.MaxX) / 2;
        var midY = (node.MinY + node.MaxY) / 2;
        var halfW = midX - node.MinX;
        var halfH = midY - node.MinY;
        var childDepth = node.Depth + 1;
        node.Nw = Rent(node.MinX, node.MinY, halfW, halfH, childDepth);
        node.Ne = Rent(midX, node.MinY, halfW, halfH, childDepth);
        node.Sw = Rent(node.MinX, midY, halfW, halfH, childDepth);
        node.Se = Rent(midX, midY, halfW, halfH, childDepth);
        node.IsSplit = true;

        for (var i = node.Items.Count - 1; i >= 0; i--)
        {
            var child = ChildFullyContaining(node, node.Items[i]);
            if (child == null) continue; // straddler stays here
            child.Items.Add(node.Items[i]);
            node.Items.RemoveAt(i);
        }
    }

    private Node Rent(double x, double y, double width, double height, int depth)
    {
        if (_nodesUsed == _nodePool.Count) _nodePool.Add(new Node()); // rare: only on a new all-time high
        var n = _nodePool[_nodesUsed++];
        n.MinX = x;
        n.MinY = y;
        n.MaxX = x + width;
        n.MaxY = y + height;
        n.Depth = depth;
        n.IsSplit = false;
        n.Items.Clear();
        n.Nw = n.Ne = n.Sw = n.Se = null;
        return n;
    }

    private static Node? ChildFullyContaining(Node node, T item)
    {
        if (Contains(node.Nw!, item)) return node.Nw;
        if (Contains(node.Ne!, item)) return node.Ne;
        if (Contains(node.Sw!, item)) return node.Sw;
        if (Contains(node.Se!, item)) return node.Se;
        return null;
    }

    private static bool Contains(Node n, T item) =>
        item.X - item.Radius >= n.MinX && item.X + item.Radius <= n.MaxX &&
        item.Y - item.Radius >= n.MinY && item.Y + item.Radius <= n.MaxY;

    private static void QueryNode(Node node, double minX, double minY, double maxX, double maxY, List<T> results)
    {
        if (node.MaxX < minX || node.MinX > maxX || node.MaxY < minY || node.MinY > maxY) return;
        CollectItems(node, results);
        if (!node.IsSplit) return;
        QueryNode(node.Nw!, minX, minY, maxX, maxY, results);
        QueryNode(node.Ne!, minX, minY, maxX, maxY, results);
        QueryNode(node.Sw!, minX, minY, maxX, maxY, results);
        QueryNode(node.Se!, minX, minY, maxX, maxY, results);
    }

    private static void CollectItems(Node node, List<T> results)
    {
        for (var i = 0; i < node.Items.Count; i++) results.Add(node.Items[i]);
    }
}
