using System;

namespace Laurent.Lee.CLB.Net.Packet
{
    /// <summary>
    /// UDP数据包
    /// </summary>
    public struct TmphUdp
    {
        /// <summary>
        /// UDP头默认长度
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
        /// 源端口
        /// </summary>
        public uint SourcePort
        {
            get { return ((uint)data.Array[data.StartIndex] << 8) + data.Array[data.StartIndex + 1]; }
        }
        /// <summary>
        /// 目的端口
        /// </summary>
        public uint DestinationPort
        {
            get { return ((uint)data.Array[data.StartIndex + 2] << 8) + data.Array[data.StartIndex + 3]; }
        }
        /// <summary>
        /// UDP数据长度，包括头部8字节
        /// </summary>
        public uint PacketSize
        {
            get { return ((uint)data.Array[data.StartIndex + 4] << 8) + data.Array[data.StartIndex + 5]; }
        }
        /// <summary>
        /// 校验和(UDP和TCP首部都包含一个12字节的伪首部：32位源IP地址,32位目的IP地址,8位保留,8位协议,16位UDP长度)
        /// 如果检验和的计算结果为0，则存入的值为全1（65535），如果传送的检验和是0，则说明发送端没有计算检验和。如果有错误，该报就被丢弃，不产生任何差错报文。
        /// </summary>
        public uint CheckSum
        {
            get { return ((uint)data.Array[data.StartIndex + 6] << 8) + data.Array[data.StartIndex + 7]; }
        }
        /// <summary>
        /// 下层应用数据包
        /// </summary>
        public TmphSubArray<byte> Packet
        {
            get
            {
                return TmphSubArray<byte>.Unsafe(data.Array, data.StartIndex + HeaderSize, data.Count - HeaderSize);
            }
        }
        /// <summary>
        /// UDP数据包
        /// </summary>
        /// <param name="data">数据</param>
        public unsafe TmphUdp(TmphSubArray<byte> data)
        {
            if (data.Count >= HeaderSize)
            {
                fixed (byte* dataFixed = data.Array)
                {
                    byte* start = dataFixed + data.StartIndex;
                    uint packetSize = (uint)(((int)*(start + 4) << 8) + *(start + 5));
                    if (data.Count >= packetSize)
                    {
                        this.data = TmphSubArray<byte>.Unsafe(data.Array, data.StartIndex, (int)packetSize);
                        return;
                    }
                }
            }
            this.data = default(TmphSubArray<byte>);
        }
    }
}
