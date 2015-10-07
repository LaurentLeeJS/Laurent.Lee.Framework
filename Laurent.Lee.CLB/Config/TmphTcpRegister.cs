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