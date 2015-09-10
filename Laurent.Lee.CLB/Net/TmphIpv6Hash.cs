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