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
using Laurent.Lee.CLB.Config;
using Laurent.Lee.CLB.Emit;
using Laurent.Lee.CLB.Net.Tcp.Http;
using Laurent.Lee.CLB.Reflection;
using Laurent.Lee.CLB.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using TmphHttp = Laurent.Lee.CLB.Config.TmphHttp;

namespace Laurent.Lee.CLB.Code.CSharp
{
    /// <summary>
    ///     WEB视图配置
    /// </summary>
    public sealed class TmphWebView : TmphWebPage
    {
        /// <summary>
        ///     HTML模板命令类型
        /// </summary>
        public enum TmphCommand
        {
            /// <summary>
            ///     输出绑定的数据
            /// </summary>
            At,

            /// <summary>
            ///     客户端代码段
            /// </summary>
            Client,

            /// <summary>
            ///     子代码段
            /// </summary>
            Value,

            /// <summary>
            ///     循环
            /// </summary>
            Loop,

            /// <summary>
            ///     循环=LOOP
            /// </summary>
            For,

            /// <summary>
            ///     屏蔽代码段输出
            /// </summary>
            Note,

            /// <summary>
            ///     绑定的数据为true非0非null时输出代码
            /// </summary>
            If,

            /// <summary>
            ///     绑定的数据为false或者0或者null时输出代码
            /// </summary>
            Not
        }

        /// <summary>
        ///     默认空WEB视图配置
        /// </summary>
        internal static readonly TmphWebView Null = new TmphWebView();

        /// <summary>
        ///     是否支持ajax视图
        /// </summary>
        public bool IsAjax = true;

        ///// <summary>
        ///// URL重写
        ///// </summary>
        //public string RewritePath;
        /// <summary>
        ///     是否支持服务器端页面
        /// </summary>
        public bool IsPage = true;

        ///// <summary>
        ///// 是否GET查询参数
        ///// </summary>
        //public bool IsQueryGet;
        ///// <summary>
        ///// 是否POST查询参数
        ///// </summary>
        //public bool IsQeuryPost;
        /// <summary>
        ///     查询参数名称
        /// </summary>
        public string QueryName = TmphWeb.Default.ViewQueryName;

        /// <summary>
        ///     WEB调用类型名称
        /// </summary>
        public string TypeCallName;

        /// <summary>
        ///     获取视图加载函数+视图加载函数配置
        /// </summary>
        /// <param name="type">视图类型</param>
        /// <returns>视图加载函数+视图加载函数配置</returns>
        public static TmphKeyValue<TmphMethodInfo, TmphWebView> GetLoadMethod(Type type)
        {
            TmphMethodInfo loadMethod = null;
            foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                )
            {
                if (method.ReturnType == typeof(bool))
                {
                    var loadWebView = method.CustomAttribute<TmphWebView>();
                    if (loadWebView != null)
                    {
                        return new TmphKeyValue<TmphMethodInfo, TmphWebView>(
                            new TmphMethodInfo(method, TmphMemberFilters.Instance), loadWebView);
                    }
                    if (loadMethod == null && method.Name == "loadView" && method.GetParameters().Length != 0)
                    {
                        loadMethod = new TmphMethodInfo(method, TmphMemberFilters.Instance);
                    }
                }
            }
            return new TmphKeyValue<TmphMethodInfo, TmphWebView>(loadMethod, loadMethod == null ? null : Null);
        }

        /// <summary>
        ///     查询字段配置
        /// </summary>
        public sealed class TmphQuery : TmphIgnoreMember
        {
        }

        /// <summary>
        ///     客户端视图绑定类型
        /// </summary>
        public sealed class TmphClientType : Attribute
        {
            /// <summary>
            ///     默认绑定成员名称
            /// </summary>
            public const string DefaultMemberName = "Id";

            /// <summary>
            ///     默认客户端视图绑定类型
            /// </summary>
            internal static readonly TmphClientType Null = new TmphClientType();

            /// <summary>
            ///     绑定成员名称,默认为Id
            /// </summary>
            public string MemberName = DefaultMemberName;

            /// <summary>
            ///     客户端视图绑定类型
            /// </summary>
            public string Name;
        }

        /// <summary>
        ///     是否输出到Ajax视图
        /// </summary>
        public sealed class TmphOutputAjax : TmphIgnoreMember
        {
            /// <summary>
            ///     输出绑定名称
            /// </summary>
            public string BindingName;
        }

        /// <summary>
        ///     WEB视图接口
        /// </summary>
        public interface IWebView : IWebPage
        {
            /// <summary>
            ///     最大接收数据字节数
            /// </summary>
            int MaxPostDataSize { get; }

            /// <summary>
            ///     根据HTTP请求表单值获取内存流最大字节数
            /// </summary>
            /// <param name="value">HTTP请求表单值</param>
            /// <returns>内存流最大字节数</returns>
            int MaxMemoryStreamSize(TmphRequestForm.TmphValue value);
        }

        /// <summary>
        ///     #!转换URL
        /// </summary>
        public struct TmphHashUrl
        {
            /// <summary>
            ///     URL路径
            /// </summary>
            public string Path;

            /// <summary>
            ///     URL查询
            /// </summary>
            public string Query;

            /// <summary>
            ///     对象转换成JSON字符串
            /// </summary>
            /// <param name="jsonStream">JSON输出流</param>
            public void ToJson(TmphCharStream jsonStream)
            {
                var queryLength = Query.Length();
                jsonStream.PrepLength(Path.Length() + queryLength + 4);
                var unsafeStream = jsonStream.Unsafer;
                unsafeStream.Write(Web.TmphAjax.Quote);
                jsonStream.Write(Path);
                if (queryLength != 0)
                {
                    unsafeStream.Write('#');
                    unsafeStream.Write('!');
                    jsonStream.WriteNotNull(Query);
                }
                unsafeStream.Write(Web.TmphAjax.Quote);
            }

            /// <summary>
            ///     转换成?查询字符串
            /// </summary>
            /// <returns></returns>
            public string ToQueryString()
            {
                if (Query == null) return Path;
                return Path + "?" + Query;
            }

            /// <summary>
            ///     转换成?查询字符串
            /// </summary>
            /// <returns></returns>
            public string ToPageString()
            {
                if (Query == null) return Path + "?";
                return Path + "?" + Query + "&";
            }

