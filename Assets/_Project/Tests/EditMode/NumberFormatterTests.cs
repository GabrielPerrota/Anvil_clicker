using AnvilClicker.Core;
using NUnit.Framework;

namespace AnvilClicker.Tests
{
    public class NumberFormatterTests
    {
        [TestCase(0, "0")]
        [TestCase(7, "7")]
        [TestCase(999, "999")]
        [TestCase(1_000, "1.000")]
        [TestCase(1_234, "1.234")]
        [TestCase(9_999, "9.999")]
        public void Short_BelowThreshold_ShowsFullNumberWithGrouping(double value, string expected)
        {
            Assert.That(NumberFormatter.Format(value), Is.EqualTo(expected));
        }

        [TestCase(2.5, "2,5")]
        [TestCase(0.25, "0,3")]
        [TestCase(0.04, "0")]
        [TestCase(12.96, "13")]
        [TestCase(1_234.56, "1.234,6")]
        public void Short_Fractions_ShowOneDecimalWhenNeeded(double value, string expected)
        {
            Assert.That(NumberFormatter.Format(value), Is.EqualTo(expected));
        }

        [TestCase(10_000, "10,0K")]
        [TestCase(12_345, "12,3K")]
        [TestCase(123_456, "123K")]
        [TestCase(999_499, "999K")]
        [TestCase(1_000_000, "1,00M")]
        [TestCase(4_560_000, "4,56M")]
        [TestCase(1.5e9, "1,50B")]
        [TestCase(2e12, "2,00T")]
        [TestCase(1e15, "1,00Qa")]
        [TestCase(1e33, "1,00Dc")]
        [TestCase(1e36, "1,00aa")]
        [TestCase(1e39, "1,00ab")]
        public void Short_AboveThreshold_UsesThreeSignificantDigitsAndSuffix(double value, string expected)
        {
            Assert.That(NumberFormatter.Format(value), Is.EqualTo(expected));
        }

        [TestCase(999_950, "1,00M")]
        [TestCase(999_999_999, "1,00B")]
        public void Short_RoundingCarriesIntoNextSuffix(double value, string expected)
        {
            Assert.That(NumberFormatter.Format(value), Is.EqualTo(expected));
        }

        [Test]
        public void Short_DoubleMaxValue_DoesNotThrow()
        {
            Assert.That(NumberFormatter.Format(double.MaxValue), Is.Not.Empty);
        }

        [TestCase(-12_345, "-12,3K")]
        [TestCase(-7, "-7")]
        public void Negative_KeepsSign(double value, string expected)
        {
            Assert.That(NumberFormatter.Format(value), Is.EqualTo(expected));
        }

        [Test]
        public void SpecialValues_HaveReadableText()
        {
            Assert.That(NumberFormatter.Format(double.NaN), Is.EqualTo("NaN"));
            Assert.That(NumberFormatter.Format(double.PositiveInfinity), Is.EqualTo("∞"));
            Assert.That(NumberFormatter.Format(double.NegativeInfinity), Is.EqualTo("-∞"));
        }

        [TestCase(12_345, "1,23e4")]
        [TestCase(1.234e45, "1,23e45")]
        [TestCase(9.999e20, "1,00e21")]
        [TestCase(999, "999")]
        public void Scientific_FormatsMantissaAndExponent(double value, string expected)
        {
            Assert.That(NumberFormatter.Format(value, NumberNotation.Scientific), Is.EqualTo(expected));
        }

        [Test]
        public void InvariantOptions_UseDotDecimalAndCommaGrouping()
        {
            Assert.That(NumberFormatter.Format(12_345, NumberNotation.Short, NumberFormatOptions.Invariant), Is.EqualTo("12.3K"));
            Assert.That(NumberFormatter.Format(1_234, NumberNotation.Short, NumberFormatOptions.Invariant), Is.EqualTo("1,234"));
        }

        [TestCase(0, "")]
        [TestCase(1, "K")]
        [TestCase(11, "Dc")]
        [TestCase(12, "aa")]
        [TestCase(37, "az")]
        [TestCase(38, "ba")]
        public void GetSuffix_MapsPowersOfThousand(int exponent, string expected)
        {
            Assert.That(NumberFormatter.GetSuffix(exponent), Is.EqualTo(expected));
        }
    }
}
