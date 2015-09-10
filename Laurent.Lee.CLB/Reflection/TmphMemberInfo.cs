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

namespace Laurent.Lee.CLB.Reflection
{
    /// <summary>
    ///     成员属性相关操作
    /// </summary>
    public static class TmphMemberInfo
    {
        /// <summary>
        ///     根据成员属性获取自定义属性
        /// </summary>
        /// <typeparam name="TAttributeType">自定义属性类型</typeparam>
        /// <param name="member">成员属性</param>
        /// <param name="isBaseType">是否搜索父类属性</param>
        /// <returns>自定义属性</returns>
        public static TAttributeType CustomAttribute<TAttributeType>(this MemberInfo member, bool isBaseType = false)
            where TAttributeType : Attribute
        {
            TAttributeType value = null;
            if (member != null)
            {
                foreach (var attribute in member.GetCustomAttributes(typeof(TAttributeType), isBaseType))
                {
                    if (attribute.GetType() == typeof(TAttributeType)) return attribute as TAttributeType;
                }
            }
            return value;
        }
    }
}