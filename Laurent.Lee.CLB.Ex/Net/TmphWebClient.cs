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

using Laurent.Lee.CLB.IO.Compression;
using Laurent.Lee.CLB.Web;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Web;

namespace Laurent.Lee.CLB.Net
{
    /// <summary>
    ///     WebClient相关操作
    /// </summary>
    public class TmphWebClient : WebClient
    {
        /// <summary>
        ///     默认浏览器参数
        /// </summary>
        public const string DefaultUserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; SV1;)";

        /// <summary>
        ///     字符集标识
        /// </summary>
        public const string CharsetName = "charset=";

        /// <summary>
        ///     默认表单提交内容类型
        /// </summary>
        public const string DefaultPostContentType = "TmphApplication/x-www-form-urlencoded";

        /// <summary>
        ///     空页面地址
        /// </summary>
        public const string BlankUrl = "about:blank";

        /// <summary>
        ///     ServicePointManager.Expect100Continue访问锁
        /// </summary>
        public static readonly object Expect100ContinueLock = new object();

        /// <summary>
        ///     过期时间
        /// </summary>
        private static readonly TmphHashString expiresHash = "expires";

        /// <summary>
        ///     有效路径
        /// </summary>
        private static readonly TmphHashString pathHash = "path";

        /// <summary>
        ///     作用域名
        /// </summary>
        private static readonly TmphHashString domainHash = "domain";

        /// <summary>
        ///     HTTP回应
        /// </summary>
        /// <summary>
        ///     是否允许跳转
        /// </summary>
        public bool AllowAutoRedirect = true;

        /// <summary>
        ///     是否保持连接
        /// </summary>
        public bool KeepAlive = true;

        /// <summary>
        ///     超时毫秒数
        /// </summary>
        public int TimeOut;

        /// <summary>
        ///     浏览器参数
        /// </summary>
        public string UserAgent = DefaultUserAgent;

        /// <summary>
        ///     HTTP请求
        /// </summary>
        private WebRequest webRequest;

        /// <summary>
        ///     初始化
        /// </summary>
        /// <param name="cookies">cookie状态</param>
        /// <param name="proxy">代理</param>
        public TmphWebClient(CookieContainer cookies = null, WebProxy proxy = null)
        {
            Credentials = new CredentialCache();
            Cookies = cookies == null ? new CookieContainer() : cookies;
            Credentials = CredentialCache.DefaultCredentials;

            Proxy = proxy;
            //string header = client.ResponseHeaders[web.header.SetCookie];
            //client.Headers.Add(header.Cookie, header);
        }

        /// <summary>
        ///     cookie状态
        /// </summary>
        public CookieContainer Cookies { get; private set; }

        /// <summary>
        ///     HTTP请求
        /// </summary>
        public HttpWebRequest HttpRequest
        {
            get { return webRequest == null ? null : webRequest as HttpWebRequest; }
        }

        /// <summary>
        ///     获取最后一次操作是否发生重定向
        /// </summary>
        public bool IsRedirect
        {
            get
            {
                return webRequest != null && webRequest is HttpWebRequest
                       && webRequest.RequestUri.Equals((webRequest as HttpWebRequest).Address);
            }
        }

        /// <summary>
        ///     获取最后一次重定向地址
        /// </summary>
        public Uri RedirectUri
        {
            get { return IsRedirect ? (webRequest as HttpWebRequest).Address : null; }
        }

        /// <summary>
        ///     HTTP回应压缩流处理
        /// </summary>
        private TmphStream compressionStream
        {
            get
            {
                if (ResponseHeaders != null)
                {
                    switch (ResponseHeaders[TmphHeader.ContentEncoding])
                    {
                        case "gzip":
                            return TmphStream.GZip;

                        case "deflate":
                            return TmphStream.Deflate;
                    }
                }
                return null;
            }
        }

        /// <summary>
        ///     HTTP回应编码字符集
        /// </summary>
        public Encoding TextEncoding
        {
            get
            {
                if (ResponseHeaders != null)
                {
                    var contentType = ResponseHeaders[TmphHeader.ContentType];
                    if (contentType != null) return getEncoding(contentType);
                }
                return null;
            }
        }

        /// <summary>
        ///     获取重定向地址
        /// </summary>
        public string Location
        {
            get
            {
                var response = webRequest == null ? null : webRequest.GetResponse();
                return response == null ? null : response.Headers[TmphHeader.Location];
            }
        }

