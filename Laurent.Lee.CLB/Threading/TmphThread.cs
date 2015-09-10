/*
 *  Copyright 2015 Tony Lee

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

     http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
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