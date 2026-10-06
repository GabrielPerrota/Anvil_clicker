using System;
using AnvilClicker.Core;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace AnvilClicker.Tests
{
    public class SaveSerializerTests
    {
        static readonly DateTime SavedAt = new DateTime(2026, 10, 4, 12, 30, 0, DateTimeKind.Utc);

        static GameState SampleState()
        {
            var state = new GameState
            {
                Gold = 1234.5,
                LifetimeGold = 98765.25,
                ForgeProgress = 3.75,
                ActiveWeaponId = "weapon.iron_dagger",
                WeaponsForged = 4242
            };
            state.UpgradeLevels["upgrade.hammer_grip"] = 7;
            state.UpgradeLevels["Upgrade.MixedCase"] = 1;
            state.Apprentices["apprentice.apprentice"] = 30;
            return state;
        }

        [Test]
        public void RoundTrip_PreservesState()
        {
            var serializer = new SaveSerializer();

            var json = serializer.Serialize(SampleState(), SavedAt, "0.0.3");
            var envelope = serializer.Deserialize(json);
            var state = envelope.State;

            Assert.That(envelope.SaveVersion, Is.EqualTo(GameState.CurrentSaveVersion));
            Assert.That(envelope.GameVersion, Is.EqualTo("0.0.3"));
            Assert.That(envelope.SavedAtUtc, Is.EqualTo(SavedAt));
            Assert.That(envelope.SavedAtUtc.Kind, Is.EqualTo(DateTimeKind.Utc));
            Assert.That(state.Gold, Is.EqualTo(1234.5));
            Assert.That(state.LifetimeGold, Is.EqualTo(98765.25));
            Assert.That(state.ForgeProgress, Is.EqualTo(3.75));
            Assert.That(state.ActiveWeaponId, Is.EqualTo("weapon.iron_dagger"));
            Assert.That(state.WeaponsForged, Is.EqualTo(4242));
            Assert.That(state.UpgradeLevels["upgrade.hammer_grip"], Is.EqualTo(7));
            Assert.That(state.Apprentices["apprentice.apprentice"], Is.EqualTo(30));
        }

        [Test]
        public void DictionaryKeys_AreNotRenamed()
        {
            var serializer = new SaveSerializer();

            var state = serializer.Deserialize(serializer.Serialize(SampleState(), SavedAt, "x")).State;

            Assert.That(state.UpgradeLevels.ContainsKey("Upgrade.MixedCase"), Is.True);
        }

        [Test]
        public void HugeNumbers_SurviveRoundTrip()
        {
            var serializer = new SaveSerializer();
            var original = new GameState { Gold = 1.7e300, LifetimeGold = double.MaxValue };

            var state = serializer.Deserialize(serializer.Serialize(original, SavedAt, "x")).State;

            Assert.That(state.Gold, Is.EqualTo(1.7e300));
            Assert.That(state.LifetimeGold, Is.EqualTo(double.MaxValue));
        }

        [Test]
        public void MissingDictionaries_LoadAsEmpty()
        {
            var json = "{ \"saveVersion\": 1, \"savedAtUtc\": \"2026-10-04T12:30:00Z\", \"state\": { \"gold\": 5 } }";

            var state = new SaveSerializer().Deserialize(json).State;

            Assert.That(state.Gold, Is.EqualTo(5));
            Assert.That(state.UpgradeLevels, Is.Not.Null.And.Empty);
            Assert.That(state.Apprentices, Is.Not.Null.And.Empty);
        }

        [TestCase("")]
        [TestCase("not json at all")]
        [TestCase("{ \"saveVersion\": 1, \"state\": { \"gold\": ")]
        [TestCase("{ \"state\": { \"gold\": 5 } }")]
        [TestCase("{ \"saveVersion\": 1 }")]
        public void InvalidSave_ThrowsSaveFormatException(string json)
        {
            Assert.Throws<SaveFormatException>(() => new SaveSerializer().Deserialize(json));
        }

        [Test]
        public void NewerSaveVersion_IsRejected()
        {
            var json = "{ \"saveVersion\": 99, \"state\": { \"gold\": 5 } }";

            Assert.Throws<SaveFormatException>(() => new SaveSerializer().Deserialize(json));
        }

        sealed class RenameGoldMigration : ISaveMigration
        {
            public int FromVersion => 1;

            public void Migrate(JObject envelope)
            {
                var state = (JObject)envelope["state"];
                state["gold"] = state["coins"];
                state.Remove("coins");
            }
        }

        sealed class DoubleGoldMigration : ISaveMigration
        {
            public int FromVersion => 2;

            public void Migrate(JObject envelope)
            {
                var state = (JObject)envelope["state"];
                state["gold"] = state.Value<double>("gold") * 2;
            }
        }

        [Test]
        public void Migrations_RunInOrderUpToCurrentVersion()
        {
            var serializer = new SaveSerializer(new ISaveMigration[] { new DoubleGoldMigration(), new RenameGoldMigration() }, currentVersion: 3);
            var json = "{ \"saveVersion\": 1, \"savedAtUtc\": \"2026-10-04T12:30:00Z\", \"state\": { \"coins\": 21 } }";

            var envelope = serializer.Deserialize(json);

            Assert.That(envelope.SaveVersion, Is.EqualTo(3));
            Assert.That(envelope.State.Gold, Is.EqualTo(42));
        }

        [Test]
        public void MissingMigration_IsReported()
        {
            var serializer = new SaveSerializer(new ISaveMigration[] { new RenameGoldMigration() }, currentVersion: 3);
            var json = "{ \"saveVersion\": 1, \"state\": { \"coins\": 1 } }";

            Assert.Throws<SaveFormatException>(() => serializer.Deserialize(json));
        }

        [Test]
        public void DuplicateMigrations_AreRejected()
        {
            Assert.Throws<ArgumentException>(() =>
                new SaveSerializer(new ISaveMigration[] { new RenameGoldMigration(), new RenameGoldMigration() }, 2));
        }
    }
}
