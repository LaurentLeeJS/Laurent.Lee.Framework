using System;
using System.Collections;
using System.Collections.Generic;

namespace Laurent.Lee.CLB
{
    public struct TmphICollection<TValueType> : ICollection<TValueType>
    {
        private readonly ICollection collection;

        public TmphICollection(ICollection collection)
        {
            this.collection = collection;
        }

        public int Count
        {
            get { return collection != null ? collection.Count : 0; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        IEnumerator<TValueType> IEnumerable<TValueType>.GetEnumerator()
        {
            if (collection != null)
            {
                foreach (TValueType value in collection) yield return value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<TValueType>)this).GetEnumerator();
        }

        public void Clear()
        {
            TmphLog.Error.Throw(TmphLog.TmphExceptionType.ErrorOperation);
        }

        public void Add(TValueType value)
        {
            TmphLog.Error.Throw(TmphLog.TmphExceptionType.ErrorOperation);
        }

        public bool Remove(TValueType value)
        {
            TmphLog.Error.Throw(TmphLog.TmphExceptionType.ErrorOperation);
            return false;
        }

        public void CopyTo(TValueType[] values, int index)
        {
            if (collection != null) collection.CopyTo(values, index);
        }

        public bool Contains(TValueType value)
        {
            if (collection != null)
            {
                foreach (TValueType nextValue in collection)
                {
                    if (nextValue.Equals(value)) return true;
                }
            }
            return false;
        }
    }

    public static class TmphICollection
    {
        public static TmphICollection<TValueType> ToGeneric<TValueType>(this ICollection value)
        {
            return new TmphICollection<TValueType>(value);
        }

        public static ICollection<TValueType> NotNull<TValueType>(this ICollection<TValueType> value)
        {
            return value ?? TmphNullValue<TValueType>.Array;
        }

        public static int Count<TValueType>(this ICollection<TValueType> value)
        {
            return value != null ? value.Count : 0;
        }

        public static TValueType[] GetArray<TValueType>(this ICollection<TValueType> values)
        {
            if (values.Count() != 0)
            {
                var newValues = new TValueType[values.Count];
                var index = 0;
                foreach (var value in values) newValues[index++] = value;
                if (index != newValues.Length) Array.Resize(ref newValues, index);
                return newValues;
            }
            return TmphNullValue<TValueType>.Array;
        }

