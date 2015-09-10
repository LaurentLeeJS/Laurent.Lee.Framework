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
using Laurent.Lee.CLB.Diagnostics;
using Laurent.Lee.CLB.Emit;
using Laurent.Lee.CLB.IO;
using Laurent.Lee.CLB.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;

namespace Laurent.Lee.CLB.Net.Tcp.Http
{
    /// <summary>
    ///     HTTP服务器
    /// </summary>
    [Code.CSharp.TmphTcpServer(Service = "httpServer", IsIdentityCommand = true,
        VerifyMethodType = typeof(TmphVerifyMethod))]
    public partial class TmphServers : IDisposable
    {
        /// <summary>
        ///     HTTP服务启动状态
        /// </summary>
        public enum TmphStartState
        {
            /// <summary>
            ///     未知状态
            /// </summary>
            Unknown,

            /// <summary>
            ///     HTTP服务已经关闭
            /// </summary>
            Disposed,

            /// <summary>
            ///     主机名称合法
            /// </summary>
            HostError,

            /// <summary>
            ///     域名不合法
            /// </summary>
            DomainError,

            /// <summary>
            ///     域名冲突
            /// </summary>
            DomainExists,

            /// <summary>
            ///     证书文件匹配错误
            /// </summary>
            CertificateMatchError,

            /// <summary>
            ///     证书文件错误
            /// </summary>
            CertificateError,

            /// <summary>
            ///     程序集文件未找到
            /// </summary>
            NotFoundAssembly,

            /// <summary>
            ///     服务启动失败
            /// </summary>
            StartError,

            /// <summary>
            ///     TCP监听服务启动失败
            /// </summary>
            TcpError,

            /// <summary>
            ///     启动成功
            /// </summary>
            Success
        }

        /// <summary>
        ///     程序集信息缓存
        /// </summary>
        private static readonly Dictionary<TmphHashString, Assembly> assemblyCache =
            TmphDictionary.CreateHashString<Assembly>();

        /// <summary>
        ///     程序集信息访问锁
        /// </summary>
        private static readonly object assemblyLock = new object();

        /// <summary>
        ///     HTTP域名服务集合访问锁
        /// </summary>
        private readonly object domainLock = new object();

        /// <summary>
        ///     TCP服务端口信息集合访问锁
        /// </summary>
        private readonly object hostLock = new object();

        /// <summary>
        ///     本地服务程序集运行目录
        /// </summary>
        private readonly string serverPath = TmphWeb.Default.HttpServerPath;

        /// <summary>
        ///     域名搜索
        /// </summary>
        private TmphDomainSearcher domains = TmphDomainSearcher.Default;

        /// <summary>
        ///     文件监视器
        /// </summary>
        private TmphCreateFlieTimeoutWatcher fileWatcher;

        /// <summary>
        ///     HTTP转发代理服务信息
        /// </summary>
        private Code.CSharp.TmphTcpServer forwardHost;

        /// <summary>
        ///     TCP服务端口信息集合
        /// </summary>
        private Dictionary<TmphHost, TmphServer> hosts = TmphDictionary.Create<TmphHost, TmphServer>();

        /// <summary>
        ///     是否已经释放资源
        /// </summary>
        private int isDisposed;

        /// <summary>
        ///     文件监视是否超时
        /// </summary>
        private int isFileWatcherTimeout;

        /// <summary>
        ///     是否加载缓存信息
        /// </summary>
        public bool IsLoadCache = true;

        /// <summary>
        ///     缓存加载访问锁
        /// </summary>
        private int loadCacheLock;

        /// <summary>
        ///     TCP调用服务器配置
        /// </summary>
        private Tcp.TmphServer server;

        /// <summary>
        ///     TCP域名服务缓存文件名
        /// </summary>
        private string cacheFileName
        {
            get { return "httpServer_" + server.attribute.ServiceName + ".cache"; }
        }

        /// <summary>
        ///     是否已经加载缓存信息
        /// </summary>
        public bool IsLoadedCache { get; private set; }

