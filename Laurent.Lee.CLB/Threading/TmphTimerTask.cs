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
using System.Timers;

namespace Laurent.Lee.CLB.Threading
{
    /// <summary>
    ///     定时任务信息
    /// </summary>
    public sealed class TmphTimerTask : IDisposable
    {
        /// <summary>
        ///     默认定时任务
        /// </summary>
        public static readonly TmphTimerTask Default = new TmphTimerTask(null);

        /// <summary>
        ///     任务处理线程池
        /// </summary>
        private readonly TmphThreadPool threadPool;

        /// <summary>
        ///     线程池任务
        /// </summary>
        private readonly Action threadTaskHandle;

        /// <summary>
        ///     最近时间
        /// </summary>
        private DateTime nearTime = DateTime.MaxValue;

        /// <summary>
        ///     已排序任务
        /// </summary>
        private TmphArrayHeap<DateTime, TmphTaskInfo> taskHeap = new TmphArrayHeap<DateTime, TmphTaskInfo>();

        /// <summary>
        ///     任务访问锁
        /// </summary>
        private int taskLock;

        /// <summary>
        ///     定时器
        /// </summary>
        private Timer timer = new Timer();

        static TmphTimerTask()
        {
            if (TmphAppSetting.IsCheckMemory) TmphCheckMemory.Add(MethodBase.GetCurrentMethod().DeclaringType);
        }

        /// <summary>
        ///     定时任务信息
        /// </summary>
        /// <param name="task">任务处理线程池</param>
        public TmphTimerTask(TmphThreadPool threadPool)
        {
            this.threadPool = threadPool ?? TmphThreadPool.TinyPool;
            timer.Elapsed += onTimer;
            timer.AutoReset = false;
            threadTaskHandle = threadTask;
        }

        /// <summary>
        ///     释放资源
        /// </summary>
        public void Dispose()
        {
            if (timer != null)
            {
                timer.Stop();
                timer.Dispose();
                timer = null;
            }
            taskHeap = null;
        }

        /// <summary>
        ///     添加新任务
        /// </summary>
        /// <param name="task">任务信息</param>
        /// <param name="runTime">执行时间</param>
        private void add(TmphTaskInfo task, DateTime runTime)
        {
            var isThread = false;
            TmphInterlocked.NoCheckCompareSetSleep0(ref taskLock);
            try
            {
                taskHeap.Push(runTime, task);
                if (runTime < nearTime)
                {
                    timer.Stop();
                    nearTime = runTime;
                    var time = (runTime - TmphDate.Now).TotalMilliseconds;
                    if (time > 0)
                    {
                        timer.Interval = time;
                        timer.Start();
                    }
                    else isThread = true;
                }
            }
            finally
            {
                taskLock = 0;
            }
            if (isThread) threadPool.FastStart(threadTaskHandle, null, null);
        }

        /// <summary>
        ///     添加任务
        /// </summary>
        /// <param name="run">任务执行委托</param>
        /// <param name="runTime">执行时间</param>
        /// <param name="onError">任务执行出错委托,停止任务参数null</param>
        public void Add(Action run, DateTime runTime, Action<Exception> onError = null)
        {
            if (run != null)
            {
                if (runTime > TmphDate.Now) add(new TmphTaskInfo { Run = run, OnError = onError }, runTime);
                else threadPool.FastStart(run, null, onError);
            }
        }

        /// <summary>
        ///     添加任务
        /// </summary>
        /// <typeparam name="TParameterType">执行参数类型</typeparam>
        /// <param name="run">任务执行委托</param>
        /// <param name="parameter">执行参数</param>
        /// <param name="runTime">执行时间</param>
        /// <param name="onError">任务执行出错委托,停止任务参数null</param>
        public void Add<TParameterType>
            (Action<TParameterType> run, TParameterType parameter, DateTime runTime, Action<Exception> onError)
        {
            if (run != null)
            {
                if (runTime > TmphDate.Now)
                    add(new TmphTaskInfo { Run = TmphRun<TParameterType>.Create(run, parameter), OnError = onError }, runTime);
                else threadPool.FastStart(run, parameter, null, onError);
            }
        }

        /// <summary>
        ///     线程池任务
        /// </summary>
        private void threadTask()
        {
            var now = TmphDate.Now;
            TmphInterlocked.NoCheckCompareSetSleep0(ref taskLock);
            try
            {
                while (taskHeap.Count != 0)
                {
                    var task = taskHeap.UnsafeTop();
                    if (task.Key <= now)
                    {
                        threadPool.FastStart(task.Value.Run, null, task.Value.OnError);
                        taskHeap.RemoveTop();
                    }
                    else
                    {
                        nearTime = task.Key;
                        timer.Interval = Math.Max((task.Key - now).TotalMilliseconds, 1);
                        timer.Start();
                        break;
                    }
                }
                if (taskHeap.Count == 0) nearTime = DateTime.MaxValue;
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, false);
                timer.Interval = 1;
                timer.Start();
            }
            finally
            {
                taskLock = 0;
            }
        }

        /// <summary>
        ///     触发定时任务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onTimer(object sender, ElapsedEventArgs e)
        {
            threadTask();
        }

        /// <summary>
        ///     任务信息
        /// </summary>
        private struct TmphTaskInfo
        {
            /// <summary>
            ///     错误处理
            /// </summary>
            public Action<Exception> OnError;

            /// <summary>
            ///     任务委托
            /// </summary>
            public Action Run;
        }
    }
}