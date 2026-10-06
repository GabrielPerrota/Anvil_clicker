namespace AnvilClicker.Core
{
    /// <summary>A room of the workshop. Implemented by ScriptableObjects in the Data assembly.</summary>
    public interface IRoomDefinition
    {
        /// <summary>Stable id persisted in saves. Never rename.</summary>
        string Id { get; }

        string DisplayName { get; }

        string Description { get; }

        /// <summary>Gold to build the room. Zero means it is part of the workshop from the start.</summary>
        double Cost { get; }

        /// <summary>Lifetime gold required before the room shows up as available.</summary>
        double UnlockAtLifetimeGold { get; }
    }
}
