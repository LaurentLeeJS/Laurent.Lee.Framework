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
using System.Net.Sockets;
using System.Threading;

namespace Laurent.Lee.CLB.Net.Tcp.Http
{
    /// <summary>
    ///     HTTP转发代理
    /// </summary>
    internal sealed class TmphForwardProxy : IDisposable
    {
        /// <summary>
        ///     接收HTTP响应数据处理
        /// </summary>
        private readonly AsyncCallback onReceiveResponseHandle;

        /// <summary>
        ///     发送HTTP响应数据处理
        /// </summary>
        private readonly AsyncCallback onSendResponseHandle;

        /// <summary>
        ///     HTTP代理套接字
        /// </summary>
        private readonly Net.TmphSocket proxySocket;

        /// <summary>
        ///     HTTP请求数据缓冲区
        /// </summary>
        private readonly byte[] requestBuffer;

        /// <summary>
        ///     HTTP请求套接字
        /// </summary>
        private readonly Socket requestSocket;

        /// <summary>
        ///     HTTP响应数据缓冲区
        /// </summary>
        private readonly byte[] responseBuffer;

        /// <summary>
        ///     HTTP套接字
        /// </summary>
        private readonly TmphSocket socket;

        /// <summary>
        ///     HTTP代理服务客户端
        /// </summary>
        private TmphClient TmphClient;

        /// <summary>
        ///     是否已经释放资源
        /// </summary>
        private int isDisposed;

        /// <summary>
        ///     接收HTTP请求数据处理
        /// </summary>
        public AsyncCallback onReceiveRequestHandle;

        /// <summary>
        ///     接收数据标识
        /// </summary>
        private int receiveFlag;

        /// <summary>
        ///     HTTP响应数据结束位置
        /// </summary>
        private int responseEndIndex;

        /// <summary>
        ///     HTTP响应数据开始位置
        /// </summary>
        private int responseStartIndex;

        /// <summary>
        ///     HTTP转发代理
        /// </summary>
        /// <param name="socket">HTTP套接字</param>
        /// <param name="TmphClient">HTTP代理服务客户端</param>
        public TmphForwardProxy(TmphSocket socket, TmphClient TmphClient)
        {
            this.socket = socket;
            this.TmphClient = TmphClient;
            requestSocket = socket.Socket;
            proxySocket = TmphClient.NetSocket;
            requestBuffer = socket.HeaderReceiver.RequestHeader.TmphBuffer;
            responseBuffer = socket.Buffer;
            onReceiveRequestHandle = onReceiveRequest;
            onReceiveResponseHandle = onReceiveResponse;
            onSendResponseHandle = onSendResponse;
        }

        /// <summary>
        ///     释放资源
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.Increment(ref isDisposed) == 1)
            {
                try
                {
                    TmphPub.Dispose(ref TmphClient);
                    requestSocket.Shutdown(SocketShutdown.Both);
                }
                catch (Exception error)
                {
                    TmphLog.Default.Add(error, null, false);
                }
                finally
                {
                    requestSocket.Close();
                    while (receiveFlag != 0) Thread.Sleep(1);
                    socket.ProxyEnd();
                }
            }
        }

        /// <summary>
        ///     开始代理HTTP转发
        /// </summary>
        public void Start()
        {
            onReceiveRequest(socket.HeaderReceiver.ReceiveEndIndex);
            receiveResponse();
        }

        /// <summary>
        ///     接收请求数据回调处理
        /// </summary>
        /// <param name="count">接收数据长度</param>
        private void onReceiveRequest(int count)
        {
            if (isDisposed == 0)
            {
                receiveFlag |= 1;
                try
                {
                    if (proxySocket.send(requestBuffer, 0, count))
                    {
                        if (isDisposed == 0)
                        {
                            SocketError error;
                            requestSocket.BeginReceive(requestBuffer, 0, requestBuffer.Length, SocketFlags.None,
                                out error, onReceiveRequestHandle, this);
                            if (error == SocketError.Success) return;
                        }
                    }
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
                receiveFlag &= (int.MaxValue - 1);
                Dispose();
            }
        }

        /// <summary>
        ///     接收请求数据回调处理
        /// </summary>
        /// <param name="result">接收数据结果</param>
        private void onReceiveRequest(IAsyncResult result)
        {
            receiveFlag &= (int.MaxValue - 1);
            if (isDisposed == 0)
            {
                try
                {
                    SocketError error;
                    var count = requestSocket.EndReceive(result, out error);
                    if (error == SocketError.Success && count > 0)
                    {
                        onReceiveRequest(count);
                        return;
                    }
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
                Dispose();
            }
        }

        /// <summary>
        ///     接收HTTP响应数据
        /// </summary>
        private void receiveResponse()
        {
            if (isDisposed == 0)
            {
                receiveFlag |= 2;
                try
                {
                    if (isDisposed == 0)
                    {
                        SocketError error;
                        proxySocket.Socket.BeginReceive(responseBuffer, 0, responseBuffer.Length, SocketFlags.None,
                            out error, onReceiveResponseHandle, this);
                        if (error == SocketError.Success) return;
                    }
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
                receiveFlag &= int.MaxValue - 2;
                Dispose();
            }
        }

        /// <summary>
        ///     接收响应数据回调处理
        /// </summary>
        /// <param name="result">接收数据结果</param>
        private void onReceiveResponse(IAsyncResult result)
        {
            receiveFlag &= int.MaxValue - 2;
            if (isDisposed == 0)
            {
                try
                {
                    SocketError error;
                    responseEndIndex = proxySocket.Socket.EndReceive(result, out error);
                    if (error == SocketError.Success && responseEndIndex > 0)
                    {
                        responseStartIndex = 0;
                        sendResponse();
                        return;
                    }
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
                Dispose();
            }
        }

        /// <summary>
        ///     发送HTTP响应数据
        /// </summary>
        private void sendResponse()
        {
            if (isDisposed == 0)
            {
                try
                {
                    SocketError error;
                    requestSocket.BeginSend(responseBuffer, responseStartIndex, responseEndIndex - responseStartIndex,
                        SocketFlags.None, out error, onSendResponseHandle, this);
                    if (error == SocketError.Success) return;
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
                Dispose();
            }
        }

        /// <summary>
        ///     发送HTTP响应数据处理
        /// </summary>
        /// <param name="result">发送数据结果</param>
        private void onSendResponse(IAsyncResult result)
        {
            if (isDisposed == 0)
            {
                try
                {
                    SocketError error;
                    var count = requestSocket.EndSend(result, out error);
                    if (error == SocketError.Success && count > 0)
                    {
                        responseStartIndex += count;
                        if (responseStartIndex == responseEndIndex) receiveResponse();
                        else sendResponse();
                        return;
                    }
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
                Dispose();
            }
        }
    }
}