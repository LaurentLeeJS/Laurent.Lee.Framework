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
    ///     Json解析类型配置
    /// </summary>
    public sealed class TmphJsonParse : TmphMemberFilter.TmphPublicInstance
    {
        /// <summary>
        ///     默认解析所有成员
        /// </summary>
        internal static readonly TmphJsonParse AllMember = new TmphJsonParse
        {
            Filter = TmphMemberFilters.Instance,
            IsAllMember = true,
            IsBaseType = false
        };

        /// <summary>
        ///     是否解析所有成员
        /// </summary>
        public bool IsAllMember;

        /// <summary>
        ///     是否作用与派生类型
        /// </summary>
        public bool IsBaseType = true;

        /// <summary>
        ///     Json解析成员配置
        /// </summary>
        public sealed class TmphMember : TmphIgnoreMember
        {
            /// <summary>
            ///     是否默认解析成员
            /// </summary>
            public bool IsDefault;
        }

        /// <summary>
        ///     自定义类型解析函数标识配置
        /// </summary>
        public sealed class TmphCustom : Attribute
        {
        }
    }
}