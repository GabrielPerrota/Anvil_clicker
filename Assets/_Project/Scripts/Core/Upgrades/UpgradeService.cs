using System;

namespace AnvilClicker.Core
{
    /// <summary>Owns upgrade levels: pricing, unlocking and purchasing.</summary>
    public sealed class UpgradeService
    {
        readonly GameState _state;
        readonly Wallet _wallet;

        /// <summary>Raised with the upgrade and its new level after a purchase.</summary>
        public event Action<IUpgradeDefinition, int> UpgradePurchased;

        public UpgradeService(GameState state, Wallet wallet)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
        }

        public int GetLevel(IUpgradeDefinition upgrade) =>
            _state.UpgradeLevels.TryGetValue(upgrade.Id, out var level) ? level : 0;

        /// <summary>Levels still available; int.MaxValue when the upgrade has no cap.</summary>
        public int GetRemainingLevels(IUpgradeDefinition upgrade) =>
            upgrade.MaxLevel <= 0 ? int.MaxValue : Math.Max(0, upgrade.MaxLevel - GetLevel(upgrade));

        public bool IsMaxed(IUpgradeDefinition upgrade) => GetRemainingLevels(upgrade) == 0;

        /// <summary>Visible in the shop once enough gold was ever earned (or after the first purchase).</summary>
        public bool IsUnlocked(IUpgradeDefinition upgrade) =>
            GetLevel(upgrade) > 0 || _wallet.LifetimeGold >= upgrade.UnlockAtLifetimeGold;

        public double GetCost(IUpgradeDefinition upgrade, int quantity) =>
            EconomyFormulas.BulkCost(upgrade.BaseCost, upgrade.CostGrowth, GetLevel(upgrade), quantity);

        public int GetMaxAffordable(IUpgradeDefinition upgrade) =>
            EconomyFormulas.MaxAffordable(upgrade.BaseCost, upgrade.CostGrowth, GetLevel(upgrade), _wallet.Gold, GetRemainingLevels(upgrade));

        /// <summary>Buys <paramref name="quantity"/> levels at once, or nothing if any rule fails.</summary>
        public bool TryBuy(IUpgradeDefinition upgrade, int quantity)
        {
            if (upgrade == null) throw new ArgumentNullException(nameof(upgrade));
            if (quantity <= 0 || !IsUnlocked(upgrade) || quantity > GetRemainingLevels(upgrade)) return false;
            if (!_wallet.TrySpend(GetCost(upgrade, quantity))) return false;

            var level = GetLevel(upgrade) + quantity;
            _state.UpgradeLevels[upgrade.Id] = level;
            UpgradePurchased?.Invoke(upgrade, level);
            return true;
        }
    }
}
