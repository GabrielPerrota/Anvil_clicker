namespace AnvilClicker.Core
{
    public enum ModifierType
    {
        /// <summary>Forge points added to every strike.</summary>
        ClickPowerFlat,

        /// <summary>Multiplies strike power.</summary>
        ClickPowerMultiplier,

        /// <summary>Multiplies all forge points, from strikes and apprentices alike.</summary>
        ForgeSpeedMultiplier,

        /// <summary>Added to the critical strike chance (0.01 = +1 percentage point).</summary>
        CritChanceFlat,

        /// <summary>Added to the critical strike multiplier.</summary>
        CritMultiplierFlat,

        /// <summary>Multiplies apprentice production.</summary>
        PassiveMultiplier,

        /// <summary>Multiplies the gold received for each weapon.</summary>
        SellValueMultiplier
    }

    /// <summary>
    /// One effect of an upgrade. Flat types add <see cref="ValuePerLevel"/> × level; multiplier types
    /// contribute a factor of 1 + <see cref="ValuePerLevel"/> × level (0.1 per level = +10% per level).
    /// Factors from different upgrades multiply together.
    /// </summary>
    public readonly struct UpgradeEffect
    {
        public UpgradeEffect(ModifierType type, double valuePerLevel)
        {
            Type = type;
            ValuePerLevel = valuePerLevel;
        }

        public ModifierType Type { get; }

        public double ValuePerLevel { get; }

        public static bool IsMultiplier(ModifierType type) =>
            type == ModifierType.ClickPowerMultiplier
            || type == ModifierType.ForgeSpeedMultiplier
            || type == ModifierType.PassiveMultiplier
            || type == ModifierType.SellValueMultiplier;
    }
}
