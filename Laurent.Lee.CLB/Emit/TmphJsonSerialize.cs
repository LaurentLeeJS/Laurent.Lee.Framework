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
    ///     JSON序列化类型配置
    /// </summary>
    public sealed class TmphJsonSerialize : TmphMemberFilter.TmphPublicInstance
    {
        /// <summary>
        ///     默认序列化类型配置
        /// </summary>
        internal static readonly TmphJsonSerialize AllMember = new TmphJsonSerialize
        {
            IsAllMember = true,
            IsBaseType = false
        };

        /// <summary>
        ///     是否序列化所有成员
        /// </summary>
        public bool IsAllMember;

        /// <summary>
        ///     是否作用与派生类型
        /// </summary>
        public bool IsBaseType = true;

        /// <summary>
        ///     Json序列化成员配置
        /// </summary>
        public sealed class TmphMember : TmphIgnoreMember
        {
        }

        /// <summary>
        ///     自定义类型函数标识配置
        /// </summary>
        public sealed class TmphCustom : Attribute
        {
        }
    }
}