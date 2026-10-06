using UnityEngine;
using UnityEngine.InputSystem;

namespace AnvilClicker.Runtime
{
    /// <summary>
    /// Lives on a "camera rig" object that moves; the camera itself is its child, so screen shake can keep
    /// working on the camera's local position. Follows the blacksmith, stays inside the workshop, zooms with
    /// the scroll wheel and pushes in on the anvil during forge mode.
    /// </summary>
    public sealed class CameraFollow : MonoBehaviour
    {
        const string ZoomActionName = "Player/Zoom";

        [SerializeField] Camera cam;
        [SerializeField] Transform target;
        [SerializeField] WorldGrid world;
        [SerializeField] ForgeMode forgeMode;

        [Header("Follow")]
        [SerializeField] float followSmoothTime = 0.18f;
        [Tooltip("Extra room around the workshop the camera may show, in world units.")]
        [SerializeField] float boundsMargin = 1.2f;

        [Header("Zoom (orthographic sizes)")]
        [SerializeField] float[] zoomLevels = { 2.8f, 3.6f, 4.6f };
        [SerializeField] int startZoomIndex = 1;
        [SerializeField] float forgeZoom = 2.2f;
        [SerializeField] float zoomSmoothTime = 0.2f;

        InputAction _zoom;
        int _zoomIndex;
        Vector3 _velocity;
        float _sizeVelocity;

        void OnEnable()
        {
            _zoomIndex = Mathf.Clamp(startZoomIndex, 0, zoomLevels.Length - 1);

            var actions = InputSystem.actions;
            if (actions == null) return;
            _zoom = actions.FindAction(ZoomActionName, throwIfNotFound: true);
            _zoom.Enable();
        }

        void Start() => SnapToTarget();

        /// <summary>Jumps straight to where the camera should be (first frame, teleports).</summary>
        public void SnapToTarget()
        {
            if (target == null || cam == null) return;
            cam.orthographicSize = DesiredSize();
            transform.position = Clamp(DesiredPoint());
        }

        void LateUpdate()
        {
            if (target == null || cam == null) return;

            ReadZoomInput();

            cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, DesiredSize(), ref _sizeVelocity, zoomSmoothTime);

            var goal = Clamp(DesiredPoint());
            transform.position = Vector3.SmoothDamp(transform.position, goal, ref _velocity, followSmoothTime);
        }

        void ReadZoomInput()
        {
            if (_zoom == null || (forgeMode != null && forgeMode.IsActive)) return;

            var scroll = _zoom.ReadValue<Vector2>().y;
            if (Mathf.Abs(scroll) < 0.01f) return;

            _zoomIndex = Mathf.Clamp(_zoomIndex + (scroll > 0f ? -1 : 1), 0, zoomLevels.Length - 1);
        }

        float DesiredSize() => forgeMode != null && forgeMode.IsActive ? forgeZoom : zoomLevels[_zoomIndex];

        Vector3 DesiredPoint()
        {
            var point = forgeMode != null && forgeMode.IsActive ? forgeMode.FocusPoint : target.position;
            point.z = transform.position.z;
            return point;
        }

        /// <summary>Keeps the view inside the floor plus a margin; a view larger than the workshop is centred on it.</summary>
        Vector3 Clamp(Vector3 point)
        {
            if (world == null) return point;

            var bounds = world.GetFloorBounds();
            bounds.Expand(new Vector3(boundsMargin * 2f, boundsMargin * 2f, 0f));

            var halfHeight = cam.orthographicSize;
            var halfWidth = halfHeight * cam.aspect;

            point.x = ClampAxis(point.x, bounds.min.x + halfWidth, bounds.max.x - halfWidth, bounds.center.x);
            point.y = ClampAxis(point.y, bounds.min.y + halfHeight, bounds.max.y - halfHeight, bounds.center.y);
            return point;
        }

        static float ClampAxis(float value, float min, float max, float center) =>
            min > max ? center : Mathf.Clamp(value, min, max);
    }
}
