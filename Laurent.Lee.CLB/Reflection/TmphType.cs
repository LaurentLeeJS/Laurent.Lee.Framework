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
using System.Collections.Generic;
using System.Reflection;

namespace Laurent.Lee.CLB.Reflection
{
    /// <summary>
    ///     类型扩展操作
    /// </summary>
    public static class TmphType
    {
        /// <summary>
        ///     类型名称泛型分隔符
        /// </summary>
        public const char GenericSplit = '`';

        /// <summary>
        ///     类型名称集合
        /// </summary>
        private static readonly Dictionary<Type, string> typeNames;

        static TmphType()
        {
            #region 初始化 类型名称集合

            typeNames = TmphDictionary.CreateOnly<Type, string>();
            typeNames.Add(typeof(bool), "bool");
            typeNames.Add(typeof(byte), "byte");
            typeNames.Add(typeof(sbyte), "sbyte");
            typeNames.Add(typeof(short), "short");
            typeNames.Add(typeof(ushort), "ushort");
            typeNames.Add(typeof(int), "int");
            typeNames.Add(typeof(uint), "uint");
            typeNames.Add(typeof(long), "long");
            typeNames.Add(typeof(ulong), "ulong");
            typeNames.Add(typeof(float), "float");
            typeNames.Add(typeof(double), "double");
            typeNames.Add(typeof(decimal), "decimal");
            typeNames.Add(typeof(char), "char");
            typeNames.Add(typeof(string), "string");
            typeNames.Add(typeof(object), "object");
            typeNames.Add(typeof(void), "void");

            #endregion 初始化 类型名称集合
        }

        /// <summary>
        ///     获取基本类型简称
        /// </summary>
        /// <param name="type">基本类型</param>
        /// <returns>基本类型简称,失败返回null</returns>
        public static string getTypeName(this Type type)
        {
            string name;
            return typeNames.TryGetValue(type, out name) ? name : null;
        }

        /// <summary>
        ///     根据类型获取可用名称
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>类型名称</returns>
        public static string fullName(this Type type)
        {
            if (type == null) return null;
            string value;
            if (typeNames.TryGetValue(type, out value)) return value;
            if (type.IsGenericParameter) return type.Name;
            return new TmphNameBuilder().GetFullName(type);
        }

        /// <summary>
        ///     根据类型获取可用名称
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>类型名称</returns>
        public static string name(this Type type)
        {
            if (type == null) return null;
            string value;
            if (typeNames.TryGetValue(type, out value)) return value;
            if (type.IsGenericParameter) return type.Name;
            return new TmphNameBuilder().GetName(type);
        }

        /// <summary>
        ///     根据类型获取可用名称
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>类型名称</returns>
        public static string onlyName(this Type type)
        {
            string value;
            if (typeNames.TryGetValue(type, out value)) return value;
            value = type.Name;
            if (type.IsGenericTypeDefinition)
            {
                var index = value.IndexOf(GenericSplit);
                if (index != -1) value = value.Substring(0, index);
            }
            return value;
        }

