using System.Collections.Generic;
using System.Linq;
using AnvilClicker.Core;
using NUnit.Framework;

namespace AnvilClicker.Tests
{
    public class GridPathfinderTests
    {
        /// <summary>Walkable inside [0, width) × [0, height), minus the blocked cells.</summary>
        sealed class TestMap : IWalkability
        {
            readonly int _width, _height;
            readonly HashSet<(int, int)> _blocked = new HashSet<(int, int)>();

            public TestMap(int width, int height)
            {
                _width = width;
                _height = height;
            }

            public TestMap Block(params (int x, int y)[] cells)
            {
                foreach (var cell in cells) _blocked.Add(cell);
                return this;
            }

            public bool IsWalkable(int x, int y) => x >= 0 && y >= 0 && x < _width && y < _height && !_blocked.Contains((x, y));
        }

        static GridCell C(int x, int y) => new GridCell(x, y);

        [Test]
        public void Find_SameStartAndGoal_ReturnsSingleCell()
        {
            var path = GridPathfinder.Find(new TestMap(5, 5), C(2, 2), C(2, 2));

            Assert.That(path, Is.EqualTo(new[] { C(2, 2) }));
        }

        [Test]
        public void Find_OpenGrid_TakesTheDiagonal()
        {
            var path = GridPathfinder.Find(new TestMap(6, 6), C(0, 0), C(4, 4));

            Assert.That(path.Count, Is.EqualTo(5));
            Assert.That(path.First(), Is.EqualTo(C(0, 0)));
            Assert.That(path.Last(), Is.EqualTo(C(4, 4)));
        }

        [Test]
        public void Find_StraightLine_IncludesEveryCell()
        {
            var path = GridPathfinder.Find(new TestMap(6, 1), C(0, 0), C(5, 0));

            Assert.That(path.Select(c => c.X), Is.EqualTo(new[] { 0, 1, 2, 3, 4, 5 }));
        }

        [Test]
        public void Find_WallInTheWay_GoesAround()
        {
            var map = new TestMap(5, 5).Block((2, 0), (2, 1), (2, 2), (2, 3)); // gap only at y = 4

            var path = GridPathfinder.Find(map, C(0, 0), C(4, 0));

            Assert.That(path, Is.Not.Null);
            Assert.That(path.Any(c => c.Equals(C(2, 4))), Is.True);
            Assert.That(path.All(c => map.IsWalkable(c.X, c.Y)), Is.True);
        }

        [Test]
        public void Find_FullyWalledOff_ReturnsNull()
        {
            var map = new TestMap(5, 5).Block((2, 0), (2, 1), (2, 2), (2, 3), (2, 4));

            Assert.That(GridPathfinder.Find(map, C(0, 0), C(4, 0)), Is.Null);
        }

        [Test]
        public void Find_BlockedGoal_ReturnsNull()
        {
            var map = new TestMap(5, 5).Block((3, 3));

            Assert.That(GridPathfinder.Find(map, C(0, 0), C(3, 3)), Is.Null);
        }

        [Test]
        public void Find_GoalOutsideTheMap_ReturnsNull()
        {
            Assert.That(GridPathfinder.Find(new TestMap(5, 5), C(0, 0), C(9, 9)), Is.Null);
        }

        [Test]
        public void Find_DoesNotCutThroughWallCorners()
        {
            // The only "shortcut" from (0,0) to (1,1) is a diagonal squeezed between two walls.
            var map = new TestMap(2, 2).Block((1, 0), (0, 1));

            Assert.That(GridPathfinder.Find(map, C(0, 0), C(1, 1)), Is.Null);
        }

        [Test]
        public void Find_EveryStepMovesToAnAdjacentCell()
        {
            var map = new TestMap(8, 8).Block((3, 1), (3, 2), (3, 3), (3, 4), (3, 5));

            var path = GridPathfinder.Find(map, C(0, 3), C(7, 3));

            for (var i = 1; i < path.Count; i++)
            {
                Assert.That(System.Math.Abs(path[i].X - path[i - 1].X), Is.LessThanOrEqualTo(1));
                Assert.That(System.Math.Abs(path[i].Y - path[i - 1].Y), Is.LessThanOrEqualTo(1));
            }
        }

        [Test]
        public void Find_ExplorationLimit_StopsInsteadOfHanging()
        {
            // Goal is walkable but sealed in; the open map around it is unbounded in practice.
            var map = new TestMap(500, 500).Block((10, 9), (10, 11), (9, 10), (11, 10), (9, 9), (9, 11), (11, 9), (11, 11));

            Assert.That(GridPathfinder.Find(map, C(0, 0), C(10, 10), maxNodes: 200), Is.Null);
        }

        [Test]
        public void Find_NullMap_Throws()
        {
            Assert.Throws<System.ArgumentNullException>(() => GridPathfinder.Find(null, C(0, 0), C(1, 1)));
        }
    }
}
