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