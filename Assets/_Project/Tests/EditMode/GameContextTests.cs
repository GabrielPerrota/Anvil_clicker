using AnvilClicker.Core;
using NUnit.Framework;

namespace AnvilClicker.Tests
{
    public class GameContextTests
    {
        [Test]
        public void ForgedWeapon_IsSoldImmediatelyForItsBaseValue()
        {
            var dagger = new FakeWeapon("dagger", forgePointsRequired: 10, baseValue: 5);
            var context = new GameContext(new GameState(), new FakeBalance(), new FakeCatalog(dagger), new FakeRandom(0.99));

            context.Forge.AddForgePoints(30);

            Assert.That(context.Wallet.Gold, Is.EqualTo(15));
            Assert.That(context.Wallet.LifetimeGold, Is.EqualTo(15));
        }

        [Test]
        public void StrikesBelowCompletion_EarnNoGold()
        {
            var dagger = new FakeWeapon("dagger", forgePointsRequired: 10, baseValue: 5);
            var context = new GameContext(new GameState(), new FakeBalance(), new FakeCatalog(dagger), new FakeRandom(0.99));

            for (var i = 0; i < 9; i++) context.Forge.Strike();

            Assert.That(context.Wallet.Gold, Is.EqualTo(0));
        }
    }
}
