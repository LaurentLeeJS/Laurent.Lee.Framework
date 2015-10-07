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