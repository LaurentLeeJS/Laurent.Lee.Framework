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

using System;
using System.Collections;
using System.Collections.Generic;

namespace Laurent.Lee.CLB
{
    /// <summary>
    ///     双向动态数组
    /// </summary>
    /// <typeparam name="TValueType">数据类型</typeparam>
    public sealed class TmphCollection<TValueType> : TmphDynamicArray<TValueType>, IList<TValueType>
    {
        /// <summary>
        ///     双向动态数组
        /// </summary>
        public TmphCollection()
        {
            array = TmphNullValue<TValueType>.Array;
        }

        /// <summary>
        ///     双向动态数组
        /// </summary>
        /// <param name="count">数组容器尺寸</param>
        public TmphCollection(int count)
        {
            array = count > 0 ? new TValueType[count] : TmphNullValue<TValueType>.Array;
        }

        /// <summary>
        ///     双向动态数组
        /// </summary>
        /// <param name="values">数据集合</param>
        public TmphCollection(ICollection<TValueType> values) : this(values == null ? null : values.GetArray(), true)
        {
        }

        /// <summary>
        ///     双向动态数组
        /// </summary>
        /// <param name="value">单向动态数组</param>
        public TmphCollection(TmphList<TValueType> value)
        {
            EndIndex = value.Count();
            if (EndIndex == 0) array = TmphNullValue<TValueType>.Array;
            else
            {
                array = value.array;
                if (EndIndex == array.Length)
                {
                    EndIndex = 0;
                    IsFull = true;
                }
            }
        }

        /// <summary>
        ///     双向动态数组
        /// </summary>
        /// <param name="values">数据数组</param>
        /// <param name="isUnsafe">true表示使用原数组,false表示需要复制数组</param>
        public TmphCollection(TValueType[] values) : this(values, false)
        {
        }

        /// <summary>
        ///     双向动态数组
        /// </summary>
        /// <param name="values">数据数组</param>
        /// <param name="isUnsafe">true表示使用原数组,false表示需要复制数组</param>
        public TmphCollection(TValueType[] values, bool isUnsafe)
        {
            if (values.length() == 0) array = TmphNullValue<TValueType>.Array;
            else
            {
                if (isUnsafe)
                {
                    array = values;
                    IsFull = true;
                }
                else
                {
                    array = TmphNullValue<TValueType>.Array;
                    setLength(values.Length);
                    Array.Copy(values, array, EndIndex = values.Length);
                    if (values.Length == array.Length) IsFull = true;
                }
            }
        }

        /// <summary>
        ///     双向动态数组
        /// </summary>
        /// <param name="values">数据数组</param>
        /// <param name="index">起始位置</param>
        /// <param name="count">数据数量</param>
        /// <param name="isUnsafe">true表示可以使用原数组,false表示需要复制数组</param>
        public TmphCollection(TValueType[] values, int index, int count, bool isUnsafe = false)
        {
            var range = new TmphArray.TmphRange(values.length(), index, count);
            count = range.GetCount;
            if (isUnsafe)
            {
                if (count != 0)
                {
                    array = values;
                    StartIndex = EndIndex = range.SkipCount;
                    if (count == values.Length) IsFull = true;
                    else
                    {
                        IsFull = false;
                        if ((EndIndex += count) >= values.Length) EndIndex -= values.Length;
                    }
                }
                else array = values ?? TmphNullValue<TValueType>.Array;
                ;
            }
            else if (count == 0) array = TmphNullValue<TValueType>.Array;
            else
            {
                Array.Copy(values, range.SkipCount, array = GetNewArray(count), 0, count);
                IsFull = true;
            }
        }

        /// <summary>
        ///     双向动态数组
        /// </summary>
        /// <param name="values">数据数组</param>
        /// <param name="isUnsafe">true表示可以使用原数组,false表示需要复制数组</param>
        public TmphCollection(TmphSubArray<TValueType> values, bool isUnsafe = false)
        {
            if (isUnsafe)
            {
                if (values.Count != 0)
                {
                    array = values.Array;
                    StartIndex = EndIndex = values.StartIndex;
                    if (values.Count == array.Length) IsFull = true;
                    else
                    {
                        IsFull = false;
                        if ((EndIndex += values.Count) == array.Length) EndIndex = 0;
                    }
                }
                else array = values.Array ?? TmphNullValue<TValueType>.Array;
                ;
            }
            else if (values.Count == 0) array = TmphNullValue<TValueType>.Array;
            else
            {
                Array.Copy(values.Array, values.StartIndex, array = GetNewArray(values.Count), 0, values.Count);
                IsFull = true;
            }
        }

        /// <summary>
        ///     非安全访问单向动态数组
        /// </summary>
        /// <returns>非安全访问单向动态数组</returns>
        public TmphUnsafer Unsafer
        {
            get { return new TmphUnsafer { Collection = this }; }
        }

        /// <summary>
        ///     起始位置
        /// </summary>
        public int StartIndex { get; private set; }

        /// <summary>
        ///     结束位置
        /// </summary>
        public int EndIndex { get; private set; }

        /// <summary>
        ///     数据是否全有效
        /// </summary>
        public bool IsFull { get; private set; }

        /// 数据数量
        /// </summary>
        protected override int ValueCount
        {
            get { return Count; }
        }

        /// <summary>
        ///     数据数量
        /// </summary>
        public int Count
        {
            get
            {
                if (IsFull) return array.Length;
                var count = EndIndex - StartIndex;
                return count >= 0 ? count : (count + array.Length);
            }
        }

