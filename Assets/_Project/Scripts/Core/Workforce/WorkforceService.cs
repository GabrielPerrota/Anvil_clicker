using System;

namespace AnvilClicker.Core
{
    /// <summary>Owns hired apprentices and the passive forge points they produce.</summary>
    public sealed class WorkforceService
    {
        readonly GameState _state;
        readonly IGameBalance _balance;
        readonly IGameCatalog _catalog;
        readonly Wallet _wallet;
        readonly ModifierStack _modifiers;

        /// <summary>Raised after apprentices are hired.</summary>
        public event Action<IApprenticeDefinition, int> WorkforceChanged;

        public WorkforceService(GameState state, IGameBalance balance, IGameCatalog catalog, Wallet wallet, ModifierStack modifiers)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _balance = balance ?? throw new ArgumentNullException(nameof(balance));
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
            _modifiers = modifiers ?? throw new ArgumentNullException(nameof(modifiers));
        }

        public int GetCount(IApprenticeDefinition apprentice) =>
            _state.Apprentices.TryGetValue(apprentice.Id, out var count) ? count : 0;

        public bool IsUnlocked(IApprenticeDefinition apprentice) =>
            GetCount(apprentice) > 0 || _wallet.LifetimeGold >= apprentice.UnlockAtLifetimeGold;

        public double GetCost(IApprenticeDefinition apprentice, int quantity) =>
            EconomyFormulas.BulkCost(apprentice.BaseCost, apprentice.CostGrowth, GetCount(apprentice), quantity);

        public int GetMaxAffordable(IApprenticeDefinition apprentice) =>
            EconomyFormulas.MaxAffordable(apprentice.BaseCost, apprentice.CostGrowth, GetCount(apprentice), _wallet.Gold);

        /// <summary>2 raised to the number of milestones reached: 25 units → ×2, 50 → ×4…</summary>
        public double GetMilestoneMultiplier(int count)
        {
            var reached = 0;
            foreach (var milestone in _balance.ApprenticeMilestones)
            {
                if (count >= milestone) reached++;
            }
            return Math.Pow(2d, reached);
        }

        /// <summary>Forge points per second of one apprentice type, every multiplier included.</summary>
        public double GetForgePointsPerSecond(IApprenticeDefinition apprentice)
        {
            var count = GetCount(apprentice);
            if (count <= 0) return 0d;
            return count * apprentice.BaseForgePointsPerSecond * GetMilestoneMultiplier(count) * _modifiers.PassiveMultiplier;
        }

        /// <summary>Total passive production of the workshop.</summary>
        public double ForgePointsPerSecond
        {
            get
            {
                var total = 0d;
                foreach (var apprentice in _catalog.Apprentices)
                {
                    if (apprentice != null) total += GetForgePointsPerSecond(apprentice);
                }
                return total;
            }
        }

        /// <summary>Hires <paramref name="quantity"/> units at once, or nothing if any rule fails.</summary>
        public bool TryHire(IApprenticeDefinition apprentice, int quantity)
        {
            if (apprentice == null) throw new ArgumentNullException(nameof(apprentice));
            if (quantity <= 0 || !IsUnlocked(apprentice)) return false;
            if (!_wallet.TrySpend(GetCost(apprentice, quantity))) return false;

            var count = GetCount(apprentice) + quantity;
            _state.Apprentices[apprentice.Id] = count;
            WorkforceChanged?.Invoke(apprentice, count);
            return true;
        }
    }
}
