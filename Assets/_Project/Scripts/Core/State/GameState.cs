using System.Collections.Generic;

namespace AnvilClicker.Core
{
    /// <summary>
    /// Serializable snapshot of a playthrough. Plain data only: every change goes through a service.
    /// Adding a field is backwards compatible; renaming or removing one needs a save migration.
    /// </summary>
    public sealed class GameState
    {
        public const int CurrentSaveVersion = 1;

        public int SaveVersion = CurrentSaveVersion;

        public double Gold;
        public double LifetimeGold;

        /// <summary>Forge points accumulated on the weapon currently on the anvil.</summary>
        public double ForgeProgress;

        /// <summary>Id of the weapon being forged. Null means "use the balance's starting weapon".</summary>
        public string ActiveWeaponId;

        public long WeaponsForged;

        /// <summary>Upgrade id → level owned.</summary>
        public Dictionary<string, int> UpgradeLevels = new Dictionary<string, int>();

        /// <summary>Apprentice id → units hired.</summary>
        public Dictionary<string, int> Apprentices = new Dictionary<string, int>();

        /// <summary>Ids of the rooms already built (the free starting workshop included).</summary>
        public List<string> UnlockedRooms = new List<string>();
    }
}