            /// <summary>
            ///     转换成字符串
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                if (Query == null) return Path;
                return Path + "#!" + Query;
            }
        }

        /// <summary>
        ///     视图错误重定向路径
        /// </summary>
        public struct TmphErrorPath
        {
            /// <summary>
            ///     错误重定向路径
            /// </summary>
            public string ErrorPath;

            /// <summary>
            ///     返回路径
            /// </summary>
            public string ReturnPath;
        }

        /// <summary>
        ///     WEB页面视图
        /// </summary>
        public abstract class TmphView : TmphPage
        {
            /// <summary>
            ///     URL查询字符位图
            /// </summary>
            private static readonly TmphString.TmphAsciiMap UrlQueryMap =
                new TmphString.TmphAsciiMap(TmphUnmanaged.Get(TmphString.TmphAsciiMap.MapBytes),
                    @"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789:/.&=_-!@$%^*()+{}|""<>[]\;',", true);

            /// <summary>
            ///     AJAX回调函数开始符
            /// </summary>
            private static readonly byte[] CallBackStart = TmphAppSetting.Encoding.GetBytes("(");

            /// <summary>
            ///     AJAX回调函数结束符
            /// </summary>
            private static readonly byte[] CallBackEnd = TmphAppSetting.Encoding.GetBytes(")");

            /// <summary>
            ///     URL查询符
            /// </summary>
            private static readonly byte[] UrlQuery = TmphAppSetting.Encoding.GetBytes("?");

            /// <summary>
            ///     URL HASH查询符
            /// </summary>
            private static readonly byte[] UrlHash = TmphAppSetting.Encoding.GetBytes("#!");

            /// <summary>
            ///     AJAX调用名称
            /// </summary>
            private static readonly byte[] AjaxWebCallName = TmphWeb.Default.AjaxWebCallName.GetBytes();

            /// <summary>
            ///     AJAX响应输出参数
            /// </summary>
            private static readonly TmphJsonSerializer.TmphConfig ToJsonConfig = new TmphJsonSerializer.TmphConfig
            {
                IsViewClientType = true,
                GetLoopObject = Web.TmphAjax.GetLoopObject,
                SetLoopObject = Web.TmphAjax.SetLoopObject,
                IsMaxNumberToString = true
            };

            /// <summary>
            ///     临时逻辑变量
            /// </summary>
            protected bool If;

            /// <summary>
            ///     是否ajax请求
            /// </summary>
            protected bool IsAjax;

            /// <summary>
            ///     URL中的#!是否需要转换成?
            /// </summary>
            protected bool IsHashQueryUri;

            /// <summary>
            ///     当前循环数量
            /// </summary>
            protected int LoopCount;

            /// <summary>
            ///     当前循环索引
            /// </summary>
            protected int LoopIndex;

            /// <summary>
            ///     页面描述
            /// </summary>
            protected string ViewDescription;

            /// <summary>
            ///     页面关键字
            /// </summary>
            protected string ViewKeywords;

            /// <summary>
            ///     请求视图路径
            /// </summary>
            public string ViewPath;

            /// <summary>
            ///     最大接收数据字节数
            /// </summary>
            public virtual int MaxPostDataSize
            {
                get { return TmphHttp.Default.MaxPostDataSize << 20; }
            }

            /// <summary>
            ///     WEB视图配置
            /// </summary>
            protected virtual TmphWebView WebView
            {
                get { return Null; }
            }

            /// <summary>
            ///     当前时间
            /// </summary>
            protected TmphTime ServerTime
            {
                get { return new TmphTime { Now = TmphDate.Now }; }
            }

            /// <summary>
            ///     清除当前请求数据
            /// </summary>
            protected override void Clear()
            {
                base.Clear();
                IsHashQueryUri = IsAjax = false;
                ViewKeywords = ViewDescription = ViewPath = null;
            }

            /// <summary>
            ///     HTTP请求头部处理
            /// </summary>
            /// <returns>是否成功</returns>
            protected virtual unsafe bool LoadHeader()
            {
                var path = RequestHeader.Path;
                if (path.Count == AjaxWebCallName.Length)
                {
                    fixed (byte* pathFixed = path.Array)
                    {
                        if (Unsafe.TmphMemory.Equal(AjaxWebCallName, pathFixed + path.StartIndex, path.Count))
                            path = RequestHeader.AjaxCallName;
                    }
                }
                if (path.Count == 0) ViewPath = string.Empty;
                else
                {
                    fixed (byte* pathFixed = path.Array)
                    {
                        ViewPath = TmphString.DeSerialize(pathFixed + path.StartIndex, -path.Count);
                    }
                }
                return true;
            }

            /// <summary>
            ///     WEB视图加载
            /// </summary>
            /// <returns>是否成功</returns>
            protected virtual bool LoadView()
            {
                return true;
            }

            /// <summary>
            ///     查询参数解析
            /// </summary>
            /// <typeparam name="TQueryType">查询参数类型</typeparam>
            /// <param name="query">查询参数</param>
            /// <returns>是否成功</returns>
            protected bool ParseQuery<TQueryType>(ref TQueryType query) where TQueryType : struct
            {
                if (Form != null && Form.Json != null)
                {
                    if (Form.Json.Length != 0
                        && !TmphJsonParser.Parse(Form.Json, ref query))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!RequestHeader.ParseQuery(ref query)) return false;
                    var queryJson = RequestHeader.QueryJson;
                    if (queryJson.Length != 0
                        && !TmphJsonParser.Parse(queryJson, ref query))
                    {
                        return false;
                    }
                }
                return true;
            }

            /// <summary>
            ///     输出HTML片段
            /// </summary>
            /// <param name="url">URL</param>
            protected void ResponseHtml(TmphHashUrl url)
            {
                ResponseData(url.Path);
                if (url.Query.Length() != 0)
                {
                    ResponseUrlQueryHash();
                    ResponseData(url.Query);
                }
            }

            /// <summary>
            ///     输出URL HASH查询符
            /// </summary>
            private void ResponseUrlQueryHash()
            {
                if (ResponseEncoding.CodePage == Encoding.Unicode.CodePage)
                {
                    if (IsHashQueryUri) Response.BodyStream.Write((short)'?');
                    else Response.BodyStream.Write('#' + ('!' << 16));
                }
                else if (ResponseEncoding.CodePage == TmphAppSetting.Encoding.CodePage)
                {
                    Response.BodyStream.Write(IsHashQueryUri ? UrlQuery : UrlHash);
                }
                else
                {
                    if (IsHashQueryUri) Response.BodyStream.Write((byte)'?');
                    else Response.BodyStream.Write((short)('#' + ('!' << 8)));
                }
            }

            /// <summary>
            ///     输出HTML片段
            /// </summary>
            /// <param name="html">HTML片段</param>
            protected void ResponseHtml(TmphSubString html)
            {
                if (html.Length != 0)
                {
                    if (ResponseEncoding.CodePage == Encoding.Unicode.CodePage)
                    {
                        TmphHtml.HtmlEncoder.ToHtml(html, Response.BodyStream);
                    }
                    else ResponseData(TmphHtml.HtmlEncoder.ToHtml(html));
                }
            }

            /// <summary>
            ///     输出HTML片段
            /// </summary>
            /// <param name="html">HTML片段</param>
            protected void ResponseHtml(string html)
            {
                ResponseHtml((TmphSubString)html);
            }

            /// <summary>
            ///     输出HTML片段
            /// </summary>
            /// <param name="url">URL</param>
            protected void ResponseHtmlUrl(TmphHashUrl url)
            {
                ResponseHtml(url.Path);
                if (url.Query.Length() != 0)
                {
                    ResponseUrlQueryHash();
                    ResponseHtml(url.Query);
                }
            }

            /// <summary>
            ///     输出HTML片段
            /// </summary>
            /// <param name="html">HTML片段</param>
            protected void ResponseTextArea(string html)
            {
                ResponseHtml((TmphSubString)html);
            }

            /// <summary>
            ///     输出HTML片段
            /// </summary>
            /// <param name="html">HTML片段</param>
            protected void ResponseTextArea(TmphSubString html)
            {
                ResponseHtml(html);
                //if (html.Length != 0)
                //{
                //    if (responseEncoding.CodePage == Encoding.Unicode.CodePage)
                //    {
                //        Laurent.Lee.CLB.Web.html.TextAreaEncoder.ToHtml(html, Response.BodyStream);
                //    }
                //    else this.response(Laurent.Lee.CLB.Web.html.TextAreaEncoder.ToHtml(html));
                //}
            }

            /// <summary>
            ///     输出HTML片段
            /// </summary>
            /// <param name="url">URL</param>
            protected void ResponseTextArea(TmphHashUrl url)
            {
                ResponseHtmlUrl(url);
                //responseTextArea(url.Path);
                //if (url.Query.length() != 0)
                //{
                //    responseUrlQueryHash();
                //    responseTextArea(url.Query);
                //}
            }

            /// <summary>
            ///     AJAX回调输出
            /// </summary>
            /// <returns>是否存在回调参数</returns>
            private bool ResponseAjaxCallBack()
            {
                if (RequestHeader != null)
                {
                    if (RequestHeader.IsWebSocket)
                    {
                        var callBack = Socket.GetWebSocketCallBack(SocketIdentity);
                        if (callBack.Length != 0)
                        {
                            ResponseData(callBack);
                            ResponseCallBackStart();
                            return true;
                        }
                    }
                    else
                    {
                        var callBack = RequestHeader.AjaxCallBackName;
                        if (callBack.Count != 0)
                        {
                            ResponseData(TmphFormQuery.JavascriptUnescape(callBack));
                            ResponseCallBackStart();
                            return true;
                        }
                    }
                }
                return false;
            }

            /// <summary>
            ///     输出AJAX回调函数开始符
            /// </summary>
            private void ResponseCallBackStart()
            {
                if (ResponseEncoding.CodePage == Encoding.Unicode.CodePage)
                {
                    Response.BodyStream.Write('(');
                }
                else if (ResponseEncoding.CodePage == TmphAppSetting.Encoding.CodePage)
                {
                    Response.BodyStream.Write(CallBackStart);
                }
                else Response.BodyStream.Write((byte)'(');
            }

            /// <summary>
            ///     AJAX响应输出
            /// </summary>
            public void AjaxResponse()
            {
                if (RequestHeader != null)
                {
                    var response = TmphResponse.Get(true);
                    Response = response;
                    AjaxResponse(ref response);
                }
            }

            /// <summary>
            ///     AJAX响应输出
            /// </summary>
            /// <param name="response">HTTP响应</param>
            public void AjaxResponse(ref TmphResponse response)
            {
                try
                {
                    if (Response == response)
                    {
                        response.SetJsContentType(DomainServer);
                        if (ResponseAjaxCallBack()) ResponseCallBackEnd();
                        ResponseEnd(ref response);
                    }
                }
                finally
                {
                    TmphResponse.Push(ref response);
                }
            }

            ///// <summary>
            ///// AJAX响应输出
            ///// </summary>
            ///// <typeparam name="TValueType">输出数据类型</typeparam>
            ///// <param name="value">输出数据</param>
            //public unsafe void AjaxResponse<TValueType>(TValueType value) where TValueType : json.IToJson
            //{
            //    AjaxResponse((json.IToJson)value);
            //}
            /// <summary>
            ///     AJAX响应输出
            /// </summary>
            /// <typeparam name="TValueType">输出数据类型</typeparam>
            /// <param name="value">输出数据</param>
            public void AjaxResponse<TValueType>(ref TValueType value) where TValueType : struct
            {
                if (RequestHeader != null)
                {
                    var response = TmphResponse.Get(true);
                    Response = response;
                    AjaxResponse(ref value, ref response);
                }
            }

            /// <summary>
            ///     输出AJAX回调函数结束符
            /// </summary>
            private void ResponseCallBackEnd()
            {
                if (ResponseEncoding.CodePage == Encoding.Unicode.CodePage) Response.BodyStream.Write((short)')');
                else if (ResponseEncoding.CodePage == TmphAppSetting.Encoding.CodePage)
                {
                    Response.BodyStream.Write(CallBackEnd);
                }
                else Response.BodyStream.Write((byte)')');
            }

            /// <summary>
            ///     AJAX响应输出
            /// </summary>
            /// <typeparam name="TValueType">输出数据类型</typeparam>
            /// <param name="response">HTTP响应</param>
            /// <param name="value">输出数据</param>
            public void AjaxResponse<TValueType>(ref TValueType value, ref TmphResponse response)
                where TValueType : struct
            {
                try
                {
                    if (Response == response)
                    {
                        response.SetJsContentType(DomainServer);
                        var bodyStream = response.BodyStream;
                        var isCallBack = ResponseAjaxCallBack();
                        if (ResponseEncoding.CodePage == Encoding.Unicode.CodePage)
                        {
                            var jsonStream = bodyStream.ToCharStream();
                            try
                            {
                                TmphJsonSerializer.ToJson(value, jsonStream, ToJsonConfig);
                            }
                            finally
                            {
                                bodyStream.From(jsonStream);
                            }
                        }
                        else
                        {
                            ResponseData(TmphJsonSerializer.ToJson(value, ToJsonConfig));
                        }
                        if (isCallBack) ResponseCallBackEnd();
                        ResponseEnd(ref response);
                    }
                }
                finally
                {
                    TmphResponse.Push(ref response);
                }
            }

            /// <summary>
            ///     AJAX响应输出
            /// </summary>
            /// <typeparam name="TValueType">输出数据类型</typeparam>
            /// <param name="value">输出数据</param>
            public void AjaxResponse<TValueType>(TValueType value) where TValueType : struct
            {
                if (RequestHeader != null)
                {
                    var response = TmphResponse.Get(true);
                    Response = response;
                    AjaxResponse(value, ref response);
                }
            }

            /// <summary>
            ///     AJAX响应输出
            /// </summary>
            /// <typeparam name="TValueType">输出数据类型</typeparam>
            /// <param name="response">HTTP响应输出</param>
            /// <param name="value">输出数据</param>
            public void AjaxResponse<TValueType>(TValueType value, ref TmphResponse response) where TValueType : struct
            {
                try
                {
                    if (Response == response)
                    {
                        response.SetJsContentType(DomainServer);
                        var bodyStream = response.BodyStream;
                        var isCallBack = ResponseAjaxCallBack();
                        if (ResponseEncoding.CodePage == Encoding.Unicode.CodePage)
                        {
                            var jsonStream = bodyStream.ToCharStream();
                            try
                            {
                                TmphJsonSerializer.ToJson(value, jsonStream, ToJsonConfig);
                            }
                            finally
                            {
                                bodyStream.From(jsonStream);
                            }
                        }
                        else
                        {
                            ResponseData(TmphJsonSerializer.ToJson(value, ToJsonConfig));
                        }
                        if (isCallBack) ResponseCallBackEnd();
                        ResponseEnd(ref response);
                    }
                }
                finally
                {
                    TmphResponse.Push(ref response);
                }
            }

            /// <summary>
            ///     输出JSON字符串
            /// </summary>
            /// <param name="jsonStream">JSON字符流</param>
            /// <param name="response">HTTP响应输出</param>
            /// <returns>是否操作成功</returns>
            protected bool ResponseJs(TmphCharStream jsonStream, ref TmphResponse response)
            {
                var isCallBack = ResponseAjaxCallBack();
                if (ResponseEncoding.CodePage == Encoding.Unicode.CodePage)
                {
                    Web.TmphAjax.FormatJavascript(jsonStream, response.BodyStream);
                }
                else
                {
                    ResponseData(Web.TmphAjax.FormatJavascript(jsonStream));
                }
                if (isCallBack) ResponseCallBackEnd();
                return ResponseEnd(ref response);
            }

            /// <summary>
            ///     HTTP响应输出处理
            /// </summary>
            /// <returns>是否成功</returns>
            protected virtual bool ResponseHttpHandle()
            {
                TmphLog.Error.Add(GetType().FullName, false, true);
                return false;
            }

            /// <summary>
            ///     AJAX响应输出处理
            /// </summary>
            /// <param name="js">JS输出流</param>
            protected virtual void Ajax(TmphCharStream js)
            {
                TmphLog.Error.Throw(GetType().FullName, false, true);
            }

            /// <summary>
            ///     AJAX视图异步回调输出
            /// </summary>
            protected void Callback()
            {
                var response = CancelAsynchronous();
                Callback(ref response);
            }

            /// <summary>
            ///     AJAX视图异步回调输出
            /// </summary>
            protected unsafe void Callback(ref TmphResponse response)
            {
                var identity = SocketIdentity;
                var TmphBuffer = new TmphPointer();
                try
                {
                    if (IsAjax)
                    {
                        TmphBuffer = TmphUnmanagedPool.StreamBuffers.Get();
                        response.SetJsContentType(DomainServer);
                        using (var js = new TmphCharStream(TmphBuffer.Char, TmphUnmanagedPool.StreamBuffers.Size >> 1))
                        {
                            Ajax(js);
                            if (ResponseJs(js, ref response)) return;
                        }
                    }
                    else
                    {
                        var isResponse = ResponseHttpHandle();
                        if (isResponse && ResponseEnd(ref response)) return;
                    }
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
                finally
                {
                    TmphUnmanagedPool.StreamBuffers.Push(ref TmphBuffer);
                    TmphResponse.Push(ref response);
                }
                if (Socket.ResponseError(identity, TmphResponse.TmphState.ServerError500)) PushPool();
            }

            /// <summary>
            ///     根据HTTP请求表单值获取内存流最大字节数
            /// </summary>
            /// <param name="value">HTTP请求表单值</param>
            /// <returns>内存流最大字节数</returns>
            public virtual int MaxMemoryStreamSize(TmphRequestForm.TmphValue value)
            {
                return TmphHttp.Default.MaxMemoryStreamSize << 10;
            }

            /// <summary>
            ///     根据HTTP请求表单值获取内存流最大字节数(单位:KB)
            /// </summary>
            /// <param name="value">
            ///     HTTP请求表单值
            ///     HTTP请求表单值
            /// </param>
            /// <returns>内存流最大字节数(单位:KB)</returns>
            /// <summary>
            ///     根据HTTP请求表单值获取保存文件全称
            /// </summary>
            /// <returns>文件全称</returns>
            public virtual string GetSaveFileName(TmphRequestForm.TmphValue value)
            {
                return null;
            }

            /// <summary>
            ///     加载HTML数据
            /// </summary>
            /// <param name="fileName">HTML文件</param>
            /// <param name="htmlCount">HTML片段数量验证</param>
            /// <returns>HTML数据,失败返回null</returns>
            protected byte[][] LoadHtml(string fileName, int htmlCount)
            {
                if (File.Exists(fileName = WorkPath + fileName))
                {
                    try
                    {
                        var treeBuilder = new TmphTreeBuilder(File.ReadAllText(fileName, DomainServer.ResponseEncoding), 1);
                        if (treeBuilder.HtmlCount == htmlCount)
                        {
                            return treeBuilder.Htmls.getArray(value => DomainServer.ResponseEncoding.GetBytes(value));
                        }
                        TmphLog.Error.Add("HTML模版文件不匹配 " + fileName, false, TmphLog.TmphCacheType.Last);
                    }
                    catch (Exception error)
                    {
                        TmphLog.Error.Add(error, fileName, TmphLog.TmphCacheType.Last);
                    }
                }
                else TmphLog.Error.Add("没有找到HTML模版文件 " + fileName, false, TmphLog.TmphCacheType.Last);
                return null;
            }

            /// <summary>
            ///     服务器端时间
            /// </summary>
            [TmphClientType(Name = "Laurent.Lee.CLB.ServerTime", MemberName = null)]
            protected struct TmphTime
            {
                /// <summary>
                ///     当前时间
                /// </summary>
                [TmphOutputAjax]
                public DateTime Now;
            }
        }

        /// <summary>
        ///     WEB页面视图
        /// </summary>
        /// <typeparam name="TPageType">WEB页面类型</typeparam>
        public abstract class TmphView<TPageType> : TmphView where TPageType : TmphView<TPageType>
        {
            /// <summary>
            ///     是否使用对象池
            /// </summary>
            private bool _isPool;

            ///// <summary>
            ///// 是否已经加载HTTP请求头部
            ///// </summary>
            //private int isLoadHeader;
            /// <summary>
            ///     当前WEB页面视图
            /// </summary>
            private TPageType _thisPage;

            /// <summary>
            ///     WEB页面回收
            /// </summary>
            internal override void PushPool()
            {
                if (_isPool)
                {
                    _isPool = false;
                    Clear();
                    if (_thisPage == null) _thisPage = (TPageType)this;
                    TmphTypePool<TPageType>.Push(_thisPage);
                }
            }

            /// <summary>
            ///     HTTP请求头部处理
            /// </summary>
            /// <param name="socketIdentity">套接字操作编号</param>
            /// <param name="request">HTTP请求头部</param>
            /// <param name="isPool">是否使用WEB页面池</param>
            /// <returns>是否成功</returns>
            internal override bool LoadHeader(long socketIdentity, TmphRequestHeader request, bool isPool)
            {
                SocketIdentity = socketIdentity;
                RequestHeader = request;
                ResponseEncoding = request.IsWebSocket ? Encoding.UTF8 : DomainServer.ResponseEncoding;
                IsHashQueryUri = !RequestHeader.IsHash && RequestHeader.IsSearchEngine;
                try
                {
                    if (LoadHeader())
                    {
                        _isPool = isPool;
                        return true;
                    }
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
                if (Socket.ResponseError(SocketIdentity, TmphResponse.TmphState.ServerError500) && isPool) PushPool();
                return false;
            }

            /// <summary>
            ///     加载查询参数
            /// </summary>
            /// <param name="form">HTTP请求表单</param>
            /// <returns>是否成功</returns>
            protected virtual bool Load(TmphRequestForm form)
            {
                Form = form;
                return LoadView();
            }

            /// <summary>
            ///     加载查询参数
            /// </summary>
            /// <param name="form">HTTP请求表单</param>
            /// <param name="isAjax">是否ajax请求</param>
            internal override void Load(TmphRequestForm form, bool isAjax)
            {
                var identity = SocketIdentity;
                var socket = Socket;
                TmphResponse response = null;
                try
                {
                    Response = (response = TmphResponse.Get(true));
                    var asynchronousIdentity = AsynchronousIdentity;
                    IsAjax = isAjax;
                    if (Load(form))
                    {
                        if (IsAsynchronous || asynchronousIdentity != AsynchronousIdentity) response = null;
                        else Callback(ref response);
                        return;
                    }
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
                finally
                {
                    TmphResponse.Push(ref response);
                }
                if (socket.ResponseError(identity, TmphResponse.TmphState.ServerError500)) PushPool();
            }
        }

        ///// <summary>
        ///// WEB页面视图
        ///// </summary>
        //public abstract class view<pageType, queryType> : view<pageType>
        //    where pageType : view<pageType, queryType>
        //{
        //    /// <summary>
        //    /// 请求查询数据
        //    /// </summary>
        //    protected queryType query;
        //    /// <summary>
        //    /// 请求查询数据
        //    /// </summary>
        //    /// <returns>新建请求查询数据</returns>
        //    protected virtual queryType newQuery()
        //    {
        //        return Laurent.Lee.CLB.Code.constructor<queryType>.New();
        //    }
        //    /// <summary>
        //    /// 加载查询参数
        //    /// </summary>
        //    /// <param name="request">HTTP请求表单</param>
        //    /// <returns>是否成功</returns>
        //    protected override bool load(Laurent.Lee.CLB.Net.Tcp.Http.requestForm form)
        //    {
        //        this.form = form;
        //        query = newQuery();
        //        if (form != null && form.Json != null)
        //        {
        //            if (form.Json.Length != 0
        //                && !Laurent.Lee.CLB.Emit.jsonParser.Parse(form.Json, ref query))
        //            {
        //                return false;
        //            }
        //        }
        //        else
        //        {
        //            if (!requestHeader.ParseQuery(ref query)) return false;
        //            subString queryJson = requestHeader.QueryJson;
        //            if (queryJson.Length != 0
        //                && !Laurent.Lee.CLB.Emit.jsonParser.Parse(queryJson, ref query))
        //            {
        //                return false;
        //            }
        //        }
        //        return loadView();
        //    }
        //}
        /// <summary>
        ///     HTML模板建树器
        /// </summary>
        public class TmphTreeBuilder
        {
            /// <summary>
            ///     =@取值字符位图尺寸
            /// </summary>
            private const int AtMapSize = 128;

            /// <summary>
            ///     模板命令HASH匹配数据容器尺寸
            /// </summary>
            private const int CommandUniqueHashDataCount = 16;

            /// <summary>
            ///     模板命令HASH匹配数据尺寸
            /// </summary>
            private const int CommandUniqueHashDataSize = 16;

            ///// <summary>
            ///// 视图body替换内容
            ///// </summary>
            //public const string ViewBody = @"<div id=""fastCSharpViewOver"" style=""position:fixed;left:0px;top:0px;width:100%;height:100%;z-index:10000;display:block;background-color:#ffffff"">视图数据加载中...</div>";
            /// <summary>
            ///     @取值command
            /// </summary>
            private static readonly string AtCommand = TmphCommand.At.ToString();

            /// <summary>
            ///     图片源
            /// </summary>
            private static readonly Regex ImageSrc = new Regex(@" @src=""", RegexOptions.Compiled);

            /// <summary>
            ///     =@取值字符位图
            /// </summary>
            private static readonly TmphFixedMap AtMap;

            /// <summary>
            ///     模板命令HASH匹配数据
            /// </summary>
            private static readonly TmphPointer CommandUniqueHashData;

            /// <summary>
            ///     客户端命令索引位置
            /// </summary>
            private static readonly int ClientCommandIndex;

            /// <summary>
            ///     HTML片段
            /// </summary>
            private readonly Dictionary<TmphHashString, int> _htmls = TmphDictionary.CreateHashString<int>();

            /// <summary>
            ///     是否仅仅生成HTML
            /// </summary>
            private readonly int _isOnlyHtml;

            /// <summary>
            ///     建树器
            /// </summary>
            private readonly TmphTreeBuilder<TmphNode, TmphTag> _tree;

            static unsafe TmphTreeBuilder()
            {
                var datas = TmphUnmanaged.Get(true, AtMapSize, CommandUniqueHashDataCount * CommandUniqueHashDataSize);
                var dataIndex = 0;
                AtMap = new TmphFixedMap(datas[dataIndex++]);
                CommandUniqueHashData = datas[dataIndex];

                AtMap.Set('0', 10);
                AtMap.Set('A', 26);
                AtMap.Set('a', 26);
                AtMap.Set('.');
                AtMap.Set('_');

                for (
                    byte* start = CommandUniqueHashData.Byte,
                        end = start + CommandUniqueHashDataCount * CommandUniqueHashDataSize;
                    start != end;
                    start += CommandUniqueHashDataSize)
                    *(int*)start = int.MinValue;
                foreach (TmphCommand command in Enum.GetValues(typeof(TmphCommand)))
                {
                    var commandString = command.ToString();
                    if (sizeof(int) + (commandString.Length << 1) > CommandUniqueHashDataSize)
                    {
                        TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
                    }
                    fixed (char* commandFixed = commandString)
                    {
                        var code = ((*commandFixed >> 1) ^ (commandFixed[commandString.Length >> 2] >> 2)) &
                                   ((1 << 4) - 1);
                        if (command == TmphCommand.Client) ClientCommandIndex = code;
                        var data = CommandUniqueHashData.Byte + (code * CommandUniqueHashDataSize);
                        if (*(int*)data != int.MinValue) TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
                        *(int*)data = commandString.Length;
                        Unsafe.TmphMemory.Copy(commandFixed, data + sizeof(int), commandString.Length << 1);
                    }
                }
            }

            /// <summary>
            ///     HTML模板建树器
            /// </summary>
            /// <param name="html">HTML</param>
            /// <param name="isOnlyHtml"></param>
            public TmphTreeBuilder(string html, int isOnlyHtml)
            {
                if ((_isOnlyHtml = isOnlyHtml) == 0) _tree = new TmphTreeBuilder<TmphNode, TmphTag>();
                Create(FormatHtml(html));
            }

            /// <summary>
            ///     HTML片段数量
            /// </summary>
            public int HtmlCount
            {
                get { return _htmls.Count; }
            }

            /// <summary>
            ///     HTML片段
            /// </summary>
            internal string[] Htmls
            {
                get
                {
                    var values = new string[_htmls.Count];
                    foreach (var value in _htmls) values[value.Value] = value.Key.ToString();
                    return values;
                }
            }

            /// <summary>
            ///     树节点
            /// </summary>
            public TmphNode Boot { get; private set; }

            /// <summary>
            ///     根据HTML获取模板树
            /// </summary>
            /// <param name="html">HTML</param>
            private unsafe void Create(string html)
            {
                if (html != null)
                {
                    if (html.Length >= 3)
                    {
                        var atFixedMap = AtMap;
                        var commandUniqueHashDataFixed = CommandUniqueHashData.Byte;
                        fixed (char* htmlFixed = html)
                        {
                            char* start = htmlFixed, end = htmlFixed + html.Length - 1, current = htmlFixed;
                            var endChar = *end;
                            do
                            {
                                for (*end = '<'; *current != '<'; ++current)
                                {
                                }
                                if (current == end) break;
                                if (*++current == '!' && *(int*)++current == ('-' | ('-' << 16)))
                                {
                                    var tagStart = current += 2;
                                    for (*end = '>'; *current != '>'; ++current)
                                    {
                                    }
                                    if (current == end && endChar != '>') break;
                                    if (*(int*)(current -= 2) == ('-' | ('-' << 16)))
                                    {
                                        var contentStart = tagStart;
                                        for (*current = ':'; *contentStart != ':'; ++contentStart)
                                        {
                                        }
                                        *current = '-';
                                        int tagLength = (int)(contentStart - tagStart),
                                            tagIndex = (((*tagStart >> 1) ^ (tagStart[tagLength >> 2] >> 2)) &
                                                        ((1 << 4) - 1));
                                        var hashData = commandUniqueHashDataFixed + (tagIndex * CommandUniqueHashDataSize);
                                        if (*(int*)hashData == tagLength &&
                                            Unsafe.TmphMemory.Equal(tagStart, hashData + sizeof(int),
                                                tagLength << 1))
                                        {
                                            var tag = new TmphTag
                                            {
                                                TagType = TmphTag.TmphType.Note,
                                                Command =
                                                    TmphSubString.Unsafe(html, (int)(tagStart - htmlFixed),
                                                        (int)(contentStart - tagStart)),
                                                Content =
                                                    contentStart == current
                                                        ? TmphSubString.Unsafe(html, 0, 0)
                                                        : TmphSubString.Unsafe(html, (int)(++contentStart - htmlFixed),
                                                            (int)(current - contentStart))
                                            };

                                            #region =@取值解析

                                            if (start != (tagStart -= 4))
                                            {
                                                contentStart = start;
                                                *tagStart = '@';
                                                do
                                                {
                                                    while (*++contentStart != '@')
                                                    {
                                                    }
                                                    if (contentStart == tagStart) break;
                                                    if (*--contentStart == '=' && *(contentStart + 2) != '[')
                                                    {
                                                        if (start != contentStart)
                                                        {
                                                            AppendHtml(TmphSubString.Unsafe(html, (int)(start - htmlFixed),
                                                                (int)(contentStart - start)));
                                                        }
                                                        start = (contentStart += 2);
                                                        if (contentStart == tagStart)
                                                        {
                                                            if (_isOnlyHtml == 0)
                                                                _tree.Append(
                                                                    new TmphNode
                                                                    {
                                                                        Tag =
                                                                            new TmphTag
                                                                            {
                                                                                TagType = TmphTag.TmphType.At,
                                                                                Command = AtCommand,
                                                                                Content = TmphSubString.Unsafe(html, 0, 0)
                                                                            }
                                                                    },
                                                                    false);
                                                            break;
                                                        }
                                                        if (*contentStart == '@' || *contentStart == '*')
                                                        {
                                                            if (++contentStart == tagStart)
                                                            {
                                                                if (_isOnlyHtml == 0)
                                                                    _tree.Append(
                                                                        new TmphNode
                                                                        {
                                                                            Tag =
                                                                                new TmphTag
                                                                                {
                                                                                    TagType = TmphTag.TmphType.At,
                                                                                    Command = AtCommand,
                                                                                    Content =
                                                                                        TmphSubString.Unsafe(html,
                                                                                            (int)(start - htmlFixed), 1)
                                                                                }
                                                                        }, false);
                                                                ++start;
                                                                break;
                                                            }
                                                        }
                                                        while (*contentStart < AtMapSize &&
                                                               atFixedMap.Get(*contentStart))
                                                            ++contentStart;
                                                        if (*contentStart == '#' &&
                                                            (*(contentStart + 1) < AtMapSize &&
                                                             atFixedMap.Get(*(contentStart + 1))))
                                                        {
                                                            for (contentStart += 2;
                                                                *contentStart < AtMapSize &&
                                                                atFixedMap.Get(*contentStart);
                                                                ++contentStart)
                                                            {
                                                            }
                                                        }
                                                        if (_isOnlyHtml == 0)
                                                            _tree.Append(
                                                                new TmphNode
                                                                {
                                                                    Tag =
                                                                        new TmphTag
                                                                        {
                                                                            TagType = TmphTag.TmphType.At,
                                                                            Command = AtCommand,
                                                                            Content =
                                                                                TmphSubString.Unsafe(html,
                                                                                    (int)(start - htmlFixed),
                                                                                    (int)(contentStart - start))
                                                                        }
                                                                },
                                                                false);
                                                        if (*contentStart == '$') ++contentStart;
                                                        start = contentStart;
                                                    }
                                                    else contentStart += 2;
                                                } while (contentStart != tagStart);
                                                *tagStart = '<';
                                                if (start != tagStart)
                                                {
                                                    AppendHtml(TmphSubString.Unsafe(html, (int)(start - htmlFixed),
                                                        (int)(tagStart - start)));
                                                }
                                            }

                                            #endregion =@取值解析

                                            if (_isOnlyHtml == 0 && !_tree.IsRound(tag, tagIndex == ClientCommandIndex))
                                                _tree.Append(new TmphNode { Tag = tag });
                                            start = current + 3;
                                        }
                                    }
                                    current += 3;
                                }
                            } while (current < end);

                            #region 最后=@取值解析

                            if (current <= end)
                            {
                                current = start;
                                *end = '@';
                                do
                                {
                                    while (*current != '@') ++current;
                                    if (current == end) break;
                                    if (*--current == '=')
                                    {
                                        if (start != current)
                                        {
                                            AppendHtml(TmphSubString.Unsafe(html, (int)(start - htmlFixed),
                                                (int)(current - start)));
                                        }
                                        start = (current += 2);
                                        if (current == end)
                                        {
                                            if (_isOnlyHtml == 0)
                                                _tree.Append(
                                                    new TmphNode
                                                    {
                                                        Tag =
                                                            new TmphTag
                                                            {
                                                                TagType = TmphTag.TmphType.At,
                                                                Command = AtCommand,
                                                                Content = TmphSubString.Unsafe(html, 0, 0)
                                                            }
                                                    }, false);
                                            break;
                                        }
                                        if (*current == '@' || *current == '*')
                                        {
                                            if (++current == end)
                                            {
                                                if (_isOnlyHtml == 0)
                                                    _tree.Append(
                                                        new TmphNode
                                                        {
                                                            Tag =
                                                                new TmphTag
                                                                {
                                                                    TagType = TmphTag.TmphType.At,
                                                                    Command = AtCommand,
                                                                    Content =
                                                                        TmphSubString.Unsafe(html,
                                                                            (int)(start - htmlFixed),
                                                                            1)
                                                                }
                                                        }, false);
                                                ++start;
                                                break;
                                            }
                                        }
                                        while (*current < AtMapSize && atFixedMap.Get(*current)) ++current;
                                        if (*current == '#' &&
                                            (*(current + 1) < AtMapSize && atFixedMap.Get(*(current + 1))))
                                        {
                                            for (current += 2;
                                                *current < AtMapSize && atFixedMap.Get(*current);
                                                ++current)
                                            {
                                            }
                                        }
                                        if (_isOnlyHtml == 0)
                                            _tree.Append(
                                                new TmphNode
                                                {
                                                    Tag =
                                                        new TmphTag
                                                        {
                                                            TagType = TmphTag.TmphType.At,
                                                            Command = AtCommand,
                                                            Content =
                                                                TmphSubString.Unsafe(html, (int)(start - htmlFixed),
                                                                    (int)(current - start))
                                                        }
                                                }, false);
                                        start = current;
                                    }
                                    else current += 2;
                                } while (current != end);
                                *end = endChar;
                                AppendHtml(TmphSubString.Unsafe(html, (int)(start - htmlFixed), (int)(end - start) + 1));
                            }

                            #endregion 最后=@取值解析

                            *end = endChar;
                        }
                    }
                    else AppendHtml(html);
                    if (_isOnlyHtml == 0) (Boot = new TmphNode()).SetChilds(_tree.End());
                }
            }

            /// <summary>
            ///     添加HTML片段
            /// </summary>
            /// <param name="html">HTML片段</param>
            private void AppendHtml(TmphSubString html)
            {
                if (_isOnlyHtml == 0)
                    _tree.Append(new TmphNode { Tag = new TmphTag { TagType = TmphTag.TmphType.Html, Content = html } }, false);
                TmphHashString htmlKey = html;
                if (!_htmls.ContainsKey(htmlKey)) _htmls.Add(htmlKey, _htmls.Count);
            }

            /// <summary>
            ///     获取HTML片段索引号
            /// </summary>
            /// <param name="html">HTML片段</param>
            /// <returns>索引号</returns>
            public int GetHtmlIndex(string html)
            {
                //if (!htmls.ContainsKey(html)) log.Default.Add("->" + html + "<-", false, false);
                return _htmls[html];
            }

            /// <summary>
            ///     HTML模板格式化处理
            /// </summary>
            /// <param name="html">HTML模板</param>
            /// <returns>格式化处理后的HTML模板</returns>
            private static string FormatHtml(string html)
            {
                html = ImageSrc.Replace(html, @" src=""");
                //html = html.Replace(ViewBody, string.Empty);
                return html;
            }

            /// <summary>
            ///     HTML模板树节点
            /// </summary>
            public sealed class TmphNode : TmphTemplate<TmphNode>.TmphINode, TmphTreeBuilder<TmphNode, TmphTag>.TmphINode
            {
                /// <summary>
                ///     子节点集合
                /// </summary>
                private TmphSubArray<TmphNode> _childs;

                /// <summary>
                ///     模板命令
                /// </summary>
                public string TemplateCommand
                {
                    get { return Tag.Command; }
                }

                /// <summary>
                ///     模板成员名称
                /// </summary>
                public string TemplateMemberName
                {
                    get { return Tag.Command.value != null ? Tag.Content : null; }
                }

                /// <summary>
                ///     模板文本代码
                /// </summary>
                public string TemplateCode
                {
                    get { return Tag.Command.value == null ? Tag.Content : null; }
                }

                /// <summary>
                ///     子节点数量
                /// </summary>
                public int ChildCount
                {
                    get { return _childs.Count; }
                }

                /// <summary>
                ///     子节点集合
                /// </summary>
                public IEnumerable<TmphNode> Childs
                {
                    get { return _childs; }
                }

                /// <summary>
                ///     树节点标识
                /// </summary>
                public TmphTag Tag { get; internal set; }

                /// <summary>
                ///     设置子节点集合
                /// </summary>
                /// <param name="childs">子节点集合</param>
                public void SetChilds(TmphNode[] childs)
                {
                    _childs = new TmphSubArray<TmphNode>(childs);
                }
            }

            /// <summary>
            ///     HTML模板树节点标识
            /// </summary>
            public sealed class TmphTag : IEquatable<TmphTag>
            {
                /// <summary>
                ///     标识command
                /// </summary>
                internal TmphSubString Command;

                /// <summary>
                ///     内容
                /// </summary>
                internal TmphSubString Content;

                /// <summary>
                ///     树节点标识类型
                /// </summary>
                internal TmphType TagType;

                /// <summary>
                ///     判断树节点标识是否相等
                /// </summary>
                /// <param name="other">树节点标识</param>
                /// <returns>是否相等</returns>
                public bool Equals(TmphTag other)
                {
                    return other != null && other.TagType == TagType && other.Command.Equals(Command) &&
                           other.Content.Equals(Content);
                }

                public override string ToString()
                {
                    return TagType + " " + Command;
                }

                /// <summary>
                ///     树节点标识类型
                /// </summary>
                internal enum TmphType
                {
                    /// <summary>
                    ///     普通HTML段
                    /// </summary>
                    Html,

                    /// <summary>
                    ///     注释子段
                    /// </summary>
                    Note,

                    /// <summary>
                    ///     =@取值代码
                    /// </summary>
                    At
                }
            }
        }
    }
}

