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

namespace Laurent.Lee.CLB.Code
{
    /// <summary>
    ///     参数信息
    /// </summary>
    public sealed partial class TmphParameterInfo
    {
        /// <summary>
        ///     是否引用参数
        /// </summary>
        public bool IsRef;

        /// <summary>
        ///     参数名称
        /// </summary>
        public string ParameterName;

        /// <summary>
        ///     参数信息
        /// </summary>
        /// <param name="parameter">参数信息</param>
        /// <param name="index">参数索引位置</param>
        /// <param name="isLast">是否最后一个参数</param>
        private TmphParameterInfo(ParameterInfo parameter, int index, bool isLast)
        {
            Parameter = parameter;
            ParameterIndex = index;
            var TParameterType = parameter.ParameterType;
            if (TParameterType.IsByRef)
            {
                if (parameter.IsOut) IsOut = true;
                else IsRef = true;
                ParameterType = TParameterType.GetElementType();
            }
            else ParameterType = TParameterType;
            ParameterName = Parameter.Name;
            ParameterJoin = isLast ? null : ", ";
        }

        /// <summary>
        ///     参数信息
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="type">参数类型</param>
        public TmphParameterInfo(string name, Type type)
        {
            ParameterName = name;
            ParameterType = type;
        }

        /// <summary>
        ///     参数信息
        /// </summary>
        public ParameterInfo Parameter { get; private set; }

        /// <summary>
        ///     参数索引位置
        /// </summary>
        public int ParameterIndex { get; private set; }

        /// <summary>
        ///     参数类型
        /// </summary>
        public TmphMemberType ParameterType { get; private set; }

        /// <summary>
        ///     参数连接名称，最后一个参数不带逗号
        /// </summary>
        public string ParameterJoinName
        {
            get { return ParameterName + ParameterJoin; }
        }

        /// <summary>
        ///     带引用修饰的参数连接名称，最后一个参数不带逗号
        /// </summary>
        public string ParameterJoinRefName
        {
            get { return GetRefName(ParameterJoinName); }
        }

        /// <summary>
        ///     带引用修饰的参数名称
        /// </summary>
        public string ParameterTypeRefName
        {
            get { return GetRefName(ParameterType.FullName); }
        }

        /// <summary>
        ///     带引用修饰的参数名称
        /// </summary>
        public string ParameterRefName
        {
            get { return GetRefName(ParameterName); }
        }

        /// <summary>
        ///     参数连接逗号，最后一个参数为null
        /// </summary>
        public string ParameterJoin { get; private set; }

        /// <summary>
        ///     是否输出参数
        /// </summary>
        public bool IsOut { get; private set; }

        /// <summary>
        ///     是否输出参数
        /// </summary>
        public bool IsRefOrOut
        {
            get { return IsRef || IsOut; }
        }

        /// <summary>
        ///     参数引用前缀
        /// </summary>
        public string ParameterRef
        {
            get { return GetRefName(null); }
        }

        /// <summary>
        ///     获取带引用修饰的名称
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns>带引用修饰的名称</returns>
        private string GetRefName(string name)
        {
            if (IsOut) return "out " + name;
            if (IsRef) return "ref " + name;
            return name;
        }

        /// <summary>
        ///     获取方法参数信息集合
        /// </summary>
        /// <param name="method">方法信息</param>
        /// <returns>参数信息集合</returns>
        internal static TmphParameterInfo[] Get(MethodInfo method)
        {
            var parameters = method.GetParameters();
            if (parameters.length() != 0)
            {
                var index = 0;
                return parameters.getArray(value => new TmphParameterInfo(value, index, ++index == parameters.Length));
            }
            return TmphNullValue<TmphParameterInfo>.Array;
        }

        /// <summary>
        ///     获取方法参数信息集合
        /// </summary>
        /// <param name="parameters">参数信息集合</param>
        /// <returns>参数信息集合</returns>
        public static TmphParameterInfo[] Get(TmphParameterInfo[] parameters)
        {
            if (parameters.length() != 0)
            {
                var parameter = parameters[parameters.Length - 1];
                parameters[parameters.Length - 1] = new TmphParameterInfo(parameter.Parameter, parameter.ParameterIndex,
                    true);
            }
            return parameters;
        }
    }
}