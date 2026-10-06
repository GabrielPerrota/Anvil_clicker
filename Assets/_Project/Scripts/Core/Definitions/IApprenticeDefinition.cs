namespace AnvilClicker.Core
{
    /// <summary>A helper that produces forge points on its own. Implemented by ScriptableObjects in the Data assembly.</summary>
    public interface IApprenticeDefinition
    {
        /// <summary>Stable id persisted in saves. Never rename.</summary>
        string Id { get; }

        string DisplayName { get; }

        /// <summary>Forge points per second produced by one unit, before multipliers.</summary>
        double BaseForgePointsPerSecond { get; }

        double BaseCost { get; }

        /// <summary>Cost multiplier per unit owned (≥ 1).</summary>
        double CostGrowth { get; }

        /// <summary>Lifetime gold required before the apprentice shows up in the shop.</summary>
        double UnlockAtLifetimeGold { get; }
    }
}
