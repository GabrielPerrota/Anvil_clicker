using System;
using System.Collections.Generic;
using AnvilClicker.Core;
using UnityEngine;

namespace AnvilClicker.Runtime
{
    /// <summary>Walks the blacksmith along an A* path (click on a station) and cancels as soon as the keyboard is used.</summary>
    public sealed class PathFollower : MonoBehaviour
    {
        [SerializeField] PlayerController player;
        [SerializeField] WorldGrid world;
        [Tooltip("How close, in world units, counts as having reached a waypoint.")]
        [SerializeField] float arriveDistance = 0.12f;

        List<GridCell> _path;
        int _index;
        Action _onArrive;

        public bool IsFollowing => _path != null;

        /// <summary>Starts walking to <paramref name="cell"/>. Returns false when there is no way to get there.</summary>
        public bool GoTo(Vector2Int cell, Action onArrive)
        {
            Cancel();

            var start = world.WorldToCell(player.Feet);
            var path = GridPathfinder.Find(world, new GridCell(start.x, start.y), new GridCell(cell.x, cell.y));
            if (path == null) return false;

            _path = path;
            _index = Math.Min(1, path.Count - 1); // path[0] is where we already are
            _onArrive = onArrive;
            return true;
        }

        public void Cancel()
        {
            _path = null;
            _onArrive = null;
            if (player != null) player.SetPathDirection(Vector2.zero);
        }

        void Update()
        {
            if (_path == null) return;

            if (player.KeyboardInput.sqrMagnitude > 0.01f)
            {
                Cancel();
                return;
            }

            while (_index < _path.Count)
            {
                var waypoint = world.CellCenter(new Vector2Int(_path[_index].X, _path[_index].Y));
                var delta = waypoint - player.Feet;
                if (delta.magnitude > arriveDistance)
                {
                    player.SetPathDirection(new Vector2(delta.x, delta.y));
                    return;
                }

                _index++;
            }

            var arrived = _onArrive;
            Cancel();
            arrived?.Invoke();
        }
    }
}
