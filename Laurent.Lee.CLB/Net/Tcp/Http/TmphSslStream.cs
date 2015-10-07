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
using Laurent.Lee.CLB.Threading;
using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using TmphHttp = Laurent.Lee.CLB.Web.TmphHttp;

namespace Laurent.Lee.CLB.Net.Tcp.Http
{
    /// <summary>
    ///     HTTP安全流
    /// </summary>
    internal sealed class TmphSslStream : TmphSocketBase
    {
        /// <summary>
        ///     身份验证完成处理
        /// </summary>
        private readonly AsyncCallback authenticateCallback;

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
        ///     网络流
        /// </summary>
        private NetworkStream networkStream;

        /// <summary>
        ///     安全网络流
        /// </summary>
        internal SslStream SslStream;

        /// <summary>
        ///     WebSocket请求接收器
        /// </summary>
        private TmphWebSocketIdentityReceiver webSocketReceiver;

        /// <summary>
        ///     HTTP安全流
        /// </summary>
        private TmphSslStream()
        {
            HeaderReceiver = new TmphHeaderReceiver(this);
            sender = new TmphDataSender(this);
            authenticateCallback = onAuthenticate;
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
        /// <param name="certificate">SSL证书</param>
        private void start(TmphServers servers, Socket socket, X509Certificate certificate)
        {
            this.servers = servers;
            Socket = socket;
            try
            {
                SslStream = new SslStream(networkStream = new NetworkStream(socket, true), false);
                SslStream.BeginAuthenticateAsServer(certificate, false, SslProtocols.Tls, false, authenticateCallback,
                    this);
                return;
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, false);
            }
            headerError();
        }

        /// <summary>
        ///     身份验证完成处理
        /// </summary>
        /// <param name="result">异步操作状态</param>
        private void onAuthenticate(IAsyncResult result)
        {
            try
            {
                SslStream.EndAuthenticateAsServer(result);
                isLoadForm = isNextRequest = 0;
                DomainServer = null;
                form.Clear();
                TmphResponse.Push(ref response);
                HeaderReceiver.RequestHeader.IsKeepAlive = false;
                HeaderReceiver.Receive();
                return;
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, false);
            }
            headerError();
        }

        /// <summary>
        ///     HTTP头部接收错误
        /// </summary>
        protected override void headerError()
        {
            if (ipv6.IsNull) TmphSslServer.SocketEnd(ipv4);
            else TmphSslServer.SocketEnd(ipv6);
            close();
            form.Clear();
            TmphTypePool<TmphSslStream>.Push(this);
        }

        /// <summary>
        ///     WebSocket结束
        /// </summary>
        protected override void webSocketEnd()
        {
            webSocketReceiver.Clear();
            if (ipv6.IsNull) TmphSslServer.SocketEnd(ipv4);
            else TmphSslServer.SocketEnd(ipv6);
            close();
            TmphTypePool<TmphSslStream>.Push(this);
        }

