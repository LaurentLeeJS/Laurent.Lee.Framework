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
using Laurent.Lee.CLB.Emit;
using Laurent.Lee.CLB.Threading;
using System;
using System.Threading;

namespace Laurent.Lee.CLB.Net.Tcp
{
    /// <summary>
    ///     TCP调用负载均衡服务端
    /// </summary>
    public abstract class TmphCommandLoadBalancingServer
    {
        /// <summary>
        ///     添加TCP调用服务端命令索引位置
        /// </summary>
        internal const int NewServerCommandIndex = TmphCommandServer.CommandStartIndex + 1;

        /// <summary>
        ///     移除TCP调用服务端命令索引位置
        /// </summary>
        internal const int RemoveServerCommandIndex = NewServerCommandIndex + 1;

        /// <summary>
        ///     最大错误间隔时钟周期
        /// </summary>
        protected static readonly long maxErrorTimeTicks = new TimeSpan(0, 0, 2).Ticks;

        /// <summary>
        ///     验证接口
        /// </summary>
        protected TmphTcpBase.ITcpClientVerify _verify_;

        /// <summary>
        ///     TCP调用服务端信息
        /// </summary>
        protected struct TmphServerInfo
        {
            /// <summary>
            ///     TCP调用服务端端口信息
            /// </summary>
            public TmphHost Host;

            /// <summary>
            ///     TCP调用套接字回话标识
            /// </summary>
            public TmphCommandServer.TmphStreamIdentity Identity;

            /// <summary>
            ///     TCP调用套接字
            /// </summary>
            public TmphCommandServer.TmphSocket Socket;
        }

        /// <summary>
        ///     TCP调用负载均衡客户端
        /// </summary>
        public sealed class TmphCommandClient : IDisposable
        {
            /// <summary>
            ///     TCP调用客户端
            /// </summary>
            private TmphCommandClient<TmphCommandClient> TmphClient;

            /// <summary>
            ///     TCP调用客户端
            /// </summary>
            /// <param name="attribute">TCP调用服务器端配置信息</param>
            public TmphCommandClient(TmphTcpServer attribute) : this(attribute, null)
            {
            }

