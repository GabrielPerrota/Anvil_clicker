using System.Collections.Generic;
using AnvilClicker.Core;

namespace AnvilClicker.Tests
{
    sealed class FakeRandom : IRandom
    {
        readonly Queue<double> _values;
        readonly double _fallback;

        /// <summary>Returns the given values in order, then <paramref name="fallback"/> forever.</summary>
        public FakeRandom(double fallback, params double[] values)
        {
            _fallback = fallback;
            _values = new Queue<double>(values);
        }

        public double NextDouble() => _values.Count > 0 ? _values.Dequeue() : _fallback;
    }

    sealed class FakeWeapon : IWeaponDefinition
    {
        public FakeWeapon(string id, double forgePointsRequired, double baseValue)
        {
            Id = id;
            DisplayName = id;
            ForgePointsRequired = forgePointsRequired;
            BaseValue = baseValue;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public double ForgePointsRequired { get; }
        public double BaseValue { get; }
    }

    sealed class FakeCatalog : IWeaponCatalog
    {
        readonly Dictionary<string, IWeaponDefinition> _weapons = new Dictionary<string, IWeaponDefinition>();

        public FakeCatalog(params IWeaponDefinition[] weapons)
        {
            foreach (var weapon in weapons) _weapons.Add(weapon.Id, weapon);
        }

        public bool TryGetWeapon(string id, out IWeaponDefinition weapon)
        {
            weapon = null;
            return id != null && _weapons.TryGetValue(id, out weapon);
        }
    }

    sealed class FakeBalance : IForgeBalance
    {
        public double BaseClickPower { get; set; } = 1;
        public double CritChance { get; set; } = 0.05;
        public double CritMultiplier { get; set; } = 5;
        public string StartingWeaponId { get; set; } = "dagger";
    }
}
