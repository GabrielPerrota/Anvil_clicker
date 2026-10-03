using System;

namespace AnvilClicker.Core
{
    /// <summary>
    /// Composes the game services around one <see cref="GameState"/>.
    /// Built once by the composition root and handed to whoever needs it; never accessed statically.
    /// </summary>
    public sealed class GameContext
    {
        public GameContext(GameState state, IForgeBalance balance, IWeaponCatalog catalog, IRandom random)
        {
            State = state ?? throw new ArgumentNullException(nameof(state));
            Catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));

            Wallet = new Wallet(state);
            Forge = new ForgeService(state, balance, catalog, random);

            // M1 rule: finished weapons are sold on the spot. Replaced by the sales counter in M4.
            Forge.WeaponForged += SellImmediately;
        }

        public GameState State { get; }

        public IWeaponCatalog Catalog { get; }

        public Wallet Wallet { get; }

        public ForgeService Forge { get; }

        void SellImmediately(IWeaponDefinition weapon, long count) => Wallet.Add(weapon.BaseValue * count);
    }
}
