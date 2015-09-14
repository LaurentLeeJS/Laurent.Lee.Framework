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

using Laurent.Lee.CLB.Threading;
using System;
using System.Collections.Generic;

namespace Laurent.Lee.CLB.Web
{
    /// <summary>
    /// 长连接轮询
    /// </summary>
    public static class TmphPoll
    {
        /// <summary>
        /// 轮询验证信息
        /// </summary>
        public struct TmphVerifyInfo
        {
            /// <summary>
            /// 用户标识
            /// </summary>
            public int UserId;

            /// <summary>
            /// 验证信息
            /// </summary>
            public string Verify;
        }

        /// <summary>
        /// 长连接轮询验证集合
        /// </summary>
        private static readonly Dictionary<int, TmphKeyValue<ulong, DateTime>> verifys = TmphDictionary.CreateInt<TmphKeyValue<ulong, DateTime>>();

        /// <summary>
        /// 长连接轮询验证访问锁
        /// </summary>
        private static int verifyLock;

        /// <summary>
        /// 获取用户长连接轮询验证
        /// </summary>
        /// <param name="userId">用户标识</param>
        /// <returns>长连接轮询验证</returns>
        private static string getString(int userId)
        {
            ulong verify = get(userId);
            return verify == 0 ? null : verify.toHex16();
        }

        /// <summary>
        /// 获取用户长连接轮询验证
        /// </summary>
        /// <param name="userId">用户标识</param>
        /// <returns>长连接轮询验证,0表示失败</returns>
        private static ulong get(int userId)
        {
            TmphKeyValue<ulong, DateTime> value;
            if (verifys.TryGetValue(userId, out value))
            {
                if (value.Value > TmphDate.NowSecond) return value.Key;
                else
                {
                    ulong verify = CLB.TmphRandom.Default.SecureNextULongNotZero();
                    TmphInterlocked.NoCheckCompareSetSleep0(ref verifyLock);
                    try
                    {
                        verifys[userId] = new TmphKeyValue<ulong, DateTime>(verify, TmphDate.NowSecond.AddHours(1));
                    }
                    finally { verifyLock = 0; }
                    return verify;
                }
            }
            return 0;
        }

        /// <summary>
        /// 获取用户长连接轮询验证
        /// </summary>
        /// <param name="userId">用户标识</param>
        /// <returns>长连接轮询验证</returns>
        public static TmphVerifyInfo Get(int userId)
        {
            return new TmphVerifyInfo { UserId = userId, Verify = getString(userId) ?? add(userId) };
        }

        /// <summary>
        /// 添加用户长连接轮询验证
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        private static string add(int userId)
        {
            TmphKeyValue<ulong, DateTime> value;
            if (!verifys.TryGetValue(userId, out value))
            {
                ulong verify = CLB.TmphRandom.Default.SecureNextULongNotZero();
                TmphInterlocked.NoCheckCompareSetSleep0(ref verifyLock);
                try
                {
                    if (!verifys.ContainsKey(userId))
                    {
                        verifys.Add(userId, value = new TmphKeyValue<ulong, DateTime>(verify, TmphDate.NowSecond.AddHours(1)));
                    }
                }
                finally { verifyLock = 0; }
            }
            return value.Key.toHex16();
        }

        /// <summary>
        /// 删除用户长连接轮询验证
        /// </summary>
        /// <param name="userId"></param>
        public static void Remove(int userId)
        {
            TmphInterlocked.NoCheckCompareSetSleep0(ref verifyLock);
            try
            {
                verifys.Remove(userId);
            }
            finally { verifyLock = 0; }
        }

        /// <summary>
        /// 轮询验证检测
        /// </summary>
        /// <param name="verify"></param>
        /// <returns></returns>
        public static bool Verify(TmphVerifyInfo verify)
        {
            ulong verify64 = verify.Verify.parseHex16NoCheck();
            if (verify64 != 0)
            {
                TmphKeyValue<ulong, DateTime> value;
                return verifys.TryGetValue(verify.UserId, out value) && value.Key == verify64;
            }
            return false;
        }

        static TmphPoll()
        {
            if (Laurent.Lee.CLB.Config.TmphAppSetting.IsCheckMemory) TmphCheckMemory.Add(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        }
    }
}