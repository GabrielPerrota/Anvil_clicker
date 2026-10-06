using AnvilClicker.Core;
using NUnit.Framework;

namespace AnvilClicker.Tests
{
    public class IsoMathTests
    {
        const double Tolerance = 1e-9;

        [Test]
        public void ScreenInputToWorld_Horizontal_IsFullSpeed()
        {
            var (x, y) = IsoMath.ScreenInputToWorld(1, 0);

            Assert.That(x, Is.EqualTo(1).Within(Tolerance));
            Assert.That(y, Is.EqualTo(0).Within(Tolerance));
        }

        [Test]
        public void ScreenInputToWorld_Vertical_IsSquashedToHalf()
        {
            var (x, y) = IsoMath.ScreenInputToWorld(0, 1);

            Assert.That(x, Is.EqualTo(0).Within(Tolerance));
            Assert.That(y, Is.EqualTo(0.5).Within(Tolerance));
        }

        [Test]
        public void ScreenInputToWorld_Diagonal_IsNotFasterThanStraight()
        {
            var (x, y) = IsoMath.ScreenInputToWorld(1, 1);
            var (sx, sy) = IsoMath.ScreenInputToWorld(1, 0);

            var diagonalSpeed = System.Math.Sqrt(x * x + (y / IsoMath.VerticalSquash) * (y / IsoMath.VerticalSquash));
            var straightSpeed = System.Math.Sqrt(sx * sx + (sy / IsoMath.VerticalSquash) * (sy / IsoMath.VerticalSquash));

            Assert.That(diagonalSpeed, Is.EqualTo(straightSpeed).Within(Tolerance));
        }

        [TestCase(1, 0, 1, 0)]
        [TestCase(-1, 0, -1, 0)]
        [TestCase(0, -1, 0, -0.5)]
        [TestCase(5, 0, 1, 0)]
        public void ScreenInputToWorld_IgnoresInputMagnitude(double inputX, double inputY, double expectedX, double expectedY)
        {
            var (x, y) = IsoMath.ScreenInputToWorld(inputX, inputY);

            Assert.That(x, Is.EqualTo(expectedX).Within(Tolerance));
            Assert.That(y, Is.EqualTo(expectedY).Within(Tolerance));
        }

        [Test]
        public void ScreenInputToWorld_NoInput_IsZero()
        {
            Assert.That(IsoMath.ScreenInputToWorld(0, 0), Is.EqualTo((0d, 0d)));
            Assert.That(IsoMath.ScreenInputToWorld(double.NaN, 1), Is.EqualTo((0d, 0d)));
        }

        [TestCase(1, -1, FacingDirection.DownRight)]
        [TestCase(-1, -1, FacingDirection.DownLeft)]
        [TestCase(-1, 1, FacingDirection.UpLeft)]
        [TestCase(1, 1, FacingDirection.UpRight)]
        [TestCase(1, 0, FacingDirection.DownRight)]
        [TestCase(0, 1, FacingDirection.UpRight)]
        public void GetFacing_PicksTheScreenQuadrant(double x, double y, FacingDirection expected)
        {
            Assert.That(IsoMath.GetFacing(x, y, FacingDirection.UpLeft), Is.EqualTo(expected));
        }

        [Test]
        public void GetFacing_WhenIdle_KeepsTheCurrentDirection()
        {
            Assert.That(IsoMath.GetFacing(0, 0, FacingDirection.UpLeft), Is.EqualTo(FacingDirection.UpLeft));
        }
    }
}
