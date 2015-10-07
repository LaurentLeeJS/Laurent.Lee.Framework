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

using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace Laurent.Lee.CLB.Net.Tcp.Http
{
    /// <summary>
    ///     基于安全连接的HTTP服务
    /// </summary>
    internal sealed class TmphSslServer : TmphServer
    {
        /// <summary>
        ///     SSL证书
        /// </summary>
        private readonly X509Certificate certificate;

        /// <summary>
        ///     SSL证书文件内容
        /// </summary>
        private readonly byte[] certificateFileData;

        /// <summary>
        ///     HTTP服务
        /// </summary>
        /// <param name="servers">HTTP服务器</param>
        /// <param name="host">TCP服务端口信息</param>
        /// <param name="certificateFileName">安全证书文件</param>
        public TmphSslServer(TmphServers servers, TmphHost host, string certificateFileName)
            : base(servers, host)
        {
            certificateFileData = File.ReadAllBytes(certificateFileName);
            certificate = X509Certificate.CreateFromCertFile(certificateFileName);
        }

        /// <summary>
        ///     客户端请求处理
        /// </summary>
        /// <param name="socket">客户端套接字</param>
        protected override void newSocket(Socket socket)
        {
            var ipv6 = default(TmphIpv6Hash);
            var ipv4 = 0;
            var type = clientQueue.NewClient(
                new TmphClient { Server = servers, Socket = socket, Certificate = certificate }, socket, ref ipv4, ref ipv6);
            if (type == TmphClientQueue.TmphSocketType.Ipv4) TmphSslStream.Start(servers, socket, ipv4, certificate);
            else if (type == TmphClientQueue.TmphSocketType.Ipv6) TmphSslStream.Start(servers, socket, ipv6, certificate);
        }

        /// <summary>
        ///     检测安全证书文件
        /// </summary>
        /// <param name="certificateFileName">安全证书文件</param>
        /// <returns>是否成功</returns>
        internal override bool CheckCertificate(string certificateFileName)
        {
            return certificateFileName != null && certificateFileData.equal(File.ReadAllBytes(certificateFileName));
        }

        /// <summary>
        ///     请求处理结束
        /// </summary>
        /// <param name="ipv4">客户端IP</param>
        internal new static void SocketEnd(int ipv4)
        {
            var socket = clientQueue.End(ipv4);
            if (socket.Server != null) TmphSslStream.Start(socket.Server, socket.Socket, ipv4, socket.Certificate);
        }

        /// <summary>
        ///     请求处理结束
        /// </summary>
        /// <param name="ipv6">客户端IP</param>
        internal new static void SocketEnd(TmphIpv6Hash ipv6)
        {
            var socket = clientQueue.End(ipv6);
            if (socket.Server != null) TmphSslStream.Start(socket.Server, socket.Socket, ipv6, socket.Certificate);
        }
    }
}