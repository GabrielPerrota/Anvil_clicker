using System;

namespace AnvilClicker.Core
{
    /// <summary>
    /// Geometric cost curves used by every purchasable: the n-th unit costs base · growthⁿ.
    /// Bulk prices use the closed-form geometric sum, so buying 100 units costs no more CPU than buying 1.
    /// </summary>
    public static class EconomyFormulas
    {
        /// <summary>Price of the next unit when <paramref name="owned"/> units are already owned.</summary>
        public static double Cost(double baseCost, double growth, int owned)
        {
            Validate(baseCost, growth, owned);
            return baseCost * Math.Pow(growth, owned);
        }

        /// <summary>Total price of the next <paramref name="count"/> units.</summary>
        public static double BulkCost(double baseCost, double growth, int owned, int count)
        {
            Validate(baseCost, growth, owned);
            if (count <= 0) return 0d;

            if (growth == 1d) return baseCost * count;
            return baseCost * Math.Pow(growth, owned) * (Math.Pow(growth, count) - 1d) / (growth - 1d);
        }

        /// <summary>How many units <paramref name="budget"/> buys, never more than <paramref name="limit"/>.</summary>
        public static int MaxAffordable(double baseCost, double growth, int owned, double budget, int limit = int.MaxValue)
        {
            Validate(baseCost, growth, owned);
            if (limit <= 0 || double.IsNaN(budget) || budget <= 0) return 0;

            var first = Cost(baseCost, growth, owned);
            if (first > budget) return 0;

            double estimate;
            if (growth == 1d)
            {
                estimate = Math.Floor(budget / baseCost);
            }
            else
            {
                // Solve first · (gⁿ − 1) / (g − 1) ≤ budget for n.
                estimate = Math.Floor(Math.Log(budget * (growth - 1d) / first + 1d) / Math.Log(growth));
            }

            var count = (int)Math.Max(0d, Math.Min(estimate, limit));

            // Floating point can put the estimate one step off either way; nudge it into place.
            for (var i = 0; i < 3 && count > 0 && BulkCost(baseCost, growth, owned, count) > budget; i++) count--;
            for (var i = 0; i < 3 && count < limit && BulkCost(baseCost, growth, owned, count + 1) <= budget; i++) count++;

            return count;
        }

        static void Validate(double baseCost, double growth, int owned)
        {
            Guard.PositiveFinite(baseCost, nameof(baseCost));
            if (double.IsNaN(growth) || double.IsInfinity(growth) || growth < 1d)
                throw new ArgumentOutOfRangeException(nameof(growth), growth, "Growth must be a finite number >= 1.");
            if (owned < 0) throw new ArgumentOutOfRangeException(nameof(owned), owned, "Owned must be >= 0.");
        }
    }
}
