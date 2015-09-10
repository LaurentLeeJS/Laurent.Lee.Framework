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
using System.Threading;

namespace Laurent.Lee.CLB.Threading
{
    /// <summary>
    ///     原子操作扩张
    /// </summary>
    public static class TmphInterlocked
    {
        /// <summary>
        ///     将目标值从0改置为1,循环等待周期切换(适应于等待时间极短的情况)
        /// </summary>
        /// <param name="value">目标值</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static void NoCheckCompareSetSleep0(ref int value)
        {
            while (Interlocked.CompareExchange(ref value, 1, 0) != 0) Thread.Sleep(0);
        }

        /// <summary>
        ///     将目标值从0改置为1,循环等待周期切换(适应于等待时间极短的情况)
        /// </summary>
        /// <param name="value">目标值</param>
        public static void CompareSetSleep0(ref int value)
        {
            if (Interlocked.CompareExchange(ref value, 1, 0) != 0)
            {
                var time = TmphDate.NowSecond.AddSeconds(2);
                do
                {
                    Thread.Sleep(0);
                    if (Interlocked.CompareExchange(ref value, 1, 0) == 0) return;
                } while (TmphDate.NowSecond < time);
                TmphLog.Error.Add("可能出现死锁", true, false);
                while (Interlocked.CompareExchange(ref value, 1, 0) != 0) Thread.Sleep(0);
            }
        }

        /// <summary>
        ///     将目标值从0改置为1,循环等待周期切换(适应于等待时间较短的情况)
        /// </summary>
        /// <param name="value">目标值</param>
        public static void CompareSetSleep1(ref int value)
        {
            if (Interlocked.CompareExchange(ref value, 1, 0) != 0)
            {
                Thread.Sleep(0);
                if (Interlocked.CompareExchange(ref value, 1, 0) != 0)
                {
                    var time = TmphDate.NowSecond.AddSeconds(2);
                    do
                    {
                        Thread.Sleep(1);
                        if (Interlocked.CompareExchange(ref value, 1, 0) == 0) return;
                    } while (TmphDate.NowSecond < time);
                    TmphLog.Default.Add("线程等待时间过长", true, false);
                    while (Interlocked.CompareExchange(ref value, 1, 0) != 0) Thread.Sleep(1);
                }
            }
        }

        /// <summary>
        ///     将目标值从0改置为1,循环等待周期固定(适应于等待时间可能较长的情况)
        /// </summary>
        /// <param name="value">目标值</param>
        /// <param name="logSeconds">输入日志秒数</param>
        public static void CompareSetSleep(ref int value, double logSeconds = 5)
        {
            if (Interlocked.CompareExchange(ref value, 1, 0) != 0)
            {
                var time = TmphDate.NowSecond.AddSeconds(logSeconds + 1);
                do
                {
                    Thread.Sleep(1);
                    if (Interlocked.CompareExchange(ref value, 1, 0) == 0) return;
                } while (TmphDate.NowSecond < time);
                TmphLog.Default.Add("线程等待时间过长", true, false);
                while (Interlocked.CompareExchange(ref value, 1, 0) != 0) Thread.Sleep(1);
            }
        }

        /// <summary>
        ///     等待单次运行
        /// </summary>
        /// <param name="runState">执行状态,0表示未运行,1表示运行中,2表示已经运行</param>
        /// <param name="run">执行委托</param>
        public static void WaitRunOnce(ref int runState, Action run)
        {
            var isRun = Interlocked.CompareExchange(ref runState, 1, 0);
            if (isRun == 0)
            {
                try
                {
                    run();
                }
                finally
                {
                    runState = 2;
                }
            }
            else if (isRun == 1)
            {
                while (runState == 1) Thread.Sleep(1);
            }
        }

        /// <summary>
        ///     字典
        /// </summary>
        /// <typeparam name="TKeyType"></typeparam>
        /// <typeparam name="TValueType"></typeparam>
        public struct TmphDictionary<TKeyType, TValueType>
        {
            /// <summary>
            ///     字典
            /// </summary>
            private readonly Dictionary<TKeyType, TValueType> values;

