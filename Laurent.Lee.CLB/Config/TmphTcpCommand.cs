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

using System;

namespace Laurent.Lee.CLB.Config
{
    /// <summary>
    ///     TCP调用配置
    /// </summary>
    public sealed class TmphTcpCommand
    {
        /// <summary>
        ///     默认TCP调用配置
        /// </summary>
        public static readonly TmphTcpCommand Default = new TmphTcpCommand();

        /// <summary>
        ///     命令套接字异步缓存字节数(单位:KB)
        /// </summary>
        private readonly int asyncBufferSize = 0;

        /// <summary>
        ///     命令套接字大数据缓存字节数(单位:KB)
        /// </summary>
        private readonly int bigBufferSize = 128;

        /// <summary>
        ///     命令套接字客户端标识验证超时秒数
        /// </summary>
        private readonly int clientVerifyTimeout = 15;

        /// <summary>
        ///     命令套接字默认超时秒数
        /// </summary>
        private readonly int defaultTimeout = 60;

        /// <summary>
        ///     TCP流超时秒数
        /// </summary>
        private readonly int tcpStreamTimeout = 60;

        /// <summary>
        ///     TCP调用配置
        /// </summary>
        private TmphTcpCommand()
        {
            TmphPub.LoadConfig(this);
        }

        /// <summary>
        ///     命令套接字默认超时秒数
        /// </summary>
        public int DefaultTimeout
        {
            get { return defaultTimeout; }
        }

        /// <summary>
        ///     命令套接字大数据缓存字节数
        /// </summary>
        public int BigBufferSize
        {
            get { return Math.Max(bigBufferSize << 10, TmphAppSetting.StreamBufferSize); }
        }

        /// <summary>
        ///     命令套接字异步缓存字节数
        /// </summary>
        public int AsyncBufferSize
        {
            get { return Math.Max(asyncBufferSize << 10, TmphAppSetting.StreamBufferSize); }
        }

        /// <summary>
        ///     命令套接字客户端标识验证超时秒数
        /// </summary>
        public int ClientVerifyTimeout
        {
            get { return clientVerifyTimeout <= 0 ? 15 : clientVerifyTimeout; }
        }

        /// <summary>
        ///     TCP流超时秒数
        /// </summary>
        public int TcpStreamTimeout
        {
            get { return tcpStreamTimeout <= 0 ? 60 : tcpStreamTimeout; }
        }
    }
}