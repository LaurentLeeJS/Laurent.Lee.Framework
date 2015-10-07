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
using System.Reflection;

namespace Laurent.Lee.CLB.Code
{
    /// <summary>
    ///     成员选择
    /// </summary>
    public abstract class TmphMemberFilter : Attribute
    {
        /// <summary>
        ///     成员是否匹配自定义属性类型
        /// </summary>
        public bool IsAttribute;

        /// <summary>
        ///     是否搜索父类自定义属性
        /// </summary>
        public bool IsBaseTypeAttribute;

        /// <summary>
        ///     成员匹配自定义属性是否可继承
        /// </summary>
        public bool IsInheritAttribute = true;

        /// <summary>
        ///     成员选择类型
        /// </summary>
        public abstract TmphMemberFilters MemberFilter { get; }

        /// <summary>
        ///     默认公有动态成员
        /// </summary>
        public abstract class TmphInstance : TmphMemberFilter
        {
            /// <summary>
            ///     成员选择类型
            /// </summary>
            public TmphMemberFilters Filter = TmphMemberFilters.Instance;

            /// <summary>
            ///     成员选择类型
            /// </summary>
            public override TmphMemberFilters MemberFilter
            {
                get { return Filter & TmphMemberFilters.Instance; }
            }
        }

        /// <summary>
        ///     默认公有动态成员
        /// </summary>
        public abstract class TmphPublicInstance : TmphMemberFilter
        {
            /// <summary>
            ///     成员选择类型
            /// </summary>
            public TmphMemberFilters Filter = TmphMemberFilters.PublicInstance;

            /// <summary>
            ///     成员选择类型
            /// </summary>
            public override TmphMemberFilters MemberFilter
            {
                get { return Filter & TmphMemberFilters.Instance; }
            }

            /// <summary>
            ///     获取字段选择
            /// </summary>
            internal BindingFlags FieldBindingFlags
            {
                get
                {
                    var flags = BindingFlags.Default;
                    if ((Filter & TmphMemberFilters.PublicInstanceField) == TmphMemberFilters.PublicInstanceField)
                        flags |= BindingFlags.Public;
                    if ((Filter & TmphMemberFilters.NonPublicInstanceField) == TmphMemberFilters.NonPublicInstanceField)
                        flags |= BindingFlags.NonPublic;
                    return flags == BindingFlags.Default ? flags : (flags | BindingFlags.Instance);
                }
            }

            /// <summary>
            ///     获取属性选择
            /// </summary>
            internal BindingFlags PropertyBindingFlags
            {
                get
                {
                    var flags = BindingFlags.Default;
                    if ((Filter & TmphMemberFilters.PublicInstanceProperty) == TmphMemberFilters.PublicInstanceProperty)
                        flags |= BindingFlags.Public;
                    if ((Filter & TmphMemberFilters.NonPublicInstanceProperty) ==
                        TmphMemberFilters.NonPublicInstanceProperty)
                        flags |= BindingFlags.NonPublic;
                    return flags == BindingFlags.Default ? flags : (flags | BindingFlags.Instance);
                }
            }
        }

        /// <summary>
        ///     默认公有动态字段成员
        /// </summary>
        public abstract class TmphPublicInstanceField : TmphMemberFilter
        {
            /// <summary>
            ///     成员选择类型
            /// </summary>
            public TmphMemberFilters Filter = TmphMemberFilters.PublicInstanceField;

            /// <summary>
            ///     成员选择类型
            /// </summary>
            public override TmphMemberFilters MemberFilter
            {
                get { return Filter & TmphMemberFilters.Instance; }
            }

            /// <summary>
            ///     获取字段选择
            /// </summary>
            internal BindingFlags FieldBindingFlags
            {
                get
                {
                    var flags = BindingFlags.Default;
                    if ((Filter & TmphMemberFilters.PublicInstanceField) == TmphMemberFilters.PublicInstanceField)
                        flags |= BindingFlags.Public;
                    if ((Filter & TmphMemberFilters.NonPublicInstanceField) == TmphMemberFilters.NonPublicInstanceField)
                        flags |= BindingFlags.NonPublic;
                    return flags == BindingFlags.Default ? flags : (flags | BindingFlags.Instance);
                }
            }
        }

        /// <summary>
        ///     默认动态字段成员
        /// </summary>
        public abstract class TmphInstanceField : TmphMemberFilter
        {
            /// <summary>
            ///     成员选择类型
            /// </summary>
            public TmphMemberFilters Filter = TmphMemberFilters.InstanceField;

            /// <summary>
            ///     成员选择类型
            /// </summary>
            public override TmphMemberFilters MemberFilter
            {
                get { return Filter & TmphMemberFilters.Instance; }
            }

            /// <summary>
            ///     获取字段选择
            /// </summary>
            public BindingFlags FieldBindingFlags
            {
                get
                {
                    var flags = BindingFlags.Default;
                    if ((Filter & TmphMemberFilters.PublicInstanceField) == TmphMemberFilters.PublicInstanceField)
                        flags |= BindingFlags.Public;
                    if ((Filter & TmphMemberFilters.NonPublicInstanceField) == TmphMemberFilters.NonPublicInstanceField)
                        flags |= BindingFlags.NonPublic;
                    return flags == BindingFlags.Default ? flags : (flags | BindingFlags.Instance);
                }
            }
        }
    }
}