using System;

namespace AnvilClicker.Core
{
    /// <summary>Owns every change to the player's gold.</summary>
    public sealed class Wallet
    {
        readonly GameState _state;

        /// <summary>Raised with the new balance whenever gold changes.</summary>
        public event Action<double> GoldChanged;

        public Wallet(GameState state)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
        }

        public double Gold => _state.Gold;

        public double LifetimeGold => _state.LifetimeGold;

        public bool CanAfford(double amount)
        {
            Guard.NonNegativeFinite(amount, nameof(amount));
            return _state.Gold >= amount;
        }

        /// <summary>Adds earned gold. Earnings also count towards <see cref="LifetimeGold"/>.</summary>
        public void Add(double amount)
        {
            Guard.NonNegativeFinite(amount, nameof(amount));
            if (amount == 0) return;

            _state.Gold += amount;
            _state.LifetimeGold += amount;
            GoldChanged?.Invoke(_state.Gold);
        }

        /// <summary>Spends gold if the balance allows it. Returns false and changes nothing otherwise.</summary>
        public bool TrySpend(double amount)
        {
            if (!CanAfford(amount)) return false;
            if (amount == 0) return true;

            _state.Gold -= amount;
            GoldChanged?.Invoke(_state.Gold);
            return true;
        }
    }
}
