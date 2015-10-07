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

using System.Threading;

namespace Laurent.Lee.CLB.Threading
{
    /// <summary>
    ///     一次性等待锁
    /// </summary>
    internal struct TmphAutoWaitHandle
    {
        /// <summary>
        ///     同步等待
        /// </summary>
        private readonly EventWaitHandle waitHandle;

        /// <summary>
        ///     同步等待
        /// </summary>
        private int isWait;

        /// <summary>
        ///     一次性等待锁
        /// </summary>
        /// <param name="isSet">是否默认结束等待</param>
        public TmphAutoWaitHandle(bool isSet)
        {
            waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
            isWait = isSet ? 1 : 0;
        }

        /// <summary>
        ///     等待结束
        /// </summary>
        public void Wait()
        {
            //Thread.Sleep(0);
            if (Interlocked.CompareExchange(ref isWait, 1, 0) == 0) waitHandle.WaitOne();
            isWait = 0;
        }

        /// <summary>
        ///     结束等待
        /// </summary>
        public void Set()
        {
            if (Interlocked.CompareExchange(ref isWait, 1, 0) != 0) waitHandle.Set();
        }
    }
}