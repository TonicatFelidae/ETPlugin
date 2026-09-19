using System.Collections.Generic;

namespace ET.Analytics
{
    /// <summary>
    /// A small builder for the parameter list of one event.
    ///
    /// Exists so call sites can stay free of any SDK type — the backend translates
    /// these entries into whatever its SDK wants (Firebase takes string / long /
    /// double, and nothing else). It also centralises the two GA4 limits that are
    /// silently enforced server-side: a string value is truncated at 100 characters,
    /// and empty values are dropped rather than sent as "".
    /// </summary>
    public sealed class AnalyticsParams
    {
        /// <summary>GA4 truncates string parameter values beyond this length.</summary>
        public const int MaxValueLength = 100;

        public readonly struct Entry
        {
            public readonly string Key;

            /// <summary>Always a string, long or double — see the Add overloads.</summary>
            public readonly object Value;

            public Entry(string key, object value)
            {
                Key = key;
                Value = value;
            }
        }

        private readonly List<Entry> _entries;

        public AnalyticsParams(int capacity = 8)
        {
            _entries = new List<Entry>(capacity);
        }

        public static AnalyticsParams New(int capacity = 8) => new AnalyticsParams(capacity);

        public int Count => _entries.Count;

        public IReadOnlyList<Entry> Entries => _entries;

        /// <summary>
        /// Adds a string parameter. Null / empty values are skipped: an optional
        /// parameter that does not apply to this event (e.g. ad_not_ready_reason on a
        /// successful request) must be absent, not blank — a blank shows up in GA4 as
        /// its own dimension value and splits every report.
        /// </summary>
        public AnalyticsParams Str(string key, string value)
        {
            if (string.IsNullOrEmpty(value)) return this;
            if (value.Length > MaxValueLength) value = value.Substring(0, MaxValueLength);
            _entries.Add(new Entry(key, value));
            return this;
        }

        public AnalyticsParams Int(string key, long value)
        {
            _entries.Add(new Entry(key, value));
            return this;
        }

        public AnalyticsParams Num(string key, double value)
        {
            _entries.Add(new Entry(key, value));
            return this;
        }

        /// <summary>
        /// Booleans go over as "true" / "false" strings, because GA4 custom
        /// *dimensions* are string-typed. Sending 0/1 as a number would make
        /// ad_ready a metric, which cannot be used to break a report down.
        /// </summary>
        public AnalyticsParams Bool(string key, bool value)
        {
            _entries.Add(new Entry(key, value ? "true" : "false"));
            return this;
        }

        public override string ToString()
        {
            if (_entries.Count == 0) return "{}";

            var sb = new System.Text.StringBuilder("{");
            for (int i = 0; i < _entries.Count; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append(_entries[i].Key).Append('=').Append(_entries[i].Value);
            }
            return sb.Append('}').ToString();
        }
    }
}
