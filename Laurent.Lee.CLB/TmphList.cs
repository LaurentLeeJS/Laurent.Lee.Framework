using Laurent.Lee.CLB.Algorithm;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Laurent.Lee.CLB
{
    /// <summary>
    ///     单向动态数组
    /// </summary>
    /// <typeparam name="TValueType">数据类型</typeparam>
    public class TmphList<TValueType> : TmphDynamicArray<TValueType>, IList<TValueType>
    {
        /// <summary>
        ///     数据数量
        /// </summary>
        private int length;

        /// <summary>
        ///     单向动态数据
        /// </summary>
        public TmphList()
        {
            array = TmphNullValue<TValueType>.Array;
        }

        /// <summary>
        ///     单向动态数据
        /// </summary>
        /// <param name="count">数组容器尺寸</param>
        public TmphList(int count)
        {
            array = count > 0 ? new TValueType[count] : TmphNullValue<TValueType>.Array;
        }

        /// <summary>
        ///     单向动态数据
        /// </summary>
        /// <param name="values">数据集合</param>
        public TmphList(ICollection<TValueType> values) : this(values == null ? null : values.GetArray(), true)
        {
        }

        /// <summary>
        ///     单向动态数据
        /// </summary>
        /// <param name="values">数据数组</param>
        /// <param name="isUnsafe">true表示使用原数组,false表示需要复制数组</param>
        public TmphList(TValueType[] values, bool isUnsafe = false)
        {
            if ((length = values.length()) == 0) array = TmphNullValue<TValueType>.Array;
            else if (isUnsafe) array = values;
            else Array.Copy(values, 0, array = GetNewArray(length), 0, length);
        }

        /// <summary>
        ///     单向动态数据
        /// </summary>
        /// <param name="values">数据数组</param>
        /// <param name="index">起始位置</param>
        /// <param name="count">数据数量</param>
        /// <param name="isUnsafe">true表示可以使用原数组,false表示需要复制数组</param>
        public TmphList(TValueType[] values, int index, int count, bool isUnsafe = false)
        {
            var length = values.length();
            var range = new TmphArray.TmphRange(length, index, count);
            this.length = range.GetCount;
            if (isUnsafe)
            {
                if (this.length == 0) array = values ?? TmphNullValue<TValueType>.Array;
                else
                {
                    if ((index = range.SkipCount) == 0) array = values;
                    else Array.Copy(values, index, array = GetNewArray(count), 0, this.length);
                }
            }
            else if (this.length == 0) array = TmphNullValue<TValueType>.Array;
            else Array.Copy(values, range.SkipCount, array = GetNewArray(count), 0, this.length);
        }

        /// <summary>
        ///     单向动态数据
        /// </summary>
        /// <param name="values">数据数组</param>
        /// <param name="isUnsafe">true表示可以使用原数组,false表示需要复制数组</param>
        public TmphList(TmphSubArray<TValueType> values, bool isUnsafe = false)
        {
            length = values.Count;
            if (isUnsafe)
            {
                if (length == 0)
                {
                    array = values.Array ?? TmphNullValue<TValueType>.Array;
                    ;
                }
                else
                {
                    if (values.StartIndex == 0) array = values.Array;
                    else Array.Copy(values.Array, values.StartIndex, array = GetNewArray(length), 0, length);
                }
            }
            else if (length == 0) array = TmphNullValue<TValueType>.Array;
            else Array.Copy(values.Array, values.StartIndex, array = GetNewArray(length), 0, length);
        }

        /// <summary>
        ///     非安全访问单向动态数组
        /// </summary>
        /// <returns>非安全访问单向动态数组</returns>
        public TmphUnsafer Unsafer
        {
            get { return new TmphUnsafer { List = this }; }
        }

        /// 数据数量
        /// </summary>
        protected override int ValueCount
        {
            get { return length; }
        }

        /// <summary>
        ///     数据数量
        /// </summary>
        public int Count
        {
            get { return length; }
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
                if ((uint)index < (uint)length) return array[index];
                TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
                return default(TValueType);
            }
            set
            {
                if ((uint)index < (uint)length) array[index] = value;
                else TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     枚举器
        /// </summary>
        /// <returns>枚举器</returns>
        IEnumerator<TValueType> IEnumerable<TValueType>.GetEnumerator()
        {
            if (length != 0) return new TmphIEnumerator<TValueType>.TmphArray(this);
            return TmphIEnumerator<TValueType>.Empty;
        }

        /// <summary>
        ///     枚举器
        /// </summary>
        /// <returns>枚举器</returns>
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
            if (array.Length != 0)
            {
                if (IsClearArray) Array.Clear(array, 0, array.Length);
                Empty();
            }
        }

        /// <summary>
        ///     添加数据
        /// </summary>
        /// <param name="value">数据</param>
        public void Add(TValueType value)
        {
            if (length != 0)
            {
                if (length == array.Length) setLength(array.Length << 1);
            }
            else if (array.Length == 0) array = new TValueType[sizeof(int)];
            array[length] = value;
            ++length;
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
                    if (length != array.Length)
                    {
                        Unsafe.TmphArray.Move(array, index, index + 1, length - index);
                        array[index] = value;
                        ++length;
                    }
                    else
                    {
                        var values = GetNewArray(array.Length << 1);
                        Array.Copy(array, 0, values, 0, index);
                        values[index] = value;
                        Array.Copy(array, index, values, index + 1, length++ - index);
                        array = values;
                    }
                }
                else Add(value);
            }
            else TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
        }

        /// <summary>
        ///     复制数据
        /// </summary>
        /// <param name="values">目标数据</param>
        /// <param name="index">目标位置</param>
        public void CopyTo(TValueType[] values, int index)
        {
            if (values != null && index >= 0 && length + index <= values.Length)
            {
                Array.Copy(array, 0, values, index, length);
            }
            else TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
        }

        /// <summary>
        ///     移除数据
        /// </summary>
        /// <param name="index">数据位置</param>
        /// <returns>被移除数据</returns>
        public override void RemoveAt(int index)
        {
            if ((uint)index < (uint)length)
            {
                Unsafe.TmphArray.Move(array, index + 1, index, --length - index);
                array[length] = default(TValueType);
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
            return length != 0 ? Array.IndexOf(array, value, 0, length) : -1;
        }

        /// <summary>
        ///     根据位置设置数据
        /// </summary>
        /// <param name="index">数据位置</param>
        /// <param name="value">数据</param>
        public void Set(int index, TValueType value)
        {
            if (index < 0) TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
            if (index >= length) addToLength(index + 1);
            array[index] = value;
        }

        /// <summary>
        ///     强制类型转换
        /// </summary>
        /// <param name="value">单向动态数组</param>
        /// <returns>数组数据</returns>
        public static explicit operator TValueType[] (TmphList<TValueType> value)
        {
            return value != null ? value.ToArray() : null;
        }

        /// <summary>
        ///     强制类型转换
        /// </summary>
        /// <param name="value">数组数据</param>
        /// <returns>单向动态数组</returns>
        public static explicit operator TmphList<TValueType>(TValueType[] value)
        {
            return value != null ? new TmphList<TValueType>(value, true) : null;
        }

        /// <summary>
        ///     长度设为0
        /// </summary>
        public void Empty()
        {
            length = 0;
        }

        /// <summary>
        ///     释放数据数组
        /// </summary>
        public void Null()
        {
            length = 0;
            array = TmphNullValue<TValueType>.Array;
        }

        /// <summary>
        ///     设置数据容器长度
        /// </summary>
        /// <param name="count">数据长度</param>
        private void setLength(int count)
        {
            var newArray = GetNewArray(count);
            Array.Copy(array, 0, newArray, 0, length);
            array = newArray;
        }

        /// <summary>
        ///     增加数据长度
        /// </summary>
        /// <param name="length">数据长度</param>
        private void addToLength(int length)
        {
            if (length > array.Length) setLength(length);
        }

        /// <summary>
        ///     增加数据长度
        /// </summary>
        public void AddLength()
        {
            var newLength = length + 1;
            addToLength(newLength);
            length = newLength;
        }

        /// <summary>
        ///     增加数据长度
        /// </summary>
        /// <param name="count">数据长度</param>
        public void AddLength(int count)
        {
            if (count > 0)
            {
                var newLength = length + count;
                addToLength(newLength);
                length = newLength;
            }
            else if ((count += length) >= 0) length = count;
            else TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
        }

        /// <summary>
        ///     添加数据集合
        /// </summary>
        /// <param name="values">数据集合</param>
        public void Add(ICollection<TValueType> values)
        {
            var count = values.Count();
            if (count != 0)
            {
                addToLength(length + count);
                foreach (var value in values) array[length++] = value;
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
            var range = new TmphArray.TmphRange(values.length(), index, count);
            if ((count = range.GetCount) != 0)
            {
                var newLength = length + count;
                addToLength(newLength);
                Array.Copy(values, range.SkipCount, array, length, count);
                length = newLength;
            }
        }

        /// <summary>
        ///     插入数据集合
        /// </summary>
        /// <param name="index">插入位置</param>
        /// <param name="values">数据集合</param>
        public void Insert(int index, IEnumerable<TValueType> values)
        {
            if (values != null)
            {
                var newValues = values.getSubArray();
                Insert(index, newValues.Array, 0, newValues.Count);
            }
        }

        /// <summary>
        ///     插入数据集合
        /// </summary>
        /// <param name="index">插入位置</param>
        /// <param name="values">数据集合</param>
        public void Insert(int index, ICollection<TValueType> values)
        {
            if ((uint)index <= (uint)this.length)
            {
                var count = values.Count();
                if (count != 0)
                {
                    var length = this.length + count;
                    if (array.Length != 0)
                    {
                        if (length <= array.Length)
                        {
                            Unsafe.TmphArray.Move(array, index, index + count, this.length - index);
                            foreach (var value in values) array[index++] = value;
                        }
                        else
                        {
                            var newValues = GetNewArray(length);
                            Array.Copy(array, 0, newValues, 0, index);
                            foreach (var value in values) array[index++] = value;
                            Array.Copy(array, index -= count, newValues, index + count, this.length - index);
                            array = newValues;
                        }
                        this.length = length;
                    }
                    else
                    {
                        array = GetNewArray(length);
                        foreach (var value in values) array[this.length++] = value;
                    }
                }
            }
            else TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
        }

        /// <summary>
        ///     插入数据集合
        /// </summary>
        /// <param name="index">插入位置</param>
        /// <param name="values">数据集合</param>
        public void Insert(int index, TValueType[] values)
        {
            if (values != null) Insert(index, values, 0, values.Length);
        }

        /// <summary>
        ///     插入数据集合
        /// </summary>
        /// <param name="index">插入位置</param>
        /// <param name="values">数据集合</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">插入数量</param>
        public void Insert(int index, TValueType[] values, int startIndex, int count)
        {
            if ((uint)index <= (uint)this.length)
            {
                var range = new TmphArray.TmphRange(values.length(), startIndex, count);
                if ((count = range.GetCount) != 0)
                {
                    var length = this.length + count;
                    if (array.Length != 0)
                    {
                        if (length <= array.Length)
                        {
                            Unsafe.TmphArray.Move(array, index, index + count, this.length - index);
                            Array.Copy(values, range.SkipCount, array, index, count);
                        }
                        else
                        {
                            var newValues = GetNewArray(length);
                            Array.Copy(array, 0, newValues, 0, index);
                            Array.Copy(values, startIndex, newValues, index, count);
                            Array.Copy(array, index, newValues, index + count, this.length - index);
                            array = newValues;
                        }
                    }
                    else Array.Copy(values, range.SkipCount, array = GetNewArray(length), 0, length);
                    this.length = length;
                }
            }
            else TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
        }

        /// <summary>
        ///     插入数据集合
        /// </summary>
        /// <param name="index">插入位置</param>
        /// <param name="values">数据集合</param>
        public void Insert(int index, TmphSubArray<TValueType> values)
        {
            Insert(index, values.Array, values.StartIndex, values.Count);
        }

        /// <summary>
        ///     插入数据集合
        /// </summary>
        /// <param name="index">插入位置</param>
        /// <param name="value">数据集合</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">插入数量</param>
        public void Insert(int index, TmphList<TValueType> value, int startIndex, int count)
        {
            if (value != null) Insert(index, value.array, startIndex, count);
        }

        /// <summary>
        ///     移除数据并使用最后一个数据移动到当前位置
        /// </summary>
        /// <param name="index">数据位置</param>
        public void RemoveAtEnd(int index)
        {
            if ((uint)index < (uint)length) array[index] = array[--length];
            else TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
        }

        /// <summary>
        ///     移除数据
        /// </summary>
        /// <param name="index">数据位置</param>
        /// <returns>被移除数据</returns>
        public override TValueType GetRemoveAt(int index)
        {
            if ((uint)index < (uint)length)
            {
                var value = array[index];
                Unsafe.TmphArray.Move(array, index + 1, index, --length - index);
                return value;
            }
            TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
            return default(TValueType);
        }

        /// <summary>
        ///     移除数据并使用最后一个数据移动到当前位置
        /// </summary>
        /// <param name="index">数据位置</param>
        /// <returns>被移除数据</returns>
        public TValueType GetRemoveAtEnd(int index)
        {
            if ((uint)index < (uint)length)
            {
                var value = array[index];
                array[index] = array[--length];
                return value;
            }
            TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
            return default(TValueType);
        }

        /// <summary>
        ///     获取匹配位置
        /// </summary>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>匹配位置,失败为-1</returns>
        public override int IndexOf(Func<TValueType, bool> isValue)
        {
            if (isValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            return indexOf(isValue);
        }

        /// <summary>
        ///     获取获取数组中的匹配位置
        /// </summary>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>数组中的匹配位置,失败为-1</returns>
        protected override int indexOf(Func<TValueType, bool> isValue)
        {
            if (length != 0)
            {
                var index = 0;
                foreach (var value in array)
                {
                    if (isValue(value)) return index;
                    if (++index == length) break;
                }
            }
            return -1;
        }

        /// <summary>
        ///     移除数据范围
        /// </summary>
        /// <param name="index">起始位置</param>
        public void RemoveRange(int index)
        {
            if ((uint)index <= (uint)length) length = index;
            else TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
        }

        /// <summary>
        ///     移除数据范围
        /// </summary>
        /// <param name="index">起始位置</param>
        /// <param name="count">移除数量</param>
        public void RemoveRange(int index, int count)
        {
            if (index + count <= length && index >= 0 && count >= 0)
            {
                Unsafe.TmphArray.Move(array, index + count, index, (length -= count) - index);
            }
            else TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
        }

        /// <summary>
        ///     弹出最后一个数据
        /// </summary>
        /// <returns>最后一个数据</returns>
        public TValueType Pop()
        {
            if (length != 0) return array[--length];
            TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
            return default(TValueType);
        }

        /// <summary>
        ///     弹出最后一个数据
        /// </summary>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最后一个数据,失败返回默认空值</returns>
        public TValueType Pop(TValueType nullValue)
        {
            return length != 0 ? array[--length] : nullValue;
        }

        /// <summary>
        ///     移除所有后端匹配值
        /// </summary>
        /// <param name="isValue">数据匹配器</param>
        private void removeEnd(Func<TValueType, bool> isValue)
        {
            while (--length >= 0 && isValue(array[length])) ;
            ++length;
        }

        /// <summary>
        ///     移除匹配值
        /// </summary>
        /// <param name="isValue">数据匹配器</param>
        public void Remove(Func<TValueType, bool> isValue)
        {
            if (isValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            removeEnd(isValue);
            var index = indexOf(isValue);
            if (index != -1)
            {
                for (var start = index; ++start != length;)
                {
                    if (!isValue(array[start])) array[index++] = array[start];
                }
                length = index;
            }
        }

        /// <summary>
        ///     逆转列表
        /// </summary>
        public void Reverse()
        {
            if (length != 0) Array.Reverse(array, 0, length);
        }

        /// <summary>
        ///     转换成前端子段集合
        /// </summary>
        /// <param name="count">数量</param>
        /// <returns>子集合</returns>
        public TmphList<TValueType> Left(int count)
        {
            if (count >= 0)
            {
                if (count < length) length = count;
                return this;
            }
            return null;
        }

        /// <summary>
        ///     转换成子集合(不清除数组)
        /// </summary>
        /// <param name="index">起始位置</param>
        /// <param name="count">数量,小于0表示所有</param>
        /// <returns>子集合</returns>
        public TmphSubArray<TValueType> Sub(int index, int count)
        {
            var range = new TmphArray.TmphRange(length, index, count < 0 ? length - index : count);
            if (range.GetCount > 0)
            {
                return TmphSubArray<TValueType>.Unsafe(array, range.SkipCount, range.GetCount);
            }
            return default(TmphSubArray<TValueType>);
        }

        /// <summary>
        ///     取子集合
        /// </summary>
        /// <param name="index">起始位置</param>
        /// <param name="count">数量</param>
        /// <returns>子集合</returns>
        public TValueType[] GetSub(int index, int count)
        {
            var range = new TmphArray.TmphRange(length, index, count);
            if (range.GetCount > 0)
            {
                var values = new TValueType[range.GetCount];
                Array.Copy(array, range.SkipCount, values, 0, range.GetCount);
                return values;
            }
            return TmphNullValue<TValueType>.Array;
        }

        /// <summary>
        ///     转换成数组子串
        /// </summary>
        /// <returns></returns>
        public TmphSubArray<TValueType> ToSubArray()
        {
            return TmphSubArray<TValueType>.Unsafe(array, 0, length);
        }

        /// <summary>
        ///     获取匹配数量
        /// </summary>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>匹配数量</returns>
        public int GetCount(Func<TValueType, bool> isValue)
        {
            var count = 0;
            if (length != 0)
            {
                if (isValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                var index = length;
                foreach (var value in array)
                {
                    if (isValue(value)) ++count;
                    if (--index == 0) break;
                }
            }
            return count;
        }

        /// <summary>
        ///     获取匹配值集合
        /// </summary>
        /// <param name="isValue">数据匹配器</param>
        /// <param name="map">匹配结果位图</param>
        /// <returns>匹配值集合</returns>
        protected override TValueType[] getFindArray(Func<TValueType, bool> isValue, TmphFixedMap map)
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
            return TmphNullValue<TValueType>.Array;
        }

        /// <summary>
        ///     排序
        /// </summary>
        /// <param name="comparer">比较器</param>
        public void Sort(Func<TValueType, TValueType, int> comparer)
        {
            TmphQuickSort.Sort(array, comparer, 0, length);
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
            for (int length = 0, endIndex = index + count; index != endIndex; ++index)
                values[length++] = getValue(array[index]);
            return values;
        }

        /// <summary>
        ///     获取数据排序范围(不清除数组)
        /// </summary>
        /// <param name="comparer">比较器</param>
        /// <param name="skipCount">跳过记录数</param>
        /// <param name="getCount">获取记录数</param>
        /// <returns>排序数据</returns>
        public TmphSubArray<TValueType> RangeSort(Func<TValueType, TValueType, int> comparer, int skipCount, int getCount)
        {
            var range = new TmphArray.TmphRange(length, skipCount, getCount);
            return TmphQuickSort.RangeSort(array, 0, length, comparer, range.SkipCount, range.GetCount);
        }

        /// <summary>
        ///     获取排序数据分页(不清除数组)
        /// </summary>
        /// <param name="comparer">比较器</param>
        /// <param name="pageSize">分页尺寸</param>
        /// <param name="currentPage">页号</param>
        /// <returns>排序数据</returns>
        public TmphSubArray<TValueType> PageSort(Func<TValueType, TValueType, int> comparer, int pageSize, int currentPage)
        {
            var page = new TmphArray.TmphPage(length, pageSize, currentPage);
            return TmphQuickSort.RangeSort(array, 0, length, comparer, page.SkipCount, page.CurrentPageSize);
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
            if (comparer == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            if (length != 0)
            {
                value = array[0];
                for (var index = 1; index != length; ++index)
                {
                    var nextValue = array[index];
                    if (comparer(nextValue, value) > 0) value = nextValue;
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
            if (getKey == null || comparer == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            if (length != 0)
            {
                value = array[0];
                if (length != 1)
                {
                    var key = getKey(value);
                    for (var index = 1; index != length; ++index)
                    {
                        var nextValue = array[index];
                        var nextKey = getKey(nextValue);
                        if (comparer(nextKey, key) > 0)
                        {
                            value = nextValue;
                            key = nextKey;
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
            if (comparer == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            if (length != 0)
            {
                value = array[0];
                for (var index = 1; index != length; ++index)
                {
                    var nextValue = array[index];
                    if (comparer(nextValue, value) < 0) value = nextValue;
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
            if (getKey == null || comparer == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            if (length != 0)
            {
                value = array[0];
                if (length != 1)
                {
                    var key = getKey(value);
                    for (var index = 1; index != length; ++index)
                    {
                        var nextValue = array[index];
                        var nextKey = getKey(nextValue);
                        if (comparer(nextKey, key) < 0)
                        {
                            value = nextValue;
                            key = nextKey;
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
            if (length != 0)
            {
                var newValues = TmphDictionary<TKeyType>.Create<TmphList<TValueType>>(length);
                TmphList<TValueType> list;
                var count = length;
                foreach (var value in array)
                {
                    var key = getKey(value);
                    if (!newValues.TryGetValue(key, out list)) newValues[key] = list = new TmphList<TValueType>();
                    list.Add(value);
                    if (--count == 0) break;
                }
                return newValues;
            }
            return TmphDictionary<TKeyType>.Create<TmphList<TValueType>>();
        }

        /// <summary>
        ///     数据去重
        /// </summary>
        /// <typeparam name="arrayType">目标数组类型</typeparam>
        /// <param name="getValue">数据获取器</param>
        /// <returns>目标集合</returns>
        public TmphSubArray<arrayType> Distinct<arrayType>(Func<TValueType, arrayType> getValue)
        {
            if (length != 0)
            {
                var newValues = new arrayType[length];
                var hash = TmphHashSet<TValueType>.Create();
                int count = length, index = 0;
                foreach (var value in array)
                {
                    if (!hash.Contains(value))
                    {
                        newValues[index++] = getValue(value);
                        hash.Add(value);
                    }
                    if (--count == 0) break;
                }
                return TmphSubArray<arrayType>.Unsafe(newValues, 0, index);
            }
            return default(TmphSubArray<arrayType>);
        }

        /// <summary>
        ///     转换数据集合
        /// </summary>
        /// <typeparam name="arrayType">目标数据类型</typeparam>
        /// <param name="getValue">数据转换器</param>
        /// <returns>数据集合</returns>
        public override arrayType[] GetArray<arrayType>(Func<TValueType, arrayType> getValue)
        {
            if (length != 0)
            {
                if (getValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                var values = new arrayType[length];
                var index = 0;
                foreach (var value in array)
                {
                    values[index] = getValue(value);
                    if (++index == length) break;
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
            if (length != 0)
            {
                if (getKey == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                var values = new TmphKeyValue<TKeyType, TValueType>[length];
                var index = 0;
                foreach (var value in array)
                {
                    values[index].Set(getKey(value), value);
                    if (++index == length) break;
                }
                return values;
            }
            return TmphNullValue<TmphKeyValue<TKeyType, TValueType>>.Array;
        }

        /// <summary>
        ///     转换双向动态数组
        /// </summary>
        /// <returns>双向动态数组</returns>
        public TmphCollection<TValueType> ToCollection()
        {
            return new TmphCollection<TValueType>(this);
        }

        /// <summary>
        ///     转换数组
        /// </summary>
        /// <returns>数组</returns>
        private TValueType[] getArray()
        {
            var values = new TValueType[length];
            Array.Copy(array, 0, values, 0, length);
            return values;
        }

        /// <summary>
        ///     转换数组
        /// </summary>
        /// <returns>数组</returns>
        public TValueType[] GetArray()
        {
            return length != 0 ? getArray() : TmphNullValue<TValueType>.Array;
        }

        /// <summary>
        ///     转换数组
        /// </summary>
        /// <returns>数组</returns>
        public TValueType[] ToArray()
        {
            if (length != 0)
            {
                return length != array.Length ? getArray() : array;
            }
            return TmphNullValue<TValueType>.Array;
        }

        /// <summary>
        ///     非安全访问单向动态数组(请自行确保数据可靠性)
        /// </summary>
        public struct TmphUnsafer
        {
            /// <summary>
            ///     单向动态数组
            /// </summary>
            public TmphList<TValueType> List;

            /// <summary>
            ///     数据数组
            /// </summary>
            public TValueType[] Array
            {
                get { return List.array; }
            }

            /// <summary>
            ///     设置或获取值
            /// </summary>
            /// <param name="index">位置</param>
            /// <returns>数据值</returns>
            public TValueType this[int index]
            {
                get { return Array[index]; }
                set { Array[index] = value; }
            }

            /// <summary>
            ///     清除数据
            /// </summary>
            public void Clear()
            {
                System.Array.Clear(List.array, 0, List.length);
                List.Empty();
            }

            /// <summary>
            ///     增加数据长度
            /// </summary>
            /// <param name="length">增加数据长度</param>
            public void AddLength(int length)
            {
                List.length += length;
            }

            /// <summary>
            ///     添加数据
            /// </summary>
            /// <param name="value">数据</param>
            public void Add(TValueType value)
            {
                List.array[List.length] = value;
                ++List.length;
            }

            /// <summary>
            ///     添加数据集合
            /// </summary>
            /// <param name="values">数据集合</param>
            /// <param name="index">起始位置</param>
            /// <param name="count">数量</param>
            public void Add(TValueType[] values, int index, int count)
            {
                System.Array.Copy(values, index, Array, List.length, count);
                List.length += count;
            }

            /// <summary>
            ///     移除数据范围
            /// </summary>
            /// <param name="index">起始位置</param>
            public void RemoveRange(int index)
            {
                List.length = index;
            }

            /// <summary>
            ///     弹出最后一个数据
            /// </summary>
            /// <returns>最后一个数据</returns>
            public TValueType Pop()
            {
                return List.array[--List.length];
            }

            /// <summary>
            ///     弹出最后一个数据
            /// </summary>
            /// <returns>最后一个数据</returns>
            public TValueType PopReset()
            {
                var value = List.array[--List.length];
                List.array[List.length] = default(TValueType);
                return value;
            }

            /// <summary>
            ///     转换成前端子段集合
            /// </summary>
            /// <param name="count">数量</param>
            public void Left(int count)
            {
                List.length = count;
            }

            /// <summary>
            ///     转换成子集合
            /// </summary>
            /// <param name="index">起始位置</param>
            /// <returns>子集合</returns>
            public TmphSubArray<TValueType> Sub(int index)
            {
                return TmphSubArray<TValueType>.Unsafe(Array, index, List.length - index);
            }

            /// <summary>
            ///     转换成子集合()
            /// </summary>
            /// <param name="index">起始位置</param>
            /// <param name="count">数量,小于0表示所有</param>
            /// <returns>子集合</returns>
            public TmphSubArray<TValueType> Sub(int index, int count)
            {
                return TmphSubArray<TValueType>.Unsafe(Array, index, count);
            }
        }
    }

    /// <summary>
    ///     单向动态数组扩展
    /// </summary>
    public static class TmphList
    {
        /// <summary>
        ///     长度设为0
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="list">单向动态数组</param>
        public static TmphList<TValueType> empty<TValueType>(this TmphList<TValueType> list)
        {
            if (list != null) list.Empty();
            return list;
        }

        /// <summary>
        ///     清除所有数据
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="list">单向动态数组</param>
        public static TmphList<TValueType> clear<TValueType>(this TmphList<TValueType> list)
        {
            if (list != null) list.Clear();
            return list;
        }

        /// <summary>
        ///     获取第一个值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="list">单向动态数组</param>
        /// <returns>第一个值,失败为default(TValueType)</returns>
        public static TValueType firstOrDefault<TValueType>(this TmphList<TValueType> list)
        {
            return list.Count() != 0 ? list.Unsafer[0] : default(TValueType);
        }

        /// <summary>
        ///     获取最后一个值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="list">单向动态数组</param>
        /// <returns>最后一个值,失败为default(TValueType)</returns>
        public static TValueType lastOrDefault<TValueType>(this TmphList<TValueType> list)
        {
            return list.Count() != 0 ? list.Unsafer[list.Count - 1] : default(TValueType);
        }

        /// <summary>
        ///     根据位置设置数据
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="list">单向动态数组</param>
        /// <param name="index">数据位置</param>
        /// <param name="value">数据</param>
        /// <returns>单向动态数组</returns>
        public static TmphList<TValueType> set<TValueType>(this TmphList<TValueType> list, int index, TValueType value)
        {
            if (list == null) list = new TmphList<TValueType>(index + 1);
            list.Set(index, value);
            return list;
        }

        /// <summary>
        ///     增加数据长度
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="list">单向动态数组</param>
        /// <returns>单向动态数组</returns>
        public static TmphList<TValueType> addLength<TValueType>(this TmphList<TValueType> list)
        {
            if (list != null)
            {
                list.AddLength();
                return list;
            }
            return new TmphList<TValueType>(sizeof(int));
        }

        /// <summary>
        ///     增加数据长度
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="list">单向动态数组</param>
        /// <param name="count">数据长度</param>
        /// <returns>单向动态数组</returns>
        public static TmphList<TValueType> addLength<TValueType>(this TmphList<TValueType> list, int count)
        {
            if (list != null)
            {
                list.AddLength(count);
                return list;
            }
            return new TmphList<TValueType>(count > sizeof(int) ? count : sizeof(int));
        }

        /// <summary>
        ///     添加数据
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="list">单向动态数组</param>
        /// <param name="value">数据</param>
        /// <returns>单向动态数组</returns>
        public static TmphList<TValueType> add<TValueType>(this TmphList<TValueType> list, TValueType value)
        {
            if (list == null) list = new TmphList<TValueType>();
            list.Add(value);
            return list;
        }

        /// <summary>
        ///     添加数据集合
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="list">单向动态数组</param>
        /// <param name="values">数据集合</param>
        /// <returns>单向动态数组</returns>
        public static TmphList<TValueType> add<TValueType>(this TmphList<TValueType> list, IEnumerable<TValueType> values)
        {
            if (list != null)
            {
                list.Add(values.getSubArray());
                return list;
            }
            return values.getSubArray().ToList();
        }

        /// <summary>
        ///     添加数据集合
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="list">单向动态数组</param>
        /// <param name="values">数据集合</param>
        /// <returns>单向动态数组</returns>
        public static TmphList<TValueType> add<TValueType>(this TmphList<TValueType> list, ICollection<TValueType> values)
        {
            if (list != null)
            {
                list.Add(values);
                return list;
            }
            return values.GetList();
        }

        /// <summary>
        ///     添加数据集合
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="list">单向动态数组</param>
        /// <param name="values">数据集合</param>
        /// <returns>单向动态数组</returns>
        public static TmphList<TValueType> add<TValueType>(this TmphList<TValueType> list, TValueType[] values)
        {
            if (list != null)
            {
                list.Add(values);
                return list;
            }
            return new TmphList<TValueType>(values);
        }

        /// <summary>
        ///     添加数据集合
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="list">单向动态数组</param>
        /// <param name="values">数据集合</param>
        /// <param name="index">起始位置</param>
        /// <param name="count">数量</param>
        /// <returns>单向动态数组</returns>
        public static TmphList<TValueType> add<TValueType>(this TmphList<TValueType> list, TValueType[] values, int index,
            int count)
        {
            if (list != null)
            {
                list.Add(values, index, count);
                return list;
            }
            return new TmphList<TValueType>(values, index, count, false);
        }

        /// <summary>
        ///     添加数据集合
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="list">单向动态数组</param>
        /// <param name="value">数据集合</param>
        /// <returns>单向动态数组</returns>
        public static TmphList<TValueType> add<TValueType>(this TmphList<TValueType> list, TmphList<TValueType> value)
        {
            if (list != null)
            {
                list.Add(value);
                return list;
            }
            return new TmphList<TValueType>(value);
        }

        /// <summary>
        ///     添加数据集合
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="list">单向动态数组</param>
        /// <param name="value">数据集合</param>
        /// <param name="index">起始位置</param>
        /// <param name="count">数量</param>
        /// <returns>单向动态数组</returns>
        public static TmphList<TValueType> add<TValueType>(this TmphList<TValueType> list, TmphList<TValueType> value, int index,
            int count)
        {
            if (list != null)
            {
                list.Add(value, index, count);
                return list;
            }
            return value.Count() != 0 ? new TmphList<TValueType>(value.array, index, count, false) : null;
        }

        /// <summary>
        ///     插入数据
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="list">单向动态数组</param>
        /// <param name="index">插入位置</param>
        /// <param name="value">数据</param>
        /// <returns>单向动态数组</returns>
        public static TmphList<TValueType> insert<TValueType>(this TmphList<TValueType> list, int index, TValueType value)
        {
            if (list != null)
            {
                list.Insert(index, value);
                return list;
            }
            if (index == 0) return list.add(value);
            TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
            return null;
        }

        /// <summary>
        ///     插入数据集合
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="list">单向动态数组</param>
        /// <param name="index">插入位置</param>
        /// <param name="values">数据集合</param>
        /// <returns>单向动态数组</returns>
        public static TmphList<TValueType> insert<TValueType>(this TmphList<TValueType> list, int index,
            IEnumerable<TValueType> values)
        {
            if (list != null)
            {
                list.Insert(index, values);
                return list;
            }
            if (index == 0) return values.getSubArray().ToList();
            TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
            return null;
        }

        /// <summary>
        ///     插入数据集合
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="list">单向动态数组</param>
        /// <param name="index">插入位置</param>
        /// <param name="values">数据集合</param>
        /// <returns>单向动态数组</returns>
        public static TmphList<TValueType> insert<TValueType>(this TmphList<TValueType> list, int index,
            ICollection<TValueType> values)
        {
            if (list != null)
            {
                list.Insert(index, values);
                return list;
            }
            if (index == 0) return values.GetList();
            TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
            return null;
        }

        /// <summary>
        ///     插入数据集合
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="list">单向动态数组</param>
        /// <param name="index">插入位置</param>
        /// <param name="values">数据集合</param>
        /// <returns>单向动态数组</returns>
        public static TmphList<TValueType> insert<TValueType>(this TmphList<TValueType> list, int index, TValueType[] values)
        {
            return list.insert(index, values, 0, values.Length);
        }

        /// <summary>
        ///     插入数据集合
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="list">单向动态数组</param>
        /// <param name="index">插入位置</param>
        /// <param name="values">数据集合</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">插入数量</param>
        /// <returns>单向动态数组</returns>
        public static TmphList<TValueType> insert<TValueType>(this TmphList<TValueType> list, int index, TValueType[] values,
            int startIndex, int count)
        {
            if (list != null)
            {
                list.Insert(index, values, startIndex, count);
                return list;
            }
            if (index == 0) return new TmphList<TValueType>(values, startIndex, count, false);
            TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
            return null;
        }

        /// <summary>
        ///     插入数据集合
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="list">单向动态数组</param>
        /// <param name="index">插入位置</param>
        /// <param name="value">数据集合</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">插入数量</param>
        /// <returns>单向动态数组</returns>
        public static TmphList<TValueType> insert<TValueType>(this TmphList<TValueType> list, int index,
            TmphList<TValueType> value, int startIndex, int count)
        {
            return value.Count() != 0 ? list.insert(index, value.array, startIndex, count) : list;
        }

        /// <summary>
        ///     复制数据
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="list">单向动态数组</param>
        /// <param name="values">目标数据</param>
        /// <param name="index">目标位置</param>
        /// <returns>目标数组</returns>
        public static TValueType[] copyTo<TValueType>(this TmphList<TValueType> list, TValueType[] values, int index)
        {
            if (list != null) list.CopyTo(values, index);
            return values;
        }

        /// <summary>
        ///     移除数据
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="list">单向动态数组</param>
        /// <param name="index">数据位置</param>
        /// <returns>被移除数据</returns>
        /// <returns>单向动态数组</returns>
        public static TmphList<TValueType> removeAt<TValueType>(this TmphList<TValueType> list, int index)
        {
            if (list != null)
            {
                list.RemoveAt(index);
                return list;
            }
            TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
            return null;
        }

        /// <summary>
        ///     移除数据
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="list">单向动态数组</param>
        /// <param name="value">数据</param>
        /// <returns>是否存在移除数据</returns>
        public static bool removeFirst<TValueType>(this TmphList<TValueType> list, TValueType value)
        {
            return list != null ? list.Remove(value) : false;
        }

        /// <summary>
        ///     替换数据
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="list">单向动态数组</param>
        /// <param name="value">新数据</param>
        /// <param name="isValue">数据匹配器</param>
        public static TmphList<TValueType> replaceFirst<TValueType>(this TmphList<TValueType> list, TValueType value,
            Func<TValueType, bool> isValue)
        {
            if (list != null) list.ReplaceFirst(value, isValue);
            return list;
        }

        /// <summary>
        ///     获取匹配数据位置
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="list">单向动态数组</param>
        /// <param name="value">匹配数据</param>
        /// <returns>匹配位置,失败为-1</returns>
        public static int indexOf<TValueType>(this TmphList<TValueType> list, TValueType value)
        {
            return list != null ? list.IndexOf(value) : -1;
        }

        /// <summary>
        ///     获取匹配位置
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="list">单向动态数组</param>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>匹配位置,失败为-1</returns>
        public static int indexOf<TValueType>(this TmphList<TValueType> list, Func<TValueType, bool> isValue)
        {
            return list != null ? list.IndexOf(isValue) : -1;
        }

        /// <summary>
        ///     获取第一个匹配值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="list">单向动态数组</param>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>匹配值,失败为 default(TValueType)</returns>
        public static TValueType firstOrDefault<TValueType>(this TmphList<TValueType> list, Func<TValueType, bool> isValue)
        {
            return list != null ? list.FirstOrDefault(isValue) : default(TValueType);
        }

        /// <summary>
        ///     判断是否存在匹配
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="list">单向动态数组</param>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>是否存在匹配</returns>
        public static bool any<TValueType>(this TmphList<TValueType> list, Func<TValueType, bool> isValue)
        {
            return list != null && list.Any(isValue);
        }

        /// <summary>
        ///     判断是否存在数据
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="list">单向动态数组</param>
        /// <param name="value">匹配数据</param>
        /// <returns>是否存在数据</returns>
        public static bool contains<TValueType>(this TmphList<TValueType> list, TValueType value)
        {
            return list.indexOf(value) != -1;
        }

        /// <summary>
        ///     移除数据范围
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="list">单向动态数组</param>
        /// <param name="index">起始位置</param>
        /// <returns>单向动态数组</returns>
        public static TmphList<TValueType> removeRange<TValueType>(this TmphList<TValueType> list, int index)
        {
            if (list != null)
            {
                list.RemoveRange(index);
                return list;
            }
            if (index == 0) return null;
            TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
            return null;
        }

        /// <summary>
        ///     移除数据范围
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="list">单向动态数组</param>
        /// <param name="index">起始位置</param>
        /// <param name="count">移除数量</param>
        /// <returns>单向动态数组</returns>
        public static TmphList<TValueType> removeRange<TValueType>(this TmphList<TValueType> list, int index, int count)
        {
            if (list != null)
            {
                list.RemoveRange(index, count);
                return list;
            }
            if ((index | count) == 0) return null;
            TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
            return null;
        }

        /// <summary>
        ///     移除匹配值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="list">单向动态数组</param>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>单向动态数组</returns>
        public static TmphList<TValueType> remove<TValueType>(this TmphList<TValueType> list, Func<TValueType, bool> isValue)
        {
            if (list != null) list.Remove(isValue);
            return list;
        }

        /// <summary>
        ///     逆转单向动态数组
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="list">单向动态数组</param>
        /// <returns>单向动态数组</returns>
        public static TmphList<TValueType> reverse<TValueType>(this TmphList<TValueType> list)
        {
            if (list != null) list.Reverse();
            return list;
        }

        /// <summary>
        ///     转换成前端子段集合
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="list">单向动态数组</param>
        /// <param name="count">数量</param>
        /// <returns>子集合</returns>
        public static TmphList<TValueType> left<TValueType>(this TmphList<TValueType> list, int count)
        {
            return list != null ? list.Left(count) : null;
        }

        /// <summary>
        ///     转换成子集合(不清除数组)
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="list">单向动态数组</param>
        /// <param name="index">起始位置</param>
        /// <returns>子集合</returns>
        public static TmphSubArray<TValueType> sub<TValueType>(this TmphList<TValueType> list, int index)
        {
            return list.sub(index, -1);
        }

        /// <summary>
        ///     转换成子集合(不清除数组)
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="list">单向动态数组</param>
        /// <param name="index">起始位置</param>
        /// <param name="count">数量,小于0表示所有</param>
        /// <returns>子集合</returns>
        public static TmphSubArray<TValueType> sub<TValueType>(this TmphList<TValueType> list, int index, int count)
        {
            return list != null ? list.Sub(index, count) : default(TmphSubArray<TValueType>);
        }

        /// <summary>
        ///     取子集合
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="list">单向动态数组</param>
        /// <param name="index">起始位置</param>
        /// <param name="count">数量</param>
        /// <returns>子集合</returns>
        public static TValueType[] getSub<TValueType>(this TmphList<TValueType> list, int index, int count)
        {
            return list != null ? list.GetSub(index, count) : TmphNullValue<TValueType>.Array;
        }

        /// <summary>
        ///     转换成数组子串
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static TmphSubArray<TValueType> toSubArray<TValueType>(this TmphList<TValueType> list)
        {
            return list != null ? list.ToSubArray() : default(TmphSubArray<TValueType>);
        }

        /// <summary>
        ///     获取匹配数量
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="list">单向动态数组</param>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>匹配数量</returns>
        public static int count<TValueType>(this TmphList<TValueType> list, Func<TValueType, bool> isValue)
        {
            return list != null ? list.GetCount(isValue) : 0;
        }

        /// <summary>
        ///     获取匹配值集合
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="list">单向动态数组</param>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>匹配值集合</returns>
        public static TValueType[] getFindArray<TValueType>(this TmphList<TValueType> list, Func<TValueType, bool> isValue)
        {
            return list != null ? list.GetFindArray(isValue) : TmphNullValue<TValueType>.Array;
        }

        /// <summary>
        ///     遍历foreach
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="list">单向动态数组</param>
        /// <param name="method">调用函数</param>
        /// <returns>单向动态数组</returns>
        public static TmphList<TValueType> each<TValueType>(this TmphList<TValueType> list, Action<TValueType> method)
        {
            if (list.Count() != 0)
            {
                if (method == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                var count = list.Count;
                foreach (var value in list.array)
                {
                    method(value);
                    if (--count == 0) break;
                }
            }
            return list;
        }

        /// <summary>
        ///     排序
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="list">单向动态数组</param>
        /// <param name="comparer">比较器</param>
        /// <returns>单向动态数组</returns>
        public static TmphList<TValueType> sort<TValueType>(this TmphList<TValueType> list,
            Func<TValueType, TValueType, int> comparer)
        {
            if (list != null) list.Sort(comparer);
            return list;
        }

        /// <summary>
        ///     获取数据分页
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="list">单向动态数组</param>
        /// <typeparam name="arrayType">目标数据类型</typeparam>
        /// <param name="pageSize">分页尺寸</param>
        /// <param name="currentPage">页号</param>
        /// <param name="getValue">数据转换器</param>
        /// <returns>目标数据</returns>
        public static arrayType[] getPage<TValueType, arrayType>
            (this TmphList<TValueType> list, int pageSize, int currentPage, Func<TValueType, arrayType> getValue)
        {
            return list != null ? list.GetPage(pageSize, currentPage, getValue) : TmphNullValue<arrayType>.Array;
        }

        /// <summary>
        ///     获取数据排序范围(不清除数组)
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="list">单向动态数组</param>
        /// <param name="comparer">比较器</param>
        /// <param name="skipCount">跳过记录数</param>
        /// <param name="getCount">获取记录数</param>
        /// <returns>排序数据</returns>
        public static TmphSubArray<TValueType> rangeSort<TValueType>
            (this TmphList<TValueType> list, Func<TValueType, TValueType, int> comparer, int skipCount, int getCount)
        {
            return list != null ? list.RangeSort(comparer, skipCount, getCount) : default(TmphSubArray<TValueType>);
        }

        /// <summary>
        ///     获取排序数据分页(不清除数组)
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="list">单向动态数组</param>
        /// <param name="comparer">比较器</param>
        /// <param name="pageSize">分页尺寸</param>
        /// <param name="currentPage">页号</param>
        /// <returns>排序数据</returns>
        public static TmphSubArray<TValueType> pageSort<TValueType>
            (this TmphList<TValueType> list, Func<TValueType, TValueType, int> comparer, int pageSize, int currentPage)
        {
            return list != null ? list.PageSort(comparer, pageSize, currentPage) : default(TmphSubArray<TValueType>);
        }

        /// <summary>
        ///     获取最大值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="list">数组数据</param>
        /// <param name="comparer">比较器</param>
        /// <param name="value">最大值</param>
        /// <returns>是否存在最大值</returns>
        public static bool max<TValueType>(this TmphList<TValueType> list, Func<TValueType, TValueType, int> comparer,
            out TValueType value)
        {
            if (list != null) return list.Max(comparer, out value);
            value = default(TValueType);
            return false;
        }

        /// <summary>
        ///     获取最大值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TKeyType">比较键类型</typeparam>
        /// <param name="list">数组数据</param>
        /// <param name="getKey">获取键值</param>
        /// <param name="comparer">比较器</param>
        /// <param name="value">最大值</param>
        /// <returns>是否存在最大值</returns>
        public static bool max<TValueType, TKeyType>
            (this TmphList<TValueType> list, Func<TValueType, TKeyType> getKey, Func<TKeyType, TKeyType, int> comparer,
                out TValueType value)
        {
            if (list != null) list.Max(getKey, comparer, out value);
            value = default(TValueType);
            return false;
        }

        /// <summary>
        ///     获取最大值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="list">数组数据</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最大值,失败返回默认空值</returns>
        public static TValueType max<TValueType>(this TmphList<TValueType> list, TValueType nullValue)
            where TValueType : IComparable<TValueType>
        {
            if (list != null)
            {
                TValueType value;
                if (list.Max((left, right) => left.CompareTo(right), out value)) return value;
            }
            return nullValue;
        }

        /// <summary>
        ///     获取最大值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TKeyType">比较键类型</typeparam>
        /// <param name="list">数组数据</param>
        /// <param name="getKey">获取键值</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最大值,失败返回默认空值</returns>
        public static TValueType max<TValueType, TKeyType>(this TmphList<TValueType> list, Func<TValueType, TKeyType> getKey,
            TValueType nullValue)
            where TKeyType : IComparable<TKeyType>
        {
            return list != null ? list.Max(getKey, (left, right) => left.CompareTo(right), nullValue) : nullValue;
        }

        /// <summary>
        ///     获取最大值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TKeyType">比较键类型</typeparam>
        /// <param name="list">数组数据</param>
        /// <param name="getKey">获取键值</param>
        /// <param name="comparer">比较器</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最大值,失败返回默认空值</returns>
        public static TValueType max<TValueType, TKeyType>
            (this TmphList<TValueType> list, Func<TValueType, TKeyType> getKey, Func<TKeyType, TKeyType, int> comparer,
                TValueType nullValue)
        {
            return list != null ? list.Max(getKey, comparer, nullValue) : nullValue;
        }

        /// <summary>
        ///     获取最大键值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TKeyType">比较键类型</typeparam>
        /// <param name="list">数组数据</param>
        /// <param name="getKey">获取键值</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最大键值,失败返回默认空值</returns>
        public static TKeyType maxKey<TValueType, TKeyType>(this TmphList<TValueType> list, Func<TValueType, TKeyType> getKey,
            TKeyType nullValue)
            where TKeyType : IComparable<TKeyType>
        {
            return list != null ? list.MaxKey(getKey, (left, right) => left.CompareTo(right), nullValue) : nullValue;
        }

        /// <summary>
        ///     获取最大键值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TKeyType">比较键类型</typeparam>
        /// <param name="list">数组数据</param>
        /// <param name="getKey">获取键值</param>
        /// <param name="comparer">比较器</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最大键值,失败返回默认空值</returns>
        public static TKeyType maxKey<TValueType, TKeyType>
            (this TmphList<TValueType> list, Func<TValueType, TKeyType> getKey, Func<TKeyType, TKeyType, int> comparer,
                TKeyType nullValue)
        {
            return list != null ? list.MaxKey(getKey, comparer, nullValue) : nullValue;
        }

        /// <summary>
        ///     获取最小值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="list">数组数据</param>
        /// <param name="comparer">比较器</param>
        /// <param name="value">最小值</param>
        /// <returns>是否存在最小值</returns>
        public static bool min<TValueType>(this TmphList<TValueType> list, Func<TValueType, TValueType, int> comparer,
            out TValueType value)
        {
            if (list != null) return list.Min(comparer, out value);
            value = default(TValueType);
            return false;
        }

        /// <summary>
        ///     获取最小值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TKeyType">比较键类型</typeparam>
        /// <param name="list">数组数据</param>
        /// <param name="getKey">获取键值</param>
        /// <param name="comparer">比较器</param>
        /// <param name="value">最小值</param>
        /// <returns>是否存在最小值</returns>
        public static bool min<TValueType, TKeyType>
            (this TmphList<TValueType> list, Func<TValueType, TKeyType> getKey, Func<TKeyType, TKeyType, int> comparer,
                out TValueType value)
        {
            if (list != null) list.Min(getKey, comparer, out value);
            value = default(TValueType);
            return false;
        }

        /// <summary>
        ///     获取最小值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="list">数组数据</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最小值,失败返回默认空值</returns>
        public static TValueType min<TValueType>(this TmphList<TValueType> list, TValueType nullValue)
            where TValueType : IComparable<TValueType>
        {
            if (list != null)
            {
                TValueType value;
                if (list.Min((left, right) => left.CompareTo(right), out value)) return value;
            }
            return nullValue;
        }

        /// <summary>
        ///     获取最小值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TKeyType">比较键类型</typeparam>
        /// <param name="list">数组数据</param>
        /// <param name="getKey">获取键值</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最小值,失败返回默认空值</returns>
        public static TValueType min<TValueType, TKeyType>(this TmphList<TValueType> list, Func<TValueType, TKeyType> getKey,
            TValueType nullValue)
            where TKeyType : IComparable<TKeyType>
        {
            return list != null ? list.Min(getKey, (left, right) => left.CompareTo(right), nullValue) : nullValue;
        }

        /// <summary>
        ///     获取最小值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TKeyType">比较键类型</typeparam>
        /// <param name="list">数组数据</param>
        /// <param name="getKey">获取键值</param>
        /// <param name="comparer">比较器</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最小值,失败返回默认空值</returns>
        public static TValueType min<TValueType, TKeyType>
            (this TmphList<TValueType> list, Func<TValueType, TKeyType> getKey, Func<TKeyType, TKeyType, int> comparer,
                TValueType nullValue)
        {
            return list != null ? list.Min(getKey, comparer, nullValue) : nullValue;
        }

        /// <summary>
        ///     获取最小键值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TKeyType">比较键类型</typeparam>
        /// <param name="list">数组数据</param>
        /// <param name="getKey">获取键值</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最小键值,失败返回默认空值</returns>
        public static TKeyType minKey<TValueType, TKeyType>(this TmphList<TValueType> list, Func<TValueType, TKeyType> getKey,
            TKeyType nullValue)
            where TKeyType : IComparable<TKeyType>
        {
            return list != null ? list.MinKey(getKey, (left, right) => left.CompareTo(right), nullValue) : nullValue;
        }

        /// <summary>
        ///     获取最小键值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TKeyType">比较键类型</typeparam>
        /// <param name="list">数组数据</param>
        /// <param name="getKey">获取键值</param>
        /// <param name="comparer">比较器</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最小键值,失败返回默认空值</returns>
        public static TKeyType minKey<TValueType, TKeyType>
            (this TmphList<TValueType> list, Func<TValueType, TKeyType> getKey, Func<TKeyType, TKeyType, int> comparer,
                TKeyType nullValue)
        {
            return list != null ? list.MinKey(getKey, comparer, nullValue) : nullValue;
        }

        /// <summary>
        ///     数据分组
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TKeyType">比较键类型</typeparam>
        /// <param name="list">单向动态数组</param>
        /// <param name="getKey">键值获取器</param>
        /// <returns>分组数据</returns>
        public static Dictionary<TKeyType, TmphList<TValueType>> group<TValueType, TKeyType>(this TmphList<TValueType> list,
            Func<TValueType, TKeyType> getKey)
            where TKeyType : IEquatable<TKeyType>
        {
            return list != null ? list.Group(getKey) : null;
        }

        /// <summary>
        ///     数据去重
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="arrayType">目标数组类型</typeparam>
        /// <param name="list">单向动态数组</param>
        /// <param name="getValue">数据获取器</param>
        /// <returns>目标集合</returns>
        public static TmphSubArray<arrayType> distinct<TValueType, arrayType>(this TmphList<TValueType> list,
            Func<TValueType, arrayType> getValue)
        {
            return list != null ? list.Distinct(getValue) : default(TmphSubArray<arrayType>);
        }

        /// <summary>
        ///     转换数据集合
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="arrayType">目标数据类型</typeparam>
        /// <param name="list">单向动态数组</param>
        /// <param name="getValue">数据转换器</param>
        /// <returns>数据集合</returns>
        public static arrayType[] getArray<TValueType, arrayType>(this TmphList<TValueType> list,
            Func<TValueType, arrayType> getValue)
        {
            return list != null ? list.GetArray(getValue) : TmphNullValue<arrayType>.Array;
        }

        /// <summary>
        ///     转换键值对数组
        /// </summary>
        /// <typeparam name="TKeyType">键类型</typeparam>
        /// <typeparam name="TValueType">值类型</typeparam>
        /// <param name="list">单向动态数组</param>
        /// <param name="getKey">键值获取器</param>
        /// <returns>键值对数组</returns>
        public static TmphKeyValue<TKeyType, TValueType>[] getKeyValueArray<TKeyType, TValueType>(this TmphList<TValueType> list,
            Func<TValueType, TKeyType> getKey)
        {
            return list != null ? list.GetKeyValueArray(getKey) : TmphNullValue<TmphKeyValue<TKeyType, TValueType>>.Array;
        }

        /// <summary>
        ///     转换双向动态数组
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="list">单向动态数组</param>
        /// <returns>双向动态数组</returns>
        public static TmphCollection<TValueType> toCollection<TValueType>(this TmphList<TValueType> list)
        {
            return list != null ? list.ToCollection() : null;
        }

        /// <summary>
        ///     连接字符串
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="list">单向动态数组</param>
        /// <param name="toString">字符串转换器</param>
        /// <returns>字符串</returns>
        public static string joinString<TValueType>(this TmphList<TValueType> list, Func<TValueType, string> toString)
        {
            return list != null ? list.JoinString(toString) : null;
        }

        /// <summary>
        ///     连接字符串
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="list">单向动态数组</param>
        /// <param name="toString">字符串转换器</param>
        /// <param name="join">连接串</param>
        /// <returns>字符串</returns>
        public static string joinString<TValueType>(this TmphList<TValueType> list, string join,
            Func<TValueType, string> toString)
        {
            return list != null ? list.JoinString(join, toString) : null;
        }

        /// <summary>
        ///     连接字符串
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="list">单向动态数组</param>
        /// <param name="toString">字符串转换器</param>
        /// <param name="join">连接字符</param>
        /// <returns>字符串</returns>
        public static string joinString<TValueType>(this TmphList<TValueType> list, char join,
            Func<TValueType, string> toString)
        {
            return list != null ? list.JoinString(join, toString) : null;
        }

        /// <summary>
        ///     转换数组
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="list">单向动态数组</param>
        /// <returns>数组</returns>
        public static TValueType[] getArray<TValueType>(this TmphList<TValueType> list)
        {
            return list != null ? list.GetArray() : TmphNullValue<TValueType>.Array;
        }

        /// <summary>
        ///     转换数组
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="list">单向动态数组</param>
        /// <returns>数组</returns>
        public static TValueType[] toArray<TValueType>(this TmphList<TValueType> list)
        {
            return list != null ? list.ToArray() : TmphNullValue<TValueType>.Array;
        }
    }
}