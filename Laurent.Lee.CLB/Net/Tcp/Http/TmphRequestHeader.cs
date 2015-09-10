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

using Laurent.Lee.CLB.Code;
using Laurent.Lee.CLB.Config;
using Laurent.Lee.CLB.Emit;
using Laurent.Lee.CLB.Reflection;
using Laurent.Lee.CLB.Web;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using TmphHttp = Laurent.Lee.CLB.Web.TmphHttp;

namespace Laurent.Lee.CLB.Net.Tcp.Http
{
    /// <summary>
    ///     HTTP请求头部
    /// </summary>
    public sealed class TmphRequestHeader
    {
        /// <summary>
        ///     提交数据类型
        /// </summary>
        public enum TmphPostType : byte
        {
            /// <summary>
            /// </summary>
            None,

            /// <summary>
            ///     JSON数据
            /// </summary>
            Json,

            /// <summary>
            ///     表单
            /// </summary>
            Form,

            /// <summary>
            ///     表单数据
            /// </summary>
            FormData
        }

        /// <summary>
        ///     Laurent.Lee.CLB爬虫标识
        /// </summary>
        public const string LaurentLeeFramework_SpiderUserAgent = "Laurent.Lee.CLB spider";

        /// <summary>
        ///     最大数据分隔符长度
        /// </summary>
        private const int maxBoundaryLength = 128;

        /// <summary>
        ///     Google请求#!查询名称
        /// </summary>
        private static readonly byte[] googleFragmentName = ("escaped_fragment_=").GetBytes();

        /// <summary>
        ///     HTTP头名称解析委托
        /// </summary>
        private static readonly TmphUniqueDictionary<TmphHeaderName, Action<TmphRequestHeader, TmphBufferIndex>> parses;

        /// <summary>
        ///     搜索引擎标识trie图
        /// </summary>
        private static readonly TmphTrieGraph searchEngines;

        /// <summary>
        ///     查询参数索引集合
        /// </summary>
        private readonly TmphList<TmphKeyValue<TmphBufferIndex, TmphBufferIndex>> queryIndexs =
            new TmphList<TmphKeyValue<TmphBufferIndex, TmphBufferIndex>>(sizeof(int));

        /// <summary>
        ///     AJAX回调函数名称
        /// </summary>
        private TmphBufferIndex ajaxCallBackName;

        /// <summary>
        ///     AJAX调用函数名称
        /// </summary>
        private TmphBufferIndex ajaxCallName;

        /// <summary>
        ///     提交数据分隔符
        /// </summary>
        private TmphBufferIndex boundary;

        /// <summary>
        ///     HTTP请求头部缓冲区
        /// </summary>
        internal byte[] TmphBuffer;

        /// <summary>
        ///     HTTP请求内容类型
        /// </summary>
        private TmphBufferIndex contentType;

        /// <summary>
        ///     Cookie
        /// </summary>
        private TmphBufferIndex cookie;

        /// <summary>
        ///     结束位置
        /// </summary>
        internal int EndIndex;

        /// <summary>
        ///     HTTP头部名称数据
        /// </summary>
        internal int HeaderCount;

        /// <summary>
        ///     请求域名
        /// </summary>
        private TmphBufferIndex host;

        /// <summary>
        ///     客户端文档时间标识
        /// </summary>
        private TmphBufferIndex ifModifiedSince;

        /// <summary>
        ///     客户端缓存有效标识
        /// </summary>
        private TmphBufferIndex ifNoneMatch;

        /// <summary>
        ///     是否100 Continue确认
        /// </summary>
        internal bool Is100Continue;

        /// <summary>
        ///     连接是否升级协议
        /// </summary>
        private byte isConnectionUpgrade;

        /// <summary>
        ///     是否已经格式化请求范围
        /// </summary>
        internal bool IsFormatRange;

        ///// <summary>
        ///// 是否google搜索引擎
        ///// </summary>
        //public bool IsGoogleQuery
        //{
        //    get { return GoogleQuery.Count != 0; }
        //}
        /// <summary>
        ///     URL中是否包含#
        /// </summary>
        private bool? isHash;

        /// <summary>
        ///     是否需要保持连接
        /// </summary>
        internal bool IsKeepAlive;

        /// <summary>
        ///     AJAX调用函数名称是否小写
        /// </summary>
        private bool isLowerAjaxCallName;

        /// <summary>
        ///     是否小写路径
        /// </summary>
        private bool isLowerPath;

        /// <summary>
        ///     判断来源页是否合法
        /// </summary>
        private bool? isReferer;

        /// <summary>
        ///     是否搜索引擎
        /// </summary>
        private bool? isSearchEngine;

        /// <summary>
        ///     升级协议是否支持WebSocket
        /// </summary>
        private byte isUpgradeWebSocket;

        /// <summary>
        ///     访问来源
        /// </summary>
        private TmphBufferIndex origin;

        /// <summary>
        ///     请求路径
        /// </summary>
        private TmphBufferIndex path;

        /// <summary>
        ///     Json字符串
        /// </summary>
        private TmphBufferIndex queryJson;

        /// <summary>
        ///     访问来源
        /// </summary>
        private TmphBufferIndex referer;

        /// <summary>
        ///     WebSocket确认连接值
        /// </summary>
        private TmphBufferIndex secWebSocketKey;

        /// <summary>
        ///     请求URI
        /// </summary>
        private TmphBufferIndex uri;

        /// <summary>
        ///     浏览器参数
        /// </summary>
        private TmphBufferIndex userAgent;

        /// <summary>
        ///     WebSocket数据
        /// </summary>
        internal TmphSubString WebSocketData;

        /// <summary>
        ///     转发信息
        /// </summary>
        private TmphBufferIndex xProwardedFor;

        static TmphRequestHeader()
        {
            var parseList = new TmphList<TmphKeyValue<TmphHeaderName, Action<TmphRequestHeader, TmphBufferIndex>>>();
            parseList.Add(new TmphKeyValue<TmphHeaderName, Action<TmphRequestHeader, TmphBufferIndex>>(TmphHeader.HostBytes,
                parseHost));
            parseList.Add(new TmphKeyValue<TmphHeaderName, Action<TmphRequestHeader, TmphBufferIndex>>(
                TmphHeader.ContentLengthBytes, parseContentLength));
            parseList.Add(new TmphKeyValue<TmphHeaderName, Action<TmphRequestHeader, TmphBufferIndex>>(
                TmphHeader.AcceptEncodingBytes, parseAcceptEncoding));
            parseList.Add(new TmphKeyValue<TmphHeaderName, Action<TmphRequestHeader, TmphBufferIndex>>(TmphHeader.ConnectionBytes,
                parseConnection));
            parseList.Add(new TmphKeyValue<TmphHeaderName, Action<TmphRequestHeader, TmphBufferIndex>>(TmphHeader.ContentTypeBytes,
                parseContentType));
            parseList.Add(new TmphKeyValue<TmphHeaderName, Action<TmphRequestHeader, TmphBufferIndex>>(TmphHeader.CookieBytes,
                parseCookie));
            parseList.Add(new TmphKeyValue<TmphHeaderName, Action<TmphRequestHeader, TmphBufferIndex>>(TmphHeader.RefererBytes,
                parseReferer));
            parseList.Add(new TmphKeyValue<TmphHeaderName, Action<TmphRequestHeader, TmphBufferIndex>>(TmphHeader.RangeBytes,
                parseRange));
            parseList.Add(new TmphKeyValue<TmphHeaderName, Action<TmphRequestHeader, TmphBufferIndex>>(TmphHeader.UserAgentBytes,
                parseUserAgent));
            parseList.Add(
                new TmphKeyValue<TmphHeaderName, Action<TmphRequestHeader, TmphBufferIndex>>(TmphHeader.IfModifiedSinceBytes,
                    parseIfModifiedSince));
            parseList.Add(new TmphKeyValue<TmphHeaderName, Action<TmphRequestHeader, TmphBufferIndex>>(TmphHeader.IfNoneMatchBytes,
                parseIfNoneMatch));
            parseList.Add(new TmphKeyValue<TmphHeaderName, Action<TmphRequestHeader, TmphBufferIndex>>(
                TmphHeader.XProwardedForBytes, parseXProwardedFor));
            parseList.Add(new TmphKeyValue<TmphHeaderName, Action<TmphRequestHeader, TmphBufferIndex>>(TmphHeader.ExpectBytes,
                parseExpect));
            parseList.Add(new TmphKeyValue<TmphHeaderName, Action<TmphRequestHeader, TmphBufferIndex>>(TmphHeader.UpgradeBytes,
                parseUpgrade));
            parseList.Add(
                new TmphKeyValue<TmphHeaderName, Action<TmphRequestHeader, TmphBufferIndex>>(TmphHeader.SecWebSocketKeyBytes,
                    parseSecWebSocketKey));
            parseList.Add(
                new TmphKeyValue<TmphHeaderName, Action<TmphRequestHeader, TmphBufferIndex>>(TmphHeader.SecWebSocketOriginBytes,
                    parseOrigin));
            parseList.Add(new TmphKeyValue<TmphHeaderName, Action<TmphRequestHeader, TmphBufferIndex>>(TmphHeader.OriginBytes,
                parseOrigin));
            parses = new TmphUniqueDictionary<TmphHeaderName, Action<TmphRequestHeader, TmphBufferIndex>>(parseList, 32);

            searchEngines = new TmphTrieGraph();
            searchEngines.BuildTree(new[]
            {
                ("Googlebot").GetBytes(),
                ("spider").GetBytes(),
                //fastCSharpSpiderUserAgent.getBytes(),
                //("iaskspider").getBytes(),
                //("Sogou web spider").getBytes(),
                //("Sogou push spider").getBytes(),
                //("Baiduspider").getBytes(),
                //("Sosospider").getBytes(),
                //("yisouspider").getBytes(),
                ("msnbot").GetBytes(),
                ("YandexBot").GetBytes(),
                ("Mediapartners-Google").GetBytes(),
                ("YoudaoBot").GetBytes(),
                ("Yandex").GetBytes(),
                ("MJ12bot").GetBytes(),
                ("bingbot").GetBytes(),
                ("Yahoo! Slurp").GetBytes(),
                ("ia_archiver").GetBytes(),
                ("GeoHasher").GetBytes(),
                ("R6_CommentReader").GetBytes(),
                ("SiteBot").GetBytes(),
                ("DotBot").GetBytes(),
                ("Twiceler").GetBytes(),
                ("renren share slurp").GetBytes()
            });
            searchEngines.BuildGraph();
        }

        /// <summary>
        ///     HTTP请求头
        /// </summary>
        public unsafe TmphRequestHeader()
        {
            TmphBuffer =
                new byte[
                    Config.TmphHttp.Default.HeaderBufferLength + sizeof(int) +
                    Config.TmphHttp.Default.MaxHeaderCount * sizeof(TmphBufferIndex) * 2];
        }

        /// <summary>
        ///     请求URI
        /// </summary>
        public TmphSubArray<byte> Uri
        {
            get { return TmphSubArray<byte>.Unsafe(TmphBuffer, uri.StartIndex, uri.Length); }
        }

        /// <summary>
        ///     请求路径
        /// </summary>
        public TmphSubArray<byte> Path
        {
            get { return TmphSubArray<byte>.Unsafe(TmphBuffer, path.StartIndex, path.Length); }
        }

        /// <summary>
        ///     请求路径是否需要做web视图路径转换
        /// </summary>
        public unsafe bool IsViewPath
        {
            get
            {
                fixed (byte* bufferFixed = TmphBuffer)
                {
                    if (path.Length > 1)
                    {
                        var start = bufferFixed + path.StartIndex;
                        if (*start == '/')
                        {
                            start += path.Length;
                            do
                            {
                                if (*--start == '/') return true;
                                if (*start == '.') return false;
                            } while (true);
                        }
                    }
                }
                return false;
            }
        }

        /// <summary>
        ///     小写路径
        /// </summary>
        internal unsafe TmphSubArray<byte> LowerPath
        {
            get
            {
                if (!isLowerPath)
                {
                    fixed (byte* bufferFixed = TmphBuffer)
                    {
                        var start = bufferFixed + path.StartIndex;
                        Unsafe.TmphMemory.ToLower(start, start + path.Length);
                    }
                    isLowerPath = true;
                }
                return TmphSubArray<byte>.Unsafe(TmphBuffer, path.StartIndex, path.Length);
            }
        }