        /// <summary>
        ///     未能识别的HTTP头部
        /// </summary>
        protected override void headerUnknown()
        {
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
                    if (HeaderReceiver.RequestHeader.Method == TmphHttp.TmphMethodType.POST && isLoadForm == 0)
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
                        HeaderReceiver.RequestHeader.Method != TmphHttp.TmphMethodType.GET)
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
                    HeaderReceiver.RequestHeader.Method == TmphHttp.TmphMethodType.GET)
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
        ///     关闭套接字
        /// </summary>
        private void close()
        {
            TmphPub.Dispose(ref SslStream);
            TmphPub.Dispose(ref networkStream);
            close(Socket);
            Socket = null;
        }

        /// <summary>
        ///     开始处理新的请求
        /// </summary>
        /// <param name="servers">HTTP服务器</param>
        /// <param name="socket">套接字</param>
        /// <param name="ip">客户端IP</param>
        /// <param name="certificate">SSL证书</param>
        internal static void Start(TmphServers servers, Socket socket, TmphIpv6Hash ip, X509Certificate certificate)
        {
            try
            {
                var value = TmphTypePool<TmphSslStream>.Pop() ?? new TmphSslStream();
                value.ipv6 = ip;
                value.ipv4 = 0;
                value.start(servers, socket, certificate);
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, false);
                TmphSslServer.SocketEnd(ip);
                close(socket);
            }
        }

        /// <summary>
        ///     开始处理新的请求
        /// </summary>
        /// <param name="servers">HTTP服务器</param>
        /// <param name="socket">套接字</param>
        /// <param name="ip">客户端IP</param>
        /// <param name="certificate">SSL证书</param>
        internal static void Start(TmphServers servers, Socket socket, int ip, X509Certificate certificate)
        {
            try
            {
                var value = TmphTypePool<TmphSslStream>.Pop() ?? new TmphSslStream();
                value.ipv4 = ip;
                value.ipv6 = default(TmphIpv6Hash);
                value.start(servers, socket, certificate);
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, false);
                TmphSslServer.SocketEnd(ip);
                close(socket);
            }
        }

        /// <summary>
        ///     HTTP头部接收器
        /// </summary>
        internal sealed class TmphHeaderReceiver : TmphHeaderReceiver<TmphSslStream>
        {
            /// <summary>
            ///     接受头部换行数据
            /// </summary>
            private readonly AsyncCallback receiveCallback;

            /// <summary>
            ///     HTTP头部接收器
            /// </summary>
            /// <param name="sslStream">HTTP安全流</param>
            public TmphHeaderReceiver(TmphSslStream sslStream)
                : base(sslStream)
            {
                receiveCallback = receive;
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
            ///     接受头部换行数据
            /// </summary>
            protected override void receive()
            {
                try
                {
                    var timeoutIdentity = socket.timeoutIdentity;
                    socket.SslStream.BeginRead(TmphBuffer, ReceiveEndIndex,
                        Config.TmphHttp.Default.HeaderBufferLength - ReceiveEndIndex, receiveCallback, this);
                    socket.setTimeout(timeoutIdentity);
                    return;
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
            /// <param name="result">异步操作状态</param>
            private void receive(IAsyncResult result)
            {
                ++socket.timeoutIdentity;
                var count = 0;
                try
                {
                    if ((count = socket.SslStream.EndRead(result)) > 0) ReceiveEndIndex += count;
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
        private sealed class TmphFormIdentityReceiver : TmphFormIdentityReceiver<TmphSslStream>
        {
            /// <summary>
            ///     接收表单数据处理
            /// </summary>
            private readonly AsyncCallback receiveCallback;

            /// <summary>
            ///     HTTP表单接收器
            /// </summary>
            /// <param name="socket">HTTP套接字</param>
            public TmphFormIdentityReceiver(TmphSslStream socket)
                : base(socket)
            {
                receiveCallback = receive;
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
                    socket.SslStream.BeginRead(TmphBuffer, receiveEndIndex, contentLength - receiveEndIndex, receiveCallback,
                        this);
                    socket.setTimeout(timeoutIdentity);
                    return;
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
            /// <param name="result">异步操作状态</param>
            private void receive(IAsyncResult result)
            {
                ++socket.timeoutIdentity;
                var count = 0;
                try
                {
                    if ((count = socket.SslStream.EndRead(result)) > 0)
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
        private sealed class TmphBoundaryIdentityReceiver : TmphBoundaryIdentityReceiver<TmphSslStream>
        {
            /// <summary>
            ///     接收表单数据处理
            /// </summary>
            private readonly AsyncCallback receiveCallback;

            /// <summary>
            ///     HTTP数据接收器
            /// </summary>
            /// <param name="socket">HTTP套接字</param>
            public TmphBoundaryIdentityReceiver(TmphSslStream socket)
                : base(socket)
            {
                receiveCallback = receive;
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
                    socket.SslStream.BeginRead(TmphBuffer, receiveEndIndex, bigBuffers.Size - receiveEndIndex - sizeof(int),
                        receiveCallback, this);
                    socket.setTimeout(timeoutIdentity);
                    return;
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
            /// <param name="result">异步操作状态</param>
            private void receive(IAsyncResult result)
            {
                ++socket.timeoutIdentity;
                var count = 0;
                try
                {
                    if ((count = socket.SslStream.EndRead(result)) > 0)
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
        private sealed class TmphDataSender : TmphDataSender<TmphSslStream>
        {
            /// <summary>
            ///     发送数据处理
            /// </summary>
            private readonly AsyncCallback sendCallback;

            /// <summary>
            ///     发送文件数据处理
            /// </summary>
            private readonly AsyncCallback sendFileCallback;

            /// <summary>
            ///     当前发送字节数
            /// </summary>
            private int sendSize;

            /// <summary>
            ///     数据发送器
            /// </summary>
            /// <param name="socket">HTTP套接字</param>
            public TmphDataSender(TmphSslStream socket)
                : base(socket)
            {
                sendCallback = send;
                sendFileCallback = sendFile;
            }

            /// <summary>
            ///     发送数据
            /// </summary>
            /// <param name="onSend">发送数据回调处理</param>
            /// <param name="TmphBuffer">发送数据缓冲区</param>
            /// <param name="pushBuffer">发送数据缓冲区回调处理</param>
            public void Send(Action<bool> onSend, TmphSubArray<byte> TmphBuffer, TmphMemoryPool memoryPool)
            {
                this.onSend = onSend;
                sendStartTime = TmphDate.NowSecond.AddTicks(TmphDate.SecondTicks);
                this.memoryPool = memoryPool;
                this.TmphBuffer = TmphBuffer.Array;
                sendIndex = TmphBuffer.StartIndex;
                sendLength = 0;
                sendEndIndex = sendIndex + TmphBuffer.Count;

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
                    socket.SslStream.BeginWrite(TmphBuffer, sendIndex,
                        sendSize = Math.Min(sendEndIndex - sendIndex, Net.TmphSocket.MaxServerSendSize), sendCallback,
                        this);
                    socket.setTimeout(timeoutIdentity);
                    return;
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
                send(false);
            }

            /// <summary>
            ///     发送数据处理
            /// </summary>
            /// <param name="async">异步调用状态</param>
            private void send(IAsyncResult result)
            {
                ++socket.timeoutIdentity;
                try
                {
                    socket.SslStream.EndWrite(result);
                    sendIndex += sendSize;
                    sendLength += sendSize;
                }
                catch (Exception error)
                {
                    send(false);
                    TmphLog.Error.Add(error, null, false);
                    return;
                }
                if (sendIndex == sendEndIndex) send(true);
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
                    socket.SslStream.BeginWrite(TmphBuffer, sendIndex, sendSize = sendEndIndex - sendIndex, sendFileCallback,
                        this);
                    socket.setTimeout(timeoutIdentity);
                    return;
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
            /// <param name="result">异步操作状态</param>
            private void sendFile(IAsyncResult result)
            {
                ++socket.timeoutIdentity;
                try
                {
                    socket.SslStream.EndWrite(result);
                    sendIndex += sendSize;
                    sendLength += sendSize;
                }
                catch (Exception error)
                {
                    sendFile(false);
                    TmphLog.Error.Add(error, null, false);
                    return;
                }
                if (sendIndex == sendEndIndex) readFile();
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
        private sealed class TmphWebSocketIdentityReceiver : TmphWebSocketIdentityReceiver<TmphSslStream>
        {
            /// <summary>
            ///     WebSocket请求数据处理
            /// </summary>
            private readonly AsyncCallback receiveCallback;

            /// <summary>
            ///     WebSocket请求接收器
            /// </summary>
            /// <param name="socket">HTTP套接字</param>
            public TmphWebSocketIdentityReceiver(TmphSslStream socket)
                : base(socket)
            {
                receiveCallback = receive;
            }

            /// <summary>
            ///     开始接收请求数据
            /// </summary>
            public void Receive()
            {
                receiveEndIndex = 0;
                timeout = TmphDate.NowSecond.AddTicks(WebSocketReceiveTimeoutQueue.CallbackTimeoutTicks);
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
                    socket.SslStream.BeginRead(TmphBuffer, receiveEndIndex,
                        Config.TmphHttp.Default.HeaderBufferLength - receiveEndIndex, receiveCallback, this);
                    socket.setTimeout(timeoutIdentity);
                    return;
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
            /// <param name="result">异步调用状态</param>
            private void receive(IAsyncResult result)
            {
                ++socket.timeoutIdentity;
                var count = int.MinValue;
                try
                {
                    if ((count = socket.SslStream.EndRead(result)) >= 0)
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