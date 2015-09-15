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

using System;

namespace Laurent.Lee.CLB.Emit
{
    /// <summary>
    ///     二进制数据序列化类型配置
    /// </summary>
    public sealed class TmphDataSerialize : TmphBinarySerialize
    {
        /// <summary>
        ///     默认二进制数据序列化类型配置
        /// </summary>
        internal static readonly TmphDataSerialize Default = new TmphDataSerialize { IsBaseType = false };

        /// <summary>
        ///     是否序列化成员位图
        /// </summary>
        public bool IsMemberMap = true;

        /// <summary>
        ///     是否检测相同的引用成员(作为根节点时有效)
        /// </summary>
        public bool IsReferenceMember = true;

        /// <summary>
        ///     自定义类型成员标识配置
        /// </summary>
        public sealed class TmphCustom : Attribute
        {
        }
    }
}