        /// <summary>
        ///     判断类型是否可空类型
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>是否可空类型</returns>
        public static bool isNull(this Type type)
        {
            if (type != null)
            {
                if (type.IsValueType)
                {
                    return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        ///     是否值类型
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>是否值类型</returns>
        public static bool isStruct(this Type type)
        {
            return type != null && type.IsValueType && !type.IsEnum;
        }

        /// <summary>
        ///     获取可空类型的值类型
        /// </summary>
        /// <param name="type">可空类型</param>
        /// <returns>值类型,失败返回null</returns>
        public static Type nullableType(this Type type)
        {
            if (type != null && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return type.GetGenericArguments()[0];
            }
            return null;
        }

        /// <summary>
        ///     值类型转换为可空类型
        /// </summary>
        /// <param name="type">值类型</param>
        /// <returns>可空类型,失败返回null</returns>
        public static Type toNullableType(this Type type)
        {
            if (type != null && type.IsValueType)
            {
                return typeof(Nullable<>).MakeGenericType(type);
            }
            return null;
        }

        /// <summary>
        ///     获取泛型接口类型
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="interfaceType">泛型接口类型定义</param>
        /// <returns>泛型接口类型,失败返回null</returns>
        public static Type getGenericInterface(this Type type, Type interfaceType)
        {
            foreach (var nextType in getGenericInterfaces(type, interfaceType)) return nextType;
            return null;
        }

        /// <summary>
        ///     获取泛型接口类型
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="interfaceType">泛型接口类型定义</param>
        /// <returns>泛型接口类型,失败返回null</returns>
        public static IEnumerable<Type> getGenericInterfaces(this Type type, Type interfaceType)
        {
            if (type != null && interfaceType != null && interfaceType.IsInterface)
            {
                if (type.IsInterface && type.IsGenericType && type.GetGenericTypeDefinition() == interfaceType)
                    yield return type;
                foreach (var nextType in type.GetInterfaces())
                {
                    if (nextType.IsGenericType && nextType.GetGenericTypeDefinition() == interfaceType)
                        yield return nextType;
                }
            }
        }

        /// <summary>
        ///     根据成员属性获取自定义属性
        /// </summary>
        /// <typeparam name="TAttributeType">自定义属性类型</typeparam>
        /// <param name="type">类型</param>
        /// <param name="isCurrentType">是否搜索父类属性</param>
        /// <param name="isInheritAttribute">是否包含继承属性</param>
        /// <returns>自定义属性</returns>
        public static TAttributeType customAttribute<TAttributeType>
            (this Type type, out Type customType, bool isInheritAttribute = false)
            where TAttributeType : Attribute
        {
            while (type != null && type != typeof(object))
            {
                foreach (var attribute in TmphTypeAttribute.GetAttributes<TAttributeType>(type))
                {
                    if (isInheritAttribute || attribute.GetType() == typeof(TAttributeType))
                    {
                        customType = type;
                        return attribute;
                    }
                }
                type = type.BaseType;
            }
            customType = null;
            return null;
        }

        /// <summary>
        ///     获取TryParse函数
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>TryParse函数,失败返回null</returns>
        public static MethodInfo getTryParse(this Type type)
        {
            if (type != null)
            {
                var tryParse = type.GetMethod("TryParse", BindingFlags.Static | BindingFlags.Public, null,
                    new[] { typeof(string), type.MakeByRefType() }, null);
                if (tryParse != null && tryParse.ReturnType == typeof(bool)) return tryParse;
            }
            return null;
        }

        /// <summary>
        ///     获取字段信息
        /// </summary>
        /// <param name="type">数据类型</param>
        /// <param name="fieldName">字段名称</param>
        /// <returns>字段信息</returns>
        public static FieldInfo getField(this Type type, string fieldName)
        {
            return type != null
                ? type.GetField(fieldName,
                    BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance |
                    BindingFlags.IgnoreCase) ??
                  type.GetField(fieldName,
                      BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Static |
                      BindingFlags.Instance | BindingFlags.IgnoreCase)
                : null;
        }

        /// <summary>
        ///     获取属性信息
        /// </summary>
        /// <param name="type">数据类型</param>
        /// <param name="fieldName">属性名称</param>
        /// <returns>属性信息</returns>
        public static PropertyInfo getProperty(this Type type, string propertyName)
        {
            return type != null
                ? type.GetProperty(propertyName,
                    BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance) ??
                  type.GetProperty(propertyName,
                      BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Static |
                      BindingFlags.Instance)
                : null;
        }

        /// <summary>
        ///     类型名称生成器
        /// </summary>
        private struct TmphNameBuilder
        {
            /// <summary>
            ///     名称缓存
            /// </summary>
            private TmphCharStream nameStream;

            /// <summary>
            ///     获取类型名称
            /// </summary>
            /// <param name="type">类型</param>
            /// <returns>类型名称</returns>
            public unsafe string GetFullName(Type type)
            {
                if (type.IsArray)
                {
                    var TmphBuffer = TmphUnmanagedPool.TinyBuffers.Get();
                    try
                    {
                        using (nameStream = new TmphCharStream(TmphBuffer.Char, TmphUnmanagedPool.TinyBuffers.Size >> 1))
                        {
                            array(type, true);
                            return nameStream.ToString();
                        }
                    }
                    finally
                    {
                        TmphUnmanagedPool.TinyBuffers.Push(ref TmphBuffer);
                    }
                }
                if (type.IsGenericType)
                {
                    var TmphBuffer = TmphUnmanagedPool.TinyBuffers.Get();
                    try
                    {
                        using (nameStream = new TmphCharStream(TmphBuffer.Char, TmphUnmanagedPool.TinyBuffers.Size >> 1))
                        {
                            genericFullName(type);
                            return nameStream.ToString();
                        }
                    }
                    finally
                    {
                        TmphUnmanagedPool.TinyBuffers.Push(ref TmphBuffer);
                    }
                }
                var reflectedType = type.ReflectedType;
                if (reflectedType == null) return type.Namespace + "." + type.Name;
                var tinyBuffer = TmphUnmanagedPool.TinyBuffers.Get();
                try
                {
                    using (nameStream = new TmphCharStream(tinyBuffer.Char, TmphUnmanagedPool.TinyBuffers.Size >> 1))
                    {
                        this.reflectedType(type, reflectedType);
                        return nameStream.ToString();
                    }
                }
                finally
                {
                    TmphUnmanagedPool.TinyBuffers.Push(ref tinyBuffer);
                }
            }

            /// <summary>
            ///     获取类型名称
            /// </summary>
            /// <param name="type">类型</param>
            /// <returns>类型名称</returns>
            public unsafe string GetName(Type type)
            {
                if (type.IsArray)
                {
                    var TmphBuffer = TmphUnmanagedPool.TinyBuffers.Get();
                    try
                    {
                        using (nameStream = new TmphCharStream(TmphBuffer.Char, TmphUnmanagedPool.TinyBuffers.Size >> 1))
                        {
                            array(type, false);
                            return nameStream.ToString();
                        }
                    }
                    finally
                    {
                        TmphUnmanagedPool.TinyBuffers.Push(ref TmphBuffer);
                    }
                }
                if (type.IsGenericType)
                {
                    var TmphBuffer = TmphUnmanagedPool.TinyBuffers.Get();
                    try
                    {
                        using (nameStream = new TmphCharStream(TmphBuffer.Char, TmphUnmanagedPool.TinyBuffers.Size >> 1))
                        {
                            genericName(type);
                            return nameStream.ToString();
                        }
                    }
                    finally
                    {
                        TmphUnmanagedPool.TinyBuffers.Push(ref TmphBuffer);
                    }
                }
                return type.Name;
            }

            /// <summary>
            ///     任意类型处理
            /// </summary>
            /// <param name="type">类型</param>
            private void getFullName(Type type)
            {
                string value;
                if (typeNames.TryGetValue(type, out value)) nameStream.WriteNotNull(value);
                else if (type.IsGenericParameter) nameStream.WriteNotNull(type.Name);
                else if (type.IsArray) array(type, true);
                else if (type.IsGenericType) genericFullName(type);
                else
                {
                    var reflectedType = type.ReflectedType;
                    if (reflectedType == null)
                    {
                        nameStream.WriteNotNull(type.Namespace);
                        nameStream.Write('.');
                        nameStream.WriteNotNull(type.Name);
                    }
                    else this.reflectedType(type, reflectedType);
                }
            }

            /// <summary>
            ///     任意类型处理
            /// </summary>
            /// <param name="type">类型</param>
            private void getNameNoArray(Type type)
            {
                string value;
                if (typeNames.TryGetValue(type, out value)) nameStream.WriteNotNull(value);
                else if (type.IsGenericParameter) nameStream.WriteNotNull(type.Name);
                else if (type.IsGenericType) genericName(type);
                else nameStream.WriteNotNull(type.Name);
            }

            /// <summary>
            ///     数组处理
            /// </summary>
            /// <param name="type">类型</param>
            /// <param name="isFullName">是否全称</param>
            private unsafe void array(Type type, bool isFullName)
            {
                var TmphBuffer = TmphUnmanagedPool.TinyBuffers.Get();
                try
                {
                    int* currentRank = TmphBuffer.Int, endRank = (int*)(TmphBuffer.Byte + TmphUnmanagedPool.TinyBuffers.Size);
                    do
                    {
                        if (currentRank == endRank) TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
                        *currentRank++ = type.GetArrayRank();
                    } while ((type = type.GetElementType()).IsArray);
                    if (isFullName) getFullName(type);
                    else getNameNoArray(type);
                    while (currentRank != TmphBuffer.Int)
                    {
                        nameStream.Write('[');
                        var rank = *--currentRank;
                        if (--rank != 0) nameStream.WriteNotNull(rank.toString());
                        nameStream.Write(']');
                    }
                }
                finally
                {
                    TmphUnmanagedPool.TinyBuffers.Push(ref TmphBuffer);
                }
            }

            /// <summary>
            ///     泛型处理
            /// </summary>
            /// <param name="type">类型</param>
            private void genericFullName(Type type)
            {
                var reflectedType = type.ReflectedType;
                if (reflectedType == null)
                {
                    if (type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        getFullName(type.GetGenericArguments()[0]);
                        nameStream.Write('?');
                        return;
                    }
                    var name = type.Name;
                    nameStream.WriteNotNull(type.Namespace);
                    nameStream.Write('.');
                    nameStream.Write(TmphSubString.Unsafe(name, 0, name.IndexOf(GenericSplit)));
                    genericParameter(type);
                    return;
                }
                var reflectedTypeList = default(TmphSubArray<Type>);
                do
                {
                    reflectedTypeList.Add(reflectedType);
                    reflectedType = reflectedType.ReflectedType;
                } while (reflectedType != null);
                var reflectedTypeArray = reflectedTypeList.array;
                var reflectedTypeIndex = reflectedTypeList.Count - 1;
                reflectedType = reflectedTypeArray[reflectedTypeIndex];
                nameStream.WriteNotNull(reflectedType.Namespace);
                var parameterTypes = type.GetGenericArguments();
                var parameterIndex = 0;
                do
                {
                    nameStream.Write('.');
                    if (reflectedType.IsGenericType)
                    {
                        var name = reflectedType.Name;
                        var splitIndex = name.IndexOf(GenericSplit);
                        if (splitIndex != -1)
                        {
                            nameStream.Write(name, 0, splitIndex);
                            var parameterCount = reflectedType.GetGenericArguments().Length;
                            genericParameter(parameterTypes, parameterIndex, parameterCount);
                            parameterIndex = parameterCount;
                        }
                        else nameStream.WriteNotNull(name);
                    }
                    else nameStream.WriteNotNull(reflectedType.Name);
                    if (reflectedTypeIndex == 0)
                    {
                        reflectedType = type;
                        type = null;
                    }
                    else reflectedType = reflectedTypeArray[--reflectedTypeIndex];
                } while (reflectedType != null);
            }

            /// <summary>
            ///     泛型处理
            /// </summary>
            /// <param name="type">类型</param>
            private void genericName(Type type)
            {
                var name = type.Name;
                var splitIndex = name.IndexOf(GenericSplit);
                var reflectedType = type.ReflectedType;
                if (reflectedType == null)
                {
                    nameStream.Write(name, 0, splitIndex);
                    genericParameter(type);
                    return;
                }
                if (splitIndex == -1)
                {
                    nameStream.WriteNotNull(name);
                    return;
                }
                var parameterTypes = type.GetGenericArguments();
                var parameterIndex = 0;
                do
                {
                    if (reflectedType.IsGenericType)
                    {
                        var parameterCount = reflectedType.GetGenericArguments().Length;
                        if (parameterCount != parameterTypes.Length)
                        {
                            parameterIndex = parameterCount;
                            break;
                        }
                    }
                    reflectedType = reflectedType.ReflectedType;
                } while (reflectedType != null);
                nameStream.Write(name, 0, splitIndex);
                genericParameter(parameterTypes, parameterIndex, parameterTypes.Length);
            }

            /// <summary>
            ///     泛型参数处理
            /// </summary>
            /// <param name="type">类型</param>
            private void genericParameter(Type type)
            {
                nameStream.Write('<');
                var index = 0;
                foreach (var parameter in type.GetGenericArguments())
                {
                    if (index != 0) nameStream.Write(',');
                    getFullName(parameter);
                    ++index;
                }
                nameStream.Write('>');
            }

            /// <summary>
            ///     泛型参数处理
            /// </summary>
            /// <param name="parameterTypes">参数类型集合</param>
            /// <param name="startIndex">起始位置</param>
            /// <param name="endIndex">结束位置</param>
            private void genericParameter(Type[] parameterTypes, int startIndex, int endIndex)
            {
                nameStream.Write('<');
                for (getFullName(parameterTypes[startIndex]);
                    ++startIndex != endIndex;
                    getFullName(parameterTypes[startIndex]))
                    nameStream.Write(',');
                nameStream.Write('>');
            }

            /// <summary>
            ///     嵌套类型处理
            /// </summary>
            /// <param name="type">类型</param>
            /// <param name="reflectedType">上层类型</param>
            private void reflectedType(Type type, Type reflectedType)
            {
                var reflectedTypeList = default(TmphSubArray<Type>);
                do
                {
                    reflectedTypeList.Add(reflectedType);
                    reflectedType = reflectedType.ReflectedType;
                } while (reflectedType != null);
                var reflectedTypeArray = reflectedTypeList.array;
                var reflectedTypeIndex = reflectedTypeList.Count - 1;
                reflectedType = reflectedTypeArray[reflectedTypeIndex];
                nameStream.WriteNotNull(reflectedType.Namespace);
                do
                {
                    nameStream.Write('.');
                    nameStream.WriteNotNull(reflectedType.Name);
                    if (reflectedTypeIndex == 0)
                    {
                        reflectedType = type;
                        type = null;
                    }
                    else reflectedType = reflectedTypeArray[--reflectedTypeIndex];
                } while (reflectedType != null);
            }
        }
    }
}