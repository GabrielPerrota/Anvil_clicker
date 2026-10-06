using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace AnvilClicker.Presentation
{
    /// <summary>
    /// The visual parts of the anvil station that strikes animate: sparks, squash and a warm glow.
    /// Lives on the anvil prefab; <see cref="StrikeFeedback"/> finds it through the forge mode.
    /// </summary>
    public sealed class AnvilRig : MonoBehaviour
    {
        [SerializeField] Transform strikePoint;
        [SerializeField] ParticleSystem sparks;
        [SerializeField] AnvilSquash squash;
        [SerializeField] Light2D glow;
        [SerializeField] float glowRecovery = 6f;

        float _baseIntensity;

        public Transform StrikePoint => strikePoint != null ? strikePoint : transform;
        public ParticleSystem Sparks => sparks;
        public AnvilSquash Squash => squash;

        void Awake()
        {
            if (glow != null) _baseIntensity = glow.intensity;
        }

        /// <summary>Brightens the glow for a moment; it fades back on its own.</summary>
        public void Flash(float extraIntensity)
        {
            if (glow != null) glow.intensity = _baseIntensity + extraIntensity;
        }

        void Update()
        {
            if (glow == null) return;
            glow.intensity = Mathf.Lerp(glow.intensity, _baseIntensity, 1f - Mathf.Exp(-glowRecovery * Time.deltaTime));
        }
    }
}
