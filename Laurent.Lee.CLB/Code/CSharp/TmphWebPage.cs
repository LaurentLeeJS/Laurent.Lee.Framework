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
using Laurent.Lee.CLB.Net.Tcp.Http;
using System;
using System.Net;
using System.Text;

namespace Laurent.Lee.CLB.Code.CSharp
{
    /// <summary>
    ///     WEB页面
    /// </summary>
    public abstract class TmphWebPage : TmphIgnoreMember
    {
        /// <summary>
        ///     是否使用WEB页面池
        /// </summary>
        public bool IsPool;

        /// <summary>
        ///     内存流最大字节数(单位:KB)
        /// </summary>
        public int MaxMemoryStreamSize = TmphHttp.Default.MaxMemoryStreamSize;

        /// <summary>
        ///     最大接收数据字节数(单位:MB)
        /// </summary>
        public int MaxPostDataSize = TmphHttp.Default.MaxPostDataSize;

        /// <summary>
        ///     WEB调用函数名称
        /// </summary>
        public string MethodName;

        /// <summary>
        ///     WEB页面
        /// </summary>
        public interface IWebPage
        {
            /// <summary>
            ///     HTTP套接字接口设置
            /// </summary>
            TmphSocketBase Socket { set; }

            /// <summary>
            ///     域名服务设置
            /// </summary>
            TmphDomainServer DomainServer { set; }

            /// <summary>
            ///     套接字请求编号
            /// </summary>
            long SocketIdentity { get; }

            /// <summary>
            ///     根据HTTP请求表单值获取保存文件全称
            /// </summary>
            /// <param name="value">HTTP请求表单值</param>
            /// <returns>文件全称</returns>
            string GetSaveFileName(TmphRequestForm.TmphValue value);
        }

        /// <summary>
        ///     WEB页面
        /// </summary>
        public abstract class TmphPage : IDisposable
        {
            /// <summary>
            ///     默认重定向路径
            /// </summary>
            private static readonly byte[] LocationPath = { (byte)'/' };

            /// <summary>
            ///     Session名称
            /// </summary>
            private static readonly byte[] SessionName = TmphHttp.Default.SessionName.GetBytes();

            /// <summary>
            ///     域名服务
            /// </summary>
            private TmphDomainServer _domainServer;

            /// <summary>
            ///     会话标识
            /// </summary>
            private TmphUint128 _sessionId;

            /// <summary>
            ///     HTTP套接字接口设置
            /// </summary>
            private TmphSocketBase _socket;

            /// <summary>
            ///     异步调用标识
            /// </summary>
            protected int AsynchronousIdentity;

            /// <summary>
            ///     HTTP请求表单
            /// </summary>
            protected internal TmphRequestForm Form;

            /// <summary>
            ///     HTTP请求头部
            /// </summary>
            protected TmphRequestHeader RequestHeader;

            /// <summary>
            ///     HTTP响应输出
            /// </summary>
            public TmphResponse Response;

            /// <summary>
            ///     输出编码
            /// </summary>
            protected Encoding ResponseEncoding;

            /// <summary>
            ///     HTTP套接字接口设置
            /// </summary>
            public TmphSocketBase Socket
            {
                internal get { return _socket; }
                set
                {
                    if (_socket == null) _socket = value;
                    else TmphLog.Error.Throw(TmphLog.TmphExceptionType.ErrorOperation);
                }
            }

            /// <summary>
            ///     远程终结点
            /// </summary>
            public EndPoint RemoteEndPoint
            {
                get { return _socket.RemoteEndPoint; }
            }

            /// <summary>
            ///     域名服务
            /// </summary>
            public TmphDomainServer DomainServer
            {
                internal get { return _domainServer; }
                set
                {
                    if (_domainServer == null) _domainServer = value;
                    else TmphLog.Error.Throw(TmphLog.TmphExceptionType.ErrorOperation);
                }
            }

            /// <summary>
            ///     域名服务工作文件路径
            /// </summary>
            public string WorkPath
            {
                get { return _domainServer.WorkPath; }
            }

            /// <summary>
            ///     套接字请求编号
            /// </summary>
            public long SocketIdentity { get; protected internal set; }

            /// <summary>
            ///     HTTP响应输出标识(用于终止同步ajax输出)
            /// </summary>
            public int ResponseIdentity { get; private set; }

