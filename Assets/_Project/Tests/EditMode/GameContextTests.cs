using AnvilClicker.Core;
using NUnit.Framework;

namespace AnvilClicker.Tests
{
    public class GameContextTests
    {
        [Test]
        public void ForgedWeapon_IsSoldImmediatelyForItsBaseValue()
        {
            var context = TestData.Context();

            context.Forge.AddForgePoints(30);

            Assert.That(context.Wallet.Gold, Is.EqualTo(15));
            Assert.That(context.Wallet.LifetimeGold, Is.EqualTo(15));
        }

        [Test]
        public void StrikesBelowCompletion_EarnNoGold()
        {
            var context = TestData.Context();

            for (var i = 0; i < 9; i++) context.Forge.Strike();

            Assert.That(context.Wallet.Gold, Is.EqualTo(0));
        }

        [Test]
        public void Tick_WithApprentices_ForgesAndSellsWeapons()
        {
            var apprentice = new FakeApprentice("apprentice", forgePointsPerSecond: 2, baseCost: 10);
            var state = new GameState();
            state.Apprentices["apprentice"] = 5; // 10 PF/s
            var context = TestData.Context(new FakeCatalog(TestData.Dagger()).With(apprentice), state: state);

            context.Tick(3); // 30 PF → 3 daggers

            Assert.That(context.Forge.WeaponsForged, Is.EqualTo(3));
            Assert.That(context.Wallet.Gold, Is.EqualTo(15));
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(double.NaN)]
        public void Tick_InvalidDelta_DoesNothing(double delta)
        {
            var state = new GameState();
            state.Apprentices["apprentice"] = 5;
            var context = TestData.Context(new FakeCatalog(TestData.Dagger()).With(new FakeApprentice("apprentice", 2, 10)), state: state);

            context.Tick(delta);

            Assert.That(context.Forge.Progress, Is.EqualTo(0));
        }

        [Test]
        public void SellValueUpgrade_IncreasesGoldPerWeapon()
        {
            var haggle = new FakeUpgrade("haggle", 10, 1.0, new UpgradeEffect(ModifierType.SellValueMultiplier, 0.5));
            var context = TestData.Context(new FakeCatalog(TestData.Dagger()).With(haggle));
            context.Wallet.Add(10);
            Assume.That(context.Upgrades.TryBuy(haggle, 1), Is.True);
            var goldAfterPurchase = context.Wallet.Gold;

            context.Forge.AddForgePoints(10);

            Assert.That(context.Wallet.Gold - goldAfterPurchase, Is.EqualTo(7.5));
        }

        [Test]
        public void BuyingAnUpgrade_RecalculatesClickPower()
        {
            var grip = new FakeUpgrade("grip", 10, 1.15, new UpgradeEffect(ModifierType.ClickPowerFlat, 1));
            var context = TestData.Context(new FakeCatalog(TestData.Dagger()).With(grip));
            context.Wallet.Add(100);

            context.Upgrades.TryBuy(grip, 2);

            Assert.That(context.Forge.ClickPower, Is.EqualTo(3));
        }
    }
}
