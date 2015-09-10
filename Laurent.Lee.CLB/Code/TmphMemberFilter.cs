/*
 *  Copyright 2015 Tony Lee

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

     http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
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