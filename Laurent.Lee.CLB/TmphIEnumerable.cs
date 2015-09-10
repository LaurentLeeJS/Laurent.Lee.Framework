using System;
using System.Collections.Generic;

namespace Laurent.Lee.CLB
{
    /// <summary>
    ///     可枚举相关扩展
    /// </summary>
    public static class TmphIEnumerable
    {
        /// <summary>
        ///     转数组
        /// </summary>
        /// <typeparam name="TValueType">数据集合类型</typeparam>
        /// <param name="values">数据集合</param>
        /// <returns>数组</returns>
        public static TValueType[] getArray<TValueType>(this IEnumerable<TValueType> values)
        {
            return values.getSubArray().ToArray();
        }

        /// <summary>
        ///     转单向动态数组
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="values">数据枚举集合</param>
        /// <returns>单向动态数组</returns>
        public static TmphSubArray<TValueType> getSubArray<TValueType>(this IEnumerable<TValueType> values)
        {
            if (values != null)
            {
                var count = 0;
                var newValues = new TValueType[sizeof(int)];
                foreach (var value in values)
                {
                    if (count == newValues.Length)
                    {
                        var nextValues = new TValueType[count << 1];
                        newValues.CopyTo(nextValues, 0);
                        newValues = nextValues;
                    }
                    newValues[count++] = value;
                }
                return TmphSubArray<TValueType>.Unsafe(newValues, 0, count);
            }
            return default(TmphSubArray<TValueType>);
        }

