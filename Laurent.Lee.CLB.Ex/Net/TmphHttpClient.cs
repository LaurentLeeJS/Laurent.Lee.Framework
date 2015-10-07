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
using System.Net;

namespace Laurent.Lee.CLB.Net
{
    /// <summary>
    /// HTTP客户端
    /// </summary>
    public static class TmphHttpClient
    {
        /// <summary>
        /// IP地址信息
        /// </summary>
        private struct TmphIpAddress
        {
            /// <summary>
            /// 超时时间
            /// </summary>
            public DateTime Timeout;

            /// <summary>
            /// IP地址
            /// </summary>
            public IPAddress[] Ips;

            /// <summary>
            /// 域名字符串
            /// </summary>
            public string Domain;
        }

        /// <summary>
        /// 域名转换IP地址集合
        /// </summary>
        private static readonly TmphFifoPriorityQueue<TmphHashBytes, TmphIpAddress> domainIps = new TmphFifoPriorityQueue<TmphHashBytes, TmphIpAddress>();

        /// <summary>
        /// 域名转换IP地址访问锁
        /// </summary>
        private static int domainIpLock;

        /// <summary>
        /// 域名转IP地址缓存超时时钟周期
        /// </summary>
        private static readonly long domainIpTimeoutTicks = new TimeSpan(0, Config.TmphHttp.Default.IpAddressTimeoutMinutes, 0).Ticks;

        /// <summary>
        /// 清除域名转换IP地址集合
        /// </summary>
        public static void ClearDomainIPAddress()
        {
            TmphInterlocked.NoCheckCompareSetSleep0(ref domainIpLock);
            domainIps.Clear();
            domainIpLock = 0;
        }

        /// <summary>
        /// 设置域名转换IP地址
        /// </summary>
        /// <param name="key">域名</param>
        /// <param name="ipAddress">IP地址</param>
        private static void setDomainIp(TmphHashBytes key, TmphIpAddress ipAddress)
        {
            TmphInterlocked.NoCheckCompareSetSleep0(ref domainIpLock);
            try
            {
                domainIps.Set(key, ipAddress);
                if (domainIps.Count == Config.TmphHttp.Default.IpAddressCacheCount) domainIps.Pop();
            }
            finally { domainIpLock = 0; }
        }

        /// <summary>
        /// 根据域名获取IP地址
        /// </summary>
        /// <param name="domain">域名</param>
        /// <returns>IP地址,失败返回null</returns>
        public unsafe static IPAddress[] GetIPAddress(TmphSubArray<byte> domain)
        {
            try
            {
                fixed (byte* domainFixed = domain.Array)
                {
                    byte* domainStart = domainFixed + domain.StartIndex;
                    Unsafe.TmphMemory.ToLower(domainStart, domainStart + domain.Count);
                    TmphHashBytes key = domain;
                    TmphIpAddress value;
                    TmphInterlocked.NoCheckCompareSetSleep0(ref domainIpLock);
                    try
                    {
                        value = domainIps.Get(key, default(TmphIpAddress));
                        if (value.Ips != null && value.Timeout < TmphDate.NowSecond)
                        {
                            domainIps.Remove(key, out value);
                            value.Ips = null;
                        }
                    }
                    finally { domainIpLock = 0; }
                    if (value.Ips == null)
                    {
                        if (value.Domain == null) value.Domain = TmphString.DeSerialize(domainStart, -domain.Count);
                        IPAddress ip;
                        if (IPAddress.TryParse(value.Domain, out ip))
                        {
                            value.Timeout = DateTime.MaxValue;
                            value.Domain = null;
                            setDomainIp(key.Copy(), value);
                            return value.Ips = new IPAddress[] { ip };
                        }
                        value.Ips = Dns.GetHostEntry(value.Domain).AddressList;
                        if (value.Ips.Length != 0)
                        {
                            value.Timeout = TmphDate.NowSecond.AddTicks(domainIpTimeoutTicks);
                            setDomainIp(key.Copy(), value);
                            return value.Ips;
                        }
                    }
                    else return value.Ips;
                }
            }
            catch (Exception error)
            {
                TmphLog.Default.Add(error, null, false);
            }
            return null;
        }

        static TmphHttpClient()
        {
            if (Laurent.Lee.CLB.Config.TmphAppSetting.IsCheckMemory) TmphCheckMemory.Add(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        }
    }
}