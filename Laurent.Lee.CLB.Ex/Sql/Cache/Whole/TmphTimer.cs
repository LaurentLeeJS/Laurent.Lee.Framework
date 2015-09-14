using System;
using System.Threading;
using Laurent.Lee.CLB.Threading;

namespace Laurent.Lee.CLB.Sql.Cache.Whole
{
    /// <summary>
    /// 缓存时间事件
    /// </summary>
    /// <typeparam name="TValueType"></typeparam>
    /// <typeparam name="TModelType"></typeparam>
    public class TmphTimer<TValueType, TModelType>
        where TValueType : class, TModelType
        where TModelType : class
    {
        /// <summary>
        /// 整表缓存
        /// </summary>
        protected Events.TmphCache<TValueType, TModelType> cache;
        /// <summary>
        /// 时间获取器
        /// </summary>
        protected Func<TValueType, DateTime> getTime;
        /// <summary>
        /// 事件委托
        /// </summary>
        private Action runTimeHandle;
        /// <summary>
        /// 事件委托
        /// </summary>
        private Action run;
        /// <summary>
        /// 最小事件时间
        /// </summary>
        private DateTime minTime;
        /// <summary>
        /// 事件时间集合
        /// </summary>
        private TmphSubArray<DateTime> times;
        /// <summary>
        /// 事件时间访问锁
        /// </summary>
        private int timeLock;
        /// <summary>
        /// 缓存时间事件
        /// </summary>
        /// <param name="cache">整表缓存</param>
        /// <param name="getTime">时间获取器</param>
        /// <param name="run">事件委托</param>
        /// <param name="isReset">是否绑定事件与重置数据</param>
        public TmphTimer(Events.TmphCache<TValueType, TModelType> cache, Func<TValueType, DateTime> getTime, Action run, bool isReset)
        {
            if (cache == null || getTime == null || run == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            runTimeHandle = runTime;
            this.cache = cache;
            this.getTime = getTime;
            this.run = run;
            minTime = DateTime.MaxValue;

            if (isReset)
            {
                cache.OnReset += reset;
                cache.OnInserted += onInserted;
                cache.OnUpdated += onUpdated;
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
            DateTime minTime = DateTime.MaxValue;
            foreach (TValueType value in cache.Values)
            {
                DateTime time = getTime(value);
                if (time < minTime && time > Laurent.Lee.CLB.TmphPub.MinTime) minTime = time;
            }
            Append(minTime);
        }
        /// <summary>
        /// 添加事件时间
        /// </summary>
        /// <param name="time"></param>
        public void Append(DateTime time)
        {
            if (time < minTime && time > Laurent.Lee.CLB.TmphPub.MinTime)
            {
                TmphInterlocked.NoCheckCompareSetSleep0(ref timeLock);
                if (time < minTime)
                {
                    try
                    {
                        times.Add(time);
                        minTime = time;
                    }
                    finally { timeLock = 0; }
                    if (time <= TmphDate.NowSecond) Laurent.Lee.CLB.Threading.TmphThreadPool.TinyPool.Start(runTimeHandle);
                    else Laurent.Lee.CLB.Threading.TmphTimerTask.Default.Add(runTimeHandle, time);
                }
                else timeLock = 0;
            }
        }
        /// <summary>
        /// 时间事件
        /// </summary>
        private unsafe void runTime()
        {
            DateTime now = TmphDate.Now;
            TmphInterlocked.NoCheckCompareSetSleep0(ref timeLock);
            if (times.Count != 0)
            {
                fixed (DateTime* timeFixed = times.Array)
                {
                    if (*timeFixed <= now)
                    {
                        times.Empty();
                        minTime = DateTime.MaxValue;
                    }
                    else
                    {
                        DateTime* end = timeFixed + times.Count;
                        while (*--end <= now) ;
                        minTime = *end;
                        times.UnsafeSetLength((int)(end - timeFixed) + 1);
                    }
                }
            }
            timeLock = 0;
            run();
        }
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="value">数据对象</param>
        protected void onInserted(TValueType value)
        {
            Append(getTime(value));
        }
        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="value">更新后的数据</param>
        /// <param name="oldValue">更新前的数据</param>
        protected void onUpdated(TValueType value, TValueType oldValue)
        {
            Append(getTime(value));
        }
    }
}
