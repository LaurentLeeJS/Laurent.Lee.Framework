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
using TmphProcessCopy = Laurent.Lee.CLB.TcpClient.TmphProcessCopy;
using TmphFileBlock = Laurent.Lee.CLB.TcpClient.TmphFileBlock;
using Laurent.Lee.CLB.TcpClient;

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