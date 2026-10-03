using System;
using AnvilClicker.Core;
using NUnit.Framework;

namespace AnvilClicker.Tests
{
    public class WalletTests
    {
        GameState _state;
        Wallet _wallet;

        [SetUp]
        public void SetUp()
        {
            _state = new GameState();
            _wallet = new Wallet(_state);
        }

        [Test]
        public void Add_IncreasesGoldAndLifetimeGold()
        {
            _wallet.Add(25);
            _wallet.Add(5);

            Assert.That(_wallet.Gold, Is.EqualTo(30));
            Assert.That(_wallet.LifetimeGold, Is.EqualTo(30));
        }

        [Test]
        public void Add_RaisesGoldChangedWithNewBalance()
        {
            double? received = null;
            _wallet.GoldChanged += gold => received = gold;

            _wallet.Add(12);

            Assert.That(received, Is.EqualTo(12));
        }

        [Test]
        public void Add_Zero_DoesNotRaiseEvent()
        {
            var raised = false;
            _wallet.GoldChanged += _ => raised = true;

            _wallet.Add(0);

            Assert.That(raised, Is.False);
        }

        [TestCase(-1)]
        [TestCase(double.NaN)]
        [TestCase(double.PositiveInfinity)]
        public void Add_InvalidAmount_Throws(double amount)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _wallet.Add(amount));
            Assert.That(_wallet.Gold, Is.EqualTo(0));
        }

        [Test]
        public void TrySpend_WithEnoughGold_DeductsButKeepsLifetimeGold()
        {
            _wallet.Add(50);

            var spent = _wallet.TrySpend(20);

            Assert.That(spent, Is.True);
            Assert.That(_wallet.Gold, Is.EqualTo(30));
            Assert.That(_wallet.LifetimeGold, Is.EqualTo(50));
        }

        [Test]
        public void TrySpend_WithoutEnoughGold_ReturnsFalseAndChangesNothing()
        {
            _wallet.Add(10);
            var raised = false;
            _wallet.GoldChanged += _ => raised = true;

            var spent = _wallet.TrySpend(10.5);

            Assert.That(spent, Is.False);
            Assert.That(_wallet.Gold, Is.EqualTo(10));
            Assert.That(raised, Is.False);
        }

        [Test]
        public void TrySpend_ExactBalance_Succeeds()
        {
            _wallet.Add(10);

            Assert.That(_wallet.TrySpend(10), Is.True);
            Assert.That(_wallet.Gold, Is.EqualTo(0));
        }

        [Test]
        public void TrySpend_NegativeAmount_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _wallet.TrySpend(-5));
        }

        [Test]
        public void CanAfford_ComparesAgainstBalance()
        {
            _wallet.Add(10);

            Assert.That(_wallet.CanAfford(10), Is.True);
            Assert.That(_wallet.CanAfford(11), Is.False);
        }
    }
}
