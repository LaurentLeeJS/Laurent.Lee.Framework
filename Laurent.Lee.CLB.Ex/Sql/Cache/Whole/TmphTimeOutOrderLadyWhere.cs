using System;
using Laurent.Lee.CLB.Code.CSharp;
using System.Threading;
using System.Collections.Generic;

namespace Laurent.Lee.CLB.Sql.Cache.Whole
{
    /// <summary>
    /// 超时缓存
    /// </summary>
    /// <typeparam name="TValueType">表格绑定类型</typeparam>
    /// <typeparam name="TModelType">表格模型类型</typeparam>
    public class TmphTimeOutOrderLadyWhere<TValueType, TModelType> : TmphTimeOutOrderLady<TValueType, TModelType>
        where TValueType : class, TModelType
        where TModelType : class
    {
        /// <summary>
        /// 数据匹配器
        /// </summary>
        private Func<TValueType, bool> isValue;
        /// <summary>
        /// 超时缓存
        /// </summary>
        /// <param name="cache">整表缓存</param>
        /// <param name="getTime">时间获取器</param>
        /// <param name="timeOutSeconds">超时秒数</param>
        /// <param name="getKey">关键字获取器</param>
        /// <param name="isValue">数据匹配器</param>
        public TmphTimeOutOrderLadyWhere(Events.TmphCache<TValueType, TModelType> cache
            , double timeOutSeconds, Func<TValueType, DateTime> getTime, Func<TValueType, bool> isValue)
            : base(cache, timeOutSeconds, getTime, false)
        {
            if (isValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            this.isValue = isValue;

            cache.OnReset += reset;
            cache.OnInserted += onInserted;
            cache.OnUpdated += onUpdated;
            cache.OnDeleted += onDeleted;
            resetLock();
        }
        /// <summary>
        /// 重新加载数据
        /// </summary>
        protected override void reset()
        {
            HashSet<TValueType> newValues = TmphHashSet<TValueType>.Create();
            DateTime minTime = this.outTime;
            foreach (TValueType value in cache.Values)
            {
                if (getTime(value) > minTime && isValue(value)) newValues.Add(value);
            }
            values = newValues;
            array = null;
        }
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="value">数据对象</param>
        private new void onInserted(TValueType value)
        {
            if (isValue(value)) base.onInserted(value);
        }
        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="value">更新后的数据</param>
        /// <param name="oldValue">更新前的数据</param>
        private new void onUpdated(TValueType value, TValueType oldValue)
        {
            onInserted(value);
        }
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="value">被删除的数据</param>
        private new void onDeleted(TValueType value)
        {
            if (isValue(value)) base.onDeleted(value);
        }
    }
}
