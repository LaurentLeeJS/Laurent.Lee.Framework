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

using System.Reflection;

namespace Laurent.Lee.CLB.Code
{
    /// <summary>
    ///     字段索引
    /// </summary>
    internal sealed class TmphFieldIndex : TmphMemberIndex<FieldInfo>
    {
        /// <summary>
        ///     字段信息
        /// </summary>
        /// <param name="field">字段信息</param>
        /// <param name="filter">选择类型</param>
        /// <param name="index">成员编号</param>
        public TmphFieldIndex(FieldInfo field, TmphMemberFilters filter, int index)
            : base(field, filter, index)
        {
            IsField = CanGet = true;
            CanSet = !field.IsInitOnly;
            Type = field.FieldType;
        }
    }
}