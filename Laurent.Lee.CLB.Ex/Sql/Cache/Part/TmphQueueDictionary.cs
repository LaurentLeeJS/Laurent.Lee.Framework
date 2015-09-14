using System;
using Laurent.Lee.CLB.Code.CSharp;
using System.Threading;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Laurent.Lee.CLB.Sql.Cache.Part
{
    /// <summary>
    /// 先进先出优先队列 字典缓存(反射模式)
    /// </summary>
    /// <typeparam name="TValueType">表格绑定类型</typeparam>
    /// <typeparam name="TCounterKeyType">缓存统计关键字类型</typeparam>
    /// <typeparam name="TKeyType">关键字类型</typeparam>
    /// <typeparam name="dictionaryKeyType">字典关键字类型</typeparam>
    public sealed class TmphQueueDictionary<TValueType, TModelType, TCounterKeyType, TKeyType, dictionaryKeyType>
        : TmphQueueExpression<TValueType, TModelType, TCounterKeyType, TKeyType, Dictionary<dictionaryKeyType, TValueType>>
        where TValueType : class, TModelType
        where TModelType : class
        where TCounterKeyType : IEquatable<TCounterKeyType>
        where TKeyType : IEquatable<TKeyType>
        where dictionaryKeyType : IEquatable<dictionaryKeyType>
    {
        /// <summary>
        /// 缓存字典关键字获取器
        /// </summary>
        private Func<TValueType, dictionaryKeyType> getDictionaryKey;
        /// <summary>
        /// 先进先出优先队列 字典缓存
        /// </summary>
        /// <param name="counter">缓存计数器</param>
        /// <param name="getKey">缓存关键字获取器</param>
        /// <param name="getWhere">条件表达式获取器</param>
        /// <param name="getDictionaryKey">缓存字典关键字获取器</param>
        /// <param name="maxCount">缓存默认最大容器大小</param>
        public TmphQueueDictionary(Events.TmphCounter<TValueType, TModelType, TCounterKeyType> counter
            , Expression<Func<TModelType, TKeyType>> getKey, Func<TKeyType, Expression<Func<TModelType, bool>>> getWhere
            , Func<TValueType, dictionaryKeyType> getDictionaryKey
            , int maxCount = 0)
            : base(counter, getKey, maxCount, getWhere)
        {
            if (getDictionaryKey == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            this.getDictionaryKey = getDictionaryKey;

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
            Dictionary<dictionaryKeyType, TValueType> values = queueCache.Get(key, null);
            if (values != null) values.Add(getDictionaryKey(value), counter.Add(value));
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
                Dictionary<dictionaryKeyType, TValueType> values;
                if (queueCache.Remove(key, out values))
                {
                    foreach (TValueType removeValue in values.Values) counter.Remove(removeValue);
                }
            }
            else
            {
                TKeyType oldKey = getKey(oldValue);
                if (key.Equals(oldKey))
                {
                    Dictionary<dictionaryKeyType, TValueType> values = queueCache.Get(key, null);
                    if (values != null)
                    {
                        dictionaryKeyType dictionaryKey = getDictionaryKey(TCacheValue), oldDictionaryKey = getDictionaryKey(oldValue);
                        if (!dictionaryKey.Equals(oldDictionaryKey))
                        {
                            values.Add(dictionaryKey, TCacheValue);
                            values.Remove(oldDictionaryKey);
                        }
                    }
                }
                else
                {
                    Dictionary<dictionaryKeyType, TValueType> values = queueCache.Get(key, null);
                    Dictionary<dictionaryKeyType, TValueType> oldValues = queueCache.Get(oldKey, null);
                    if (values != null)
                    {
                        if (oldValues != null)
                        {
                            values.Add(getDictionaryKey(TCacheValue), TCacheValue);
                            if (!oldValues.Remove(getDictionaryKey(oldValue)))
                            {
                                TmphLog.Error.Add(typeof(TValueType).FullName + " 缓存同步错误", false, true);
                            }
                        }
                        else values.Add(getDictionaryKey(TCacheValue), counter.Add(TCacheValue));
                    }
                    else if (oldValues != null)
                    {
                        if (oldValues.Remove(getDictionaryKey(value))) counter.Remove(TCacheValue);
                        else TmphLog.Error.Add(typeof(TValueType).FullName + " 缓存同步错误", false, true);
                    }
                }
            }
        }
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="value">被删除的数据</param>
        private void onDeleted(TValueType value)
        {
            Dictionary<dictionaryKeyType, TValueType> values = queueCache.Get(getKey(value), null);
            if (values != null && !values.Remove(getDictionaryKey(value)))
            {
                TmphLog.Error.Add(typeof(TValueType).FullName + " 缓存同步错误", false, true);
            }
        }
        /// <summary>
        /// 判断是否存在关键字匹配的缓存
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="dictionaryKey">字典关键字</param>
        /// <param name="isKey">是否存在关键字</param>
        /// <returns>是否存在关键字匹配的缓存</returns>
        public bool TryIsKey(TKeyType key, dictionaryKeyType dictionaryKey, out bool isKey)
        {
            Monitor.Enter(counter.SqlTool.Lock);
            try
            {
                Dictionary<dictionaryKeyType, TValueType> values = queueCache.Get(key, null);
                isKey = values != null;
                if (isKey) return values.ContainsKey(dictionaryKey);
            }
            finally { Monitor.Exit(counter.SqlTool.Lock); }
            return false;
        }
        /// <summary>
        /// 获取字典缓存
        /// </summary>
        /// <param name="key">关键字</param>
        /// <returns>字典缓存</returns>
        private Dictionary<dictionaryKeyType, TValueType> getDictionary(TKeyType key)
        {
            Dictionary<dictionaryKeyType, TValueType> values = queueCache.Get(key, null);
            if (values == null)
            {
                values = TmphDictionary<dictionaryKeyType>.Create<TValueType>();
                foreach (TValueType value in counter.SqlTool.Where(getWhere(key), counter.MemberMap))
                {
                    values.Add(getDictionaryKey(value), counter.Add(value));
                }
                queueCache[key] = values;
            }
            return values;
        }
        /// <summary>
        /// 获取匹配数据
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="dictionaryKey">字典关键字</param>
        /// <param name="nullValue">失败返回值</param>
        /// <returns>匹配数据</returns>
        public TValueType Get(TKeyType key, dictionaryKeyType dictionaryKey, TValueType nullValue)
        {
            Monitor.Enter(counter.SqlTool.Lock);
            try
            {
                Dictionary<dictionaryKeyType, TValueType> values = getDictionary(key);
                if (values != null)
                {
                    TValueType value;
                    if (values.TryGetValue(dictionaryKey, out value)) return value;
                }
            }
            finally { Monitor.Exit(counter.SqlTool.Lock); }
            return nullValue;
        }
        /// <summary>
        /// 判断关键字是否存在
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="dictionaryKey">字典关键字</param>
        /// <returns>关键字是否存在</returns>
        public bool IsKey(TKeyType key, dictionaryKeyType dictionaryKey)
        {
            Monitor.Enter(counter.SqlTool.Lock);
            try
            {
                Dictionary<dictionaryKeyType, TValueType> values = getDictionary(key);
                return values != null && values.ContainsKey(dictionaryKey);
            }
            finally { Monitor.Exit(counter.SqlTool.Lock); }
        }
        /// <summary>
        /// 获取缓存数据数量
        /// </summary>
        /// <param name="key">关键字</param>
        /// <returns>缓存数据数量</returns>
        public int GetCount(TKeyType key)
        {
            Monitor.Enter(counter.SqlTool.Lock);
            try
            {
                Dictionary<dictionaryKeyType, TValueType> values = getDictionary(key);
                if (values != null) return values.Count;
            }
            finally { Monitor.Exit(counter.SqlTool.Lock); }
            return 0;
        }
        /// <summary>
        /// 获取缓存数组
        /// </summary>
        /// <param name="key">关键字</param>
        /// <returns>缓存数组</returns>
        public TValueType[] GetArray(TKeyType key)
        {
            Monitor.Enter(counter.SqlTool.Lock);
            try
            {
                Dictionary<dictionaryKeyType, TValueType> values = getDictionary(key);
                return values != null ? values.Values.GetArray() : TmphNullValue<TValueType>.Array;
            }
            finally { Monitor.Exit(counter.SqlTool.Lock); }
        }
        /// <summary>
        /// 获取缓存关键字数组
        /// </summary>
        /// <param name="key">关键字</param>
        /// <returns>关键字数组</returns>
        public dictionaryKeyType[] GetKeyArray(TKeyType key)
        {
            Monitor.Enter(counter.SqlTool.Lock);
            try
            {
                Dictionary<dictionaryKeyType, TValueType> values = getDictionary(key);
                return values != null ? values.Keys.GetArray() : TmphNullValue<dictionaryKeyType>.Array;
            }
            finally { Monitor.Exit(counter.SqlTool.Lock); }
        }
        /// <summary>
        /// 获取匹配缓存数据集合
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="isValue">数据匹配器,禁止锁操作</param>
        /// <returns>匹配缓存数据集合</returns>
        public TmphSubArray<TValueType> GetFindLock(TKeyType key, Func<TValueType, bool> isValue)
        {
            Monitor.Enter(counter.SqlTool.Lock);
            try
            {
                Dictionary<dictionaryKeyType, TValueType> values = getDictionary(key);
                if (values != null) return values.Values.GetFind(isValue);
            }
            finally { Monitor.Exit(counter.SqlTool.Lock); }
            return default(TmphSubArray<TValueType>);
        }
        /// <summary>
        /// 获取一个匹配数据
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="isValue">数据匹配器,禁止锁操作</param>
        /// <returns>匹配数据,失败返回null</returns>
        public TValueType FirstOrDefaultLock(TKeyType key, Func<TValueType, bool> isValue)
        {
            Monitor.Enter(counter.SqlTool.Lock);
            try
            {
                Dictionary<dictionaryKeyType, TValueType> values = getDictionary(key);
                if (values != null) return values.Values.firstOrDefault(isValue);
            }
            finally { Monitor.Exit(counter.SqlTool.Lock); }
            return null;
        }
    }
}