            /// <summary>
            ///     TCP调用客户端
            /// </summary>
            /// <param name="attribute">TCP调用服务器端配置信息</param>
            /// <param name="verifyMethod">TCP验证方法</param>
            public TmphCommandClient(TmphTcpServer attribute, TmphTcpBase.ITcpClientVerifyMethod<TmphCommandClient> verifyMethod)
            {
                if (attribute == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                TmphClient = new TmphCommandClient<TmphCommandClient>(attribute, 1024, verifyMethod ?? new TmphVerifyMethod(),
                    this);
            }

            /// <summary>
            ///     释放资源
            /// </summary>
            public void Dispose()
            {
                TmphPub.Dispose(ref TmphClient);
            }

            /// <summary>
            ///     TCP调用客户端验证
            /// </summary>
            /// <param name="verify">验证字符串</param>
            /// <returns>是否验证成功</returns>
            public bool Verify(string verify)
            {
                var wait = getWait(true);
                if (wait.Value != null)
                {
                    try
                    {
                        wait.Key.Get(wait.Value.OnReturn, TmphCommandServer.CommandStartIndex,
                            Config.TmphTcpRegister.Default.Verify, 1024, false, false, false);
                        var value = wait.Value.Value;
                        return value.IsReturn && value.Value;
                    }
                    catch (Exception error)
                    {
                        TmphLog.Error.Add(error, null, false);
                    }
                }
                return false;
            }

            /// <summary>
            ///     添加TCP调用服务端
            /// </summary>
            /// <param name="host">TCP调用服务端端口信息</param>
            /// <returns>是否添加成功</returns>
            public bool NewServer(TmphHost host)
            {
                var wait = getWait(false);
                if (wait.Value != null)
                {
                    try
                    {
                        wait.Key.Get(wait.Value.OnReturn, NewServerCommandIndex, host, 1024, false, false, false);
                        var value = wait.Value.Value;
                        return value.IsReturn && value.Value;
                    }
                    catch (Exception error)
                    {
                        TmphLog.Error.Add(error, null, false);
                    }
                }
                return false;
            }

            /// <summary>
            ///     添加TCP调用服务端
            /// </summary>
            /// <param name="host">TCP调用服务端端口信息</param>
            /// <param name="onReturn">创建完成回调处理</param>
            public void NewServer(TmphHost host, Action<bool> onReturn)
            {
                try
                {
                    TmphClient.StreamSocket.Get(
                        onReturn == null
                            ? null
                            : (Action<TmphAsynchronousMethod.TmphReturnValue<bool>>)
                                new TmphServerReturn { OnReturn = onReturn }.OnNewServer, NewServerCommandIndex, host, 1024,
                        false, true, false);
                    return;
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
                if (onReturn != null) onReturn(false);
            }

            /// <summary>
            ///     移除TCP调用服务端
            /// </summary>
            /// <param name="host">TCP调用服务端端口信息</param>
            /// <returns>是否移除成功</returns>
            public bool RemoveServer(TmphHost host)
            {
                var wait = getWait(false);
                if (wait.Value != null)
                {
                    try
                    {
                        wait.Key.Get(wait.Value.OnReturn, RemoveServerCommandIndex, host, 1024, false, false, false);
                        var value = wait.Value.Value;
                        return value.IsReturn && value.Value;
                    }
                    catch (Exception error)
                    {
                        TmphLog.Error.Add(error, null, false);
                    }
                }
                return false;
            }

            /// <summary>
            ///     获取同步等待调用
            /// </summary>
            /// <param name="isVerify">是否验证调用</param>
            /// <returns>TCP客户端套接字+同步等待调用</returns>
            private TmphKeyValue<Tcp.TmphCommandClient.TmphStreamCommandSocket, TmphAsynchronousMethod.TmphWaitCall<bool>> getWait(
                bool isVerify)
            {
                try
                {
                    if (TmphClient != null)
                    {
                        var socket = isVerify ? TmphClient.VerifyStreamSocket : TmphClient.StreamSocket;
                        if (socket != null)
                        {
                            return
                                new TmphKeyValue
                                    <Tcp.TmphCommandClient.TmphStreamCommandSocket, TmphAsynchronousMethod.TmphWaitCall<bool>>(
                                    socket, TmphAsynchronousMethod.TmphWaitCall<bool>.Get());
                        }
                    }
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
                return
                    default(TmphKeyValue<Tcp.TmphCommandClient.TmphStreamCommandSocket, TmphAsynchronousMethod.TmphWaitCall<bool>>);
            }

            /// <summary>
            ///     添加TCP调用服务端回调处理
            /// </summary>
            private sealed class TmphServerReturn
            {
                /// <summary>
                ///     创建完成回调处理
                /// </summary>
                public Action<bool> OnReturn;

                /// <summary>
                ///     创建完成回调处理
                /// </summary>
                /// <param name="returnValue">返回值</param>
                public void OnNewServer(TmphAsynchronousMethod.TmphReturnValue<bool> returnValue)
                {
                    OnReturn(returnValue.IsReturn && returnValue.Value);
                }
            }
        }
    }

    /// <summary>
    ///     TCP调用负载均衡服务端
    /// </summary>
    /// <typeparam name="TClientType">TCP调用客户端类型</typeparam>
    public abstract class TmphCommandLoadBalancingServer<TClientType> : TmphCommandLoadBalancingServer, IDisposable
        where TClientType : class, IDisposable
    {
        /// <summary>
        ///     TCP调用服务器端配置信息
        /// </summary>
        private readonly TmphTcpServer attribute;

        /// <summary>
        ///     检测任务
        /// </summary>
        private readonly Action checkHandle;

        /// <summary>
        ///     TCP调用客户端空闲索引
        /// </summary>
        private readonly TmphList<int> freeIndexs = new TmphList<int>();

        /// <summary>
        ///     添加TCP调用服务端
        /// </summary>
        private readonly Action<TmphServerInfo> newServerHandle;

        /// <summary>
        ///     TCP调用负载均衡服务端访问锁
        /// </summary>
        private readonly object serverLock = new object();

        /// <summary>
        ///     验证函数接口
        /// </summary>
        protected TmphTcpBase.ITcpClientVerifyMethod<TClientType> _verifyMethod_;

        /// <summary>
        ///     当前调用总数
        /// </summary>
        private int callCount;

        /// <summary>
        ///     超时检测时钟周期
        /// </summary>
        private long checkTicks;

        /// <summary>
        ///     最后一次检测时间
        /// </summary>
        private DateTime checkTime;

        /// <summary>
        ///     TCP调用客户端访问锁
        /// </summary>
        private int clientLock;

        /// <summary>
        ///     TCP调用客户端集合
        /// </summary>
        private TmphClientHost[] clients = new TmphClientHost[sizeof(int)];

        /// <summary>
        ///     已使用的TCP调用客户端数量(包括空闲索引)
        /// </summary>
        private int currentCount;

        /// <summary>
        ///     当前访问TCP调用客户端索引
        /// </summary>
        private int currentIndex;

        /// <summary>
        ///     是否启动检测任务
        /// </summary>
        private byte isCheckTask;

        /// <summary>
        ///     是否已经释放资源
        /// </summary>
        private byte isDisposed;

        /// <summary>
        ///     移除TCP调用客户端集合
        /// </summary>
        private TmphKeyValue<TClientType, int>[] removeClients = TmphNullValue<TmphKeyValue<TClientType, int>>.Array;

        /// <summary>
        ///     TCP调用负载均衡服务端
        /// </summary>
        private TmphCommandServer server;

        /// <summary>
        ///     TCP调用负载均衡服务端
        /// </summary>
        private TmphCommandLoadBalancingServer()
        {
            checkHandle = check;
        }

        /// <summary>
        ///     TCP调用负载均衡服务端
        /// </summary>
        /// <param name="attribute">TCP调用服务器端配置信息</param>
        /// <param name="verifyMethod">验证函数接口</param>
        /// <param name="verify">验证接口</param>
        protected TmphCommandLoadBalancingServer(TmphTcpServer attribute,
            TmphTcpBase.ITcpClientVerifyMethod<TClientType> verifyMethod, TmphTcpBase.ITcpClientVerify verify)
        {
        }

        /// <summary>
        ///     TCP调用负载均衡服务端
        /// </summary>
        /// <param name="attribute">TCP调用服务器端配置信息</param>
        /// <param name="verifyMethod">验证函数接口</param>
        /// <param name="verify">验证接口</param>
        protected TmphCommandLoadBalancingServer(TmphTcpServer attribute,
            TmphTcpBase.ITcpClientVerifyMethod<TClientType> verifyMethod)
            : this()
        {
            this.attribute = attribute;
            _verifyMethod_ = verifyMethod;
        }

        /// <summary>
        ///     TCP调用负载均衡服务端
        /// </summary>
        /// <param name="attribute">TCP调用服务器端配置信息</param>
        /// <param name="verify">验证接口</param>
        protected TmphCommandLoadBalancingServer(TmphTcpServer attribute, TmphTcpBase.ITcpClientVerify verify)
            : this()
        {
            this.attribute = attribute;
            _verify_ = verify;
            newServerHandle = newServer;
        }

        /// <summary>
        ///     释放资源
        /// </summary>
        public void Dispose()
        {
            isDisposed = 1;
            Monitor.Enter(serverLock);
            TmphPub.Dispose(ref server);
            Monitor.Exit(serverLock);
            var count = 0;
            TClientType[] clients;
            do
            {
                clients = new TClientType[this.clients.Length];
                TmphInterlocked.NoCheckCompareSetSleep0(ref clientLock);
                currentIndex = callCount = 0;
                if (currentCount <= clients.Length)
                {
                    while (count != currentCount) clients[count++] = this.clients[count++].Client.GetRemove();
                    freeIndexs.Clear();
                    currentCount = 0;
                    clientLock = 0;
                    break;
                }
                clientLock = 0;
            } while (true);
            while (count != 0) TmphPub.Dispose(ref clients[--count]);
        }

        /// <summary>
        ///     启动负载均衡服务
        /// </summary>
        /// <param name="isVerify">自定义TCP调用服务端验证</param>
        /// <returns>是否成功</returns>
        public bool StartLoadBalancingServer(Func<TmphSubArray<byte>, bool> isVerify = null)
        {
            attribute.IsLoadBalancing = false;
            checkTicks = new TimeSpan(0, 0, Math.Max(attribute.LoadBalancingCheckSeconds + 2, 2)).Ticks;
            Monitor.Enter(serverLock);
            try
            {
                if (server == null)
                {
                    server = new TmphCommandServer(this, isVerify);
                    if (server.Start()) return true;
                    TmphPub.Dispose(ref server);
                }
            }
            finally
            {
                Monitor.Exit(serverLock);
            }
            return false;
        }

        /// <summary>
        ///     获取一个TCP调用客户端
        /// </summary>
        /// <returns>TCP调用服务器信息</returns>
        protected TmphClientIdentity _getClient_()
        {
            if (isDisposed == 0)
            {
                TmphInterlocked.NoCheckCompareSetSleep0(ref clientLock);
                var count = currentCount - freeIndexs.Count;
                if (count != 0)
                {
                    int callCount = this.callCount / count + 1, index = currentIndex;
                    do
                    {
                        if (clients[currentIndex].TryCount(callCount))
                        {
                            ++this.callCount;
                            var value = clients[currentIndex].Client;
                            clientLock = 0;
                            return value;
                        }
                    } while (++currentIndex != currentCount);
                    for (currentIndex = 0; currentIndex != index; ++currentIndex)
                    {
                        if (clients[currentIndex].TryCount(callCount))
                        {
                            ++this.callCount;
                            var value = clients[currentIndex].Client;
                            clientLock = 0;
                            return value;
                        }
                    }
                }
                clientLock = 0;
            }
            return default(TmphClientIdentity);
        }

        /// <summary>
        ///     TCP调用客户端调用结束
        /// </summary>
        /// <param name="TmphClient">TCP调用服务器信息</param>
        /// <param name="isReturn">是否回调成功</param>
        protected void _end_(ref TmphClientIdentity TmphClient, bool isReturn)
        {
            if (isDisposed == 0)
            {
                if (isReturn)
                {
                    TmphInterlocked.NoCheckCompareSetSleep0(ref clientLock);
                    callCount -= clients[TmphClient.Index].End(TmphClient.Identity);
                    clientLock = 0;
                }
                else
                {
                    TmphInterlocked.NoCheckCompareSetSleep0(ref clientLock);
                    var errorClient = clients[TmphClient.Index].Error(TmphClient.Identity);
                    if (errorClient.Key == null) clientLock = 0;
                    else
                    {
                        callCount -= errorClient.Value.Value;
                        try
                        {
                            freeIndexs.Add(TmphClient.Index);
                        }
                        finally
                        {
                            clientLock = 0;
                            TmphPub.Dispose(ref errorClient.Key);

                            var host = errorClient.Value.Key;
                            var isCreate = newServer(host);
                            if (isCreate)
                            {
                                tryCheck();
                                TmphLog.Default.Add("恢复TCP调用服务端[调用错误] " + host.Host + ":" + host.Port.toString(), false,
                                    false);
                            }
                            else
                                TmphLog.Default.Add("移除TCP调用服务端[调用错误] " + host.Host + ":" + host.Port.toString(), false,
                                    false);
                        }
                    }
                }
            }
            TmphClient.Identity = int.MinValue;
            TmphClient.Client = null;
        }

        /// <summary>
        ///     创建TCP调用客户端
        /// </summary>
        /// <param name="TmphClient">TCP调用服务器端配置信息</param>
        /// <returns>TCP调用客户端</returns>
        protected abstract TClientType _createClient_(TmphTcpServer attribute);

        /// <summary>
        ///     获取负载均衡联通最后检测时间
        /// </summary>
        /// <param name="TmphClient">TCP调用客户端</param>
        /// <returns>负载均衡联通最后检测时间</returns>
        protected abstract DateTime _loadBalancingCheckTime_(TClientType TmphClient);

        /// <summary>
        ///     负载均衡超时检测
        /// </summary>
        /// <param name="TmphClient">TCP调用客户端</param>
        /// <returns>TCP调用客户端是否可用</returns>
        protected abstract bool _loadBalancingCheck_(TClientType TmphClient);

        /// <summary>
        ///     添加TCP调用服务端
        /// </summary>
        /// <param name="server">TCP调用服务端信息</param>
        private void newServer(TmphServerInfo server)
        {
            var isCreate = newServer(server.Host);
            if (isCreate)
            {
                tryCheck();
                TmphLog.Default.Add("添加TCP调用服务端 " + server.Host.Host + ":" + server.Host.Port.toString(), false, false);
            }
            server.Socket.SendStream(server.Identity,
                new TmphAsynchronousMethod.TmphReturnValue<bool> { IsReturn = true, Value = isCreate });
        }

        /// <summary>
        ///     添加TCP调用服务端
        /// </summary>
        /// <param name="host">TCP服务端口信息</param>
        /// <param name="isCheckTask">是否启动检测任务</param>
        /// <returns>是否添加成功</returns>
        private bool newServer(TmphHost host)
        {
            if (isDisposed == 0)
            {
                try
                {
                    var attribute = TmphMemberCopyer<TmphTcpServer>.MemberwiseClone(this.attribute);
                    attribute.IsLoadBalancing = true;
                    attribute.Host = host.Host;
                    attribute.Port = host.Port;
                    var TmphClient = _createClient_(attribute);
                    if (TmphClient != null)
                    {
                        TmphInterlocked.NoCheckCompareSetSleep0(ref clientLock);
                        for (var index = 0; index != currentCount; ++index)
                        {
                            var removeClient = clients[index].ReSet(host, TmphClient);
                            if (removeClient.Key != null)
                            {
                                callCount -= removeClient.Value;
                                clientLock = 0;
                                TmphPub.Dispose(ref removeClient.Key);
                                return true;
                            }
                        }
                        if (freeIndexs.Count == 0)
                        {
                            if (currentCount == this.clients.Length)
                            {
                                try
                                {
                                    var clients = new TmphClientHost[currentCount << 1];
                                    this.clients.CopyTo(clients, 0);
                                    clients[currentCount].Set(host, TmphClient, currentCount);
                                    this.clients = clients;
                                    ++currentCount;
                                    TmphClient = null;
                                }
                                finally
                                {
                                    clientLock = 0;
                                    TmphPub.Dispose(ref TmphClient);
                                }
                            }
                            else
                            {
                                clients[currentCount].Set(host, TmphClient, currentCount);
                                ++currentCount;
                                clientLock = 0;
                            }
                        }
                        else
                        {
                            clients[freeIndexs.Unsafer.Pop()].Set(host, TmphClient);
                            clientLock = 0;
                        }
                        return true;
                    }
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
            }
            return false;
        }

        /// <summary>
        ///     移除TCP调用服务端
        /// </summary>
        /// <param name="host">TCP调用服务端端口信息</param>
        /// <returns>是否移除成功</returns>
        private bool removeServer(TmphHost host)
        {
            if (isDisposed == 0)
            {
                TmphInterlocked.NoCheckCompareSetSleep0(ref clientLock);
                for (var index = 0; index != currentCount; ++index)
                {
                    var removeClient = clients[index].Remove(host);
                    if (removeClient.Key != null)
                    {
                        callCount -= removeClient.Value;
                        try
                        {
                            freeIndexs.Add(index);
                        }
                        finally
                        {
                            clientLock = 0;
                            TmphPub.Dispose(ref removeClient.Key);
                        }
                        TmphLog.Default.Add("移除TCP调用服务端 " + host.Host + ":" + host.Port.toString(), false, false);
                        return true;
                    }
                }
                clientLock = 0;
            }
            return false;
        }

        /// <summary>
        ///     添加检测任务
        /// </summary>
        private void tryCheck()
        {
            TmphInterlocked.NoCheckCompareSetSleep0(ref clientLock);
            if (isCheckTask == 0)
            {
                isCheckTask = 1;
                clientLock = 0;
                addCheck();
            }
            else clientLock = 0;
        }

        /// <summary>
        ///     添加检测任务
        /// </summary>
        private void addCheck()
        {
            if (checkTime < TmphDate.NowSecond) checkTime = TmphDate.NowSecond;
            TmphTimerTask.Default.Add(checkHandle, checkTime = checkTime.AddSeconds(1), null);
        }

        /// <summary>
        ///     检测任务
        /// </summary>
        private void check()
        {
            if (isDisposed == 0)
            {
                var count = 0;
                var now = TmphDate.NowSecond.AddTicks(-checkTicks);
                TmphInterlocked.NoCheckCompareSetSleep0(ref clientLock);
                try
                {
                    if (removeClients.Length < currentCount)
                        removeClients = new TmphKeyValue<TClientType, int>[clients.Length];
                    for (var index = 0; index != currentCount; ++index)
                    {
                        var TmphClient = clients[index].CheckTimeout(now);
                        if (TmphClient != null && _loadBalancingCheckTime_(TmphClient) < now)
                            removeClients[count++].Set(TmphClient, index);
                    }
                }
                finally
                {
                    clientLock = 0;
                }
                while (count != 0)
                {
                    var isClient = false;
                    var TmphClient = removeClients[--count].Key;
                    try
                    {
                        isClient = _loadBalancingCheck_(TmphClient);
                    }
                    catch (Exception error)
                    {
                        TmphLog.Error.Add(error, null, false);
                    }
                    if (!isClient)
                    {
                        var index = removeClients[count].Value;
                        TmphInterlocked.NoCheckCompareSetSleep0(ref clientLock);
                        var host = clients[index].Remove(TmphClient);
                        if (host.Key.Host == null) clientLock = 0;
                        else
                        {
                            callCount -= host.Value;
                            try
                            {
                                freeIndexs.Add(index);
                            }
                            finally
                            {
                                clientLock = 0;
                            }
                            TmphPub.Dispose(ref TmphClient);

                            if (newServer(host.Key))
                            {
                                TmphLog.Default.Add("恢复TCP调用服务端[检测超时] " + host.Key.Host + ":" + host.Key.Port.toString(),
                                    false, false);
                            }
                            else
                                TmphLog.Default.Add("移除TCP调用服务端[检测超时] " + host.Key.Host + ":" + host.Key.Port.toString(),
                                    false, false);
                        }
                    }
                }
                TmphInterlocked.NoCheckCompareSetSleep0(ref clientLock);
                if (currentCount == freeIndexs.Count)
                {
                    isCheckTask = 0;
                    clientLock = 0;
                }
                else
                {
                    clientLock = 0;
                    addCheck();
                }
            }
        }

        /// <summary>
        ///     TCP调用负载均衡服务端
        /// </summary>
        private sealed class TmphCommandServer : Tcp.TmphCommandServer
        {
            /// <summary>
            ///     自定义TCP调用服务端验证
            /// </summary>
            private readonly Func<TmphSubArray<byte>, bool> isVerify;

            /// <summary>
            ///     TCP调用负载均衡服务端目标对象
            /// </summary>
            private readonly TmphCommandLoadBalancingServer<TClientType> server;

            /// <summary>
            ///     TCP调用负载均衡服务端
            /// </summary>
            /// <param name="server">TCP调用负载均衡服务端目标对象</param>
            public TmphCommandServer(TmphCommandLoadBalancingServer<TClientType> server) : this(server, null)
            {
            }

            /// <summary>
            ///     TCP调用负载均衡服务端
            /// </summary>
            /// <param name="server">TCP调用负载均衡服务端目标对象</param>
            /// <param name="isVerify">自定义TCP调用服务端验证</param>
            public TmphCommandServer(TmphCommandLoadBalancingServer<TClientType> server, Func<TmphSubArray<byte>, bool> isVerify)
                : base(server.attribute)
            {
                this.server = server;
                this.isVerify = isVerify;
                setCommands(3);
                identityOnCommands[verifyCommandIdentity = CommandStartIndex].Set(verify, 1024);
                identityOnCommands[NewServerCommandIndex].Set(newServer, 1024);
                identityOnCommands[RemoveServerCommandIndex].Set(removeServer, 1024);
            }

            /// <summary>
            ///     TCP调用服务端验证
            /// </summary>
            /// <param name="socket">TCP调用套接字</param>
            /// <param name="data">参数序列化数据</param>
            private void verify(TmphSocket socket, TmphSubArray<byte> data)
            {
                try
                {
                    string inputParameter = null;
                    if (TmphDataDeSerializer.DeSerialize(data, ref inputParameter))
                    {
                        var isVerify = false;
                        if (this.isVerify == null)
                        {
                            if (Config.TmphTcpRegister.Default.Verify == null && !Config.TmphPub.Default.IsDebug)
                            {
                                TmphLog.Error.Add("TCP服务注册验证数据不能为空", false, true);
                            }
                            else isVerify = Config.TmphTcpRegister.Default.Verify == inputParameter;
                        }
                        else isVerify = this.isVerify(data);
                        socket.IsVerifyMethod = true;
                        socket.SendStream(socket.Identity,
                            new TmphAsynchronousMethod.TmphReturnValue<bool> { IsReturn = true, Value = isVerify });
                        return;
                    }
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, true);
                }
                socket.SendStream(socket.Identity, new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false });
            }

            /// <summary>
            ///     添加TCP调用服务端
            /// </summary>
            /// <param name="socket">TCP调用套接字</param>
            /// <param name="data">参数序列化数据</param>
            private void newServer(TmphSocket socket, TmphSubArray<byte> data)
            {
                try
                {
                    var host = new TmphHost();
                    if (TmphDataDeSerializer.DeSerialize(data, ref host))
                    {
                        TmphThreadPool.TinyPool.FastStart(server.newServerHandle,
                            new TmphServerInfo { Socket = socket, Identity = socket.Identity, Host = host }, null, null);
                        return;
                    }
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, true);
                }
                socket.SendStream(socket.Identity, new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false });
            }

