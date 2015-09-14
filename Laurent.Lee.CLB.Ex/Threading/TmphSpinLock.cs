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