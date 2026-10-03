using AnvilClicker.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AnvilClicker.Runtime
{
    /// <summary>
    /// Turns the "Forge/Strike" action into hammer strikes. Pointer presses only count on the anvil;
    /// keyboard presses (Space) always count.
    /// </summary>
    public sealed class ForgeInput : MonoBehaviour, IGameContextConsumer
    {
        const string StrikeActionName = "Forge/Strike";
        const string PointActionName = "Forge/Point";

        [SerializeField] Collider2D anvilCollider;
        [SerializeField] Camera worldCamera;

        GameContext _context;
        InputAction _strike;
        InputAction _point;

        public void Bind(GameContext context) => _context = context;

        void OnEnable()
        {
            var actions = InputSystem.actions;
            if (actions == null)
            {
                Debug.LogError("No project-wide input actions are assigned (Project Settings > Input System Package).", this);
                enabled = false;
                return;
            }

            _strike = actions.FindAction(StrikeActionName, throwIfNotFound: true);
            _point = actions.FindAction(PointActionName, throwIfNotFound: true);

            _strike.performed += OnStrike;
            _strike.Enable();
            _point.Enable();
        }

        void OnDisable()
        {
            if (_strike != null) _strike.performed -= OnStrike;
        }

        void OnStrike(InputAction.CallbackContext callback)
        {
            if (_context == null) return;
            if (callback.control.device is Pointer && !IsPointerOverAnvil()) return;

            _context.Forge.Strike();
        }

        bool IsPointerOverAnvil()
        {
            if (anvilCollider == null) return false;

            var cam = worldCamera != null ? worldCamera : Camera.main;
            if (cam == null) return false;

            var screen = _point.ReadValue<Vector2>();
            var world = cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -cam.transform.position.z));
            return anvilCollider.OverlapPoint(world);
        }
    }
}