        /// <summary>
        ///     判断来源页是否合法
        /// </summary>
        public bool IsReferer
        {
            get
            {
                if (isReferer == null)
                {
                    if (host.Length != 0)
                    {
                        var domain = default(TmphSubArray<byte>);
                        if (referer.Length != 0)
                        {
                            domain = Web.TmphDomain.GetMainDomainByUrl(Referer);
                        }
                        else if (origin.Length != 0)
                        {
                            domain =
                                Web.TmphDomain.GetMainDomainByUrl(TmphSubArray<byte>.Unsafe(TmphBuffer, origin.StartIndex,
                                    origin.Length));
                        }
                        if (domain.Array != null && domain.equal(Web.TmphDomain.GetMainDomain(Host))) isReferer = true;
                    }
                    if (isReferer == null) isReferer = false;
                }
                return (bool)isReferer;
            }
        }

        /// <summary>
        ///     请求域名
        /// </summary>
        public TmphSubArray<byte> Host
        {
            get { return TmphSubArray<byte>.Unsafe(TmphBuffer, host.StartIndex, host.Length); }
        }

        /// <summary>
        ///     提交数据分隔符
        /// </summary>
        public TmphSubArray<byte> Boundary
        {
            get { return TmphSubArray<byte>.Unsafe(TmphBuffer, boundary.StartIndex, boundary.Length); }
        }

        /// <summary>
        ///     访问来源
        /// </summary>
        public TmphSubArray<byte> Referer
        {
            get { return TmphSubArray<byte>.Unsafe(TmphBuffer, referer.StartIndex, referer.Length); }
        }

        /// <summary>
        ///     请求范围起始位置
        /// </summary>
        internal long RangeStart { get; private set; }

        /// <summary>
        ///     请求范围结束位置
        /// </summary>
        internal long RangeEnd { get; private set; }

        /// <summary>
        ///     请求范围长度
        /// </summary>
        internal long RangeLength
        {
            get { return RangeEnd - RangeStart + 1; }
        }

        /// <summary>
        ///     是否存在请求范围
        /// </summary>
        public bool IsRange
        {
            get { return RangeStart >= 0 || RangeEnd >= 0; }
        }

        /// <summary>
        ///     请求范围是否错误
        /// </summary>
        internal bool IsRangeError
        {
            get { return RangeStart > RangeEnd && RangeEnd != long.MinValue; }
        }

        /// <summary>
        ///     浏览器参数
        /// </summary>
        public TmphSubArray<byte> UserAgent
        {
            get { return TmphSubArray<byte>.Unsafe(TmphBuffer, userAgent.StartIndex, userAgent.Length); }
        }

        /// <summary>
        ///     客户端文档时间标识
        /// </summary>
        public TmphSubArray<byte> IfModifiedSince
        {
            get { return TmphSubArray<byte>.Unsafe(TmphBuffer, ifModifiedSince.StartIndex, ifModifiedSince.Length); }
        }

        /// <summary>
        ///     客户端缓存有效标识
        /// </summary>
        public TmphSubArray<byte> IfNoneMatch
        {
            get { return TmphSubArray<byte>.Unsafe(TmphBuffer, ifNoneMatch.StartIndex, ifNoneMatch.Length); }
        }

        /// <summary>
        ///     AJAX调用函数名称
        /// </summary>
        internal TmphSubArray<byte> AjaxCallName
        {
            get { return TmphSubArray<byte>.Unsafe(TmphBuffer, ajaxCallName.StartIndex, ajaxCallName.Length); }
        }

        /// <summary>
        ///     AJAX调用函数名称
        /// </summary>
        internal unsafe TmphSubArray<byte> LowerAjaxCallName
        {
            get
            {
                if (!isLowerAjaxCallName)
                {
                    fixed (byte* bufferFixed = TmphBuffer)
                    {
                        var start = bufferFixed + ajaxCallName.StartIndex;
                        Unsafe.TmphMemory.ToLower(start, start + ajaxCallName.Length);
                    }
                    isLowerAjaxCallName = true;
                }
                return TmphSubArray<byte>.Unsafe(TmphBuffer, ajaxCallName.StartIndex, ajaxCallName.Length);
            }
        }

        /// <summary>
        ///     AJAX回调函数名称
        /// </summary>
        internal TmphSubArray<byte> AjaxCallBackName
        {
            get { return TmphSubArray<byte>.Unsafe(TmphBuffer, ajaxCallBackName.StartIndex, ajaxCallBackName.Length); }
        }

        /// <summary>
        ///     Json字符串
        /// </summary>
        internal TmphSubString QueryJson
        {
            get
            {
                if (queryJson.Length != 0)
                {
                    return
                        TmphFormQuery.JavascriptUnescape(TmphSubArray<byte>.Unsafe(TmphBuffer, queryJson.StartIndex,
                            queryJson.Length));
                }
                return default(TmphSubString);
            }
        }

        /// <summary>
        ///     是否重新加载视图
        /// </summary>
        public bool IsReView { get; private set; }

        /// <summary>
        ///     请求内存字节长度,int.MinValue表示未知,-1表示错误
        /// </summary>
        public int ContentLength { get; private set; }

        /// <summary>
        ///     JSON编码
        /// </summary>
        internal Encoding JsonEncoding { get; private set; }

        /// <summary>
        ///     查询模式类型
        /// </summary>
        public TmphHttp.TmphMethodType Method { get; internal set; }

        /// <summary>
        ///     提交数据类型
        /// </summary>
        internal TmphPostType PostType { get; private set; }

        /// <summary>
        ///     是否WebSocket连接
        /// </summary>
        internal bool IsWebSocket
        {
            get { return (isConnectionUpgrade & isUpgradeWebSocket) != 0 && secWebSocketKey.Length != 0; }
        }

        /// <summary>
        ///     WebSocket确认连接值
        /// </summary>
        public TmphSubArray<byte> SecWebSocketKey
        {
            get { return TmphSubArray<byte>.Unsafe(TmphBuffer, secWebSocketKey.StartIndex, secWebSocketKey.Length); }
        }

        /// <summary>
        ///     客户端是否支持GZip压缩
        /// </summary>
        internal bool IsGZip { get; set; }

        /// <summary>
        ///     HTTP头部是否存在解析错误
        /// </summary>
        internal bool IsHeaderError { get; private set; }

        /// <summary>
        ///     URL中是否包含#
        /// </summary>
        public bool IsHash
        {
            get
            {
                if (isHash == null) isHash = false;
                return isHash.Value;
            }
        }

        /// <summary>
        ///     是否搜索引擎
        /// </summary>
        public bool IsSearchEngine
        {
            get
            {
                if (isSearchEngine == null)
                {
                    isSearchEngine = userAgent.Length != 0 &&
                                     searchEngines.IsMatchLess(TmphBuffer, userAgent.StartIndex, userAgent.Length);
                }
                return isSearchEngine.Value;
            }
        }

        /// <summary>
        ///     格式化请求范围
        /// </summary>
        /// <param name="contentLength">内容字节长度</param>
        /// <returns>范围是否有效</returns>
        public bool FormatRange(long contentLength)
        {
            IsFormatRange = true;
            if (RangeStart == 0)
            {
                if (RangeEnd >= contentLength - 1 || RangeEnd < 0) RangeStart = RangeEnd = long.MinValue;
            }
            else if (RangeStart > 0)
            {
                if (RangeStart >= contentLength || (ulong)RangeEnd < (ulong)RangeStart) return false;
                if (RangeEnd >= contentLength || RangeEnd < 0) RangeEnd = contentLength - 1;
            }
            else if (RangeEnd >= 0)
            {
                if (RangeEnd < contentLength) RangeStart = 0;
                else RangeEnd = long.MinValue;
            }
            return true;
        }

