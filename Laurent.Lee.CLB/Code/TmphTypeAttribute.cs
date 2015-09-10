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

using Laurent.Lee.CLB.Threading;
using System;
using System.Collections.Generic;

namespace Laurent.Lee.CLB.Code
{
    /// <summary>
    ///     类型自定义属性信息
    /// </summary>
    public static class TmphTypeAttribute
    {
        /// <summary>
        ///     自定义属性信息集合
        /// </summary>
        private static readonly TmphInterlocked.TmphDictionary<Type, object[]> Attributes =
            new TmphInterlocked.TmphDictionary<Type, object[]>(TmphDictionary.CreateOnly<Type, object[]>());

        /// <summary>
        ///     根据类型获取自定义属性信息集合
        /// </summary>
        /// <param name="type">对象类型</param>
        /// <returns>自定义属性信息集合</returns>
        private static object[] Get(Type type)
        {
            object[] values;
            if (Attributes.TryGetValue(type, out values)) return values;
            Attributes.Set(type, values = type.GetCustomAttributes(false));
            return values;
        }

        /// <summary>
        ///     获取自定义属性集合
        /// </summary>
        /// <param name="type">对象类型</param>
        /// <typeparam name="TAttributeType"></typeparam>
        /// <returns></returns>
        internal static IEnumerable<TAttributeType> GetAttributes<TAttributeType>(Type type)
            where TAttributeType : Attribute
        {
            foreach (var value in Get(type))
            {
                if (typeof(TAttributeType).IsAssignableFrom(value.GetType())) yield return (TAttributeType)value;
            }
        }

        /// <summary>
        ///     根据类型获取自定义属性
        /// </summary>
        /// <typeparam name="TAttributeType">自定义属性类型</typeparam>
        /// <param name="type">类型</param>
        /// <param name="isBaseType">是否搜索父类属性</param>
        /// <param name="isInheritAttribute">是否包含继承属性</param>
        /// <returns>自定义属性</returns>
        public static TAttributeType GetAttribute<TAttributeType>(Type type, bool isBaseType, bool isInheritAttribute)
            where TAttributeType : Attribute
        {
            while (type != null && type != typeof(object))
            {
                foreach (var attribute in GetAttributes<TAttributeType>(type))
                {
                    if (isInheritAttribute || attribute.GetType() == typeof(TAttributeType)) return attribute;
                }
                if (isBaseType) type = type.BaseType;
                else return null;
            }
            return null;
        }
    }
}