            /// <summary>
            ///     是否异步调用
            /// </summary>
            public bool IsAsynchronous { get; internal set; }

            /// <summary>
            ///     客户端缓存版本号
            /// </summary>
            public virtual int ETagVersion
            {
                get { return 0; }
            }

            /// <summary>
            ///     是否支持压缩
            /// </summary>
            protected virtual bool IsGZip
            {
                get { return true; }
            }

            /// <summary>
            ///     释放资源
            /// </summary>
            public void Dispose()
            {
                Response = null;
                var requestHeader = RequestHeader;
                RequestHeader = null;
                var socket = _socket;
                _socket = null;
                if (socket != null && requestHeader != null)
                {
                    socket.ResponseError(SocketIdentity, TmphResponse.TmphState.ServerError500);
                }
            }

            /// <summary>
            ///     HTTP请求头部处理
            /// </summary>
            /// <param name="socketIdentity">套接字操作编号</param>
            /// <param name="request">HTTP请求头部</param>
            /// <param name="isPool">是否使用WEB页面池</param>
            /// <returns>是否成功</returns>
            internal virtual bool LoadHeader(long socketIdentity, TmphRequestHeader request, bool isPool)
            {
                return false;
            }

            /// <summary>
            ///     加载查询参数
            /// </summary>
            /// <param name="form">HTTP请求表单</param>
            /// <param name="isAjax">是否ajax请求</param>
            /// <returns>是否成功</returns>
            internal virtual void Load(TmphRequestForm form, bool isAjax)
            {
                TmphLog.Error.Throw(TmphLog.TmphExceptionType.ErrorOperation);
            }

            /// <summary>
            ///     清除当前请求数据
            /// </summary>
            protected virtual void Clear()
            {
                ++ResponseIdentity;
                RequestHeader = null;
                _socket = null;
                if (IsAsynchronous)
                {
                    IsAsynchronous = false;
                    ++AsynchronousIdentity;
                    TmphResponse.Push(ref Response);
                }
                else Response = null;
                _domainServer = null;
                Form = null;
                _sessionId.Low = _sessionId.High = 0;
            }

            /// <summary>
            ///     设置为异步调用模式(回调中需要cancelAsynchronous)
            /// </summary>
            protected internal void SetAsynchronous()
            {
                IsAsynchronous = true;
            }

            /// <summary>
            ///     取消异步调用模式，获取HTTP响应
            /// </summary>
            /// <returns>HTTP响应</returns>
            protected internal TmphResponse CancelAsynchronous()
            {
                if (IsAsynchronous)
                {
                    ++AsynchronousIdentity;
                    IsAsynchronous = false;
                    return Response;
                }
                return null;
            }

            /// <summary>
            ///     WEB页面回收
            /// </summary>
            internal abstract void PushPool();

            /// <summary>
            ///     重定向
            /// </summary>
            /// <param name="path">重定向地址</param>
            /// <param name="is302">是否临时重定向</param>
            protected void Location(string path, bool is302 = true)
            {
                Location(path.Length() != 0 ? path.GetBytes() : null, is302);
            }

            /// <summary>
            ///     重定向
            /// </summary>
            /// <param name="path">重定向地址</param>
            /// <param name="is302">是否临时重定向</param>
            protected void Location(byte[] path, bool is302 = true)
            {
                if (RequestHeader != null)
                {
                    var response = Response = TmphResponse.Copy(Response);
                    response.State = is302 ? TmphResponse.TmphState.Found302 : TmphResponse.TmphState.MovedPermanently301;
                    if (path == null) path = LocationPath;
                    response.Location.UnsafeSet(path, 0, path.Length);
                    if (_socket.Response(SocketIdentity, ref response)) PushPool();
                }
            }

            /// <summary>
            ///     资源未修改
            /// </summary>
            protected void NotChanged304()
            {
                if (_socket.Response(SocketIdentity, TmphResponse.NotChanged304)) PushPool();
            }

            /// <summary>
            ///     服务器发生不可预期的错误
            /// </summary>
            protected internal void ServerError500()
            {
                if (_socket.ResponseError(SocketIdentity, TmphResponse.TmphState.ServerError500)) PushPool();
            }

            /// <summary>
            ///     请求资源不存在
            /// </summary>
            protected void NotFound404()
            {
                if (_socket.ResponseError(SocketIdentity, TmphResponse.TmphState.NotFound404)) PushPool();
            }

