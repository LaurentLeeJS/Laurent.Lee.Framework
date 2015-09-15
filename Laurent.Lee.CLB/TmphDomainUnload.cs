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
using Laurent.Lee.CLB.Threading;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

#if MONO
#else

#endif

namespace Laurent.Lee.CLB
{
    /// <summary>
    ///     应用程序卸载处理
    /// </summary>
    public static class TmphDomainUnload
    {
        /// <summary>
        ///     卸载状态
        /// </summary>
        private enum TmphUnloadState
        {
            /// <summary>
            ///     正常运行状态
            /// </summary>
            Run,

            /// <summary>
            ///     卸载中，等待事务结束
            /// </summary>
            WaitTransaction,

            /// <summary>
            ///     卸载事件处理
            /// </summary>
            Event,

            /// <summary>
            ///     已经卸载
            /// </summary>
            Unloaded
        }

        /// <summary>
        ///     是否已关闭
        /// </summary>
        private static TmphUnloadState state = TmphUnloadState.Run;

        /// <summary>
        ///     卸载处理函数集合
        /// </summary>
        private static readonly HashSet<Action> unloaders = TmphHashSet.CreateOnly<Action>();

        /// <summary>
        ///     卸载处理函数集合
        /// </summary>
        private static readonly HashSet<Action> lastUnloaders = TmphHashSet.CreateOnly<Action>();

        /// <summary>
        ///     卸载处理函数访问锁
        /// </summary>
        private static readonly object unloaderLock = new object();

        /// <summary>
        ///     事务数量
        /// </summary>
        private static int transactionCount;

        ///// <summary>
        ///// 事务锁
        ///// </summary>
        //private readonly object transactionLock = new object();
        /// <summary>
        ///     添加应用程序卸载处理
        /// </summary>
        /// <param name="onUnload">卸载处理函数</param>
        /// <param name="isLog">添加失败是否输出日志</param>
        /// <returns>是否添加成功</returns>
        public static bool Add(Action onUnload, bool isLog = true)
        {
            var isAdd = false;
            Monitor.Enter(unloaderLock);
            try
            {
                if (state == TmphUnloadState.Run || state == TmphUnloadState.WaitTransaction)
                {
                    unloaders.Add(onUnload);
                    isAdd = true;
                }
            }
            finally
            {
                Monitor.Exit(unloaderLock);
            }
            if (!isAdd && isLog) TmphLog.Default.Real("应用程序正在退出", true, false);
            return isAdd;
        }

        /// <summary>
        ///     添加应用程序卸载处理
        /// </summary>
        /// <param name="onUnload">卸载处理函数</param>
        /// <returns>是否添加成功</returns>
        internal static bool AddLast(Action onUnload)
        {
            var isAdd = false;
            Monitor.Enter(unloaderLock);
            try
            {
                if (state == TmphUnloadState.Run || state == TmphUnloadState.WaitTransaction)
                {
                    lastUnloaders.Add(onUnload);
                    isAdd = true;
                }
            }
            finally
            {
                Monitor.Exit(unloaderLock);
            }
            if (!isAdd) TmphLog.Default.Real("应用程序正在退出", true, false);
            return isAdd;
        }

        /// <summary>
        ///     删除卸载处理函数
        /// </summary>
        /// <param name="onUnload">卸载处理函数</param>
        /// <param name="isRun">是否执行删除的函数</param>
        /// <returns>是否删除成功</returns>
        public static bool Remove(Action onUnload, bool isRun)
        {
            bool isRemove;
            Monitor.Enter(unloaderLock);
            try
            {
                isRemove = (state == TmphUnloadState.Run || state == TmphUnloadState.WaitTransaction)
                           && unloaders.Count != 0 && unloaders.Remove(onUnload);
            }
            finally
            {
                Monitor.Exit(unloaderLock);
            }
            if (isRemove && isRun) onUnload();
            return isRemove;
        }

        /// <summary>
        ///     删除卸载处理函数
        /// </summary>
        /// <param name="onUnload">卸载处理函数</param>
        /// <param name="isRun">是否执行删除的函数</param>
        /// <returns>是否删除成功</returns>
        internal static bool RemoveLast(Action onUnload, bool isRun)
        {
            bool isRemove;
            Monitor.Enter(unloaderLock);
            try
            {
                isRemove = (state == TmphUnloadState.Run || state == TmphUnloadState.WaitTransaction)
                           && lastUnloaders.Count != 0 && lastUnloaders.Remove(onUnload);
            }
            finally
            {
                Monitor.Exit(unloaderLock);
            }
            if (isRemove && isRun) onUnload();
            return isRemove;
        }

        /// <summary>
        ///     新事务开始,请保证唯一调用TransactionEnd,否则将导致卸载事件不被执行
        /// </summary>
        /// <param name="ignoreWait">忽略卸载中的等待事务，用于事务派生的事务</param>
        /// <returns>是否成功</returns>
        public static bool TransactionStart(bool ignoreWait)
        {
            var isTransaction = false;
            Monitor.Enter(unloaderLock);
            if (state == TmphUnloadState.Run || (ignoreWait && state == TmphUnloadState.WaitTransaction))
            {
                ++transactionCount;
                isTransaction = true;
            }
            Monitor.Exit(unloaderLock);
            if (!isTransaction) TmphLog.Default.Real("应用程序正在退出", true, false);
            return isTransaction;
        }

