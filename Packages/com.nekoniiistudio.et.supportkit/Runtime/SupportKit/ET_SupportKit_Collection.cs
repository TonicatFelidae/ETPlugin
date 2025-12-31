using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;
using ET;
using UnityEngine;

namespace ET.SupportKit.Collection
{
    public static class Empty<T>
    {
        public static readonly T[] Array = new T[0];
    }

    public static class Generate
    {
        public static readonly IEnumerable<bool> True = Value(true);

        public static readonly IEnumerable<bool> False = Value(false);

        public static IEnumerable<T> Value<T>(T value)
        {
            while (true) yield return value;
        }
    }

    public static class ET_Collection
    {
        public static int GetHashCodeForElements<T>(this IList<T> self)
        {
            unchecked
            {
                int hashCode = 0;
                if (!self.IsNullOrEmpty())
                {
                    for (int i = 0; i < self.Count; i++)
                    {
                        T selfElement = self[i];
                        if (!EqualityComparer<T>.Default.Equals(selfElement, default(T)))
                        {
                            hashCode = (hashCode * 397) ^ selfElement.GetHashCode();
                        }
                    }
                }
                return hashCode;
            }
        }

        public static bool ArrayEquals<T>(this T[] self, T[] other) where T : IEquatable<T>
        {
            if (self == null)
            {
                return other == null;
            }

            if (other == null) return false;
            if (self.Length != other.Length) return false;
            for (int i = 0; i < self.Length; i++)
            {
                T selfElement = self[i];
                T otherElement = other[i];
                if (selfElement == null)
                {
                    if (otherElement != null) return false;
                }
                else
                {
                    if (otherElement == null) return false;
                    if (!selfElement.Equals(otherElement)) return false;
                }
            }

            return true;
        }

        public static T[] ToUnitArray<T>(this T item)
        {
            return new T[] { item };
        }

        public static List<T> ToUnitList<T>(this T item)
        {
            return new List<T> { item };
        }

        public static T[] Append<T>(this T[] array, T item)
        {
            if (array == null)
            {
                return new T[] { item };
            }
            T[] result = new T[array.Length + 1];
            array.CopyTo(result, 0);
            result[array.Length] = item;
            return result;
        }

        public static T[] Append<T>(this T[] array, T[] items)
        {
            T[] result;
            if (array == null)
            {
                result = new T[items.Length];
                items.CopyTo(result, 0);
            }
            else
            {
                result = new T[array.Length + items.Length];
                array.CopyTo(result, 0);
                items.CopyTo(result, array.Length);
            }
            return result;
        }

        /// <summary>
        /// Copy elements from <paramref name="source"/> into <paramref name="destination"/>, allocating a new array for <paramref name="destination"/> if it is <c>null</c> or isn't large enough.
        /// </summary>
        /// <typeparam name="T">element type</typeparam>
        /// <typeparam name="U">source element type (which must be assignable to <typeparamref name="T"/></typeparam>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="sourceIndex">An offset into <paramref name="source"/> at which to start copying elements</param>
        /// <param name="destinationIndex">An offset into <paramref name="destination"/> at which to start writing elements</param>
        /// <param name="length">The number of elements to copy. If not specified, all elements from <paramref name="sourceIndex"/> through the end of <paramref name="source"/> will be copied.</param>
        /// <param name="eraseRemainder">If true, any elements of <paramref name="destination"/> beyond those replaced by the copy operation will be filled with <c>default(T)</c>.</param>
        public static void Copy<T, U>(IList<U> source, ref T[] destination, int sourceIndex = 0, int destinationIndex = 0, int? length = null, bool eraseRemainder = false)
            where U : T
        {
            int copyLength = length.GetValueOrDefault(source.Count - sourceIndex);
            if (destination == null || destination.Length < destinationIndex + copyLength)
            {
                T[] newSelf = new T[destinationIndex + copyLength];
                if (destination != null)
                {
                    Array.Copy(destination, 0, newSelf, 0, destinationIndex);
                }
                destination = newSelf;
            }
            // ReSharper disable once SuspiciousTypeConversion.Global -- at runtime Array instances do implement IList<ElementType>.
            Array sourceArray = source as Array;
            if (sourceArray != null)
            {
                Array.Copy(sourceArray, sourceIndex, destination, destinationIndex, copyLength);
            }
            else
            {
                for (int i = 0; i < copyLength; ++i)
                {
                    destination[destinationIndex + i] = source[sourceIndex + i];
                }
            }
            if (eraseRemainder && destinationIndex + copyLength < destination.Length)
            {
                destination.Clear(destinationIndex + copyLength);
            }
        }

        public static void AddIfNotPresent<T>(ref T[] self, T value, T[] sharedUnitArray = null)
        {
            if (self == null || self.Length == 0)
            {
                self = sharedUnitArray ?? value.ToUnitArray();
            }
            else if (Array.IndexOf(self, value) < 0)
            {
                var length = self.Length;
                var newSelf = new T[length + 1];
                Array.Copy(self, newSelf, length);
                newSelf[length] = value;
                self = newSelf;
            }
        }