            /// <summary>
            ///     任意客户端缓存有效标识处理
            /// </summary>
            /// <returns>是否需要继续加载</returns>
            protected bool AnyIfNoneMatch()
            {
                if (RequestHeader.IfNoneMatch.Count == 0)
                {
                    Response.SetETag(LocationPath);
                    return true;
                }
                NotChanged304();
                return false;
            }

            /// <summary>
            ///     客户端缓存有效标识匹配处理
            /// </summary>
            /// <param name="eTag">客户端缓存有效标识</param>
            /// <returns>是否需要继续加载</returns>
            protected unsafe bool LoadIfNoneMatch(byte[] eTag)
            {
                if (eTag != null)
                {
                    var ifNoneMatch = RequestHeader.IfNoneMatch;
                    if (ifNoneMatch.Count == eTag.Length)
                    {
                        fixed (byte* eTagFixed = eTag, ifNoneMatchFixed = ifNoneMatch.Array)
                        {
                            if (Unsafe.TmphMemory.Equal(eTag, ifNoneMatchFixed + ifNoneMatch.StartIndex,
                                eTag.Length))
                            {
                                NotChanged304();
                                return false;
                            }
                        }
                    }
                    Response.SetETag(eTag);
                }
                return true;
            }

            /// <summary>
            ///     输出数据
            /// </summary>
            /// <param name="data"></param>
            protected void ResponseData(byte[] data)
            {
                Response.BodyStream.Write(data);
            }

            /// <summary>
            ///     输出数据
            /// </summary>
            /// <param name="data"></param>
            protected void ResponseData(TmphSubArray<byte> data)
            {
                Response.BodyStream.Write(data);
            }

            /// <summary>
            ///     输出HTML片段
            /// </summary>
            /// <param name="html">HTML片段</param>
            protected unsafe void ResponseData(char html)
            {
                var bodyStream = Response.BodyStream;
                if (ResponseEncoding.CodePage == Encoding.Unicode.CodePage) bodyStream.Write(html);
                else
                {
                    var count = ResponseEncoding.GetByteCount(&html, 1);
                    bodyStream.PrepLength(count);
                    ResponseEncoding.GetBytes(&html, 1, bodyStream.CurrentData, count);
                    bodyStream.Unsafer.AddLength(count);
                }
            }

            /// <summary>
            ///     输出字符串数据
            /// </summary>
            /// <param name="value"></param>
            protected unsafe void ResponseData(TmphSubString value)
            {
                if (value.Length != 0)
                {
                    var bodyStream = Response.BodyStream;
                    fixed (char* valueFixed = value.value)
                    {
                        var valueStart = valueFixed + value.StartIndex;
                        if (ResponseEncoding.CodePage == Encoding.Unicode.CodePage)
                        {
                            var count = value.Length << 1;
                            bodyStream.PrepLength(count);
                            Unsafe.TmphMemory.Copy(valueStart, bodyStream.CurrentData, count);
                            bodyStream.Unsafer.AddLength(count);
                        }
                        else
                        {
                            var count = ResponseEncoding.GetByteCount(valueStart, value.Length);
                            bodyStream.PrepLength(count);
                            ResponseEncoding.GetBytes(valueStart, value.Length, bodyStream.CurrentData, count);
                            bodyStream.Unsafer.AddLength(count);
                        }
                    }
                }
            }

            /// <summary>
            ///     输出HTML片段
            /// </summary>
            /// <param name="html">HTML片段</param>
            protected void ResponseData(string html)
            {
                ResponseData((TmphSubString)html);
            }

