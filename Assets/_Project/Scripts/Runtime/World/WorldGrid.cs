using System;
using System.Collections.Generic;
using AnvilClicker.Core;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace AnvilClicker.Runtime
{
    /// <summary>
    /// The isometric map as the game logic sees it: which cells can be walked on, where the stations are,
    /// and how world positions map to grid cells. Rooms add their floor, walls and stations through it.
    /// </summary>
    public sealed class WorldGrid : MonoBehaviour, IWalkability
    {
        [SerializeField] Grid grid;
        [SerializeField] Tilemap floor;
        [SerializeField] Tilemap walls;

        readonly List<Station> _stations = new List<Station>();
        readonly HashSet<Vector2Int> _blocked = new HashSet<Vector2Int>();

        /// <summary>Raised whenever the floor, the walls or the stations changed.</summary>
        public event Action Changed;

        public Grid Grid => grid;
        public Tilemap Floor => floor;
        public Tilemap Walls => walls;
        public IReadOnlyList<Station> Stations => _stations;

        public bool IsWalkable(int x, int y)
        {
            var cell = new Vector3Int(x, y, 0);
            return floor.HasTile(cell) && !walls.HasTile(cell) && !_blocked.Contains(new Vector2Int(x, y));
        }

        public Vector2Int WorldToCell(Vector3 world)
        {
            var cell = grid.WorldToCell(world);
            return new Vector2Int(cell.x, cell.y);
        }

        public Vector3 CellCenter(Vector2Int cell) => grid.GetCellCenterWorld(new Vector3Int(cell.x, cell.y, 0));

        public void NotifyChanged() => Changed?.Invoke();

        public void Register(Station station, Vector2Int[] blockedOffsets)
        {
            _stations.Add(station);
            foreach (var offset in blockedOffsets) _blocked.Add(station.Cell + offset);
            Changed?.Invoke();
        }

        /// <summary>World-space box around every floor cell, for keeping the camera inside the workshop.</summary>
        public Bounds GetFloorBounds()
        {
            floor.CompressBounds();
            var cells = floor.cellBounds;
            if (cells.size.x <= 0 || cells.size.y <= 0) return new Bounds(Vector3.zero, Vector3.one);

            var bounds = new Bounds(CellCenter(new Vector2Int(cells.xMin, cells.yMin)), Vector3.zero);
            bounds.Encapsulate(CellCenter(new Vector2Int(cells.xMax - 1, cells.yMin)));
            bounds.Encapsulate(CellCenter(new Vector2Int(cells.xMin, cells.yMax - 1)));
            bounds.Encapsulate(CellCenter(new Vector2Int(cells.xMax - 1, cells.yMax - 1)));
            bounds.Expand(new Vector3(1f, 0.5f, 0f));
            return bounds;
        }

        /// <summary>The walkable cell next to a station that is closest to <paramref name="from"/>.</summary>
        public bool TryFindStandCell(Station station, Vector3 from, out Vector2Int stand)
        {
            stand = default;
            var best = float.MaxValue;
            var found = false;

            for (var dx = -1; dx <= 1; dx++)
            for (var dy = -1; dy <= 1; dy++)
            {
                var candidate = station.Cell + new Vector2Int(dx, dy);
                if (!IsWalkable(candidate.x, candidate.y)) continue;

                var distance = (CellCenter(candidate) - from).sqrMagnitude;
                if (distance >= best) continue;
                best = distance;
                stand = candidate;
                found = true;
            }

            return found;
        }
    }
}
