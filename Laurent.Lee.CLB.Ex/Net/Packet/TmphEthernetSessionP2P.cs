using System;

namespace Laurent.Lee.CLB.Net.Packet
{
    /// <summary>
    /// 以太网会话点到点协议数据包
    /// </summary>
    public struct TmphEthernetSessionP2P
    {
        /// <summary>
        /// 以太网会话点到点协议数据包代码
        /// </summary>
        public enum TmphCode : byte
        {
            ActiveDiscoveryInitiation = 9,
            ActiveDiscoveryOffer = 7,
            ActiveDiscoveryTerminate = 0xa7,
            SessionStage = 0
        }
        /// <summary>
        /// 点到点协议
        /// </summary>
        public enum TmphProtocol : byte
        {
            IPv4 = 0x21,
            IPv6 = 0x57,
            Padding = 1
        }
        /// <summary>
        /// 数据包头长度
        /// </summary>
        public const int HeaderSize = 8;
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
        /// 版本号
        /// </summary>
        public int Version
        {
            get { return (data.Array[data.StartIndex] >> 4) & 240; }
        }
        /// <summary>
        /// 类型
        /// </summary>
        public int Type
        {
            get { return data.Array[data.StartIndex] & 15; }
        }
        /// <summary>
        /// 代码类型
        /// </summary>
        public TmphCode Code
        {
            get { return (TmphCode)data.Array[data.StartIndex + 1]; }
        }
        /// <summary>
        /// 标识
        /// </summary>
        public ushort SessionId
        {
            get { return Laurent.Lee.CLB.Unsafe.TmphMemory.GetUShort(data.Array, data.StartIndex + 2); }
        }
        /// <summary>
        /// 数据包长度(单位未知)
        /// </summary>
        public uint packetSize
        {
            get { return ((uint)data.Array[data.StartIndex + 4] << 8) + data.Array[data.StartIndex + 5]; }
        }
        /// <summary>
        /// 帧类型
        /// </summary>
        public TmphFrame Frame
        {
            get
            {
                if (data.Array[data.StartIndex + 6] == 0)
                {
                    switch (data.Array[data.StartIndex + 7])
                    {
                        case (byte)TmphProtocol.IPv4:
                            return TmphFrame.IpV4;
                        case (byte)TmphProtocol.IPv6:
                            return TmphFrame.IpV6;
                    }
                }
                return TmphFrame.None;
            }
        }
        /// <summary>
        /// 以太网会话点到点协议数据包
        /// </summary>
        /// <param name="data">数据</param>
        public unsafe TmphEthernetSessionP2P(TmphSubArray<byte> data)
        {
            if (data.Count >= HeaderSize)
            {
                fixed (byte* dataFixed = data.Array)
                {
                    byte* start = dataFixed + data.StartIndex;
                    uint packetSize = ((uint)*(start + 4) << 8) + *(start + 5);
                    if (data.Count >= packetSize)
                    {
                        this.data = data;
                        return;
                    }
                }
            }
            this.data = default(TmphSubArray<byte>);
        }
    }
}
