using AnvilClicker.Core;
using NUnit.Framework;

namespace AnvilClicker.Tests
{
    public class RoomServiceTests
    {
        FakeRoom _workshop;
        FakeRoom _storage;
        GameContext _context;

        [SetUp]
        public void SetUp()
        {
            _workshop = new FakeRoom("workshop");
            _storage = new FakeRoom("storage", cost: 500, unlockAtLifetimeGold: 200);
            _context = TestData.Context(new FakeCatalog(TestData.Dagger()).With(_workshop, _storage));
        }

        RoomService Rooms => _context.Rooms;

        [Test]
        public void FreeRoom_IsUnlockedFromTheStart()
        {
            Assert.That(Rooms.IsUnlocked(_workshop), Is.True);
            Assert.That(_context.State.UnlockedRooms, Contains.Item("workshop"));
        }

        [Test]
        public void PaidRoom_StartsLocked()
        {
            Assert.That(Rooms.IsUnlocked(_storage), Is.False);
        }

        [Test]
        public void PaidRoom_IsHiddenUntilEnoughGoldWasEarned()
        {
            Assert.That(Rooms.IsAvailable(_storage), Is.False);

            _context.Wallet.Add(200);

            Assert.That(Rooms.IsAvailable(_storage), Is.True);
        }

        [Test]
        public void TryBuy_WithEnoughGold_BuildsTheRoomAndSpendsTheCost()
        {
            _context.Wallet.Add(600);

            var bought = Rooms.TryBuy(_storage);

            Assert.That(bought, Is.True);
            Assert.That(Rooms.IsUnlocked(_storage), Is.True);
            Assert.That(_context.Wallet.Gold, Is.EqualTo(100));
        }

        [Test]
        public void TryBuy_RaisesRoomUnlocked()
        {
            _context.Wallet.Add(600);
            IRoomDefinition unlocked = null;
            Rooms.RoomUnlocked += room => unlocked = room;

            Rooms.TryBuy(_storage);

            Assert.That(unlocked, Is.SameAs(_storage));
        }

        [Test]
        public void TryBuy_WithoutEnoughGold_ChangesNothing()
        {
            _context.Wallet.Add(300); // available (>= 200 lifetime) but cannot pay 500

            Assert.That(Rooms.CanBuy(_storage), Is.False);
            Assert.That(Rooms.TryBuy(_storage), Is.False);
            Assert.That(_context.Wallet.Gold, Is.EqualTo(300));
        }

        [Test]
        public void TryBuy_BeforeItIsAvailable_Fails()
        {
            _context.State.Gold = 10_000; // rich now, but only 0 earned in lifetime

            Assert.That(Rooms.TryBuy(_storage), Is.False);
        }

        [Test]
        public void TryBuy_AlreadyBuilt_DoesNotChargeTwice()
        {
            _context.Wallet.Add(2000);
            Rooms.TryBuy(_storage);
            var goldAfterFirst = _context.Wallet.Gold;

            Assert.That(Rooms.TryBuy(_storage), Is.False);
            Assert.That(_context.Wallet.Gold, Is.EqualTo(goldAfterFirst));
            Assert.That(_context.State.UnlockedRooms, Has.Exactly(1).EqualTo("storage"));
        }

        [Test]
        public void SavedRooms_StayUnlockedOnLoad()
        {
            var state = new GameState();
            state.UnlockedRooms.Add("storage");

            var context = TestData.Context(new FakeCatalog(TestData.Dagger()).With(_workshop, _storage), state: state);

            Assert.That(context.Rooms.IsUnlocked(_storage), Is.True);
            Assert.That(context.Rooms.IsUnlocked(_workshop), Is.True);
        }

        [Test]
        public void StartupGrant_DoesNotRaiseEventsOrDuplicate()
        {
            var state = new GameState();
            state.UnlockedRooms.Add("workshop");

            var context = TestData.Context(new FakeCatalog(TestData.Dagger()).With(_workshop), state: state);

            Assert.That(context.State.UnlockedRooms, Has.Exactly(1).EqualTo("workshop"));
        }
    }
}
