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
    ///     HTTP服务相关参数
    /// </summary>
    public sealed class TmphHttp
    {
        /// <summary>
        ///     默认HTTP服务相关参数
        /// </summary>
        public static readonly TmphHttp Default = new TmphHttp();

        /// <summary>
        ///     HTTP内存流最大字节数(单位:KB)
        /// </summary>
        private readonly int _maxMemoryStreamSize = 64;

        /// <summary>
        ///     HTTP最大接收数据字节数(单位:MB)
        /// </summary>
        private readonly int _maxPostDataSize = 4;

        /// <summary>
        ///     HTTP每秒最小表单数据接收字节数(单位KB)
        /// </summary>
        private readonly int _minReceiveSizePerSecond = 8;

        /// <summary>
        ///     套接字接收超时
        /// </summary>
        private readonly int _receiveSeconds = 15;

        /// <summary>
        ///     WebSocket超时
        /// </summary>
        private readonly int _webSocketReceiveSeconds = 60;

        /// <summary>
        ///     大数据缓存字节数(单位:KB)
        /// </summary>
        private readonly int bigBufferSize = 64;

        /// <summary>
        ///     HTTP头部缓存数据大小
        /// </summary>
        private readonly int headerBufferLength = 1 << 10;

        /// <summary>
        ///     HTTP连接每IP最大活动连接数量,小于等于0表示不限
        /// </summary>
        private readonly int ipActiveClientCount = 256;

        /// <summary>
        ///     域名转IP地址缓存数量(小于等于0表示不限)
        /// </summary>
        private readonly int ipAddressCacheCount = 1 << 10;

        /// <summary>
        ///     域名转IP地址缓存时间
        /// </summary>
        private readonly int ipAddressTimeoutMinutes = 60;

        /// <summary>
        ///     HTTP连接每IP最大连接数量,小于等于0表示不限
        /// </summary>
        private readonly int ipClientCount = 1024;

        /// <summary>
        ///     HTTP头部最大项数
        /// </summary>
        private readonly int maxHeaderCount = 32;

        /// <summary>
        ///     HTTP服务名称
        /// </summary>
        private readonly string serviceName = "LeeFrameWork.HttpServer";

        /// <summary>
        ///     Session超时分钟数
        /// </summary>
        private readonly int sessionMinutes = 60;

        /// <summary>
        ///     Session名称
        /// </summary>
        private readonly string sessionName = "fastCSharpSession";

        /// <summary>
        ///     Session超时刷新分钟数
        /// </summary>
        private readonly int sessionRefreshMinutes = 10;

        /// <summary>
        ///     Session服务名称
        /// </summary>
        private readonly string sessionServiceName = "Laurent.Lee.CLB.httpSessionServer";

        /// <summary>
        ///     HTTP服务验证
        /// </summary>
        public string HttpVerify;

        /// <summary>
        ///     HTTP服务启动后启动的进程
        /// </summary>
        public string[] OnStartProcesses;

        /// <summary>
        ///     HTTP服务密码
        /// </summary>
        public string ServicePassword;

        /// <summary>
        ///     HTTP服务用户名
        /// </summary>
        public string ServiceUsername;

        /// <summary>
        ///     Session服务密码
        /// </summary>
        public string SessionServicePassword;

        /// <summary>
        ///     Session服务用户名
        /// </summary>
        public string SessionServiceUsername;

        /// <summary>
        ///     Session服务验证
        /// </summary>
        public string SessionVerify;

        /// <summary>
        ///     HTTP服务相关参数
        /// </summary>
        private TmphHttp()
        {
            TmphPub.LoadConfig(this);
        }

        /// <summary>
        ///     HTTP服务名称
        /// </summary>
        public string ServiceName
        {
            get { return serviceName.Length() == 0 ? "LeeFrameWork.HttpServer" : serviceName; }
        }

        /// <summary>
        ///     HTTP连接每IP最大连接数量,0表示不限
        /// </summary>
        public int IpClientCount
        {
            get { return ipClientCount; }
        }

        /// <summary>
        ///     HTTP连接每IP最大活动连接数量,0表示不限
        /// </summary>
        public int IpActiveClientCount
        {
            get { return ipActiveClientCount; }
        }

        /// <summary>
        ///     HTTP头部缓存数据大小
        /// </summary>
        public int HeaderBufferLength
        {
            get
            {
                return headerBufferLength >= (1 << 10) && headerBufferLength < (1 << 15) ? headerBufferLength : (1 << 10);
            }
        }

        /// <summary>
        ///     HTTP头部最大项数
        /// </summary>
        public int MaxHeaderCount
        {
            get { return maxHeaderCount; }
        }

        /// <summary>
        ///     HTTP每秒最小表单数据接收字节数
        /// </summary>
        public int MinReceiveSizePerSecond
        {
            get { return _minReceiveSizePerSecond > 0 ? _minReceiveSizePerSecond << 10 : 0; }
        }

        /// <summary>
        ///     套接字接收超时
        /// </summary>
        public int ReceiveSeconds
        {
            get { return _receiveSeconds; }
        }

        /// <summary>
        ///     WebSocket超时
        /// </summary>
        public int WebSocketReceiveSeconds
        {
            get { return _webSocketReceiveSeconds; }
        }

        /// <summary>
        ///     HTTP最大接收数据字节数(单位:MB)
        /// </summary>
        public int MaxPostDataSize
        {
            get { return _maxPostDataSize > 0 ? _maxPostDataSize : 4; }
        }

        /// <summary>
        ///     HTTP内存流最大字节数(单位:KB)
        /// </summary>
        public int MaxMemoryStreamSize
        {
            get { return _maxMemoryStreamSize >= 0 ? _maxMemoryStreamSize : 64; }
        }

        /// <summary>
        ///     大数据缓存字节数
        /// </summary>
        public int BigBufferSize
        {
            get
            {
                return Math.Max(Math.Max(bigBufferSize << 10, headerBufferLength << 1), TmphAppSetting.StreamBufferSize);
            }
        }

        /// <summary>
        ///     Session名称
        /// </summary>
        public string SessionName
        {
            get { return sessionName ?? "fastCSharpSession"; }
        }

        /// <summary>
        ///     Session服务名称
        /// </summary>
        public string SessionServiceName
        {
            get
            {
                return sessionServiceName.Length() == 0
                    ? "Laurent.Lee.CLB.httpSessionServer"
                    : sessionServiceName;
            }
        }

        /// <summary>
        ///     Session超时分钟数
        /// </summary>
        public int SessionMinutes
        {
            get { return sessionMinutes; }
        }

        /// <summary>
        ///     Session超时刷新分钟数
        /// </summary>
        public int SessionRefreshMinutes
        {
            get { return sessionRefreshMinutes > sessionMinutes ? sessionMinutes : sessionRefreshMinutes; }
        }

        /// <summary>
        ///     域名转IP地址缓存时间
        /// </summary>
        public int IpAddressTimeoutMinutes
        {
            get { return Math.Max(ipAddressTimeoutMinutes, 1); }
        }

        /// <summary>
        ///     域名转IP地址缓存时间
        /// </summary>
        public int IpAddressCacheCount
        {
            get { return ipAddressCacheCount; }
        }
    }
}