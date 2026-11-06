using System;
using System.Globalization;

namespace ET.Monetization
{
    /// <summary>
    /// Standardized IAP pricing tier tags matching the 172 regions tax reference matrix.
    /// Devs select a price tag (e.g. tag_0.99, tag_1.99, tag_4.99) to automatically apply
    /// predefined regional pricing across Android (Google Play) and iOS (Apple App Store).
    /// </summary>
    public enum IAPPriceTag
    {
        tag_0_99 = 0,
        tag_1_99 = 1,
        tag_2_99 = 2,
        tag_3_99 = 3,
        tag_4_99 = 4,
        tag_5_99 = 5,
        tag_6_99 = 6,
        tag_7_99 = 7,
        tag_8_99 = 8,
        tag_9_99 = 9,
        tag_10_99 = 10,
        tag_11_99 = 11,
        tag_12_99 = 12
    }

    public static class IAPPriceTagExtensions
    {
        public static readonly float[] UsdPrices = new float[]
        {
            0.99f, 1.99f, 2.99f, 3.99f, 4.99f, 5.99f, 6.99f, 7.99f, 8.99f, 9.99f, 10.99f, 11.99f, 12.99f
        };

        public static readonly string[] TagNames = new string[]
        {
            "tag_0.99", "tag_1.99", "tag_2.99", "tag_3.99", "tag_4.99", "tag_5.99",
            "tag_6.99", "tag_7.99", "tag_8.99", "tag_9.99", "tag_10.99", "tag_11.99", "tag_12.99"
        };

        public static readonly string[] DisplayLabels = new string[]
        {
            "tag_0.99 ($0.99)",
            "tag_1.99 ($1.99)",
            "tag_2.99 ($2.99)",
            "tag_3.99 ($3.99)",
            "tag_4.99 ($4.99)",
            "tag_5.99 ($5.99)",
            "tag_6.99 ($6.99)",
            "tag_7.99 ($7.99)",
            "tag_8.99 ($8.99)",
            "tag_9.99 ($9.99)",
            "tag_10.99 ($10.99)",
            "tag_11.99 ($11.99)",
            "tag_12.99 ($12.99)"
        };

        public static readonly string[] CsvHeaders = new string[]
        {
            "Tag 0.99$", "Tag 1.99$", "Tag 2.99$", "Tag 3.99$", "Tag 4.99$", "Tag 5.99$",
            "Tag 6.99$", "Tag 7.99$", "Tag 8.99$", "Tag 9.99$", "Tag 10.99$", "Tag 11.99$", "Tag 12.99$"
        };

        public static float ToUsd(this IAPPriceTag tag)
        {
            int idx = (int)tag;
            if (idx >= 0 && idx < UsdPrices.Length) return UsdPrices[idx];
            return 0.99f;
        }

        public static string ToTagName(this IAPPriceTag tag)
        {
            int idx = (int)tag;
            if (idx >= 0 && idx < TagNames.Length) return TagNames[idx];
            return "tag_0.99";
        }

        public static string ToDisplayLabel(this IAPPriceTag tag)
        {
            int idx = (int)tag;
            if (idx >= 0 && idx < DisplayLabels.Length) return DisplayLabels[idx];
            return "tag_0.99 ($0.99)";
        }

        public static string ToCsvHeader(this IAPPriceTag tag)
        {
            int idx = (int)tag;
            if (idx >= 0 && idx < CsvHeaders.Length) return CsvHeaders[idx];
            return "Tag 0.99$";
        }

        public static (long units, int nanos) ToUnitsAndNanos(this IAPPriceTag tag)
        {
            float usd = tag.ToUsd();
            decimal decPrice = Math.Round((decimal)usd, 2, MidpointRounding.AwayFromZero);
            long wholeUnits = (long)decimal.Truncate(decPrice);
            decimal fraction = decPrice - wholeUnits;
            int nanos = (int)Math.Round(fraction * 1_000_000_000m, 0, MidpointRounding.AwayFromZero);
            return (wholeUnits, nanos);
        }

        public static IAPPriceTag FromUsd(float usd)
        {
            int bestIdx = 0;
            float bestDiff = float.MaxValue;
            for (int i = 0; i < UsdPrices.Length; i++)
            {
                float diff = Math.Abs(UsdPrices[i] - usd);
                if (diff < bestDiff)
                {
                    bestDiff = diff;
                    bestIdx = i;
                }
            }
            return (IAPPriceTag)bestIdx;
        }

        public static IAPPriceTag FromString(string tagStr)
        {
            if (string.IsNullOrEmpty(tagStr)) return IAPPriceTag.tag_0_99;
            string clean = tagStr.Trim().ToLowerInvariant().Replace("$", "").Replace(" ", "").Replace("_", ".");
            if (clean.StartsWith("tag.")) clean = clean.Substring(4);
            if (clean.StartsWith("tag")) clean = clean.Substring(3);

            if (float.TryParse(clean, NumberStyles.Float, CultureInfo.InvariantCulture, out float usd))
            {
                return FromUsd(usd);
            }
            return IAPPriceTag.tag_0_99;
        }
    }
}
