using AnvilClicker.Core;
using AnvilClicker.Runtime;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace AnvilClicker.Presentation
{
    /// <summary>
    /// The forge's fire: a point light that flickers, and burns brighter and wider with every
    /// "forge speed" upgrade the player owns.
    /// </summary>
    public sealed class ForgeGlow : MonoBehaviour, IGameContextConsumer
    {
        [SerializeField] Light2D fire;
        [SerializeField] float baseIntensity = 1.1f;
        [SerializeField] float baseOuterRadius = 3.2f;
        [Tooltip("Extra intensity per doubling of the forge speed factor.")]
        [SerializeField] float intensityPerDoubling = 0.35f;
        [SerializeField] float maxHeat = 3f;
        [SerializeField] float flickerAmount = 0.12f;
        [SerializeField] float flickerSpeed = 7f;

        GameContext _context;
        float _heat; // 0 = cold start, 1 = one doubling of speed
        float _seed;

        public void Bind(GameContext context) => _context = context;

        void Start()
        {
            _seed = Random.value * 50f;
            if (_context == null) return;

            _context.Modifiers.Changed += RefreshHeat;
            RefreshHeat();
        }

        void OnDestroy()
        {
            if (_context != null) _context.Modifiers.Changed -= RefreshHeat;
        }

        void RefreshHeat() =>
            _heat = Mathf.Clamp((float)System.Math.Log(System.Math.Max(1d, _context.Modifiers.ForgeSpeedFactor), 2d), 0f, maxHeat);

        void Update()
        {
            if (fire == null) return;

            var flicker = 1f + (Mathf.PerlinNoise(_seed, Time.time * flickerSpeed) - 0.5f) * 2f * flickerAmount;
            fire.intensity = (baseIntensity + _heat * intensityPerDoubling) * flicker;
            fire.pointLightOuterRadius = baseOuterRadius + _heat * 0.4f;
        }
    }
}
