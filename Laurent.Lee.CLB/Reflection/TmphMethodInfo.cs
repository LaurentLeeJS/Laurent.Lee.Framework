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
    ///     成员方法相关操作
    /// </summary>
    public static class TmphMethodInfo
    {
        /// <summary>
        ///     获取匹配的泛型定义方法
        /// </summary>
        /// <typeparam name="TAttributeType">自定义属性类型</typeparam>
        /// <param name="methods">待匹配方法集合</param>
        /// <param name="isMethod">方法匹配器</param>
        /// <returns>匹配的泛型定义方法,失败返回null</returns>
        public static MethodInfo getGenericDefinition<TAttributeType>(this MethodInfo[] methods,
            Func<MethodInfo, bool> isMethod)
            where TAttributeType : Attribute
        {
            if (methods != null)
            {
                foreach (var method in methods)
                {
                    if (method.IsGenericMethod && isMethod(method) && method.CustomAttribute<TAttributeType>() != null)
                    {
                        return method.IsGenericMethodDefinition ? method : method.GetGenericMethodDefinition();
                    }
                }
            }
            return null;
        }

        /// <summary>
        ///     成员方法全名
        /// </summary>
        /// <param name="method">成员方法</param>
        /// <returns>成员方法全名</returns>
        public static string fullName(this MethodInfo method)
        {
            return method != null ? method.DeclaringType.fullName() + "." + method.Name : null;
        }

        /// <summary>
        ///     判断成员方法参数是否匹配
        /// </summary>
        /// <param name="method">成员方法</param>
        /// <param name="TReturnType">返回值类型</param>
        /// <param name="parameters">参数类型集合</param>
        /// <returns>参数是否匹配</returns>
        public static bool isParameter(this MethodInfo method, Type TReturnType, params Type[] parameters)
        {
            if (method != null && method.ReturnType == TReturnType)
            {
                var methodParameters = method.GetParameters();
                if (methodParameters.Length == parameters.length())
                {
                    var index = 0;
                    foreach (var parameter in methodParameters)
                    {
                        if (parameter.ParameterType != parameters[index]) return false;
                        ++index;
                    }
                    return true;
                }
            }
            return false;
        }
    }
}