using System;

namespace AnvilClicker.Core
{
    /// <summary>Owns which rooms of the workshop have been built.</summary>
    public sealed class RoomService
    {
        readonly GameState _state;
        readonly IGameCatalog _catalog;
        readonly Wallet _wallet;

        /// <summary>Raised after a room is bought (not for the free rooms granted at startup).</summary>
        public event Action<IRoomDefinition> RoomUnlocked;

        public RoomService(GameState state, IGameCatalog catalog, Wallet wallet)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));

            // Free rooms (the first workshop) exist from the very first run.
            foreach (var room in _catalog.Rooms)
            {
                if (room != null && room.Cost <= 0 && !_state.UnlockedRooms.Contains(room.Id)) _state.UnlockedRooms.Add(room.Id);
            }
        }

        public bool IsUnlocked(IRoomDefinition room) => _state.UnlockedRooms.Contains(room.Id);

        /// <summary>Shown in the shop once enough gold was ever earned (or once it is built).</summary>
        public bool IsAvailable(IRoomDefinition room) => IsUnlocked(room) || _wallet.LifetimeGold >= room.UnlockAtLifetimeGold;

        public double GetCost(IRoomDefinition room) => room.Cost;

        public bool CanBuy(IRoomDefinition room) => !IsUnlocked(room) && IsAvailable(room) && _wallet.CanAfford(room.Cost);

        public bool TryBuy(IRoomDefinition room)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));
            if (IsUnlocked(room) || !IsAvailable(room) || !_wallet.TrySpend(room.Cost)) return false;

            _state.UnlockedRooms.Add(room.Id);
            RoomUnlocked?.Invoke(room);
            return true;
        }
    }
}
