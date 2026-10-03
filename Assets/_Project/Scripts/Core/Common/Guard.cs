using System;

namespace AnvilClicker.Core
{
    internal static class Guard
    {
        /// <summary>Throws unless <paramref name="value"/> is a finite number greater than or equal to zero.</summary>
        public static void NonNegativeFinite(double value, string paramName)
        {
            if (double.IsNaN(value) || double.IsInfinity(value) || value < 0)
                throw new ArgumentOutOfRangeException(paramName, value, "Value must be a finite number >= 0.");
        }

        /// <summary>Throws unless <paramref name="value"/> is a finite number greater than zero.</summary>
        public static void PositiveFinite(double value, string paramName)
        {
            if (double.IsNaN(value) || double.IsInfinity(value) || value <= 0)
                throw new ArgumentOutOfRangeException(paramName, value, "Value must be a finite number > 0.");
        }
    }
}
