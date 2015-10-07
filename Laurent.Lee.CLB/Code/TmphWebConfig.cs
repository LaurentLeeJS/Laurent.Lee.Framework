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

using Laurent.Lee.CLB.Config;
using System;
using System.Text;

namespace Laurent.Lee.CLB.Code
{
    /// <summary>
    ///     网站生成配置
    /// </summary>
    public abstract class TmphWebConfig
    {
        /// <summary>
        ///     Session类型
        /// </summary>
        public virtual Type SessionType
        {
            get { return typeof(int); }
        }

        /// <summary>
        ///     文件编码
        /// </summary>
        public virtual Encoding Encoding
        {
            get { return TmphAppSetting.Encoding; }
        }

        /// <summary>
        ///     默认Cookie域名
        /// </summary>
        public abstract string CookieDomain { get; }

        /// <summary>
        ///     静态文件网站域名
        /// </summary>
        public virtual string StaticFileDomain
        {
            get { return CookieDomain; }
        }

        /// <summary>
        ///     图片文件域名
        /// </summary>
        public virtual string ImageDomain
        {
            get { return CookieDomain; }
        }

        /// <summary>
        ///     轮询网站域名
        /// </summary>
        public virtual string PollDomain
        {
            get { return CookieDomain; }
        }

        /// <summary>
        ///     视图加载失败重定向
        /// </summary>
        public virtual string NoViewLocation
        {
            get { return null; }
        }

        /// <summary>
        ///     WEB视图扩展默认文件名称
        /// </summary>
        public virtual string ViewJsFileName
        {
            get { return "webView"; }
        }

        /// <summary>
        ///     客户端缓存标识版本
        /// </summary>
        public virtual int ETagVersion
        {
            get { return 0; }
        }

        /// <summary>
        ///     是否忽略大小写
        /// </summary>
        public virtual bool IgnoreCase
        {
            get { return true; }
        }

        /// <summary>
        ///     文件缓存是否预留HTTP头部空间
        /// </summary>
        public virtual bool IsFileCacheHeader
        {
            get { return true; }
        }
    }
}