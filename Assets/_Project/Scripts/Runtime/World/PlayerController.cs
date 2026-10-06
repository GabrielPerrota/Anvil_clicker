using AnvilClicker.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AnvilClicker.Runtime
{
    /// <summary>The blacksmith: WASD movement relative to the screen, with wall collisions and a facing sprite.</summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerController : MonoBehaviour
    {
        const string MoveActionName = "Player/Move";

        [SerializeField] float speed = 3.2f;
        [SerializeField] SpriteRenderer body;
        [Tooltip("One sprite per direction, in FacingDirection order: DownRight, DownLeft, UpLeft, UpRight.")]
        [SerializeField] Sprite[] facingSprites = new Sprite[4];
        [SerializeField] float bobHeight = 0.04f;
        [SerializeField] float bobSpeed = 14f;

        Rigidbody2D _rigidbody;
        InputAction _move;
        Vector2 _pathDirection;
        FacingDirection _facing = FacingDirection.DownRight;
        Vector3 _bodyRestPosition;
        float _bobTime;

        /// <summary>When true the blacksmith stands still (forge mode, for instance).</summary>
        public bool Frozen { get; set; }

        /// <summary>Raw keyboard input this frame, W = up on screen.</summary>
        public Vector2 KeyboardInput { get; private set; }

        /// <summary>Movement keys held right now, even while frozen (used to leave forge mode by walking away).</summary>
        public Vector2 RawInput { get; private set; }

        public bool IsMoving { get; private set; }

        public FacingDirection Facing => _facing;

        /// <summary>Position of the feet, which is also what the depth sorting uses.</summary>
        public Vector3 Feet => transform.position;

        void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _rigidbody.gravityScale = 0f;
            _rigidbody.freezeRotation = true;
            _rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            if (body != null) _bodyRestPosition = body.transform.localPosition;
        }

        void OnEnable()
        {
            var actions = InputSystem.actions;
            if (actions == null)
            {
                Debug.LogError("No project-wide input actions are assigned.", this);
                enabled = false;
                return;
            }

            _move = actions.FindAction(MoveActionName, throwIfNotFound: true);
            _move.Enable();
        }

        /// <summary>True while a movement key is held, whatever the frozen state.</summary>
        public bool IsLeaveRequested() => RawInput.sqrMagnitude > 0.01f;

        /// <summary>Walks in a world-space direction without keyboard input (used by the path follower). Zero stops.</summary>
        public void SetPathDirection(Vector2 worldDirection) => _pathDirection = worldDirection;

        /// <summary>Turns to look at a world position (when starting to use a station).</summary>
        public void FaceTowards(Vector3 worldPosition)
        {
            var delta = worldPosition - transform.position;
            _facing = IsoMath.GetFacing(delta.x, delta.y, _facing);
            ApplyFacing();
        }

        public void TeleportTo(Vector3 position)
        {
            transform.position = position;
            if (_rigidbody != null) _rigidbody.position = position;
        }

        void Update()
        {
            RawInput = _move != null ? _move.ReadValue<Vector2>() : Vector2.zero;
            KeyboardInput = Frozen ? Vector2.zero : RawInput;

            var direction = CurrentDirection();
            IsMoving = !Frozen && direction.sqrMagnitude > 1e-6f;

            if (IsMoving)
            {
                _facing = IsoMath.GetFacing(direction.x, direction.y, _facing);
                _bobTime += Time.deltaTime * bobSpeed;
            }
            else
            {
                _bobTime = 0f;
            }

            ApplyFacing();
            if (body != null) body.transform.localPosition = _bodyRestPosition + Vector3.up * (Mathf.Abs(Mathf.Sin(_bobTime)) * bobHeight);
        }

        void FixedUpdate()
        {
            _rigidbody.linearVelocity = Frozen ? Vector2.zero : CurrentDirection() * speed;
        }

        /// <summary>Keyboard wins over the path; both are turned into the 2:1 isometric world velocity.</summary>
        Vector2 CurrentDirection()
        {
            if (Frozen) return Vector2.zero;

            if (KeyboardInput.sqrMagnitude > 0.01f)
            {
                var (x, y) = IsoMath.ScreenInputToWorld(KeyboardInput.x, KeyboardInput.y);
                return new Vector2((float)x, (float)y);
            }

            if (_pathDirection.sqrMagnitude > 1e-6f)
            {
                // A world-space direction already is a screen direction; undo the squash so ScreenInputToWorld can re-apply it.
                var (x, y) = IsoMath.ScreenInputToWorld(_pathDirection.x, _pathDirection.y / IsoMath.VerticalSquash);
                return new Vector2((float)x, (float)y);
            }

            return Vector2.zero;
        }

        void ApplyFacing()
        {
            var index = (int)_facing;
            if (body != null && facingSprites != null && index < facingSprites.Length && facingSprites[index] != null)
                body.sprite = facingSprites[index];
        }
    }
}
