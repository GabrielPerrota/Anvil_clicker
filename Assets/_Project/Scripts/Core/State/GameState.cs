namespace AnvilClicker.Core
{
    /// <summary>
    /// Serializable snapshot of a playthrough. Plain data only: every change goes through a service.
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
    }
}
