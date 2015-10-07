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

using Laurent.Lee.CLB.Threading;
using System;

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