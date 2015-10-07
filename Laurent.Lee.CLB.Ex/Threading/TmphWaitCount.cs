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

using System;
using System.Threading;

namespace Laurent.Lee.CLB.Threading
{
    /// <summary>
    /// 计数等待
    /// </summary>
    public sealed class TmphWaitCount : IDisposable
    {
        /// <summary>
        /// 当前计数
        /// </summary>
        private int count;

        /// <summary>
        /// 等待计数
        /// </summary>
        private int wait;

        /// <summary>
        /// 等待事件
        /// </summary>
        private readonly EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset, null);

        /// <summary>
        /// 计数等待
        /// </summary>
        /// <param name="count">当前计数</param>
        public TmphWaitCount(int count = 0)
        {
            this.count = count + 1;
            wait = 1;
        }

        /// <summary>
        /// 重置计数等待
        /// </summary>
        /// <param name="count">当前计数</param>
        public void Reset(int count)
        {
            waitHandle.Reset();
            this.count = count + 1;
            wait = 1;
        }

        /// <summary>
        /// 增加计数
        /// </summary>
        public void Increment()
        {
            Interlocked.Increment(ref count);
        }

        /// <summary>
        /// 减少计数
        /// </summary>
        public void Decrement()
        {
            if (Interlocked.Decrement(ref count) == 0) waitHandle.Set();
        }

        /// <summary>
        /// 等待计数完成
        /// </summary>
        /// <returns>当前未完成计数,0表示正常结束</returns>
        public int Wait()
        {
            if (Interlocked.Decrement(ref wait) == 0) Interlocked.Decrement(ref count);
            if (count != 0) waitHandle.WaitOne();
            return count;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            waitHandle.Set();
            waitHandle.Close();
        }
    }
}