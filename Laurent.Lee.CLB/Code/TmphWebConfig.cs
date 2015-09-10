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