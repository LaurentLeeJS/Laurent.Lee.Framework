using System;

namespace Laurent.Lee.CLB.Sql.Cache.Whole
{
    /// <summary>
    /// 缓存时间事件
    /// </summary>
    /// <typeparam name="TValueType"></typeparam>
    /// <typeparam name="TModelType"></typeparam>
    public sealed class TmphTimerWhere<TValueType, TModelType> : TmphTimer<TValueType, TModelType>
        where TValueType : class, TModelType
        where TModelType : class
    {
        /// <summary>
        /// 数据匹配器
        /// </summary>
        private Func<TValueType, bool> isValue;
        /// <summary>
        /// 缓存时间事件
        /// </summary>
        /// <param name="cache">整表缓存</param>
        /// <param name="getTime">时间获取器</param>
        /// <param name="run">事件委托</param>
        /// <param name="isValue">数据匹配器</param>
        public TmphTimerWhere(Events.TmphCache<TValueType, TModelType> cache, Func<TValueType, DateTime> getTime, Action run, Func<TValueType, bool> isValue)
            : base(cache, getTime, run, false)
        {
            if (isValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            this.isValue = isValue;

            cache.OnReset += reset;
            cache.OnInserted += onInserted;
            cache.OnUpdated += onUpdated;
            resetLock();
        }
        /// <summary>
        /// 重新加载数据
        /// </summary>
        protected override void reset()
        {
            DateTime minTime = DateTime.MaxValue;
            foreach (TValueType value in cache.Values)
            {
                if (isValue(value))
                {
                    DateTime time = getTime(value);
                    if (time < minTime && time > Laurent.Lee.CLB.TmphPub.MinTime) minTime = time;
                }
            }
            Append(minTime);
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
            if (isValue(value)) base.onInserted(value);
        }
    }
}