namespace Laurent.Lee.CLB.Code
{
    /// <summary>
    ///     成员类型
    /// </summary>
    public sealed partial class TmphMemberType
    {
        /// <summary>
        ///     AJAX toString重定向类型集合
        /// </summary>
        private static readonly HashSet<Type> AjaxToStringTypes =
            new HashSet<Type>(new[]
            {
                typeof (bool), typeof (byte), typeof (sbyte), typeof (short), typeof (ushort), typeof (int),
                typeof (uint),
                typeof (long), typeof (ulong), typeof (float), typeof (double), typeof (decimal), typeof (char)
            });

        /// <summary>
        ///     客户端视图绑定类型
        /// </summary>
        private TmphWebView.TmphClientType _clientViewType;

        /// <summary>
        ///     是否#!URL
        /// </summary>
        public bool IsHashUrl
        {
            get { return Type == typeof(TmphWebView.TmphHashUrl); }
        }

        /// <summary>
        ///     是否AJAX toString重定向类型
        /// </summary>
        public bool IsAjaxToString
        {
            get { return AjaxToStringTypes.Contains(Type.nullableType() ?? Type); }
        }

        /// <summary>
        ///     客户端视图绑定类型
        /// </summary>
        public string ClientViewTypeName
        {
            get
            {
                if (_clientViewType == null)
                {
                    _clientViewType = TmphTypeAttribute.GetAttribute<TmphWebView.TmphClientType>(Type, true, true) ??
                                      TmphWebView.TmphClientType.Null;
                }
                return _clientViewType.Name;
            }
        }

        /// <summary>
        ///     客户端视图绑定成员名称
        /// </summary>
        public string ClientViewMemberName
        {
            get
            {
                if (_clientViewType == null)
                {
                    _clientViewType = TmphTypeAttribute.GetAttribute<TmphWebView.TmphClientType>(Type, true, true) ??
                                      TmphWebView.TmphClientType.Null;
                }
                return _clientViewType.MemberName;
            }
        }
    }
}