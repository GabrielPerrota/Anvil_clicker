using AnvilClicker.Core;
using NUnit.Framework;

namespace AnvilClicker.Tests
{
    public class ModifierStackTests
    {
        GameState _state;
        FakeBalance _balance;
        FakeCatalog _catalog;

        [SetUp]
        public void SetUp()
        {
            _state = new GameState();
            _balance = new FakeBalance();
            _catalog = new FakeCatalog(TestData.Dagger());
        }

        ModifierStack Stack() => new ModifierStack(_state, _balance, _catalog);

        FakeUpgrade Owned(string id, int level, params UpgradeEffect[] effects)
        {
            var upgrade = new FakeUpgrade(id, 10, 1.15, effects);
            _catalog.With(upgrade);
            _state.UpgradeLevels[id] = level;
            return upgrade;
        }

        [Test]
        public void NoUpgrades_UsesBaseBalance()
        {
            var stack = Stack();

            Assert.That(stack.ClickPower, Is.EqualTo(1));
            Assert.That(stack.CritChance, Is.EqualTo(0.05));
            Assert.That(stack.CritMultiplier, Is.EqualTo(5));
            Assert.That(stack.PassiveMultiplier, Is.EqualTo(1));
            Assert.That(stack.SellMultiplier, Is.EqualTo(1));
        }

        [Test]
        public void FlatClickPower_ScalesWithLevel()
        {
            Owned("grip", 3, new UpgradeEffect(ModifierType.ClickPowerFlat, 1));

            Assert.That(Stack().ClickPower, Is.EqualTo(4));
        }

        [Test]
        public void Multipliers_FromDifferentUpgradesMultiply()
        {
            Owned("grip", 1, new UpgradeEffect(ModifierType.ClickPowerFlat, 1));             // base 1 + 1 = 2
            Owned("steel", 1, new UpgradeEffect(ModifierType.ClickPowerMultiplier, 1.0));     // ×2
            Owned("master", 1, new UpgradeEffect(ModifierType.ClickPowerMultiplier, 2.0));    // ×3

            Assert.That(Stack().ClickPower, Is.EqualTo(12));
        }

        [Test]
        public void MultiplierLevels_AddWithinOneUpgrade()
        {
            Owned("bellows", 3, new UpgradeEffect(ModifierType.ForgeSpeedMultiplier, 0.1)); // 1 + 0.3

            var stack = Stack();

            Assert.That(stack.ClickPower, Is.EqualTo(1.3).Within(1e-9));
            Assert.That(stack.PassiveMultiplier, Is.EqualTo(1.3).Within(1e-9));
        }

        [Test]
        public void ForgeSpeed_AppliesToClicksAndApprentices()
        {
            Owned("coal", 1, new UpgradeEffect(ModifierType.ForgeSpeedMultiplier, 0.5));
            Owned("training", 1, new UpgradeEffect(ModifierType.PassiveMultiplier, 1.0));

            var stack = Stack();

            Assert.That(stack.ClickPower, Is.EqualTo(1.5));
            Assert.That(stack.PassiveMultiplier, Is.EqualTo(3));
        }

        [Test]
        public void CritChance_IsClampedToOne()
        {
            Owned("eye", 200, new UpgradeEffect(ModifierType.CritChanceFlat, 0.01));

            Assert.That(Stack().CritChance, Is.EqualTo(1));
        }

        [Test]
        public void CritMultiplier_AddsFlat()
        {
            Owned("true_strike", 2, new UpgradeEffect(ModifierType.CritMultiplierFlat, 1));

            Assert.That(Stack().CritMultiplier, Is.EqualTo(7));
        }

        [Test]
        public void UnknownOrUnownedUpgradeLevels_AreIgnored()
        {
            _state.UpgradeLevels["deleted_upgrade"] = 5;
            _catalog.With(new FakeUpgrade("grip", 10, 1.15, new UpgradeEffect(ModifierType.ClickPowerFlat, 1)));

            Assert.That(Stack().ClickPower, Is.EqualTo(1));
        }

        [Test]
        public void Recalculate_PicksUpNewLevelsAndRaisesChanged()
        {
            Owned("grip", 0, new UpgradeEffect(ModifierType.ClickPowerFlat, 1));
            var stack = Stack();
            var raised = false;
            stack.Changed += () => raised = true;

            _state.UpgradeLevels["grip"] = 2;
            stack.Recalculate();

            Assert.That(stack.ClickPower, Is.EqualTo(3));
            Assert.That(raised, Is.True);
        }
    }
}
