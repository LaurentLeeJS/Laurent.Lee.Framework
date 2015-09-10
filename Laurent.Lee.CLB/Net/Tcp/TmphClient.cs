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
using System;
using System.Net;
using System.Net.Sockets;

namespace Laurent.Lee.CLB.Net.Tcp
{
    /// <summary>
    ///     TCP客户端
    /// </summary>
    public class TmphClient : IDisposable
    {
        /// <summary>
        ///     最后一次连接是否没有指定IP地址
        /// </summary>
        private static bool isAnyIpAddress;

        /// <summary>
        ///     配置信息
        /// </summary>
        protected TmphTcpServer attribute;

        /// <summary>
        ///     是否正在释放资源
        /// </summary>
        protected bool isDispose;

        /// <summary>
        ///     套接字
        /// </summary>
        private TmphSocket netSocket;

        /// <summary>
        ///     TCP客户端
        /// </summary>
        protected Socket tcpClient;

        /// <summary>
        ///     TCP调用服务端
        /// </summary>
        /// <param name="attribute">配置信息</param>
        /// <param name="isStart">是否启动客户端</param>
        public TmphClient(TmphTcpServer attribute, bool isStart)
        {
            this.attribute = attribute;
            if (isStart) start();
        }

        /// <summary>
        ///     套接字
        /// </summary>
        public TmphSocket NetSocket
        {
            get
            {
                if (tcpClient == null) netSocket = new TmphSocket(tcpClient, true);
                return netSocket;
            }
        }

        /// <summary>
        ///     是否启动TCP客户端
        /// </summary>
        public bool IsStart
        {
            get { return tcpClient != null; }
        }

        /// <summary>
        ///     停止客户端链接
        /// </summary>
        public virtual void Dispose()
        {
            if (!isDispose)
            {
                isDispose = true;
                dispose();
            }
        }

        /// <summary>
        ///     启动客户端链接
        /// </summary>
        protected virtual void start()
        {
            tcpClient = Create(attribute);
        }

        /// <summary>
        ///     停止客户端链接
        /// </summary>
        protected void dispose()
        {
            //if (tcpClient != null) tcpClient.Close();
            tcpClient.shutdown();
            tcpClient = null;
            netSocket = null;
        }

        /// <summary>
        ///     创建TCP客户端
        /// </summary>
        /// <param name="attribute">配置信息</param>
        /// <returns>TCP客户端,失败返回null</returns>
        internal static Socket Create(TmphTcpServer attribute)
        {
            Socket socket = null;
            try
            {
                if (attribute.IpAddress == IPAddress.Any)
                {
                    if (!isAnyIpAddress)
                        TmphLog.Error.Add(
                            "客户端TCP连接失败(" + attribute.ServiceName + " " + attribute.Host + ":" +
                            attribute.Port.toString() + ")", true, false);
                    isAnyIpAddress = true;
                    return null;
                }
                isAnyIpAddress = false;
                socket = new Socket(attribute.IpAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(attribute.IpAddress, attribute.Port);
                return socket;
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error,
                    "客户端TCP连接失败(" + attribute.ServiceName + " " + attribute.IpAddress + ":" + attribute.Port.toString() +
                    ")", false);
                TmphLog.Error.Add(
                    "客户端TCP连接失败(" + attribute.ServiceName + " " + attribute.IpAddress + ":" + attribute.Port.toString() +
                    ")", true, false);
                if (socket != null) socket.Close();
            }
            return null;
        }
    }
}