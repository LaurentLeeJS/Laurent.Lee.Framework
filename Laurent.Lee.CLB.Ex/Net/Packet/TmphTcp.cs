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
    /// TCP数据包
    /// </summary>
    public struct TmphTcp
    {
        /// <summary>
        /// TCP头默认长度
        /// </summary>
        public const int DefaultHeaderSize = 20;

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
        /// 初始连接的请求号，即SEQ值
        /// </summary>
        public uint SequenceNumber
        {
            get { return Laurent.Lee.CLB.Unsafe.TmphMemory.GetUIntBigEndian(data.Array, data.StartIndex + 4); }
        }

        /// <summary>
        /// 对方的应答号，即ACK值
        /// </summary>
        public uint AnswerNumber
        {
            get { return Laurent.Lee.CLB.Unsafe.TmphMemory.GetUIntBigEndian(data.Array, data.StartIndex + 8); }
        }

        /// <summary>
        /// TCP头长度
        /// </summary>
        public int HeaderSize
        {
            get { return (int)((data.Array[data.StartIndex + 12] >> 4) << 2); }
        }

        /// <summary>
        /// 紧急数据标志URG
        /// </summary>
        public int IsUrgent
        {
            get { return data.Array[data.StartIndex + 13] & 0x20; }
        }

        /// <summary>
        /// 确认标志位ACK
        /// </summary>
        public int IsAffirmance
        {
            get { return data.Array[data.StartIndex + 13] & 0x10; }
        }

        /// <summary>
        /// PUSH标志位PSH
        /// </summary>
        public int IsPush
        {
            get { return data.Array[data.StartIndex + 13] & 8; }
        }

        /// <summary>
        /// 复位标志位RST
        /// </summary>
        public int IsReset
        {
            get { return data.Array[data.StartIndex + 13] & 4; }
        }

        /// <summary>
        /// 连接请求标志位SYN(同步)
        /// </summary>
        public int IsConnection
        {
            get { return data.Array[data.StartIndex + 13] & 2; }
        }

        /// <summary>
        /// 结束连接请求标志位FIN
        /// </summary>
        public int IsFinish
        {
            get { return data.Array[data.StartIndex + 13] & 1; }
        }

        /// <summary>
        /// 窗口大小
        /// </summary>
        public uint WindowSize
        {
            get { return ((uint)data.Array[data.StartIndex + 14] << 8) + data.Array[data.StartIndex + 15]; }
        }

        /// <summary>
        /// 校验和
        /// </summary>
        public uint CheckSum
        {
            get { return ((uint)data.Array[data.StartIndex + 16] << 8) + data.Array[data.StartIndex + 17]; }
        }

        /// <summary>
        /// 紧急指针，只有当URG标志置1时紧急指针才有效
        /// </summary>
        public uint UrgentPointer
        {
            get { return ((uint)data.Array[data.StartIndex + 18] << 8) + data.Array[data.StartIndex + 19]; }
        }

        /// <summary>
        /// TCP头扩展
        /// </summary>
        public TmphSubArray<byte> Expand
        {
            get
            {
                int headerSize = HeaderSize;
                return headerSize > DefaultHeaderSize ? TmphSubArray<byte>.Unsafe(data.Array, data.StartIndex + DefaultHeaderSize, DefaultHeaderSize - headerSize) : default(TmphSubArray<byte>);
            }
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
        /// TCP数据包
        /// </summary>
        /// <param name="data">数据</param>
        public TmphTcp(TmphSubArray<byte> data)
        {
            if (data.Count >= DefaultHeaderSize && data.Count >= (uint)((data.Array[data.StartIndex + 12] >> 4) << 2))
            {
                this.data = data;
            }
            else this.data = default(TmphSubArray<byte>);
        }
    }
}