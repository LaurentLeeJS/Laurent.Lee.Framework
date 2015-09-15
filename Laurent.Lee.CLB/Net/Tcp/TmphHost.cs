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

using Laurent.Lee.CLB.Code.CSharp;
using System;

namespace Laurent.Lee.CLB.Net.Tcp
{
    /// <summary>
    ///     TCP服务端口信息
    /// </summary>
    [Emit.TmphDataSerialize(IsReferenceMember = false, IsMemberMap = false)]
    public struct TmphHost : IEquatable<TmphHost>
    {
        /// <summary>
        ///     主机名称或者IP地址
        /// </summary>
        public string Host;

        /// <summary>
        ///     端口号
        /// </summary>
        public int Port;

        /// <summary>
        ///     判断是否TCP服务端口信息
        /// </summary>
        /// <param name="other">TCP服务端口信息</param>
        /// <returns>是否同一TCP服务端口信息</returns>
        public bool Equals(TmphHost other)
        {
            return Host == other.Host && Port == other.Port;
        }

        /// <summary>
        ///     主机名称转换成IP地址
        /// </summary>
        /// <returns>是否转换成功</returns>
        public bool HostToIpAddress()
        {
            var ipAddress = TmphTcpBase.HostToIpAddress(Host);
            if (ipAddress == null) return false;
            Host = ipAddress.ToString();
            return true;
        }

        /// <summary>
        ///     获取哈希值
        /// </summary>
        /// <returns>哈希值</returns>
        public override int GetHashCode()
        {
            return Host == null ? Port : (Host.GetHashCode() ^ Port);
        }

        /// <summary>
        ///     判断是否TCP服务端口信息
        /// </summary>
        /// <param name="other">TCP服务端口信息</param>
        /// <returns>是否同一TCP服务端口信息</returns>
        public override bool Equals(object other)
        {
            return Equals((TmphHost)other);
            //return other != null && other.GetType() == typeof(host) && Equals((host)other);
        }
    }
}