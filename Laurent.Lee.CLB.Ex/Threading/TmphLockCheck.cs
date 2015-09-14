using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Laurent.Lee.CLB.Threading
{
    /// <summary>
    /// 死锁检测
    /// </summary>
    public static class TmphLockCheck
    {
        /// <summary>
        /// 未释放锁信息
        /// </summary>
        private struct TmphLockInfo
        {
            /// <summary>
            /// 申请次数
            /// </summary>
            public int Count;

            /// <summary>
            /// 第一次申请时间
            /// </summary>
            public DateTime LockTime;

            /// <summary>
            /// 最后一次申请堆栈
            /// </summary>
            public StackTrace StackTrace;

            /// <summary>
            /// 未释放锁信息
            /// </summary>
            /// <returns>未释放锁信息</returns>
            public override string ToString()
            {
                return LockTime.toString() + "[" + Count.toString() + @"]
" + StackTrace.ToString();
            }
        }

        /// <summary>
        /// 当前未释放的锁
        /// </summary>
        private static readonly Dictionary<TmphObjectReference, TmphLockInfo> lockInfos;

        /// <summary>
        /// 访问锁
        /// </summary>
        private static int interlock;

        /// <summary>
        /// 是否输出日志
        /// </summary>
        private static bool isOutput;

        /// <summary>
        /// 输出日志休眠时间
        /// </summary>
        private static TimeSpan sleepTime;

        /// <summary>
        /// 申请锁
        /// </summary>
        /// <param name="value">锁对象, 必须是引用类型且不能为null</param>
        public static void Enter(object value)
        {
            Monitor.Enter(value);
            if (Laurent.Lee.CLB.Config.TmphPub.Default.LockCheckMinutes != 0)
            {
                TmphLockInfo lockInfo;
                TmphInterlocked.NoCheckCompareSetSleep0(ref interlock);
                try
                {
                    if (lockInfos.TryGetValue(new TmphObjectReference { Value = value }, out lockInfo))
                    {
                        ++lockInfo.Count;
                        lockInfo.StackTrace = new StackTrace();
                        lockInfos[new TmphObjectReference { Value = value }] = lockInfo;
                    }
                    else lockInfos.Add(new TmphObjectReference { Value = value }, new TmphLockInfo { Count = 1, LockTime = Laurent.Lee.CLB.TmphDate.NowSecond, StackTrace = new StackTrace() });
                }
                finally { interlock = 0; }
            }
        }

        /// <summary>
        /// 释放锁
        /// </summary>
        /// <param name="value">必须是当前线程已经申请的锁对象, 必须是引用类型且不能为null</param>
        public static void Exit(object value)
        {
            if (Laurent.Lee.CLB.Config.TmphPub.Default.LockCheckMinutes != 0)
            {
                TmphLockInfo lockInfo;
                TmphInterlocked.NoCheckCompareSetSleep0(ref interlock);
                try
                {
                    if (lockInfos.TryGetValue(new TmphObjectReference { Value = value }, out lockInfo))
                    {
                        if (--lockInfo.Count == 0) lockInfos.Remove(new TmphObjectReference { Value = value });
                        else
                        {
                            lockInfo.StackTrace = new StackTrace();
                            lockInfos[new TmphObjectReference { Value = value }] = lockInfo;
                        }
                    }
                }
                finally { interlock = 0; }
                if (lockInfo.StackTrace == null) TmphLog.Error.Add("没有找到需要释放的锁", true, true);
            }
            Monitor.Exit(value);
        }

        /// <summary>
        /// 释放锁并等待唤醒
        /// </summary>
        /// <param name="value">必须是当前线程已经申请的锁对象, 是引用类型且不能为null</param>
        public static void Wait(object value)
        {
            object newValue = null;
            if (Laurent.Lee.CLB.Config.TmphPub.Default.LockCheckMinutes != 0)
            {
                TmphLockInfo lockInfo;
                TmphInterlocked.NoCheckCompareSetSleep0(ref interlock);
                try
                {
                    if (lockInfos.TryGetValue(new TmphObjectReference { Value = value }, out lockInfo))
                    {
                        lockInfo.StackTrace = new StackTrace();
                        lockInfos.Add(new TmphObjectReference { Value = newValue = new object() }, lockInfo);
                        lockInfos.Remove(new TmphObjectReference { Value = value });
                    }
                }
                finally { interlock = 0; }
                if (lockInfo.StackTrace == null) TmphLog.Error.Add("没有找到需要释放的锁", true, true);
            }
            Monitor.Wait(value);
            if (newValue != null)
            {
                TmphInterlocked.NoCheckCompareSetSleep0(ref interlock);
                try
                {
                    TmphLockInfo lockInfo = lockInfos[new TmphObjectReference { Value = newValue }];
                    lockInfos.Add(new TmphObjectReference { Value = value }, lockInfo);
                    lockInfos.Remove(new TmphObjectReference { Value = newValue });
                }
                finally { interlock = 0; }
            }
        }

        /// <summary>
        /// 输出未释放的锁信息
        /// </summary>
        private static void output()
        {
            while (isOutput)
            {
                try
                {
                    DateTime time = Laurent.Lee.CLB.TmphDate.NowSecond.AddMinutes(-Laurent.Lee.CLB.Config.TmphPub.Default.LockCheckMinutes);
                    TmphLockInfo[] values;
                    TmphInterlocked.NoCheckCompareSetSleep0(ref interlock);
                    try
                    {
                        values = lockInfos.Values.GetArray();
                    }
                    finally { interlock = 0; }
                    TmphList<TmphLockInfo> list = values.toList().remove(value => value.LockTime > time);
                    if (list.Count() != 0)
                    {
                        TmphLog.Default.Add("未释放锁数量 " + list.Count.toString() + @"
" + list.JoinString(@"
", value => value.ToString()), true, false);
                    }
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
                Thread.Sleep(sleepTime);
            }
        }

        /// <summary>
        /// 停止输出日志
        /// </summary>
        private static void dispose()
        {
            isOutput = false;
        }

        static TmphLockCheck()
        {
            if (Laurent.Lee.CLB.Config.TmphPub.Default.LockCheckMinutes != 0)
            {
                lockInfos = TmphDictionary<TmphObjectReference>.Create<TmphLockInfo>();
                isOutput = true;
                sleepTime = new TimeSpan(0, Laurent.Lee.CLB.Config.TmphPub.Default.LockCheckMinutes, 0);
                Threading.TmphThreadPool.TinyPool.Start(output, dispose, null);
            }
            if (Laurent.Lee.CLB.Config.TmphAppSetting.IsCheckMemory) TmphCheckMemory.Add(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        }
    }
}