using System;

namespace Laurent.Lee.CLB.Net.Packet
{
    /// <summary>
    /// linuxSLL数据包
    /// </summary>
    public struct TmphLinuxSLL
    {
        /// <summary>
        /// 数据包类型
        /// </summary>
        public enum TmphType : ushort
        {
            PacketSentToUs,
            PacketBroadCast,
            PacketMulticast,
            PacketSentToSomeoneElse,
            PacketSentByUs
        }
        /// <summary>
        /// 数据包头部长度
        /// </summary>
        public const int HeaderSize = 16;
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
        /// 数据包类型
        /// </summary>
        public TmphType Type
        {
            get { return (TmphType)(ushort)((uint)data.Array[data.StartIndex] << 8) + data.Array[data.StartIndex + 1]; }
        }
        /// <summary>
        /// 地址类型
        /// </summary>
        public uint AddressType
        {
            get { return ((uint)data.Array[data.StartIndex + 2] << 8) + data.Array[data.StartIndex + 3]; }
        }
        /// <summary>
        /// 地址长度
        /// </summary>
        public uint AddressSize
        {
            get { return ((uint)data.Array[data.StartIndex + 4] << 8) + data.Array[data.StartIndex + 5]; }
        }
        /// <summary>
        /// 地址
        /// </summary>
        public TmphSubArray<byte> Address
        {
            get { return TmphSubArray<byte>.Unsafe(data.Array, data.StartIndex + 6, (int)AddressSize); }
        }
        /// <summary>
        /// 帧类型
        /// </summary>
        public TmphFrame Frame
        {
            get
            {
                return (TmphFrame)(ushort)(((uint)data.Array[data.StartIndex + 14] << 8) + data.Array[data.StartIndex + 15]);
            }
        }
        /// <summary>
        /// linuxSLL数据包
        /// </summary>
        /// <param name="data">数据</param>
        public unsafe TmphLinuxSLL(TmphSubArray<byte> data)
        {
            if (data.Count >= HeaderSize)
            {
                fixed (byte* dataFixed = data.Array)
                {
                    byte* start = dataFixed + data.StartIndex;
                    if (data.Count >= ((uint)*(start + 4) << 8) + *(start + 5) + 6)
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
