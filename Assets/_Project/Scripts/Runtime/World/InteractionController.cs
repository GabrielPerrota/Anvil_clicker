using System;
using AnvilClicker.Data;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace AnvilClicker.Runtime
{
    /// <summary>
    /// Finds the station within reach, opens it with E (or a click that walks there first) and closes it
    /// with Esc, E again or by walking away. The anvil starts <see cref="ForgeMode"/>; every other station
    /// is just announced, and the UI decides what to show (the Presentation layer listens to the events).
    /// </summary>
    public sealed class InteractionController : MonoBehaviour
    {
        const string InteractActionName = "Player/Interact";
        const string CancelActionName = "Player/Cancel";
        const string ClickActionName = "Player/Click";
        const string PointActionName = "Forge/Point";

        [SerializeField] PlayerController player;
        [SerializeField] PathFollower pathFollower;
        [SerializeField] WorldGrid world;
        [SerializeField] ForgeMode forgeMode;
        [SerializeField] Camera worldCamera;
        [SerializeField] UIDocument uiDocument;
        [Tooltip("A panel closes on its own once the blacksmith is this many times farther than the reach.")]
        [SerializeField] float closeDistanceFactor = 1.35f;

        InputAction _interact;
        InputAction _cancel;
        InputAction _click;
        InputAction _point;
        Station _nearby;
        Station _open;

        /// <summary>The station in reach (null if none). Drives the "E — ..." prompt.</summary>
        public Station Nearby => _nearby;

        /// <summary>The station whose panel is open (null if none).</summary>
        public Station Open => _open;

        public event Action<Station> NearbyChanged;
        public event Action<Station> StationOpened;
        public event Action<Station> StationClosed;

        void OnEnable()
        {
            var actions = InputSystem.actions;
            if (actions == null)
            {
                Debug.LogError("No project-wide input actions are assigned.", this);
                enabled = false;
                return;
            }

            _interact = actions.FindAction(InteractActionName, throwIfNotFound: true);
            _cancel = actions.FindAction(CancelActionName, throwIfNotFound: true);
            _click = actions.FindAction(ClickActionName, throwIfNotFound: true);
            _point = actions.FindAction(PointActionName, throwIfNotFound: true);
            _interact.Enable();
            _cancel.Enable();
            _click.Enable();
            _point.Enable();

            _interact.performed += OnInteract;
            _cancel.performed += OnCancel;
            _click.performed += OnClick;
        }

        void OnDisable()
        {
            if (_interact != null) _interact.performed -= OnInteract;
            if (_cancel != null) _cancel.performed -= OnCancel;
            if (_click != null) _click.performed -= OnClick;
        }

        void Update()
        {
            UpdateNearby();

            if (_open != null && _open.GroundDistanceTo(player.Feet) > _open.Definition.InteractionRadius * closeDistanceFactor)
                CloseOpen();
        }

        // --- Finding the station in reach ----------------------------------------------------------

        void UpdateNearby()
        {
            Station best = null;
            var bestDistance = float.MaxValue;

            foreach (var station in world.Stations)
            {
                var distance = station.GroundDistanceTo(player.Feet);
                if (distance > station.Definition.InteractionRadius || distance >= bestDistance) continue;
                best = station;
                bestDistance = distance;
            }

            if (best == _nearby) return;
            _nearby = best;
            NearbyChanged?.Invoke(_nearby);
        }

        // --- Input ---------------------------------------------------------------------------------

        void OnInteract(InputAction.CallbackContext _)
        {
            if (forgeMode.IsActive)
            {
                forgeMode.Exit();
                return;
            }

            if (_open != null)
            {
                CloseOpen();
                return;
            }

            if (_nearby != null) Activate(_nearby);
        }

        void OnCancel(InputAction.CallbackContext _)
        {
            if (forgeMode.IsActive) forgeMode.Exit();
            else CloseOpen();
        }

        void OnClick(InputAction.CallbackContext _)
        {
            // In forge mode the click belongs to the anvil strike (ForgeInput).
            if (forgeMode.IsActive) return;

            var screen = _point.ReadValue<Vector2>();
            if (UiPointerBlocker.IsOverBlockingUi(uiDocument, screen)) return;

            var station = StationAt(screen);
            if (station == null) return;

            if (station.GroundDistanceTo(player.Feet) <= station.Definition.InteractionRadius)
            {
                Activate(station);
                return;
            }

            if (world.TryFindStandCell(station, player.Feet, out var stand))
                pathFollower.GoTo(stand, () => Activate(station));
        }

        Station StationAt(Vector2 screen)
        {
            var cam = worldCamera != null ? worldCamera : Camera.main;
            if (cam == null) return null;

            var point = cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -cam.transform.position.z));
            var hit = Physics2D.OverlapPoint(point);
            return hit != null ? hit.GetComponentInParent<Station>() : null;
        }

        // --- Opening and closing -------------------------------------------------------------------

        void Activate(Station station)
        {
            if (station == null) return;
            CloseOpen();
            player.FaceTowards(station.Position);

            if (station.Kind == StationKind.Anvil)
            {
                forgeMode.Enter(station);
                return;
            }

            if (station.Kind == StationKind.Forge) return; // decoration for now: its light reacts to upgrades

            _open = station;
            StationOpened?.Invoke(station);
        }

        public void CloseOpen()
        {
            if (_open == null) return;

            var closed = _open;
            _open = null;
            StationClosed?.Invoke(closed);
        }
    }
}
