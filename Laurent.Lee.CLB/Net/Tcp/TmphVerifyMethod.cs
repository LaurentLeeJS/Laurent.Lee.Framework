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
using Laurent.Lee.CLB.Config;
using Laurent.Lee.CLB.TcpClient;
using TmphFileBlock = Laurent.Lee.CLB.TcpClient.TmphFileBlock;
using TmphProcessCopy = Laurent.Lee.CLB.TcpClient.TmphProcessCopy;

namespace Laurent.Lee.CLB.Net.Tcp
{
    /// <summary>
    ///     默认TCP调用验证函数
    /// </summary>
    public sealed class TmphVerifyMethod : TmphTcpBase.ITcpClientVerifyMethod<TmphCommandLoadBalancingServer.TmphCommandClient>
        , TmphTcpBase.ITcpClientVerifyMethod<TcpClient.TmphTcpRegister>
        , TmphTcpBase.ITcpClientVerifyMethod<TmphMemoryDatabasePhysical>
        , TmphTcpBase.ITcpClientVerifyMethod<TmphFileBlock>
        , TmphTcpBase.ITcpClientVerifyMethod<TmphHttpServer>
        , TmphTcpBase.ITcpClientVerifyMethod<TmphProcessCopy>
    {
        /// <summary>
        ///     负载均衡服务客户端验证
        /// </summary>
        /// <param name="client">负载均衡服务客户端</param>
        /// <returns>是否通过验证</returns>
        public bool Verify(TmphCommandLoadBalancingServer.TmphCommandClient client)
        {
            return client.Verify(Config.TmphTcpRegister.Default.Verify);
        }

        /// <summary>
        ///     文件分块服务客户端验证
        /// </summary>
        /// <param name="client">TCP调用客户端</param>
        /// <returns>是否通过验证</returns>
        public bool Verify(TmphFileBlock client)
        {
            return client.verify(Config.TmphFileBlock.Default.Verify).Value;
        }

        /// <summary>
        ///     HTTP服务客户端验证
        /// </summary>
        /// <param name="client">TCP调用客户端</param>
        /// <returns>是否通过验证</returns>
        public bool Verify(TmphHttpServer client)
        {
            return client.verify(TmphHttp.Default.HttpVerify).Value;
        }

        /// <summary>
        ///     数据库物理层服务客户端验证
        /// </summary>
        /// <param name="client">TCP调用客户端</param>
        /// <returns>是否通过验证</returns>
        public bool Verify(TmphMemoryDatabasePhysical client)
        {
            return client.verify(TmphMemoryDatabase.Default.PhysicalVerify).Value;
        }

        /// <summary>
        ///     进程复制重启服务客户端验证
        /// </summary>
        /// <param name="client">TCP调用客户端</param>
        /// <returns>是否通过验证</returns>
        public bool Verify(TmphProcessCopy client)
        {
            return client.verify(Config.TmphProcessCopy.Default.Verify).Value;
        }

        /// <summary>
        ///     TCP注册服务客户端验证
        /// </summary>
        /// <param name="client">TCP调用客户端</param>
        /// <returns>是否通过验证</returns>
        public bool Verify(TcpClient.TmphTcpRegister client)
        {
            return client.verify(Config.TmphTcpRegister.Default.Verify).Value;
        }
    }
}