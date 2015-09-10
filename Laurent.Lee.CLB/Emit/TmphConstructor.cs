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
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Laurent.Lee.CLB.Emit
{
    /// <summary>
    ///     默认构造函数
    /// </summary>
    public sealed class TmphConstructor : Attribute
    {
    }

    /// <summary>
    ///     默认构造函数
    /// </summary>
    /// <typeparam name="TValueType">数据类型</typeparam>
    public static class TmphConstructor<TValueType>
    {
        /// <summary>
        ///     默认构造函数
        /// </summary>
        public static readonly Func<TValueType> New;

        static TmphConstructor()
        {
            var type = typeof(TValueType);
            if (type.IsValueType || type.IsArray || type == typeof(string))
            {
                New = Default;
                return;
            }
            if (TmphTypeAttribute.GetAttribute<TmphConstructor>(type, false, true) != null)
            {
                foreach (var methodInfo in TmphAttributeMethod.GetStatic(type))
                {
                    if (methodInfo.Method.ReflectedType == type && methodInfo.Method.GetParameters().Length == 0 &&
                        methodInfo.GetAttribute<TmphConstructor>(true) != null)
                    {
                        New = (Func<TValueType>)Delegate.CreateDelegate(typeof(Func<TValueType>), methodInfo.Method);
                        return;
                    }
                }
            }
            if (!type.IsInterface && !type.IsAbstract)
            {
                var constructorInfo =
                    type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null,
                        TmphNullValue<Type>.Array, null);
                if (constructorInfo != null)
                {
                    var dynamicMethod = new DynamicMethod("constructor", type, TmphNullValue<Type>.Array, type, true);
                    dynamicMethod.InitLocals = true;
                    var generator = dynamicMethod.GetILGenerator();
                    generator.Emit(OpCodes.Newobj, constructorInfo);
                    generator.Emit(OpCodes.Ret);
                    New = (Func<TValueType>)dynamicMethod.CreateDelegate(typeof(Func<TValueType>));
                }
            }
        }

        /// <summary>
        ///     默认空值
        /// </summary>
        /// <returns>默认空值</returns>
        public static TValueType Default()
        {
            return default(TValueType);
        }
    }
}