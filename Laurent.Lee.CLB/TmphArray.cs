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
using System.Collections.Generic;

namespace Laurent.Lee.CLB
{
    /// <summary>
    ///     数组扩展操作
    /// </summary>
    public static class TmphArray
    {
        /// <summary>
        ///     获取数组长度
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <returns>null为0</returns>
        public static int length<TValueType>(this TValueType[] array)
        {
            return array != null ? array.Length : 0;
        }

        /// <summary>
        ///     空值转0长度数组
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <returns>非空数组</returns>
        public static TValueType[] notNull<TValueType>(this TValueType[] array)
        {
            return array != null ? array : TmphNullValue<TValueType>.Array;
        }

        /// <summary>
        ///     根据索引位置获取数组元素值
        /// </summary>
        /// <typeparam name="TValueType">值类型</typeparam>
        /// <param name="array">值集合</param>
        /// <param name="index">索引位置</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>数组元素值</returns>
        public static TValueType get<TValueType>(this TValueType[] array, int index, TValueType nullValue)
        {
            return array != null && (uint)index < (uint)array.Length ? array[index] : nullValue;
        }

        /// <summary>
        ///     获取最后一个值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <returns>最后一个值,失败为default(TValueType)</returns>
        public static TValueType lastOrDefault<TValueType>(this TValueType[] array)
        {
            return array != null && array.Length != 0 ? array[array.Length - 1] : default(TValueType);
        }

