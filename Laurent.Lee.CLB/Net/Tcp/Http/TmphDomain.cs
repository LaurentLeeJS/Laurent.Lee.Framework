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