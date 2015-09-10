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

using Laurent.Lee.CLB.Code.CSharp;
using Laurent.Lee.CLB.Config;
using Laurent.Lee.CLB.Emit;
using Laurent.Lee.CLB.IO.Compression;
using Laurent.Lee.CLB.Threading;
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace Laurent.Lee.CLB.Net.Tcp
{
    /// <summary>
    ///     TCP调用客户端
    /// </summary>
    public class TmphCommandClient : IDisposable
    {
        /// <summary>
        ///     TCP参数流集合访问锁
        /// </summary>
        private readonly object tcpStreamCreateLock = new object();

        /// <summary>
        ///     验证接口
        /// </summary>
        private readonly TmphTcpBase.ITcpClientVerify verify;

        /// <summary>
        ///     验证函数接口
        /// </summary>
        private readonly TmphTcpBase.ITcpClientVerifyMethod verifyMethod;

        /// <summary>
        ///     是否释放资源
        /// </summary>
        private int isDisposed;

        /// <summary>
        ///     TCP客户端命令流处理套接字
        /// </summary>
        private TmphStreamCommandSocket streamSocket;

        /// <summary>
        ///     TCP客户端命令流处理套接字访问锁
        /// </summary>
        private int streamSocketLock;

        /// <summary>
        ///     TCP客户端流命令处理客户端验证函数调用访问锁
        /// </summary>
        protected int streamVerifyMethodLock;

        /// <summary>
        ///     TCP注册服务 客户端
        /// </summary>
        private TmphTcpRegister.TmphClient tcpRegisterClient;

        /// <summary>
        ///     TCP服务信息集合
        /// </summary>
        internal TmphTcpRegister.TmphServices TcpRegisterServices;

        /// <summary>
        ///     TCP服务信息集合版本
        /// </summary>
        internal int TcpRegisterServicesVersion;

        /// <summary>
        ///     TCP参数流集合访问锁
        /// </summary>
        private int tcpStreamLock;

        /// <summary>
        ///     TCP参数流集合
        /// </summary>
        private TmphTcpStream[] tcpStreams;

        /// <summary>
        ///     当前验证函数调用客户端套接字类型访问锁
        /// </summary>
        protected int verifyMethodLock;

        /// <summary>
        ///     TCP调用服务端
        /// </summary>
        /// <param name="attribute">配置信息</param>
        /// <param name="maxCommandLength">最大命令长度</param>
        public unsafe TmphCommandClient(TmphTcpServer attribute, int maxCommandLength)
        {
            Attribute = attribute;
            if (attribute.SendBufferSize <= (sizeof(int) * 2 + sizeof(TmphCommandServer.TmphStreamIdentity)))
                attribute.SendBufferSize = Math.Max(sizeof(int) * 2 + sizeof(TmphCommandServer.TmphStreamIdentity),
                    TmphAppSetting.StreamBufferSize);
            if (attribute.ReceiveBufferSize <=
                maxCommandLength + (sizeof(int) * 3 + sizeof(TmphCommandServer.TmphStreamIdentity)))
                attribute.ReceiveBufferSize =
                    Math.Max(maxCommandLength + (sizeof(int) * 3 + sizeof(TmphCommandServer.TmphStreamIdentity)),
                        TmphAppSetting.StreamBufferSize);
            if (attribute.TcpRegisterName == null) TcpRegisterServices = TmphTcpRegister.TmphServices.Null;
            else
            {
                tcpRegisterClient = TmphTcpRegister.TmphClient.Get(attribute.TcpRegisterName);
                tcpRegisterClient.Register(this);
            }
        }

        /// <summary>
        ///     TCP调用服务端
        /// </summary>
        /// <param name="attribute">配置信息</param>
        /// <param name="isStart">是否启动客户端</param>
        /// <param name="maxCommandLength">最大命令长度</param>
        /// <param name="verify">验证接口</param>
        public TmphCommandClient(TmphTcpServer attribute, int maxCommandLength, TmphTcpBase.ITcpClientVerify verify)
            : this(attribute, maxCommandLength)
        {
            this.verify = verify;
        }

        /// <summary>
        ///     TCP调用服务端
        /// </summary>
        /// <param name="attribute">配置信息</param>
        /// <param name="isStart">是否启动客户端</param>
        /// <param name="maxCommandLength">最大命令长度</param>
        /// <param name="verifyMethod">验证函数接口</param>
        public TmphCommandClient(TmphTcpServer attribute, int maxCommandLength,
            TmphTcpBase.ITcpClientVerifyMethod verifyMethod)
            : this(attribute, maxCommandLength)
        {
            this.verifyMethod = verifyMethod;
        }

        /// <summary>
        ///     TCP调用服务端
        /// </summary>
        /// <param name="attribute">配置信息</param>
        /// <param name="isStart">是否启动客户端</param>
        /// <param name="maxCommandLength">最大命令长度</param>
        /// <param name="verifyMethod">验证函数接口</param>
        public TmphCommandClient(TmphTcpServer attribute, int maxCommandLength,
            TmphTcpBase.ITcpClientVerifyMethod verifyMethod, TmphTcpBase.ITcpClientVerify verify)
            : this(attribute, maxCommandLength)
        {
            this.verify = verify;
            this.verifyMethod = verifyMethod;
        }

        /// <summary>
        ///     配置信息
        /// </summary>
        public TmphTcpServer Attribute { get; private set; }

        /// <summary>
        ///     TCP客户端命令流处理套接字
        /// </summary>
        public TmphStreamCommandSocket StreamSocket
        {
            get
            {
                if (TcpRegisterServices.Version != TcpRegisterServicesVersion)
                {
                    var socket = this.streamSocket;
                    if (tcpRegisterClient.GetHost(this) && socket != null)
                    {
                        TmphInterlocked.CompareSetSleep1(ref streamSocketLock);
                        if (this.streamSocket == socket) this.streamSocket = null;
                        streamSocketLock = 0;
                        TmphLog.Default.Add("TCP服务更新，关闭客户端 " + Attribute.ServiceName, false, false);
                        TmphPub.Dispose(ref socket);
                    }
                }
                var streamSocket = this.streamSocket;
                if ((streamSocket == null || streamSocket.IsDisposed) && Attribute.Port != 0)
                {
                    var isCreate = false;
                    TmphInterlocked.CompareSetSleep1(ref streamSocketLock);
                    try
                    {
                        if (isDisposed == 0)
                        {
                            if (this.streamSocket == null || this.streamSocket.IsDisposed)
                            {
                                this.streamSocket = TmphStreamCommandSocket.Create(this);
                                if (this.streamSocket != null)
                                {
                                    if (isDisposed == 0)
                                    {
                                        isCreate = true;
                                        while (streamVerifyMethodLock != 0) Thread.Sleep(0);
                                        streamVerifyMethodLock = 1;
                                    }
                                    else this.streamSocket.Dispose();
                                }
                            }
                            streamSocket = this.streamSocket;
                        }
                    }
                    finally
                    {
                        streamSocketLock = 0;
                    }
                    if (isCreate)
                    {
                        try
                        {
                            streamSocket.Receive();
                            if (callVerifyMethod()) streamSocket.SetCheck();
                        }
                        finally
                        {
                            streamVerifyMethodLock = 0;
                        }
                    }
                }
                if (streamSocket != null)
                {
                    while (streamVerifyMethodLock != 0) Thread.Sleep(1);
                    if (streamSocket.IsVerifyMethod) return streamSocket;
                }
                return null;
            }
        }

        /// <summary>
        ///     验证函数TCP客户端命令流处理套接字
        /// </summary>
        public TmphStreamCommandSocket VerifyStreamSocket
        {
            get { return streamSocket; }
        }

        /// <summary>
        ///     是否释放资源
        /// </summary>
        public bool IsDisposed
        {
            get { return isDisposed != 0; }
        }

        /// <summary>
        ///     服务器端负载均衡联通测试时间
        /// </summary>
        public DateTime LoadBalancingCheckTime { get; private set; }

        /// <summary>
        ///     服务名称
        /// </summary>
        internal string ServiceName
        {
            get { return Attribute.ServiceName; }
        }

        /// <summary>
        ///     停止客户端链接
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.Increment(ref isDisposed) == 1)
            {
                TmphLog.Default.Add("关闭TCP客户端 " + Attribute.ServiceName, true, TmphLog.TmphCacheType.Last);
                if (tcpRegisterClient != null)
                {
                    tcpRegisterClient.Remove(this);
                    tcpRegisterClient = null;
                }
                TmphPub.Dispose(ref streamSocket);
                var streams = TmphNullValue<Stream>.Array;
                TmphInterlocked.NoCheckCompareSetSleep0(ref tcpStreamLock);
                try
                {
                    streams = new Stream[tcpStreams.length()];
                    for (var index = streams.Length; index != 0;)
                    {
                        --index;
                        streams[index] = tcpStreams[index].Cancel();
                    }
                }
                finally
                {
                    tcpStreamLock = 0;
                }
                foreach (var stream in streams) TmphPub.Dispose(stream);
            }
        }

        /// <summary>
        ///     函数验证
        /// </summary>
        /// <returns>是否验证成功</returns>
        protected virtual bool callVerifyMethod()
        {
            if (verifyMethod == null) return true;
            var isError = false;
            TmphInterlocked.CompareSetSleep1(ref verifyMethodLock);
            try
            {
                if (verifyMethod.Verify()) return true;
            }
            catch (Exception error)
            {
                isError = true;
                TmphLog.Error.Add(error, "TCP客户端验证失败", false);
            }
            finally
            {
                verifyMethodLock = 0;
            }
            if (!isError) TmphLog.Error.Add("TCP客户端验证失败", true, false);
            Dispose();
            return false;
        }

        /// <summary>
        ///     获取TCP参数流
        /// </summary>
        /// <param name="stream">字节流</param>
        /// <returns>TCP参数流</returns>
        public TmphTcpBase.TmphTcpStream GetTcpStream(Stream stream)
        {
            if (stream != null)
            {
                try
                {
                    var tcpStream = new TmphTcpBase.TmphTcpStream
                    {
                        CanRead = stream.CanRead,
                        CanWrite = stream.CanWrite,
                        CanSeek = stream.CanSeek,
                        CanTimeout = stream.CanTimeout
                    };
                    START:
                    TmphInterlocked.NoCheckCompareSetSleep0(ref tcpStreamLock);
                    if (tcpStreams == null)
                    {
                        try
                        {
                            tcpStreams = new TmphTcpStream[4];
                            tcpStream.ClientIndex = tcpStream.ClientIdentity = 0;
                            tcpStreams[0].Stream = stream;
                        }
                        finally
                        {
                            tcpStreamLock = 0;
                        }
                    }
                    else
                    {
                        foreach (var value in tcpStreams)
                        {
                            if (value.Stream == null)
                            {
                                tcpStream.ClientIdentity = tcpStreams[tcpStream.ClientIndex].Set(stream);
                                tcpStreamLock = 0;
                                break;
                            }
                            ++tcpStream.ClientIndex;
                        }
                        if (tcpStream.ClientIndex == tcpStreams.Length)
                        {
                            tcpStreamLock = 0;
                            Monitor.Enter(tcpStreamCreateLock);
                            if (tcpStream.ClientIndex == tcpStreams.Length)
                            {
                                try
                                {
                                    var newTcpStreams = new TmphTcpStream[tcpStream.ClientIndex << 1];
                                    TmphInterlocked.NoCheckCompareSetSleep0(ref tcpStreamLock);
                                    tcpStreams.CopyTo(newTcpStreams, 0);
                                    tcpStreams = newTcpStreams;
                                    tcpStream.ClientIdentity = tcpStreams[tcpStream.ClientIndex].Set(stream);
                                    tcpStreamLock = 0;
                                }
                                finally
                                {
                                    Monitor.Exit(tcpStreamCreateLock);
                                }
                            }
                            else
                            {
                                Monitor.Exit(tcpStreamCreateLock);
                                tcpStream.ClientIndex = 0;
                                goto START;
                            }
                        }
                    }
                    stream = null;
                    tcpStream.IsStream = true;
                    return tcpStream;
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
                finally
                {
                    TmphPub.Dispose(ref stream);
                }
            }
            return default(TmphTcpBase.TmphTcpStream);
        }

        /// <summary>
        ///     获取TCP参数流
        /// </summary>
        /// <param name="index">TCP参数流索引</param>
        /// <param name="identity">TCP参数流序号</param>
        /// <returns>TCP参数流</returns>
        private Stream getTcpStream(int index, int identity)
        {
            Stream stream;
            TmphInterlocked.NoCheckCompareSetSleep0(ref tcpStreamLock);
            try
            {
                stream = tcpStreams[index].Get(identity);
            }
            finally
            {
                tcpStreamLock = 0;
            }
            return stream;
        }

        /// <summary>
        ///     关闭TCP参数流
        /// </summary>
        /// <param name="index">TCP参数流索引</param>
        /// <param name="identity">TCP参数流序号</param>
        private void closeTcpStream(int index, int identity)
        {
            TmphInterlocked.NoCheckCompareSetSleep0(ref tcpStreamLock);
            try
            {
                tcpStreams[index].Close(identity);
            }
            finally
            {
                tcpStreamLock = 0;
            }
        }

        /// <summary>
        ///     忽略TCP调用分组
        /// </summary>
        /// <param name="groupId">分组标识</param>
        /// <returns>是否调用成功</returns>
        public bool IgnoreGroup(int groupId)
        {
            try
            {
                var socket = StreamSocket;
                if (socket != null) return socket.IgnoreGroup(groupId);
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, false);
            }
            return false;
        }

        /// <summary>
        ///     负载均衡超时检测
        /// </summary>
        /// <returns>客户端是否可用</returns>
        public bool LoadBalancingCheck()
        {
            var socket = StreamSocket;
            return socket != null && socket.LoadBalancingCheck();
        }

        /// <summary>
        ///     TCP客户端套接字
        /// </summary>
        public abstract class TmphSocket : TmphCommandSocket<TmphCommandClient>
        {
            /// <summary>
            ///     检测时间周期
            /// </summary>
            protected readonly long checkTimeTicks;

            /// <summary>
            ///     配置信息
            /// </summary>
            protected TmphTcpServer attribute;

            /// <summary>
            ///     连接检测设置
            /// </summary>
            protected Action checkHandle;

            /// <summary>
            ///     最后一次检测时间
            /// </summary>
            protected DateTime lastCheckTime;

            /// <summary>
            ///     TCP客户端套接字
            /// </summary>
            /// <param name="commandClient">TCP调用客户端</param>
            /// <param name="TmphClient">TCP调用客户端</param>
            /// <param name="sendData">接收数据缓冲区</param>
            /// <param name="receiveData">发送数据缓冲区</param>
            protected TmphSocket(TmphCommandClient commandClient, Socket TmphClient, byte[] sendData, byte[] receiveData)
                : base(TmphClient, sendData, receiveData, commandClient, true)
            {
                attribute = commandClient.Attribute;
                if (attribute.ClientCheckSeconds > 0)
                    checkTimeTicks = new TimeSpan(0, 0, attribute.ClientCheckSeconds).Ticks;
            }

            /// <summary>
            ///     TCP套接字添加到池
            /// </summary>
            internal override void PushPool()
            {
                TmphLog.Error.Throw(TmphLog.TmphExceptionType.ErrorOperation);
            }

            /// <summary>
            ///     连接检测设置
            /// </summary>
            internal void SetCheck()
            {
                IsVerifyMethod = true;
                if (commandSocketProxy.Attribute.IsLoadBalancing) loadBalancingCheck();
                else if (checkTimeTicks != 0)
                {
                    lastCheckTime = TmphDate.NowSecond;
                    checkHandle();
                }
            }

            /// <summary>
            ///     负载均衡连接检测
            /// </summary>
            protected abstract void loadBalancingCheck();

            /// <summary>
            ///     客户端验证
            /// </summary>
            /// <returns>是否验证成功</returns>
            protected unsafe bool verify()
            {
                fixed (byte* dataFixed = receiveData)
                    *(int*)dataFixed = attribute.IsIdentityCommand
                        ? TmphCommandServer.IdentityVerifyIdentity
                        : TmphCommandServer.VerifyIdentity;
                if (send(receiveData, 0, sizeof(int)) &&
                    (commandSocketProxy.verify == null || commandSocketProxy.verify.Verify(this)))
                {
                    identity.Identity = attribute.IsIdentityCommand
                        ? TmphCommandServer.IdentityVerifyIdentity
                        : TmphCommandServer.VerifyIdentity;
                    if (IsSuccess()) return true;
                }
                TmphLog.Error.Add(null, "TCP客户端验证失败", false);
                Dispose();
                return false;
            }

            /// <summary>
            ///     设置会话标识
            /// </summary>
            protected internal void setIdentity()
            {
                identity.Identity = ((int)TmphPub.Identity ^ (int)TmphPub.StartTime.Ticks) & int.MaxValue;
                if (identity.Identity == 0) identity.Identity = int.MaxValue;
            }

            /// <summary>
            ///     判断操作状态是否成功
            /// </summary>
            /// <returns>操作状态是否成功</returns>
            public unsafe bool IsSuccess()
            {
                if (tryReceive(0, sizeof(int), DateTime.MaxValue) == sizeof(int))
                {
                    fixed (byte* dataFixed = receiveData)
                    {
                        if (*(int*)dataFixed == identity.Identity) return true;
                    }
                }
                Dispose();
                return false;
            }
        }

        /// <summary>
        ///     TCP客户端命令流处理套接字
        /// </summary>
        public sealed unsafe class TmphStreamCommandSocket : TmphSocket
        {
            /// <summary>
            ///     关闭连接命令数据
            /// </summary>
            private static readonly byte[] closeCommandData;

            /// <summary>
            ///     关闭连接命令数据
            /// </summary>
            private static readonly byte[] closeIdentityCommandData;

            /// <summary>
            ///     连接检测命令数据
            /// </summary>
            private static readonly byte[] checkCommandData;

            /// <summary>
            ///     负载均衡连接检测命令数据
            /// </summary>
            private static readonly byte[] loadBalancingCheckCommandData;

            /// <summary>
            ///     TCP流回应命令数据
            /// </summary>
            private static readonly byte[] tcpStreamCommandData;

            /// <summary>
            ///     忽略分组命令数据
            /// </summary>
            private static readonly byte[] ignoreGroupCommandData;

            /// <summary>
            ///     创建命令输入数据并执行
            /// </summary>
            private readonly Action buildCommandHandle;

            /// <summary>
            ///     命令信息空闲索引集合
            /// </summary>
            private readonly TmphList<int> freeIndexs = new TmphList<int>();

            /// <summary>
            ///     接收会话标识
            /// </summary>
            private readonly Action<int> onReceiveIdentityHandle;

            /// <summary>
            ///     已经创建命令集合
            /// </summary>
            private TmphCommandQueue buildCommands;

            /// <summary>
            ///     命令索引信息集合访问锁
            /// </summary>
            private int commandIndexLock;

            /// <summary>
            ///     命令索引信息集合
            /// </summary>
            private TmphCommandIndex[] commandIndexs;

            /// <summary>
            ///     命令集合访问锁
            /// </summary>
            private int commandLock;

            /// <summary>
            ///     命令队列集合
            /// </summary>
            private TmphCommandQueue commands;

            /// <summary>
            ///     当前接收会话标识
            /// </summary>
            private TmphCommandServer.TmphStreamIdentity currentIdentity;

            /// <summary>
            ///     是否可以添加命令
            /// </summary>
            private byte disabledCommand;

            /// <summary>
            ///     是否正在创建命令
            /// </summary>
            private byte isBuildCommand;

            /// <summary>
            ///     是否正在创建命令
            /// </summary>
            private int isCommandBuilding;

            /// <summary>
            ///     命令信息集合最大索引号
            /// </summary>
            private int maxIndex;

            /// <summary>
            ///     获取压缩数据
            /// </summary>
            private Action<bool> receiveCompressHandle;

            /// <summary>
            ///     接收数据起始位置
            /// </summary>
            private byte* receiveDataFixed;

            /// <summary>
            ///     接收数据处理递归深度
            /// </summary>
            private int receiveDepth;

            /// <summary>
            ///     接收数据结束位置
            /// </summary>
            private int receiveEndIndex;

            /// <summary>
            ///     接收服务器端数据
            /// </summary>
            private Action receiveHandle;

            /// <summary>
            ///     接收会话标识
            /// </summary>
            private Action<int> receiveIdentityHandle;

            /// <summary>
            ///     获取非压缩数据
            /// </summary>
            private Action<bool> receiveNoCompressHandle;

            static TmphStreamCommandSocket()
            {
                closeIdentityCommandData = new byte[(sizeof(int) * 2 + sizeof(TmphCommandServer.TmphStreamIdentity))];
                fixed (byte* commandFixed = closeIdentityCommandData)
                {
                    *(int*)(commandFixed) = TmphCommandServer.CloseIdentityCommand;
                    *(TmphCommandServer.TmphStreamIdentity*)(commandFixed + sizeof(int)) =
                        new TmphCommandServer.TmphStreamIdentity { Index = 0, Identity = int.MinValue };
                    *(int*)(commandFixed + sizeof(int) + sizeof(TmphCommandServer.TmphStreamIdentity)) = 0;
                }
                closeCommandData = new byte[(sizeof(int) * 3 + sizeof(TmphCommandServer.TmphStreamIdentity))];
                fixed (byte* commandFixed = closeCommandData)
                {
                    *(int*)(commandFixed) = sizeof(int) * 3 + sizeof(TmphCommandServer.TmphStreamIdentity);
                    *(int*)(commandFixed + sizeof(int)) = TmphCommandServer.CloseIdentityCommand +
                                                            TmphCommandServer.CommandDataIndex;
                    *(TmphCommandServer.TmphStreamIdentity*)(commandFixed + sizeof(int) * 2) =
                        new TmphCommandServer.TmphStreamIdentity { Index = 0, Identity = int.MinValue };
                    *(int*)(commandFixed + sizeof(int) * 2 + sizeof(TmphCommandServer.TmphStreamIdentity)) = 0;
                }

                checkCommandData = new byte[sizeof(int) + sizeof(int)];
                fixed (byte* commandFixed = checkCommandData)
                {
                    *(int*)commandFixed = sizeof(int) * 3 + sizeof(TmphCommandServer.TmphStreamIdentity);
                    *(int*)(commandFixed + sizeof(int)) = TmphCommandServer.CheckIdentityCommand +
                                                            TmphCommandServer.CommandDataIndex;
                }
                loadBalancingCheckCommandData = new byte[sizeof(int) + sizeof(int)];
                fixed (byte* commandFixed = loadBalancingCheckCommandData)
                {
                    *(int*)commandFixed = sizeof(int) * 3 + sizeof(TmphCommandServer.TmphStreamIdentity);
                    *(int*)(commandFixed + sizeof(int)) = TmphCommandServer.LoadBalancingCheckIdentityCommand +
                                                            TmphCommandServer.CommandDataIndex;
                }
                tcpStreamCommandData = new byte[sizeof(int) + sizeof(int)];
                fixed (byte* commandFixed = tcpStreamCommandData)
                {
                    *(int*)commandFixed = sizeof(int) * 3 + sizeof(TmphCommandServer.TmphStreamIdentity);
                    *(int*)(commandFixed + sizeof(int)) = TmphCommandServer.TcpStreamCommand +
                                                            TmphCommandServer.CommandDataIndex;
                }
                ignoreGroupCommandData = new byte[sizeof(int) + sizeof(int)];
                fixed (byte* commandFixed = ignoreGroupCommandData)
                {
                    *(int*)commandFixed = sizeof(int) * 3 + sizeof(TmphCommandServer.TmphStreamIdentity);
                    *(int*)(commandFixed + sizeof(int)) = TmphCommandServer.IgnoreGroupCommand +
                                                            TmphCommandServer.CommandDataIndex;
                }
            }

            ///// <summary>
            ///// 是否输出调试信息
            ///// </summary>
            //private bool isOutputDebug;
            /// <summary>
            ///     TCP客户端套接字
            /// </summary>
            /// <param name="commandClient">TCP调用客户端</param>
            /// <param name="TmphClient">TCP调用客户端</param>
            /// <param name="sendData">接收数据缓冲区</param>
            /// <param name="receiveData">发送数据缓冲区</param>
            private TmphStreamCommandSocket(TmphCommandClient commandClient, Socket TmphClient, byte[] sendData,
                byte[] receiveData)
                : base(commandClient, TmphClient, sendData, receiveData)
            {
                commandIndexs = new TmphCommandIndex[255];
                maxIndex = 2;
                commandIndexs[0].Set(doTcpStream, 0, 1);
                commandIndexs[1].Set(loadBalancingCheckTime, 0, 1);
                onReceiveIdentityHandle = onReceiveIdentity;
                checkHandle = check;
                buildCommandHandle = buildCommand;
                //isOutputDebug = commandClient.attribute.IsOutputDebug;
            }

            /// <summary>
            ///     关闭套接字连接
            /// </summary>
            protected override void dispose()
            {
                TmphInterlocked.NoCheckCompareSetSleep0(ref commandLock);
                disabledCommand = 1;
                commands.Clear();
                commandLock = 0;
                try
                {
                    if (attribute.IsIdentityCommand) send(closeIdentityCommandData, 0, closeIdentityCommandData.Length);
                    else send(closeCommandData, 0, closeCommandData.Length);
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
                try
                {
                    Action<TmphMemoryPool.TmphPushSubArray>[] onReceives = null;
                    TmphInterlocked.NoCheckCompareSetSleep0(ref commandIndexLock);
                    try
                    {
                        var count = freeIndexs.Count;
                        if (count != 0)
                        {
                            foreach (var index in freeIndexs.array)
                            {
                                commandIndexs[index].Clear();
                                if (--count == 0) break;
                            }
                            freeIndexs.Empty();
                        }
                        if (maxIndex != 0)
                        {
                            onReceives = new Action<TmphMemoryPool.TmphPushSubArray>[maxIndex];
                            do
                            {
                                --maxIndex;
                                onReceives[maxIndex] = commandIndexs[maxIndex].Cancel().Key;
                            } while (maxIndex != 0);
                        }
                    }
                    finally
                    {
                        commandIndexLock = 0;
                    }
                    if (onReceives != null) TmphTask.Tiny.Add(cancelOnReceives, onReceives, null);
                    buildCommands.Clear();
                }
                finally
                {
                    base.dispose();
                }
            }

            /// <summary>
            ///     取消命令回调
            /// </summary>
            /// <param name="onReceives">命令回调集合</param>
            private static void cancelOnReceives(Action<TmphMemoryPool.TmphPushSubArray>[] onReceives)
            {
                foreach (var onReceive in onReceives)
                {
                    if (onReceive != null) onReceive(default(TmphMemoryPool.TmphPushSubArray));
                }
            }

            /// <summary>
            ///     连接检测设置
            /// </summary>
            private void check()
            {
                if (isDisposed == 0)
                {
                    var checkTime = lastCheckTime.AddTicks(checkTimeTicks);
                    if (checkTime <= TmphDate.NowSecond)
                    {
                        if (attribute.IsIdentityCommand) Call(null, TmphCommandServer.CheckIdentityCommand, false, false);
                        else Call(null, checkCommandData, false, false);
                        TmphTimerTask.Default.Add(checkHandle, (lastCheckTime = TmphDate.NowSecond).AddTicks(checkTimeTicks),
                            null);
                    }
                    else TmphTimerTask.Default.Add(checkHandle, checkTime, null);
                }
            }

            /// <summary>
            ///     负载均衡连接检测
            /// </summary>
            protected override void loadBalancingCheck()
            {
                if (attribute.IsIdentityCommand)
                    Call(null, TmphCommandServer.LoadBalancingCheckIdentityCommand, false, false);
                else Call(null, loadBalancingCheckCommandData, false, false);
            }

            /// <summary>
            ///     负载均衡连接检测
            /// </summary>
            /// <returns>是否成功</returns>
            internal bool LoadBalancingCheck()
            {
                var wait = TmphAsynchronousMethod.TmphWaitCall.Get();
                if (wait != null)
                {
                    if (attribute.IsIdentityCommand)
                        Call(wait.OnReturn, TmphCommandServer.LoadBalancingCheckIdentityCommand, false, false);
                    else Call(wait.OnReturn, loadBalancingCheckCommandData, false, false);
                    return wait.Value.IsReturn;
                }
                return false;
            }

            /// <summary>
            ///     忽略TCP调用分组
            /// </summary>
            /// <param name="groupId">分组标识</param>
            /// <returns>是否调用成功</returns>
            internal bool IgnoreGroup(int groupId)
            {
                var wait = TmphAsynchronousMethod.TmphWaitCall.Get();
                if (wait != null)
                {
                    if (attribute.IsIdentityCommand)
                        Call(wait.OnReturn, TmphCommandServer.IgnoreGroupCommand, groupId, int.MaxValue, false, false);
                    else Call(wait.OnReturn, ignoreGroupCommandData, groupId, int.MaxValue, false, false);
                    return wait.Value.IsReturn;
                }
                return false;
            }

            /// <summary>
            ///     获取命令信息集合索引
            /// </summary>
            /// <param name="onReceive">接收数据回调</param>
            /// <param name="isTask">回调是否使用任务池</param>
            /// <param name="isKeep">是否保持回调</param>
            /// <returns>命令信息集合索引</returns>
            private TmphCommandServer.TmphStreamIdentity newIndex(Action<TmphMemoryPool.TmphPushSubArray> onReceive, byte isTask,
                byte isKeep)
            {
                int index;
                TmphInterlocked.NoCheckCompareSetSleep0(ref commandIndexLock);
                if (freeIndexs.Count != 0)
                {
                    index = freeIndexs.Unsafer.Pop();
                    commandIndexLock = 0;
                }
                else if (maxIndex == commandIndexs.Length)
                {
                    try
                    {
                        var newCommands = new TmphCommandIndex[maxIndex << 1];
                        Array.Copy(commandIndexs, 0, newCommands, 0, maxIndex);
                        index = maxIndex++;
                        commandIndexs = newCommands;
                    }
                    finally
                    {
                        commandIndexLock = 0;
                    }
                }
                else
                {
                    index = maxIndex++;
                    commandIndexLock = 0;
                }
                commandIndexs[index].Set(onReceive, isTask, isKeep);
                return new TmphCommandServer.TmphStreamIdentity { Index = index, Identity = commandIndexs[index].Identity };
            }

            /// <summary>
            ///     释放命令信息集合索引
            /// </summary>
            /// <param name="index">命令信息集合索引</param>
            private void freeIndex(int index)
            {
                TmphInterlocked.NoCheckCompareSetSleep0(ref commandIndexLock);
                if ((uint)index < commandIndexs.Length)
                {
                    commandIndexs[index].Clear();
                    freeIndexLock(index);
                }
                else commandIndexLock = 0;
            }

            /// <summary>
            ///     释放命令信息集合索引
            /// </summary>
            /// <param name="index">命令信息集合索引</param>
            private void freeIndexLock(int index)
            {
                if (freeIndexs.Count == freeIndexs.array.Length)
                {
                    try
                    {
                        freeIndexs.Add(index);
                    }
                    finally
                    {
                        commandIndexLock = 0;
                    }
                }
                else
                {
                    freeIndexs.Unsafer.Add(index);
                    commandIndexLock = 0;
                }
            }

            /// <summary>
            ///     TCP流处理
            /// </summary>
            /// <param name="data">输出数据</param>
            private void doTcpStream(TmphMemoryPool.TmphPushSubArray data)
            {
                byte[] TmphBuffer = null;
                try
                {
                    var parameter = TmphDataDeSerializer.DeSerialize<TmphCommandServer.TmphTcpStreamParameter>(data.Value);
                    if (parameter != null)
                    {
                        var stream = commandSocketProxy.getTcpStream(parameter.ClientIndex, parameter.ClientIdentity);
                        if (stream != null)
                        {
                            parameter.IsClientStream = true;
                            try
                            {
                                switch (parameter.Command)
                                {
                                    case TmphCommandServer.TmphTcpStreamCommand.GetLength:
                                        parameter.Offset = stream.Length;
                                        break;

                                    case TmphCommandServer.TmphTcpStreamCommand.SetLength:
                                        stream.SetLength(parameter.Offset);
                                        break;

                                    case TmphCommandServer.TmphTcpStreamCommand.GetPosition:
                                        parameter.Offset = stream.Position;
                                        break;

                                    case TmphCommandServer.TmphTcpStreamCommand.SetPosition:
                                        stream.Position = parameter.Offset;
                                        break;

                                    case TmphCommandServer.TmphTcpStreamCommand.GetReadTimeout:
                                        parameter.Offset = stream.ReadTimeout;
                                        break;

                                    case TmphCommandServer.TmphTcpStreamCommand.SetReadTimeout:
                                        stream.ReadTimeout = (int)parameter.Offset;
                                        break;

                                    case TmphCommandServer.TmphTcpStreamCommand.GetWriteTimeout:
                                        parameter.Offset = stream.WriteTimeout;
                                        break;

                                    case TmphCommandServer.TmphTcpStreamCommand.SetWriteTimeout:
                                        stream.WriteTimeout = (int)parameter.Offset;
                                        break;

                                    case TmphCommandServer.TmphTcpStreamCommand.BeginRead:
                                        TmphBuffer = asyncBuffers.Get((int)parameter.Offset);
                                        var tcpStreamReader = TmphTcpStreamReader.Get(this, stream, parameter);
                                        if (tcpStreamReader != null)
                                        {
                                            parameter.Data.UnsafeSet(TmphBuffer, 0, 0);
                                            TmphBuffer = null;
                                            tcpStreamReader.Read();
                                        }
                                        return;

                                    case TmphCommandServer.TmphTcpStreamCommand.Read:
                                        parameter.Data.UnsafeSet(TmphBuffer = asyncBuffers.Get(), 0,
                                            stream.Read(TmphBuffer, 0, Math.Min(TmphBuffer.Length, (int)parameter.Offset)));
                                        TmphBuffer = null;
                                        break;

                                    case TmphCommandServer.TmphTcpStreamCommand.ReadByte:
                                        parameter.Offset = stream.ReadByte();
                                        break;

                                    case TmphCommandServer.TmphTcpStreamCommand.BeginWrite:
                                        var tcpStreamWriter = TmphTcpStreamWriter.Get(this, stream, parameter);
                                        if (tcpStreamWriter != null) tcpStreamWriter.Write();
                                        return;

                                    case TmphCommandServer.TmphTcpStreamCommand.Write:
                                        stream.Write(parameter.Data.Array, parameter.Data.StartIndex,
                                            parameter.Data.Count);
                                        parameter.Data.Null();
                                        break;

                                    case TmphCommandServer.TmphTcpStreamCommand.WriteByte:
                                        stream.WriteByte((byte)parameter.Offset);
                                        break;

                                    case TmphCommandServer.TmphTcpStreamCommand.Seek:
                                        parameter.Offset = stream.Seek(parameter.Offset, parameter.SeekOrigin);
                                        break;

                                    case TmphCommandServer.TmphTcpStreamCommand.Flush:
                                        stream.Flush();
                                        break;

                                    case TmphCommandServer.TmphTcpStreamCommand.Close:
                                        commandSocketProxy.closeTcpStream(parameter.ClientIndex,
                                            parameter.ClientIdentity);
                                        stream.Dispose();
                                        break;
                                }
                                parameter.IsCommand = true;
                            }
                            catch (Exception error)
                            {
                                TmphLog.Error.Add(error, null, false);
                            }
                        }
                        doTcpStream(parameter);
                    }
                }
                finally
                {
                    data.Push();
                    asyncBuffers.Push(ref TmphBuffer);
                }
            }

            /// <summary>
            ///     服务器端负载均衡联通测试
            /// </summary>
            /// <param name="data">输出数据</param>
            private void loadBalancingCheckTime(TmphMemoryPool.TmphPushSubArray data)
            {
                commandSocketProxy.LoadBalancingCheckTime = TmphDate.NowSecond;
            }

            /// <summary>
            ///     发送TCP流参数
            /// </summary>
            /// <param name="parameter">TCP流参数</param>
            private void doTcpStream(TmphCommandServer.TmphTcpStreamParameter parameter)
            {
                if (isDisposed == 0)
                {
                    if (attribute.IsIdentityCommand)
                        Call(parameter.PushClientBuffer, TmphCommandServer.TcpStreamCommand, parameter, int.MaxValue,
                            false, false);
                    else Call(parameter.PushClientBuffer, tcpStreamCommandData, parameter, int.MaxValue, false, false);
                }
            }

            /// <summary>
            ///     添加命令
            /// </summary>
            /// <param name="command">当前命令</param>
            private void pushCommand(TmphCommand command)
            {
                TmphInterlocked.NoCheckCompareSetSleep0(ref commandLock);
                if (disabledCommand == 0)
                {
                    var isBuildCommand = this.isBuildCommand;
                    commands.Push(command);
                    this.isBuildCommand = 1;
                    commandLock = 0;
                    if (isBuildCommand == 0) TmphThreadPool.TinyPool.FastStart(buildCommandHandle, null, null);
                }
                else
                {
                    commandLock = 0;
                    command.Cancel();
                }
            }

            /// <summary>
            ///     创建命令输入数据并执行
            /// </summary>
            private void buildCommand()
            {
                TmphInterlocked.NoCheckCompareSetSleep0(ref isCommandBuilding);
                int bufferSize = BigBuffers.Size, bufferSize2 = bufferSize >> 1;
                using (var commandStream = new TmphUnmanagedStream((byte*)&bufferSize, sizeof(int)))
                {
                    var commandBuilder = new TmphCommandBuilder
                    {
                        Socket = this,
                        CommandStream = commandStream,
                        MergeIndex =
                            attribute.IsIdentityCommand
                                ? (sizeof(int) * 2 + sizeof(TmphCommandServer.TmphStreamIdentity))
                                : (sizeof(int) * 3 + sizeof(TmphCommandServer.TmphStreamIdentity))
                    };
                    try
                    {
                        START:
                        var TmphBuffer = sendData;
                        fixed (byte* dataFixed = TmphBuffer)
                        {
                            commandBuilder.Reset(dataFixed, TmphBuffer.Length);
                            do
                            {
                                TmphInterlocked.NoCheckCompareSetSleep0(ref commandLock);
                                var command = commands.Pop();
                                if (command == null)
                                {
                                    if (buildCommands.Head == null)
                                    {
                                        isBuildCommand = 0;
                                        commandLock = 0;
                                        isCommandBuilding = 0;
                                        return;
                                    }
                                    commandLock = 0;
                                    commandBuilder.Send();
                                    if (sendData != TmphBuffer) goto START;
                                }
                                else
                                {
                                    commandLock = 0;
                                    commandBuilder.Build(command);
                                    if (commandStream.Length + commandBuilder.MaxCommandLength > bufferSize)
                                    {
                                        commandBuilder.Send();
                                        if (sendData != TmphBuffer) goto START;
                                    }
                                    if (commands.Head == null && commandStream.Length <= bufferSize2) Thread.Sleep(0);
                                }
                            } while (true);
                        }
                    }
                    catch (Exception error)
                    {
                        commandBuilder.Cancel();
                        buildCommands.Clear();
                        TmphInterlocked.NoCheckCompareSetSleep0(ref commandLock);
                        isBuildCommand = 0;
                        commandLock = 0;
                        isCommandBuilding = 0;
                        Socket.shutdown();
                        TmphLog.Error.Add(error, attribute.ServiceName, false);
                    }
                }
            }

            /// <summary>
            ///     TCP调用并返回参数值
            /// </summary>
            /// <typeparam name="TOutputParameterType">输出参数类型</typeparam>
            /// <param name="onGet">回调委托,返回null表示失败</param>
            /// <param name="commandData">TCP调用命令</param>
            /// <param name="outputParameter">输出参数</param>
            /// <param name="maxLength">输入参数数据最大长度</param>
            /// <param name="isTask">回调是否使用任务池</param>
            /// <param name="isKeepCallback">是否保持回调</param>
            /// <returns>保持回调</returns>
            public TmphKeepCallback Get<TOutputParameterType>
                (Action<TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType>> onGet, byte[] commandData
                    , TOutputParameterType outputParameter, bool isTask, bool isKeepCallback)
            {
                if (isDisposed == 0)
                {
                    var command = TmphAsyncOutputDataCommand<TOutputParameterType>.Get(this, onGet, commandData,
                        outputParameter, isTask, isKeepCallback);
                    if (command != null)
                    {
                        pushCommand(command);
                        return command.KeepCallback;
                    }
                }
                onGet(new TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType> { IsReturn = false });
                return null;
            }

            /// <summary>
            ///     TCP调用并返回参数值
            /// </summary>
            /// <typeparam name="TOutputParameterType">输出参数类型</typeparam>
            /// <param name="onGet">回调委托,返回null表示失败</param>
            /// <param name="commandData">TCP调用命令</param>
            /// <param name="outputParameter">输出参数</param>
            /// <param name="maxLength">输入参数数据最大长度</param>
            /// <param name="isTask">回调是否使用任务池</param>
            /// <param name="isKeepCallback">是否保持回调</param>
            /// <returns>保持回调</returns>
            public TmphKeepCallback GetJson<TOutputParameterType>
                (Action<TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType>> onGet
                    , byte[] commandData, TOutputParameterType outputParameter, bool isTask, bool isKeepCallback)
            {
                if (isDisposed == 0)
                {
                    var command = TmphAsyncOutputJsonDataCommand<TOutputParameterType>.Get(this, onGet, commandData,
                        outputParameter, isTask, isKeepCallback);
                    if (command != null)
                    {
                        pushCommand(command);
                        return command.KeepCallback;
                    }
                }
                onGet(new TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType> { IsReturn = false });
                return null;
            }

            /// <summary>
            ///     TCP调用并返回参数值
            /// </summary>
            /// <typeparam name="TInputParameterType">输入参数类型</typeparam>
            /// <typeparam name="TOutputParameterType">输出参数类型</typeparam>
            /// <param name="onGet">回调委托,返回null表示失败</param>
            /// <param name="commandData">TCP调用命令</param>
            /// <param name="inputParameter">输入参数</param>
            /// <param name="maxLength">输入参数数据最大长度</param>
            /// <param name="outputParameter">输出参数</param>
            /// <param name="isTask">回调是否使用任务池</param>
            /// <param name="isKeepCallback">是否保持回调</param>
            /// <returns>保持回调</returns>
            public TmphKeepCallback Get<TInputParameterType, TOutputParameterType>
                (Action<TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType>> onGet, byte[] commandData
                    , TInputParameterType inputParameter, int maxLength, TOutputParameterType outputParameter, bool isTask,
                    bool isKeepCallback)
            {
                if (isDisposed == 0)
                {
                    var command = TmphAsyncInputOutputDataCommand<TInputParameterType, TOutputParameterType>.Get(this, onGet,
                        commandData, inputParameter, maxLength, outputParameter, isTask, isKeepCallback);
                    if (command != null)
                    {
                        pushCommand(command);
                        return command.KeepCallback;
                    }
                }
                onGet(new TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType> { IsReturn = false });
                return null;
            }

            /// <summary>
            ///     TCP调用并返回参数值
            /// </summary>
            /// <typeparam name="TInputParameterType">输入参数类型</typeparam>
            /// <typeparam name="TOutputParameterType">输出参数类型</typeparam>
            /// <param name="onGet">回调委托,返回null表示失败</param>
            /// <param name="commandData">TCP调用命令</param>
            /// <param name="inputParameter">输入参数</param>
            /// <param name="maxLength">输入参数数据最大长度</param>
            /// <param name="outputParameter">输出参数</param>
            /// <param name="isTask">回调是否使用任务池</param>
            /// <param name="isKeepCallback">是否保持回调</param>
            /// <returns>保持回调</returns>
            public TmphKeepCallback GetJson<TInputParameterType, TOutputParameterType>
                (Action<TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType>> onGet
                    , byte[] commandData, TInputParameterType inputParameter, int maxLength,
                    TOutputParameterType outputParameter
                    , bool isTask, bool isKeepCallback)
            {
                if (isDisposed == 0)
                {
                    var command = TmphAsyncInputOutputJsonDataCommand<TInputParameterType, TOutputParameterType>.Get(this,
                        onGet, commandData, inputParameter, maxLength, outputParameter, isTask, isKeepCallback);
                    if (command != null)
                    {
                        pushCommand(command);
                        return command.KeepCallback;
                    }
                }
                onGet(new TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType> { IsReturn = false });
                return null;
            }

            /// <summary>
            ///     TCP调用并返回参数值
            /// </summary>
            /// <typeparam name="TOutputParameterType">输出参数类型</typeparam>
            /// <param name="onGet">回调委托,返回null表示失败</param>
            /// <param name="commandIdentity">TCP调用命令</param>
            /// <param name="outputParameter">输出参数</param>
            /// <param name="isTask">回调是否使用任务池</param>
            /// <param name="isKeepCallback">是否保持回调</param>
            /// <returns>保持回调</returns>
            public TmphKeepCallback Get<TOutputParameterType>
                (Action<TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType>> onGet, int commandIdentity
                    , TOutputParameterType outputParameter, bool isTask, bool isKeepCallback)
            {
                if (isDisposed == 0)
                {
                    var command = TmphAsyncOutputIdentityCommand<TOutputParameterType>.Get(this, onGet, commandIdentity,
                        outputParameter, isTask, isKeepCallback);
                    if (command != null)
                    {
                        pushCommand(command);
                        return command.KeepCallback;
                    }
                }
                onGet(new TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType> { IsReturn = false });
                return null;
            }

            /// <summary>
            ///     TCP调用并返回参数值
            /// </summary>
            /// <typeparam name="TOutputParameterType">输出参数类型</typeparam>
            /// <param name="onGet">回调委托,返回null表示失败</param>
            /// <param name="commandIdentity">TCP调用命令</param>
            /// <param name="outputParameter">输出参数</param>
            /// <param name="isTask">回调是否使用任务池</param>
            /// <param name="isKeepCallback">是否保持回调</param>
            /// <returns>保持回调</returns>
            public TmphKeepCallback GetJson<TOutputParameterType>
                (Action<TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType>> onGet
                    , int commandIdentity, TOutputParameterType outputParameter, bool isTask, bool isKeepCallback)
            {
                if (isDisposed == 0)
                {
                    var command = TmphAsyncOutputJsonIdentityCommand<TOutputParameterType>.Get(this, onGet, commandIdentity,
                        outputParameter, isTask, isKeepCallback);
                    if (command != null)
                    {
                        pushCommand(command);
                        return command.KeepCallback;
                    }
                }
                onGet(new TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType> { IsReturn = false });
                return null;
            }

            /// <summary>
            ///     TCP调用并返回参数值
            /// </summary>
            /// <typeparam name="TInputParameterType">输入参数类型</typeparam>
            /// <typeparam name="TOutputParameterType">输出参数类型</typeparam>
            /// <param name="onGet">回调委托,返回null表示失败</param>
            /// <param name="commandIdentity">TCP调用命令</param>
            /// <param name="inputParameter">输入参数</param>
            /// <param name="maxLength">输入参数数据最大长度</param>
            /// <param name="outputParameter">输出参数</param>
            /// <param name="isTask">回调是否使用任务池</param>
            /// <param name="isKeepCallback">是否保持回调</param>
            /// <returns>保持回调</returns>
            public TmphKeepCallback Get<TInputParameterType, TOutputParameterType>
                (Action<TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType>> onGet, int commandIdentity
                    , TInputParameterType inputParameter, int maxLength, TOutputParameterType outputParameter, bool isTask,
                    bool isKeepCallback)
            {
                if (isDisposed == 0)
                {
                    var command = TmphAsyncInputOutputIdentityCommand<TInputParameterType, TOutputParameterType>.Get(this,
                        onGet, commandIdentity, inputParameter, maxLength, outputParameter, isTask, isKeepCallback);
                    if (command != null)
                    {
                        pushCommand(command);
                        return command.KeepCallback;
                    }
                }
                onGet(new TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType> { IsReturn = false });
                return null;
            }

            /// <summary>
            ///     TCP调用并返回参数值
            /// </summary>
            /// <typeparam name="TInputParameterType">输入参数类型</typeparam>
            /// <typeparam name="TOutputParameterType">输出参数类型</typeparam>
            /// <param name="onGet">回调委托,返回null表示失败</param>
            /// <param name="commandIdentity">TCP调用命令</param>
            /// <param name="inputParameter">输入参数</param>
            /// <param name="maxLength">输入参数数据最大长度</param>
            /// <param name="outputParameter">输出参数</param>
            /// <param name="isTask">回调是否使用任务池</param>
            /// <param name="isKeepCallback">是否保持回调</param>
            /// <returns>保持回调</returns>
            public TmphKeepCallback GetJson<TInputParameterType, TOutputParameterType>
                (Action<TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType>> onGet
                    , int commandIdentity, TInputParameterType inputParameter, int maxLength,
                    TOutputParameterType outputParameter, bool isTask, bool isKeepCallback)
            {
                if (isDisposed == 0)
                {
                    var command =
                        TmphAsyncInputOutputJsonIdentityCommand<TInputParameterType, TOutputParameterType>.Get(this, onGet,
                            commandIdentity, inputParameter, maxLength, outputParameter, isTask, isKeepCallback);
                    if (command != null)
                    {
                        pushCommand(command);
                        return command.KeepCallback;
                    }
                }
                onGet(new TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType> { IsReturn = false });
                return null;
            }

            /// <summary>
            ///     TCP调用
            /// </summary>
            /// <typeparam name="TInputParameterType">输入参数类型</typeparam>
            /// <param name="onCall">回调委托,返回false表示失败</param>
            /// <param name="commandData">TCP调用命令</param>
            /// <param name="inputParameter">输入参数</param>
            /// <param name="maxLength">输入参数数据最大长度</param>
            /// <param name="isTask">回调是否使用任务池</param>
            /// <param name="isKeepCallback">是否保持回调</param>
            /// <returns>保持回调</returns>
            public TmphKeepCallback Call<TInputParameterType>
                (Action<bool> onCall, byte[] commandData, TInputParameterType inputParameter, int maxLength, bool isTask,
                    bool isKeepCallback)
            {
                if (isDisposed == 0)
                {
                    var command = TmphAsyncInputDataCommand<TInputParameterType>.Get(this, onCall, commandData,
                        inputParameter, maxLength, isTask, isKeepCallback);
                    if (command != null)
                    {
                        pushCommand(command);
                        return command.KeepCallback;
                    }
                }
                onCall(false);
                return null;
            }

            /// <summary>
            ///     TCP调用
            /// </summary>
            /// <typeparam name="TInputParameterType">输入参数类型</typeparam>
            /// <param name="onCall">回调委托,返回false表示失败</param>
            /// <param name="commandData">TCP调用命令</param>
            /// <param name="inputParameter">输入参数</param>
            /// <param name="maxLength">输入参数数据最大长度</param>
            /// <param name="isTask">回调是否使用任务池</param>
            /// <param name="isKeepCallback">是否保持回调</param>
            /// <returns>保持回调</returns>
            public TmphKeepCallback CallJson<TInputParameterType>
                (Action<bool> onCall, byte[] commandData, TInputParameterType inputParameter, int maxLength, bool isTask,
                    bool isKeepCallback)
            {
                return Call(onCall, commandData,
                    new TmphTcpBase.TmphParameterJsonToSerialize<TInputParameterType> { Return = inputParameter }, maxLength,
                    isTask, isKeepCallback);
            }

            /// <summary>
            ///     TCP调用
            /// </summary>
            /// <param name="onCall">回调委托,返回false表示失败</param>
            /// <param name="commandData">TCP调用命令</param>
            /// <param name="isTask">回调是否使用任务池</param>
            /// <param name="isKeepCallback">是否保持回调</param>
            /// <returns>保持回调</returns>
            public TmphKeepCallback Call(Action<bool> onCall, byte[] commandData, bool isTask, bool isKeepCallback)
            {
                if (isDisposed == 0)
                {
                    var command = TmphAsyncDataCommand.Get(this, onCall, commandData, isTask, isKeepCallback);
                    if (command != null)
                    {
                        pushCommand(command);
                        return command.KeepCallback;
                    }
                }
                onCall(false);
                return null;
            }

            /// <summary>
            ///     TCP调用
            /// </summary>
            /// <typeparam name="TInputParameterType">输入参数类型</typeparam>
            /// <param name="onCall">回调委托,返回false表示失败</param>
            /// <param name="commandIdentity">TCP调用命令</param>
            /// <param name="inputParameter">输入参数</param>
            /// <param name="maxLength">输入参数数据最大长度</param>
            /// <param name="isTask">回调是否使用任务池</param>
            /// <param name="isKeepCallback">是否保持回调</param>
            /// <returns>保持回调</returns>
            public TmphKeepCallback Call<TInputParameterType>
                (Action<bool> onCall, int commandIdentity, TInputParameterType inputParameter, int maxLength, bool isTask,
                    bool isKeepCallback)
            {
                if (isDisposed == 0)
                {
                    var command = TmphAsyncInputIdentityCommand<TInputParameterType>.Get(this, onCall, commandIdentity,
                        inputParameter, maxLength, isTask, isKeepCallback);
                    if (command != null)
                    {
                        pushCommand(command);
                        return command.KeepCallback;
                    }
                }
                onCall(false);
                return null;
            }

            /// <summary>
            ///     TCP调用
            /// </summary>
            /// <typeparam name="TInputParameterType">输入参数类型</typeparam>
            /// <param name="onCall">回调委托,返回false表示失败</param>
            /// <param name="commandIdentity">TCP调用命令</param>
            /// <param name="inputParameter">输入参数</param>
            /// <param name="maxLength">输入参数数据最大长度</param>
            /// <param name="isTask">回调是否使用任务池</param>
            /// <param name="isKeepCallback">是否保持回调</param>
            /// <returns>保持回调</returns>
            public TmphKeepCallback CallJson<TInputParameterType>
                (Action<bool> onCall, int commandIdentity, TInputParameterType inputParameter, int maxLength, bool isTask,
                    bool isKeepCallback)
            {
                return Call(onCall, commandIdentity,
                    new TmphTcpBase.TmphParameterJsonToSerialize<TInputParameterType> { Return = inputParameter }, maxLength,
                    isTask, isKeepCallback);
            }

            /// <summary>
            ///     TCP调用
            /// </summary>
            /// <param name="onCall">回调委托,返回false表示失败</param>
            /// <param name="commandIdentity">TCP调用命令</param>
            /// <param name="isTask">回调是否使用任务池</param>
            /// <param name="isKeepCallback">是否保持回调</param>
            /// <returns>保持回调</returns>
            public TmphKeepCallback Call(Action<bool> onCall, int commandIdentity, bool isTask, bool isKeepCallback)
            {
                if (isDisposed == 0)
                {
                    var command = TmphAsyncIdentityCommand.Get(this, onCall, commandIdentity, isTask, isKeepCallback);
                    if (command != null)
                    {
                        pushCommand(command);
                        return command.KeepCallback;
                    }
                }
                onCall(false);
                return null;
            }

            /// <summary>
            ///     取消保持回调
            /// </summary>
            /// <param name="index">命令集合索引</param>
            /// <param name="identity">会话标识</param>
            private void cancel(int index, int identity)
            {
                TmphInterlocked.NoCheckCompareSetSleep0(ref commandIndexLock);
                if ((uint)index < commandIndexs.Length && commandIndexs[index].Cancel(identity)) freeIndexLock(index);
                else commandIndexLock = 0;
            }

            /// <summary>
            ///     接收数据回调
            /// </summary>
            /// <param name="identity">会话标识</param>
            /// <param name="data">输出数据</param>
            /// <param name="isTask">检测回调是否使用任务池</param>
            /// <param name="isTaskCopyData">回调任务池是否复制数据</param>
            private void onReceive(TmphCommandServer.TmphStreamIdentity identity, TmphMemoryPool.TmphPushSubArray data,
                byte checkTask, bool isTaskCopyData)
            {
                Action<TmphMemoryPool.TmphPushSubArray> onReceive = null;
                byte isTask = 0;
                TmphInterlocked.NoCheckCompareSetSleep0(ref commandIndexLock);
                if ((uint)identity.Index < commandIndexs.Length)
                {
                    if (commandIndexs[identity.Index].Get(identity.Identity, ref onReceive, ref isTask))
                        freeIndexLock(identity.Index);
                    else commandIndexLock = 0;
                }
                else
                {
                    commandIndexLock = 0;
                    TmphLog.Error.Add(
                        attribute.ServiceName + " " + commandIndexs.Length.toString() + "[" + identity.Index.toString() +
                        "," + identity.Identity.toString() + "] Data[" + data.Value.StartIndex.toString() + "," +
                        data.Value.Count.toString() + "]", false, false);
                    TmphLog.Error.Add(
                        data.Value.Array == receiveData
                            ? "[" + receiveEndIndex.toString() + "]" +
                              receiveData.sub(0, receiveEndIndex).ToArray().JoinString(',')
                            : null, true, false);
                }
                if (onReceive != null)
                {
                    if (isTask == 0 || checkTask == 0)
                    {
                        try
                        {
                            onReceive(data);
                        }
                        catch (Exception error)
                        {
                            TmphLog.Error.Add(error, null, false);
                        }
                    }
                    else
                    {
                        if (isTaskCopyData)
                        {
                            var dataArray = data.Value;
                            var buffer = asyncBuffers.Get(dataArray.Count);
                            Buffer.BlockCopy(dataArray.Array, dataArray.StartIndex, buffer, 0, dataArray.Count);
                            data.Value.UnsafeSet(buffer, 0, dataArray.Count);
                            data.PushPool = asyncBuffers.PushHandle;
                        }
                        TmphTask.Tiny.Add(onReceive, data, null);
                    }
                }
            }

            /// <summary>
            ///     接收服务器端数据
            /// </summary>
            internal void Receive()
            {
                if (attribute.IsClientAsynchronousReceive) receiveAsynchronous();
                else
                {
                    if (receiveHandle == null) receiveHandle = receive;
                    TmphThreadPool.TinyPool.FastStart(receiveHandle, null, null);
                }
            }

            /// <summary>
            ///     同步接收服务器端数据
            /// </summary>
            private void receive()
            {
                try
                {
                    var index = receiveEndIndex = 0;
                    fixed (byte* dataFixed = receiveData)
                    {
                        do
                        {
                            var receiveLength = receiveEndIndex - index;
                            if (receiveLength < sizeof(TmphCommandServer.TmphStreamIdentity) + sizeof(int))
                            {
                                if (receiveLength != 0)
                                    Unsafe.TmphMemory.Copy(dataFixed + index, dataFixed, receiveLength);
                                receiveEndIndex = tryReceive(receiveLength,
                                    sizeof(TmphCommandServer.TmphStreamIdentity) + sizeof(int), DateTime.MaxValue);
                                if (receiveEndIndex < sizeof(TmphCommandServer.TmphStreamIdentity) + sizeof(int))
                                {
                                    //log.Error.Add("receiveEndIndex " + receiveEndIndex.toString(), false, false);
                                    break;
                                }
                                receiveLength = receiveEndIndex;
                                index = 0;
                            }
                            var start = dataFixed + index;
                            var identity = *(TmphCommandServer.TmphStreamIdentity*)start;
                            if (identity.Identity < 0)
                            {
                                //log.Error.Add("identity.Identity " + identity.Identity.toString(), false, false);
                                break;
                            }
                            var length = *(int*)(start + sizeof(TmphCommandServer.TmphStreamIdentity));
                            index += sizeof(TmphCommandServer.TmphStreamIdentity) + sizeof(int);
                            if (length == 0)
                            {
                                onReceive(identity,
                                    new TmphMemoryPool.TmphPushSubArray { Value = TmphSubArray<byte>.Unsafe(receiveData, 0, 0) },
                                    1, false);
                                continue;
                            }
                            if (length == TmphCommandServer.ErrorStreamReturnLength)
                            {
                                onReceive(identity, default(TmphMemoryPool.TmphPushSubArray), 1, false);
                                continue;
                            }
                            var dataLength = length >= 0 ? length : -length;
                            receiveLength -= sizeof(TmphCommandServer.TmphStreamIdentity) + sizeof(int);
                            if (dataLength <= receiveData.Length)
                            {
                                if (dataLength > receiveLength)
                                {
                                    Unsafe.TmphMemory.Copy(dataFixed + index, dataFixed, receiveLength);
                                    receiveEndIndex = tryReceive(receiveLength, dataLength, DateTime.MaxValue);
                                    if (receiveEndIndex < dataLength)
                                    {
                                        //log.Error.Add("receiveEndIndex[" + receiveEndIndex.toString() + "] < dataLength[" + dataLength.toString() + "]", false, false);
                                        break;
                                    }
                                    index = 0;
                                }
                                if (length > 0)
                                {
                                    onReceive(identity,
                                        new TmphMemoryPool.TmphPushSubArray
                                        {
                                            Value = TmphSubArray<byte>.Unsafe(receiveData, index, dataLength)
                                        }, 1, true);
                                }
                                else
                                {
                                    var data = TmphStream.Deflate.GetDeCompressUnsafe(receiveData, index, dataLength,
                                        TmphMemoryPool.StreamBuffers);
                                    onReceive(identity,
                                        new TmphMemoryPool.TmphPushSubArray
                                        {
                                            Value = data,
                                            PushPool = TmphMemoryPool.StreamBuffers.PushHandle
                                        }, 1, false);
                                }
                                index += dataLength;
                            }
                            else
                            {
                                var TmphBuffer = BigBuffers.Get(dataLength);
                                Unsafe.TmphMemory.Copy(dataFixed + index, TmphBuffer, receiveLength);
                                if (!receive(TmphBuffer, receiveLength, dataLength, DateTime.MaxValue))
                                {
                                    //log.Error.Add("receive Error", false, false);
                                    break;
                                }
                                if (length > 0)
                                {
                                    onReceive(identity,
                                        new TmphMemoryPool.TmphPushSubArray
                                        {
                                            Value = TmphSubArray<byte>.Unsafe(TmphBuffer, 0, dataLength)
                                        }, 1, false);
                                }
                                else
                                {
                                    var data = TmphStream.Deflate.GetDeCompressUnsafe(TmphBuffer, 0, dataLength,
                                        TmphMemoryPool.StreamBuffers);
                                    onReceive(identity,
                                        new TmphMemoryPool.TmphPushSubArray
                                        {
                                            Value = data,
                                            PushPool = TmphMemoryPool.StreamBuffers.PushHandle
                                        }, 1, false);
                                }
                                BigBuffers.Push(ref TmphBuffer);
                                index = receiveEndIndex = 0;
                            }
                        } while (true);
                    }
                }
                //catch (Exception error)
                //{
                //    log.Error.Add(error, null, false);
                //}
                finally
                {
                    Dispose();
                }
            }

            /// <summary>
            ///     接收服务器端数据
            /// </summary>
            private void receiveAsynchronous()
            {
                tryReceive(onReceiveIdentityHandle, 0, sizeof(TmphCommandServer.TmphStreamIdentity) + sizeof(int),
                    DateTime.MaxValue);
            }

            /// <summary>
            ///     接收会话标识
            /// </summary>
            /// <param name="receiveEndIndex">接收数据结束位置</param>
            private void onReceiveIdentity(int receiveEndIndex)
            {
                //if (isOutputDebug) commandServer.DebugLog.Add(attribute.ServiceName + ".onReceiveIdentity(" + receiveEndIndex.toString() + ")", false, false);
                if (receiveEndIndex >= sizeof(TmphCommandServer.TmphStreamIdentity) + sizeof(int))
                {
                    fixed (byte* dataFixed = receiveData)
                    {
                        this.receiveEndIndex = receiveEndIndex;
                        receiveDataFixed = dataFixed;
                        receiveDepth = 512;
                        onReceiveIdentity();
                        return;
                    }
                }
                Dispose();
            }

            /// <summary>
            ///     接收会话标识
            /// </summary>
            private void onReceiveIdentity()
            {
                var identity = *(TmphCommandServer.TmphStreamIdentity*)receiveDataFixed;
                if (identity.Identity >= 0)
                {
                    var length = *(int*)(receiveDataFixed + sizeof(TmphCommandServer.TmphStreamIdentity));
                    if (length == 0)
                    {
                        receiveIdentity(sizeof(TmphCommandServer.TmphStreamIdentity) + sizeof(int));
                        onReceive(identity,
                            new TmphMemoryPool.TmphPushSubArray { Value = TmphSubArray<byte>.Unsafe(receiveData, 0, 0) }, 0, false);
                    }
                    else if (length == TmphCommandServer.ErrorStreamReturnLength)
                    {
                        receiveIdentity(sizeof(TmphCommandServer.TmphStreamIdentity) + sizeof(int));
                        onReceive(identity, default(TmphMemoryPool.TmphPushSubArray), 0, false);
                    }
                    else
                    {
                        var index = onReceiveIdentity(0, length);
                        if (index != 0) receiveIdentity(index);
                    }
                    return;
                }
                Dispose();
            }

            /// <summary>
            ///     接收会话标识
            /// </summary>
            /// <param name="index">起始位置</param>
            private void receiveNextIdentity(int index)
            {
                receiveDepth = 512;
                fixed (byte* dataFixed = receiveData)
                {
                    receiveDataFixed = dataFixed;
                    receiveIdentity(index);
                }
            }

            /// <summary>
            ///     接收会话标识
            /// </summary>
            /// <param name="index">起始位置</param>
            private void receiveIdentity(int index)
            {
                if (--receiveDepth == 0)
                {
                    if (receiveIdentityHandle == null) receiveIdentityHandle = receiveNextIdentity;
                    TmphThreadPool.Default.FastStart(receiveIdentityHandle, index, null, null);
                    return;
                }
                NEXT:
                var receiveLength = receiveEndIndex - index;
                if (receiveLength >= sizeof(TmphCommandServer.TmphStreamIdentity) + sizeof(int))
                {
                    var start = receiveDataFixed + index;
                    var identity = *(TmphCommandServer.TmphStreamIdentity*)start;
                    if (identity.Identity >= 0)
                    {
                        var length = *(int*)(start + sizeof(TmphCommandServer.TmphStreamIdentity));
                        if (length == 0)
                        {
                            receiveIdentity(index + sizeof(TmphCommandServer.TmphStreamIdentity) + sizeof(int));
                            onReceive(identity,
                                new TmphMemoryPool.TmphPushSubArray { Value = TmphSubArray<byte>.Unsafe(receiveData, 0, 0) }, 0,
                                false);
                        }
                        else if (length == TmphCommandServer.ErrorStreamReturnLength)
                        {
                            receiveIdentity(index + sizeof(TmphCommandServer.TmphStreamIdentity) + sizeof(int));
                            onReceive(identity, default(TmphMemoryPool.TmphPushSubArray), 0, false);
                        }
                        else if ((index = onReceiveIdentity(index, length)) != 0) goto NEXT;
                    }
                    else Dispose();
                }
                else
                {
                    if (receiveLength != 0)
                        Unsafe.TmphMemory.Copy(receiveDataFixed + index, receiveDataFixed, receiveLength);
                    tryReceive(onReceiveIdentityHandle, receiveLength,
                        sizeof(TmphCommandServer.TmphStreamIdentity) + sizeof(int), DateTime.MaxValue);
                }
            }

            /// <summary>
            ///     接收会话标识
            /// </summary>
            /// <param name="index">起始位置</param>
            /// <param name="length">数据长度</param>
            /// <returns>下一个数据起始位置,失败返回0</returns>
            private int onReceiveIdentity(int index, int length)
            {
                var identity = *(TmphCommandServer.TmphStreamIdentity*)(receiveDataFixed + index);
                if (identity.Identity >= 0)
                {
                    int dataLength = length >= 0 ? length : -length,
                        receiveLength = receiveEndIndex -
                                        (index += (sizeof(TmphCommandServer.TmphStreamIdentity) + sizeof(int)));
                    if (dataLength <= receiveLength)
                    {
                        if (length > 0)
                        {
                            onReceive(identity,
                                new TmphMemoryPool.TmphPushSubArray
                                {
                                    Value = TmphSubArray<byte>.Unsafe(receiveData, index, dataLength)
                                }, 1, true);
                            return index + dataLength;
                        }
                        var data = TmphStream.Deflate.GetDeCompressUnsafe(receiveData, index, dataLength,
                            TmphMemoryPool.StreamBuffers);
                        receiveIdentity(index + dataLength);
                        onReceive(identity,
                            new TmphMemoryPool.TmphPushSubArray
                            {
                                Value = data,
                                PushPool = TmphMemoryPool.StreamBuffers.PushHandle
                            }, 0, false);
                    }
                    else
                    {
                        currentIdentity = identity;
                        Unsafe.TmphMemory.Copy(receiveDataFixed + index, currentReceiveData = BigBuffers.Get(dataLength),
                            receiveLength);
                        if (length > 0)
                        {
                            if (receiveNoCompressHandle == null) receiveNoCompressHandle = receiveNoCompress;
                            receive(receiveNoCompressHandle, receiveLength, dataLength - receiveLength,
                                DateTime.MaxValue);
                        }
                        else
                        {
                            if (receiveCompressHandle == null) receiveCompressHandle = receiveCompress;
                            receive(receiveCompressHandle, receiveLength, dataLength - receiveLength, DateTime.MaxValue);
                        }
                    }
                }
                else Dispose();
                return 0;
            }

            /// <summary>
            ///     获取非压缩数据
            /// </summary>
            /// <param name="isSocket">是否成功</param>
            private void receiveNoCompress(bool isSocket)
            {
                //if (isOutputDebug) commandServer.DebugLog.Add(attribute.ServiceName + ".receiveNoCompress(" + isSocket.ToString() + ")", false, false);
                if (isSocket)
                {
                    var identity = currentIdentity;
                    var data = TmphSubArray<byte>.Unsafe(currentReceiveData, 0, currentReceiveEndIndex);
                    currentReceiveData = receiveData;
                    receiveAsynchronous();
                    onReceive(identity, new TmphMemoryPool.TmphPushSubArray { Value = data, PushPool = BigBuffers.PushHandle },
                        0, false);
                }
                else Dispose();
            }

            /// <summary>
            ///     获取压缩数据
            /// </summary>
            /// <param name="isSocket">是否成功</param>
            private void receiveCompress(bool isSocket)
            {
                //if (isOutputDebug) commandServer.DebugLog.Add(attribute.ServiceName + ".receiveCompress(" + isSocket.ToString() + ")", false, false);
                if (isSocket)
                {
                    var data = TmphStream.Deflate.GetDeCompressUnsafe(currentReceiveData, 0, currentReceiveEndIndex,
                        TmphMemoryPool.StreamBuffers);
                    var identity = currentIdentity;
                    BigBuffers.Push(ref currentReceiveData);
                    currentReceiveData = receiveData;
                    receiveAsynchronous();
                    onReceive(identity,
                        new TmphMemoryPool.TmphPushSubArray { Value = data, PushPool = TmphMemoryPool.StreamBuffers.PushHandle },
                        0, false);
                }
                else Dispose();
            }

            /// <summary>
            ///     创建TCP客户端套接字
            /// </summary>
            /// <param name="commandClient">TCP调用客户端</param>
            /// <returns>TCP客户端套接字</returns>
            internal static TmphStreamCommandSocket Create(TmphCommandClient commandClient)
            {
                var socket = TmphClient.Create(commandClient.Attribute);
                if (socket != null)
                {
                    var isVerify = false;
                    try
                    {
                        var pool = TmphMemoryPool.StreamBuffers;
                        var receiveData = pool.Size == commandClient.Attribute.SendBufferSize
                            ? pool.Get()
                            : new byte[commandClient.Attribute.SendBufferSize];
                        var sendData = pool.Size == commandClient.Attribute.ReceiveBufferSize
                            ? pool.Get()
                            : new byte[commandClient.Attribute.ReceiveBufferSize];
                        var commandSocket = new TmphStreamCommandSocket(commandClient, socket, sendData, receiveData);
                        isVerify = commandSocket.verify();
                        if (isVerify) return commandSocket;
                    }
                    finally
                    {
                        if (!isVerify) socket.shutdown();
                        //if (!isVerify && tcpClient != null) tcpClient.Close();
                    }
                }
                return null;
            }

            /// <summary>
            ///     命令索引信息
            /// </summary>
            private struct TmphCommandIndex
            {
                /// <summary>
                ///     索引编号
                /// </summary>
                public int Identity;

                /// <summary>
                ///     是否保持回调
                /// </summary>
                public byte IsKeep;

                /// <summary>
                ///     回调是否使用任务池
                /// </summary>
                public byte IsTask;

                /// <summary>
                ///     接收数据回调
                /// </summary>
                public Action<TmphMemoryPool.TmphPushSubArray> OnReceive;

                /// <summary>
                ///     清除命令信息
                /// </summary>
                public void Clear()
                {
                    ++Identity;
                    OnReceive = null;
                    IsKeep = IsTask = 0;
                }

                /// <summary>
                ///     设置接收数据回调
                /// </summary>
                /// <param name="onReceive">接收数据回调</param>
                /// <param name="isTask">回调是否使用任务池</param>
                /// <param name="isKeep">是否保持回调</param>
                public void Set(Action<TmphMemoryPool.TmphPushSubArray> onReceive, byte isTask, byte isKeep)
                {
                    OnReceive = onReceive;
                    IsKeep = isKeep;
                    IsTask = isTask;
                }

                /// <summary>
                ///     取消接收数据
                /// </summary>
                /// <returns>接收数据回调+回调是否使用任务池</returns>
                public TmphKeyValue<Action<TmphMemoryPool.TmphPushSubArray>, byte> Cancel()
                {
                    var onReceive = OnReceive;
                    var isTask = IsTask;
                    ++Identity;
                    OnReceive = null;
                    IsKeep = IsTask = 0;
                    return new TmphKeyValue<Action<TmphMemoryPool.TmphPushSubArray>, byte>(onReceive, isTask);
                }

                /// <summary>
                ///     取消接收数据回调
                /// </summary>
                /// <param name="identity">索引编号</param>
                /// <returns>是否释放</returns>
                public bool Cancel(int identity)
                {
                    if (identity == Identity)
                    {
                        ++Identity;
                        OnReceive = null;
                        IsKeep = IsTask = 0;
                        return true;
                    }
                    return false;
                }

                /// <summary>
                ///     获取接收数据回调
                /// </summary>
                /// <param name="identity">索引编号</param>
                /// <param name="onReceive">接收数据回调</param>
                /// <param name="isTask">回调是否使用任务池</param>
                /// <returns>是否释放</returns>
                public bool Get(int identity, ref Action<TmphMemoryPool.TmphPushSubArray> onReceive, ref byte isTask)
                {
                    if (identity == Identity)
                    {
                        onReceive = OnReceive;
                        isTask = IsTask;
                        if (IsKeep == 0)
                        {
                            ++Identity;
                            OnReceive = null;
                            IsTask = 0;
                            return true;
                        }
                    }
                    return false;
                }
            }

            /// <summary>
            ///     保持回调
            /// </summary>
            public sealed class TmphKeepCallback : IDisposable
            {
                /// <summary>
                ///     默认空保持回调
                /// </summary>
                internal static readonly TmphKeepCallback Null = new TmphKeepCallback(null, 0, 0, 0);

                /// <summary>
                ///     命令序号
                /// </summary>
                private readonly int commandIdentity;

                /// <summary>
                ///     命令集合索引
                /// </summary>
                private readonly int commandIndex;

                /// <summary>
                ///     保持回调序号
                /// </summary>
                private readonly int identity;

                /// <summary>
                ///     终止回调委托
                /// </summary>
                private Action<int, int, int> cancel;

                /// <summary>
                ///     保持回调
                /// </summary>
                /// <param name="cancel">终止回调委托</param>
                /// <param name="identity">保持回调序号</param>
                /// <param name="commandIndex">命令集合索引</param>
                /// <param name="commandIdentity">命令序号</param>
                internal TmphKeepCallback(Action<int, int, int> cancel, int identity, int commandIndex,
                    int commandIdentity)
                {
                    this.cancel = cancel;
                    this.identity = identity;
                    this.commandIndex = commandIndex;
                    this.commandIdentity = commandIdentity;
                }

                /// <summary>
                ///     终止回调
                /// </summary>
                public void Dispose()
                {
                    var cancel = this.cancel;
                    this.cancel = null;
                    if (cancel != null) cancel(identity, commandIndex, commandIdentity);
                }
            }

            /// <summary>
            ///     TCP流读取器
            /// </summary>
            private sealed class TmphTcpStreamReader
            {
                /// <summary>
                ///     读取回调
                /// </summary>
                private readonly AsyncCallback callback;

                /// <summary>
                ///     TCP流参数
                /// </summary>
                private TmphCommandServer.TmphTcpStreamParameter parameter;

                /// <summary>
                ///     TCP客户端命令流处理套接字
                /// </summary>
                private TmphStreamCommandSocket socket;

                /// <summary>
                ///     字节流
                /// </summary>
                private Stream stream;

                /// <summary>
                ///     TCP流读取器
                /// </summary>
                private TmphTcpStreamReader()
                {
                    callback = onRead;
                }

                /// <summary>
                ///     读取回调
                /// </summary>
                /// <param name="result">回调状态</param>
                private void onRead(IAsyncResult result)
                {
                    try
                    {
                        this.parameter.Offset = stream.EndRead(result);
                        this.parameter.IsCommand = true;
                    }
                    catch (Exception error)
                    {
                        TmphLog.Error.Add(error, null, false);
                    }
                    var socket = this.socket;
                    var parameter = this.parameter;
                    stream = null;
                    this.socket = null;
                    this.parameter = null;
                    try
                    {
                        TmphTypePool<TmphTcpStreamReader>.Push(this);
                    }
                    finally
                    {
                        socket.doTcpStream(parameter);
                        asyncBuffers.Push(ref parameter.Data.array);
                    }
                }

                /// <summary>
                ///     读取数据
                /// </summary>
                public void Read()
                {
                    stream.BeginRead(parameter.Data.Array, 0, (int)parameter.Offset, callback, this);
                }

                /// <summary>
                ///     获取TCP流读取器
                /// </summary>
                /// <param name="socket">TCP客户端命令流处理套接字</param>
                /// <param name="stream">字节流</param>
                /// <param name="parameter">TCP流参数</param>
                /// <returns>TCP流读取器</returns>
                public static TmphTcpStreamReader Get(TmphStreamCommandSocket socket, Stream stream,
                    TmphCommandServer.TmphTcpStreamParameter parameter)
                {
                    var tcpStreamReader = TmphTypePool<TmphTcpStreamReader>.Pop();
                    if (tcpStreamReader == null)
                    {
                        try
                        {
                            tcpStreamReader = new TmphTcpStreamReader();
                        }
                        catch
                        {
                        }
                        if (tcpStreamReader == null)
                        {
                            socket.doTcpStream(parameter);
                            return null;
                        }
                    }
                    tcpStreamReader.socket = socket;
                    tcpStreamReader.stream = stream;
                    tcpStreamReader.parameter = parameter;
                    return tcpStreamReader;
                }
            }

            /// <summary>
            ///     TCP流写入器
            /// </summary>
            private sealed class TmphTcpStreamWriter
            {
                /// <summary>
                ///     写入回调
                /// </summary>
                private readonly AsyncCallback callback;

                /// <summary>
                ///     TCP流参数
                /// </summary>
                private TmphCommandServer.TmphTcpStreamParameter parameter;

                /// <summary>
                ///     TCP客户端命令流处理套接字
                /// </summary>
                private TmphStreamCommandSocket socket;

                /// <summary>
                ///     字节流
                /// </summary>
                private Stream stream;

                /// <summary>
                ///     TCP流写入器
                /// </summary>
                private TmphTcpStreamWriter()
                {
                    callback = onWrite;
                }

                /// <summary>
                ///     写入回调
                /// </summary>
                /// <param name="result">回调状态</param>
                private void onWrite(IAsyncResult result)
                {
                    try
                    {
                        stream.EndWrite(result);
                        this.parameter.IsCommand = true;
                    }
                    catch (Exception error)
                    {
                        TmphLog.Error.Add(error, null, false);
                    }
                    var socket = this.socket;
                    var parameter = this.parameter;
                    stream = null;
                    this.socket = null;
                    this.parameter = null;
                    try
                    {
                        TmphTypePool<TmphTcpStreamWriter>.Push(this);
                    }
                    finally
                    {
                        socket.doTcpStream(parameter);
                    }
                }

                /// <summary>
                ///     写入数据
                /// </summary>
                public void Write()
                {
                    var data = parameter.Data;
                    stream.BeginWrite(data.Array, data.StartIndex, data.Count, callback, this);
                }

                /// <summary>
                ///     获取TCP流写入器
                /// </summary>
                /// <param name="socket">TCP客户端命令流处理套接字</param>
                /// <param name="stream">字节流</param>
                /// <param name="parameter">TCP流参数</param>
                /// <returns>TCP流写入器</returns>
                public static TmphTcpStreamWriter Get(TmphStreamCommandSocket socket, Stream stream,
                    TmphCommandServer.TmphTcpStreamParameter parameter)
                {
                    var tcpStreamWriter = TmphTypePool<TmphTcpStreamWriter>.Pop();
                    if (tcpStreamWriter == null)
                    {
                        try
                        {
                            tcpStreamWriter = new TmphTcpStreamWriter();
                        }
                        catch
                        {
                        }
                        if (tcpStreamWriter == null)
                        {
                            socket.doTcpStream(parameter);
                            return null;
                        }
                    }
                    tcpStreamWriter.socket = socket;
                    tcpStreamWriter.stream = stream;
                    tcpStreamWriter.parameter = parameter;
                    return tcpStreamWriter;
                }
            }

            /// <summary>
            ///     命令队列集合
            /// </summary>
            private struct TmphCommandQueue
            {
                /// <summary>
                ///     最后一个节点
                /// </summary>
                public TmphCommand End;

                /// <summary>
                ///     第一个节点
                /// </summary>
                public TmphCommand Head;

                /// <summary>
                ///     是否只存在一个节点
                /// </summary>
                public bool IsSingle
                {
                    get { return Head == End; }
                }

                /// <summary>
                ///     清除命令
                /// </summary>
                public void Clear()
                {
                    Head = End = null;
                }

                /// <summary>
                ///     添加命令
                /// </summary>
                /// <param name="command"></param>
                public void Push(TmphCommand command)
                {
                    if (Head == null) Head = End = command;
                    else
                    {
                        End.Next = command;
                        End = command;
                    }
                }

                /// <summary>
                ///     添加命令
                /// </summary>
                /// <param name="command"></param>
                /// <returns>是否第一个命令</returns>
                public bool IsPushHead(TmphCommand command)
                {
                    if (Head == null)
                    {
                        Head = End = command;
                        return true;
                    }
                    End.Next = command;
                    End = command;
                    return false;
                }

                /// <summary>
                ///     获取命令
                /// </summary>
                /// <returns></returns>
                public TmphCommand Pop()
                {
                    if (Head == null) return null;
                    var command = Head;
                    Head = Head.Next;
                    command.Next = null;
                    return command;
                }
            }

            /// <summary>
            ///     命令创建
            /// </summary>
            private struct TmphCommandBuilder
            {
                /// <summary>
                ///     命令流字节长度
                /// </summary>
                private int bufferLength;

                /// <summary>
                ///     第一个命令数据其实位置
                /// </summary>
                private int buildIndex;

                /// <summary>
                ///     命令流
                /// </summary>
                public TmphUnmanagedStream CommandStream;

                /// <summary>
                ///     当前命令
                /// </summary>
                private TmphCommand currentCommand;

                /// <summary>
                ///     命令数据
                /// </summary>
                private TmphSubArray<byte> data;

                /// <summary>
                ///     命令流数据起始位置
                /// </summary>
                private byte* dataFixed;

                /// <summary>
                ///     最大命令长度
                /// </summary>
                public int MaxCommandLength;

                /// <summary>
                ///     命令流数据位置
                /// </summary>
                public int MergeIndex;

                /// <summary>
                ///     TCP客户端命令流处理套接字
                /// </summary>
                public TmphStreamCommandSocket Socket;

                /// <summary>
                ///     重置命令流
                /// </summary>
                /// <param name="data">命令流数据起始位置</param>
                /// <param name="length">命令流字节长度</param>
                public void Reset(byte* data, int length)
                {
                    CommandStream.Reset(dataFixed = data, bufferLength = length);
                    CommandStream.Unsafer.SetLength(MergeIndex);
                    MaxCommandLength = 0;
                }

                /// <summary>
                ///     创建命令流
                /// </summary>
                /// <param name="command">命令</param>
                public void Build(TmphCommand command)
                {
                    currentCommand = command;
                    int streamLength = CommandStream.Length, buildIndex = command.BuildIndex(CommandStream);
                    currentCommand = null;
                    if (buildIndex == 0) command.Cancel();
                    else
                    {
                        if (Socket.buildCommands.IsPushHead(command)) this.buildIndex = buildIndex;
                        var commandLength = CommandStream.Length - streamLength;
                        if (commandLength > MaxCommandLength) MaxCommandLength = commandLength;
                    }
                }

                /// <summary>
                ///     发送数据
                /// </summary>
                public void Send()
                {
                    MaxCommandLength = 0;
                    TmphPushPool<byte[]> pushPool = null;
                    int commandLength = CommandStream.Length, dataLength = commandLength - MergeIndex, isNewBuffer = 0;
                    if (Socket.buildCommands.IsSingle)
                    {
                        if (commandLength <= bufferLength)
                        {
                            if (CommandStream.DataLength != bufferLength)
                            {
                                Unsafe.TmphMemory.Copy(CommandStream.Data + MergeIndex, dataFixed + MergeIndex,
                                    dataLength);
                                CommandStream.Reset(dataFixed, bufferLength);
                            }
                            data.UnsafeSet(Socket.sendData, MergeIndex, dataLength);
                        }
                        else
                        {
                            var newCommandBuffer = CommandStream.GetSizeArray(MergeIndex, bufferLength << 1);
                            TmphMemoryPool.StreamBuffers.Push(ref Socket.sendData);
                            data.UnsafeSet(Socket.sendData = newCommandBuffer, MergeIndex, dataLength);
                            isNewBuffer = 1;
                        }
                        if (Socket.attribute.IsCompress && dataLength > TmphUnmanagedStreamBase.DefaultLength)
                        {
                            var startIndex = buildIndex - MergeIndex;
                            var compressData = TmphStream.Deflate.GetCompressUnsafe(data.Array, buildIndex,
                                dataLength - startIndex, startIndex, TmphMemoryPool.StreamBuffers);
                            if (compressData.array != null)
                            {
                                fixed (byte* compressFixed = compressData.array, sendFixed = data.Array)
                                {
                                    Unsafe.TmphMemory.Copy(sendFixed + MergeIndex, compressFixed,
                                        startIndex - sizeof(int));
                                    *(int*)(compressFixed + startIndex - sizeof(int)) = -compressData.Count;
                                }
                                data.UnsafeSet(compressData.array, 0, compressData.Count + startIndex);
                                pushPool = TmphMemoryPool.StreamBuffers.PushHandle;
                            }
                        }
                    }
                    else
                    {
                        if (commandLength > bufferLength)
                        {
                            var newCommandBuffer = CommandStream.GetSizeArray(MergeIndex, bufferLength << 1);
                            TmphMemoryPool.StreamBuffers.Push(ref Socket.sendData);
                            data.UnsafeSet(Socket.sendData = newCommandBuffer, 0, commandLength);
                            isNewBuffer = 1;
                        }
                        else
                        {
                            if (CommandStream.DataLength != bufferLength)
                            {
                                Unsafe.TmphMemory.Copy(CommandStream.Data + MergeIndex, dataFixed + MergeIndex,
                                    commandLength - MergeIndex);
                                CommandStream.Reset(dataFixed, bufferLength);
                            }
                            data.UnsafeSet(Socket.sendData, 0, commandLength);
                        }
                        if (Socket.attribute.IsCompress && dataLength > TmphUnmanagedStreamBase.DefaultLength)
                        {
                            var compressData = TmphStream.Deflate.GetCompressUnsafe(data.Array, MergeIndex, dataLength,
                                MergeIndex, TmphMemoryPool.StreamBuffers);
                            if (compressData.array != null)
                            {
                                dataLength = -compressData.Count;
                                data.UnsafeSet(compressData.array, 0, compressData.Count + MergeIndex);
                                pushPool = TmphMemoryPool.StreamBuffers.PushHandle;
                            }
                        }
                        fixed (byte* megerDataFixed = data.Array)
                        {
                            byte* write;
                            if (Socket.attribute.IsIdentityCommand)
                                *(int*)(write = megerDataFixed) = TmphCommandServer.StreamMergeIdentityCommand;
                            else
                            {
                                write = megerDataFixed + sizeof(int);
                                *(int*)megerDataFixed = sizeof(int) * 3 + sizeof(TmphCommandServer.TmphStreamIdentity);
                                *(int*)write = TmphCommandServer.StreamMergeIdentityCommand +
                                                TmphCommandServer.CommandDataIndex;
                            }
                            //*(commandServer.streamIdentity*)(write + sizeof(int)) = default(commandServer.streamIdentity);
                            *(int*)(write + (sizeof(int) + sizeof(TmphCommandServer.TmphStreamIdentity))) = dataLength;
                        }
                    }
                    if (isNewBuffer == 0) CommandStream.Unsafer.SetLength(MergeIndex);
                    Socket.lastCheckTime = TmphDate.NowSecond;
                    try
                    {
                        //if (Socket.isOutputDebug) commandServer.DebugLog.Add(Socket.attribute.ServiceName + ".Send(" + data.Length.toString() + ")", false, false);
                        if (Socket.send(data)) Socket.buildCommands.Clear();
                        else Cancel();
                    }
                    finally
                    {
                        if (pushPool != null) pushPool(ref data.array);
                    }
                }

                /// <summary>
                ///     取消命令
                /// </summary>
                public void Cancel()
                {
                    if (currentCommand != null)
                    {
                        currentCommand.Cancel();
                        currentCommand = null;
                    }
                    var command = Socket.buildCommands.Head;
                    Socket.buildCommands.Clear();
                    while (command != null)
                    {
                        var next = command.Next;
                        command.Next = null;
                        command.Cancel();
                        command = next;
                    }
                }
            }

            /// <summary>
            ///     客户端命令
            /// </summary>
            private abstract class TmphCommand
            {
                /// <summary>
                ///     接收数据回调处理
                /// </summary>
                public readonly Action<TmphMemoryPool.TmphPushSubArray> OnReceive;

                /// <summary>
                ///     会话标识
                /// </summary>
                public TmphCommandServer.TmphStreamIdentity Identity;

                /// <summary>
                ///     下一个客户端命令
                /// </summary>
                public TmphCommand Next;

                /// <summary>
                ///     TCP客户端命令流处理套接字
                /// </summary>
                public TmphStreamCommandSocket Socket;

                /// <summary>
                ///     客户端命令
                /// </summary>
                protected TmphCommand()
                {
                    OnReceive = onReceive;
                }

                /// <summary>
                ///     创建第一个命令输入数据
                /// </summary>
                /// <param name="stream">命令内存流</param>
                /// <returns>数据起始位置,失败返回0</returns>
                public abstract int BuildIndex(TmphUnmanagedStream stream);

                /// <summary>
                ///     接收数据回调处理
                /// </summary>
                /// <param name="data">输出数据</param>
                protected abstract void onReceive(TmphMemoryPool.TmphPushSubArray data);

                /// <summary>
                ///     取消错误命令
                /// </summary>
                public virtual void Cancel()
                {
                    Socket.onReceive(Identity, default(TmphMemoryPool.TmphPushSubArray), 1, false);
                }
            }

            /// <summary>
            ///     客户端命令
            /// </summary>
            /// <typeparam name="TCommandType">客户端命令类型</typeparam>
            private abstract class TmphAsyncCommand<TCommandType> : TmphCommand
                where TCommandType : TmphAsyncCommand<TCommandType>
            {
                /// <summary>
                ///     当前命令
                /// </summary>
                private readonly TCommandType thisCommand;

                /// <summary>
                ///     终止保持回调
                /// </summary>
                protected Action<int, int, int> cancelKeepCallback;

                /// <summary>
                ///     保持回调
                /// </summary>
                public TmphKeepCallback KeepCallback;

                /// <summary>
                ///     保持回调序号
                /// </summary>
                protected int keepCallbackIdentity;

                /// <summary>
                ///     客户端命令
                /// </summary>
                protected TmphAsyncCommand()
                {
                    thisCommand = (TCommandType)this;
                }

                /// <summary>
                ///     保持回调
                /// </summary>
                public bool SetKeepCallback()
                {
                    try
                    {
                        KeepCallback = new TmphKeepCallback(cancelKeepCallback, ++keepCallbackIdentity, Identity.Index,
                            Socket.commandIndexs[Identity.Index].Identity);
                        return true;
                    }
                    catch (Exception error)
                    {
                        TmphLog.Error.Add(error, null, false);
                    }
                    Socket.freeIndex(Identity.Index);
                    return false;
                }

                /// <summary>
                ///     取消错误命令
                /// </summary>
                public override void Cancel()
                {
                    if (KeepCallback == null) base.Cancel();
                    else KeepCallback.Dispose();
                }

                /// <summary>
                ///     添加到对象池
                /// </summary>
                protected void push()
                {
                    if (KeepCallback == null)
                    {
                        Next = null;
                        Socket = null;
                        TmphTypePool<TCommandType>.Push(thisCommand);
                    }
                }
            }

            /// <summary>
            ///     客户端命令
            /// </summary>
            /// <typeparam name="TCommandType">客户端命令类型</typeparam>
            /// <typeparam name="TCallbackType">回调数据类型</typeparam>
            private abstract class TmphAsyncCommand<TCommandType, TCallbackType> : TmphAsyncCommand<TCommandType>
                where TCommandType : TmphAsyncCommand<TCommandType, TCallbackType>
            {
                /// <summary>
                ///     回调委托
                /// </summary>
                public Action<TCallbackType> Callback;

                /// <summary>
                ///     客户端命令
                /// </summary>
                protected TmphAsyncCommand()
                {
                    cancelKeepCallback = cancelKeep;
                }

                /// <summary>
                ///     添加回调对象
                /// </summary>
                /// <param name="value">回调值</param>
                protected void push(TCallbackType value)
                {
                    var callback = Callback;
                    Callback = null;
                    push();
                    if (callback != null)
                    {
                        try
                        {
                            callback(value);
                        }
                        catch (Exception error)
                        {
                            TmphLog.Error.Add(error, null, false);
                        }
                    }
                }

                /// <summary>
                ///     回调处理
                /// </summary>
                /// <param name="value">回调值</param>
                protected void onlyCallback(TCallbackType value)
                {
                    var callback = Callback;
                    if (callback != null)
                    {
                        try
                        {
                            callback(value);
                        }
                        catch (Exception error)
                        {
                            TmphLog.Error.Add(error, null, false);
                        }
                    }
                }

                /// <summary>
                ///     终止保持回调
                /// </summary>
                /// <param name="identity">保持回调序号</param>
                /// <param name="commandIndex">命令集合索引</param>
                /// <param name="commandIdentity">命令序号</param>
                private void cancelKeep(int identity, int commandIndex, int commandIdentity)
                {
                    if (Interlocked.CompareExchange(ref keepCallbackIdentity, identity + 1, identity) == identity)
                    {
                        Callback = null;
                        Socket.cancel(commandIndex, commandIdentity);
                        onReceive(default(TmphMemoryPool.TmphPushSubArray));
                    }
                }
            }

            /// <summary>
            ///     客户端命令
            /// </summary>
            /// <typeparam name="TCommandType">客户端命令类型</typeparam>
            /// <typeparam name="TCallbackType">回调数据类型</typeparam>
            private abstract class TmphAsyncDataCommand<TCommandType, TCallbackType> :
                TmphAsyncCommand<TCommandType, TCallbackType>
                where TCommandType : TmphAsyncDataCommand<TCommandType, TCallbackType>
            {
                /// <summary>
                ///     TCP调用命令
                /// </summary>
                public byte[] Command;

                /// <summary>
                ///     创建第一个命令输入数据
                /// </summary>
                /// <param name="stream">命令内存流</param>
                /// <returns>数据起始位置,失败返回0</returns>
                public override int BuildIndex(TmphUnmanagedStream stream)
                {
                    stream.PrepLength(Command.Length + (sizeof(int) + sizeof(TmphCommandServer.TmphStreamIdentity)));
                    var write = stream.CurrentData;
                    Unsafe.TmphMemory.Copy(Command, write, Command.Length);
                    write += Command.Length;
                    *(TmphCommandServer.TmphStreamIdentity*)(write) = Identity;
                    *(int*)(write + sizeof(TmphCommandServer.TmphStreamIdentity)) = 0;
                    stream.Unsafer.AddLength(Command.Length + (sizeof(int) + sizeof(TmphCommandServer.TmphStreamIdentity)));
                    return stream.Length;
                }
            }

            /// <summary>
            ///     客户端命令
            /// </summary>
            /// <typeparam name="TOutputParameterType">输出参数类型</typeparam>
            private sealed class TmphAsyncOutputDataCommand<TOutputParameterType> :
                TmphAsyncDataCommand
                    <TmphAsyncOutputDataCommand<TOutputParameterType>,
                        TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType>>
            {
                /// <summary>
                ///     输出参数
                /// </summary>
                public TOutputParameterType OutputParameter;

                /// <summary>
                ///     接收数据回调处理
                /// </summary>
                /// <param name="data">输出数据</param>
                protected override void onReceive(TmphMemoryPool.TmphPushSubArray data)
                {
                    var isReturn = false;
                    var TmphBuffer = data.Value.Array;
                    if (KeepCallback == null)
                    {
                        try
                        {
                            if (TmphBuffer != null && TmphDataDeSerializer.DeSerialize(data.Value, ref OutputParameter))
                                isReturn = true;
                        }
                        finally
                        {
                            var outputParameter = OutputParameter;
                            OutputParameter = default(TOutputParameterType);
                            push(new TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType>
                            {
                                IsReturn = isReturn,
                                Value = outputParameter
                            });
                            data.Push();
                        }
                    }
                    else if (TmphBuffer == null)
                    {
                        onlyCallback(new TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType> { IsReturn = false });
                    }
                    else
                    {
                        Monitor.Enter(this);
                        try
                        {
                            if (TmphDataDeSerializer.DeSerialize(data.Value, ref OutputParameter)) isReturn = true;
                        }
                        finally
                        {
                            Monitor.Exit(this);
                            onlyCallback(new TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType>
                            {
                                IsReturn = isReturn,
                                Value = OutputParameter
                            });
                            data.Push();
                        }
                    }
                }

                /// <summary>
                ///     获取客户端命令
                /// </summary>
                /// <param name="socket">TCP客户端命令流处理套接字</param>
                /// <param name="commandData">TCP调用命令</param>
                /// <param name="outputParameter">输出参数</param>
                /// <param name="isTask">回调是否使用任务池</param>
                /// <param name="isKeepCallback">是否保持回调</param>
                /// <returns>客户端命令</returns>
                public static TmphAsyncOutputDataCommand<TOutputParameterType> Get
                    (TmphStreamCommandSocket socket, Action<TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType>> onGet
                        , byte[] commandData, TOutputParameterType outputParameter, bool isTask, bool isKeepCallback)
                {
                    var command = TmphTypePool<TmphAsyncOutputDataCommand<TOutputParameterType>>.Pop();
                    if (command == null)
                    {
                        try
                        {
                            command = new TmphAsyncOutputDataCommand<TOutputParameterType>();
                        }
                        catch (Exception error)
                        {
                            TmphLog.Error.Add(error, null, false);
                        }
                        if (command == null) return null;
                    }
                    command.Socket = socket;
                    command.Callback = onGet;
                    command.Command = commandData;
                    command.OutputParameter = outputParameter;
                    command.Identity = socket.newIndex(command.OnReceive, isTask ? (byte)1 : (byte)0,
                        isKeepCallback ? (byte)1 : (byte)0);
                    if (isKeepCallback)
                    {
                        if (command.SetKeepCallback()) return command;
                        command.OutputParameter = default(TOutputParameterType);
                        command.Callback = null;
                        command.push();
                        return null;
                    }
                    return command;
                }
            }

            /// <summary>
            ///     客户端命令
            /// </summary>
            /// <typeparam name="TOutputParameterType">输出参数类型</typeparam>
            private sealed class TmphAsyncOutputJsonDataCommand<TOutputParameterType> :
                TmphAsyncDataCommand
                    <TmphAsyncOutputJsonDataCommand<TOutputParameterType>,
                        TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType>>
            {
                /// <summary>
                ///     输出参数
                /// </summary>
                public TmphTcpBase.TmphParameterJsonToSerialize<TOutputParameterType> OutputParameter;

                /// <summary>
                ///     接收数据回调处理
                /// </summary>
                /// <param name="data">输出数据</param>
                protected override void onReceive(TmphMemoryPool.TmphPushSubArray data)
                {
                    var isReturn = false;
                    var TmphBuffer = data.Value.Array;
                    if (KeepCallback == null)
                    {
                        try
                        {
                            if (TmphBuffer != null && TmphDataDeSerializer.DeSerialize(data.Value, ref OutputParameter))
                                isReturn = true;
                        }
                        finally
                        {
                            var outputParameter = OutputParameter.Return;
                            OutputParameter.Return = default(TOutputParameterType);
                            push(new TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType>
                            {
                                IsReturn = isReturn,
                                Value = outputParameter
                            });
                            data.Push();
                        }
                    }
                    else if (TmphBuffer == null)
                    {
                        onlyCallback(new TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType> { IsReturn = false });
                    }
                    else
                    {
                        Monitor.Enter(this);
                        try
                        {
                            if (TmphDataDeSerializer.DeSerialize(data.Value, ref OutputParameter)) isReturn = true;
                        }
                        finally
                        {
                            Monitor.Exit(this);
                            onlyCallback(new TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType>
                            {
                                IsReturn = isReturn,
                                Value = OutputParameter.Return
                            });
                            data.Push();
                        }
                    }
                }

                /// <summary>
                ///     获取客户端命令
                /// </summary>
                /// <param name="socket">TCP客户端命令流处理套接字</param>
                /// <param name="commandData">TCP调用命令</param>
                /// <param name="outputParameter">输出参数</param>
                /// <param name="isTask">回调是否使用任务池</param>
                /// <param name="isKeepCallback">是否保持回调</param>
                /// <returns>客户端命令</returns>
                public static TmphAsyncOutputJsonDataCommand<TOutputParameterType> Get
                    (TmphStreamCommandSocket socket, Action<TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType>> onGet
                        , byte[] commandData, TOutputParameterType outputParameter, bool isTask, bool isKeepCallback)
                {
                    var command = TmphTypePool<TmphAsyncOutputJsonDataCommand<TOutputParameterType>>.Pop();
                    if (command == null)
                    {
                        try
                        {
                            command = new TmphAsyncOutputJsonDataCommand<TOutputParameterType>();
                        }
                        catch (Exception error)
                        {
                            TmphLog.Error.Add(error, null, false);
                        }
                        if (command == null) return null;
                    }
                    command.Socket = socket;
                    command.Callback = onGet;
                    command.Command = commandData;
                    command.OutputParameter = new TmphTcpBase.TmphParameterJsonToSerialize<TOutputParameterType>
                    {
                        Return = outputParameter
                    };
                    command.Identity = socket.newIndex(command.OnReceive, isTask ? (byte)1 : (byte)0,
                        isKeepCallback ? (byte)1 : (byte)0);
                    if (isKeepCallback)
                    {
                        if (command.SetKeepCallback()) return command;
                        command.OutputParameter.Return = default(TOutputParameterType);
                        command.Callback = null;
                        command.push();
                        return null;
                    }
                    return command;
                }
            }

            /// <summary>
            ///     客户端命令
            /// </summary>
            /// <typeparam name="TInputParameterType">输入参数类型</typeparam>
            /// <typeparam name="TOutputParameterType">输出参数类型</typeparam>
            private sealed class TmphAsyncInputOutputDataCommand<TInputParameterType, TOutputParameterType> :
                TmphAsyncInputDataCommand
                    <TInputParameterType, TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType>,
                        TmphAsyncInputOutputDataCommand<TInputParameterType, TOutputParameterType>>
            {
                /// <summary>
                ///     输出参数
                /// </summary>
                public TOutputParameterType OutputParameter;

                /// <summary>
                ///     接收数据回调处理
                /// </summary>
                /// <param name="data">输出数据</param>
                protected override void onReceive(TmphMemoryPool.TmphPushSubArray data)
                {
                    var isReturn = false;
                    var TmphBuffer = data.Value.Array;
                    if (KeepCallback == null)
                    {
                        try
                        {
                            if (TmphBuffer != null && TmphDataDeSerializer.DeSerialize(data.Value, ref OutputParameter))
                                isReturn = true;
                        }
                        finally
                        {
                            var outputParameter = OutputParameter;
                            OutputParameter = default(TOutputParameterType);
                            push(new TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType>
                            {
                                IsReturn = isReturn,
                                Value = outputParameter
                            });
                            data.Push();
                        }
                    }
                    else if (TmphBuffer == null)
                    {
                        onlyCallback(new TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType> { IsReturn = false });
                    }
                    else
                    {
                        Monitor.Enter(this);
                        try
                        {
                            if (TmphDataDeSerializer.DeSerialize(data.Value, ref OutputParameter)) isReturn = true;
                        }
                        finally
                        {
                            Monitor.Exit(this);
                            onlyCallback(new TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType>
                            {
                                IsReturn = isReturn,
                                Value = OutputParameter
                            });
                            data.Push();
                        }
                    }
                }

                /// <summary>
                ///     获取客户端命令
                /// </summary>
                /// <param name="socket">TCP客户端命令流处理套接字</param>
                /// <param name="onCall">回调委托,返回false表示失败</param>
                /// <param name="commandData">TCP调用命令</param>
                /// <param name="inputParameter">输入参数</param>
                /// <param name="maxLength">输入参数数据最大长度</param>
                /// <param name="isTask">回调是否使用任务池</param>
                /// <param name="isKeepCallback">是否保持回调</param>
                /// <returns>客户端命令</returns>
                public static TmphAsyncInputOutputDataCommand<TInputParameterType, TOutputParameterType> Get
                    (TmphStreamCommandSocket socket, Action<TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType>> onGet
                        , byte[] commandData, TInputParameterType inputParameter, int maxLength,
                        TOutputParameterType outputParameter
                        , bool isTask, bool isKeepCallback)
                {
                    var command =
                        TmphTypePool<TmphAsyncInputOutputDataCommand<TInputParameterType, TOutputParameterType>>.Pop();
                    if (command == null)
                    {
                        try
                        {
                            command = new TmphAsyncInputOutputDataCommand<TInputParameterType, TOutputParameterType>();
                        }
                        catch (Exception error)
                        {
                            TmphLog.Error.Add(error, null, false);
                        }
                        if (command == null) return null;
                    }
                    command.Socket = socket;
                    command.Callback = onGet;
                    command.Command = commandData;
                    command.InputParameter = inputParameter;
                    command.MaxInputSize = maxLength;
                    command.OutputParameter = outputParameter;
                    command.Identity = socket.newIndex(command.OnReceive, isTask ? (byte)1 : (byte)0,
                        isKeepCallback ? (byte)1 : (byte)0);
                    if (isKeepCallback)
                    {
                        if (command.SetKeepCallback()) return command;
                        command.InputParameter = default(TInputParameterType);
                        command.OutputParameter = default(TOutputParameterType);
                        command.Callback = null;
                        command.push();
                        return null;
                    }
                    return command;
                }
            }

            /// <summary>
            ///     客户端命令
            /// </summary>
            /// <typeparam name="TInputParameterType">输入参数类型</typeparam>
            /// <typeparam name="TOutputParameterType">输出参数类型</typeparam>
            private sealed class TmphAsyncInputOutputJsonDataCommand<TInputParameterType, TOutputParameterType> :
                TmphAsyncInputDataCommand
                    <TmphTcpBase.TmphParameterJsonToSerialize<TInputParameterType>,
                        TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType>,
                        TmphAsyncInputOutputJsonDataCommand<TInputParameterType, TOutputParameterType>>
            {
                /// <summary>
                ///     输出参数
                /// </summary>
                public TmphTcpBase.TmphParameterJsonToSerialize<TOutputParameterType> OutputParameter;

                /// <summary>
                ///     接收数据回调处理
                /// </summary>
                /// <param name="data">输出数据</param>
                protected override void onReceive(TmphMemoryPool.TmphPushSubArray data)
                {
                    var isReturn = false;
                    var TmphBuffer = data.Value.Array;
                    if (KeepCallback == null)
                    {
                        try
                        {
                            if (TmphBuffer != null && TmphDataDeSerializer.DeSerialize(data.Value, ref OutputParameter))
                                isReturn = true;
                        }
                        finally
                        {
                            var outputParameter = OutputParameter.Return;
                            OutputParameter.Return = default(TOutputParameterType);
                            push(new TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType>
                            {
                                IsReturn = isReturn,
                                Value = outputParameter
                            });
                            data.Push();
                        }
                    }
                    else if (TmphBuffer == null)
                    {
                        onlyCallback(new TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType> { IsReturn = false });
                    }
                    else
                    {
                        Monitor.Enter(this);
                        try
                        {
                            if (TmphDataDeSerializer.DeSerialize(data.Value, ref OutputParameter)) isReturn = true;
                        }
                        finally
                        {
                            Monitor.Exit(this);
                            onlyCallback(new TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType>
                            {
                                IsReturn = isReturn,
                                Value = OutputParameter.Return
                            });
                            data.Push();
                        }
                    }
                }

                /// <summary>
                ///     获取客户端命令
                /// </summary>
                /// <param name="socket">TCP客户端命令流处理套接字</param>
                /// <param name="onCall">回调委托,返回false表示失败</param>
                /// <param name="commandData">TCP调用命令</param>
                /// <param name="inputParameter">输入参数</param>
                /// <param name="maxLength">输入参数数据最大长度</param>
                /// <param name="isTask">回调是否使用任务池</param>
                /// <param name="isKeepCallback">是否保持回调</param>
                /// <returns>客户端命令</returns>
                public static TmphAsyncInputOutputJsonDataCommand<TInputParameterType, TOutputParameterType> Get
                    (TmphStreamCommandSocket socket, Action<TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType>> onGet
                        , byte[] commandData, TInputParameterType inputParameter, int maxLength,
                        TOutputParameterType outputParameter
                        , bool isTask, bool isKeepCallback)
                {
                    var command =
                        TmphTypePool<TmphAsyncInputOutputJsonDataCommand<TInputParameterType, TOutputParameterType>>.Pop();
                    if (command == null)
                    {
                        try
                        {
                            command = new TmphAsyncInputOutputJsonDataCommand<TInputParameterType, TOutputParameterType>();
                        }
                        catch (Exception error)
                        {
                            TmphLog.Error.Add(error, null, false);
                        }
                        if (command == null) return null;
                    }
                    command.Socket = socket;
                    command.Callback = onGet;
                    command.Command = commandData;
                    command.InputParameter = new TmphTcpBase.TmphParameterJsonToSerialize<TInputParameterType>
                    {
                        Return = inputParameter
                    };
                    command.MaxInputSize = maxLength;
                    command.OutputParameter = new TmphTcpBase.TmphParameterJsonToSerialize<TOutputParameterType>
                    {
                        Return = outputParameter
                    };
                    command.Identity = socket.newIndex(command.OnReceive, isTask ? (byte)1 : (byte)0,
                        isKeepCallback ? (byte)1 : (byte)0);
                    if (isKeepCallback)
                    {
                        if (command.SetKeepCallback()) return command;
                        command.InputParameter.Return = default(TInputParameterType);
                        command.OutputParameter.Return = default(TOutputParameterType);
                        command.Callback = null;
                        command.push();
                        return null;
                    }
                    return command;
                }
            }

            /// <summary>
            ///     客户端命令
            /// </summary>
            /// <typeparam name="TCommandType">客户端命令类型</typeparam>
            /// <typeparam name="TCallbackType">回调数据类型</typeparam>
            private abstract class TmphAsyncIdentityCommand<TCommandType, TCallbackType> :
                TmphAsyncCommand<TCommandType, TCallbackType>
                where TCommandType : TmphAsyncIdentityCommand<TCommandType, TCallbackType>
            {
                /// <summary>
                ///     TCP调用命令
                /// </summary>
                public int Command;

                /// <summary>
                ///     创建第一个命令输入数据
                /// </summary>
                /// <param name="stream">命令内存流</param>
                /// <returns>数据起始位置,失败返回0</returns>
                public override int BuildIndex(TmphUnmanagedStream stream)
                {
                    stream.PrepLength(sizeof(int) * 2 + sizeof(TmphCommandServer.TmphStreamIdentity));
                    var write = stream.CurrentData;
                    *(int*)(write) = Command;
                    *(TmphCommandServer.TmphStreamIdentity*)(write + sizeof(int)) = Identity;
                    *(int*)(write + (sizeof(int) + sizeof(TmphCommandServer.TmphStreamIdentity))) = 0;
                    stream.Unsafer.AddLength(sizeof(int) * 2 + sizeof(TmphCommandServer.TmphStreamIdentity));
                    return stream.Length;
                }
            }

            /// <summary>
            ///     客户端命令
            /// </summary>
            /// <typeparam name="TOutputParameterType">输出参数类型</typeparam>
            private sealed class TmphAsyncOutputIdentityCommand<TOutputParameterType> :
                TmphAsyncIdentityCommand
                    <TmphAsyncOutputIdentityCommand<TOutputParameterType>,
                        TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType>>
            {
                /// <summary>
                ///     输出参数
                /// </summary>
                public TOutputParameterType OutputParameter;

                /// <summary>
                ///     接收数据回调处理
                /// </summary>
                /// <param name="data">输出数据</param>
                protected override void onReceive(TmphMemoryPool.TmphPushSubArray data)
                {
                    var isReturn = false;
                    var TmphBuffer = data.Value.Array;
                    if (KeepCallback == null)
                    {
                        try
                        {
                            if (TmphBuffer != null && TmphDataDeSerializer.DeSerialize(data.Value, ref OutputParameter))
                                isReturn = true;
                        }
                        finally
                        {
                            var outputParameter = OutputParameter;
                            OutputParameter = default(TOutputParameterType);
                            push(new TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType>
                            {
                                IsReturn = isReturn,
                                Value = outputParameter
                            });
                            data.Push();
                        }
                    }
                    else if (TmphBuffer == null)
                    {
                        onlyCallback(new TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType> { IsReturn = false });
                    }
                    else
                    {
                        Monitor.Enter(this);
                        try
                        {
                            if (TmphDataDeSerializer.DeSerialize(data.Value, ref OutputParameter)) isReturn = true;
                        }
                        finally
                        {
                            Monitor.Exit(this);
                            onlyCallback(new TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType>
                            {
                                IsReturn = isReturn,
                                Value = OutputParameter
                            });
                            data.Push();
                        }
                    }
                }

                /// <summary>
                ///     获取客户端命令
                /// </summary>
                /// <param name="socket">TCP客户端命令流处理套接字</param>
                /// <param name="commandIdentity">TCP调用命令</param>
                /// <param name="outputParameter">输出参数</param>
                /// <param name="isTask">回调是否使用任务池</param>
                /// <param name="isKeepCallback">是否保持回调</param>
                /// <returns>客户端命令</returns>
                public static TmphAsyncOutputIdentityCommand<TOutputParameterType> Get
                    (TmphStreamCommandSocket socket, Action<TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType>> onGet
                        , int commandIdentity, TOutputParameterType outputParameter, bool isTask, bool isKeepCallback)
                {
                    var command = TmphTypePool<TmphAsyncOutputIdentityCommand<TOutputParameterType>>.Pop();
                    if (command == null)
                    {
                        try
                        {
                            command = new TmphAsyncOutputIdentityCommand<TOutputParameterType>();
                        }
                        catch (Exception error)
                        {
                            TmphLog.Error.Add(error, null, false);
                        }
                        if (command == null) return null;
                    }
                    command.Socket = socket;
                    command.Callback = onGet;
                    command.Command = commandIdentity;
                    command.OutputParameter = outputParameter;
                    command.Identity = socket.newIndex(command.OnReceive, isTask ? (byte)1 : (byte)0,
                        isKeepCallback ? (byte)1 : (byte)0);
                    if (isKeepCallback)
                    {
                        if (command.SetKeepCallback()) return command;
                        command.OutputParameter = default(TOutputParameterType);
                        command.Callback = null;
                        command.push();
                        return null;
                    }
                    return command;
                }
            }

            /// <summary>
            ///     客户端命令
            /// </summary>
            /// <typeparam name="TOutputParameterType">输出参数类型</typeparam>
            private sealed class TmphAsyncOutputJsonIdentityCommand<TOutputParameterType> :
                TmphAsyncIdentityCommand
                    <TmphAsyncOutputJsonIdentityCommand<TOutputParameterType>,
                        TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType>>
            {
                /// <summary>
                ///     输出参数
                /// </summary>
                public TmphTcpBase.TmphParameterJsonToSerialize<TOutputParameterType> OutputParameter;

                /// <summary>
                ///     接收数据回调处理
                /// </summary>
                /// <param name="data">输出数据</param>
                protected override void onReceive(TmphMemoryPool.TmphPushSubArray data)
                {
                    var isReturn = false;
                    var TmphBuffer = data.Value.Array;
                    if (KeepCallback == null)
                    {
                        try
                        {
                            if (TmphBuffer != null && TmphDataDeSerializer.DeSerialize(data.Value, ref OutputParameter))
                                isReturn = true;
                        }
                        finally
                        {
                            var outputParameter = OutputParameter.Return;
                            OutputParameter.Return = default(TOutputParameterType);
                            push(new TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType>
                            {
                                IsReturn = isReturn,
                                Value = outputParameter
                            });
                            data.Push();
                        }
                    }
                    else if (TmphBuffer == null)
                    {
                        onlyCallback(new TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType> { IsReturn = false });
                    }
                    else
                    {
                        Monitor.Enter(this);
                        try
                        {
                            if (TmphDataDeSerializer.DeSerialize(data.Value, ref OutputParameter)) isReturn = true;
                        }
                        finally
                        {
                            Monitor.Exit(this);
                            onlyCallback(new TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType>
                            {
                                IsReturn = isReturn,
                                Value = OutputParameter.Return
                            });
                            data.Push();
                        }
                    }
                }

                /// <summary>
                ///     获取客户端命令
                /// </summary>
                /// <param name="socket">TCP客户端命令流处理套接字</param>
                /// <param name="commandIdentity">TCP调用命令</param>
                /// <param name="outputParameter">输出参数</param>
                /// <param name="isTask">回调是否使用任务池</param>
                /// <param name="isKeepCallback">是否保持回调</param>
                /// <returns>客户端命令</returns>
                public static TmphAsyncOutputJsonIdentityCommand<TOutputParameterType> Get
                    (TmphStreamCommandSocket socket, Action<TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType>> onGet
                        , int commandIdentity, TOutputParameterType outputParameter, bool isTask, bool isKeepCallback)
                {
                    var command = TmphTypePool<TmphAsyncOutputJsonIdentityCommand<TOutputParameterType>>.Pop();
                    if (command == null)
                    {
                        try
                        {
                            command = new TmphAsyncOutputJsonIdentityCommand<TOutputParameterType>();
                        }
                        catch (Exception error)
                        {
                            TmphLog.Error.Add(error, null, false);
                        }
                        if (command == null) return null;
                    }
                    command.Socket = socket;
                    command.Callback = onGet;
                    command.Command = commandIdentity;
                    command.OutputParameter = new TmphTcpBase.TmphParameterJsonToSerialize<TOutputParameterType>
                    {
                        Return = outputParameter
                    };
                    command.Identity = socket.newIndex(command.OnReceive, isTask ? (byte)1 : (byte)0,
                        isKeepCallback ? (byte)1 : (byte)0);
                    if (isKeepCallback)
                    {
                        if (command.SetKeepCallback()) return command;
                        command.OutputParameter.Return = default(TOutputParameterType);
                        command.Callback = null;
                        command.push();
                        return null;
                    }
                    return command;
                }
            }

            /// <summary>
            ///     客户端命令
            /// </summary>
            /// <typeparam name="TInputParameterType">输入参数类型</typeparam>
            /// <typeparam name="TOutputParameterType">输出参数类型</typeparam>
            private sealed class TmphAsyncInputOutputIdentityCommand<TInputParameterType, TOutputParameterType> :
                TmphAsyncInputIdentityCommand
                    <TInputParameterType, TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType>,
                        TmphAsyncInputOutputIdentityCommand<TInputParameterType, TOutputParameterType>>
            {
                /// <summary>
                ///     输出参数
                /// </summary>
                public TOutputParameterType OutputParameter;

                /// <summary>
                ///     接收数据回调处理
                /// </summary>
                /// <param name="data">输出数据</param>
                protected override void onReceive(TmphMemoryPool.TmphPushSubArray data)
                {
                    var isReturn = false;
                    var TmphBuffer = data.Value.Array;
                    if (KeepCallback == null)
                    {
                        try
                        {
                            if (TmphBuffer != null && TmphDataDeSerializer.DeSerialize(data.Value, ref OutputParameter))
                                isReturn = true;
                        }
                        finally
                        {
                            var outputParameter = OutputParameter;
                            OutputParameter = default(TOutputParameterType);
                            push(new TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType>
                            {
                                IsReturn = isReturn,
                                Value = outputParameter
                            });
                            data.Push();
                        }
                    }
                    else if (TmphBuffer == null)
                    {
                        onlyCallback(new TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType> { IsReturn = false });
                    }
                    else
                    {
                        Monitor.Enter(this);
                        try
                        {
                            if (TmphDataDeSerializer.DeSerialize(data.Value, ref OutputParameter)) isReturn = true;
                        }
                        finally
                        {
                            Monitor.Exit(this);
                            onlyCallback(new TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType>
                            {
                                IsReturn = isReturn,
                                Value = OutputParameter
                            });
                            data.Push();
                        }
                    }
                }

                /// <summary>
                ///     获取客户端命令
                /// </summary>
                /// <param name="socket">TCP客户端命令流处理套接字</param>
                /// <param name="onCall">回调委托,返回false表示失败</param>
                /// <param name="commandIdentity">TCP调用命令</param>
                /// <param name="inputParameter">输入参数</param>
                /// <param name="maxLength">输入参数数据最大长度</param>
                /// <param name="isTask">回调是否使用任务池</param>
                /// <param name="isKeepCallback">是否保持回调</param>
                /// <returns>客户端命令</returns>
                public static TmphAsyncInputOutputIdentityCommand<TInputParameterType, TOutputParameterType> Get
                    (TmphStreamCommandSocket socket, Action<TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType>> onGet
                        , int commandIdentity, TInputParameterType inputParameter, int maxLength,
                        TOutputParameterType outputParameter
                        , bool isTask, bool isKeepCallback)
                {
                    var command =
                        TmphTypePool<TmphAsyncInputOutputIdentityCommand<TInputParameterType, TOutputParameterType>>.Pop();
                    if (command == null)
                    {
                        try
                        {
                            command = new TmphAsyncInputOutputIdentityCommand<TInputParameterType, TOutputParameterType>();
                        }
                        catch (Exception error)
                        {
                            TmphLog.Error.Add(error, null, false);
                        }
                        if (command == null) return null;
                    }
                    command.Socket = socket;
                    command.Callback = onGet;
                    command.Command = commandIdentity;
                    command.InputParameter = inputParameter;
                    command.MaxInputSize = maxLength;
                    command.OutputParameter = outputParameter;
                    command.Identity = socket.newIndex(command.OnReceive, isTask ? (byte)1 : (byte)0,
                        isKeepCallback ? (byte)1 : (byte)0);
                    if (isKeepCallback)
                    {
                        if (command.SetKeepCallback()) return command;
                        command.InputParameter = default(TInputParameterType);
                        command.OutputParameter = default(TOutputParameterType);
                        command.Callback = null;
                        command.push();
                        return null;
                    }
                    return command;
                }
            }

            /// <summary>
            ///     客户端命令
            /// </summary>
            /// <typeparam name="TInputParameterType">输入参数类型</typeparam>
            /// <typeparam name="TOutputParameterType">输出参数类型</typeparam>
            private sealed class TmphAsyncInputOutputJsonIdentityCommand<TInputParameterType, TOutputParameterType> :
                TmphAsyncInputIdentityCommand
                    <TmphTcpBase.TmphParameterJsonToSerialize<TInputParameterType>,
                        TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType>,
                        TmphAsyncInputOutputJsonIdentityCommand<TInputParameterType, TOutputParameterType>>
            {
                /// <summary>
                ///     输出参数
                /// </summary>
                public TmphTcpBase.TmphParameterJsonToSerialize<TOutputParameterType> OutputParameter;

                /// <summary>
                ///     接收数据回调处理
                /// </summary>
                /// <param name="data">输出数据</param>
                protected override void onReceive(TmphMemoryPool.TmphPushSubArray data)
                {
                    var isReturn = false;
                    var TmphBuffer = data.Value.Array;
                    if (KeepCallback == null)
                    {
                        try
                        {
                            if (TmphBuffer != null && TmphDataDeSerializer.DeSerialize(data.Value, ref OutputParameter))
                                isReturn = true;
                        }
                        finally
                        {
                            var outputParameter = OutputParameter.Return;
                            OutputParameter.Return = default(TOutputParameterType);
                            push(new TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType>
                            {
                                IsReturn = isReturn,
                                Value = outputParameter
                            });
                            data.Push();
                        }
                    }
                    else if (TmphBuffer == null)
                    {
                        onlyCallback(new TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType> { IsReturn = false });
                    }
                    else
                    {
                        Monitor.Enter(this);
                        try
                        {
                            if (TmphDataDeSerializer.DeSerialize(data.Value, ref OutputParameter)) isReturn = true;
                        }
                        finally
                        {
                            Monitor.Exit(this);
                            onlyCallback(new TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType>
                            {
                                IsReturn = isReturn,
                                Value = OutputParameter.Return
                            });
                            data.Push();
                        }
                    }
                }

                /// <summary>
                ///     获取客户端命令
                /// </summary>
                /// <param name="socket">TCP客户端命令流处理套接字</param>
                /// <param name="onCall">回调委托,返回false表示失败</param>
                /// <param name="commandIdentity">TCP调用命令</param>
                /// <param name="inputParameter">输入参数</param>
                /// <param name="maxLength">输入参数数据最大长度</param>
                /// <param name="isTask">回调是否使用任务池</param>
                /// <param name="isKeepCallback">是否保持回调</param>
                /// <returns>客户端命令</returns>
                public static TmphAsyncInputOutputJsonIdentityCommand<TInputParameterType, TOutputParameterType> Get
                    (TmphStreamCommandSocket socket, Action<TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType>> onGet
                        , int commandIdentity, TInputParameterType inputParameter, int maxLength,
                        TOutputParameterType outputParameter
                        , bool isTask, bool isKeepCallback)
                {
                    var command =
                        TmphTypePool<TmphAsyncInputOutputJsonIdentityCommand<TInputParameterType, TOutputParameterType>>.Pop();
                    if (command == null)
                    {
                        try
                        {
                            command =
                                new TmphAsyncInputOutputJsonIdentityCommand<TInputParameterType, TOutputParameterType>();
                        }
                        catch (Exception error)
                        {
                            TmphLog.Error.Add(error, null, false);
                        }
                        if (command == null) return null;
                    }
                    command.Socket = socket;
                    command.Callback = onGet;
                    command.Command = commandIdentity;
                    command.InputParameter = new TmphTcpBase.TmphParameterJsonToSerialize<TInputParameterType>
                    {
                        Return = inputParameter
                    };
                    command.MaxInputSize = maxLength;
                    command.OutputParameter = new TmphTcpBase.TmphParameterJsonToSerialize<TOutputParameterType>
                    {
                        Return = outputParameter
                    };
                    command.Identity = socket.newIndex(command.OnReceive, isTask ? (byte)1 : (byte)0,
                        isKeepCallback ? (byte)1 : (byte)0);
                    if (isKeepCallback)
                    {
                        if (command.SetKeepCallback()) return command;
                        command.InputParameter.Return = default(TInputParameterType);
                        command.OutputParameter.Return = default(TOutputParameterType);
                        command.Callback = null;
                        command.push();
                        return null;
                    }
                    return command;
                }
            }

            /// <summary>
            ///     客户端命令
            /// </summary>
            /// <typeparam name="TInputParameterType">输入参数类型</typeparam>
            /// <typeparam name="TCallbackType">回调数据类型</typeparam>
            /// <typeparam name="TCommandType">客户端命令类型</typeparam>
            private abstract class TmphAsyncInputDataCommand<TInputParameterType, TCallbackType, TCommandType> :
                TmphAsyncDataCommand<TCommandType, TCallbackType>
                where TCommandType : TmphAsyncInputDataCommand<TInputParameterType, TCallbackType, TCommandType>
            {
                /// <summary>
                ///     输入参数
                /// </summary>
                public TInputParameterType InputParameter;

                /// <summary>
                ///     输入参数数据最大长度
                /// </summary>
                public int MaxInputSize;

                /// <summary>
                ///     创建第一个命令输入数据
                /// </summary>
                /// <param name="stream">命令内存流</param>
                /// <returns>数据起始位置,失败返回0</returns>
                public override int BuildIndex(TmphUnmanagedStream stream)
                {
                    int streamLength = stream.Length,
                        commandLength = Command.Length + sizeof(TmphCommandServer.TmphStreamIdentity) + sizeof(int);
                    stream.PrepLength(commandLength);
                    stream.Unsafer.AddLength(commandLength);
                    var serializeIndex = stream.Length;
                    TmphDataSerializer.Serialize(InputParameter, stream);
                    var dataLength = stream.Length - serializeIndex;
                    InputParameter = default(TInputParameterType);
                    if (dataLength <= MaxInputSize)
                    {
                        var write = stream.Data + streamLength;
                        Unsafe.TmphMemory.Copy(Command, write, Command.Length);
                        *(TmphCommandServer.TmphStreamIdentity*)(write += Command.Length) = Identity;
                        *(int*)(write + sizeof(TmphCommandServer.TmphStreamIdentity)) = dataLength;
                        return stream.Length - dataLength;
                    }
                    stream.Unsafer.SetLength(streamLength);
                    return 0;
                }
            }

            /// <summary>
            ///     客户端命令
            /// </summary>
            /// <typeparam name="TInputParameterType">输入参数类型</typeparam>
            private sealed class TmphAsyncInputDataCommand<TInputParameterType> :
                TmphAsyncInputDataCommand<TInputParameterType, bool, TmphAsyncInputDataCommand<TInputParameterType>>
            {
                /// <summary>
                ///     接收数据回调处理
                /// </summary>
                /// <param name="data">输出数据</param>
                protected override void onReceive(TmphMemoryPool.TmphPushSubArray data)
                {
                    if (data.Value.Array == null)
                    {
                        if (KeepCallback == null) push(false);
                        else onlyCallback(false);
                    }
                    else
                    {
                        if (KeepCallback == null) push(true);
                        else onlyCallback(true);
                    }
                }

                /// <summary>
                ///     获取客户端命令
                /// </summary>
                /// <param name="socket">TCP客户端命令流处理套接字</param>
                /// <param name="onCall">回调委托,返回false表示失败</param>
                /// <param name="commandData">TCP调用命令</param>
                /// <param name="inputParameter">输入参数</param>
                /// <param name="maxLength">输入参数数据最大长度</param>
                /// <param name="isTask">回调是否使用任务池</param>
                /// <param name="isKeepCallback">是否保持回调</param>
                /// <returns>客户端命令</returns>
                public static TmphAsyncInputDataCommand<TInputParameterType> Get(TmphStreamCommandSocket socket,
                    Action<bool> onCall
                    , byte[] commandData, TInputParameterType inputParameter, int maxLength, bool isTask,
                    bool isKeepCallback)
                {
                    var command = TmphTypePool<TmphAsyncInputDataCommand<TInputParameterType>>.Pop();
                    if (command == null)
                    {
                        try
                        {
                            command = new TmphAsyncInputDataCommand<TInputParameterType>();
                        }
                        catch (Exception error)
                        {
                            TmphLog.Error.Add(error, null, false);
                        }
                        if (command == null) return null;
                    }
                    command.Socket = socket;
                    command.Callback = onCall;
                    command.Command = commandData;
                    command.InputParameter = inputParameter;
                    command.MaxInputSize = maxLength;
                    command.Identity = socket.newIndex(command.OnReceive, isTask ? (byte)1 : (byte)0,
                        isKeepCallback ? (byte)1 : (byte)0);
                    if (isKeepCallback)
                    {
                        if (command.SetKeepCallback()) return command;
                        command.InputParameter = default(TInputParameterType);
                        command.Callback = null;
                        command.push();
                        return null;
                    }
                    return command;
                }
            }

            /// <summary>
            ///     客户端命令
            /// </summary>
            private sealed class TmphAsyncDataCommand : TmphAsyncDataCommand<TmphAsyncDataCommand, bool>
            {
                /// <summary>
                ///     接收数据回调处理
                /// </summary>
                /// <param name="data">输出数据</param>
                protected override void onReceive(TmphMemoryPool.TmphPushSubArray data)
                {
                    if (data.Value.Array == null)
                    {
                        if (KeepCallback == null) push(false);
                        else onlyCallback(false);
                    }
                    else
                    {
                        if (KeepCallback == null) push(true);
                        else onlyCallback(true);
                    }
                }

                /// <summary>
                ///     获取客户端命令
                /// </summary>
                /// <param name="socket">TCP客户端命令流处理套接字</param>
                /// <param name="onCall">回调委托,返回false表示失败</param>
                /// <param name="commandData">TCP调用命令</param>
                /// <param name="isTask">回调是否使用任务池</param>
                /// <param name="isKeepCallback">是否保持回调</param>
                /// <returns>客户端命令</returns>
                public static TmphAsyncDataCommand Get(TmphStreamCommandSocket socket, Action<bool> onCall,
                    byte[] commandData, bool isTask, bool isKeepCallback)
                {
                    var command = TmphTypePool<TmphAsyncDataCommand>.Pop();
                    if (command == null)
                    {
                        try
                        {
                            command = new TmphAsyncDataCommand();
                        }
                        catch (Exception error)
                        {
                            TmphLog.Error.Add(error, null, false);
                        }
                        if (command == null) return null;
                    }
                    command.Socket = socket;
                    command.Callback = onCall;
                    command.Command = commandData;
                    command.Identity = socket.newIndex(command.OnReceive, isTask ? (byte)1 : (byte)0,
                        isKeepCallback ? (byte)1 : (byte)0);
                    if (isKeepCallback)
                    {
                        if (command.SetKeepCallback()) return command;
                        command.Callback = null;
                        command.push();
                        return null;
                    }
                    return command;
                }
            }

            /// <summary>
            ///     客户端命令
            /// </summary>
            /// <typeparam name="TInputParameterType">输入参数类型</typeparam>
            /// <typeparam name="TCallbackType">回调数据类型</typeparam>
            /// <typeparam name="TCommandType">客户端命令类型</typeparam>
            private abstract class TmphAsyncInputIdentityCommand<TInputParameterType, TCallbackType, TCommandType> :
                TmphAsyncIdentityCommand<TCommandType, TCallbackType>
                where TCommandType : TmphAsyncInputIdentityCommand<TInputParameterType, TCallbackType, TCommandType>
            {
                /// <summary>
                ///     输入参数
                /// </summary>
                public TInputParameterType InputParameter;

                /// <summary>
                ///     输入参数数据最大长度
                /// </summary>
                public int MaxInputSize;

                /// <summary>
                ///     创建第一个命令输入数据
                /// </summary>
                /// <param name="stream">命令内存流</param>
                /// <returns>数据起始位置,失败返回0</returns>
                public override int BuildIndex(TmphUnmanagedStream stream)
                {
                    var streamLength = stream.Length;
                    stream.PrepLength(sizeof(int) * 2 + sizeof(TmphCommandServer.TmphStreamIdentity));
                    stream.Unsafer.AddLength(sizeof(int) * 2 + sizeof(TmphCommandServer.TmphStreamIdentity));
                    TmphDataSerializer.Serialize(InputParameter, stream);
                    var dataLength = stream.Length - streamLength -
                                     (sizeof(int) * 2 + sizeof(TmphCommandServer.TmphStreamIdentity));
                    InputParameter = default(TInputParameterType);
                    if (dataLength <= MaxInputSize)
                    {
                        var write = stream.Data + streamLength;
                        *(int*)(write) = Command;
                        *(TmphCommandServer.TmphStreamIdentity*)(write + sizeof(int)) = Identity;
                        *(int*)(write + (sizeof(int) + sizeof(TmphCommandServer.TmphStreamIdentity))) = dataLength;
                        return stream.Length - dataLength;
                    }
                    stream.Unsafer.SetLength(streamLength);
                    return 0;
                }
            }

            /// <summary>
            ///     客户端命令
            /// </summary>
            /// <typeparam name="TInputParameterType">输入参数类型</typeparam>
            private sealed class TmphAsyncInputIdentityCommand<TInputParameterType> :
                TmphAsyncInputIdentityCommand<TInputParameterType, bool, TmphAsyncInputIdentityCommand<TInputParameterType>>
            {
                /// <summary>
                ///     接收数据回调处理
                /// </summary>
                /// <param name="data">输出数据</param>
                protected override void onReceive(TmphMemoryPool.TmphPushSubArray data)
                {
                    if (data.Value.Array == null)
                    {
                        if (KeepCallback == null) push(false);
                        else onlyCallback(false);
                    }
                    else
                    {
                        if (KeepCallback == null) push(true);
                        else onlyCallback(true);
                    }
                }

                /// <summary>
                ///     获取客户端命令
                /// </summary>
                /// <param name="socket">TCP客户端命令流处理套接字</param>
                /// <param name="onCall">回调委托,返回false表示失败</param>
                /// <param name="commandIdentity">TCP调用命令</param>
                /// <param name="inputParameter">输入参数</param>
                /// <param name="maxLength">输入参数数据最大长度</param>
                /// <param name="isTask">回调是否使用任务池</param>
                /// <param name="isKeepCallback">是否保持回调</param>
                /// <returns>客户端命令</returns>
                public static TmphAsyncInputIdentityCommand<TInputParameterType> Get(TmphStreamCommandSocket socket,
                    Action<bool> onCall
                    , int commandIdentity, TInputParameterType inputParameter, int maxLength, bool isTask,
                    bool isKeepCallback)
                {
                    var command = TmphTypePool<TmphAsyncInputIdentityCommand<TInputParameterType>>.Pop();
                    if (command == null)
                    {
                        try
                        {
                            command = new TmphAsyncInputIdentityCommand<TInputParameterType>();
                        }
                        catch (Exception error)
                        {
                            TmphLog.Error.Add(error, null, false);
                        }
                        if (command == null) return null;
                    }
                    command.Socket = socket;
                    command.Callback = onCall;
                    command.Command = commandIdentity;
                    command.InputParameter = inputParameter;
                    command.MaxInputSize = maxLength;
                    command.Identity = socket.newIndex(command.OnReceive, isTask ? (byte)1 : (byte)0,
                        isKeepCallback ? (byte)1 : (byte)0);
                    if (isKeepCallback)
                    {
                        if (command.SetKeepCallback()) return command;
                        command.InputParameter = default(TInputParameterType);
                        command.Callback = null;
                        command.push();
                        return null;
                    }
                    return command;
                }
            }

            /// <summary>
            ///     客户端命令
            /// </summary>
            private sealed class TmphAsyncIdentityCommand : TmphAsyncIdentityCommand<TmphAsyncIdentityCommand, bool>
            {
                /// <summary>
                ///     接收数据回调处理
                /// </summary>
                /// <param name="data">输出数据</param>
                protected override void onReceive(TmphMemoryPool.TmphPushSubArray data)
                {
                    if (data.Value.Array == null)
                    {
                        if (KeepCallback == null) push(false);
                        else onlyCallback(false);
                    }
                    else
                    {
                        if (KeepCallback == null) push(true);
                        else onlyCallback(true);
                    }
                }

                /// <summary>
                ///     获取客户端命令
                /// </summary>
                /// <param name="socket">TCP客户端命令流处理套接字</param>
                /// <param name="onCall">回调委托,返回false表示失败</param>
                /// <param name="commandIdentity">TCP调用命令</param>
                /// <param name="isTask">回调是否使用任务池</param>
                /// <param name="isKeepCallback">是否保持回调</param>
                /// <returns>客户端命令</returns>
                public static TmphAsyncIdentityCommand Get
                    (TmphStreamCommandSocket socket, Action<bool> onCall, int commandIdentity, bool isTask,
                        bool isKeepCallback)
                {
                    var command = TmphTypePool<TmphAsyncIdentityCommand>.Pop();
                    if (command == null)
                    {
                        try
                        {
                            command = new TmphAsyncIdentityCommand();
                        }
                        catch (Exception error)
                        {
                            TmphLog.Error.Add(error, null, false);
                        }
                        if (command == null) return null;
                    }
                    command.Socket = socket;
                    command.Callback = onCall;
                    command.Command = commandIdentity;
                    command.Identity = socket.newIndex(command.OnReceive, isTask ? (byte)1 : (byte)0,
                        isKeepCallback ? (byte)1 : (byte)0);
                    if (isKeepCallback)
                    {
                        if (command.SetKeepCallback()) return command;
                        command.Callback = null;
                        command.push();
                        return null;
                    }
                    return command;
                }
            }
        }

        /// <summary>
        ///     TCP参数流
        /// </summary>
        private struct TmphTcpStream
        {
            /// <summary>
            ///     当前序号
            /// </summary>
            public int Identity;

            /// <summary>
            ///     字节流
            /// </summary>
            public Stream Stream;

            /// <summary>
            ///     设置TCP参数流
            /// </summary>
            /// <param name="stream">字节流</param>
            /// <returns>当前序号</returns>
            public int Set(Stream stream)
            {
                Stream = stream;
                return Identity;
            }

            /// <summary>
            ///     获取TCP参数流
            /// </summary>
            /// <param name="identity">当前序号</param>
            /// <returns>TCP参数流</returns>
            public Stream Get(int identity)
            {
                return identity == Identity ? Stream : null;
            }

            /// <summary>
            ///     取消TCP参数流
            /// </summary>
            /// <returns>字节流</returns>
            public Stream Cancel()
            {
                ++Identity;
                var stream = Stream;
                Stream = null;
                return stream;
            }

            /// <summary>
            ///     关闭TCP参数流
            /// </summary>
            /// <param name="identity">当前序号</param>
            public void Close(int identity)
            {
                if (identity == Identity)
                {
                    ++Identity;
                    Stream = null;
                }
            }
        }
    }

    /// <summary>
    ///     TCP调用客户端(tcpServer)
    /// </summary>
    /// <typeparam name="TClientType">客户端类型</typeparam>
    public class TmphCommandClient<TClientType> : TmphCommandClient
    {
        /// <summary>
        ///     验证函数客户端
        /// </summary>
        private readonly TClientType TmphClient;

        /// <summary>
        ///     验证函数接口
        /// </summary>
        private readonly TmphTcpBase.ITcpClientVerifyMethod<TClientType> verifyMethod;

        /// <summary>
        ///     TCP调用服务端
        /// </summary>
        /// <param name="attribute">配置信息</param>
        /// <param name="isStart">是否启动客户端</param>
        /// <param name="maxCommandLength">最大命令长度</param>
        /// <param name="verify">验证接口</param>
        public TmphCommandClient(TmphTcpServer attribute, int maxCommandLength, TmphTcpBase.ITcpClientVerify verify)
            : base(attribute, maxCommandLength, verify)
        {
        }

        /// <summary>
        ///     TCP调用服务端
        /// </summary>
        /// <param name="attribute">配置信息</param>
        /// <param name="isStart">是否启动客户端</param>
        /// <param name="maxCommandLength">最大命令长度</param>
        /// <param name="verifyMethod">验证函数接口</param>
        /// <param name="TmphClient">验证函数客户端</param>
        public TmphCommandClient(TmphTcpServer attribute, int maxCommandLength,
            TmphTcpBase.ITcpClientVerifyMethod<TClientType> verifyMethod, TClientType TmphClient)
            : base(attribute, maxCommandLength, (TmphTcpBase.ITcpClientVerify)null)
        {
            this.verifyMethod = verifyMethod;
            this.TmphClient = TmphClient;
        }

        /// <summary>
        ///     TCP调用服务端
        /// </summary>
        /// <param name="attribute">配置信息</param>
        /// <param name="isStart">是否启动客户端</param>
        /// <param name="maxCommandLength">最大命令长度</param>
        /// <param name="verifyMethod">验证函数接口</param>
        /// <param name="TmphClient">验证函数客户端</param>
        /// <param name="verify">验证接口</param>
        public TmphCommandClient(TmphTcpServer attribute, int maxCommandLength,
            TmphTcpBase.ITcpClientVerifyMethod<TClientType> verifyMethod, TClientType TmphClient,
            TmphTcpBase.ITcpClientVerify verify)
            : base(attribute, maxCommandLength, verify)
        {
            this.verifyMethod = verifyMethod;
            this.TmphClient = TmphClient;
        }

        /// <summary>
        ///     函数验证
        /// </summary>
        /// <returns>是否验证成功</returns>
        protected override bool callVerifyMethod()
        {
            if (verifyMethod == null) return true;
            var isError = false;
            TmphInterlocked.CompareSetSleep1(ref verifyMethodLock);
            try
            {
                if (verifyMethod.Verify(TmphClient)) return true;
            }
            catch (Exception error)
            {
                isError = true;
                TmphLog.Error.Add(error, "TCP客户端验证失败", false);
            }
            finally
            {
                verifyMethodLock = 0;
            }
            if (!isError) TmphLog.Error.Add("TCP客户端验证失败", true, false);
            Dispose();
            return false;
        }
    }
}