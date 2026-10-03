using System;
using System.Globalization;
using System.Text;

namespace AnvilClicker.Core
{
    public enum NumberNotation
    {
        /// <summary>12,3K · 4,56M · 1,00aa</summary>
        Short,

        /// <summary>1,23e45</summary>
        Scientific
    }

    /// <summary>
    /// Separators used by <see cref="NumberFormatter"/>. Explicit instead of CultureInfo so output is
    /// identical on every platform (WebGL builds may ship without culture data).
    /// </summary>
    public sealed class NumberFormatOptions
    {
        public static readonly NumberFormatOptions PtBr = new NumberFormatOptions(',', '.');
        public static readonly NumberFormatOptions Invariant = new NumberFormatOptions('.', ',');

        public NumberFormatOptions(char decimalSeparator, char groupSeparator)
        {
            DecimalSeparator = decimalSeparator;
            GroupSeparator = groupSeparator;
        }

        public char DecimalSeparator { get; }

        public char GroupSeparator { get; }
    }

    /// <summary>Formats game numbers for display. Never format gold or forge points with ToString() directly.</summary>
    public static class NumberFormatter
    {
        /// <summary>Values below this are shown in full ("9.999"); from here on they are abbreviated.</summary>
        public const double AbbreviationThreshold = 10_000;

        const int SignificantDigits = 3;

        static readonly string[] NamedSuffixes = { "", "K", "M", "B", "T", "Qa", "Qi", "Sx", "Sp", "Oc", "No", "Dc" };

        public static string Format(double value, NumberNotation notation = NumberNotation.Short, NumberFormatOptions options = null)
        {
            options ??= NumberFormatOptions.PtBr;

            if (double.IsNaN(value)) return "NaN";
            if (double.IsPositiveInfinity(value)) return "∞";
            if (double.IsNegativeInfinity(value)) return "-∞";
            if (value < 0) return "-" + Format(-value, notation, options);

            if (value < AbbreviationThreshold) return FormatPlain(value, options);

            return notation == NumberNotation.Scientific
                ? FormatScientific(value, options)
                : FormatShort(value, options);
        }

        /// <summary>Suffix for a power of one thousand: 1 → "K", 11 → "Dc", 12 → "aa", 13 → "ab"…</summary>
        public static string GetSuffix(int thousandsExponent)
        {
            if (thousandsExponent < 0) throw new ArgumentOutOfRangeException(nameof(thousandsExponent));
            if (thousandsExponent < NamedSuffixes.Length) return NamedSuffixes[thousandsExponent];

            var index = thousandsExponent - NamedSuffixes.Length;
            var first = (char)('a' + index / 26);
            var second = (char)('a' + index % 26);
            return new string(new[] { first, second });
        }

        static string FormatPlain(double value, NumberFormatOptions options)
        {
            var rounded = Math.Round(value, 1, MidpointRounding.AwayFromZero);
            var integerPart = Math.Floor(rounded);
            var grouped = GroupDigits((long)integerPart, options.GroupSeparator);

            var tenths = (int)Math.Round((rounded - integerPart) * 10, MidpointRounding.AwayFromZero);
            return tenths == 0 ? grouped : grouped + options.DecimalSeparator + tenths.ToString(CultureInfo.InvariantCulture);
        }

        static string FormatShort(double value, NumberFormatOptions options)
        {
            var exponent = (int)Math.Floor(Math.Log10(value) / 3);
            var mantissa = value / Math.Pow(1000, exponent);

            // Log10 can land one step off for exact powers of ten; normalise to [1, 1000).
            if (mantissa >= 1000) { mantissa /= 1000; exponent++; }
            else if (mantissa < 1) { mantissa *= 1000; exponent--; }

            var decimals = SignificantDigits - IntegerDigits(mantissa);
            var rounded = Math.Round(mantissa, decimals, MidpointRounding.AwayFromZero);

            // Rounding can carry into the next suffix: 999.95K → 1,00M.
            if (rounded >= 1000)
            {
                rounded /= 1000;
                exponent++;
                decimals = SignificantDigits - 1;
            }

            return FixedPoint(rounded, decimals, options) + GetSuffix(exponent);
        }

        static string FormatScientific(double value, NumberFormatOptions options)
        {
            var exponent = (int)Math.Floor(Math.Log10(value));
            var mantissa = value / Math.Pow(10, exponent);

            if (mantissa >= 10) { mantissa /= 10; exponent++; }
            else if (mantissa < 1) { mantissa *= 10; exponent--; }

            var rounded = Math.Round(mantissa, SignificantDigits - 1, MidpointRounding.AwayFromZero);
            if (rounded >= 10)
            {
                rounded /= 10;
                exponent++;
            }

            return FixedPoint(rounded, SignificantDigits - 1, options) + "e" + exponent.ToString(CultureInfo.InvariantCulture);
        }

        static int IntegerDigits(double mantissa) => mantissa < 10 ? 1 : mantissa < 100 ? 2 : 3;

        static string FixedPoint(double value, int decimals, NumberFormatOptions options)
        {
            var text = value.ToString("F" + decimals, CultureInfo.InvariantCulture);
            return options.DecimalSeparator == '.' ? text : text.Replace('.', options.DecimalSeparator);
        }

        static string GroupDigits(long value, char separator)
        {
            var digits = value.ToString(CultureInfo.InvariantCulture);
            if (digits.Length <= 3) return digits;

            var builder = new StringBuilder(digits.Length + digits.Length / 3);
            var firstGroup = digits.Length % 3;
            if (firstGroup == 0) firstGroup = 3;

            builder.Append(digits, 0, firstGroup);
            for (var i = firstGroup; i < digits.Length; i += 3)
            {
                builder.Append(separator);
                builder.Append(digits, i, 3);
            }

            return builder.ToString();
        }
    }
}