        /// <summary>
        ///     释放资源
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.Increment(ref isDisposed) == 1)
            {
                save();
                forwardHost = null;

                TmphPub.Dispose(ref fileWatcher);

                Monitor.Enter(domainLock);
                var domains = this.domains;
                this.domains = TmphDomainSearcher.Default;
                Monitor.Exit(domainLock);
                domains.Close();
                domains.Dispose();

                TmphServer[] servers = null;
                Monitor.Enter(hostLock);
                try
                {
                    servers = hosts.Values.GetArray();
                    hosts = null;
                }
                finally
                {
                    Monitor.Exit(hostLock);
                    if (servers != null) foreach (var server in servers) server.Dispose();
                }
            }
        }

        /// <summary>
        ///     设置TCP服务端
        /// </summary>
        /// <param name="tcpServer">TCP服务端</param>
        public void SetTcpServer(Tcp.TmphServer tcpServer)
        {
            server = tcpServer;
            fileWatcher = new TmphCreateFlieTimeoutWatcher(TmphProcessCopy.Default.CheckTimeoutSeconds, onFileWatcherTimeout,
                TmphProcessCopyServer.DefaultFileWatcherFilter);
            if (!Config.TmphPub.Default.IsService && TmphProcessCopy.Default.WatcherPath != null)
            {
                try
                {
                    fileWatcher.Add(TmphProcessCopy.Default.WatcherPath);
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, TmphProcessCopy.Default.WatcherPath, false);
                }
            }
            if (IsLoadCache)
            {
                try
                {
                    var cacheFileName = this.cacheFileName;
                    if (File.Exists(cacheFileName))
                    {
                        TmphInterlocked.NoCheckCompareSetSleep0(ref loadCacheLock);
                        try
                        {
                            if (!IsLoadedCache)
                            {
                                var saveInfo =
                                    TmphDataDeSerializer.DeSerialize<TmphSaveInfo>(File.ReadAllBytes(cacheFileName));
                                if (saveInfo.ForwardHost.Port != 0) setForward(saveInfo.ForwardHost);
                                if (saveInfo.Domains.length() != 0)
                                {
                                    foreach (var domain in saveInfo.Domains)
                                    {
                                        try
                                        {
                                            start(domain.AssemblyPath, domain.ServerType, domain.Domains,
                                                domain.IsShareAssembly);
                                        }
                                        catch (Exception error)
                                        {
                                            TmphLog.Error.Add(error, null, false);
                                        }
                                    }
                                }
                                IsLoadedCache = true;
                            }
                        }
                        finally
                        {
                            loadCacheLock = 0;
                        }
                    }
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
            }
        }

        /// <summary>
        ///     保存域名服务器参数集合
        /// </summary>
        private void save()
        {
            try
            {
                var saveInfo = new TmphSaveInfo();
                if (forwardHost != null)
                {
                    saveInfo.ForwardHost.Host = forwardHost.Host;
                    saveInfo.ForwardHost.Port = forwardHost.Port;
                }
                Monitor.Enter(domainLock);
                try
                {
                    saveInfo.Domains = domains.Servers.getHash().GetArray(domain => new TmphSaveInfo.TmphDomainServer
                    {
                        AssemblyPath = domain.AssemblyPath,
                        ServerType = domain.ServerType,
                        IsShareAssembly = domain.IsShareAssembly,
                        Domains = domain.Domains.getFindArray(value => value.Value == 0, value => value.Key)
                    }).getFindArray(value => value.Domains.length() != 0);
                }
                finally
                {
                    Monitor.Exit(domainLock);
                }
                File.WriteAllBytes(cacheFileName, TmphDataSerializer.Serialize(saveInfo));
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, false);
            }
        }

        /// <summary>
        ///     HTTP服务验证函数
        /// </summary>
        /// <returns>是否验证成功</returns>
        [Code.CSharp.TmphTcpServer(IsVerifyMethod = true, IsServerAsynchronousTask = false, InputParameterMaxLength = 1024
            )]
        protected virtual bool verify(string value)
        {
            if (TmphHttp.Default.HttpVerify == null && !Config.TmphPub.Default.IsDebug)
            {
                TmphLog.Error.Add("HTTP服务验证数据不能为空", false, true);
                return false;
            }
            return TmphHttp.Default.HttpVerify == value;
        }

        /// <summary>
        ///     域名状态检测
        /// </summary>
        /// <param name="domain">域名信息</param>
        /// <param name="server">域名服务</param>
        /// <returns>域名状态</returns>
        private TmphStartState checkDomain(ref TmphDomain domain, TmphDomainServer server)
        {
            var domainData = domain.Domain;
            if (domain.Host.Port == 0) domain.Host.Port = domain.CertificateFileName == null ? 80 : 443;
            if (domainData == null)
            {
                var domainString = domain.Host.Host;
                if (domainString.Length() == 0) return TmphStartState.DomainError;
                if (domain.Host.Port != (domain.CertificateFileName == null ? 80 : 443))
                {
                    domainString += ":" + domain.Host.Port.toString();
                }
                domain.Domain = domainData = domainString.GetBytes();
                TmphLog.Default.Add(domainString + " 缺少指定域名", false, false);
            }
            else if (domainData.Length == 0) return TmphStartState.DomainError;
            else if (domain.Host.Port != (domain.CertificateFileName == null ? 80 : 443) &&
                     domainData.indexOf((byte)':') == -1)
            {
                domain.Domain =
                    domainData = (domainData.DeSerialize() + ":" + domain.Host.Port.toString()).GetBytes();
            }
            if (!domain.Host.HostToIpAddress()) return TmphStartState.HostError;
            if (domain.CertificateFileName != null && !File.Exists(domain.CertificateFileName))
            {
                TmphLog.Error.Add("没有找到安全证书文件 " + domain.CertificateFileName, false, false);
                return TmphStartState.CertificateError;
            }
            domainData.toLower();
            TmphDomainSearcher removeDomains = null;
            Monitor.Enter(domainLock);
            try
            {
                domains = domains.Add(domainData, server, out removeDomains);
            }
            finally
            {
                Monitor.Exit(domainLock);
                if (removeDomains != null) removeDomains.Dispose();
            }
            return removeDomains == null ? TmphStartState.DomainExists : TmphStartState.Success;
        }

        /// <summary>
        ///     删除域名信息
        /// </summary>
        /// <param name="domain">域名信息</param>
        private void removeDomain(TmphDomain domain)
        {
            TmphDomainSearcher removeDomains = null;
            Monitor.Enter(domainLock);
            try
            {
                domains = domains.Remove(domain.Domain, out removeDomains);
            }
            finally
            {
                Monitor.Exit(domainLock);
                if (removeDomains != null) removeDomains.Dispose();
            }
        }

        /// <summary>
        ///     启动域名服务
        /// </summary>
        /// <param name="assemblyPath">程序集文件名,包含路径</param>
        /// <param name="TServerType">服务程序类型名称</param>
        /// <param name="domain">域名信息</param>
        /// <param name="isShare">是否共享程序集</param>
        /// <returns>域名服务启动状态</returns>
        [Code.CSharp.TmphTcpServer]
        private TmphStartState start(string assemblyPath, string TServerType, TmphDomain domain, bool isShareAssembly)
        {
            return start(assemblyPath, TServerType, new[] { domain }, isShareAssembly);
        }

        /// <summary>
        ///     启动域名服务
        /// </summary>
        /// <param name="assemblyPath">程序集文件名,包含路径</param>
        /// <param name="TServerType">服务程序类型名称</param>
        /// <param name="domains">域名信息集合</param>
        /// <param name="isShareAssembly">是否共享程序集</param>
        /// <returns>域名服务启动状态</returns>
        [Code.CSharp.TmphTcpServer]
        private TmphStartState start(string assemblyPath, string TServerType, TmphDomain[] domains, bool isShareAssembly)
        {
            if (isDisposed != 0) return TmphStartState.Disposed;
            if (domains.length() == 0) return TmphStartState.DomainError;
            var assemblyFile = new FileInfo(assemblyPath);
            if (!File.Exists(assemblyPath))
            {
                TmphLog.Error.Add("未找到程序集 " + assemblyPath, false, false);
                return TmphStartState.NotFoundAssembly;
            }
            var domainCount = 0;
            var state = TmphStartState.Unknown;
            var server = new TmphDomainServer
            {
                AssemblyPath = assemblyPath,
                ServerType = TServerType,
                Servers = this,
                IsShareAssembly = isShareAssembly
            };
            foreach (var domain in domains)
            {
                if ((state = checkDomain(ref domains[domainCount], server)) != TmphStartState.Success) break;
                ++domainCount;
            }
            try
            {
                if (state == TmphStartState.Success)
                {
                    state = TmphStartState.StartError;
                    Assembly assembly = null;
                    var directory = assemblyFile.Directory;
                    var domainFlags = domains.getArray(value => new TmphKeyValue<TmphDomain, int>(value, 0));
                    TmphHashString pathKey = assemblyPath;
                    Monitor.Enter(assemblyLock);
                    try
                    {
                        if (!isShareAssembly || !assemblyCache.TryGetValue(pathKey, out assembly))
                        {
                            var serverPath = this.serverPath + ((ulong)TmphPub.StartTime.Ticks).toHex16() +
                                             ((ulong)TmphPub.Identity).toHex16() + TmphDirectory.DirectorySeparator;
                            Directory.CreateDirectory(serverPath);
                            foreach (var file in directory.GetFiles()) file.CopyTo(serverPath + file.Name);
                            assembly = Assembly.LoadFrom(serverPath + assemblyFile.Name);
                            if (isShareAssembly) assemblyCache.Add(pathKey, assembly);
                        }
                    }
                    finally
                    {
                        Monitor.Exit(assemblyLock);
                    }
                    server.Server = (Http.TmphDomainServer)Activator.CreateInstance(assembly.GetType(TServerType));
                    var loadDirectory = directory;
                    do
                    {
                        var loadPath = loadDirectory.Name.ToLower();
                        if (loadPath == "release" || loadPath == "bin" || loadPath == "debug")
                        {
                            loadDirectory = loadDirectory.Parent;
                        }
                        else break;
                    } while (loadDirectory != null);
                    server.Server.LoadCheckPath = (loadDirectory ?? directory).FullName;
                    if (server.Server.Start(domains, server.RemoveFileWatcher))
                    {
                        fileWatcher.Add(directory.FullName);
                        server.FileWatcherPath = directory.FullName;
                        if ((state = start(domains)) == TmphStartState.Success)
                        {
                            server.DomainCount = domains.Length;
                            server.Domains = domainFlags;
                            server.IsStart = true;
                            TmphLog.Default.Add(@"domain success
" + domains.joinString(@"
", domain => domain.Host.Host + ":" + domain.Host.Port.toString() + "[" + domain.Domain.DeSerialize() + "]"), false,
                                false);
                            return TmphStartState.Success;
                        }
                    }
                }
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, false);
            }
            foreach (var domain in domains)
            {
                if (domainCount-- == 0) break;
                removeDomain(domain);
            }
            server.Dispose();
            return state;
        }

        /// <summary>
        ///     启动TCP服务
        /// </summary>
        /// <param name="domains">域名信息集合</param>
        /// <returns>HTTP服务启动状态</returns>
        private TmphStartState start(TmphDomain[] domains)
        {
            int hostCount = 0, startCount = 0;
            foreach (var domain in domains)
            {
                if (!domain.IsOnlyHost)
                {
                    var state = start(domain.Host, domain.CertificateFileName);
                    if (state != TmphStartState.Success) break;
                    ++startCount;
                }
                ++hostCount;
            }
            if (startCount != 0 && hostCount == domains.Length) return TmphStartState.Success;
            foreach (var domain in domains)
            {
                if (hostCount-- == 0) break;
                if (!domain.IsOnlyHost) stop(domain.Host);
            }
            return TmphStartState.TcpError;
        }

        /// <summary>
        ///     启动TCP服务
        /// </summary>
        /// <param name="host">TCP服务端口信息</param>
        /// <param name="certificateFileName">安全证书文件</param>
        /// <returns>HTTP服务启动状态</returns>
        private TmphStartState start(TmphHost host, string certificateFileName)
        {
            var state = TmphStartState.TcpError;
            TmphServer server = null;
            Monitor.Enter(hostLock);
            try
            {
                if (hosts.TryGetValue(host, out server))
                {
                    if (server.CheckCertificate(certificateFileName))
                    {
                        ++server.DomainCount;
                        return TmphStartState.Success;
                    }
                    server = null;
                    state = TmphStartState.CertificateMatchError;
                }
                else
                {
                    state = TmphStartState.CertificateError;
                    server = certificateFileName == null
                        ? new TmphServer(this, host)
                        : new TmphSslServer(this, host, certificateFileName);
                    state = TmphStartState.TcpError;
                    if (server.Start())
                    {
                        hosts.Add(host, server);
                        return TmphStartState.Success;
                    }
                }
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, false);
            }
            finally
            {
                Monitor.Exit(hostLock);
            }
            TmphPub.Dispose(ref server);
            return state;
        }

        /// <summary>
        ///     停止域名服务
        /// </summary>
        /// <param name="domain">域名信息</param>
        [Code.CSharp.TmphTcpServer]
        private void stop(TmphDomain domain)
        {
            TmphDomainSearcher removeDomains = null;
            TmphDomainServer domainServer = null;
            var domainData = domain.Domain.toLower();
            Monitor.Enter(domainLock);
            try
            {
                domains = domains.Remove(domainData, out removeDomains, out domainServer);
            }
            finally
            {
                Monitor.Exit(domainLock);
                if (removeDomains != null) removeDomains.Dispose();
            }
            if (domainServer != null && domainServer.Domains != null)
            {
                for (var index = domainServer.Domains.Length; index != 0;)
                {
                    var stopDomain = domainServer.Domains[--index];
                    if ((stopDomain.Value | (stopDomain.Key.Domain.Length ^ domainData.Length)) == 0
                        && Unsafe.TmphMemory.Equal(stopDomain.Key.Domain, domainData, domainData.Length)
                        && Interlocked.CompareExchange(ref domainServer.Domains[index].Value, 1, 0) == 0)
                    {
                        if (!stopDomain.Key.IsOnlyHost) stop(stopDomain.Key.Host);
                        if (Interlocked.Decrement(ref domainServer.DomainCount) == 0) domainServer.Dispose();
                        break;
                    }
                }
            }
        }

        /// <summary>
        ///     停止域名服务
        /// </summary>
        /// <param name="domains">域名信息集合</param>
        [Code.CSharp.TmphTcpServer]
        private void stop(TmphDomain[] domains)
        {
            if (domains != null)
            {
                foreach (var domain in domains) stop(domain);
            }
        }

        ///// <summary>
        ///// 停止域名服务
        ///// </summary>
        ///// <param name="domainServer">域名服务</param>
        //private unsafe void stop(domainServer domainServer)
        //{
        //    if (domainServer != null && domainServer.Domains != null)
        //    {
        //        try
        //        {
        //            for (int index = domainServer.Domains.Length; index != 0; )
        //            {
        //                if (Interlocked.CompareExchange(ref domainServer.Domains[--index].Value, 1, 0) == 0)
        //                {
        //                    stop(domainServer.Domains[index].Key);
        //                    Interlocked.Decrement(ref domainServer.DomainCount);
        //                }
        //            }
        //        }
        //        catch (Exception error)
        //        {
        //            log.Default.Add(error, null, false);
        //        }
        //        finally
        //        {
        //            domainServer.Dispose();
        //        }
        //    }
        //}
        /// <summary>
        ///     停止TCP服务
        /// </summary>
        /// <param name="host">TCP服务端口信息</param>
        private void stop(TmphHost host)
        {
            TmphServer server;
            Monitor.Enter(hostLock);
            try
            {
                if (hosts.TryGetValue(host, out server))
                {
                    if (--server.DomainCount == 0) hosts.Remove(host);
                    else server = null;
                }
            }
            finally
            {
                Monitor.Exit(hostLock);
            }
            if (server != null) server.Dispose();
        }

        /// <summary>
        ///     文件监视超时处理
        /// </summary>
        private void onFileWatcherTimeout()
        {
            if (Interlocked.CompareExchange(ref isFileWatcherTimeout, 1, 0) == 0)
            {
                using (var process = Process.GetCurrentProcess())
                {
                    var file = new FileInfo(process.MainModule.FileName);
                    if (TmphProcessCopy.Default.WatcherPath == null)
                    {
                        var info = new ProcessStartInfo(file.FullName, null);
                        info.UseShellExecute = true;
                        info.WorkingDirectory = file.DirectoryName;
                        using (var newProcess = Process.Start(info)) Environment.Exit(-1);
                    }
                    else
                    {
                        TmphProcessCopyServer.Remove();
                        fileWatcherTimeout();
                    }
                }
            }
        }

        /// <summary>
        ///     文件监视超时处理
        /// </summary>
        private void fileWatcherTimeout()
        {
            if (TmphProcessCopyServer.CopyStart())
            {
                Dispose();
                server.Dispose();
                Environment.Exit(-1);
            }
            else
            {
                TmphTimerTask.Default.Add(fileWatcherTimeout,
                    TmphDate.NowSecond.AddSeconds(TmphProcessCopy.Default.CheckTimeoutSeconds), null);
            }
        }

        /// <summary>
        ///     设置HTTP转发代理服务信息
        /// </summary>
        /// <param name="host">HTTP转发代理服务信息</param>
        /// <returns>是否设置成功</returns>
        [Code.CSharp.TmphTcpServer(IsServerAsynchronousTask = false)]
        private bool setForward(TmphHost host)
        {
            if (isDisposed == 0 && host.HostToIpAddress())
            {
                var tcpServer = new Code.CSharp.TmphTcpServer { Host = host.Host, Port = host.Port };
                if (tcpServer.IpAddress != IPAddress.Any)
                {
                    forwardHost = tcpServer;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///     清除HTTP转发代理服务信息
        /// </summary>
        [Code.CSharp.TmphTcpServer(IsServerAsynchronousTask = false)]
        private void removeForward()
        {
            forwardHost = null;
        }

        /// <summary>
        ///     获取HTTP转发代理服务客户端
        /// </summary>
        /// <returns>HTTP转发代理服务客户端,失败返回null</returns>
        internal virtual TmphClient GetForwardClient()
        {
            var host = forwardHost;
            if (host != null)
            {
                var TmphClient = new TmphClient(host, true);
                if (TmphClient.IsStart) return TmphClient;
                TmphClient.Dispose();
            }
            return null;
        }

        /// <summary>
        ///     获取域名服务信息
        /// </summary>
        /// <param name="domain">域名</param>
        /// <returns>域名服务信息</returns>
        internal virtual TmphDomainServer GetServer(TmphSubArray<byte> domain)
        {
            var server = domains.Get(domain);
            return server != null && server.IsStart ? server : null;
        }

        /// <summary>
        ///     保存信息
        /// </summary>
        [TmphDataSerialize(IsMemberMap = false)]
        private sealed class TmphSaveInfo
        {
            /// <summary>
            ///     域名服务信息集合
            /// </summary>
            [TmphDataSerialize(IsMemberMap = false)]
            public TmphDomainServer[] Domains;

            /// <summary>
            ///     转发服务端口信息
            /// </summary>
            public TmphHost ForwardHost;

            /// <summary>
            ///     域名服务信息
            /// </summary>
            public sealed class TmphDomainServer
            {
                /// <summary>
                ///     程序集文件名,包含路径
                /// </summary>
                public string AssemblyPath;

                /// <summary>
                ///     域名信息集合
                /// </summary>
                public TmphDomain[] Domains;

                /// <summary>
                ///     是否共享程序集
                /// </summary>
                public bool IsShareAssembly;

                /// <summary>
                ///     服务程序类型名称
                /// </summary>
                public string ServerType;
            }
        }

        /// <summary>
        ///     域名服务信息
        /// </summary>
        internal sealed class TmphDomainServer : IDisposable
        {
            /// <summary>
            ///     程序集文件名,包含路径
            /// </summary>
            public string AssemblyPath;

            /// <summary>
            ///     有效域名数量
            /// </summary>
            public int DomainCount;

            /// <summary>
            ///     域名信息集合
            /// </summary>
            public TmphKeyValue<TmphDomain, int>[] Domains;

            /// <summary>
            ///     文件监视路径
            /// </summary>
            public string FileWatcherPath;

            /// <summary>
            ///     是否共享程序集
            /// </summary>
            public bool IsShareAssembly;

            /// <summary>
            ///     是否已经启动
            /// </summary>
            public bool IsStart;

            /// <summary>
            ///     域名服务
            /// </summary>
            public Http.TmphDomainServer Server;

            /// <summary>
            ///     HTTP服务器
            /// </summary>
            public TmphServers Servers;

            /// <summary>
            ///     服务程序类型名称
            /// </summary>
            public string ServerType;

            /// <summary>
            ///     释放资源
            /// </summary>
            public void Dispose()
            {
                RemoveFileWatcher();
                TmphPub.Dispose(ref Server);
            }

            /// <summary>
            ///     删除文件监视路径
            /// </summary>
            public void RemoveFileWatcher()
            {
                var fileWatcher = Servers.fileWatcher;
                if (fileWatcher != null)
                {
                    var path = Interlocked.Exchange(ref FileWatcherPath, null);
                    if (path != null) fileWatcher.Remove(path);
                }
            }
        }

        /// <summary>
        ///     域名搜索
        /// </summary>
        private sealed unsafe class TmphDomainSearcher : IDisposable
        {
            /// <summary>
            ///     默认空域名搜索
            /// </summary>
            public static readonly TmphDomainSearcher Default = new TmphDomainSearcher();

            /// <summary>
            ///     域名信息集合
            /// </summary>
            private readonly byte[][] domains;

            /// <summary>
            ///     域名搜索数据
            /// </summary>
            private TmphPointer data;

            /// <summary>
            ///     域名搜索
            /// </summary>
            public TmphDomainSearcher()
            {
            }

            /// <summary>
            ///     域名搜索
            /// </summary>
            /// <param name="domains">域名信息集合</param>
            /// <param name="servers">域名服务信息集合</param>
            private TmphDomainSearcher(byte[][] domains, TmphDomainServer[] servers)
            {
                this.domains = domains;
                Servers = servers;
                data = TmphStateSearcher.TmphByteArray.Create(domains);
            }

            /// <summary>
            ///     域名服务信息集合
            /// </summary>
            public TmphDomainServer[] Servers { get; private set; }

            /// <summary>
            ///     释放资源
            /// </summary>
            public void Dispose()
            {
                TmphUnmanaged.Free(ref data);
            }

            /// <summary>
            ///     获取域名服务信息
            /// </summary>
            /// <param name="domain">域名</param>
            /// <returns>域名服务信息</returns>
            public TmphDomainServer Get(TmphSubArray<byte> domain)
            {
                var data = this.data;
                if (domain.Count != 0 && data.Data != null)
                {
                    var index = new TmphSearcher(data).Search(domain);
                    if (index >= 0) return Servers[index];
                }
                return null;
            }

            /// <summary>
            ///     添加域名服务信息
            /// </summary>
            /// <param name="domain"></param>
            /// <param name="server"></param>
            /// <returns></returns>
            public TmphDomainSearcher Add(byte[] domain, TmphDomainServer server, out TmphDomainSearcher removeDomains)
            {
                var domains = this.domains;
                var servers = Servers;
                var data = this.data;
                if (domain.Length != 0 && ((data.Data == null || new TmphSearcher(data).Search(domain) < 0)))
                {
                    var reverseDomain = new byte[domain.Length];
                    fixed (byte* domainFixed = domain, reverseDomainFixed = reverseDomain)
                    {
                        for (
                            byte* start = domainFixed,
                                end = domainFixed + domain.Length,
                                write = reverseDomainFixed + domain.Length;
                            start != end;
                            *--write = *start++)
                            ;
                    }
                    var searcher = new TmphDomainSearcher(domains.getAdd(reverseDomain), servers.getAdd(server));
                    removeDomains = this;
                    return searcher;
                }
                removeDomains = null;
                return this;
            }

            /// <summary>
            ///     删除域名服务信息
            /// </summary>
            /// <param name="domain"></param>
            /// <returns></returns>
            public TmphDomainSearcher Remove(byte[] domain, out TmphDomainSearcher removeDomains)
            {
                TmphDomainServer server;
                return Remove(domain, out removeDomains, out server);
            }

            /// <summary>
            ///     删除域名服务信息
            /// </summary>
            /// <param name="domain"></param>
            /// <param name="server">域名服务信息</param>
            /// <returns></returns>
            public TmphDomainSearcher Remove(byte[] domain, out TmphDomainSearcher removeDomains, out TmphDomainServer server)
            {
                var domains = this.domains;
                var servers = Servers;
                var data = this.data;
                if (data.Data != null && domain.Length != 0)
                {
                    var index = new TmphSearcher(data).Search(domain);
                    if (index >= 0)
                    {
                        var searcher = Default;
                        if (domains.Length != 1)
                        {
                            var length = domains.Length - 1;
                            var newDomains = new byte[length][];
                            var newServers = new TmphDomainServer[length];
                            Array.Copy(domains, 0, newDomains, 0, index);
                            Array.Copy(servers, 0, newServers, 0, index);
                            Array.Copy(domains, index + 1, newDomains, index, length - index);
                            Array.Copy(servers, index + 1, newServers, index, length - index);
                            searcher = new TmphDomainSearcher(newDomains, newServers);
                        }
                        server = servers[index];
                        removeDomains = this;
                        return searcher;
                    }
                }
                server = null;
                removeDomains = null;
                return this;
            }

            /// <summary>
            ///     关闭所有域名服务
            /// </summary>
            public void Close()
            {
                foreach (var domain in Servers) domain.Dispose();
            }

            /// <summary>
            ///     字节数组搜索器
            /// </summary>
            private struct TmphSearcher
            {
                /// <summary>
                ///     字节查找表
                /// </summary>
                private readonly byte* bytes;

                /// <summary>
                ///     状态集合
                /// </summary>
                private readonly byte* state;

                /// <summary>
                ///     查询矩阵单位尺寸类型
                /// </summary>
                private readonly byte tableType;

                /// <summary>
                ///     当前状态
                /// </summary>
                private byte* currentState;

                /// <summary>
                ///     ASCII字节搜索器
                /// </summary>
                /// <param name="data">数据起始位置</param>
                [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
                public TmphSearcher(TmphPointer data)
                {
                    var stateCount = *data.Int;
                    currentState = state = data.Byte + sizeof(int);
                    bytes = state + stateCount * 3 * sizeof(int);
                    if (stateCount < 256) tableType = 0;
                    else if (stateCount < 65536) tableType = 1;
                    else tableType = 2;
                }

                /// <summary>
                ///     获取状态索引
                /// </summary>
                /// <param name="end">匹配起始位置</param>
                /// <param name="start">匹配结束位置</param>
                /// <returns>状态索引,失败返回-1</returns>
                private int search(byte* start, byte* end)
                {
                    int dotIndex = -1, value = 0;
                    currentState = state;
                    do
                    {
                        var prefix = currentState + *(int*)currentState;
                        int prefixSize = *(ushort*)(prefix - sizeof(ushort));
                        if (prefixSize != 0)
                        {
                            for (var endPrefix = prefix + prefixSize; prefix != endPrefix; ++prefix)
                            {
                                if (end == start) return dotIndex;
                                if ((uint)((value = *--end) - 'A') < 26) value |= 0x20;
                                if (value != *prefix) return dotIndex;
                            }
                        }
                        if (end == start) return *(int*)(currentState + sizeof(int) * 2);
                        if (value == '.' && (value = *(int*)(currentState + sizeof(int) * 2)) >= 0) dotIndex = value;
                        if (*(int*)(currentState + sizeof(int)) == 0) return dotIndex;
                        if ((uint)((value = *--end) - 'A') < 26) value |= 0x20;
                        int index = *(bytes + value);
                        var table = currentState + *(int*)(currentState + sizeof(int));
                        if (tableType == 0)
                        {
                            if ((index = *(table + index)) == 0) return dotIndex;
                            currentState = state + index * 3 * sizeof(int);
                        }
                        else if (tableType == 1)
                        {
                            if ((index = *(ushort*)(table + index * sizeof(ushort))) == 0) return dotIndex;
                            currentState = state + index * 3 * sizeof(int);
                        }
                        else
                        {
                            if ((index = *(int*)(table + index * sizeof(int))) == 0) return dotIndex;
                            currentState = state + index;
                        }
                    } while (true);
                }

                /// <summary>
                ///     获取状态索引
                /// </summary>
                /// <param name="data">匹配状态</param>
                /// <returns>状态索引,失败返回-1</returns>
                [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
                public int Search(TmphSubArray<byte> data)
                {
                    fixed (byte* dataFixed = data.array)
                    {
                        var start = dataFixed + data.StartIndex;
                        return search(start, start + data.Count);
                    }
                }

                /// <summary>
                ///     获取状态索引
                /// </summary>
                /// <param name="data">匹配状态</param>
                /// <returns>状态索引,失败返回-1</returns>
                [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
                public int Search(byte[] data)
                {
                    fixed (byte* dataFixed = data) return search(dataFixed, dataFixed + data.Length);
                }
            }
        }
    }
}