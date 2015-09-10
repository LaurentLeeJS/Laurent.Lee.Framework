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
using System.IO;

namespace Laurent.Lee.CLB.Config
{
    /// <summary>
    ///     网站模块相关参数
    /// </summary>
    public sealed class TmphWeb
    {
        /// <summary>
        ///     web视图重新加载禁用输出成员名称
        /// </summary>
        public const string ViewOnlyName = "ViewOnly";

        /// <summary>
        ///     默认网站模块相关参数
        /// </summary>
        public static readonly TmphWeb Default = new TmphWeb();

        /// <summary>
        ///     AJAX调用名称
        /// </summary>
        private readonly string _ajaxWebCallName = "/ajax";

        /// <summary>
        ///     AJAX回调函数名称
        /// </summary>
        private readonly char ajaxCallBackName = 'c';

        /// <summary>
        ///     AJAX调用函数名称
        /// </summary>
        private readonly char ajaxCallName = 'n';

        ///// <summary>
        ///// Json转换时间差
        ///// </summary>
        //private DateTime parseJavascriptMinTime = new DateTime(1970, 1, 1, 0, 0, 0);
        ///// <summary>
        ///// Json转换时间差
        ///// </summary>
        //public DateTime ParseJavascriptMinTime
        //{
        //    get { return parseJavascriptMinTime; }
        //}
        /// <summary>
        ///     客户端缓存时间(单位:秒)
        /// </summary>
        private readonly int clientCacheSeconds = 0;

        /// <summary>
        ///     AJAX是否判定Referer来源
        /// </summary>
        private readonly bool isAjaxReferer = true;

        /// <summary>
        ///     最大文件缓存字节数(单位KB)
        /// </summary>
        private readonly int maxCacheFileSize = 1 << 9;

        /// <summary>
        ///     最大缓存字节数(单位MB)
        /// </summary>
        private readonly int maxCacheSize = 1 << 10;

        /// <summary>
        ///     公用错误处理最大缓存数量
        /// </summary>
        private readonly int pubErrorMaxCacheCount = 1 << 10;

        /// <summary>
        ///     公用错误缓存最大字节数
        /// </summary>
        private readonly int pubErrorMaxSize = 1 << 10;

        /// <summary>
        ///     json查询对象名称
        /// </summary>
        private readonly char queryJsonName = 'j';

        /// <summary>
        ///     重新加载视图查询名称
        /// </summary>
        private readonly char reViewName = 'v';

        /// <summary>
        ///     web视图默认查询参数名称
        /// </summary>
        private readonly string viewQueryName = "query";

        /// <summary>
        ///     HTTP服务路径
        /// </summary>
        private string _httpServerPath;

        /// <summary>
        ///     Laurent.Lee.CLB js文件路径
        /// </summary>
        private string _leeFrameWorkJsPath;

        /// <summary>
        ///     网站模块相关参数
        /// </summary>
        private TmphWeb()
        {
            TmphPub.LoadConfig(this);
        }

        /// <summary>
        ///     Laurent.Lee.CLB js文件路径
        /// </summary>
        public string FastCSharpJsPath
        {
            get
            {
                if (_leeFrameWorkJsPath != null && !Directory.Exists(_leeFrameWorkJsPath)) _leeFrameWorkJsPath = null;
                if (_leeFrameWorkJsPath == null)
                {
                    try
                    {
                        var directoryInfo = new DirectoryInfo(CLB.TmphPub.ApplicationPath).Parent;
                        if (directoryInfo != null)
                        {
                            var jsPath = directoryInfo.Parent.fullName() + @"js\";
                            if (Directory.Exists(jsPath))
                                _leeFrameWorkJsPath = new DirectoryInfo(jsPath).fullName().ToLower();
                        }
                    }
                    catch (Exception error)
                    {
                        TmphLog.Default.Add(error, "没有找到js文件路径", false);
                    }
                }
                return _leeFrameWorkJsPath;
            }
        }

        /// <summary>
        ///     AJAX调用名称
        /// </summary>
        public string AjaxWebCallName
        {
            get { return _ajaxWebCallName; }
        }

        /// <summary>
        ///     AJAX调用函数名称
        /// </summary>
        public char AjaxCallName
        {
            get { return ajaxCallName; }
        }

        /// <summary>
        ///     AJAX回调函数名称
        /// </summary>
        public char AjaxCallBackName
        {
            get { return ajaxCallBackName; }
        }

        /// <summary>
        ///     AJAX是否判定Referer来源
        /// </summary>
        public bool IsAjaxReferer
        {
            get { return isAjaxReferer; }
        }

        /// <summary>
        ///     json查询对象名称
        /// </summary>
        public char QueryJsonName
        {
            get { return queryJsonName; }
        }

        /// <summary>
        ///     重新加载视图查询名称
        /// </summary>
        public char ReViewName
        {
            get { return reViewName; }
        }

        /// <summary>
        ///     web视图默认查询参数名称
        /// </summary>
        public string ViewQueryName
        {
            get { return viewQueryName; }
        }

        /// <summary>
        ///     客户端缓存时间(单位:秒)
        /// </summary>
        public int ClientCacheSeconds
        {
            get { return clientCacheSeconds < 0 ? 0 : clientCacheSeconds; }
        }

        /// <summary>
        ///     最大文件缓存字节数(单位KB)
        /// </summary>
        public int MaxCacheFileSize
        {
            get { return maxCacheFileSize < 0 ? 0 : maxCacheFileSize; }
        }

        /// <summary>
        ///     最大缓存字节数(单位MB)
        /// </summary>
        public int MaxCacheSize
        {
            get { return maxCacheSize < 0 ? 0 : maxCacheSize; }
        }

        /// <summary>
        ///     HTTP服务路径
        /// </summary>
        public string HttpServerPath
        {
            get
            {
                if (_httpServerPath == null) _httpServerPath = TmphPub.Default.WorkPath;
                return _httpServerPath;
            }
        }

        /// <summary>
        ///     公用错误缓存最大字节数
        /// </summary>
        public int PubErrorMaxSize
        {
            get { return pubErrorMaxSize; }
        }

        /// <summary>
        ///     公用错误处理最大缓存数量
        /// </summary>
        public int PubErrorMaxCacheCount
        {
            get { return pubErrorMaxCacheCount; }
        }
    }
}