        /// <summary>
        ///     设置或获取值
        /// </summary>
        /// <param name="index">位置</param>
        /// <returns>数据值</returns>
        public TValueType this[int index]
        {
            get
            {
                if (index >= 0)
                {
                    index += StartIndex;
                    if (EndIndex > StartIndex
                        ? index < EndIndex
                        : ((IsFull || EndIndex < StartIndex) &&
                           (index < array.Length || (index -= array.Length) < EndIndex)))
                    {
                        return array[index];
                    }
                }
                TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
                return default(TValueType);
            }
            set
            {
                if (index >= 0)
                {
                    index += StartIndex;
                    if (EndIndex > StartIndex
                        ? index < EndIndex
                        : ((IsFull || EndIndex < StartIndex) &&
                           (index < array.Length || (index -= array.Length) < EndIndex)))
                    {
                        array[index] = value;
                        return;
                    }
                }
                TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     枚举器
        /// </summary>
        /// <returns>枚举器</returns>
        IEnumerator<TValueType> IEnumerable<TValueType>.GetEnumerator()
        {
            if (EndIndex == 0)
            {
                if (StartIndex != 0 || IsFull) return new TmphIEnumerator<TValueType>.TmphArray(this);
            }
            else if (EndIndex > StartIndex) return new TmphIEnumerator<TValueType>.TmphArray(this);
            else if (IsFull || EndIndex < StartIndex) return new TmphEnumerator(this);
            return TmphIEnumerator<TValueType>.Empty;
        }

        /// <summary>
        ///     枚举器
        /// </summary>
        /// <returns>枚举器</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            if (EndIndex == 0)
            {
                if (StartIndex != 0 || IsFull) return new TmphIEnumerator<TValueType>.TmphArray(this);
            }
            else if (EndIndex > StartIndex) return new TmphIEnumerator<TValueType>.TmphArray(this);
            else if (IsFull || EndIndex < StartIndex) return new TmphEnumerator(this);
            return TmphIEnumerator<TValueType>.Empty;
        }

        /// <summary>
        ///     清除所有数据
        /// </summary>
        public void Clear()
        {
            if (IsClearArray) Array.Clear(array, 0, array.Length);
            Empty();
        }

        /// <summary>
        ///     添加数据
        /// </summary>
        /// <param name="value">数据</param>
        public void Add(TValueType value)
        {
            if (IsFull) setLength(array.Length << 1);
            else if (array.Length == 0) setLength(sizeof(int));
            array[EndIndex] = value;
            if (++EndIndex == array.Length) EndIndex = 0;
            if (StartIndex == EndIndex) IsFull = true;
        }

        /// <summary>
        ///     插入数据
        /// </summary>
        /// <param name="index">插入位置</param>
        /// <param name="value">数据</param>
        public void Insert(int index, TValueType value)
        {
            if (index != 0)
            {
                var count = Count;
                if (index != count)
                {
                    if (index > 0 && index < count)
                    {
                        var length = count + 1;
                        if (length <= array.Length)
                        {
                            if (index < (count >> 1))
                            {
                                copyLeft(StartIndex, index, 1);
                                if (StartIndex != 0) --StartIndex;
                                else StartIndex = array.Length - 1;
                            }
                            else
                            {
                                copyRight(StartIndex + index, count - index, 1);
                                if (++EndIndex == array.Length) EndIndex = 0;
                            }
                            if ((index += StartIndex) >= array.Length) index -= array.Length;
                            array[index] = value;
                            if (StartIndex == EndIndex) IsFull = true;
                        }
                        else
                        {
                            var values = GetNewArray(length);
                            copyTo(values, 0, index);
                            values[index] = value;
                            if ((StartIndex += index) >= array.Length) StartIndex -= array.Length;
                            IsFull = false;
                            copyTo(values, index + 1, count - index);
                            StartIndex = 0;
                            if (length == values.Length)
                            {
                                IsFull = true;
                                EndIndex = 0;
                            }
                            else EndIndex = length;
                            array = values;
                        }
                    }
                    else TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
                }
                else Add(value);
            }
            else AddExpand(value);
        }

        /// <summary>
        ///     移除数据
        /// </summary>
        /// <param name="index">数据位置</param>
        public override void RemoveAt(int index)
        {
            GetRemoveAt(index);
        }

        /// <summary>
        ///     复制数据
        /// </summary>
        /// <param name="values">目标数据</param>
        /// <param name="index">目标位置</param>
        public void CopyTo(TValueType[] values, int index)
        {
            var length = Count;
            if (values != null && index >= 0 && length + index <= values.Length)
            {
                if (length != 0) copyTo(values, index);
            }
            else TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
        }

        /// <summary>
        ///     获取匹配数据位置
        /// </summary>
        /// <param name="value">匹配数据</param>
        /// <returns>匹配位置,失败为-1</returns>
        public override int IndexOf(TValueType value)
        {
            var index = -1;
            if (EndIndex > StartIndex) index = Array.IndexOf(array, value, StartIndex, EndIndex - StartIndex);
            else if (IsFull || EndIndex < StartIndex)
            {
                index = Array.IndexOf(array, value, StartIndex, array.Length - StartIndex);
                if (index < 0)
                {
                    index = Array.IndexOf(array, value, 0, EndIndex);
                    if (index >= 0) index += array.Length;
                }
            }
            return index - StartIndex;
        }

        /// <summary>
        ///     获取第一个值
        /// </summary>
        /// <returns>第一个值,失败为default(TValueType)</returns>
        public TValueType FirstOrDefault()
        {
            return EndIndex != StartIndex || IsFull ? array[StartIndex] : default(TValueType);
        }

        /// <summary>
        ///     获取最后一个值
        /// </summary>
        /// <returns>最后一个值,失败为default(TValueType)</returns>
        public TValueType LastOrDefault()
        {
            return StartIndex != EndIndex || IsFull
                ? array[(EndIndex != 0 ? EndIndex : array.Length) - 1]
                : default(TValueType);
        }

        /// <summary>
        ///     强制类型转换
        /// </summary>
        /// <param name="value">单向动态数组</param>
        /// <returns>数组数据</returns>
        public static explicit operator TValueType[] (TmphCollection<TValueType> value)
        {
            return value != null ? value.ToArray() : null;
        }

        /// <summary>
        ///     强制类型转换
        /// </summary>
        /// <param name="value">数组数据</param>
        /// <returns>单向动态数组</returns>
        public static explicit operator TmphCollection<TValueType>(TValueType[] value)
        {
            return value != null ? new TmphCollection<TValueType>(value, true) : null;
        }

        /// <summary>
        ///     设置数据长度
        /// </summary>
        /// <param name="length">数据长度</param>
        private void setLength(int length)
        {
            var values = GetNewArray(length);
            copyTo(values, 0);
            EndIndex = Count;
            StartIndex = 0;
            IsFull = false;
            array = values;
        }

        /// <summary>
        ///     长度设为0
        /// </summary>
        public void Empty()
        {
            IsFull = false;
            StartIndex = EndIndex = 0;
        }

        /// <summary>
        ///     添加数据集合
        /// </summary>
        /// <param name="values">数据集合</param>
        public void Add(ICollection<TValueType> values)
        {
            Add(values.GetArray());
        }

        /// <summary>
        ///     添加数据集合
        /// </summary>
        /// <param name="values">数据集合</param>
        /// <param name="index">起始位置</param>
        /// <param name="count">数量</param>
        private void add(TValueType[] values, int index, int count)
        {
            if (EndIndex >= StartIndex)
            {
                var copyCount = array.Length - EndIndex;
                if (copyCount < count)
                {
                    Array.Copy(values, index, array, EndIndex, copyCount);
                    index += copyCount;
                    count -= copyCount;
                    EndIndex = 0;
                }
            }
            Array.Copy(values, index, array, EndIndex, count);
            if ((EndIndex += count) == array.Length) EndIndex = 0;
            if (EndIndex == StartIndex) IsFull = true;
        }

        /// <summary>
        ///     添加数据集合
        /// </summary>
        /// <param name="values">数据集合</param>
        public new void Add(TValueType[] values)
        {
            if (values.length() != 0)
            {
                var length = Count + values.Length;
                if (array == null || length > array.Length) setLength(length);
                add(values, 0, values.Length);
            }
        }

        /// <summary>
        ///     添加数据集合
        /// </summary>
        /// <param name="values">数据集合</param>
        /// <param name="index">起始位置</param>
        /// <param name="count">数量</param>
        public override void Add(TValueType[] values, int index, int count)
        {
            if (values != null && values.Length != 0 && count >= 0)
            {
                var endIndex = index + count;
                if (endIndex <= values.Length && index >= 0)
                {
                    var length = Count + count;
                    if (array == null || length > array.Length) setLength(length);
                    add(values, index, count);
                }
                else TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     添加数据集合
        /// </summary>
        /// <param name="value">数据集合</param>
        public void Add(TmphCollection<TValueType> value)
        {
            if (value != null) Add(value, 0, value.Count);
        }

        /// <summary>
        ///     添加数据集合
        /// </summary>
        /// <param name="value">数据集合</param>
        /// <param name="index">起始位置</param>
        /// <param name="count">数量</param>
        public void Add(TmphCollection<TValueType> value, int index, int count)
        {
            int endIndex = index + count, valueCount = value != null ? value.Count : 0;
            if (endIndex > valueCount || index < 0 || count < 0)
                TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
            if (count != 0)
            {
                var length = Count + count;
                if (array == null || length > array.Length) setLength(length);
                var values = value.array;
                if ((index += value.StartIndex) >= values.Length) add(values, index - values.Length, count);
                else if ((endIndex += value.StartIndex) <= values.Length) add(values, index, count);
                else
                {
                    add(values, index, length = values.Length - index);
                    add(values, 0, count - length);
                }
            }
        }

        /// <summary>
        ///     向前端添加一个数据
        /// </summary>
        /// <param name="value">数据</param>
        public void AddExpand(TValueType value)
        {
            if (IsFull) setLength(array.Length << 1);
            else if (array.Length == 0) setLength(sizeof(int));
            if (StartIndex == 0) StartIndex = array.Length;
            array[--StartIndex] = value;
            if (StartIndex == EndIndex) IsFull = true;
        }

        /// <summary>
        ///     插入数据集合
        /// </summary>
        /// <param name="index">插入位置</param>
        /// <param name="value">数据集合</param>
        public void Insert(int index, TmphCollection<TValueType> value)
        {
            int count = Count, valueCount = value.Count();
            if (index >= 0 && index <= count)
            {
                if (valueCount != 0)
                {
                    var length = count + valueCount;
                    if (length <= array.Length)
                    {
                        if (index == 0)
                        {
                            count = EndIndex;
                            if ((EndIndex += array.Length - length) > array.Length) EndIndex -= array.Length;
                            value.copyTo(this);
                            if ((StartIndex -= valueCount) < 0) StartIndex += array.Length;
                            if (StartIndex == (EndIndex = count)) IsFull = true;
                        }
                        else if (index == count) value.copyTo(this);
                        else if (index < (count >> 1))
                        {
                            copyLeft(StartIndex, index, valueCount);
                            if ((StartIndex -= valueCount) < 0) StartIndex += array.Length;
                            length = EndIndex;
                            if ((EndIndex -= valueCount + (count - index)) < 0) EndIndex += array.Length;
                            value.copyTo(this);
                            if (StartIndex == (EndIndex = length)) IsFull = true;
                        }
                        else
                        {
                            copyRight(StartIndex + index, count - index, valueCount);
                            length = EndIndex;
                            if ((EndIndex -= count - index) < 0) EndIndex += array.Length;
                            value.copyTo(this);
                            if ((EndIndex = length + valueCount) >= array.Length) EndIndex -= array.Length;
                            if (StartIndex == EndIndex) IsFull = true;
                        }
                    }
                    else
                    {
                        var values = GetNewArray(length);
                        copyTo(values, 0, index);
                        value.copyTo(values, index);
                        if ((StartIndex += index) >= array.Length) StartIndex -= array.Length;
                        IsFull = false;
                        copyTo(values, index + valueCount, count - index);
                        StartIndex = 0;
                        if (length == values.Length)
                        {
                            IsFull = true;
                            EndIndex = 0;
                        }
                        else EndIndex = length;
                        array = values;
                    }
                }
            }
            else TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
        }

        /// <summary>
        ///     移除数据
        /// </summary>
        /// <param name="index">数据位置</param>
        /// <returns>被移除数据</returns>
        public override TValueType GetRemoveAt(int index)
        {
            if (index != 0)
            {
                var count = Count;
                if (index != count - 1)
                {
                    if (index < count && index > 0)
                    {
                        var valueIndex = index + StartIndex;
                        var value = array[valueIndex < array.Length ? valueIndex : valueIndex - array.Length];
                        if (index < (count >> 1))
                        {
                            copyRight(StartIndex, index, 1);
                            if (++StartIndex == array.Length) StartIndex = 0;
                        }
                        else
                        {
                            copyLeft(StartIndex + ++index, count - index, 1);
                            if (EndIndex == 0) EndIndex = array.Length;
                            --EndIndex;
                        }
                        IsFull = false;
                        return value;
                    }
                }
                else return Pop();
            }
            else return PopExpand();
            TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
            return default(TValueType);
        }

        /// <summary>
        ///     移除数据范围
        /// </summary>
        /// <param name="index">起始位置</param>
        public void RemoveRange(int index)
        {
            RemoveRange(index, Count - index);
        }

        /// <summary>
        ///     移除数据范围
        /// </summary>
        /// <param name="index">起始位置</param>
        /// <param name="count">移除数量</param>
        public void RemoveRange(int index, int count)
        {
            var endCount = Count - index - count;
            if (endCount >= 0 && index >= 0 && count >= 0)
            {
                if (count != 0)
                {
                    if (index != 0)
                    {
                        if (endCount == 0)
                        {
                            if ((EndIndex -= count) < 0) EndIndex += array.Length;
                        }
                        else if (index < endCount)
                        {
                            copyRight(StartIndex, index, count);
                            if ((StartIndex += count) >= array.Length) StartIndex -= array.Length;
                        }
                        else
                        {
                            copyLeft(StartIndex + index + count, endCount, count);
                            if ((EndIndex -= count) < 0) EndIndex += array.Length;
                        }
                    }
                    else if ((StartIndex += count) >= array.Length) StartIndex -= array.Length;
                    IsFull = false;
                }
            }
            else TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
        }

        /// <summary>
        ///     弹出最后一个数据
        /// </summary>
        /// <returns>最后一个数据</returns>
        public TValueType Pop()
        {
            if (IsFull || StartIndex != EndIndex)
            {
                if (EndIndex == 0) EndIndex = array.Length;
                var value = array[--EndIndex];
                IsFull = false;
                return value;
            }
            TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
            return default(TValueType);
        }

        /// <summary>
        ///     弹出第一个数据
        /// </summary>
        /// <returns>第一个数据</returns>
        public TValueType PopExpand()
        {
            if (IsFull || StartIndex != EndIndex) return popExpand();
            TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
            return default(TValueType);
        }

        /// <summary>
        ///     弹出第一个数据
        /// </summary>
        /// <returns>第一个数据</returns>
        private TValueType popExpand()
        {
            var value = array[StartIndex];
            if (++StartIndex == array.Length) StartIndex = 0;
            IsFull = false;
            return value;
        }

        /// <summary>
        ///     元素右移
        /// </summary>
        /// <param name="index">起始位置</param>
        /// <param name="count">移动元素数量</param>
        /// <param name="shiftCount">移动位置数量</param>
        private void copyRight(int index, int count, int shiftCount)
        {
            int endIndex = index + count, copyIndex = endIndex + shiftCount;
            if (index >= array.Length)
            {
                index -= array.Length;
                endIndex -= array.Length;
                copyIndex -= array.Length;
            }
            else
            {
                if (endIndex > array.Length)
                {
                    for (endIndex -= array.Length, copyIndex -= array.Length;
                        --endIndex >= 0;
                        array[--copyIndex] = array[endIndex])
                        ;
                    if ((endIndex = array.Length) - index > copyIndex)
                    {
                        while (--copyIndex >= 0) array[copyIndex] = array[--endIndex];
                        copyIndex = array.Length;
                    }
                }
                else if (copyIndex > array.Length)
                {
                    if (count > (copyIndex -= array.Length))
                    {
                        while (--copyIndex >= 0) array[copyIndex] = array[--endIndex];
                        copyIndex = array.Length;
                    }
                }
            }
            while (--endIndex >= index) array[--copyIndex] = array[endIndex];
        }

        /// <summary>
        ///     元素左移
        /// </summary>
        /// <param name="index">起始位置</param>
        /// <param name="count">移动元素数量</param>
        /// <param name="shiftCount">移动位置数量</param>
        private void copyLeft(int index, int count, int shiftCount)
        {
            int endIndex = index + count, copyIndex = index - shiftCount;
            if (index >= array.Length)
            {
                index -= array.Length;
                endIndex -= array.Length;
                if (copyIndex < array.Length)
                {
                    if ((array.Length - copyIndex) < count)
                    {
                        while (copyIndex != array.Length) array[copyIndex++] = array[index++];
                        copyIndex = 0;
                    }
                }
                else copyIndex -= array.Length;
            }
            else if (endIndex > array.Length)
            {
                while (index != array.Length) array[copyIndex++] = array[index++];
                index = 0;
                endIndex -= array.Length;
            }
            else if (copyIndex < 0)
            {
                if (-copyIndex >= count) copyIndex += array.Length;
                else
                {
                    for (copyIndex += array.Length; copyIndex != array.Length; array[copyIndex++] = array[index++]) ;
                    copyIndex = 0;
                }
            }
            while (index != endIndex) array[copyIndex++] = array[index++];
        }

        /// <summary>
        ///     复制数据
        /// </summary>
        /// <param name="values">目标数组</param>
        /// <param name="index">起始位置</param>
        /// <param name="count">数量</param>
        private void copyTo(TValueType[] values, int index, int count)
        {
            var endIndex = StartIndex + count;
            if (endIndex <= array.Length) Array.Copy(array, StartIndex, values, index, count);
            else
            {
                Array.Copy(array, StartIndex, values, index, count = array.Length - StartIndex);
                Array.Copy(array, 0, values, index + count, endIndex - array.Length);
            }
        }

        /// <summary>
        ///     复制数据
        /// </summary>
        /// <param name="values">目标数组</param>
        /// <param name="index">起始位置</param>
        private void copyTo(TValueType[] values, int index)
        {
            if (EndIndex > StartIndex) Array.Copy(array, StartIndex, values, index, EndIndex - StartIndex);
            else if (IsFull || EndIndex < StartIndex)
            {
                var count = array.Length - StartIndex;
                Array.Copy(array, StartIndex, values, index, count);
                Array.Copy(array, 0, values, index + count, EndIndex);
            }
        }

        /// <summary>
        ///     复制数据
        /// </summary>
        /// <param name="value">目标列表</param>
        private void copyTo(TmphCollection<TValueType> value)
        {
            if (EndIndex > StartIndex) value.add(array, StartIndex, EndIndex - StartIndex);
            else if (IsFull || EndIndex < StartIndex)
            {
                value.add(array, StartIndex, array.Length - StartIndex);
                value.add(array, 0, EndIndex);
            }
        }

        /// <summary>
        ///     复制数据
        /// </summary>
        /// <param name="index">起始位置</param>
        /// <param name="values">目标数组</param>
        /// <param name="count">数量</param>
        private void copyTo(int index, TValueType[] values, int count)
        {
            if (EndIndex > StartIndex) Array.Copy(array, StartIndex + index, values, 0, count);
            else if ((index += StartIndex) >= array.Length) Array.Copy(array, index - array.Length, values, 0, count);
            else
            {
                var endCount = array.Length - index;
                if (endCount >= count) Array.Copy(array, index, values, 0, count);
                else
                {
                    Array.Copy(array, index, values, 0, endCount);
                    Array.Copy(array, 0, values, endCount, count - endCount);
                }
            }
        }

        /// <summary>
        ///     转换数组
        /// </summary>
        /// <returns>数组</returns>
        public TValueType[] GetArray()
        {
            var values = new TValueType[Count];
            copyTo(values, 0);
            return values;
        }

        /// <summary>
        ///     转换数组
        /// </summary>
        /// <returns>数组</returns>
        public TValueType[] ToArray()
        {
            return IsFull && StartIndex == 0 ? array : GetArray();
        }

        /// <summary>
        ///     转换数据集合
        /// </summary>
        /// <typeparam name="arrayType">目标数据类型</typeparam>
        /// <param name="getValue">数据转换器</param>
        /// <returns>数据集合</returns>
        public override arrayType[] GetArray<arrayType>(Func<TValueType, arrayType> getValue)
        {
            var length = Count;
            if (length != 0)
            {
                if (getValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                var values = new arrayType[length];
                length = 0;
                if (EndIndex > StartIndex)
                {
                    for (var index = StartIndex; index != EndIndex; ++index) values[length++] = getValue(array[index]);
                }
                else
                {
                    for (var index = StartIndex; index != array.Length; ++index)
                        values[length++] = getValue(array[index]);
                    for (var index = 0; index != EndIndex; ++index) values[length++] = getValue(array[index]);
                }
                return values;
            }
            return TmphNullValue<arrayType>.Array;
        }

        /// <summary>
        ///     转换键值对数组
        /// </summary>
        /// <typeparam name="TKeyType">键类型</typeparam>
        /// <param name="getKey">键值获取器</param>
        /// <returns>键值对数组</returns>
        public override TmphKeyValue<TKeyType, TValueType>[] GetKeyValueArray<TKeyType>(Func<TValueType, TKeyType> getKey)
        {
            var length = Count;
            if (length != 0)
            {
                if (getKey == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                var values = new TmphKeyValue<TKeyType, TValueType>[length];
                length = 0;
                if (EndIndex > StartIndex)
                {
                    for (var index = StartIndex; index != EndIndex; ++index)
                    {
                        var value = array[index];
                        values[length++].Set(getKey(value), value);
                    }
                }
                else
                {
                    for (var index = StartIndex; index != array.Length; ++index)
                    {
                        var value = array[index];
                        values[length++].Set(getKey(value), value);
                    }
                    for (var index = 0; index != EndIndex; ++index)
                    {
                        var value = array[index];
                        values[length++].Set(getKey(value), value);
                    }
                }
                return values;
            }
            return TmphNullValue<TmphKeyValue<TKeyType, TValueType>>.Array;
        }

        /// <summary>
        ///     转换单向列表
        /// </summary>
        /// <returns>单向列表</returns>
        public TmphList<TValueType> ToList()
        {
            return StartIndex == 0 ? new TmphList<TValueType>(array, 0, Count, true) : new TmphList<TValueType>(this);
        }

        /// <summary>
        ///     逆转列表
        /// </summary>
        public void Reverse()
        {
            if (EndIndex > StartIndex)
            {
                for (int leftIndex = StartIndex, endIndex = (StartIndex + EndIndex) >> 1, rightIndex = EndIndex;
                    leftIndex != endIndex;
                    ++leftIndex)
                {
                    var value = array[leftIndex];
                    array[leftIndex] = array[--rightIndex];
                    array[rightIndex] = value;
                }
            }
            else if (IsFull || EndIndex < StartIndex)
            {
                int leftCount = array.Length - StartIndex, leftIndex = StartIndex, rightIndex = EndIndex, endIndex;
                if (leftCount > EndIndex)
                {
                    for (; --rightIndex >= 0; ++leftIndex)
                    {
                        var value = array[leftIndex];
                        array[leftIndex] = array[rightIndex];
                        array[rightIndex] = value;
                    }
                    endIndex = (leftIndex + array.Length) >> 1;
                    rightIndex = array.Length;
                }
                else
                {
                    while (leftIndex != array.Length)
                    {
                        var value = array[leftIndex];
                        array[leftIndex++] = array[--rightIndex];
                        array[rightIndex] = value;
                    }
                    leftIndex = 0;
                    endIndex = rightIndex >> 1;
                }
                while (leftIndex != endIndex)
                {
                    var value = array[leftIndex];
                    array[leftIndex++] = array[--rightIndex];
                    array[rightIndex] = value;
                }
            }
        }

        /// <summary>
        ///     取子集合
        /// </summary>
        /// <param name="index">起始位置</param>
        /// <param name="count">数量,小于0表示所有</param>
        /// <returns>子集合</returns>
        public TmphCollection<TValueType> Sub(int index, int count = -1)
        {
            var range = new TmphArray.TmphRange(Count, index, count);
            if ((count = range.GetCount) > 0)
            {
                if ((StartIndex += range.SkipCount) >= array.Length) StartIndex -= array.Length;
                if ((EndIndex = StartIndex + count) >= array.Length) EndIndex -= array.Length;
                if (count != array.Length) IsFull = false;
            }
            else
            {
                StartIndex = EndIndex = 0;
                IsFull = false;
            }
            return this;
        }

        /// <summary>
        ///     取子集合
        /// </summary>
        /// <param name="index">起始位置</param>
        /// <param name="count">数量,小于0表示所有</param>
        /// <returns>子集合</returns>
        public TValueType[] GetSub(int index, int count)
        {
            var range = new TmphArray.TmphRange(Count, index, count);
            if ((count = range.GetCount) > 0)
            {
                var values = new TValueType[count];
                copyTo(index, values, count);
                return values;
            }
            return TmphNullValue<TValueType>.Array;
        }

        /// <summary>
        ///     获取匹配位置
        /// </summary>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>数组中的匹配位置,失败为-1</returns>
        protected override int indexOf(Func<TValueType, bool> isValue)
        {
            if (EndIndex > StartIndex)
            {
                for (var index = StartIndex; index != EndIndex; ++index)
                {
                    if (isValue(array[index])) return index;
                }
            }
            else if (IsFull || EndIndex < StartIndex)
            {
                for (var index = StartIndex; index != array.Length; ++index)
                {
                    if (isValue(array[index])) return index;
                }
                for (var index = 0; index != EndIndex; ++index)
                {
                    if (isValue(array[index])) return index;
                }
            }
            return -1;
        }

        /// <summary>
        ///     获取匹配位置
        /// </summary>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>匹配位置,失败为-1</returns>
        public override int IndexOf(Func<TValueType, bool> isValue)
        {
            if (isValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            var index = indexOf(isValue);
            if (index != -1)
            {
                if (index < StartIndex) index += array.Length;
                return index - StartIndex;
            }
            return -1;
        }

        /// <summary>
        ///     获取匹配值集合
        /// </summary>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>匹配值集合</returns>
        public TmphSubArray<TValueType> GetFind(Func<TValueType, bool> isValue)
        {
            if (isValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            var length = Count;
            if (length > 0)
            {
                var values = new TValueType[length < sizeof(int) ? sizeof(int) : length];
                length = 0;
                if (EndIndex > StartIndex)
                {
                    for (var index = StartIndex; index != EndIndex; ++index)
                    {
                        if (isValue(array[index])) values[length++] = array[index];
                    }
                }
                else
                {
                    for (var index = StartIndex; index != array.Length; ++index)
                    {
                        if (isValue(array[index])) values[length++] = array[index];
                    }
                    for (var index = 0; index != EndIndex; ++index)
                    {
                        if (isValue(array[index])) values[length++] = array[index];
                    }
                }
                return TmphSubArray<TValueType>.Unsafe(values, 0, length);
            }
            return default(TmphSubArray<TValueType>);
        }

        /// <summary>
        ///     获取匹配值集合
        /// </summary>
        /// <param name="isValue">数据匹配器</param>
        /// <param name="map">匹配结果位图</param>
        /// <returns>匹配值集合</returns>
        protected override TValueType[] getFindArray(Func<TValueType, bool> isValue, TmphFixedMap map)
        {
            var length = 0;
            if (EndIndex > StartIndex)
            {
                for (var index = StartIndex; index != EndIndex; ++index)
                {
                    if (isValue(array[index]))
                    {
                        ++length;
                        map.Set(index);
                    }
                }
            }
            else
            {
                for (var index = StartIndex; index != array.Length; ++index)
                {
                    if (isValue(array[index]))
                    {
                        ++length;
                        map.Set(index);
                    }
                }
                for (var index = 0; index != EndIndex; ++index)
                {
                    if (isValue(array[index]))
                    {
                        ++length;
                        map.Set(index);
                    }
                }
            }
            if (length != 0)
            {
                var values = new TValueType[length];
                length = 0;
                if (EndIndex > StartIndex)
                {
                    for (var index = StartIndex; index != EndIndex; ++index)
                    {
                        if (map.Get(index)) values[length++] = array[index];
                    }
                }
                else
                {
                    for (var index = StartIndex; index != array.Length; ++index)
                    {
                        if (map.Get(index)) values[length++] = array[index];
                    }
                    for (var index = 0; index != EndIndex; ++index)
                    {
                        if (map.Get(index)) values[length++] = array[index];
                    }
                }
                return values;
            }
            return TmphNullValue<TValueType>.Array;
        }

        /// <summary>
        ///     获取匹配数量
        /// </summary>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>匹配数量</returns>
        public int GetCount(Func<TValueType, bool> isValue)
        {
            if (isValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            var value = 0;
            if (EndIndex > StartIndex)
            {
                for (var index = StartIndex; index != EndIndex; ++index)
                {
                    if (isValue(array[index])) ++value;
                }
            }
            else if (IsFull || EndIndex < StartIndex)
            {
                for (var index = StartIndex; index != array.Length; ++index)
                {
                    if (isValue(array[index])) ++value;
                }
                for (var index = 0; index != EndIndex; ++index)
                {
                    if (isValue(array[index])) ++value;
                }
            }
            return value;
        }

        /// <summary>
        ///     移除所有前端匹配值
        /// </summary>
        /// <param name="isValue">数据匹配器</param>
        private void removeStart(Func<TValueType, bool> isValue)
        {
            if (EndIndex > StartIndex)
            {
                while (StartIndex != EndIndex && isValue(array[StartIndex])) StartIndex++;
                if (StartIndex == EndIndex) StartIndex = EndIndex = 0;
            }
            else if (IsFull || EndIndex < StartIndex)
            {
                while (StartIndex != array.Length && isValue(array[StartIndex])) StartIndex++;
                if (StartIndex == array.Length)
                {
                    for (IsFull = false, StartIndex = 0;
                        StartIndex != EndIndex && isValue(array[StartIndex]);
                        StartIndex++)
                        ;
                    if (StartIndex == EndIndex) StartIndex = EndIndex = 0;
                }
                else if (StartIndex != EndIndex) IsFull = false;
            }
        }

        /// <summary>
        ///     移除所有后端匹配值
        /// </summary>
        /// <param name="isValue">数据匹配器</param>
        private void removeEnd(Func<TValueType, bool> isValue)
        {
            if (EndIndex > StartIndex)
            {
                while (StartIndex != EndIndex && isValue(array[EndIndex - 1])) --EndIndex;
                if (StartIndex == EndIndex) StartIndex = EndIndex = 0;
            }
            else if (IsFull || EndIndex < StartIndex)
            {
                var endIndex = EndIndex;
                while (endIndex != 0 && isValue(array[--endIndex])) --EndIndex;
                if (EndIndex == 0)
                {
                    for (endIndex = array.Length; StartIndex != endIndex && isValue(array[--endIndex]); --EndIndex) ;
                    if (EndIndex != 0)
                    {
                        IsFull = false;
                        if (StartIndex == (EndIndex += array.Length)) StartIndex = EndIndex = 0;
                    }
                }
                if (StartIndex != EndIndex) IsFull = false;
            }
        }

        /// <summary>
        ///     移除匹配值
        /// </summary>
        /// <param name="isValue">数据匹配器</param>
        public void Remove(Func<TValueType, bool> isValue)
        {
            if (isValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            removeStart(isValue);
            removeEnd(isValue);
            var index = indexOf(isValue);
            if (index != -1)
            {
                int count = 1, writeIndex = index;
                if (EndIndex > StartIndex || index < StartIndex)
                {
                    while (++index != EndIndex)
                    {
                        if (isValue(array[index])) ++count;
                        else array[writeIndex++] = array[index];
                    }
                }
                else if (index >= StartIndex)
                {
                    while (++index != array.Length)
                    {
                        if (isValue(array[index])) ++count;
                        else array[writeIndex++] = array[index];
                    }
                    for (index = 0; index != EndIndex; ++index)
                    {
                        if (isValue(array[index])) ++count;
                        else
                        {
                            array[writeIndex++] = array[index];
                            if (writeIndex == array.Length) writeIndex = 0;
                        }
                    }
                }
                if ((EndIndex -= count) < 0) EndIndex += array.Length;
                IsFull = false;
            }
        }

        /// <summary>
        ///     排序
        /// </summary>
        /// <param name="comparer">比较器</param>
        public void Sort(Func<TValueType, TValueType, int> comparer)
        {
            if (StartIndex != EndIndex)
            {
                if (EndIndex < StartIndex) setLength(array.Length);
                array.sort(comparer, StartIndex, EndIndex - StartIndex);
            }
            else if (IsFull)
            {
                if (StartIndex != 0) array = GetArray();
                StartIndex = EndIndex = 0;
                array.sort(comparer);
            }
        }

        /// <summary>
        ///     获取数据范围
        /// </summary>
        /// <typeparam name="arrayType">目标数据类型</typeparam>
        /// <param name="index">起始位置</param>
        /// <param name="count">数量</param>
        /// <param name="getValue">数据转换器</param>
        /// <returns>目标数据</returns>
        protected override arrayType[] getRange<arrayType>(int index, int count, Func<TValueType, arrayType> getValue)
        {
            var values = new arrayType[count];
            var length = 0;
            if (EndIndex > StartIndex)
            {
                for (var endIndex = (index += StartIndex) + count; index != endIndex; ++index)
                    values[length++] = getValue(array[index]);
            }
            else if ((index += StartIndex) >= array.Length)
            {
                for (var endIndex = (index -= array.Length) + count; index != endIndex; ++index)
                    values[length++] = getValue(array[index]);
            }
            else
            {
                var endCount = array.Length - index;
                if (endCount >= count)
                {
                    for (var endIndex = index + count; index != endIndex; ++index)
                        values[length++] = getValue(array[index]);
                }
                else
                {
                    while (index != array.Length) values[length++] = getValue(array[index++]);
                    for (index = 0, count -= endCount; index != count; ++index)
                        values[length++] = getValue(array[index]);
                }
            }
            return values;
        }

        /// <summary>
        ///     获取数据排序范围
        /// </summary>
        /// <param name="comparer">比较器</param>
        /// <param name="skipCount">跳过记录数</param>
        /// <param name="getCount">获取记录数</param>
        /// <returns>排序数据</returns>
        public TmphSubArray<TValueType> RangeSort(Func<TValueType, TValueType, int> comparer, int skipCount, int getCount)
        {
            if (StartIndex != EndIndex)
            {
                if (EndIndex < StartIndex) setLength(array.Length);
                return array.rangeSort(StartIndex, EndIndex - StartIndex, comparer, skipCount, getCount);
            }
            if (IsFull)
            {
                if (StartIndex != 0) array = GetArray();
                StartIndex = EndIndex = 0;
                return array.rangeSort(comparer, skipCount, getCount);
            }
            return default(TmphSubArray<TValueType>);
        }

        /// <summary>
        ///     获取排序数据分页
        /// </summary>
        /// <param name="comparer">比较器</param>
        /// <param name="pageSize">分页尺寸</param>
        /// <param name="currentPage">页号</param>
        /// <returns>排序数据</returns>
        public TmphSubArray<TValueType> PageSort(Func<TValueType, TValueType, int> comparer, int pageSize, int currentPage)
        {
            if (StartIndex != EndIndex)
            {
                var page = new TmphArray.TmphPage(Count, pageSize, currentPage);
                if (EndIndex < StartIndex) setLength(array.Length);
                return array.rangeSort(StartIndex, EndIndex - StartIndex, comparer, page.SkipCount, page.CurrentPageSize);
            }
            if (IsFull)
            {
                var page = new TmphArray.TmphPage(array.Length, pageSize, currentPage);
                if (StartIndex != 0) array = GetArray();
                StartIndex = EndIndex = 0;
                return array.rangeSort(comparer, page.SkipCount, page.CurrentPageSize);
            }
            return default(TmphSubArray<TValueType>);
        }

        /// <summary>
        ///     获取最大值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="comparer">比较器</param>
        /// <param name="value">最大值</param>
        /// <returns>是否存在最大值</returns>
        public override bool Max(Func<TValueType, TValueType, int> comparer, out TValueType value)
        {
            if (Count != 0)
            {
                value = array[StartIndex];
                if (Count != 1)
                {
                    if (comparer == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                    if (EndIndex > StartIndex)
                    {
                        for (var index = StartIndex; ++index != EndIndex;)
                        {
                            if (comparer(array[index], value) > 0) value = array[index];
                        }
                    }
                    else
                    {
                        for (var index = StartIndex; ++index != array.Length;)
                        {
                            if (comparer(array[index], value) > 0) value = array[index];
                        }
                        for (var index = 0; index != EndIndex; ++index)
                        {
                            if (comparer(array[index], value) > 0) value = array[index];
                        }
                    }
                }
                return true;
            }
            value = default(TValueType);
            return false;
        }

        /// <summary>
        ///     获取最大值
        /// </summary>
        /// <typeparam name="TKeyType">比较键类型</typeparam>
        /// <param name="getKey">获取键值</param>
        /// <param name="comparer">比较器</param>
        /// <param name="value">最大值</param>
        /// <returns>是否存在最大值</returns>
        public override bool Max<TKeyType>
            (Func<TValueType, TKeyType> getKey, Func<TKeyType, TKeyType, int> comparer, out TValueType value)
        {
            if (Count != 0)
            {
                value = array[StartIndex];
                if (Count != 1)
                {
                    if (getKey == null || comparer == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                    var key = getKey(value);
                    if (EndIndex > StartIndex)
                    {
                        for (var index = StartIndex; ++index != EndIndex;)
                        {
                            var nextKey = getKey(array[index]);
                            if (comparer(nextKey, key) > 0)
                            {
                                value = array[index];
                                key = nextKey;
                            }
                        }
                    }
                    else
                    {
                        for (var index = StartIndex; ++index != array.Length;)
                        {
                            var nextKey = getKey(array[index]);
                            if (comparer(nextKey, key) > 0)
                            {
                                value = array[index];
                                key = nextKey;
                            }
                        }
                        for (var index = 0; index != EndIndex; ++index)
                        {
                            var nextKey = getKey(array[index]);
                            if (comparer(nextKey, key) > 0)
                            {
                                value = array[index];
                                key = nextKey;
                            }
                        }
                    }
                }
                return true;
            }
            value = default(TValueType);
            return false;
        }

        /// <summary>
        ///     获取最小值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="comparer">比较器</param>
        /// <param name="value">最小值</param>
        /// <returns>是否存在最小值</returns>
        public override bool Min(Func<TValueType, TValueType, int> comparer, out TValueType value)
        {
            if (Count != 0)
            {
                value = array[StartIndex];
                if (Count != 1)
                {
                    if (comparer == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                    if (EndIndex > StartIndex)
                    {
                        for (var index = StartIndex; ++index != EndIndex;)
                        {
                            if (comparer(array[index], value) < 0) value = array[index];
                        }
                    }
                    else
                    {
                        for (var index = StartIndex; ++index != array.Length;)
                        {
                            if (comparer(array[index], value) < 0) value = array[index];
                        }
                        for (var index = 0; index != EndIndex; ++index)
                        {
                            if (comparer(array[index], value) < 0) value = array[index];
                        }
                    }
                }
                return true;
            }
            value = default(TValueType);
            return false;
        }

        /// <summary>
        ///     获取最小值
        /// </summary>
        /// <typeparam name="TKeyType">比较键类型</typeparam>
        /// <param name="getKey">获取键值</param>
        /// <param name="comparer">比较器</param>
        /// <param name="value">最小值</param>
        /// <returns>是否存在最小值</returns>
        public override bool Min<TKeyType>
            (Func<TValueType, TKeyType> getKey, Func<TKeyType, TKeyType, int> comparer, out TValueType value)
        {
            if (Count != 0)
            {
                value = array[StartIndex];
                if (Count != 1)
                {
                    if (getKey == null || comparer == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                    var key = getKey(value);
                    if (EndIndex > StartIndex)
                    {
                        for (var index = StartIndex; ++index != EndIndex;)
                        {
                            var nextKey = getKey(array[index]);
                            if (comparer(nextKey, key) < 0)
                            {
                                value = array[index];
                                key = nextKey;
                            }
                        }
                    }
                    else
                    {
                        for (var index = StartIndex; ++index != array.Length;)
                        {
                            var nextKey = getKey(array[index]);
                            if (comparer(nextKey, key) < 0)
                            {
                                value = array[index];
                                key = nextKey;
                            }
                        }
                        for (var index = 0; index != EndIndex; ++index)
                        {
                            var nextKey = getKey(array[index]);
                            if (comparer(nextKey, key) < 0)
                            {
                                value = array[index];
                                key = nextKey;
                            }
                        }
                    }
                }
                return true;
            }
            value = default(TValueType);
            return false;
        }

        /// <summary>
        ///     数据分组
        /// </summary>
        /// <typeparam name="TKeyType">键类型</typeparam>
        /// <param name="getKey">键值获取器</param>
        /// <returns>分组数据</returns>
        public Dictionary<TKeyType, TmphList<TValueType>> Group<TKeyType>(Func<TValueType, TKeyType> getKey)
            where TKeyType : IEquatable<TKeyType>
        {
            if (getKey == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            if (Count != 0)
            {
                var newValues = TmphDictionary<TKeyType>.Create<TmphList<TValueType>>(Count);
                TmphList<TValueType> value;
                if (EndIndex > StartIndex)
                {
                    for (var index = StartIndex; index != EndIndex; ++index)
                    {
                        var key = getKey(array[index]);
                        if (!newValues.TryGetValue(key, out value)) newValues[key] = value = new TmphList<TValueType>();
                        value.Add(array[index]);
                    }
                }
                else if (IsFull || EndIndex < StartIndex)
                {
                    for (var index = StartIndex; index != array.Length; ++index)
                    {
                        var key = getKey(array[index]);
                        if (!newValues.TryGetValue(key, out value)) newValues[key] = value = new TmphList<TValueType>();
                        value.Add(array[index]);
                    }
                    for (var index = 0; index != EndIndex; ++index)
                    {
                        var key = getKey(array[index]);
                        if (!newValues.TryGetValue(key, out value)) newValues[key] = value = new TmphList<TValueType>();
                        value.Add(array[index]);
                    }
                }
                return newValues;
            }
            return TmphDictionary<TKeyType>.Create<TmphList<TValueType>>();
        }

        /// <summary>
        ///     数据去重
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="arrayType">目标数据类型</typeparam>
        /// <param name="getValue">目标数据获取器</param>
        /// <returns>目标集合</returns>
        public TmphSubArray<arrayType> Distinct<arrayType>(Func<TValueType, arrayType> getValue)
        {
            var newValues = new arrayType[Count];
            var count = 0;
            TValueType value;
            if (EndIndex > StartIndex)
            {
                var hash = TmphHashSet<TValueType>.Create();
                for (var index = StartIndex; index != EndIndex;)
                {
                    if (!hash.Contains(value = array[index++]))
                    {
                        newValues[count++] = getValue(value);
                        hash.Add(value);
                    }
                }
            }
            else if (IsFull || EndIndex < StartIndex)
            {
                var hash = TmphHashSet<TValueType>.Create();
                for (var index = StartIndex; index != array.Length;)
                {
                    if (!hash.Contains(value = array[index++]))
                    {
                        newValues[count++] = getValue(value);
                        hash.Add(value);
                    }
                }
                for (var index = 0; index != EndIndex;)
                {
                    if (!hash.Contains(value = array[index++]))
                    {
                        newValues[count++] = getValue(value);
                        hash.Add(value);
                    }
                }
            }
            return TmphSubArray<arrayType>.Unsafe(newValues, 0, count);
        }

        /// <summary>
        ///     非安全访问双向动态数组(请自行确保数据可靠性)
        /// </summary>
        public struct TmphUnsafer
        {
            /// <summary>
            ///     双向动态数组
            /// </summary>
            public TmphCollection<TValueType> Collection;

            /// <summary>
            ///     数据数组
            /// </summary>
            public TValueType[] Array
            {
                get { return Collection.array; }
            }

            /// <summary>
            ///     设置或获取值
            /// </summary>
            /// <param name="index">位置</param>
            /// <returns>数据值</returns>
            public TValueType this[int index]
            {
                get
                {
                    var array = Array;
                    index += Collection.StartIndex;
                    return array[index < array.Length ? index : (index - array.Length)];
                }
                set
                {
                    var array = Array;
                    index += Collection.StartIndex;
                    array[index < array.Length ? index : (index - array.Length)] = value;
                }
            }

            /// <summary>
            ///     获取第一个值
            /// </summary>
            /// <returns>第一个值</returns>
            public TValueType FirstOrDefault()
            {
                return Array[Collection.StartIndex];
            }

            /// <summary>
            ///     获取最后一个值
            /// </summary>
            /// <returns>最后一个值</returns>
            public TValueType LastOrDefault()
            {
                return Array[(Collection.EndIndex != 0 ? Collection.EndIndex : Array.Length) - 1];
            }

            /// <summary>
            ///     添加数据
            /// </summary>
            /// <param name="value">数据</param>
            public void Add(TValueType value)
            {
                Array[Collection.EndIndex] = value;
                if (++Collection.EndIndex == Array.Length) Collection.EndIndex = 0;
                if (Collection.StartIndex == Collection.EndIndex) Collection.IsFull = true;
            }

            /// <summary>
            ///     向前端添加一个数据
            /// </summary>
            /// <param name="value">数据</param>
            public void AddExpand(TValueType value)
            {
                if (Collection.StartIndex == 0) Collection.StartIndex = Array.Length;
                Array[--Collection.StartIndex] = value;
                if (Collection.StartIndex == Collection.EndIndex) Collection.IsFull = true;
            }

            /// <summary>
            ///     弹出最后一个数据
            /// </summary>
            /// <returns>最后一个数据</returns>
            public TValueType Pop()
            {
                if (Collection.EndIndex == 0) Collection.EndIndex = Array.Length;
                var value = Array[--Collection.EndIndex];
                Collection.IsFull = false;
                return value;
            }

            /// <summary>
            ///     弹出第一个数据
            /// </summary>
            /// <returns>第一个数据</returns>
            public TValueType PopExpand()
            {
                return Collection.popExpand();
            }

            /// <summary>
            ///     弹出第一个数据
            /// </summary>
            /// <returns>第一个数据</returns>
            public TValueType PopExpandReset()
            {
                var value = Array[Collection.StartIndex];
                Array[Collection.StartIndex] = default(TValueType);
                if (++Collection.StartIndex == Array.Length) Collection.StartIndex = 0;
                Collection.IsFull = false;
                return value;
            }

            /// <summary>
            ///     取子集合
            /// </summary>
            /// <param name="index">起始位置</param>
            /// <returns>子集合</returns>
            public TmphCollection<TValueType> Sub(int index)
            {
                if (index != 0)
                {
                    if ((Collection.StartIndex += index) >= Array.Length) Collection.StartIndex -= Array.Length;
                    Collection.IsFull = false;
                }
                return Collection;
            }

            /// <summary>
            ///     取子集合
            /// </summary>
            /// <param name="index">起始位置</param>
            /// <param name="count">数量,小于0表示所有</param>
            /// <returns>子集合</returns>
            public TmphCollection<TValueType> Sub(int index, int count)
            {
                var array = Array;
                if ((Collection.StartIndex += index) >= array.Length) Collection.StartIndex -= array.Length;
                if ((Collection.EndIndex = Collection.StartIndex + count) >= array.Length)
                    Collection.EndIndex -= array.Length;
                if (count != array.Length) Collection.IsFull = false;
                return Collection;
            }
        }

        /// <summary>
        ///     枚举器
        /// </summary>
        private sealed class TmphEnumerator : IEnumerator<TValueType>
        {
            /// <summary>
            ///     结束位置
            /// </summary>
            private readonly int endIndex;

            /// <summary>
            ///     起始位置
            /// </summary>
            private readonly int startIndex;

            /// <summary>
            ///     被枚举数组
            /// </summary>
            private TValueType[] array;

            /// <summary>
            ///     当前结束位置
            /// </summary>
            private int currentEndIndex;

            /// <summary>
            ///     当前位置
            /// </summary>
            private int currentIndex;

            /// <summary>
            ///     枚举器
            /// </summary>
            /// <param name="value">双向动态数组</param>
            public TmphEnumerator(TmphCollection<TValueType> value)
            {
                array = value.array;
                startIndex = value.StartIndex;
                endIndex = value.EndIndex;
                Reset();
            }

            /// <summary>
            ///     当前数据元素
            /// </summary>
            TValueType IEnumerator<TValueType>.Current
            {
                get { return array[currentIndex]; }
            }

            /// <summary>
            ///     当前数据元素
            /// </summary>
            object IEnumerator.Current
            {
                get { return array[currentIndex]; }
            }

            /// <summary>
            ///     转到下一个数据元素
            /// </summary>
            /// <returns>是否存在下一个数据元素</returns>
            public bool MoveNext()
            {
                if (++currentIndex == currentEndIndex)
                {
                    if (currentEndIndex == endIndex)
                    {
                        --currentIndex;
                        return false;
                    }
                    currentIndex = 0;
                    currentEndIndex = endIndex;
                }
                return true;
            }

            /// <summary>
            ///     重置枚举器状态
            /// </summary>
            public void Reset()
            {
                currentEndIndex = array.Length;
                currentIndex = startIndex - 1;
            }

            /// <summary>
            ///     释放枚举器
            /// </summary>
            public void Dispose()
            {
                array = null;
            }
        }
    }

    /// <summary>
    ///     双向动态数组扩展
    /// </summary>
    public static class TmphCollection
    {
        /// <summary>
        ///     长度设为0
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="arrayType">目标数据类型</typeparam>
        /// <returns>双向动态数组</returns>
        public static TmphCollection<TValueType> empty<TValueType>(this TmphCollection<TValueType> collection)
        {
            if (collection != null) collection.Empty();
            return collection;
        }

        /// <summary>
        ///     清除所有数据
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="arrayType">目标数据类型</typeparam>
        /// <returns>双向动态数组</returns>
        public static TmphCollection<TValueType> clear<TValueType>(this TmphCollection<TValueType> collection)
        {
            if (collection != null) collection.Clear();
            return collection;
        }

        /// <summary>
        ///     获取第一个值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="collection">双向动态数组</param>
        /// <returns>第一个值,失败为default(TValueType)</returns>
        public static TValueType firstOrDefault<TValueType>(this TmphCollection<TValueType> collection)
        {
            return collection != null ? collection.FirstOrDefault() : default(TValueType);
        }

        /// <summary>
        ///     获取最后一个值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="collection">双向动态数组</param>
        /// <returns>最后一个值,失败为default(TValueType)</returns>
        public static TValueType lastOrDefault<TValueType>(this TmphCollection<TValueType> collection)
        {
            return collection != null ? collection.LastOrDefault() : default(TValueType);
        }

        /// <summary>
        ///     添加数据
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="collection">双向动态数组</param>
        /// <param name="value">数据</param>
        /// <returns>双向动态数组</returns>
        public static TmphCollection<TValueType> add<TValueType>(this TmphCollection<TValueType> collection, TValueType value)
        {
            if (collection == null) collection = new TmphCollection<TValueType>();
            collection.Add(value);
            return collection;
        }

        /// <summary>
        ///     添加数据集合
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="collection">双向动态数组</param>
        /// <param name="values">数据集合</param>
        /// <returns>双向动态数组</returns>
        public static TmphCollection<TValueType> add<TValueType>(this TmphCollection<TValueType> collection,
            IEnumerable<TValueType> values)
        {
            if (collection != null)
            {
                collection.Add(values.getSubArray());
                return collection;
            }
            return values.getSubArray().ToCollection();
        }

        /// <summary>
        ///     添加数据集合
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="collection">双向动态数组</param>
        /// <param name="values">数据集合</param>
        /// <returns>双向动态数组</returns>
        public static TmphCollection<TValueType> add<TValueType>(this TmphCollection<TValueType> collection,
            ICollection<TValueType> values)
        {
            if (collection != null)
            {
                collection.Add(values.getSubArray());
                return collection;
            }
            return values.GetCollection();
        }

        /// <summary>
        ///     添加数据集合
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="collection">双向动态数组</param>
        /// <param name="values">数据集合</param>
        /// <returns>双向动态数组</returns>
        public static TmphCollection<TValueType> add<TValueType>(this TmphCollection<TValueType> collection, TValueType[] values)
        {
            if (collection != null)
            {
                collection.Add(values);
                return collection;
            }
            return new TmphCollection<TValueType>(values, false);
        }

        /// <summary>
        ///     添加数据集合
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="collection">双向动态数组</param>
        /// <param name="values">数据集合</param>
        /// <param name="index">起始位置</param>
        /// <param name="count">数量</param>
        /// <returns>双向动态数组</returns>
        public static TmphCollection<TValueType> add<TValueType>(this TmphCollection<TValueType> collection, TValueType[] values,
            int index, int count)
        {
            if (collection != null)
            {
                collection.Add(values, index, count);
                return collection;
            }
            return new TmphCollection<TValueType>(values, index, count, false);
        }

        /// <summary>
        ///     添加数据集合
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="collection">双向动态数组</param>
        /// <param name="values">数据集合</param>
        /// <returns>双向动态数组</returns>
        public static TmphCollection<TValueType> add<TValueType>(this TmphCollection<TValueType> collection,
            TmphSubArray<TValueType> values)
        {
            if (collection != null)
            {
                collection.Add(values.Array, values.StartIndex, values.Count);
                return collection;
            }
            return new TmphCollection<TValueType>(values.Array, values.StartIndex, values.Count, false);
        }

        /// <summary>
        ///     添加数据集合
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="collection">双向动态数组</param>
        /// <param name="value">数据集合</param>
        /// <returns>双向动态数组</returns>
        public static TmphCollection<TValueType> add<TValueType>(this TmphCollection<TValueType> collection,
            TmphCollection<TValueType> value)
        {
            if (collection != null)
            {
                collection.Add(value);
                return collection;
            }
            return new TmphCollection<TValueType>(value);
        }

        /// <summary>
        ///     添加数据集合
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="collection">双向动态数组</param>
        /// <param name="value">数据集合</param>
        /// <param name="index">起始位置</param>
        /// <param name="count">数量</param>
        /// <returns>双向动态数组</returns>
        public static TmphCollection<TValueType> add<TValueType>
            (this TmphCollection<TValueType> collection, TmphCollection<TValueType> value, int index, int count)
        {
            if (collection != null)
            {
                collection.Add(value, index, count);
                return collection;
            }
            return new TmphCollection<TValueType>(value.getArray(), index, count, true);
        }

        /// <summary>
        ///     添加数据集合
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="collection">双向动态数组</param>
        /// <param name="value">数据集合</param>
        /// <returns>双向动态数组</returns>
        public static TmphCollection<TValueType> add<TValueType>(this TmphCollection<TValueType> collection,
            TmphList<TValueType> value)
        {
            if (collection != null)
            {
                collection.Add(value);
                return collection;
            }
            return new TmphCollection<TValueType>(value.getArray(), true);
        }

        /// <summary>
        ///     添加数据集合
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="collection">双向动态数组</param>
        /// <param name="value">数据集合</param>
        /// <param name="index">起始位置</param>
        /// <param name="count">数量</param>
        /// <returns>双向动态数组</returns>
        public static TmphCollection<TValueType> add<TValueType>(this TmphCollection<TValueType> collection,
            TmphList<TValueType> value, int index, int count)
        {
            if (collection != null)
            {
                collection.Add(value, index, count);
                return collection;
            }
            return new TmphCollection<TValueType>(value.getArray(), index, count, true);
        }

        /// <summary>
        ///     向前端添加一个数据
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="collection">双向动态数组</param>
        /// <param name="value">数据</param>
        /// <returns>双向动态数组</returns>
        public static TmphCollection<TValueType> addExpand<TValueType>(this TmphCollection<TValueType> collection,
            TValueType value)
        {
            if (collection == null) collection = new TmphCollection<TValueType>();
            collection.AddExpand(value);
            return collection;
        }

        /// <summary>
        ///     插入数据
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="collection">双向动态数组</param>
        /// <param name="index">插入位置</param>
        /// <param name="value">数据</param>
        /// <returns>双向动态数组</returns>
        public static TmphCollection<TValueType> insert<TValueType>(this TmphCollection<TValueType> collection, int index,
            TValueType value)
        {
            if (collection != null)
            {
                collection.Insert(index, value);
                return collection;
            }
            if (index == 0) return collection.add(value);
            TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
            return null;
        }

        /// <summary>
        ///     插入数据集合
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="collection">双向动态数组</param>
        /// <param name="index">插入位置</param>
        /// <param name="values">数据集合</param>
        /// <returns>双向动态数组</returns>
        public static TmphCollection<TValueType> insert<TValueType>(this TmphCollection<TValueType> collection, int index,
            TmphCollection<TValueType> values)
        {
            if (collection != null)
            {
                collection.Insert(index, values);
                return collection;
            }
            if (index == 0) return new TmphCollection<TValueType>(values);
            TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
            return null;
        }

        /// <summary>
        ///     移除数据
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="collection">双向动态数组</param>
        /// <param name="value">数据</param>
        /// <returns>双向动态数组</returns>
        public static TmphCollection<TValueType> removeFirst<TValueType>(this TmphCollection<TValueType> collection,
            TValueType value)
        {
            if (collection != null) collection.Remove(value);
            return collection;
        }

        /// <summary>
        ///     移除数据
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="collection">双向动态数组</param>
        /// <param name="index">数据位置</param>
        /// <returns>被移除数据</returns>
        public static TValueType removeAt<TValueType>(this TmphCollection<TValueType> collection, int index)
        {
            if (collection != null) return collection.GetRemoveAt(index);
            TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
            return default(TValueType);
        }

        /// <summary>
        ///     移除数据范围
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="collection">双向动态数组</param>
        /// <param name="index">起始位置</param>
        /// <returns>双向动态数组</returns>
        public static TmphCollection<TValueType> removeRange<TValueType>(this TmphCollection<TValueType> collection, int index)
        {
            if (collection != null)
            {
                collection.RemoveRange(index);
                return collection;
            }
            TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
            return null;
        }

        /// <summary>
        ///     移除数据范围
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="collection">双向动态数组</param>
        /// <param name="index">起始位置</param>
        /// <param name="count">移除数量</param>
        /// <returns>双向动态数组</returns>
        public static TmphCollection<TValueType> removeRange<TValueType>(this TmphCollection<TValueType> collection, int index,
            int count)
        {
            if (collection != null)
            {
                collection.RemoveRange(index, count);
                return collection;
            }
            if (count != 0) return null;
            TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
            return null;
        }

        /// <summary>
        ///     弹出最后一个数据
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="collection">双向动态数组</param>
        /// <returns>最后一个数据</returns>
        public static TValueType pop<TValueType>(this TmphCollection<TValueType> collection)
        {
            if (collection != null) return collection.Pop();
            TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
            return default(TValueType);
        }

        /// <summary>
        ///     弹出第一个数据
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="collection">双向动态数组</param>
        /// <returns>第一个数据</returns>
        public static TValueType popExpand<TValueType>(this TmphCollection<TValueType> collection)
        {
            if (collection != null) return collection.PopExpand();
            TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
            return default(TValueType);
        }

        /// <summary>
        ///     复制数据
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="collection">双向动态数组</param>
        /// <param name="values">目标数据</param>
        /// <param name="index">目标位置</param>
        /// <returns>目标数组</returns>
        public static TValueType[] copyTo<TValueType>(this TmphCollection<TValueType> collection, TValueType[] values,
            int index)
        {
            if (collection != null) collection.CopyTo(values, index);
            return values;
        }

        /// <summary>
        ///     转换数组
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="collection">双向动态数组</param>
        /// <returns>数组</returns>
        public static TValueType[] getArray<TValueType>(this TmphCollection<TValueType> collection)
        {
            return collection != null ? collection.GetArray() : TmphNullValue<TValueType>.Array;
        }

        /// <summary>
        ///     转换数组
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="collection">双向动态数组</param>
        /// <returns>数组</returns>
        public static TValueType[] toArray<TValueType>(this TmphCollection<TValueType> collection)
        {
            return collection != null ? collection.ToArray() : null;
        }

        /// <summary>
        ///     转换数据集合
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="arrayType">目标数据类型</typeparam>
        /// <param name="collection">双向动态数组</param>
        /// <param name="getValue">数据转换器</param>
        /// <returns>数据集合</returns>
        public static arrayType[] getArray<TValueType, arrayType>(this TmphCollection<TValueType> collection,
            Func<TValueType, arrayType> getValue)
        {
            return collection != null ? collection.GetArray(getValue) : TmphNullValue<arrayType>.Array;
        }

        /// <summary>
        ///     转换键值对数组
        /// </summary>
        /// <typeparam name="TKeyType">键类型</typeparam>
        /// <typeparam name="TValueType">值类型</typeparam>
        /// <param name="collection">双向动态数组</param>
        /// <param name="getKey">键值获取器</param>
        /// <returns>键值对数组</returns>
        public static TmphKeyValue<TKeyType, TValueType>[] getKeyValueArray<TKeyType, TValueType>(
            this TmphCollection<TValueType> collection, Func<TValueType, TKeyType> getKey)
        {
            return collection != null
                ? collection.GetKeyValueArray(getKey)
                : TmphNullValue<TmphKeyValue<TKeyType, TValueType>>.Array;
        }

        /// <summary>
        ///     转换单向动态数组
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="collection">双向动态数组</param>
        /// <returns>单向动态数组</returns>
        public static TmphList<TValueType> toList<TValueType>(this TmphCollection<TValueType> collection)
        {
            return collection != null ? collection.ToList() : null;
        }

        /// <summary>
        ///     逆转列表
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="arrayType">目标数据类型</typeparam>
        /// <returns>双向动态数组</returns>
        public static TmphCollection<TValueType> reverse<TValueType>(this TmphCollection<TValueType> collection)
        {
            if (collection != null) collection.Reverse();
            return collection;
        }

        /// <summary>
        ///     取子集合
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="collection">双向动态数组</param>
        /// <param name="index">起始位置</param>
        /// <returns>子集合</returns>
        public static TmphCollection<TValueType> sub<TValueType>(this TmphCollection<TValueType> collection, int index)
        {
            return collection != null ? collection.Sub(index) : null;
        }

        /// <summary>
        ///     取子集合
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="collection">双向动态数组</param>
        /// <param name="index">起始位置</param>
        /// <param name="count">数量,小于0表示所有</param>
        /// <returns>子集合</returns>
        public static TmphCollection<TValueType> sub<TValueType>(this TmphCollection<TValueType> collection, int index,
            int count)
        {
            return collection != null ? collection.Sub(index, count) : null;
        }

        /// <summary>
        ///     取子集合
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="collection">双向动态数组</param>
        /// <param name="index">起始位置</param>
        /// <param name="count">数量,小于0表示所有</param>
        /// <returns>子集合</returns>
        public static TValueType[] getSub<TValueType>(this TmphCollection<TValueType> collection, int index, int count)
        {
            return collection != null ? collection.GetSub(index, count) : null;
        }

        /// <summary>
        ///     获取匹配数据位置
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="collection">双向动态数组</param>
        /// <param name="value">匹配数据</param>
        /// <returns>匹配位置,失败为-1</returns>
        public static int indexOf<TValueType>(this TmphCollection<TValueType> collection, TValueType value)
        {
            return collection != null ? collection.IndexOf(value) : -1;
        }

        /// <summary>
        ///     判断是否存在匹配
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="collection">双向动态数组</param>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>是否存在匹配</returns>
        public static bool any<TValueType>(this TmphCollection<TValueType> collection, Func<TValueType, bool> isValue)
        {
            return collection != null && collection.Any(isValue);
        }

        /// <summary>
        ///     判断是否存在数据
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="collection">双向动态数组</param>
        /// <param name="value">匹配数据</param>
        /// <returns>是否存在数据</returns>
        public static bool contains<TValueType>(this TmphCollection<TValueType> collection, TValueType value)
        {
            return collection != null && collection.Contains(value);
        }

        /// <summary>
        ///     获取匹配位置
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="collection">双向动态数组</param>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>匹配位置,失败为-1</returns>
        public static int indexOf<TValueType>(this TmphCollection<TValueType> collection, Func<TValueType, bool> isValue)
        {
            return collection != null ? collection.IndexOf(isValue) : -1;
        }

        /// <summary>
        ///     获取第一个匹配值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="collection">双向动态数组</param>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>匹配值,失败为 default(TValueType)</returns>
        public static TValueType firstOrDefault<TValueType>(this TmphCollection<TValueType> collection,
            Func<TValueType, bool> isValue)
        {
            return collection != null ? collection.FirstOrDefault(isValue) : default(TValueType);
        }

        /// <summary>
        ///     获取匹配值集合
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="collection">双向动态数组</param>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>匹配值集合</returns>
        public static TmphSubArray<TValueType> getFind<TValueType>(this TmphCollection<TValueType> collection,
            Func<TValueType, bool> isValue)
        {
            return collection != null ? collection.GetFind(isValue) : default(TmphSubArray<TValueType>);
        }

        /// <summary>
        ///     获取匹配值集合
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="collection">双向动态数组</param>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>匹配值集合</returns>
        public static TValueType[] getFindArray<TValueType>(this TmphCollection<TValueType> collection,
            Func<TValueType, bool> isValue)
        {
            return collection != null ? collection.GetFindArray(isValue) : null;
        }

        /// <summary>
        ///     获取匹配数量
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="collection">双向动态数组</param>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>匹配数量</returns>
        public static int count<TValueType>(this TmphCollection<TValueType> collection, Func<TValueType, bool> isValue)
        {
            return collection != null ? collection.GetCount(isValue) : 0;
        }

        /// <summary>
        ///     遍历foreach
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="collection">双向动态数组</param>
        /// <param name="method">调用函数</param>
        /// <returns>双向动态数组</returns>
        public static TmphCollection<TValueType> each<TValueType>(this TmphCollection<TValueType> collection,
            Action<TValueType> method)
        {
            if (collection.Count() != 0)
            {
                if (method == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                var array = collection.array;
                int endIndex = collection.EndIndex, startIndex = collection.StartIndex;
                if (endIndex > startIndex)
                {
                    for (var index = startIndex; index != endIndex; method(array[index++])) ;
                }
                else if (collection.IsFull || endIndex < startIndex)
                {
                    for (var index = startIndex; index != array.Length; method(array[index++])) ;
                    for (var index = 0; index != endIndex; method(array[index++])) ;
                }
            }
            return collection;
        }

        /// <summary>
        ///     移除匹配值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="collection">双向动态数组</param>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>双向动态数组</returns>
        public static TmphCollection<TValueType> removeFirst<TValueType>(this TmphCollection<TValueType> collection,
            Func<TValueType, bool> isValue)
        {
            if (collection != null) collection.RemoveFirst(isValue);
            return null;
        }

        /// <summary>
        ///     移除匹配值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="collection">双向动态数组</param>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>被移除的数据元素,失败返回default(TValueType)</returns>
        public static TValueType getRemoveFirst<TValueType>(this TmphCollection<TValueType> collection,
            Func<TValueType, bool> isValue)
        {
            return collection != null ? collection.GetRemoveFirst(isValue) : default(TValueType);
        }

        /// <summary>
        ///     移除匹配值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="collection">双向动态数组</param>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>双向动态数组</returns>
        public static TmphCollection<TValueType> remove<TValueType>(this TmphCollection<TValueType> collection,
            Func<TValueType, bool> isValue)
        {
            if (collection != null) collection.Remove(isValue);
            return collection;
        }

        /// <summary>
        ///     替换第一个匹配值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="collection">双向动态数组</param>
        /// <param name="value">新数据元素</param>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>双向动态数组</returns>
        public static TmphCollection<TValueType> replaceFirst<TValueType>
            (this TmphCollection<TValueType> collection, TValueType value, Func<TValueType, bool> isValue)
        {
            if (collection != null) collection.ReplaceFirst(value, isValue);
            return collection;
        }

        /// <summary>
        ///     排序
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="collection">双向动态数组</param>
        /// <param name="comparer">比较器</param>
        /// <returns>双向动态数组</returns>
        public static TmphCollection<TValueType> sort<TValueType>(this TmphCollection<TValueType> collection,
            Func<TValueType, TValueType, int> comparer)
        {
            if (collection != null) collection.Sort(comparer);
            return collection;
        }

        /// <summary>
        ///     获取数据分页
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="arrayType">目标数据类型</typeparam>
        /// <param name="collection">双向动态数组</param>
        /// <param name="pageSize">分页尺寸</param>
        /// <param name="currentPage">页号</param>
        /// <param name="getValue">数据转换器</param>
        /// <returns>目标数据</returns>
        public static arrayType[] getPage<TValueType, arrayType>
            (this TmphCollection<TValueType> collection, int pageSize, int currentPage, Func<TValueType, arrayType> getValue)
        {
            return collection != null ? collection.GetPage(pageSize, currentPage, getValue) : null;
        }

        /// <summary>
        ///     获取数据排序范围
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="collection">双向动态数组</param>
        /// <param name="comparer">比较器</param>
        /// <param name="skipCount">跳过记录数</param>
        /// <param name="getCount">获取记录数</param>
        /// <returns>排序数据</returns>
        public static TmphSubArray<TValueType> rangeSort<TValueType>
            (this TmphCollection<TValueType> collection, Func<TValueType, TValueType, int> comparer, int skipCount,
                int getCount)
        {
            return collection != null
                ? collection.RangeSort(comparer, skipCount, getCount)
                : default(TmphSubArray<TValueType>);
        }

        /// <summary>
        ///     获取排序数据分页
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="collection">双向动态数组</param>
        /// <param name="comparer">比较器</param>
        /// <param name="pageSize">分页尺寸</param>
        /// <param name="currentPage">页号</param>
        /// <returns>排序数据</returns>
        public static TmphSubArray<TValueType> pageSort<TValueType>
            (this TmphCollection<TValueType> collection, Func<TValueType, TValueType, int> comparer, int pageSize,
                int currentPage)
        {
            return collection != null
                ? collection.PageSort(comparer, pageSize, currentPage)
                : default(TmphSubArray<TValueType>);
        }

        /// <summary>
        ///     获取最大值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="collection">数组数据</param>
        /// <param name="comparer">比较器</param>
        /// <param name="value">最大值</param>
        /// <returns>是否存在最大值</returns>
        public static bool max<TValueType>(this TmphCollection<TValueType> collection,
            Func<TValueType, TValueType, int> comparer, out TValueType value)
        {
            if (collection != null) return collection.Max(comparer, out value);
            value = default(TValueType);
            return false;
        }

        /// <summary>
        ///     获取最大值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TKeyType">比较键类型</typeparam>
        /// <param name="collection">数组数据</param>
        /// <param name="getKey">获取键值</param>
        /// <param name="comparer">比较器</param>
        /// <param name="value">最大值</param>
        /// <returns>是否存在最大值</returns>
        public static bool max<TValueType, TKeyType>
            (this TmphCollection<TValueType> collection, Func<TValueType, TKeyType> getKey,
                Func<TKeyType, TKeyType, int> comparer, out TValueType value)
        {
            if (collection != null) collection.Max(getKey, comparer, out value);
            value = default(TValueType);
            return false;
        }

        /// <summary>
        ///     获取最大值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="collection">数组数据</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最大值,失败返回默认空值</returns>
        public static TValueType max<TValueType>(this TmphCollection<TValueType> collection, TValueType nullValue)
            where TValueType : IComparable<TValueType>
        {
            if (collection != null)
            {
                TValueType value;
                if (collection.Max((left, right) => left.CompareTo(right), out value)) return value;
            }
            return nullValue;
        }

        /// <summary>
        ///     获取最大值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TKeyType">比较键类型</typeparam>
        /// <param name="collection">数组数据</param>
        /// <param name="getKey">获取键值</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最大值,失败返回默认空值</returns>
        public static TValueType max<TValueType, TKeyType>(this TmphCollection<TValueType> collection,
            Func<TValueType, TKeyType> getKey, TValueType nullValue)
            where TKeyType : IComparable<TKeyType>
        {
            return collection != null
                ? collection.Max(getKey, (left, right) => left.CompareTo(right), nullValue)
                : nullValue;
        }

        /// <summary>
        ///     获取最大值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TKeyType">比较键类型</typeparam>
        /// <param name="collection">数组数据</param>
        /// <param name="getKey">获取键值</param>
        /// <param name="comparer">比较器</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最大值,失败返回默认空值</returns>
        public static TValueType max<TValueType, TKeyType>
            (this TmphCollection<TValueType> collection, Func<TValueType, TKeyType> getKey,
                Func<TKeyType, TKeyType, int> comparer, TValueType nullValue)
        {
            return collection != null ? collection.Max(getKey, comparer, nullValue) : nullValue;
        }

        /// <summary>
        ///     获取最大键值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TKeyType">比较键类型</typeparam>
        /// <param name="collection">数组数据</param>
        /// <param name="getKey">获取键值</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最大键值,失败返回默认空值</returns>
        public static TKeyType maxKey<TValueType, TKeyType>(this TmphCollection<TValueType> collection,
            Func<TValueType, TKeyType> getKey, TKeyType nullValue)
            where TKeyType : IComparable<TKeyType>
        {
            return collection != null
                ? collection.MaxKey(getKey, (left, right) => left.CompareTo(right), nullValue)
                : nullValue;
        }

        /// <summary>
        ///     获取最大键值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TKeyType">比较键类型</typeparam>
        /// <param name="collection">数组数据</param>
        /// <param name="getKey">获取键值</param>
        /// <param name="comparer">比较器</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最大键值,失败返回默认空值</returns>
        public static TKeyType maxKey<TValueType, TKeyType>
            (this TmphCollection<TValueType> collection, Func<TValueType, TKeyType> getKey,
                Func<TKeyType, TKeyType, int> comparer, TKeyType nullValue)
        {
            return collection != null ? collection.MaxKey(getKey, comparer, nullValue) : nullValue;
        }

        /// <summary>
        ///     获取最小值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="collection">数组数据</param>
        /// <param name="comparer">比较器</param>
        /// <param name="value">最小值</param>
        /// <returns>是否存在最小值</returns>
        public static bool min<TValueType>(this TmphCollection<TValueType> collection,
            Func<TValueType, TValueType, int> comparer, out TValueType value)
        {
            if (collection != null) return collection.Min(comparer, out value);
            value = default(TValueType);
            return false;
        }

        /// <summary>
        ///     获取最小值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TKeyType">比较键类型</typeparam>
        /// <param name="collection">数组数据</param>
        /// <param name="getKey">获取键值</param>
        /// <param name="comparer">比较器</param>
        /// <param name="value">最小值</param>
        /// <returns>是否存在最小值</returns>
        public static bool min<TValueType, TKeyType>
            (this TmphCollection<TValueType> collection, Func<TValueType, TKeyType> getKey,
                Func<TKeyType, TKeyType, int> comparer, out TValueType value)
        {
            if (collection != null) collection.Min(getKey, comparer, out value);
            value = default(TValueType);
            return false;
        }

        /// <summary>
        ///     获取最小值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="collection">数组数据</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最小值,失败返回默认空值</returns>
        public static TValueType min<TValueType>(this TmphCollection<TValueType> collection, TValueType nullValue)
            where TValueType : IComparable<TValueType>
        {
            if (collection != null)
            {
                TValueType value;
                if (collection.Min((left, right) => left.CompareTo(right), out value)) return value;
            }
            return nullValue;
        }

        /// <summary>
        ///     获取最小值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TKeyType">比较键类型</typeparam>
        /// <param name="collection">数组数据</param>
        /// <param name="getKey">获取键值</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最小值,失败返回默认空值</returns>
        public static TValueType min<TValueType, TKeyType>(this TmphCollection<TValueType> collection,
            Func<TValueType, TKeyType> getKey, TValueType nullValue)
            where TKeyType : IComparable<TKeyType>
        {
            return collection != null
                ? collection.Min(getKey, (left, right) => left.CompareTo(right), nullValue)
                : nullValue;
        }

        /// <summary>
        ///     获取最小值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TKeyType">比较键类型</typeparam>
        /// <param name="collection">数组数据</param>
        /// <param name="getKey">获取键值</param>
        /// <param name="comparer">比较器</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最小值,失败返回默认空值</returns>
        public static TValueType min<TValueType, TKeyType>
            (this TmphCollection<TValueType> collection, Func<TValueType, TKeyType> getKey,
                Func<TKeyType, TKeyType, int> comparer, TValueType nullValue)
        {
            return collection != null ? collection.Min(getKey, comparer, nullValue) : nullValue;
        }

        /// <summary>
        ///     获取最小键值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TKeyType">比较键类型</typeparam>
        /// <param name="collection">数组数据</param>
        /// <param name="getKey">获取键值</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最小键值,失败返回默认空值</returns>
        public static TKeyType minKey<TValueType, TKeyType>(this TmphCollection<TValueType> collection,
            Func<TValueType, TKeyType> getKey, TKeyType nullValue)
            where TKeyType : IComparable<TKeyType>
        {
            return collection != null
                ? collection.MinKey(getKey, (left, right) => left.CompareTo(right), nullValue)
                : nullValue;
        }

        /// <summary>
        ///     获取最小键值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TKeyType">比较键类型</typeparam>
        /// <param name="collection">数组数据</param>
        /// <param name="getKey">获取键值</param>
        /// <param name="comparer">比较器</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最小键值,失败返回默认空值</returns>
        public static TKeyType minKey<TValueType, TKeyType>
            (this TmphCollection<TValueType> collection, Func<TValueType, TKeyType> getKey,
                Func<TKeyType, TKeyType, int> comparer, TKeyType nullValue)
        {
            return collection != null ? collection.MinKey(getKey, comparer, nullValue) : nullValue;
        }

        /// <summary>
        ///     数据分组
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TKeyType">键类型</typeparam>
        /// <param name="collection">双向动态数组</param>
        /// <param name="getKey">键值获取器</param>
        /// <returns>分组数据</returns>
        public static Dictionary<TKeyType, TmphList<TValueType>> group<TValueType, TKeyType>
            (this TmphCollection<TValueType> collection, Func<TValueType, TKeyType> getKey)
            where TKeyType : IEquatable<TKeyType>
        {
            return collection != null ? collection.Group(getKey) : null;
        }

        /// <summary>
        ///     数据去重
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="arrayType">目标数据类型</typeparam>
        /// <param name="collection">双向动态数组</param>
        /// <param name="getValue">目标数据获取器</param>
        /// <returns>目标集合</returns>
        public static TmphSubArray<arrayType> distinct<TValueType, arrayType>(this TmphCollection<TValueType> collection,
            Func<TValueType, arrayType> getValue)
        {
            return collection != null ? collection.Distinct(getValue) : default(TmphSubArray<arrayType>);
        }

        /// <summary>
        ///     连接字符串
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="collection">双向动态数组</param>
        /// <param name="toString">字符串转换器</param>
        /// <returns>字符串</returns>
        public static string joinString<TValueType>(this TmphCollection<TValueType> collection,
            Func<TValueType, string> toString)
        {
            return collection != null ? collection.JoinString(toString) : null;
        }

        /// <summary>
        ///     连接字符串
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="collection">双向动态数组</param>
        /// <param name="toString">字符串转换器</param>
        /// <param name="join">连接串</param>
        /// <returns>字符串</returns>
        public static string joinString<TValueType>(this TmphCollection<TValueType> collection, string join,
            Func<TValueType, string> toString)
        {
            return collection != null ? collection.JoinString(join, toString) : null;
        }

        /// <summary>
        ///     连接字符串
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="collection">双向动态数组</param>
        /// <param name="toString">字符串转换器</param>
        /// <param name="join">连接字符</param>
        /// <returns>字符串</returns>
        public static string joinString<TValueType>(this TmphCollection<TValueType> collection, char join,
            Func<TValueType, string> toString)
        {
            return collection != null ? collection.JoinString(join, toString) : null;
        }
    }
}