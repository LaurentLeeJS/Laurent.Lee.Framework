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
using System.Threading;

namespace Laurent.Lee.CLB.Sql.Cache.Whole
{
    /// <summary>
    /// 分组字典缓存
    /// </summary>
    /// <typeparam name="TValueType">表格绑定类型</typeparam>
    /// <typeparam name="TModelType">表格模型类型</typeparam>
    /// <typeparam name="TmphGroupKeyType">分组关键字类型</typeparam>
    /// <typeparam name="TKeyType">字典关键字类型</typeparam>
    public class TmphDictionaryDictionary<TValueType, TModelType, TmphGroupKeyType, TKeyType>
        where TValueType : class, TModelType
        where TModelType : class
        where TmphGroupKeyType : IEquatable<TmphGroupKeyType>
        where TKeyType : IEquatable<TKeyType>
    {
        /// <summary>
        /// 整表缓存
        /// </summary>
        protected Events.TmphCache<TValueType, TModelType> cache;

        /// <summary>
        /// 分组关键字获取器
        /// </summary>
        protected Func<TValueType, TmphGroupKeyType> getGroupKey;

        /// <summary>
        /// 字典关键字获取器
        /// </summary>
        protected Func<TValueType, TKeyType> getKey;

        /// <summary>
        /// 分组数据
        /// </summary>
        protected Dictionary<TmphGroupKeyType, Dictionary<TKeyType, TValueType>> groups;

        /// <summary>
        /// 分组字典缓存
        /// </summary>
        /// <param name="cache">整表缓存</param>
        /// <param name="getGroupKey">分组关键字获取器</param>
        /// <param name="getKey">字典关键字获取器</param>
        /// <param name="isValue">数据匹配器</param>
        public TmphDictionaryDictionary(Events.TmphCache<TValueType, TModelType> cache
            , Func<TValueType, TmphGroupKeyType> getGroupKey, Func<TValueType, TKeyType> getKey, bool isReset)
        {
            if (cache == null || getGroupKey == null || getKey == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            this.cache = cache;
            this.getGroupKey = getGroupKey;
            this.getKey = getKey;

            if (isReset)
            {
                cache.OnReset += reset;
                cache.OnInserted += onInserted;
                cache.OnUpdated += onUpdated;
                cache.OnDeleted += onDeleted;
                resetLock();
            }
        }

        /// <summary>
        /// 重新加载数据
        /// </summary>
        protected void resetLock()
        {
            Monitor.Enter(cache.SqlTool.Lock);
            try
            {
                reset();
            }
            finally { Monitor.Exit(cache.SqlTool.Lock); }
        }

        /// <summary>
        /// 重新加载数据
        /// </summary>
        protected virtual void reset()
        {
            groups = TmphDictionary<TmphGroupKeyType>.Create<Dictionary<TKeyType, TValueType>>();
            foreach (TValueType value in cache.Values) onInserted(value);
        }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="value">数据对象</param>
        protected void onInserted(TValueType value)
        {
            onInserted(value, getGroupKey(value));
        }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="value">数据对象</param>
        protected void onInserted(TValueType value, TmphGroupKeyType key)
        {
            Dictionary<TKeyType, TValueType> values;
            if (!groups.TryGetValue(key, out values)) groups.Add(key, values = TmphDictionary<TKeyType>.Create<TValueType>());
            values.Add(getKey(value), value);
        }

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="value">更新后的数据</param>
        /// <param name="oldValue">更新前的数据</param>
        protected void onUpdated(TValueType value, TValueType oldValue)
        {
            TmphGroupKeyType groupKey = getGroupKey(value), oldGroupKey = getGroupKey(oldValue);
            if (groupKey.Equals(oldGroupKey))
            {
                TKeyType key = getKey(value), oldKey = getKey(oldValue);
                if (!key.Equals(oldKey))
                {
                    Dictionary<TKeyType, TValueType> dictionary;
                    if (groups.TryGetValue(groupKey, out dictionary) && dictionary.Remove(oldKey))
                    {
                        dictionary.Add(key, value);
                    }
                    else TmphLog.Error.Add(typeof(TValueType).FullName + " 缓存同步错误", false, true);
                }
            }
            else
            {
                onInserted(value, groupKey);
                onDeleted(oldValue, oldGroupKey);
            }
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="value">被删除的数据</param>
        /// <param name="groupKey">分组关键字</param>
        protected void onDeleted(TValueType value, TmphGroupKeyType groupKey)
        {
            Dictionary<TKeyType, TValueType> dictionary;
            if (groups.TryGetValue(groupKey, out dictionary) && dictionary.Remove(getKey(value)))
            {
                if (dictionary.Count == 0) groups.Remove(groupKey);
            }
            else TmphLog.Error.Add(typeof(TValueType).FullName + " 缓存同步错误", false, true);
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="value">被删除的数据</param>
        protected void onDeleted(TValueType value)
        {
            onDeleted(value, getGroupKey(value));
        }

        /// <summary>
        /// 获取关键字集合
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public ICollection<TKeyType> GetKeys(TmphGroupKeyType key)
        {
            Dictionary<TKeyType, TValueType> dictionary;
            if (groups.TryGetValue(key, out dictionary)) return dictionary.Keys;
            return TmphNullValue<TKeyType>.Array;
        }
    }
}