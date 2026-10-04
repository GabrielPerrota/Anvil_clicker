using AnvilClicker.Core;
using NUnit.Framework;

namespace AnvilClicker.Tests
{
    public class WorkforceServiceTests
    {
        FakeApprentice _apprentice;
        FakeApprentice _journeyman;
        GameContext _context;

        [SetUp]
        public void SetUp()
        {
            _apprentice = new FakeApprentice("apprentice", forgePointsPerSecond: 1, baseCost: 15);
            _journeyman = new FakeApprentice("journeyman", forgePointsPerSecond: 8, baseCost: 100) { UnlockAtLifetimeGold = 50 };
            _context = TestData.Context(new FakeCatalog(TestData.Dagger()).With(_apprentice, _journeyman));
        }

        WorkforceService Workforce => _context.Workforce;

        [Test]
        public void NoApprentices_ProduceNothing()
        {
            Assert.That(Workforce.ForgePointsPerSecond, Is.EqualTo(0));
        }

        [Test]
        public void TryHire_AddsUnitsAndProduction()
        {
            _context.Wallet.Add(1000);

            Assert.That(Workforce.TryHire(_apprentice, 3), Is.True);
            Assert.That(Workforce.GetCount(_apprentice), Is.EqualTo(3));
            Assert.That(Workforce.ForgePointsPerSecond, Is.EqualTo(3));
        }

        [Test]
        public void TryHire_SpendsBulkCost()
        {
            _context.Wallet.Add(1000);
            var cost = Workforce.GetCost(_apprentice, 10);

            Workforce.TryHire(_apprentice, 10);

            Assert.That(_context.Wallet.Gold, Is.EqualTo(1000 - cost).Within(1e-9));
        }

        [Test]
        public void TryHire_WithoutGold_Fails()
        {
            Assert.That(Workforce.TryHire(_apprentice, 1), Is.False);
            Assert.That(Workforce.GetCount(_apprentice), Is.EqualTo(0));
        }

        [Test]
        public void LockedApprentice_CannotBeHired()
        {
            _context.Wallet.Add(40);
            _context.State.Gold = 1_000; // rich, but lifetime gold is only 40

            Assert.That(Workforce.IsUnlocked(_journeyman), Is.False);
            Assert.That(Workforce.TryHire(_journeyman, 1), Is.False);
        }

        [TestCase(24, 1)]
        [TestCase(25, 2)]
        [TestCase(50, 4)]
        [TestCase(100, 8)]
        [TestCase(250, 16)]
        public void Milestones_DoubleProduction(int count, double multiplier)
        {
            Assert.That(Workforce.GetMilestoneMultiplier(count), Is.EqualTo(multiplier));
        }

        [Test]
        public void Production_CombinesCountMilestonesAndTypes()
        {
            _context.State.Apprentices["apprentice"] = 25; // 25 × 1 × 2 = 50
            _context.State.Apprentices["journeyman"] = 2;  // 2 × 8 = 16

            Assert.That(Workforce.ForgePointsPerSecond, Is.EqualTo(66));
        }

        [Test]
        public void PassiveUpgrade_MultipliesProduction()
        {
            var training = new FakeUpgrade("training", 10, 1, new UpgradeEffect(ModifierType.PassiveMultiplier, 1.0));
            var context = TestData.Context(new FakeCatalog(TestData.Dagger()).With(_apprentice).With(training));
            context.State.Apprentices["apprentice"] = 4;
            context.Wallet.Add(10);

            context.Upgrades.TryBuy(training, 1);

            Assert.That(context.Workforce.ForgePointsPerSecond, Is.EqualTo(8));
        }

        [Test]
        public void TryHire_RaisesWorkforceChanged()
        {
            _context.Wallet.Add(1000);
            int? count = null;
            Workforce.WorkforceChanged += (_, newCount) => count = newCount;

            Workforce.TryHire(_apprentice, 2);

            Assert.That(count, Is.EqualTo(2));
        }
    }
}
