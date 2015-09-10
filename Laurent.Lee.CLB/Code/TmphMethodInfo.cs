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

using Laurent.Lee.CLB.Reflection;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Laurent.Lee.CLB.Code
{
    /// <summary>
    ///     成员方法
    /// </summary>
    public sealed class TmphMethodInfo : TmphMemberInfo
    {
        /// <summary>
        ///     类型成员方法缓存
        /// </summary>
        private static readonly Dictionary<Type, TmphMethodInfo[]> MethodCache =
            TmphDictionary.CreateOnly<Type, TmphMethodInfo[]>();

        /// <summary>
        ///     泛型参数拼写
        /// </summary>
        private string _genericParameterName;

        /// <summary>
        ///     成员方法
        /// </summary>
        /// <param name="method">成员方法信息</param>
        /// <param name="filter">选择类型</param>
        internal TmphMethodInfo(MethodInfo method, TmphMemberFilters filter)
            : base(method, filter)
        {
            Method = method;
            ReturnType = Method.ReturnType;
            Parameters = TmphParameterInfo.Get(method);
            OutputParameters =
                Parameters.getFindArray(value => value.Parameter.IsOut || value.Parameter.ParameterType.IsByRef);
            GenericParameters = method.GetGenericArguments().getArray(value => (TmphMemberType)value);
        }

        /// <summary>
        ///     成员方法信息
        /// </summary>
        public MethodInfo Method { get; private set; }

        /// <summary>
        ///     方法名称
        /// </summary>
        public string MethodName
        {
            get { return Method.Name; }
        }

        /// <summary>
        ///     方法泛型名称
        /// </summary>
        public string MethodGenericName
        {
            get { return MethodName + GenericParameterName; }
        }

        /// <summary>
        ///     方法泛型名称
        /// </summary>
        public string StaticMethodGenericName
        {
            get { return MethodGenericName; }
        }

        /// <summary>
        ///     方法全称标识
        /// </summary>
        public string MethodKeyFullName
        {
            get { return Method.DeclaringType.fullName() + MethodKeyName; }
        }

        /// <summary>
        ///     方法标识
        /// </summary>
        public string MethodKeyName
        {
            get
            {
                return "(" + Parameters.joinString(',', value => value.ParameterRef + value.ParameterType.FullName) +
                       ")" + GenericParameterName + MethodName;
            }
        }

        /// <summary>
        ///     返回值类型
        /// </summary>
        public TmphMemberType ReturnType { get; private set; }

        /// <summary>
        ///     是否有返回值
        /// </summary>
        public bool IsReturn
        {
            get { return ReturnType.Type != typeof(void); }
        }

        /// <summary>
        ///     参数集合
        /// </summary>
        public TmphParameterInfo[] Parameters { get; private set; }

        /// <summary>
        ///     泛型参数类型集合
        /// </summary>
        public TmphMemberType[] GenericParameters { get; private set; }

        /// <summary>
        ///     泛型参数拼写
        /// </summary>
        public string GenericParameterName
        {
            get
            {
                if (_genericParameterName == null)
                {
                    var genericParameters = GenericParameters;
                    _genericParameterName = genericParameters.Length == 0
                        ? string.Empty
                        : ("<" + genericParameters.joinString(',', value => value.FullName) + ">");
                }
                return _genericParameterName;
            }
        }

        /// <summary>
        ///     参数集合
        /// </summary>
        public TmphParameterInfo[] OutputParameters { get; private set; }

        /// <summary>
        ///     获取类型的成员方法集合
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>成员方法集合</returns>
        private static TmphMethodInfo[] GetMethods(Type type)
        {
            TmphMethodInfo[] methods;
            if (!MethodCache.TryGetValue(type, out methods))
            {
                var index = 0;
                MethodCache[type] = methods = TmphArray.concat(
                    type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                        .getArray(value => new TmphMethodInfo(value, TmphMemberFilters.PublicStatic)),
                    type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                        .getArray(value => new TmphMethodInfo(value, TmphMemberFilters.PublicInstance)),
                    type.GetMethods(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                        .getArray(value => new TmphMethodInfo(value, TmphMemberFilters.NonPublicStatic)),
                    type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                        .getArray(value => new TmphMethodInfo(value, TmphMemberFilters.NonPublicInstance)))
                    .each(value => value.MemberIndex = index++);
            }
            return methods;
        }

        /// <summary>
        ///     获取匹配成员方法集合
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="filter">选择类型</param>
        /// <param name="isFilter">是否完全匹配选择类型</param>
        /// <returns>匹配的成员方法集合</returns>
        public static TmphSubArray<TmphMethodInfo> GetMethods(Type type, TmphMemberFilters filter, bool isFilter)
        {
            return
                GetMethods(type)
                    .getFind(value => isFilter ? (value.Filter & filter) == filter : ((value.Filter & filter) != 0));
        }

        /// <summary>
        ///     获取匹配成员方法集合
        /// </summary>
        /// <typeparam name="TAttributeType">自定义属性类型</typeparam>
        /// <param name="type">类型</param>
        /// <param name="methods">成员方法集合</param>
        /// <param name="isAttribute">是否匹配自定义属性类型</param>
        /// <param name="isBaseType">是否搜索父类自定义属性</param>
        /// <param name="isInheritAttribute">自定义属性类型是否可继承</param>
        /// <returns>匹配成员方法集合</returns>
        private static TmphMethodInfo[] GetMethods<TAttributeType>
            (Type type, TmphSubArray<TmphMethodInfo> methods, bool isAttribute, bool isBaseType, bool isInheritAttribute)
            where TAttributeType : TmphIgnoreMember
        {
            if (isAttribute)
            {
                return
                    methods.ToList()
                        .getFindArray(value => value.IsAttribute<TAttributeType>(isBaseType, isInheritAttribute));
            }
            return
                methods.ToList()
                    .getFindArray(
                        value =>
                            value.Method.DeclaringType == type &&
                            !value.IsIgnoreAttribute<TAttributeType>(isBaseType, isInheritAttribute));
        }

        /// <summary>
        ///     获取匹配成员方法集合
        /// </summary>
        /// <typeparam name="TAttributeType">自定义属性类型</typeparam>
        /// <param name="type">类型</param>
        /// <param name="filter">选择类型</param>
        /// <param name="isFilter">是否完全匹配选择类型</param>
        /// <param name="isAttribute">是否匹配自定义属性类型</param>
        /// <param name="isBaseType">是否搜索父类自定义属性</param>
        /// <param name="isInheritAttribute">自定义属性类型是否可继承</param>
        /// <returns>匹配成员方法集合</returns>
        public static TmphMethodInfo[] GetMethods<TAttributeType>(Type type, TmphMemberFilters filter, bool isFilter,
            bool isAttribute, bool isBaseType, bool isInheritAttribute)
            where TAttributeType : TmphIgnoreMember
        {
            return GetMethods<TAttributeType>(type, GetMethods(type, filter, isFilter), isAttribute, isBaseType,
                isInheritAttribute);
        }
    }
}