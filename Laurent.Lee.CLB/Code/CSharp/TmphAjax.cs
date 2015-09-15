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
using Laurent.Lee.CLB.Emit;
using Laurent.Lee.CLB.Net.Tcp.Http;
using Laurent.Lee.CLB.Threading;
using System;
using System.Reflection;
using TmphHttp = Laurent.Lee.CLB.Web.TmphHttp;

namespace Laurent.Lee.CLB.Code.CSharp
{
    /// <summary>
    ///     AJAX调用配置
    /// </summary>
    public sealed class TmphAjax : TmphWebPage
    {
        /// <summary>
        ///     公用错误处理函数名称
        /// </summary>
        public const string PubErrorCallName = "pub.Error";

        /// <summary>
        ///     调用全名
        /// </summary>
        public string FullName;

        /// <summary>
        ///     成员是否匹配自定义属性类型
        /// </summary>
        public bool IsAttribute;

        /// <summary>
        ///     是否搜索父类自定义属性
        /// </summary>
        public bool IsBaseTypeAttribute;

        /// <summary>
        ///     成员匹配自定义属性是否可继承
        /// </summary>
        public bool IsInheritAttribute;

        /// <summary>
        ///     是否仅支持POST请求
        /// </summary>
        public bool IsOnlyPost = true;

        /// <summary>
        ///     AJAX调用
        /// </summary>
        /// <typeparam name="TAjaxType">AJAX调用类型</typeparam>
        public abstract class TmphCall<TAjaxType> : TmphWebView.TmphView<TAjaxType>, TmphWebView.IWebView
            where TAjaxType : TmphCall<TAjaxType>
        {
            /// <summary>
            ///     WEB视图加载
            /// </summary>
            /// <returns>是否成功</returns>
            protected override bool LoadView()
            {
                return false;
            }
        }

        /// <summary>
        ///     AJAX调用信息
        /// </summary>
        public sealed class TmphCall
        {
            /// <summary>
            ///     AJAX调用
            /// </summary>
            public Action<TmphLoader> Call;

            /// <summary>
            ///     是否只接受POST请求
            /// </summary>
            public bool IsPost;

            /// <summary>
            ///     内存流最大字节数
            /// </summary>
            public int MaxMemoryStreamSize;

            /// <summary>
            ///     最大接收数据字节数
            /// </summary>
            public int MaxPostDataSize;

            /// <summary>
            ///     AJAX调用信息
            /// </summary>
            /// <param name="call">AJAX调用</param>
            /// <param name="maxPostDataSize">最大接收数据字节数</param>
            /// <param name="maxMemoryStreamSize">内存流最大字节数</param>
            /// <param name="isPost">是否只接受POST请求</param>
            public TmphCall(Action<TmphLoader> call, int maxPostDataSize, int maxMemoryStreamSize, bool isPost = true)
            {
                Call = call;
                MaxPostDataSize = maxPostDataSize;
                MaxMemoryStreamSize = maxMemoryStreamSize;
                IsPost = isPost;
            }
        }

        /// <summary>
        ///     表单加载
        /// </summary>
        public class TmphLoader : TmphRequestForm.TmphILoadForm
        {
            /// <summary>
            ///     AJAX调用信息
            /// </summary>
            private TmphCall _call;

            /// <summary>
            ///     域名服务
            /// </summary>
            private TmphDomainServer _domainServer;

            /// <summary>
            ///     HTTP请求表单
            /// </summary>
            private TmphRequestForm _form;

            /// <summary>
            ///     HTTP请求头
            /// </summary>
            private TmphRequestHeader _request;

            /// <summary>
            ///     HTTP套接字接口
            /// </summary>
            private TmphSocketBase _socket;

            /// <summary>
            ///     表单加载
            /// </summary>
            private TmphLoader()
            {
            }

            /// <summary>
            ///     套接字请求编号
            /// </summary>
            public long SocketIdentity { get; private set; }

            /// <summary>
            ///     表单回调处理
            /// </summary>
            /// <param name="form">HTTP请求表单</param>
            public void OnGetForm(TmphRequestForm form)
            {
                if (form == null) Push();
                else
                {
                    SocketIdentity = form.Identity;
                    Load(form);
                }
            }

            /// <summary>
            ///     根据HTTP请求表单值获取内存流最大字节数
            /// </summary>
            /// <param name="value">HTTP请求表单值</param>
            /// <returns>内存流最大字节数</returns>
            public int MaxMemoryStreamSize(TmphRequestForm.TmphValue value)
            {
                return _call.MaxMemoryStreamSize;
            }

            /// <summary>
            ///     根据HTTP请求表单值获取保存文件全称
            /// </summary>
            /// <param name="value">HTTP请求表单值</param>
            /// <returns>文件全称</returns>
            public string GetSaveFileName(TmphRequestForm.TmphValue value)
            {
                return null;
            }