        /// <summary>
        ///     请保证TransactionStart与TransactionEnd一一对应，否则将导致卸载事件不被执行
        /// </summary>
        public static void TransactionEnd()
        {
            Monitor.Enter(unloaderLock);
            try
            {
                if ((state == TmphUnloadState.Run || state == TmphUnloadState.WaitTransaction) && --transactionCount == 0)
                    Monitor.Pulse(unloaderLock);
            }
            finally
            {
                Monitor.Exit(unloaderLock);
            }
        }

        /// <summary>
        ///     事务结束
        /// </summary>
        private struct TmphTransactionEnd
        {
            /// <summary>
            ///     任务执行委托
            /// </summary>
            public Action Action;

            /// <summary>
            ///     任务执行
            /// </summary>
            public void Run()
            {
                try
                {
                    Action();
                }
                finally
                {
                    TransactionEnd();
                }
            }
        }

        /// <summary>
        ///     获取事务结束委托
        /// </summary>
        /// <param name="run">任务执行委托</param>
        /// <returns>事务结束委托</returns>
        public static Action Transaction(Action run)
        {
            if (TransactionStart(true))
            {
                return new TmphTransactionEnd { Action = run }.Run;
            }
            TmphLog.Error.Throw(TmphLog.TmphExceptionType.ErrorOperation);
            return null;
        }

        /// <summary>
        ///     获取事务结束委托
        /// </summary>
        /// <typeparam name="TParameterType">参数类型</typeparam>
        /// <param name="run">任务执行委托</param>
        /// <param name="parameter">参数</param>
        /// <returns>事务结束委托</returns>
        public static Action Transaction<TParameterType>(Action<TParameterType> run, TParameterType parameter)
        {
            if (TransactionStart(true))
            {
                return new TmphTransactionEnd { Action = TmphRun<TParameterType>.Create(run, parameter) }.Run;
            }
            TmphLog.Error.Throw(TmphLog.TmphExceptionType.ErrorOperation);
            return null;
        }

        /// <summary>
        ///     等待事务结束
        /// </summary>
        public static void WaitTransaction()
        {
            while (transactionCount != 0) Thread.Sleep(1);
        }

        /// <summary>
        ///     事务结束检测
        /// </summary>
        private static void transactionCheck()
        {
            if (transactionCount != 0) TmphLog.Default.Real("事务未结束 " + transactionCount.toString(), false, false);
        }

        /// <summary>
        ///     退出程序
        /// </summary>
        public static void Exit()
        {
            unload(null, null);
            Environment.Exit(-1);
        }

        /// <summary>
        ///     应用程序卸载事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void unload(object sender, EventArgs e)
        {
            unload();
        }

        /// <summary>
        ///     应用程序卸载事件
        /// </summary>
        private static void unload()
        {
            Monitor.Enter(unloaderLock);
            try
            {
                if (state == TmphUnloadState.Run)
                {
                    state = TmphUnloadState.WaitTransaction;
                    TmphTimerTask.Default.Add(transactionCheck, TmphDate.NowSecond.AddMinutes(1), null);
                    if (transactionCount != 0) Monitor.Wait(unloaderLock);
                    state = TmphUnloadState.Event;
                    foreach (var value in unloaders)
                    {
                        try
                        {
                            value();
                        }
                        catch (Exception error)
                        {
                            TmphLog.Error.Real(error, null, false);
                        }
                    }
                    foreach (var value in lastUnloaders)
                    {
                        try
                        {
                            value();
                        }
                        catch (Exception error)
                        {
                            TmphLog.Error.Real(error, null, false);
                        }
                    }
                    state = TmphUnloadState.Unloaded;
                }
            }
            finally
            {
                Monitor.Exit(unloaderLock);
            }
        }

        /// <summary>
        ///     线程错误事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="error"></param>
        private static void onError(object sender, UnhandledExceptionEventArgs error)
        {
            var exception = error.ExceptionObject as Exception;
            if (exception != null) TmphLog.Error.Real(exception, null, false);
            else TmphLog.Error.Real(null, error.ExceptionObject.ToString(), false);
            unload(null, null);
        }

#if MONO
#else

        /// <summary>
        ///     UI线程错误事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="error"></param>
        private static void onError(object sender, ThreadExceptionEventArgs e)
        {
            TmphLog.Error.Real(e.Exception, null, false);
            if (IsThrowThreadException)
            {
                unload();
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
                throw e.Exception;
            }
        }

        /// <summary>
        ///     绑定到WinForm应用程序
        /// </summary>
        public static void BindWinFormApplication()
        {
            Application.ApplicationExit += unload;
            Application.ThreadException += onError;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        }

#endif

        /// <summary>
        ///     是否抛出UI线程异常
        /// </summary>
        public static bool IsThrowThreadException;

        static TmphDomainUnload()
        {
            if (AppDomain.CurrentDomain.IsDefaultAppDomain()) AppDomain.CurrentDomain.ProcessExit += unload;
            else AppDomain.CurrentDomain.DomainUnload += unload;
            AppDomain.CurrentDomain.UnhandledException += onError;
            if (TmphAppSetting.IsCheckMemory) TmphCheckMemory.Add(MethodBase.GetCurrentMethod().DeclaringType);
        }
    }
}