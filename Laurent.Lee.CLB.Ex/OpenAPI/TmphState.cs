﻿/*
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

using Laurent.Lee.CLB.Threading;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Laurent.Lee.CLB.OpenAPI
{
    /// <summary>
    /// 状态验证
    /// </summary>
    public static class TmphState
    {
        /// <summary>
        /// 第三方登录URL重定向Cooike名称
        /// </summary>
        public static readonly byte[] OpenLoginUrlCookieName = ("OpenLoginUrl").GetBytes();

        /// <summary>
        /// 状态验证
        /// </summary>
        private static readonly Dictionary<ulong, DateTime> states = TmphDictionary.CreateULong<DateTime>();

        /// <summary>
        /// 状态验证访问锁
        /// </summary>
        private static int stateLock;

        /// <summary>
        /// 验证状态
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static bool Verify(string state)
        {
            return Verify(state.parseHex16NoCheck());
        }

        /// <summary>
        /// 验证状态
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static bool Verify(ulong state)
        {
            if (state != 0)
            {
                DateTime timeout;
                TmphInterlocked.CompareSetSleep1(ref stateLock);
                try
                {
                    if (states.TryGetValue(state, out timeout)) states.Remove(state);
                    else timeout = DateTime.MinValue;
                }
                finally { stateLock = 0; }
                return timeout >= TmphDate.NowSecond;
            }
            return false;
        }

        /// <summary>
        /// 获取状态验证
        /// </summary>
        /// <returns></returns>
        public static ulong Get()
        {
            do
            {
                DateTime timeout = TmphDate.NowSecond.AddMinutes(10);
                ulong value = CLB.TmphRandom.Default.SecureNextULongNotZero();
                TmphInterlocked.CompareSetSleep1(ref stateLock);
                try
                {
                    if (states.ContainsKey(value)) value = 0;
                    else states.Add(value, timeout);
                }
                finally { stateLock = 0; }
                if (value != 0)
                {
                    if (Interlocked.CompareExchange(ref isRefresh, 1, 0) == 0) Laurent.Lee.CLB.Threading.TmphTimerTask.Default.Add(refreshHandle, timeout);
                    return value;
                }
            }
            while (true);
        }

        /// <summary>
        /// 获取状态验证字符串
        /// </summary>
        /// <returns></returns>
        public static string GetString()
        {
            return Get().toHex16();
        }

        /// <summary>
        /// 刷新状态验证集合
        /// </summary>
        private static TmphSubArray<ulong> refreshStates;

        /// <summary>
        /// 是否正在刷新状态验证集合
        /// </summary>
        private static int isRefresh;

        /// <summary>
        /// 刷新状态验证集合
        /// </summary>
        private static readonly Action refreshHandle = refresh;

        /// <summary>
        /// 刷新状态验证集合
        /// </summary>
        private static void refresh()
        {
            DateTime timeout = TmphDate.Now;
            TmphInterlocked.NoCheckCompareSetSleep0(ref stateLock);
            try
            {
                foreach (KeyValuePair<ulong, DateTime> state in states)
                {
                    if (state.Value <= timeout) refreshStates.Add(state.Key);
                }
                int count = refreshStates.Count;
                if (count != 0)
                {
                    if (count == states.Count) states.Clear();
                    else
                    {
                        foreach (ulong state in refreshStates.Array) states.Remove(state);
                    }
                }
            }
            finally
            {
                stateLock = 0;
                refreshStates.Empty();
                if (states.Count == 0) isRefresh = 0;
                else Laurent.Lee.CLB.Threading.TmphTimerTask.Default.Add(refreshHandle, TmphDate.NowSecond.AddMinutes(10));
            }
        }

        static TmphState()
        {
            if (Laurent.Lee.CLB.Config.TmphAppSetting.IsCheckMemory) TmphCheckMemory.Add(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        }
    }
}