            /// <summary>
            ///     表单加载回收
            /// </summary>
            private void Push()
            {
                _socket = null;
                _domainServer = null;
                _request = null;
                _form = null;
                _call = null;
                TmphTypePool<TmphLoader>.Push(this);
            }

            /// <summary>
            ///     WEB视图表单加载
            /// </summary>
            /// <param name="form">HTTP请求表单</param>
            internal void Load(TmphRequestForm form)
            {
                var identity = SocketIdentity;
                var socket = _socket;
                try
                {
                    _form = form;
                    _call.Call(this);
                    return;
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
                finally
                {
                    Push();
                }
                socket.ResponseError(identity, TmphResponse.TmphState.ServerError500);
            }

            /// <summary>
            ///     AJAX数据加载
            /// </summary>
            /// <typeparam name="TAjaxType">AJAX调用类型</typeparam>
            /// <param name="ajax">AJAX调用</param>
            /// <param name="isPool">是否使用WEB页面池</param>
            /// <returns>HTTP响应,失败返回null</returns>
            public TmphResponse Load<TAjaxType>(TAjaxType ajax, bool isPool)
                where TAjaxType : TmphWebView.TmphView, TmphWebView.IWebView
            {
                var socket = _socket;
                ajax.Socket = socket;
                ajax.DomainServer = _domainServer;
                if (ajax.LoadHeader(SocketIdentity, _request, isPool))
                {
                    ajax.Form = _form;
                    return ajax.Response = TmphResponse.Get(true);
                }
                return null;
            }

            /// <summary>
            ///     AJAX数据加载
            /// </summary>
            /// <typeparam name="TAjaxType">AJAX调用类型</typeparam>
            /// <typeparam name="TValueType"></typeparam>
            /// <param name="ajax">AJAX调用</param>
            /// <param name="parameter">参数值</param>
            /// <param name="isPool">是否使用WEB页面池</param>
            /// <returns>HTTP响应,失败返回null</returns>
            public TmphResponse Load<TAjaxType, TValueType>(TAjaxType ajax, ref TValueType parameter, bool isPool)
                where TAjaxType : TmphWebView.TmphView, TmphWebView.IWebView
            {
                var socket = _socket;
                try
                {
                    if (_form != null && _form.Json != null)
                    {
                        if (_form.Json.Length != 0 && TmphJsonParser.Parse(_form.Json, ref parameter))
                        {
                            ajax.Socket = socket;
                            ajax.DomainServer = _domainServer;
                            if (ajax.LoadHeader(SocketIdentity, _request, isPool))
                            {
                                ajax.Form = _form;
                                return ajax.Response = TmphResponse.Get(true);
                            }
                            isPool = false;
                        }
                    }
                    else
                    {
                        if (_request.ParseQuery(ref parameter))
                        {
                            var queryJson = _request.QueryJson;
                            if (queryJson.Length == 0 || TmphJsonParser.Parse(queryJson, ref parameter))
                            {
                                ajax.Socket = socket;
                                ajax.DomainServer = _domainServer;
                                if (ajax.LoadHeader(SocketIdentity, _request, isPool))
                                {
                                    ajax.Form = _form;
                                    return ajax.Response = TmphResponse.Get(true);
                                }
                                isPool = false;
                            }
                        }
                    }
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
                if (socket.ResponseError(SocketIdentity, TmphResponse.TmphState.ServerError500) && isPool)
                    TmphTypePool<TAjaxType>.Push(ajax);
                return null;
            }

            /// <summary>
            ///     加载WEB视图
            /// </summary>
            /// <typeparam name="TValueType">WEB视图类型</typeparam>
            /// <param name="view">WEB视图</param>
            /// <param name="isPool">是否使用WEB视图池</param>
            public void LoadView<TValueType>(TValueType view, bool isPool)
                where TValueType : TmphWebView.TmphView, TmphWebView.IWebView
            {
                view.Socket = _socket;
                view.DomainServer = _domainServer;
                if (view.LoadHeader(SocketIdentity, _request, isPool)) view.Load(_form, true);
                else _socket.ResponseError(SocketIdentity, TmphResponse.TmphState.ServerError500);
            }

            /// <summary>
            ///     表单加载
            /// </summary>
            /// <param name="socket">HTTP套接字接口</param>
            /// <param name="domainServer">域名服务</param>
            /// <param name="socketIdentity">套接字操作编号</param>
            /// <param name="request">HTTP请求头</param>
            /// <param name="call">AJAX调用信息</param>
            /// <returns>表单加载</returns>
            internal static TmphLoader Get(TmphSocketBase socket, TmphDomainServer domainServer, long socketIdentity,
                TmphRequestHeader request, TmphCall call)
            {
                var loader = TmphTypePool<TmphLoader>.Pop() ?? new TmphLoader();
                loader._socket = socket;
                loader._domainServer = domainServer;
                loader.SocketIdentity = socketIdentity;
                loader._request = request;
                loader._call = call;
                return loader;
            }
        }

        /// <summary>
        ///     公用AJAX调用
        /// </summary>
        private sealed class TmphPub : TmphCall<TmphPub>
        {
            /// <summary>
            ///     错误信息队列
            /// </summary>
            private static readonly TmphFifoPriorityQueue<TmphHashString, string> ErrorQueue =
                new TmphFifoPriorityQueue<TmphHashString, string>();

            /// <summary>
            ///     错误信息队列访问锁
            /// </summary>
            private static int _errorQueueLock;

            static TmphPub()
            {
                if (TmphAppSetting.IsCheckMemory) TmphCheckMemory.Add(MethodBase.GetCurrentMethod().DeclaringType);
            }

            /// <summary>
            ///     公用错误处理函数
            /// </summary>
            /// <param name="error">错误信息</param>
            public void Error(string error)
            {
                if (error.Length() != 0)
                {
                    var isLog = false;
                    if (error.Length <= TmphWeb.Default.PubErrorMaxSize)
                    {
                        TmphHashString errorHash = error;
                        TmphInterlocked.NoCheckCompareSetSleep0(ref _errorQueueLock);
                        try
                        {
                            if (ErrorQueue.Set(errorHash, error) == null)
                            {
                                isLog = true;
                                if (ErrorQueue.Count > TmphWeb.Default.PubErrorMaxCacheCount) ErrorQueue.Pop();
                            }
                        }
                        finally
                        {
                            _errorQueueLock = 0;
                        }
                    }
                    else isLog = true;
                    if (isLog) TmphLog.Default.Add(error);
                }
            }

            /// <summary>
            ///     公用错误处理参数
            /// </summary>
            public struct TmphErrorParameter
            {
                /// <summary>
                ///     错误信息
                /// </summary>
#pragma warning disable
                public string error;
            }
        }

        /// <summary>
        ///     AJAX调用加载
        /// </summary>
        /// <typeparam name="TLoaderType">AJAX调用加载类型</typeparam>
        public abstract class TmphLoader<TLoaderType> : TmphWebCall.TmphCall where TLoaderType : TmphLoader<TLoaderType>
        {
            /// <summary>
            ///     AJAX调用
            /// </summary>
            /// <param name="methods">AJAX函数调用集合</param>
            protected void Load(TmphStateSearcher.TmphAscii<TmphCall> methods)
            {
                try
                {
                    var identity = SocketIdentity;
                    if (!TmphWeb.Default.IsAjaxReferer || Config.TmphPub.Default.IsDebug || RequestHeader.IsReferer)
                    {
                        var call =
                            methods.Get(DomainServer.WebConfig.IgnoreCase
                                ? RequestHeader.LowerAjaxCallName
                                : RequestHeader.AjaxCallName);
                        if (call == null)
                        {
                            var path =
                                DomainServer.GetViewRewrite(DomainServer.WebConfig.IgnoreCase
                                    ? RequestHeader.LowerAjaxCallName
                                    : RequestHeader.AjaxCallName);
                            if (path != null) call = methods.Get(path);
                        }
                        if (call != null &&
                            (RequestHeader.Method == TmphHttp.TmphMethodType.POST || !call.IsPost ||
                             RequestHeader.IsWebSocket))
                        {
                            if (RequestHeader.ContentLength <= call.MaxPostDataSize)
                            {
                                var loadForm = TmphLoader.Get(Socket, DomainServer, identity, RequestHeader, call);
                                if (RequestHeader.Method == TmphHttp.TmphMethodType.POST)
                                    Socket.GetForm(identity, loadForm);
                                else loadForm.Load(null);
                            }
                            else Socket.ResponseError(identity, TmphResponse.TmphState.ServerError500);
                            return;
                        }
                    }
                    Socket.ResponseError(identity, TmphResponse.TmphState.NotFound404);
                }
                finally
                {
                    PushPool();
                }
            }

            /// <summary>
            ///     公用错误处理函数
            /// </summary>
            /// <param name="loader">表单加载</param>
            protected static void PubError(TmphLoader loader)
            {
                var view = TmphTypePool<TmphPub>.Pop() ?? new TmphPub();
                var parameter = new TmphPub.TmphErrorParameter();
                var response = loader.Load(view, ref parameter, true);
                if (response != null)
                {
                    try
                    {
                        view.Error(parameter.error);
                    }
                    finally
                    {
                        view.AjaxResponse(ref response);
                    }
                }
            }
        }

        /// <summary>
        ///     AJAX异步回调
        /// </summary>
        /// <typeparam name="TCallbackType">异步回调类型</typeparam>
        /// <typeparam name="TAjaxType">AJAX类型</typeparam>
        public abstract class TmphCallback<TCallbackType, TAjaxType>
            where TCallbackType : TmphCallback<TCallbackType, TAjaxType>
            where TAjaxType : TmphCall<TAjaxType>
        {
            /// <summary>
            ///     AJAX回调处理
            /// </summary>
            private readonly Action<TmphAsynchronousMethod.TmphReturnValue> _onReturnHandle;

            /// <summary>
            ///     当前AJAX异步回调
            /// </summary>
            private readonly TCallbackType _thisCallback;

            /// <summary>
            ///     AJAX回调对象
            /// </summary>
            private TAjaxType _ajax;

            /// <summary>
            ///     HTTP响应
            /// </summary>
            private TmphResponse _response;

            /// <summary>
            ///     AJAX异步回调
            /// </summary>
            protected TmphCallback()
            {
                _thisCallback = (TCallbackType)this;
                _onReturnHandle = OnReturn;
            }

            /// <summary>
            ///     AJAX回调处理
            /// </summary>
            /// <param name="value">回调值</param>
            private void OnReturn(TmphAsynchronousMethod.TmphReturnValue value)
            {
                try
                {
                    if (value.IsReturn)
                    {
                        _ajax.CancelAsynchronous();
                        _ajax.AjaxResponse(ref _response);
                    }
                    else _ajax.ServerError500();
                }
                finally
                {
                    _ajax = null;
                    _response = null;
                    TmphTypePool<TCallbackType>.Push(_thisCallback);
                }
            }

            /// <summary>
            ///     获取AJAX回调处理
            /// </summary>
            /// <param name="ajax">AJAX回调对象</param>
            /// <param name="response">HTTP响应</param>
            /// <returns>AJAX回调处理</returns>
            public Action<TmphAsynchronousMethod.TmphReturnValue> Get(TAjaxType ajax, TmphResponse response)
            {
                _ajax = ajax;
                _response = response;
                ajax.SetAsynchronous();
                return _onReturnHandle;
            }
        }

        /// <summary>
        ///     AJAX异步回调
        /// </summary>
        /// <typeparam name="TCallbackType">异步回调类型</typeparam>
        /// <typeparam name="TAjaxType">AJAX类型</typeparam>
        /// <typeparam name="TReturnType">返回值类型</typeparam>
        public abstract class TmphCallback<TCallbackType, TAjaxType, TReturnType>
            where TCallbackType : TmphCallback<TCallbackType, TAjaxType, TReturnType>
            where TAjaxType : TmphWebView.TmphView
        {
            /// <summary>
            ///     AJAX回调处理
            /// </summary>
            private readonly Action<TmphAsynchronousMethod.TmphReturnValue<TReturnType>> _onReturnHandle;

            /// <summary>
            ///     当前AJAX异步回调
            /// </summary>
            private readonly TCallbackType _thisCallback;

            /// <summary>
            ///     AJAX回调对象
            /// </summary>
            protected TAjaxType Ajax;

            /// <summary>
            ///     HTTP响应
            /// </summary>
            protected TmphResponse Response;

            /// <summary>
            ///     AJAX异步回调
            /// </summary>
            protected TmphCallback()
            {
                _thisCallback = (TCallbackType)this;
                _onReturnHandle = OnReturn;
            }

            /// <summary>
            ///     AJAX回调处理
            /// </summary>
            /// <param name="value">回调值</param>
            private void OnReturn(TmphAsynchronousMethod.TmphReturnValue<TReturnType> value)
            {
                try
                {
                    if (value.IsReturn)
                    {
                        Ajax.CancelAsynchronous();
                        OnReturnValue(value.Value);
                    }
                    else Ajax.ServerError500();
                }
                finally
                {
                    Ajax = null;
                    Response = null;
                    TmphTypePool<TCallbackType>.Push(_thisCallback);
                }
            }

            /// <summary>
            ///     AJAX回调处理
            /// </summary>
            /// <param name="value">回调值</param>
            protected abstract void OnReturnValue(TReturnType value);

            /// <summary>
            ///     获取AJAX回调处理
            /// </summary>
            /// <param name="ajax">AJAX回调对象</param>
            /// <param name="response">HTTP响应</param>
            /// <returns>AJAX回调处理</returns>
            public Action<TmphAsynchronousMethod.TmphReturnValue<TReturnType>> Get(TAjaxType ajax, TmphResponse response)
            {
                Ajax = ajax;
                Response = response;
                ajax.SetAsynchronous();
                return _onReturnHandle;
            }
        }
    }
}