        /// <summary>
        /// Returns <paramref name="self"/> if it is already an array
        /// otherwise uses <see cref="System.Linq.Enumerable.ToArray{T}"/>() to make a new one.
        /// </summary>
        /// <param name="allowNull">if <c>true</c> and self is null, return null.</param>
        /// <param name="nullAsEmpty">if <c>true</c> and self is null, return <see cref="Empty{T}.Array"/>.</param>
        public static T[] ToArrayAvoidClone<T>(this IEnumerable<T> self, bool allowNull = true, bool nullAsEmpty = true)
        {
            if (allowNull && self == null)
            {
                return nullAsEmpty ? Empty<T>.Array : null;
            }
            return (self as T[]) ?? self.ToArray();
        }

        /// <summary>
        /// Returns <paramref name="self"/> if it is already a <see cref="List{T}"/>,
        /// otherwise uses <see cref="System.Linq.Enumerable.ToList{T}"/>() to make a new one.
        /// </summary>
        /// <param name="allowNull">if <c>true</c> and self is null, return null.</param>
        public static List<T> ToListAvoidClone<T>(this IEnumerable<T> self, bool allowNull = true)
        {
            if (allowNull && self == null)
            {
                return null;
            }
            return (self as List<T>) ?? self.ToList();
        }

        /// <summary>
        /// Fills an array with default values.
        /// </summary>
        public static void Clear<T>(this T[] @this, int startIndex = 0, int? length = null)
        {
            for (int i = startIndex, len = startIndex + length.GetValueOrDefault(@this.Length - startIndex); i < len; i++)
            {
                @this[i] = default(T);
            }
        }

        /// <summary>
        /// Fills an array.
        /// </summary>
        public static void Fill<T>(this T[] @this, T value, int startIndex = 0, int? length = null)
        {
            for (int i = startIndex, len = startIndex + length.GetValueOrDefault(@this.Length - startIndex); i < len; i++)
            {
                @this[i] = value;
            }
        }
        //Find Class

        public static T Find<T>(this IList<T> self, Func<T, bool> predicate)
        {
            for (int i = 0; i < self.Count; i++)
            {
                if (predicate(self[i]))
                {
                    return self[i];
                }
            }

            return default(T);
        }

        public static T Find<T>(this ICollection<T> self, Func<T, bool> predicate)
        {
            using (var enumerator = self.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    T current = enumerator.Current;
                    if (predicate(current))
                    {
                        return current;
                    }
                }
            }

            return default(T);
        }

