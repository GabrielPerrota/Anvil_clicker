using System;
using AnvilClicker.Core;
using NUnit.Framework;

namespace AnvilClicker.Tests
{
    public class OfflineProgressTests
    {
        [Test]
        public void Calculate_AppliesEfficiency()
        {
            var gain = OfflineProgressCalculator.Calculate(TimeSpan.FromHours(1), forgePointsPerSecond: 10, capHours: 8, efficiency: 0.5);

            Assert.That(gain.ForgePoints, Is.EqualTo(10 * 3600 * 0.5));
            Assert.That(gain.Credited, Is.EqualTo(TimeSpan.FromHours(1)));
        }

        [Test]
        public void Calculate_CapsCreditedTime()
        {
            var gain = OfflineProgressCalculator.Calculate(TimeSpan.FromHours(20), 10, capHours: 8, efficiency: 1);

            Assert.That(gain.Away, Is.EqualTo(TimeSpan.FromHours(20)));
            Assert.That(gain.Credited, Is.EqualTo(TimeSpan.FromHours(8)));
            Assert.That(gain.ForgePoints, Is.EqualTo(10 * 8 * 3600));
        }

        [Test]
        public void Calculate_BelowMinimumAway_EarnsNothing()
        {
            var gain = OfflineProgressCalculator.Calculate(TimeSpan.FromSeconds(59), 10, 8, 1);

            Assert.That(gain.ForgePoints, Is.EqualTo(0));
        }

        [Test]
        public void Calculate_ClockWentBackwards_EarnsNothing()
        {
            var gain = OfflineProgressCalculator.Calculate(TimeSpan.FromHours(-3), 10, 8, 1);

            Assert.That(gain.ForgePoints, Is.EqualTo(0));
            Assert.That(gain.Away, Is.EqualTo(TimeSpan.Zero));
        }

        [TestCase(-0.5, 0)]
        [TestCase(2.0, 36000)]
        public void Calculate_ClampsEfficiency(double efficiency, double expected)
        {
            var gain = OfflineProgressCalculator.Calculate(TimeSpan.FromHours(1), 10, 8, efficiency);

            Assert.That(gain.ForgePoints, Is.EqualTo(expected));
        }

        [Test]
        public void ApplyOfflineProgress_ForgesAndSellsWeapons()
        {
            var state = new GameState();
            state.Apprentices["apprentice"] = 1; // 1 PF/s
            var catalog = new FakeCatalog(TestData.Dagger()).With(new FakeApprentice("apprentice", 1, 15));
            var context = TestData.Context(catalog, state: state);

            // 10 min × 1 PF/s × 50% = 300 PF → 30 daggers × 5 gold.
            var report = context.ApplyOfflineProgress(TimeSpan.FromMinutes(10));

            Assert.That(report.HasProgress, Is.True);
            Assert.That(report.ForgePoints, Is.EqualTo(300));
            Assert.That(report.WeaponsForged, Is.EqualTo(30));
            Assert.That(report.GoldEarned, Is.EqualTo(150));
            Assert.That(context.Wallet.Gold, Is.EqualTo(150));
            Assert.That(context.LastOfflineReport.GoldEarned, Is.EqualTo(150));
        }

        [Test]
        public void ApplyOfflineProgress_WithoutApprentices_HasNoProgress()
        {
            var context = TestData.Context();

            var report = context.ApplyOfflineProgress(TimeSpan.FromHours(2));

            Assert.That(report.HasProgress, Is.False);
            Assert.That(context.Wallet.Gold, Is.EqualTo(0));
        }

        [Test]
        public void ApplyOfflineProgress_RaisesEvent()
        {
            var context = TestData.Context();
            var raised = false;
            context.OfflineProgressApplied += _ => raised = true;

            context.ApplyOfflineProgress(TimeSpan.FromHours(1));

            Assert.That(raised, Is.True);
        }

        [Test]
        public void Report_FlagsCappedAbsence()
        {
            var state = new GameState();
            state.Apprentices["apprentice"] = 1;
            var context = TestData.Context(new FakeCatalog(TestData.Dagger()).With(new FakeApprentice("apprentice", 1, 15)), state: state);

            var report = context.ApplyOfflineProgress(TimeSpan.FromHours(30));

            Assert.That(report.WasCapped, Is.True);
            Assert.That(report.Credited, Is.EqualTo(TimeSpan.FromHours(8)));
        }
    }
}
