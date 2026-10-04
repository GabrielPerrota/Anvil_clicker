using System.Collections.Generic;

namespace AnvilClicker.Core
{
    /// <summary>Global tunable numbers. Implemented by a ScriptableObject in the Data assembly.</summary>
    public interface IGameBalance
    {
        /// <summary>Forge points added by a single strike before upgrades.</summary>
        double BaseClickPower { get; }

        /// <summary>Probability in [0, 1] that a strike is critical, before upgrades.</summary>
        double CritChance { get; }

        /// <summary>Multiplier applied to a critical strike's power, before upgrades.</summary>
        double CritMultiplier { get; }

        /// <summary>Weapon on the anvil when a new game starts.</summary>
        string StartingWeaponId { get; }

        /// <summary>Longest absence, in hours, that still earns offline progress.</summary>
        double OfflineCapHours { get; }

        /// <summary>Fraction in [0, 1] of apprentice production earned while the game is closed.</summary>
        double OfflineEfficiency { get; }

        double AutosaveIntervalSeconds { get; }

        /// <summary>Owned counts (ascending) at which one apprentice type's production doubles.</summary>
        IReadOnlyList<int> ApprenticeMilestones { get; }
    }
}