        public static bool ContainsValue<T>(this ICollection<T> self, Func<T, bool> predicate)
        {
            using (var enumerator = self.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    T current = enumerator.Current;
                    if (predicate(current))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        public static int FindDownActive(this Component[] self, int startIndex, bool loopable = false)
        {
            for (int i = 0; i < self.Length; i++)
            {
                int value = startIndex + i + 1;
                if (loopable)
                {
                    value = ETMath.ClampInLoop(value,0, self.Length);
                }
                else
                {
                    value = Mathf.Clamp(value, 0, self.Length - 1);
                }
                if (self[value].gameObject.activeSelf == true )
                {
                    return value;
                }
            }
            return startIndex;
        }
        public static int FindUpActive(this Component[] self, int startIndex, bool loopable = false)
        {
            for (int i = 0; i < self.Length; i++)
            {
                int value = startIndex - i - 1;
                if (loopable)
                {
                    value = ETMath.ClampInLoop(value, 0, self.Length);
                }
                else
                {
                    value = Mathf.Clamp(value, 0, self.Length - 1);
                }
                if (self[value].gameObject.activeSelf == true)
                {
                    return value;
                }
            }
            return startIndex;
        }
        public static List<T> FindAll<T>(this IList<T> self, Func<T, bool> predicate)
        {
            var result = new List<T>();
            for (int i = 0; i < self.Count; i++)
            {
                if (predicate(self[i]))
                {
                    result.Add(self[i]);
                }
            }

            return result;
        }

        public static List<T> FindAll<T>(this ICollection<T> self, Func<T, bool> predicate)
        {
            var result = new List<T>();
            using (var enumerator = self.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    T current = enumerator.Current;
                    if (predicate(current))
                    {
                        result.Add(current);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Fills an array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="value"></param>
        public static void Fill<T>(this T[,] @this, T value)
        {
            for (int i = 0, len0 = @this.GetLength(0); i < len0; i++)
            {
                for (int j = 0, len1 = @this.GetLength(1); j < len1; j++)
                {
                    @this[i, j] = value;
                }
            }
        }

        /// <summary>
        /// Extension wrapper for Array.IndexOf(), required for .NET 3.5
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int IndexOf<T>(this T[] @this, T value)
        {
            return Array.IndexOf(@this, value);
        }

        /// <summary>
        /// Returns true if two collections represent the same set of objects,
        /// even if an item appears multiple times and regardless of order.
        /// </summary>
        public static bool SameSet<Q>(this ICollection<Q> a, ICollection<Q> b)
        {
            if (a.Count != b.Count)
                return false;
            if (!a.All(b.Contains))
                return false;
            return true;
        }

        /// <summary>
        /// Returns true if two collections represent the same set of objects,
        /// even if an item appears multiple times and regardless of order.
        /// </summary>
        public static bool SameSet<Q>(this IEnumerable<Q> ia, IEnumerable<Q> ib)
        {
            return SameSet(ia.ToCollection(), ib.ToCollection());
        }

        /// <summary>
        /// Returns true if two collections represent the same list of objects
        /// as determined by their typesafe equality operator.
        ///
        /// Nulls are considered equal.
        /// </summary>
        public static bool SameList<T>(this ICollection<T> a, ICollection<T> b, bool allowNull = false, bool nullAsEmpty = false)
            where T : IEquatable<T>
        {
            if (allowNull || nullAsEmpty)
            {
                var aIsNull = a == null;
                var bIsNull = b == null;
                if (allowNull)
                {
                    if (aIsNull && bIsNull)
                        return true;
                    if (aIsNull != bIsNull && !nullAsEmpty)
                        return false;
                }
                if (nullAsEmpty)
                {
                    if (aIsNull)
                        a = Empty<T>.Array;
                    if (bIsNull)
                        b = Empty<T>.Array;
                }
            }
            if (a.Count != b.Count)
                return false;
            return a.SequenceEqual(b);
        }

        /// <summary>
        /// Return true if this list contains any element contained in the provided list.
        /// Returns false if either list is empty or null or if this list does not contain anything contained in the provided list.
        /// O(m*n) operation.
        /// </summary>
        public static bool ContainsAny<T>(this IList<T> self, IList<T> other) where T : IEquatable<T>
        {
            if (self.IsNullOrEmpty()
                || other.IsNullOrEmpty())
            {
                return false;
            }

            int selfCount = self.Count;
            int otherCount = other.Count;
            for (int otherIndex = 0; otherIndex < otherCount; ++otherIndex)
            {
                for (int selfIndex = 0; selfIndex < selfCount; ++selfIndex)
                {
                    if (Equals(self[selfIndex], other[otherIndex]))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Returns true if a collection is null or empty.  Useful for contracts where the serialization methods treat empty arrays differently.
        /// </summary>
        /// <remarks>
        /// Note that this is *not* implemented for <c>IEnumerable</c> to avoid the likelyhood of unexpected problems with methods
        /// returning <c>IEnumerable</c>s via <c>yield</c> that aren't designed to be iterated multiple times.  This problem already
        /// exists with Linq's <c>Count()</c> and <c>Any()</c> but we decided not to exacerbate it. [jpollak/bgiftge]
        /// </remarks>
        public static bool IsNullOrEmpty<T>(this ICollection<T> self)
        {
            if (self == null)
            {
                return true;
            }
            return self.Count == 0;
        }

        /// <summary>
        /// If <paramref name="self"/> is <c>null</c> return an empty array, otherwise return self.
        /// </summary>
        public static IEnumerable<T> NullAsEmpty<T>(this IEnumerable<T> self)
        {
            if (self == null)
            {
                return Empty<T>.Array;
            }
            return self;
        }

        /// <summary>
        /// If <paramref name="self"/> is <c>null</c> return an empty array, otherwise return self.
        /// </summary>
        public static ICollection<T> NullAsEmpty<T>(this ICollection<T> self)
        {
            if (self == null)
            {
                return Empty<T>.Array;
            }
            return self;
        }

        /// <summary>
        /// Returns true if the number of elements in a collection is a particular value.
        /// </summary>
        public static bool CountIs<T>(this IEnumerable<T> self, int count)
        {
            foreach (T e in self)
            {
                --count;
                if (count < 0)
                {
                    return false;
                }
            }
            return count == 0;
        }

        [ThreadStatic]
        private static System.Random _threadsafeRandom;

        public static System.Random ThreadsafeRandom
        {
            get
            {
                if (_threadsafeRandom == null)
                {
                    _threadsafeRandom = new System.Random(Guid.NewGuid().GetHashCode());
                }
                return _threadsafeRandom;
            }
        }

        /// <summary>
        /// Returns a random element of a collection.
        /// </summary>
        /// <exception cref="System.ArgumentOutOfRangeException">if the collection is empty.</exception>
        public static T RandomElement<T>(this IEnumerable<T> self)
        {
            int i = ThreadsafeRandom.Next(self.Count());
            return self.ElementAt(i);
        }

        /// <summary>
        /// Returns a random element of a collection or <c>default(T)</c> if the collection is empty.
        /// </summary>
        public static T RandomElementOrDefault<T>(this IEnumerable<T> self)
        {
            int count = self.Count();
            if (count == 0)
                return default(T);
            int i = ThreadsafeRandom.Next(count);
            return self.ElementAt(i);
        }

        /// <summary>
        /// Shuffle the elements of <paramref name="self"/> in-place.
        /// </summary>
        public static void Shuffle<T>(this T[] self)
        {
            // Fisher–Yates/Durstenfeld/Knuth
            var random = ThreadsafeRandom;
            for (int i = self.Length - 1; i >= 1; --i)
            {
                var j = random.Next(i + 1);
                var a = self[i];
                var b = self[j];
                self[j] = a;
                self[i] = b;
            }
        }

        /// <summary>
        /// Shuffle the elements of <paramref name="self"/> in-place.
        /// </summary>
        public static void Shuffle<T>(this IList<T> self)
        {
            // Fisher–Yates/Durstenfeld/Knuth
            var random = ThreadsafeRandom;
            for (int i = self.Count - 1; i >= 1; --i)
            {
                var j = random.Next(i + 1);
                var a = self[i];
                var b = self[j];
                self[j] = a;
                self[i] = b;
            }
        }

        /// <summary>
        /// Shuffle the elements of <paramref name="self"/> into a new array.
        /// </summary>
        public static IEnumerable<T> Shuffled<T>(this IEnumerable<T> self)
        {
            System.Random random = ThreadsafeRandom;
            return self.OrderBy(_ => random.Next());
        }

        /// <summary>
        /// Replace the first occurance of <see cref="existing"/> with <see cref="replacement"/>.
        /// </summary>
        /// <returns>true if an item was replaced, false otherwise.</returns>
        public static bool ReplaceFirst<T>(this IList<T> self, T existing, T replacement)
        {
            var i = self.IndexOf(existing);
            if (i < 0) return false;
            self[i] = replacement;
            return true;
        }

        /// <summary>
        /// Returns the minimum element of a collection or <c>default(T)</c> (usually zero) if the collection is empty.
        /// </summary>
        public static T MinOrDefault<T>(this ICollection<T> self)
        {
            if (self.Count == 0)
                return default(T);
            return self.Min();
        }

        /// <summary>
        /// Returns the maximum element of a collection or <c>default(T)</c> (usually zero) if the collection is empty.
        /// </summary>
        public static T MaxOrDefault<T>(this ICollection<T> self)
        {
            if (self.Count == 0)
                return default(T);
            return self.Max();
        }

        /// <summary>
        /// Returns the mean value of a collection or <c>default(T)</c> (usually zero) if the collection is empty.
        /// </summary>
        public static float AverageOrDefault(this ICollection<int> self)
        {
            if (self.Count == 0)
                return default(int);
            return (float)self.Average();
        }

        /// <summary>
        /// Yields all elements of a collection that are greater than a particular value.
        /// </summary>
        public static IEnumerable<T> GreaterThan<T>(this IEnumerable<T> self, T item)
            where T : IComparable<T>
        {
            foreach (var i in self)
                if (item.CompareTo(i) < 0)
                    yield return i;
        }

        /// <summary>
        /// Count the number of appearances in an enumeration of each distinct
        /// value of <typeparamref name="T"/> that appears at least once.
        /// </summary>
        public static Dictionary<T, int> Tally<T>(this IEnumerable<T> self)
            where T : IEquatable<T>
        {
            var result = new Dictionary<T, int>();
            foreach (var i in self)
            {
                result[i] = result.GetDefault(i) + 1;
            }
            return result;
        }

        /// <summary>
        /// Generate the difference in the values between two dictionaries based
        /// on keys that appear in both.  A value of zero in the other dictionary
        /// is assumed for any key that appears in only one.
        /// </summary>
        public static Dictionary<T, int> Delta<T>(this IDictionary<T, int> self, IDictionary<T, int> other)
            where T : IComparable<T>
        {
            var result = new Dictionary<T, int>();
            foreach (var i in self.Keys.Union(other.Keys))
            {
                result.Add(i, self.GetDefault(i).CompareTo(other.GetDefault(i)));
            }
            return result;
        }

        /// <summary>
        /// Replace the contents of dest with the elements of this collection.
        /// This method allocates no memory unless the destination collection
        /// is too small.
        /// </summary>
        public static void CopyTo<T>(this IList<T> src, IList<T> dest, int count = -1)
        {
            if (count < 0)
            {
                count = src.Count;
            }
            for (int i = 0; i < count; ++i)
            {
                var value = src[i];
                if (i < dest.Count)
                {
                    dest[i] = value;
                }
                else
                {
                    dest.Add(value);
                }
            }
            for (int j = dest.Count - 1; j >= count; --j)
            {
                dest.RemoveAt(j);
            }
        }
        /// <summary>
        /// Returns a new dictionary containing all elements of a list of key/value pairs.
        /// </summary>
        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> self)
        {
            var result = new Dictionary<TKey, TValue>();
            foreach (var e in self)
            {
                result.Add(e.Key, e.Value);
            }
            return result;
        }
        /// <summary>
        /// ET Returns a new dictionary containing all elements of a list of key/value pairs. 
        /// </summary>
        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<TValue> self, Func<TValue, TKey> keyFunc)
        {
            var result = new Dictionary<TKey, TValue>();
            foreach (var e in self)
            {
                result.Add(keyFunc(e), e);
            }
            return result;
        }
        /// <summary>
        /// ET Returns a new dictionary containing all elements of a list of key/value pairs. With both key and value require function to acsess
        /// </summary>
        public static Dictionary<TKey, TValue> ToDictionary<TOrigin, TKey, TValue>
            (this IEnumerable<TOrigin> self, Func<TOrigin, TKey> keyFunc, Func<TOrigin, TValue> valFunc)
        {
            var result = new Dictionary<TKey, TValue>();
            foreach (var e in self)
            {
                result.Add(keyFunc(e), valFunc(e));
            }
            return result;
        }
        public static Dictionary<TKey, TValueNew> ToDictionary_Dynamic<TKey, TValueNew>(this IEnumerable<KeyValuePair<TKey, object>> self)
        {
            Func<object, TValueNew> valueFunc = x => (TValueNew)x;
            var result = new Dictionary<TKey, TValueNew>();
            foreach (var e in self)
            {
                result.Add(e.Key, valueFunc(e.Value));
            }
            return result;
        }

        public static Dictionary<TKey, TValueNew> ToDictionary_Dynamic<TValue, TKey, TValueNew>(this IEnumerable<KeyValuePair<TKey, TValue>> self, Func<TValue, TValueNew> keyFunc)
        {
            var result = new Dictionary<TKey, TValueNew>();
            foreach (var e in self)
            {
                result.Add(e.Key, keyFunc(e.Value));
            }
            return result;
        }
        public static Dictionary<TKeyNew, TValueNew> ToDictionary_Dynamic<TValue, TKey, TKeyNew, TValueNew>(this IEnumerable<KeyValuePair<TKey, TValue>> self, Func<TKey, TKeyNew> keyFunc, Func<TValue, TValueNew> valueFunc)
        {
            var result = new Dictionary<TKeyNew, TValueNew>();
            foreach (var e in self)
            {
                result.Add(keyFunc(e.Key), valueFunc(e.Value));
            }
            return result;
        }
        /// <summary>
        /// Returns a new dictionary containing all elements of a list of key/value pairs.
        /// </summary>
        public static Dictionary<TKey, List<TValue>> ToDictionaryOfLists<TKey, TValue>(this IEnumerable<TValue> self, Func<TValue, TKey> keyFunc)
        {
            var result = new Dictionary<TKey, List<TValue>>();
            foreach (var e in self)
            {
                result.GetDefault(keyFunc(e)).Add(e); 
            }
            return result;
        }


        /// <summary>
        /// Returns a new dictionary containing all elements of a list of key/value pairs.
        /// </summary>
        public static Dictionary<TKey, TValue> ToDictionaryFromList<TKey, TValue>(this IEnumerable<TValue> self, Func<TValue, TKey> keyFunc)
        {
            var result = new Dictionary<TKey, TValue>();
            foreach (var e in self)
            {
                result.Add(keyFunc(e), e);
            }
            return result;
        }
        /// <summary>
        /// Yields the result of invoking <paramref name="zip"/>() on each horizontal slice of the collections.
        /// Throws exceptions if all the collections don't have the same number of elements.
        /// </summary>
        /// <remarks>Provided for source compatibility with .NET 4.0</remarks>
        public static IEnumerable<TZip> Zip<TA, TB, TZip>(this IEnumerable<TA> ea, IEnumerable<TB> eb, Func<TA, TB, TZip> zip)
        {
            var a = ea.GetEnumerator();
            var b = eb.GetEnumerator();
            while (a.MoveNext())
            {
                b.MoveNext();
                yield return zip(a.Current, b.Current);
            }
        }

        /// <summary>
        /// Yields the result of invoking <paramref name="zip"/>() on each horizontal slice of the collections.
        /// Throws exceptions if all the collections don't have the same number of elements.
        /// </summary>
        /// <remarks>Provided for source compatibility with .NET 4.0</remarks>
        public static IEnumerable<TZip> Zip<TA, TB, TC, TZip>(this IEnumerable<TA> ea, IEnumerable<TB> eb, IEnumerable<TC> ec, Func<TA, TB, TC, TZip> zip)
        {
            var a = ea.GetEnumerator();
            var b = eb.GetEnumerator();
            var c = ec.GetEnumerator();
            while (a.MoveNext())
            {
                b.MoveNext();
                c.MoveNext();
                yield return zip(a.Current, b.Current, c.Current);
            }
        }

        /// <summary>
        /// Invokes <paramref name="action"/>() on each element of a collection.
        /// </summary>
        public static void ForEach<T>(this IEnumerable<T> self, Action<T> action)
        {
            foreach (var i in self)
                action(i);
        }

        /// <summary>
        /// Invokes <paramref name="action"/>() on each element of a collection, yielding any exceptions that are thrown.
        /// </summary>
        public static IEnumerable<Exception> TryForEach<T>(this IEnumerable<T> self, Action<T> action)
        {
            foreach (var i in self)
            {
                Exception toYield = null;
                try
                {
                    action(i);
                }
                catch (Exception e)
                {
                    toYield = e;
                }
                yield return toYield;
            }
        }

        /// <summary>
        ///     Invokes <paramref name="action" />() on each element of a collection, ignoring the return value.
        /// </summary>
        /// <remarks>
        ///     The name ends in "Func" because the compiler can't resolve between this and the other ForEach without explicitly
        ///     providing type arguments, which is lame.
        /// </remarks>
        public static void ForEachFunc<T, TIgnored>(this IEnumerable<T> self, Func<T, TIgnored> action)
        {
            foreach (var i in self)
                action(i);
        }

        // VS won't compile this with IEnumerable<IEnumerable<T>>. Sad, huh?
        /// <summary>
        /// Enumerate the elements in each list one after another.
        /// </summary>
        public static IEnumerable<T> Concat<T>(this IEnumerable<List<T>> self)
        {
            return self.SelectMany(e => e);
        }

        /// <summary>
        /// Yields all elements of a collection for which a predicate returns false.
        /// </summary>
        public static IEnumerable<T> WhereNot<T>(this IEnumerable<T> self, Func<T, bool> predicate)
        {
            return self.Where(t => !predicate(t));
        }

        /// <summary>
        /// Gets an entry in a dictionary or adds a mapping for a key to a new instance.
        /// </summary>
        public static TValue GetDefault<TKey, TValue>(this IDictionary<TKey, TValue> self, TKey key)
            where TValue : new()
        {
            TValue result;
            if (!self.TryGetValue(key, out result))
            {
                self[key] = result = new TValue();
            }
            return result;
        }

        /// <summary>
        /// Gets an entry in a dictionary or adds a mapping for a key to the given default value.
        /// </summary>
        public static TValue GetDefault<TKey, TValue>(this IDictionary<TKey, TValue> self, TKey key, TValue defaultValue)
        {
            TValue result;
            if (!self.TryGetValue(key, out result))
            {
                self[key] = result = defaultValue;
            }
            return result;
        }

        /// <summary>
        /// Returns an ICollection with the same contents as self, avoiding constructing a new object if self is already an ICollection.
        /// </summary>
        public static ICollection<T> ToCollection<T>(this IEnumerable<T> self)
        {
            var result = self as ICollection<T>;
            if (result != null)
            {
                return result;
            }
            return self.ToArray();
        }

        /// <summary>
        /// Returns a HashSet constructed from the elements of a collection.
        /// </summary>
        /// <param name="self">the source collection</param>
        /// <param name="allowNull">if <c>true</c>, returns <c>null</c> if <paramref name="self"/> is <c>null</c>.</param>
        /// <param name="avoidClone">if <c>true</c> and <paramref name="self"/> is already a <see cref="HashSet{T}"/>, returns <paramref name="self"/> instead of constructing a new collection/</param>
        /// <param name="nullAsEmpty">if <c>true</c>, returns a new empty <see cref="HashSet{T}"/> if <paramref name="self"/> is <c>null</c>.</param>
        /// <exception cref="ArgumentException">if <paramref name="allowNull"/> and <paramref name="nullAsEmpty"/> are both <c>true</c>.</exception>
        /// <remarks>Provided as a complement to <c>ToList()</c>.</remarks>
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> self, bool allowNull = false, bool avoidClone = false, bool nullAsEmpty = false)
        {
            if (allowNull && nullAsEmpty)
            {
                throw new ArgumentException("allowNull and nullAsEmpty cannot both be true.");
            }
            if (self == null)
            {
                if (allowNull)
                {
                    return null;
                }
                if (nullAsEmpty)
                {
                    return new HashSet<T>();
                }
            }
            if (avoidClone)
            {
                var result = self as HashSet<T>;
                if (result != null)
                {
                    return result;
                }
            }
            return new HashSet<T>(self);
        }

        /// <summary>
        /// A "fluent" version of List.Sort().
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static List<T> FluentSort<T>(this List<T> self)
        {
            self.Sort();
            return self;
        }

        /// <summary>
        /// Returns only one element for each distinct value produced by <paramref name="getId"/>.
        /// </summary>
        /// <remarks>Provided because Linq's <code>Distinct()</code> requires an IEqualityComparer implementation.</remarks>
        /// <typeparam name="TId">The <code>GetHashcode()</code> and <code>Equals()</code> of this type will be used to determine which items should count as "distinct."</typeparam>
        public static IEnumerable<TItem> Distinct<TItem, TId>(this IEnumerable<TItem> self, Func<TItem, TId> getId)
        {
            var marks = new HashSet<TId>();
            foreach (var i in self)
            {
                var k = getId(i);
                if (marks.Add(k))
                {
                    yield return i;
                }
            }
        }

        /// <summary>
        /// Skips duplicate elements like Unix "uniq".
        /// </summary>
        public static IEnumerable<T> Uniq<T>(this IEnumerable<T> self)
            where T : IEquatable<T>
        {
            var enumerator = self.GetEnumerator();
            enumerator.Reset();
            if (!enumerator.MoveNext())
            {
                yield break;
            }
            var last = enumerator.Current;
            yield return last;
            while (enumerator.MoveNext())
            {
                var next = enumerator.Current;
                if (!last.Equals(next))
                {
                    yield return next;
                    last = next;
                }
            }
        }

        /// <summary>
        /// Skip elements until <paramref name="breakAt"/> is encountered. Yield all
        /// elements after (not including) <paramref name="breakAt"/>.
        /// </summary>
        public static IEnumerable<T> SkipThrough<T>(this IEnumerable<T> self, T breakAt)
            where T : IEquatable<T>
        {
            bool broken = false;
            foreach (var i in self)
            {
                if (broken)
                {
                    yield return i;
                }
                else
                {
                    if (breakAt == null)
                    {
                        if (i == null)
                        {
                            broken = true;
                        }
                    }
                    else
                    {
                        if (breakAt.Equals(i))
                        {
                            broken = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Yield every <paramref name="interval"/>-th element, starting at element <paramref name="offset"/>.
        /// This is the same as the Python slice <code>self[offset:interval:]</code>
        /// </summary>
        public static IEnumerable<T> EveryNth<T>(this IEnumerable<T> self, int interval, int offset = 0)
        {
            var n = offset;
            var e = self.GetEnumerator();
            while (true)
            {
                for (int i = 0; i <= n; ++i)
                {
                    if (!e.MoveNext())
                    {
                        yield break;
                    }
                }
                yield return e.Current;
                n = interval - 1;
            }
        }
        
        /// <summary>
        /// If <paramref name="item"/> is already contained by <paramref name="self"/>,
        /// return false, otherwise add <paramref name="item"/> to <paramref name="self"/>
        /// and return true. This provides a mirror operation to <c>Remove()</c>.
        /// </summary>
        public static bool TryAdd<T>(this ICollection<T> self, T item)
        {
            if (self.Contains(item))
                return false;
            self.Add(item);
            return true;
        }

        /// <summary>
        /// If Dictionary null, create new
        /// If <paramref name="key"/> is already contained by <paramref name="self"/>,
        /// return false, otherwise add <paramref name="value"/> to <paramref name="self"/>
        /// and return true. This provides a mirror operation to <c>Remove()</c>.
        /// </summary>
        public static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> self, TKey key, TValue value)
        {
            if (self.ContainsKey(key))
                return false;
            self.Add(key, value);
            return true;
        }
        
        /// <summary>
        /// A fluent version of <see cref="IList{T}.RemoveAt"/>.  Removes the
        /// element at <paramref name="index"/> and returns that element.
        /// </summary>
        public static T RemoveAtFluent<T>(this IList<T> self, int index)
        {
            T item = self[index];
            self.RemoveAt(index);
            return item;
        }

        /// <summary>
        /// Removes and returns the last element of <paramref name="self"/>, or,
        /// if <paramref name="self"/> is empty, returns the default value for
        /// <typeparamref name="T"/>.
        /// </summary>
        public static T RemoveLastOrDefault<T>(this IList<T> self)
        {
            var count = self.Count;
            if (count > 0)
            {
                return self.RemoveAtFluent(count - 1);
            }
            return default(T);
        }

        /// <summary>
        /// Removes and returns the last element of <paramref name="self"/>, or,
        /// if <paramref name="self"/> is empty, returns a new instance of
        /// <typeparamref name="T"/>.
        /// </summary>
        public static T RemoveLastOrNew<T>(this IList<T> self)
            where T : new()
        {
            var count = self.Count;
            if (count > 0)
            {
                return self.RemoveAtFluent(count - 1);
            }
            return new T();
        }

        /// <summary>
        /// Yields all elements of <c>self</c> that are not among <paramref name="exceptions"/>.
        /// </summary>
        public static IEnumerable<T> Except<T>(this IEnumerable<T> self, params T[] exceptions)
        {
            return System.Linq.Enumerable.Except(self, exceptions);
        }

        /// <summary>
        /// Yields all elements of <c>self</c> plus any among <paramref name="additions"/> that are not in <c>self</c>.
        /// </summary>
        public static IEnumerable<T> Union<T>(this IEnumerable<T> self, params T[] additions)
        {
            return System.Linq.Enumerable.Union(self, additions);
        }

        /// <summary>
        /// Yields all elements of <c>self</c> that are non-null.
        /// </summary>
        public static IEnumerable<T> ExceptNulls<T>(this IEnumerable<T> self)
            where T : class
        {
            Func<T, bool> predicate = i => i != null;
            return self.Where(predicate);
        }

        /// <summary>
        /// Yields all non-null elements of <c>self</c> that can be cast to <c>T</c>.
        /// </summary>
        public static IEnumerable<T> OfType<T>(this IEnumerable self)
            where T : class
        {
            foreach (var i in self)
            {
                var t = i as T;
                if (t != null)
                    yield return t;
            }
        }

        /// <summary>
        /// Yields all elements of <paramref name="self"/> where <paramref name="predicate"/>
        /// returns <c>true</c>.  If <paramref name="predicate"/> returns <c>false</c> or
        /// throws an exception, that element will be skipped.
        /// </summary>
        public static IEnumerable<T> TryWhere<T>(this IEnumerable<T> self, Func<T, bool> predicate)
        {
            foreach (var i in self)
            {
                try
                {
                    if (!predicate(i))
                        continue;
                }
                catch
                {
                    continue;
                }
                yield return i;
            }
        }

        /// <summary>
        /// Change a list of key-value pairs of one type into key-value pairs with transformed keys and values.
        /// The new keys and values may be of another type from the originals.
        /// </summary>
        public static IEnumerable<KeyValuePair<TNewKey, TNewValue>> Select<TKey, TValue, TNewKey, TNewValue>(this IEnumerable<KeyValuePair<TKey, TValue>> self, Func<TKey, TNewKey> selectKey, Func<TValue, TNewValue> selectValue)
        {
            foreach (var kvp in self)
            {
                yield return new KeyValuePair<TNewKey, TNewValue>(selectKey(kvp.Key), selectValue(kvp.Value));
            }
        }

        /// <summary>
        /// Change a list of key-value pairs of one type into key-value pairs with a transformed key.
        /// The new key may be of another type.
        /// </summary>
        public static IEnumerable<KeyValuePair<TNewKey, TValue>> SelectKey<TKey, TValue, TNewKey>(this IEnumerable<KeyValuePair<TKey, TValue>> self, Func<TKey, TNewKey> select)
        {
            foreach (var kvp in self)
            {
                yield return new KeyValuePair<TNewKey, TValue>(select(kvp.Key), kvp.Value);
            }
        }

        /// <summary>
        /// Change a list of key-value pairs of one type into key-value pairs with a transformed key.
        /// The new key may be of another type.
        /// </summary>
        public static IEnumerable<KeyValuePair<TKey, TNewValue>> SelectValue<TKey, TValue, TNewValue>(this IEnumerable<KeyValuePair<TKey, TValue>> self, Func<TValue, TNewValue> select)
        {
            foreach (var kvp in self)
            {
                yield return new KeyValuePair<TKey, TNewValue>(kvp.Key, select(kvp.Value));
            }
        }

        /// <summary>
        /// Compares <paramref name="a"/> and <paramref name="b"/> first by their keys and then by their values.
        /// </summary>
        public static int Compare<TA, TB>(KeyValuePair<TA, TB> a, KeyValuePair<TA, TB> b)
            where TA : IComparable
            where TB : IComparable
        {
            var c = a.Key.CompareTo(b.Key);
            if (c != 0)
            {
                return c;
            }
            return a.Value.CompareTo(b.Value);
        }

        /// <summary>
        /// Sorts <paramref name="self"/> in-place and returns it.
        /// </summary>
        public static T[] Sort<T>(this T[] self)
            where T : IComparable
        {
            Array.Sort(self);
            return self;
        }

        /// <summary>
        /// Sorts <paramref name="self"/> in-place using <paramref name="orderBy"/> to order elements and returns it.
        /// </summary>
        public static T[] Sort<T, TOrder>(this T[] self, Func<T, TOrder> orderBy)
            where TOrder : IComparable
        {
            Array.Sort(self, (a, b) => orderBy(a).CompareTo(orderBy(b)));
            return self;
        }

        /// <summary>
        /// Returns a sorted version of the elements in <paramref name="self"/> ordered first by the keys and then by the values.
        /// </summary>
        public static List<KeyValuePair<TA, TB>> Sorted<TA, TB>(this IEnumerable<KeyValuePair<TA, TB>> self)
            where TA : IComparable
            where TB : IComparable
        {
            var result = self.ToList();
            result.Sort(Compare);
            return result;
        }

        /// <summary>
        /// Returns a sorted version of the elements in <paramref name="self"/>.
        /// </summary>
        public static List<TValue> Sorted<TValue, TOrderBy>(this IEnumerable<TValue> self, Func<TValue, TOrderBy> orderBy = null)
            where TOrderBy : IComparable
        {
            var result = self.ToList();
            if (orderBy != null)
            {
                result.Sort((a, b) => orderBy(a).CompareTo(orderBy(b)));
            }
            else
            {
                result.Sort();
            }
            return result;
        }
        //ET custom
        /// <summary>
        /// Returns <paramref name="self"/> if it is already an array
        /// otherwise uses <see cref="System.Linq.Enumerable.ToArray{T}"/>() to make a new one.
        /// </summary>
        /// <param name="allowNull">if <c>true</c> and self is null, return null.</param>
        /// <param name="nullAsEmpty">if <c>true</c> and self is null, return <see cref="Empty{T}.Array"/>.</param>
        public static List<T> AddInit<T>(this List<T> self, T value, UnityAction action = null)
        {
            if (self == null) self = new List<T>();
            self.Add(value);
            if (action != null) action.Invoke();
            return self;
        }
        public static T[] AddInit<T>(this T[] self, T value, UnityAction action = null)
        {
            self.Append(value);
            if (action != null) action.Invoke();
            return self;
        }
        public static Dictionary<TA, TB> AddInit<TA,TB>(this Dictionary<TA, TB> self, TA key, TB value, UnityAction action = null)
        {
            if (self == null) self = new Dictionary<TA, TB>();
            self.ForceAdd(key, value);
            if (action != null) action.Invoke();
            return self;
        }
        /// <summary>
        /// ET Dictionary add value, rule: always add value, create new dictionary if dictionary null, create new key if key null, replace value if key exist
        /// </summary>
        /// <typeparam name="TA"></typeparam>
        /// <typeparam name="TB"></typeparam>
        /// <param name="self"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="action">action after add sucessful</param>
        /// <returns></returns>
        public static Dictionary<TA, TB> ForceAdd<TA, TB>(this Dictionary<TA, TB> self, TA key, TB value, UnityAction action = null)
        {
            if (self == null) self = new Dictionary<TA, TB>();
            if (self.ContainsKey(key))
            {
                self[key] = value;
            }
            else
            {
                self.Add(key, value);
            }
            action?.Invoke();
            return self;
        }
        public static Dictionary<TA, TB> Merge<TA, TB>(this Dictionary<TA, TB> self, Dictionary<TA, TB> target)
        {
            Dictionary<TA, TB> ret = target;
            foreach (var item in self)
            {
                if (ret.ContainsKey(item.Key))
                {
                    ret[item.Key] = item.Value;
                }
                else
                {
                    ret.Add(item.Key, item.Value);
                }
            }
            return ret;
        }
        /// <summary>
        /// Pick all element form target and add to self
        /// </summary>
        /// <typeparam name="TA"></typeparam>
        /// <typeparam name="TB"></typeparam>
        /// <param name="self"></param>
        /// <param name="target"></param>
        public static void Pick<TA, TB>(ref Dictionary<TA, TB> self, Dictionary<TA, TB> target)
        {
            foreach (var item in target)
            {
                if (!self.ContainsKey(item.Key))
                {
                    self.Add(item.Key, item.Value);
                }
            }
        }
        public static List<T> GetComponents<T>(this List<GameObject> listGO) where T: Component
        {
            List<T> ret = new();
            foreach(var item in listGO)
            {
                if (item.GetComponent<T>()) ret.Add(item.GetComponent<T>()); 
            }
            return ret;
        }

        /// <summary>
        /// ET Dictionary add value, rule: always add value, create new dictionary if dictionary null, create new key if key null, 
        /// if key exist destroy value instance then rea=place
        /// </summary>
        /// <typeparam name="TA"></typeparam>
        /// <typeparam name="TB"></typeparam>
        /// <param name="self"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="action">action after add sucessful</param>
        /// <returns></returns>
        public static Dictionary<TA, TB> ForceAddDestroyOld<TA, TB>(this Dictionary<TA, TB> self, TA key, TB value, UnityAction action = null) where TB : Component
        {
            if (self == null) self = new Dictionary<TA, TB>();
            if (self.ContainsKey(key))
            {
                GameObject.Destroy(self[key]);
                self[key] = value;
            }
            else
            {
                self.Add(key, value);
            }
            action?.Invoke();
            return self;
        }

    }
    public struct DictionaryItem
    {
        public object key;
        public object value;

        public DictionaryItem(object key, object value)
        {
            this.key = key;
            this.value = value;
        }
    }
}