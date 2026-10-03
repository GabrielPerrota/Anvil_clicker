using System;

namespace AnvilClicker.Core
{
    /// <summary>
    /// The anvil: turns forge points (from strikes, later from apprentices) into finished weapons.
    /// Leftover points carry over to the next weapon.
    /// </summary>
    public sealed class ForgeService
    {
        readonly GameState _state;
        readonly IForgeBalance _balance;
        readonly IRandom _random;

        /// <summary>Raised after every strike, once its points have been applied.</summary>
        public event Action<StrikeResult> StrikeApplied;

        /// <summary>Raised with the weapon and how many units were finished in one go.</summary>
        public event Action<IWeaponDefinition, long> WeaponForged;

        /// <summary>Raised with the new progress of the current weapon, in [0, 1).</summary>
        public event Action<double> ProgressChanged;

        public ForgeService(GameState state, IForgeBalance balance, IWeaponCatalog catalog, IRandom random)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _balance = balance ?? throw new ArgumentNullException(nameof(balance));
            _random = random ?? throw new ArgumentNullException(nameof(random));
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));

            Guard.NonNegativeFinite(balance.BaseClickPower, nameof(balance.BaseClickPower));
            Guard.PositiveFinite(balance.CritMultiplier, nameof(balance.CritMultiplier));

            var weaponId = string.IsNullOrEmpty(state.ActiveWeaponId) ? balance.StartingWeaponId : state.ActiveWeaponId;
            if (!catalog.TryGetWeapon(weaponId, out var weapon))
                throw new InvalidOperationException($"Weapon '{weaponId}' was not found in the catalog.");
            Guard.PositiveFinite(weapon.ForgePointsRequired, nameof(weapon.ForgePointsRequired));

            ActiveWeapon = weapon;
            _state.ActiveWeaponId = weapon.Id;
        }

        public IWeaponDefinition ActiveWeapon { get; }

        /// <summary>Forge points added by a non-critical strike.</summary>
        public double ClickPower => _balance.BaseClickPower;

        public double Progress => _state.ForgeProgress;

        public double Progress01 => Math.Min(_state.ForgeProgress / ActiveWeapon.ForgePointsRequired, 1d);

        public long WeaponsForged => _state.WeaponsForged;

        public StrikeResult Strike()
        {
            var isCritical = _random.NextDouble() < _balance.CritChance;
            var power = ClickPower * (isCritical ? _balance.CritMultiplier : 1d);

            var completed = AddForgePoints(power);

            var result = new StrikeResult(power, isCritical, completed, Progress01);
            StrikeApplied?.Invoke(result);
            return result;
        }

        /// <summary>Adds forge points to the current weapon and finishes as many units as they pay for.</summary>
        /// <returns>Number of weapons finished.</returns>
        public long AddForgePoints(double points)
        {
            Guard.NonNegativeFinite(points, nameof(points));
            if (points == 0) return 0;

            var required = ActiveWeapon.ForgePointsRequired;
            var progress = _state.ForgeProgress + points;
            var completed = (long)Math.Floor(progress / required);

            if (completed > 0)
            {
                // Clamp to guard against floating point drift leaving a tiny negative or a full bar.
                progress = Math.Max(0d, progress - completed * required);
                if (progress >= required) progress = 0d;
            }

            _state.ForgeProgress = progress;

            if (completed > 0)
            {
                _state.WeaponsForged += completed;
                WeaponForged?.Invoke(ActiveWeapon, completed);
            }

            ProgressChanged?.Invoke(Progress01);
            return completed;
        }
    }
}