        /// <summary>
        ///     HTTP头部解析
        /// </summary>
        /// <param name="headerEndIndex">HTTP头部数据结束位置</param>
        /// <param name="receiveEndIndex">HTTP缓冲区接收数据结束位置</param>
        /// <returns>是否成功</returns>
        internal unsafe bool Parse(int headerEndIndex, int receiveEndIndex)
        {
            host.Null();
            IsGZip = false;
            try
            {
                EndIndex = headerEndIndex;
                fixed (byte* bufferFixed = TmphBuffer)
                {
                    if ((Method = TmphHttp.GetMethod(bufferFixed)) == TmphHttp.TmphMethodType.None) return false;
                    byte* current = bufferFixed, end = bufferFixed + headerEndIndex;
                    for (*end = 32; *current != 32; ++current) ;
                    *end = 13;
                    if (current == end) return false;
                    while (*++current == 32) ;
                    if (current == end) return false;
                    var start = current;
                    while (*current != 32 && *current != 13) ++current;
                    uri.Set(start - bufferFixed, current - start);
                    if (uri.Length == 0) return false;
                    while (*current != 13) ++current;

                    var headerIndex = bufferFixed + Config.TmphHttp.Default.HeaderBufferLength + sizeof(int);
                    HeaderCount = ContentLength = 0;
                    cookie.Null();
                    boundary.Null();
                    contentType.Null();
                    referer.Null();
                    userAgent.Null();
                    ifModifiedSince.Null();
                    ifNoneMatch.Null();
                    xProwardedFor.Null();
                    secWebSocketKey.Null();
                    WebSocketData.Null();
                    JsonEncoding = null;
                    isReferer = null;
                    PostType = TmphPostType.None;
                    RangeStart = RangeEnd = long.MinValue;
                    IsFormatRange = false;
                    Is100Continue = IsHeaderError = false;
                    isUpgradeWebSocket = isConnectionUpgrade = 0;
                    while (current != end)
                    {
                        if ((current += 2) >= end) return false;
                        for (start = current, *end = (byte)':'; *current != (byte)':'; ++current) ;
                        var name = TmphSubArray<byte>.Unsafe(TmphBuffer, (int)(start - bufferFixed), (int)(current - start));
                        *end = 13;
                        if (current == end || *++current != ' ') return false;
                        for (start = ++current; *current != 13; ++current) ;
                        var parseHeaderName = parses.Get(name, null);
                        if (parseHeaderName != null)
                            parseHeaderName(this,
                                new TmphBufferIndex
                                {
                                    StartIndex = (short)(start - bufferFixed),
                                    Length = (short)(current - start)
                                });
                        else if (HeaderCount == Config.TmphHttp.Default.MaxHeaderCount)
                        {
                            IsHeaderError = true;
                            break;
                        }
                        else
                        {
                            (*(TmphBufferIndex*)headerIndex).Set(name.StartIndex, name.Count);
                            (*(TmphBufferIndex*)(headerIndex + sizeof(TmphBufferIndex))).Set(start - bufferFixed,
                                current - start);
                            ++HeaderCount;
                            headerIndex += sizeof(TmphBufferIndex) * 2;
                        }
                    }
                    if (host.Length == 0 || ContentLength < 0 ||
                        (IsWebSocket && (IsGZip || Method != TmphHttp.TmphMethodType.GET || ifModifiedSince.Length != 0)))
                        return false;

                    if (contentType.Length != 0)
                    {
                        start = bufferFixed + contentType.StartIndex;
                        end = start + contentType.Length;
                        current = Unsafe.TmphMemory.Find(start, end, (byte)';');
                        var length = current == null ? contentType.Length : (int)(current - start);
                        if (length == 33)
                        {
                            //application/x-www-form-urlencoded
                            if ((((*(int*)start | 0x20202020) ^ ('a' | ('p' << 8) | ('p' << 16) | ('l' << 24)))
                                 |
                                 ((*(int*)(start + sizeof(int)) | 0x20202020) ^
                                  ('i' | ('c' << 8) | ('a' << 16) | ('t' << 24)))
                                 |
                                 ((*(int*)(start + sizeof(int) * 2) | 0x00202020) ^
                                  ('i' | ('o' << 8) | ('n' << 16) | ('/' << 24)))
                                 |
                                 ((*(int*)(start + sizeof(int) * 3) | 0x20200020) ^
                                  ('x' | ('-' << 8) | ('w' << 16) | ('w' << 24)))
                                 |
                                 ((*(int*)(start + sizeof(int) * 4) | 0x20200020) ^
                                  ('w' | ('-' << 8) | ('f' << 16) | ('o' << 24)))
                                 |
                                 ((*(int*)(start + sizeof(int) * 5) | 0x20002020) ^
                                  ('r' | ('m' << 8) | ('-' << 16) | ('u' << 24)))
                                 |
                                 ((*(int*)(start + sizeof(int) * 6) | 0x20202020) ^
                                  ('r' | ('l' << 8) | ('e' << 16) | ('n' << 24)))
                                 |
                                 ((*(int*)(start + sizeof(int) * 7) | 0x20202020) ^
                                  ('c' | ('o' << 8) | ('d' << 16) | ('e' << 24)))
                                 | ((*(start + sizeof(int) * 8) | 0x20) ^ 'd')) == 0)
                            {
                                PostType = TmphPostType.Form;
                            }
                        }
                        else if (length == 16)
                        {
                            //application/json; charset=utf-8
                            if ((((*(int*)start | 0x20202020) ^ ('a' | ('p' << 8) | ('p' << 16) | ('l' << 24)))
                                 |
                                 ((*(int*)(start + sizeof(int)) | 0x20202020) ^
                                  ('i' | ('c' << 8) | ('a' << 16) | ('t' << 24)))
                                 |
                                 ((*(int*)(start + sizeof(int) * 2) | 0x00202020) ^
                                  ('i' | ('o' << 8) | ('n' << 16) | ('/' << 24)))
                                 |
                                 ((*(int*)(start + sizeof(int) * 3) | 0x20202020) ^
                                  ('j' | ('s' << 8) | ('o' << 16) | ('n' << 24)))) == 0)
                            {
                                *end = (byte)'c';
                                if (*(start += 16) == ';')
                                {
                                    while (*++start != 'c') ;
                                    if (start != end &&
                                        (((*(int*)start | 0x20202020) ^ ('c' | ('h' << 8) | ('a' << 16) | ('r' << 24)))
                                         |
                                         ((*(int*)(start + sizeof(int)) | 0x00202020) ^
                                          ('s' | ('e' << 8) | ('t' << 16) | ('=' << 24)))) == 0)
                                    {
                                        switch ((byte)(*(start + 10) | 0x20))
                                        {
                                            case (byte)'2': //gb2312
                                                if (*(int*)(start + 10) ==
                                                    ('2' | ('3' << 8) | ('1' << 16) | ('2' << 24)))
                                                    JsonEncoding = TmphPub.Gb2312;
                                                break;

                                            case (byte)'f': //utf-8
                                                if ((*(int*)(start + 9) | 0x2020) ==
                                                    ('t' | ('f' << 8) | ('-' << 16) | ('8' << 24)))
                                                    JsonEncoding = Encoding.UTF8;
                                                break;

                                            case (byte)'k': //gbk
                                                if ((*(int*)(start + 7) | 0x20202000) ==
                                                    ('=' | ('g' << 8) | ('b' << 16) | ('k' << 24)))
                                                    JsonEncoding = TmphPub.Gbk;
                                                break;

                                            case (byte)'g': //big5
                                                if ((*(int*)(start + 8) | 0x00202020) ==
                                                    ('b' | ('i' << 8) | ('g' << 16) | ('5' << 24)))
                                                    JsonEncoding = TmphPub.Big5;
                                                break;

                                            case (byte)'1': //gb18030
                                                if (*(int*)(start + 11) ==
                                                    ('8' | ('0' << 8) | ('3' << 16) | ('0' << 24)))
                                                    JsonEncoding = TmphPub.Gb18030;
                                                break;

                                            case (byte)'i': //unicode
                                                if ((*(int*)(start + 11) | 0x20202020) ==
                                                    ('c' | ('o' << 8) | ('d' << 16) | ('e' << 24)))
                                                    JsonEncoding = Encoding.Unicode;
                                                break;
                                        }
                                    }
                                }
                                *end = 13;
                                if (JsonEncoding == null) JsonEncoding = Encoding.UTF8;
                                PostType = TmphPostType.Json;
                            }
                        }
                        else if (length == 19 && contentType.Length > 30)
                        {
                            //multipart/form-data; boundary=---------------------------7dc2e63860144
                            if ((((*(int*)start | 0x20202020) ^ ('m' | ('u' << 8) | ('l' << 16) | ('t' << 24)))
                                 |
                                 ((*(int*)(start + sizeof(int)) | 0x20202020) ^
                                  ('i' | ('p' << 8) | ('a' << 16) | ('r' << 24)))
                                 |
                                 ((*(int*)(start + sizeof(int) * 2) | 0x20200020) ^
                                  ('t' | ('/' << 8) | ('f' << 16) | ('o' << 24)))
                                 |
                                 ((*(int*)(start + sizeof(int) * 3) | 0x20002020) ^
                                  ('r' | ('m' << 8) | ('-' << 16) | ('d' << 24)))
                                 |
                                 ((*(int*)(start + sizeof(int) * 4) | 0x00202020) ^
                                  ('a' | ('t' << 8) | ('a' << 16) | (';' << 24)))
                                 |
                                 ((*(int*)(start + sizeof(int) * 5) | 0x20202000) ^
                                  (' ' | ('b' << 8) | ('o' << 16) | ('u' << 24)))
                                 |
                                 ((*(int*)(start + sizeof(int) * 6) | 0x20202020) ^
                                  ('n' | ('d' << 8) | ('a' << 16) | ('r' << 24)))
                                 | ((*(short*)(start += sizeof(int) * 7) | 0x20) ^ ('y' | ('=' << 8)))) == 0)
                            {
                                boundary.Set(contentType.StartIndex + sizeof(int) * 7 + 2,
                                    contentType.Length - (sizeof(int) * 7 + 2));
                                if (boundary.Length > maxBoundaryLength) IsHeaderError = true;
                                PostType = TmphPostType.FormData;
                            }
                        }
                    }

                    if (Method == TmphHttp.TmphMethodType.POST)
                    {
                        if (PostType == TmphPostType.None ||
                            ContentLength < (receiveEndIndex - headerEndIndex - sizeof(int)))
                            return false;
                    }
                    else
                    {
                        if (PostType != TmphPostType.None ||
                            (!IsKeepAlive && receiveEndIndex != headerEndIndex + sizeof(int)) ||
                            ((uint)ContentLength | (uint)boundary.Length) != 0)
                            return false;
                    }
                    if (!IsHeaderError && !IsRangeError)
                    {
                        queryIndexs.Empty();
                        ajaxCallName.Null();
                        ajaxCallBackName.Null();
                        queryJson.Null();
                        IsReView = isLowerAjaxCallName = false;
                        if (IsWebSocket)
                        {
                            isSearchEngine = isHash = false;
                        }
                        else
                        {
                            isSearchEngine = isHash = null;
                            start = bufferFixed + uri.StartIndex;
                            end = Unsafe.TmphMemory.Find(start, start + uri.Length, (byte)'?');
                            if (end == null)
                            {
                                end = Unsafe.TmphMemory.Find(start, start + uri.Length, (byte)'#');
                                if (end != null) isSearchEngine = isHash = true;
                            }
                            else if (*(end + 1) == '_')
                            {
                                fixed (byte* googleFixed = googleFragmentName)
                                {
                                    if (Unsafe.TmphMemory.Equal(googleFixed, end + 2, googleFragmentName.Length))
                                    {
                                        isSearchEngine = isHash = true;
                                        byte* write = end + 1, urlEnd = start + uri.Length;
                                        current = write + googleFragmentName.Length + 1;
                                        var endValue = *urlEnd;
                                        *urlEnd = (byte)'%';
                                        do
                                        {
                                            while (*current != '%') *write++ = *current++;
                                            if (current == urlEnd) break;
                                            uint code = (uint)(*++current - '0'), number = (uint)(*++current - '0');
                                            if (code > 9) code = ((code - ('A' - '0')) & 0xffdfU) + 10;
                                            code = (number > 9 ? (((number - ('A' - '0')) & 0xffdfU) + 10) : number) +
                                                   (code << 4);
                                            *write++ = code == 0 ? (byte)' ' : (byte)code;
                                        } while (++current < urlEnd);
                                        *urlEnd = endValue;
                                        uri.Length = (short)((int)(write - bufferFixed) - uri.StartIndex);
                                    }
                                }
                            }
                            isLowerPath = false;
                            if (end == null) path = uri;
                            else
                            {
                                path.Set(uri.StartIndex, (short)(end - start));
                                TmphBufferIndex nameIndex, valueIndex = new TmphBufferIndex();
                                current = end;
                                var endValue = *(end = start + uri.Length);
                                *end = (byte)'&';
                                if (isHash != null)
                                {
                                    if (*current == '!') ++current;
                                    else if (*current == '%' && *(short*)(current + 1) == '2' + ('1' << 8))
                                        current += 3;
                                }
                                do
                                {
                                    nameIndex.StartIndex = (short)(++current - bufferFixed);
                                    while (*current != '&' && *current != '=') ++current;
                                    nameIndex.Length = (short)((int)(current - bufferFixed) - nameIndex.StartIndex);
                                    if (*current == '=')
                                    {
                                        valueIndex.StartIndex = (short)(++current - bufferFixed);
                                        while (*current != '&') ++current;
                                        valueIndex.Length =
                                            (short)((int)(current - bufferFixed) - valueIndex.StartIndex);
                                    }
                                    else valueIndex.Null();
                                    if (nameIndex.Length == 1)
                                    {
                                        var name = (char)*(bufferFixed + nameIndex.StartIndex);
                                        if (name == TmphWeb.Default.AjaxCallName) ajaxCallName = valueIndex;
                                        else if (name == TmphWeb.Default.AjaxCallBackName) ajaxCallBackName = valueIndex;
                                        else if (name == TmphWeb.Default.ReViewName) IsReView = true;
                                        else if (name == TmphWeb.Default.QueryJsonName) queryJson = valueIndex;
                                        else
                                            queryIndexs.Add(new TmphKeyValue<TmphBufferIndex, TmphBufferIndex>(nameIndex,
                                                valueIndex));
                                    }
                                    else
                                        queryIndexs.Add(new TmphKeyValue<TmphBufferIndex, TmphBufferIndex>(nameIndex, valueIndex));
                                } while (current != end);
                                *end = endValue;
                            }
                        }
                    }
                    return true;
                }
            }
            catch (Exception error)
            {
                TmphLog.Default.Add(error, null, false);
            }
            return false;
        }

        /// <summary>
        ///     WebSocket重置URL
        /// </summary>
        /// <param name="start">起始位置</param>
        /// <param name="end">结束位置</param>
        /// <returns>是否成功</returns>
        internal unsafe bool SetWebSocketUrl(byte* start, byte* end)
        {
            var length = (int)(end - start);
            if (EndIndex + length <= Config.TmphHttp.Default.HeaderBufferLength)
            {
                fixed (byte* bufferFixed = TmphBuffer)
                {
                    Unsafe.TmphMemory.Copy(start, bufferFixed + EndIndex, length);
                    start = bufferFixed + EndIndex;
                    uri.Set(EndIndex, length);
                    var current = start;
                    for (*(end = start + length) = (byte)'?'; *current != '?'; ++current) ;
                    queryIndexs.Empty();
                    if (current == end) path = uri;
                    else
                    {
                        path.Set(EndIndex, (short)(current - start));
                        TmphBufferIndex nameIndex, valueIndex;
                        *end = (byte)'&';
                        do
                        {
                            nameIndex.StartIndex = (short)(++current - bufferFixed);
                            while (*current != '&' && *current != '=') ++current;
                            nameIndex.Length = (short)((int)(current - bufferFixed) - nameIndex.StartIndex);
                            if (*current == '=')
                            {
                                valueIndex.StartIndex = (short)(++current - bufferFixed);
                                while (*current != '&') ++current;
                                valueIndex.Length = (short)((int)(current - bufferFixed) - valueIndex.StartIndex);
                                queryIndexs.Add(new TmphKeyValue<TmphBufferIndex, TmphBufferIndex>(nameIndex, valueIndex));
                            }
                            else if (nameIndex.Length != 0)
                                queryIndexs.Add(new TmphKeyValue<TmphBufferIndex, TmphBufferIndex>(nameIndex,
                                    new TmphBufferIndex()));
                        } while (current != end);
                    }
                }
                return true;
            }
            return false;
        }

