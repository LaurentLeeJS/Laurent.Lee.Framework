using System;
using Laurent.Lee.CLB.Code.CSharp;
using System.Collections.Generic;
using System.Threading;

namespace Laurent.Lee.CLB.Sql.Cache.Whole
{
    /// <summary>
    /// 字典缓存
    /// </summary>
    /// <typeparam name="TValueType">表格绑定类型</typeparam>
    /// <typeparam name="TModelType">表格模型类型</typeparam>
    /// <typeparam name="TKeyType">关键字类型</typeparam>
    public class TmphDictionary<TValueType, TModelType, TKeyType>
        where TValueType : class, TModelType
        where TModelType : class
        where TKeyType : IEquatable<TKeyType>
    {
        /// <summary>
        /// 整表缓存
        /// </summary>
        protected Events.TmphCache<TValueType, TModelType> cache;
        /// <summary>
        /// 分组字典关键字获取器
        /// </summary>
        protected Func<TValueType, TKeyType> getKey;
        /// <summary>
        /// 字典缓存
        /// </summary>
        protected Dictionary<TKeyType, TValueType> values;
        /// <summary>
        /// 缓存数据
        /// </summary>
        /// <param name="key">关键字</param>
        /// <returns>缓存数据,失败返回null</returns>
        public TValueType this[TKeyType key]
        {
            get
            {
                TValueType value;
                return values.TryGetValue(key, out value) ? value : null;
            }
        }
        /// <summary>
        /// 分组列表缓存
        /// </summary>
        /// <param name="cache">整表缓存</param>
        /// <param name="getKey">分组字典关键字获取器</param>
        /// <param name="isReset">是否初始化</param>
        public TmphDictionary(Events.TmphCache<TValueType, TModelType> cache, Func<TValueType, TKeyType> getKey, bool isReset = true)
        {
            if (cache == null || getKey == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            this.cache = cache;
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
            Dictionary<TKeyType, TValueType> newValues = TmphDictionary<TKeyType>.Create<TValueType>();
            foreach (TValueType value in cache.Values) newValues.Add(getKey(value), value);
            values = newValues;
        }
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="value">数据对象</param>
        protected virtual void onInserted(TValueType value)
        {
            onInserted(value, getKey(value));
        }
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="value">数据对象</param>
        /// <param name="key">数据对象的关键字</param>
        protected void onInserted(TValueType value, TKeyType key)
        {
            values.Add(key, value);
        }
        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="value">更新后的数据</param>
        /// <param name="oldValue">更新前的数据</param>
        protected virtual void onUpdated(TValueType value, TValueType oldValue)
        {
            TKeyType key = getKey(value), oldKey = getKey(oldValue);
            if (!key.Equals(oldKey))
            {
                onInserted(value, key);
                onDeleted(oldKey);
            }
        }
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="key">被删除数据的关键字</param>
        protected void onDeleted(TKeyType key)
        {
            if (!values.Remove(key))
            {
                TmphLog.Error.Add(typeof(TValueType).FullName + " 缓存同步错误", false, true);
            }
        }
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="value">被删除的数据</param>
        protected virtual void onDeleted(TValueType value)
        {
            onDeleted(getKey(value));
        }
        /// <summary>
        /// 获取数据集合
        /// </summary>
        /// <returns></returns>
        public TValueType[] GetArray()
        {
            Monitor.Enter(cache.SqlTool.Lock);
            try
            {
                return values.Values.GetArray();
            }
            finally { Monitor.Exit(cache.SqlTool.Lock); }
        }
        /// <summary>
        /// 获取数据集合
        /// </summary>
        /// <param name="isValue"></param>
        /// <returns></returns>
        public TmphSubArray<TValueType> GetFindArray(Func<TValueType, bool> isValue)
        {
            if (isValue == null) TmphLog.Default.Throw(TmphLog.TmphExceptionType.Null);
            TValueType[] array;
            int count = 0;
            Monitor.Enter(cache.SqlTool.Lock);
            try
            {
                array = new TValueType[values.Count];
                foreach (TValueType value in values.Values)
                {
                    if (isValue(value)) array[count++] = value;
                }
            }
            finally { Monitor.Exit(cache.SqlTool.Lock); }
            return TmphSubArray<TValueType>.Unsafe(array, 0, count);
        }
    }
}
