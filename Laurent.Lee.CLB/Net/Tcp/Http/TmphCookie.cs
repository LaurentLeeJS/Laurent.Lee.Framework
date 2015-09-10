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

namespace Laurent.Lee.CLB.Net.Tcp.Http
{
    /// <summary>
    ///     HTTP响应Cookie
    /// </summary>
    //[Laurent.Lee.CLB.Code.CSharp.serialize(IsReferenceMember = false)]
    public sealed class TmphCookie
    {
        /// <summary>
        ///     有效域名
        /// </summary>
        public TmphSubArray<byte> Domain;

        /// <summary>
        ///     超时时间
        /// </summary>
        public DateTime Expires = DateTime.MinValue;

        /// <summary>
        ///     是否HTTP Only
        /// </summary>
        public bool IsHttpOnly;

        /// <summary>
        ///     是否安全
        /// </summary>
        public bool IsSecure;

        /// <summary>
        ///     名称
        /// </summary>
        public byte[] Name;

        /// <summary>
        ///     有效路径
        /// </summary>
        public byte[] Path;

        /// <summary>
        ///     值
        /// </summary>
        public byte[] Value;

        /// <summary>
        ///     HTTP响应Cookie
        /// </summary>
        public TmphCookie()
        {
        }

        /// <summary>
        ///     HTTP响应Cookie
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="value">值</param>
        public TmphCookie(string name, string value)
        {
            if (name.Length() != 0) Name = name.GetBytes();
            if (value.Length() != 0) Value = value.GetBytes();
        }

        /// <summary>
        ///     HTTP响应Cookie
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="value">值</param>
        /// <param name="domain">有效域名</param>
        /// <param name="path">有效路径</param>
        /// <param name="isSecure">是否安全</param>
        /// <param name="isHttpOnly">是否HTTP Only</param>
        public TmphCookie(string name, string value, string domain, string path, bool isSecure, bool isHttpOnly)
        {
            if (name.Length() != 0) Name = name.GetBytes();
            if (value.Length() != 0) Value = value.GetBytes();
            if (domain.Length() != 0)
            {
                var data = domain.GetBytes();
                Domain = TmphSubArray<byte>.Unsafe(data, 0, data.Length);
            }
            if (path.Length() != 0) Path = path.GetBytes();
            IsSecure = isSecure;
            IsHttpOnly = isHttpOnly;
        }

        /// <summary>
        ///     HTTP响应Cookie
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="value">值</param>
        /// <param name="expires">超时时间,DateTime.MinValue表示忽略</param>
        /// <param name="domain">有效域名</param>
        /// <param name="path">有效路径</param>
        /// <param name="isSecure">是否安全</param>
        /// <param name="isHttpOnly">是否HTTP Only</param>
        public TmphCookie(string name, string value, DateTime expires
            , string domain, string path, bool isSecure, bool isHttpOnly)
            : this(name, value, domain, path, isSecure, isHttpOnly)
        {
            Expires = expires;
        }

        /// <summary>
        ///     HTTP响应Cookie
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="value">值</param>
        /// <param name="expires">超时时间,DateTime.MinValue表示忽略</param>
        /// <param name="domain">有效域名</param>
        /// <param name="path">有效路径</param>
        /// <param name="isSecure">是否安全</param>
        /// <param name="isHttpOnly">是否HTTP Only</param>
        internal TmphCookie(byte[] name, byte[] value, DateTime expires, TmphSubArray<byte> domain, byte[] path,
            bool isSecure, bool isHttpOnly)
        {
            Name = name;
            Value = value;
            Expires = expires;
            Domain = domain;
            Path = path;
            IsSecure = isSecure;
            IsHttpOnly = isHttpOnly;
        }
    }
}