namespace AnvilClicker.Core
{
    /// <summary>Outcome of one hammer strike, consumed by feedback (VFX, sound, floating numbers).</summary>
    public readonly struct StrikeResult
    {
        public StrikeResult(double power, bool isCritical, long weaponsCompleted, double progress01)
        {
            Power = power;
            IsCritical = isCritical;
            WeaponsCompleted = weaponsCompleted;
            Progress01 = progress01;
        }

        /// <summary>Forge points added by this strike.</summary>
        public double Power { get; }

        public bool IsCritical { get; }

        /// <summary>Weapons finished by this strike (usually 0 or 1).</summary>
        public long WeaponsCompleted { get; }

        /// <summary>Progress of the weapon now on the anvil, in [0, 1).</summary>
        public double Progress01 { get; }
    }
}
