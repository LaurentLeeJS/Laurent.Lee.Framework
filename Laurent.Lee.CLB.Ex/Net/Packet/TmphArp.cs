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

namespace Laurent.Lee.CLB.Net.Packet
{
    /// <summary>
    /// ARP数据包(对于ARP和RARP请求/应答数据报大小只有28字节，为了达到46字节的最小长度，必须在后面添加18字节的填充字节)
    /// ARP查询/应答包(ARP是一个无状态的协议，只要有发往本机的ARP应答包，计算机都不加验证的接收，并更新自己的ARP缓存)
    /// 使用ARP欺骗功能前，必须安装winpcap驱动并启动ip路由功能，修改(添加)注册表选项：HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\IPEnableRouter = 0x1　
    /// 对于ARP和RARP请求/应答数据报大小只有28字节，为了达到46字节的最小长度，必须在后面添加18字节的填充字节。
    /// 使用sendarp有个问题，在远端主机不在线后（如拔掉网线后），使用该方法仍然能探测到在线。原因是ARP缓存还在，需要使用ARP -D来删除。
    /// </summary>
    public struct TmphArp
    {
        /// <summary>
        /// ARP数据包长度
        /// </summary>
        public const int PacketSize = 28;

        /// <summary>
        /// 数据
        /// </summary>
        private TmphSubArray<byte> data;

        /// <summary>
        /// 数据包是否有效
        /// </summary>
        public bool IsPacket
        {
            get { return data.Array != null; }
        }

        /// <summary>
        /// 硬件类型,0x0001表示10Mb以太网
        /// </summary>
        public uint Hardware
        {
            get { return ((uint)data.Array[data.StartIndex] << 8) + data.Array[data.StartIndex + 1]; }
        }

        /// <summary>
        /// 协议类型,为0x0800表示IP地址
        /// </summary>
        public uint Protocol
        {
            get { return ((uint)data.Array[data.StartIndex + 2] << 8) + data.Array[data.StartIndex + 3]; }
        }

        /// <summary>
        /// 硬件地址长度(即MAC地址长度),以太网为0x06
        /// </summary>
        public byte HardwareSize
        {
            get { return data.Array[data.StartIndex + 4]; }
        }

        /// <summary>
        /// 协议地址长度(即IP地址长度),以太网为0x04
        /// </summary>
        public byte AgreementSize
        {
            get { return data.Array[data.StartIndex + 5]; }
        }

        /// <summary>
        /// ARP请求包的OP值为1，ARP应答包的OP值为2，RARP请求包的OP值为3，RARP应答包的OP值为4
        /// </summary>
        public uint RequestOrResponse
        {
            get { return ((uint)data.Array[data.StartIndex + 6] << 8) + data.Array[data.StartIndex + 7]; }
        }

        /// <summary>
        /// 发送者以太网地址
        /// </summary>
        public TmphSubArray<byte> SendMac
        {
            get { return TmphSubArray<byte>.Unsafe(data.Array, data.StartIndex + 8, 6); }
        }

        /// <summary>
        /// 发送者的IP地址
        /// </summary>
        public uint SendIp
        {
            get { return Laurent.Lee.CLB.Unsafe.TmphMemory.GetUInt(data.Array, data.StartIndex + 14); }
        }

        /// <summary>
        /// 目的以太网地址
        /// </summary>
        public TmphSubArray<byte> DestinationMac
        {
            get { return TmphSubArray<byte>.Unsafe(data.Array, data.StartIndex + 18, 6); }
        }

        /// <summary>
        /// 目的IP(查询MAC地址的IP)
        /// </summary>
        public uint destinationIp
        {
            get { return Laurent.Lee.CLB.Unsafe.TmphMemory.GetUInt(data.Array, data.StartIndex + 24); }
        }

        /// <summary>
        /// ARP数据包
        /// </summary>
        /// <param name="data">数据</param>
        public TmphArp(TmphSubArray<byte> data)
        {
            this.data = data.Count >= PacketSize ? data : default(TmphSubArray<byte>);
        }
    }
}