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