        /// <summary>
        ///     转单向动态数组
        /// </summary>
        /// <typeparam name="collectionType">集合类型</typeparam>
        /// <typeparam name="TValueType">枚举值类型</typeparam>
        /// <typeparam name="arrayType">返回数组类型</typeparam>
        /// <param name="values">数据枚举集合</param>
        /// <param name="getValue">获取数组值的委托</param>
        /// <returns>单向动态数组</returns>
        public static TmphSubArray<arrayType> getSubArray<TValueType, arrayType>
            (this IEnumerable<TValueType> values, Func<TValueType, arrayType> getValue)
        {
            if (values != null)
            {
                if (getValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                var count = 0;
                var newValues = new arrayType[sizeof(int)];
                foreach (var value in values)
                {
                    if (count == newValues.Length)
                    {
                        var nextValues = new arrayType[count << 1];
                        newValues.CopyTo(nextValues, 0);
                        newValues = nextValues;
                    }
                    newValues[count++] = getValue(value);
                }
                return TmphSubArray<arrayType>.Unsafe(newValues, 0, count);
            }
            return default(TmphSubArray<arrayType>);
        }

        /// <summary>
        ///     转单向动态数组
        /// </summary>
        /// <typeparam name="collectionType">集合类型</typeparam>
        /// <typeparam name="TValueType">枚举值类型</typeparam>
        /// <typeparam name="arrayType">返回数组类型</typeparam>
        /// <param name="values">数据枚举集合</param>
        /// <param name="getValue">获取数组值的委托</param>
        /// <returns>单向动态数组</returns>
        public static arrayType[] getArray<TValueType, arrayType>
            (this IEnumerable<TValueType> values, Func<TValueType, arrayType> getValue)
        {
            return values.getSubArray(getValue).ToArray();
        }

        /// <summary>
        ///     转换成字典
        /// </summary>
        /// <typeparam name="TValueType">枚举值类型</typeparam>
        /// <typeparam name="TKeyType">哈希键值类型</typeparam>
        /// <param name="values">值集合</param>
        /// <param name="getKey">键值获取器</param>
        /// <returns>字典</returns>
        public static Dictionary<TKeyType, TValueType> getDictionary<TValueType, TKeyType>
            (this IEnumerable<TValueType> values, Func<TValueType, TKeyType> getKey)
            where TKeyType : IEquatable<TKeyType>
        {
            if (values != null)
            {
                if (getKey == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                var dictionary = TmphDictionary<TKeyType>.Create<TValueType>();
                foreach (var value in values) dictionary[getKey(value)] = value;
                return dictionary;
            }
            return null;
        }

        /// <summary>
        ///     转换成字典
        /// </summary>
        /// <typeparam name="TValueType">枚举值类型</typeparam>
        /// <typeparam name="TKeyType">哈希键值类型</typeparam>
        /// <typeparam name="dictionaryValueType">哈希值类型</typeparam>
        /// <param name="values">值集合</param>
        /// <param name="getKey">键值获取器</param>
        /// <param name="getValue">哈希值获取器</param>
        /// <returns>字典</returns>
        public static Dictionary<TKeyType, dictionaryValueType> getDictionary<TValueType, TKeyType, dictionaryValueType>
            (this IEnumerable<TValueType> values, Func<TValueType, TKeyType> getKey,
                Func<TValueType, dictionaryValueType> getValue)
            where TKeyType : IEquatable<TKeyType>
        {
            if (values != null)
            {
                if (getKey == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                var dictionary = TmphDictionary<TKeyType>.Create<dictionaryValueType>();
                foreach (var value in values) dictionary[getKey(value)] = getValue(value);
                return dictionary;
            }
            return null;
        }

        /// <summary>
        ///     查找符合条件的记录集合
        /// </summary>
        /// <typeparam name="TValueType">值类型</typeparam>
        /// <param name="values">值集合</param>
        /// <param name="isValue">判断记录是否符合条件的委托</param>
        /// <returns>符合条件的记录集合</returns>
        public static TmphSubArray<TValueType> getFind<TValueType>
            (this IEnumerable<TValueType> values, Func<TValueType, bool> isValue)
        {
            if (values != null)
            {
                if (isValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                var value = new TmphSubArray<TValueType>();
                foreach (var nextValue in values)
                {
                    if (isValue(nextValue)) value.Add(nextValue);
                }
                return value;
            }
            return default(TmphSubArray<TValueType>);
        }

        /// <summary>
        ///     查找符合条件的记录集合
        /// </summary>
        /// <typeparam name="TValueType">值类型</typeparam>
        /// <param name="values">值集合</param>
        /// <param name="isValue">判断记录是否符合条件的委托</param>
        /// <returns>符合条件的记录集合</returns>
        public static TValueType[] getFindArray<TValueType>
            (this IEnumerable<TValueType> values, Func<TValueType, bool> isValue)
        {
            return values.getFind(isValue).ToArray();
        }

        /// <summary>
        ///     根据集合内容返回哈希表
        /// </summary>
        /// <typeparam name="TValueType">枚举值类型</typeparam>
        /// <param name="values">值集合</param>
        /// <param name="getValue">获取数组值的委托</param>
        /// <returns>哈希表</returns>
        public static HashSet<TValueType> getHash<TValueType>(this IEnumerable<TValueType> values)
        {
            if (values != null)
            {
                var newValues = TmphHashSet<TValueType>.Create();
                foreach (var value in values) newValues.Add(value);
                return newValues;
            }
            return null;
        }

        /// <summary>
        ///     分组计数
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TKeyType">分组键值类型</typeparam>
        /// <param name="values">数据集合</param>
        /// <param name="getKey">键值获取器</param>
        /// <param name="capacity">集合容器大小</param>
        /// <returns>分组计数</returns>
        public static Dictionary<TKeyType, int> groupCount<TValueType, TKeyType>
            (this IEnumerable<TValueType> values, Func<TValueType, TKeyType> getKey, int capacity)
            where TKeyType : IEquatable<TKeyType>
        {
            if (values != null)
            {
                if (getKey == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                int count;
                var dictionary = capacity > 0
                    ? TmphDictionary<TKeyType>.Create<int>(capacity)
                    : TmphDictionary<TKeyType>.Create<int>(capacity);
                foreach (var value in values)
                {
                    var key = getKey(value);
                    if (dictionary.TryGetValue(key, out count)) dictionary[key] = count + 1;
                    else dictionary.Add(key, 1);
                }
                return dictionary;
            }
            return null;
        }
    }
}