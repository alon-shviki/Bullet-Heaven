using BulletHeaven.Client.Game;
using BulletHeaven.Client.Game.Collision;

namespace BulletHeaven.Tests;

/// <summary>
/// xUnit tests for <see cref="Quadtree{T}"/> (QUAD-001 broad-phase index).
/// Covers: happy path (insert/query, spatial pruning after splits), boundary
/// conditions (capacity threshold, out-of-bounds items, degenerate same-point
/// inserts vs MaxDepth), expected contracts (results cleared, no duplicates,
/// Clear + pooled-node reuse), and a seeded randomized completeness sweep
/// (broad-phase must never miss a circle that could overlap the query circle).
/// Uses a private ICollidable stub — never touches the static GameBounds.
/// </summary>
public class QuadtreeTests
{
    // Minimal ICollidable stub so tests don't depend on entity construction details.
    private sealed class Circle : ICollidable
    {
        public double X { get; init; }
        public double Y { get; init; }
        public double Radius { get; init; }
    }

    private static Circle At(double x, double y, double radius = 1) =>
        new() { X = x, Y = y, Radius = radius };

    // ── Happy Path ────────────────────────────────────────────────────────────

    // Happy Path: a lone inserted item is returned when querying at its position.
    [Fact]
    public void Query_AtInsertedItemPosition_ReturnsItem()
    {
        var tree = new Quadtree<Circle>(0, 0, 800, 600);
        var item = At(400, 300, 10);
        tree.Insert(item);

        var results = new List<Circle>();
        tree.Query(400, 300, 5, results);

        Assert.Contains(item, results);
    }

    // Happy Path: a zero-radius query at the exact item point still returns it.
    [Fact]
    public void Query_ZeroRadiusAtExactPoint_ReturnsItem()
    {
        var tree = new Quadtree<Circle>(0, 0, 800, 600);
        var item = At(100, 100, 8);
        tree.Insert(item);

        var results = new List<Circle>();
        tree.Query(100, 100, 0, results);

        Assert.Contains(item, results);
    }

    // Happy Path: after a split (capacity exceeded in one corner), a query in the
    // opposite corner is pruned and returns nothing — the broad phase actually narrows.
    [Fact]
    public void Query_OppositeCornerAfterSplit_ReturnsEmpty()
    {
        var tree = new Quadtree<Circle>(0, 0, 800, 600, capacity: 8);
        // 9 items clustered in the NW corner force a split; all fit fully in the NW child.
        for (var i = 0; i < 9; i++)
            tree.Insert(At(20 + i * 5, 20 + i * 5, 2));

        var results = new List<Circle>();
        tree.Query(780, 580, 10, results);

        Assert.Empty(results);
    }

    // Happy Path: all clustered items remain reachable when querying their own corner after a split.
    [Fact]
    public void Query_SameCornerAfterSplit_ReturnsAllClusteredItems()
    {
        var tree = new Quadtree<Circle>(0, 0, 800, 600, capacity: 8);
        var items = new List<Circle>();
        for (var i = 0; i < 9; i++)
        {
            var c = At(20 + i * 5, 20 + i * 5, 2);
            items.Add(c);
            tree.Insert(c);
        }

        var results = new List<Circle>();
        tree.Query(40, 40, 60, results);

        foreach (var c in items)
            Assert.Contains(c, results);
    }

    // ── Boundary Conditions ───────────────────────────────────────────────────

    // Boundary: exactly `capacity` items do not split (all stay at the root, so a
    // far-away query still sees them); capacity + 1 splits (far query goes empty).
    // Observes the split threshold purely through public Query behaviour.
    [Theory]
    [InlineData(4)]
    [InlineData(8)]
    public void Insert_CapacityThreshold_SplitsOnlyWhenExceeded(int capacity)
    {
        var atCapacity = new Quadtree<Circle>(0, 0, 800, 600, capacity);
        var overCapacity = new Quadtree<Circle>(0, 0, 800, 600, capacity);
        for (var i = 0; i < capacity; i++)
        {
            atCapacity.Insert(At(20 + i * 3, 20 + i * 3, 1));
            overCapacity.Insert(At(20 + i * 3, 20 + i * 3, 1));
        }
        overCapacity.Insert(At(25, 25, 1)); // one past capacity → split

        var results = new List<Circle>();
        atCapacity.Query(780, 580, 5, results);
        Assert.Equal(capacity, results.Count); // unsplit root: every item is a candidate

        overCapacity.Query(780, 580, 5, results);
        Assert.Empty(results); // split: far corner is pruned
    }

    // Boundary: an item outside the root bounds (off-screen spawn, x = -50) is
    // returned by every query — even a far-corner query after the tree has split.
    [Fact]
    public void Insert_OutsideRootBounds_ReturnedByEveryQueryAfterSplit()
    {
        var tree = new Quadtree<Circle>(0, 0, 800, 600, capacity: 8);
        var offscreen = At(-50, 300, 14);
        tree.Insert(offscreen);
        for (var i = 0; i < 9; i++) // force a split far from the off-screen item
            tree.Insert(At(700 + i * 4, 500 + i * 4, 2));

        var results = new List<Circle>();
        tree.Query(790, 590, 5, results); // far SE corner
        Assert.Contains(offscreen, results);

        tree.Query(10, 10, 5, results); // NW corner
        Assert.Contains(offscreen, results);
    }

