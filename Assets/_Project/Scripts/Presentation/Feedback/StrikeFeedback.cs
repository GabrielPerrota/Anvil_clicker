using AnvilClicker.Core;
using AnvilClicker.Runtime;
using UnityEngine;

namespace AnvilClicker.Presentation
{
    /// <summary>
    /// Juice for the anvil: sparks, squash, floating numbers, sound, light flash and (on crits) screen shake.
    /// Listens to forge events only; nothing here affects game logic.
    /// The anvil itself is a station spawned with its room, so its effects are found through <see cref="ForgeMode"/>.
    /// </summary>
    public sealed class StrikeFeedback : MonoBehaviour, IGameContextConsumer
    {
        [Header("References")]
        [SerializeField] ForgeMode forgeMode;
        [SerializeField] CameraShake cameraShake;
        [SerializeField] FloatingTextLayer floatingText;
        [SerializeField] ProceduralSfx sfx;

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

        [Header("Text")]
        [Tooltip("Horizontal random spread of floating numbers, in world units.")]
        [SerializeField] float textJitter = 0.35f;
        [Tooltip("{0} = gold earned.")]
        [SerializeField] string rewardFormat = "+{0} ouro";
        [Tooltip("Earnings are grouped into one popup (and one chime) per interval, so apprentices finishing a weapon every frame don't flood the screen.")]
        [SerializeField] float rewardInterval = 0.35f;

        GameContext _context;
        AnvilRig _rig;
        double _lastGold;
        double _pendingReward;
        float _sinceReward;

        public void Bind(GameContext context) => _context = context;

        void Start()
        {
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
            _sinceReward += Time.deltaTime;
            if (_pendingReward > 0 && _sinceReward >= rewardInterval) FlushReward();
        }

        /// <summary>The anvil in use, or the first one in the workshop (apprentices earn gold while nobody is at the anvil).</summary>
        AnvilRig CurrentRig()
        {
            if (forgeMode != null && forgeMode.Anvil != null) return forgeMode.Anvil.GetComponent<AnvilRig>();
            if (_rig == null) _rig = FindFirstObjectByType<AnvilRig>();
            return _rig;
        }

        void OnStrikeApplied(StrikeResult result)
        {
            var critical = result.IsCritical;
            var rig = CurrentRig();
            var origin = rig != null ? rig.StrikePoint.position : transform.position;

            if (rig != null)
            {
                if (rig.Sparks != null) rig.Sparks.Emit(critical ? sparksCritical : sparksNormal);
                if (rig.Squash != null) rig.Squash.Punch(critical ? squashCritical : squashNormal);
                rig.Flash(critical ? lightFlashCritical : lightFlash);
            }

            if (sfx != null) sfx.PlayStrike(critical);
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

            var rig = CurrentRig();
            if (floatingText != null && rig != null)
            {
                var origin = rig.StrikePoint.position + Vector3.up * 0.5f;
                floatingText.Show(origin, string.Format(rewardFormat, NumberFormatter.Format(amount)), FloatingTextStyle.Reward);
            }
        }
    }
}
