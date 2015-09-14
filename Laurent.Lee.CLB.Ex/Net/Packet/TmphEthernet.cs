using System;

namespace Laurent.Lee.CLB.Net.Packet
{
    /// <summary>
    /// 以太网数据包
    /// </summary>
    public struct TmphEthernet
    {
        /// <summary>
        /// 以太网数据包头部长度
        /// </summary>
        public const int HeaderSize = 14;
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
        /// 以太网目的地址
        /// </summary>
        public TmphSubArray<byte> DestinationMac
        {
            get { return TmphSubArray<byte>.Unsafe(data.Array, data.StartIndex, 6); }
        }
        /// <summary>
        /// 以太网源地址
        /// </summary>
        public TmphSubArray<byte> SourceMac
        {
            get { return TmphSubArray<byte>.Unsafe(data.Array, data.StartIndex + 6, 6); }
        }
        /// <summary>
        /// 帧类型
        /// </summary>
        public TmphFrame Frame
        {
            get
            {
                return (TmphFrame)(ushort)(((uint)data.Array[data.StartIndex + 12] << 8) + data.Array[data.StartIndex + 13]);
            }
        }
        /// <summary>
        /// 以太网数据包
        /// </summary>
        /// <param name="data">数据</param>
        public TmphEthernet(TmphSubArray<byte> data)
        {
            this.data = data.Count >= HeaderSize ? data : default(TmphSubArray<byte>);
        }
    }
}
