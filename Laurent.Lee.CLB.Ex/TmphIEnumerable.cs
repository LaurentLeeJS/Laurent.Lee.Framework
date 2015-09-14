using System;
using System.Collections;
using System.Collections.Generic;

namespace Laurent.Lee.CLB
{
    public struct TmphIEnumerable<TValueType> : IEnumerable<TValueType>
    {
        private IEnumerable enumerable;

        public TmphIEnumerable(IEnumerable enumerable)
        {
            this.enumerable = enumerable;
        }

        IEnumerator<TValueType> IEnumerable<TValueType>.GetEnumerator()
        {
            if (enumerable != null)
            {
                foreach (TValueType value in enumerable) yield return value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<TValueType>)this).GetEnumerator();
        }
    }

    public static class TmphIEnumerableExpand
    {
        public static TmphIEnumerable<TValueType> toGeneric<TValueType>(this IEnumerable value)
        {
            return new TmphIEnumerable<TValueType>(value);
        }

        public static IEnumerable<TValueType> toEnumerable<TValueType>(this TValueType value)
        {
            yield return value;
        }

        public static IEnumerable<TValueType> notNull<TValueType>(this IEnumerable<TValueType> value)
        {
            return value != null ? value : TmphNullValue<TValueType>.Array;
        }

        public static int count<TValueType>(this IEnumerable<TValueType> value)
        {
            if (value != null)
            {
                int count = 0;
                for (IEnumerator<TValueType> enumerator = value.GetEnumerator(); enumerator.MoveNext(); ++count) ;
                return count;
            }
            return 0;
        }

        public static TmphList<TValueType> getList<TValueType>(this IEnumerable<TValueType> values)
        {
            TmphSubArray<TValueType> newValues = values.getSubArray();
            return newValues.Array == null ? null : new TmphList<TValueType>(newValues.Array, 0, newValues.Count, true);
        }

        public static TmphCollection<TValueType> getCollection<TValueType>(this IEnumerable<TValueType> values)
        {
            return values != null ? new TmphCollection<TValueType>(values.getSubArray(), true) : null;
        }

        public static IEnumerable<TmphArrayType> getEnumerable<TValueType, TmphArrayType>(this IEnumerable<TValueType> values, Func<TValueType, TmphArrayType> getValue)
        {
            if (values != null)
            {
                foreach (TValueType value in values) yield return getValue(value);
            }
        }

        public static TmphCollection<TmphArrayType> getCollection<TValueType, TmphArrayType>
            (this IEnumerable<TValueType> values, Func<TValueType, TmphArrayType> getValue)
        {
            return values != null ? new TmphCollection<TmphArrayType>(values.getSubArray(getValue), true) : null;
        }

        public static Dictionary<TKeyType, TValueType> getDictionaryByKey<TKeyType, TValueType>
            (this IEnumerable<TKeyType> keys, Func<TKeyType, TValueType> getValue)
            where TKeyType : IEquatable<TKeyType>
        {
            if (keys != null)
            {
                if (getValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                Dictionary<TKeyType, TValueType> dictionary = TmphDictionary<TKeyType>.Create<TValueType>();
                foreach (TKeyType key in keys) dictionary[key] = getValue(key);
                return dictionary;
            }
            return null;
        }

        public static int count<TValueType>(this IEnumerable<TValueType> values, Func<TValueType, bool> isValue)
        {
            if (isValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            if (values != null)
            {
                int count = 0;
                foreach (TValueType value in values)
                {
                    if (isValue(value)) ++count;
                }
                return count;
            }
            return 0;
        }

        public static TValueType firstOrDefault<TValueType>(this IEnumerable<TValueType> values, Func<TValueType, bool> isValue)
        {
            foreach (TValueType value in values)
            {
                if (isValue(value)) return value;
            }
            return default(TValueType);
        }

        public static bool any<TValueType>(this IEnumerable<TValueType> values, Func<TValueType, bool> isValue)
        {
            if (isValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            foreach (TValueType value in values)
            {
                if (isValue(value)) return true;
            }
            return false;
        }

        public static TmphSubArray<TValueType> getSub<TValueType>(this IEnumerable<TValueType> values, int index, int count)
        {
            if (values != null)
            {
                if (index < 0 || count < 0) TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
                if (count != 0)
                {
                    TValueType[] newValues = new TValueType[count];
                    count = 0;
                    foreach (TValueType value in values)
                    {
                        if (index != 0) --index;
                        else
                        {
                            newValues[count] = value;
                            if (++count == newValues.Length) break;
                        }
                    }
                    return TmphSubArray<TValueType>.Unsafe(newValues, 0, count);
                }
            }
            return default(TmphSubArray<TValueType>);
        }

        public static long sum<TValueType>(this IEnumerable<TValueType> values, Func<TValueType, int> getValue)
        {
            if (values != null)
            {
                long sum = 0;
                foreach (TValueType value in values) sum += getValue(value);
                return sum;
            }
            return 0;
        }

        public static bool max<TValueType>(this IEnumerable<TValueType> values, Func<TValueType, TValueType, int> comparer, out TValueType value)
        {
            value = default(TValueType);
            if (values != null)
            {
                if (comparer == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                int index = -1;
                foreach (TValueType nextValue in values)
                {
                    if (++index == 0) value = nextValue;
                    else if (comparer(nextValue, value) > 0) value = nextValue;
                }
                if (index != -1) return true;
            }
            return false;
        }

        public static bool max<TValueType, TKeyType>
            (this IEnumerable<TValueType> values, Func<TValueType, TKeyType> getKey, Func<TKeyType, TKeyType, int> comparer, out TValueType value)
        {
            value = default(TValueType);
            if (values != null)
            {
                if (getKey == null || comparer == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                int index = -1;
                TKeyType key = default(TKeyType);
                foreach (TValueType nextValue in values)
                {
                    if (++index == 0) key = getKey(value = nextValue);
                    else
                    {
                        TKeyType nextKey = getKey(nextValue);
                        if (comparer(nextKey, key) > 0)
                        {
                            value = nextValue;
                            key = nextKey;
                        }
                    }
                }
                if (index != -1) return true;
            }
            return false;
        }

        public static TValueType max<TValueType>(this IEnumerable<TValueType> values, TValueType nullValue)
            where TValueType : IComparable<TValueType>
        {
            TValueType value;
            return max(values, (left, right) => left.CompareTo(right), out value) ? value : nullValue;
        }

        public static TValueType max<TValueType, TKeyType>(this IEnumerable<TValueType> values, Func<TValueType, TKeyType> getKey, TValueType nullValue)
            where TKeyType : IComparable<TKeyType>
        {
            TValueType value;
            return max(values, getKey, (left, right) => left.CompareTo(right), out value) ? value : nullValue;
        }

        public static TValueType max<TValueType, TKeyType>
            (this IEnumerable<TValueType> values, Func<TValueType, TKeyType> getKey, Func<TKeyType, TKeyType, int> comparer, TValueType nullValue)
        {
            TValueType value;
            return max(values, getKey, comparer, out value) ? value : nullValue;
        }

        public static TKeyType maxKey<TValueType, TKeyType>(this IEnumerable<TValueType> values, Func<TValueType, TKeyType> getKey, TKeyType nullValue)
            where TKeyType : IComparable<TKeyType>
        {
            TValueType value;
            return max(values, getKey, (left, right) => left.CompareTo(right), out value) ? getKey(value) : nullValue;
        }

        public static TKeyType maxKey<TValueType, TKeyType>
            (this IEnumerable<TValueType> values, Func<TValueType, TKeyType> getKey, Func<TKeyType, TKeyType, int> comparer, TKeyType nullValue)
        {
            TValueType value;
            return max(values, getKey, comparer, out value) ? getKey(value) : nullValue;
        }

        public static bool min<TValueType>(this IEnumerable<TValueType> values, Func<TValueType, TValueType, int> comparer, out TValueType value)
        {
            value = default(TValueType);
            if (values != null)
            {
                if (comparer == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                int index = -1;
                foreach (TValueType nextValue in values)
                {
                    if (++index == 0) value = nextValue;
                    else if (comparer(nextValue, value) < 0) value = nextValue;
                }
                if (index != -1) return true;
            }
            return false;
        }

        public static bool min<TValueType, TKeyType>
            (this IEnumerable<TValueType> values, Func<TValueType, TKeyType> getKey, Func<TKeyType, TKeyType, int> comparer, out TValueType value)
        {
            value = default(TValueType);
            if (values != null)
            {
                if (getKey == null || comparer == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                int index = -1;
                TKeyType key = default(TKeyType);
                foreach (TValueType nextValue in values)
                {
                    if (++index == 0) key = getKey(value = nextValue);
                    else
                    {
                        TKeyType nextKey = getKey(nextValue);
                        if (comparer(nextKey, key) < 0)
                        {
                            value = nextValue;
                            key = nextKey;
                        }
                    }
                }
                if (index != -1) return true;
            }
            return false;
        }

        public static TValueType min<TValueType>(this IEnumerable<TValueType> values, TValueType nullValue)
            where TValueType : IComparable<TValueType>
        {
            TValueType value;
            return min(values, (left, right) => left.CompareTo(right), out value) ? value : nullValue;
        }

        public static TValueType min<TValueType, TKeyType>(this IEnumerable<TValueType> values, Func<TValueType, TKeyType> getKey, TValueType nullValue)
            where TKeyType : IComparable<TKeyType>
        {
            TValueType value;
            return min(values, getKey, (left, right) => left.CompareTo(right), out value) ? value : nullValue;
        }

        public static TValueType min<TValueType, TKeyType>
            (this IEnumerable<TValueType> values, Func<TValueType, TKeyType> getKey, Func<TKeyType, TKeyType, int> comparer, TValueType nullValue)
        {
            TValueType value;
            return min(values, getKey, comparer, out value) ? value : nullValue;
        }

        public static TKeyType minKey<TValueType, TKeyType>(this IEnumerable<TValueType> values, Func<TValueType, TKeyType> getKey, TKeyType nullValue)
            where TKeyType : IComparable<TKeyType>
        {
            TValueType value;
            return min(values, getKey, (left, right) => left.CompareTo(right), out value) ? getKey(value) : nullValue;
        }

        public static TKeyType minKey<TValueType, TKeyType>
            (this IEnumerable<TValueType> values, Func<TValueType, TKeyType> getKey, Func<TKeyType, TKeyType, int> comparer, TKeyType nullValue)
        {
            TValueType value;
            return min(values, getKey, comparer, out value) ? getKey(value) : nullValue;
        }

        public static HashSet<hashType> getHash<TValueType, hashType>
            (this IEnumerable<TValueType> values, Func<TValueType, hashType> getValue)
        {
            if (values != null)
            {
                if (getValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                HashSet<hashType> newValues = TmphHashSet<hashType>.Create();
                foreach (TValueType value in values) newValues.Add(getValue(value));
                return newValues;
            }
            return null;
        }

        public static HashSet<hashType> fillHash<TValueType, hashType>
            (this IEnumerable<TValueType> values, HashSet<hashType> hash, Func<TValueType, hashType> getValue)
        {
            if (values != null)
            {
                if (getValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                if (hash == null) hash = TmphHashSet<hashType>.Create();
                foreach (TValueType value in values) hash.Add(getValue(value));
            }
            return hash;
        }

        public static HashSet<TValueType> fillHash<TValueType>(this IEnumerable<TValueType> values, HashSet<TValueType> hash)
        {
            if (values != null)
            {
                if (hash == null) hash = TmphHashSet<TValueType>.Create();
                foreach (TValueType value in values) hash.Add(value);
            }
            return hash;
        }

        public static Dictionary<TKeyType, TmphList<TValueType>> group<TValueType, TKeyType>
            (this IEnumerable<TValueType> values, Func<TValueType, TKeyType> getKey)
            where TKeyType : IEquatable<TKeyType>
        {
            if (values != null)
            {
                if (getKey == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                Dictionary<TKeyType, TmphList<TValueType>> newValues = TmphDictionary<TKeyType>.Create<TmphList<TValueType>>();
                TmphList<TValueType> value;
                foreach (TValueType nextValue in values)
                {
                    TKeyType key = getKey(nextValue);
                    if (!newValues.TryGetValue(key, out value)) newValues[key] = value = new TmphList<TValueType>();
                    value.Add(nextValue);
                }
                return newValues;
            }
            return null;
        }

        public static Dictionary<TKeyType, TValueType> groupDistinct<TValueType, TKeyType>
           (this IEnumerable<TValueType> values, Func<TValueType, TKeyType> getKey, Func<TValueType, TValueType, bool> isLeftValue)
            where TKeyType : IEquatable<TKeyType>
        {
            if (values != null)
            {
                if (getKey == null || isLeftValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                Dictionary<TKeyType, TValueType> newValues = TmphDictionary<TKeyType>.Create<TValueType>();
                TValueType value;
                foreach (TValueType nextValue in values)
                {
                    TKeyType key = getKey(nextValue);
                    if (newValues.TryGetValue(key, out value))
                    {
                        if (isLeftValue(nextValue, value)) newValues[key] = nextValue;
                    }
                    else newValues.Add(key, nextValue);
                }
                return newValues;
            }
            return null;
        }

        public static Dictionary<TKeyType, int> groupCount<TValueType, TKeyType>
            (this IEnumerable<TValueType> values, Func<TValueType, TKeyType> getKey)
            where TKeyType : IEquatable<TKeyType>
        {
            return values.groupCount(getKey, 0);
        }

        public static string joinString<TValueType>(this IEnumerable<TValueType> values, Func<TValueType, string> toString)
        {
            return string.Concat(values.getSubArray(toString).ToArray());
        }

        public static string joinString<TValueType>(this IEnumerable<TValueType> values, string join, Func<TValueType, string> toString)
        {
            return string.Join(join, values.getSubArray(toString).ToArray());
        }

        public static string joinString<TValueType>(this IEnumerable<TValueType> values, char join, Func<TValueType, string> toString)
        {
            return values.getSubArray(toString).JoinString(join);
        }
    }
}