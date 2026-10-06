using System;
using UnityEngine;

namespace AnvilClicker.Runtime
{
    /// <summary>
    /// "Forge mode": the blacksmith stands at the anvil, the camera moves in and only now do clicks and Space
    /// strike. Leaving it (Esc, E, walking away) returns control to the player.
    /// </summary>
    public sealed class ForgeMode : MonoBehaviour
    {
        [SerializeField] PlayerController player;
        [Tooltip("How far above the anvil's feet the camera looks, in world units.")]
        [SerializeField] float focusHeight = 0.55f;

        Station _anvil;

        public event Action<bool> ActiveChanged;

        public bool IsActive => _anvil != null;

        /// <summary>Where the camera should look while active.</summary>
        public Vector3 FocusPoint => _anvil != null ? _anvil.Position + Vector3.up * focusHeight : Vector3.zero;

        public Station Anvil => _anvil;

        public void Enter(Station anvil)
        {
            if (anvil == null || IsActive) return;

            _anvil = anvil;
            player.FaceTowards(anvil.Position);
            player.Frozen = true;
            ActiveChanged?.Invoke(true);
        }

        public void Exit()
        {
            if (!IsActive) return;

            _anvil = null;
            player.Frozen = false;
            ActiveChanged?.Invoke(false);
        }

        void Update()
        {
            // Frozen players read no input, so watch the raw movement keys to know when to leave.
            if (IsActive && player.IsLeaveRequested()) Exit();
        }
    }
}
