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
        readonly IForgeStats _stats;
        readonly IRandom _random;

        /// <summary>Raised after every strike, once its points have been applied.</summary>
        public event Action<StrikeResult> StrikeApplied;

        /// <summary>Raised with the weapon and how many units were finished in one go.</summary>
        public event Action<IWeaponDefinition, long> WeaponForged;

        /// <summary>Raised with the new progress of the current weapon, in [0, 1).</summary>
        public event Action<double> ProgressChanged;

        /// <param name="startingWeaponId">
        /// Weapon used for a new game, or when the saved weapon no longer exists (its progress is then dropped).
        /// </param>
        public ForgeService(GameState state, IForgeStats stats, IWeaponCatalog catalog, string startingWeaponId, IRandom random)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _stats = stats ?? throw new ArgumentNullException(nameof(stats));
            _random = random ?? throw new ArgumentNullException(nameof(random));
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));

            if (string.IsNullOrEmpty(state.ActiveWeaponId) || !catalog.TryGetWeapon(state.ActiveWeaponId, out var weapon))
            {
                if (!catalog.TryGetWeapon(startingWeaponId, out weapon))
                    throw new InvalidOperationException($"Starting weapon '{startingWeaponId}' was not found in the catalog.");
                if (!string.IsNullOrEmpty(state.ActiveWeaponId)) state.ForgeProgress = 0d;
            }
            Guard.PositiveFinite(weapon.ForgePointsRequired, nameof(weapon.ForgePointsRequired));

            ActiveWeapon = weapon;
            _state.ActiveWeaponId = weapon.Id;
        }

        public IWeaponDefinition ActiveWeapon { get; }

        /// <summary>Forge points added by a non-critical strike.</summary>
        public double ClickPower => _stats.ClickPower;

        public double Progress => _state.ForgeProgress;

        public double Progress01 => Math.Min(_state.ForgeProgress / ActiveWeapon.ForgePointsRequired, 1d);

        public long WeaponsForged => _state.WeaponsForged;

        public StrikeResult Strike()
        {
            var isCritical = _random.NextDouble() < _stats.CritChance;
            var power = ClickPower * (isCritical ? _stats.CritMultiplier : 1d);

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
