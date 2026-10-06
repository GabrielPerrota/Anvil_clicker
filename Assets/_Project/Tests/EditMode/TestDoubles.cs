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

    sealed class FakeUpgrade : IUpgradeDefinition
    {
        public FakeUpgrade(string id, double baseCost, double costGrowth, params UpgradeEffect[] effects)
        {
            Id = id;
            DisplayName = id;
            BaseCost = baseCost;
            CostGrowth = costGrowth;
            Effects = effects;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public string Description => string.Empty;
        public UpgradeCategory Category => UpgradeCategory.Hammer;
        public double BaseCost { get; }
        public double CostGrowth { get; }
        public int MaxLevel { get; set; }
        public double UnlockAtLifetimeGold { get; set; }
        public IReadOnlyList<UpgradeEffect> Effects { get; }
    }

    sealed class FakeApprentice : IApprenticeDefinition
    {
        public FakeApprentice(string id, double forgePointsPerSecond, double baseCost, double costGrowth = 1.15)
        {
            Id = id;
            DisplayName = id;
            BaseForgePointsPerSecond = forgePointsPerSecond;
            BaseCost = baseCost;
            CostGrowth = costGrowth;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public double BaseForgePointsPerSecond { get; }
        public double BaseCost { get; }
        public double CostGrowth { get; }
        public double UnlockAtLifetimeGold { get; set; }
    }

    sealed class FakeRoom : IRoomDefinition
    {
        public FakeRoom(string id, double cost = 0, double unlockAtLifetimeGold = 0)
        {
            Id = id;
            DisplayName = id;
            Cost = cost;
            UnlockAtLifetimeGold = unlockAtLifetimeGold;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public string Description => string.Empty;
        public double Cost { get; }
        public double UnlockAtLifetimeGold { get; }
    }

    sealed class FakeCatalog : IGameCatalog
    {
        readonly Dictionary<string, IWeaponDefinition> _weapons = new Dictionary<string, IWeaponDefinition>();

        public FakeCatalog(params IWeaponDefinition[] weapons)
        {
            foreach (var weapon in weapons) _weapons.Add(weapon.Id, weapon);
        }

        public List<IUpgradeDefinition> UpgradeList { get; } = new List<IUpgradeDefinition>();
        public List<IApprenticeDefinition> ApprenticeList { get; } = new List<IApprenticeDefinition>();
        public List<IRoomDefinition> RoomList { get; } = new List<IRoomDefinition>();

        public IReadOnlyList<IUpgradeDefinition> Upgrades => UpgradeList;
        public IReadOnlyList<IApprenticeDefinition> Apprentices => ApprenticeList;
        public IReadOnlyList<IRoomDefinition> Rooms => RoomList;

        public FakeCatalog With(params IUpgradeDefinition[] upgrades)
        {
            UpgradeList.AddRange(upgrades);
            return this;
        }

        public FakeCatalog With(params IRoomDefinition[] rooms)
        {
            RoomList.AddRange(rooms);
            return this;
        }

        public FakeCatalog With(params IApprenticeDefinition[] apprentices)
        {
            ApprenticeList.AddRange(apprentices);
            return this;
        }

        public bool TryGetWeapon(string id, out IWeaponDefinition weapon)
        {
            weapon = null;
            return id != null && _weapons.TryGetValue(id, out weapon);
        }
    }

    sealed class FakeBalance : IGameBalance
    {
        public double BaseClickPower { get; set; } = 1;
        public double CritChance { get; set; } = 0.05;
        public double CritMultiplier { get; set; } = 5;
        public string StartingWeaponId { get; set; } = "dagger";
        public double OfflineCapHours { get; set; } = 8;
        public double OfflineEfficiency { get; set; } = 0.5;
        public double AutosaveIntervalSeconds { get; set; } = 30;
        public List<int> Milestones { get; set; } = new List<int> { 25, 50, 100, 200 };
        public IReadOnlyList<int> ApprenticeMilestones => Milestones;
    }

    sealed class FakeStats : IForgeStats
    {
        public double ClickPower { get; set; } = 1;
        public double CritChance { get; set; } = 0.05;
        public double CritMultiplier { get; set; } = 5;
    }

    static class TestData
    {
        public static FakeWeapon Dagger() => new FakeWeapon("dagger", forgePointsRequired: 10, baseValue: 5);

        /// <summary>Context with a 10-PF dagger worth 5 gold; crits never happen.</summary>
        public static GameContext Context(FakeCatalog catalog = null, FakeBalance balance = null, GameState state = null) =>
            new GameContext(state ?? new GameState(), balance ?? new FakeBalance(), catalog ?? new FakeCatalog(Dagger()), new FakeRandom(0.99));

        public static int Level(this GameState state, string id) => state.UpgradeLevels.TryGetValue(id, out var level) ? level : 0;
    }
}
