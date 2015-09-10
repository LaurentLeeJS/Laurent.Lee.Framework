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

using Laurent.Lee.CLB.Emit;
using System.Reflection;

namespace Laurent.Lee.CLB.Code.CSharp
{
    /// <summary>
    ///     内存数据库表格模型配置
    /// </summary>
    public class TmphMemoryDatabaseModel : TmphDataModel
    {
        /// <summary>
        ///     默认空属性
        /// </summary>
        internal static readonly TmphMemoryDatabaseModel Default = new TmphMemoryDatabaseModel();

        /// <summary>
        ///     字段信息
        /// </summary>
        internal struct TmphFieldInfo
        {
            /// <summary>
            ///     数据库成员信息
            /// </summary>
            public TmphDataMember DataMember;

            /// <summary>
            ///     字段信息
            /// </summary>
            public FieldInfo Field;

            /// <summary>
            ///     成员位图索引
            /// </summary>
            public int MemberMapIndex;

            /// <summary>
            ///     字段信息
            /// </summary>
            /// <param name="field"></param>
            /// <param name="attribute"></param>
            public TmphFieldInfo(TmphFieldIndex field, TmphDataMember attribute)
            {
                Field = field.Member;
                DataMember = attribute;
                MemberMapIndex = field.MemberIndex;
            }
        }
    }
}