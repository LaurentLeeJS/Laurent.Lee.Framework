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

using Laurent.Lee.CLB.Config;
using System;
using System.Reflection;
using System.Threading;

namespace Laurent.Lee.CLB.Threading
{
    /// <summary>
    ///     任务处理基类
    /// </summary>
    public abstract class TmphTaskBase : IDisposable
    {
        /// <summary>
        ///     等待空闲事件
        /// </summary>
        protected readonly EventWaitHandle freeWaitHandle = new EventWaitHandle(true, EventResetMode.ManualReset, null);

        /// <summary>
        ///     默认释放资源是否等待线程结束
        /// </summary>
        protected bool isDisposeWait;

        /// <summary>
        ///     是否停止任务
        /// </summary>
        protected byte isStop;

        /// <summary>
        ///     新任务集合
        /// </summary>
        internal TmphList<TmphTaskInfo> PushTasks = new TmphList<TmphTaskInfo>();

        /// <summary>
        ///     任务访问锁
        /// </summary>
        protected int taskLock;

        /// <summary>
        ///     线程数量
        /// </summary>
        protected int threadCount;

        /// <summary>
        ///     线程池
        /// </summary>
        protected TmphThreadPool threadPool;

        /// <summary>
        ///     释放资源
        /// </summary>
        public void Dispose()
        {
            TmphDomainUnload.Remove(Dispose, false);
            Dispose(isDisposeWait);
        }

        /// <summary>
        ///     释放资源
        /// </summary>
        /// <param name="isWait">是否等待线程结束</param>
        public void Dispose(bool isWait)
        {
            TmphInterlocked.NoCheckCompareSetSleep0(ref taskLock);
            var threadCount = this.threadCount | PushTasks.Count;
            isStop = 1;
            taskLock = 0;
            if (isWait && threadCount != 0)
            {
                freeWaitHandle.WaitOne();
                freeWaitHandle.Close();
            }
        }

        /// <summary>
        ///     单线程添加任务后，等待所有线程空闲
        /// </summary>
        public void WaitFree()
        {
            TmphInterlocked.NoCheckCompareSetSleep0(ref taskLock);
            var threadCount = this.threadCount | PushTasks.Count;
            taskLock = 0;
            if (threadCount != 0) freeWaitHandle.WaitOne();
        }
    }

    /// <summary>
    ///     任务处理类(适用于短小任务，因为处理阻塞)
    /// </summary>
    public sealed class TmphTask : TmphTaskBase
    {
        /// <summary>
        ///     微型线程任务
        /// </summary>
        public static readonly TmphTask Tiny = new TmphTask(Config.TmphPub.Default.TinyThreadCount);

        /// <summary>
        ///     默认任务
        /// </summary>
        public static readonly TmphTask Default = new TmphTask(Config.TmphPub.Default.TaskThreadCount, true,
            TmphThreadPool.Default);

        /// <summary>
        ///     执行任务
        /// </summary>
        private readonly Action<TmphTaskInfo> runHandle;

        static TmphTask()
        {
            if (TmphAppSetting.IsCheckMemory) TmphCheckMemory.Add(MethodBase.GetCurrentMethod().DeclaringType);
        }

        /// <summary>
        ///     任务处理
        /// </summary>
        /// <param name="count">线程数</param>
        /// <param name="isDisposeWait">默认释放资源是否等待线程结束</param>
        /// <param name="threadPool">线程池</param>
        public TmphTask(int count, bool isDisposeWait = true, TmphThreadPool threadPool = null)
        {
            if (count <= 0 || count > Config.TmphPub.Default.TaskMaxThreadCount)
                TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
            MaxThreadCount = count;
            runHandle = run;
            this.isDisposeWait = isDisposeWait;
            this.threadPool = threadPool ?? TmphThreadPool.TinyPool;
            TmphDomainUnload.Add(Dispose, false);
        }

        /// <summary>
        ///     最大线程数量
        /// </summary>
        public int MaxThreadCount { get; private set; }

        /// <summary>
        ///     添加任务
        /// </summary>
        /// <param name="task">任务信息</param>
        /// <returns>任务添加是否成功</returns>
        internal bool Add(TmphTaskInfo task)
        {
            TmphInterlocked.NoCheckCompareSetSleep0(ref taskLock);
            if (isStop == 0)
            {
                if (threadCount == MaxThreadCount)
                {
                    try
                    {
                        PushTasks.Add(task);
                    }
                    finally
                    {
                        taskLock = 0;
                    }
                    return true;
                }
                if (threadCount == 0) freeWaitHandle.Reset();
                ++threadCount;
                taskLock = 0;
                try
                {
                    threadPool.FastStart(runHandle, task, null, null);
                    return true;
                }
                catch (Exception error)
                {
                    TmphInterlocked.NoCheckCompareSetSleep0(ref taskLock);
                    var count = --threadCount | PushTasks.Count;
                    taskLock = 0;
                    if (count == 0) freeWaitHandle.Set();
                    TmphLog.Error.Add(error, null, false);
                }
            }
            else taskLock = 0;
            return false;
        }

        /// <summary>
        ///     添加任务
        /// </summary>
        /// <param name="run">任务执行委托</param>
        /// <param name="onError">任务执行出错委托,停止任务参数null</param>
        /// <returns>任务添加是否成功</returns>
        public bool Add(Action run, Action<Exception> onError = null)
        {
            return run != null && Add(new TmphTaskInfo { Call = run, OnError = onError });
        }

        /// <summary>
        ///     添加任务
        /// </summary>
        /// <typeparam name="TParameterType">执行参数类型</typeparam>
        /// <param name="run">任务执行委托</param>
        /// <param name="parameter">执行参数</param>
        /// <param name="onError">任务执行出错委托,停止任务参数null</param>
        /// <returns>任务添加是否成功</returns>
        public bool Add<TParameterType>(Action<TParameterType> run, TParameterType parameter,
            Action<Exception> onError = null)
        {
            return run != null &&
                   Add(new TmphTaskInfo { Call = TmphRun<TParameterType>.Create(run, parameter), OnError = onError });
        }

        /// <summary>
        ///     执行任务
        /// </summary>
        /// <param name="task">任务信息</param>
        private void run(TmphTaskInfo task)
        {
            task.Run();
            do
            {
                TmphInterlocked.NoCheckCompareSetSleep0(ref taskLock);
                if (PushTasks.Count == 0)
                {
                    var threadCount = --this.threadCount;
                    taskLock = 0;
                    if (threadCount == 0)
                    {
                        freeWaitHandle.Set();
                        if (isStop != 0) freeWaitHandle.Close();
                    }
                    break;
                }
                task = PushTasks.Unsafer.PopReset();
                taskLock = 0;
                task.Run();
            } while (true);
        }
    }
}