using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

namespace ET.Monetization
{
    [Serializable]
    public class IAPRegionalPriceEntry
    {
        public string regionCode;
        public string countryName;
        public string currency;
        public string taxRateType;
        public string priceDisplayType;
        public string collectionResponsibility;
        public string withholdingTaxNotes;
        public string estimatePricePercentage;
        public float[] prices = new float[13];

        public float GetPrice(IAPPriceTag tag)
        {
            int idx = (int)tag;
            if (idx >= 0 && idx < prices.Length) return prices[idx];
            return 0f;
        }

        public (long units, long nanos) GetUnitsAndNanos(IAPPriceTag tag)
        {
            float p = GetPrice(tag);
            decimal dec = Math.Round((decimal)p, 2, MidpointRounding.AwayFromZero);
            long units = (long)decimal.Truncate(dec);
            decimal fraction = dec - units;
            long nanos = (long)Math.Round(fraction * 1_000_000_000m, 0, MidpointRounding.AwayFromZero);
            return (units, nanos);
        }

        public string GetFormattedPrice(IAPPriceTag tag)
        {
            float p = GetPrice(tag);
            if (currency == "VND" || currency == "JPY" || currency == "KRW" || currency == "IDR" || currency == "CLP" || currency == "HUF" || currency == "PYG")
            {
                return $"{p:N0} {currency}";
            }
            return $"{p:N2} {currency}";
        }
    }

    public struct GoogleRegionalPriceConfig
    {
        public string regionCode;
        public string currencyCode;
        public string units;
        public long nanos;
    }

    public static class IAPRegionalPricingDatabase
    {
        public const string DefaultCsvPath = "/Users/taniafelidae/ETStudio/Google_Play_172_Regions_Tax_Reference_2026_Full_Prices.csv";
        public const string PrefKeyCustomCsvPath = "ET_RegionalPricingCsvPath";

        private static readonly object LockObj = new object();
        private static List<IAPRegionalPriceEntry> _entries;
        private static Dictionary<string, IAPRegionalPriceEntry> _lookupByCode;
        private static string _loadedFromSource = "";

        public static string LoadedSource => _loadedFromSource;

        public static IReadOnlyList<IAPRegionalPriceEntry> GetAllEntries()
        {
            EnsureLoaded();
            return _entries;
        }

        public static IAPRegionalPriceEntry GetEntry(string regionCode)
        {
            if (string.IsNullOrEmpty(regionCode)) return null;
            EnsureLoaded();
            return _lookupByCode.TryGetValue(regionCode.Trim().ToUpperInvariant(), out var entry) ? entry : null;
        }

        public static float GetPrice(string regionCode, IAPPriceTag tag)
        {
            var entry = GetEntry(regionCode);
            return entry != null ? entry.GetPrice(tag) : tag.ToUsd();
        }

        public static string GetFormattedPrice(string regionCode, IAPPriceTag tag)
        {
            var entry = GetEntry(regionCode);
            return entry != null ? entry.GetFormattedPrice(tag) : $"${tag.ToUsd():F2} USD";
        }

        public static (string units, long nanos) GetEurPrice(IAPPriceTag tag)
        {
            var de = GetEntry("DE");
            if (de != null)
            {
                var (u, n) = de.GetUnitsAndNanos(tag);
                return (u.ToString(CultureInfo.InvariantCulture), n);
            }
            var (usdU, usdN) = tag.ToUnitsAndNanos();
            return (usdU.ToString(CultureInfo.InvariantCulture), (long)usdN);
        }

        public static List<GoogleRegionalPriceConfig> GetGooglePlayRegionalConfigs(IAPPriceTag tag, HashSet<string> excludeRegions = null)
        {
            EnsureLoaded();
            var list = new List<GoogleRegionalPriceConfig>(_entries.Count);

            for (int i = 0; i < _entries.Count; i++)
            {
                var e = _entries[i];
                if (e == null || string.IsNullOrEmpty(e.regionCode)) continue;
                if (excludeRegions != null && excludeRegions.Contains(e.regionCode)) continue;

                var (units, nanos) = e.GetUnitsAndNanos(tag);
                list.Add(new GoogleRegionalPriceConfig
                {
                    regionCode = e.regionCode,
                    currencyCode = e.currency,
                    units = units.ToString(CultureInfo.InvariantCulture),
                    nanos = nanos
                });
            }

            return list;
        }

        public static void Reload(string customPath = null)
        {
            lock (LockObj)
            {
                LoadDataInternal(customPath);
            }
        }

        private static void EnsureLoaded()
        {
            if (_entries != null && _lookupByCode != null) return;
            lock (LockObj)
            {
                if (_entries == null || _lookupByCode == null)
                {
                    LoadDataInternal();
                }
            }
        }

        private static void LoadDataInternal(string customPath = null)
        {
            string path = !string.IsNullOrEmpty(customPath) ? customPath : GetPreferredCsvPath();
            string csvContent = null;

            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                try
                {
                    csvContent = File.ReadAllText(path, Encoding.UTF8);
                    _loadedFromSource = $"File: {path}";
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[IAPRegionalPricingDatabase] Failed reading CSV from '{path}': {ex.Message}. Falling back to embedded dataset.");
                }
            }

            if (string.IsNullOrEmpty(csvContent))
            {
                csvContent = EmbeddedCsvData;
                _loadedFromSource = "Embedded Dataset (173 regions)";
            }

            ParseCsvContent(csvContent);
        }

