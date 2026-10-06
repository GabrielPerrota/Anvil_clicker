using AnvilClicker.Core;
using AnvilClicker.Runtime;
using UnityEngine;
using UnityEngine.UIElements;

namespace AnvilClicker.Presentation
{
    /// <summary>Shows gold, passive rate and the anvil's progress. Read-only: it never changes game state.</summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class HudPresenter : MonoBehaviour, IGameContextConsumer
    {
        [Tooltip("{0} = forge points per strike.")]
        [SerializeField] string strikePowerFormat = "Golpe: {0} PF";
        [Tooltip("{0} = weapons forged so far.")]
        [SerializeField] string forgedCountFormat = "Armas forjadas: {0}";
        [Tooltip("{0} = passive forge points per second.")]
        [SerializeField] string passiveRateFormat = "ouro · aprendizes: {0} PF/s";
        [SerializeField] string noPassiveText = "ouro";

        GameContext _context;
        Label _goldLabel;
        Label _rateLabel;
        Label _weaponLabel;
        Label _powerLabel;
        Label _forgedLabel;
        VisualElement _progressFill;

        public void Bind(GameContext context) => _context = context;

        void Start()
        {
            if (_context == null) return;

            var root = GetComponent<UIDocument>().rootVisualElement;
            _goldLabel = root.Q<Label>("gold-label");
            _rateLabel = root.Q<Label>("rate-label");
            _weaponLabel = root.Q<Label>("weapon-label");
            _powerLabel = root.Q<Label>("power-label");
            _forgedLabel = root.Q<Label>("forged-label");
            _progressFill = root.Q<VisualElement>("progress-fill");

            _context.Wallet.GoldChanged += OnGoldChanged;
            _context.Forge.ProgressChanged += OnProgressChanged;
            _context.Forge.WeaponForged += OnWeaponForged;
            _context.Modifiers.Changed += RefreshStats;
            _context.Workforce.WorkforceChanged += OnWorkforceChanged;

            _weaponLabel.text = _context.Forge.ActiveWeapon.DisplayName;
            OnGoldChanged(_context.Wallet.Gold);
            OnProgressChanged(_context.Forge.Progress01);
            RefreshForgedCount();
            RefreshStats();
        }

        void OnDestroy()
        {
            if (_context == null) return;
            _context.Wallet.GoldChanged -= OnGoldChanged;
            _context.Forge.ProgressChanged -= OnProgressChanged;
            _context.Forge.WeaponForged -= OnWeaponForged;
            _context.Modifiers.Changed -= RefreshStats;
            _context.Workforce.WorkforceChanged -= OnWorkforceChanged;
        }

        void OnGoldChanged(double gold) => _goldLabel.text = NumberFormatter.Format(gold);

        void OnProgressChanged(double progress01) =>
            _progressFill.style.width = Length.Percent((float)(progress01 * 100d));

        void OnWeaponForged(IWeaponDefinition weapon, long count) => RefreshForgedCount();

        void OnWorkforceChanged(IApprenticeDefinition apprentice, int count) => RefreshStats();

        void RefreshForgedCount() =>
            _forgedLabel.text = string.Format(forgedCountFormat, NumberFormatter.Format(_context.Forge.WeaponsForged));

        void RefreshStats()
        {
            _powerLabel.text = string.Format(strikePowerFormat, NumberFormatter.Format(_context.Forge.ClickPower));

            var rate = _context.Workforce.ForgePointsPerSecond;
            _rateLabel.text = rate > 0 ? string.Format(passiveRateFormat, NumberFormatter.Format(rate)) : noPassiveText;
        }
    }
}
