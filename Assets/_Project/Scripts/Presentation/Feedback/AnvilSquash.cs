using UnityEngine;

namespace AnvilClicker.Presentation
{
    /// <summary>Squash-and-stretch punch on the anvil's scale. Pivot the sprite at its base so it squashes into the floor.</summary>
    public sealed class AnvilSquash : MonoBehaviour
    {
        [SerializeField] float duration = 0.22f;
        [Tooltip("Oscillations during one punch.")]
        [SerializeField] float frequency = 2.5f;

        Vector3 _baseScale;
        float _strength;
        float _elapsed;

        void Awake() => _baseScale = transform.localScale;

        /// <param name="strength">Fraction of the height squashed at the start, e.g. 0.12.</param>
        public void Punch(float strength)
        {
            var remaining = _strength * (1f - Mathf.Clamp01(_elapsed / duration));
            _strength = Mathf.Max(remaining, strength);
            _elapsed = 0f;
        }

        void Update()
        {
            if (_strength <= 0f) return;

            _elapsed += Time.deltaTime;
            if (_elapsed >= duration)
            {
                _strength = 0f;
                transform.localScale = _baseScale;
                return;
            }

            var t = _elapsed / duration;
            var decay = (1f - t) * (1f - t);
            var offset = _strength * decay * Mathf.Cos(t * frequency * Mathf.PI * 2f);
            transform.localScale = new Vector3(_baseScale.x * (1f + offset), _baseScale.y * (1f - offset), _baseScale.z);
        }
    }
}
