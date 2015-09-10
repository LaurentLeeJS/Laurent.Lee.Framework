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
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

namespace Laurent.Lee.CLB.Net.Tcp.Http
{
    /// <summary>
    ///     HTTP服务
    /// </summary>
    internal class TmphServer : Tcp.TmphServer
    {
        /// <summary>
        ///     客户端队列
        /// </summary>
        protected static readonly TmphClientQueue<TmphClient> clientQueue =
            TmphClientQueue<TmphClient>.Create(TmphHttp.Default.IpActiveClientCount, TmphHttp.Default.IpClientCount, dispose);

        /// <summary>
        ///     已绑定域名数量
        /// </summary>
        internal int DomainCount;

        /// <summary>
        ///     HTTP服务器
        /// </summary>
        protected TmphServers servers;

        static TmphServer()
        {
            if (TmphAppSetting.IsCheckMemory) TmphCheckMemory.Add(MethodBase.GetCurrentMethod().DeclaringType);
        }

        /// <summary>
        ///     HTTP服务
        /// </summary>
        /// <param name="servers">HTTP服务器</param>
        /// <param name="host">TCP服务端口信息</param>
        public TmphServer(TmphServers servers, TmphHost host)
            : base(new TmphTcpServer { Host = host.Host, Port = host.Port, IsServer = true })
        {
            this.servers = servers;
            DomainCount = 1;
        }

        /// <summary>
        ///     获取客户端请求
        /// </summary>
        protected override void getSocket()
        {
            acceptSocket();
        }

        /// <summary>
        ///     客户端请求处理
        /// </summary>
        /// <param name="socket">客户端套接字</param>
        protected override void newSocket(Socket socket)
        {
            var ipv6 = default(TmphIpv6Hash);
            var ipv4 = 0;
            var type = clientQueue.NewClient(new TmphClient { Server = servers, Socket = socket }, socket, ref ipv4, ref ipv6);
            if (type == TmphClientQueue.TmphSocketType.Ipv4) TmphSocket.Start(servers, socket, ipv4);
            else if (type == TmphClientQueue.TmphSocketType.Ipv6) TmphSocket.Start(servers, socket, ipv6);
        }

        /// <summary>
        ///     检测安全证书文件
        /// </summary>
        /// <param name="certificateFileName">安全证书文件</param>
        /// <returns>是否成功</returns>
        internal virtual bool CheckCertificate(string certificateFileName)
        {
            return certificateFileName == null;
        }

        /// <summary>
        ///     请求处理结束
        /// </summary>
        /// <param name="ipv4">客户端IP</param>
        internal static void SocketEnd(int ipv4)
        {
            var socket = clientQueue.End(ipv4);
            if (socket.Server != null) TmphSocket.Start(socket.Server, socket.Socket, ipv4);
        }

        /// <summary>
        ///     请求处理结束
        /// </summary>
        /// <param name="ipv6">客户端IP</param>
        internal static void SocketEnd(TmphIpv6Hash ipv6)
        {
            var socket = clientQueue.End(ipv6);
            if (socket.Server != null) TmphSocket.Start(socket.Server, socket.Socket, ipv6);
        }

        /// <summary>
        ///     关闭套接字
        /// </summary>
        /// <param name="socket">套接字</param>
        private static void dispose(TmphClient socket)
        {
            socket.Socket.shutdown();
        }

        /// <summary>
        ///     客户端队列信息
        /// </summary>
        protected struct TmphClient
        {
            /// <summary>
            ///     SSL证书
            /// </summary>
            public X509Certificate Certificate;

            /// <summary>
            ///     HTTP服务器
            /// </summary>
            public TmphServers Server;

            /// <summary>
            ///     套接字
            /// </summary>
            public Socket Socket;
        }
    }
}