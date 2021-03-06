﻿/*
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
    /// IPv6数据包
    /// </summary>
    public struct TmphIp6
    {
        /// <summary>
        /// IP标头默认字节数
        /// </summary>
        public const int DefaultHeaderSize = 40;

        /// <summary>
        /// IP标头扩展首部位图
        /// </summary>
        public const ulong ExpandProtocol = (1UL << 60) + (1UL << 51) + (1UL << 50) + (1UL << 44) + (1UL << 43) + 1UL;

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
        /// IP标头结束位置
        /// </summary>
        private uint headerEndIndex;

        /// <summary>
        /// IP版本号，值为6
        /// </summary>
        public int Version
        {
            get { return data.Array[data.StartIndex] >> 4; }
        }

        /// <summary>
        /// 流量类型，默认为0
        /// </summary>
        public uint FlowType
        {
            get { return (((uint)data.Array[data.StartIndex] & 15) << 4) + ((uint)data.Array[data.StartIndex + 1] >> 4); }
        }

        /// <summary>
        /// 标识唯一的流标签(实验阶段的可选字段)
        /// </summary>
        public uint FlowTag
        {
            get
            {
                return (((uint)data.Array[data.StartIndex + 1] & 15) << 16) + ((uint)data.Array[data.StartIndex + 2] << 8)
                    + data.Array[data.StartIndex + 3];
            }
        }

        /// <summary>
        /// 数据包长度(不包括40字节的头部,但包括扩展部分)
        /// </summary>
        public uint PacketSize
        {
            get { return ((uint)data.Array[data.StartIndex + 4] << 8) + data.Array[data.StartIndex + 5]; }
        }

        /// <summary>
        /// IP协议
        /// </summary>
        public TmphIp.TmphProtocol Protocol
        {
            get { return (TmphIp.TmphProtocol)data.Array[data.StartIndex + 6]; }
        }

        /// <summary>
        /// 跳数限制
        /// </summary>
        public byte Hops
        {
            get { return data.Array[data.StartIndex + 7]; }
        }

        /// <summary>
        /// 源IP地址
        /// </summary>
        public TmphSubArray<byte> Source
        {
            get
            {
                return TmphSubArray<byte>.Unsafe(data.Array, data.StartIndex + 8, 16);
            }
        }

        /// <summary>
        /// 目的IP地址
        /// </summary>
        public TmphSubArray<byte> Destination
        {
            get
            {
                return TmphSubArray<byte>.Unsafe(data.Array, data.StartIndex + 24, 16);
            }
        }

        /// <summary>
        /// IP头校验和(应用于TCP,UDP,ICMPv6等协议)
        /// </summary>
        private unsafe uint headerCheckSum
        {
            get
            {
                fixed (byte* dataFixed = data.Array)
                {
                    byte* start = dataFixed + data.StartIndex;
                    uint packetSize = (uint)((*(start + 4) << 8) + *(start + 5) - ((int)headerEndIndex - DefaultHeaderSize));
                    uint value = (packetSize >> 8) + (((packetSize & 0xffU) + *(start + 6)) << 8);
                    value += *(ushort*)(start + 8);
                    value += *(ushort*)(start + 10);
                    value += *(ushort*)(start + 12);
                    value += *(ushort*)(start + 14);
                    value += *(ushort*)(start + 16);
                    value += *(ushort*)(start + 18);
                    value += *(ushort*)(start + 20);
                    value += *(ushort*)(start + 22);
                    value += *(ushort*)(start + 24);
                    value += *(ushort*)(start + 26);
                    value += *(ushort*)(start + 28);
                    value += *(ushort*)(start + 30);
                    value += *(ushort*)(start + 32);
                    value += *(ushort*)(start + 34);
                    value += *(ushort*)(start + 36);
                    value += *(ushort*)(start + 38);
                    return value;
                }
            }
        }

        /// <summary>
        /// 下层应用数据包
        /// </summary>
        public TmphSubArray<byte> Packet
        {
            get
            {
                return TmphSubArray<byte>.Unsafe(data.Array, data.StartIndex + (int)headerEndIndex, data.Count - (int)headerEndIndex);
            }
        }

        /// <summary>
        /// IPv6包
        /// </summary>
        /// <param name="data">数据</param>
        public unsafe TmphIp6(TmphSubArray<byte> data)
        {
            headerEndIndex = (uint)DefaultHeaderSize;
            if (data.Count >= DefaultHeaderSize)
            {
                fixed (byte* dataFixed = data.Array)
                {
                    byte* start = dataFixed + data.StartIndex;
                    int packetSize = (*(start + 4) << 8) + *(start + 5), dataLength = DefaultHeaderSize + packetSize;
                    if (dataLength <= data.Count)
                    {
                        data.UnsafeSetLength(dataLength);
                        byte protocol = *(start + 6);
                        if (protocol < 64)
                        {
                            if ((ExpandProtocol & (1UL << protocol)) == 0) this.data = data;
                            else if (packetSize >= 8)
                            {
                                do
                                {
                                    protocol = *(start + headerEndIndex);
                                    headerEndIndex += 8;
                                    if (protocol == (byte)TmphIp.TmphProtocol.IPv6FragmentHeader)
                                    {
                                        if (headerEndIndex > dataLength) break;
                                    }
                                    else
                                    {
                                        headerEndIndex += (uint)(start + headerEndIndex + 1) << 3;
                                        if (headerEndIndex > dataLength) break;
                                        if (protocol >= 64 || (ExpandProtocol & (1UL << protocol)) == 0)
                                        {
                                            this.data = data;
                                            return;
                                        }
                                    }
                                }
                                while (true);
                                this.data = default(TmphSubArray<byte>);
                            }
                            else this.data = default(TmphSubArray<byte>);
                        }
                        else this.data = data;
                    }
                    else this.data = default(TmphSubArray<byte>);
                }
            }
            else this.data = default(TmphSubArray<byte>);
        }
    }
}