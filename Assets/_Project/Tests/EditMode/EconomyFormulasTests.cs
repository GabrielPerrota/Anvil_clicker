using System;
using AnvilClicker.Core;
using NUnit.Framework;

namespace AnvilClicker.Tests
{
    public class EconomyFormulasTests
    {
        [TestCase(0, 15)]
        [TestCase(1, 17.25)]
        [TestCase(10, 60.68336)]
        public void Cost_GrowsGeometrically(int owned, double expected)
        {
            Assert.That(EconomyFormulas.Cost(15, 1.15, owned), Is.EqualTo(expected).Within(1e-4));
        }

        [TestCase(0, 1)]
        [TestCase(0, 10)]
        [TestCase(7, 25)]
        [TestCase(40, 100)]
        public void BulkCost_EqualsSumOfIndividualCosts(int owned, int count)
        {
            var sum = 0d;
            for (var i = 0; i < count; i++) sum += EconomyFormulas.Cost(15, 1.15, owned + i);

            Assert.That(EconomyFormulas.BulkCost(15, 1.15, owned, count), Is.EqualTo(sum).Within(sum * 1e-9));
        }

        [Test]
        public void BulkCost_WithoutGrowth_IsLinear()
        {
            Assert.That(EconomyFormulas.BulkCost(400, 1, 3, 5), Is.EqualTo(2000));
        }

        [Test]
        public void BulkCost_ZeroOrNegativeCount_IsFree()
        {
            Assert.That(EconomyFormulas.BulkCost(15, 1.15, 0, 0), Is.EqualTo(0));
            Assert.That(EconomyFormulas.BulkCost(15, 1.15, 0, -3), Is.EqualTo(0));
        }

        [Test]
        public void MaxAffordable_ExactBudget_BuysAll()
        {
            var budget = EconomyFormulas.BulkCost(15, 1.15, 4, 12);

            Assert.That(EconomyFormulas.MaxAffordable(15, 1.15, 4, budget), Is.EqualTo(12));
        }

        [Test]
        public void MaxAffordable_JustBelowBudget_BuysOneLess()
        {
            var budget = EconomyFormulas.BulkCost(15, 1.15, 4, 12) - 0.01;

            Assert.That(EconomyFormulas.MaxAffordable(15, 1.15, 4, budget), Is.EqualTo(11));
        }

        [Test]
        public void MaxAffordable_CannotAffordFirst_ReturnsZero()
        {
            Assert.That(EconomyFormulas.MaxAffordable(15, 1.15, 0, 14.99), Is.EqualTo(0));
            Assert.That(EconomyFormulas.MaxAffordable(15, 1.15, 0, 0), Is.EqualTo(0));
        }

        [Test]
        public void MaxAffordable_RespectsLimit()
        {
            Assert.That(EconomyFormulas.MaxAffordable(15, 1.15, 0, 1e12, limit: 3), Is.EqualTo(3));
        }

        [Test]
        public void MaxAffordable_WithoutGrowth_DividesBudget()
        {
            Assert.That(EconomyFormulas.MaxAffordable(100, 1, 0, 950), Is.EqualTo(9));
        }

        [Test]
        public void MaxAffordable_HugeBudget_StaysConsistentWithBulkCost()
        {
            var count = EconomyFormulas.MaxAffordable(15, 1.15, 0, 1e30);

            Assert.That(EconomyFormulas.BulkCost(15, 1.15, 0, count), Is.LessThanOrEqualTo(1e30));
            Assert.That(EconomyFormulas.BulkCost(15, 1.15, 0, count + 1), Is.GreaterThan(1e30));
        }

        [Test]
        public void InvalidArguments_Throw()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => EconomyFormulas.Cost(0, 1.15, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => EconomyFormulas.Cost(15, 0.9, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => EconomyFormulas.Cost(15, 1.15, -1));
        }
    }
}