            /// <summary>
            ///     输出结束
            /// </summary>
            /// <param name="response">HTTP响应输出</param>
            /// <returns>是否操作成功</returns>
            protected unsafe bool ResponseEnd(ref TmphResponse response)
            {
                var identity = SocketIdentity;
                try
                {
                    var buffer = response.Body.Array;
                    var length = response.BodyStream.Length;
                    if (RequestHeader.IsRange && !RequestHeader.FormatRange(length))
                    {
                        if (_socket.ResponseError(SocketIdentity, TmphResponse.TmphState.RangeNotSatisfiable416))
                        {
                            PushPool();
                            return true;
                        }
                    }
                    else
                    {
                        if (buffer.Length >= length)
                        {
                            Unsafe.TmphMemory.Copy(response.BodyStream.Data, buffer, length);
                            response.Body.UnsafeSet(0, length);
                        }
                        else
                        {
                            var data = response.BodyStream.GetArray();
                            response.Body.UnsafeSet(data, 0, data.Length);
                            response.Buffer = buffer;
                        }
                        response.State = TmphResponse.TmphState.Ok200;
                        if (response.ContentType == null) response.ContentType = DomainServer.HtmlContentType;
                        if (RequestHeader.IsGZip && IsGZip)
                        {
                            if (IsGZip)
                            {
                                if (!RequestHeader.IsRange)
                                {
                                    var compressData = TmphResponse.GetCompress(response.Body, TmphMemoryPool.StreamBuffers);
                                    if (compressData.array != null)
                                    {
                                        Buffer.BlockCopy(compressData.array, 0, response.Body.array, 0,
                                            compressData.Count);
                                        response.Body.UnsafeSet(0, compressData.Count);
                                        response.ContentEncoding = TmphResponse.GZipEncoding;
                                        TmphMemoryPool.StreamBuffers.Push(ref compressData.array);
                                    }
                                }
                            }
                            else RequestHeader.IsGZip = false;
                        }
                        response.NoStore();
                        if (_socket.Response(identity, ref response))
                        {
                            PushPool();
                            return true;
                        }
                    }
                }
                finally
                {
                    TmphResponse.Push(ref response);
                }
                return false;
            }

            /// <summary>
            ///     获取请求会话标识
            /// </summary>
            /// <returns>请求会话标识</returns>
            private TmphUint128 GetSessionId()
            {
                if ((_sessionId.Low | _sessionId.High) == 0)
                {
                    _sessionId = TmphSession.FromCookie(RequestHeader.GetCookie(SessionName));
                }
                return _sessionId;
            }

            /// <summary>
            ///     获取Session值
            /// </summary>
            /// <returns>Session值</returns>
            private object GetSession()
            {
                var session = _socket.Session;
                if (session != null)
                {
                    var sessionId = GetSessionId();
                    if (sessionId.Low != 0) return session.Get(sessionId, null);
                }
                return null;
            }

            /// <summary>
            ///     获取Session值
            /// </summary>
            /// <typeparam name="TValueType">值类型</typeparam>
            /// <returns>Session值</returns>
            public TValueType GetSession<TValueType>() where TValueType : class
            {
                var value = GetSession();
                return value != null ? value as TValueType : null;
            }

            /// <summary>
            ///     获取Session值
            /// </summary>
            /// <typeparam name="TValueType">值类型</typeparam>
            /// <param name="nullValue">默认空值</param>
            /// <returns>Session值</returns>
            public TValueType GetSession<TValueType>(TValueType nullValue) where TValueType : struct
            {
                var value = GetSession();
                return value != null ? (TValueType)value : nullValue;
            }

            /// <summary>
            ///     设置Session值
            /// </summary>
            /// <param name="value">值</param>
            /// <returns>是否设置成功</returns>
            public bool SetSession(object value)
            {
                var session = _socket.Session;
                if (session != null)
                {
                    TmphUint128 sessionId = GetSessionId(), newSessionId = session.Set(sessionId, value);
                    if (!sessionId.Equals(newSessionId))
                    {
                        Response.Cookies.Add(new TmphCookie(SessionName, newSessionId.ToHex(), DateTime.MinValue,
                            RequestHeader.Host, LocationPath, false, true));
                        _sessionId = newSessionId;
                    }
                    return true;
                }
                return false;
            }

            /// <summary>
            ///     删除Session
            /// </summary>
            public void RemoveSession()
            {
                var session = _socket.Session;
                if (session != null)
                {
                    var sessionId = GetSessionId();
                    if (sessionId.Low != 0)
                    {
                        session.Remove(sessionId);
                        Response.Cookies.Add(new TmphCookie(SessionName, TmphNullValue<byte>.Array, TmphPub.MinTime,
                            RequestHeader.Host, LocationPath, false, true));
                    }
                }
            }

            /// <summary>
            ///     设置Cookie
            /// </summary>
            /// <param name="cookie">Cookie</param>
            public void SetCookie(TmphCookie cookie)
            {
                if (cookie != null && cookie.Name != null) Response.Cookies.Add(cookie);
            }

