using UnityEngine;

namespace AnvilClicker.Presentation
{
    /// <summary>
    /// Trauma-based screen shake applied as an offset on top of the camera's resting position.
    /// Shake strength grows with trauma squared, so small hits stay subtle.
    /// </summary>
    public sealed class CameraShake : MonoBehaviour
    {
        [Tooltip("Accessibility: when off, Shake() does nothing.")]
        [SerializeField] bool shakeEnabled = true;
        [SerializeField] float maxOffset = 0.25f;
        [SerializeField] float maxRollDegrees = 1.5f;
        [Tooltip("Trauma lost per second.")]
        [SerializeField] float recovery = 2.5f;
        [SerializeField] float noiseFrequency = 25f;

        Vector3 _restPosition;
        Quaternion _restRotation;
        float _trauma;
        float _seed;

        public bool ShakeEnabled
        {
            get => shakeEnabled;
            set => shakeEnabled = value;
        }

        void Awake()
        {
            _restPosition = transform.localPosition;
            _restRotation = transform.localRotation;
            _seed = Random.value * 100f;
        }

        /// <param name="trauma">Amount in [0, 1] added to the current trauma.</param>
        public void Shake(float trauma)
        {
            if (!shakeEnabled) return;
            _trauma = Mathf.Clamp01(_trauma + trauma);
        }

        void LateUpdate()
        {
            if (_trauma <= 0f) return;

            _trauma = Mathf.Max(0f, _trauma - recovery * Time.deltaTime);
            var amount = _trauma * _trauma;
            var time = Time.time * noiseFrequency;

            var x = (Mathf.PerlinNoise(_seed, time) * 2f - 1f) * maxOffset * amount;
            var y = (Mathf.PerlinNoise(_seed + 1f, time) * 2f - 1f) * maxOffset * amount;
            var roll = (Mathf.PerlinNoise(_seed + 2f, time) * 2f - 1f) * maxRollDegrees * amount;

            transform.localPosition = _restPosition + new Vector3(x, y, 0f);
            transform.localRotation = _restRotation * Quaternion.Euler(0f, 0f, roll);
        }
    }
}