        private static string GetPreferredCsvPath()
        {
#if UNITY_EDITOR
            string pref = UnityEditor.EditorPrefs.GetString(PrefKeyCustomCsvPath, "");
            if (!string.IsNullOrEmpty(pref) && File.Exists(pref)) return pref;
#endif
            if (File.Exists(DefaultCsvPath)) return DefaultCsvPath;

            string projectRelative = Path.GetFullPath(Path.Combine(Application.dataPath, "../../ETStudio/Google_Play_172_Regions_Tax_Reference_2026_Full_Prices.csv"));
            if (File.Exists(projectRelative)) return projectRelative;

            return "";
        }

        private static void ParseCsvContent(string text)
        {
            var list = new List<IAPRegionalPriceEntry>();
            var lookup = new Dictionary<string, IAPRegionalPriceEntry>(StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrEmpty(text))
            {
                _entries = list;
                _lookupByCode = lookup;
                return;
            }

            using (var reader = new StringReader(text))
            {
                string line = reader.ReadLine(); // header
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var parts = line.Split(',');
                    if (parts.Length < 21) continue;

                    var entry = new IAPRegionalPriceEntry
                    {
                        regionCode = parts[0].Trim().ToUpperInvariant(),
                        countryName = parts[1].Trim(),
                        currency = parts[2].Trim().ToUpperInvariant(),
                        taxRateType = parts[3].Trim(),
                        priceDisplayType = parts[4].Trim(),
                        collectionResponsibility = parts[5].Trim(),
                        withholdingTaxNotes = parts[6].Trim(),
                        estimatePricePercentage = parts[7].Trim()
                    };

                    for (int t = 0; t < 13; t++)
                    {
                        string pStr = parts[8 + t].Trim();
                        if (float.TryParse(pStr, NumberStyles.Float, CultureInfo.InvariantCulture, out float val))
                        {
                            entry.prices[t] = val;
                        }
                    }

                    list.Add(entry);
                    if (!lookup.ContainsKey(entry.regionCode))
                    {
                        lookup.Add(entry.regionCode, entry);
                    }
                }
            }

            _entries = list;
            _lookupByCode = lookup;
        }

        public const string EmbeddedCsvData = @"Region_Code,Country_Name,Currency,Tax_Rate_Type,Price_Display_Type,Collection_Responsibility,Withholding_Tax_Notes,Estimate Price Percentage,Tag 0.99$,Tag 1.99$,Tag 2.99$,Tag 3.99$,Tag 4.99$,Tag 5.99$,Tag 6.99$,Tag 7.99$,Tag 8.99$,Tag 9.99$,Tag 10.99$,Tag 11.99$,Tag 12.99$
