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
using System.Linq.Expressions;
using System.Threading;

namespace Laurent.Lee.CLB.Sql.Cache.Part
{
    /// <summary>
    /// 先进先出优先队列自定义缓存
    /// </summary>
    /// <typeparam name="TValueType">表格绑定类型</typeparam>
    /// <typeparam name="TModelType">表格模型类型</typeparam>
    /// <typeparam name="TCounterKeyType">缓存统计关键字类型</typeparam>
    /// <typeparam name="TKeyType">关键字类型</typeparam>
    /// <typeparam name="TCacheValueType">缓存数据类型</typeparam>
    public class TmphQueueExpressionCustom<TValueType, TModelType, TCounterKeyType, TKeyType, TCacheValueType>
        : TmphQueueExpression<TValueType, TModelType, TCounterKeyType, TKeyType, TCacheValueType>
        where TValueType : class, TModelType
        where TModelType : class
        where TCounterKeyType : IEquatable<TCounterKeyType>
        where TKeyType : IEquatable<TKeyType>
        where TCacheValueType : class, TmphICustom<TValueType>
    {
        /// <summary>
        /// 自定义缓存获取器
        /// </summary>
        private Func<TKeyType, IEnumerable<TValueType>, TCacheValueType> getValue;

        /// <summary>
        /// 获取缓存数据
        /// </summary>
        /// <param name="key">关键字</param>
        /// <returns>缓存数据</returns>
        public TCacheValueType this[TKeyType key]
        {
            get
            {
                Monitor.Enter(counter.SqlTool.Lock);
                try
                {
                    TCacheValueType values = queueCache.Get(key, null);
                    if (values != null) return values;
                    queueCache[key] = values = getValue(key, counter.SqlTool.Where(getWhere(key), counter.MemberMap).getArray(value => counter.Add(value)));
                    if (queueCache.Count > maxCount)
                    {
                        foreach (TValueType value in queueCache.Pop().Value.Values) counter.Remove(value);
                    }
                    return values;
                }
                finally { Monitor.Exit(counter.SqlTool.Lock); }
            }
        }

        /// <summary>
        /// 先进先出优先队列自定义缓存
        /// </summary>
        /// <param name="counter">缓存计数器</param>
        /// <param name="getKey">缓存关键字获取器</param>
        /// <param name="where">条件表达式</param>
        /// <param name="maxCount">缓存默认最大容器大小</param>
        public TmphQueueExpressionCustom(Events.TmphCounter<TValueType, TModelType, TCounterKeyType> counter, Expression<Func<TModelType, TKeyType>> getKey
            , Func<TKeyType, Expression<Func<TModelType, bool>>> getWhere, Func<TKeyType, IEnumerable<TValueType>, TCacheValueType> getValue, int maxCount = 0)
            : base(counter, getKey, maxCount, getWhere)
        {
            if (getValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            this.getValue = getValue;

            counter.OnReset += reset;
            counter.SqlTool.OnInsertedLock += onInserted;
            counter.OnUpdated += onUpdated;
            counter.OnDeleted += onDeleted;
        }

        /// <summary>
        /// 增加数据
        /// </summary>
        /// <param name="value">新增的数据</param>
        private void onInserted(TValueType value)
        {
            TKeyType key = getKey(value);
            TCacheValueType values = queueCache.Get(key, null);
            if (values != null && !values.Add(value = counter.Add(value))) counter.Remove(value);
        }

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="TCacheValue">缓存数据</param>
        /// <param name="value">更新后的数据</param>
        /// <param name="oldValue">更新前的数据</param>
        private void onUpdated(TValueType TCacheValue, TValueType value, TValueType oldValue)
        {
            TKeyType key = getKey(value);
            if (TCacheValue == null)
            {
                TCacheValueType values;
                if (queueCache.Remove(key, out values))
                {
                    foreach (TValueType removeValue in values.Values) counter.Remove(removeValue);
                }
            }
            else
            {
                TKeyType oldKey = getKey(oldValue);
                TCacheValueType values = queueCache.Get(key, null);
                if (key.Equals(oldKey))
                {
                    if (values != null)
                    {
                        int updateValue = values.Update(TCacheValue, oldValue);
                        if (updateValue != 0)
                        {
                            if (updateValue > 1) counter.Add(TCacheValue);
                            else counter.Remove(TCacheValue);
                        }
                    }
                }
                else
                {
                    TCacheValueType oldValues = queueCache.Get(oldKey, null);
                    if (values != null)
                    {
                        if (oldValues != null)
                        {
                            if (oldValues.Remove(oldValue))
                            {
                                if (!values.Add(TCacheValue)) counter.Remove(TCacheValue);
                            }
                            else if (values.Add(TCacheValue)) counter.Add(TCacheValue);
                        }
                        else if (values.Add(TCacheValue)) counter.Add(TCacheValue);
                    }
                    else if (oldValues != null && oldValues.Remove(oldValue)) counter.Remove(TCacheValue);
                }
            }
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="value">被删除的数据</param>
        private void onDeleted(TValueType value)
        {
            TCacheValueType values = queueCache.Get(getKey(value), null);
            if (values != null) values.Remove(value);
        }
    }
}