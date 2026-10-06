using AnvilClicker.Core;
using AnvilClicker.Runtime;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace AnvilClicker.Presentation
{
    /// <summary>
    /// Juice for the anvil: sparks, squash, floating numbers, sound, light flash and (on crits) screen shake.
    /// Listens to forge events only; nothing here affects game logic.
    /// </summary>
    public sealed class StrikeFeedback : MonoBehaviour, IGameContextConsumer
    {
        [Header("References")]
        [SerializeField] Transform strikePoint;
        [SerializeField] ParticleSystem sparks;
        [SerializeField] AnvilSquash squash;
        [SerializeField] CameraShake cameraShake;
        [SerializeField] FloatingTextLayer floatingText;
        [SerializeField] ProceduralSfx sfx;
        [SerializeField] Light2D forgeLight;

        [Header("Normal strike")]
        [SerializeField] int sparksNormal = 10;
        [SerializeField] float squashNormal = 0.10f;

        [Header("Critical strike")]
        [SerializeField] int sparksCritical = 45;
        [SerializeField] float squashCritical = 0.22f;
        [SerializeField] float shakeCritical = 0.55f;

        [Header("Light")]
        [SerializeField] float lightFlash = 1.6f;
        [SerializeField] float lightFlashCritical = 4f;
        [SerializeField] float lightRecovery = 6f;

        [Header("Text")]
        [Tooltip("Horizontal random spread of floating numbers, in world units.")]
        [SerializeField] float textJitter = 0.35f;
        [Tooltip("{0} = gold earned.")]
        [SerializeField] string rewardFormat = "+{0} ouro";
        [Tooltip("Earnings are grouped into one popup (and one chime) per interval, so apprentices finishing a weapon every frame don't flood the screen.")]
        [SerializeField] float rewardInterval = 0.35f;

        GameContext _context;
        float _baseLightIntensity;
        double _lastGold;
        double _pendingReward;
        float _sinceReward;

        public void Bind(GameContext context) => _context = context;

        void Start()
        {
            if (forgeLight != null) _baseLightIntensity = forgeLight.intensity;
            if (_context == null) return;

            _lastGold = _context.Wallet.Gold;
            _context.Forge.StrikeApplied += OnStrikeApplied;
            _context.Wallet.GoldChanged += OnGoldChanged;
        }

        void OnDestroy()
        {
            if (_context == null) return;
            _context.Forge.StrikeApplied -= OnStrikeApplied;
            _context.Wallet.GoldChanged -= OnGoldChanged;
        }

        void Update()
        {
            if (forgeLight != null)
                forgeLight.intensity = Mathf.Lerp(forgeLight.intensity, _baseLightIntensity, 1f - Mathf.Exp(-lightRecovery * Time.deltaTime));

            _sinceReward += Time.deltaTime;
            if (_pendingReward > 0 && _sinceReward >= rewardInterval) FlushReward();
        }

        void OnStrikeApplied(StrikeResult result)
        {
            var critical = result.IsCritical;
            var origin = strikePoint != null ? strikePoint.position : transform.position;

            if (sparks != null) sparks.Emit(critical ? sparksCritical : sparksNormal);
            if (squash != null) squash.Punch(critical ? squashCritical : squashNormal);
            if (sfx != null) sfx.PlayStrike(critical);
            if (forgeLight != null) forgeLight.intensity = _baseLightIntensity + (critical ? lightFlashCritical : lightFlash);
            if (critical && cameraShake != null) cameraShake.Shake(shakeCritical);

            if (floatingText != null)
            {
                var jitter = new Vector3(Random.Range(-textJitter, textJitter), 0f, 0f);
                var text = "+" + NumberFormatter.Format(result.Power) + (critical ? "!" : "");
                floatingText.Show(origin + jitter, text, critical ? FloatingTextStyle.Critical : FloatingTextStyle.Normal);
            }
        }

        /// <summary>Only increases count as earnings; spending in the shop lowers gold silently.</summary>
        void OnGoldChanged(double gold)
        {
            var delta = gold - _lastGold;
            _lastGold = gold;
            if (delta > 0) _pendingReward += delta;
        }

        void FlushReward()
        {
            var amount = _pendingReward;
            _pendingReward = 0;
            _sinceReward = 0f;

            if (sfx != null) sfx.PlayForged();

            if (floatingText != null)
            {
                var origin = (strikePoint != null ? strikePoint.position : transform.position) + Vector3.up * 0.5f;
                floatingText.Show(origin, string.Format(rewardFormat, NumberFormatter.Format(amount)), FloatingTextStyle.Reward);
            }
        }
    }
}
