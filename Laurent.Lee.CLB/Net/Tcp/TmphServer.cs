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
using Laurent.Lee.CLB.Threading;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Laurent.Lee.CLB.Net.Tcp
{
    /// <summary>
    ///     TCP调用服务端基类
    /// </summary>
    public abstract class TmphServer : IDisposable
    {
        /// <summary>
        ///     配置信息
        /// </summary>
        protected internal TmphTcpServer attribute;

        /// <summary>
        ///     是否正在处理客户端集合
        /// </summary>
        private int isNewClientThread;

        /// <summary>
        ///     是否已启动服务
        /// </summary>
        protected int isStart;

        /// <summary>
        ///     待处理的客户端数量
        /// </summary>
        private int newClientCount;

        /// <summary>
        ///     待处理的客户端集合访问锁
        /// </summary>
        private int newClientLock;

        /// <summary>
        ///     待处理的客户端集合
        /// </summary>
        private Socket[] newClients;

        /// <summary>
        ///     TCP监听服务器端套接字
        /// </summary>
        private Socket socket;

        /// <summary>
        ///     TCP注册服务 客户端
        /// </summary>
        private TmphTcpRegister.TmphClient tcpRegisterClient;

        /// <summary>
        ///     处理待处理客户端请求
        /// </summary>
        private Action waitSocketHandle;

        /// <summary>
        ///     TCP调用服务端
        /// </summary>
        /// <param name="attribute">配置信息</param>
        protected TmphServer(TmphTcpServer attribute)
        {
            if (attribute == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            if (attribute.TcpRegisterName != null)
            {
                tcpRegisterClient = TmphTcpRegister.TmphClient.Get(attribute.TcpRegisterName);
                if (tcpRegisterClient == null)
                    TmphLog.Error.Throw("TCP注册服务 " + attribute.TcpRegisterName + " 链接失败", true, false);
                var state = tcpRegisterClient.Register(attribute);
                if (state != TmphTcpRegister.TmphRegisterState.Success)
                    TmphLog.Error.Throw("TCP服务注册 " + attribute.ServiceName + " 失败 " + state, true, false);
                TmphLog.Default.Add(attribute.ServiceName + " 注册 " + attribute.Host + ":" + attribute.Port.toString(),
                    false, false);
            }
            if (!attribute.IsServer) TmphLog.Default.Add("配置未指明的TCP服务端 " + attribute.ServiceName, true, false);
            this.attribute = attribute;
        }

        /// <summary>
        ///     服务名称
        /// </summary>
        public string ServiceName
        {
            get { return attribute.ServiceName; }
        }

        /// <summary>
        ///     是否已启动服务
        /// </summary>
        public bool IsStart
        {
            get { return isStart != 0; }
        }

        /// <summary>
        ///     停止服务
        /// </summary>
        public virtual void Dispose()
        {
            if (Interlocked.CompareExchange(ref isStart, 0, 1) == 1)
            {
                TmphLog.Default.Add(
                    "停止服务 " + attribute.ServiceName + "[" + attribute.Host + ":" + attribute.Port.toString() + "]", true,
                    false);
                TmphDomainUnload.Remove(Dispose, false);
                if (tcpRegisterClient != null)
                {
                    tcpRegisterClient.RemoveRegister(attribute);
                    tcpRegisterClient = null;
                }
                TmphPub.Dispose(ref this.socket);
                Socket[] sockets = null;
                TmphInterlocked.NoCheckCompareSetSleep0(ref newClientLock);
                if (newClients == null || newClientCount == 0) newClientLock = 0;
                else
                {
                    try
                    {
                        Array.Copy(newClients, sockets = new Socket[newClientCount], newClientCount);
                        Array.Clear(newClients, 0, newClientCount);
                        newClientCount = 0;
                    }
                    finally
                    {
                        newClientLock = 0;
                    }
                    if (sockets != null)
                    {
                        foreach (var socket in sockets) socket.shutdown();
                    }
                }
                if (OnDisposed != null) OnDisposed();
            }
        }

        /// <summary>
        ///     停止服务事件
        /// </summary>
        public event Action OnDisposed;

        /// <summary>
        ///     启动服务
        /// </summary>
        /// <returns>是否成功</returns>
        protected bool start()
        {
            if (Interlocked.CompareExchange(ref isStart, 1, 0) == 0)
            {
                try
                {
                    socket = new Socket(attribute.IpAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    socket.Bind(new IPEndPoint(attribute.IpAddress, attribute.Port));
                    socket.Listen(int.MaxValue);
                    newClients = new Socket[16];
                    waitSocketHandle = waitSocket;
                }
                catch (Exception error)
                {
                    Dispose();
                    TmphLog.Error.ThrowReal(error,
                        GetType().FullName + "服务器端口 " + attribute.Host + ":" + attribute.Port.toString() + " TCP连接失败)",
                        false);
                }
                return isStart != 0;
            }
            return false;
        }

        /// <summary>
        ///     启动服务
        /// </summary>
        /// <returns>是否成功</returns>
        public bool Start()
        {
            if (start())
            {
                TmphThreadPool.TinyPool.FastStart(getSocket, null, null);
                Thread.Sleep(0);
                TmphDomainUnload.Add(Dispose);
                return true;
            }
            return false;
        }

        /// <summary>
        ///     获取客户端请求
        /// </summary>
        protected abstract void getSocket();

        /// <summary>
        ///     获取客户端请求
        /// </summary>
        protected void acceptSocket()
        {
            while (isStart != 0)
            {
                try
                {
                    while (isStart != 0)
                    {
                        var socket = this.socket.Accept();
                        TmphInterlocked.NoCheckCompareSetSleep0(ref newClientLock);
                        var isNewClientThread = this.isNewClientThread;
                        if (newClientCount == this.newClients.Length)
                        {
                            try
                            {
                                var newClients = new Socket[newClientCount << 1];
                                this.newClients.CopyTo(newClients, 0);
                                newClients[newClientCount] = socket;
                                this.newClients = newClients;
                                this.isNewClientThread = 1;
                                ++newClientCount;
                            }
                            finally
                            {
                                newClientLock = 0;
                            }
                        }
                        else
                        {
                            newClients[newClientCount] = socket;
                            this.isNewClientThread = 1;
                            ++newClientCount;
                            newClientLock = 0;
                        }
                        if (isNewClientThread == 0) TmphThreadPool.TinyPool.FastStart(waitSocketHandle, null, null);
                    }
                }
                catch (Exception error)
                {
                    if (isStart != 0)
                    {
                        TmphLog.Error.Add(error, null, false);
                        Thread.Sleep(1);
                    }
                }
            }
        }

        /// <summary>
        ///     处理待处理客户端请求
        /// </summary>
        private void waitSocket()
        {
            while (isStart != 0)
            {
                TmphInterlocked.NoCheckCompareSetSleep0(ref newClientLock);
                if (newClientCount == 0)
                {
                    isNewClientThread = 0;
                    newClientLock = 0;
                    return;
                }
                var socket = newClients[--newClientCount];
                newClientLock = 0;
                newSocket(socket);
            }
        }

        /// <summary>
        ///     客户端请求处理
        /// </summary>
        /// <param name="socket">客户端套接字</param>
        protected abstract void newSocket(Socket socket);
    }
}