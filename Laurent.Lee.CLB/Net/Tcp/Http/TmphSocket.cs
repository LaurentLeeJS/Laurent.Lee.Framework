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
using Laurent.Lee.CLB.IO;
using Laurent.Lee.CLB.Threading;
using Laurent.Lee.CLB.Web;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TmphHttp = Laurent.Lee.CLB.Config.TmphHttp;

namespace Laurent.Lee.CLB.Net.Tcp.Http
{
    /// <summary>
    ///     HTTP套接字
    /// </summary>
    public abstract class TmphSocketBase : TmphRequestForm.TmphILoadForm, IDisposable
    {
        /// <summary>
        ///     服务器类型
        /// </summary>
        private const string fastCSharpServer = @"Server: Laurent.Lee.CLB.http[C#]/1.0
";

        /// <summary>
        ///     HTTP服务版本号
        /// </summary>
        private const string httpVersionString = "HTTP/1.1";

        /// <summary>
        ///     大数据缓冲区
        /// </summary>
        protected static readonly TmphMemoryPool bigBuffers = TmphMemoryPool.GetPool(TmphHttp.Default.BigBufferSize);

        /// <summary>
        ///     HTTP套接字数量
        /// </summary>
        protected static int newCount;

        /// <summary>
        ///     HTTP头部接收超时队列
        /// </summary>
        internal static readonly TmphTimeoutQueue ReceiveTimeoutQueue = new TmphTimeoutQueue(TmphHttp.Default.ReceiveSeconds);

        /// <summary>
        ///     WebSocket超时队列
        /// </summary>
        internal static readonly TmphTimeoutQueue WebSocketReceiveTimeoutQueue =
            new TmphTimeoutQueue(TmphHttp.Default.WebSocketReceiveSeconds);

        /// <summary>
        ///     HTTP每4秒最小表单数据接收字节数
        /// </summary>
        protected static readonly int minReceiveSizePerSecond4 = TmphHttp.Default.MinReceiveSizePerSecond * 4;

        /// <summary>
        ///     错误输出缓存数据
        /// </summary>
        protected static readonly byte[][] errorResponseDatas;

        /// <summary>
        ///     HTTP服务版本号
        /// </summary>
        private static readonly byte[] httpVersion = httpVersionString.GetBytes();

        /// <summary>
        ///     服务器类型
        /// </summary>
        private static readonly byte[] responseServer = fastCSharpServer.GetBytes();

