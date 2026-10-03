using System;

namespace AnvilClicker.Core
{
    /// <summary>Source of randomness, injectable so gameplay rolls can be tested deterministically.</summary>
    public interface IRandom
    {
        /// <summary>Returns a value in [0, 1).</summary>
        double NextDouble();
    }

    public sealed class SystemRandom : IRandom
    {
        readonly Random _random;

        public SystemRandom() => _random = new Random();

        public SystemRandom(int seed) => _random = new Random(seed);

        public double NextDouble() => _random.NextDouble();
    }
}