            /// <summary>
            ///     获取Cookie
            /// </summary>
            /// <param name="name">名称</param>
            /// <returns>值</returns>
            public string GetCookie(string name)
            {
                return RequestHeader.GetCookie(name);
            }

            /// <summary>
            ///     获取Cookie
            /// </summary>
            /// <param name="name">名称</param>
            /// <returns>值</returns>
            public string GetCookie(byte[] name)
            {
                return RequestHeader.GetCookieString(name);
            }

            /// <summary>
            ///     判断是否存在Cookie值
            /// </summary>
            /// <param name="name">名称</param>
            /// <returns>是否存在Cookie值</returns>
            public bool IsCookie(byte[] name)
            {
                return RequestHeader.IsCookie(name);
            }

            ///// <summary>
            ///// 获取查询整数值
            ///// </summary>
            ///// <param name="name"></param>
            ///// <param name="nullValue"></param>
            ///// <returns></returns>
            //public int GetQueryInt(byte[] name, int nullValue)
            //{
            //    return requestHeader.GetQueryInt(name, nullValue);
            //}
            /// <summary>
            ///     设置内容类型
            /// </summary>
            public void JsContentType()
            {
                Response.SetJsContentType(DomainServer);
            }

            /// <summary>
            ///     缓存标识处理
            /// </summary>
            public unsafe struct TmphETag
            {
                /// <summary>
                ///     起始字符
                /// </summary>
                private const uint StartChar = ':' + 1;

                /// <summary>
                ///     是否处理页面版本
                /// </summary>
                private readonly bool _isPageVersion;

                /// <summary>
                ///     是否处理网站配置版本
                /// </summary>
                private readonly bool _isServerVersion;

                /// <summary>
                ///     WEB页面
                /// </summary>
                private readonly TmphPage _page;

                /// <summary>
                ///     当前数据位置
                /// </summary>
                private byte* _data;

                /// <summary>
                ///     缓存标识长度
                /// </summary>
                private int _length;

                /// <summary>
                ///     缓存标识处理
                /// </summary>
                /// <param name="page">WEB页面</param>
                public TmphETag(TmphPage page) : this(page, true, true)
                {
                }

                /// <summary>
                ///     缓存标识处理
                /// </summary>
                /// <param name="page">WEB页面</param>
                /// <param name="isPageVersion">是否处理页面版本</param>
                /// <param name="isServerVersion">是否处理网站配置版本</param>
                public TmphETag(TmphPage page, bool isPageVersion, bool isServerVersion)
                {
                    _page = page;
                    _isServerVersion = isServerVersion;
                    _isPageVersion = isPageVersion;
                    _length = 0;
                    _data = null;
                    if (isServerVersion) AddLength(0);
                    if (isPageVersion) AddLength(0);
                }

                /// <summary>
                ///     客户端缓存有效标识
                /// </summary>
                public TmphSubArray<byte> IfNoneMatch
                {
                    get { return _page.RequestHeader.IfNoneMatch; }
                }

                /// <summary>
                ///     判断长度是否匹配
                /// </summary>
                public bool IsLength
                {
                    get { return IfNoneMatch.Count == _length; }
                }

                /// <summary>
                ///     添加长度
                /// </summary>
                /// <param name="value">数据</param>
                public void AddLength(DateTime value)
                {
                    _length += 12;
                }

                /// <summary>
                ///     添加长度
                /// </summary>
                /// <param name="value">数据</param>
                public void AddLength(ulong value)
                {
                    _length += 12;
                }

                /// <summary>
                ///     添加长度
                /// </summary>
                /// <param name="value">数据</param>
                public void AddLength(long value)
                {
                    _length += 12;
                }

                /// <summary>
                ///     添加长度
                /// </summary>
                /// <param name="value">数据</param>
                public void AddLength(uint value)
                {
                    _length += 6;
                }

                /// <summary>
                ///     添加长度
                /// </summary>
                /// <param name="value">数据</param>
                public void AddLength(int value)
                {
                    _length += 6;
                }

                /// <summary>
                ///     添加长度
                /// </summary>
                /// <param name="value">数据</param>
                public void AddLength(ushort value)
                {
                    _length += 3;
                }

