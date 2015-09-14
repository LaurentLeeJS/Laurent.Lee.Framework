using System.Collections.Generic;

namespace Laurent.Lee.CLB
{
    /// <summary>
    /// 字典扩展操作
    /// </summary>
    public static class TmphDictionaryExpand
    {
        /// <summary>
        /// 根据键值获取数据
        /// </summary>
        /// <typeparam name="TKeyType">键类型</typeparam>
        /// <typeparam name="TValueType">值类型</typeparam>
        /// <param name="values">字典</param>
        /// <param name="key">键值</param>
        /// <param name="nullValue">失败值</param>
        /// <returns>数据</returns>
        public static TValueType get<TKeyType, TValueType>
            (this Dictionary<TKeyType, TValueType> values, TKeyType key, TValueType nullValue = default(TValueType))
        {
            TValueType value;
            return values.TryGetValue(key, out value) ? value : nullValue;
        }

        /// <summary>
        /// 获取键值集合
        /// </summary>
        /// <typeparam name="TKeyType">键类型</typeparam>
        /// <typeparam name="TValueType">值类型</typeparam>
        /// <param name="values">字典</param>
        /// <returns>键值集合</returns>
        public static ICollection<TKeyType> keys<TKeyType, TValueType>(this Dictionary<TKeyType, TValueType> values)
        {
            return values != null ? values.Keys : null;
        }

        /// <summary>
        /// 获取数据集合
        /// </summary>
        /// <typeparam name="TKeyType">键类型</typeparam>
        /// <typeparam name="TValueType">值类型</typeparam>
        /// <param name="values">字典</param>
        /// <returns>数据集合</returns>
        public static ICollection<TValueType> values<TKeyType, TValueType>(this Dictionary<TKeyType, TValueType> values)
        {
            return values != null ? values.Values : null;
        }
    }
}