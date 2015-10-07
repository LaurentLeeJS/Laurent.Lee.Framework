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

using Laurent.Lee.CLB.Threading;
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace Laurent.Lee.CLB.Net
{
    /// <summary>
    ///     套接字
    /// </summary>
    public class TmphSocket : IDisposable
    {
        /// <summary>
        ///     服务器端套接字单次最大发送数据量
        /// </summary>
        internal static readonly int MaxServerSendSize = Config.TmphPub.Default.MaxServerSocketSendSize;

        /// <summary>
        ///     服务器端套接字发送缓冲区
        /// </summary>
        internal static readonly TmphMemoryPool ServerSendBuffers = TmphMemoryPool.GetPool(MaxServerSendSize);

        /// <summary>
        ///     操作错误是否自动调用析构函数
        /// </summary>
        private readonly bool isErrorDispose;

        /// <summary>
        ///     当前接收数据
        /// </summary>
        protected byte[] currentReceiveData;

        /// <summary>
        ///     当前接收数据起始位置
        /// </summary>
        private int currentReceiveStartIndex;

        /// <summary>
        ///     是否释放资源
        /// </summary>
        protected int isDisposed;

        /// <summary>
        ///     最后一次异常
        /// </summary>
        protected Exception lastException;

        /// <summary>
        ///     操作套接字
        /// </summary>
        protected internal Socket Socket;

        /// <summary>
        ///     套接字错误
        /// </summary>
        protected SocketError socketError;

        /// <summary>
        ///     初始化同步套接字
        /// </summary>
        /// <param name="socket">操作套接字</param>
        /// <param name="isErrorDispose">操作错误是否自动调用析构函数</param>
        public TmphSocket(Socket socket, bool isErrorDispose = true)
            : this(isErrorDispose)
        {
            if (socket == null) TmphLog.Error.Throw(null, "缺少套接字", true);
            Socket = socket;
        }

        /// <summary>
        ///     初始化同步套接字
        /// </summary>
        /// <param name="isErrorDispose">操作错误是否自动调用析构函数</param>
        protected TmphSocket(bool isErrorDispose)
        {
            this.isErrorDispose = isErrorDispose;
        }

        /// <summary>
        ///     当前接收数据结束位置
        /// </summary>
        protected int currentReceiveEndIndex { get; private set; }

        /// <summary>
        ///     最后一次异常
        /// </summary>
        public Exception LastException
        {
            get
            {
                if (lastException != null) return lastException;
                if (socketError != SocketError.Success) return new SocketException((int)socketError);
                return null;
            }
        }

        /// <summary>
        ///     是否释放资源
        /// </summary>
        public bool IsDisposed
        {
            get { return isDisposed != 0; }
        }

        /// <summary>
        ///     关闭套接字连接
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.Increment(ref isDisposed) == 1)
            {
                TmphLog.Default.Add("关闭套接字连接", true, false);
                try
                {
                    dispose();
                }
                finally
                {
                    close();
                }
            }
        }

        /// <summary>
        ///     关闭套接字连接
        /// </summary>
        protected virtual void dispose()
        {
        }

        /// <summary>
        ///     设置错误异常
        /// </summary>
        /// <param name="error">错误异常</param>
        private void setException(Exception error)
        {
            lastException = error;
        }

        /// <summary>
        ///     操作错误
        /// </summary>
        protected void error()
        {
            if (isErrorDispose) Dispose();
            else close();
        }

        /// <summary>
        ///     关闭套接字
        /// </summary>
        protected virtual void close()
        {
            //if (Socket != null) Socket.Close();
            Socket.shutdown();
            Socket = null;
        }

        /// <summary>
        ///     接收数据
        /// </summary>
        /// <param name="data">数据缓存</param>
        /// <param name="index">起始位置</param>
        /// <param name="endIndex">结束位置</param>
        /// <param name="timeout">超时时间</param>
        /// <returns>是否成功</returns>
        protected internal bool receive(byte[] data, int index, int endIndex, DateTime timeout)
        {
            var socket = Socket;
            if (socket == null) return false;
            while (index != endIndex)
            {
                SocketError error;
                var length = Socket.Receive(data, index, endIndex - index, SocketFlags.None, out error);
                if (error != SocketError.Success)
                {
                    socketError = error;
                    this.error();
                    return false;
                }
                if (length == 0 || TmphDate.NowSecond > timeout)
                {
                    this.error();
                    return false;
                }
                index += length;
            }
            return true;
        }

        /// <summary>
        ///     接收数据
        /// </summary>
        /// <param name="data">数据缓存</param>
        /// <param name="index">起始位置</param>
        /// <param name="endIndex">结束位置</param>
        /// <param name="timeout">超时时间</param>
        /// <returns>数据结束位置</returns>
        protected internal int tryReceive(byte[] data, int index, int endIndex, DateTime timeout)
        {
            var socket = Socket;
            if (socket == null) return -1;
            while (index < endIndex)
            {
                SocketError error;
                var length = socket.Receive(data, index, data.Length - index, SocketFlags.None, out error);
                if (error != SocketError.Success)
                {
                    socketError = error;
                    this.error();
                    return -1;
                }
                if (length == 0 || TmphDate.NowSecond > timeout)
                {
                    this.error();
                    return -1;
                }
                index += length;
            }
            return index;
        }

        /// <summary>
        ///     接收数据
        /// </summary>
        /// <param name="index">起始位置</param>
        /// <param name="endIndex">结束位置</param>
        /// <param name="timeout">超时时间</param>
        /// <returns>数据结束位置</returns>
        protected internal int tryReceive(int index, int endIndex, DateTime timeout)
        {
            return tryReceive(currentReceiveData, index, endIndex, timeout);
        }

        /// <summary>
        ///     发送数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns>是否成功</returns>
        protected internal bool send(TmphSubArray<byte> data)
        {
            if (Socket.send(data.Array, data.StartIndex, data.Count, ref socketError)) return true;
            error();
            return false;
        }

        /// <summary>
        ///     服务器端发送数据(限制单次发送数据量)
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns>是否成功</returns>
        protected internal bool serverSend(TmphSubArray<byte> data)
        {
            if (Socket.serverSend(data.Array, data.StartIndex, data.Count, ref socketError)) return true;
            error();
            return false;
        }

        /// <summary>
        ///     发送数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="length">发送长度</param>
        /// <returns>是否成功</returns>
        protected internal bool send(byte[] data, int startIndex, int length)
        {
            if (Socket.send(data, startIndex, length, ref socketError)) return true;
            error();
            return false;
        }

        /// <summary>
        ///     接收数据
        /// </summary>
        /// <param name="onReceive">数据接收完成后的回调委托</param>
        /// <param name="index">起始位置</param>
        /// <param name="endIndex">结束位置</param>
        /// <param name="timeout">接收超时</param>
        protected void tryReceive(Action<int> onReceive, int startIndex, int endIndex, DateTime timeout)
        {
            var tryReceiver = TmphTypePool<TmphTryReceiver>.Pop();
            if (tryReceiver == null)
            {
                try
                {
                    tryReceiver = new TmphTryReceiver();
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
                if (tryReceiver == null)
                {
                    onReceive(-1);
                    return;
                }
            }
            currentReceiveStartIndex = startIndex;
            currentReceiveEndIndex = endIndex;
            tryReceiver.Socket = this;
            tryReceiver.Callback = onReceive;
            tryReceiver.Timeout = timeout;
            tryReceiver.Receive();
        }

        /// <summary>
        ///     接收数据
        /// </summary>
        /// <param name="onSend">数据接收完成后的回调委托</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="data">数据缓存</param>
        /// <param name="length">待接收数据长度</param>
        /// <param name="timeout">超时时间</param>
        protected internal void receive(Action<bool> onReceived, byte[] data, int startIndex, int length,
            DateTime timeout)
        {
            var receiver = TmphTypePool<TmphReceiver>.Pop();
            if (receiver == null)
            {
                try
                {
                    receiver = new TmphReceiver();
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
                if (receiver == null)
                {
                    onReceived(false);
                    return;
                }
            }
            currentReceiveData = data;
            currentReceiveStartIndex = startIndex;
            currentReceiveEndIndex = length + startIndex;
            receiver.Callback = onReceived;
            receiver.Socket = this;
            receiver.Timeout = timeout;
            receiver.Receive();
        }

        /// <summary>
        ///     接收数据
        /// </summary>
        /// <param name="onSend">数据接收完成后的回调委托</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="length">待接收数据长度</param>
        /// <param name="timeout">超时时间</param>
        protected internal void receive(Action<bool> onReceived, int startIndex, int length, DateTime timeout)
        {
            receive(onReceived, currentReceiveData, startIndex, length, timeout);
        }

        /// <summary>
        ///     错误日志
        /// </summary>
        /// <param name="error">错误异常</param>
        public static void ErrorLog(Exception error)
        {
            if (error != null)
            {
                var ioError = error as IOException;
                if (ioError != null) error = ioError.InnerException;
                var disposedError = error.InnerException as ObjectDisposedException;
                if (disposedError == null)
                {
                    var socketException = error as SocketException;
                    if (socketException == null || socketException.ErrorCode != 10053 ||
                        socketException.ErrorCode != 10054)
                        TmphLog.Default.Add(error, null, true);
                }
                else TmphLog.Default.Add(error, null, true);
            }
        }

        ///// <summary>
        ///// 服务器端发送数据(限制单次发送数据量)
        ///// </summary>
        ///// <param name="data">数据</param>
        ///// <param name="startIndex">起始位置</param>
        ///// <param name="length">发送长度</param>
        ///// <returns>是否成功</returns>
        //protected internal bool serverSend(byte[] data, int startIndex, int length)
        //{
        //    if (Socket.serverSend(data, startIndex, length, ref socketError)) return true;
        //    error();
        //    return false;
        //}
        /// <summary>
        ///     数据接收器
        /// </summary>
        private sealed class TmphTryReceiver : TmphCallbackActionPool<TmphTryReceiver, int>
        {
            /// <summary>
            ///     数据接收完成后的回调委托
            /// </summary>
            private readonly EventHandler<SocketAsyncEventArgs> asyncCallback;

            /// <summary>
            ///     异步套接字
            /// </summary>
            public TmphSocket Socket;

            /// <summary>
            ///     接收超时
            /// </summary>
            public DateTime Timeout;

            /// <summary>
            ///     数据接收器
            /// </summary>
            public TmphTryReceiver()
            {
                asyncCallback = onReceive;
            }

            /// <summary>
            ///     接收数据
            /// </summary>
            public void Receive()
            {
                SocketAsyncEventArgs async = null;
                try
                {
                    async = TmphSocketAsyncEventArgs.Get();
                    async.UserToken = this;
                    async.Completed += asyncCallback;
                    if (receive(async)) return;
                }
                catch (Exception error)
                {
                    Socket.lastException = error;
                }
                callback(async);
            }

            /// <summary>
            ///     继续接收数据
            /// </summary>
            /// <param name="async">异步套接字操作</param>
            /// <returns>是否接收成功</returns>
            private bool receive(SocketAsyncEventArgs async)
            {
                async.SetBuffer(Socket.currentReceiveData, Socket.currentReceiveStartIndex,
                    Socket.currentReceiveData.Length - Socket.currentReceiveStartIndex);
                if (Socket.Socket.ReceiveAsync(async)) return true;
                if (async.SocketError != SocketError.Success) Socket.socketError = async.SocketError;
                return false;
            }

            /// <summary>
            ///     数据接收完成后的回调委托
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="async">异步回调参数</param>
            private void onReceive(object sender, SocketAsyncEventArgs async)
            {
                var isSuccess = false;
                try
                {
                    if (async.SocketError == SocketError.Success)
                    {
                        var count = async.BytesTransferred;
                        if (count != 0)
                        {
                            Socket.currentReceiveStartIndex += count;
                            if (Socket.currentReceiveStartIndex >= Socket.currentReceiveEndIndex)
                            {
                                isSuccess = true;
                                callback(Socket.currentReceiveStartIndex, async);
                                return;
                            }
                            if (receive(async)) return;
                        }
                    }
                    else Socket.socketError = async.SocketError;
                }
                catch (Exception error)
                {
                    Socket.lastException = error;
                }
                if (!isSuccess) callback(async);
            }

            /// <summary>
            ///     异步回调
            /// </summary>
            /// <param name="count">接收数据长度</param>
            /// <param name="async">异步回调参数</param>
            private void callback(int count, SocketAsyncEventArgs async)
            {
                Socket = null;
                if (async == null) push(this, count);
                else
                {
                    try
                    {
                        async.Completed -= asyncCallback;
                        TmphSocketAsyncEventArgs.Push(ref async);
                    }
                    finally
                    {
                        push(this, count);
                    }
                }
            }

            /// <summary>
            ///     异步回调
            /// </summary>
            /// <param name="async">异步回调参数</param>
            private void callback(SocketAsyncEventArgs async)
            {
                try
                {
                    Socket.error();
                }
                finally
                {
                    callback(-1, async);
                }
            }
        }

        /// <summary>
        ///     数据接收器
        /// </summary>
        private sealed class TmphReceiver : TmphCallbackActionPool<TmphReceiver, bool>
        {
            /// <summary>
            ///     数据接收完成后的回调委托
            /// </summary>
            private readonly EventHandler<SocketAsyncEventArgs> asyncCallback;

            /// <summary>
            ///     异步套接字
            /// </summary>
            public TmphSocket Socket;

            /// <summary>
            ///     超时时间
            /// </summary>
            public DateTime Timeout;

            /// <summary>
            ///     数据接收器
            /// </summary>
            public TmphReceiver()
            {
                asyncCallback = onReceive;
            }

            /// <summary>
            ///     接收数据
            /// </summary>
            public void Receive()
            {
                SocketAsyncEventArgs async = null;
                try
                {
                    async = TmphSocketAsyncEventArgs.Get();
                    async.UserToken = this;
                    async.Completed += asyncCallback;
                    if (receive(async)) return;
                }
                catch (Exception error)
                {
                    Socket.lastException = error;
                }
                callback(async);
            }

            /// <summary>
            ///     继续接收数据
            /// </summary>
            /// <param name="async">异步套接字操作</param>
            /// <returns>是否接收成功</returns>
            private bool receive(SocketAsyncEventArgs async)
            {
                async.SetBuffer(Socket.currentReceiveData, Socket.currentReceiveStartIndex,
                    Socket.currentReceiveEndIndex - Socket.currentReceiveStartIndex);
                if (Socket.Socket.ReceiveAsync(async)) return true;
                if (async.SocketError != SocketError.Success) Socket.socketError = async.SocketError;
                return false;
            }

            /// <summary>
            ///     数据接收完成后的回调委托
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="async">异步回调参数</param>
            private void onReceive(object sender, SocketAsyncEventArgs async)
            {
                var isSuccess = false;
                if (TmphDate.NowSecond <= Timeout)
                {
                    try
                    {
                        if (async.SocketError == SocketError.Success)
                        {
                            var count = async.BytesTransferred;
                            if (count != 0)
                            {
                                Socket.currentReceiveStartIndex += count;
                                if (Socket.currentReceiveStartIndex == Socket.currentReceiveEndIndex)
                                {
                                    isSuccess = true;
                                    callback(true, async);
                                    return;
                                }
                                if (receive(async)) return;
                            }
                        }
                        else Socket.socketError = async.SocketError;
                    }
                    catch (Exception error)
                    {
                        Socket.lastException = error;
                    }
                }
                if (!isSuccess) callback(async);
            }

            /// <summary>
            ///     异步回调
            /// </summary>
            /// <param name="isReceive">数据接收是否成功</param>
            /// <param name="async">异步回调参数</param>
            private void callback(bool isReceive, SocketAsyncEventArgs async)
            {
                Socket = null;
                if (async == null) push(this, isReceive);
                else
                {
                    try
                    {
                        async.Completed -= asyncCallback;
                        TmphSocketAsyncEventArgs.Push(ref async);
                    }
                    finally
                    {
                        push(this, isReceive);
                    }
                }
            }

            /// <summary>
            ///     异步回调
            /// </summary>
            /// <param name="async">异步回调参数</param>
            private void callback(SocketAsyncEventArgs async)
            {
                try
                {
                    Socket.error();
                }
                finally
                {
                    callback(false, async);
                }
            }
        }
    }

    /// <summary>
    ///     套接字扩展
    /// </summary>
    public static class TmphSocketExpand
    {
        /// <summary>
        ///     关闭套接字
        /// </summary>
        /// <param name="socket">套接字</param>
        public static void shutdown(this Socket socket)
        {
            if (socket != null)
            {
                try
                {
                    socket.Shutdown(SocketShutdown.Both);
                }
                catch
                {
                }
                finally
                {
                    socket.Close();
                }
            }
        }

        /// <summary>
        ///     发送数据
        /// </summary>
        /// <param name="socket">套接字</param>
        /// <param name="data">数据</param>
        /// <param name="error">套接字错误</param>
        /// <returns>是否成功</returns>
        public static bool send(this Socket socket, TmphSubArray<byte> data, ref SocketError error)
        {
            return send(socket, data.array, data.StartIndex, data.Count, ref error);
        }

        /// <summary>
        ///     发送数据
        /// </summary>
        /// <param name="socket">套接字</param>
        /// <param name="data">数据</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="length">发送长度</param>
        /// <param name="error">套接字错误</param>
        /// <returns>是否成功</returns>
        public static bool send(this Socket socket, byte[] data, int startIndex, int length, ref SocketError error)
        {
            if (socket != null)
            {
                while (length > 0)
                {
                    var count = socket.Send(data, startIndex, length, SocketFlags.None, out error);
                    if (count <= 0 || error != SocketError.Success) break;
                    if ((length -= count) == 0) return true;
                    startIndex += count;
                }
            }
            return false;
        }

        /// <summary>
        ///     服务器端发送数据(限制单次发送数据量)
        /// </summary>
        /// <param name="socket">套接字</param>
        /// <param name="data">数据</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="length">发送长度</param>
        /// <param name="error">套接字错误</param>
        /// <returns>是否成功</returns>
        internal static bool serverSend(this Socket socket, byte[] data, int startIndex, int length,
            ref SocketError error)
        {
            var maxSendSize = TmphSocket.MaxServerSendSize;
            while (length > maxSendSize)
            {
                var count = socket.Send(data, startIndex, maxSendSize, SocketFlags.None, out error);
                if (count <= 0 || error != SocketError.Success) return false;
                length -= count;
                startIndex += count;
            }
            while (length > 0)
            {
                //Thread.Sleep(0);
                var count = socket.Send(data, startIndex, length, SocketFlags.None, out error);
                if (count <= 0 || error != SocketError.Success) break;
                if ((length -= count) == 0) return true;
                startIndex += count;
            }
            return false;
        }
    }
}