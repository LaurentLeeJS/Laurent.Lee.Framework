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

namespace Laurent.Lee.CLB.Threading
{
    /// <summary>
    ///     任务队列
    /// </summary>
    public sealed class TmphQueue : TmphTaskBase
    {
        /// <summary>
        ///     微型线程任务队列
        /// </summary>
        public static readonly TmphQueue Tiny = new TmphQueue();

        /// <summary>
        ///     默认任务队列
        /// </summary>
        public static readonly TmphQueue Default = new TmphQueue(true, TmphThreadPool.Default);

        /// <summary>
        ///     执行任务
        /// </summary>
        private readonly Action runHandle;

        /// <summary>
        ///     当前执行任务集合
        /// </summary>
        private TmphList<TmphTaskInfo> currentTasks = new TmphList<TmphTaskInfo>();

        static TmphQueue()
        {
            if (TmphAppSetting.IsCheckMemory) TmphCheckMemory.Add(MethodBase.GetCurrentMethod().DeclaringType);
        }

        /// <summary>
        ///     任务处理
        /// </summary>
        /// <param name="isDisposeWait">默认释放资源是否等待线程结束</param>
        /// <param name="threadPool">线程池</param>
        public TmphQueue(bool isDisposeWait = true, TmphThreadPool threadPool = null)
        {
            runHandle = run;
            this.isDisposeWait = isDisposeWait;
            this.threadPool = threadPool ?? TmphThreadPool.TinyPool;
            TmphDomainUnload.Add(Dispose, false);
        }

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
                var threadCount = this.threadCount;
                try
                {
                    PushTasks.Add(task);
                    if (this.threadCount == 0)
                    {
                        this.threadCount = 1;
                        freeWaitHandle.Reset();
                    }
                }
                finally
                {
                    taskLock = 0;
                }
                if (threadCount == 0)
                {
                    try
                    {
                        threadPool.FastStart(runHandle, null, null);
                        return true;
                    }
                    catch (Exception error)
                    {
                        TmphInterlocked.NoCheckCompareSetSleep0(ref taskLock);
                        this.threadCount = 0;
                        taskLock = 0;
                        TmphLog.Error.Add(error, null, false);
                    }
                }
                else return true;
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
        private void run()
        {
            do
            {
                TmphInterlocked.NoCheckCompareSetSleep0(ref taskLock);
                var taskCount = PushTasks.Count;
                if (taskCount == 0)
                {
                    threadCount = 0;
                    taskLock = 0;
                    freeWaitHandle.Set();
                    if (isStop != 0) freeWaitHandle.Close();
                    break;
                }
                var runTasks = PushTasks;
                PushTasks = currentTasks;
                currentTasks = runTasks;
                taskLock = 0;
                var taskArray = runTasks.array;
                var index = 0;
                do
                {
                    taskArray[index++].RunClear();
                } while (index != taskCount);
                runTasks.Empty();
            } while (true);
        }
    }
}