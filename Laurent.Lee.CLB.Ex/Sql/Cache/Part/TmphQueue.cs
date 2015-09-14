using System;
using System.Threading;
using System.Linq.Expressions;
using Laurent.Lee.CLB.Code.CSharp;

namespace Laurent.Lee.CLB.Sql.Cache.Part
{
    /// <summary>
    /// 先进先出优先队列缓存
    /// </summary>
    /// <typeparam name="TValueType">表格绑定类型</typeparam>
    /// <typeparam name="TModelType">表格模型类型</typeparam>
    /// <typeparam name="TCounterKeyType">缓存统计关键字类型</typeparam>
    /// <typeparam name="TKeyType">关键字类型</typeparam>
    /// <typeparam name="TCacheValueType">缓存数据类型</typeparam>
    public abstract class TmphQueue<TValueType, TModelType, TCounterKeyType, TKeyType, TCacheValueType>
        : TmphCounterCache<TValueType, TModelType, TCounterKeyType>
        where TValueType : class, TModelType
        where TModelType : class
        where TKeyType : IEquatable<TKeyType>
        where TCounterKeyType : IEquatable<TCounterKeyType>
        where TCacheValueType : class
    {
        /// <summary>
        /// 缓存关键字获取器
        /// </summary>
        protected Func<TModelType, TKeyType> getKey;
        /// <summary>
        /// 缓存默认最大容器大小
        /// </summary>
        protected int maxCount;
        /// <summary>
        /// 数据集合
        /// </summary>
        protected TmphFifoPriorityQueue<TKeyType, TCacheValueType> queueCache = new TmphFifoPriorityQueue<TKeyType, TCacheValueType>();
        /// <summary>
        /// 先进先出优先队列缓存
        /// </summary>
        /// <param name="counter">缓存计数器</param>
        /// <param name="getKey">缓存关键字获取器</param>
        /// <param name="maxCount">缓存默认最大容器大小</param>
        protected TmphQueue(Events.TmphCounter<TValueType, TModelType, TCounterKeyType> counter, Expression<Func<TModelType, TKeyType>> getKey, int maxCount)
            : base(counter)
        {
            if (getKey == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            counter.SqlTool.SetSelectMember(getKey);
            this.getKey = getKey.Compile();
            this.maxCount = maxCount <= 0 ? Config.TmphSql.Default.CacheMaxCount : maxCount;
        }
        /// <summary>
        /// 先进先出优先队列缓存
        /// </summary>
        /// <param name="counter">缓存计数器</param>
        /// <param name="getKey">缓存关键字获取器</param>
        /// <param name="maxCount">缓存默认最大容器大小</param>
        protected TmphQueue(Events.TmphCounter<TValueType, TModelType, TCounterKeyType> counter, Func<TModelType, TKeyType> getKey, int maxCount)
            : base(counter)
        {
            if (getKey == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            this.getKey = getKey;
            this.maxCount = maxCount <= 0 ? Config.TmphSql.Default.CacheMaxCount : maxCount;
        }
        /// <summary>
        /// 重置缓存
        /// </summary>
        protected void reset()
        {
            queueCache.Clear();
        }
        /// <summary>
        /// 获取缓存数据
        /// </summary>
        /// <param name="key">关键字</param>
        /// <returns>缓存数据,失败返回null</returns>
        public TCacheValueType TryGet(TKeyType key)
        {
            Monitor.Enter(counter.SqlTool.Lock);
            try
            {
                return queueCache.Get(key, null);
            }
            finally { Monitor.Exit(counter.SqlTool.Lock); }
        }
    }
    /// <summary>
    /// 先进先出优先队列缓存
    /// </summary>
    /// <typeparam name="TValueType">表格绑定类型</typeparam>
    /// <typeparam name="TModelType">表格模型类型</typeparam>
    /// <typeparam name="TKeyType">关键字类型</typeparam>
    public sealed class TmphQueue<TValueType, TModelType, TKeyType>
        : TmphQueue<TValueType, TModelType, TKeyType, TKeyType, TValueType>
        where TValueType : class, TModelType
        where TModelType : class
        where TKeyType : IEquatable<TKeyType>
    {
        /// <summary>
        /// 数据获取器
        /// </summary>
        private Func<TKeyType, Laurent.Lee.CLB.Code.TmphMemberMap, TValueType> getValue;
        /// <summary>
        /// 缓存数据
        /// </summary>
        /// <param name="key">关键字</param>
        /// <returns>缓存数据</returns>
        public TValueType this[TKeyType key]
        {
            get
            {
                Monitor.Enter(counter.SqlTool.Lock);
                try
                {
                    TValueType value = queueCache.Get(key, null);
                    if (value != null) return value;
                    if (getKey == counter.GetKey)
                    {
                        value = counter.Get(key);
                        if (value != null) return value;
                    }
                    if ((value = getValue(key, counter.MemberMap)) != null) onInserted(value);
                    return value;
                }
                finally { Monitor.Exit(counter.SqlTool.Lock); }
            }
        }
        /// <summary>
        /// 先进先出优先队列缓存
        /// </summary>
        /// <param name="counter">缓存计数器</param>
        /// <param name="getValue">数据获取器,禁止锁操作</param>
        /// <param name="maxCount">缓存默认最大容器大小</param>
        public TmphQueue(Events.TmphCounter<TValueType, TModelType, TKeyType> counter
            , Func<TKeyType, Laurent.Lee.CLB.Code.TmphMemberMap, TValueType> getValue, int maxCount = 0)
            : base(counter, counter.GetKey, maxCount)
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
            onInserted(value, getKey(value));
        }
        /// <summary>
        /// 增加数据
        /// </summary>
        /// <param name="value">新增的数据</param>
        /// <param name="key">关键字</param>
        private void onInserted(TValueType value, TKeyType key)
        {
            queueCache[key] = counter.Add(value);
            if (queueCache.Count > maxCount) counter.Remove(queueCache.Pop().Value);
        }
        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="TCacheValue">缓存数据</param>
        /// <param name="value">更新后的数据</param>
        /// <param name="oldValue">更新前的数据</param>
        private void onUpdated(TValueType TCacheValue, TValueType value, TValueType oldValue)
        {
            if (TCacheValue != null)
            {
                TKeyType key = getKey(value), oldKey = getKey(oldValue);
                if (!key.Equals(oldKey))
                {
                    TValueType removeValue;
                    if (queueCache.Remove(oldKey, out removeValue)) queueCache.Set(key, TCacheValue);
                    else onInserted(TCacheValue, key);
                }
            }
        }
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="value">被删除的数据</param>
        private void onDeleted(TValueType value)
        {
            queueCache.Remove(getKey(value), out value);
        }
    }
}
