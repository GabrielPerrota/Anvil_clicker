using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AnvilClicker.Core;
using AnvilClicker.Data;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace AnvilClicker.Runtime
{
    /// <summary>
    /// Builds the workshop on the map: floor, walls and stations of every unlocked room at startup, and with
    /// a little show (dust, floor spreading from the door, stations popping in) when a room is bought.
    /// </summary>
    public sealed class RoomBuilder : MonoBehaviour, IGameContextConsumer
    {
        [SerializeField] GameDatabase database;
        [SerializeField] WorldGrid world;
        [SerializeField] Transform stationsRoot;

        [Header("Tiles and props")]
        [SerializeField] TileBase floorTile;
        [SerializeField] TileBase wallTile;
        [Tooltip("Collision only, no sprite: closes the front edges of the map.")]
        [SerializeField] TileBase invisibleWallTile;
        [SerializeField] GameObject barrierPrefab;
        [SerializeField] ParticleSystem dustPrefab;

        [Header("Build animation")]
        [SerializeField] float stepDelay = 0.03f;
        [SerializeField] int cellsPerStep = 3;
        [SerializeField] float popDuration = 0.35f;

        readonly Dictionary<string, List<GameObject>> _barriers = new Dictionary<string, List<GameObject>>();
        GameContext _context;

        public void Bind(GameContext context) => _context = context;

        void Start()
        {
            if (_context == null) return;

            foreach (var room in database.RoomAssets)
            {
                if (_context.Rooms.IsUnlocked(room)) Run(BuildRoutine(room, animate: false), animate: false);
            }

            // Rooms still for sale are walled off from the rest of the workshop by a barrier at their door.
            foreach (var room in database.RoomAssets)
            {
                if (!_context.Rooms.IsUnlocked(room)) PlaceBarriers(room);
            }

            world.NotifyChanged();
            _context.Rooms.RoomUnlocked += OnRoomUnlocked;
        }

        void OnDestroy()
        {
            if (_context != null) _context.Rooms.RoomUnlocked -= OnRoomUnlocked;
        }

        void OnRoomUnlocked(IRoomDefinition room)
        {
            var definition = database.RoomAssets.FirstOrDefault(r => r.Id == room.Id);
            if (definition != null) Run(BuildRoutine(definition, animate: true), animate: true);
        }

        void Run(IEnumerator routine, bool animate)
        {
            if (animate) StartCoroutine(routine);
            else
            {
                while (routine.MoveNext()) { }
            }
        }

        // --- Building ------------------------------------------------------------------------------

        IEnumerator BuildRoutine(RoomDefinition room, bool animate)
        {
            var area = room.Area;
            var doors = new HashSet<Vector2Int>(room.DoorCells);

            // 1. Tear down the barrier and open the door.
            RemoveBarriers(room, animate);
            foreach (var door in doors)
            {
                world.Walls.SetTile(ToCell(door), null);
                world.Floor.SetTile(ToCell(door), floorTile);
                if (animate) SpawnDust(world.CellCenter(door));
            }
            world.NotifyChanged();
            if (animate) yield return new WaitForSeconds(0.25f);

            // 2. Floor, spreading from the door.
            var origin = doors.Count > 0 ? world.CellCenter(doors.First()) : world.CellCenter(area.center.ToVector2Int());
            var cells = new List<Vector2Int>();
            for (var x = area.xMin; x < area.xMax; x++)
            for (var y = area.yMin; y < area.yMax; y++) cells.Add(new Vector2Int(x, y));
            cells.Sort((a, b) => (world.CellCenter(a) - origin).sqrMagnitude.CompareTo((world.CellCenter(b) - origin).sqrMagnitude));

            for (var i = 0; i < cells.Count; i++)
            {
                world.Floor.SetTile(ToCell(cells[i]), floorTile);
                if (animate && i % cellsPerStep == cellsPerStep - 1) yield return new WaitForSeconds(stepDelay);
            }

            // 3. Walls around it.
            var ring = RingCells(area).ToList();
            for (var i = 0; i < ring.Count; i++)
            {
                PaintWall(ring[i], area, doors);
                if (animate && i % (cellsPerStep * 2) == 0) yield return new WaitForSeconds(stepDelay);
            }
            world.NotifyChanged();

            // 4. Stations.
            foreach (var placement in room.Stations)
            {
                if (placement.station == null) continue;
                var station = SpawnStation(placement.station, placement.cell);
                if (animate)
                {
                    SpawnDust(station.Position);
                    yield return PopIn(station.transform);
                }
            }
        }

        void PaintWall(Vector2Int cell, RectInt area, HashSet<Vector2Int> doors)
        {
            if (doors.Contains(cell)) return;                       // the opening itself
            if (IsBarrierCell(cell)) return;                        // a room still for sale boards this cell up
            if (world.Floor.HasTile(ToCell(cell))) return;          // another room's floor
            if (world.Walls.HasTile(ToCell(cell))) return;          // keep the visible wall that is already there

            // The two edges facing away from the camera are drawn; the front edges only block.
            var visible = cell.x >= area.xMax || cell.y >= area.yMax;
            world.Walls.SetTile(ToCell(cell), visible ? wallTile : invisibleWallTile);
        }

        IEnumerable<Vector2Int> RingCells(RectInt area)
        {
            for (var x = area.xMin - 1; x <= area.xMax; x++)
            for (var y = area.yMin - 1; y <= area.yMax; y++)
            {
                if (x >= area.xMin && x < area.xMax && y >= area.yMin && y < area.yMax) continue;
                yield return new Vector2Int(x, y);
            }
        }

        // --- Barriers ------------------------------------------------------------------------------

        void PlaceBarriers(RoomDefinition room)
        {
            var list = new List<GameObject>();
            foreach (var door in room.DoorCells)
            {
                world.Walls.SetTile(ToCell(door), invisibleWallTile);
                if (barrierPrefab == null) continue;

                var barrier = Instantiate(barrierPrefab, world.CellCenter(door), Quaternion.identity, stationsRoot);
                barrier.name = $"Barrier ({room.DisplayName})";
                list.Add(barrier);
            }

            _barriers[room.Id] = list;
        }

        bool IsBarrierCell(Vector2Int cell) =>
            database.RoomAssets.Any(r => !_context.Rooms.IsUnlocked(r) && r.DoorCells.Contains(cell));

        void RemoveBarriers(RoomDefinition room, bool animate)
        {
            if (!_barriers.TryGetValue(room.Id, out var list)) return;

            foreach (var barrier in list)
            {
                if (barrier == null) continue;
                if (animate) StartCoroutine(ShrinkAndDestroy(barrier));
                else Destroy(barrier);
            }

            _barriers.Remove(room.Id);
        }

        IEnumerator ShrinkAndDestroy(GameObject target)
        {
            SpawnDust(target.transform.position);
            var start = target.transform.localScale;
            for (var t = 0f; t < 0.3f; t += Time.deltaTime)
            {
                if (target == null) yield break;
                target.transform.localScale = Vector3.Lerp(start, Vector3.zero, t / 0.3f);
                yield return null;
            }

            if (target != null) Destroy(target);
        }

        // --- Stations and effects ------------------------------------------------------------------

        Station SpawnStation(StationDefinition definition, Vector2Int cell)
        {
            var instance = Instantiate(definition.Prefab, world.CellCenter(cell), Quaternion.identity, stationsRoot);
            var station = instance.GetComponent<Station>();
            if (station == null) station = instance.AddComponent<Station>();

            station.Initialize(definition, cell);
            world.Register(station, definition.BlockedOffsets);
            GameContextBinding.BindAll(instance, _context);
            return station;
        }

        IEnumerator PopIn(Transform target)
        {
            var final = target.localScale;
            for (var t = 0f; t < popDuration; t += Time.deltaTime)
            {
                if (target == null) yield break;
                var k = t / popDuration;
                var overshoot = 1f + 0.18f * Mathf.Sin(k * Mathf.PI); // grows past full size, then settles
                target.localScale = final * (k * overshoot);
                yield return null;
            }

            if (target != null) target.localScale = final;
        }

        void SpawnDust(Vector3 position)
        {
            if (dustPrefab == null) return;
            var dust = Instantiate(dustPrefab, position, Quaternion.identity, stationsRoot);
            dust.Play();
            Destroy(dust.gameObject, 2f);
        }

        static Vector3Int ToCell(Vector2Int cell) => new Vector3Int(cell.x, cell.y, 0);
    }

    static class Vector2Extensions
    {
        public static Vector2Int ToVector2Int(this Vector2 value) => new Vector2Int(Mathf.RoundToInt(value.x), Mathf.RoundToInt(value.y));
    }
}
