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
    /// 读写锁(写优先)，用于高并发的IO读取操作
    /// </summary>
    public sealed class TmphReadWriteLock
    {
        /// <summary>
        /// 锁状态访问锁
        /// </summary>
        private int stateLock;

        /// <summary>
        /// 写操作等待数量(等待读取完毕)
        /// </summary>
        private int waitWriteCount;

        /// <summary>
        /// 当前写操作数量
        /// </summary>
        private int writeCount;

        /// <summary>
        /// 读操作等待数量(等待写入完毕)
        /// </summary>
        private int waitReadCount;

        /// <summary>
        /// 当前读操作数量
        /// </summary>
        private int readCount;

        /// <summary>
        /// 等待读操作锁
        /// </summary>
        private readonly object waitReadLock = new object();

        /// <summary>
        /// 等待写操作锁
        /// </summary>
        private readonly object waitWriteLock = new object();

        /// <summary>
        /// 写操作锁
        /// </summary>
        private readonly object writeLock = new object();

        /// <summary>
        /// 是否正在唤醒 读操作等待
        /// </summary>
        private bool isPulseRead;

        /// <summary>
        /// 是否正在唤醒 写操作等待
        /// </summary>
        private bool isPulseWrite;

        /// <summary>
        /// 添加一个读取状态
        /// </summary>
        public void EnterRead()
        {
            int isWait = 1;
            while (true)
            {
                TmphInterlocked.CompareSetSleep(ref stateLock);
                if (writeCount == 0)
                {
                    waitReadCount -= isWait ^ 1;
                    ++readCount;
                    stateLock = 0;
                    break;
                }
                waitReadCount += isWait;
                stateLock = 0;
                isWait = 0;
                Monitor.Enter(waitReadLock);
                try
                {
                    Monitor.Wait(waitReadLock);
                }
                finally { Monitor.Exit(waitReadLock); }
            }
        }

        /// <summary>
        /// 结束一个读取状态
        /// </summary>
        public void ExitRead()
        {
            TmphInterlocked.CompareSetSleep(ref stateLock);
            if (--readCount == 0 && waitWriteCount != 0 && !isPulseWrite)
            {
                isPulseWrite = true;
                stateLock = 0;
                do
                {
                    Monitor.Enter(waitWriteLock);
                    try
                    {
                        Monitor.PulseAll(waitWriteLock);
                    }
                    finally { Monitor.Exit(waitWriteLock); }
                    Thread.Sleep(1);
                    TmphInterlocked.CompareSetSleep(ref stateLock);
                    if (readCount != 0 || waitWriteCount == 0)
                    {
                        isPulseWrite = false;
                        stateLock = 0;
                        break;
                    }
                    stateLock = 0;
                }
                while (true);
            }
            else stateLock = 0;
        }

        /// <summary>
        /// 添加一个写入状态
        /// </summary>
        public void EnterWrite()
        {
            TmphInterlocked.CompareSetSleep(ref stateLock);
            ++writeCount;
            if (readCount == 0) stateLock = 0;
            else
            {
                ++waitWriteCount;
                stateLock = 0;
                Monitor.Enter(waitWriteLock);
                try
                {
                    Monitor.Wait(waitWriteLock);
                }
                finally { Monitor.Exit(waitWriteLock); }
                TmphInterlocked.CompareSetSleep(ref stateLock);
                --waitWriteCount;
                stateLock = 0;
            }
            Monitor.Enter(writeLock);
        }

        /// <summary>
        /// 结束一个写入状态
        /// </summary>
        public void ExitWrite()
        {
            Monitor.Exit(writeLock);
            TmphInterlocked.CompareSetSleep(ref stateLock);
            if (--writeCount == 0 && waitReadCount != 0 && !isPulseRead)
            {
                isPulseRead = true;
                stateLock = 0;
                do
                {
                    Monitor.Enter(waitReadLock);
                    try
                    {
                        Monitor.PulseAll(waitReadLock);
                    }
                    finally { Monitor.Exit(waitReadLock); }
                    Thread.Sleep(1);
                    TmphInterlocked.CompareSetSleep(ref stateLock);
                    if (writeCount != 0 || waitReadCount == 0)
                    {
                        isPulseRead = false;
                        stateLock = 0;
                        break;
                    }
                    stateLock = 0;
                }
                while (true);
            }
            else stateLock = 0;
        }
    }
}