            /// <summary>
            ///     访问锁
            /// </summary>
            private int valueLock;

            /// <summary>
            ///     字典
            /// </summary>
            /// <param name="values"></param>
            public TmphDictionary(Dictionary<TKeyType, TValueType> values)
            {
                if (values == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                this.values = values;
                valueLock = 0;
            }

            /// <summary>
            ///     获取数据
            /// </summary>
            /// <param name="key"></param>
            /// <param name="value"></param>
            /// <returns>是否存在数据</returns>
            public bool TryGetValue(TKeyType key, out TValueType value)
            {
                NoCheckCompareSetSleep0(ref valueLock);
                if (values.TryGetValue(key, out value))
                {
                    valueLock = 0;
                    return true;
                }
                valueLock = 0;
                return false;
            }

            /// <summary>
            ///     设置数据
            /// </summary>
            /// <param name="key"></param>
            /// <param name="value"></param>
            public void Set(TKeyType key, TValueType value)
            {
                NoCheckCompareSetSleep0(ref valueLock);
                try
                {
                    values[key] = value;
                }
                finally
                {
                    valueLock = 0;
                }
            }

            /// <summary>
            ///     获取数据
            /// </summary>
            /// <param name="key"></param>
            /// <param name="value"></param>
            /// <returns>是否存在数据</returns>
            public bool Set(TKeyType key, TValueType value, out TValueType oldValue)
            {
                NoCheckCompareSetSleep0(ref valueLock);
                if (values.TryGetValue(key, out oldValue))
                {
                    try
                    {
                        values[key] = value;
                    }
                    finally
                    {
                        valueLock = 0;
                    }
                    return true;
                }
                try
                {
                    values.Add(key, value);
                }
                finally
                {
                    valueLock = 0;
                }
                return false;
            }
        }

        /// <summary>
        ///     字典
        /// </summary>
        /// <typeparam name="TKeyType"></typeparam>
        /// <typeparam name="TValueType"></typeparam>
        public struct TmphLastDictionary<TKeyType, TValueType> where TKeyType : struct, IEquatable<TKeyType>
        {
            /// <summary>
            ///     字典
            /// </summary>
            private readonly Dictionary<TKeyType, TValueType> values;

            /// <summary>
            ///     最后一次访问的关键字
            /// </summary>
            private TKeyType lastKey;

            /// <summary>
            ///     最后一次访问的数据
            /// </summary>
            private TValueType lastValue;

            /// <summary>
            ///     访问锁
            /// </summary>
            private int valueLock;

            /// <summary>
            ///     字典
            /// </summary>
            /// <param name="values"></param>
            public TmphLastDictionary(Dictionary<TKeyType, TValueType> values)
            {
                if (values == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                this.values = values;
                valueLock = 0;
                lastKey = default(TKeyType);
                lastValue = default(TValueType);
            }

            /// <summary>
            ///     是否空字典
            /// </summary>
            public bool IsNull
            {
                get { return values == null; }
            }

            /// <summary>
            ///     获取数据
            /// </summary>
            /// <param name="key"></param>
            /// <param name="value"></param>
            /// <returns>是否存在数据</returns>
            public bool TryGetValue(TKeyType key, out TValueType value)
            {
                NoCheckCompareSetSleep0(ref valueLock);
                if (lastKey.Equals(key))
                {
                    value = lastValue;
                    valueLock = 0;
                    return true;
                }
                if (values.TryGetValue(key, out value))
                {
                    lastKey = key;
                    lastValue = value;
                    valueLock = 0;
                    return true;
                }
                valueLock = 0;
                return false;
            }

            /// <summary>
            ///     设置数据
            /// </summary>
            /// <param name="key"></param>
            /// <param name="value"></param>
            public void Set(TKeyType key, TValueType value)
            {
                NoCheckCompareSetSleep0(ref valueLock);
                try
                {
                    values[lastKey = key] = lastValue = value;
                }
                finally
                {
                    valueLock = 0;
                }
            }
        }
    }
}