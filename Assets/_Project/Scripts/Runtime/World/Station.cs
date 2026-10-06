using AnvilClicker.Data;
using UnityEngine;

namespace AnvilClicker.Runtime
{
    /// <summary>A place in the workshop the blacksmith can use. Spawned by <see cref="RoomBuilder"/>.</summary>
    public sealed class Station : MonoBehaviour
    {
        public StationDefinition Definition { get; private set; }

        /// <summary>Isometric grid cell the station stands on.</summary>
        public Vector2Int Cell { get; private set; }

        public StationKind Kind => Definition.Kind;

        /// <summary>Where the station's feet touch the floor.</summary>
        public Vector3 Position => transform.position;

        public void Initialize(StationDefinition definition, Vector2Int cell)
        {
            Definition = definition;
            Cell = cell;
            name = definition.DisplayName;
        }

        /// <summary>
        /// Distance to a point on the floor, measured as it looks on screen: the map is drawn twice as wide
        /// as tall, so a vertical gap counts double and the reach is a circle on the ground, not an ellipse.
        /// </summary>
        public float GroundDistanceTo(Vector3 point)
        {
            var dx = point.x - Position.x;
            var dy = (point.y - Position.y) * 2f;
            return Mathf.Sqrt(dx * dx + dy * dy);
        }
    }
}
