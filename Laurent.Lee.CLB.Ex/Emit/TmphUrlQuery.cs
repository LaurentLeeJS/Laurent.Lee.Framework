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
using System;

namespace Laurent.Lee.CLB.Emit
{
    /// <summary>
    /// web查询字符串类型配置
    /// </summary>
    public sealed class TmphUrlQuery : TmphMemberFilter.TmphInstanceField
    {
        /// <summary>
        /// 默认web查询字符串类型配置
        /// </summary>
        public static readonly TmphUrlQuery AllMember = new TmphUrlQuery { IsAllMember = true };

        /// <summary>
        /// 是否序列化所有成员
        /// </summary>
        public bool IsAllMember;

        /// <summary>
        /// web查询字符串成员配置
        /// </summary>
        public sealed class TmphMember : TmphIgnoreMember
        {
        }

        /// <summary>
        /// 自定义类型函数标识配置
        /// </summary>
        public sealed class TmphCustom : Attribute
        {
        }
    }
}