        /// <summary>
        ///     获取HTTP请求
        /// </summary>
        /// <param name="address">URI地址</param>
        /// <returns>HTTP请求</returns>
        protected override WebRequest GetWebRequest(Uri address)
        {
            webRequest = base.GetWebRequest(address);
            var TmphRequest = HttpRequest;
            if (TmphRequest != null)
            {
                TmphRequest.KeepAlive = KeepAlive;
                TmphRequest.AllowAutoRedirect = AllowAutoRedirect;
                TmphRequest.CookieContainer = Cookies;
                if (TimeOut > 0) TmphRequest.Timeout = TimeOut;
            }
            return TmphRequest;
        }

        ///// <summary>
        ///// 获取HTTP回应
        ///// </summary>
        ///// <param name="TmphRequest">HTTP请求</param>
        ///// <returns>HTTP回应</returns>
        //protected override WebResponse GetWebResponse(WebRequest TmphRequest)
        //{
        //    Response = base.GetWebResponse(TmphRequest);
        //    if (TimeOut > 0)
        //    {
        //        HttpWebResponse response = Response as HttpWebResponse;
        //        if (response != null) response.GetResponseStream().ReadTimeout = TimeOut;
        //    }
        //    return Response;
        //}
        ///// <summary>
        ///// 获取HTTP回应
        ///// </summary>
        ///// <param name="TmphRequest">HTTP请求</param>
        ///// <param name="result"></param>
        ///// <returns>HTTP回应</returns>
        //protected override WebResponse GetWebResponse(WebRequest TmphRequest, IAsyncResult result)
        //{
        //    Response = base.GetWebResponse(TmphRequest, result);
        //    if (TimeOut > 0)
        //    {
        //        HttpWebResponse response = Response as HttpWebResponse;
        //        if (response != null) response.GetResponseStream().ReadTimeout = TimeOut;
        //    }
        //    return Response;
        //}
        /// <summary>
        ///     添加COOKIE
        /// </summary>
        /// <param name="address">URI地址</param>
        /// <param name="cookieString">COOKIE字符串</param>
        public void AddCookie(Uri address, string cookieString)
        {
            if (address != null && cookieString != null && cookieString.Length != 0)
            {
                int cookieIndex;
                foreach (TmphSubString cookie in cookieString.Split(';'))
                {
                    if ((cookieIndex = cookie.IndexOf('=')) > 0)
                    {
                        try
                        {
                            Cookies.Add(address,
                                new Cookie(Web.TmphCookie.FormatCookieName(cookie.Substring(0, cookieIndex).Trim()),
                                    Web.TmphCookie.FormatCookieValue(cookie.Substring(cookieIndex + 1).Trim()), "/"));
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     合并同域cookie(用于处理跨域BUG)
        /// </summary>
        /// <param name="address">URI地址</param>
        /// <param name="cookies">默认cookie集合信息</param>
        /// <param name="documentCookie">登录后的cookie信息</param>
        /// <param name="httpOnlyCookie">登录后的httpOnly相关cookie信息</param>
        public void MergeDomainCookie(Uri address, TmphList<Cookie> cookies, string documentCookie, string httpOnlyCookie)
        {
            if (cookies != null)
            {
                foreach (Cookie cookie in cookies) Cookies.Add(address, cookie);
            }
            if (address != null)
            {
                TmphList<Cookie> newCookies = new TmphList<Cookie>();
                Dictionary<TmphHashString, int> nameCounts = null;
                TmphList<string> documentCookies = new TmphList<string>(2);
                if (documentCookie.Length != 0) documentCookies.Unsafer.Add(documentCookie);
                if (httpOnlyCookie.Length != 0) documentCookies.Unsafer.Add(httpOnlyCookie);
                if (documentCookies.Count != 0)
                {
                    int index, nextCount, count;
                    string name;
                    Cookie newCookie;
                    Dictionary<TmphHashString, int> nextNameCounts = TmphDictionary.CreateHashString<int>();
                    nameCounts = TmphDictionary.CreateHashString<int>();
                    foreach (string nextCookie in documentCookies)
                    {
                        nextNameCounts.Clear();
                        foreach (TmphSubString cookie in nextCookie.Split(';'))
                        {
                            if (cookie.Length != 0 && (index = cookie.IndexOf('=')) != 0)
                            {
                                if (index == -1)
                                {
                                    name = Web.TmphCookie.FormatCookieName(cookie.Trim());
                                }
                                else name = Web.TmphCookie.FormatCookieName(cookie.Substring(0, index).Trim());
                                TmphHashString nameKey = name;
                                if (nextNameCounts.TryGetValue(nameKey, out nextCount))
                                    nextNameCounts[nameKey] = ++nextCount;
                                else nextNameCounts.Add(nameKey, nextCount = 1);
                                if (!nameCounts.TryGetValue(nameKey, out count)) count = 0;
                                if (nextCount > count)
                                {
                                    if (index == -1) newCookie = new Cookie(name, string.Empty);
                                    else
                                        newCookie = new Cookie(name,
                                            Web.TmphCookie.FormatCookieValue(cookie.Substring(index + 1)));
                                    newCookies.Add(newCookie);
                                    if (count != 0) newCookie.HttpOnly = true;
                                    if (count == 0) nameCounts.Add(nameKey, nextCount);
                                    else nameCounts[nameKey] = nextCount;
                                }
                            }
                        }
                    }
                }
                foreach (Cookie cookie in Cookies.GetCookies(address))
                {
                    if (nameCounts != null && nameCounts.ContainsKey(cookie.Name)) cookie.Expired = true;
                }
                if (newCookies.Count != 0)
                {
                    try
                    {
                        foreach (Cookie cookie in newCookies) Cookies.Add(address, cookie);
                    }
                    catch (Exception error)
                    {
                        TmphLog.Default.Add(error, "合并同域cookie失败", true);
                    }
                }
            }
        }

        /// <summary>
        ///     合并同域cookie(用于处理跨域BUG)
        /// </summary>
        /// <param name="address">URI地址</param>
        /// <param name="responseHeaderCookie">HTTP头cookie信息</param>
        /// <param name="replaceCookie">需要替换的cookie</param>
        public void MergeDomainCookie(Uri address, string responseHeaderCookie, string replaceCookie)
        {
            if (address != null)
            {
                int index;
                string name;
                Cookie newCookie;
                var cookies = new CookieCollection();
                Dictionary<TmphHashString, Cookie> replaceCookies = null;
                if (responseHeaderCookie != null && responseHeaderCookie.Length != 0)
                {
                    replaceCookies = TmphDictionary.CreateHashString<Cookie>();
                    DateTime expires;
                    string value, domain, path, expiresString;
                    string lastCookie = null;
                    TmphList<string> newCookies = new TmphList<string>();
                    foreach (var cookie in responseHeaderCookie.Split(','))
                    {
                        if (lastCookie == null)
                        {
                            var lowerCookie = cookie.ToLower();
                            index = lowerCookie.IndexOf("; expires=", StringComparison.Ordinal);
                            if (index == -1) index = lowerCookie.IndexOf(";expires=", StringComparison.Ordinal);
                            if (index == -1 || cookie.IndexOf(';', index + 10) != -1) newCookies.Add(cookie);
                            else lastCookie = cookie;
                        }
                        else
                        {
                            newCookies.Add(lastCookie + "," + cookie);
                            lastCookie = null;
                        }
                    }
                    Dictionary<TmphHashString, string> cookieInfo = TmphDictionary.CreateHashString<string>();
                    foreach (string cookie in newCookies)
                    {
                        newCookie = null;
                        foreach (TmphSubString values in cookie.Split(';'))
                        {
                            if ((index = values.IndexOf('=')) != 0)
                            {
                                if ((index = values.IndexOf('=')) == -1)
                                {
                                    name = values.Trim();
                                    value = string.Empty;
                                }
                                else
                                {
                                    name = values.Substring(0, index).Trim();
                                    value = values.Substring(index + 1);
                                }
                                if (newCookie == null)
                                    newCookie = new Cookie(Web.TmphCookie.FormatCookieName(name),
                                        Web.TmphCookie.FormatCookieValue(value));
                                else cookieInfo[name.ToLower()] = value;
                            }
                        }
                        if (cookieInfo.TryGetValue(expiresHash, out expiresString)
                            && DateTime.TryParse(expiresString, out expires))
                        {
                            newCookie.Expires = expires;
                        }
                        if (cookieInfo.TryGetValue(pathHash, out path)) newCookie.Path = path;
                        replaceCookies[newCookie.Name] = newCookie;
                        newCookie = new Cookie(newCookie.Name, newCookie.Value, newCookie.Path);
                        if (cookieInfo.TryGetValue(domainHash, out domain)) newCookie.Domain = domain;
                        Cookies.Add(address, newCookie);
                        cookieInfo.Clear();
                    }
                }
                if (replaceCookie != null && replaceCookie.Length != 0)
                {
                    if (replaceCookies == null) replaceCookies = TmphDictionary.CreateHashString<Cookie>();
                    foreach (TmphSubString cookie in replaceCookie.Split(';'))
                    {
                        if ((index = cookie.IndexOf('=')) != 0)
                        {
                            if (index == -1)
                            {
                                name = Web.TmphCookie.FormatCookieName(cookie.Trim());
                                newCookie = new Cookie(name, string.Empty);
                            }
                            else
                            {
                                name = Web.TmphCookie.FormatCookieName(cookie.Substring(0, index).Trim());
                                newCookie = new Cookie(name, Web.TmphCookie.FormatCookieValue(cookie.Substring(index + 1)));
                            }
                            TmphHashString nameKey = name;
                            if (replaceCookies.ContainsKey(nameKey)) replaceCookies[nameKey].Value = newCookie.Value;
                            else replaceCookies.Add(nameKey, newCookie);
                        }
                    }
                }
                bool isCookie;
                foreach (Cookie cookie in Cookies.GetCookies(address))
                {
                    if (isCookie = replaceCookies != null && replaceCookies.ContainsKey(cookie.Name))
                    {
                        newCookie = replaceCookies[cookie.Name];
                    }
                    else newCookie = new Cookie(cookie.Name, cookie.Value, HttpUtility.UrlDecode((cookie.Path)));
                    cookies.Add(newCookie);
                    if (isCookie) replaceCookies.Remove(cookie.Name);
                    newCookie.Expires = cookie.Expires;
                    cookie.Expired = true;
                }
                if (replaceCookies != null)
                {
                    foreach (var cookie in replaceCookies.Values) cookies.Add(cookie);
                }
                if (cookies.Count != 0)
                {
                    try
                    {
                        Cookies.Add(address, cookies);
                    }
                    catch (Exception error)
                    {
                        TmphLog.Default.Add(error, "合并同域cookie失败", true);
                    }
                }
            }
        }

        /// <summary>
        ///     将网页保存到文件
        /// </summary>
        /// <param name="TmphRequest">URI请求信息</param>
        /// <param name="fileName">保存文件名</param>
        /// <returns>是否保存成功</returns>
        public bool SaveFile(TmphRequest TmphRequest, string fileName)
        {
            if (TmphRequest.Uri != null && fileName != null)
            {
                try
                {
                    Headers.Add(TmphHeader.UserAgent, UserAgent);
                    Headers.Add(TmphHeader.Referer,
                        TmphRequest.RefererUrl == null || TmphRequest.RefererUrl.Length == 0
                            ? TmphRequest.Uri.AbsoluteUri
                            : TmphRequest.RefererUrl);
                    DownloadFile(TmphRequest.Uri, fileName);
                    return true;
                }
                catch (Exception error)
                {
                    if (TmphRequest.IsErrorOut)
                    {
                        TmphLog.Default.Add(error, (TmphRequest.IsErrorOutUri ? TmphRequest.Uri.AbsoluteUri : null) + " 抓取失败",
                            !TmphRequest.IsErrorOutUri);
                    }
                }
            }
            return false;
        }

        /// <summary>
        ///     抓取页面字节流
        /// </summary>
        /// <param name="TmphRequest">URI请求信息</param>
        /// <returns>页面字节流,失败返回null</returns>
        public byte[] CrawlData(TmphRequest TmphRequest)
        {
            if (TmphRequest.Uri != null)
            {
                try
                {
                    Headers.Add(TmphHeader.UserAgent, UserAgent);
                    Headers.Add(TmphHeader.Referer,
                        TmphRequest.RefererUrl == null || TmphRequest.RefererUrl.Length == 0
                            ? TmphRequest.Uri.AbsoluteUri
                            : TmphRequest.RefererUrl);
                    return
                        deCompress(
                            TmphRequest.Form == null
                                ? DownloadData(TmphRequest.Uri)
                                : UploadValues(TmphRequest.Uri, Web.TmphHttp.TmphMethodType.POST.ToString(), TmphRequest.Form), TmphRequest);
                }
                catch (Exception error)
                {
                    onError(error, TmphRequest);
                }
            }
            return null;
        }

        /// <summary>
        ///     抓取页面字节流
        /// </summary>
        /// <param name="onCrawlData">异步事件</param>
        /// <param name="TmphRequest">URI请求信息</param>
        public void CrawlData(Action<byte[]> onCrawlData, TmphRequest TmphRequest)
        {
            TmphDataCrawler.Crawl(this, onCrawlData, TmphRequest);
        }

        /// <summary>
        ///     抓取页面HTML代码
        /// </summary>
        /// <param name="TmphRequest">URI请求信息</param>
        /// <param name="encoding">页面编码</param>
        /// <returns>页面HTML代码,失败返回null</returns>
        public string CrawlHtml(TmphRequest TmphRequest, Encoding encoding)
        {
            return TmphChineseEncoder.ToString(CrawlData(TmphRequest), encoding ?? TextEncoding);
        }

        /// <summary>
        ///     异步抓取页面HTML代码
        /// </summary>
        /// <param name="onCrawlHtml">异步事件</param>
        /// <param name="TmphRequest">URI请求信息</param>
        /// <param name="encoding">页面编码</param>
        /// <returns>页面HTML代码,失败返回null</returns>
        public void CrawlHtml(Action<string> onCrawlHtml, TmphRequest TmphRequest, Encoding encoding)
        {
            CrawlData(new TmphHtmlCrawler { WebClient = this, OnCrawlHtml = onCrawlHtml, Encoding = encoding }.onCrawlData,
                TmphRequest);
        }

        /// <summary>
        ///     错误处理
        /// </summary>
        /// <param name="error">异常信息</param>
        /// <param name="TmphRequest">请求信息</param>
        private void onError(Exception error, TmphRequest TmphRequest)
        {
            if (TmphRequest.IsErrorOut)
            {
                TmphLog.Default.Add(error, (TmphRequest.IsErrorOutUri ? TmphRequest.Uri.AbsoluteUri : null) + " 抓取失败",
                    !TmphRequest.IsErrorOutUri);
            }
        }

        /// <summary>
        ///     数据解压缩
        /// </summary>
        /// <param name="data">原始数据</param>
        /// <param name="TmphRequest">请求信息</param>
        /// <returns>解压缩数据</returns>
        private byte[] deCompress(byte[] data, TmphRequest TmphRequest)
        {
            TmphStream compressionStream = this.compressionStream;
            if (compressionStream != null)
            {
                try
                {
                    return compressionStream.GetDeCompress(data).ToArray();
                }
                catch (Exception error)
                {
                    onError(error, TmphRequest);
                    return null;
                }
            }
            return data;
        }

        /// <summary>
        ///     根据提交类型获取编码字符集
        /// </summary>
        /// <param name="contentType">提交类型</param>
        /// <returns>编码字符集</returns>
        private static Encoding getEncoding(string contentType)
        {
            foreach (TmphSubString value in contentType.Split(';'))
            {
                TmphSubString key = value.Trim();
                if (key.StartsWith(CharsetName))
                {
                    try
                    {
                        Encoding encoding = Encoding.GetEncoding(key.Substring(CharsetName.Length));
                        return encoding;
                    }
                    catch
                    {
                    }
                }
            }
            return null;
        }

        //public void mergeDomainCookie(Uri address, Cookie[] replaceCookie)
        //{
        //    if (address != null)
        //    {
        //        Cookie newCookie;
        //        CookieCollection cookies = new CookieCollection();
        //        Dictionary<string, Cookie> replaceCookies = null;
        //        if (replaceCookie != null && replaceCookie.Length != 0)
        //        {
        //            replaceCookies = dictionary.CreateOnly<string, Cookie>();
        //            foreach (Cookie cookie in replaceCookie)
        //            {
        //                if (replaceCookies.ContainsKey(cookie.Name)) replaceCookies[cookie.Name] = cookie;
        //                else replaceCookies.Add(cookie.Name, cookie);
        //            }
        //        }
        //        bool isCookie;
        //        foreach (Cookie cookie in cookieContainer.GetCookies(address))
        //        {
        //            cookies.Add(newCookie = (isCookie = replaceCookies != null && replaceCookies.ContainsKey(cookie.Name)) ? replaceCookies[cookie.Name] : new Cookie(cookie.Name, cookie.Value, cookie.Path));
        //            if (isCookie) replaceCookies.Remove(cookie.Name);
        //            newCookie.Expires = cookie.Expires;
        //            cookie.Expired = true;
        //        }
        //        if (replaceCookies != null)
        //        {
        //            foreach (Cookie cookie in replaceCookies.Values) cookies.Add(cookie);
        //        }
        //        if (cookies.Count != 0) cookieContainer.Add(address, cookies);
        //    }
        //}
        //private void BugFix_CookieDomain(CookieContainer cookieContainer)
        //{
        //    Hashtable table = (Hashtable)typeof(CookieContainer).InvokeMember("m_domainTable",
        //                               System.Reflection.BindingFlags.NonPublic |
        //                               System.Reflection.BindingFlags.GetField |
        //                               System.Reflection.BindingFlags.Instance,
        //                               null,
        //                               cookieContainer,
        //                               new object[] { });
        //    ArrayList keys = new ArrayList(table.Keys);
        //    foreach (string keyObj in keys)
        //    {
        //        string key = (keyObj as string);
        //        if (key[0] == '.')
        //        {
        //            string newKey = key.Remove(0, 1);
        //            table[newKey] = table[keyObj];
        //        }
        //    }
        //}
        //public void addCookie(string address, CookieCollection cookies)
        //{
        //    cookieContainer.Add(new Uri(address), cookies);
        //}
        /// <summary>
        ///     URI请求信息
        /// </summary>
        public struct TmphRequest
        {
            /// <summary>
            ///     POST内容
            /// </summary>
            public NameValueCollection Form;

            /// <summary>
            ///     出错时是否写日志
            /// </summary>
            public bool IsErrorOut;

            /// <summary>
            ///     出错时是否输出页面地址
            /// </summary>
            public bool IsErrorOutUri;

            /// <summary>
            ///     来源页面地址
            /// </summary>
            public string RefererUrl;

            /// <summary>
            ///     页面地址
            /// </summary>
            public Uri Uri;

            /// <summary>
            ///     页面地址
            /// </summary>
            public string UriString
            {
                set { Uri = TmphUri.Create(value); }
            }

            /// <summary>
            ///     清除请求信息
            /// </summary>
            public void Clear()
            {
                Uri = null;
                Form = null;
                RefererUrl = null;
                IsErrorOut = IsErrorOutUri = true;
            }

            /// <summary>
            ///     URI隐式转换为请求信息
            /// </summary>
            /// <param name="uri">URI</param>
            /// <returns>请求信息</returns>
            public static implicit operator TmphRequest(Uri uri)
            {
                return new TmphRequest { Uri = uri, IsErrorOut = true, IsErrorOutUri = true };
            }

            /// <summary>
            ///     URI隐式转换为请求信息
            /// </summary>
            /// <param name="uri">URI</param>
            /// <returns>请求信息</returns>
            public static implicit operator TmphRequest(string uri)
            {
                return new TmphRequest { Uri = new Uri(uri), IsErrorOut = true, IsErrorOutUri = true };
            }
        }

        /// <summary>
        ///     异步抓取页面HTML代码
        /// </summary>
        private sealed class TmphDataCrawler
        {
            /// <summary>
            ///     抓取回调
            /// </summary>
            private readonly DownloadDataCompletedEventHandler onDownloadHandle;

            /// <summary>
            ///     上传事件
            /// </summary>
            private readonly UploadValuesCompletedEventHandler onUploadHandle;

            /// <summary>
            ///     抓取回调事件
            /// </summary>
            private Action<byte[]> onCrawlData;

            /// <summary>
            ///     URI请求信息
            /// </summary>
            private TmphRequest TmphRequest;

            /// <summary>
            ///     Web客户端
            /// </summary>
            private TmphWebClient webClient;

            /// <summary>
            ///     异步抓取页面HTML代码
            /// </summary>
            private TmphDataCrawler()
            {
                onDownloadHandle = onDownload;
                onUploadHandle = onUpload;
            }

            /// <summary>
            ///     抓取页面字节流
            /// </summary>
            private void crawl()
            {
                if (TmphRequest.Uri != null)
                {
                    try
                    {
                        webClient.Headers.Add(TmphHeader.UserAgent, webClient.UserAgent);
                        webClient.Headers.Add(TmphHeader.Referer,
                            TmphRequest.RefererUrl == null || TmphRequest.RefererUrl.Length == 0
                                ? TmphRequest.Uri.AbsoluteUri
                                : TmphRequest.RefererUrl);
                        if (TmphRequest.Form == null) downloadData();
                        else upload();
                        return;
                    }
                    catch (Exception error)
                    {
                        webClient.onError(error, TmphRequest);
                    }
                }
                onCrawl(null);
            }

            /// <summary>
            ///     抓取页面字节流
            /// </summary>
            private void downloadData()
            {
                try
                {
                    webClient.DownloadDataCompleted += onDownloadHandle;
                    webClient.DownloadDataAsync(TmphRequest.Uri, webClient);
                }
                catch (Exception error)
                {
                    webClient.DownloadDataCompleted -= onDownloadHandle;
                    webClient.onError(error, TmphRequest);
                    onCrawl(null);
                }
            }

            /// <summary>
            ///     抓取页面字节流
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void onDownload(object sender, DownloadDataCompletedEventArgs e)
            {
                try
                {
                    webClient.DownloadDataCompleted -= onDownloadHandle;
                    if (e.Error != null) webClient.onError(e.Error, TmphRequest);
                }
                finally
                {
                    onCrawl(e.Error == null ? webClient.deCompress(e.Result, TmphRequest) : null);
                }
            }

            /// <summary>
            ///     抓取页面字节流
            /// </summary>
            private void upload()
            {
                try
                {
                    webClient.UploadValuesCompleted += onUploadHandle;
                    webClient.UploadValuesAsync(TmphRequest.Uri, Web.TmphHttp.TmphMethodType.POST.ToString(), TmphRequest.Form,
                        webClient);
                }
                catch (Exception error)
                {
                    webClient.UploadValuesCompleted -= onUploadHandle;
                    webClient.onError(error, TmphRequest);
                    onCrawl(null);
                }
            }

            /// <summary>
            ///     抓取页面字节流
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void onUpload(object sender, UploadValuesCompletedEventArgs e)
            {
                try
                {
                    webClient.UploadValuesCompleted -= onUploadHandle;
                    if (e.Error != null) webClient.onError(e.Error, TmphRequest);
                }
                finally
                {
                    onCrawl(e.Error == null ? webClient.deCompress(e.Result, TmphRequest) : null);
                }
            }

            /// <summary>
            ///     抓取回调
            /// </summary>
            /// <param name="data">抓取数据</param>
            private void onCrawl(byte[] data)
            {
                try
                {
                    onCrawlData(data);
                }
                finally
                {
                    webClient = null;
                    onCrawlData = null;
                    TmphRequest.Clear();
                    TmphTypePool<TmphDataCrawler>.Push(this);
                }
            }

            /// <summary>
            ///     异步抓取页面HTML代码
            /// </summary>
            /// <param name="webClient">Web客户端</param>
            /// <param name="onCrawlData">抓取回调事件</param>
            /// <param name="TmphRequest">URI请求信息</param>
            public static void Crawl(TmphWebClient webClient, Action<byte[]> onCrawlData, TmphRequest TmphRequest)
            {
                TmphDataCrawler TmphDataCrawler = TmphTypePool<TmphDataCrawler>.Pop();
                if (TmphDataCrawler == null)
                {
                    try
                    {
                        TmphDataCrawler = new TmphDataCrawler();
                    }
                    catch (Exception error)
                    {
                        TmphLog.Default.Add(error, null, false);
                        onCrawlData(null);
                        return;
                    }
                }
                TmphDataCrawler.webClient = webClient;
                TmphDataCrawler.onCrawlData = onCrawlData;
                TmphDataCrawler.TmphRequest = TmphRequest;
                TmphDataCrawler.crawl();
            }
        }

        /// <summary>
        ///     异步抓取页面HTML代码
        /// </summary>
        private struct TmphHtmlCrawler
        {
            /// <summary>
            ///     页面编码
            /// </summary>
            public Encoding Encoding;

            /// <summary>
            ///     异步事件
            /// </summary>
            public Action<string> OnCrawlHtml;

            /// <summary>
            ///     Web客户端
            /// </summary>
            public TmphWebClient WebClient;

            /// <summary>
            ///     抓取页面字节流事件
            /// </summary>
            /// <param name="data">页面字节流</param>
            public void onCrawlData(byte[] data)
            {
                OnCrawlHtml(TmphChineseEncoder.ToString(data, Encoding ?? WebClient.TextEncoding));
            }
        }
    }
}