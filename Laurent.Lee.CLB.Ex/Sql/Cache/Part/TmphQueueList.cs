using System;
using Laurent.Lee.CLB.Code.CSharp;
using System.Threading;
using System.Linq.Expressions;

namespace Laurent.Lee.CLB.Sql.Cache.Part
{
    /// <summary>
    /// 先进先出优先队列 列表缓存
    /// </summary>
    /// <typeparam name="TValueType">表格绑定类型</typeparam>
    /// <typeparam name="TModelType">表格模型类型</typeparam>
    /// <typeparam name="TCounterKeyType">缓存统计关键字类型</typeparam>
    /// <typeparam name="TKeyType">关键字类型</typeparam>
    public class TmphQueueList<TValueType, TModelType, TCounterKeyType, TKeyType>
        : TmphQueueExpression<TValueType, TModelType, TCounterKeyType, TKeyType, TmphList<TValueType>>
        where TValueType : class, TModelType
        where TModelType : class
        where TCounterKeyType : IEquatable<TCounterKeyType>
        where TKeyType : IEquatable<TKeyType>
    {
        /// <summary>
        /// 先进先出优先队列 列表缓存
        /// </summary>
        /// <param name="counter">缓存计数器</param>
        /// <param name="getKey">缓存关键字获取器</param>
        /// <param name="getWhere">条件表达式获取器</param>
        /// <param name="maxCount">缓存默认最大容器大小</param>
        public TmphQueueList(Events.TmphCounter<TValueType, TModelType, TCounterKeyType> counter
            , Expression<Func<TModelType, TKeyType>> getKey, Func<TKeyType, Expression<Func<TModelType, bool>>> getWhere, int maxCount = 0)
            : base(counter, getKey, maxCount, getWhere)
        {
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
            TmphList<TValueType> values = queueCache.Get(key, null);
            if (values != null) values.Add(counter.Add(value));
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
                TmphList<TValueType> values;
                if (queueCache.Remove(key, out values))
                {
                    foreach (TValueType removeValue in values) counter.Remove(removeValue);
                }
            }
            else
            {
                TKeyType oldKey = getKey(oldValue);
                if (!key.Equals(oldKey))
                {
                    TmphList<TValueType> values = queueCache.Get(key, null), oldValues = queueCache.Get(oldKey, null);
                    if (values != null)
                    {
                        if (oldValues != null)
                        {
                            values.Add(TCacheValue);
                            if (!oldValues.Remove(TCacheValue)) TmphLog.Error.Add(typeof(TValueType).FullName + " 缓存同步错误", false, true);
                        }
                        else values.Add(counter.Add(TCacheValue));
                    }
                    else if (oldValues != null)
                    {
                        if (oldValues.Remove(TCacheValue)) counter.Remove(TCacheValue);
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
            TmphList<TValueType> values = queueCache.Get(getKey(value), null);
            if (values != null && !values.Remove(value)) TmphLog.Error.Add(typeof(TValueType).FullName + " 缓存同步错误", false, true);
        }
        /// <summary>
        /// 读取数据库数据列表
        /// </summary>
        /// <param name="key">关键字</param>
        /// <returns>数据列表</returns>
        private TmphList<TValueType> getList(TKeyType key)
        {
            TmphList<TValueType> values = counter.SqlTool.Where(getWhere(key), counter.MemberMap).getList();
            if (values != null)
            {
                if (values.Count != 0)
                {
                    int index = 0, count = values.Count;
                    TValueType[] array = values.Unsafer.Array;
                    foreach (TValueType value in array)
                    {
                        array[index] = counter.Add(value);
                        if (++index == count) break;
                    }
                }
            }
            else values = new TmphList<TValueType>();
            queueCache[key] = values;
            if (queueCache.Count > maxCount)
            {
                foreach (TValueType value in queueCache.Pop().Value) counter.Remove(value);
            }
            return values;
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
                TmphList<TValueType> values = queueCache.Get(key, null);
                if (values != null) return values.FirstOrDefault(isValue);
                values = getList(key);
                if (values != null) return values.FirstOrDefault(isValue);
            }
            finally { Monitor.Exit(counter.SqlTool.Lock); }
            return null;
        }
        /// <summary>
        /// 获取一个匹配数据
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="isValue">数据匹配器,禁止锁操作</param>
        /// <param name="isKey">是否存在关键字</param>
        /// <returns>匹配数据,失败返回null</returns>
        public TValueType TryFirstOrDefaultLock(TKeyType key, Func<TValueType, bool> isValue, out bool isKey)
        {
            Monitor.Enter(counter.SqlTool.Lock);
            try
            {
                TmphList<TValueType> values = queueCache.Get(key, null);
                if (values != null)
                {
                    isKey = true;
                    return values.FirstOrDefault(isValue);
                }
            }
            finally { Monitor.Exit(counter.SqlTool.Lock); }
            isKey = false;
            return null;
        }
        /// <summary>
        /// 获取缓存数据集合
        /// </summary>
        /// <param name="key">关键字</param>
        /// <returns>数据集合</returns>
        public TValueType[] GetArray(TKeyType key)
        {
            Monitor.Enter(counter.SqlTool.Lock);
            try
            {
                TmphList<TValueType> values = queueCache.Get(key, null);
                if (values != null) return values.GetArray();
                values = getList(key);
                if (values != null) return values.GetArray();
            }
            finally { Monitor.Exit(counter.SqlTool.Lock); }
            return TmphNullValue<TValueType>.Array;
        }
        /// <summary>
        /// 获取缓存数据集合
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="isValue">数据匹配器,禁止锁操作</param>
        /// <returns>数据集合</returns>
        public TValueType[] GetFindArray(TKeyType key, Func<TValueType, bool> isValue)
        {
            TmphList<TValueType> list;
            Monitor.Enter(counter.SqlTool.Lock);
            try
            {
                list = queueCache.Get(key, null);
                if (list == null) list = getList(key);
            }
            finally { Monitor.Exit(counter.SqlTool.Lock); }
            return list.ToSubArray().GetFindArray(isValue);
        }
        /// <summary>
        /// 获取范围排序数据集合
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="comparer">排序比较器,禁止锁操作</param>
        /// <param name="skipCount">跳过记录数</param>
        /// <param name="getCount">获取记录数</param>
        /// <returns>范围排序数据集合</returns>
        public TValueType[] GetRangeSortLock(TKeyType key, Func<TValueType, TValueType, int> comparer, int skipCount, int getCount)
        {
            Monitor.Enter(counter.SqlTool.Lock);
            try
            {
                TmphList<TValueType> values = queueCache.Get(key, null);
                if (values != null) return values.rangeSort(comparer, skipCount, getCount).GetArray();
                values = getList(key);
                if (values != null) return values.rangeSort(comparer, skipCount, getCount).GetArray();
            }
            finally { Monitor.Exit(counter.SqlTool.Lock); }
            return TmphNullValue<TValueType>.Array;
        }
        /// <summary>
        /// 获取缓存数据集合
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="getValue">数据获取器,禁止锁操作</param>
        /// <returns>数据集合</returns>
        public TmphArrayType[] GetArrayLock<TmphArrayType>(TKeyType key, Func<TValueType, TmphArrayType> getValue)
        {
            Monitor.Enter(counter.SqlTool.Lock);
            try
            {
                TmphList<TValueType> values = queueCache.Get(key, null);
                if (values != null) return values.GetArray(getValue);
                values = getList(key);
                if (values != null) return values.GetArray(getValue);
            }
            finally { Monitor.Exit(counter.SqlTool.Lock); }
            return TmphNullValue<TmphArrayType>.Array;
        }
        /// <summary>
        /// 获取缓存数据数量
        /// </summary>
        /// <param name="key">关键字</param>
        /// <returns>缓存数据数量</returns>
        public int GetCount(TKeyType key)
        {
            TmphList<TValueType> values = queueCache.Get(key, null);
            if (values != null) return values.Count;
            Monitor.Enter(counter.SqlTool.Lock);
            try
            {
                values = getList(key);
            }
            finally { Monitor.Exit(counter.SqlTool.Lock); }
            return values.Count();
        }

        /// <summary>
        /// 获取缓存数据数量
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="isValue">数据匹配器,禁止锁操作</param>
        /// <returns>缓存数据数量</returns>
        public int GetCountLock(TKeyType key, Func<TValueType, bool> isValue)
        {
            Monitor.Enter(counter.SqlTool.Lock);
            try
            {
                return (queueCache.Get(key, null) ?? getList(key)).GetCount(isValue);
            }
            finally { Monitor.Exit(counter.SqlTool.Lock); }
        }
    }
}
