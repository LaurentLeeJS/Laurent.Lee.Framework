using System;
using System.Collections.Generic;
using Laurent.Lee.CLB.Code.CSharp;
using System.Threading;

namespace Laurent.Lee.CLB.Sql.Cache.Whole
{
    /// <summary>
    /// 超时缓存
    /// </summary>
    /// <typeparam name="TValueType">表格绑定类型</typeparam>
    /// <typeparam name="TModelType">表格模型类型</typeparam>
    public class TmphTimeOutOrderLady<TValueType, TModelType>
        where TValueType : class, TModelType
        where TModelType : class
    {
        /// <summary>
        /// 整表缓存
        /// </summary>
        protected Events.TmphCache<TValueType, TModelType> cache;
        /// <summary>
        /// 超时秒数
        /// </summary>
        protected double timeOutSeconds;
        /// <summary>
        /// 最小有效时间
        /// </summary>
        protected DateTime outTime
        {
            get
            {
                return TmphDate.NowSecond.AddSeconds(timeOutSeconds);
            }
        }
        /// <summary>
        /// 排序数据最小时间
        /// </summary>
        protected DateTime minTime;
        /// <summary>
        /// 时间获取器
        /// </summary>
        protected Func<TValueType, DateTime> getTime;
        /// <summary>
        /// 数据集合
        /// </summary>
        protected HashSet<TValueType> values;
        /// <summary>
        /// 数据数组
        /// </summary>
        protected TValueType[] array;
        /// <summary>
        /// 数据数组是否排序
        /// </summary>
        protected bool isSort;
        /// <summary>
        /// 超时缓存
        /// </summary>
        /// <param name="cache">整表缓存</param>
        /// <param name="getTime">时间获取器</param>
        /// <param name="timeOutSeconds">超时秒数</param>
        /// <param name="getKey">关键字获取器</param>
        /// <param name="isReset">是否绑定事件与重置数据</param>
        public TmphTimeOutOrderLady(Events.TmphCache<TValueType, TModelType> cache
            , double timeOutSeconds, Func<TValueType, DateTime> getTime, bool isReset)
        {
            if (cache == null || timeOutSeconds < 1 || getTime == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            this.cache = cache;
            this.timeOutSeconds = -timeOutSeconds;
            this.getTime = getTime;

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
            HashSet<TValueType> newValues = TmphHashSet<TValueType>.Create();
            DateTime minTime = this.outTime;
            foreach (TValueType value in cache.Values)
            {
                if (getTime(value) > minTime) newValues.Add(value);
            }
            values = newValues;
            array = null;
        }
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="value">数据对象</param>
        protected void onInserted(TValueType value)
        {
            if (getTime(value) > outTime)
            {
                values.Add(value);
                array = null;
            }
        }
        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="value">更新后的数据</param>
        /// <param name="oldValue">更新前的数据</param>
        protected void onUpdated(TValueType value, TValueType oldValue)
        {
            onInserted(value);
        }
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="value">被删除的数据</param>
        protected void onDeleted(TValueType value)
        {
            if (values.Remove(value)) array = null;
        }
        /// <summary>
        /// 删除过期数据
        /// </summary>
        private void remove()
        {
            DateTime outTime = this.outTime;
            TmphSubArray<TValueType> removeValues = this.values.GetFind(value => getTime(value) < outTime);
            int count = removeValues.Count;
            if (count != 0)
            {
                foreach (TValueType value in removeValues.Array)
                {
                    this.values.Remove(value);
                    if (--count == 0) break;
                }
            }
        }
        /// <summary>
        /// 获取数据数量
        /// </summary>
        /// <returns>数据数量</returns>
        public int GetCount()
        {
            return GetArray().Length;
        }
        /// <summary>
        /// 获取数据集合
        /// </summary>
        /// <returns>数据集合</returns>
        public TValueType[] GetArray()
        {
            TValueType[] values = array;
            if (values == null || minTime < TmphDate.NowSecond)
            {
                Monitor.Enter(cache.SqlTool.Lock);
                try
                {
                    if ((values = array) == null || minTime < TmphDate.NowSecond)
                    {
                        remove();
                        values = array = this.values.GetArray();
                        minTime = array.Length != 0 ? array.maxKey(getTime, DateTime.MinValue).AddSeconds(timeOutSeconds) : DateTime.MaxValue;
                        isSort = array.Length < 2;
                    }
                }
                finally { Monitor.Exit(cache.SqlTool.Lock); }
            }
            return values;
        }
        /// <summary>
        /// 获取排序后的数据集合
        /// </summary>
        /// <returns>排序后的数据集合</returns>
        public TValueType[] GetSort()
        {
            Monitor.Enter(cache.SqlTool.Lock);
            try
            {
                if (array == null || minTime < TmphDate.NowSecond)
                {
                    remove();
                    array = this.values.GetArray().getSortDesc(getTime);
                    minTime = array.Length != 0 ? getTime(array[array.Length - 1]).AddSeconds(timeOutSeconds) : DateTime.MaxValue;
                    isSort = true;
                }
                else if (!isSort) array = array.getSortDesc(getTime);
                return array;
            }
            finally { Monitor.Exit(cache.SqlTool.Lock); }
        }
    }
}
