using System;
using System.Collections.Generic;
using AnvilClicker.Core;
using NUnit.Framework;

namespace AnvilClicker.Tests
{
    public class ForgeServiceTests
    {
        static readonly FakeWeapon Dagger = new FakeWeapon("dagger", forgePointsRequired: 10, baseValue: 5);

        GameState _state;
        FakeBalance _balance;

        [SetUp]
        public void SetUp()
        {
            _state = new GameState();
            _balance = new FakeBalance();
        }

        ForgeService CreateForge(IRandom random) =>
            new ForgeService(_state, _balance, new FakeCatalog(Dagger), random);

        [Test]
        public void Constructor_UsesStartingWeaponForNewGame()
        {
            var forge = CreateForge(new FakeRandom(0.99));

            Assert.That(forge.ActiveWeapon, Is.SameAs(Dagger));
            Assert.That(_state.ActiveWeaponId, Is.EqualTo("dagger"));
        }

        [Test]
        public void Constructor_UnknownWeapon_Throws()
        {
            _balance.StartingWeaponId = "missing";

            Assert.Throws<InvalidOperationException>(() => CreateForge(new FakeRandom(0.99)));
        }

        [Test]
        public void Strike_RollAboveCritChance_AddsBasePower()
        {
            var forge = CreateForge(new FakeRandom(0.99));

            var result = forge.Strike();

            Assert.That(result.IsCritical, Is.False);
            Assert.That(result.Power, Is.EqualTo(1));
            Assert.That(forge.Progress, Is.EqualTo(1));
            Assert.That(result.Progress01, Is.EqualTo(0.1).Within(1e-9));
        }

        [Test]
        public void Strike_RollBelowCritChance_MultipliesPower()
        {
            var forge = CreateForge(new FakeRandom(0.0));

            var result = forge.Strike();

            Assert.That(result.IsCritical, Is.True);
            Assert.That(result.Power, Is.EqualTo(5));
            Assert.That(forge.Progress, Is.EqualTo(5));
        }

        [Test]
        public void Strike_RollEqualToCritChance_IsNotCritical()
        {
            var forge = CreateForge(new FakeRandom(0.05));

            Assert.That(forge.Strike().IsCritical, Is.False);
        }

        [Test]
        public void Strike_ZeroCritChance_NeverCrits()
        {
            _balance.CritChance = 0;
            var forge = CreateForge(new FakeRandom(0.0));

            Assert.That(forge.Strike().IsCritical, Is.False);
        }

        [Test]
        public void Strike_FinishingWeapon_RaisesWeaponForgedAndResetsProgress()
        {
            var forge = CreateForge(new FakeRandom(0.99));
            var forged = new List<(IWeaponDefinition weapon, long count)>();
            forge.WeaponForged += (weapon, count) => forged.Add((weapon, count));

            StrikeResult last = default;
            for (var i = 0; i < 10; i++) last = forge.Strike();

            Assert.That(forged, Has.Count.EqualTo(1));
            Assert.That(forged[0].weapon, Is.SameAs(Dagger));
            Assert.That(forged[0].count, Is.EqualTo(1));
            Assert.That(last.WeaponsCompleted, Is.EqualTo(1));
            Assert.That(forge.Progress, Is.EqualTo(0));
            Assert.That(forge.WeaponsForged, Is.EqualTo(1));
        }

        [Test]
        public void Strike_RaisesStrikeAppliedAfterProgressIsApplied()
        {
            var forge = CreateForge(new FakeRandom(0.99));
            double progressSeenInEvent = -1;
            forge.StrikeApplied += _ => progressSeenInEvent = forge.Progress;

            forge.Strike();

            Assert.That(progressSeenInEvent, Is.EqualTo(1));
        }

        [Test]
        public void AddForgePoints_Overflow_CarriesLeftoverAndCountsAllWeapons()
        {
            var forge = CreateForge(new FakeRandom(0.99));
            long forgedCount = 0;
            forge.WeaponForged += (_, count) => forgedCount += count;

            var completed = forge.AddForgePoints(25);

            Assert.That(completed, Is.EqualTo(2));
            Assert.That(forgedCount, Is.EqualTo(2));
            Assert.That(forge.Progress, Is.EqualTo(5));
            Assert.That(forge.WeaponsForged, Is.EqualTo(2));
        }

        [Test]
        public void AddForgePoints_HugeAmount_FinishesInOneEvent()
        {
            var forge = CreateForge(new FakeRandom(0.99));
            var events = 0;
            forge.WeaponForged += (_, _) => events++;

            var completed = forge.AddForgePoints(1e9);

            Assert.That(completed, Is.EqualTo(100_000_000));
            Assert.That(events, Is.EqualTo(1));
        }

        [Test]
        public void AddForgePoints_RaisesProgressChanged()
        {
            var forge = CreateForge(new FakeRandom(0.99));
            double? progress = null;
            forge.ProgressChanged += value => progress = value;

            forge.AddForgePoints(4);

            Assert.That(progress, Is.EqualTo(0.4).Within(1e-9));
        }

        [TestCase(-1)]
        [TestCase(double.NaN)]
        [TestCase(double.PositiveInfinity)]
        public void AddForgePoints_InvalidAmount_Throws(double points)
        {
            var forge = CreateForge(new FakeRandom(0.99));

            Assert.Throws<ArgumentOutOfRangeException>(() => forge.AddForgePoints(points));
        }

        [Test]
        public void Constructor_ResumesSavedWeaponAndProgress()
        {
            var sword = new FakeWeapon("sword", 20, 12);
            _state.ActiveWeaponId = "sword";
            _state.ForgeProgress = 15;

            var forge = new ForgeService(_state, _balance, new FakeCatalog(Dagger, sword), new FakeRandom(0.99));

            Assert.That(forge.ActiveWeapon, Is.SameAs(sword));
            Assert.That(forge.Progress01, Is.EqualTo(0.75).Within(1e-9));
        }
    }
}