        /// <summary>
        ///     服务器类型
        /// </summary>
        private static readonly byte[] responseServerEnd = (fastCSharpServer + @"Content-Length: 0

").GetBytes();

        /// <summary>
        ///     100 Continue确认
        /// </summary>
        private static readonly byte[] continue100 =
            (httpVersionString +
             TmphEnum<TmphResponse.TmphState, TmphResponse.TmphStateInfo>.Array(TmphResponse.TmphState.Continue100).Text + @"
").GetBytes();

        /// <summary>
        ///     WebSocket握手确认
        /// </summary>
        private static readonly byte[] webSocket101 =
            (httpVersionString +
             TmphEnum<TmphResponse.TmphState, TmphResponse.TmphStateInfo>.Array(TmphResponse.TmphState.WebSocket101).Text +
             @"Connection: Upgrade
Upgrade: WebSocket
" + fastCSharpServer + @"Sec-WebSocket-Accept: ").GetBytes();

        /// <summary>
        ///     WebSocket确认哈希值
        /// </summary>
        private static readonly byte[] webSocketKey = ("258EAFA5-E914-47DA-95CA-C5AB0DC85B11").GetBytes();

        /// <summary>
        ///     HTTP响应输出内容长度名称
        /// </summary>
        private static readonly byte[] contentLengthResponseName = (TmphHeader.ContentLength + ": ").GetBytes();

        /// <summary>
        ///     HTTP响应输出日期名称
        /// </summary>
        private static readonly byte[] dateResponseName = (TmphHeader.Date + ": ").GetBytes();

        /// <summary>
        ///     HTTP响应输出最后修改名称
        /// </summary>
        private static readonly byte[] lastModifiedResponseName = (TmphHeader.LastModified + ": ").GetBytes();

        /// <summary>
        ///     重定向名称
        /// </summary>
        private static readonly byte[] locationResponseName = (TmphHeader.Location + ": ").GetBytes();

        /// <summary>
        ///     缓存参数名称
        /// </summary>
        private static readonly byte[] cacheControlResponseName = (TmphHeader.CacheControl + ": ").GetBytes();

        /// <summary>
        ///     内容类型名称
        /// </summary>
        private static readonly byte[] contentTypeResponseName = (TmphHeader.ContentType + ": ").GetBytes();

        /// <summary>
        ///     内容压缩编码名称
        /// </summary>
        private static readonly byte[] contentEncodingResponseName = (TmphHeader.ContentEncoding + ": ").GetBytes();

        /// <summary>
        ///     缓存匹配标识名称
        /// </summary>
        private static readonly byte[] eTagResponseName = (TmphHeader.ETag + @": """).GetBytes();

        /// <summary>
        ///     内容描述名称
        /// </summary>
        private static readonly byte[] contentDispositionResponseName = (TmphHeader.ContentDisposition + ": ").GetBytes();

        /// <summary>
        ///     请求范围名称
        /// </summary>
        private static readonly byte[] rangeName = (TmphHeader.AcceptRanges + @": bytes
" + TmphHeader.ContentRange + ": bytes ").GetBytes();

        /// <summary>
        ///     HTTP响应输出保持连接
        /// </summary>
        private static readonly byte[] defaultKeepAlive = (TmphHeader.Connection + @": Keep-Alive
").GetBytes();

        /// <summary>
        ///     HTTP响应输出Cookie名称
        /// </summary>
        private static readonly byte[] setCookieResponseName = (TmphHeader.SetCookie + ": ").GetBytes();

        /// <summary>
        ///     Cookie域名
        /// </summary>
        private static readonly byte[] cookieDomainName = ("; Domain=").GetBytes();

        /// <summary>
        ///     Cookie有效路径
        /// </summary>
        private static readonly byte[] cookiePathName = ("; Path=").GetBytes();

        /// <summary>
        ///     Cookie有效期
        /// </summary>
        private static readonly byte[] cookieExpiresName = ("; Expires=").GetBytes();

        /// <summary>
        ///     Cookie安全标识
        /// </summary>
        private static readonly byte[] cookieSecureName = ("; Secure").GetBytes();

        /// <summary>
        ///     Cookie是否http only
        /// </summary>
        private static readonly byte[] cookieHttpOnlyName = ("; HttpOnly").GetBytes();

        /// <summary>
        ///     Cookie最小时间超时时间
        /// </summary>
        private static readonly byte[] pubMinTimeCookieExpires = TmphPub.MinTime.toBytes();

        /// <summary>
        ///     最后一次生成的时间字节数组
        /// </summary>
        private static TmphKeyValue<long, byte[]> dateCache = new TmphKeyValue<long, byte[]>(0,
            new byte[(TmphDate.ToByteLength + 3) & (int.MaxValue - 3)]);

        /// <summary>
        ///     时间字节数组访问锁
        /// </summary>
        private static int dateCacheLock;

        /// <summary>
        ///     超时检测
        /// </summary>
        private readonly Func<int, bool> checkTimeoutHandle;

        /// <summary>
        ///     HTTP内容数据缓冲区
        /// </summary>
        internal byte[] Buffer;

        /// <summary>
        ///     域名服务
        /// </summary>
        internal TmphServers.TmphDomainServer DomainServer;

        /// <summary>
        ///     HTTP请求表单
        /// </summary>
        protected TmphRequestForm form;

        /// <summary>
        ///     操作标识
        /// </summary>
        protected long identity;

        /// <summary>
        ///     客户端IP地址
        /// </summary>
        protected int ipv4;

        /// <summary>
        ///     客户端IP地址
        /// </summary>
        protected TmphIpv6Hash ipv6;

        /// <summary>
        ///     是否加载表单
        /// </summary>
        protected byte isLoadForm;

        /// <summary>
        ///     是否正在处理下一个请求
        /// </summary>
        protected byte isNextRequest;

        /// <summary>
        ///     当前输出HTTP响应
        /// </summary>
        protected TmphResponse response;

        /// <summary>
        ///     当前输出HTTP响应字节数
        /// </summary>
        protected long responseSize;

        /// <summary>
        ///     发送数据后关闭套接字
        /// </summary>
        protected Action<bool> sendClose;

        /// <summary>
        ///     发送数据后处理下一个请求
        /// </summary>
        protected Action<bool> sendNext;

        /// <summary>
        ///     发送数据后继续发送HTTP响应内容
        /// </summary>
        protected Action<bool> sendResponseBody;

        /// <summary>
        ///     HTTP服务器
        /// </summary>
        protected TmphServers servers;

        /// <summary>
        ///     套接字
        /// </summary>
        internal Socket Socket;

        /// <summary>
        ///     超时标识
        /// </summary>
        protected int timeoutIdentity;

        static TmphSocketBase()
        {
            errorResponseDatas = new byte[TmphEnum.GetMaxValue<TmphResponse.TmphState>(-1) + 1][];
            foreach (TmphResponse.TmphState type in Enum.GetValues(typeof(TmphResponse.TmphState)))
            {
                var state = TmphEnum<TmphResponse.TmphState, TmphResponse.TmphStateInfo>.Array((int)type);
                if (state != null && state.IsError)
                {
                    byte[] stateData = state.Bytes,
                        responseData = new byte[httpVersion.Length + stateData.Length + responseServerEnd.Length];
                    System.Buffer.BlockCopy(httpVersion, 0, responseData, 0, httpVersion.Length);
                    var index = httpVersion.Length;
                    System.Buffer.BlockCopy(stateData, 0, responseData, index, stateData.Length);
                    index += stateData.Length;
                    System.Buffer.BlockCopy(responseServerEnd, 0, responseData, index, responseServerEnd.Length);
                    errorResponseDatas[(int)type] = responseData;
                }
            }
        }

        /// <summary>
        ///     HTTP套接字
        /// </summary>
        protected TmphSocketBase()
        {
            Buffer = new byte[TmphHttp.Default.HeaderBufferLength + sizeof(int)];
            form = new TmphRequestForm();
            sendClose = close;
            sendNext = next;
            sendResponseBody = responseBody;
            checkTimeoutHandle = checkTimeout;
        }

        /// <summary>
        ///     HTTP套接字数量
        /// </summary>
        public static int NewCount
        {
            get { return newCount; }
        }

        /// <summary>
        /// </summary>
        public static int PoolCount
        {
            get { return TmphTypePool<TmphSocket>.Count(); }
        }

        /// <summary>
        ///     获取HTTP请求头部
        /// </summary>
        internal abstract TmphRequestHeader RequestHeader { get; }

        /// <summary>
        ///     TCP调用套接字
        /// </summary>
        public TmphCommandSocket TcpCommandSocket { get; protected set; }

        /// <summary>
        ///     远程终结点
        /// </summary>
        internal EndPoint RemoteEndPoint
        {
            get { return Socket.RemoteEndPoint; }
        }

        /// <summary>
        ///     获取Session
        /// </summary>
        internal TmphISession Session
        {
            get { return DomainServer.Server.Session; }
        }

        /// <summary>
        ///     释放资源
        /// </summary>
        public virtual void Dispose()
        {
            form.Clear();
            TmphResponse.Push(ref response);
        }

        /// <summary>
        ///     表单回调处理
        /// </summary>
        /// <param name="form">HTTP请求表单</param>
        public void OnGetForm(TmphRequestForm form)
        {
            if (form == null) responseHeader();
            else
            {
                TmphResponse.Push(ref response);
                headerError();
            }
        }

        /// <summary>
        ///     根据HTTP请求表单值获取内存流最大字节数
        /// </summary>
        /// <param name="value">HTTP请求表单值</param>
        /// <returns>内存流最大字节数</returns>
        public int MaxMemoryStreamSize(TmphRequestForm.TmphValue value)
        {
            return 0;
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
        ///     获取数据缓冲区
        /// </summary>
        /// <param name="length">缓冲区字节长度</param>
        /// <returns>数据缓冲区</returns>
        protected static TmphMemoryPool getMemoryPool(int length)
        {
            return length <= TmphAppSetting.StreamBufferSize ? TmphMemoryPool.StreamBuffers : bigBuffers;
        }

        /// <summary>
        ///     获取当前时间字节数组
        /// </summary>
        /// <param name="data">输出数据起始位置</param>
        private static unsafe void getDate(byte* data)
        {
            var now = TmphDate.NowSecond;
            var second = now.Ticks / 10000000;
            fixed (byte* cacheFixed = dateCache.Value)
            {
                TmphInterlocked.NoCheckCompareSetSleep0(ref dateCacheLock);
                try
                {
                    if (dateCache.Key != second)
                    {
                        dateCache.Key = second;
                        TmphDate.ToBytes(now, cacheFixed);
                    }
                    Unsafe.TmphMemory.Copy(cacheFixed, data, dateCache.Value.Length);
                }
                finally
                {
                    dateCacheLock = 0;
                }
            }
        }

        /// <summary>
        ///     HTTP头部接收错误
        /// </summary>
        protected abstract void headerError();

        /// <summary>
        ///     WebSocket结束
        /// </summary>
        protected abstract void webSocketEnd();

        /// <summary>
        ///     处理下一个请求
        /// </summary>
        /// <param name="isSend">是否发送成功</param>
        private void next(bool isSend)
        {
            TmphResponse.Push(ref response);
            if (isSend)
            {
                form.Clear();
                if (RequestHeader.IsKeepAlive)
                {
                    isNextRequest = 1;
                    isLoadForm = 0;
                    receiveHeader();
                    return;
                }
            }
            headerError();
        }

        /// <summary>
        ///     开始接收头部数据
        /// </summary>
        protected abstract void receiveHeader();

        /// <summary>
        ///     未能识别的HTTP头部
        /// </summary>
        protected abstract void headerUnknown();

        /// <summary>
        ///     获取域名服务信息
        /// </summary>
        private void request()
        {
            var identity = this.identity;
            try
            {
                if (RequestHeader.IsWebSocket) responseWebSocket101();
                else DomainServer.Server.Request(this, identity);
                return;
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, false);
            }
            ResponseError(identity, TmphResponse.TmphState.ServerError500);
        }

        /// <summary>
        ///     检测100 Continue确认
        /// </summary>
        protected bool check100Continue()
        {
            if (RequestHeader.Is100Continue)
            {
                RequestHeader.Is100Continue = false;
                try
                {
                    var error = SocketError.Success;
                    if (Socket.send(continue100, 0, continue100.Length, ref error)) return true;
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
            }
            else return true;
            return false;
        }

        /// <summary>
        ///     获取请求表单数据
        /// </summary>
        /// <param name="identity">HTTP操作标识</param>
        /// <param name="loadForm">HTTP请求表单加载接口</param>
        internal abstract void GetForm(long identity, TmphRequestForm.TmphILoadForm loadForm);

        /// <summary>
        ///     输出HTTP响应数据
        /// </summary>
        /// <param name="identity">HTTP操作标识</param>
        /// <param name="response">HTTP响应数据</param>
        public abstract bool Response(long identity, ref TmphResponse response);

        /// <summary>
        ///     输出HTTP响应数据
        /// </summary>
        /// <param name="identity">HTTP操作标识</param>
        /// <param name="response">HTTP响应数据</param>
        internal bool Response(long identity, TmphResponse response)
        {
            return Response(identity, ref response);
        }

        /// <summary>
        ///     输出错误状态
        /// </summary>
        /// <param name="identity">操作标识</param>
        /// <param name="state">错误状态</param>
        public bool ResponseError(long identity, TmphResponse.TmphState state)
        {
            if (identity >= 0)
            {
                if (Interlocked.CompareExchange(ref this.identity, identity + 1, identity) == identity)
                {
                    responseError(state);
                    return true;
                }
                return false;
            }
            return WebSocketResponseError(identity, state);
        }

        /// <summary>
        ///     输出错误状态
        /// </summary>
        /// <param name="identity">操作标识</param>
        /// <param name="state">错误状态</param>
        internal abstract bool WebSocketResponseError(long identity, TmphResponse.TmphState state);

        /// <summary>
        ///     输出错误状态
        /// </summary>
        /// <param name="state">错误状态</param>
        protected abstract void responseError(TmphResponse.TmphState state);

        /// <summary>
        ///     HTTP响应头部输出
        /// </summary>
        protected unsafe void responseHeader()
        {
            try
            {
                var requestHeader = RequestHeader;
                if (response.Body.Count != 0)
                {
                    response.BodyFile = null;
                    if (requestHeader.IsKeepAlive && response.CanHeader && !requestHeader.IsRange)
                    {
                        var body = response.HeaderBody;
                        if (body.Count != 0)
                        {
                            responseSize = 0;
                            responseHeader(body, null);
                            return;
                        }
                    }
                }
                var bodySize = response.BodySize;
                byte* responseSizeFixed = stackalloc byte[24 * 4], bodySizeFixed = responseSizeFixed + 24;
                byte* rangeStartFixed = responseSizeFixed + 24 * 2, rangeEndFixed = responseSizeFixed + 24 * 3;
                byte* responseSizeWrite = responseSizeFixed, bodySizeWrite = responseSizeFixed;
                byte* rangeStartWrite = rangeStartFixed, rangeEndWrite = rangeEndFixed;
                if (requestHeader.IsRange)
                {
                    if (requestHeader.IsFormatRange || requestHeader.FormatRange(bodySize))
                    {
                        if (response.State == TmphResponse.TmphState.Ok200)
                        {
                            responseSize = requestHeader.RangeLength;
                            if (requestHeader.RangeStart != 0)
                                rangeStartWrite = sizeToBytes(requestHeader.RangeStart, rangeStartFixed);
                            if (requestHeader.RangeEnd != bodySize - 1)
                                rangeEndWrite = zeroSizeToBytes(requestHeader.RangeEnd, rangeEndFixed);
                            bodySizeWrite = zeroSizeToBytes(bodySize, bodySizeFixed);
                            response.State = TmphResponse.TmphState.PartialContent206;
                        }
                        else responseSize = bodySize;
                    }
                    else
                    {
                        response.State = TmphResponse.TmphState.RangeNotSatisfiable416;
                        responseSize = 0;
                    }
                }
                else responseSize = bodySize;
                responseSizeWrite = zeroSizeToBytes(responseSize, responseSizeFixed);
                var state = TmphEnum<TmphResponse.TmphState, TmphResponse.TmphStateInfo>.Array(response.State);
                if (state == null)
                    state = TmphEnum<TmphResponse.TmphState, TmphResponse.TmphStateInfo>.Array(TmphResponse.TmphState.ServerError500);
                var index = httpVersion.Length + state.Bytes.Length + contentLengthResponseName.Length +
                            (int)(responseSizeWrite - responseSizeFixed) + 2
                            + responseServer.Length + dateResponseName.Length + TmphDate.ToByteLength + 2 + 2;
                if (response.State == TmphResponse.TmphState.PartialContent206)
                {
                    index += rangeName.Length + (int)(rangeStartWrite - rangeStartFixed) +
                             (int)(rangeEndWrite - rangeEndFixed) + (int)(bodySizeWrite - bodySizeFixed) + 2 + 2;
                }
                if (response.Location.Count != 0) index += locationResponseName.Length + response.Location.Count + 2;
                if (response.LastModified != null)
                    index += lastModifiedResponseName.Length + response.LastModified.Length + 2;
                if (response.CacheControl != null)
                    index += cacheControlResponseName.Length + response.CacheControl.Length + 2;
                if (response.ContentType != null)
                    index += contentTypeResponseName.Length + response.ContentType.Length + 2;
                if (response.ContentEncoding != null)
                    index += contentEncodingResponseName.Length + response.ContentEncoding.Length + 2;
                if (response.ETag != null) index += eTagResponseName.Length + response.ETag.Length + 2 + 1;
                if (response.ContentDisposition != null)
                    index += contentDispositionResponseName.Length + response.ContentDisposition.Length + 2;
                if (requestHeader.IsKeepAlive) index += defaultKeepAlive.Length;
                var cookieCount = response.Cookies.Count;
                if (cookieCount != 0)
                {
                    index += (setCookieResponseName.Length + 3) * cookieCount;
                    foreach (var cookie in response.Cookies.array)
                    {
                        index += cookie.Name.Length + cookie.Value.length();
                        if (cookie.Domain.Count != 0) index += cookieDomainName.Length + cookie.Domain.Count;
                        if (cookie.Path != null) index += cookiePathName.Length + cookie.Path.Length;
                        if (cookie.Expires != DateTime.MinValue)
                            index += cookieExpiresName.Length + TmphDate.ToByteLength;
                        if (cookie.IsSecure) index += cookieSecureName.Length;
                        if (cookie.IsHttpOnly) index += cookieHttpOnlyName.Length;
                        if (--cookieCount == 0) break;
                    }
                }
                var checkIndex = index;
                byte[] buffer;
                TmphMemoryPool memoryPool;
                if ((index += 3) <= (TmphHttp.Default.HeaderBufferLength + 4))
                {
                    buffer = Buffer;
                    memoryPool = null;
                }
                else
                {
                    memoryPool = getMemoryPool(index);
                    buffer = memoryPool.Get(index);
                }
                fixed (byte* bufferFixed = buffer)
                {
                    System.Buffer.BlockCopy(httpVersion, 0, buffer, 0, index = httpVersion.Length);
                    System.Buffer.BlockCopy(state.Bytes, 0, buffer, index, state.Bytes.Length);
                    System.Buffer.BlockCopy(contentLengthResponseName, 0, buffer, index += state.Bytes.Length,
                        contentLengthResponseName.Length);
                    var write = bufferFixed + (index += contentLengthResponseName.Length);
                    index += (int)(responseSizeWrite - responseSizeFixed) + 2;
                    while (responseSizeWrite != responseSizeFixed) *write++ = *--responseSizeWrite;
                    *(short*)write = 0x0a0d;
                    if (response.State == TmphResponse.TmphState.PartialContent206)
                    {
                        System.Buffer.BlockCopy(rangeName, 0, buffer, index, rangeName.Length);
                        write = bufferFixed + (index += rangeName.Length);
                        index += (int)(rangeStartWrite - rangeStartFixed) + (int)(rangeEndWrite - rangeEndFixed) +
                                 (int)(bodySizeWrite - bodySizeFixed) + 2 + 2;
                        while (rangeStartWrite != rangeStartFixed) *write++ = *--rangeStartWrite;
                        *write++ = (byte)'-';
                        while (rangeEndWrite != rangeEndFixed) *write++ = *--rangeEndWrite;
                        *write++ = (byte)'/';
                        while (bodySizeWrite != bodySizeFixed) *write++ = *--bodySizeWrite;
                        *(short*)write = 0x0a0d;
                    }
                    if (response.Location.Count != 0)
                    {
                        System.Buffer.BlockCopy(locationResponseName, 0, buffer, index, locationResponseName.Length);
                        System.Buffer.BlockCopy(response.Location.Array, response.Location.StartIndex, buffer,
                            index += locationResponseName.Length, response.Location.Count);
                        *(short*)(bufferFixed + (index += response.Location.Count)) = 0x0a0d;
                        index += 2;
                    }
                    if (response.CacheControl != null)
                    {
                        System.Buffer.BlockCopy(cacheControlResponseName, 0, buffer, index,
                            cacheControlResponseName.Length);
                        System.Buffer.BlockCopy(response.CacheControl, 0, buffer,
                            index += cacheControlResponseName.Length, response.CacheControl.Length);
                        *(short*)(bufferFixed + (index += response.CacheControl.Length)) = 0x0a0d;
                        index += 2;
                    }
                    if (response.ContentType != null)
                    {
                        System.Buffer.BlockCopy(contentTypeResponseName, 0, buffer, index,
                            contentTypeResponseName.Length);
                        System.Buffer.BlockCopy(response.ContentType, 0, buffer, index += contentTypeResponseName.Length,
                            response.ContentType.Length);
                        *(short*)(bufferFixed + (index += response.ContentType.Length)) = 0x0a0d;
                        index += 2;
                    }
                    if (response.ContentEncoding != null)
                    {
                        System.Buffer.BlockCopy(contentEncodingResponseName, 0, buffer, index,
                            contentEncodingResponseName.Length);
                        System.Buffer.BlockCopy(response.ContentEncoding, 0, buffer,
                            index += contentEncodingResponseName.Length, response.ContentEncoding.Length);
                        *(short*)(bufferFixed + (index += response.ContentEncoding.Length)) = 0x0a0d;
                        index += 2;
                    }
                    if (response.ETag != null)
                    {
                        System.Buffer.BlockCopy(eTagResponseName, 0, buffer, index, eTagResponseName.Length);
                        System.Buffer.BlockCopy(response.ETag, 0, buffer, index += eTagResponseName.Length,
                            response.ETag.Length);
                        *(bufferFixed + (index += response.ETag.Length)) = (byte)'"';
                        *(short*)(bufferFixed + (++index)) = 0x0a0d;
                        index += 2;
                    }
                    if (response.ContentDisposition != null)
                    {
                        System.Buffer.BlockCopy(contentDispositionResponseName, 0, buffer, index,
                            contentDispositionResponseName.Length);
                        System.Buffer.BlockCopy(response.ContentDisposition, 0, buffer,
                            index += contentDispositionResponseName.Length, response.ContentDisposition.Length);
                        *(short*)(bufferFixed + (index += response.ContentDisposition.Length)) = 0x0a0d;
                        index += 2;
                    }
                    if ((cookieCount = response.Cookies.Count) != 0)
                    {
                        foreach (var cookie in response.Cookies.array)
                        {
                            System.Buffer.BlockCopy(setCookieResponseName, 0, buffer, index,
                                setCookieResponseName.Length);
                            System.Buffer.BlockCopy(cookie.Name, 0, buffer, index += setCookieResponseName.Length,
                                cookie.Name.Length);
                            *(bufferFixed + (index += cookie.Name.Length)) = (byte)'=';
                            ++index;
                            if (cookie.Value.length() != 0)
                            {
                                System.Buffer.BlockCopy(cookie.Value, 0, buffer, index, cookie.Value.Length);
                                index += cookie.Value.Length;
                            }
                            if (cookie.Domain.Count != 0)
                            {
                                System.Buffer.BlockCopy(cookieDomainName, 0, buffer, index, cookieDomainName.Length);
                                System.Buffer.BlockCopy(cookie.Domain.Array, cookie.Domain.StartIndex, buffer,
                                    index += cookieDomainName.Length, cookie.Domain.Count);
                                index += cookie.Domain.Count;
                            }
                            if (cookie.Path != null)
                            {
                                System.Buffer.BlockCopy(cookiePathName, 0, buffer, index, cookiePathName.Length);
                                System.Buffer.BlockCopy(cookie.Path, 0, buffer, index += cookiePathName.Length,
                                    cookie.Path.Length);
                                index += cookie.Path.Length;
                            }
                            if (cookie.Expires != DateTime.MinValue)
                            {
                                System.Buffer.BlockCopy(cookieExpiresName, 0, buffer, index, cookieExpiresName.Length);
                                index += cookieExpiresName.Length;
                                if (cookie.Expires == TmphPub.MinTime)
                                {
                                    System.Buffer.BlockCopy(pubMinTimeCookieExpires, 0, buffer, index,
                                        pubMinTimeCookieExpires.Length);
                                }
                                else TmphDate.ToBytes(cookie.Expires, bufferFixed + index);
                                index += TmphDate.ToByteLength;
                            }
                            if (cookie.IsSecure)
                            {
                                System.Buffer.BlockCopy(cookieSecureName, 0, buffer, index, cookieSecureName.Length);
                                index += cookieSecureName.Length;
                            }
                            if (cookie.IsHttpOnly)
                            {
                                System.Buffer.BlockCopy(cookieHttpOnlyName, 0, buffer, index, cookieHttpOnlyName.Length);
                                index += cookieHttpOnlyName.Length;
                            }
                            *(short*)(bufferFixed + index) = 0x0a0d;
                            index += 2;
                            if (--cookieCount == 0) break;
                        }
                    }
                    System.Buffer.BlockCopy(responseServer, 0, buffer, index, responseServer.Length);
                    index += responseServer.Length;
                    if (requestHeader.IsKeepAlive)
                    {
                        System.Buffer.BlockCopy(defaultKeepAlive, 0, buffer, index, defaultKeepAlive.Length);
                        index += defaultKeepAlive.Length;
                    }
                    if (response.LastModified != null)
                    {
                        System.Buffer.BlockCopy(lastModifiedResponseName, 0, buffer, index,
                            lastModifiedResponseName.Length);
                        System.Buffer.BlockCopy(response.LastModified, 0, buffer,
                            index += lastModifiedResponseName.Length, response.LastModified.Length);
                        *(short*)(bufferFixed + (index += response.LastModified.Length)) = 0x0a0d;
                        index += 2;
                    }
                    System.Buffer.BlockCopy(dateResponseName, 0, buffer, index, dateResponseName.Length);
                    getDate(bufferFixed + (index += dateResponseName.Length));
                    *(int*)(bufferFixed + (index += TmphDate.ToByteLength)) = 0x0a0d0a0d;
                    index += 4;
                    //if (checkIndex != index) log.Default.Add("responseHeader checkIndex[" + checkIndex.toString() + "] != index[" + index.toString() + "]", true, false);
                    if (response.Body.Count != 0)
                    {
                        if (requestHeader.IsKeepAlive && response.CanHeader &&
                            (index + sizeof(int)) <= response.Body.StartIndex && !requestHeader.IsRange)
                        {
                            fixed (byte* bodyFixed = response.Body.array)
                            {
                                Unsafe.TmphMemory.Copy(bufferFixed, bodyFixed + response.Body.StartIndex - index, index);
                                *(int*)bodyFixed = index;
                            }
                            responseSize = 0;
                            responseHeader(response.HeaderBody, null);
                            return;
                        }
                        if (buffer.Length - index >= (int)responseSize)
                        {
                            //if (response.Body.Length != responseSize) log.Default.Add("response.Body.Length[" + response.Body.Length.toString() + "] != responseSize[" + responseSize.toString() + "]", true, false);
                            System.Buffer.BlockCopy(response.Body.Array,
                                response.State == TmphResponse.TmphState.PartialContent206
                                    ? response.Body.StartIndex + (int)requestHeader.RangeStart
                                    : response.Body.StartIndex, buffer, index, (int)responseSize);
                            index += (int)responseSize;
                            responseSize = 0;
                        }
                        ////showjim
                        //else if (response.Body.Count > response.Body.array.Length)
                        //{
                        //    fixed (byte* headerBufferFixed = requestHeader.TmphBuffer)
                        //    {
                        //        log.Error.Add(Laurent.Lee.CLB.String.DeSerialize(headerBufferFixed + requestHeader.Uri.StartIndex, -2 - requestHeader.Uri.Count) + Laurent.Lee.CLB.String.DeSerialize(bufferFixed, -index) + response.Body.Count.toString() + " > " + response.Body.array.Length.toString(), true, false);
                        //    }
                        //}
                    }
                }
                responseHeader(TmphSubArray<byte>.Unsafe(buffer, 0, index), memoryPool);
                return;
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, false);
                fixed (byte* headerBufferFixed = RequestHeader.TmphBuffer)
                {
                    TmphLog.Error.Add(@"responseSize[" + responseSize.toString() + "] body[" +
                                    response.Body.array.Length.toString() + "," + response.Body.StartIndex.toString() +
                                    "," + response.Body.Count.toString() + @"]
" + TmphString.DeSerialize(headerBufferFixed, -RequestHeader.EndIndex), true, false);
                }
            }
            headerError();
        }

        /// <summary>
        ///     HTTP响应头部输出
        /// </summary>
        /// <param name="TmphBuffer">输出数据</param>
        /// <param name="memoryPool">内存池</param>
        protected abstract void responseHeader(TmphSubArray<byte> TmphBuffer, TmphMemoryPool memoryPool);

        /// <summary>
        ///     获取AJAX回调函数
        /// </summary>
        /// <param name="identity">操作标识</param>
        /// <returns>AJAX回调函数,失败返回null</returns>
        internal abstract TmphSubString GetWebSocketCallBack(long identity);

        /// <summary>
        ///     WebSocket响应协议输出
        /// </summary>
        private unsafe void responseWebSocket101()
        {
            var index = webSocket101.Length;
            System.Buffer.BlockCopy(webSocket101, 0, Buffer, 0, index);
            var secWebSocketKey = RequestHeader.SecWebSocketKey;
            System.Buffer.BlockCopy(secWebSocketKey.Array, secWebSocketKey.StartIndex, Buffer, index,
                secWebSocketKey.Count);
            System.Buffer.BlockCopy(webSocketKey, 0, Buffer, index + secWebSocketKey.Count, webSocketKey.Length);
            var acceptKey = TmphPub.Sha1(Buffer, index, secWebSocketKey.Count + webSocketKey.Length);
            fixed (byte* bufferFixed = Buffer, acceptKeyFixed = acceptKey)
            {
                byte* write = bufferFixed + webSocket101.Length,
                    keyEnd = acceptKeyFixed + 18,
                    base64 = TmphString.Base64.Byte;
                for (var read = acceptKeyFixed; read != keyEnd; read += 3)
                {
                    *write++ = *(base64 + (*read >> 2));
                    *write++ = *(base64 + (((*read << 4) | (*(read + 1) >> 4)) & 0x3f));
                    *write++ = *(base64 + (((*(read + 1) << 2) | (*(read + 2) >> 6)) & 0x3f));
                    *write++ = *(base64 + (*(read + 2) & 0x3f));
                }
                *write++ = *(base64 + (*keyEnd >> 2));
                *write++ = *(base64 + (((*keyEnd << 4) | (*(keyEnd + 1) >> 4)) & 0x3f));
                *write++ = *(base64 + ((*(keyEnd + 1) << 2) & 0x3f));
                *write++ = (byte)'=';
                *(int*)write = 0x0a0d0a0d;
            }
            var error = SocketError.Success;
            if (Socket.send(Buffer, 0, index += 32, ref error))
            {
                Interlocked.Increment(ref identity);
                receiveWebSocket();
            }
            else ResponseError(identity, TmphResponse.TmphState.ServerError500);
        }

        /// <summary>
        ///     开始接收WebSocket数据
        /// </summary>
        protected abstract void receiveWebSocket();

        /// <summary>
        ///     发送HTTP响应内容
        /// </summary>
        /// <param name="isSend">是否发送成功</param>
        protected abstract void responseBody(bool isSend);

        /// <summary>
        ///     关闭套接字
        /// </summary>
        /// <param name="isSend">是否发送成功</param>
        private void close(bool isSend)
        {
            headerError();
        }

        /// <summary>
        ///     设置超时
        /// </summary>
        /// <param name="identity">超时标识</param>
        protected void setTimeout(int identity)
        {
            if (identity == timeoutIdentity) ReceiveTimeoutQueue.Add(Socket, checkTimeoutHandle, identity);
        }

        /// <summary>
        ///     超时检测
        /// </summary>
        /// <param name="identity">超时标识</param>
        /// <returns>是否超时</returns>
        private bool checkTimeout(int identity)
        {
            return identity == timeoutIdentity;
        }

        /// <summary>
        ///     关闭套接字
        /// </summary>
        /// <param name="socket">套接字</param>
        protected static void close(Socket socket)
        {
            if (socket != null)
            {
                if (!socket.Connected)
                {
                    try
                    {
                        socket.Shutdown(SocketShutdown.Both);
                    }
                    catch (Exception error)
                    {
                        TmphLog.Default.Add(error, null, false);
                    }
                }
                socket.Close();
            }
        }

        /// <summary>
        ///     长度数字转换为字节串
        /// </summary>
        /// <param name="size">长度数字,不能为0</param>
        /// <param name="start">字节串起始位置</param>
        /// <returns>字节串结束位置</returns>
        private static unsafe byte* sizeToBytes(long size, byte* start)
        {
            if (size <= int.MaxValue)
            {
                var size32 = (int)size;
                for (*start++ = (byte)((size32 % 10) + '0'); (size32 /= 10) != 0; *start++ = (byte)((size32 % 10) + '0'))
                    ;
            }
            else
            {
                for (*start++ = (byte)((size % 10) + '0'); (size /= 10) != 0; *start++ = (byte)((size % 10) + '0')) ;
            }
            return start;
        }

        /// <summary>
        ///     长度数字转换为字节串
        /// </summary>
        /// <param name="size">长度数字,可能为0</param>
        /// <param name="start">字节串起始位置</param>
        /// <returns>字节串结束位置</returns>
        private static unsafe byte* zeroSizeToBytes(long size, byte* start)
        {
            if (size == 0)
            {
                *start = (byte)'0';
                return start + 1;
            }
            return sizeToBytes(size, start);
        }

        /// <summary>
        ///     HTTP头部接收器
        /// </summary>
        /// <typeparam name="TSocketType">套接字类型</typeparam>
        internal abstract class TmphHeaderReceiver<TSocketType> where TSocketType : TmphSocketBase
        {
            /// <summary>
            ///     HTTP头部数据缓冲区
            /// </summary>
            protected byte[] TmphBuffer;

            /// <summary>
            ///     HTTP头部数据结束位置
            /// </summary>
            public int HeaderEndIndex;

            /// <summary>
            ///     接收数据结束位置
            /// </summary>
            public int ReceiveEndIndex;

            /// <summary>
            ///     HTTP请求头部
            /// </summary>
            public TmphRequestHeader RequestHeader;

            /// <summary>
            ///     HTTP套接字
            /// </summary>
            protected TSocketType socket;

            /// <summary>
            ///     超时时间
            /// </summary>
            protected DateTime timeout;

            /// <summary>
            ///     HTTP头部接收器
            /// </summary>
            /// <param name="socket">HTTP套接字</param>
            public TmphHeaderReceiver(TSocketType socket)
            {
                this.socket = socket;
                TmphBuffer = (RequestHeader = new TmphRequestHeader()).TmphBuffer;
            }

            /// <summary>
            ///     接受头部换行数据
            /// </summary>
            protected abstract void receive();

            /// <summary>
            ///     接受头部数据处理
            /// </summary>
            protected unsafe void onReceive()
            {
                var searchEndIndex = ReceiveEndIndex - sizeof(int);
                if (HeaderEndIndex <= searchEndIndex)
                {
                    fixed (byte* dataFixed = TmphBuffer)
                    {
                        byte* start = dataFixed + HeaderEndIndex,
                            searchEnd = dataFixed + searchEndIndex,
                            end = dataFixed + ReceiveEndIndex;
                        *end = 13;
                        do
                        {
                            while (*start != 13) ++start;
                            if (start <= searchEnd)
                            {
                                if (*(int*)start == 0x0a0d0a0d)
                                {
                                    HeaderEndIndex = (int)(start - dataFixed);
                                    var isParseHeader = RequestHeader.Parse(HeaderEndIndex, ReceiveEndIndex);
                                    if (RequestHeader.Host.Count != 0)
                                        socket.DomainServer = socket.servers.GetServer(RequestHeader.Host);
                                    if (isParseHeader && socket.DomainServer != null)
                                    {
                                        if (RequestHeader.IsHeaderError) socket.headerError();
                                        else if (RequestHeader.IsRangeError)
                                            socket.responseError(TmphResponse.TmphState.RangeNotSatisfiable416);
                                        else socket.request();
                                    }
                                    else socket.headerUnknown();
                                    return;
                                }
                                ++start;
                            }
                            else
                            {
                                HeaderEndIndex = (int)(start - dataFixed);
                                break;
                            }
                        } while (true);
                    }
                }
                if (ReceiveEndIndex == TmphHttp.Default.HeaderBufferLength) socket.headerUnknown();
                else receive();
            }
        }

        /// <summary>
        ///     表单数据接收器
        /// </summary>
        /// <typeparam name="TSocketType">套接字类型</typeparam>
        protected abstract class TmphFormIdentityReceiver<TSocketType> where TSocketType : TmphSocketBase
        {
            /// <summary>
            ///     HTTP请求表单
            /// </summary>
            private readonly TmphRequestForm form;

            /// <summary>
            ///     表单接收缓冲区
            /// </summary>
            protected byte[] TmphBuffer;

            /// <summary>
            ///     表单数据内容长度
            /// </summary>
            protected int contentLength;

            /// <summary>
            ///     HTTP请求表单加载
            /// </summary>
            protected TmphRequestForm.TmphILoadForm loadForm;

            /// <summary>
            ///     缓冲区内存池
            /// </summary>
            protected TmphMemoryPool memoryPool;

            /// <summary>
            ///     接收数据结束位置
            /// </summary>
            protected int receiveEndIndex;

            /// <summary>
            ///     接收数据起始时间
            /// </summary>
            protected DateTime receiveStartTime;

            /// <summary>
            ///     HTTP套接字
            /// </summary>
            protected TSocketType socket;

            /// <summary>
            ///     HTTP表单接收器
            /// </summary>
            /// <param name="socket">HTTP套接字</param>
            public TmphFormIdentityReceiver(TSocketType socket)
            {
                this.socket = socket;
                form = socket.form;
            }

            /// <summary>
            ///     表单接收错误
            /// </summary>
            protected void receiveError()
            {
                try
                {
                    loadForm.OnGetForm(null);
                }
                finally
                {
                    if (memoryPool != null) memoryPool.Push(ref TmphBuffer);
                    socket.headerError();
                }
            }

            /// <summary>
            ///     解析表单数据
            /// </summary>
            protected void parse()
            {
                var header = socket.RequestHeader;
                if (header.PostType == TmphRequestHeader.TmphPostType.Json
                    ? form.Parse(TmphBuffer, 0, receiveEndIndex, header.JsonEncoding)
                    : form.Parse(TmphBuffer, receiveEndIndex))
                {
                    var identity = form.Identity = socket.identity;
                    try
                    {
                        loadForm.OnGetForm(form);
                        return;
                    }
                    catch (Exception error)
                    {
                        TmphLog.Error.Add(error, null, false);
                    }
                    if (memoryPool != null) memoryPool.Push(ref TmphBuffer);
                    socket.ResponseError(identity, TmphResponse.TmphState.ServerError500);
                }
                else receiveError();
            }
        }

        /// <summary>
        ///     数据接收器
        /// </summary>
        /// <typeparam name="TSocketType">套接字类型</typeparam>
        protected abstract class TmphBoundaryIdentityReceiver<TSocketType> where TSocketType : TmphSocketBase
        {
            /// <summary>
            ///     缓存文件名称前缀
            /// </summary>
            protected static readonly string cacheFileName = Config.TmphPub.Default.CachePath +
                                                             ((ulong)TmphDate.NowSecond.Ticks).toHex16();

            /// <summary>
            ///     HTTP请求表单
            /// </summary>
            private readonly TmphRequestForm form;

            /// <summary>
            ///     接收换行数据
            /// </summary>
            private readonly Action onReceiveEnter;

            /// <summary>
            ///     接受第一个分隔符处理
            /// </summary>
            private readonly Action onReceiveFirstBoundary;

            /// <summary>
            ///     接收表单值
            /// </summary>
            private readonly Action onReceiveValue;

            /// <summary>
            ///     文件流写入回调事件
            /// </summary>
            private readonly TmphPushPool<byte[]> onWriteFile;

            /// <summary>
            ///     数据分割符
            /// </summary>
            protected TmphSubArray<byte> boundary;

            /// <summary>
            ///     表单接收缓冲区
            /// </summary>
            internal byte[] TmphBuffer;

            /// <summary>
            ///     表单数据内容长度
            /// </summary>
            protected int contentLength;

            /// <summary>
            ///     当前数据位置
            /// </summary>
            private int currentIndex;

            /// <summary>
            ///     当前表单值文件流
            /// </summary>
            private TmphFileStreamWriter fileStream;

            /// <summary>
            ///     当前处理表单值
            /// </summary>
            private TmphRequestForm.TmphValue formValue;

            /// <summary>
            ///     HTTP请求表单加载
            /// </summary>
            protected TmphRequestForm.TmphILoadForm loadForm;

            /// <summary>
            ///     接受数据处理
            /// </summary>
            protected Action onReceiveData;

            /// <summary>
            ///     接收数据结束位置
            /// </summary>
            protected int receiveEndIndex;

            /// <summary>
            ///     当前接收数据字节长度
            /// </summary>
            protected int receiveLength;

            /// <summary>
            ///     接收数据起始时间
            /// </summary>
            protected DateTime receiveStartTime;

            /// <summary>
            ///     HTTP套接字
            /// </summary>
            protected TSocketType socket;

            /// <summary>
            ///     数据起始位置
            /// </summary>
            private int startIndex;

            /// <summary>
            ///     表单值当前起始位置换行符标识
            /// </summary>
            private int valueEnterIndex;

            /// <summary>
            ///     HTTP数据接收器
            /// </summary>
            /// <param name="socket">HTTP套接字</param>
            public TmphBoundaryIdentityReceiver(TSocketType socket)
            {
                this.socket = socket;
                form = socket.form;
                onReceiveFirstBoundary = onFirstBoundary;
                onReceiveEnter = onEnter;
                onReceiveValue = onValue;
                onWriteFile = onFile;
            }

            /// <summary>
            ///     开始接收表单数据
            /// </summary>
            protected abstract void receive();

            /// <summary>
            ///     数据接收错误
            /// </summary>
            protected void error()
            {
                try
                {
                    form.Clear();
                    loadForm.OnGetForm(null);
                }
                finally
                {
                    bigBuffers.Push(ref TmphBuffer);
                    TmphPub.Dispose(ref fileStream);
                    socket.headerError();
                }
            }

            /// <summary>
            ///     表单数据接收完成
            /// </summary>
            private void boundaryReceiverFinally()
            {
                if (receiveLength == contentLength)
                {
                    var identity = form.Identity = socket.identity;
                    try
                    {
                        TmphPub.Dispose(ref fileStream);
                        form.SetFileValue();
                        loadForm.OnGetForm(form);
                        return;
                    }
                    catch (Exception error)
                    {
                        TmphLog.Error.Add(error, null, false);
                    }
                    bigBuffers.Push(ref TmphBuffer);
                    socket.ResponseError(identity, TmphResponse.TmphState.ServerError500);
                }
                else this.error();
            }

            /// <summary>
            ///     接收第一个分割符
            /// </summary>
            protected void onFirstBoundary()
            {
                if (receiveEndIndex >= boundary.Count + 4) checkFirstBoundary();
                else
                {
                    onReceiveData = onReceiveFirstBoundary;
                    receive();
                }
            }

            /// <summary>
            ///     检测第一个分隔符
            /// </summary>
            private unsafe void checkFirstBoundary()
            {
                var boundaryLength4 = boundary.Count + 4;
                fixed (byte* bufferFixed = TmphBuffer, boundaryFixed = boundary.Array)
                {
                    if (*(short*)bufferFixed == '-' + ('-' << 8) &&
                        Unsafe.TmphMemory.Equal(boundaryFixed + boundary.StartIndex, bufferFixed + 2, boundary.Count))
                    {
                        int endValue = *(short*)(bufferFixed + 2 + boundary.Count);
                        if (endValue == 0x0a0d)
                        {
                            startIndex = currentIndex = boundaryLength4;
                            onEnter();
                            return;
                        }
                        if (((endValue ^ ('-' + ('-' << 8))) | (receiveEndIndex ^ boundaryLength4)) == 0)
                        {
                            boundaryReceiverFinally();
                            return;
                        }
                    }
                }
                error();
            }

            /// <summary>
            ///     查找换行处理
            /// </summary>
            private void onEnter()
            {
                var length = receiveEndIndex - currentIndex;
                if (length > sizeof(int)) checkEnter();
                else receiveEnter();
            }

            /// <summary>
            ///     继续接收换行
            /// </summary>
            private unsafe void receiveEnter()
            {
                var length = receiveEndIndex - startIndex;
                if (length >= TmphHttp.Default.HeaderBufferLength) error();
                else
                {
                    if (receiveEndIndex == bigBuffers.Size - sizeof(int))
                    {
                        fixed (byte* bufferFixed = TmphBuffer)
                        {
                            Unsafe.TmphMemory.Copy(bufferFixed + startIndex, bufferFixed, length);
                        }
                        currentIndex -= startIndex;
                        receiveEndIndex = length;
                        startIndex = 0;
                    }
                    onReceiveData = onReceiveEnter;
                    receive();
                }
            }

            /// <summary>
            ///     查找换行
            /// </summary>
            private unsafe void checkEnter()
            {
                var searchEndIndex = receiveEndIndex - sizeof(int);
                fixed (byte* dataFixed = TmphBuffer)
                {
                    byte* start = dataFixed + currentIndex,
                        searchEnd = dataFixed + searchEndIndex,
                        end = dataFixed + receiveEndIndex;
                    *end = 13;
                    do
                    {
                        while (*start != 13) ++start;
                        if (start <= searchEnd)
                        {
                            if (*(int*)start == 0x0a0d0a0d)
                            {
                                currentIndex = (int)(start - dataFixed);
                                parseName();
                                return;
                            }
                            ++start;
                        }
                        else
                        {
                            currentIndex = (int)(start - dataFixed);
                            break;
                        }
                    } while (true);
                }
                receiveEnter();
            }

            /// <summary>
            ///     解析表单名称
            /// </summary>
            private unsafe void parseName()
            {
                formValue.Null();
                try
                {
                    fixed (byte* dataFixed = TmphBuffer)
                    {
                        byte* start = dataFixed + startIndex, end = dataFixed + currentIndex;
                        *end = (byte)';';
                        do
                        {
                            while (*start == ' ') ++start;
                            if (start == end) break;
                            if (*(int*)start == ('n' | ('a' << 8) | ('m' << 16) | ('e' << 24)))
                            {
                                formValue.Name = getFormNameValue(dataFixed, start += sizeof(int), end);
                                start += formValue.Name.Count + 3;
                            }
                            else if (((*(int*)start ^ ('f' | ('i' << 8) | ('l' << 16) | ('e' << 24)))
                                      |
                                      (*(int*)(start + sizeof(int)) ^ ('n' | ('a' << 8) | ('m' << 16) | ('e' << 24)))) ==
                                     0)
                            {
                                formValue.FileName = getFormNameValue(dataFixed, start += sizeof(int) * 2, end);
                                start += formValue.FileName.Count + 3;
                            }
                            for (*end = (byte)';'; *start != ';'; ++start) ;
                        } while (start++ != end);
                        *end = 13;
                    }
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
                if (formValue.Name.Array == null) this.error();
                else
                {
                    startIndex = valueEnterIndex = (currentIndex += 4);
                    onValue();
                }
            }

            /// <summary>
            ///     获取表单名称值
            /// </summary>
            /// <param name="dataFixed">数据</param>
            /// <param name="start">数据起始位置</param>
            /// <param name="end">数据结束位置</param>
            /// <returns>表单名称值,失败返回null</returns>
            private unsafe TmphSubArray<byte> getFormNameValue(byte* dataFixed, byte* start, byte* end)
            {
                while (*start == ' ') ++start;
                if (*start == '=')
                {
                    while (*++start == ' ') ;
                    if (*start == '"')
                    {
                        var valueStart = ++start;
                        for (*end = (byte)'"'; *start != '"'; ++start) ;
                        if (start != end)
                        {
                            var value = new byte[start - valueStart];
                            System.Buffer.BlockCopy(TmphBuffer, (int)(valueStart - dataFixed), value, 0, value.Length);
                            return TmphSubArray<byte>.Unsafe(value, 0, value.Length);
                        }
                    }
                }
                return default(TmphSubArray<byte>);
            }

            /// <summary>
            ///     接收表单值处理
            /// </summary>
            private void onValue()
            {
                if (valueEnterIndex >= 0
                    ? receiveEndIndex - valueEnterIndex >= (boundary.Count + 4)
                    : (receiveEndIndex - currentIndex >= (boundary.Count + 6)))
                    checkValue();
                else receiveValue();
            }

            /// <summary>
            ///     继续接收数据
            /// </summary>
            private unsafe void receiveValue()
            {
                try
                {
                    if (receiveEndIndex == bigBuffers.Size - sizeof(int))
                    {
                        if (startIndex == 0)
                        {
                            if (fileStream == null)
                            {
                                formValue.SaveFileName = loadForm.GetSaveFileName(formValue);
                                if (formValue.SaveFileName == null)
                                    formValue.SaveFileName = cacheFileName + ((ulong)TmphPub.Identity).toHex16();
                                fileStream = new TmphFileStreamWriter(formValue.SaveFileName, FileMode.CreateNew,
                                    FileShare.None, FileOptions.None, false, null);
                            }
                            fileStream.UnsafeWrite(new TmphMemoryPool.TmphPushSubArray
                            {
                                Value =
                                    TmphSubArray<byte>.Unsafe(TmphBuffer, 0,
                                        valueEnterIndex > 0 ? valueEnterIndex : receiveEndIndex),
                                PushPool = onWriteFile
                            });
                            fileStream.WaitWriteBuffer();
                            return;
                        }
                        var length = receiveEndIndex - startIndex;
                        fixed (byte* bufferFixed = TmphBuffer)
                        {
                            Unsafe.TmphMemory.Copy(bufferFixed + startIndex, bufferFixed, length);
                        }
                        currentIndex -= startIndex;
                        valueEnterIndex -= startIndex;
                        receiveEndIndex = length;
                        startIndex = 0;
                    }
                    onReceiveData = onReceiveValue;
                    receive();
                    return;
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
                this.error();
            }

            /// <summary>
            ///     接收表单值处理
            /// </summary>
            private unsafe void checkValue()
            {
                var boundaryLength2 = boundary.Count + 2;
                fixed (byte* bufferFixed = TmphBuffer, boundaryFixed = boundary.Array)
                {
                    var boundaryStart = boundaryFixed + boundary.StartIndex;
                    byte* start = bufferFixed + currentIndex,
                        end = bufferFixed + receiveEndIndex,
                        last = bufferFixed + valueEnterIndex;
                    *end-- = 13;
                    do
                    {
                        while (*start != 13) ++start;
                        if (start >= end) break;
                        if ((int)(start - last) == boundaryLength2 && *(short*)last == ('-') + ('-' << 8)
                            && Unsafe.TmphMemory.Equal(boundaryStart, last + 2, boundary.Count) && *(start + 1) == 10)
                        {
                            currentIndex = (int)(last - bufferFixed) - 2;
                            if (getValue())
                            {
                                startIndex = currentIndex = (int)(start - bufferFixed) + 2;
                                onEnter();
                            }
                            else error();
                            return;
                        }
                        last = *++start == 10 ? ++start : (bufferFixed - TmphBuffer.Length);
                    } while (true);
                    var hash = (*(int*)(end -= 3) ^ ('-') + ('-' << 8) + 0x0a0d0000);
                    if ((hash | (*(int*)(end -= boundary.Count + sizeof(int)) ^ 0x0a0d + ('-' << 16) + ('-' << 24))) ==
                        0
                        && Unsafe.TmphMemory.Equal(boundaryStart, end + sizeof(int), boundary.Count))
                    {
                        currentIndex = (int)(end - bufferFixed);
                        if (getValue()) boundaryReceiverFinally();
                        else error();
                        return;
                    }
                    valueEnterIndex = (int)(last - bufferFixed);
                    currentIndex = (int)(start - bufferFixed);
                }
                receiveValue();
            }

            /// <summary>
            ///     文件流写入回调事件
            /// </summary>
            /// <param name="TmphBuffer">表单接收缓冲区</param>
            private unsafe void onFile(ref byte[] TmphBuffer)
            {
                if (fileStream.LastException == null)
                {
                    if (valueEnterIndex > 0)
                    {
                        fixed (byte* bufferFixed = TmphBuffer)
                        {
                            Unsafe.TmphMemory.Copy(bufferFixed + valueEnterIndex, bufferFixed,
                                receiveEndIndex -= valueEnterIndex);
                        }
                        valueEnterIndex = 0;
                    }
                    else
                    {
                        receiveEndIndex = 0;
                        valueEnterIndex = -TmphBuffer.Length;
                    }
                    startIndex = 0;
                    currentIndex = receiveEndIndex;
                    onReceiveData = onReceiveValue;
                    receive();
                }
                else error();
            }

            /// <summary>
            ///     获取表单值
            /// </summary>
            /// <returns>是否成功</returns>
            private bool getValue()
            {
                try
                {
                    if (fileStream == null)
                    {
                        var value = new byte[currentIndex - startIndex];
                        System.Buffer.BlockCopy(TmphBuffer, startIndex, value, 0, value.Length);
                        formValue.Value.UnsafeSet(value, 0, value.Length);
                    }
                    else
                    {
                        fileStream.UnsafeWrite(new TmphMemoryPool.TmphPushSubArray
                        {
                            Value = TmphSubArray<byte>.Unsafe(TmphBuffer, startIndex, currentIndex - startIndex)
                        });
                        fileStream.Dispose();
                        fileStream = null;
                    }
                    if (formValue.FileName.Count == 0)
                    {
                        if (formValue.Name.Count == 1 &&
                            formValue.Name.array[formValue.Name.StartIndex] == TmphWeb.Default.QueryJsonName)
                        {
                            form.Parse(formValue.Value.array, formValue.Value.StartIndex, formValue.Value.Count,
                                Encoding.UTF8); //showjim编码问题
                        }
                        else form.FormValues.Add(formValue);
                    }
                    else form.Files.Add(formValue);
                    return true;
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
                return false;
            }
        }

        /// <summary>
        ///     数据发送器
        /// </summary>
        /// <typeparam name="TSocketType">套接字类型</typeparam>
        protected abstract class TmphDataSender<TSocketType> where TSocketType : TmphSocketBase
        {
            /// <summary>
            ///     发送数据缓冲区
            /// </summary>
            protected byte[] TmphBuffer;

            /// <summary>
            ///     待发送文件数据字节数
            /// </summary>
            protected long fileSize;

            /// <summary>
            ///     正在发送的文件流
            /// </summary>
            protected FileStream fileStream;

            /// <summary>
            ///     缓冲区内存池
            /// </summary>
            protected TmphMemoryPool memoryPool;

            /// <summary>
            ///     发送数据回调处理
            /// </summary>
            protected Action<bool> onSend;

            /// <summary>
            ///     发送数据结束位置
            /// </summary>
            protected int sendEndIndex;

            /// <summary>
            ///     发送数据起始位置
            /// </summary>
            protected int sendIndex;

            /// <summary>
            ///     已经发送数据长度
            /// </summary>
            protected long sendLength;

            /// <summary>
            ///     发送数据起始时间
            /// </summary>
            protected DateTime sendStartTime;

            /// <summary>
            ///     HTTP套接字
            /// </summary>
            protected TSocketType socket;

            /// <summary>
            ///     数据发送器
            /// </summary>
            /// <param name="socket">HTTP套接字</param>
            public TmphDataSender(TSocketType socket)
            {
                this.socket = socket;
            }

            /// <summary>
            ///     发送数据完毕
            /// </summary>
            /// <param name="isSend">是否发送成功</param>
            protected void send(bool isSend)
            {
                if (memoryPool == null) TmphBuffer = null;
                else memoryPool.Push(ref TmphBuffer);
                onSend(isSend);
            }

            /// <summary>
            ///     读取文件并发送数据
            /// </summary>
            protected void readFile()
            {
                if (fileSize == 0) sendFile(true);
                else
                {
                    try
                    {
                        sendEndIndex = TmphMemoryPool.StreamBuffers.Size - sizeof(int);
                        if (sendEndIndex > fileSize) sendEndIndex = (int)fileSize;
                        if (fileStream.Read(TmphBuffer, 0, sendEndIndex) == sendEndIndex)
                        {
                            fileSize -= sendEndIndex;
                            sendIndex = 0;
                            sendFile();
                            return;
                        }
                    }
                    catch (Exception error)
                    {
                        TmphLog.Error.Add(error, null, false);
                    }
                    sendFile(false);
                }
            }

            /// <summary>
            ///     开始发送文件数据
            /// </summary>
            protected abstract void sendFile();

            /// <summary>
            ///     文件发送数据完毕
            /// </summary>
            /// <param name="isSend">是否发送成功</param>
            protected void sendFile(bool isSend)
            {
                TmphPub.Dispose(ref fileStream);
                send(isSend);
            }
        }

        /// <summary>
        ///     WebSocket请求接收器
        /// </summary>
        /// <typeparam name="TSocketType">套接字类型</typeparam>
        protected abstract class TmphWebSocketIdentityReceiver<TSocketType> where TSocketType : TmphSocketBase
        {
            /// <summary>
            ///     操作编码
            /// </summary>
            public enum TmphTypeCode : byte
            {
                /// <summary>
                ///     连续消息片断
                /// </summary>
                Continuous = 0,

                /// <summary>
                ///     文本消息片断
                /// </summary>
                Text = 1,

                /// <summary>
                ///     二进制消息片断
                /// </summary>
                Binary = 2,

                /// <summary>
                ///     连接关闭
                /// </summary>
                Close = 8,

                /// <summary>
                ///     心跳检查的ping
                /// </summary>
                Ping = 9,

                /// <summary>
                ///     心跳检查的pong
                /// </summary>
                Pong = 10
            }

            /// <summary>
            ///     关闭连接数据
            /// </summary>
            private static readonly byte[] closeData = { 0x88, 0 };

            /// <summary>
            ///     AJAX回调缓冲区
            /// </summary>
            private readonly byte[] callBackBuffer;

            /// <summary>
            ///     请求信息集合
            /// </summary>
            private readonly TmphList<TmphRequestInfo> requests = new TmphList<TmphRequestInfo>();

            /// <summary>
            ///     套接字访问锁
            /// </summary>
            private readonly object socketLock = new object();

            /// <summary>
            ///     表单接收缓冲区
            /// </summary>
            protected byte[] TmphBuffer;

            /// <summary>
            ///     当前请求编号
            /// </summary>
            private long currentIdentity;

            /// <summary>
            ///     请求编号访问锁
            /// </summary>
            private int identityLock;

            /// <summary>
            ///     接收数据结束位置
            /// </summary>
            protected int receiveEndIndex;

            /// <summary>
            ///     开始接收数据
            /// </summary>
            protected Action receiveHandle;

            /// <summary>
            ///     HTTP套接字
            /// </summary>
            protected TSocketType socket;

            /// <summary>
            ///     套接字请求编号
            /// </summary>
            private int socketIdentity = 1;

            /// <summary>
            ///     超时时间
            /// </summary>
            protected DateTime timeout;

            /// <summary>
            ///     WebSocket请求接收器
            /// </summary>
            /// <param name="socket">HTTP套接字</param>
            public TmphWebSocketIdentityReceiver(TSocketType socket)
            {
                this.socket = socket;
                receiveHandle = receive;
                TmphBuffer = socket.Buffer;
                callBackBuffer = new byte[128];
                callBackBuffer[0] = 0x81;
            }

            /// <summary>
            ///     开始接收数据
            /// </summary>
            protected abstract void receive();

            /// <summary>
            ///     关闭连接
            /// </summary>
            protected void close()
            {
                Monitor.Enter(socketLock);
                try
                {
                    var error = SocketError.Success;
                    socket.Socket.send(closeData, 0, 2, ref error);
                }
                catch
                {
                }
                finally
                {
                    Monitor.Exit(socketLock);
                    socket.webSocketEnd();
                }
            }

            /// <summary>
            ///     清除请求
            /// </summary>
            public void Clear()
            {
                Monitor.Enter(socketLock);
                ++socketIdentity;
                Monitor.Exit(socketLock);
            }

            /// <summary>
            ///     获取AJAX回调函数
            /// </summary>
            /// <param name="identity">操作标识</param>
            /// <returns>AJAX回调函数,失败返回null</returns>
            public TmphSubString GetCallBack(long identity)
            {
                TmphInterlocked.NoCheckCompareSetSleep0(ref identityLock);
                var count = requests.Count;
                if (count != 0)
                {
                    var requestArray = requests.array;
                    foreach (var request in requestArray)
                    {
                        if (request.Identity == identity)
                        {
                            identityLock = 0;
                            return request.CallBack;
                        }
                        if (--count == 0) break;
                    }
                }
                identityLock = 0;
                return default(TmphSubString);
            }

            /// <summary>
            ///     尝试解析数据
            /// </summary>
            protected unsafe void tryParse()
            {
                fixed (byte* bufferFixed = TmphBuffer)
                {
                    uint value = *(uint*)bufferFixed, code = value & 0xf;
                    if (code == (byte)TmphTypeCode.Close)
                    {
                        close();
                        return;
                    }
                    if ((value & 0xf0) == 0x80 && (code & 7) <= 2 &&
                        ((value & 0x8000) == 0x8000 || (value & 0xff00) == 0))
                    {
                        var dataLength = ((value >> 8) & 0x7f);
                        if (dataLength <= 126)
                        {
                            var dataIndex = (uint)((value & 0x8000) == 0 ? 2 : 6);
                            if (dataLength == 126)
                            {
                                dataLength = value >> 16;
                                dataIndex += 2;
                            }
                            var packetLength = dataLength + dataIndex;
                            if (packetLength <= TmphHttp.Default.HeaderBufferLength)
                            {
                                if (receiveEndIndex >= packetLength)
                                {
                                    timeout =
                                        TmphDate.NowSecond.AddTicks(WebSocketReceiveTimeoutQueue.CallbackTimeoutTicks);
                                    if (code == (byte)TmphTypeCode.Text && dataLength != 0)
                                    {
                                        byte* start = bufferFixed + dataIndex,
                                            end = start + ((dataLength + 3) & (uint.MaxValue - 3)),
                                            data = start;
                                        code = *(uint*)(start - sizeof(uint));
                                        do
                                        {
                                            *(uint*)data ^= code;
                                        } while ((data += sizeof(uint)) != end);
                                        for (data = start, *(end = start + dataLength) = (byte)'\n';
                                            *data != '\n';
                                            ++data)
                                            ;
                                        var requestHeader = socket.RequestHeader;
                                        if (requestHeader.SetWebSocketUrl(start, data))
                                        {
                                            var callBack = requestHeader.AjaxCallBackName;
                                            var request = new TmphRequestInfo { SocketIdentity = socketIdentity };
                                            if (data != end) ++data;
                                            dataLength = (uint)(int)(end - data);
                                            if (callBack.Count == 0)
                                            {
                                                if (dataLength != 0)
                                                {
                                                    try
                                                    {
                                                        requestHeader.WebSocketData = Encoding.UTF8.GetString(TmphBuffer,
                                                            (int)(data - bufferFixed), (int)dataLength);
                                                    }
                                                    catch (Exception error)
                                                    {
                                                        request.Identity = long.MinValue;
                                                        TmphLog.Error.Add(error, null, false);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                try
                                                {
                                                    var webSocketData = Encoding.UTF8.GetString(TmphBuffer,
                                                        (int)(start - bufferFixed), (int)(end - start));
                                                    requestHeader.WebSocketData.UnsafeSet(webSocketData,
                                                        (int)(data - start), (int)dataLength);
                                                    request.CallBack.UnsafeSet(webSocketData,
                                                        callBack.StartIndex - requestHeader.EndIndex, callBack.Count);
                                                }
                                                catch (Exception error)
                                                {
                                                    request.Identity = long.MinValue;
                                                    TmphLog.Error.Add(error, null, false);
                                                }
                                            }
                                            if (request.Identity == 0) this.request(request);
                                            else response(callBack);
                                        }
                                    }
                                    if (receiveEndIndex > packetLength)
                                    {
                                        Unsafe.TmphMemory.Copy(bufferFixed + packetLength, bufferFixed,
                                            receiveEndIndex -= (int)packetLength);
                                    }
                                }
                                receive();
                                return;
                            }
                        }
                    }
                }
                socket.webSocketEnd();
            }

            /// <summary>
            ///     HTTP请求处理
            /// </summary>
            /// <param name="request">请求信息</param>
            private void request(TmphRequestInfo request)
            {
                TmphInterlocked.NoCheckCompareSetSleep0(ref identityLock);
                request.Identity = ++currentIdentity;
                try
                {
                    requests.Add(request);
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                    request.Identity = 0;
                }
                finally
                {
                    identityLock = 0;
                }
                if (request.Identity != 0) socket.DomainServer.Server.Request(socket, request.Identity);
            }

            /// <summary>
            ///     错误输出
            /// </summary>
            /// <param name="callBack">AJAX回调函数</param>
            private unsafe void response(TmphSubArray<byte> callBack)
            {
                if (callBack.Count != 0 && callBack.Count <= 123)
                {
                    fixed (byte* bufferFixed = callBack.Array)
                    {
                        var start = bufferFixed + callBack.StartIndex;
                        *(short*)(start + callBack.Count) = '(' + (')' << 8);
                        *(short*)(start - sizeof(short)) = (short)(((callBack.Count + 2) << 8) + 0x81);
                        Monitor.Enter(socketLock);
                        try
                        {
                            var error = SocketError.Success;
                            socket.Socket.send(callBack.Array, callBack.StartIndex - 2, callBack.Count + 4, ref error);
                        }
                        catch (Exception error)
                        {
                            TmphLog.Error.Add(error, null, false);
                        }
                        finally
                        {
                            Monitor.Exit(socketLock);
                        }
                    }
                }
            }

            /// <summary>
            ///     获取请求信息
            /// </summary>
            /// <param name="identity">HTTP操作标识</param>
            /// <returns>请求信息</returns>
            private TmphRequestInfo getRequest(long identity)
            {
                var index = 0;
                TmphInterlocked.NoCheckCompareSetSleep0(ref identityLock);
                var count = requests.Count;
                if (count != 0)
                {
                    var requestArray = requests.array;
                    foreach (var request in requestArray)
                    {
                        if (request.Identity == identity)
                        {
                            requests.Unsafer.AddLength(-1);
                            requestArray[index] = requestArray[requests.Count];
                            requestArray[requests.Count].CallBack.Null();
                            identityLock = 0;
                            return request;
                        }
                        if (--count == 0) break;
                        ++index;
                    }
                }
                identityLock = 0;
                return default(TmphRequestInfo);
            }

            /// <summary>
            ///     输出HTTP响应数据
            /// </summary>
            /// <param name="identity">HTTP操作标识</param>
            /// <param name="response">HTTP响应数据</param>
            public unsafe bool Response(long identity, ref TmphResponse response)
            {
                fixed (byte* bufferFixed = callBackBuffer)
                {
                    Monitor.Enter(socketLock);
                    try
                    {
                        var request = getRequest(identity);
                        if (request.SocketIdentity != 0)
                        {
                            if (request.SocketIdentity == socketIdentity)
                            {
                                timeout = TmphDate.NowSecond.AddTicks(WebSocketReceiveTimeoutQueue.CallbackTimeoutTicks);
                                if (response.State == TmphResponse.TmphState.Ok200)
                                {
                                    var body = response.Body;
                                    var length = body.Count;
                                    if (length == 0) this.response(request.CallBack);
                                    else
                                    {
                                        var error = SocketError.Success;
                                        if (length > 125)
                                        {
                                            int bufferLength;
                                            if (length <= ushort.MaxValue)
                                            {
                                                *(bufferFixed + 1) = 126;
                                                *(ushort*)(bufferFixed + 2) = (ushort)length;
                                                bufferLength = 4;
                                            }
                                            else
                                            {
                                                *(bufferFixed + 1) = 127;
                                                *(long*)(bufferFixed + 2) = length;
                                                bufferLength = 10;
                                            }
                                            if (socket.Socket.send(callBackBuffer, 0, bufferLength, ref error))
                                            {
                                                if (
                                                    !socket.Socket.serverSend(body.Array, body.StartIndex, length,
                                                        ref error))
                                                    socket.Socket.Close();
                                            }
                                        }
                                        else
                                        {
                                            *(bufferFixed + 1) = (byte)length;
                                            fixed (byte* bodyFixed = body.Array)
                                            {
                                                Unsafe.TmphMemory.Copy(bodyFixed + body.StartIndex, bufferFixed + 2,
                                                    length);
                                            }
                                            socket.Socket.send(callBackBuffer, 0, length + 2, ref error);
                                        }
                                    }
                                }
                                else if (response.State == TmphResponse.TmphState.NotChanged304)
                                    responseNotChanged(request.CallBack);
                                else this.response(request.CallBack);
                            }
                            return true;
                        }
                    }
                    catch (Exception error)
                    {
                        TmphLog.Error.Add(error, null, false);
                    }
                    finally
                    {
                        Monitor.Exit(socketLock);
                        TmphResponse.Push(ref response);
                    }
                }
                return false;
            }

            /// <summary>
            ///     输出错误状态
            /// </summary>
            /// <param name="identity">操作标识</param>
            /// <param name="state">错误状态</param>
            public bool ResponseError(long identity, TmphResponse.TmphState state)
            {
                Monitor.Enter(socketLock);
                try
                {
                    var request = getRequest(identity);
                    if (request.SocketIdentity != 0)
                    {
                        if (request.SocketIdentity == socketIdentity)
                        {
                            timeout = TmphDate.NowSecond.AddTicks(WebSocketReceiveTimeoutQueue.CallbackTimeoutTicks);
                            response(request.CallBack);
                        }
                        return true;
                    }
                }
                finally
                {
                    Monitor.Exit(socketLock);
                }
                return false;
            }

            /// <summary>
            ///     错误输出
            /// </summary>
            /// <param name="callBack">AJAX回调函数</param>
            private unsafe void response(TmphSubString callBack)
            {
                if (callBack.Length != 0 && callBack.Length <= 125 - 4)
                {
                    fixed (byte* bufferFixed = callBackBuffer)
                    fixed (char* callBackFixed = callBack.value)
                    {
                        var start = bufferFixed + 2;
                        Monitor.Enter(socketLock);
                        try
                        {
                            *(bufferFixed + 1) = (byte)(callBack.Length + 2);
                            Unsafe.TmphString.WriteBytes(callBackFixed + callBack.StartIndex, callBack.Length, start);
                            *(short*)(start + callBack.Length) = '(' + (')' << 8);
                            var error = SocketError.Success;
                            socket.Socket.send(callBackBuffer, 0, callBack.Length + 4, ref error);
                        }
                        catch (Exception error)
                        {
                            TmphLog.Error.Add(error, null, false);
                        }
                        finally
                        {
                            Monitor.Exit(socketLock);
                        }
                    }
                }
            }

            /// <summary>
            ///     输出空对象
            /// </summary>
            /// <param name="callBack">AJAX回调函数</param>
            private unsafe void responseNotChanged(TmphSubString callBack)
            {
                if (callBack.Length != 0 && callBack.Length + TmphAsynchronousMethod.ReturnParameterName.Length <= 125 - 9)
                {
                    fixed (byte* bufferFixed = callBackBuffer)
                    fixed (char* callBackFixed = callBack.value, returnFixed = TmphAsynchronousMethod.ReturnParameterName)
                    {
                        var start = bufferFixed + 2;
                        Monitor.Enter(socketLock);
                        try
                        {
                            *(bufferFixed + 1) = (byte)(callBack.Length + 7);
                            Unsafe.TmphString.WriteBytes(callBackFixed + callBack.StartIndex, callBack.Length, start);
                            *(short*)(start += callBack.Length) = '(' + ('{' << 8);
                            Unsafe.TmphString.WriteBytes(returnFixed, TmphAsynchronousMethod.ReturnParameterName.Length,
                                start += sizeof(short));
                            *(start += TmphAsynchronousMethod.ReturnParameterName.Length) = (byte)':';
                            *(int*)(start + 1) = '{' + ('}' << 8) + ('}' << 16) + (')' << 24);
                            var error = SocketError.Success;
                            socket.Socket.send(callBackBuffer, 0,
                                callBack.Length + TmphAsynchronousMethod.ReturnParameterName.Length + 9, ref error);
                        }
                        catch (Exception error)
                        {
                            TmphLog.Error.Add(error, null, false);
                        }
                        finally
                        {
                            Monitor.Exit(socketLock);
                        }
                    }
                }
            }

            /// <summary>
            ///     请求信息
            /// </summary>
            private struct TmphRequestInfo
            {
                /// <summary>
                ///     AJAX回调函数
                /// </summary>
                public TmphSubString CallBack;

                /// <summary>
                ///     请求编号
                /// </summary>
                public long Identity;

                /// <summary>
                ///     套接字请求编号
                /// </summary>
                public int SocketIdentity;
            }
        }
    }

    /// <summary>
    ///     HTTP套接字
    /// </summary>
    internal sealed class TmphSocket : TmphSocketBase
    {
        /// <summary>
        ///     数据发送器
        /// </summary>
        private readonly TmphDataSender sender;

        /// <summary>
        ///     数据接收器
        /// </summary>
        private TmphBoundaryIdentityReceiver boundaryReceiver;

        /// <summary>
        ///     表单数据接收器
        /// </summary>
        private TmphFormIdentityReceiver formReceiver;

        /// <summary>
        ///     HTTP头部接收器
        /// </summary>
        internal TmphHeaderReceiver HeaderReceiver;

        /// <summary>
        ///     是否已经释放资源
        /// </summary>
        private int isDisposed;

        /// <summary>
        ///     WebSocket请求接收器
        /// </summary>
        private TmphWebSocketIdentityReceiver webSocketReceiver;

        /// <summary>
        ///     HTTP套接字
        /// </summary>
        private TmphSocket()
        {
            HeaderReceiver = new TmphHeaderReceiver(this);
            sender = new TmphDataSender(this);
        }

        /// <summary>
        ///     获取HTTP请求头部
        /// </summary>
        internal override TmphRequestHeader RequestHeader
        {
            get { return HeaderReceiver.RequestHeader; }
        }

        /// <summary>
        ///     开始处理新的请求
        /// </summary>
        /// <param name="servers">HTTP服务器</param>
        /// <param name="socket">套接字</param>
        private void start(TmphServers servers, Socket socket)
        {
            this.servers = servers;
            Socket = socket;
            isLoadForm = isNextRequest = 0;
            DomainServer = null;
            form.Clear();
            TmphResponse.Push(ref response);
            HeaderReceiver.RequestHeader.IsKeepAlive = false;
            HeaderReceiver.Receive();
        }

        /// <summary>
        ///     开始处理新的请求
        /// </summary>
        /// <param name="servers">HTTP服务器</param>
        /// <param name="socket">TCP调用套接字</param>
        private void start(TmphServers servers, TmphCommandSocket socket)
        {
            this.servers = servers;
            TcpCommandSocket = socket;
            Socket = socket.Socket;
            isLoadForm = isNextRequest = 0;
            DomainServer = null;
            form.Clear();
            TmphResponse.Push(ref response);
            HeaderReceiver.RequestHeader.IsKeepAlive = false;
            HeaderReceiver.Receive(socket.receiveData);
        }

        /// <summary>
        ///     释放资源
        /// </summary>
        public override void Dispose()
        {
            if (Interlocked.Increment(ref isDisposed) == 1) Interlocked.Decrement(ref newCount);
            form.Clear();
            TmphResponse.Push(ref response);
            base.Dispose();
        }

        /// <summary>
        ///     HTTP头部接收错误
        /// </summary>
        protected override void headerError()
        {
            if (TcpCommandSocket == null)
            {
                if (ipv6.IsNull) TmphServer.SocketEnd(ipv4);
                else TmphServer.SocketEnd(ipv6);
                close(Socket);
                Socket = null;
            }
            else
            {
                TcpCommandSocket.PushPool();
                TcpCommandSocket = null;
            }
            form.Clear();
            TmphTypePool<TmphSocket>.Push(this);
        }

        /// <summary>
        ///     HTTP代理结束
        /// </summary>
        internal void ProxyEnd()
        {
            Socket = null;
            if (ipv6.IsNull) TmphServer.SocketEnd(ipv4);
            else TmphServer.SocketEnd(ipv6);
            TmphTypePool<TmphSocket>.Push(this);
        }

        /// <summary>
        ///     WebSocket结束
        /// </summary>
        protected override void webSocketEnd()
        {
            webSocketReceiver.Clear();
            if (ipv6.IsNull) TmphServer.SocketEnd(ipv4);
            else TmphServer.SocketEnd(ipv6);
            close(Socket);
            Socket = null;
            TmphTypePool<TmphSocket>.Push(this);
        }

        /// <summary>
        ///     未能识别的HTTP头部
        /// </summary>
        protected override void headerUnknown()
        {
            if (isNextRequest == 0)
            {
                try
                {
                    var TmphClient = servers.GetForwardClient();
                    if (TmphClient != null)
                    {
                        new TmphForwardProxy(this, TmphClient).Start();
                        return;
                    }
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
            }
            responseError(TmphResponse.TmphState.BadRequest400);
        }

        /// <summary>
        ///     开始接收头部数据
        /// </summary>
        protected override void receiveHeader()
        {
            HeaderReceiver.Receive();
        }

        /// <summary>
        ///     获取AJAX回调函数
        /// </summary>
        /// <param name="identity">操作标识</param>
        /// <returns>AJAX回调函数,失败返回null</returns>
        internal override TmphSubString GetWebSocketCallBack(long identity)
        {
            return webSocketReceiver.GetCallBack(identity);
        }

        /// <summary>
        ///     输出错误状态
        /// </summary>
        /// <param name="identity">操作标识</param>
        /// <param name="state">错误状态</param>
        internal override bool WebSocketResponseError(long identity, TmphResponse.TmphState state)
        {
            return webSocketReceiver.ResponseError(identity, state);
        }

        /// <summary>
        ///     开始接收WebSocket数据
        /// </summary>
        protected override void receiveWebSocket()
        {
            if (webSocketReceiver == null) webSocketReceiver = new TmphWebSocketIdentityReceiver(this);
            webSocketReceiver.Receive();
        }

        /// <summary>
        ///     获取请求表单数据
        /// </summary>
        /// <param name="identity">HTTP操作标识</param>
        /// <param name="loadForm">HTTP请求表单加载接口</param>
        internal override void GetForm(long identity, TmphRequestForm.TmphILoadForm loadForm)
        {
            if (Interlocked.CompareExchange(ref this.identity, identity + 1, identity) == identity)
            {
                if (isLoadForm == 0)
                {
                    isLoadForm = 1;
                    if (check100Continue())
                    {
                        var type = HeaderReceiver.RequestHeader.PostType;
                        if (type == TmphRequestHeader.TmphPostType.Json || type == TmphRequestHeader.TmphPostType.Form)
                        {
                            if (formReceiver == null) formReceiver = new TmphFormIdentityReceiver(this);
                            formReceiver.Receive(loadForm);
                        }
                        else
                        {
                            if (boundaryReceiver == null) boundaryReceiver = new TmphBoundaryIdentityReceiver(this);
                            boundaryReceiver.Receive(loadForm);
                        }
                        return;
                    }
                }
                else TmphLog.Error.Add("表单已加载", true, true);
            }
            loadForm.OnGetForm(null);
        }

        /// <summary>
        ///     HTTP响应头部输出
        /// </summary>
        /// <param name="TmphBuffer">输出数据</param>
        /// <param name="memoryPool">内存池</param>
        protected override void responseHeader(TmphSubArray<byte> TmphBuffer, TmphMemoryPool memoryPool)
        {
            if (responseSize == 0)
            {
                TmphResponse.Push(ref response);
                sender.Send(sendNext, TmphBuffer, memoryPool);
            }
            else sender.Send(sendResponseBody, TmphBuffer, memoryPool);
        }

        /// <summary>
        ///     输出HTTP响应数据
        /// </summary>
        /// <param name="identity">HTTP操作标识</param>
        /// <param name="response">HTTP响应数据</param>
        public override unsafe bool Response(long identity, ref TmphResponse response)
        {
            if (identity >= 0)
            {
                if (Interlocked.CompareExchange(ref this.identity, identity + 1, identity) == identity)
                {
                    this.response = response;
                    response = null;
                    if (this.response.LastModified != null)
                    {
                        var ifModifiedSince = HeaderReceiver.RequestHeader.IfModifiedSince;
                        if (ifModifiedSince.Count == this.response.LastModified.Length)
                        {
                            fixed (byte* TmphBuffer = ifModifiedSince.Array)
                            {
                                if (Unsafe.TmphMemory.Equal(this.response.LastModified,
                                    TmphBuffer + ifModifiedSince.StartIndex, ifModifiedSince.Count))
                                {
                                    TmphResponse.Push(ref this.response);
                                    this.response = TmphResponse.NotChanged304;
                                }
                            }
                        }
                    }
                    if (boundaryReceiver != null) bigBuffers.Push(ref boundaryReceiver.TmphBuffer);
                    if (HeaderReceiver.RequestHeader.Method == Web.TmphHttp.TmphMethodType.POST && isLoadForm == 0)
                    {
                        if (HeaderReceiver.RequestHeader.PostType == TmphRequestHeader.TmphPostType.Json ||
                            HeaderReceiver.RequestHeader.PostType == TmphRequestHeader.TmphPostType.Form)
                        {
                            if (formReceiver == null) formReceiver = new TmphFormIdentityReceiver(this);
                            formReceiver.Receive(this);
                        }
                        else
                        {
                            if (boundaryReceiver == null) boundaryReceiver = new TmphBoundaryIdentityReceiver(this);
                            boundaryReceiver.Receive(this);
                        }
                    }
                    else responseHeader();
                    return true;
                }
                TmphResponse.Push(ref response);
                return false;
            }
            return webSocketReceiver.Response(identity, ref response);
        }

        /// <summary>
        ///     发送HTTP响应内容
        /// </summary>
        /// <param name="isSend">是否发送成功</param>
        protected override void responseBody(bool isSend)
        {
            if (isSend)
            {
                if (response.BodyFile == null)
                {
                    var body = response.Body;
                    if (response.State == TmphResponse.TmphState.PartialContent206)
                    {
                        body.UnsafeSet(body.StartIndex + (int)HeaderReceiver.RequestHeader.RangeStart,
                            (int)responseSize);
                    }
                    sender.Send(sendNext, body, null);
                }
                else
                    sender.SendFile(sendNext, response.BodyFile,
                        response.State == TmphResponse.TmphState.PartialContent206
                            ? HeaderReceiver.RequestHeader.RangeStart
                            : 0, responseSize);
            }
            else headerError();
        }

        /// <summary>
        ///     输出错误状态
        /// </summary>
        /// <param name="state">错误状态</param>
        protected override void responseError(TmphResponse.TmphState state)
        {
            if (boundaryReceiver != null) bigBuffers.Push(ref boundaryReceiver.TmphBuffer);
            if (DomainServer != null)
            {
                response = DomainServer.Server.GetErrorResponseData(state, HeaderReceiver.RequestHeader.IsGZip);
                if (response != null)
                {
                    if (state != TmphResponse.TmphState.NotFound404 ||
                        HeaderReceiver.RequestHeader.Method != Web.TmphHttp.TmphMethodType.GET)
                    {
                        HeaderReceiver.RequestHeader.IsKeepAlive = false;
                    }
                    responseHeader();
                    return;
                }
            }
            var data = errorResponseDatas[(int)state];
            if (data != null)
            {
                if (state == TmphResponse.TmphState.NotFound404 &&
                    HeaderReceiver.RequestHeader.Method == Web.TmphHttp.TmphMethodType.GET)
                {
                    sender.Send(sendNext, TmphSubArray<byte>.Unsafe(data, 0, data.Length), null);
                }
                else
                {
                    HeaderReceiver.RequestHeader.IsKeepAlive = false;
                    sender.Send(sendClose, TmphSubArray<byte>.Unsafe(data, 0, data.Length), null);
                }
            }
            else headerError();
        }

        /// <summary>
        ///     开始处理新的请求
        /// </summary>
        /// <param name="servers">HTTP服务器</param>
        /// <param name="socket">套接字</param>
        /// <param name="ip">客户端IP</param>
        internal static void Start(TmphServers servers, Socket socket, TmphIpv6Hash ip)
        {
            try
            {
                var value = getSocket();
                value.ipv6 = ip;
                value.ipv4 = 0;
                value.start(servers, socket);
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, false);
                TmphServer.SocketEnd(ip);
                close(socket);
            }
        }

        /// <summary>
        ///     开始处理新的请求
        /// </summary>
        /// <param name="servers">HTTP服务器</param>
        /// <param name="socket">套接字</param>
        /// <param name="ip">客户端IP</param>
        internal static void Start(TmphServers servers, Socket socket, int ip)
        {
            try
            {
                var value = getSocket();
                value.ipv4 = ip;
                value.ipv6 = default(TmphIpv6Hash);
                value.start(servers, socket);
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, false);
                TmphServer.SocketEnd(ip);
                close(socket);
            }
        }

        /// <summary>
        ///     开始处理新的请求(用于TCP调用)
        /// </summary>
        /// <param name="servers">HTTP服务器</param>
        /// <param name="socket">TCP调用套接字</param>
        internal static void Start(TmphServers servers, TmphCommandSocket socket)
        {
            getSocket().start(servers, socket);
        }

        /// <summary>
        ///     获取套接字
        /// </summary>
        /// <returns></returns>
        private static TmphSocket getSocket()
        {
            var socket = TmphTypePool<TmphSocket>.Pop();
            if (socket != null) return socket;
            Interlocked.Increment(ref newCount);
            return new TmphSocket();
        }

        /// <summary>
        ///     HTTP头部接收器
        /// </summary>
        internal sealed class TmphHeaderReceiver : TmphHeaderReceiver<TmphSocket>, IDisposable
        {
            /// <summary>
            ///     异步套接字操作
            /// </summary>
            public SocketAsyncEventArgs Async;

            /// <summary>
            ///     HTTP头部接收器
            /// </summary>
            /// <param name="socket">HTTP套接字</param>
            public TmphHeaderReceiver(TmphSocket socket)
                : base(socket)
            {
                Async = TmphSocketAsyncEventArgs.Get();
                Async.SocketFlags = SocketFlags.None;
                Async.DisconnectReuseSocket = false;
                Async.Completed += receive;
                Async.UserToken = this;
                Async.SetBuffer(TmphBuffer, 0, TmphBuffer.Length);
            }

            /// <summary>
            ///     释放资源
            /// </summary>
            public void Dispose()
            {
                Async.Completed -= receive;
                TmphSocketAsyncEventArgs.Push(ref Async);
            }

            /// <summary>
            ///     开始接收数据
            /// </summary>
            public void Receive()
            {
                timeout = TmphDate.NowSecond.AddTicks(ReceiveTimeoutQueue.CallbackTimeoutTicks);
                if (socket.isNextRequest == 0)
                {
                    ReceiveEndIndex = HeaderEndIndex = 0;
                    Async.SocketError = SocketError.Success;
                    receive();
                }
                else
                {
                    if ((ReceiveEndIndex -= (HeaderEndIndex + sizeof(int))) > 0)
                    {
                        System.Buffer.BlockCopy(TmphBuffer, HeaderEndIndex + sizeof(int), TmphBuffer, 0, ReceiveEndIndex);
                        HeaderEndIndex = 0;
                        onReceive();
                    }
                    else
                    {
                        ReceiveEndIndex = HeaderEndIndex = 0;
                        receive();
                    }
                }
            }

            /// <summary>
            ///     开始接收数据(用于TCP调用)
            /// </summary>
            /// <param name="data">已接受数据</param>
            public unsafe void Receive(byte[] data)
            {
                timeout = TmphDate.NowSecond.AddTicks(ReceiveTimeoutQueue.CallbackTimeoutTicks);
                HeaderEndIndex = 0;
                fixed (byte* dataFixed = data, bufferFixed = TmphBuffer) *(int*)bufferFixed = *(int*)dataFixed;
                ReceiveEndIndex = sizeof(int);

                Async.SocketError = SocketError.Success;
                receive();
            }

            /// <summary>
            ///     接受头部换行数据
            /// </summary>
            protected override void receive()
            {
                try
                {
                    var timeoutIdentity = socket.timeoutIdentity;
                    Async.SetBuffer(ReceiveEndIndex, TmphHttp.Default.HeaderBufferLength - ReceiveEndIndex);
                    if (socket.Socket.ReceiveAsync(Async))
                    {
                        socket.setTimeout(timeoutIdentity);
                        return;
                    }
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
                socket.headerError();
            }

            /// <summary>
            ///     接受头部换行数据
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="async">异步回调参数</param>
            private void receive(object sender, SocketAsyncEventArgs async)
            {
                ++socket.timeoutIdentity;
                var count = 0;
                try
                {
                    if (async.SocketError == SocketError.Success && (count = async.BytesTransferred) > 0)
                        ReceiveEndIndex += count;
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
                if (count <= 0 || TmphDate.NowSecond >= timeout) socket.headerError();
                else onReceive();
            }
        }

        /// <summary>
        ///     表单数据接收器
        /// </summary>
        private sealed class TmphFormIdentityReceiver : TmphFormIdentityReceiver<TmphSocket>, IDisposable
        {
            /// <summary>
            ///     异步套接字操作
            /// </summary>
            public SocketAsyncEventArgs Async;

            /// <summary>
            ///     HTTP表单接收器
            /// </summary>
            /// <param name="socket">HTTP套接字</param>
            public TmphFormIdentityReceiver(TmphSocket socket)
                : base(socket)
            {
                Async = TmphSocketAsyncEventArgs.Get();
                Async.SocketFlags = SocketFlags.None;
                Async.DisconnectReuseSocket = false;
                Async.Completed += receive;
                Async.UserToken = this;
            }

            /// <summary>
            ///     释放资源
            /// </summary>
            public void Dispose()
            {
                Async.Completed -= receive;
                TmphSocketAsyncEventArgs.Push(ref Async);
            }

            /// <summary>
            ///     开始接收表单数据
            /// </summary>
            /// <param name="loadForm">HTTP请求表单加载接口</param>
            public void Receive(TmphRequestForm.TmphILoadForm loadForm)
            {
                this.loadForm = loadForm;
                var headerReceiver = socket.HeaderReceiver;
                contentLength = headerReceiver.RequestHeader.ContentLength;
                if (contentLength < socket.Buffer.Length)
                {
                    TmphBuffer = socket.Buffer;
                    memoryPool = null;
                }
                else
                {
                    memoryPool = getMemoryPool(contentLength + 1);
                    TmphBuffer = memoryPool.Get(contentLength + 1);
                }
                receiveEndIndex = headerReceiver.ReceiveEndIndex - headerReceiver.HeaderEndIndex - sizeof(int);
                System.Buffer.BlockCopy(headerReceiver.RequestHeader.TmphBuffer,
                    headerReceiver.HeaderEndIndex + sizeof(int), TmphBuffer, 0, receiveEndIndex);
                headerReceiver.ReceiveEndIndex = headerReceiver.HeaderEndIndex;

                if (receiveEndIndex == contentLength) parse();
                else
                {
                    receiveStartTime = TmphDate.NowSecond.AddTicks(TmphDate.SecondTicks);
                    Async.SocketError = SocketError.Success;
                    Async.SetBuffer(TmphBuffer, 0, TmphBuffer.Length);
                    receive();
                }
            }

            /// <summary>
            ///     开始接收表单数据
            /// </summary>
            private void receive()
            {
                try
                {
                    var timeoutIdentity = socket.timeoutIdentity;
                    Async.SetBuffer(receiveEndIndex, contentLength - receiveEndIndex);
                    if (socket.Socket.ReceiveAsync(Async))
                    {
                        socket.setTimeout(timeoutIdentity);
                        return;
                    }
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
                receiveError();
            }

            /// <summary>
            ///     接收表单数据处理
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="async">异步回调参数</param>
            private void receive(object sender, SocketAsyncEventArgs async)
            {
                ++socket.timeoutIdentity;
                var count = 0;
                try
                {
                    if (async.SocketError == SocketError.Success && (count = async.BytesTransferred) > 0)
                    {
                        receiveEndIndex += count;
                    }
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
                if (count <= 0) receiveError();
                else if (receiveEndIndex == contentLength) parse();
                else if (TmphDate.NowSecond > receiveStartTime &&
                         receiveEndIndex <
                         minReceiveSizePerSecond4 * ((int)(TmphDate.NowSecond - receiveStartTime).TotalSeconds >> 2))
                    receiveError();
                else receive();
            }
        }

        /// <summary>
        ///     数据接收器
        /// </summary>
        private sealed class TmphBoundaryIdentityReceiver : TmphBoundaryIdentityReceiver<TmphSocket>, IDisposable
        {
            /// <summary>
            ///     异步套接字操作
            /// </summary>
            public SocketAsyncEventArgs Async;

            /// <summary>
            ///     HTTP数据接收器
            /// </summary>
            /// <param name="socket">HTTP套接字</param>
            public TmphBoundaryIdentityReceiver(TmphSocket socket)
                : base(socket)
            {
                Async = TmphSocketAsyncEventArgs.Get();
                Async.SocketFlags = SocketFlags.None;
                Async.DisconnectReuseSocket = false;
                Async.Completed += receive;
                Async.UserToken = this;
            }

            /// <summary>
            ///     释放资源
            /// </summary>
            public void Dispose()
            {
                Async.Completed -= receive;
                TmphSocketAsyncEventArgs.Push(ref Async);
            }

            /// <summary>
            ///     开始接收表单数据
            /// </summary>
            /// <param name="loadForm">HTTP请求表单加载接口</param>
            public void Receive(TmphRequestForm.TmphILoadForm loadForm)
            {
                this.loadForm = loadForm;
                try
                {
                    TmphBuffer = bigBuffers.Get();
                    var headerReceiver = socket.HeaderReceiver;
                    boundary = headerReceiver.RequestHeader.Boundary;
                    receiveLength =
                        receiveEndIndex = headerReceiver.ReceiveEndIndex - headerReceiver.HeaderEndIndex - sizeof(int);
                    System.Buffer.BlockCopy(headerReceiver.RequestHeader.TmphBuffer,
                        headerReceiver.HeaderEndIndex + sizeof(int), TmphBuffer, 0, receiveEndIndex);
                    headerReceiver.ReceiveEndIndex = headerReceiver.HeaderEndIndex;
                    contentLength = headerReceiver.RequestHeader.ContentLength;

                    receiveStartTime = TmphDate.NowSecond.AddTicks(TmphDate.SecondTicks);
                    Async.SocketError = SocketError.Success;
                    Async.SetBuffer(TmphBuffer, 0, TmphBuffer.Length);
                    onFirstBoundary();
                    return;
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
                this.error();
            }

            /// <summary>
            ///     开始接收表单数据
            /// </summary>
            protected override void receive()
            {
                try
                {
                    var timeoutIdentity = socket.timeoutIdentity;
                    Async.SetBuffer(receiveEndIndex, bigBuffers.Size - receiveEndIndex - sizeof(int));
                    if (socket.Socket.ReceiveAsync(Async))
                    {
                        socket.setTimeout(timeoutIdentity);
                        return;
                    }
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
                this.error();
            }

            /// <summary>
            ///     接收表单数据处理
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="async">异步回调参数</param>
            private void receive(object sender, SocketAsyncEventArgs async)
            {
                ++socket.timeoutIdentity;
                var count = 0;
                try
                {
                    if (async.SocketError == SocketError.Success && (count = async.BytesTransferred) > 0)
                    {
                        receiveEndIndex += count;
                        receiveLength += count;
                    }
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
                if (count <= 0 || receiveLength > contentLength
                    ||
                    (TmphDate.NowSecond > receiveStartTime &&
                     receiveLength <
                     minReceiveSizePerSecond4 * ((int)(TmphDate.NowSecond - receiveStartTime).TotalSeconds >> 2)))
                {
                    error();
                }
                else onReceiveData();
            }
        }

        /// <summary>
        ///     数据发送器
        /// </summary>
        private sealed class TmphDataSender : TmphDataSender<TmphSocket>, IDisposable
        {
            /// <summary>
            ///     异步套接字操作
            /// </summary>
            public SocketAsyncEventArgs Async;

            /// <summary>
            ///     发送文件异步套接字操作
            /// </summary>
            public SocketAsyncEventArgs FileAsync;

            /// <summary>
            ///     数据发送器
            /// </summary>
            /// <param name="socket">HTTP套接字</param>
            public TmphDataSender(TmphSocket socket)
                : base(socket)
            {
                Async = TmphSocketAsyncEventArgs.Get();
                Async.SocketFlags = SocketFlags.None;
                Async.DisconnectReuseSocket = false;
                Async.Completed += send;
                Async.UserToken = this;

                FileAsync = TmphSocketAsyncEventArgs.Get();
                FileAsync.SocketFlags = SocketFlags.None;
                FileAsync.DisconnectReuseSocket = false;
                FileAsync.Completed += sendFile;
                FileAsync.UserToken = this;
            }

            /// <summary>
            ///     释放资源
            /// </summary>
            public void Dispose()
            {
                Async.Completed -= send;
                TmphSocketAsyncEventArgs.Push(ref Async);
                FileAsync.Completed -= sendFile;
                TmphSocketAsyncEventArgs.Push(ref FileAsync);
            }

            /// <summary>
            ///     发送数据
            /// </summary>
            /// <param name="onSend">发送数据回调处理</param>
            /// <param name="TmphBuffer">发送数据缓冲区</param>
            /// <param name="pushBuffer">发送数据缓冲区回调处理</param>
            public unsafe void Send(Action<bool> onSend, TmphSubArray<byte> TmphBuffer, TmphMemoryPool memoryPool)
            {
                this.onSend = onSend;
                sendStartTime = TmphDate.NowSecond.AddTicks(TmphDate.SecondTicks);
                this.memoryPool = memoryPool;
                this.TmphBuffer = TmphBuffer.Array;
                sendIndex = TmphBuffer.StartIndex;
                sendLength = 0;
                sendEndIndex = sendIndex + TmphBuffer.Count;
                //showjim
                if (sendEndIndex > this.TmphBuffer.Length || sendEndIndex <= sendIndex)
                {
                    var requestHeader = socket.HeaderReceiver.RequestHeader;
                    fixed (byte* headerBufferFixed = requestHeader.TmphBuffer)
                    {
                        TmphLog.Error.Add(
                            "TmphBuffer[" + this.TmphBuffer.Length.toString() + "] sendIndex[" + sendIndex.toString() +
                            "] sendEndIndex[" + sendEndIndex.toString() + "] State[" + socket.response.State +
                            "] responseSize[" + socket.responseSize.toString() + "]" +
                            TmphString.DeSerialize(headerBufferFixed + requestHeader.Uri.StartIndex,
                                requestHeader.Uri.Count + 2), true, false);
                    }
                }

                Async.SocketError = SocketError.Success;
                Async.SetBuffer(this.TmphBuffer, 0, this.TmphBuffer.Length);
                send();
            }

            /// <summary>
            ///     开始发送数据
            /// </summary>
            private void send()
            {
                try
                {
                    var timeoutIdentity = socket.timeoutIdentity;
                    Async.SetBuffer(sendIndex, Math.Min(sendEndIndex - sendIndex, Net.TmphSocket.MaxServerSendSize));
                    if (socket.Socket.SendAsync(Async))
                    {
                        socket.setTimeout(timeoutIdentity);
                        return;
                    }
                }
                catch (Exception error)
                {
                    //showjim
                    TmphLog.Error.Add(error,
                        "sendIndex[" + sendIndex.toString() + "] sendEndIndex[" + sendEndIndex.toString() + "]", false);
                }
                send(false);
            }

            /// <summary>
            ///     发送数据处理
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="async">异步回调参数</param>
            private void send(object sender, SocketAsyncEventArgs async)
            {
                ++socket.timeoutIdentity;
                var count = 0;
                try
                {
                    if (async.SocketError == SocketError.Success && (count = async.BytesTransferred) > 0)
                    {
                        sendIndex += count;
                        sendLength += count;
                    }
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
                if (count <= 0) send(false);
                else if (sendIndex == sendEndIndex) send(true);
                else if (TmphDate.NowSecond > sendStartTime &&
                         sendLength <
                         minReceiveSizePerSecond4 * ((int)(TmphDate.NowSecond - sendStartTime).TotalSeconds >> 2))
                    send(false);
                else send();
            }

            /// <summary>
            ///     发送文件数据
            /// </summary>
            /// <param name="onSend">发送数据回调处理</param>
            /// <param name="fileName">文件名称</param>
            /// <param name="seek">起始位置</param>
            /// <param name="size">发送字节长度</param>
            public void SendFile(Action<bool> onSend, string fileName, long seek, long size)
            {
                try
                {
                    fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read,
                        TmphAppSetting.StreamBufferSize, FileOptions.SequentialScan);
                    if (fileStream.Length >= seek + size)
                    {
                        if (seek != 0) fileStream.Seek(seek, SeekOrigin.Begin);
                        this.onSend = onSend;
                        sendStartTime = TmphDate.NowSecond.AddTicks(TmphDate.SecondTicks);
                        fileSize = size;
                        sendLength = 0;

                        memoryPool = Net.TmphSocket.ServerSendBuffers;
                        TmphBuffer = memoryPool.Get();
                        FileAsync.SetBuffer(TmphBuffer, 0, TmphBuffer.Length);
                        FileAsync.SocketError = SocketError.Success;
                        readFile();
                        return;
                    }
                    fileStream.Dispose();
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
                onSend(false);
            }

            /// <summary>
            ///     开始发送文件数据
            /// </summary>
            protected override void sendFile()
            {
                try
                {
                    var timeoutIdentity = socket.timeoutIdentity;
                    FileAsync.SetBuffer(sendIndex, sendEndIndex - sendIndex);
                    if (socket.Socket.SendAsync(FileAsync))
                    {
                        socket.setTimeout(timeoutIdentity);
                        return;
                    }
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
                sendFile(false);
            }

            /// <summary>
            ///     发送文件数据处理
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="async">异步回调参数</param>
            private void sendFile(object sender, SocketAsyncEventArgs async)
            {
                ++socket.timeoutIdentity;
                var count = 0;
                try
                {
                    if (async.SocketError == SocketError.Success && (count = async.BytesTransferred) > 0)
                    {
                        sendIndex += count;
                        sendLength += count;
                    }
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
                if (count <= 0) sendFile(false);
                else if (sendIndex == sendEndIndex) readFile();
                else if (TmphDate.NowSecond > sendStartTime &&
                         sendLength <
                         minReceiveSizePerSecond4 * ((int)(TmphDate.NowSecond - sendStartTime).TotalSeconds >> 2))
                    sendFile(false);
                else sendFile();
            }
        }

        /// <summary>
        ///     WebSocket请求接收器
        /// </summary>
        private sealed class TmphWebSocketIdentityReceiver : TmphWebSocketIdentityReceiver<TmphSocket>, IDisposable
        {
            /// <summary>
            ///     异步套接字操作
            /// </summary>
            public SocketAsyncEventArgs Async;

            /// <summary>
            ///     WebSocket请求接收器
            /// </summary>
            /// <param name="socket">HTTP套接字</param>
            public TmphWebSocketIdentityReceiver(TmphSocket socket)
                : base(socket)
            {
                Async = TmphSocketAsyncEventArgs.Get();
                Async.SocketFlags = SocketFlags.None;
                Async.DisconnectReuseSocket = false;
                Async.Completed += receive;
                Async.UserToken = this;
                Async.SetBuffer(TmphBuffer, 0, TmphHttp.Default.HeaderBufferLength);
            }

            /// <summary>
            ///     释放资源
            /// </summary>
            public void Dispose()
            {
                Async.Completed -= receive;
                TmphSocketAsyncEventArgs.Push(ref Async);
            }

            /// <summary>
            ///     开始接收请求数据
            /// </summary>
            public void Receive()
            {
                receiveEndIndex = 0;
                timeout = TmphDate.NowSecond.AddTicks(WebSocketReceiveTimeoutQueue.CallbackTimeoutTicks);
                Async.SocketError = SocketError.Success;
                receive();
            }

            /// <summary>
            ///     开始接收数据
            /// </summary>
            protected override void receive()
            {
                try
                {
                    var timeoutIdentity = socket.timeoutIdentity;
                    Async.SetBuffer(receiveEndIndex, TmphHttp.Default.HeaderBufferLength - receiveEndIndex);
                    if (socket.Socket.ReceiveAsync(Async))
                    {
                        socket.setTimeout(timeoutIdentity);
                        return;
                    }
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
                socket.webSocketEnd();
            }

            /// <summary>
            ///     WebSocket请求数据处理
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="async">异步回调参数</param>
            private void receive(object sender, SocketAsyncEventArgs async)
            {
                ++socket.timeoutIdentity;
                var count = int.MinValue;
                try
                {
                    if (async.SocketError == SocketError.Success && (count = async.BytesTransferred) >= 0)
                    {
                        receiveEndIndex += count;
                    }
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
                if (count < 0) socket.webSocketEnd();
                else if (TmphDate.NowSecond >= timeout) close();
                else if (count == 0) TmphTimerTask.Default.Add(receiveHandle, TmphDate.NowSecond.AddSeconds(1), null);
                else if (receiveEndIndex >= 6) tryParse();
                else receive();
            }
        }
    }
}