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
    ///     属性索引
    /// </summary>
    internal sealed class TmphPropertyIndex : TmphMemberIndex<PropertyInfo>
    {
        /// <summary>
        ///     属性信息
        /// </summary>
        /// <param name="property">属性信息</param>
        /// <param name="filter">选择类型</param>
        /// <param name="index">成员编号</param>
        public TmphPropertyIndex(PropertyInfo property, TmphMemberFilters filter, int index)
            : base(property, filter, index)
        {
            CanSet = property.CanWrite;
            CanGet = property.CanRead;
            Type = property.PropertyType;
        }
    }
}