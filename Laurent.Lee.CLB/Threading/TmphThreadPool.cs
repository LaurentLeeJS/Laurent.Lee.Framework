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

using Laurent.Lee.CLB.Config;
using System;
using System.Reflection;

namespace Laurent.Lee.CLB.Threading
{
    /// <summary>
    ///     线程池
    /// </summary>
    public sealed class TmphThreadPool
    {
        /// <summary>
        ///     最低线程堆栈大小
        /// </summary>
        private const int minStackSize = 128 << 10;

        /// <summary>
        ///     微型线程池,堆栈大小可能只有128K
        /// </summary>
        public static readonly TmphThreadPool TinyPool = new TmphThreadPool(TmphAppSetting.TinyThreadStackSize);

        /// <summary>
        ///     默认线程池
        /// </summary>
        public static readonly TmphThreadPool Default = TmphAppSetting.ThreadStackSize != TmphAppSetting.TinyThreadStackSize
            ? new TmphThreadPool(TmphAppSetting.ThreadStackSize)
            : TinyPool;

        /// <summary>
        ///     线程堆栈大小
        /// </summary>
        private readonly int stackSize;

        /// <summary>
        ///     是否已经释放资源
        /// </summary>
        private bool isDisposed;

        /// <summary>
        ///     线程集合
        /// </summary>
        private TmphObjectPool<TmphThread> threads = TmphObjectPool<TmphThread>.Create();

        static TmphThreadPool()
        {
            if (TmphAppSetting.IsCheckMemory) TmphCheckMemory.Add(MethodBase.GetCurrentMethod().DeclaringType);
        }

        /// <summary>
        ///     线程池
        /// </summary>
        /// <param name="stackSize">线程堆栈大小</param>
        private TmphThreadPool(int stackSize = 1 << 20)
        {
            this.stackSize = stackSize < minStackSize ? minStackSize : stackSize;
            TmphDomainUnload.Add(dispose);
        }

        /// <summary>
        ///     释放资源
        /// </summary>
        private void dispose()
        {
            isDisposed = true;
            disposePool();
        }

        /// <summary>
        ///     释放线程池
        /// </summary>
        private void disposePool()
        {
            foreach (var value in threads.GetClear(0)) value.Value.Stop();
        }

        /// <summary>
        ///     获取一个线程并执行任务
        /// </summary>
        /// <param name="task">任务委托</param>
        /// <param name="domainUnload">应用程序退出处理</param>
        /// <param name="onError">应用程序退出处理</param>
        private void start(Action task, Action domainUnload, Action<Exception> onError)
        {
            if (task == null) TmphLog.Error.Throw(null, "缺少 线程委托", false);
            if (isDisposed) TmphLog.Default.Real("线程池已经被释放", true, false);
            else
            {
                var thread = threads.Pop();
                if (thread == null) new TmphThread(this, stackSize, task, domainUnload, onError);
                else thread.RunTask(task, domainUnload, onError);
            }
        }

        /// <summary>
        ///     获取一个线程并执行任务
        /// </summary>
        /// <param name="task">任务委托</param>
        /// <param name="domainUnload">应用程序退出处理</param>
        /// <param name="onError">应用程序退出处理</param>
        internal void FastStart(Action task, Action domainUnload, Action<Exception> onError)
        {
            var thread = threads.Pop();
            if (thread == null) new TmphThread(this, stackSize, task, domainUnload, onError);
            else thread.RunTask(task, domainUnload, onError);
        }

        /// <summary>
        ///     获取一个线程并执行任务
        /// </summary>
        /// <typeparam name="TParameterType">参数类型</typeparam>
        /// <param name="task">任务委托</param>
        /// <param name="parameter">线程参数</param>
        /// <param name="domainUnload">应用程序退出处理</param>
        /// <param name="onError">应用程序退出处理</param>
        internal void FastStart<TParameterType>(Action<TParameterType> task, TParameterType parameter, Action domainUnload,
            Action<Exception> onError)
        {
            FastStart(TmphRun<TParameterType>.Create(task, parameter), domainUnload, onError);
        }

        /// <summary>
        ///     获取一个线程并执行任务
        /// </summary>
        /// <param name="task">任务委托</param>
        /// <param name="domainUnload">应用程序退出处理</param>
        /// <param name="onError">应用程序退出处理</param>
        public void Start(Action task, Action domainUnload = null, Action<Exception> onError = null)
        {
            if (task == null) TmphLog.Error.Throw(null, "缺少 线程委托", false);
            start(task, domainUnload, onError);
        }

        /// <summary>
        ///     获取一个线程并执行任务
        /// </summary>
        /// <typeparam name="TParameterType">参数类型</typeparam>
        /// <param name="task">任务委托</param>
        /// <param name="parameter">线程参数</param>
        /// <param name="domainUnload">应用程序退出处理</param>
        /// <param name="onError">应用程序退出处理</param>
        public void Start<TParameterType>
            (Action<TParameterType> task, TParameterType parameter, Action domainUnload = null,
                Action<Exception> onError = null)
        {
            if (task == null) TmphLog.Error.Throw(null, "缺少 线程委托", false);
            start(TmphRun<TParameterType>.Create(task, parameter), domainUnload, onError);
        }

        /// <summary>
        ///     获取一个线程并执行任务
        /// </summary>
        /// <typeparam name="TParameterType">参数类型</typeparam>
        /// <typeparam name="TReturnType">返回值类型</typeparam>
        /// <param name="task">任务委托</param>
        /// <param name="parameter">线程参数</param>
        /// <param name="onReturn">返回值执行委托</param>
        /// <param name="domainUnload">应用程序退出处理</param>
        /// <param name="onError">应用程序退出处理</param>
        public void Start<TParameterType, TReturnType>(Func<TParameterType, TReturnType> task, TParameterType parameter,
            Action<TReturnType> onReturn, Action domainUnload = null, Action<Exception> onError = null)
        {
            if (task == null) TmphLog.Error.Throw(null, "缺少 线程委托", false);
            start(TmphRun<TParameterType, TReturnType>.Create(task, parameter, onReturn), domainUnload, onError);
        }

        /// <summary>
        ///     线程入池
        /// </summary>
        /// <param name="thread">线程池线程</param>
        internal void Push(TmphThread thread)
        {
            if (isDisposed) thread.Stop();
            else
            {
                threads.Push(thread);
                if (isDisposed) disposePool();
            }
        }

        /// <summary>
        ///     检测日志输出
        /// </summary>
        public static void CheckLog()
        {
            var threads = TmphThread.StackTraces;
            TmphLog.Default.Add("活动线程数量 " + threads.Count.toString(), false, false);
            foreach (var value in threads)
            {
                try
                {
                    TmphLog.Default.Add(value.ToString(), false, false);
                }
                catch
                {
                }
            }
        }
    }
}