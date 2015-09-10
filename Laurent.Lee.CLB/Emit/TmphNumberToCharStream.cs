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

using Laurent.Lee.CLB.Web;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Laurent.Lee.CLB.Emit
{
    /// <summary>
    ///     数字转换成字符串
    /// </summary>
    internal static class TmphNumberToCharStream
    {
        /// <summary>
        ///     连接字符串集合函数信息
        /// </summary>
        public static readonly MethodInfo StructJoinCharMethod =
            typeof(TmphNumberToCharStream).GetMethod("structJoinChar", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     连接字符串集合函数信息
        /// </summary>
        public static readonly MethodInfo StructSubArrayJoinCharMethod =
            typeof(TmphNumberToCharStream).GetMethod("structSubArrayJoinChar",
                BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     连接字符串集合函数信息
        /// </summary>
        public static readonly MethodInfo NullableJoinCharMethod =
            typeof(TmphNumberToCharStream).GetMethod("nullableJoinChar", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     连接字符串集合函数信息
        /// </summary>
        public static readonly MethodInfo NullableSubArrayJoinCharMethod =
            typeof(TmphNumberToCharStream).GetMethod("nullableSubArrayJoinChar",
                BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     连接字符串集合函数信息
        /// </summary>
        public static readonly MethodInfo ClassJoinCharMethod = typeof(TmphNumberToCharStream).GetMethod(
            "classJoinChar", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     连接字符串集合函数信息
        /// </summary>
        public static readonly MethodInfo ClassSubArrayJoinCharMethod =
            typeof(TmphNumberToCharStream).GetMethod("classSubArrayJoinChar",
                BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     连接字符串集合函数信息
        /// </summary>
        public static readonly MethodInfo StringJoinCharMethod =
            typeof(TmphNumberToCharStream).GetMethod("stringJoinChar", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     连接字符串集合函数信息
        /// </summary>
        public static readonly MethodInfo StringSubArrayJoinCharMethod =
            typeof(TmphNumberToCharStream).GetMethod("stringSubArrayJoinChar",
                BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     数值转换调用函数信息集合
        /// </summary>
        private static readonly Dictionary<Type, MethodInfo> toStringMethods;

        static TmphNumberToCharStream()
        {
            toStringMethods = TmphDictionary.CreateOnly<Type, MethodInfo>();
            foreach (var method in typeof(TmphNumber).GetMethods(BindingFlags.Static | BindingFlags.NonPublic))
            {
                if (method.Name == "ToString")
                {
                    var parameters = method.GetParameters();
                    if (parameters.Length == 2 && parameters[1].ParameterType == typeof(TmphCharStream))
                    {
                        toStringMethods.Add(parameters[0].ParameterType, method);
                    }
                }
            }
        }

        /// <summary>
        ///     连接字符串集合
        /// </summary>
        /// <param name="array">字符串集合</param>
        /// <param name="join">字符连接</param>
        /// <returns>连接后的字符串</returns>
        private static unsafe string joinString(string[] array, char join)
        {
            var length = 0;
            foreach (var nextString in array)
            {
                if (nextString != null) length += nextString.Length;
            }
            var value = TmphString.FastAllocateString(length + array.Length - 1);
            fixed (char* valueFixed = value)
            {
                var write = valueFixed;
                foreach (var nextString in array)
                {
                    if (write != valueFixed) *write++ = join;
                    if (nextString != null)
                    {
                        Unsafe.TmphString.Copy(nextString, write);
                        write += nextString.Length;
                    }
                }
            }
            return value;
        }

        /// <summary>
        ///     连接字符串集合
        /// </summary>
        /// <param name="array">字符串集合</param>
        /// <param name="join">字符连接</param>
        /// <returns>连接后的字符串</returns>
        private static unsafe string joinNullString(string[] array, char join)
        {
            var length = 0;
            foreach (var nextString in array) length += nextString == null ? 4 : nextString.Length;
            var value = TmphString.FastAllocateString(length + array.Length - 1);
            fixed (char* valueFixed = value)
            {
                var write = valueFixed;
                foreach (var nextString in array)
                {
                    if (write != valueFixed) *write++ = join;
                    if (nextString == null)
                    {
                        *(int*)write = 'n' + ('u' << 16);
                        *(int*)(write + 2) = 'l' + ('l' << 16);
                        write += 4;
                    }
                    else
                    {
                        Unsafe.TmphString.Copy(nextString, write);
                        write += nextString.Length;
                    }
                }
            }
            return value;
        }

        /// <summary>
        ///     连接字符串集合
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="array"></param>
        /// <param name="join"></param>
        /// <param name="isNull"></param>
        /// <returns></returns>
        private static string structJoinChar<TValueType>(TValueType[] array, char join, bool isNull)
            where TValueType : struct
        {
            if (array.Length == 1)
            {
                foreach (var value in array) return value.ToString();
            }
            var stringArray = new string[array.Length];
            var index = 0;
            foreach (var value in array) stringArray[index++] = value.ToString();
            return joinString(stringArray, join);
        }

        /// <summary>
        ///     连接字符串集合
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="array"></param>
        /// <param name="join"></param>
        /// <param name="isNull"></param>
        /// <returns></returns>
        private static string structSubArrayJoinChar<TValueType>(TmphSubArray<TValueType> subArray, char join, bool isNull)
            where TValueType : struct
        {
            var array = subArray.array;
            if (subArray.Count == 1) return array[subArray.StartIndex].ToString();
            var stringArray = new string[subArray.Count];
            int index = 0, startIndex = subArray.StartIndex, endIndex = startIndex + subArray.Count;
            do
            {
                stringArray[index++] = array[startIndex++].ToString();
            } while (startIndex != endIndex);
            return joinString(stringArray, join);
        }

        /// <summary>
        ///     连接字符串集合
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="array"></param>
        /// <param name="join"></param>
        /// <param name="isNull"></param>
        /// <returns></returns>
        private static string nullableJoinChar<TValueType>(TValueType[] array, char join, bool isNull)
            where TValueType : struct
        {
            if (array.Length == 1)
            {
                foreach (TValueType? value in array)
                    return value.HasValue ? value.Value.ToString() : (isNull ? TmphAjax.Null : string.Empty);
            }
            var stringArray = new string[array.Length];
            var index = 0;
            foreach (TValueType? value in array) stringArray[index++] = value.HasValue ? value.Value.ToString() : null;
            return isNull ? joinNullString(stringArray, join) : joinString(stringArray, join);
        }

        /// <summary>
        ///     连接字符串集合
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="array"></param>
        /// <param name="join"></param>
        /// <param name="isNull"></param>
        /// <returns></returns>
        private static string nullableSubArrayJoinChar<TValueType>(TmphSubArray<TValueType> subArray, char join, bool isNull)
            where TValueType : struct
        {
            TValueType[] array = subArray.array;
            if (subArray.Count == 1)
            {
                TValueType? value = array[subArray.StartIndex];
                return value.HasValue ? value.Value.ToString() : (isNull ? TmphAjax.Null : string.Empty);
            }
            var stringArray = new string[subArray.Count];
            int index = 0, startIndex = subArray.StartIndex, endIndex = startIndex + subArray.Count;
            do
            {
                TValueType? value = array[startIndex++];
                stringArray[index++] = value.HasValue ? value.Value.ToString() : null;
            } while (startIndex != endIndex);
            return isNull ? joinNullString(stringArray, join) : joinString(stringArray, join);
        }

        /// <summary>
        ///     连接字符串集合
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="array"></param>
        /// <param name="join"></param>
        /// <param name="isNull"></param>
        /// <returns></returns>
        private static string classJoinChar<TValueType>(TValueType[] array, char join, bool isNull)
            where TValueType : class
        {
            if (array.Length == 1)
            {
                foreach (var value in array)
                    return value == null ? (isNull ? TmphAjax.Null : string.Empty) : value.ToString();
            }
            var stringArray = new string[array.Length];
            var index = 0;
            foreach (var value in array) stringArray[index++] = value == null ? null : value.ToString();
            return isNull ? joinNullString(stringArray, join) : joinString(stringArray, join);
        }

        /// <summary>
        ///     连接字符串集合
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="array"></param>
        /// <param name="join"></param>
        /// <param name="isNull"></param>
        /// <returns></returns>
        private static string classSubArrayJoinChar<TValueType>(TmphSubArray<TValueType> subArray, char join, bool isNull)
            where TValueType : class
        {
            var array = subArray.array;
            if (subArray.Count == 1)
            {
                var value = array[subArray.StartIndex];
                return value == null ? (isNull ? TmphAjax.Null : string.Empty) : value.ToString();
            }
            var stringArray = new string[subArray.Count];
            int index = 0, startIndex = subArray.StartIndex, endIndex = startIndex + subArray.Count;
            do
            {
                var value = array[startIndex++];
                stringArray[index++] = value == null ? null : value.ToString();
            } while (startIndex != endIndex);
            return isNull ? joinNullString(stringArray, join) : joinString(stringArray, join);
        }

        /// <summary>
        ///     连接字符串集合
        /// </summary>
        /// <param name="array"></param>
        /// <param name="join"></param>
        /// <param name="isNull"></param>
        /// <returns></returns>
        private static string stringJoinChar(string[] array, char join, bool isNull)
        {
            if (array.Length == 1)
            {
                foreach (var value in array) return value == null ? (isNull ? TmphAjax.Null : string.Empty) : value;
            }
            return isNull ? joinNullString(array, join) : joinString(array, join);
        }

        /// <summary>
        ///     连接字符串集合
        /// </summary>
        /// <param name="array"></param>
        /// <param name="join"></param>
        /// <param name="isNull"></param>
        /// <returns></returns>
        private static unsafe string stringSubArrayJoinChar(TmphSubArray<string> subArray, char join, bool isNull)
        {
            var array = subArray.array;
            if (subArray.Count == 1)
            {
                var value = array[subArray.StartIndex];
                return value == null ? (isNull ? TmphAjax.Null : string.Empty) : value;
            }
            int startIndex = subArray.StartIndex, length = 0, endIndex = startIndex + subArray.Count;
            if (isNull)
            {
                do
                {
                    var nextString = array[startIndex++];
                    length += nextString == null ? 4 : nextString.Length;
                } while (startIndex != endIndex);
                var value = TmphString.FastAllocateString(length + subArray.Count - 1);
                fixed (char* valueFixed = value)
                {
                    var write = valueFixed;
                    startIndex = subArray.StartIndex;
                    do
                    {
                        var nextString = array[startIndex++];
                        if (write != valueFixed) *write++ = join;
                        if (nextString == null)
                        {
                            *(int*)write = 'n' + ('u' << 16);
                            *(int*)(write + 2) = 'l' + ('l' << 16);
                            write += 4;
                        }
                        else
                        {
                            Unsafe.TmphString.Copy(nextString, write);
                            write += nextString.Length;
                        }
                    } while (startIndex != endIndex);
                }
                return value;
            }
            else
            {
                do
                {
                    var nextString = array[startIndex++];
                    if (nextString != null) length += nextString.Length;
                } while (startIndex != endIndex);
                var value = TmphString.FastAllocateString(length + subArray.Count - 1);
                fixed (char* valueFixed = value)
                {
                    var write = valueFixed;
                    startIndex = subArray.StartIndex;
                    do
                    {
                        var nextString = array[startIndex++];
                        if (write != valueFixed) *write++ = join;
                        if (nextString != null)
                        {
                            Unsafe.TmphString.Copy(nextString, write);
                            write += nextString.Length;
                        }
                    } while (startIndex != endIndex);
                }
                return value;
            }
        }

        /// <summary>
        ///     获取数值转换委托调用函数信息
        /// </summary>
        /// <param name="type">数值类型</param>
        /// <returns>数值转换委托调用函数信息</returns>
        public static MethodInfo GetToStringMethod(Type type)
        {
            MethodInfo method;
            return toStringMethods.TryGetValue(type, out method) ? method : null;
        }

        /// <summary>
        ///     动态函数
        /// </summary>
        public struct TmphNumberDynamicMethod
        {
            /// <summary>
            ///     动态函数
            /// </summary>
            private readonly DynamicMethod dynamicMethod;

            /// <summary>
            /// </summary>
            private readonly ILGenerator generator;

            /// <summary>
            /// </summary>
            private readonly Label indexLable;

            /// <summary>
            /// </summary>
            private readonly Label nextLable;

            /// <summary>
            ///     动态函数
            /// </summary>
            /// <param name="type"></param>
            /// <param name="arrayType"></param>
            public TmphNumberDynamicMethod(Type type, Type arrayType)
            {
                dynamicMethod = new DynamicMethod("numberJoinChar", null,
                    new[] { typeof(TmphCharStream), arrayType, typeof(int), typeof(int), typeof(char), typeof(bool) },
                    type, true);
                generator = dynamicMethod.GetILGenerator();
                generator.DeclareLocal(typeof(int));

                indexLable = generator.DefineLabel();
                nextLable = generator.DefineLabel();
                var toString = generator.DefineLabel();

                generator.Emit(OpCodes.Ldarg_2);
                generator.Emit(OpCodes.Stloc_0);
                generator.Emit(OpCodes.Br_S, indexLable);

                generator.MarkLabel(nextLable);
                generator.Emit(OpCodes.Ldloc_0);
                generator.Emit(OpCodes.Ldarg_2);
                generator.Emit(OpCodes.Beq_S, toString);

                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldarg_S, 4);
                generator.Emit(OpCodes.Callvirt, TmphPub.CharStreamWriteCharMethod);

                generator.MarkLabel(toString);
            }

            /// <summary>
            /// </summary>
            /// <param name="method"></param>
            public void JoinChar(MethodInfo method, Type type)
            {
                generator.Emit(OpCodes.Ldarg_1);
                generator.Emit(OpCodes.Ldloc_0);
                if (type == typeof(int) || type == typeof(uint)) generator.Emit(OpCodes.Ldelem_I4);
                else if (type == typeof(byte) || type == typeof(sbyte)) generator.Emit(OpCodes.Ldelem_I1);
                else if (type == typeof(long) || type == typeof(ulong)) generator.Emit(OpCodes.Ldelem_I8);
                else if (type == typeof(short) || type == typeof(ushort)) generator.Emit(OpCodes.Ldelem_I2);
                else generator.Emit(OpCodes.Ldelem, type);
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Call, method);
            }

            /// <summary>
            /// </summary>
            /// <param name="method"></param>
            public void JoinCharNull(MethodInfo method, Type type)
            {
                Label writeNull = generator.DefineLabel(), end = generator.DefineLabel();
                generator.Emit(OpCodes.Ldarg_1);
                generator.Emit(OpCodes.Ldloc_0);
                generator.Emit(OpCodes.Ldelem, type);
                generator.Emit(OpCodes.Call, TmphPub.GetNullableHasValue(type));
                generator.Emit(OpCodes.Brfalse_S, writeNull);

                generator.Emit(OpCodes.Ldarg_1);
                generator.Emit(OpCodes.Ldloc_0);
                generator.Emit(OpCodes.Ldelema, type);
                generator.Emit(OpCodes.Call, TmphPub.GetNullableValue(type));
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Call, method);
                generator.Emit(OpCodes.Br_S, end);

                generator.MarkLabel(writeNull);
                generator.Emit(OpCodes.Ldarg_S, 5);
                generator.Emit(OpCodes.Brfalse_S, end);
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Call, TmphPub.CharStreamWriteNullMethod);
                generator.MarkLabel(end);
            }

            /// <summary>
            ///     创建成员转换委托
            /// </summary>
            /// <returns>成员转换委托</returns>
            public Delegate Create<delegateType>()
            {
                generator.Emit(OpCodes.Ldloc_0);
                generator.Emit(OpCodes.Ldc_I4_1);
                generator.Emit(OpCodes.Add);
                generator.Emit(OpCodes.Stloc_0);

                generator.MarkLabel(indexLable);
                generator.Emit(OpCodes.Ldloc_0);
                generator.Emit(OpCodes.Ldarg_3);
                generator.Emit(OpCodes.Bne_Un_S, nextLable);

                generator.Emit(OpCodes.Ret);
                return dynamicMethod.CreateDelegate(typeof(delegateType));
            }
        }
    }

    /// <summary>
    ///     数字转换成字符串
    /// </summary>
    /// <typeparam name="TValueType"></typeparam>
    internal static class TmphNumberToCharStream<TValueType>
    {
        /// <summary>
        ///     连接字符串集合
        /// </summary>
        internal static readonly Action<TmphCharStream, TValueType[], int, int, char, bool> NumberJoinChar;

        /// <summary>
        ///     连接字符串集合
        /// </summary>
        private static readonly Func<TValueType[], char, bool, string> otherJoinChar;

        /// <summary>
        ///     连接字符串集合
        /// </summary>
        private static readonly Func<TmphSubArray<TValueType>, char, bool, string> subArrayJoinChar;

        static TmphNumberToCharStream()
        {
            var type = typeof(TValueType);
            if (type.IsValueType)
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    var parameterTypes = type.GetGenericArguments();
                    var method = TmphNumberToCharStream.GetToStringMethod(parameterTypes[0]);
                    if (method == null)
                    {
                        otherJoinChar =
                            (Func<TValueType[], char, bool, string>)
                                Delegate.CreateDelegate(typeof(Func<TValueType[], char, bool, string>),
                                    TmphNumberToCharStream.NullableJoinCharMethod.MakeGenericMethod(parameterTypes));
                        subArrayJoinChar =
                            (Func<TmphSubArray<TValueType>, char, bool, string>)
                                Delegate.CreateDelegate(typeof(Func<TmphSubArray<TValueType>, char, bool, string>),
                                    TmphNumberToCharStream.NullableSubArrayJoinCharMethod.MakeGenericMethod(parameterTypes));
                    }
                    else
                    {
                        var dynamicMethod = new TmphNumberToCharStream.TmphNumberDynamicMethod(type, typeof(TValueType[]));
                        dynamicMethod.JoinCharNull(method, type);
                        NumberJoinChar =
                            (Action<TmphCharStream, TValueType[], int, int, char, bool>)
                                dynamicMethod.Create<Action<TmphCharStream, TValueType[], int, int, char, bool>>();
                    }
                }
                else
                {
                    var method = TmphNumberToCharStream.GetToStringMethod(type);
                    if (method == null)
                    {
                        otherJoinChar =
                            (Func<TValueType[], char, bool, string>)
                                Delegate.CreateDelegate(typeof(Func<TValueType[], char, bool, string>),
                                    TmphNumberToCharStream.StructJoinCharMethod.MakeGenericMethod(type));
                        subArrayJoinChar =
                            (Func<TmphSubArray<TValueType>, char, bool, string>)
                                Delegate.CreateDelegate(typeof(Func<TmphSubArray<TValueType>, char, bool, string>),
                                    TmphNumberToCharStream.StructSubArrayJoinCharMethod.MakeGenericMethod(type));
                    }
                    else
                    {
                        var dynamicMethod = new TmphNumberToCharStream.TmphNumberDynamicMethod(type, typeof(TValueType[]));
                        dynamicMethod.JoinChar(method, type);
                        NumberJoinChar =
                            (Action<TmphCharStream, TValueType[], int, int, char, bool>)
                                dynamicMethod.Create<Action<TmphCharStream, TValueType[], int, int, char, bool>>();
                    }
                }
            }
            else
            {
                MethodInfo method, subArrayMethod;
                if (type == typeof(string))
                {
                    method = TmphNumberToCharStream.StringJoinCharMethod;
                    subArrayMethod = TmphNumberToCharStream.StringSubArrayJoinCharMethod;
                }
                else
                {
                    method = TmphNumberToCharStream.ClassJoinCharMethod.MakeGenericMethod(type);
                    subArrayMethod = TmphNumberToCharStream.ClassSubArrayJoinCharMethod.MakeGenericMethod(type);
                }
                otherJoinChar =
                    (Func<TValueType[], char, bool, string>)
                        Delegate.CreateDelegate(typeof(Func<TValueType[], char, bool, string>), method);
                subArrayJoinChar =
                    (Func<TmphSubArray<TValueType>, char, bool, string>)
                        Delegate.CreateDelegate(typeof(Func<TmphSubArray<TValueType>, char, bool, string>), subArrayMethod);
            }
        }

        /// <summary>
        ///     连接字符串集合
        /// </summary>
        /// <param name="array"></param>
        /// <param name="join"></param>
        /// <param name="isNull"></param>
        /// <returns></returns>
        public static unsafe string JoinString(TValueType[] array, char join, bool isNull = false)
        {
            if (array.length() == 0) return isNull ? TmphAjax.Null : string.Empty;
            if (NumberJoinChar == null) return otherJoinChar(array, join, isNull);
            var TmphBuffer = TmphUnmanagedPool.StreamBuffers.Get();
            try
            {
                using (var stream = new TmphCharStream(TmphBuffer.Char, TmphUnmanagedPool.StreamBuffers.Size >> 1))
                {
                    NumberJoinChar(stream, array, 0, array.Length, join, isNull);
                    return new string(stream.Char, 0, stream.Length);
                }
            }
            finally
            {
                TmphUnmanagedPool.StreamBuffers.Push(ref TmphBuffer);
            }
        }

        /// <summary>
        ///     连接字符串集合
        /// </summary>
        /// <param name="array"></param>
        /// <param name="join"></param>
        /// <param name="isNull"></param>
        /// <returns></returns>
        public static unsafe string JoinString(TmphSubArray<TValueType> array, char join, bool isNull = false)
        {
            if (array.Count == 0) return isNull ? TmphAjax.Null : string.Empty;
            if (NumberJoinChar == null) return subArrayJoinChar(array, join, isNull);
            var TmphBuffer = TmphUnmanagedPool.StreamBuffers.Get();
            try
            {
                using (var stream = new TmphCharStream(TmphBuffer.Char, TmphUnmanagedPool.StreamBuffers.Size >> 1))
                {
                    NumberJoinChar(stream, array.array, array.StartIndex, array.Count, join, isNull);
                    return new string(stream.Char, 0, stream.Length);
                }
            }
            finally
            {
                TmphUnmanagedPool.StreamBuffers.Push(ref TmphBuffer);
            }
        }
    }
}