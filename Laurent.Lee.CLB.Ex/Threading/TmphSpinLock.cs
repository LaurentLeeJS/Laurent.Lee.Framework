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
    /// 1读1写自旋锁(必须手动初始化)
    /// </summary>
    public struct TmphSpinLock
    {
        /// <summary>
        /// 左旋数
        /// </summary>
        private volatile int left;

        /// <summary>
        /// 右旋数
        /// </summary>
        private volatile int right;

        /// <summary>
        /// 进入左旋锁
        /// </summary>
        public void EnterLeft()
        {
            for (int sleep = 0; true; sleep ^= 1)
            {
                if (++left == 0)
                {
                    if (++right == 0) break;
                    left = -1;
                }
                Thread.Sleep(sleep);
            }
        }

        /// <summary>
        /// 进入右旋锁
        /// </summary>
        public void EnterRight()
        {
            for (int sleep = 0; true; sleep ^= 1)
            {
                if (++right == 0)
                {
                    if (++left == 0) break;
                    right = -1;
                }
                Thread.Sleep(sleep);
            }
        }

        /// <summary>
        /// 锁复位
        /// </summary>
        public void Exit()
        {
            left = right = -1;
        }
    }
}