        /// <summary>
        ///     判断是否存在Cookie值
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns>是否存在Cookie值</returns>
        internal unsafe bool IsCookie(byte[] name)
        {
            if (cookie.Length > name.Length)
            {
                fixed (byte* nameFixed = name)
                {
                    return getCookie(nameFixed, name.Length).StartIndex == 0;
                }
            }
            return false;
        }

        /// <summary>
        ///     获取Cookie值
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns>值</returns>
        internal unsafe string GetCookieString(byte[] name)
        {
            if (cookie.Length > name.Length)
            {
                fixed (byte* nameFixed = name)
                {
                    var index = getCookie(nameFixed, name.Length);
                    if (index.StartIndex != 0)
                    {
                        if (index.Length == 0) return string.Empty;
                        fixed (byte* bufferFixed = TmphBuffer)
                            return TmphString.DeSerialize(bufferFixed + index.StartIndex, -index.Length);
                    }
                }
            }
            return null;
        }

        /// <summary>
        ///     获取Cookie值
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns>值</returns>
        internal unsafe TmphSubArray<byte> GetCookie(byte[] name)
        {
            if (cookie.Length > name.Length)
            {
                fixed (byte* nameFixed = name)
                {
                    var index = getCookie(nameFixed, name.Length);
                    if (index.StartIndex != 0) return TmphSubArray<byte>.Unsafe(TmphBuffer, index.StartIndex, index.Length);
                }
            }
            return default(TmphSubArray<byte>);
        }

        /// <summary>
        ///     获取Cookie值
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns>值</returns>
        public unsafe string GetCookie(string name)
        {
            if (cookie.Length > name.Length && name.Length <= TmphUnmanagedStreamBase.DefaultLength)
            {
                TmphBufferIndex index;
                fixed (char* nameFixed = name)
                {
                    var cookieNameBuffer = TmphUnmanagedPool.TinyBuffers.Get();
                    Unsafe.TmphString.WriteBytes(nameFixed, name.Length, cookieNameBuffer.Byte);
                    index = getCookie(cookieNameBuffer.Byte, name.Length);
                    TmphUnmanagedPool.TinyBuffers.Push(ref cookieNameBuffer);
                }
                if (index.StartIndex != 0)
                {
                    if (index.Length == 0) return string.Empty;
                    fixed (byte* bufferFixed = TmphBuffer)
                        return TmphString.DeSerialize(bufferFixed + index.StartIndex, -index.Length);
                }
            }
            return null;
        }

        /// <summary>
        ///     获取Cookie值
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="nameLength">名称长度</param>
        /// <returns>值</returns>
        private unsafe TmphBufferIndex getCookie(byte* name, int nameLength)
        {
            fixed (byte* bufferFixed = TmphBuffer)
            {
                byte* start = bufferFixed + cookie.StartIndex, end = start + cookie.Length, searchEnd = end - nameLength;
                *end = (byte)';';
                do
                {
                    while (*start == ' ') ++start;
                    if (start >= searchEnd) break;
                    if (*(start + nameLength) == '=')
                    {
                        if (Unsafe.TmphMemory.Equal(name, start, nameLength))
                        {
                            for (start += nameLength + 1; *start == ' '; ++start) ;
                            var startIndex = (int)(start - bufferFixed);
                            while (*start != ';') ++start;
                            return new TmphBufferIndex
                            {
                                StartIndex = (short)startIndex,
                                Length = (short)((int)(start - bufferFixed) - startIndex)
                            };
                        }
                        start += nameLength + 1;
                    }
                    while (*start != ';') ++start;
                } while (++start < searchEnd);
            }
            return default(TmphBufferIndex);
        }

        /// <summary>
        ///     获取查询值
        /// </summary>
        /// <param name="name">查询名称</param>
        /// <returns>查询值</returns>
        private unsafe TmphSubArray<byte> getQuery(byte[] name)
        {
            var count = queryIndexs.Count;
            if (count != 0)
            {
                fixed (byte* bufferFixed = TmphBuffer)
                {
                    foreach (var index in queryIndexs.array)
                    {
                        if (index.Key.Length == name.Length
                            && Unsafe.TmphMemory.Equal(name, bufferFixed + index.Key.StartIndex, name.Length))
                        {
                            return TmphSubArray<byte>.Unsafe(TmphBuffer, index.Value.StartIndex, index.Value.Length);
                        }
                        if (--count == 0) break;
                    }
                }
            }
            return default(TmphSubArray<byte>);
        }

        /// <summary>
        ///     获取查询整数值
        /// </summary>
        /// <param name="name"></param>
        /// <param name="nullValue"></param>
        /// <returns></returns>
        public unsafe int GetQueryInt(byte[] name, int nullValue)
        {
            var value = getQuery(name);
            if (value.Count != 0)
            {
                var intValue = 0;
                fixed (byte* bufferFixed = TmphBuffer)
                {
                    byte* start = bufferFixed + value.StartIndex, end = start + value.Count;
                    for (intValue = *(start) - '0'; ++start != end; intValue += *(start) - '0') intValue *= 10;
                }
                return intValue;
            }
            return nullValue;
        }

        /// <summary>
        ///     查询解析
        /// </summary>
        /// <typeparam name="TValueType">目标类型</typeparam>
        /// <param name="value">目标数据</param>
        /// <returns>是否解析成功</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal bool ParseQuery<TValueType>(ref TValueType value)
        {
            return TmphQueryParser.Parse(this, ref value);
        }

        /// <summary>
        ///     请求域名解析
        /// </summary>
        /// <param name="socket">HTTP头部接收器</param>
        /// <param name="value">请求域名索引位置</param>
        private static void parseHost(TmphRequestHeader header, TmphBufferIndex value)
        {
            header.host = value;
        }

        /// <summary>
        ///     提交内容数据长度解析
        /// </summary>
        /// <param name="socket">HTTP头部接收器</param>
        /// <param name="value">提交内容数据长度索引位置</param>
        private static unsafe void parseContentLength(TmphRequestHeader header, TmphBufferIndex value)
        {
            fixed (byte* dataFixed = header.TmphBuffer)
            {
                for (byte* start = dataFixed + value.StartIndex, end = start + value.Length; start != end; ++start)
                {
                    header.ContentLength *= 10;
                    header.ContentLength += *start - '0';
                }
            }
        }

        /// <summary>
        ///     内容数据编码方式解析
        /// </summary>
        /// <param name="socket">HTTP头部接收器</param>
        /// <param name="value">内容数据编码方式索引位置</param>
        private static unsafe void parseAcceptEncoding(TmphRequestHeader header, TmphBufferIndex value)
        {
            if (value.Length >= 4)
            {
                fixed (byte* dataFixed = header.TmphBuffer)
                {
                    byte* start = dataFixed + value.StartIndex, end = start + value.Length;
                    var endValue = *end;
                    *end = (byte)'g';
                    while (true)
                    {
                        while (*start != 'g') ++start;
                        if (start != end)
                        {
                            if ((*(int*)start | 0x20202020) == ('g' | ('z' << 8) | ('i' << 16) | ('p' << 24)))
                            {
                                header.IsGZip = true;
                                break;
                            }
                            ++start;
                        }
                        else break;
                    }
                    *end = endValue;
                }
            }
        }

        /// <summary>
        ///     保持连接解析
        /// </summary>
        /// <param name="socket">HTTP头部接收器</param>
        /// <param name="value">保持连接索引位置</param>
        private static unsafe void parseConnection(TmphRequestHeader header, TmphBufferIndex value)
        {
            if (value.Length == 10)
            {
                fixed (byte* dataFixed = header.TmphBuffer)
                {
                    var start = dataFixed + value.StartIndex;
                    if ((((*(int*)start | 0x20202020) ^ ('k' | ('e' << 8) | ('e' << 16) | ('p' << 24)))
                         |
                         ((*(int*)(start + sizeof(int)) | 0x20202000) ^ ('-' | ('a' << 8) | ('l' << 16) | ('i' << 24)))
                         | ((*(short*)(start + sizeof(int) * 2) | 0x2020) ^ ('v' | ('e' << 8)))) == 0)
                    {
                        header.IsKeepAlive = true;
                    }
                }
            }
            else if (value.Length == 5)
            {
                fixed (byte* dataFixed = header.TmphBuffer)
                {
                    var start = dataFixed + value.StartIndex;
                    if ((((*(int*)start | 0x20202020) ^ ('c' | ('l' << 8) | ('o' << 16) | ('s' << 24)))
                         | ((*(start + sizeof(int)) | 0x20) ^ 'e')) == 0)
                    {
                        header.IsKeepAlive = false;
                    }
                }
            }
            else if (value.Length == 7)
            {
                fixed (byte* dataFixed = header.TmphBuffer)
                {
                    var start = dataFixed + value.StartIndex;
                    if ((((*(int*)start | 0x20202020) ^ ('u' | ('p' << 8) | ('g' << 16) | ('r' << 24)))
                         | (((*(int*)(start + sizeof(int)) | 0x202020) & 0xffffff) ^ ('a' | ('d' << 8) | ('e' << 16)))) ==
                        0)
                    {
                        header.isConnectionUpgrade = 1;
                    }
                }
            }
        }

        /// <summary>
        ///     HTTP请求内容类型解析
        /// </summary>
        /// <param name="header">HTTP头部接收器</param>
        /// <param name="value">HTTP请求内容类型索引位置</param>
        private static void parseContentType(TmphRequestHeader header, TmphBufferIndex value)
        {
            header.contentType = value;
        }

        /// <summary>
        ///     访问来源解析
        /// </summary>
        /// <param name="header">HTTP头部接收器</param>
        /// <param name="value">访问来源索引位置</param>
        private static void parseReferer(TmphRequestHeader header, TmphBufferIndex value)
        {
            header.referer = value;
        }

        /// <summary>
        ///     访问来源解析
        /// </summary>
        /// <param name="header">HTTP头部接收器</param>
        /// <param name="value">访问来源索引位置</param>
        private static void parseOrigin(TmphRequestHeader header, TmphBufferIndex value)
        {
            header.origin = value;
        }

