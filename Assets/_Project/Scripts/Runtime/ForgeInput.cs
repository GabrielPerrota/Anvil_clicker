using AnvilClicker.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace AnvilClicker.Runtime
{
    /// <summary>
    /// Turns the "Forge/Strike" action into hammer strikes, but only while in <see cref="ForgeMode"/>.
    /// Pointer presses only count on the anvil; keyboard presses (Space) always count.
    /// </summary>
    public sealed class ForgeInput : MonoBehaviour, IGameContextConsumer
    {
        const string StrikeActionName = "Forge/Strike";
        const string PointActionName = "Forge/Point";

        [SerializeField] ForgeMode forgeMode;
        [SerializeField] Collider2D anvilCollider;
        [SerializeField] Camera worldCamera;
        [Tooltip("Clicks landing on this document's blocking elements (panels, modals) are not strikes.")]
        [SerializeField] UIDocument uiDocument;

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
            if (_context == null || forgeMode == null || !forgeMode.IsActive) return;
            if (callback.control.device is Pointer && !IsPointerOverAnvil()) return;

            _context.Forge.Strike();
        }

        bool IsPointerOverAnvil()
        {
            var collider = ActiveAnvilCollider();
            if (collider == null) return false;

            var cam = worldCamera != null ? worldCamera : Camera.main;
            if (cam == null) return false;

            var screen = _point.ReadValue<Vector2>();
            if (UiPointerBlocker.IsOverBlockingUi(uiDocument, screen)) return false;

            var world = cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -cam.transform.position.z));
            return collider.OverlapPoint(world);
        }

        /// <summary>The anvil is spawned with its room, so the collider comes from the station in use.</summary>
        Collider2D ActiveAnvilCollider() =>
            forgeMode.Anvil != null ? forgeMode.Anvil.GetComponentInChildren<Collider2D>() : anvilCollider;
    }
}
