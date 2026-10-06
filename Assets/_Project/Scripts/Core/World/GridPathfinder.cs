using System;
using System.Collections.Generic;

namespace AnvilClicker.Core
{
    /// <summary>Tells the pathfinder which grid cells can be walked on.</summary>
    public interface IWalkability
    {
        bool IsWalkable(int x, int y);
    }

    /// <summary>A grid cell.</summary>
    public readonly struct GridCell : IEquatable<GridCell>
    {
        public GridCell(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; }
        public int Y { get; }

        public bool Equals(GridCell other) => X == other.X && Y == other.Y;
        public override bool Equals(object obj) => obj is GridCell other && Equals(other);
        public override int GetHashCode() => unchecked(X * 397 ^ Y);
        public override string ToString() => $"({X}, {Y})";
    }

    /// <summary>
    /// A* over an unbounded grid with 8-way movement. Diagonal steps are only allowed when both
    /// orthogonal neighbours are walkable, so paths never cut through wall corners.
    /// </summary>
    public static class GridPathfinder
    {
        /// <summary>Upper bound on explored cells, so an unreachable goal in open space cannot hang the game.</summary>
        public const int DefaultMaxNodes = 4096;

        const int StraightCost = 10;
        const int DiagonalCost = 14;

        static readonly (int dx, int dy)[] Directions =
        {
            (1, 0), (-1, 0), (0, 1), (0, -1),
            (1, 1), (1, -1), (-1, 1), (-1, -1)
        };

        /// <summary>
        /// Shortest path from <paramref name="start"/> to <paramref name="goal"/>, both included.
        /// Returns null when the goal is blocked, unreachable or farther than <paramref name="maxNodes"/> cells explored.
        /// </summary>
        public static List<GridCell> Find(IWalkability map, GridCell start, GridCell goal, int maxNodes = DefaultMaxNodes)
        {
            if (map == null) throw new ArgumentNullException(nameof(map));
            if (!map.IsWalkable(goal.X, goal.Y)) return null;
            if (start.Equals(goal)) return new List<GridCell> { start };

            var open = new PriorityQueue();
            var cameFrom = new Dictionary<GridCell, GridCell>();
            var cost = new Dictionary<GridCell, int> { [start] = 0 };
            var closed = new HashSet<GridCell>();

            open.Enqueue(start, Heuristic(start, goal));

            while (open.Count > 0 && closed.Count < maxNodes)
            {
                var current = open.Dequeue();
                if (!closed.Add(current)) continue;

                if (current.Equals(goal)) return Rebuild(cameFrom, current);

                foreach (var (dx, dy) in Directions)
                {
                    var next = new GridCell(current.X + dx, current.Y + dy);
                    if (closed.Contains(next) || !map.IsWalkable(next.X, next.Y)) continue;

                    var diagonal = dx != 0 && dy != 0;
                    if (diagonal && (!map.IsWalkable(current.X + dx, current.Y) || !map.IsWalkable(current.X, current.Y + dy))) continue;

                    var newCost = cost[current] + (diagonal ? DiagonalCost : StraightCost);
                    if (cost.TryGetValue(next, out var known) && known <= newCost) continue;

                    cost[next] = newCost;
                    cameFrom[next] = current;
                    open.Enqueue(next, newCost + Heuristic(next, goal));
                }
            }

            return null;
        }

        /// <summary>Octile distance: exact cost on an obstacle-free grid with these step costs.</summary>
        static int Heuristic(GridCell a, GridCell b)
        {
            var dx = Math.Abs(a.X - b.X);
            var dy = Math.Abs(a.Y - b.Y);
            return StraightCost * (dx + dy) + (DiagonalCost - 2 * StraightCost) * Math.Min(dx, dy);
        }

        static List<GridCell> Rebuild(Dictionary<GridCell, GridCell> cameFrom, GridCell end)
        {
            var path = new List<GridCell> { end };
            while (cameFrom.TryGetValue(path[path.Count - 1], out var previous)) path.Add(previous);
            path.Reverse();
            return path;
        }

        /// <summary>Minimal binary min-heap keyed by priority.</summary>
        sealed class PriorityQueue
        {
            readonly List<(GridCell cell, int priority)> _items = new List<(GridCell, int)>();

            public int Count => _items.Count;

            public void Enqueue(GridCell cell, int priority)
            {
                _items.Add((cell, priority));
                var i = _items.Count - 1;
                while (i > 0)
                {
                    var parent = (i - 1) / 2;
                    if (_items[parent].priority <= _items[i].priority) break;
                    (_items[parent], _items[i]) = (_items[i], _items[parent]);
                    i = parent;
                }
            }

            public GridCell Dequeue()
            {
                var top = _items[0].cell;
                var last = _items.Count - 1;
                _items[0] = _items[last];
                _items.RemoveAt(last);

                var i = 0;
                while (true)
                {
                    var left = 2 * i + 1;
                    var right = left + 1;
                    var smallest = i;
                    if (left < _items.Count && _items[left].priority < _items[smallest].priority) smallest = left;
                    if (right < _items.Count && _items[right].priority < _items[smallest].priority) smallest = right;
                    if (smallest == i) break;
                    (_items[smallest], _items[i]) = (_items[i], _items[smallest]);
                    i = smallest;
                }

                return top;
            }
        }
    }
}
