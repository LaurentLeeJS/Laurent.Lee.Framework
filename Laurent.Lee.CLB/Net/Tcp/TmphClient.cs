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