        public static TArrayType[] GetArray<TValueType, TArrayType>(this ICollection<TValueType> values,
            Func<TValueType, TArrayType> getValue)
        {
            if (values.Count() != 0)
            {
                if (getValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                var newValues = new TArrayType[values.Count];
                var index = 0;
                foreach (var value in values) if (getValue != null) newValues[index++] = getValue(value);
                if (index != newValues.Length) Array.Resize(ref newValues, index);
                return newValues;
            }
            return TmphNullValue<TArrayType>.Array;
        }

        public static TmphKeyValue<TKeyType, TValueType>[] GetKeyValueArray<TKeyType, TValueType>(
            this ICollection<TValueType> values, Func<TValueType, TKeyType> getKey)
        {
            if (values.Count() != 0)
            {
                if (getKey == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                var newValues = new TmphKeyValue<TKeyType, TValueType>[values.Count];
                var index = 0;
                foreach (var value in values) if (getKey != null) newValues[index++].Set(getKey(value), value);
                if (index != newValues.Length) Array.Resize(ref newValues, index);
                return newValues;
            }
            return TmphNullValue<TmphKeyValue<TKeyType, TValueType>>.Array;
        }

        public static TmphKeyValue<TKeyType, TValueType>[] GetKeyValueArray<TKeyType, TValueType>(
            this ICollection<KeyValuePair<TKeyType, TValueType>> values)
        {
            if (values.Count() != 0)
            {
                var newValues = new TmphKeyValue<TKeyType, TValueType>[values.Count];
                var index = 0;
                foreach (var value in values) newValues[index++].Set(value.Key, value.Value);
                if (index != newValues.Length) Array.Resize(ref newValues, index);
                return newValues;
            }
            return TmphNullValue<TmphKeyValue<TKeyType, TValueType>>.Array;
        }

        public static TmphList<TValueType> GetList<TValueType>(this ICollection<TValueType> values)
        {
            return values != null ? new TmphList<TValueType>(GetArray(values), true) : null;
        }

        public static TmphList<TArrayType> GetList<TValueType, TArrayType>(this ICollection<TValueType> values,
            Func<TValueType, TArrayType> getValue)
        {
            return values != null ? new TmphList<TArrayType>(GetArray(values, getValue), true) : null;
        }

        public static TmphCollection<TValueType> GetCollection<TValueType>(this ICollection<TValueType> values)
        {
            return values != null ? new TmphCollection<TValueType>(GetArray(values), true) : null;
        }

        public static TmphCollection<TArrayType> GetCollection<TValueType, TArrayType>
            (this ICollection<TValueType> values, Func<TValueType, TArrayType> getValue)
        {
            return values != null ? new TmphCollection<TArrayType>(GetArray(values, getValue), true) : null;
        }

        public static void CopyTo<TValueType>(this ICollection<TValueType> values, TValueType[] array, int index)
        {
            if (values.Count() != 0)
            {
                var range = new TmphArray.TmphRange(array.length(), index, values.Count);
                if (range.GetCount == values.Count)
                {
                    foreach (var value in values) array[index++] = value;
                }
                else TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
            }
        }

        public static void CopyTo<TValueType>(this ICollection<TValueType> values, TValueType[] array, int index, int count)
        {
            if (values.Count() != 0)
            {
                if (count > values.Count) count = values.Count;
                var range = new TmphArray.TmphRange(array.length(), index, count);
                if (range.GetCount == count)
                {
                    foreach (var value in values) array[index++] = value;
                }
                else TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
            }
        }

        public static TmphSubArray<TValueType> GetFind<TValueType>(this ICollection<TValueType> values,
            Func<TValueType, bool> isValue)
        {
            if (values.Count() != 0)
            {
                if (isValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                var value = new TValueType[values.Count];
                var count = 0;
                foreach (var nextValue in values)
                {
                    if (isValue != null && isValue(nextValue)) value[count++] = nextValue;
                }
                return TmphSubArray<TValueType>.Unsafe(value, 0, count);
            }
            return default(TmphSubArray<TValueType>);
        }

        public static TmphList<TValueType> GetFindList<TValueType>(this ICollection<TValueType> values,
            Func<TValueType, bool> isValue)
        {
            return GetFind(values, isValue).ToList();
        }

        public static TValueType[] GetFindArray<TValueType>(this ICollection<TValueType> values,
            Func<TValueType, bool> isValue)
        {
            return values != null ? GetFind(values, isValue).ToArray().notNull() : TmphNullValue<TValueType>.Array;
        }

        public static Dictionary<TKeyType, int> GroupCount<TValueType, TKeyType>
            (this ICollection<TValueType> values, Func<TValueType, TKeyType> getKey)
            where TKeyType : IEquatable<TKeyType>
        {
            return values.groupCount(getKey, values.Count());
        }

        public static TmphSubArray<TmphSubArray<TValueType>> GetSplit<TValueType>(this ICollection<TValueType> values,
            TValueType split)
            where TValueType : IComparable<TValueType>
        {
            if (values != null)
            {
                var value = new TmphSubArray<TmphSubArray<TValueType>>();
                var newValue = new TmphSubArray<TValueType>();
                foreach (var nextValue in values)
                {
                    if (nextValue.CompareTo(split) == 0)
                    {
                        value.Add(newValue);
                        newValue.Null();
                    }
                    else newValue.Add(nextValue);
                }
                if (newValue.Count != 0) value.Add(newValue);
                return value;
            }
            return default(TmphSubArray<TmphSubArray<TValueType>>);
        }

        public static string JoinString<TValueType>(this ICollection<TValueType> values, Func<TValueType, string> toString)
        {
            return string.Concat(values.GetArray(toString));
        }

        public static string JoinString<TValueType>(this ICollection<TValueType> values, string join,
            Func<TValueType, string> toString)
        {
            return string.Join(join, values.GetArray(toString));
        }

        public static string JoinString<TValueType>(this ICollection<TValueType> values, char join,
            Func<TValueType, string> toString)
        {
            return values.GetArray(toString).JoinString(join);
        }
    }
}