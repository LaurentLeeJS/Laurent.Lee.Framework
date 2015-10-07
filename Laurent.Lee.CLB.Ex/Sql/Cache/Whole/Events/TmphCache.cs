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
using System.Collections.Generic;

namespace Laurent.Lee.CLB.Sql.Cache.Whole.Events
{
    /// <summary>
    /// 事件缓存
    /// </summary>
    /// <typeparam name="TValueType">表格类型</typeparam>
    /// <typeparam name="TModelType">模型类型</typeparam>
    public abstract class TmphCache<TValueType, TModelType> : TmphCopy<TValueType, TModelType>
        where TValueType : class, TModelType
        where TModelType : class
    {
        /// <summary>
        /// 数据集合
        /// </summary>
        public abstract IEnumerable<TValueType> Values { get; }

        /// <summary>
        /// 添加记录事件
        /// </summary>
        public event Action<TValueType> OnInserted;

        /// <summary>
        /// 添加记录
        /// </summary>
        /// <param name="value">新添加的对象</param>
        protected void callOnInserted(TValueType value)
        {
            if (OnInserted != null) OnInserted(value);
        }

        /// <summary>
        /// 更新记录事件
        /// </summary>
        public event Action<TValueType, TValueType> OnUpdated;

        /// <summary>
        /// 更新记录
        /// </summary>
        /// <param name="value">更新后的对象</param>
        /// <param name="oldValue">更新前的对象</param>
        protected void callOnUpdated(TValueType value, TValueType oldValue)
        {
            if (OnUpdated != null) OnUpdated(value, oldValue);
        }

        /// <summary>
        /// 删除记录事件
        /// </summary>
        public event Action<TValueType> OnDeleted;

        /// <summary>
        /// 删除记录
        /// </summary>
        /// <param name="value">被删除的对象</param>
        protected void callOnDeleted(TValueType value)
        {
            if (OnDeleted != null) OnDeleted(value);
        }

        /// <summary>
        /// SQL操作缓存
        /// </summary>
        /// <param name="sqlTool">SQL操作工具</param>
        /// <param name="group">数据分组</param>
        protected TmphCache(Laurent.Lee.CLB.Emit.TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, int group)
            : base(sqlTool, group)
        {
        }
    }
}