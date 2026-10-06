using System;

namespace AnvilClicker.Core
{
    /// <summary>Forge points earned while away, before they are turned into weapons.</summary>
    public readonly struct OfflineGain
    {
        public OfflineGain(TimeSpan away, TimeSpan credited, double forgePoints)
        {
            Away = away;
            Credited = credited;
            ForgePoints = forgePoints;
        }

        /// <summary>Real time spent away (zero if the clock went backwards).</summary>
        public TimeSpan Away { get; }

        /// <summary>Part of <see cref="Away"/> that counted, after the cap.</summary>
        public TimeSpan Credited { get; }

        public double ForgePoints { get; }
    }

    /// <summary>What happened while the game was closed, shown in the welcome-back summary.</summary>
    public readonly struct OfflineReport
    {
        public static readonly OfflineReport Empty = new OfflineReport(TimeSpan.Zero, TimeSpan.Zero, 0d, 0, 0d);

        public OfflineReport(TimeSpan away, TimeSpan credited, double forgePoints, long weaponsForged, double goldEarned)
        {
            Away = away;
            Credited = credited;
            ForgePoints = forgePoints;
            WeaponsForged = weaponsForged;
            GoldEarned = goldEarned;
        }

        public TimeSpan Away { get; }
        public TimeSpan Credited { get; }
        public double ForgePoints { get; }
        public long WeaponsForged { get; }
        public double GoldEarned { get; }

        public bool WasCapped => Away > Credited;

        public bool HasProgress => ForgePoints > 0;
    }

    public static class OfflineProgressCalculator
    {
        /// <summary>Absences shorter than this earn nothing (avoids a summary popup after a quick restart).</summary>
        public static readonly TimeSpan MinimumAway = TimeSpan.FromSeconds(60);

        public static OfflineGain Calculate(TimeSpan away, double forgePointsPerSecond, double capHours, double efficiency)
        {
            if (away < MinimumAway) return new OfflineGain(away < TimeSpan.Zero ? TimeSpan.Zero : away, TimeSpan.Zero, 0d);

            var cap = TimeSpan.FromHours(Math.Max(0d, capHours));
            var credited = away > cap ? cap : away;

            var rate = double.IsNaN(forgePointsPerSecond) || forgePointsPerSecond < 0 ? 0d : forgePointsPerSecond;
            var factor = Math.Min(1d, Math.Max(0d, efficiency));
            var points = rate * credited.TotalSeconds * factor;

            return new OfflineGain(away, credited, double.IsInfinity(points) ? double.MaxValue : points);
        }
    }
}
