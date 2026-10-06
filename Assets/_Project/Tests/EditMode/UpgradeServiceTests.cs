using AnvilClicker.Core;
using NUnit.Framework;

namespace AnvilClicker.Tests
{
    public class UpgradeServiceTests
    {
        GameState _state;
        Wallet _wallet;
        UpgradeService _upgrades;
        FakeUpgrade _grip;

        [SetUp]
        public void SetUp()
        {
            _state = new GameState();
            _wallet = new Wallet(_state);
            _upgrades = new UpgradeService(_state, _wallet);
            _grip = new FakeUpgrade("grip", 15, 1.15, new UpgradeEffect(ModifierType.ClickPowerFlat, 1));
        }

        [Test]
        public void TryBuy_WithEnoughGold_RaisesLevelAndSpendsCost()
        {
            _wallet.Add(100);
            var cost = _upgrades.GetCost(_grip, 2);

            var bought = _upgrades.TryBuy(_grip, 2);

            Assert.That(bought, Is.True);
            Assert.That(_upgrades.GetLevel(_grip), Is.EqualTo(2));
            Assert.That(_wallet.Gold, Is.EqualTo(100 - cost).Within(1e-9));
        }

        [Test]
        public void TryBuy_WithoutEnoughGold_ChangesNothing()
        {
            _wallet.Add(14);

            Assert.That(_upgrades.TryBuy(_grip, 1), Is.False);
            Assert.That(_upgrades.GetLevel(_grip), Is.EqualTo(0));
            Assert.That(_wallet.Gold, Is.EqualTo(14));
        }

        [Test]
        public void TryBuy_RaisesEventWithNewLevel()
        {
            _wallet.Add(1000);
            int? level = null;
            _upgrades.UpgradePurchased += (_, newLevel) => level = newLevel;

            _upgrades.TryBuy(_grip, 3);

            Assert.That(level, Is.EqualTo(3));
        }

        [Test]
        public void TryBuy_BeyondMaxLevel_Fails()
        {
            _grip.MaxLevel = 2;
            _wallet.Add(1_000_000);

            Assert.That(_upgrades.TryBuy(_grip, 3), Is.False);
            Assert.That(_upgrades.TryBuy(_grip, 2), Is.True);
            Assert.That(_upgrades.IsMaxed(_grip), Is.True);
            Assert.That(_upgrades.TryBuy(_grip, 1), Is.False);
        }

        [Test]
        public void GetMaxAffordable_IsCappedByRemainingLevels()
        {
            _grip.MaxLevel = 5;
            _wallet.Add(1_000_000);

            Assert.That(_upgrades.GetMaxAffordable(_grip), Is.EqualTo(5));
        }

        [Test]
        public void LockedUpgrade_CannotBeBoughtUntilLifetimeGoldIsReached()
        {
            _grip.UnlockAtLifetimeGold = 500;
            _wallet.Add(200);

            Assert.That(_upgrades.IsUnlocked(_grip), Is.False);
            Assert.That(_upgrades.TryBuy(_grip, 1), Is.False);

            _wallet.Add(300);

            Assert.That(_upgrades.IsUnlocked(_grip), Is.True);
            Assert.That(_upgrades.TryBuy(_grip, 1), Is.True);
        }

        [Test]
        public void LifetimeGold_IsNotReducedBySpending_SoUnlocksStay()
        {
            _grip.UnlockAtLifetimeGold = 100;
            _wallet.Add(100);
            _wallet.TrySpend(100);

            Assert.That(_upgrades.IsUnlocked(_grip), Is.True);
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void TryBuy_NonPositiveQuantity_Fails(int quantity)
        {
            _wallet.Add(1000);

            Assert.That(_upgrades.TryBuy(_grip, quantity), Is.False);
        }
    }
}