AL,Albania,USD,20%,Inclusive,Google collects & remits,0%,70%,0.99,1.49,2.49,3.49,3.99,4.99,5.99,6.49,7.49,8.49,8.99,9.99,10.99
DZ,Algeria,DZD,0%,Inclusive,Developer responsible,0%,50%,65,135,200,270,335,405,470,540,605,675,740,810,875
AO,Angola,USD,0%,Inclusive,Developer responsible,0%,50%,0.49,0.99,1.49,1.99,2.49,2.99,3.49,3.99,4.49,4.99,5.49,5.99,6.49
AG,Antigua & Barbuda,USD,0%,Inclusive,Developer responsible,0%,80%,0.99,1.49,2.49,2.99,3.99,4.99,5.49,6.49,6.99,7.99,8.99,9.49,10.49
AR,Argentina,ARS,0%,Inclusive,Not available for auto-converted IAP / Card issuers collect,0%,45%,500,900,1400,1900,2400,2800,3300,3800,4200,4700,5200,5700,6100
AM,Armenia,USD,0%,Inclusive,Developer responsible,0%,70%,0.49,1.49,1.99,2.99,3.49,3.99,4.99,5.49,6.49,6.99,7.49,8.49,8.99
AW,Aruba,USD,0%,Inclusive,Developer responsible,0%,100%,0.99,1.99,2.99,3.99,4.99,5.99,6.99,7.99,8.99,9.99,10.99,11.99,12.99
AU,Australia,AUD,10%,Inclusive,Google collects & remits for foreign developers,0%,100%,1.49,2.99,4.99,6.49,7.99,9.49,10.99,12.49,14.49,15.99,17.49,18.99,20.49
AT,Austria,EUR,20%,Inclusive,Google (Merchant of Record),0%,100%,0.99,1.99,2.99,4.49,5.49,6.49,7.49,8.49,9.49,10.99,11.99,12.99,13.99
AZ,Azerbaijan,AZN,0%,Inclusive,Developer responsible,0%,70%,0.99,2.49,3.49,4.49,5.99,6.99,8.49,9.49,10.49,11.99,12.99,14.49,15.49
BS,Bahamas,USD,0%,Inclusive,Developer responsible,0%,100%,0.99,1.99,2.99,3.99,4.99,5.99,6.99,7.99,8.99,9.99,10.99,11.99,12.99
BH,Bahrain,USD,10%,Inclusive,Google collects & remits for foreign developers,0%,100%,0.99,1.99,3.49,4.49,5.49,6.49,7.49,8.99,9.99,10.99,11.99,12.99,14.49
BD,Bangladesh,BDT,15%,Inclusive,Google collects & remits,0%,45%,60,120,185,245,305,365,425,490,550,610,670,730,795
BY,Belarus,USD,20%,Inclusive,Google collects & remits (Paid purchases currently paused),0%,70%,0.99,1.49,2.49,3.49,3.99,4.99,5.99,6.49,7.49,8.49,8.99,9.99,10.99
BE,Belgium,EUR,21%,Inclusive,Google (Merchant of Record),0%,100%,0.99,1.99,3.49,4.49,5.49,6.49,7.49,8.49,9.99,10.99,11.99,12.99,13.99
BZ,Belize,USD,0%,Inclusive,Developer responsible,0%,70%,0.49,1.49,1.99,2.99,3.49,3.99,4.99,5.49,6.49,6.99,7.49,8.49,8.99
BJ,Benin,EUR,0%,Exclusive/Inclusive,Google collects 18% VAT for foreign developers,0%,45%,0.49,0.99,1.49,1.99,2.49,2.99,3.49,3.99,4.49,4.99,5.49,5.99,6.49
BM,Bermuda,USD,0%,Inclusive,Developer responsible,0%,100%,0.99,1.99,2.99,3.99,4.99,5.99,6.99,7.99,8.99,9.99,10.99,11.99,12.99
BO,Bolivia,BOB,0%,Inclusive,Developer responsible,0%,50%,3.49,6.99,10.49,13.99,16.90,20.49,23.90,27.49,30.90,34.49,37.99,41.49,44.99
BA,Bosnia & Herzegovina,USD,0%,Inclusive,Developer responsible,0%,70%,0.49,1.49,1.99,2.99,3.49,3.99,4.99,5.49,6.49,6.99,7.49,8.49,8.99
BW,Botswana,USD,0%,Inclusive,Developer responsible,0%,70%,0.49,1.49,1.99,2.99,3.49,3.99,4.99,5.49,6.49,6.99,7.49,8.49,8.99
BR,Brazil,BRL,0%,Inclusive,Developer responsible,IRRF & CIDE withheld on FX payout,50%,2.49,4.90,7.99,10.49,12.99,15.49,17.90,20.99,23.49,25.99,28.49,30.90,33.99
VG,British Virgin Islands,USD,0%,Inclusive,Developer responsible,0%,100%,0.99,1.99,2.99,3.99,4.99,5.99,6.99,7.99,8.99,9.99,10.99,11.99,12.99
BG,Bulgaria,EUR,20%,Inclusive,Google (Merchant of Record),0%,80%,0.99,1.49,2.49,3.49,4.49,4.99,5.99,6.99,7.99,8.49,9.49,10.49,10.99
BF,Burkina Faso,EUR,0%,Inclusive,Developer responsible,0%,45%,0.49,0.99,1.49,1.99,2.49,2.99,3.49,3.99,4.49,4.99,5.49,5.99,6.49
KH,Cambodia,USD,0%,Exclusive/Inclusive,Google collects 10% VAT,0%,45%,0.49,0.99,1.49,1.99,2.49,2.99,3.49,3.99,4.49,4.99,5.49,5.99,6.49
CM,Cameroon,XAF,19.25%,Inclusive,Google collects & remits,0%,50%,350,700,1050,1400,1750,2100,2450,2800,3150,3500,3850,4200,4550
CA,Canada,CAD,0%,Exclusive,Google collects GST/HST and PST/QST at checkout,0% WHT,100%,1.49,2.99,3.99,5.49,6.99,8.49,9.99,10.99,12.49,13.99,15.49,16.99,17.99
CV,Cape Verde,USD,0%,Inclusive,Developer responsible,0%,50%,0.49,0.99,1.49,1.99,2.49,2.99,3.49,3.99,4.49,4.99,5.49,5.99,6.49
KY,Cayman Islands,USD,0%,Inclusive,Developer responsible,0%,100%,0.99,1.99,2.99,3.99,4.99,5.99,6.99,7.99,8.99,9.99,10.99,11.99,12.99
CF,Central African Republic,EUR,0%,Inclusive,Developer responsible,0%,45%,0.49,0.99,1.49,1.99,2.49,2.99,3.49,3.99,4.49,4.99,5.49,5.99,6.49
TD,Chad,USD,0%,Inclusive,Developer responsible,0%,45%,0.49,0.99,1.49,1.99,2.49,2.99,3.49,3.99,4.49,4.99,5.49,5.99,6.49
CL,Chile,CLP,19%,Inclusive,Google collects & remits for foreign developers,0%,80%,900,1800,2700,3600,4500,5400,6300,7250,8150,9050,9950,10850,11750
CO,Colombia,COP,0%,Inclusive,Developer responsible,0%,60%,2000,5000,7000,10000,12000,14000,17000,19000,22000,24000,26000,29000,31000
KM,Comoros,USD,0%,Inclusive,Developer responsible,0%,45%,0.49,0.99,1.49,1.99,2.49,2.99,3.49,3.99,4.49,4.99,5.49,5.99,6.49
CG,Congo - Brazzaville,USD,0%,Inclusive,Developer responsible,0%,45%,0.49,0.99,1.49,1.99,2.49,2.99,3.49,3.99,4.49,4.99,5.49,5.99,6.49
CD,Congo - Kinshasa,USD,0%,Inclusive,Developer responsible,0%,45%,0.49,0.99,1.49,1.99,2.49,2.99,3.49,3.99,4.49,4.99,5.49,5.99,6.49
CR,Costa Rica,CRC,0%,Inclusive,Developer responsible,0%,80%,410,820,1230,1640,2060,2470,2880,3290,3700,4120,4530,4940,5350
HR,Croatia,EUR,25%,Inclusive,Google (Merchant of Record),0%,80%,0.99,1.99,2.49,3.49,4.49,5.49,6.49,6.99,7.99,8.99,9.99,10.99,11.49
CY,Cyprus,EUR,19%,Inclusive,Google (Merchant of Record),0%,85%,0.99,1.99,2.49,3.49,4.49,5.49,6.49,7.49,7.99,8.99,9.99,10.99,11.99
CZ,Czechia,CZK,21%,Inclusive,Google (Merchant of Record),0%,85%,24,48,72,96,121,145,169,193,217,241,266,290,314
CI,Côte d’Ivoire,XOF,18%,Inclusive,Google collects & remits,0%,50%,350,700,1050,1400,1750,2100,2450,2800,3150,3500,3850,4150,4500
DK,Denmark,DKK,25%,Inclusive,Google (Merchant of Record),0%,100%,8.49,16.99,25.49,33.99,42.49,50.99,59.49,67.99,76.49,84.99,93.49,101.99,110.49
DJ,Djibouti,USD,0%,Inclusive,Developer responsible,0%,45%,0.49,0.99,1.49,1.99,2.49,2.99,3.49,3.99,4.49,4.99,5.49,5.99,6.49
DM,Dominica,USD,0%,Inclusive,Developer responsible,0%,70%,0.49,1.49,1.99,2.99,3.49,3.99,4.99,5.49,6.49,6.99,7.49,8.49,8.99
DO,Dominican Republic,USD,0%,Inclusive,Developer responsible,0%,70%,0.49,1.49,1.99,2.99,3.49,3.99,4.99,5.49,6.49,6.99,7.49,8.49,8.99
EC,Ecuador,USD,0%,Inclusive,Developer responsible,0%,60%,0.49,0.99,1.99,2.49,2.99,3.49,3.99,4.99,5.49,5.99,6.49,6.99,7.99
EG,Egypt,EGP,14%,Inclusive,Google collects & remits,Up to 20% on DCB,45%,25,50,75,100,125,151,176,201,226,251,276,301,327
SV,El Salvador,USD,0%,Inclusive,Developer responsible,0%,60%,0.49,0.99,1.99,2.49,2.99,3.49,3.99,4.99,5.49,5.99,6.49,6.99,7.99
ER,Eritrea,USD,0%,Inclusive,Developer responsible,0%,45%,0.49,0.99,1.49,1.99,2.49,2.99,3.49,3.99,4.49,4.99,5.49,5.99,6.49
EE,Estonia,EUR,24%,Inclusive,Google (Merchant of Record),0%,85%,0.99,1.99,2.99,3.99,4.49,5.49,6.49,7.49,8.49,9.49,10.49,11.49,12.49
ET,Ethiopia,ETB,0%,Inclusive,Developer responsible,0%,45%,55,110,170,225,280,335,395,450,505,560,620,675,730
FJ,Fiji,USD,0%,Inclusive,Developer responsible,0%,60%,0.49,0.99,1.99,2.49,2.99,3.49,3.99,4.99,5.49,5.99,6.49,6.99,7.99
FI,Finland,EUR,25.50%,Inclusive,Google (Merchant of Record),0%,100%,0.99,1.99,3.49,4.49,5.49,6.99,7.99,8.99,9.99,11.49,12.49,13.49,14.49
FR,France,EUR,20%,Inclusive,Google (Merchant of Record),0%,100%,0.99,1.99,2.99,4.49,5.49,6.49,7.49,8.49,9.49,10.99,11.99,12.99,13.99
GA,Gabon,EUR,0%,Inclusive,Developer responsible,0%,70%,0.49,1.49,1.99,2.49,2.99,3.99,4.49,4.99,5.49,6.49,6.99,7.49,7.99
GM,Gambia,USD,0%,Inclusive,Developer responsible,0%,45%,0.49,0.99,1.49,1.99,2.49,2.99,3.49,3.99,4.49,4.99,5.49,5.99,6.49
GE,Georgia,GEL,18%,Inclusive,Google collects & remits,0%,70%,1.90,4.49,6.49,8.99,10.90,13.49,15.49,17.99,19.90,22.49,24.49,26.49,28.99
DE,Germany,EUR,19%,Inclusive,Google (Merchant of Record),0%,100%,0.99,1.99,2.99,4.49,5.49,6.49,7.49,8.49,9.49,10.49,11.99,12.99,13.99
GH,Ghana,GHS,20%,Inclusive,Google collects & remits,0%,50%,8.99,17.99,26.99,35.99,44.99,53.99,62.99,71.99,80.99,89.99,98.99,107.99,116.99
GI,Gibraltar,GBP,0%,Inclusive,Developer responsible,0%,100%,0.99,1.49,2.49,2.99,3.99,4.99,5.49,6.49,6.99,7.99,8.99,9.49,10.49
GR,Greece,EUR,24%,Inclusive,Google (Merchant of Record),0%,85%,0.99,1.99,2.99,3.99,4.49,5.49,6.49,7.49,8.49,9.49,10.49,11.49,12.49
GD,Grenada,USD,0%,Inclusive,Developer responsible,0%,70%,0.49,1.49,1.99,2.99,3.49,3.99,4.99,5.49,6.49,6.99,7.49,8.49,8.99
GT,Guatemala,USD,0%,Inclusive,Developer responsible,0%,60%,0.49,0.99,1.99,2.49,2.99,3.49,3.99,4.99,5.49,5.99,6.49,6.99,7.99
GN,Guinea,USD,0%,Inclusive,Developer responsible,0%,45%,0.49,0.99,1.49,1.99,2.49,2.99,3.49,3.99,4.49,4.99,5.49,5.99,6.49
GW,Guinea-Bissau,EUR,0%,Inclusive,Developer responsible,0%,45%,0.49,0.99,1.49,1.99,2.49,2.99,3.49,3.99,4.49,4.99,5.49,5.99,6.49
HT,Haiti,USD,0%,Inclusive,Developer responsible,0%,45%,0.49,0.99,1.49,1.99,2.49,2.99,3.49,3.99,4.49,4.99,5.49,5.99,6.49
HN,Honduras,USD,0%,Inclusive,Developer responsible,0%,50%,0.49,0.99,1.49,1.99,2.49,2.99,3.49,3.99,4.49,4.99,5.49,5.99,6.49
HK,Hong Kong,HKD,0%,Inclusive,Developer responsible,0%,100%,7.49,15.49,23.49,30.90,38.99,46.49,54.49,62.49,69.90,77.99,85.49,93.49,101.49
HU,Hungary,HUF,27%,Inclusive,Google (Merchant of Record),0%,80%,370,740,1110,1480,1850,2220,2590,2960,3330,3700,4080,4450,4820
IS,Iceland,EUR,24%,Inclusive,Google (Merchant of Record),0%,100%,0.99,1.99,3.49,4.49,5.49,6.49,7.99,8.99,9.99,10.99,12.49,13.49,14.49
IN,India,INR,18%,Inclusive,Google collects & remits GST,0.1% TDS (with PAN) / 5% (without),45%,45,90,135,180,225,265,310,355,400,445,490,535,580
ID,Indonesia,IDR,0%,Exclusive/Inclusive,Google collects 11% VAT,0%,50%,8000,16000,24000,32000,40000,48000,56000,64000,72000,80000,88000,96000,104000
IQ,Iraq,IQD,0%,Inclusive,Developer responsible,0%,60%,800,1600,2400,3100,3900,4700,5500,6300,7100,7900,8600,9400,10200
IE,Ireland,EUR,23%,Inclusive,Google (Merchant of Record),0%,100%,0.99,1.99,3.49,4.49,5.49,6.49,7.49,8.99,9.99,10.99,11.99,13.49,14.49
IL,Israel,ILS,0%,Inclusive,Developer responsible,0%,100%,3.49,7.49,10.90,14.99,18.49,22.49,25.90,29.99,33.49,37.49,40.90,44.99,48.49
IT,Italy,EUR,22%,Inclusive,Google (Merchant of Record),0%,100%,0.99,1.99,3.49,4.49,5.49,6.49,7.49,8.99,9.99,10.99,11.99,12.99,14.49
JM,Jamaica,USD,0%,Inclusive,Developer responsible,0%,70%,0.49,1.49,1.99,2.99,3.49,3.99,4.99,5.49,6.49,6.99,7.49,8.49,8.99
JP,Japan,JPY,10%,Inclusive,Google collects & remits for foreign developers,0% with tax treaty,100%,160,330,490,660,820,990,1150,1320,1480,1650,1810,1980,2140
JO,Jordan,JOD,0%,Inclusive,Developer responsible,0%,60%,0.400,0.850,1.250,1.700,2.150,2.550,3.000,3.400,3.850,4.250,4.700,5.100,5.550
KZ,Kazakhstan,KZT,16%,Inclusive,Google collects & remits,0%,70%,390,780,1170,1560,1940,2330,2720,3110,3500,3890,4280,4670,5060
KE,Kenya,KES,16%,Inclusive,Google collects & remits,5% Digital Service WHT,50%,75,150,225,300,375,450,525,600,680,755,830,905,980
KW,Kuwait,USD,0%,Inclusive,Developer responsible,Up to 5% on DCB,100%,0.99,1.99,2.99,3.99,4.99,5.99,6.99,7.99,8.99,9.99,10.99,11.99,12.99
KG,Kyrgyzstan,USD,0%,Inclusive,Developer responsible,0%,50%,0.49,0.99,1.49,1.99,2.49,2.99,3.49,3.99,4.49,4.99,5.49,5.99,6.49
LA,Laos,USD,0%,Exclusive/Inclusive,Google collects 10% VAT,0%,45%,0.49,0.99,1.49,1.99,2.49,2.99,3.49,3.99,4.49,4.99,5.49,5.99,6.49
LV,Latvia,EUR,21%,Inclusive,Google (Merchant of Record),0%,85%,0.99,1.99,2.99,3.49,4.49,5.49,6.49,7.49,8.49,8.99,9.99,10.99,11.99
LB,Lebanon,USD,0%,Inclusive,Developer responsible,0%,60%,0.49,0.99,1.99,2.49,2.99,3.49,3.99,4.99,5.49,5.99,6.49,6.99,7.99
LR,Liberia,USD,0%,Inclusive,Developer responsible,0%,45%,0.49,0.99,1.49,1.99,2.49,2.99,3.49,3.99,4.49,4.99,5.49,5.99,6.49
LY,Libya,USD,0%,Inclusive,Developer responsible,0%,60%,0.49,0.99,1.99,2.49,2.99,3.49,3.99,4.99,5.49,5.99,6.49,6.99,7.99
LI,Liechtenstein,CHF,8.10%,Inclusive,Google collects & remits,0%,100%,0.99,1.99,2.99,3.99,4.99,5.99,6.99,7.99,8.49,9.49,10.49,11.49,12.49
LT,Lithuania,EUR,21%,Inclusive,Google (Merchant of Record),0%,85%,0.99,1.99,2.99,3.49,4.49,5.49,6.49,7.49,8.49,8.99,9.99,10.99,11.99
LU,Luxembourg,EUR,17%,Inclusive,Google (Merchant of Record),0%,100%,0.99,1.99,2.99,3.99,5.49,6.49,7.49,8.49,9.49,10.49,11.49,12.49,13.49
MO,Macao,MOP,0%,Inclusive,Developer responsible,0%,100%,7.99,15.90,23.90,31.90,39.90,47.90,56.49,64.49,72.49,80.49,88.49,96.49,104.49
MY,Malaysia,MYR,8%,Inclusive,Google collects & remits,0%,70%,3.49,6.99,9.90,13.49,16.99,20.49,23.99,26.90,30.49,33.99,37.49,40.99,43.90
MV,Maldives,USD,0%,Inclusive,Developer responsible,0%,70%,0.49,1.49,1.99,2.99,3.49,3.99,4.99,5.49,6.49,6.99,7.49,8.49,8.99
ML,Mali,EUR,0%,Inclusive,Developer responsible,0%,45%,0.49,0.99,1.49,1.99,2.49,2.99,3.49,3.99,4.49,4.99,5.49,5.99,6.49
MT,Malta,EUR,18%,Inclusive,Google (Merchant of Record),0%,85%,0.99,1.99,2.49,3.49,4.49,5.49,6.49,6.99,7.99,8.99,9.99,10.99,11.49
MU,Mauritius,USD,0%,Inclusive,Developer responsible,0%,70%,0.49,1.49,1.99,2.99,3.49,3.99,4.99,5.49,6.49,6.99,7.49,8.49,8.99
MX,Mexico,MXN,16%,Inclusive,Google collects & remits for foreign developers,Up to 20% ISR if local individual,70%,14,29,44,58,73,88,102,117,131,146,161,175,190
FM,Micronesia,USD,0%,Inclusive,Developer responsible,0%,60%,0.49,0.99,1.99,2.49,2.99,3.49,3.99,4.99,5.49,5.99,6.49,6.99,7.99
MD,Moldova,USD,20%,Inclusive,Google collects & remits for foreign developers,0%,70%,0.99,1.49,2.49,3.49,3.99,4.99,5.99,6.49,7.49,8.49,8.99,9.99,10.99
MC,Monaco,EUR,20%,Inclusive,Developer responsible,0%,100%,0.99,1.99,2.99,4.49,5.49,6.49,7.49,8.49,9.49,10.99,11.99,12.99,13.99
MN,Mongolia,MNT,0%,Inclusive,Developer responsible,0%,50%,1700,3400,5100,6800,8500,10200,11900,13600,15300,17000,18700,20400,22100
MA,Morocco,MAD,20%,Inclusive,Google collects & remits,0%,50%,5.99,11.99,17.99,23.99,29.99,35.99,41.99,47.99,53.99,59.99,65.99,71.99,77.99
MZ,Mozambique,USD,0%,Inclusive,Developer responsible,0%,45%,0.49,0.99,1.49,1.99,2.49,2.99,3.49,3.99,4.49,4.99,5.49,5.99,6.49
MM,Myanmar (Burma),MMK,0%,Inclusive,Developer responsible,Up to 2.5% on DCB,45%,900,1900,2800,3800,4700,5700,6600,7600,8500,9400,10400,11300,12300
NA,Namibia,USD,0%,Inclusive,Developer responsible,0%,60%,0.49,0.99,1.99,2.49,2.99,3.49,3.99,4.99,5.49,5.99,6.49,6.99,7.99
NP,Nepal,USD,13%,Inclusive,Google collects & remits for foreign developers,0%,45%,0.49,0.99,1.49,1.99,2.49,2.99,3.49,3.99,4.49,4.99,5.49,5.99,6.49
NL,Netherlands,EUR,21%,Inclusive,Google (Merchant of Record),0%,100%,0.99,1.99,3.49,4.49,5.49,6.49,7.49,8.49,9.99,10.99,11.99,12.99,13.99
NZ,New Zealand,NZD,15%,Inclusive,Google collects & remits,0%,100%,1.99,3.99,5.99,7.49,9.49,11.49,13.49,15.49,17.49,19.49,20.99,22.99,24.99
NI,Nicaragua,USD,0%,Inclusive,Developer responsible,0%,50%,0.49,0.99,1.49,1.99,2.49,2.99,3.49,3.99,4.49,4.99,5.49,5.99,6.49
NE,Niger,EUR,0%,Inclusive,Developer responsible,0%,45%,0.49,0.99,1.49,1.99,2.49,2.99,3.49,3.99,4.49,4.99,5.49,5.99,6.49
NG,Nigeria,NGN,7.50%,Inclusive,Google collects & remits,0%,45%,750,1500,2250,3000,3750,4500,5250,6000,6750,7500,8250,9000,9750
MK,North Macedonia,USD,18%,Inclusive,Google collects & remits,0%,70%,0.99,1.49,2.49,3.49,3.99,4.99,5.99,6.49,7.49,8.49,8.99,9.99,10.49
NO,Norway,NOK,25%,Inclusive,Google (Merchant of Record),0%,100%,13,27,40,54,67,81,94,108,121,135,148,162,175
OM,Oman,USD,5%,Inclusive,Google collects & remits for foreign developers,0%,100%,0.99,1.99,2.99,3.99,4.99,6.49,7.49,8.49,9.49,10.49,11.49,12.49,13.49
PK,Pakistan,PKR,0%,Inclusive,Developer responsible,0%,45%,120,250,370,500,620,750,870,1000,1120,1250,1370,1500,1630
PA,Panama,USD,0%,Inclusive,Developer responsible,0%,80%,0.99,1.49,2.49,2.99,3.99,4.99,5.49,6.49,6.99,7.99,8.99,9.49,10.49
PG,Papua New Guinea,USD,0%,Inclusive,Developer responsible,0%,50%,0.49,0.99,1.49,1.99,2.49,2.99,3.49,3.99,4.49,4.99,5.49,5.99,6.49
PY,Paraguay,PYG,0%,Inclusive,Developer responsible,0%,60%,4000,9000,13000,18000,22000,27000,31000,36000,40000,45000,49000,54000,58000
PE,Peru,PEN,0%,Inclusive,Developer responsible,0%,60%,1.90,4.49,6.49,8.99,10.90,13.49,15.49,17.99,19.90,22.49,24.49,26.99,28.90
PH,Philippines,PHP,12%,Inclusive,Google collects & remits for foreign developers,0%,50%,30,65,95,130,160,195,225,260,290,325,355,390,420
PL,Poland,PLN,23%,Inclusive,Google (Merchant of Record),0%,80%,3.99,7.99,11.99,15.49,19.49,23.49,27.49,31.49,35.49,39.49,43.49,46.90,50.90
PT,Portugal,EUR,23%,Inclusive,Google (Merchant of Record),0%,85%,0.99,1.99,2.99,3.99,4.49,5.49,6.49,7.49,8.49,9.49,10.49,11.49,11.99
QA,Qatar,QAR,0%,Inclusive,Developer responsible,0%,100%,3.49,6.90,10.99,14.49,17.90,21.99,25.49,28.90,32.49,36.49,39.90,43.49,47.49
RO,Romania,RON,21%,Inclusive,Google (Merchant of Record),0%,80%,4.49,8.99,13.49,17.99,21.90,26.49,30.90,35.49,39.90,44.49,48.99,53.49,57.99
RW,Rwanda,USD,0%,Inclusive,Developer responsible,0%,45%,0.49,0.99,1.49,1.99,2.49,2.99,3.49,3.99,4.49,4.99,5.49,5.99,6.49
WS,Samoa,USD,0%,Inclusive,Developer responsible,0%,60%,0.49,0.99,1.99,2.49,2.99,3.49,3.99,4.99,5.49,5.99,6.49,6.99,7.99
SM,San Marino,EUR,0%,Inclusive,Developer responsible,0%,100%,0.99,1.99,2.49,3.49,4.49,5.49,6.49,6.99,7.99,8.99,9.99,10.99,11.49
SA,Saudi Arabia,SAR,15%,Inclusive,Google collects & remits for foreign developers,0%,100%,4.49,8.49,12.99,16.90,21.49,25.99,29.90,34.49,38.99,42.90,47.49,51.49,55.90
SN,Senegal,XOF,18%,Inclusive,Google collects & remits,0%,50%,350,700,1050,1400,1750,2100,2450,2800,3150,3500,3850,4150,4500
RS,Serbia,RSD,20%,Inclusive,Google collects & remits for foreign developers,0%,70%,90,180,270,360,455,545,635,725,815,905,995,1090,1180
SC,Seychelles,USD,0%,Inclusive,Developer responsible,0%,80%,0.99,1.49,2.49,2.99,3.99,4.99,5.49,6.49,6.99,7.99,8.99,9.49,10.49
SL,Sierra Leone,USD,0%,Inclusive,Developer responsible,0%,45%,0.49,0.99,1.49,1.99,2.49,2.99,3.49,3.99,4.49,4.99,5.49,5.99,6.49
SG,Singapore,SGD,9%,Inclusive,Google collects & remits for foreign developers,0%,100%,1.49,2.99,4.49,5.99,7.49,8.99,10.49,11.99,12.99,14.49,15.99,17.49,18.99
SK,Slovakia,EUR,23%,Inclusive,Google (Merchant of Record),0%,85%,0.99,1.99,2.99,3.99,4.49,5.49,6.49,7.49,8.49,9.49,10.49,11.49,11.99
SI,Slovenia,EUR,22%,Inclusive,Google (Merchant of Record),0%,85%,0.99,1.99,2.99,3.49,4.49,5.49,6.49,7.49,8.49,9.49,10.49,10.99,11.99
SB,Solomon Islands,USD,0%,Inclusive,Developer responsible,0%,50%,0.49,0.99,1.49,1.99,2.49,2.99,3.49,3.99,4.49,4.99,5.49,5.99,6.49
SO,Somalia,USD,0%,Inclusive,Developer responsible,0%,45%,0.49,0.99,1.49,1.99,2.49,2.99,3.49,3.99,4.49,4.99,5.49,5.99,6.49
ZA,South Africa,ZAR,15%,Inclusive,Google collects & remits,0%,60%,13,25,38,51,64,76,89,102,115,128,140,153,166
KR,South Korea,KRW,10%,Inclusive,Google collects & remits for foreign developers,0%,100%,1500,3000,4500,6100,7600,9100,10600,12100,13600,15200,16700,18200,19700
ES,Spain,EUR,21%,Inclusive,Google (Merchant of Record),0%,100%,0.99,1.99,3.49,4.49,5.49,6.49,7.49,8.49,9.99,10.99,11.99,12.99,13.99
LK,Sri Lanka,LKR,0%,Inclusive,Developer responsible,Up to 10% on DCB,45%,130,270,400,540,670,810,940,1080,1210,1350,1480,1620,1750
KN,St. Kitts & Nevis,USD,0%,Inclusive,Developer responsible,0%,80%,0.99,1.49,2.49,2.99,3.99,4.99,5.49,6.49,6.99,7.99,8.99,9.49,10.49
LC,St. Lucia,USD,0%,Inclusive,Developer responsible,0%,70%,0.49,1.49,1.99,2.99,3.49,3.99,4.99,5.49,6.49,6.99,7.49,8.49,8.99
SR,Suriname,USD,0%,Inclusive,Developer responsible,0%,60%,0.49,0.99,1.99,2.49,2.99,3.49,3.99,4.99,5.49,5.99,6.49,6.99,7.99
SE,Sweden,SEK,25%,Inclusive,Google (Merchant of Record),0%,100%,13,26,40,53,66,79,93,106,119,132,146,159,172
CH,Switzerland,CHF,0%,Inclusive,Developer responsible,0%,100%,0.99,1.99,2.49,3.49,4.49,5.49,6.49,6.99,7.99,8.99,9.99,10.99,11.49
TW,Taiwan,TWD,5%,Inclusive,Google collects & remits,3% WHT (unless exempt/registered),100%,33,67,100,134,168,201,235,268,302,336,369,403,436
TJ,Tajikistan,USD,0%,Inclusive,Developer responsible,0%,50%,0.49,0.99,1.49,1.99,2.49,2.99,3.49,3.99,4.49,4.99,5.49,5.99,6.49
TZ,Tanzania,TZS,0%,Inclusive,Developer responsible,5% gross revenue WHT,45%,1200,2300,3500,4700,5800,7000,8200,9300,10500,11700,12900,14000,15200
TH,Thailand,THB,7%,Inclusive,Google collects & remits for foreign developers,0%,70%,26,52,78,105,131,157,183,209,236,262,288,314,341
TG,Togo,EUR,0%,Inclusive,Developer responsible,0%,45%,0.49,0.99,1.49,1.99,2.49,2.99,3.49,3.99,4.49,4.99,5.49,5.99,6.49
TO,Tonga,USD,0%,Inclusive,Developer responsible,0%,60%,0.49,0.99,1.99,2.49,2.99,3.49,3.99,4.99,5.49,5.99,6.49,6.99,7.99
TT,Trinidad & Tobago,USD,0%,Inclusive,Developer responsible,0%,80%,0.99,1.49,2.49,2.99,3.99,4.99,5.49,6.49,6.99,7.99,8.99,9.49,10.49
TN,Tunisia,USD,0%,Inclusive,Developer responsible,0%,50%,0.49,0.99,1.49,1.99,2.49,2.99,3.49,3.99,4.49,4.99,5.49,5.99,6.49
TM,Turkmenistan,USD,0%,Inclusive,Developer responsible,0%,60%,0.49,0.99,1.99,2.49,2.99,3.49,3.99,4.99,5.49,5.99,6.49,6.99,7.99
TC,Turks & Caicos Islands,USD,0%,Inclusive,Developer responsible,0%,100%,0.99,1.99,2.99,3.99,4.99,5.99,6.99,7.99,8.99,9.99,10.99,11.99,12.99
TR,Türkiye,TRY,20%,Inclusive,Google collects & remits,0%,50%,23,45,68,91,114,137,159,182,205,228,251,273,296
UG,Uganda,USD,18%,Inclusive,Google collects & remits,0%,45%,0.49,0.99,1.49,1.99,2.49,2.99,3.49,3.99,4.99,5.49,5.99,6.49,6.99
UA,Ukraine,UAH,20%,Inclusive,Google collects & remits,0%,50%,24,49,74,98,123,147,172,197,221,246,270,295,320
AE,United Arab Emirates,AED,5%,Inclusive,Google collects & remits for foreign developers,0%,100%,3.99,7.49,11.49,15.49,18.90,22.90,26.99,30.99,34.49,38.49,42.49,45.90,49.90
GB,United Kingdom,GBP,20%,Inclusive,Google (Merchant of Record),0%,100%,0.99,1.99,2.99,3.99,4.99,5.99,6.49,7.49,8.49,9.49,10.49,11.49,12.49
US,United States,USD,0%,Exclusive,Google collects state sales tax (0%-10.25%) at checkout,No WHT if W-8BEN submitted (0%),100%,0.99,1.99,2.99,3.99,4.99,5.99,6.99,7.99,8.99,9.99,10.99,11.99,12.99
UY,Uruguay,USD,0%,Inclusive,Developer responsible,0%,80%,0.99,1.49,2.49,2.99,3.99,4.99,5.49,6.49,6.99,7.99,8.99,9.49,10.49
UZ,Uzbekistan,UZS,12%,Inclusive,Google collects & remits for foreign developers,0%,50%,7000,14000,21000,28000,35000,42000,49000,56000,63000,70000,78000,85000,92000
VU,Vanuatu,USD,0%,Inclusive,Developer responsible,0%,60%,0.49,0.99,1.99,2.49,2.99,3.49,3.99,4.99,5.49,5.99,6.49,6.99,7.99
VA,Vatican City,EUR,0%,Inclusive,Developer responsible,0%,100%,0.99,1.99,2.49,3.49,4.49,5.49,6.49,6.99,7.99,8.99,9.99,10.99,11.49
VE,Venezuela,USD,0%,Inclusive,Developer responsible,0%,50%,0.49,0.99,1.49,1.99,2.49,2.99,3.49,3.99,4.49,4.99,5.49,5.99,6.49
VN,Vietnam,VND,0%,Inclusive,Google collects & remits (5% VAT + 5% CIT for foreign developers),5% local developer deduction,45%,11000,22000,34000,45000,56000,67000,79000,90000,101000,112000,124000,135000,146000
YE,Yemen,USD,0%,Inclusive,Developer responsible,0%,45%,0.49,0.99,1.49,1.99,2.49,2.99,3.49,3.99,4.49,4.99,5.49,5.99,6.49
ZM,Zambia,USD,0%,Exclusive/Inclusive,Google collects 16% VAT,0%,45%,0.49,0.99,1.49,1.99,2.49,2.99,3.49,3.99,4.49,4.99,5.49,5.99,6.49
ZW,Zimbabwe,USD,0%,Inclusive,Developer responsible,0%,45%,0.49,0.99,1.49,1.99,2.49,2.99,3.49,3.99,4.49,4.99,5.49,5.99,6.49";
    }
}
