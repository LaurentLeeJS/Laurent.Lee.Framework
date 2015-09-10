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