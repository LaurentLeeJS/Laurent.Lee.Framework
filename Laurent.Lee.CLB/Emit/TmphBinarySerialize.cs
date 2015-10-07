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

using Laurent.Lee.CLB.Code;

namespace Laurent.Lee.CLB.Emit
{
    /// <summary>
    ///     二进制序列化配置
    /// </summary>
    public abstract class TmphBinarySerialize : TmphMemberFilter.TmphInstanceField
    {
        /// <summary>
        ///     是否作用于未知派生类型
        /// </summary>
        public bool IsBaseType = true;

        /// <summary>
        ///     当没有JSON序列化成员时是否预留JSON序列化标记
        /// </summary>
        public bool IsJson;

        /// <summary>
        ///     二进制数据序列化成员配置
        /// </summary>
        public sealed class TmphMember : TmphIgnoreMember
        {
            /// <summary>
            ///     是否采用JSON混合序列化
            /// </summary>
            public bool IsJson;
        }
    }
}