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
using System.Reflection;
using System.Threading;

namespace Laurent.Lee.CLB.Code
{
    /// <summary>
    ///     自定义属性函数信息
    /// </summary>
    public struct TmphAttributeMethod
    {
        ///// <summary>
        ///// 自定义属性函数信息集合
        ///// </summary>
        //private static THInterlocked.dictionary<Type, attributeMethod[]> methods = new THInterlocked.dictionary<Type,attributeMethod[]>(dictionary.CreateOnly<Type, attributeMethod[]>());
        ///// <summary>
        ///// 自定义属性函数信息集合访问锁
        ///// </summary>
        //private static readonly object createLock = new object();
        ///// <summary>
        ///// 根据类型获取自定义属性函数信息集合
        ///// </summary>
        ///// <param name="type">对象类型</param>
        ///// <returns>自定义属性函数信息集合</returns>
        //public static attributeMethod[] Get(Type type)
        //{
        //    attributeMethod[] values;
        //    if (methods.TryGetValue(type, out values)) return values;
        //    Monitor.Enter(createLock);
        //    try
        //    {
        //        if (methods.TryGetValue(type, out values)) return values;
        //        subArray<attributeMethod> array = default(subArray<attributeMethod>);
        //        foreach (MethodInfo method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        //        {
        //            object[] attributes = method.GetCustomAttributes(true);
        //            if (attributes.Length != 0) array.Add(new attributeMethod { Method = method, attributes = attributes });
        //        }
        //        methods.Set(type, values = array.ToArray());
        //    }
        //    finally { Monitor.Exit(createLock); }
        //    return values;
        //}
        /// <summary>
        ///     自定义属性函数信息集合
        /// </summary>
        private static readonly TmphInterlocked.TmphDictionary<Type, TmphAttributeMethod[]> StaticMethods =
            new TmphInterlocked.TmphDictionary<Type, TmphAttributeMethod[]>(
                TmphDictionary.CreateOnly<Type, TmphAttributeMethod[]>());

        /// <summary>
        ///     自定义属性函数信息集合访问锁
        /// </summary>
        private static readonly object CreateStaticLock = new object();

        /// <summary>
        ///     自定义属性集合
        /// </summary>
        private object[] _attributes;

        /// <summary>
        ///     函数信息
        /// </summary>
        public MethodInfo Method;

        /// <summary>
        ///     获取自定义属性集合
        /// </summary>
        /// <typeparam name="TAttributeType"></typeparam>
        /// <returns></returns>
        internal IEnumerable<TAttributeType> Attributes<TAttributeType>() where TAttributeType : Attribute
        {
            foreach (var value in _attributes)
            {
                if (typeof(TAttributeType).IsAssignableFrom(value.GetType())) yield return (TAttributeType)value;
            }
        }

        /// <summary>
        ///     根据成员属性获取自定义属性
        /// </summary>
        /// <typeparam name="TAttributeType">自定义属性类型</typeparam>
        /// <param name="isInheritAttribute">是否包含继承属性</param>
        /// <returns>自定义属性</returns>
        public TAttributeType GetAttribute<TAttributeType>(bool isInheritAttribute) where TAttributeType : Attribute
        {
            TAttributeType value = null;
            var minDepth = int.MaxValue;
            foreach (var attribute in Attributes<TAttributeType>())
            {
                if (isInheritAttribute)
                {
                    var depth = 0;
                    for (var type = attribute.GetType(); type != typeof(TAttributeType); type = type.BaseType) ++depth;
                    if (depth < minDepth)
                    {
                        if (depth == 0) return attribute;
                        minDepth = depth;
                        value = attribute;
                    }
                }
                else if (attribute.GetType() == typeof(TAttributeType)) return attribute;
            }
            return value;
        }

        /// <summary>
        ///     根据类型获取自定义属性函数信息集合
        /// </summary>
        /// <param name="type">对象类型</param>
        /// <returns>自定义属性函数信息集合</returns>
        public static TmphAttributeMethod[] GetStatic(Type type)
        {
            TmphAttributeMethod[] values;
            if (StaticMethods.TryGetValue(type, out values)) return values;
            Monitor.Enter(CreateStaticLock);
            try
            {
                if (StaticMethods.TryGetValue(type, out values)) return values;
                var array = default(TmphSubArray<TmphAttributeMethod>);
                foreach (
                    var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    var attributes = method.GetCustomAttributes(true);
                    if (attributes.Length != 0)
                        array.Add(new TmphAttributeMethod { Method = method, _attributes = attributes });
                }
                StaticMethods.Set(type, values = array.ToArray());
            }
            finally
            {
                Monitor.Exit(CreateStaticLock);
            }
            return values;
        }
    }
}