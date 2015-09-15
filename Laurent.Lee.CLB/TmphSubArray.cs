/*
-------------------------------------------------- -----------------------------------------
The frame content is protected by copyright law. In order to facilitate individual learning,
allows to download the program source information, but does not allow individuals or a third
party for profit, the commercial use of the source information. Without consent,
does not allow any form (even if partial, or modified) database storage,
copy the source of information. If the source content provided by third parties,
which corresponds to the third party content is also protected by copyright.

If you are found to have infringed copyright behavior, please give me a hint. THX!

Here in particular it emphasized that the third party is not allowed to contact addresses
published in this "version copyright statement" to send advertising material.
I will take legal means to resist sending spam.
-------------------------------------------------- ----------------------------------------
The framework under the GNU agreement, Detail View GNU License.
If you think about this item affection join the development team,
Please contact me: LaurentLeeJS@gmail.com
-------------------------------------------------- ----------------------------------------
Laurent.Lee.Framework Coded by Laurent Lee
*/

using Laurent.Lee.CLB.Algorithm;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Laurent.Lee.CLB
{
    public struct TmphSubArray<TValueType> : IList<TValueType>
    {
        internal TValueType[] array;
        private int length;
        private int startIndex;

        public TmphSubArray(int size)
        {
            array = size > 0 ? new TValueType[size] : null;
            startIndex = length = 0;
        }

        public TmphSubArray(TValueType[] value)
        {
            array = value;
            startIndex = 0;
            length = value == null ? 0 : value.Length;
        }

        public TmphSubArray(TValueType[] value, int startIndex, int length)
        {
            if (value == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            var range = new TmphArray.TmphRange(value.Length, startIndex, length);
            if (range.GetCount != length) TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
            if (range.GetCount != 0)
            {
                array = value;
                this.startIndex = range.SkipCount;
                this.length = range.GetCount;
            }
            else
            {
                array = TmphNullValue<TValueType>.Array;
                this.startIndex = this.length = 0;
            }
        }

        public TValueType[] Array
        {
            get { return array; }
        }

        public int StartIndex
        {
            get { return startIndex; }
        }

        internal int EndIndex
        {
            get { return startIndex + length; }
        }

        internal int FreeLength
        {
            get { return array.Length - startIndex - length; }
        }

        public TValueType this[int index]
        {
            get
            {
                if ((uint)index < (uint)length) return array[startIndex + index];
                TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
                return default(TValueType);
            }
            set
            {
                if ((uint)index < (uint)length) array[startIndex + index] = value;
                else TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
            }
        }

        public int Count
        {
            get { return length; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        IEnumerator<TValueType> IEnumerable<TValueType>.GetEnumerator()
        {
            if (length != 0) return new TmphIEnumerator<TValueType>.TmphArray(this);
            return TmphIEnumerator<TValueType>.Empty;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (length != 0) return new TmphIEnumerator<TValueType>.TmphArray(this);
            return TmphIEnumerator<TValueType>.Empty;
        }

        /// <summary>
        ///     清除所有数据
        /// </summary>
        public void Clear()
        {
            if (array != null)
            {
                if (TmphDynamicArray<TValueType>.IsClearArray) System.Array.Clear(array, 0, array.Length);
                Empty();
            }
        }

        /// <summary>
        ///     判断是否存在数据
        /// </summary>
        /// <param name="value">匹配数据</param>
        /// <returns>是否存在数据</returns>
        public bool Contains(TValueType value)
        {
            return IndexOf(value) != -1;
        }

        /// <summary>
        ///     获取匹配数据位置
        /// </summary>
        /// <param name="value">匹配数据</param>
        /// <returns>匹配位置,失败为-1</returns>
        public int IndexOf(TValueType value)
        {
            if (length != 0)
            {
                var index = System.Array.IndexOf(array, value, startIndex, length);
                if (index >= 0) return index - startIndex;
            }
            return -1;
        }

        /// <summary>
        ///     移除数据
        /// </summary>
        /// <param name="value">数据</param>
        /// <returns>是否存在移除数据</returns>
        public bool Remove(TValueType value)
        {
            var index = IndexOf(value);
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }
            return false;
        }

        /// <summary>
        ///     移除数据
        /// </summary>
        /// <param name="index">数据位置</param>
        /// <returns>被移除数据</returns>
        public void RemoveAt(int index)
        {
            if ((uint)index < (uint)length)
            {
                CLB.Unsafe.TmphArray.Move(array, startIndex + index + 1, startIndex + index, --length - index);
                array[startIndex + length] = default(TValueType);
            }
            else TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
        }

        /// <summary>
        ///     插入数据
        /// </summary>
        /// <param name="index">插入位置</param>
        /// <param name="value">数据</param>
        public void Insert(int index, TValueType value)
        {
            if ((uint)index <= (uint)length)
            {
                if (index != length)
                {
                    if (startIndex + length != array.Length)
                    {
                        CLB.Unsafe.TmphArray.Move(array, startIndex + index, startIndex + index + 1, length - index);
                        array[startIndex + index] = value;
                        ++length;
                    }
                    else
                    {
                        var values = TmphDynamicArray<TValueType>.GetNewArray(array.Length << 1);
                        System.Array.Copy(array, startIndex, values, startIndex, index);
                        values[startIndex + index] = value;
                        System.Array.Copy(array, startIndex + index, values, startIndex + index + 1, length++ - index);
                        array = values;
                    }
                }
                else Add(value);
            }
            else TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
        }

        public void CopyTo(TValueType[] values, int index)
        {
            if (values != null && index >= 0 && length + index <= values.Length)
            {
                if (length != 0) System.Array.Copy(array, startIndex, values, index, length);
            }
            else TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
        }

        /// <summary>
        ///     添加数据
        /// </summary>
        /// <param name="value">数据</param>
        public void Add(TValueType value)
        {
            if (array == null)
            {
                array = new TValueType[sizeof(int)];
                array[0] = value;
                length = 1;
            }
            else
            {
                var index = startIndex + length;
                if (index == array.Length)
                {
                    if (index == 0)
                        array = new TValueType[sizeof(int)];
                    else
                        setLength(index << 1);
                }
                array[index] = value;
                ++length;
            }
        }

        public static explicit operator TValueType[] (TmphSubArray<TValueType> value)
        {
            return value.ToArray();
        }

        public static explicit operator TmphSubArray<TValueType>(TValueType[] value)
        {
            return new TmphSubArray<TValueType>(value);
        }

        public IEnumerable<TValueType> ReverseEnumerable()
        {
            for (var endIndex = startIndex + length; endIndex > startIndex;)
                yield return array[--endIndex];
        }

        public TValueType[] ToArray()
        {
            if (length == 0)
                return TmphNullValue<TValueType>.Array;
            return length == array.Length ? array : getArray();
        }

        public TValueType[] GetArray()
        {
            return length != 0 ? getArray() : TmphNullValue<TValueType>.Array;
        }

        private TValueType[] getArray()
        {
            var newArray = new TValueType[length];
            System.Array.Copy(array, startIndex, newArray, 0, length);
            return newArray;
        }

        public TmphSubArray<TValueType> Reverse()
        {
            for (int index = startIndex, endIndex = index + length, middle = index + (length >> 1); index != middle;)
            {
                var value = array[index];
                array[index++] = array[--endIndex];
                array[endIndex] = value;
            }
            return this;
        }

        public TValueType[] GetReverse()
        {
            if (length == 0) return TmphNullValue<TValueType>.Array;
            var newArray = new TValueType[length];
            if (startIndex == 0)
            {
                int index = length;
                foreach (var value in array)
                {
                    newArray[--index] = value;
                    if (index == 0) break;
                }
            }
            else
            {
                int index = length, copyIndex = startIndex;
                do
                {
                    newArray[--index] = array[copyIndex++];
                } while (index != 0);
            }
            return newArray;
        }

        public void Null()
        {
            array = null;
            startIndex = length = 0;
        }

        public void UnsafeSet(TValueType[] value, int startIndex, int length)
        {
            array = value;
            this.startIndex = startIndex;
            this.length = length;
        }

        public void UnsafeSet(int startIndex, int length)
        {
            this.startIndex = startIndex;
            this.length = length;
        }

        public void UnsafeSetLength(int length)
        {
            this.length = length;
        }

        private void setLength(int count)
        {
            var newArray = TmphDynamicArray<TValueType>.GetNewArray(count);
            System.Array.Copy(array, startIndex, newArray, startIndex, length);
            array = newArray;
        }

        public void Empty()
        {
            startIndex = length = 0;
        }

        public int GetCount(Func<TValueType, bool> isValue)
        {
            if (isValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            if (length != 0)
            {
                var count = 0;
                if (startIndex == 0)
                {
                    var index = length;
                    foreach (var value in array)
                    {
                        if (isValue(value)) ++count;
                        if (--index == 0) break;
                    }
                }
                else
                {
                    int index = startIndex, endIndex = startIndex + length;
                    do
                    {
                        if (isValue(array[index])) ++count;
                    } while (++index != endIndex);
                }
                return count;
            }
            return 0;
        }

        public bool Any(Func<TValueType, bool> isValue)
        {
            if (isValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            return indexOf(isValue) != -1;
        }

        private int indexOf(Func<TValueType, bool> isValue)
        {
            if (length != 0)
            {
                if (startIndex == 0)
                {
                    var index = 0;
                    foreach (var value in array)
                    {
                        if (isValue(value)) return index;
                        if (++index == length) break;
                    }
                }
                else
                {
                    int index = startIndex, endIndex = startIndex + length;
                    do
                    {
                        if (isValue(array[index])) return index;
                    } while (++index != endIndex);
                }
            }
            return -1;
        }

        public void RemoveAtEnd(int index)
        {
            if ((uint)index < (uint)length) array[startIndex + index] = array[startIndex + --length];
            else TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
        }

        private void removeEnd(Func<TValueType, bool> isValue)
        {
            for (var index = startIndex + length; index != startIndex; --length)
            {
                if (!isValue(array[--index])) return;
            }
        }

        public TmphSubArray<TValueType> Remove(Func<TValueType, bool> isValue)
        {
            if (isValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            if (length != 0)
            {
                removeEnd(isValue);
                var index = indexOf(isValue);
                if (index != -1)
                {
                    for (int start = index, endIndex = startIndex + length; ++start != endIndex;)
                    {
                        if (!isValue(array[start])) array[index++] = array[start];
                    }
                    length = index - startIndex;
                }
            }
            return this;
        }

        public TValueType FirstOrDefault(Func<TValueType, bool> isValue)
        {
            if (isValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            var index = indexOf(isValue);
            return index != -1 ? array[index] : default(TValueType);
        }

        public TValueType LastOrDefault()
        {
            return length != 0 ? array[startIndex + length - 1] : default(TValueType);
        }

        public TmphSubArray<TValueType> Sub(int index, int count)
        {
            var range = new TmphArray.TmphRange(length, index, count < 0 ? length - index : count);
            if (range.GetCount > 0)
            {
                startIndex += range.SkipCount;
                length = range.GetCount;
            }
            return this;
        }

        public TmphSubArray<TValueType> GetSub(int index, int count)
        {
            var range = new TmphArray.TmphRange(length, index, count < 0 ? length - index : count);
            if (range.GetCount > 0)
            {
                return Unsafe(array, startIndex + range.SkipCount, range.GetCount);
            }
            return default(TmphSubArray<TValueType>);
        }

        public TmphSubArray<TValueType> Page(int pageSize, int currentPage)
        {
            var page = new TmphArray.TmphPage(length, pageSize, currentPage);
            return Unsafe(array, startIndex + page.SkipCount, page.CurrentPageSize);
        }

        public TmphSubArray<TValueType> PageDesc(int pageSize, int currentPage)
        {
            var page = new TmphArray.TmphPage(length, pageSize, currentPage);
            return Unsafe(array, startIndex + page.DescSkipCount, page.CurrentPageSize).Reverse();
        }

        public TValueType[] GetPageDesc(int pageSize, int currentPage)
        {
            var page = new TmphArray.TmphPage(length, pageSize, currentPage);
            return Unsafe(array, startIndex + page.DescSkipCount, page.CurrentPageSize).GetReverse();
        }

        private void addToLength(int length)
        {
            if (array == null) array = new TValueType[length < sizeof(int) ? sizeof(int) : length];
            else if (length > array.Length) setLength(length);
        }

        internal void UnsafeAdd(TValueType value)
        {
            array[startIndex + length++] = value;
        }

        public void Add(ICollection<TValueType> values)
        {
            var count = values.Count();
            if (count != 0)
            {
                var index = startIndex + length;
                addToLength(index + count);
                foreach (var value in values) array[index++] = value;
                length += count;
            }
        }

        public TmphSubArray<TValueType> Add(TValueType[] values)
        {
            if (values != null && values.Length != 0) add(values, 0, values.Length);
            return this;
        }

        public void Add(TValueType[] values, int index, int count)
        {
            var range = new TmphArray.TmphRange(values.length(), index, count);
            if ((count = range.GetCount) != 0) add(values, range.SkipCount, count);
        }

        public void Add(TmphSubArray<TValueType> values)
        {
            if (values.Count != 0) add(values.array, values.StartIndex, values.Count);
        }

        private void add(TValueType[] values, int index, int count)
        {
            var newLength = startIndex + length + count;
            addToLength(newLength);
            System.Array.Copy(values, index, array, startIndex + length, count);
            length += count;
        }

        internal void UnsafeAddExpand(TValueType value)
        {
            array[--startIndex] = value;
            ++length;
        }

        public TValueType Pop()
        {
            if (length == 0) TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
            return array[startIndex + --length];
        }

        internal TValueType UnsafePop()
        {
            return array[startIndex + --length];
        }

        internal TValueType UnsafePopReset()
        {
            var index = startIndex + --length;
            var value = array[index];
            array[index] = default(TValueType);
            return value;
        }

        public arrayType[] GetArray<arrayType>(Func<TValueType, arrayType> getValue)
        {
            //if (array != null)
            //{
            if (length != 0)
            {
                if (getValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                var newArray = new arrayType[length];
                for (int count = 0, index = startIndex, endIndex = startIndex + length; index != endIndex; ++index)
                {
                    newArray[count++] = getValue(array[index]);
                }
                return newArray;
            }
            return TmphNullValue<arrayType>.Array;
        }

        public unsafe TValueType[] GetFindArray(Func<TValueType, bool> isValue)
        {
            if (isValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            if (this.length != 0)
            {
                var length = ((this.length + 31) >> 5) << 2;
                var pool = TmphMemoryPool.GetDefaultPool(length);
                var data = pool.Get(length);
                try
                {
                    fixed (byte* dataFixed = data)
                    {
                        System.Array.Clear(data, 0, length);
                        return getFindArray(isValue, new TmphFixedMap(dataFixed));
                    }
                }
                finally
                {
                    pool.Push(ref data);
                }
            }
            return TmphNullValue<TValueType>.Array;
        }

        private TValueType[] getFindArray(Func<TValueType, bool> isValue, TmphFixedMap map)
        {
            if (startIndex == 0)
            {
                int count = 0, index = 0;
                foreach (var value in array)
                {
                    if (isValue(value))
                    {
                        ++count;
                        map.Set(index);
                    }
                    if (++index == length) break;
                }
                if (count != 0)
                {
                    var values = new TValueType[count];
                    for (index = length; count != 0; values[--count] = array[index])
                    {
                        while (!map.Get(--index)) ;
                    }
                    return values;
                }
            }
            else
            {
                int count = 0, index = startIndex, endIndex = startIndex + length;
                do
                {
                    if (isValue(array[index]))
                    {
                        ++count;
                        map.Set(index - startIndex);
                    }
                } while (++index != endIndex);
                if (count != 0)
                {
                    var values = new TValueType[count];
                    for (index = length; count != 0; values[--count] = array[startIndex + index])
                    {
                        while (!map.Get(--index)) ;
                    }
                    return values;
                }
            }
            return TmphNullValue<TValueType>.Array;
        }

        public TmphList<TValueType> ToList()
        {
            return length != 0 ? new TmphList<TValueType>(this, true) : null;
        }

        public TmphCollection<TValueType> ToCollection()
        {
            return length != 0 ? new TmphCollection<TValueType>(this, true) : null;
        }

        public TmphSubArray<arrayType> Concat<arrayType>(Func<TValueType, TmphSubArray<arrayType>> getValue)
        {
            if (getValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            if (length != 0)
            {
                var values = new TmphSubArray<arrayType>();
                if (startIndex == 0)
                {
                    var index = length;
                    foreach (var value in array)
                    {
                        values.Add(getValue(value));
                        if (--index == 0) break;
                    }
                }
                else
                {
                    int index = startIndex, endIndex = startIndex + length;
                    do
                    {
                        values.Add(getValue(array[index]));
                    } while (++index != endIndex);
                }
                return values;
            }
            return default(TmphSubArray<arrayType>);
        }

        public TmphSubArray<TValueType> Sort(Func<TValueType, TValueType, int> comparer)
        {
            TmphQuickSort.Sort(array, comparer, startIndex, length);
            return this;
        }

        public static TmphSubArray<TValueType> Unsafe(TValueType[] value, int startIndex, int length)
        {
            return new TmphSubArray<TValueType> { array = value, startIndex = startIndex, length = length };
        }
    }
}