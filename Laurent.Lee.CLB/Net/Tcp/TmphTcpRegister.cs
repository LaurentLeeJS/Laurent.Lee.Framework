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

using Laurent.Lee.CLB.Code;
using Laurent.Lee.CLB.Code.CSharp;
using Laurent.Lee.CLB.Emit;
using Laurent.Lee.CLB.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Laurent.Lee.CLB.Net.Tcp
{
    /// <summary>
    ///     TCP注册服务
    /// </summary>
    [Code.CSharp.TmphTcpServer(Service = "tcpRegister", IsIdentityCommand = true,
        VerifyMethodType = typeof(TmphVerifyMethod))]
    public partial class TmphTcpRegister
    {
        /// <summary>
        ///     轮询状态
        /// </summary>
        public enum TmphPollState
        {
            /// <summary>
            ///     客户端标识错误
            /// </summary>
            ClientError,

            /// <summary>
            ///     检测是否在线
            /// </summary>
            Check,

            /// <summary>
            ///     TCP服务信息版本号不匹配
            /// </summary>
            VersionError,

            /// <summary>
            ///     轮询TCP客户端冲突
            /// </summary>
            NewClient,

            /// <summary>
            ///     TCP服务端注册更新
            /// </summary>
            RegisterChange
        }

        /// <summary>
        ///     注册状态
        /// </summary>
        public enum TmphRegisterState
        {
            /// <summary>
            ///     客户端不可用
            /// </summary>
            NoClient,

            /// <summary>
            ///     客户端标识错误
            /// </summary>
            ClientError,

            /// <summary>
            ///     单例服务冲突
            /// </summary>
            SingleError,

            /// <summary>
            ///     TCP服务端口信息不合法
            /// </summary>
            HostError,

            /// <summary>
            ///     TCP服务端口信息已存在
            /// </summary>
            HostExists,

            /// <summary>
            ///     没有可用的端口号
            /// </summary>
            PortError,

            /// <summary>
            ///     TCP服务信息检测被更新,需要重试
            /// </summary>
            ServiceChange,

            /// <summary>
            ///     注册成功
            /// </summary>
            Success
        }

        /// <summary>
        ///     预申请服务集合
        /// </summary>
        private readonly Dictionary<TmphHashString, TmphKeyValue<TmphClientId, TmphService>> perpServices =
            TmphDictionary.CreateHashString<TmphKeyValue<TmphClientId, TmphService>>();

        /// <summary>
        ///     TCP服务信息 访问锁
        /// </summary>
        private readonly object serviceLock = new object();

        /// <summary>
        ///     缓存文件名称
        /// </summary>
        private string cacheFile;

        /// <summary>
        ///     轮询TCP服务端集合
        /// </summary>
        private Dictionary<TmphClientId, TmphClientState> clients;

        /// <summary>
        ///     TCP服务端信息集合
        /// </summary>
        private Dictionary<TmphHost, TmphClientId> hostClients;

        /// <summary>
        ///     TCP服务端口信息集合
        /// </summary>
        private Dictionary<TmphHashString, int> hostPorts;

        /// <summary>
        ///     TCP服务注册通知轮询TCP服务端
        /// </summary>
        private Action<TmphKeyValue<TmphServices, int>> onRegisterHandle;

        /// <summary>
        ///     TCP服务注册通知轮询TCP服务端
        /// </summary>
        private Action<TmphKeyValue<TmphSubArray<TmphServices>, int>> onRegistersHandle;

        /// <summary>
        ///     TCP服务信息集合
        /// </summary>
        private Dictionary<TmphHashString, TmphServices> serviceCache;

        /// <summary>
        ///     TCP服务信息 版本号
        /// </summary>
        private int serviceVersion;

        /// <summary>
        ///     设置TCP服务端
        /// </summary>
        /// <param name="tcpServer">TCP服务端</param>
        public void SetTcpServer(TmphServer tcpServer)
        {
            onRegisterHandle = onRegister;
            onRegistersHandle = onRegister;
            cacheFile = Config.TmphPub.Default.CachePath + tcpServer.ServiceName + @".cache";
            fromCacheFile();
        }

        /// <summary>
        ///     TCP服务端注册验证函数
        /// </summary>
        /// <returns>是否验证成功</returns>
        [Code.CSharp.TmphTcpServer(IsVerifyMethod = true, IsServerAsynchronousTask = false, InputParameterMaxLength = 1024
            )]
        protected virtual bool verify(string value)
        {
            if (Config.TmphTcpRegister.Default.Verify == null && !Config.TmphPub.Default.IsDebug)
            {
                TmphLog.Error.Add("TCP服务注册验证数据不能为空", false, true);
                return false;
            }
            return Config.TmphTcpRegister.Default.Verify == value;
        }

        /// <summary>
        ///     TCP服务端注册
        /// </summary>
        /// <returns>TCP服务端标识</returns>
        [Code.CSharp.TmphTcpServer(IsServerAsynchronousTask = false)]
        private TmphClientId register()
        {
            var clientId = new TmphClientId
            {
                Tick = TmphPub.StartTime.Ticks,
                Identity = TmphPub.Identity32,
                Version = int.MinValue
            };
            var state = new TmphClientState();
            Monitor.Enter(serviceLock);
            try
            {
                clients.Add(clientId, state);
            }
            finally
            {
                Monitor.Exit(serviceLock);
            }
            return clientId;
        }

        /// <summary>
        ///     获取TCP服务信息集合
        /// </summary>
        /// <param name="version">TCP服务信息 版本号</param>
        /// <returns>TCP服务信息集合</returns>
        [Code.CSharp.TmphTcpServer]
        private TmphServices[] getServices(out int version)
        {
            TmphServices[] services = null;
            Monitor.Enter(serviceLock);
            try
            {
                version = serviceVersion;
                services = serviceCache.Values.GetArray();
            }
            finally
            {
                Monitor.Exit(serviceLock);
            }
            return services;
        }

        /// <summary>
        ///     获取客户端状态
        /// </summary>
        /// <param name="TmphClient"></param>
        /// <returns></returns>
        private TmphClientState getClient(TmphClientId TmphClient)
        {
            TmphClientState state;
            Monitor.Enter(serviceLock);
            if (clients.TryGetValue(TmphClient, out state))
            {
                Monitor.Exit(serviceLock);
                state.Time = TmphDate.NowSecond;
            }
            else Monitor.Exit(serviceLock);
            return state;
        }

        /// <summary>
        ///     注册TCP服务信息
        /// </summary>
        /// <param name="TmphClient">TCP服务端标识</param>
        /// <param name="service">TCP服务信息</param>
        /// <returns>注册状态</returns>
        [Code.CSharp.TmphTcpServer]
        private TmphRegisterResult register(TmphClientId TmphClient, TmphService service)
        {
            var state = getClient(TmphClient);
            if (state == null) return new TmphRegisterResult { State = TmphRegisterState.ClientError };
            if (!service.Host.HostToIpAddress()) return new TmphRegisterResult { State = TmphRegisterState.HostError };
            TmphServices oldService, newService = new TmphServices { Name = service.Name };
            int version = int.MinValue, hostCount = 0;
            TmphHashString serviceName = service.Name;
            Monitor.Enter(serviceLock);
            try
            {
                if (serviceCache.TryGetValue(serviceName, out oldService))
                {
                    if (oldService.IsSingle || service.IsSingle)
                    {
                        foreach (var host in oldService.Hosts)
                        {
                            TmphClientId oldClient;
                            if (hostClients.TryGetValue(host, out oldClient))
                            {
                                TmphClientState oldState;
                                if (clients.TryGetValue(oldClient, out oldState))
                                {
                                    if (oldState.IsClient) oldService.Hosts[hostCount++] = host;
                                    else TmphThreadPool.TinyPool.FastStart(removeRegister, oldClient, null, null);
                                }
                            }
                        }
                        if (hostCount != 0)
                        {
                            if (hostCount != oldService.Hosts.Length) Array.Resize(ref oldService.Hosts, hostCount);
                            newService = oldService;
                            version = ++serviceVersion;
                            if (service.IsPerp)
                            {
                                if (service.Host.Port == 0)
                                {
                                    service.Host.Port = getPort(TmphClient, service.Host.Host, true);
                                    if (service.Host.Port == 0)
                                        return new TmphRegisterResult { State = TmphRegisterState.PortError };
                                }
                                perpServices[serviceName] = new TmphKeyValue<TmphClientId, TmphService>(TmphClient, service);
                                return new TmphRegisterResult { State = TmphRegisterState.Success, Service = service };
                            }
                            return new TmphRegisterResult { State = TmphRegisterState.SingleError };
                        }
                        oldService.Hosts = TmphNullValue<TmphHost>.Array;
                    }
                    if (service.Host.Port == 0)
                    {
                        service.Host.Port = getPort(TmphClient, service.Host.Host, false);
                        if (service.Host.Port == 0) return new TmphRegisterResult { State = TmphRegisterState.PortError };
                    }
                    else
                    {
                        if (hostClients.ContainsKey(service.Host))
                            return new TmphRegisterResult { State = TmphRegisterState.HostExists };
                        hostClients.Add(service.Host, TmphClient);
                    }
                    newService.Hosts = new TmphHost[oldService.Hosts.Length + 1];
                    Array.Copy(oldService.Hosts, 0, newService.Hosts, 1, oldService.Hosts.Length);
                    newService.Hosts[0] = service.Host;
                    serviceCache[serviceName] = newService;
                    version = ++serviceVersion;
                }
                else
                {
                    if (service.Host.Port == 0)
                    {
                        service.Host.Port = getPort(TmphClient, service.Host.Host, false);
                        if (service.Host.Port == 0) return new TmphRegisterResult { State = TmphRegisterState.PortError };
                    }
                    else
                    {
                        if (hostClients.ContainsKey(service.Host))
                            return new TmphRegisterResult { State = TmphRegisterState.HostExists };
                        hostClients.Add(service.Host, TmphClient);
                    }
                    newService.Hosts = new[] { service.Host };
                    newService.IsSingle = service.IsSingle;
                    serviceCache.Add(serviceName, newService);
                    version = ++serviceVersion;
                }
            }
            finally
            {
                Monitor.Exit(serviceLock);
                if (version != int.MinValue)
                    TmphQueue.Tiny.Add(onRegisterHandle, new TmphKeyValue<TmphServices, int>(newService, version), null);
            }
            return new TmphRegisterResult { State = TmphRegisterState.Success, Service = service };
        }

        /// <summary>
        ///     获取TCP服务端口号
        /// </summary>
        /// <param name="TmphClient">TCP服务端标识</param>
        /// <param name="host">主机IP地址</param>
        /// <returns>TCP服务端口号</returns>
        private int getPort(TmphClientId TmphClient, string ipAddress, bool isPerp)
        {
            var host = new TmphHost { Host = ipAddress };
            TmphHashString ipKey = ipAddress;
            if (!hostPorts.TryGetValue(ipKey, out host.Port)) host.Port = Config.TmphTcpRegister.Default.PortStart;
            var startPort = host.Port;
            while (hostClients.ContainsKey(host)) ++host.Port;
            if (host.Port >= 65536)
            {
                host.Port = Config.TmphTcpRegister.Default.PortStart;
                while (host.Port != startPort && hostClients.ContainsKey(host)) ++host.Port;
                if (host.Port == startPort) return 0;
            }
            hostPorts[ipKey] = host.Port + 1;
            if (!isPerp) hostClients.Add(host, TmphClient);
            return host.Port;
        }

        /// <summary>
        ///     TCP服务注册通知轮询TCP服务端
        /// </summary>
        /// <param name="serviceVersion">TCP服务信息</param>
        private void onRegister(TmphKeyValue<TmphServices, int> serviceVersion)
        {
            callPoll(new TmphPollResult
            {
                State = TmphPollState.RegisterChange,
                Services = new[] { serviceVersion.Key },
                Version = serviceVersion.Value
            });
        }

        /// <summary>
        ///     TCP服务注册通知轮询TCP服务端
        /// </summary>
        /// <param name="result">TCP服务信息</param>
        /// <param name="isCheckTime">是否检测轮询时间</param>
        private void callPoll(TmphPollResult result)
        {
            KeyValuePair<TmphClientId, TmphClientState>[] clients = null;
            Monitor.Enter(serviceLock);
            try
            {
                clients = this.clients.GetArray();
            }
            finally
            {
                Monitor.Exit(serviceLock);
            }
            if (clients.Length != 0)
            {
                var returnValue = new TmphAsynchronousMethod.TmphReturnValue<TmphPollResult>
                {
                    IsReturn = true,
                    Value = result
                };
                foreach (var TmphClient in clients)
                {
                    try
                    {
                        if (!(TmphClient.Value.Poll == null ? TmphClient.Value.IsClient : TmphClient.Value.Poll(returnValue)))
                            removeRegister(TmphClient.Key);
                    }
                    catch (Exception error)
                    {
                        TmphLog.Error.Add(error, null, false);
                    }
                }
            }
            if (result.State == TmphPollState.RegisterChange) TmphTask.Tiny.Add(saveCacheFile);
        }

        /// <summary>
        ///     TCP服务端轮询
        /// </summary>
        /// <param name="TmphClient">TCP服务端标识</param>
        /// <param name="onRegisterChanged">TCP服务注册通知委托</param>
        [Code.CSharp.TmphTcpServer(IsServerAsynchronousCallback = true, IsClientAsynchronous = true,
            IsClientSynchronous = false, IsKeepCallback = true)]
        private void poll(TmphClientId TmphClient, Func<TmphAsynchronousMethod.TmphReturnValue<TmphPollResult>, bool> onRegisterChanged)
        {
            var state = getClient(TmphClient);
            if (state == null) onRegisterChanged(new TmphPollResult { State = TmphPollState.ClientError });
            else
            {
                var value = new TmphPollResult();
                Monitor.Enter(serviceLock);
                if (TmphClient.Version != serviceVersion) value.State = TmphPollState.VersionError;
                else
                {
                    var poll = state.Poll;
                    state.Poll = onRegisterChanged;
                    onRegisterChanged = poll;
                    value.State = TmphPollState.NewClient;
                }
                Monitor.Exit(serviceLock);
                if (onRegisterChanged != null) onRegisterChanged(value);
            }
        }

        /// <summary>
        ///     注销TCP服务信息
        /// </summary>
        /// <param name="TmphClient">TCP服务端标识</param>
        /// <param name="serviceName">TCP服务名称</param>
        [Code.CSharp.TmphTcpServer]
        private void removeRegister(TmphClientId TmphClient, string serviceName)
        {
            var state = getClient(TmphClient);
            if (state != null)
            {
                TmphServices services;
                var service = default(TmphKeyValue<TmphClientId, TmphService>);
                TmphHashString nameKey = serviceName;
                var version = int.MinValue;
                var isPerp = false;
                Monitor.Enter(serviceLock);
                try
                {
                    if (serviceCache.TryGetValue(nameKey, out services))
                    {
                        if (perpServices.TryGetValue(nameKey, out service))
                        {
                            perpServices.Remove(nameKey);
                            isPerp = true;
                        }
                        var hosts = removeRegister(TmphClient, services);
                        if (hosts != services.Hosts)
                        {
                            if (isPerp) version = 0;
                            else version = ++serviceVersion;
                            services.Hosts = hosts;
                        }
                    }
                }
                finally
                {
                    Monitor.Exit(serviceLock);
                }
                if (isPerp)
                {
                    service.Value.IsPerp = false;
                    try
                    {
                        if (register(service.Key, service.Value).State == TmphRegisterState.Success) isPerp = false;
                    }
                    finally
                    {
                        if (isPerp && version == 0)
                        {
                            Monitor.Enter(serviceLock);
                            version = ++serviceVersion;
                            Monitor.Exit(serviceLock);
                            TmphQueue.Tiny.Add(onRegisterHandle, new TmphKeyValue<TmphServices, int>(services, version), null);
                        }
                    }
                }
                else if (version != int.MinValue)
                    TmphQueue.Tiny.Add(onRegisterHandle, new TmphKeyValue<TmphServices, int>(services, version), null);
            }
        }

        /// <summary>
        ///     注销TCP服务信息
        /// </summary>
        /// <param name="TmphClient">TCP服务端标识</param>
        /// <param name="serviceName">TCP服务信息</param>
        /// <returns>注销操作之后的TCP服务端口信息集合</returns>
        private unsafe TmphHost[] removeRegister(TmphClientId TmphClient, TmphServices service)
        {
            int count = (service.Hosts.Length + 7) >> 3, index = 0;
            byte* isRemove = stackalloc byte[count];
            var removeMap = new TmphFixedMap(isRemove, count);
            count = 0;
            foreach (var host in service.Hosts)
            {
                if (TmphClient.Equals(hostClients[host])) removeMap.Set(index);
                else ++count;
                ++index;
            }
            if (count != service.Hosts.Length)
            {
                TmphHashString serviceName = service.Name;
                if (count == 0)
                {
                    serviceCache.Remove(serviceName);
                    foreach (var host in service.Hosts) hostClients.Remove(host);
                    return null;
                }
                var hosts = new TmphHost[count];
                count = index = 0;
                foreach (var host in service.Hosts)
                {
                    if (removeMap.Get(index++)) hostClients.Remove(host);
                    else hosts[count++] = host;
                }
                service.Hosts = hosts;
                serviceCache[serviceName] = service;
            }
            return service.Hosts;
        }

        /// <summary>
        ///     注销TCP服务信息
        /// </summary>
        /// <param name="TmphClient">TCP服务端标识</param>
        [Code.CSharp.TmphTcpServer]
        private void removeRegister(TmphClientId TmphClient)
        {
            var state = getClient(TmphClient);
            if (state != null)
            {
                TmphSubArray<TmphServices> removeServices;
                var removePerpServices = new TmphSubArray<TmphKeyValue<TmphClientId, TmphService>>();
                TmphKeyValue<TmphClientId, TmphService> perpService;
                var version = int.MinValue;
                Monitor.Enter(serviceLock);
                try
                {
                    removeServices = new TmphSubArray<TmphServices>(serviceCache.Count);
                    foreach (var service in serviceCache.Values.GetArray())
                    {
                        var hosts = removeRegister(TmphClient, service);
                        if (hosts != service.Hosts)
                        {
                            removeServices.UnsafeAdd(new TmphServices
                            {
                                Name = service.Name,
                                Hosts = hosts,
                                IsSingle = service.IsSingle
                            });
                            if (perpServices.TryGetValue(service.Name, out perpService))
                            {
                                perpServices.Remove(service.Name);
                                removePerpServices.Add(perpService);
                            }
                        }
                    }
                    if (removeServices.Count != 0) version = ++serviceVersion;
                    clients.Remove(TmphClient);
                }
                finally
                {
                    Monitor.Exit(serviceLock);
                }
                if (version != int.MinValue)
                    TmphTask.Tiny.Add(onRegistersHandle,
                        new TmphKeyValue<TmphSubArray<TmphServices>, int>(removeServices, version), null);
                if (removePerpServices.Count != 0)
                {
                    foreach (var removePerpService in removePerpServices)
                    {
                        perpService.Value = removePerpService.Value;
                        perpService.Value.IsPerp = false;
                        register(removePerpService.Key, perpService.Value);
                    }
                }
            }
        }

        /// <summary>
        ///     TCP服务注册通知轮询TCP服务端
        /// </summary>
        /// <param name="serviceVersion">TCP服务信息</param>
        private void onRegister(TmphKeyValue<TmphSubArray<TmphServices>, int> serviceVersion)
        {
            callPoll(new TmphPollResult
            {
                State = TmphPollState.RegisterChange,
                Services = serviceVersion.Key.ToArray(),
                Version = serviceVersion.Value
            });
        }

        /// <summary>
        ///     保存TCP服务信息集合到缓存文件
        /// </summary>
        private unsafe void saveCacheFile()
        {
            var cache = new TmphCache();
            var TmphBuffer = TmphMemoryPool.StreamBuffers.Get();
            try
            {
                fixed (byte* bufferFixed = TmphBuffer)
                {
                    using (var stream = new TmphUnmanagedStream(bufferFixed, TmphBuffer.Length))
                    {
                        Monitor.Enter(serviceLock);
                        try
                        {
                            cache.ServiceCache =
                                serviceCache.GetArray(
                                    value => new TmphKeyValue<string, TmphServices>(value.Key.ToString(), value.Value));
                            cache.HostClients = hostClients;
                            cache.HostPorts =
                                hostPorts.GetArray(
                                    value => new TmphKeyValue<string, int>(value.Key.ToString(), value.Value));
                            cache.clients = clients.Keys.GetArray();
                            TmphDataSerializer.Serialize(cache, stream);
                            if (stream.Data == bufferFixed)
                            {
                                using (
                                    var file = new FileStream(cacheFile, FileMode.Create, FileAccess.Write,
                                        FileShare.None))
                                {
                                    file.Write(TmphBuffer, 0, stream.Length);
                                }
                            }
                            else File.WriteAllBytes(cacheFile, stream.GetArray());
                        }
                        finally
                        {
                            Monitor.Exit(serviceLock);
                        }
                    }
                }
            }
            finally
            {
                TmphMemoryPool.StreamBuffers.Push(ref TmphBuffer);
            }
        }

        /// <summary>
        ///     从缓存文件恢复TCP服务信息集合
        /// </summary>
        private void fromCacheFile()
        {
            var cache = new TmphCache();
            if (File.Exists(cacheFile))
            {
                var isCache = 0;
                try
                {
                    if (TmphDataDeSerializer.DeSerialize(File.ReadAllBytes(cacheFile), ref cache)) isCache = 1;
                    else cache = new TmphCache();
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
                if (isCache == 0)
                {
                    try
                    {
                        File.Delete(cacheFile);
                    }
                    catch (Exception error)
                    {
                        TmphLog.Default.Add(error, null, false);
                    }
                }
            }
            Monitor.Enter(serviceLock);
            try
            {
                serviceCache = TmphDictionary.CreateHashString<TmphServices>();
                if (cache.ServiceCache != null)
                {
                    foreach (var value in cache.ServiceCache) serviceCache.Add(value.Key, value.Value);
                }
                hostClients = cache.HostClients ?? TmphDictionary<TmphHost>.Create<TmphClientId>();
                hostPorts = TmphDictionary.CreateHashString<int>();
                if (cache.HostPorts != null)
                {
                    foreach (var value in cache.HostPorts) hostPorts.Add(value.Key, value.Value);
                }
                clients = TmphDictionary<TmphClientId>.Create<TmphClientState>();
                if (cache.clients != null)
                    foreach (var TmphClient in cache.clients)
                        clients.Add(TmphClient, new TmphClientState { Time = DateTime.MinValue });
            }
            finally
            {
                Monitor.Exit(serviceLock);
            }
        }

        /// <summary>
        ///     TCP注册服务 客户端
        /// </summary>
        public sealed class TmphClient : IDisposable
        {
            /// <summary>
            ///     TCP注册服务客户端缓存
            /// </summary>
            private static Dictionary<TmphHashString, TmphClient> clients = TmphDictionary.CreateHashString<TmphClient>();

            /// <summary>
            ///     TCP注册服务客户端 访问锁
            /// </summary>
            private static readonly object clientsLock = new object();

            /// <summary>
            ///     TCP服务配置信息
            /// </summary>
            private readonly Code.CSharp.TmphTcpServer attribute;

            /// <summary>
            ///     TCP注册服务访问锁
            /// </summary>
            private readonly object clientLock = new object();

            /// <summary>
            ///     客户端轮询
            /// </summary>
            private readonly Action<TmphAsynchronousMethod.TmphReturnValue<TmphPollResult>> pollHandle;

            /// <summary>
            ///     TCP注册服务名称
            /// </summary>
            private readonly string serviceName;

            /// <summary>
            ///     TCP服务信息
            /// </summary>
            private readonly Dictionary<TmphHashString, TmphServices> services = TmphDictionary.CreateHashString<TmphServices>();

            ///// <summary>
            ///// 创建TCP注册服务客户端失败是否输出日志
            ///// </summary>
            //private bool isNewClientErrorLog;
            /// <summary>
            ///     启动TCP注册服务客户端
            /// </summary>
            private readonly Action startHandle;

            /// <summary>
            ///     TCP服务端标识
            /// </summary>
            private TmphClientId clientId;

            /// <summary>
            ///     客户端保持回调
            /// </summary>
            private TmphCommandClient.TmphStreamCommandSocket.TmphKeepCallback pollKeep;

            /// <summary>
            ///     TCP注册服务客户端
            /// </summary>
            private TcpClient.TmphTcpRegister registerClient;

            /// <summary>
            ///     TCP服务信息访问锁
            /// </summary>
            private int servicesLock;

            /// <summary>
            ///     TCP注册服务客户端
            /// </summary>
            /// <param name="serviceName">TCP注册服务服务名称</param>
            public TmphClient(string serviceName)
            {
                attribute = Config.TmphPub.LoadConfig(new Code.CSharp.TmphTcpServer(), serviceName);
                attribute.IsIdentityCommand = true;
                attribute.TcpRegister = null;
                registerClient = new TcpClient.TmphTcpRegister(attribute, null);
                this.serviceName = serviceName;
                //isNewClientErrorLog = true;
                startHandle = start;
                pollHandle = poll;
                start();
            }

            /// <summary>
            ///     释放资源
            /// </summary>
            public void Dispose()
            {
                Monitor.Enter(clientLock);
                try
                {
                    if (registerClient != null) close();
                    TmphPub.Dispose(ref registerClient);
                }
                finally
                {
                    Monitor.Exit(clientLock);
                }
                TmphInterlocked.NoCheckCompareSetSleep0(ref servicesLock);
                foreach (var services in this.services.Values) services.Clients = null;
                servicesLock = 0;
            }

            /// <summary>
            ///     关闭客户端
            /// </summary>
            private void close()
            {
                if (clientId.Tick != 0)
                {
                    try
                    {
                        registerClient.removeRegister(clientId);
                    }
                    catch (Exception error)
                    {
                        TmphLog.Error.Add(error, null, false);
                    }
                    clientId.Tick = 0;
                }
                TmphPub.Dispose(ref pollKeep);
            }

            /// <summary>
            ///     启动TCP注册服务客户端
            /// </summary>
            private void start()
            {
                var isStart = false;
                Monitor.Enter(clientLock);
                try
                {
                    if (registerClient != null)
                    {
                        close();
                        if ((clientId = registerClient.register().Value).Tick != 0)
                        {
                            var services = registerClient.getServices(out clientId.Version).Value;
                            if (services != null)
                            {
                                newServices(services);
                                if ((pollKeep = registerClient.poll(clientId, pollHandle)) != null) isStart = true;
                            }
                        }
                    }
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
                finally
                {
                    Monitor.Exit(clientLock);
                }
                TmphLog.Default.Add("TCP注册客户端启动 " + (isStart ? "成功" : "失败"), false, false);
                if (!isStart && registerClient != null)
                {
                    TmphTimerTask.Default.Add(startHandle, TmphDate.NowSecond.AddSeconds(2), null);
                }
            }

            /// <summary>
            ///     TCP服务信息集合更新
            /// </summary>
            /// <param name="services">TCP服务信息集合</param>
            private void newServices(TmphServices[] services)
            {
                TmphServices cacheServices;
                TmphInterlocked.NoCheckCompareSetSleep0(ref servicesLock);
                try
                {
                    foreach (var service in services)
                    {
                        TmphHashString name = service.Name;
                        if (this.services.TryGetValue(name, out cacheServices)) cacheServices.Copy(service);
                        else
                        {
                            service.SetClient();
                            this.services.Add(name, service);
                        }
                    }
                }
                finally
                {
                    servicesLock = 0;
                }
            }

            /// <summary>
            ///     客户端轮询
            /// </summary>
            /// <param name="result">轮询结果</param>
            private void poll(TmphAsynchronousMethod.TmphReturnValue<TmphPollResult> result)
            {
                if (result.IsReturn)
                {
                    switch (result.Value.State)
                    {
                        case TmphPollState.RegisterChange:
                            Monitor.Enter(clientLock);
                            try
                            {
                                if (clientId.Version < result.Value.Version)
                                {
                                    clientId.Version = result.Value.Version;
                                    newServices(result.Value.Services);
                                }
                            }
                            finally
                            {
                                Monitor.Exit(clientLock);
                            }
                            break;

                        case TmphPollState.VersionError:
                            TmphServices[] services = null;
                            Monitor.Enter(clientLock);
                            try
                            {
                                services = registerClient.getServices(out clientId.Version).Value;
                            }
                            finally
                            {
                                Monitor.Exit(clientLock);
                                if (services != null) newServices(services);
                            }
                            break;

                        case TmphPollState.ClientError:
                            Monitor.Enter(clientLock);
                            try
                            {
                                close();
                            }
                            finally
                            {
                                Monitor.Exit(clientLock);
                            }
                            break;

                        case TmphPollState.NewClient:
                            TmphLog.Default.Add(serviceName + " 轮询客户端冲突", false, false);
                            break;

                        default:
                            if (result.Value.State != TmphPollState.Check)
                            {
                                TmphLog.Error.Add("不可识别的轮询状态 " + result.Value.State, false, false);
                            }
                            break;
                    }
                }
                else start();
            }

            /// <summary>
            ///     注册TCP服务端
            /// </summary>
            /// <param name="attribute">TCP服务配置</param>
            /// <returns>是否注册成功</returns>
            public TmphRegisterState Register(Code.CSharp.TmphTcpServer attribute)
            {
                var result = new TmphRegisterResult { State = TmphRegisterState.NoClient };
                Monitor.Enter(clientLock);
                try
                {
                    result =
                        registerClient.register(clientId,
                            new TmphService
                            {
                                Host = new TmphHost { Host = attribute.Host, Port = attribute.Port },
                                Name = attribute.ServiceName,
                                IsSingle = attribute.IsSingleRegister,
                                IsPerp = attribute.IsPerpleRegister
                            }).Value;
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
                finally
                {
                    Monitor.Exit(clientLock);
                }
                if (result.State == TmphRegisterState.Success)
                {
                    attribute.Host = result.Service.Host.Host;
                    attribute.Port = result.Service.Host.Port;
                }
                return result.State;
            }

            /// <summary>
            ///     删除注册TCP服务端
            /// </summary>
            /// <param name="attribute">TCP服务配置</param>
            public void RemoveRegister(Code.CSharp.TmphTcpServer attribute)
            {
                Monitor.Enter(clientLock);
                try
                {
                    registerClient.removeRegister(clientId, attribute.ServiceName);
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
                finally
                {
                    Monitor.Exit(clientLock);
                }
            }

            /// <summary>
            ///     绑定TCP调用客户端
            /// </summary>
            /// <param name="commandClient">TCP调用客户端</param>
            public void Register(TmphCommandClient commandClient)
            {
                TmphHashString name = commandClient.ServiceName;
                TmphInterlocked.NoCheckCompareSetSleep0(ref servicesLock);
                try
                {
                    if (!services.TryGetValue(name, out commandClient.TcpRegisterServices))
                    {
                        services.Add(name,
                            commandClient.TcpRegisterServices = new TmphServices { Hosts = TmphNullValue<TmphHost>.Array });
                    }
                    commandClient.TcpRegisterServices.AddClient(commandClient);
                }
                finally
                {
                    servicesLock = 0;
                }
            }

            /// <summary>
            ///     删除TCP调用客户端
            /// </summary>
            /// <param name="commandClient">TCP调用客户端</param>
            internal void Remove(TmphCommandClient commandClient)
            {
                var services = commandClient.TcpRegisterServices;
                if (services != null)
                {
                    commandClient.TcpRegisterServices = null;
                    TmphInterlocked.NoCheckCompareSetSleep0(ref servicesLock);
                    services.RemoveClient(commandClient);
                    servicesLock = 0;
                }
            }

            /// <summary>
            ///     获取TCP服务端口信息
            /// </summary>
            /// <param name="commandClient">TCP调用客户端</param>
            /// <returns>TCP服务端口信息是否更新</returns>
            internal bool GetHost(TmphCommandClient commandClient)
            {
                var services = commandClient.TcpRegisterServices;
                if (services == null)
                {
                    var attribute = commandClient.Attribute;
                    attribute.Port = 0;
                    return true;
                }
                TmphInterlocked.NoCheckCompareSetSleep0(ref servicesLock);
                var isHost = services.GetHost(commandClient);
                servicesLock = 0;
                return isHost;
            }

            /// <summary>
            ///     关闭TCP注册服务客户端
            /// </summary>
            private static void disposeClients()
            {
                TmphClient[] clientArray = null;
                Monitor.Enter(clientsLock);
                try
                {
                    clientArray = clients.Values.GetArray();
                    clients = null;
                }
                finally
                {
                    Monitor.Exit(clientsLock);
                }
                foreach (var TmphClient in clientArray) TmphClient.Dispose();
            }

            /// <summary>
            ///     获取TCP注册服务客户端
            /// </summary>
            /// <param name="serviceName">服务名称</param>
            /// <returns>TCP注册服务客户端,失败返回null</returns>
            public static TmphClient Get(string serviceName)
            {
                if (serviceName.Length() != 0)
                {
                    var count = int.MinValue;
                    TmphClient TmphClient = null;
                    TmphHashString nameKey = serviceName;
                    Monitor.Enter(clientsLock);
                    try
                    {
                        if (clients != null && !clients.TryGetValue(nameKey, out TmphClient))
                        {
                            try
                            {
                                TmphClient = new TmphClient(serviceName);
                            }
                            catch (Exception error)
                            {
                                TmphLog.Error.Add(error, null, false);
                            }
                            if (TmphClient != null)
                            {
                                count = clients.Count;
                                clients.Add(nameKey, TmphClient);
                            }
                        }
                    }
                    finally
                    {
                        Monitor.Exit(clientsLock);
                    }
                    if (count == 0) TmphDomainUnload.Add(disposeClients);
                    return TmphClient;
                }
                return null;
            }
        }

        /// <summary>
        ///     TCP服务端标识
        /// </summary>
        [Emit.TmphDataSerialize(IsReferenceMember = false, IsMemberMap = false)]
        public struct TmphClientId : IEquatable<TmphClientId>
        {
            /// <summary>
            ///     进程级唯一编号
            /// </summary>
            public int Identity;

            /// <summary>
            ///     TCP注册服务进程时间
            /// </summary>
            public long Tick;

            /// <summary>
            ///     注册信息版本
            /// </summary>
            public int Version;

            /// <summary>
            ///     判断是否同一TCP服务端
            /// </summary>
            /// <param name="other">TCP服务端</param>
            /// <returns>是否同一TCP服务端</returns>
            public bool Equals(TmphClientId other)
            {
                return Tick == other.Tick && Identity == other.Identity;
            }

            /// <summary>
            ///     获取哈希值
            /// </summary>
            /// <returns>哈希值</returns>
            public override int GetHashCode()
            {
                return (int)(Tick >> 32) ^ (int)Tick ^ Identity;
            }

            /// <summary>
            ///     判断是否同一TCP服务端
            /// </summary>
            /// <param name="other">TCP服务端</param>
            /// <returns>是否同一TCP服务端</returns>
            public override bool Equals(object other)
            {
                return Equals((TmphClientId)other);
            }
        }

        /// <summary>
        ///     TCP服务信息
        /// </summary>
        [Emit.TmphDataSerialize(IsReferenceMember = false, IsMemberMap = false)]
        public struct TmphService
        {
            /// <summary>
            ///     端口信息集合
            /// </summary>
            public TmphHost Host;

            /// <summary>
            ///     是否预申请服务
            /// </summary>
            public bool IsPerp;

            /// <summary>
            ///     是否只允许一个TCP服务实例
            /// </summary>
            public bool IsSingle;

            /// <summary>
            ///     TCP服务名称标识
            /// </summary>
            public string Name;
        }

        /// <summary>
        ///     TCP服务信息集合
        /// </summary>
        [Emit.TmphDataSerialize(IsReferenceMember = false, IsMemberMap = false)]
        public sealed class TmphServices
        {
            /// <summary>
            ///     最后一次未找到注册的服务名称
            /// </summary>
            private static string errorServiceName;

            /// <summary>
            ///     默认空TCP服务信息集合
            /// </summary>
            internal static readonly TmphServices Null = new TmphServices();

            /// <summary>
            ///     TCP调用客户端集合
            /// </summary>
            [TmphIgnore]
            internal TmphList<TmphCommandClient> Clients;

            /// <summary>
            ///     当前端口信息位置
            /// </summary>
            [TmphIgnore]
            private int hostIndex;

            /// <summary>
            ///     端口信息集合
            /// </summary>
            public TmphHost[] Hosts;

            /// <summary>
            ///     是否只允许一个TCP服务实例
            /// </summary>
            public bool IsSingle;

            /// <summary>
            ///     TCP服务名称标识
            /// </summary>
            public string Name;

            /// <summary>
            ///     端口信息更新版本
            /// </summary>
            [TmphIgnore]
            internal int Version;

            /// <summary>
            ///     获取TCP服务端口信息
            /// </summary>
            /// <param name="commandClient">TCP调用客户端</param>
            /// <returns>是否更新</returns>
            internal bool GetHost(TmphCommandClient commandClient)
            {
                var attribute = commandClient.Attribute;
                if (attribute != null)
                {
                    commandClient.TcpRegisterServicesVersion = Version;
                    if (Hosts.Length == 0)
                    {
                        attribute.Port = 0;
                        if (errorServiceName != attribute.ServiceName)
                            TmphLog.Error.Add(attribute.ServiceName + " 未找到注册服务信息", false, false);
                        errorServiceName = attribute.ServiceName;
                    }
                    else
                    {
                        if (errorServiceName == attribute.ServiceName) errorServiceName = null;
                        TmphHost host;
                        var index = hostIndex;
                        if (index < Hosts.Length)
                        {
                            ++hostIndex;
                            host = Hosts[index];
                        }
                        else
                        {
                            hostIndex = 1;
                            host = Hosts[0];
                        }
                        if (attribute.Host != host.Host || attribute.Port != host.Port)
                        {
                            attribute.Host = host.Host;
                            attribute.Port = host.Port;
                        }
                    }
                    return true;
                }
                return false;
            }

            /// <summary>
            ///     添加TCP调用客户端
            /// </summary>
            /// <param name="TmphClient">TCP调用客户端</param>
            internal void AddClient(TmphCommandClient TmphClient)
            {
                if (Clients == null) Clients = new TmphList<TmphCommandClient>(sizeof(int));
                Clients.Add(TmphClient);
            }

            /// <summary>
            ///     删除TCP调用客户端
            /// </summary>
            /// <param name="removeClient">TCP调用客户端</param>
            internal void RemoveClient(TmphCommandClient removeClient)
            {
                if (Clients != null && Clients.Count != 0)
                {
                    var count = Clients.Count;
                    var clientArray = Clients.array;
                    foreach (var TmphClient in clientArray)
                    {
                        if (TmphClient == removeClient)
                        {
                            count = Clients.Count - count;
                            Clients.Unsafer.AddLength(-1);
                            clientArray[count] = clientArray[Clients.Count];
                            clientArray[Clients.Count] = null;
                            return;
                        }
                        if (--count == 0) break;
                    }
                }
            }

            /// <summary>
            ///     复制TCP服务信息
            /// </summary>
            /// <param name="services">TCP服务信息集合</param>
            internal void Copy(TmphServices services)
            {
                Hosts = services.Hosts ?? TmphNullValue<TmphHost>.Array;
                IsSingle = services.IsSingle;
                hostIndex = 0;
                ++Version;
            }

            /// <summary>
            ///     客户端初始化设置
            /// </summary>
            internal void SetClient()
            {
                if (Hosts == null) Hosts = TmphNullValue<TmphHost>.Array;
                Version = 1;
            }
        }

        /// <summary>
        ///     注册结果
        /// </summary>
        [Emit.TmphDataSerialize(IsReferenceMember = false, IsMemberMap = false)]
        public struct TmphRegisterResult
        {
            /// <summary>
            ///     注册成功的TCP服务信息
            /// </summary>
            public TmphService Service;

            /// <summary>
            ///     注册状态
            /// </summary>
            public TmphRegisterState State;
        }

        /// <summary>
        ///     轮询结果
        /// </summary>
        [Emit.TmphDataSerialize(IsReferenceMember = false, IsMemberMap = false)]
        public struct TmphPollResult
        {
            /// <summary>
            ///     检测是否在线
            /// </summary>
            internal static readonly TmphPollResult Check = new TmphPollResult { State = TmphPollState.Check };

            /// <summary>
            ///     TCP服务端注册信息
            /// </summary>
            public TmphServices[] Services;

            /// <summary>
            ///     轮询状态
            /// </summary>
            public TmphPollState State;

            /// <summary>
            ///     TCP服务端注册版本号
            /// </summary>
            public int Version;
        }

        /// <summary>
        ///     缓存信息
        /// </summary>
        [Emit.TmphDataSerialize(IsReferenceMember = false, IsMemberMap = false)]
        private struct TmphCache
        {
            /// <summary>
            ///     轮询TCP服务端集合
            /// </summary>
            public TmphClientId[] clients;

            /// <summary>
            ///     TCP服务端标识信息集合
            /// </summary>
            public Dictionary<TmphHost, TmphClientId> HostClients;

            /// <summary>
            ///     TCP服务端口信息集合
            /// </summary>
            public TmphKeyValue<string, int>[] HostPorts;

            /// <summary>
            ///     TCP服务信息集合
            /// </summary>
            public TmphKeyValue<string, TmphServices>[] ServiceCache;
        }

        /// <summary>
        ///     客户端状态
        /// </summary>
        private sealed class TmphClientState
        {
            /// <summary>
            ///     轮询委托
            /// </summary>
            public Func<TmphAsynchronousMethod.TmphReturnValue<TmphPollResult>, bool> Poll;

            /// <summary>
            ///     最后响应时间
            /// </summary>
            public DateTime Time = TmphDate.NowSecond;

            /// <summary>
            ///     判断客户端是否有效
            /// </summary>
            public bool IsClient
            {
                get
                {
                    return Poll == null
                        ? Time.AddSeconds(Config.TmphTcpRegister.Default.RegisterTimeoutSeconds) >= TmphDate.NowSecond
                        : Poll(TmphPollResult.Check);
                }
            }
        }
    }
}