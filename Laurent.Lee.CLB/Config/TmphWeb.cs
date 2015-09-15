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