                /// <summary>
                ///     添加长度
                /// </summary>
                /// <param name="value">数据</param>
                public void AddLength(short value)
                {
                    _length += 3;
                }

                /// <summary>
                ///     添加长度
                /// </summary>
                /// <param name="value">数据</param>
                public void AddLength(char value)
                {
                    _length += 3;
                }

                /// <summary>
                ///     添加长度
                /// </summary>
                /// <param name="value">数据</param>
                public void AddLength(byte value)
                {
                    _length += 2;
                }

                /// <summary>
                ///     添加长度
                /// </summary>
                /// <param name="value">数据</param>
                public void AddLength(sbyte value)
                {
                    _length += 2;
                }

                /// <summary>
                ///     添加长度
                /// </summary>
                /// <param name="value">数据</param>
                public void AddLength(bool value)
                {
                    ++_length;
                }

                /// <summary>
                ///     添加长度
                /// </summary>
                /// <param name="value">数据</param>
                public void AddLength(string value)
                {
                    if (value != null) _length += (value.Length << 1) + value.Length;
                }

                /// <summary>
                ///     设置检测数据
                /// </summary>
                /// <param name="data">检测数据</param>
                public void SetCheckData(byte* data)
                {
                    _data = data;
                    if (_isServerVersion) Check(_page.DomainServer.WebConfig.ETagVersion);
                    if (_isPageVersion) Check(_page.ETagVersion);
                }

                /// <summary>
                ///     检测数据
                /// </summary>
                /// <param name="value">数据</param>
                public void Check(DateTime value)
                {
                    Check((ulong)value.Ticks);
                }

                /// <summary>
                ///     检测数据
                /// </summary>
                /// <param name="value">数据</param>
                public void Check(long value)
                {
                    Check((ulong)value);
                }

                /// <summary>
                ///     检测数据
                /// </summary>
                /// <param name="value">数据</param>
                public void Check(ulong value)
                {
                    Check((uint)value);
                    Check((uint)(value >> 32));
                }

                /// <summary>
                ///     检测数据
                /// </summary>
                /// <param name="value">数据</param>
                public void Check(int value)
                {
                    Check((uint)value);
                }

                /// <summary>
                ///     检测数据
                /// </summary>
                /// <param name="value">数据</param>
                public void Check(uint value)
                {
                    if (_data != null)
                    {
                        if (*_data == (value & 0x3fU) + StartChar
                            && _data[1] == ((value >> 6) & 0x3fU) + StartChar
                            && _data[2] == ((value >> 12) & 0x3fU) + StartChar
                            && _data[3] == ((value >> 18) & 0x3fU) + StartChar
                            && _data[4] == ((value >> 24) & 0x3fU) + StartChar
                            && _data[5] == (value >> 30) + StartChar)
                        {
                            _data += 6;
                        }
                        else _data = null;
                    }
                }

                /// <summary>
                ///     检测数据
                /// </summary>
                /// <param name="value">数据</param>
                public void Check(short value)
                {
                    Check((ushort)value);
                }

                /// <summary>
                ///     检测数据
                /// </summary>
                /// <param name="value">数据</param>
                public void Check(char value)
                {
                    Check((ushort)value);
                }

                /// <summary>
                ///     检测数据
                /// </summary>
                /// <param name="value">数据</param>
                public void Check(ushort value)
                {
                    if (_data != null)
                    {
                        if (*_data == (value & 0x3fU) + StartChar
                            && _data[1] == ((value >> 6) & 0x3fU) + StartChar
                            && _data[2] == (value >> 12) + StartChar)
                        {
                            _data += 3;
                        }
                        else _data = null;
                    }
                }

                /// <summary>
                ///     检测数据
                /// </summary>
                /// <param name="value">数据</param>
                public void Check(sbyte value)
                {
                    Check((byte)value);
                }

                /// <summary>
                ///     检测数据
                /// </summary>
                /// <param name="value">数据</param>
                public void Check(byte value)
                {
                    if (_data != null)
                    {
                        if (*_data == (value & 0x3fU) + StartChar
                            && _data[1] == (value >> 6) + StartChar)
                        {
                            _data += 2;
                        }
                        else _data = null;
                    }
                }

                /// <summary>
                ///     检测数据
                /// </summary>
                /// <param name="value">数据</param>
                public void Check(bool value)
                {
                    if (_data != null)
                    {
                        if (*_data == (value ? (byte)'1' : (byte)'0')) ++_data;
                        else _data = null;
                    }
                }

