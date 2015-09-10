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

using Laurent.Lee.CLB.Algorithm;
using Laurent.Lee.CLB.Emit;
using System;

namespace Laurent.Lee.CLB.Net.Tcp.Http
{
    /// <summary>
    ///     域名信息
    /// </summary>
    [TmphDataSerialize(IsReferenceMember = false, IsMemberMap = false)]
    public sealed class TmphDomain : IEquatable<TmphDomain>
    {
        /// <summary>
        ///     安全证书文件
        /// </summary>
        public string CertificateFileName;

        /// <summary>
        ///     域名
        /// </summary>
        public byte[] Domain;

        /// <summary>
        ///     HASH值
        /// </summary>
        private int hashCode;

        /// <summary>
        ///     TCP服务端口信息
        /// </summary>
        public TmphHost Host;

        /// <summary>
        ///     域名是否全名,否则表示泛域名后缀
        /// </summary>
        public bool IsFullName = true;

        /// <summary>
        ///     是否仅用于内网IP映射
        /// </summary>
        public bool IsOnlyHost;

        /// <summary>
        ///     域名信息
        /// </summary>
        public TmphDomain()
        {
        }

        /// <summary>
        ///     域名信息
        /// </summary>
        /// <param name="domain">域名</param>
        /// <param name="host">TCP服务端口信息</param>
        /// <param name="isFullName">域名是否全名,否则表示泛域名后缀</param>
        public TmphDomain(string domain, TmphHost host, bool isFullName = true)
        {
            Domain = domain.GetBytes();
            Host = host;
            IsFullName = isFullName;
        }

        /// <summary>
        ///     判断是否TCP服务端口信息
        /// </summary>
        /// <param name="other">TCP服务端口信息</param>
        /// <returns>是否同一TCP服务端口信息</returns>
        public bool Equals(TmphDomain other)
        {
            return other != null && GetHashCode() == other.GetHashCode()
                   && (IsFullName ? other.IsFullName : !other.IsFullName)
                   && Domain.equal(other.Domain) && Host.Equals(other.Host);
        }

        /// <summary>
        ///     获取哈希值
        /// </summary>
        /// <returns>哈希值</returns>
        public override int GetHashCode()
        {
            if (hashCode == 0)
            {
                hashCode = Host.GetHashCode() ^ TmphHashCode.GetHashCode32(Domain) ^ (IsFullName ? int.MinValue : 0);
                if (hashCode == 0) hashCode = int.MaxValue;
            }
            return hashCode;
        }

        /// <summary>
        ///     判断是否TCP服务端口信息
        /// </summary>
        /// <param name="other">TCP服务端口信息</param>
        /// <returns>是否同一TCP服务端口信息</returns>
        public override bool Equals(object other)
        {
            return Equals((TmphDomain)other);
        }
    }
}