using System;

namespace AnvilClicker.Core
{
    /// <summary>
    /// Composes the game services around one <see cref="GameState"/>.
    /// Built once by the composition root and handed to whoever needs it; never accessed statically.
    /// </summary>
    public sealed class GameContext
    {
        /// <summary>Raised after offline progress was applied (on load, or from debug tools).</summary>
        public event Action<OfflineReport> OfflineProgressApplied;

        public GameContext(GameState state, IGameBalance balance, IGameCatalog catalog, IRandom random)
        {
            State = state ?? throw new ArgumentNullException(nameof(state));
            Balance = balance ?? throw new ArgumentNullException(nameof(balance));
            Catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));

            Wallet = new Wallet(state);
            Modifiers = new ModifierStack(state, balance, catalog);
            Upgrades = new UpgradeService(state, Wallet);
            Workforce = new WorkforceService(state, balance, catalog, Wallet, Modifiers);
            Forge = new ForgeService(state, Modifiers, catalog, balance.StartingWeaponId, random);

            Upgrades.UpgradePurchased += (_, _) => Modifiers.Recalculate();

            // Finished weapons are sold on the spot until the sales counter arrives in M4.
            Forge.WeaponForged += SellImmediately;
        }

        public GameState State { get; }

        public IGameBalance Balance { get; }

        public IGameCatalog Catalog { get; }

        public Wallet Wallet { get; }

        public ModifierStack Modifiers { get; }

        public UpgradeService Upgrades { get; }

        public WorkforceService Workforce { get; }

        public ForgeService Forge { get; }

        /// <summary>The most recent offline report, or <see cref="OfflineReport.Empty"/>.</summary>
        public OfflineReport LastOfflineReport { get; private set; } = OfflineReport.Empty;

        /// <summary>Advances passive production by <paramref name="deltaSeconds"/>.</summary>
        public void Tick(double deltaSeconds)
        {
            if (double.IsNaN(deltaSeconds) || deltaSeconds <= 0) return;

            var points = Workforce.ForgePointsPerSecond * deltaSeconds;
            if (points > 0 && !double.IsInfinity(points)) Forge.AddForgePoints(points);
        }

        /// <summary>Credits apprentice production for time spent away and records what was earned.</summary>
        public OfflineReport ApplyOfflineProgress(TimeSpan away)
        {
            var gain = OfflineProgressCalculator.Calculate(away, Workforce.ForgePointsPerSecond, Balance.OfflineCapHours, Balance.OfflineEfficiency);

            var goldBefore = Wallet.Gold;
            var weaponsBefore = Forge.WeaponsForged;
            if (gain.ForgePoints > 0) Forge.AddForgePoints(gain.ForgePoints);

            var report = new OfflineReport(gain.Away, gain.Credited, gain.ForgePoints, Forge.WeaponsForged - weaponsBefore, Wallet.Gold - goldBefore);
            LastOfflineReport = report;
            OfflineProgressApplied?.Invoke(report);
            return report;
        }

        void SellImmediately(IWeaponDefinition weapon, long count) =>
            Wallet.Add(weapon.BaseValue * count * Modifiers.SellMultiplier);
    }
}
