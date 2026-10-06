using AnvilClicker.Core;
using NUnit.Framework;

namespace AnvilClicker.Tests
{
    public class LaunchArgumentsTests
    {
        [Test]
        public void GetValue_ReturnsTheValueAfterTheName()
        {
            var args = new[] { "game.exe", "-screen-width", "1280", "-saveDir", "C:/tmp/save" };

            Assert.That(LaunchArguments.GetValue(args, "-saveDir"), Is.EqualTo("C:/tmp/save"));
        }

        [Test]
        public void GetValue_IsCaseInsensitive()
        {
            Assert.That(LaunchArguments.GetValue(new[] { "-SAVEDIR", "x" }, "-saveDir"), Is.EqualTo("x"));
        }

        [Test]
        public void GetValue_MissingName_ReturnsNull()
        {
            Assert.That(LaunchArguments.GetValue(new[] { "game.exe" }, "-saveDir"), Is.Null);
        }

        [Test]
        public void GetValue_NameWithoutValue_ReturnsNull()
        {
            Assert.That(LaunchArguments.GetValue(new[] { "game.exe", "-saveDir" }, "-saveDir"), Is.Null);
        }

        [Test]
        public void GetValue_NextTokenIsAnotherFlag_ReturnsNull()
        {
            Assert.That(LaunchArguments.GetValue(new[] { "-saveDir", "-batchmode" }, "-saveDir"), Is.Null);
        }

        [Test]
        public void GetValue_NullArguments_ReturnNull()
        {
            Assert.That(LaunchArguments.GetValue(null, "-saveDir"), Is.Null);
            Assert.That(LaunchArguments.GetValue(new[] { "-saveDir", "x" }, null), Is.Null);
        }
    }
}