        /// <summary>
        ///     获取第一个匹配值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>第一个匹配值,失败为default(TValueType)</returns>
        public static TValueType firstOrDefault<TValueType>(this TValueType[] array, Func<TValueType, bool> isValue)
        {
            if (array != null)
            {
                if (isValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                foreach (var value in array)
                {
                    if (isValue(value)) return value;
                }
            }
            return default(TValueType);
        }

        /// <summary>
        ///     获取第一个匹配值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="isValue">数据匹配器</param>
        /// <param name="index">起始位置</param>
        /// <returns>第一个匹配值,失败为default(TValueType)</returns>
        public static TValueType firstOrDefault<TValueType>(this TValueType[] array, Func<TValueType, bool> isValue,
            int index)
        {
            if (array != null && (uint)index < (uint)array.Length)
            {
                if (isValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                while (index != array.Length)
                {
                    if (isValue(array[index])) return array[index];
                    ++index;
                }
            }
            return default(TValueType);
        }

        /// <summary>
        ///     获取匹配数据位置
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="value">匹配数据</param>
        /// <returns>匹配位置,失败为-1</returns>
        public static int indexOf<TValueType>(this TValueType[] array, TValueType value)
        {
            return array != null ? Array.IndexOf(array, value) : -1;
        }

        /// <summary>
        ///     获取匹配数据位置
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>匹配位置,失败为-1</returns>
        public static int indexOf<TValueType>(this TValueType[] array, Func<TValueType, bool> isValue)
        {
            if (array.length() != 0)
            {
                for (var index = 0; index != array.Length; ++index)
                {
                    if (isValue(array[index])) return index;
                }
            }
            return -1;
        }

        /// <summary>
        ///     判断是否存在匹配值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="value">匹配值</param>
        /// <returns>是否存在匹配值</returns>
        public static bool any<TValueType>(this TValueType[] array, TValueType value)
        {
            return array != null && Array.IndexOf(array, value) != -1;
        }

        /// <summary>
        ///     判断是否存在匹配值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>是否存在匹配值</returns>
        public static bool any<TValueType>(this TValueType[] array, Func<TValueType, bool> isValue)
        {
            if (array != null)
            {
                if (isValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                foreach (var value in array)
                {
                    if (isValue(value)) return true;
                }
            }
            return false;
        }

        /// <summary>
        ///     获取匹配数量
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>匹配数量</returns>
        public static int count<TValueType>(this TValueType[] array, Func<TValueType, bool> isValue)
        {
            var count = 0;
            if (array != null)
            {
                if (isValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                foreach (var value in array)
                {
                    if (isValue(value)) ++count;
                }
            }
            return count;
        }

        /// <summary>
        ///     复制数组
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">待复制数组</param>
        /// <returns>复制后的新数组</returns>
        public static TValueType[] copy<TValueType>(this TValueType[] array)
        {
            if (array.length() != 0)
            {
                var newValues = new TValueType[array.Length];
                Array.Copy(array, 0, newValues, 0, array.Length);
                return newValues;
            }
            return TmphNullValue<TValueType>.Array;
        }

        /// <summary>
        ///     翻转数组
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <returns>翻转后的数组</returns>
        public static TValueType[] reverse<TValueType>(this TValueType[] array)
        {
            if (array.length() != 0)
            {
                Array.Reverse(array);
                return array;
            }
            return TmphNullValue<TValueType>.Array;
        }

        /// <summary>
        ///     添加数据
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="value">添加的数据</param>
        /// <returns>添加数据的数组</returns>
        public static TValueType[] getAdd<TValueType>(this TValueType[] array, TValueType value)
        {
            if (array != null)
            {
                var newValues = new TValueType[array.Length + 1];
                Array.Copy(array, 0, newValues, 0, array.Length);
                newValues[array.Length] = value;
                return newValues;
            }
            return new[] { value };
        }

        /// <summary>
        ///     获取前端子段集合
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">原数组</param>
        /// <param name="count">数量</param>
        /// <returns>子段集合</returns>
        public static TmphSubArray<TValueType> left<TValueType>(this TValueType[] array, int count)
        {
            return array != null
                ? TmphSubArray<TValueType>.Unsafe(array, 0, count <= array.Length ? count : array.Length)
                : default(TmphSubArray<TValueType>);
        }

        /// <summary>
        ///     获取子段集合
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="index">起始位置</param>
        /// <returns>子段集合</returns>
        public static TmphSubArray<TValueType> sub<TValueType>(this TValueType[] array, int index)
        {
            return array != null ? sub(array, index, -1) : default(TmphSubArray<TValueType>);
        }

        /// <summary>
        ///     获取子段集合
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="index">起始位置</param>
        /// <param name="count">数量,小于0表示所有</param>
        /// <returns>子段集合</returns>
        public static TmphSubArray<TValueType> sub<TValueType>(this TValueType[] array, int index, int count)
        {
            if (array != null)
            {
                var range = new TmphRange(array.Length, index, count);
                return TmphSubArray<TValueType>.Unsafe(array, range.SkipCount, range.GetCount);
            }
            return default(TmphSubArray<TValueType>);
        }

        /// <summary>
        ///     取子集合
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="index">起始位置</param>
        /// <param name="count">数量</param>
        /// <returns>子集合</returns>
        public static TValueType[] getSub<TValueType>(this TValueType[] array, int index, int count)
        {
            return array.sub(index, count).GetArray();
        }

        /// <summary>
        ///     获取分页字段数组
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="arrayType">目标数组类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="pageSize">分页尺寸</param>
        /// <param name="currentPage">页号</param>
        /// <param name="getValue">数据转换器</param>
        /// <returns>分页字段数组</returns>
        public static arrayType[] getPage<TValueType, arrayType>
            (this TValueType[] array, int pageSize, int currentPage, Func<TValueType, arrayType> getValue)
        {
            var page = new TmphPage(array.length(), pageSize, currentPage);
            return array.sub(page.SkipCount, page.CurrentPageSize).GetArray(getValue);
        }

        /// <summary>
        ///     获取分页字段数组
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <typeparam name="arrayType">目标数组类型</typeparam>
        /// <param name="array"></param>
        /// <param name="pageSize">分页尺寸</param>
        /// <param name="currentPage">页号</param>
        /// <param name="getValue"></param>
        /// <returns>分页字段数组</returns>
        public static TValueType[] getPage<TValueType>(this TValueType[] array, int pageSize, int currentPage)
        {
            var page = new TmphPage(array.length(), pageSize, currentPage);
            return array.getSub(page.SkipCount, page.CurrentPageSize);
        }

        /// <summary>
        ///     移除第一个匹配数据数据
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="value">待移除的数据</param>
        /// <returns>移除数据后的数组</returns>
        public static TValueType[] removeFirst<TValueType>(this TValueType[] array, TValueType value)
            where TValueType : IComparable<TValueType>
        {
            if (array != null)
            {
                var index = Array.IndexOf(array, value);
                if (index != -1) return Unsafe.TmphArray.GetRemoveAt(array, index);
                return array;
            }
            return TmphNullValue<TValueType>.Array;
        }

        /// <summary>
        ///     移除第一个匹配数据数据
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>移除数据后的数组</returns>
        public static TValueType[] removeFirst<TValueType>(this TValueType[] array, Func<TValueType, bool> isValue)
        {
            if (isValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            if (array != null)
            {
                var index = 0;
                while (index != array.Length && !isValue(array[index])) ++index;
                if (index != array.Length) return Unsafe.TmphArray.GetRemoveAt(array, index);
            }
            return array.notNull();
        }

        /// <summary>
        ///     替换数据
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="value">新值</param>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>替换数据后的数组</returns>
        public static TValueType[] replaceFirst<TValueType>(this TValueType[] array, TValueType value,
            Func<TValueType, bool> isValue)
        {
            if (array != null)
            {
                if (isValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                for (var index = 0; index != array.Length; ++index)
                {
                    if (isValue(array[index])) array[index] = value;
                }
            }
            return array.notNull();
        }

        /// <summary>
        ///     移动数据块
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">待处理数组</param>
        /// <param name="index">原始数据位置</param>
        /// <param name="writeIndex">目标数据位置</param>
        /// <param name="count">移动数据数量</param>
        public static void move<TValueType>(this TValueType[] array, int index, int writeIndex, int count)
        {
            if (count > 0)
            {
                var writeEndIndex = writeIndex + count;
                if (index >= 0 && writeEndIndex <= array.Length)
                {
                    var endIndex = index + count;
                    if (index < writeIndex && endIndex > writeIndex)
                    {
                        while (endIndex != index) array[--writeEndIndex] = array[--endIndex];
                    }
                    else if (writeIndex >= 0 && endIndex <= array.Length)
                        Array.Copy(array, index, array, writeIndex, count);
                    else TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
                }
                else TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
            }
            else if (count != 0) TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
        }

        /// <summary>
        ///     转换成数组子串
        /// </summary>
        /// <typeparam name="TValueType">枚举值类型</typeparam>
        /// <param name="array">值集合</param>
        /// <returns>单向列表</returns>
        public static TmphSubArray<TValueType> toSubArray<TValueType>(this TValueType[] array)
        {
            if (array == null) return default(TmphSubArray<TValueType>);
            return TmphSubArray<TValueType>.Unsafe(array, 0, array.Length);
        }

        /// <summary>
        ///     转换成数组子串
        /// </summary>
        /// <typeparam name="TValueType">枚举值类型</typeparam>
        /// <param name="array">值集合</param>
        /// <param name="index">起始位置</param>
        /// <param name="length">长度</param>
        /// <returns>单向列表</returns>
        public static TmphSubArray<TValueType> toSubArray<TValueType>(this TValueType[] array, int index, int length)
        {
            return new TmphSubArray<TValueType>(array, index, length);
        }

        /// <summary>
        ///     根据集合内容返回单向列表
        /// </summary>
        /// <typeparam name="TValueType">枚举值类型</typeparam>
        /// <param name="array">值集合</param>
        /// <returns>单向列表</returns>
        public static TmphList<TValueType> toList<TValueType>(this TValueType[] array)
        {
            return new TmphList<TValueType>(array, true);
        }

        /// <summary>
        ///     根据集合内容返回双向列表
        /// </summary>
        /// <typeparam name="TValueType">枚举值类型</typeparam>
        /// <param name="array">值集合</param>
        /// <returns>双向列表</returns>
        public static TmphCollection<TValueType> toCollection<TValueType>(this TValueType[] array)
        {
            return new TmphCollection<TValueType>(array, true);
        }

        /// <summary>
        ///     转换HASH
        /// </summary>
        /// <typeparam name="TValueType">枚举值类型</typeparam>
        /// <param name="array">值集合</param>
        /// <returns>HASH</returns>
        public static HashSet<TValueType> getHash<TValueType>(this TValueType[] array)
        {
            if (array != null)
            {
                var hash = TmphHashSet<TValueType>.Create();
                foreach (var value in array) hash.Add(value);
                return hash;
            }
            return null;
        }

        /// <summary>
        ///     数据去重
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="arrayType">目标数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="getValue">数据获取器</param>
        /// <returns>目标数据集合</returns>
        public static TmphSubArray<arrayType> distinct<TValueType, arrayType>(this TValueType[] array,
            Func<TValueType, arrayType> getValue)
        {
            if (array != null)
            {
                if (getValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                var newValues = new arrayType[array.Length];
                var hash = TmphHashSet<TValueType>.Create();
                var count = 0;
                foreach (var value in array)
                {
                    if (!hash.Contains(value))
                    {
                        newValues[count++] = getValue(value);
                        hash.Add(value);
                    }
                }
                return TmphSubArray<arrayType>.Unsafe(newValues, 0, count);
            }
            return default(TmphSubArray<arrayType>);
        }

        /// <summary>
        ///     转换HASH
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>HASH</returns>
        public static HashSet<TValueType> getHash<TValueType>(this TValueType[] array, Func<TValueType, bool> isValue)
        {
            if (array != null)
            {
                if (isValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                var hash = TmphHashSet<TValueType>.Create();
                foreach (var value in array)
                {
                    if (isValue(value)) hash.Add(value);
                }
                return hash;
            }
            return null;
        }

        /// <summary>
        ///     转换HASH
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="hashType">目标数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="getValue">数据获取器</param>
        /// <returns>HASH</returns>
        public static HashSet<hashType> getHash<TValueType, hashType>(this TValueType[] array,
            Func<TValueType, hashType> getValue)
        {
            if (array != null)
            {
                if (getValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                var hash = TmphHashSet<hashType>.Create();
                foreach (var value in array) hash.Add(getValue(value));
                return hash;
            }
            return null;
        }

        /// <summary>
        ///     将数组转化为字典
        /// </summary>
        /// <typeparam name="TKeyType">关键字类型</typeparam>
        /// <typeparam name="TValueType">默认值类型</typeparam>
        /// <param name="array">待转化的数组</param>
        /// <param name="defaultValue">默认值数组</param>
        /// <returns>数组转化后字典</returns>
        public static Dictionary<TKeyType, TValueType> getDictionary<TKeyType, TValueType>(this TKeyType[] array,
            TValueType[] defaultValue)
            where TKeyType : IEquatable<TKeyType>
        {
            if (array != null)
            {
                var dictionary = TmphDictionary<TKeyType>.Create<TValueType>(array.Length << 1);
                if (defaultValue.length() == array.Length)
                {
                    var index = 0;
                    foreach (var key in array) dictionary.Add(key, defaultValue[index++]);
                }
                return dictionary;
            }
            return null;
        }

        /// <summary>
        ///     将数组转化为字典
        /// </summary>
        /// <typeparam name="TKeyType">关键字类型</typeparam>
        /// <typeparam name="TValueType">默认值类型</typeparam>
        /// <param name="array">待转化的数组</param>
        /// <param name="defaultValue">默认值数组</param>
        /// <returns>数组转化后字典</returns>
        public static Dictionary<TKeyType, TValueType> getDictionary<TKeyType, TValueType>(this TKeyType[] array,
            TValueType defaultValue)
            where TKeyType : IEquatable<TKeyType>
        {
            if (array != null)
            {
                var dictionary = TmphDictionary<TKeyType>.Create<TValueType>(array.Length << 1);
                foreach (var key in array) dictionary.Add(key, defaultValue);
                return dictionary;
            }
            return null;
        }

        /// <summary>
        ///     将键值数组转化为字典
        /// </summary>
        /// <typeparam name="keyValues">关键字类型</typeparam>
        /// <typeparam name="TValueType">默认值类型</typeparam>
        /// <param name="array">键值数组</param>
        /// <returns>数组转化后字典</returns>
        public static Dictionary<TKeyType, TValueType> getDictionary<TKeyType, TValueType>(
            this TmphKeyValue<TKeyType, TValueType>[] array)
            where TKeyType : IEquatable<TKeyType>
        {
            if (array != null)
            {
                var dictionary = TmphDictionary<TKeyType>.Create<TValueType>(array.Length << 1);
                foreach (var value in array) dictionary.Add(value.Key, value.Value);
                return dictionary;
            }
            return null;
        }

        /// <summary>
        ///     数据分组
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TKeyType">分组键类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="getKey">获取键值</param>
        /// <returns>分组数据</returns>
        public static Dictionary<TKeyType, TmphList<TValueType>> group<TValueType, TKeyType>(this TValueType[] array,
            Func<TValueType, TKeyType> getKey)
            where TKeyType : IEquatable<TKeyType>
        {
            if (array != null)
            {
                if (getKey == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                var newValues = TmphDictionary<TKeyType>.Create<TmphList<TValueType>>(array.Length << 1);
                TmphList<TValueType> list;
                foreach (var value in array)
                {
                    var key = getKey(value);
                    if (!newValues.TryGetValue(key, out list)) newValues[key] = list = new TmphList<TValueType>();
                    list.Add(value);
                }
                return newValues;
            }
            return null;
        }

        /// <summary>
        ///     分组计数
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TKeyType">分组键类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="getKey">键值获取器</param>
        /// <returns>分组计数</returns>
        public static Dictionary<TKeyType, int> groupCount<TValueType, TKeyType>(this TValueType[] array,
            Func<TValueType, TKeyType> getKey)
            where TKeyType : IEquatable<TKeyType>
        {
            if (array != null)
            {
                if (getKey == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                var dictionary = TmphDictionary<TKeyType>.Create<int>(array.Length);
                int count;
                foreach (var value in array)
                {
                    var key = getKey(value);
                    if (dictionary.TryGetValue(key, out count)) dictionary[key] = count + 1;
                    else dictionary.Add(key, 1);
                }
                return dictionary;
            }
            return null;
        }

        /// <summary>
        ///     HASH统计
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <returns>HASH统计</returns>
        public static Dictionary<TValueType, int> valueCount<TValueType>(this TValueType[] array)
        {
            if (array != null)
            {
                int count;
                var dictionary = TmphDictionary.CreateAny<TValueType, int>(array.Length << 1);
                foreach (var value in array)
                {
                    if (dictionary.TryGetValue(value, out count)) dictionary[value] = ++count;
                    else dictionary.Add(value, 1);
                }
                return dictionary;
            }
            return null;
        }

        /// <summary>
        ///     求交集
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="left">左侧数据</param>
        /// <param name="right">右侧数据</param>
        /// <returns>数据交集</returns>
        public static TmphSubArray<TValueType> intersect<TValueType>(this TValueType[] left, TValueType[] right)
        {
            int leftLength = left.length();
            if (leftLength != 0)
            {
                int rightLength = right.length();
                if (rightLength != 0)
                {
                    TValueType[] min = leftLength <= rightLength ? left : right, values = new TValueType[min.Length];
                    var hash = TmphHashSet<TValueType>.Create();
                    var count = 0;
                    foreach (var value in leftLength <= rightLength ? right : left)
                    {
                        if (hash.Contains(value)) values[count++] = value;
                    }
                    return TmphSubArray<TValueType>.Unsafe(values, 0, count);
                }
            }
            return default(TmphSubArray<TValueType>);
        }

        /// <summary>
        ///     遍历foreach
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="method">调用函数</param>
        /// <returns>数组数据</returns>
        public static TValueType[] each<TValueType>(this TValueType[] array, Action<TValueType> method)
        {
            if (array != null)
            {
                if (method == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                foreach (var value in array) method(value);
            }
            return array.notNull();
        }

        /// <summary>
        ///     根据集合内容返回新的数据集合
        /// </summary>
        /// <typeparam name="TValueType">枚举值类型</typeparam>
        /// <typeparam name="arrayType">返回数组类型</typeparam>
        /// <param name="values">值集合</param>
        /// <param name="getValue">获取数组值的委托</param>
        /// <returns>数据集合</returns>
        public static IEnumerable<arrayType> getEnumerable<TValueType, arrayType>(this TValueType[] values,
            Func<TValueType, arrayType> getValue)
        {
            if (values != null)
            {
                foreach (var value in values) yield return getValue(value);
            }
        }

        /// <summary>
        ///     数据转换
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="arrayType">目标数组类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="getValue">数据获取器</param>
        /// <returns>目标数组</returns>
        public static arrayType[] getArray<TValueType, arrayType>(this TValueType[] array,
            Func<TValueType, arrayType> getValue)
        {
            if (array.length() != 0)
            {
                if (getValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                var newValues = new arrayType[array.Length];
                var index = 0;
                foreach (var value in array) newValues[index++] = getValue(value);
                return newValues;
            }
            return TmphNullValue<arrayType>.Array;
        }

        /// <summary>
        ///     获取键值对数组
        /// </summary>
        /// <typeparam name="TKeyType">键类型</typeparam>
        /// <typeparam name="TValueType">值类型</typeparam>
        /// <param name="values">键值对数组</param>
        /// <param name="getKey">键值获取器</param>
        /// <returns>键值对数组</returns>
        public static TmphKeyValue<TKeyType, TValueType>[] getKeyValueArray<TKeyType, TValueType>(this TValueType[] array,
            Func<TValueType, TKeyType> getKey)
        {
            if (array.length() != 0)
            {
                if (getKey == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                var newValues = new TmphKeyValue<TKeyType, TValueType>[array.Length];
                var index = 0;
                foreach (var value in array) newValues[index++].Set(getKey(value), value);
                return newValues;
            }
            return TmphNullValue<TmphKeyValue<TKeyType, TValueType>>.Array;
        }

        /// <summary>
        ///     数组转换
        /// </summary>
        /// <typeparam name="TValueType">目标数组类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <returns>目标数组</returns>
        public static TValueType[] toArray<TValueType>(this Array array)
        {
            return array != null ? array as TValueType[] : TmphNullValue<TValueType>.Array;
        }

        /// <summary>
        ///     数据转换
        /// </summary>
        /// <typeparam name="TValueType">目标数组类型</typeparam>
        /// <typeparam name="arrayType">目标数组类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="getValue">一次处理两个数据</param>
        /// <returns>目标数组</returns>
        public static arrayType[] getArray<TValueType, arrayType>(this TValueType[] array,
            Func<TValueType, TValueType, arrayType> getValue)
        {
            if (getValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            if (array.length() != 0)
            {
                int length = array.Length, index = (length + 1) >> 1;
                var newValues = new arrayType[index];
                if ((length & 1) != 0) newValues[--index] = getValue(array[--length], default(TValueType));
                while (--index >= 0)
                {
                    var right = array[--length];
                    newValues[index] = getValue(array[--length], right);
                }
                return newValues;
            }
            return TmphNullValue<arrayType>.Array;
        }

        /// <summary>
        ///     连接数组
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组集合</param>
        /// <returns>连接后的数组</returns>
        private static TValueType[] GetArray<TValueType>(TValueType[][] array)
        {
            var length = 0;
            foreach (var value in array)
            {
                if (value != null) length += value.Length;
            }
            if (length != 0)
            {
                var newValues = new TValueType[length];
                length = 0;
                foreach (var value in array)
                {
                    if (value != null)
                    {
                        value.CopyTo(newValues, length);
                        length += value.Length;
                    }
                }
                return newValues;
            }
            return TmphNullValue<TValueType>.Array;
        }

        /// <summary>
        ///     连接数组
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组集合</param>
        /// <returns>连接后的数组</returns>
        public static TValueType[] toArray<TValueType>(this TValueType[][] array)
        {
            if (array.length() != 0)
            {
                return array.Length == 1 ? array[0].notNull() : GetArray(array);
            }
            return TmphNullValue<TValueType>.Array;
        }

        /// <summary>
        ///     连接数组
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组集合</param>
        /// <returns>连接后的数组</returns>
        public static TValueType[] getArray<TValueType>(this TValueType[][] array)
        {
            if (array.length() != 0)
            {
                if (array.Length != 1) return GetArray(array);
                return array[0].copy();
            }
            return TmphNullValue<TValueType>.Array;
        }

        /// <summary>
        ///     连接数组
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组集合</param>
        /// <param name="addValues">数组集合</param>
        /// <returns>连接后的数组</returns>
        public static TValueType[] concat<TValueType>(this TValueType[] array, TValueType[] addValues)
        {
            return getArray(new[] { array, addValues });
        }

        /// <summary>
        ///     连接数组
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组集合</param>
        /// <returns>连接后的数组</returns>
        public static TValueType[] concat<TValueType>(params TValueType[][] array)
        {
            return array.getArray();
        }

        /// <summary>
        ///     连接数组
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="arrayType">目标数组类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="getValue">数据获取器</param>
        /// <returns>目标数组</returns>
        public static arrayType[] concat<TValueType, arrayType>(this TValueType[] array,
            Func<TValueType, arrayType[]> getValue)
        {
            if (getValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            return array.getArray(value => getValue(value)).toArray();
        }

        /// <summary>
        ///     连接数组
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="arrayType">目标数组类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="getValue">数据获取器</param>
        /// <returns>目标数组</returns>
        public static TmphSubArray<arrayType> concat<TValueType, arrayType>(this TValueType[] array,
            Func<TValueType, TmphSubArray<arrayType>> getValue)
        {
            if (getValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            if (array != null)
            {
                var values = new TmphSubArray<arrayType>();
                foreach (var value in array) values.Add(getValue(value));
                return values;
            }
            return default(TmphSubArray<arrayType>);
        }

        /// <summary>
        ///     分割数组
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="count">子数组长度</param>
        /// <returns>分割后的数组集合</returns>
        public static TmphSubArray<TValueType>[] split<TValueType>(this TValueType[] array, int count)
        {
            if (array != null && array.Length != 0 && count > 0)
            {
                var length = (array.Length + count - 1) / count;
                var newValues = new TmphSubArray<TValueType>[length];
                int index = (--length) * count, lastCount = array.Length - index;
                if (lastCount != 0) newValues[length--].UnsafeSet(array, index, lastCount);
                while (index != 0) newValues[length--].UnsafeSet(array, index -= count, count);
                return newValues;
            }
            return TmphNullValue<TmphSubArray<TValueType>>.Array;
        }

        /// <summary>
        ///     分割数组
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="count">子数组长度</param>
        /// <returns>分割后的数组集合</returns>
        public static TValueType[][] getSplit<TValueType>(this TValueType[] array, int count)
        {
            if (array != null && count > 0)
            {
                if (count < array.Length)
                {
                    int length = (array.Length + count - 1) / count, copyIndex = 0;
                    var newValues = new TValueType[length--][];
                    for (var index = 0; index != length; ++index, copyIndex += count)
                    {
                        Array.Copy(array, copyIndex, newValues[index] = new TValueType[count], 0, count);
                    }
                    Array.Copy(array, copyIndex, newValues[length] = new TValueType[count = array.Length - copyIndex], 0,
                        count);
                    return newValues;
                }
                if (array.Length != 0) return new[] { array };
            }
            return TmphNullValue<TValueType[]>.Array;
        }

        /// <summary>
        ///     转换键值对集合
        /// </summary>
        /// <typeparam name="TKeyType">键值类型</typeparam>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">键值数组</param>
        /// <param name="values">数组数据</param>
        /// <returns>键值对数组</returns>
        public static TmphKeyValue<TKeyType, TValueType>[] getKeyValue<TKeyType, TValueType>(this TKeyType[] array,
            TValueType[] values)
        {
            int length = array.length();
            if (length != values.length()) TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
            if (length != 0)
            {
                var newValues = new TmphKeyValue<TKeyType, TValueType>[array.Length];
                var index = 0;
                foreach (var key in array)
                {
                    newValues[index].Set(key, values[index]);
                    ++index;
                }
                return newValues;
            }
            return TmphNullValue<TmphKeyValue<TKeyType, TValueType>>.Array;
        }

        /// <summary>
        ///     获取匹配集合
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>匹配集合</returns>
        public static TmphSubArray<TValueType> getFind<TValueType>(this TValueType[] array, Func<TValueType, bool> isValue)
        {
            if (isValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            if (array != null)
            {
                var length = array.Length;
                if (length != 0)
                {
                    var newValues = new TValueType[array.Length < sizeof(int) ? sizeof(int) : length];
                    length = 0;
                    foreach (var value in array)
                    {
                        if (isValue(value)) newValues[length++] = value;
                    }
                    return TmphSubArray<TValueType>.Unsafe(newValues, 0, length);
                }
            }
            return default(TmphSubArray<TValueType>);
        }

        /// <summary>
        ///     获取匹配数组
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>匹配数组</returns>
        public static unsafe TValueType[] getFindArray<TValueType>(this TValueType[] array, Func<TValueType, bool> isValue)
        {
            if (isValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            int length = array.length();
            if (length != 0)
            {
                var pool = TmphMemoryPool.GetDefaultPool(length = ((length + 31) >> 5) << 2);
                var data = pool.Get(length);
                try
                {
                    fixed (byte* dataFixed = data)
                    {
                        Array.Clear(data, 0, length);
                        return GetFindArray(array, isValue, new TmphFixedMap(dataFixed));
                    }
                }
                finally
                {
                    pool.Push(ref data);
                }
            }
            return TmphNullValue<TValueType>.Array;
        }

        /// <summary>
        ///     获取匹配数组
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="isValue">数据匹配器</param>
        /// <param name="map">匹配结果位图</param>
        /// <returns>匹配数组</returns>
        private static TValueType[] GetFindArray<TValueType>(TValueType[] array, Func<TValueType, bool> isValue,
            TmphFixedMap map)
        {
            var length = 0;
            for (var index = 0; index != array.Length; ++index)
            {
                if (isValue(array[index]))
                {
                    ++length;
                    map.Set(index);
                }
            }
            if (length != 0)
            {
                var newValues = new TValueType[length];
                for (var index = array.Length; length != 0;)
                {
                    if (map.Get(--index)) newValues[--length] = array[index];
                }
                return newValues;
            }
            return TmphNullValue<TValueType>.Array;
        }

        /// <summary>
        ///     获取匹配数组
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="arrayType">目标数组类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="isValue">数据匹配器</param>
        /// <param name="getValue">数据获取器</param>
        /// <returns>匹配数组</returns>
        public static unsafe arrayType[] getFindArray<TValueType, arrayType>
            (this TValueType[] array, Func<TValueType, bool> isValue, Func<TValueType, arrayType> getValue)
        {
            if (isValue == null || getValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            int length = array.length();
            if (length != 0)
            {
                var pool = TmphMemoryPool.GetDefaultPool(length = ((length + 31) >> 5) << 2);
                var data = pool.Get(length);
                try
                {
                    fixed (byte* dataFixed = data)
                    {
                        Array.Clear(data, 0, length);
                        return GetFindArray(array, isValue, getValue, new TmphFixedMap(dataFixed));
                    }
                }
                finally
                {
                    pool.Push(ref data);
                }
            }
            return TmphNullValue<arrayType>.Array;
        }

        /// <summary>
        ///     获取匹配数组
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="arrayType">目标数组类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="isValue">数据匹配器</param>
        /// <param name="getValue">数据获取器</param>
        /// <param name="map">匹配结果位图</param>
        /// <returns>匹配数组</returns>
        private static arrayType[] GetFindArray<TValueType, arrayType>
            (TValueType[] array, Func<TValueType, bool> isValue, Func<TValueType, arrayType> getValue, TmphFixedMap map)
        {
            var length = 0;
            for (var index = 0; index != array.Length; ++index)
            {
                if (isValue(array[index]))
                {
                    ++length;
                    map.Set(index);
                }
            }
            if (length != 0)
            {
                var newValues = new arrayType[length];
                for (var index = array.Length; length != 0;)
                {
                    if (map.Get(--index)) newValues[--length] = getValue(array[index]);
                }
                return newValues;
            }
            return TmphNullValue<arrayType>.Array;
        }

        /// <summary>
        ///     排序
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">待排序数组</param>
        /// <param name="comparer">比较器</param>
        /// <returns>排序后的数组</returns>
        public static TValueType[] sort<TValueType>(this TValueType[] array, Func<TValueType, TValueType, int> comparer)
        {
            TmphQuickSort.Sort(array, comparer);
            return array.notNull();
        }

        /// <summary>
        ///     排序
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">待排序数组</param>
        /// <param name="comparer">比较器</param>
        /// <returns>排序后的新数组</returns>
        public static TValueType[] getSort<TValueType>(this TValueType[] array, Func<TValueType, TValueType, int> comparer)
        {
            return TmphQuickSort.GetSort(array, comparer);
        }

        /// <summary>
        ///     排序
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">待排序数组</param>
        /// <param name="comparer">比较器</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">数量</param>
        public static TValueType[] sort<TValueType>(this TValueType[] array, Func<TValueType, TValueType, int> comparer,
            int startIndex, int count)
        {
            TmphQuickSort.Sort(array, comparer, startIndex, count);
            return array.notNull();
        }

        /// <summary>
        ///     排序
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">待排序数组</param>
        /// <param name="comparer">比较器</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">数量</param>
        /// <returns>排序后的新数组</returns>
        public static TValueType[] getSort<TValueType>(this TValueType[] array, Func<TValueType, TValueType, int> comparer,
            int startIndex, int count)
        {
            return TmphQuickSort.GetSort(array, comparer, startIndex, count);
        }

        /// <summary>
        ///     范围排序
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="value">待排序数组</param>
        /// <param name="comparer">比较器</param>
        /// <param name="skipCount">跳过记录数</param>
        /// <param name="getCount">获取记录数,小于0表示所有</param>
        /// <returns>排序范围数组</returns>
        public static TmphSubArray<TValueType> rangeSort<TValueType>
            (this TValueType[] array, Func<TValueType, TValueType, int> comparer, int skipCount, int getCount)
        {
            return TmphQuickSort.RangeSort(array, comparer, skipCount, getCount);
        }

        /// <summary>
        ///     范围排序
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="value">待排序数组</param>
        /// <param name="comparer">比较器</param>
        /// <param name="skipCount">跳过记录数</param>
        /// <param name="getCount">获取记录数,小于0表示所有</param>
        /// <returns>排序范围数组</returns>
        public static TmphSubArray<TValueType> getRangeSort<TValueType>
            (this TValueType[] array, Func<TValueType, TValueType, int> comparer, int skipCount, int getCount)
        {
            return TmphQuickSort.GetRangeSort(array, comparer, skipCount, getCount);
        }

        /// <summary>
        ///     范围排序
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="value">待排序数组</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">结束位置</param>
        /// <param name="comparer">比较器</param>
        /// <param name="skipCount">跳过记录数</param>
        /// <param name="getCount">获取记录数</param>
        /// <returns>排序范围数组</returns>
        public static TmphSubArray<TValueType> rangeSort<TValueType>
            (this TValueType[] array, int startIndex, int count, Func<TValueType, TValueType, int> comparer, int skipCount,
                int getCount)
        {
            return TmphQuickSort.RangeSort(array, startIndex, count, comparer, skipCount, getCount);
        }

        /// <summary>
        ///     范围排序
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="value">待排序数组</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">结束位置</param>
        /// <param name="comparer">比较器</param>
        /// <param name="skipCount">跳过记录数</param>
        /// <param name="getCount">获取记录数</param>
        /// <returns>排序范围数组</returns>
        public static TmphSubArray<TValueType> getRangeSort<TValueType>
            (this TValueType[] array, int startIndex, int count, Func<TValueType, TValueType, int> comparer, int skipCount,
                int getCount)
        {
            return TmphQuickSort.GetRangeSort(array, startIndex, count, comparer, skipCount, getCount);
        }

        /// <summary>
        ///     分页排序
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="value">待排序数组</param>
        /// <param name="comparer">比较器</param>
        /// <param name="pageSize">分页尺寸</param>
        /// <param name="currentPage">页号</param>
        /// <returns>分页排序数据</returns>
        public static TmphSubArray<TValueType> pageSort<TValueType>
            (this TValueType[] array, Func<TValueType, TValueType, int> comparer, int pageSize, int currentPage)
        {
            var page = new TmphPage(array.length(), pageSize, currentPage);
            return TmphQuickSort.RangeSort(array, comparer, page.SkipCount, page.CurrentPageSize);
        }

        /// <summary>
        ///     分页排序
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="value">待排序数组</param>
        /// <param name="comparer">比较器</param>
        /// <param name="pageSize">分页尺寸</param>
        /// <param name="currentPage">页号</param>
        /// <returns>分页排序数据</returns>
        public static TmphSubArray<TValueType> getPageSort<TValueType>
            (this TValueType[] array, Func<TValueType, TValueType, int> comparer, int pageSize, int currentPage)
        {
            var page = new TmphPage(array.length(), pageSize, currentPage);
            return TmphQuickSort.GetRangeSort(array, comparer, page.SkipCount, page.CurrentPageSize);
        }

        /// <summary>
        ///     获取最大值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="comparer">比较器</param>
        /// <param name="value">最大值</param>
        /// <returns>是否存在最大值</returns>
        public static bool max<TValueType>(this TValueType[] array, Func<TValueType, TValueType, int> comparer,
            out TValueType value)
        {
            if (comparer == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            if (array.length() != 0)
            {
                value = array[0];
                foreach (var nextValue in array)
                {
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
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TKeyType">比较键类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="getKey">获取键值</param>
        /// <param name="comparer">比较器</param>
        /// <param name="value">最大值</param>
        /// <returns>是否存在最大值</returns>
        public static bool max<TValueType, TKeyType>
            (this TValueType[] array, Func<TValueType, TKeyType> getKey, Func<TKeyType, TKeyType, int> comparer,
                out TValueType value)
        {
            if (getKey == null || comparer == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            if (array.length() != 0)
            {
                value = array[0];
                if (array.Length != 1)
                {
                    var key = getKey(value);
                    foreach (var nextValue in array)
                    {
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
        ///     获取最大值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最大值,失败返回默认空值</returns>
        public static TValueType max<TValueType>(this TValueType[] array, TValueType nullValue)
            where TValueType : IComparable<TValueType>
        {
            TValueType value;
            return max(array, (left, right) => left.CompareTo(right), out value) ? value : nullValue;
        }

        /// <summary>
        ///     获取最大值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TKeyType">比较键类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="getKey">获取键值</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最大值,失败返回默认空值</returns>
        public static TValueType max<TValueType, TKeyType>(this TValueType[] array, Func<TValueType, TKeyType> getKey,
            TValueType nullValue)
            where TKeyType : IComparable<TKeyType>
        {
            TValueType value;
            return max(array, getKey, (left, right) => left.CompareTo(right), out value) ? value : nullValue;
        }

        /// <summary>
        ///     获取最大值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TKeyType">比较键类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="getKey">获取键值</param>
        /// <param name="comparer">比较器</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最大值,失败返回默认空值</returns>
        public static TValueType max<TValueType, TKeyType>
            (this TValueType[] array, Func<TValueType, TKeyType> getKey, Func<TKeyType, TKeyType, int> comparer,
                TValueType nullValue)
        {
            TValueType value;
            return max(array, getKey, comparer, out value) ? value : nullValue;
        }

        /// <summary>
        ///     获取最大键值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TKeyType">比较键类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="getKey">获取键值</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最大键值,失败返回默认空值</returns>
        public static TKeyType maxKey<TValueType, TKeyType>(this TValueType[] array, Func<TValueType, TKeyType> getKey,
            TKeyType nullValue)
            where TKeyType : IComparable<TKeyType>
        {
            TValueType value;
            return max(array, getKey, (left, right) => left.CompareTo(right), out value) ? getKey(value) : nullValue;
        }

        /// <summary>
        ///     获取最大键值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TKeyType">比较键类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="getKey">获取键值</param>
        /// <param name="comparer">比较器</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最大键值,失败返回默认空值</returns>
        public static TKeyType maxKey<TValueType, TKeyType>
            (this TValueType[] array, Func<TValueType, TKeyType> getKey, Func<TKeyType, TKeyType, int> comparer,
                TKeyType nullValue)
        {
            TValueType value;
            return max(array, getKey, comparer, out value) ? getKey(value) : nullValue;
        }

        /// <summary>
        ///     获取最小值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="comparer">比较器</param>
        /// <param name="value">最小值</param>
        /// <returns>是否存在最小值</returns>
        public static bool min<TValueType>(this TValueType[] array, Func<TValueType, TValueType, int> comparer,
            out TValueType value)
        {
            if (comparer == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            if (array.length() != 0)
            {
                value = array[0];
                foreach (var nextValue in array)
                {
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
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TKeyType">比较键类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="getKey">获取键值</param>
        /// <param name="comparer">比较器</param>
        /// <param name="value">最小值</param>
        /// <returns>是否存在最小值</returns>
        public static bool min<TValueType, TKeyType>
            (this TValueType[] array, Func<TValueType, TKeyType> getKey, Func<TKeyType, TKeyType, int> comparer,
                out TValueType value)
        {
            if (getKey == null || comparer == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            if (array.length() != 0)
            {
                value = array[0];
                if (array.Length != 1)
                {
                    var key = getKey(value);
                    foreach (var nextValue in array)
                    {
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
        ///     获取最小值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最小值,失败返回默认空值</returns>
        public static TValueType min<TValueType>(this TValueType[] array, TValueType nullValue)
            where TValueType : IComparable<TValueType>
        {
            TValueType value;
            return min(array, (left, right) => left.CompareTo(right), out value) ? value : nullValue;
        }

        /// <summary>
        ///     获取最小值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TKeyType">比较键类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="getKey">获取键值</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最小值,失败返回默认空值</returns>
        public static TValueType min<TValueType, TKeyType>(this TValueType[] array, Func<TValueType, TKeyType> getKey,
            TValueType nullValue)
            where TKeyType : IComparable<TKeyType>
        {
            TValueType value;
            return min(array, getKey, (left, right) => left.CompareTo(right), out value) ? value : nullValue;
        }

        /// <summary>
        ///     获取最小值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TKeyType">比较键类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="getKey">获取键值</param>
        /// <param name="comparer">比较器</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最小值,失败返回默认空值</returns>
        public static TValueType min<TValueType, TKeyType>
            (this TValueType[] array, Func<TValueType, TKeyType> getKey, Func<TKeyType, TKeyType, int> comparer,
                TValueType nullValue)
        {
            TValueType value;
            return min(array, getKey, comparer, out value) ? value : nullValue;
        }

        /// <summary>
        ///     获取最小键值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TKeyType">比较键类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="getKey">获取键值</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最小键值,失败返回默认空值</returns>
        public static TKeyType minKey<TValueType, TKeyType>(this TValueType[] array, Func<TValueType, TKeyType> getKey,
            TKeyType nullValue)
            where TKeyType : IComparable<TKeyType>
        {
            TValueType value;
            return min(array, getKey, (left, right) => left.CompareTo(right), out value) ? getKey(value) : nullValue;
        }

        /// <summary>
        ///     获取最小键值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TKeyType">比较键类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="getKey">获取键值</param>
        /// <param name="comparer">比较器</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最小键值,失败返回默认空值</returns>
        public static TKeyType minKey<TValueType, TKeyType>
            (this TValueType[] array, Func<TValueType, TKeyType> getKey, Func<TKeyType, TKeyType, int> comparer,
                TKeyType nullValue)
        {
            TValueType value;
            return min(array, getKey, comparer, out value) ? getKey(value) : nullValue;
        }

        /// <summary>
        ///     连接字符串
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数据集合</param>
        /// <param name="toString">字符串转换器</param>
        /// <returns>字符串</returns>
        public static string joinString<TValueType>(this TValueType[] array, Func<TValueType, string> toString)
        {
            return string.Concat(array.getArray(toString));
        }

        /// <summary>
        ///     连接字符串
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数据集合</param>
        /// <param name="toString">字符串转换器</param>
        /// <param name="join">连接串</param>
        /// <returns>字符串</returns>
        public static string joinString<TValueType>(this TValueType[] array, string join, Func<TValueType, string> toString)
        {
            return string.Join(join, array.getArray(toString));
        }

        /// <summary>
        ///     连接字符串
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数据集合</param>
        /// <param name="toString">字符串转换器</param>
        /// <param name="join">连接字符</param>
        /// <returns>字符串</returns>
        public static string joinString<TValueType>(this TValueType[] array, char join, Func<TValueType, string> toString)
        {
            return array.getArray(toString).JoinString(join);
        }

        /// <summary>
        ///     数组数据
        /// </summary>
        public struct TmphValue<TValueType> where TValueType : class
        {
            /// <summary>
            ///     数据对象
            /// </summary>
            public TValueType Value;

            /// <summary>
            ///     释放数据对象
            /// </summary>
            /// <returns>缓冲区</returns>
            public TValueType Free()
            {
                var value = Value;
                Value = null;
                return value;
            }
        }

        #region 数据记录范围

        /// <summary>
        ///     数据记录范围
        /// </summary>
        public struct TmphRange
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
            ///     数据总量
            /// </summary>
            private int count;

            /// <summary>
            ///     数据记录范围
            /// </summary>
            /// <param name="count">数据总量</param>
            /// <param name="skipCount">跳过记录数</param>
            /// <param name="getCount">获取记录数</param>
            public TmphRange(int count, int skipCount, int getCount)
            {
                this.count = count < 0 ? 0 : count;
                if (skipCount < count && getCount != 0)
                {
                    if (getCount > 0)
                    {
                        if (skipCount >= 0)
                        {
                            startIndex = skipCount;
                            if ((endIndex = skipCount + getCount) > count) endIndex = count;
                        }
                        else
                        {
                            startIndex = 0;
                            if ((endIndex = skipCount + getCount) > count) endIndex = count;
                            else if (endIndex < 0) endIndex = 0;
                        }
                    }
                    else
                    {
                        startIndex = skipCount >= 0 ? skipCount : 0;
                        endIndex = count;
                    }
                }
                else startIndex = endIndex = 0;
            }

            /// <summary>
            ///     跳过记录数
            /// </summary>
            public int SkipCount
            {
                get { return startIndex; }
            }

            /// <summary>
            ///     结束位置
            /// </summary>
            public int EndIndex
            {
                get { return endIndex; }
            }

            /// <summary>
            ///     获取记录数
            /// </summary>
            public int GetCount
            {
                get { return endIndex - startIndex; }
            }
        }

        #endregion 数据记录范围

        #region 分页记录范围

        /// <summary>
        ///     分页记录范围
        /// </summary>
        public struct TmphPage
        {
            /// <summary>
            ///     数据总量
            /// </summary>
            private readonly int count;

            /// <summary>
            ///     当前页号
            /// </summary>
            private readonly int currentPage;

            /// <summary>
            ///     当前页记录数
            /// </summary>
            private readonly int currentPageSize;

            /// <summary>
            ///     分页总数
            /// </summary>
            private readonly int pageCount;

            /// <summary>
            ///     分页尺寸
            /// </summary>
            private readonly int pageSize;

            /// <summary>
            ///     跳过记录数
            /// </summary>
            private readonly int skipCount;

            /// <summary>
            ///     分页记录范围
            /// </summary>
            /// <param name="count">数据总量</param>
            /// <param name="pageSize">分页尺寸</param>
            /// <param name="currentPage">页号</param>
            public TmphPage(int count, int pageSize, int currentPage)
            {
                this.pageSize = pageSize > 0 ? pageSize : Config.TmphPub.Default.PageSize;
                this.count = count < 0 ? 0 : count;
                pageCount = (this.count + this.pageSize - 1) / this.pageSize;
                if (pageCount < 0) pageCount = 0;
                this.currentPage = currentPage > 0
                    ? (currentPage <= pageCount ? currentPage : (pageCount == 0 ? 1 : pageCount))
                    : 1;
                skipCount = (this.currentPage - 1) * this.pageSize;
                currentPageSize = Math.Min(skipCount + this.pageSize, this.count) - skipCount;
            }

            /// <summary>
            ///     数据总量
            /// </summary>
            public int Count
            {
                get { return count; }
            }

            /// <summary>
            ///     分页总数
            /// </summary>
            public int PageCount
            {
                get { return pageCount; }
            }

            /// <summary>
            ///     当前页号
            /// </summary>
            public int CurrentPage
            {
                get { return currentPage; }
            }

            /// <summary>
            ///     分页尺寸
            /// </summary>
            public int PageSize
            {
                get { return pageSize; }
            }

            /// <summary>
            ///     跳过记录数
            /// </summary>
            public int SkipCount
            {
                get { return skipCount; }
            }

            /// <summary>
            ///     逆序跳过记录数
            /// </summary>
            public int DescSkipCount
            {
                get { return count - skipCount - currentPageSize; }
            }

            /// <summary>
            ///     当前页记录数
            /// </summary>
            public int CurrentPageSize
            {
                get { return currentPageSize; }
            }
        }

        #endregion 分页记录范围

        #region 二分查找匹配值位置

        /// <summary>
        ///     二分查找匹配值位置
        /// </summary>
        /// <typeparam name="TValueType">值类型</typeparam>
        /// <param name="value">匹配值</param>
        /// <param name="values">匹配数组</param>
        /// <returns>匹配值位置,失败返回-1</returns>
        public static int binaryIndexOf<TValueType>(this TValueType[] values, TValueType value)
            where TValueType : IComparable<TValueType>
        {
            return binaryIndexOf(values, value, (left, right) => left.CompareTo(right));
        }

        /// <summary>
        ///     二分查找匹配值位置
        /// </summary>
        /// <typeparam name="TValueType">值类型</typeparam>
        /// <param name="value">匹配值</param>
        /// <param name="values">匹配数组</param>
        /// <param name="comparer">数组值比较器</param>
        /// <returns>匹配值位置,失败返回-1</returns>
        public static int binaryIndexOf<TValueType>(this TValueType[] values, TValueType value
            , Func<TValueType, TValueType, int> comparer)
        {
            if (values != null && values.Length != 0)
            {
                if (comparer == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);

                int average, start = 0, length = values.Length, cmp;
                if (comparer(values[0], values[length - 1]) <= 0)
                {
                    while (start < length)
                    {
                        average = start + ((length - start) >> 1);
                        cmp = comparer(value, values[average]);
                        if (cmp < 0) length = average;
                        else if (cmp == 0) return average;
                        else start = average + 1;
                    }
                }
                else
                {
                    while (start < length)
                    {
                        average = start + ((length - start) >> 1);
                        cmp = comparer(value, values[average]);
                        if (cmp > 0) length = average;
                        else if (cmp == 0) return average;
                        else start = average + 1;
                    }
                }
            }
            return -1;
        }

        /// <summary>
        ///     二分查找匹配值位置
        /// </summary>
        /// <typeparam name="TKeyType">查找值类型</typeparam>
        /// <typeparam name="TValueType">数组值类型</typeparam>
        /// <param name="value">匹配值</param>
        /// <param name="values">匹配数组</param>
        /// <param name="comparer">查找值比较器</param>
        /// <param name="isAscending">是否升序</param>
        /// <returns>匹配值位置,失败返回-1</returns>
        public static int binaryIndexOf<TKeyType, TValueType>(this TValueType[] values, TKeyType value
            , Func<TKeyType, TValueType, int> comparer, bool isAscending)
        {
            return binaryIndexOf(values, value, comparer, (left, right) => isAscending ? -1 : 1);
        }

        /// <summary>
        ///     二分查找匹配值位置
        /// </summary>
        /// <typeparam name="TKeyType">查找值类型</typeparam>
        /// <typeparam name="TValueType">数组值类型</typeparam>
        /// <param name="value">匹配值</param>
        /// <param name="values">匹配数组</param>
        /// <param name="comparer">查找值比较器</param>
        /// <param name="orderComparer">数组值比较器</param>
        /// <returns>匹配值位置,失败返回-1</returns>
        public static int binaryIndexOf<TKeyType, TValueType>(this TValueType[] values, TKeyType value
            , Func<TKeyType, TValueType, int> comparer, Func<TValueType, TValueType, int> orderComparer)
        {
            if (values != null && values.Length != 0)
            {
                if (comparer == null || orderComparer == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);

                int average, start = 0, length = values.Length, cmp;
                if (orderComparer(values[0], values[length - 1]) <= 0)
                {
                    while (start < length)
                    {
                        average = start + ((length - start) >> 1);
                        cmp = comparer(value, values[average]);
                        if (cmp < 0) length = average;
                        else if (cmp == 0) return average;
                        else start = average + 1;
                    }
                }
                else
                {
                    while (start < length)
                    {
                        average = start + ((length - start) >> 1);
                        cmp = comparer(value, values[average]);
                        if (cmp > 0) length = average;
                        else if (cmp == 0) return average;
                        else start = average + 1;
                    }
                }
            }
            return -1;
        }

        #endregion 二分查找匹配值位置

        #region 二分查找第一个匹配值位置

        /// <summary>
        ///     二分查找第一个匹配值位置
        /// </summary>
        /// <typeparam name="TValueType">值类型</typeparam>
        /// <param name="value">匹配值</param>
        /// <param name="values">匹配数组</param>
        /// <returns>匹配值位置,失败返回-1</returns>
        public static int binaryFirstIndexOf<TValueType>(this TValueType[] values, TValueType value)
            where TValueType : IComparable<TValueType>
        {
            return binaryFirstIndexOf(values, value, (left, right) => left.CompareTo(right));
        }

        /// <summary>
        ///     二分查找第一个匹配值位置
        /// </summary>
        /// <typeparam name="TValueType">值类型</typeparam>
        /// <param name="value">匹配值</param>
        /// <param name="values">匹配数组</param>
        /// <param name="comparer">数组值比较器</param>
        /// <returns>匹配值位置,失败返回-1</returns>
        public static int binaryFirstIndexOf<TValueType>(this TValueType[] values, TValueType value,
            Func<TValueType, TValueType, int> comparer)
        {
            var index = -1;
            if (values != null && values.Length != 0)
            {
                if (comparer == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);

                int average, start = 0, length = values.Length, cmp;
                if (comparer(values[0], values[length - 1]) <= 0)
                {
                    while (start < length)
                    {
                        average = start + ((length - start) >> 1);
                        cmp = comparer(value, values[average]);
                        if (cmp <= 0)
                        {
                            length = average;
                            if (cmp == 0) index = average;
                        }
                        else start = average + 1;
                    }
                }
                else
                {
                    while (start < length)
                    {
                        average = start + ((length - start) >> 1);
                        cmp = comparer(value, values[average]);
                        if (cmp >= 0)
                        {
                            length = average;
                            if (cmp == 0) index = average;
                        }
                        else start = average + 1;
                    }
                }
            }
            return index;
        }

        /// <summary>
        ///     二分查找第一个匹配值位置
        /// </summary>
        /// <typeparam name="TKeyType">查找值类型</typeparam>
        /// <typeparam name="TValueType">数组值类型</typeparam>
        /// <param name="value">匹配值</param>
        /// <param name="values">匹配数组</param>
        /// <param name="comparer">查找值比较器</param>
        /// <param name="isAscending">是否升序</param>
        /// <returns>匹配值位置,失败返回-1</returns>
        public static int binaryFirstIndexOf<TKeyType, TValueType>(this TValueType[] values, TKeyType value
            , Func<TKeyType, TValueType, int> comparer
            , bool isAscending)
        {
            return binaryFirstIndexOf(values, value, comparer, (left, right) => isAscending ? -1 : 1);
        }

        /// <summary>
        ///     二分查找第一个匹配值位置
        /// </summary>
        /// <typeparam name="TKeyType">查找值类型</typeparam>
        /// <typeparam name="TValueType">数组值类型</typeparam>
        /// <param name="value">匹配值</param>
        /// <param name="values">匹配数组</param>
        /// <param name="comparer">查找值比较器</param>
        /// <param name="orderComparer">数组值比较器</param>
        /// <returns>匹配值位置,失败返回-1</returns>
        public static int binaryFirstIndexOf<TKeyType, TValueType>(this TValueType[] values, TKeyType value
            , Func<TKeyType, TValueType, int> comparer
            , Func<TValueType, TValueType, int> orderComparer)
        {
            var index = -1;
            if (values != null && values.Length != 0)
            {
                if (comparer == null || orderComparer == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);

                int average, start = 0, length = values.Length, cmp;
                if (orderComparer(values[0], values[length - 1]) <= 0)
                {
                    while (start < length)
                    {
                        average = start + ((length - start) >> 1);
                        cmp = comparer(value, values[average]);
                        if (cmp <= 0)
                        {
                            length = average;
                            if (cmp == 0) index = average;
                        }
                        else start = average + 1;
                    }
                }
                else
                {
                    while (start < length)
                    {
                        average = start + ((length - start) >> 1);
                        cmp = comparer(value, values[average]);
                        if (cmp >= 0)
                        {
                            length = average;
                            if (cmp == 0) index = average;
                        }
                        else start = average + 1;
                    }
                }
            }
            return index;
        }

        #endregion 二分查找第一个匹配值位置

        #region 二分查找最后一个匹配值位置

        /// <summary>
        ///     二分查找最后一个匹配值位置
        /// </summary>
        /// <typeparam name="TValueType">值类型</typeparam>
        /// <param name="value">匹配值</param>
        /// <param name="values">匹配数组</param>
        /// <returns>匹配值位置,失败返回-1</returns>
        public static int binaryLastIndexOf<TValueType>(this TValueType[] values, TValueType value)
            where TValueType : IComparable<TValueType>
        {
            return binaryLastIndexOf(values, value, (left, right) => left.CompareTo(right));
        }

        /// <summary>
        ///     二分查找最后一个匹配值位置
        /// </summary>
        /// <typeparam name="TValueType">值类型</typeparam>
        /// <param name="value">匹配值</param>
        /// <param name="values">匹配数组</param>
        /// <param name="comparer">数组值比较器</param>
        /// <returns>匹配值位置,失败返回-1</returns>
        public static int binaryLastIndexOf<TValueType>(this TValueType[] values, TValueType value,
            Func<TValueType, TValueType, int> comparer)
        {
            var index = -1;
            if (values != null && values.Length != 0)
            {
                if (comparer == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);

                int average, start = 0, length = values.Length, cmp;
                if (comparer(values[0], values[length - 1]) <= 0)
                {
                    while (start < length)
                    {
                        average = start + ((length - start) >> 1);
                        cmp = comparer(value, values[average]);
                        if (cmp >= 0)
                        {
                            start = average + 1;
                            if (cmp == 0) index = average;
                        }
                        else length = average;
                    }
                }
                else
                {
                    while (start < length)
                    {
                        average = start + ((length - start) >> 1);
                        cmp = comparer(value, values[average]);
                        if (cmp <= 0)
                        {
                            start = average + 1;
                            if (cmp == 0) index = average;
                        }
                        else length = average;
                    }
                }
            }
            return index;
        }

        /// <summary>
        ///     二分查找最后一个匹配值位置
        /// </summary>
        /// <typeparam name="TKeyType">查找值类型</typeparam>
        /// <typeparam name="TValueType">数组值类型</typeparam>
        /// <param name="value">匹配值</param>
        /// <param name="values">匹配数组</param>
        /// <param name="comparer">查找值比较器</param>
        /// <param name="isAscending">是否升序</param>
        /// <returns>匹配值位置,失败返回-1</returns>
        public static int binaryLastIndexOf<TKeyType, TValueType>(this TValueType[] values, TKeyType value
            , Func<TKeyType, TValueType, int> comparer
            , bool isAscending)
        {
            return binaryLastIndexOf(values, value, comparer, (left, right) => isAscending ? -1 : 1);
        }

        /// <summary>
        ///     二分查找最后一个匹配值位置
        /// </summary>
        /// <typeparam name="TKeyType">查找值类型</typeparam>
        /// <typeparam name="TValueType">数组值类型</typeparam>
        /// <param name="value">匹配值</param>
        /// <param name="values">匹配数组</param>
        /// <param name="comparer">查找值比较器</param>
        /// <param name="orderComparer">数组值比较器</param>
        /// <returns>匹配值位置,失败返回-1</returns>
        public static int binaryLastIndexOf<TKeyType, TValueType>(this TValueType[] values, TKeyType value
            , Func<TKeyType, TValueType, int> comparer
            , Func<TValueType, TValueType, int> orderComparer)
        {
            var index = -1;
            if (values != null && values.Length != 0)
            {
                if (comparer == null || orderComparer == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);

                int average, start = 0, length = values.Length, cmp;
                if (orderComparer(values[0], values[length - 1]) <= 0)
                {
                    while (start < length)
                    {
                        average = start + ((length - start) >> 1);
                        cmp = comparer(value, values[average]);
                        if (cmp >= 0)
                        {
                            start = average + 1;
                            if (cmp == 0) index = average;
                        }
                        else length = average;
                    }
                }
                else
                {
                    while (start < length)
                    {
                        average = start + ((length - start) >> 1);
                        cmp = comparer(value, values[average]);
                        if (cmp <= 0)
                        {
                            start = average + 1;
                            if (cmp == 0) index = average;
                        }
                        else length = average;
                    }
                }
            }
            return index;
        }

        #endregion 二分查找最后一个匹配值位置

        #region 二分查找匹配值之后的位置(用于查找插入值的位置)

        /// <summary>
        ///     二分查找匹配值之后的位置(用于查找插入值的位置)
        /// </summary>
        /// <typeparam name="TValueType">值类型</typeparam>
        /// <param name="value">匹配值</param>
        /// <param name="values">匹配数组</param>
        /// <returns>匹配值之后的位置,失败返回-1</returns>
        public static int binaryIndexOfThan<TValueType>(this TValueType[] values, TValueType value)
            where TValueType : IComparable<TValueType>
        {
            return binaryIndexOfThan(values, value, (left, right) => left.CompareTo(right));
        }

        /// <summary>
        ///     二分查找匹配值之后的位置(用于查找插入值的位置)
        /// </summary>
        /// <typeparam name="TValueType">值类型</typeparam>
        /// <param name="value">匹配值</param>
        /// <param name="values">匹配数组</param>
        /// <param name="comparer">数组值比较器</param>
        /// <returns>匹配值之后的位置,失败返回-1</returns>
        public static int binaryIndexOfThan<TValueType>(this TValueType[] values, TValueType value,
            Func<TValueType, TValueType, int> comparer)
        {
            var index = 0;
            if (values == null) index = -1;
            else if (values.Length != 0)
            {
                if (comparer == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);

                int average, length = values.Length;
                if (comparer(values[0], values[length - 1]) <= 0)
                {
                    while (index < length)
                    {
                        average = index + ((length - index) >> 1);
                        if (comparer(value, values[average]) >= 0) index = average + 1;
                        else length = average;
                    }
                }
                else
                {
                    while (index < length)
                    {
                        average = index + ((length - index) >> 1);
                        if (comparer(value, values[average]) <= 0) index = average + 1;
                        else length = average;
                    }
                }
            }
            return index;
        }

        /// <summary>
        ///     二分查找匹配值之后的位置(用于查找插入值的位置)
        /// </summary>
        /// <typeparam name="TKeyType">查找值类型</typeparam>
        /// <typeparam name="TValueType">数组值类型</typeparam>
        /// <param name="value">匹配值</param>
        /// <param name="values">匹配数组</param>
        /// <param name="comparer">查找值比较器</param>
        /// <param name="isAscending">是否升序</param>
        /// <returns>匹配值之后的位置,失败返回-1</returns>
        public static int binaryIndexOfThan<TKeyType, TValueType>(this TValueType[] values, TKeyType value
            , Func<TKeyType, TValueType, int> comparer
            , bool isAscending)
        {
            return binaryIndexOfThan(values, value, comparer, (left, right) => isAscending ? -1 : 1);
        }

        /// <summary>
        ///     二分查找匹配值之后的位置(用于查找插入值的位置)
        /// </summary>
        /// <typeparam name="TKeyType">查找值类型</typeparam>
        /// <typeparam name="TValueType">数组值类型</typeparam>
        /// <param name="value">匹配值</param>
        /// <param name="values">匹配数组</param>
        /// <param name="comparer">查找值比较器</param>
        /// <param name="orderComparer">数组值比较器</param>
        /// <returns>匹配值之后的位置,失败返回-1</returns>
        public static int binaryIndexOfThan<TKeyType, TValueType>(this TValueType[] values, TKeyType value
            , Func<TKeyType, TValueType, int> comparer
            , Func<TValueType, TValueType, int> orderComparer)
        {
            var index = 0;
            if (values == null) index = -1;
            else if (values.Length != 0)
            {
                if (comparer == null || orderComparer == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);

                int average, length = values.Length;
                if (orderComparer(values[0], values[length - 1]) <= 0)
                {
                    while (index < length)
                    {
                        average = index + ((length - index) >> 1);
                        if (comparer(value, values[average]) >= 0) index = average + 1;
                        else length = average;
                    }
                }
                else
                {
                    while (index < length)
                    {
                        average = index + ((length - index) >> 1);
                        if (comparer(value, values[average]) <= 0) index = average + 1;
                        else length = average;
                    }
                }
            }
            return index;
        }

        #endregion 二分查找匹配值之后的位置(用于查找插入值的位置)

        #region 二分查找匹配值之前的位置(用于查找插入值的位置)

        /// <summary>
        ///     二分查找匹配值之前的位置(用于查找插入值的位置)
        /// </summary>
        /// <typeparam name="TValueType">值类型</typeparam>
        /// <param name="value">匹配值</param>
        /// <param name="values">匹配数组</param>
        /// <returns>匹配值之前的位置,失败返回-1</returns>
        public static int binaryIndexOfLess<TValueType>(this TValueType[] values, TValueType value)
            where TValueType : IComparable<TValueType>
        {
            return binaryIndexOfLess(values, value, (left, right) => left.CompareTo(right));
        }

        /// <summary>
        ///     二分查找匹配值之前的位置(用于查找插入值的位置)
        /// </summary>
        /// <typeparam name="TValueType">值类型</typeparam>
        /// <param name="value">匹配值</param>
        /// <param name="values">匹配数组</param>
        /// <param name="comparer">数组值比较器</param>
        /// <returns>匹配值之前的位置,失败返回-1</returns>
        public static int binaryIndexOfLess<TValueType>(this TValueType[] values, TValueType value,
            Func<TValueType, TValueType, int> comparer)
        {
            var index = 0;
            if (values == null) index = -1;
            else if (values.Length != 0)
            {
                if (comparer == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);

                int average, length = values.Length;
                if (comparer(values[0], values[length - 1]) <= 0)
                {
                    while (index < length)
                    {
                        average = index + ((length - index) >> 1);
                        if (comparer(value, values[average]) > 0) index = average + 1;
                        else length = average;
                    }
                }
                else
                {
                    while (index < length)
                    {
                        average = index + ((length - index) >> 1);
                        if (comparer(value, values[average]) < 0) index = average + 1;
                        else length = average;
                    }
                }
            }
            return index;
        }

        /// <summary>
        ///     二分查找匹配值之前的位置(用于查找插入值的位置)
        /// </summary>
        /// <typeparam name="TKeyType">查找值类型</typeparam>
        /// <typeparam name="TValueType">数组值类型</typeparam>
        /// <param name="value">匹配值</param>
        /// <param name="values">匹配数组</param>
        /// <param name="comparer">查找值比较器</param>
        /// <param name="isAscending">是否升序</param>
        /// <returns>匹配值之前的位置,失败返回-1</returns>
        public static int binaryIndexOfLess<TKeyType, TValueType>(this TValueType[] values, TKeyType value
            , Func<TKeyType, TValueType, int> comparer
            , bool isAscending)
        {
            return binaryIndexOfLess(values, value, comparer, (left, right) => isAscending ? -1 : 1);
        }

        /// <summary>
        ///     二分查找匹配值之前的位置(用于查找插入值的位置)
        /// </summary>
        /// <typeparam name="TKeyType">查找值类型</typeparam>
        /// <typeparam name="TValueType">数组值类型</typeparam>
        /// <param name="value">匹配值</param>
        /// <param name="values">匹配数组</param>
        /// <param name="comparer">查找值比较器</param>
        /// <param name="orderComparer">数组值比较器</param>
        /// <returns>匹配值之前的位置,失败返回-1</returns>
        public static int binaryIndexOfLess<TKeyType, TValueType>(this TValueType[] values, TKeyType value
            , Func<TKeyType, TValueType, int> comparer
            , Func<TValueType, TValueType, int> orderComparer)
        {
            var index = 0;
            if (values == null) index = -1;
            else if (values.Length != 0)
            {
                if (comparer == null || orderComparer == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);

                int average, length = values.Length;
                if (orderComparer(values[0], values[length - 1]) <= 0)
                {
                    while (index < length)
                    {
                        average = index + ((length - index) >> 1);
                        if (comparer(value, values[average]) > 0) index = average + 1;
                        else length = average;
                    }
                }
                else
                {
                    while (index < length)
                    {
                        average = index + ((length - index) >> 1);
                        if (comparer(value, values[average]) < 0) index = average + 1;
                        else length = average;
                    }
                }
            }
            return index;
        }

        #endregion 二分查找匹配值之前的位置(用于查找插入值的位置)
    }
}