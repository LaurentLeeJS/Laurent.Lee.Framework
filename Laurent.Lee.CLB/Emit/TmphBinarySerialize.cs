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