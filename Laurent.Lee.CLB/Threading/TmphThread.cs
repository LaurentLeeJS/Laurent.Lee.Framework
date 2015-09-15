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
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using ThreadState = System.Threading.ThreadState;

namespace Laurent.Lee.CLB.Threading
{
    /// <summary>
    ///     线程池线程
    /// </summary>
    internal sealed class TmphThread
    {
        /// <summary>
        ///     线程池线程集合
        /// </summary>
        private static readonly HashSet<Thread> threads = TmphHashSet.CreateOnly<Thread>();

        /// <summary>
        ///     线程池线程集合访问锁
        /// </summary>
        private static int threadLock;

        /// <summary>
        ///     线程池线程默认堆栈帧数
        /// </summary>
        private static int defaultFrameCount;

        /// <summary>
        ///     线程句柄
        /// </summary>
        private readonly Thread threadHandle;

        /// <summary>
        ///     线程池
        /// </summary>
        private readonly TmphThreadPool threadPool;

        /// <summary>
        ///     等待事件
        /// </summary>
        private readonly EventWaitHandle waitHandle;

        /// <summary>
        ///     应用程序退出处理
        /// </summary>
        private Action domainUnload;

        /// <summary>
        ///     应用程序退出处理
        /// </summary>
        private Action<Exception> onError;

        /// <summary>
        ///     任务委托
        /// </summary>
        private Action task;

        /// <summary>
        ///     线程池线程
        /// </summary>
        /// <param name="threadPool">线程池</param>
        /// <param name="stackSize">堆栈大小</param>
        /// <param name="handle">任务委托</param>
        /// <param name="domainUnload">应用程序退出处理</param>
        /// <param name="onError">应用程序退出处理</param>
        internal TmphThread(TmphThreadPool threadPool, int stackSize, Action task, Action domainUnload,
            Action<Exception> onError)
        {
            this.task = task;
            this.domainUnload = domainUnload;
            this.onError = onError;
            waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, null);
            this.threadPool = threadPool;
            threadHandle = new Thread(run, stackSize);
            threadHandle.IsBackground = true;
            TmphInterlocked.NoCheckCompareSetSleep0(ref threadLock);
            try
            {
                threads.Add(threadHandle);
            }
            finally
            {
                threadLock = 0;
                threadHandle.Start();
            }
        }

        /// <summary>
        ///     活动的线程池线程集合
        /// </summary>
        public static TmphSubArray<StackTrace> StackTraces
        {
            get
            {
                Thread[] array;
                TmphInterlocked.NoCheckCompareSetSleep0(ref threadLock);
                try
                {
                    array = threads.GetArray();
                }
                finally
                {
                    threadLock = 0;
                }
                var stacks = new TmphSubArray<StackTrace>();
                var currentId = Thread.CurrentThread.ManagedThreadId;
                foreach (var thread in array)
                {
                    if (thread.ManagedThreadId != currentId)
                    {
                        var isSuspend = false;
                        try
                        {
                            if ((thread.ThreadState | ThreadState.Suspended) != 0)
                            {
#pragma warning disable 618
                                thread.Suspend();
#pragma warning restore 618
                                isSuspend = true;
                            }
                            var stack = new StackTrace(thread, true);
                            if (stack.FrameCount != defaultFrameCount) stacks.Add(stack);
                        }
                        catch (Exception error)
                        {
                            TmphLog.Default.Add(error, null, false);
                        }
                        finally
                        {
#pragma warning disable 618
                            if (isSuspend) thread.Resume();
#pragma warning restore 618
                        }
                    }
                }
                return stacks;
            }
        }

        /// <summary>
        ///     线程ID
        /// </summary>
        public int ManagedThreadId
        {
            get { return threadHandle.ManagedThreadId; }
        }

        /// <summary>
        ///     运行线程
        /// </summary>
        private void run()
        {
            if (defaultFrameCount == 0) defaultFrameCount = new StackTrace(threadHandle, false).FrameCount;
            do
            {
                if (domainUnload != null) TmphDomainUnload.Add(domainUnload);
                try
                {
                    task();
                }
                catch (Exception error)
                {
                    if (onError != null)
                    {
                        try
                        {
                            onError(error);
                        }
                        catch (Exception error1)
                        {
                            TmphLog.Error.Add(error1, null, false);
                        }
                    }
                    else TmphLog.Error.Add(error, null, false);
                }
                finally
                {
                    task = null;
                    onError = null;
                    if (domainUnload != null)
                    {
                        TmphDomainUnload.Remove(domainUnload, false);
                        domainUnload = null;
                    }
                }
                threadPool.Push(this);
                waitHandle.WaitOne();
            } while (task != null);
            TmphInterlocked.NoCheckCompareSetSleep0(ref threadLock);
            try
            {
                threads.Remove(threadHandle);
            }
            finally
            {
                threadLock = 0;
                waitHandle.Close();
            }
        }

        /// <summary>
        ///     结束线程
        /// </summary>
        internal void Stop()
        {
            waitHandle.Set();
        }

        /// <summary>
        ///     执行任务
        /// </summary>
        /// <param name="handle">任务委托</param>
        /// <param name="domainUnload">应用程序退出处理</param>
        /// <param name="onError">应用程序退出处理</param>
        internal void RunTask(Action task, Action domainUnload, Action<Exception> onError)
        {
            this.domainUnload = domainUnload;
            this.onError = onError;
            this.task = task;
            waitHandle.Set();
        }
    }
}