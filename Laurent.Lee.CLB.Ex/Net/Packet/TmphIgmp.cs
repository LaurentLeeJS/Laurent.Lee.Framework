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
    /// IGMP数据包，IGMP报告和查询的生存时间(TTL)一般设置为1，更大的TTL值能被多播路由器转发，一个初始TTL为0的多播数据报将被限制在同一主机。
    /// </summary>
    public struct TmphIgmp
    {
        /// <summary>
        /// 默认IGMP数据长度
        /// </summary>
        public const int DefaultSize = 8;

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
            get { return data.Array[data.StartIndex] >> 4; }
        }

        /// <summary>
        /// 类型为1说明是由多播路由器发出的查询报文，为2说明是主机发出的报告报文。
        /// </summary>
        public int Type
        {
            get { return data.Array[data.StartIndex] & 15; }
        }

        /// <summary>
        /// 校验和
        /// </summary>
        public uint CheckSum
        {
            get { return ((uint)data.Array[data.StartIndex + 2] << 8) + data.Array[data.StartIndex + 3]; }
        }

        /// <summary>
        /// 32位组地址(D类IP地址)
        /// </summary>
        public uint GroupAddress
        {
            get { return Laurent.Lee.CLB.Unsafe.TmphMemory.GetUInt(data.Array, data.StartIndex + 4); }
        }

        /// <summary>
        /// IGMP数据包
        /// </summary>
        /// <param name="data">数据</param>
        public TmphIgmp(TmphSubArray<byte> data)
        {
            this.data = data.Count >= DefaultSize ? data : default(TmphSubArray<byte>);
        }
    }
}