    // Boundary (degenerate split): more-than-capacity items at the exact same point
    // can never separate into children — MaxDepth must stop recursion (no infinite
    // loop / stack overflow) and a query at that point must return every item.
    [Fact]
    public void Insert_AllItemsAtSamePoint_DoesNotRecurseForeverAndQueryReturnsAll()
    {
        var tree = new Quadtree<Circle>(0, 0, 800, 600, capacity: 8);
        const int count = 40;
        var items = new List<Circle>();
        for (var i = 0; i < count; i++)
        {
            var c = At(123, 456, 3);
            items.Add(c);
            tree.Insert(c); // would recurse forever without the MaxDepth guard
        }

        var results = new List<Circle>();
        tree.Query(123, 456, 1, results);

        Assert.Equal(count, results.Count);
        foreach (var c in items)
            Assert.Contains(c, results);
    }

    // ── Query Contract ────────────────────────────────────────────────────────

    // Contract: Query clears the results list first — stale entries from a previous
    // frame must not survive.
    [Fact]
    public void Query_PrePopulatedResultsList_StaleEntriesRemoved()
    {
        var tree = new Quadtree<Circle>(0, 0, 800, 600);
        var item = At(50, 50, 5);
        tree.Insert(item);

        var stale = At(999, 999, 1);
        var results = new List<Circle> { stale, stale, stale };
        tree.Query(50, 50, 10, results);

        Assert.DoesNotContain(stale, results);
        Assert.Single(results);
        Assert.Same(item, results[0]);
    }

    // Contract: no item appears twice in the results, even with boundary straddlers
    // kept at parent nodes and a query circle covering the whole tree.
    [Fact]
    public void Query_CoveringWholeTreeWithStraddlers_ReturnsNoDuplicates()
    {
        var tree = new Quadtree<Circle>(0, 0, 800, 600, capacity: 4);
        var items = new List<Circle>
        {
            At(400, 300, 50), // straddles the root midpoint — stays at root
            At(200, 300, 30), // straddles the NW/SW boundary
        };
        for (var i = 0; i < 12; i++) // plus enough items to force several splits
            items.Add(At(30 + i * 60, 40 + (i % 5) * 100, 5));
        foreach (var c in items)
            tree.Insert(c);

        var results = new List<Circle>();
        tree.Query(400, 300, 1000, results); // covers everything

        Assert.Equal(results.Count, results.Distinct().Count());
        Assert.Equal(items.Count, results.Count);
    }

    // ── Clear / Pooled-Node Reuse ─────────────────────────────────────────────

    // Contract: Clear empties the tree — a query that previously matched returns nothing.
    [Fact]
    public void Clear_AfterInserts_QueryReturnsNothing()
    {
        var tree = new Quadtree<Circle>(0, 0, 800, 600);
        tree.Insert(At(100, 100, 10));
        tree.Insert(At(200, 200, 10));

        tree.Clear(0, 0, 800, 600);

        var results = new List<Circle>();
        tree.Query(100, 100, 500, results);
        Assert.Empty(results);
    }

    // Contract (pooled-node correctness): force splits, Clear with different bounds,
    // re-insert — queries reflect only the new items (no stale items from reused
    // nodes) and pruning still works against the new bounds.
    [Fact]
    public void Clear_WithNewBounds_ReusedNodesHoldNoStaleItemsAndStayCorrect()
    {
        var tree = new Quadtree<Circle>(0, 0, 800, 600, capacity: 4);
        for (var i = 0; i < 20; i++) // first frame: force splits so child nodes get items
            tree.Insert(At(10 + i * 35, 10 + (i % 4) * 140, 3));

        tree.Clear(100, 100, 400, 400); // second frame: new viewport bounds

        var fresh = new List<Circle>();
        for (var i = 0; i < 6; i++) // exceed capacity again so reused nodes are revisited
        {
            var c = At(130 + i * 4, 130 + i * 4, 2);
            fresh.Add(c);
            tree.Insert(c);
        }

        var results = new List<Circle>();
        tree.Query(140, 140, 30, results);
        foreach (var c in fresh)
            Assert.Contains(c, results); // every new item still findable

        tree.Query(480, 480, 5, results); // far corner of the NEW bounds
        Assert.Empty(results); // no stale items leaked out of reused pool nodes

        tree.Query(250, 250, 600, results); // covers everything
        Assert.Equal(fresh.Count, results.Count); // exactly the new items, nothing stale
    }

    // ── Randomized Completeness Sweep ─────────────────────────────────────────

    // Completeness (the core broad-phase guarantee): for seeded-random items and
    // queries, every item whose circle overlaps the query circle (GameMath.CirclesOverlap)
    // must appear in the results. Over-returning is allowed; missing is a defect.
    [Fact]
    public void Query_RandomizedSweep_NeverMissesAnOverlappingItem()
    {
        var rng = new Random(42); // seeded for determinism
        var tree = new Quadtree<Circle>(0, 0, 800, 600, capacity: 8);

        var items = new List<Circle>();
        for (var i = 0; i < 200; i++)
        {
            // Include positions outside the root bounds (off-screen spawns).
            var c = At(
                rng.NextDouble() * 900 - 50,
                rng.NextDouble() * 700 - 50,
                1 + rng.NextDouble() * 19);
            items.Add(c);
            tree.Insert(c);
        }

        var results = new List<Circle>();
        for (var q = 0; q < 50; q++)
        {
            var query = At(
                rng.NextDouble() * 800,
                rng.NextDouble() * 600,
                5 + rng.NextDouble() * 55);
            tree.Query(query.X, query.Y, query.Radius, results);

            foreach (var item in items)
            {
                if (GameMath.CirclesOverlap(query, item))
                    Assert.True(results.Contains(item),
                        $"Broad phase missed an overlapping item: query ({query.X:F1}, {query.Y:F1}, r={query.Radius:F1}) " +
                        $"vs item ({item.X:F1}, {item.Y:F1}, r={item.Radius:F1}).");
            }
        }
    }
}
