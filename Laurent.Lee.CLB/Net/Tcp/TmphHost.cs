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