                /// <summary>
                ///     检测数据
                /// </summary>
                /// <param name="value">数据</param>
                public void Check(string value)
                {
                    if (_data != null && value != null)
                    {
                        foreach (var code in value)
                        {
                            Check((ushort)code);
                            if (_data == null) break;
                        }
                    }
                }

                /// <summary>
                ///     检测数据结束
                /// </summary>
                /// <returns>当前数据位置</returns>
                public byte* Check()
                {
                    if (_data != null) _page.NotChanged304();
                    return _data;
                }

                /// <summary>
                ///     设置数据
                /// </summary>
                /// <returns>缓存数据</returns>
                public byte[] Set()
                {
                    var eTag = new byte[_length];
                    _page.Response.SetETag(eTag);
                    return eTag;
                }

                /// <summary>
                ///     设置数据
                /// </summary>
                /// <param name="data">数据</param>
                public void SetData(byte* data)
                {
                    _data = data;
                    if (_isServerVersion) Set(_page.DomainServer.WebConfig.ETagVersion);
                    if (_isPageVersion) Set(_page.ETagVersion);
                }

                /// <summary>
                ///     设置数据
                /// </summary>
                /// <param name="value">数据</param>
                public void Set(DateTime value)
                {
                    Set((ulong)value.Ticks);
                }

                /// <summary>
                ///     设置数据
                /// </summary>
                /// <param name="value">数据</param>
                public void Set(long value)
                {
                    Set((ulong)value);
                }

                /// <summary>
                ///     设置数据
                /// </summary>
                /// <param name="value">数据</param>
                public void Set(ulong value)
                {
                    Set((uint)value);
                    Set((uint)(value >> 32));
                }

                /// <summary>
                ///     设置数据
                /// </summary>
                /// <param name="value">数据</param>
                public void Set(int value)
                {
                    Set((uint)value);
                }

                /// <summary>
                ///     设置数据
                /// </summary>
                /// <param name="value">数据</param>
                public void Set(uint value)
                {
                    *_data = (byte)((value & 0x3fU) + StartChar);
                    _data[1] = (byte)(((value >> 6) & 0x3fU) + StartChar);
                    _data[2] = (byte)(((value >> 12) & 0x3fU) + StartChar);
                    _data[3] = (byte)(((value >> 18) & 0x3fU) + StartChar);
                    _data[4] = (byte)(((value >> 24) & 0x3fU) + StartChar);
                    _data[5] = (byte)((value >> 30) + StartChar);
                    _data += 6;
                }

                /// <summary>
                ///     设置数据
                /// </summary>
                /// <param name="value">数据</param>
                public void Set(short value)
                {
                    Set((ushort)value);
                }

                /// <summary>
                ///     设置数据
                /// </summary>
                /// <param name="value">数据</param>
                public void Set(char value)
                {
                    Set((ushort)value);
                }

                /// <summary>
                ///     设置数据
                /// </summary>
                /// <param name="value">数据</param>
                public void Set(ushort value)
                {
                    *_data = (byte)((value & 0x3fU) + StartChar);
                    _data[1] = (byte)(((value >> 6) & 0x3fU) + StartChar);
                    _data[2] = (byte)((value >> 12) + StartChar);
                    _data += 3;
                }

                /// <summary>
                ///     设置数据
                /// </summary>
                /// <param name="value">数据</param>
                public void Set(sbyte value)
                {
                    Set((byte)value);
                }

                /// <summary>
                ///     设置数据
                /// </summary>
                /// <param name="value">数据</param>
                public void Set(byte value)
                {
                    *_data = (byte)((value & 0x3fU) + StartChar);
                    _data[1] = (byte)((value >> 6) + StartChar);
                    _data += 2;
                }

                /// <summary>
                ///     设置数据
                /// </summary>
                /// <param name="value">数据</param>
                public void Set(bool value)
                {
                    *_data++ = value ? (byte)'1' : (byte)'0';
                }

                /// <summary>
                ///     设置数据
                /// </summary>
                /// <param name="value">数据</param>
                public void Set(string value)
                {
                    if (value != null)
                    {
                        foreach (var code in value) Set((ushort)code);
                    }
                }
            }
        }
    }
}