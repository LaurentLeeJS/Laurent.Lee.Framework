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

namespace Laurent.Lee.CLB.Config
{
    /// <summary>
    ///     TCP注册服务配置
    /// </summary>
    public sealed class TmphTcpRegister
    {
        /// <summary>
        ///     默认TCP注册服务配置
        /// </summary>
        public static readonly TmphTcpRegister Default = new TmphTcpRegister();

        /// <summary>
        ///     TCP服务注册无响应超时秒数
        /// </summary>
        private readonly int _registerTimeoutSeconds = 10;

        /// <summary>
        ///     TCP注册服务名称
        /// </summary>
        private readonly string _serviceName = "LeeFrameWork.THTcpResiter";

        /// <summary>
        ///     TCP服务注册起始端口号
        /// </summary>
        private readonly int portStart = 9000;

        /// <summary>
        ///     TCP注册服务依赖
        /// </summary>
        public string[] DependedOn;

        /// <summary>
        ///     TCP注册服务启动后启动的进程
        /// </summary>
        public string[] OnStartProcesses;

        /// <summary>
        ///     TCP注册服务密码
        /// </summary>
        public string Password;

        /// <summary>
        ///     TCP注册服务用户名
        /// </summary>
        public string Username;

        /// <summary>
        ///     TCP服务注册验证
        /// </summary>
        public string Verify;

        /// <summary>
        ///     TCP注册服务配置
        /// </summary>
        private TmphTcpRegister()
        {
            TmphPub.LoadConfig(this);
        }

        /// <summary>
        ///     TCP注册服务名称
        /// </summary>
        public string ServiceName
        {
            get { return _serviceName.Length() == 0 ? "LeeFrameWork.THTcpResiter" : _serviceName; }
        }

        /// <summary>
        ///     TCP服务注册起始端口号
        /// </summary>
        public int PortStart
        {
            get { return portStart; }
        }

        /// <summary>
        ///     TCP服务注册无响应超时秒数
        /// </summary>
        public int RegisterTimeoutSeconds
        {
            get { return _registerTimeoutSeconds; }
        }
    }
}