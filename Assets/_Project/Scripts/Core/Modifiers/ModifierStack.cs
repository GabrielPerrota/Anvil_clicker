using System;

namespace AnvilClicker.Core
{
    /// <summary>Strike numbers the forge needs, after every upgrade has been applied.</summary>
    public interface IForgeStats
    {
        double ClickPower { get; }

        double CritChance { get; }

        double CritMultiplier { get; }
    }

    /// <summary>
    /// Folds the base balance and every owned upgrade into the final stats. Results are cached and only
    /// recomputed by <see cref="Recalculate"/>, which <see cref="GameContext"/> calls after each purchase.
    /// </summary>
    public sealed class ModifierStack : IForgeStats
    {
        readonly GameState _state;
        readonly IGameBalance _balance;
        readonly IGameCatalog _catalog;

        /// <summary>Raised after the stats were recomputed.</summary>
        public event Action Changed;

        public ModifierStack(GameState state, IGameBalance balance, IGameCatalog catalog)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _balance = balance ?? throw new ArgumentNullException(nameof(balance));
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            Recalculate();
        }

        public double ClickPower { get; private set; }

        public double CritChance { get; private set; }

        public double CritMultiplier { get; private set; }

        /// <summary>Multiplier on apprentice production (includes forge speed).</summary>
        public double PassiveMultiplier { get; private set; }

        public double SellMultiplier { get; private set; }

        public void Recalculate()
        {
            double clickFlat = 0, critChanceFlat = 0, critMultiplierFlat = 0;
            double clickFactor = 1, forgeSpeedFactor = 1, passiveFactor = 1, sellFactor = 1;

            foreach (var upgrade in _catalog.Upgrades)
            {
                if (upgrade == null || !_state.UpgradeLevels.TryGetValue(upgrade.Id, out var level) || level <= 0) continue;

                foreach (var effect in upgrade.Effects)
                {
                    var amount = effect.ValuePerLevel * level;
                    switch (effect.Type)
                    {
                        case ModifierType.ClickPowerFlat: clickFlat += amount; break;
                        case ModifierType.CritChanceFlat: critChanceFlat += amount; break;
                        case ModifierType.CritMultiplierFlat: critMultiplierFlat += amount; break;
                        case ModifierType.ClickPowerMultiplier: clickFactor *= 1d + amount; break;
                        case ModifierType.ForgeSpeedMultiplier: forgeSpeedFactor *= 1d + amount; break;
                        case ModifierType.PassiveMultiplier: passiveFactor *= 1d + amount; break;
                        case ModifierType.SellValueMultiplier: sellFactor *= 1d + amount; break;
                        default: throw new ArgumentOutOfRangeException(nameof(effect.Type), effect.Type, "Unknown modifier type.");
                    }
                }
            }

            ClickPower = Math.Max(0d, (_balance.BaseClickPower + clickFlat) * clickFactor * forgeSpeedFactor);
            CritChance = Math.Min(1d, Math.Max(0d, _balance.CritChance + critChanceFlat));
            CritMultiplier = Math.Max(1d, _balance.CritMultiplier + critMultiplierFlat);
            PassiveMultiplier = Math.Max(0d, passiveFactor * forgeSpeedFactor);
            SellMultiplier = Math.Max(0d, sellFactor);

            Changed?.Invoke();
        }
    }
}
