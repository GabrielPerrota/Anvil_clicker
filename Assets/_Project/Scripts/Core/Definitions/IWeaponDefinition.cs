namespace AnvilClicker.Core
{
    /// <summary>A forgeable weapon. Implemented by ScriptableObjects in the Data assembly.</summary>
    public interface IWeaponDefinition
    {
        /// <summary>Stable id persisted in saves. Never rename.</summary>
        string Id { get; }

        string DisplayName { get; }

        /// <summary>Forge points needed to finish one unit. Always &gt; 0.</summary>
        double ForgePointsRequired { get; }

        /// <summary>Gold received when one unit is sold.</summary>
        double BaseValue { get; }
    }

    public interface IWeaponCatalog
    {
        bool TryGetWeapon(string id, out IWeaponDefinition weapon);
    }
}
