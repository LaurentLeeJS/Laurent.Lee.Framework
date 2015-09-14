using System;
using Laurent.Lee.CLB.Threading;

namespace Laurent.Lee.CLB.Sql
{
    /// <summary>
    /// 当前时间
    /// </summary>
    public struct TmphNowTime
    {
        /// <summary>
        /// 增加
        /// </summary>
        private static readonly long ticks = 4 * TmphDate.MillisecondTicks;
        /// <summary>
        /// 下一次最小时间
        /// </summary>
        private DateTime minTime;
        /// <summary>
        /// 时间访问锁
        /// </summary>
        private int timeLock;
        /// <summary>
        /// 获取下一个时间
        /// </summary>
        public DateTime Next
        {
            get
            {
                DateTime now = TmphDate.NowSecond;
                TmphInterlocked.NoCheckCompareSetSleep0(ref timeLock);
                if (now < minTime) now = minTime;
                minTime = now.AddTicks(ticks);
                timeLock = 0;
                return now;
            }
        }
        /// <summary>
        /// 设置时间
        /// </summary>
        /// <param name="time"></param>
        public void Set(DateTime time)
        {
            TmphInterlocked.NoCheckCompareSetSleep0(ref timeLock);
            minTime = time.AddTicks(ticks);
            timeLock = 0;
        }
    }
}