            /// <summary>
            ///     移除TCP调用服务端
            /// </summary>
            /// <param name="socket">TCP调用套接字</param>
            /// <param name="data">参数序列化数据</param>
            private void removeServer(TmphSocket socket, TmphSubArray<byte> data)
            {
                try
                {
                    var host = new TmphHost();
                    if (TmphDataDeSerializer.DeSerialize(data, ref host))
                    {
                        socket.SendStream(socket.Identity,
                            new TmphAsynchronousMethod.TmphReturnValue<bool>
                            {
                                IsReturn = true,
                                Value = server.removeServer(host)
                            });
                        return;
                    }
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, true);
                }
                socket.SendStream(socket.Identity, new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false });
            }
        }

        /// <summary>
        ///     TCP调用服务器信息
        /// </summary>
        public struct TmphClientIdentity
        {
            /// <summary>
            ///     TCP调用客户端
            /// </summary>
            public TClientType Client;

            /// <summary>
            ///     验证编号
            /// </summary>
            public int Identity;

            /// <summary>
            ///     TCP调用客户端索引
            /// </summary>
            public int Index;

            /// <summary>
            ///     设置TCP调用客户端
            /// </summary>
            /// <param name="TmphClient">TCP调用客户端</param>
            /// <param name="index">TCP调用客户端索引</param>
            internal void Set(TClientType TmphClient, int index)
            {
                Client = TmphClient;
                Index = index;
            }

            /// <summary>
            ///     重置TCP调用客户端
            /// </summary>
            /// <param name="TmphClient">TCP调用客户端</param>
            internal void Set(TClientType TmphClient)
            {
                Client = TmphClient;
                ++Identity;
            }

            /// <summary>
            ///     移除TCP调用客户端
            /// </summary>
            /// <returns>TCP调用客户端</returns>
            internal TClientType GetRemove()
            {
                var TmphClient = Client;
                Client = null;
                ++Identity;
                return TmphClient;
            }

            /// <summary>
            ///     移除TCP调用客户端
            /// </summary>
            internal void Remove()
            {
                Client = null;
                ++Identity;
            }
        }

        /// <summary>
        ///     TCP调用服务器信息
        /// </summary>
        public struct TmphClientHost
        {
            /// <summary>
            ///     TCP调用服务器信息
            /// </summary>
            public TmphClientIdentity Client;

            /// <summary>
            ///     当前处理数量
            /// </summary>
            public int Count;

            /// <summary>
            ///     TCP调用端口信息
            /// </summary>
            public TmphHost Host;

            /// <summary>
            ///     最后响应时间
            /// </summary>
            public DateTime LastTime;

            /// <summary>
            ///     设置TCP调用客户端
            /// </summary>
            /// <param name="host">TCP调用端口信息</param>
            /// <param name="TmphClient">TCP调用客户端</param>
            /// <param name="index">TCP调用客户端索引</param>
            internal void Set(TmphHost host, TClientType TmphClient, int index)
            {
                Client.Set(TmphClient, index);
                Host = host;
                Count = 0;
            }

            /// <summary>
            ///     重置TCP调用客户端
            /// </summary>
            /// <param name="host">TCP调用端口信息</param>
            /// <param name="TmphClient">TCP调用客户端</param>
            internal void Set(TmphHost host, TClientType TmphClient)
            {
                Client.Client = TmphClient;
                Host = host;
                Count = 0;
            }

            /// <summary>
            ///     重置TCP调用客户端
            /// </summary>
            /// <param name="host">TCP调用端口信息</param>
            /// <param name="TmphClient">TCP调用客户端</param>
            /// <returns>TCP调用客户端+未完成处理数量</returns>
            internal TmphKeyValue<TClientType, int> ReSet(TmphHost host, TClientType TmphClient)
            {
                if (Client.Client != null && Host.Equals(host))
                {
                    var removeClient = Client.Client;
                    var count = Count;
                    Client.Set(TmphClient);
                    Count = 0;
                    return new TmphKeyValue<TClientType, int>(removeClient, count);
                }
                return default(TmphKeyValue<TClientType, int>);
            }

            /// <summary>
            ///     移除TCP调用客户端
            /// </summary>
            /// <param name="host">TCP调用端口信息</param>
            /// <returns>TCP调用客户端+未完成处理数量</returns>
            internal TmphKeyValue<TClientType, int> Remove(TmphHost host)
            {
                if (Client.Client != null && Host.Equals(host)) return Remove();
                return default(TmphKeyValue<TClientType, int>);
            }

            /// <summary>
            ///     移除TCP调用客户端
            /// </summary>
            /// <returns>TCP调用客户端+未完成处理数量</returns>
            internal TmphKeyValue<TClientType, int> Remove()
            {
                var TmphClient = Client.GetRemove();
                var count = Count;
                return new TmphKeyValue<TClientType, int>(TmphClient, count);
            }

            /// <summary>
            ///     移除TCP调用客户端
            /// </summary>
            /// <param name="TmphClient">TCP调用客户端</param>
            /// <returns>TCP调用端口信息+未完成处理数量</returns>
            internal TmphKeyValue<TmphHost, int> Remove(TClientType TmphClient)
            {
                if (Client.Client == TmphClient)
                {
                    Client.Remove();
                    return new TmphKeyValue<TmphHost, int>(Host, Count);
                }
                return default(TmphKeyValue<TmphHost, int>);
            }

            /// <summary>
            ///     测试当前处理数量
            /// </summary>
            /// <param name="count">最大处理数量</param>
            /// <returns>是否测试成功</returns>
            internal bool TryCount(int count)
            {
                if (Count <= count && Client.Client != null)
                {
                    ++Count;
                    return true;
                }
                return false;
            }

            /// <summary>
            ///     TCP调用客户端调用结束
            /// </summary>
            /// <param name="identity">验证编号</param>
            /// <returns>是否验证成功</returns>
            internal int End(int identity)
            {
                if (Client.Identity == identity)
                {
                    LastTime = TmphDate.NowSecond;
                    --Count;
                    return 1;
                }
                return 0;
            }

            /// <summary>
            ///     TCP调用客户端调用错误
            /// </summary>
            /// <param name="identity">验证编号</param>
            /// <returns>开始错误时间</returns>
            internal TmphKeyValue<TClientType, TmphKeyValue<TmphHost, int>> Error(int identity)
            {
                if (Client.Identity == identity)
                {
                    var TmphClient = Client.GetRemove();
                    return new TmphKeyValue<TClientType, TmphKeyValue<TmphHost, int>>(TmphClient,
                        new TmphKeyValue<TmphHost, int>(Host, Count - 1));
                }
                return default(TmphKeyValue<TClientType, TmphKeyValue<TmphHost, int>>);
            }

            /// <summary>
            ///     超时检测
            /// </summary>
            /// <param name="timeout">超时时间</param>
            /// <returns>TCP调用客户端</returns>
            internal TClientType CheckTimeout(DateTime timeout)
            {
                return LastTime < timeout ? Client.Client : null;
            }
        }
    }
}