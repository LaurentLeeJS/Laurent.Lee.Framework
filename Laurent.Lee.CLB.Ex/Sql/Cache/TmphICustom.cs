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

namespace Laurent.Lee.CLB.Sql.Cache
{
    /// <summary>
    /// 自定义缓存
    /// </summary>
    /// <typeparam name="TValueType">表格绑定类型</typeparam>
    public interface TmphICustom<TValueType>
    {
        /// <summary>
        /// 添加缓存数据
        /// </summary>
        /// <param name="value">缓存数据</param>
        /// <returns>是否添加数据</returns>
        bool Add(TValueType value);

        /// <summary>
        /// 更新缓存数据
        /// </summary>
        /// <param name="value">缓存数据</param>
        /// <param name="oldValue">旧数据</param>
        /// <returns>添加数据返回正数，删除数据返回负数，没有变化返回0</returns>
        int Update(TValueType value, TValueType oldValue);

        /// <summary>
        /// 删除缓存数据
        /// </summary>
        /// <param name="value">缓存数据</param>
        /// <returns>是否存在被删除数据</returns>
        bool Remove(TValueType value);

        /// <summary>
        /// 所有缓存数据
        /// </summary>
        IEnumerable<TValueType> Values { get; }
    }
}