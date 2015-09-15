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

using System;
using System.Net;

namespace Laurent.Lee.CLB.Net
{
    /// <summary>
    ///     IPv6地址哈希
    /// </summary>
    public struct TmphIpv6Hash : IEquatable<TmphIpv6Hash>
    {
        /// <summary>
        ///     IPv6地址
        /// </summary>
        private static readonly Func<IPAddress, ushort[]> ipAddress =
            Emit.TmphPub.GetField<IPAddress, ushort[]>("m_Numbers");

        /// <summary>
        ///     IP地址
        /// </summary>
        private ushort[] ip;

        /// <summary>
        ///     是否为空
        /// </summary>
        public bool IsNull
        {
            get { return ip == null; }
        }

        /// <summary>
        ///     IPv6地址哈希是否相等
        /// </summary>
        /// <param name="other">IPv6地址哈希</param>
        /// <returns>是否相等</returns>
        public unsafe bool Equals(TmphIpv6Hash other)
        {
            fixed (ushort* ipFixed = ip, otherFixed = other.ip)
            {
                if (*(int*)ipFixed == *(int*)otherFixed)
                {
                    return ((*(int*)(ipFixed + 2) ^ *(int*)(otherFixed + 2))
                            | (*(int*)(ipFixed + 4) ^ *(int*)(otherFixed + 4))
                            | (*(int*)(ipFixed + 6) ^ *(int*)(otherFixed + 6))) == 0;
                }
            }
            return false;
        }

        /// <summary>
        ///     IPv6地址哈希隐式转换
        /// </summary>
        /// <param name="ip">IP地址</param>
        /// <returns>IPv6地址哈希</returns>
        public static implicit operator TmphIpv6Hash(IPAddress ip)
        {
            return new TmphIpv6Hash { ip = ipAddress(ip) };
        }

        /// <summary>
        ///     设置为空值
        /// </summary>
        internal void Null()
        {
            ip = null;
        }

        /// <summary>
        ///     IPv6地址哈希值
        /// </summary>
        /// <returns>哈希值</returns>
        public override unsafe int GetHashCode()
        {
            if (ip != null)
            {
                fixed (ushort* ipFixed = ip)
                {
                    return *(int*)ipFixed ^ *(int*)(ipFixed + 2) ^ *(int*)(ipFixed + 4) ^ *(int*)(ipFixed + 6) ^
                           TmphRandom.Hash;
                }
            }
            return 0;
        }

        /// <summary>
        ///     IPv6地址哈希是否相等
        /// </summary>
        /// <param name="obj">IPv6地址哈希</param>
        /// <returns>是否相等</returns>
        public override bool Equals(object obj)
        {
            return Equals((TmphIpv6Hash)obj);
            //return obj != null & obj.GetType() == typeof(ipv6Hash) && Equals((ipv6Hash)obj);
        }
    }
}