        /// <summary>
        ///     请求字节范围解析
        /// </summary>
        /// <param name="header">HTTP头部接收器</param>
        /// <param name="value">请求字节范围索引位置</param>
        private static unsafe void parseRange(TmphRequestHeader header, TmphBufferIndex value)
        {
            if (value.Length > 6)
            {
                fixed (byte* dataFixed = header.TmphBuffer)
                {
                    var start = dataFixed + value.StartIndex;
                    if (((*(int*)start ^ ('b' + ('y' << 8) + ('t' << 16) + ('e' << 24))) |
                         (*(short*)(start + 4) ^ ('s' + ('=' << 8)))) == 0)
                    {
                        var end = start + value.Length;
                        if (*(start += 6) == '-')
                        {
                            long rangeEnd = 0;
                            while (++start != end)
                            {
                                rangeEnd *= 10;
                                rangeEnd += *start - '0';
                            }
                            header.RangeEnd = rangeEnd;
                        }
                        else
                        {
                            long rangeStart = 0;
                            do
                            {
                                var number = *start - '0';
                                if ((uint)number > 9) break;
                                rangeStart *= 10;
                                ++start;
                                rangeStart += number;
                            } while (true);
                            if (rangeStart >= 0 && *start == '-')
                            {
                                if (++start == end)
                                {
                                    header.RangeStart = rangeStart;
                                }
                                else
                                {
                                    long rangeEnd = *start - '0';
                                    while (++start != end)
                                    {
                                        rangeEnd *= 10;
                                        rangeEnd += *start - '0';
                                    }
                                    if (rangeEnd >= rangeStart)
                                    {
                                        header.RangeStart = rangeStart;
                                        header.RangeEnd = rangeEnd;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Cookie解析
        /// </summary>
        /// <param name="header">HTTP头部接收器</param>
        /// <param name="value">Cookie索引位置</param>
        private static void parseCookie(TmphRequestHeader header, TmphBufferIndex value)
        {
            header.cookie = value;
        }

        /// <summary>
        ///     浏览器参数解析
        /// </summary>
        /// <param name="header">HTTP头部接收器</param>
        /// <param name="value">浏览器参数索引位置</param>
        private static void parseUserAgent(TmphRequestHeader header, TmphBufferIndex value)
        {
            header.userAgent = value;
        }

        /// <summary>
        ///     客户端文档时间标识解析
        /// </summary>
        /// <param name="header">HTTP头部接收器</param>
        /// <param name="value">客户端文档时间标识索引位置</param>
        private static void parseIfModifiedSince(TmphRequestHeader header, TmphBufferIndex value)
        {
            header.ifModifiedSince = value;
        }

        /// <summary>
        ///     客户端缓存有效标识解析
        /// </summary>
        /// <param name="header">HTTP头部接收器</param>
        /// <param name="value">客户端缓存有效标识索引位置</param>
        private static unsafe void parseIfNoneMatch(TmphRequestHeader header, TmphBufferIndex value)
        {
            if (value.Length >= 2)
            {
                fixed (byte* dataFixed = header.TmphBuffer)
                {
                    var start = dataFixed + value.StartIndex;
                    if (*(start + value.Length - 1) == '"')
                    {
                        if (*start == '"') header.ifNoneMatch.Set(value.StartIndex + 1, value.Length - 2);
                        else if ((*(int*)start & 0xffffff) == ('W' + ('/' << 8) + ('"' << 16)) && value.Length >= 4)
                        {
                            header.ifNoneMatch.Set(value.StartIndex + 3, value.Length - 4);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     转发信息解析
        /// </summary>
        /// <param name="header">HTTP头部接收器</param>
        /// <param name="value">转发信息索引位置</param>
        private static void parseXProwardedFor(TmphRequestHeader header, TmphBufferIndex value)
        {
            header.xProwardedFor = value;
        }

        /// <summary>
        ///     100 Continue确认解析
        /// </summary>
        /// <param name="header">HTTP头部接收器</param>
        /// <param name="value">100 Continue确认索引位置</param>
        private static unsafe void parseExpect(TmphRequestHeader header, TmphBufferIndex value)
        {
            if (value.Length == 12)
            {
                fixed (byte* dataFixed = header.TmphBuffer)
                {
                    var start = dataFixed + value.StartIndex;
                    if (((*(int*)start ^ ('1' | ('0' << 8) | ('0' << 16) | ('-' << 24)))
                         |
                         ((*(int*)(start + sizeof(int)) | 0x20202020) ^ ('c' | ('o' << 8) | ('n' << 16) | ('t' << 24)))
                         |
                         ((*(int*)(start + sizeof(int) * 2) | 0x20202020) ^
                          ('i' | ('n' << 8) | ('u' << 16) | ('e' << 24)))) == 0)
                    {
                        header.Is100Continue = true;
                    }
                }
            }
        }

        /// <summary>
        ///     协议升级支持解析
        /// </summary>
        /// <param name="header">HTTP头部接收器</param>
        /// <param name="value">协议升级支持索引位置</param>
        private static unsafe void parseUpgrade(TmphRequestHeader header, TmphBufferIndex value)
        {
            if (value.Length == 9)
            {
                fixed (byte* dataFixed = header.TmphBuffer)
                {
                    var start = dataFixed + value.StartIndex;
                    if ((((*(int*)start | 0x20202020) ^ ('w' | ('e' << 8) | ('b' << 16) | ('s' << 24)))
                         |
                         ((*(int*)(start + sizeof(int)) | 0x20202020) ^ ('o' | ('c' << 8) | ('k' << 16) | ('e' << 24)))
                         | ((*(start + sizeof(int) * 2) | 0x20) ^ 't')) == 0)
                    {
                        header.isUpgradeWebSocket = 1;
                    }
                }
            }
        }

        /// <summary>
        ///     WebSocket确认连接值解析
        /// </summary>
        /// <param name="header">HTTP头部接收器</param>
        /// <param name="value">WebSocket确认连接值索引位置</param>
        private static void parseSecWebSocketKey(TmphRequestHeader header, TmphBufferIndex value)
        {
            if (value.Length <= 32) header.secWebSocketKey = value;
        }

        /// <summary>
        ///     HTTP头名称唯一哈希
        /// </summary>
        private struct TmphHeaderName : IEquatable<TmphHeaderName>
        {
            //string[] keys = new string[] { "host", "content-length", "accept-encoding", "connection", "content-type", "cookie", "referer", "range", "user-agent", "if-modified-since", "x-prowarded-for", "if-none-match", "expect", "upgrade", "origin", "sec-webSocket-key", "sec-webSocket-origin" };
            /// <summary>
            ///     HTTP头名称
            /// </summary>
            public TmphSubArray<byte> Name;

            /// <summary>
            ///     判断是否相等
            /// </summary>
            /// <param name="other">待匹配数据</param>
            /// <returns>是否相等</returns>
            public unsafe bool Equals(TmphHeaderName other)
            {
                if (Name.Count == other.Name.Count)
                {
                    fixed (byte* nameFixed = Name.Array, otherNameFixed = other.Name.Array)
                    {
                        return Unsafe.TmphMemory.EqualCase(nameFixed + Name.StartIndex,
                            otherNameFixed + other.Name.StartIndex, Name.Count);
                    }
                }
                return false;
            }

            /// <summary>
            ///     隐式转换
            /// </summary>
            /// <param name="name">HTTP头名称</param>
            /// <returns>HTTP头名称唯一哈希</returns>
            public static implicit operator TmphHeaderName(byte[] name)
            {
                return new TmphHeaderName { Name = TmphSubArray<byte>.Unsafe(name, 0, name.Length) };
            }

            /// <summary>
            ///     隐式转换
            /// </summary>
            /// <param name="name">HTTP头名称</param>
            /// <returns>HTTP头名称唯一哈希</returns>
            public static implicit operator TmphHeaderName(TmphSubArray<byte> name)
            {
                return new TmphHeaderName { Name = name };
            }

            /// <summary>
            ///     获取哈希值
            /// </summary>
            /// <returns>哈希值</returns>
            public override unsafe int GetHashCode()
            {
                fixed (byte* nameFixed = Name.Array)
                {
                    var start = nameFixed + Name.StartIndex;
                    return (((*(start + (Name.Count >> 1)) | 0x20) >> 2) ^ (*(start + Name.Count - 3) << 1)) &
                           ((1 << 5) - 1);
                }
            }

            /// <summary>
            ///     判断是否相等
            /// </summary>
            /// <param name="obj">待匹配数据</param>
            /// <returns>是否相等</returns>
            public override bool Equals(object obj)
            {
                return Equals((TmphHeaderName)obj);
            }
        }

        /// <summary>
        ///     搜索引擎标识trie图
        /// </summary>
        private sealed class TmphTrieGraph
        {
            /// <summary>
            ///     根节点
            /// </summary>
            private readonly TmphNode boot;

            /// <summary>
            ///     字节数组trie图
            /// </summary>
            public TmphTrieGraph()
            {
                (boot = new TmphNode()).Create();
            }

            /// <summary>
            ///     创建trie树
            /// </summary>
            /// <param name="keys">关键字集合</param>
            public void BuildTree(byte[][] keys)
            {
                var treeBuilder = new TmphTreeBuilder();
                foreach (var key in keys)
                {
                    treeBuilder.Node = boot;
                    treeBuilder.Build(key);
                }
            }

            /// <summary>
            ///     建图
            /// </summary>
            public void BuildGraph()
            {
                var builder = new TmphGraphBuilder();
                builder.Set(boot);
                var reader = boot.Nodes.GetFindList(node => node != null && node.Nodes != null);
                if (reader != null)
                {
                    while (reader.Count != 0)
                    {
                        builder.Build(reader.array, reader.Count);
                        var values = reader;
                        reader = builder.Writer;
                        builder.Writer = values;
                    }
                }
            }

            /// <summary>
            ///     是否存在最小匹配
            /// </summary>
            /// <param name="data">匹配数据</param>
            /// <param name="startIndex">匹配起始位置</param>
            /// <param name="length">匹配数据长度</param>
            /// <returns>是否存在最小匹配</returns>
            public unsafe bool IsMatchLess(byte[] data, int startIndex, int length)
            {
                fixed (byte* valueFixed = data)
                {
                    var start = valueFixed + startIndex;
                    return isMatchLess(start, start + length);
                }
            }

            /// <summary>
            ///     是否存在最小匹配
            /// </summary>
            /// <param name="start">匹配起始位置</param>
            /// <param name="end">匹配结束位置</param>
            /// <returns>是否存在最小匹配</returns>
            private unsafe bool isMatchLess(byte* start, byte* end)
            {
                for (TmphNode node = boot, nextNode = null; start != end; ++start)
                {
                    var letter = *start;
                    if (node.Nodes == null || (nextNode = node[letter]) == null)
                    {
                        do
                        {
                            if ((node = node.Link) == null) break;
                            if (node.Value != null) return true;
                        } while (node.Nodes == null || (nextNode = node[letter]) == null);
                        if (node == null && (nextNode = boot[letter]) == null) nextNode = boot;
                    }
                    if (nextNode.Value != null) return true;
                    node = nextNode;
                }
                return false;
            }

            /// <summary>
            ///     trie图节点
            /// </summary>
            private sealed class TmphNode
            {
                /// <summary>
                ///     关键字集合
                /// </summary>
                public TmphSubArray<byte> Keys;

                /// <summary>
                ///     失败节点
                /// </summary>
                public TmphNode Link;

                /// <summary>
                ///     节点集合
                /// </summary>
                public TmphNode[] Nodes;

                /// <summary>
                ///     节点值
                /// </summary>
                public byte[] Value;

                /// <summary>
                ///     获取节点
                /// </summary>
                /// <param name="key">关键字</param>
                /// <returns>节点</returns>
                public TmphNode this[byte key]
                {
                    get { return Nodes[KeyToIndex(key)]; }
                }

                /// <summary>
                ///     创建节点集合
                /// </summary>
                public void Create()
                {
                    Keys = new TmphSubArray<byte>(27);
                    Nodes = new TmphNode[27];
                }

                /// <summary>
                ///     创建子节点
                /// </summary>
                /// <param name="letter">当前字符</param>
                /// <returns>子节点</returns>
                public TmphNode Create(byte letter)
                {
                    if (Nodes == null)
                    {
                        Create();
                        var nextNode = new TmphNode();
                        Nodes[KeyToIndex(letter)] = nextNode;
                        Keys.UnsafeAdd(letter);
                        return nextNode;
                    }
                    else
                    {
                        var index = KeyToIndex(letter);
                        var nextNode = Nodes[index];
                        if (nextNode == null)
                        {
                            Nodes[index] = nextNode = new TmphNode();
                            Keys.UnsafeAdd(letter);
                        }
                        return nextNode;
                    }
                }

                /// <summary>
                ///     关键字转节点索引
                /// </summary>
                /// <param name="key">关键字</param>
                /// <returns>节点索引</returns>
                public static int KeyToIndex(byte key)
                {
                    key |= 0x20;
                    key -= (byte)'a';
                    return key < 26 ? key : 26;
                }
            }

            /// <summary>
            ///     树创建器
            /// </summary>
            private unsafe struct TmphTreeBuilder
            {
                /// <summary>
                ///     结束字符
                /// </summary>
                private byte* end;

                /// <summary>
                ///     当前节点
                /// </summary>
                public TmphNode Node;

                /// <summary>
                ///     创建树
                /// </summary>
                /// <param name="keys">字符数组</param>
                public void Build(byte[] keys)
                {
                    fixed (byte* start = keys)
                    {
                        end = start + keys.Length;
                        build(start);
                    }
                    Node.Value = keys;
                }

                /// <summary>
                ///     创建树
                /// </summary>
                /// <param name="start">当前字符位置</param>
                private void build(byte* start)
                {
                    Node = Node.Create(*start);
                    if (++start != end) build(start);
                }
            }

            /// <summary>
            ///     图创建器
            /// </summary>
            private struct TmphGraphBuilder
            {
                /// <summary>
                ///     根节点
                /// </summary>
                public TmphNode Boot;

                /// <summary>
                ///     当前处理结果节点集合
                /// </summary>
                public TmphList<TmphNode> Writer;

                /// <summary>
                ///     设置根节点
                /// </summary>
                /// <param name="node">根节点</param>
                public void Set(TmphNode node)
                {
                    Boot = node;
                    Writer = new TmphList<TmphNode>();
                }

                /// <summary>
                ///     建图
                /// </summary>
                /// <param name="reader">处理节点集合</param>
                /// <param name="count">处理节点数量</param>
                public unsafe void Build(TmphNode[] reader, int count)
                {
                    Writer.Empty();
                    foreach (var father in reader)
                    {
                        var nodes = father.Nodes;
                        fixed (byte* keyFixed = father.Keys.array)
                        {
                            var keyEnd = keyFixed + father.Keys.Count;
                            if (father.Link == null)
                            {
                                for (var keyStart = keyFixed; keyStart != keyEnd; ++keyStart)
                                {
                                    var index = TmphNode.KeyToIndex(*keyStart);
                                    var node = nodes[index];
                                    node.Link = Boot.Nodes[index];
                                    if (node.Nodes != null) Writer.Add(node);
                                }
                            }
                            else
                            {
                                for (var keyStart = keyFixed; keyStart != keyEnd; ++keyStart)
                                {
                                    var index = TmphNode.KeyToIndex(*keyStart);
                                    TmphNode node = nodes[index], link = father.Link;
                                    while ((link.Nodes == null || (node.Link = link.Nodes[index]) == null) &&
                                           (link = link.Link) != null)
                                        ;
                                    if (node.Link == null) node.Link = Boot.Nodes[index];
                                    if (node.Nodes != null) Writer.Add(node);
                                }
                            }
                        }
                        if (--count == 0) break;
                    }
                }
            }
        }

        /// <summary>
        ///     查询解析器
        /// </summary>
        private sealed unsafe class TmphQueryParser
        {
            /// <summary>
            ///     解析状态
            /// </summary>
            public enum TmphParseState : byte
            {
                /// <summary>
                ///     成功
                /// </summary>
                Success,

                /// <summary>
                ///     逻辑值解析错误
                /// </summary>
                NotBool,

                /// <summary>
                ///     非数字解析错误
                /// </summary>
                NotNumber,

                /// <summary>
                ///     16进制数字解析错误
                /// </summary>
                NotHex,

                /// <summary>
                ///     时间解析错误
                /// </summary>
                NotDateTime,

                /// <summary>
                ///     Guid解析错误
                /// </summary>
                NotGuid,

                /// <summary>
                ///     未知类型解析错误
                /// </summary>
                Unknown
            }

            /// <summary>
            ///     未知类型解析函数信息
            /// </summary>
            private static readonly MethodInfo parseEnumMethod = typeof(TmphQueryParser).GetMethod("parseEnum",
                BindingFlags.Instance | BindingFlags.NonPublic);

            /// <summary>
            ///     未知类型解析函数信息
            /// </summary>
            private static readonly MethodInfo unknownMethod = typeof(TmphQueryParser).GetMethod("unknown",
                BindingFlags.Instance | BindingFlags.NonPublic);

            /// <summary>
            ///     基本类型解析函数
            /// </summary>
            private static readonly Dictionary<Type, MethodInfo> parseMethods;

            /// <summary>
            ///     缓冲区起始位置
            /// </summary>
            private byte* bufferFixed;

            /// <summary>
            ///     当前解析位置
            /// </summary>
            private byte* current;

            /// <summary>
            ///     解析结束位置
            /// </summary>
            private byte* end;

            /// <summary>
            ///     当前处理位置
            /// </summary>
            private int queryIndex;

            /// <summary>
            ///     HTTP请求头部
            /// </summary>
            private TmphRequestHeader requestHeader;

            /// <summary>
            ///     解析状态
            /// </summary>
            private TmphParseState state;

            static TmphQueryParser()
            {
                parseMethods = TmphDictionary.CreateOnly<Type, MethodInfo>();
                foreach (var method in typeof(TmphQueryParser).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                    )
                {
                    if (method.CustomAttribute<TmphParseType>() != null)
                    {
                        parseMethods.Add(method.GetParameters()[0].ParameterType.GetElementType(), method);
                    }
                }
            }

            /// <summary>
            ///     查询解析器
            /// </summary>
            private TmphQueryParser()
            {
            }

            /// <summary>
            ///     创建解析委托函数
            /// </summary>
            /// <param name="type"></param>
            /// <param name="name">成员名称</param>
            /// <param name="memberType">成员类型</param>
            /// <param name="generator"></param>
            /// <returns>解析委托函数</returns>
            private static DynamicMethod createDynamicMethod(Type type, string name, Type memberType,
                out ILGenerator generator)
            {
                var dynamicMethod = new DynamicMethod("queryParser" + name, null,
                    new[] { typeof(TmphQueryParser), type.MakeByRefType() }, type, true);
                generator = dynamicMethod.GetILGenerator();
                var loadMember = generator.DeclareLocal(memberType);
                generator.DeclareLocal(memberType);
                var methodInfo = getParseMethod(memberType);
                if (methodInfo == null)
                {
                    if (memberType.IsEnum) methodInfo = parseEnumMethod.MakeGenericMethod(memberType);
                    else methodInfo = unknownMethod.MakeGenericMethod(memberType);
                }
                if (!memberType.IsValueType)
                {
                    generator.Emit(OpCodes.Ldloca_S, loadMember);
                    generator.Emit(OpCodes.Initobj, memberType);
                }
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldloca_S, loadMember);
                generator.Emit(methodInfo.IsFinal || !methodInfo.IsVirtual ? OpCodes.Call : OpCodes.Callvirt, methodInfo);

                generator.Emit(OpCodes.Ldarg_1);
                if (!type.IsValueType) generator.Emit(OpCodes.Ldind_Ref);
                generator.Emit(OpCodes.Ldloc_0);
                return dynamicMethod;
            }

            /// <summary>
            ///     查询解析
            /// </summary>
            /// <typeparam name="TValueType">目标类型</typeparam>
            /// <param name="requestHeader">HTTP请求头部</param>
            /// <param name="value">目标数据</param>
            /// <returns>解析状态</returns>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private TmphParseState parse<TValueType>(TmphRequestHeader requestHeader, ref TValueType value)
            {
                this.requestHeader = requestHeader;
                state = TmphParseState.Success;
                fixed (byte* bufferFixed = requestHeader.TmphBuffer)
                {
                    this.bufferFixed = bufferFixed;
                    queryIndex = -1;
                    TmphParser<TValueType>.Parse(this, ref value);
                }
                return state;
            }

            /// <summary>
            ///     释放查询解析器
            /// </summary>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private void free()
            {
                requestHeader = null;
                TmphTypePool<TmphQueryParser>.Push(this);
            }

            /// <summary>
            ///     解析10进制数字
            /// </summary>
            /// <param name="value">第一位数字</param>
            /// <returns>数字</returns>
            private uint parseUInt32(uint value)
            {
                uint number;
                do
                {
                    if ((number = (uint)(*current - '0')) > 9) return value;
                    value *= 10;
                    value += number;
                    if (++current == end) return value;
                } while (true);
            }

            /// <summary>
            ///     解析16进制数字
            /// </summary>
            /// <param name="value">数值</param>
            private void parseHex32(ref uint value)
            {
                var number = (uint)(*current - '0');
                if (number > 9)
                {
                    if ((number = (number - ('A' - '0')) & 0xffdfU) > 5)
                    {
                        state = TmphParseState.NotHex;
                        return;
                    }
                    number += 10;
                }
                value = number;
                if (++current == end) return;
                do
                {
                    if ((number = (uint)(*current - '0')) > 9)
                    {
                        if ((number = (number - ('A' - '0')) & 0xffdfU) > 5) return;
                        number += 10;
                    }
                    value <<= 4;
                    value += number;
                } while (++current != end);
            }

            /// <summary>
            ///     逻辑值解析
            /// </summary>
            /// <param name="value">数据</param>
            /// <returns>解析状态</returns>
            [TmphParseType]
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            internal void Parse(ref bool value)
            {
                var indexs = requestHeader.queryIndexs.array[queryIndex].Value;
                if (indexs.Length == 5)
                {
                    current = bufferFixed + indexs.StartIndex;
                    if ((*current | 0x20) == 'f' &&
                        *(int*)(current + 1) == ('a' + ('l' << 8) + ('s' << 16) + ('e' << 24)))
                        value = false;
                    else state = TmphParseState.NotBool;
                }
                else if (indexs.Length == 4)
                {
                    current = bufferFixed + indexs.StartIndex;
                    if (*(int*)current == ('t' + ('r' << 8) + ('u' << 16) + ('e' << 24))) value = true;
                    else state = TmphParseState.NotBool;
                }
                else if (value = indexs.Length == 0) value = false;
                else
                {
                    var byteValue = (byte)(*(bufferFixed + indexs.StartIndex) - '0');
                    if (byteValue < 10) value = byteValue != 0;
                    else state = TmphParseState.NotBool;
                }
            }

            /// <summary>
            ///     数字解析
            /// </summary>
            /// <param name="value">数据</param>
            [TmphParseType]
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            internal void Parse(ref byte value)
            {
                var indexs = requestHeader.queryIndexs.array[queryIndex].Value;
                if (indexs.Length == 0)
                {
                    value = 0;
                    return;
                }
                current = bufferFixed + indexs.StartIndex;
                end = current + indexs.Length;
                var number = (uint)(*current - '0');
                if (number > 9)
                {
                    state = TmphParseState.NotNumber;
                    return;
                }
                if (++current == end)
                {
                    value = (byte)number;
                    return;
                }
                if (number == 0)
                {
                    if (*current != 'x')
                    {
                        value = 0;
                        return;
                    }
                    if (++current == end)
                    {
                        state = TmphParseState.NotNumber;
                        return;
                    }
                    parseHex32(ref number);
                    value = (byte)number;
                    return;
                }
                value = (byte)parseUInt32(number);
            }

            /// <summary>
            ///     数字解析
            /// </summary>
            /// <param name="value">数据</param>
            [TmphParseType]
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            internal void Parse(ref sbyte value)
            {
                var indexs = requestHeader.queryIndexs.array[queryIndex].Value;
                if (indexs.Length == 0)
                {
                    value = 0;
                    return;
                }
                current = bufferFixed + indexs.StartIndex;
                end = current + indexs.Length;
                var sign = 0;
                if (*current == '-')
                {
                    if (++current == end)
                    {
                        state = TmphParseState.NotNumber;
                        return;
                    }
                    sign = 1;
                }
                var number = (uint)(*current - '0');
                if (number > 9)
                {
                    state = TmphParseState.NotNumber;
                    return;
                }
                if (++current == end)
                {
                    value = sign == 0 ? (sbyte)(byte)number : (sbyte)-(int)number;
                    return;
                }
                if (number == 0)
                {
                    if (*current != 'x')
                    {
                        value = 0;
                        return;
                    }
                    if (++current == end)
                    {
                        state = TmphParseState.NotNumber;
                        return;
                    }
                    parseHex32(ref number);
                    value = sign == 0 ? (sbyte)(byte)number : (sbyte)-(int)number;
                    return;
                }
                value = sign == 0 ? (sbyte)(byte)parseUInt32(number) : (sbyte)-(int)parseUInt32(number);
            }

            /// <summary>
            ///     数字解析
            /// </summary>
            /// <param name="value">数据</param>
            [TmphParseType]
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            internal void Parse(ref ushort value)
            {
                var indexs = requestHeader.queryIndexs.array[queryIndex].Value;
                if (indexs.Length == 0)
                {
                    value = 0;
                    return;
                }
                current = bufferFixed + indexs.StartIndex;
                end = current + indexs.Length;
                var number = (uint)(*current - '0');
                if (number > 9)
                {
                    state = TmphParseState.NotNumber;
                    return;
                }
                if (++current == end)
                {
                    value = (ushort)number;
                    return;
                }
                if (number == 0)
                {
                    if (*current != 'x')
                    {
                        value = 0;
                        return;
                    }
                    if (++current == end)
                    {
                        state = TmphParseState.NotNumber;
                        return;
                    }
                    parseHex32(ref number);
                    value = (ushort)number;
                    return;
                }
                value = (ushort)parseUInt32(number);
            }

            /// <summary>
            ///     数字解析
            /// </summary>
            /// <param name="value">数据</param>
            [TmphParseType]
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            internal void Parse(ref short value)
            {
                var indexs = requestHeader.queryIndexs.array[queryIndex].Value;
                if (indexs.Length == 0)
                {
                    value = 0;
                    return;
                }
                current = bufferFixed + indexs.StartIndex;
                end = current + indexs.Length;
                var sign = 0;
                if (*current == '-')
                {
                    if (++current == end)
                    {
                        state = TmphParseState.NotNumber;
                        return;
                    }
                    sign = 1;
                }
                var number = (uint)(*current - '0');
                if (number > 9)
                {
                    state = TmphParseState.NotNumber;
                    return;
                }
                if (++current == end)
                {
                    value = sign == 0 ? (short)(ushort)number : (short)-(int)number;
                    return;
                }
                if (number == 0)
                {
                    if (*current != 'x')
                    {
                        value = 0;
                        return;
                    }
                    if (++current == end)
                    {
                        state = TmphParseState.NotNumber;
                        return;
                    }
                    parseHex32(ref number);
                    value = sign == 0 ? (short)(ushort)number : (short)-(int)number;
                    return;
                }
                value = sign == 0 ? (short)(ushort)parseUInt32(number) : (short)-(int)parseUInt32(number);
            }

            /// <summary>
            ///     数字解析
            /// </summary>
            /// <param name="value">数据</param>
            [TmphParseType]
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            internal void Parse(ref uint value)
            {
                var indexs = requestHeader.queryIndexs.array[queryIndex].Value;
                if (indexs.Length == 0)
                {
                    value = 0;
                    return;
                }
                current = bufferFixed + indexs.StartIndex;
                end = current + indexs.Length;
                var number = (uint)(*current - '0');
                if (number > 9)
                {
                    state = TmphParseState.NotNumber;
                    return;
                }
                if (++current == end)
                {
                    value = number;
                    return;
                }
                if (number == 0)
                {
                    if (*current != 'x')
                    {
                        value = 0;
                        return;
                    }
                    if (++current == end)
                    {
                        state = TmphParseState.NotNumber;
                        return;
                    }
                    parseHex32(ref number);
                    value = number;
                    return;
                }
                value = parseUInt32(number);
            }

            /// <summary>
            ///     数字解析
            /// </summary>
            /// <param name="value">数据</param>
            [TmphParseType]
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            internal void Parse(ref int value)
            {
                var indexs = requestHeader.queryIndexs.array[queryIndex].Value;
                if (indexs.Length == 0)
                {
                    value = 0;
                    return;
                }
                current = bufferFixed + indexs.StartIndex;
                end = current + indexs.Length;
                var sign = 0;
                if (*current == '-')
                {
                    if (++current == end)
                    {
                        state = TmphParseState.NotNumber;
                        return;
                    }
                    sign = 1;
                }
                var number = (uint)(*current - '0');
                if (number > 9)
                {
                    state = TmphParseState.NotNumber;
                    return;
                }
                if (++current == end)
                {
                    value = sign == 0 ? (int)number : -(int)number;
                    return;
                }
                if (number == 0)
                {
                    if (*current != 'x')
                    {
                        value = 0;
                        return;
                    }
                    if (++current == end)
                    {
                        state = TmphParseState.NotNumber;
                        return;
                    }
                    parseHex32(ref number);
                    value = sign == 0 ? (int)number : -(int)number;
                    return;
                }
                value = sign == 0 ? (int)parseUInt32(number) : -(int)parseUInt32(number);
            }

            /// <summary>
            ///     解析10进制数字
            /// </summary>
            /// <param name="value">第一位数字</param>
            /// <returns>数字</returns>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private ulong parseUInt64(uint value)
            {
                var end32 = current + 8;
                if (end32 > end) end32 = end;
                uint number;
                do
                {
                    if ((number = (uint)(*current - '0')) > 9) return value;
                    value *= 10;
                    value += number;
                } while (++current != end32);
                if (current == end) return value;
                ulong value64 = value;
                do
                {
                    if ((number = (uint)(*current - '0')) > 9) return value64;
                    value64 *= 10;
                    value64 += number;
                    if (++current == end) return value64;
                } while (true);
            }

            /// <summary>
            ///     解析16进制数字
            /// </summary>
            /// <returns>数字</returns>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private ulong parseHex64()
            {
                var number = (uint)(*current - '0');
                if (number > 9)
                {
                    if ((number = (number - ('A' - '0')) & 0xffdfU) > 5)
                    {
                        state = TmphParseState.NotHex;
                        return 0;
                    }
                    number += 10;
                }
                if (++current == end) return number;
                var high = number;
                var end32 = current + 7;
                if (end32 > end) end32 = end;
                do
                {
                    if ((number = (uint)(*current - '0')) > 9)
                    {
                        if ((number = (number - ('A' - '0')) & 0xffdfU) > 5) return high;
                        number += 10;
                    }
                    high <<= 4;
                    high += number;
                } while (++current != end32);
                if (current == end) return high;
                var start = current;
                ulong low = number;
                do
                {
                    if ((number = (uint)(*current - '0')) > 9)
                    {
                        if ((number = (number - ('A' - '0')) & 0xffdfU) > 5)
                        {
                            return low | (ulong)high << ((int)(current - start) << 1);
                        }
                        number += 10;
                    }
                    low <<= 4;
                    low += number;
                } while (++current != end);
                return low | (ulong)high << ((int)(current - start) << 1);
            }

            /// <summary>
            ///     数字解析
            /// </summary>
            /// <param name="value">数据</param>
            [TmphParseType]
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            internal void Parse(ref ulong value)
            {
                var indexs = requestHeader.queryIndexs.array[queryIndex].Value;
                if (indexs.Length == 0)
                {
                    value = 0;
                    return;
                }
                current = bufferFixed + indexs.StartIndex;
                end = current + indexs.Length;
                var number = (uint)(*current - '0');
                if (number > 9)
                {
                    state = TmphParseState.NotNumber;
                    return;
                }
                if (++current == end)
                {
                    value = number;
                    return;
                }
                if (number == 0)
                {
                    if (*current != 'x')
                    {
                        value = 0;
                        return;
                    }
                    if (++current == end)
                    {
                        state = TmphParseState.NotNumber;
                        return;
                    }
                    value = parseHex64();
                    return;
                }
                value = parseUInt64(number);
            }

            /// <summary>
            ///     数字解析
            /// </summary>
            /// <param name="value">数据</param>
            [TmphParseType]
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            internal void Parse(ref long value)
            {
                var indexs = requestHeader.queryIndexs.array[queryIndex].Value;
                if (indexs.Length == 0)
                {
                    value = 0;
                    return;
                }
                current = bufferFixed + indexs.StartIndex;
                end = current + indexs.Length;
                var sign = 0;
                if (*current == '-')
                {
                    if (++current == end)
                    {
                        state = TmphParseState.NotNumber;
                        return;
                    }
                    sign = 1;
                }
                var number = (uint)(*current - '0');
                if (number > 9)
                {
                    state = TmphParseState.NotNumber;
                    return;
                }
                if (++current == end)
                {
                    value = sign == 0 ? (int)number : -(long)(int)number;
                    return;
                }
                if (number == 0)
                {
                    if (*current != 'x')
                    {
                        value = 0;
                        return;
                    }
                    if (++current == end)
                    {
                        state = TmphParseState.NotNumber;
                        return;
                    }
                    value = (long)parseHex64();
                    if (sign != 0) value = -value;
                    return;
                }
                value = (long)parseUInt64(number);
                if (sign != 0) value = -value;
            }

            /// <summary>
            ///     数字解析
            /// </summary>
            /// <param name="value">数据</param>
            [TmphParseType]
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            internal void Parse(ref float value)
            {
                var indexs = requestHeader.queryIndexs.array[queryIndex].Value;
                if (indexs.Length == 0) value = 0;
                else
                {
                    string number =
                        TmphFormQuery.JavascriptUnescape(TmphSubArray<byte>.Unsafe(requestHeader.TmphBuffer, indexs.StartIndex,
                            indexs.Length));
                    if (!float.TryParse(number, out value)) state = TmphParseState.NotNumber;
                }
            }

            /// <summary>
            ///     数字解析
            /// </summary>
            /// <param name="value">数据</param>
            [TmphParseType]
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            internal void Parse(ref double value)
            {
                var indexs = requestHeader.queryIndexs.array[queryIndex].Value;
                if (indexs.Length == 0) value = 0;
                else
                {
                    string number =
                        TmphFormQuery.JavascriptUnescape(TmphSubArray<byte>.Unsafe(requestHeader.TmphBuffer, indexs.StartIndex,
                            indexs.Length));
                    if (!double.TryParse(number, out value)) state = TmphParseState.NotNumber;
                }
            }

            /// <summary>
            ///     数字解析
            /// </summary>
            /// <param name="value">数据</param>
            [TmphParseType]
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            internal void Parse(ref decimal value)
            {
                var indexs = requestHeader.queryIndexs.array[queryIndex].Value;
                if (indexs.Length == 0) value = 0;
                else
                {
                    string number =
                        TmphFormQuery.JavascriptUnescape(TmphSubArray<byte>.Unsafe(requestHeader.TmphBuffer, indexs.StartIndex,
                            indexs.Length));
                    if (!decimal.TryParse(number, out value)) state = TmphParseState.NotNumber;
                }
            }

            /// <summary>
            ///     时间解析
            /// </summary>
            /// <param name="value">数据</param>
            [TmphParseType]
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            internal void Parse(ref DateTime value)
            {
                var indexs = requestHeader.queryIndexs.array[queryIndex].Value;
                if (indexs.Length == 0) value = DateTime.MinValue;
                else
                {
                    string dateTime =
                        TmphFormQuery.JavascriptUnescape(TmphSubArray<byte>.Unsafe(requestHeader.TmphBuffer, indexs.StartIndex,
                            indexs.Length));
                    if (!DateTime.TryParse(dateTime, out value)) state = TmphParseState.NotDateTime;
                }
            }

            /// <summary>
            ///     解析16进制字符
            /// </summary>
            /// <returns>字符</returns>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private uint parseHex4()
            {
                uint code = (uint)(*++current - '0'), number = (uint)(*++current - '0');
                if (code > 9) code = ((code - ('A' - '0')) & 0xffdfU) + 10;
                if (number > 9) number = ((number - ('A' - '0')) & 0xffdfU) + 10;
                code <<= 12;
                code += (number << 8);
                if ((number = (uint)(*++current - '0')) > 9) number = ((number - ('A' - '0')) & 0xffdfU) + 10;
                code += (number << 4);
                number = (uint)(*++current - '0');
                return code + (number > 9 ? (((number - ('A' - '0')) & 0xffdfU) + 10) : number);
            }

            /// <summary>
            ///     解析16进制字符
            /// </summary>
            /// <returns>字符</returns>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private uint parseHex2()
            {
                uint code = (uint)(*++current - '0'), number = (uint)(*++current - '0');
                if (code > 9) code = ((code - ('A' - '0')) & 0xffdfU) + 10;
                return (number > 9 ? (((number - ('A' - '0')) & 0xffdfU) + 10) : number) + (code << 4);
            }

            /// <summary>
            ///     Guid解析
            /// </summary>
            /// <param name="value">数据</param>
            [TmphParseType]
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            internal void Parse(ref Guid value)
            {
                var indexs = requestHeader.queryIndexs.array[queryIndex].Value;
                if (indexs.Length == 0) value = new Guid();
                else if (end - current != 36) state = TmphParseState.NotGuid;
                else
                {
                    current = bufferFixed + indexs.StartIndex;
                    end = current + indexs.Length;
                    var guid = new TmphGuid();
                    guid.Byte3 = (byte)parseHex2();
                    guid.Byte2 = (byte)parseHex2();
                    guid.Byte1 = (byte)parseHex2();
                    guid.Byte0 = (byte)parseHex2();
                    if (*++current != '-')
                    {
                        state = TmphParseState.NotGuid;
                        return;
                    }
                    guid.Byte45 = (ushort)parseHex4();
                    if (*++current != '-')
                    {
                        state = TmphParseState.NotGuid;
                        return;
                    }
                    guid.Byte67 = (ushort)parseHex4();
                    if (*++current != '-')
                    {
                        state = TmphParseState.NotGuid;
                        return;
                    }
                    guid.Byte8 = (byte)parseHex2();
                    guid.Byte9 = (byte)parseHex2();
                    if (*++current != '-')
                    {
                        state = TmphParseState.NotGuid;
                        return;
                    }
                    guid.Byte10 = (byte)parseHex2();
                    guid.Byte11 = (byte)parseHex2();
                    guid.Byte12 = (byte)parseHex2();
                    guid.Byte13 = (byte)parseHex2();
                    guid.Byte14 = (byte)parseHex2();
                    guid.Byte15 = (byte)parseHex2();
                    value = guid.Value;
                }
            }

            /// <summary>
            ///     字符串解析
            /// </summary>
            /// <param name="value">数据</param>
            [TmphParseType]
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            internal void Parse(ref string value)
            {
                var indexs = requestHeader.queryIndexs.array[queryIndex].Value;
                if (indexs.Length == 0) value = string.Empty;
                else
                    value =
                        TmphFormQuery.JavascriptUnescape(TmphSubArray<byte>.Unsafe(requestHeader.TmphBuffer, indexs.StartIndex,
                            indexs.Length));
            }

            /// <summary>
            ///     字符串解析
            /// </summary>
            /// <param name="value">数据</param>
            [TmphParseType]
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            internal void Parse(ref TmphSubString value)
            {
                var indexs = requestHeader.queryIndexs.array[queryIndex].Value;
                if (indexs.Length == 0) value.UnsafeSet(string.Empty, 0, 0);
                else
                    value =
                        TmphFormQuery.JavascriptUnescape(TmphSubArray<byte>.Unsafe(requestHeader.TmphBuffer, indexs.StartIndex,
                            indexs.Length));
            }

            /// <summary>
            ///     未知类型解析
            /// </summary>
            /// <param name="value">目标数据</param>
            private void parseEnum<TValueType>(ref TValueType value)
            {
                var indexs = requestHeader.queryIndexs.array[queryIndex].Value;
                if (indexs.Length == 0) value = default(TValueType);
                else
                {
                    var json =
                        TmphFormQuery.JavascriptUnescape(TmphSubArray<byte>.Unsafe(requestHeader.TmphBuffer,
                            indexs.StartIndex - 1, indexs.Length + 2));
                    fixed (char* jsonFixed = json.value) *jsonFixed = *(jsonFixed + json.Length - 1) = '"';
                    if (!TmphJsonParser.Parse(json, ref value)) state = TmphParseState.Unknown;
                }
            }

            /// <summary>
            ///     未知类型解析
            /// </summary>
            /// <param name="value">目标数据</param>
            private void unknown<TValueType>(ref TValueType value)
            {
                var indexs = requestHeader.queryIndexs.array[queryIndex].Value;
                if (indexs.Length == 0) value = default(TValueType);
                else if (
                    !TmphJsonParser.Parse(
                        TmphFormQuery.JavascriptUnescape(TmphSubArray<byte>.Unsafe(requestHeader.TmphBuffer,
                            indexs.StartIndex, indexs.Length)), ref value))
                {
                    state = TmphParseState.Unknown;
                }
            }

            /// <summary>
            ///     是否存在未结束的查询
            /// </summary>
            /// <returns>是否存在未结束的查询</returns>
            private bool isQuery()
            {
                if (++queryIndex == requestHeader.queryIndexs.Count) return false;
                var indexs = requestHeader.queryIndexs.array[queryIndex].Key;
                current = bufferFixed + indexs.StartIndex;
                end = current + indexs.Length;
                return true;
            }

            /// <summary>
            ///     获取当前名称字符
            /// </summary>
            /// <returns>当前名称字符,结束返回0</returns>
            private byte getName()
            {
                return current == end ? (byte)0 : *current++;
            }

            /// <summary>
            ///     查询解析
            /// </summary>
            /// <typeparam name="TValueType">目标类型</typeparam>
            /// <param name="requestHeader">HTTP请求头部</param>
            /// <param name="value">目标数据</param>
            /// <returns>是否解析成功</returns>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            public static bool Parse<TValueType>(TmphRequestHeader requestHeader, ref TValueType value)
            {
                if (requestHeader.queryIndexs.Count != 0)
                {
                    var parser = TmphTypePool<TmphQueryParser>.Pop() ?? new TmphQueryParser();
                    try
                    {
                        return parser.parse(requestHeader, ref value) == TmphParseState.Success;
                    }
                    finally
                    {
                        parser.free();
                    }
                }
                return true;
            }

            /// <summary>
            ///     获取基本类型解析函数
            /// </summary>
            /// <param name="type">基本类型</param>
            /// <returns>解析函数</returns>
            private static MethodInfo getParseMethod(Type type)
            {
                MethodInfo method;
                return parseMethods.TryGetValue(type, out method) ? method : null;
            }

            /// <summary>
            ///     解析类型
            /// </summary>
            private sealed class TmphParseType : Attribute
            {
            }

            /// <summary>
            ///     名称状态查找器
            /// </summary>
            internal struct TmphStateSearcher
            {
                /// <summary>
                ///     ASCII字符查找表
                /// </summary>
                private readonly byte* charsAscii;

                /// <summary>
                ///     特殊字符串查找表
                /// </summary>
                private readonly byte* charStart;

                /// <summary>
                ///     查询解析器
                /// </summary>
                private readonly TmphQueryParser parser;

                /// <summary>
                ///     状态集合
                /// </summary>
                private readonly byte* state;

                /// <summary>
                ///     查询矩阵单位尺寸类型
                /// </summary>
                private readonly byte tableType;

                /// <summary>
                ///     特殊字符串查找表结束位置
                /// </summary>
                private byte* charEnd;

                /// <summary>
                ///     特殊字符起始值
                /// </summary>
                private int charIndex;

                /// <summary>
                ///     当前状态
                /// </summary>
                private byte* currentState;

                /// <summary>
                ///     名称查找器
                /// </summary>
                /// <param name="parser">查询解析器</param>
                /// <param name="data">数据起始位置</param>
                internal TmphStateSearcher(TmphQueryParser parser, TmphPointer data)
                {
                    this.parser = parser;
                    if (data.Data == null)
                    {
                        state = charsAscii = charStart = charEnd = currentState = null;
                        charIndex = 0;
                        tableType = 0;
                    }
                    else
                    {
                        var stateCount = *data.Int;
                        currentState = state = data.Byte + sizeof(int);
                        charsAscii = state + stateCount * 3 * sizeof(int);
                        charStart = charsAscii + 128 * sizeof(ushort);
                        charIndex = *(ushort*)charStart;
                        charStart += sizeof(ushort) * 2;
                        charEnd = charStart + *(ushort*)(charStart - sizeof(ushort)) * sizeof(ushort);
                        if (stateCount < 256) tableType = 0;
                        else if (stateCount < 65536) tableType = 1;
                        else tableType = 2;
                    }
                }

                /// <summary>
                ///     获取名称索引
                /// </summary>
                /// <returns>名称索引,失败返回-1</returns>
                internal int SearchName()
                {
                    if (state == null) return -1;
                    var value = parser.getName();
                    if (value == 0) return *(int*)(currentState + sizeof(int) * 2);
                    currentState = state;
                    do
                    {
                        var prefix = (char*)(currentState + *(int*)currentState);
                        if (*prefix != 0)
                        {
                            if (value != *prefix) return -1;
                            while (*++prefix != 0)
                            {
                                if (parser.getName() != *prefix) return -1;
                            }
                            value = parser.getName();
                        }
                        if (value == 0) return *(int*)(currentState + sizeof(int) * 2);
                        if (*(int*)(currentState + sizeof(int)) == 0 || value >= 128) return -1;
                        int index = *(ushort*)(charsAscii + (value << 1));
                        var table = currentState + *(int*)(currentState + sizeof(int));
                        if (tableType == 0)
                        {
                            if ((index = *(table + index)) == 0) return -1;
                            currentState = state + index * 3 * sizeof(int);
                        }
                        else if (tableType == 1)
                        {
                            if ((index = *(ushort*)(table + index * sizeof(ushort))) == 0) return -1;
                            currentState = state + index * 3 * sizeof(int);
                        }
                        else
                        {
                            if ((index = *(int*)(table + index * sizeof(int))) == 0) return -1;
                            currentState = state + index;
                        }
                        value = parser.getName();
                    } while (true);
                }
            }

            /// <summary>
            ///     类型解析器
            /// </summary>
            /// <typeparam name="TValueType">目标类型</typeparam>
            internal static class TmphParser<TValueType>
            {
                /// <summary>
                ///     成员解析器集合
                /// </summary>
                private static readonly TmphTryParse[] memberParsers;

                /// <summary>
                ///     成员名称查找数据
                /// </summary>
                private static readonly TmphPointer memberSearcher;

                static TmphParser()
                {
                    var type = typeof(TValueType);
                    var attribute = TmphTypeAttribute.GetAttribute<TmphJsonParse>(type, true, true) ?? TmphJsonParse.AllMember;
                    TmphFieldIndex defaultMember = null;
                    var fields =
                        TmphJsonParser.TmphTypeParser.GetFields(
                            TmphMemberIndexGroup<TValueType>.GetFields(attribute.MemberFilter), attribute,
                            ref defaultMember);
                    var properties =
                        TmphJsonParser.TmphTypeParser.GetProperties(
                            TmphMemberIndexGroup<TValueType>.GetProperties(attribute.MemberFilter), attribute);
                    memberParsers = new TmphTryParse[fields.Count + properties.Count + (defaultMember == null ? 0 : 1)];
                    var names = new string[memberParsers.Length];
                    var index = 0;
                    foreach (var member in fields)
                    {
                        ILGenerator generator;
                        var dynamicMethod = createDynamicMethod(type, member.Member.Name, member.Member.FieldType,
                            out generator);
                        generator.Emit(OpCodes.Stfld, member.Member);
                        generator.Emit(OpCodes.Ret);
                        var tryParse = (TmphTryParse)dynamicMethod.CreateDelegate(typeof(TmphTryParse));
                        memberParsers[index] = tryParse;
                        names[index++] = member.Member.Name;
                        if (member == defaultMember)
                        {
                            memberParsers[index] = tryParse;
                            names[index++] = string.Empty;
                        }
                    }
                    foreach (var member in properties)
                    {
                        ILGenerator generator;
                        var dynamicMethod = createDynamicMethod(type, member.Key.Member.Name,
                            member.Key.Member.PropertyType, out generator);
                        generator.Emit(
                            member.Value.IsFinal || !member.Value.IsVirtual ? OpCodes.Call : OpCodes.Callvirt,
                            member.Value);
                        generator.Emit(OpCodes.Ret);
                        memberParsers[index] = (TmphTryParse)dynamicMethod.CreateDelegate(typeof(TmphTryParse));
                        names[index++] = member.Key.Member.Name;
                    }
                    if (type.IsGenericType)
                        memberSearcher = TmphJsonParser.TmphStateSearcher.GetGenericDefinitionMember(type, names);
                    else memberSearcher = CLB.TmphStateSearcher.TmphChars.Create(names);
                }

                /// <summary>
                ///     对象解析
                /// </summary>
                /// <param name="parser">查询解析器</param>
                /// <param name="value">目标数据</param>
                [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
                internal static void Parse(TmphQueryParser parser, ref TValueType value)
                {
                    var searcher = new TmphStateSearcher(parser, memberSearcher);
                    while (parser.isQuery())
                    {
                        var index = searcher.SearchName();
                        if (index != -1) memberParsers[index](parser, ref value);
                    }
                }

                /// <summary>
                ///     解析委托
                /// </summary>
                /// <param name="parser">查询解析器</param>
                /// <param name="value">目标数据</param>
                internal delegate void TmphTryParse(TmphQueryParser parser, ref TValueType value);
            }
        }
    }
}