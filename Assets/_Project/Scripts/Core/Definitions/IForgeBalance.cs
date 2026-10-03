namespace AnvilClicker.Core
{
    /// <summary>Tunable numbers for the anvil. Implemented by a ScriptableObject in the Data assembly.</summary>
    public interface IForgeBalance
    {
        /// <summary>Forge points added by a single strike before multipliers.</summary>
        double BaseClickPower { get; }

        /// <summary>Probability in [0, 1] that a strike is critical.</summary>
        double CritChance { get; }

        /// <summary>Multiplier applied to a critical strike's power.</summary>
        double CritMultiplier { get; }

        /// <summary>Weapon on the anvil when a new game starts.</summary>
        string StartingWeaponId { get; }
    }
}
