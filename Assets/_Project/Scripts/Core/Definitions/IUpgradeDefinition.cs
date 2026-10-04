using System.Collections.Generic;

namespace AnvilClicker.Core
{
    public enum UpgradeCategory
    {
        Hammer,
        Forge,
        Precision,
        Commerce,
        Workforce
    }

    /// <summary>A levelled upgrade bought with gold. Implemented by ScriptableObjects in the Data assembly.</summary>
    public interface IUpgradeDefinition
    {
        /// <summary>Stable id persisted in saves. Never rename.</summary>
        string Id { get; }

        string DisplayName { get; }

        string Description { get; }

        UpgradeCategory Category { get; }

        double BaseCost { get; }

        /// <summary>Cost multiplier per level owned (≥ 1).</summary>
        double CostGrowth { get; }

        /// <summary>Highest level that can be bought; 0 means unlimited.</summary>
        int MaxLevel { get; }

        /// <summary>Lifetime gold required before the upgrade shows up in the shop.</summary>
        double UnlockAtLifetimeGold { get; }

        IReadOnlyList<UpgradeEffect> Effects { get; }
    }
}
