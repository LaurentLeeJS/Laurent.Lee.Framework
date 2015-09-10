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
using Laurent.Lee.CLB.Reflection;
using Laurent.Lee.CLB.Threading;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Laurent.Lee.CLB.Emit
{
    /// <summary>
    ///     二进制数据序列化(内存数据库专用)
    /// </summary>
    public sealed unsafe class TmphIndexSerializer : TmphBinarySerializer
    {
        /// <summary>
        ///     对象序列化函数信息
        /// </summary>
        private static readonly MethodInfo nullableSerializeMethod =
            typeof(TmphIndexSerializer).GetMethod("nullableSerialize", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     对象序列化函数信息
        /// </summary>
        private static readonly MethodInfo structSerializeMethod =
            typeof(TmphIndexSerializer).GetMethod("structSerialize", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     对象序列化函数信息
        /// </summary>
        private static readonly MethodInfo nullableMemberSerializeMethod =
            typeof(TmphIndexSerializer).GetMethod("nullableMemberSerialize",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     对象序列化函数信息
        /// </summary>
        private static readonly MethodInfo classSerializeMethod = typeof(TmphIndexSerializer).GetMethod(
            "classSerialize", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     对象序列化函数信息
        /// </summary>
        private static readonly MethodInfo memberMapClassSerializeMethod =
            typeof(TmphIndexSerializer).GetMethod("memberMapClassSerialize",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     对象序列化函数信息
        /// </summary>
        private static readonly MethodInfo memberMapNullableSerializeMethod =
            typeof(TmphIndexSerializer).GetMethod("memberMapNullableSerialize",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     对象序列化函数信息
        /// </summary>
        private static readonly MethodInfo keyValuePairSerializeMethod =
            typeof(TmphIndexSerializer).GetMethod("keyValuePairSerialize", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     对象序列化函数信息
        /// </summary>
        private static readonly MethodInfo memberKeyValuePairSerializeMethod =
            typeof(TmphIndexSerializer).GetMethod("memberKeyValuePairSerialize",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     字典序列化函数信息
        /// </summary>
        private static readonly MethodInfo dictionarySerializeMethod =
            typeof(TmphIndexSerializer).GetMethod("dictionarySerializeType",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     字典序列化函数信息
        /// </summary>
        private static readonly MethodInfo dictionaryMemberMethod =
            typeof(TmphIndexSerializer).GetMethod("dictionaryMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     字典序列化函数信息
        /// </summary>
        private static readonly MethodInfo memberMapDictionaryMethod =
            typeof(TmphIndexSerializer).GetMethod("memberMapDictionary", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     对象序列化函数信息
        /// </summary>
        private static readonly MethodInfo subArraySerializeMethod =
            typeof(TmphIndexSerializer).GetMethod("subArraySerialize", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     对象序列化函数信息
        /// </summary>
        private static readonly MethodInfo baseSerializeMethod = typeof(TmphIndexSerializer).GetMethod("baseSerialize",
            BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     数组转换函数信息
        /// </summary>
        private static readonly MethodInfo structArrayTypeMethod =
            typeof(TmphIndexSerializer).GetMethod("structArrayType", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组转换函数信息
        /// </summary>
        private static readonly MethodInfo structArrayMemberMethod =
            typeof(TmphIndexSerializer).GetMethod("structArrayMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组转换函数信息
        /// </summary>
        private static readonly MethodInfo structArrayMemberMapMethod =
            typeof(TmphIndexSerializer).GetMethod("structArrayMemberMap", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组转换函数信息
        /// </summary>
        private static readonly MethodInfo nullableArrayTypeMethod =
            typeof(TmphIndexSerializer).GetMethod("nullableArrayType", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组转换函数信息
        /// </summary>
        private static readonly MethodInfo nullableArrayMemberMethod =
            typeof(TmphIndexSerializer).GetMethod("nullableArrayMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组转换函数信息
        /// </summary>
        private static readonly MethodInfo nullableArrayMemberMapMethod =
            typeof(TmphIndexSerializer).GetMethod("nullableArrayMemberMap",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组转换函数信息
        /// </summary>
        private static readonly MethodInfo arrayTypeMethod = typeof(TmphIndexSerializer).GetMethod("arrayType",
            BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组转换函数信息
        /// </summary>
        private static readonly MethodInfo arrayMemberMethod = typeof(TmphIndexSerializer).GetMethod("arrayMember",
            BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组转换函数信息
        /// </summary>
        private static readonly MethodInfo arrayMemberMapMethod = typeof(TmphIndexSerializer).GetMethod(
            "arrayMemberMap", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     集合序列化函数信息
        /// </summary>
        private static readonly MethodInfo collectionMethod = typeof(TmphIndexSerializer).GetMethod("collection",
            BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     集合序列化函数信息
        /// </summary>
        private static readonly MethodInfo dictionaryMethod = typeof(TmphIndexSerializer).GetMethod("dictionary",
            BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     枚举转换函数信息
        /// </summary>
        private static readonly MethodInfo enumByteMemberMethod = typeof(TmphIndexSerializer).GetMethod(
            "enumByteMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     枚举转换函数信息
        /// </summary>
        private static readonly MethodInfo enumSByteMemberMethod =
            typeof(TmphIndexSerializer).GetMethod("enumSByteMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     枚举转换函数信息
        /// </summary>
        private static readonly MethodInfo enumShortMemberMethod =
            typeof(TmphIndexSerializer).GetMethod("enumShortMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     枚举转换函数信息
        /// </summary>
        private static readonly MethodInfo enumUShortMemberMethod =
            typeof(TmphIndexSerializer).GetMethod("enumUShortMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     枚举转换函数信息
        /// </summary>
        private static readonly MethodInfo enumIntMemberMethod = typeof(TmphIndexSerializer).GetMethod("enumIntMember",
            BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     枚举转换函数信息
        /// </summary>
        private static readonly MethodInfo enumUIntMemberMethod = typeof(TmphIndexSerializer).GetMethod(
            "enumUIntMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     枚举转换函数信息
        /// </summary>
        private static readonly MethodInfo enumLongMemberMethod = typeof(TmphIndexSerializer).GetMethod(
            "enumLongMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     枚举转换函数信息
        /// </summary>
        private static readonly MethodInfo enumULongMemberMethod =
            typeof(TmphIndexSerializer).GetMethod("enumULongMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     枚举转换函数信息
        /// </summary>
        private static readonly MethodInfo enumByteMemberMapMethod =
            typeof(TmphIndexSerializer).GetMethod("enumByteMemberMap", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     枚举转换函数信息
        /// </summary>
        private static readonly MethodInfo enumSByteMemberMapMethod =
            typeof(TmphIndexSerializer).GetMethod("enumSByteMemberMap", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     枚举转换函数信息
        /// </summary>
        private static readonly MethodInfo enumShortMemberMapMethod =
            typeof(TmphIndexSerializer).GetMethod("enumShortMemberMap", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     枚举转换函数信息
        /// </summary>
        private static readonly MethodInfo enumUShortMemberMapMethod =
            typeof(TmphIndexSerializer).GetMethod("enumUShortMemberMap", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组转换函数信息
        /// </summary>
        private static readonly MethodInfo enumByteArrayTypeMethod =
            typeof(TmphIndexSerializer).GetMethod("enumByteArrayType", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组转换函数信息
        /// </summary>
        private static readonly MethodInfo enumByteArrayMemberMethod =
            typeof(TmphIndexSerializer).GetMethod("enumByteArrayMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组转换函数信息
        /// </summary>
        private static readonly MethodInfo enumByteArrayMemberMapMethod =
            typeof(TmphIndexSerializer).GetMethod("enumByteArrayMemberMap",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组转换函数信息
        /// </summary>
        private static readonly MethodInfo enumSByteArrayTypeMethod =
            typeof(TmphIndexSerializer).GetMethod("enumSByteArrayType", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组转换函数信息
        /// </summary>
        private static readonly MethodInfo enumSByteArrayMemberMethod =
            typeof(TmphIndexSerializer).GetMethod("enumSByteArrayMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组转换函数信息
        /// </summary>
        private static readonly MethodInfo enumSByteArrayMemberMapMethod =
            typeof(TmphIndexSerializer).GetMethod("enumSByteArrayMemberMap",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组转换函数信息
        /// </summary>
        private static readonly MethodInfo enumShortArrayTypeMethod =
            typeof(TmphIndexSerializer).GetMethod("enumShortArrayType", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组转换函数信息
        /// </summary>
        private static readonly MethodInfo enumShortArrayMemberMethod =
            typeof(TmphIndexSerializer).GetMethod("enumShortArrayMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组转换函数信息
        /// </summary>
        private static readonly MethodInfo enumShortArrayMemberMapMethod =
            typeof(TmphIndexSerializer).GetMethod("enumShortArrayMemberMap",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组转换函数信息
        /// </summary>
        private static readonly MethodInfo enumUShortArrayTypeMethod =
            typeof(TmphIndexSerializer).GetMethod("enumUShortArrayType", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组转换函数信息
        /// </summary>
        private static readonly MethodInfo enumUShortArrayMemberMethod =
            typeof(TmphIndexSerializer).GetMethod("enumUShortArrayMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组转换函数信息
        /// </summary>
        private static readonly MethodInfo enumUShortArrayMemberMapMethod =
            typeof(TmphIndexSerializer).GetMethod("enumUShortArrayMemberMap",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组转换函数信息
        /// </summary>
        private static readonly MethodInfo enumIntArrayTypeMethod =
            typeof(TmphIndexSerializer).GetMethod("enumIntArrayType", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组转换函数信息
        /// </summary>
        private static readonly MethodInfo enumIntArrayMemberMethod =
            typeof(TmphIndexSerializer).GetMethod("enumIntArrayMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组转换函数信息
        /// </summary>
        private static readonly MethodInfo enumIntArrayMemberMapMethod =
            typeof(TmphIndexSerializer).GetMethod("enumIntArrayMemberMap", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组转换函数信息
        /// </summary>
        private static readonly MethodInfo enumUIntArrayTypeMethod =
            typeof(TmphIndexSerializer).GetMethod("enumUIntArrayType", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组转换函数信息
        /// </summary>
        private static readonly MethodInfo enumUIntArrayMemberMethod =
            typeof(TmphIndexSerializer).GetMethod("enumUIntArrayMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组转换函数信息
        /// </summary>
        private static readonly MethodInfo enumUIntArrayMemberMapMethod =
            typeof(TmphIndexSerializer).GetMethod("enumUIntArrayMemberMap",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组转换函数信息
        /// </summary>
        private static readonly MethodInfo enumLongArrayTypeMethod =
            typeof(TmphIndexSerializer).GetMethod("enumLongArrayType", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组转换函数信息
        /// </summary>
        private static readonly MethodInfo enumLongArrayMemberMethod =
            typeof(TmphIndexSerializer).GetMethod("enumLongArrayMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组转换函数信息
        /// </summary>
        private static readonly MethodInfo enumLongArrayMemberMapMethod =
            typeof(TmphIndexSerializer).GetMethod("enumLongArrayMemberMap",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组转换函数信息
        /// </summary>
        private static readonly MethodInfo enumULongArrayTypeMethod =
            typeof(TmphIndexSerializer).GetMethod("enumULongArrayType", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组转换函数信息
        /// </summary>
        private static readonly MethodInfo enumULongArrayMemberMethod =
            typeof(TmphIndexSerializer).GetMethod("enumULongArrayMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组转换函数信息
        /// </summary>
        private static readonly MethodInfo enumULongArrayMemberMapMethod =
            typeof(TmphIndexSerializer).GetMethod("enumULongArrayMemberMap",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     公共默认配置参数
        /// </summary>
        private static readonly TmphConfig defaultConfig = new TmphConfig();

        /// <summary>
        ///     基本类型转换函数
        /// </summary>
        private static readonly Dictionary<Type, MethodInfo> serializeMethods;

        /// <summary>
        ///     基本类型转换函数
        /// </summary>
        private static readonly Dictionary<Type, MethodInfo> memberSerializeMethods;

        /// <summary>
        ///     基本类型转换函数
        /// </summary>
        private static readonly Dictionary<Type, MethodInfo> memberMapSerializeMethods;

        static TmphIndexSerializer()
        {
            serializeMethods = TmphDictionary.CreateOnly<Type, MethodInfo>();
            memberSerializeMethods = TmphDictionary.CreateOnly<Type, MethodInfo>();
            memberMapSerializeMethods = TmphDictionary.CreateOnly<Type, MethodInfo>();
            foreach (var method in typeof(TmphIndexSerializer).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                )
            {
                Type TParameterType = null;
                if (method.CustomAttribute<TmphSerializeMethod>() != null)
                {
                    serializeMethods.Add(TParameterType = method.GetParameters()[0].ParameterType, method);
                }
                if (method.CustomAttribute<TmphMemberSerializeMethod>() != null)
                {
                    if (TParameterType == null) TParameterType = method.GetParameters()[0].ParameterType;
                    memberSerializeMethods.Add(TParameterType, method);
                }
                if (method.CustomAttribute<TmphMemberMapSerializeMethod>() != null)
                {
                    memberMapSerializeMethods.Add(TParameterType ?? method.GetParameters()[0].ParameterType, method);
                }
            }
        }

        /// <summary>
        ///     对象序列化
        /// </summary>
        /// <typeparam name="TValueType">目标数据类型</typeparam>
        /// <param name="value">数据对象</param>
        /// <param name="TmphConfig">配置参数</param>
        /// <returns>序列化数据</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private byte[] serialize<TValueType>(TValueType value, TmphConfig TmphConfig)
        {
            binarySerializerConfig = TmphConfig;
            var TmphBuffer = TmphUnmanagedPool.StreamBuffers.Get();
            try
            {
                Stream.Reset(TmphBuffer.Byte, TmphUnmanagedPool.StreamBuffers.Size);
                using (Stream)
                {
                    serialize(value);
                    return Stream.GetArray();
                }
            }
            finally
            {
                TmphUnmanagedPool.StreamBuffers.Push(ref TmphBuffer);
            }
        }

        /// <summary>
        ///     对象序列化
        /// </summary>
        /// <typeparam name="TValueType">目标数据类型</typeparam>
        /// <param name="value">数据对象</param>
        /// <param name="stream">序列化输出缓冲区</param>
        /// <param name="TmphConfig">配置参数</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize<TValueType>(TValueType value, TmphUnmanagedStream stream, TmphConfig TmphConfig)
        {
            binarySerializerConfig = TmphConfig;
            Stream.From(stream);
            try
            {
                serialize(value);
            }
            finally
            {
                stream.From(Stream);
            }
        }

        /// <summary>
        ///     对象序列化
        /// </summary>
        /// <typeparam name="TValueType">目标数据类型</typeparam>
        /// <param name="value">数据对象</param>
        /// <param name="TmphConfig">配置参数</param>
        /// <returns>序列化数据</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize<TValueType>(TValueType value)
        {
            memberMap = binarySerializerConfig.MemberMap;
            streamStartIndex = Stream.OffsetLength;
            Stream.Write(binarySerializerConfig.HeaderValue);
            TmphTypeSerializer<TValueType>.Serialize(this, value);
            Stream.Write(Stream.OffsetLength - streamStartIndex);
        }

        /// <summary>
        ///     释放资源
        /// </summary>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private new void free()
        {
            base.free();
            TmphTypePool<TmphIndexSerializer>.Push(this);
        }

        /// <summary>
        ///     逻辑值序列化
        /// </summary>
        /// <param name="value">逻辑值</param>
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapSerialize(bool value)
        {
            Stream.Unsafer.Write(value ? 1 : 0);
        }

        /// <summary>
        ///     逻辑值序列化
        /// </summary>
        /// <param name="value">逻辑值</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(bool[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else Serialize(Stream, value);
        }

        /// <summary>
        ///     逻辑值序列化
        /// </summary>
        /// <param name="value">逻辑值</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serializeBoolArray(bool[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else
            {
                var length = Stream.Length;
                Stream.PrepLength(sizeof(int) * 2);
                Stream.Unsafer.AddLength(sizeof(int));
                Serialize(Stream, value);
                *(int*)(Stream.Data + length) = Stream.Length - length;
                Stream.PrepLength();
            }
        }

        /// <summary>
        ///     逻辑值序列化
        /// </summary>
        /// <param name="value">逻辑值</param>
        [TmphMemberSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(bool[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serializeBoolArray(value);
        }

        /// <summary>
        ///     逻辑值序列化
        /// </summary>
        /// <param name="value">逻辑值</param>
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapSerialize(bool[] value)
        {
            if (value == null) *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
            else serializeBoolArray(value);
        }

        /// <summary>
        ///     逻辑值序列化
        /// </summary>
        /// <param name="value">逻辑值</param>
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapSerialize(bool? value)
        {
            if (value.HasValue) Stream.Unsafer.Write((bool)value ? 1 : 0);
            else *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
        }

        /// <summary>
        ///     逻辑值序列化
        /// </summary>
        /// <param name="value">逻辑值</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(bool?[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else Serialize(Stream, value);
        }

        /// <summary>
        ///     逻辑值序列化
        /// </summary>
        /// <param name="value">逻辑值</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serializeBoolArray(bool?[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else
            {
                var length = Stream.Length;
                Stream.PrepLength(sizeof(int) * 2);
                Stream.Unsafer.AddLength(sizeof(int));
                Serialize(Stream, value);
                *(int*)(Stream.Data + length) = Stream.Length - length;
                Stream.PrepLength();
            }
        }

        /// <summary>
        ///     逻辑值序列化
        /// </summary>
        /// <param name="value">逻辑值</param>
        [TmphMemberSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(bool?[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serializeBoolArray(value);
        }

        /// <summary>
        ///     逻辑值序列化
        /// </summary>
        /// <param name="value">逻辑值</param>
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapSerialize(bool?[] value)
        {
            if (value == null) *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
            else serializeBoolArray(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapSerialize(byte value)
        {
            Stream.Unsafer.Write((uint)value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(byte[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else Serialize(Stream, value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serializeByteArray(byte[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else
            {
                var length = Stream.Length;
                Stream.PrepLength(sizeof(int) * 2);
                Stream.Unsafer.AddLength(sizeof(int));
                Serialize(Stream, value);
                *(int*)(Stream.Data + length) = Stream.Length - length;
                Stream.PrepLength();
            }
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(byte[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serializeByteArray(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapSerialize(byte[] value)
        {
            if (value == null) *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
            else serializeByteArray(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberSerializeMethod]
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(TmphSubArray<byte> value)
        {
            if (value.Count == 0) Stream.Write(0);
            else
            {
                var length = Stream.Length;
                Stream.PrepLength(sizeof(int) * 2);
                Stream.Unsafer.AddLength(sizeof(int));
                Serialize(Stream, value);
                *(int*)(Stream.Data + length) = Stream.Length - length;
                Stream.PrepLength();
            }
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapSerialize(byte? value)
        {
            if (value.HasValue) Stream.Unsafer.Write((uint)(byte)value);
            else *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(byte?[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else Serialize(Stream, value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serializeByteArray(byte?[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else
            {
                var length = Stream.Length;
                Stream.PrepLength(sizeof(int) * 2);
                Stream.Unsafer.AddLength(sizeof(int));
                Serialize(Stream, value);
                *(int*)(Stream.Data + length) = Stream.Length - length;
                Stream.PrepLength();
            }
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(byte?[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serializeByteArray(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapSerialize(byte?[] value)
        {
            if (value == null) *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
            else serializeByteArray(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapSerialize(sbyte value)
        {
            Stream.Unsafer.Write((int)value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(sbyte[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else Serialize(Stream, value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serializeSByteArray(sbyte[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else
            {
                var length = Stream.Length;
                Stream.PrepLength(sizeof(int) * 2);
                Stream.Unsafer.AddLength(sizeof(int));
                Serialize(Stream, value);
                *(int*)(Stream.Data + length) = Stream.Length - length;
                Stream.PrepLength();
            }
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(sbyte[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serializeSByteArray(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapSerialize(sbyte[] value)
        {
            if (value == null) *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
            else serializeSByteArray(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapSerialize(sbyte? value)
        {
            if (value.HasValue) Stream.Unsafer.Write((int)(sbyte)value);
            else *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(sbyte?[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else Serialize(Stream, value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serializeSByteArray(sbyte?[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else
            {
                var length = Stream.Length;
                Stream.PrepLength(sizeof(int) * 2);
                Stream.Unsafer.AddLength(sizeof(int));
                Serialize(Stream, value);
                *(int*)(Stream.Data + length) = Stream.Length - length;
                Stream.PrepLength();
            }
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(sbyte?[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serializeSByteArray(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapSerialize(sbyte?[] value)
        {
            if (value == null) *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
            else serializeSByteArray(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapSerialize(short value)
        {
            Stream.Unsafer.Write((int)value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(short[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else Serialize(Stream, value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serializeShortArray(short[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else
            {
                var length = Stream.Length;
                Stream.PrepLength(sizeof(int) * 2);
                Stream.Unsafer.AddLength(sizeof(int));
                Serialize(Stream, value);
                *(int*)(Stream.Data + length) = Stream.Length - length;
                Stream.PrepLength();
            }
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(short[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serializeShortArray(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapSerialize(short[] value)
        {
            if (value == null) *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
            else serializeShortArray(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapSerialize(short? value)
        {
            if (value.HasValue) Stream.Unsafer.Write((int)(short)value);
            else *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(short?[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else Serialize(Stream, value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serializeShortArray(short?[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else
            {
                var length = Stream.Length;
                Stream.PrepLength(sizeof(int) * 2);
                Stream.Unsafer.AddLength(sizeof(int));
                Serialize(Stream, value);
                *(int*)(Stream.Data + length) = Stream.Length - length;
                Stream.PrepLength();
            }
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(short?[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serializeShortArray(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapSerialize(short?[] value)
        {
            if (value == null) *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
            else serializeShortArray(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapSerialize(ushort value)
        {
            Stream.Unsafer.Write((uint)value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(ushort[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else Serialize(Stream, value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serializeUShortArray(ushort[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else
            {
                var length = Stream.Length;
                Stream.PrepLength(sizeof(int) * 2);
                Stream.Unsafer.AddLength(sizeof(int));
                Serialize(Stream, value);
                *(int*)(Stream.Data + length) = Stream.Length - length;
                Stream.PrepLength();
            }
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(ushort[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serializeUShortArray(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapSerialize(ushort[] value)
        {
            if (value == null) *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
            else serializeUShortArray(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapSerialize(ushort? value)
        {
            if (value.HasValue) Stream.Unsafer.Write((uint)(ushort)value);
            else *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(ushort?[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else Serialize(Stream, value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serializeUShortArray(ushort?[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else
            {
                var length = Stream.Length;
                Stream.PrepLength(sizeof(int) * 2);
                Stream.Unsafer.AddLength(sizeof(int));
                Serialize(Stream, value);
                *(int*)(Stream.Data + length) = Stream.Length - length;
                Stream.PrepLength();
            }
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(ushort?[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serializeUShortArray(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapSerialize(ushort?[] value)
        {
            if (value == null) *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
            else serializeUShortArray(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(int[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else Serialize(Stream, value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serializeIntArray(int[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else
            {
                var length = Stream.Length;
                Stream.PrepLength(sizeof(int) * 2);
                Stream.Unsafer.AddLength(sizeof(int));
                Serialize(Stream, value);
                *(int*)(Stream.Data + length) = Stream.Length - length;
                Stream.PrepLength();
            }
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(int[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serializeIntArray(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapSerialize(int[] value)
        {
            if (value == null) *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
            else serializeIntArray(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapSerialize(int? value)
        {
            if (value.HasValue) Stream.Unsafer.Write((int)value);
            else *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(int?[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else Serialize(Stream, value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serializeIntArray(int?[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else
            {
                var length = Stream.Length;
                Stream.PrepLength(sizeof(int) * 2);
                Stream.Unsafer.AddLength(sizeof(int));
                Serialize(Stream, value);
                *(int*)(Stream.Data + length) = Stream.Length - length;
                Stream.PrepLength();
            }
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(int?[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serializeIntArray(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapSerialize(int?[] value)
        {
            if (value == null) *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
            else serializeIntArray(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(uint[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else Serialize(Stream, value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serializeUIntArray(uint[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else
            {
                var length = Stream.Length;
                Stream.PrepLength(sizeof(int) * 2);
                Stream.Unsafer.AddLength(sizeof(int));
                Serialize(Stream, value);
                *(int*)(Stream.Data + length) = Stream.Length - length;
                Stream.PrepLength();
            }
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(uint[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serializeUIntArray(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapSerialize(uint[] value)
        {
            if (value == null) *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
            else serializeUIntArray(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapSerialize(uint? value)
        {
            if (value.HasValue) Stream.Unsafer.Write((uint)value);
            else *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(uint?[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else Serialize(Stream, value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serializeUIntArray(uint?[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else
            {
                var length = Stream.Length;
                Stream.PrepLength(sizeof(int) * 2);
                Stream.Unsafer.AddLength(sizeof(int));
                Serialize(Stream, value);
                *(int*)(Stream.Data + length) = Stream.Length - length;
                Stream.PrepLength();
            }
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(uint?[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serializeUIntArray(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapSerialize(uint?[] value)
        {
            if (value == null) *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
            else serializeUIntArray(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        /// x
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(long[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else Serialize(Stream, value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serializeLongArray(long[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else
            {
                var length = Stream.Length;
                Stream.PrepLength(sizeof(int) * 2);
                Stream.Unsafer.AddLength(sizeof(int));
                Serialize(Stream, value);
                *(int*)(Stream.Data + length) = Stream.Length - length;
                Stream.PrepLength();
            }
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(long[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serializeLongArray(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapSerialize(long[] value)
        {
            if (value == null) *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
            else serializeLongArray(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapSerialize(long? value)
        {
            if (value.HasValue) Stream.Unsafer.Write((long)value);
            else *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(long?[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else Serialize(Stream, value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serializeLongArray(long?[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else
            {
                var length = Stream.Length;
                Stream.PrepLength(sizeof(int) * 2);
                Stream.Unsafer.AddLength(sizeof(int));
                Serialize(Stream, value);
                *(int*)(Stream.Data + length) = Stream.Length - length;
                Stream.PrepLength();
            }
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(long?[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serializeLongArray(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapSerialize(long?[] value)
        {
            if (value == null) *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
            else serializeLongArray(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(ulong[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else Serialize(Stream, value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serializeULongArray(ulong[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else
            {
                var length = Stream.Length;
                Stream.PrepLength(sizeof(int) * 2);
                Stream.Unsafer.AddLength(sizeof(int));
                Serialize(Stream, value);
                *(int*)(Stream.Data + length) = Stream.Length - length;
                Stream.PrepLength();
            }
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(ulong[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serializeULongArray(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapSerialize(ulong[] value)
        {
            if (value == null) *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
            else serializeULongArray(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapSerialize(ulong? value)
        {
            if (value.HasValue) Stream.Unsafer.Write((ulong)value);
            else *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(ulong?[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else Serialize(Stream, value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serializeULongArray(ulong?[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else
            {
                var length = Stream.Length;
                Stream.PrepLength(sizeof(int) * 2);
                Stream.Unsafer.AddLength(sizeof(int));
                Serialize(Stream, value);
                *(int*)(Stream.Data + length) = Stream.Length - length;
                Stream.PrepLength();
            }
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(ulong?[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serializeULongArray(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapSerialize(ulong?[] value)
        {
            if (value == null) *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
            else serializeULongArray(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(float[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else Serialize(Stream, value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serializeFloatArray(float[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else
            {
                var length = Stream.Length;
                Stream.PrepLength(sizeof(int) * 2);
                Stream.Unsafer.AddLength(sizeof(int));
                Serialize(Stream, value);
                *(int*)(Stream.Data + length) = Stream.Length - length;
                Stream.PrepLength();
            }
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(float[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serializeFloatArray(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapSerialize(float[] value)
        {
            if (value == null) *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
            else serializeFloatArray(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapSerialize(float? value)
        {
            if (value.HasValue) Stream.Unsafer.Write((float)value);
            else *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(float?[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else Serialize(Stream, value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serializeFloatArray(float?[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else
            {
                var length = Stream.Length;
                Stream.PrepLength(sizeof(int) * 2);
                Stream.Unsafer.AddLength(sizeof(int));
                Serialize(Stream, value);
                *(int*)(Stream.Data + length) = Stream.Length - length;
                Stream.PrepLength();
            }
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(float?[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serializeFloatArray(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapSerialize(float?[] value)
        {
            if (value == null) *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
            else serializeFloatArray(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(double[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else Serialize(Stream, value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serializeDoubleArray(double[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else
            {
                var length = Stream.Length;
                Stream.PrepLength(sizeof(int) * 2);
                Stream.Unsafer.AddLength(sizeof(int));
                Serialize(Stream, value);
                *(int*)(Stream.Data + length) = Stream.Length - length;
                Stream.PrepLength();
            }
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(double[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serializeDoubleArray(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapSerialize(double[] value)
        {
            if (value == null) *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
            else serializeDoubleArray(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapSerialize(double? value)
        {
            if (value.HasValue) Stream.Unsafer.Write((double)value);
            else *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(double?[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else Serialize(Stream, value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serializeDoubleArray(double?[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else
            {
                var length = Stream.Length;
                Stream.PrepLength(sizeof(int) * 2);
                Stream.Unsafer.AddLength(sizeof(int));
                Serialize(Stream, value);
                *(int*)(Stream.Data + length) = Stream.Length - length;
                Stream.PrepLength();
            }
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(double?[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serializeDoubleArray(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapSerialize(double?[] value)
        {
            if (value == null) *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
            else serializeDoubleArray(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(decimal[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else Serialize(Stream, value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serializeDecimalArray(decimal[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else
            {
                var length = Stream.Length;
                Stream.PrepLength(sizeof(int) * 2);
                Stream.Unsafer.AddLength(sizeof(int));
                Serialize(Stream, value);
                *(int*)(Stream.Data + length) = Stream.Length - length;
                Stream.PrepLength();
            }
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(decimal[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serializeDecimalArray(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapSerialize(decimal[] value)
        {
            if (value == null) *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
            else serializeDecimalArray(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapSerialize(decimal? value)
        {
            if (value.HasValue) Stream.Unsafer.Write((decimal)value);
            else *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(decimal?[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else Serialize(Stream, value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serializeDecimalArray(decimal?[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else
            {
                var length = Stream.Length;
                Stream.PrepLength(sizeof(int) * 2);
                Stream.Unsafer.AddLength(sizeof(int));
                Serialize(Stream, value);
                *(int*)(Stream.Data + length) = Stream.Length - length;
                Stream.PrepLength();
            }
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(decimal?[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serializeDecimalArray(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapSerialize(decimal?[] value)
        {
            if (value == null) *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
            else serializeDecimalArray(value);
        }

        /// <summary>
        ///     字符序列化
        /// </summary>
        /// <param name="value">字符</param>
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapSerialize(char value)
        {
            Stream.Unsafer.Write((uint)value);
        }

        /// <summary>
        ///     字符序列化
        /// </summary>
        /// <param name="value">字符</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(char[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else Serialize(Stream, value);
        }

        /// <summary>
        ///     字符序列化
        /// </summary>
        /// <param name="value">字符</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serializeCharArray(char[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else
            {
                var length = Stream.Length;
                Stream.PrepLength(sizeof(int) * 2);
                Stream.Unsafer.AddLength(sizeof(int));
                Serialize(Stream, value);
                *(int*)(Stream.Data + length) = Stream.Length - length;
                Stream.PrepLength();
            }
        }

        /// <summary>
        ///     字符序列化
        /// </summary>
        /// <param name="value">字符</param>
        [TmphMemberSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(char[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serializeCharArray(value);
        }

        /// <summary>
        ///     字符序列化
        /// </summary>
        /// <param name="value">字符</param>
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapSerialize(char[] value)
        {
            if (value == null) *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
            else serializeCharArray(value);
        }

        /// <summary>
        ///     字符序列化
        /// </summary>
        /// <param name="value">字符</param>
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapSerialize(char? value)
        {
            if (value.HasValue) Stream.Unsafer.Write((uint)(char)value);
            else *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
        }

        /// <summary>
        ///     字符序列化
        /// </summary>
        /// <param name="value">字符</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(char?[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else Serialize(Stream, value);
        }

        /// <summary>
        ///     字符序列化
        /// </summary>
        /// <param name="value">字符</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serializeCharArray(char?[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else
            {
                var length = Stream.Length;
                Stream.PrepLength(sizeof(int) * 2);
                Stream.Unsafer.AddLength(sizeof(int));
                Serialize(Stream, value);
                *(int*)(Stream.Data + length) = Stream.Length - length;
                Stream.PrepLength();
            }
        }

        /// <summary>
        ///     字符序列化
        /// </summary>
        /// <param name="value">字符</param>
        [TmphMemberSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(char?[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serializeCharArray(value);
        }

        /// <summary>
        ///     字符序列化
        /// </summary>
        /// <param name="value">字符</param>
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapSerialize(char?[] value)
        {
            if (value == null) *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
            else serializeCharArray(value);
        }

        /// <summary>
        ///     时间序列化
        /// </summary>
        /// <param name="value">时间</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(DateTime[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else Serialize(Stream, value);
        }

        /// <summary>
        ///     时间序列化
        /// </summary>
        /// <param name="value">时间</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serializeDateTimeArray(DateTime[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else
            {
                var length = Stream.Length;
                Stream.PrepLength(sizeof(int) * 2);
                Stream.Unsafer.AddLength(sizeof(int));
                Serialize(Stream, value);
                *(int*)(Stream.Data + length) = Stream.Length - length;
                Stream.PrepLength();
            }
        }

        /// <summary>
        ///     时间序列化
        /// </summary>
        /// <param name="value">时间</param>
        [TmphMemberSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(DateTime[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serializeDateTimeArray(value);
        }

        /// <summary>
        ///     时间序列化
        /// </summary>
        /// <param name="value">时间</param>
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapSerialize(DateTime[] value)
        {
            if (value == null) *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
            else serializeDateTimeArray(value);
        }

        /// <summary>
        ///     时间序列化
        /// </summary>
        /// <param name="value">时间</param>
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapSerialize(DateTime? value)
        {
            if (value.HasValue) Stream.Unsafer.Write((DateTime)value);
            else *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
        }

        /// <summary>
        ///     时间序列化
        /// </summary>
        /// <param name="value">时间</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(DateTime?[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else Serialize(Stream, value);
        }

        /// <summary>
        ///     时间序列化
        /// </summary>
        /// <param name="value">时间</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serializeDateTimeArray(DateTime?[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else
            {
                var length = Stream.Length;
                Stream.PrepLength(sizeof(int) * 2);
                Stream.Unsafer.AddLength(sizeof(int));
                Serialize(Stream, value);
                *(int*)(Stream.Data + length) = Stream.Length - length;
                Stream.PrepLength();
            }
        }

        /// <summary>
        ///     时间序列化
        /// </summary>
        /// <param name="value">时间</param>
        [TmphMemberSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(DateTime?[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serializeDateTimeArray(value);
        }

        /// <summary>
        ///     时间序列化
        /// </summary>
        /// <param name="value">时间</param>
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapSerialize(DateTime?[] value)
        {
            if (value == null) *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
            else serializeDateTimeArray(value);
        }

        /// <summary>
        ///     Guid序列化
        /// </summary>
        /// <param name="value">Guid</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(Guid[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else Serialize(Stream, value);
        }

        /// <summary>
        ///     Guid序列化
        /// </summary>
        /// <param name="value">Guid</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serializeGuidArray(Guid[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else
            {
                var length = Stream.Length;
                Stream.PrepLength(sizeof(int) * 2);
                Stream.Unsafer.AddLength(sizeof(int));
                Serialize(Stream, value);
                *(int*)(Stream.Data + length) = Stream.Length - length;
                Stream.PrepLength();
            }
        }

        /// <summary>
        ///     Guid序列化
        /// </summary>
        /// <param name="value">Guid</param>
        [TmphMemberSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(Guid[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serializeGuidArray(value);
        }

        /// <summary>
        ///     Guid序列化
        /// </summary>
        /// <param name="value">Guid</param>
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapSerialize(Guid[] value)
        {
            if (value == null) *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
            else serializeGuidArray(value);
        }

        /// <summary>
        ///     Guid序列化
        /// </summary>
        /// <param name="value">Guid</param>
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapSerialize(Guid? value)
        {
            if (value.HasValue) Stream.Unsafer.Write((Guid)value);
            else *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
        }

        /// <summary>
        ///     Guid序列化
        /// </summary>
        /// <param name="value">Guid</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(Guid?[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else Serialize(Stream, value);
        }

        /// <summary>
        ///     Guid序列化
        /// </summary>
        /// <param name="value">Guid</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serializeGuidArray(Guid?[] value)
        {
            if (value.Length == 0) Stream.Write(0);
            else
            {
                var length = Stream.Length;
                Stream.PrepLength(sizeof(int) * 2);
                Stream.Unsafer.AddLength(sizeof(int));
                Serialize(Stream, value);
                *(int*)(Stream.Data + length) = Stream.Length - length;
                Stream.PrepLength();
            }
        }

        /// <summary>
        ///     Guid序列化
        /// </summary>
        /// <param name="value">Guid</param>
        [TmphMemberSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(Guid?[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serializeGuidArray(value);
        }

        /// <summary>
        ///     Guid序列化
        /// </summary>
        /// <param name="value">Guid</param>
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapSerialize(Guid?[] value)
        {
            if (value == null) *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
            else serializeGuidArray(value);
        }

        /// <summary>
        ///     字符串序列化
        /// </summary>
        /// <param name="value">字符串</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(string value)
        {
            if (value.Length == 0) Stream.Write(0);
            else Serialize(Stream, value);
        }

        /// <summary>
        ///     字符串序列化
        /// </summary>
        /// <param name="value">字符串</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serializeString(string value)
        {
            if (value.Length == 0) Stream.Write(0);
            else
            {
                var length = Stream.Length;
                Stream.PrepLength(sizeof(int) * 2);
                Stream.Unsafer.AddLength(sizeof(int));
                Serialize(Stream, value);
                *(int*)(Stream.Data + length) = Stream.Length - length;
                Stream.PrepLength();
            }
        }

        /// <summary>
        ///     字符串序列化
        /// </summary>
        /// <param name="value">字符串</param>
        [TmphMemberSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(string value)
        {
            if (value == null) Stream.Write(NullValue);
            else serializeString(value);
        }

        /// <summary>
        ///     字符串序列化
        /// </summary>
        /// <param name="value">字符串</param>
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapSerialize(string value)
        {
            if (value == null) *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
            else serializeString(value);
        }

        /// <summary>
        ///     字符串序列化
        /// </summary>
        /// <param name="value">字符串</param>
        [TmphMemberSerializeMethod]
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(TmphSubString value)
        {
            if (value.Length == 0) Stream.Write(0);
            else
            {
                var length = Stream.Length;
                Stream.PrepLength(sizeof(int) * 2);
                Stream.Unsafer.AddLength(sizeof(int));
                Serialize(Stream, value);
                *(int*)(Stream.Data + length) = Stream.Length - length;
                Stream.PrepLength();
            }
        }

        /// <summary>
        ///     字符串序列化
        /// </summary>
        /// <param name="array">字符串数组</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(string[] array)
        {
            if (array.Length == 0) Stream.Write(0);
            else
            {
                var arrayMap = new TmphArrayMap(Stream, array.Length, array.Length);
                foreach (var value in array) arrayMap.Next(value != null);
                arrayMap.End(Stream);
                foreach (var value in array)
                {
                    if (value != null)
                    {
                        if (value.Length == 0) Stream.Write(0);
                        else Serialize(Stream, value);
                    }
                }
            }
        }

        /// <summary>
        ///     字符串序列化
        /// </summary>
        /// <param name="array">字符串数组</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serializeStringArray(string[] array)
        {
            if (array.Length == 0) Stream.Write(0);
            else
            {
                var length = Stream.Length;
                Stream.PrepLength(sizeof(int) * 2);
                Stream.Unsafer.AddLength(sizeof(int));

                var arrayMap = new TmphArrayMap(Stream, array.Length, array.Length);
                foreach (var value in array) arrayMap.Next(value != null);
                arrayMap.End(Stream);
                foreach (var value in array)
                {
                    if (value != null)
                    {
                        if (value.Length == 0) Stream.Write(0);
                        else Serialize(Stream, value);
                    }
                }

                *(int*)(Stream.Data + length) = Stream.Length - length;
                Stream.PrepLength();
            }
        }

        /// <summary>
        ///     字符串序列化
        /// </summary>
        /// <param name="array">字符串数组</param>
        [TmphMemberSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(string[] array)
        {
            if (array == null) Stream.Write(NullValue);
            else serializeStringArray(array);
        }

        /// <summary>
        ///     字符串序列化
        /// </summary>
        /// <param name="array">字符串数组</param>
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapSerialize(string[] array)
        {
            if (array == null) *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
            else serializeStringArray(array);
        }

        /// <summary>
        ///     对象序列化
        /// </summary>
        /// <param name="value">数据对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private static void nullableSerialize<TValueType>(TmphIndexSerializer serializer, TValueType? value)
            where TValueType : struct
        {
            TmphTypeSerializer<TValueType>.Serialize(serializer, value.Value);
        }

        /// <summary>
        ///     对象序列化
        /// </summary>
        /// <param name="value">数据对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void structSerialize<TValueType>(TValueType value)
        {
            var length = Stream.Length;
            Stream.PrepLength(sizeof(int) * 2);
            Stream.Unsafer.AddLength(sizeof(int));
            TmphTypeSerializer<TValueType>.Serialize(this, value);
            *(int*)(Stream.Data + length) = Stream.Length - length;
            Stream.PrepLength();
        }

        /// <summary>
        ///     对象序列化
        /// </summary>
        /// <param name="value">数据对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void nullableMemberSerialize<TValueType>(TValueType? value) where TValueType : struct
        {
            if (value.HasValue) structSerialize(value.Value);
            else Stream.Write(NullValue);
        }

        /// <summary>
        ///     对象序列化
        /// </summary>
        /// <param name="value">数据对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void classSerialize<TValueType>(TValueType value) where TValueType : class
        {
            if (value == null) Stream.Write(NullValue);
            else structSerialize(value);
        }

        /// <summary>
        ///     对象序列化
        /// </summary>
        /// <param name="value">数据对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapClassSerialize<TValueType>(TValueType value) where TValueType : class
        {
            if (value == null) *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
            else structSerialize(value);
        }

        /// <summary>
        ///     对象序列化
        /// </summary>
        /// <param name="value">数据对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapNullableSerialize<TValueType>(TValueType? value) where TValueType : struct
        {
            if (value.HasValue) structSerialize(value.Value);
            else *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
        }

        /// <summary>
        ///     对象序列化
        /// </summary>
        /// <param name="value">数据对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private static void keyValuePairSerialize<TKeyType, TValueType>(TmphIndexSerializer serializer,
            KeyValuePair<TKeyType, TValueType> value)
        {
            TmphTypeSerializer<TmphKeyValue<TKeyType, TValueType>>.Serialize(serializer,
                new TmphKeyValue<TKeyType, TValueType>(value.Key, value.Value));
        }

        /// <summary>
        ///     对象序列化
        /// </summary>
        /// <param name="value">数据对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberKeyValuePairSerialize<TKeyType, TValueType>(KeyValuePair<TKeyType, TValueType> value)
        {
            structSerialize(new TmphKeyValue<TKeyType, TValueType>(value.Key, value.Value));
        }

        /// <summary>
        ///     字典序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="value">数据对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void dictionarySerializeType<dictionaryType, TKeyType, TValueType>(dictionaryType value)
            where dictionaryType : IDictionary<TKeyType, TValueType>
        {
            var index = 0;
            var keys = new TKeyType[value.Count];
            var values = new TValueType[keys.Length];
            foreach (var keyValue in value)
            {
                keys[index] = keyValue.Key;
                values[index++] = keyValue.Value;
            }
            TmphTypeSerializer<TKeyType[]>.DefaultSerializer(this, keys);
            TmphTypeSerializer<TValueType[]>.DefaultSerializer(this, values);
        }

        /// <summary>
        ///     字典序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="value">数据对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void dictionarySerialize<dictionaryType, TKeyType, TValueType>(dictionaryType value)
            where dictionaryType : IDictionary<TKeyType, TValueType>
        {
            var length = Stream.Length;
            Stream.PrepLength(sizeof(int) * 2);
            Stream.Unsafer.AddLength(sizeof(int));
            dictionarySerializeType<dictionaryType, TKeyType, TValueType>(value);
            *(int*)(Stream.Data + length) = Stream.Length - length;
            Stream.PrepLength();
        }

        /// <summary>
        ///     字典序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="value">数据对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void dictionaryMember<dictionaryType, TKeyType, TValueType>(dictionaryType value)
            where dictionaryType : IDictionary<TKeyType, TValueType>
        {
            if (value == null) Stream.Write(NullValue);
            else dictionarySerialize<dictionaryType, TKeyType, TValueType>(value);
        }

        /// <summary>
        ///     字典序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="value">数据对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapDictionary<dictionaryType, TKeyType, TValueType>(dictionaryType value)
            where dictionaryType : IDictionary<TKeyType, TValueType>
        {
            if (value == null) *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
            else dictionarySerialize<dictionaryType, TKeyType, TValueType>(value);
        }

        /// <summary>
        ///     对象序列化
        /// </summary>
        /// <param name="value">数据对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void subArraySerialize<TValueType>(TmphSubArray<TValueType> value)
        {
            var array = value.ToArray();
            TmphTypeSerializer<TValueType[]>.DefaultSerializer(this, array);
        }

        /// <summary>
        ///     对象序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="value">数据对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private static void baseSerialize<TValueType, childType>(TmphIndexSerializer serializer, childType value)
            where childType : TValueType
        {
            TmphTypeSerializer<TValueType>.BaseSerialize(serializer, value);
        }

        /// <summary>
        ///     数组转换
        /// </summary>
        /// <param name="array">数组对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void structArrayType<TValueType>(TValueType[] array)
        {
            if (array.Length == 0) Stream.Write(0);
            else
            {
                Stream.Write(array.Length);
                foreach (var value in array) TmphTypeSerializer<TValueType>.Serialize(this, value);
            }
        }

        /// <summary>
        ///     数组转换
        /// </summary>
        /// <param name="array">数组对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void structArray<TValueType>(TValueType[] array)
        {
            if (array.Length == 0) Stream.Write(0);
            else
            {
                var length = Stream.Length;
                Stream.PrepLength(sizeof(int) * 2);
                Stream.Unsafer.AddLength(sizeof(int));
                Stream.Unsafer.Write(array.Length);
                foreach (var value in array) TmphTypeSerializer<TValueType>.Serialize(this, value);
                *(int*)(Stream.Data + length) = Stream.Length - length;
                Stream.PrepLength();
            }
        }

        /// <summary>
        ///     数组转换
        /// </summary>
        /// <param name="array">数组对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void structArrayMember<TValueType>(TValueType[] array)
        {
            if (array == null) Stream.Write(NullValue);
            else structArray(array);
        }

        /// <summary>
        ///     数组转换
        /// </summary>
        /// <param name="array">数组对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void structArrayMemberMap<TValueType>(TValueType[] array)
        {
            if (array == null) *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
            else structArray(array);
        }

        /// <summary>
        ///     数组转换
        /// </summary>
        /// <param name="array">数组对象</param>
        private void nullableArrayType<TValueType>(TValueType[] array) where TValueType : struct
        {
            if (array.Length == 0) Stream.Write(0);
            else
            {
                var arrayMap = new TmphArrayMap(Stream, array.Length);
                foreach (TValueType? value in array) arrayMap.Next(value.HasValue);
                arrayMap.End(Stream);

                foreach (TValueType? value in array)
                {
                    if (value.HasValue) TmphTypeSerializer<TValueType>.Serialize(this, value.Value);
                }
            }
        }

        /// <summary>
        ///     数组转换
        /// </summary>
        /// <param name="array">数组对象</param>
        private void nullableArray<TValueType>(TValueType[] array) where TValueType : struct
        {
            if (array.Length == 0) Stream.Write(0);
            else
            {
                var length = Stream.Length;
                Stream.PrepLength(sizeof(int) * 2);
                Stream.Unsafer.AddLength(sizeof(int));

                var arrayMap = new TmphArrayMap(Stream, array.Length);
                foreach (TValueType? value in array) arrayMap.Next(value.HasValue);
                arrayMap.End(Stream);

                foreach (TValueType? value in array)
                {
                    if (value.HasValue) TmphTypeSerializer<TValueType>.Serialize(this, value.Value);
                }

                *(int*)(Stream.Data + length) = Stream.Length - length;
                Stream.PrepLength();
            }
        }

        /// <summary>
        ///     数组转换
        /// </summary>
        /// <param name="array">数组对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void nullableArrayMember<TValueType>(TValueType[] array) where TValueType : struct
        {
            if (array == null) Stream.Write(NullValue);
            else nullableArray(array);
        }

        /// <summary>
        ///     数组转换
        /// </summary>
        /// <param name="array">数组对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void nullableArrayMemberMap<TValueType>(TValueType[] array) where TValueType : struct
        {
            if (array == null) *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
            else nullableArray(array);
        }

        /// <summary>
        ///     数组转换
        /// </summary>
        /// <param name="array">数组对象</param>
        private void arrayType<TValueType>(TValueType[] array) where TValueType : class
        {
            if (array.Length == 0) Stream.Write(0);
            else
            {
                var arrayMap = new TmphArrayMap(Stream, array.Length);
                foreach (var value in array) arrayMap.Next(value != null);
                arrayMap.End(Stream);

                foreach (var value in array)
                {
                    if (value != null) TmphTypeSerializer<TValueType>.Serialize(this, value);
                }
            }
        }

        /// <summary>
        ///     数组转换
        /// </summary>
        /// <param name="array">数组对象</param>
        private void array<TValueType>(TValueType[] array) where TValueType : class
        {
            if (array.Length == 0) Stream.Write(0);
            else
            {
                var length = Stream.Length;
                Stream.PrepLength(sizeof(int) * 2);
                Stream.Unsafer.AddLength(sizeof(int));

                var arrayMap = new TmphArrayMap(Stream, array.Length);
                foreach (var value in array) arrayMap.Next(value != null);
                arrayMap.End(Stream);

                foreach (var value in array)
                {
                    if (value != null) TmphTypeSerializer<TValueType>.Serialize(this, value);
                }

                *(int*)(Stream.Data + length) = Stream.Length - length;
                Stream.PrepLength();
            }
        }

        /// <summary>
        ///     数组转换
        /// </summary>
        /// <param name="array">数组对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void arrayMember<TValueType>(TValueType[] array) where TValueType : class
        {
            if (array == null) Stream.Write(NullValue);
            else this.array(array);
        }

        /// <summary>
        ///     数组转换
        /// </summary>
        /// <param name="array">数组对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void arrayMemberMap<TValueType>(TValueType[] array) where TValueType : class
        {
            if (array == null) *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
            else this.array(array);
        }

        /// <summary>
        ///     集合转换
        /// </summary>
        /// <param name="collection">对象集合</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void collection<TValueType, collectionType>(collectionType collection)
            where collectionType : ICollection<TValueType>
        {
            TmphTypeSerializer<TValueType[]>.DefaultSerializer(this, collection.GetArray());
        }

        /// <summary>
        ///     集合转换
        /// </summary>
        /// <param name="collection">对象集合</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void dictionary<dictionaryType, TKeyType, TValueType>(dictionaryType dictionary)
            where dictionaryType : IDictionary<TKeyType, TValueType>
        {
            var keys = new TKeyType[dictionary.Count];
            var values = new TValueType[keys.Length];
            var index = 0;
            foreach (var keyValue in dictionary)
            {
                keys[index] = keyValue.Key;
                values[index++] = keyValue.Value;
            }
            TmphTypeSerializer<TKeyType[]>.DefaultSerializer(this, keys);
            TmphTypeSerializer<TValueType[]>.DefaultSerializer(this, values);
        }

        /// <summary>
        ///     枚举值序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举值序列化</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void enumByteMemberMap<TValueType>(TValueType value)
        {
            Stream.Unsafer.Write((uint)TmphPub.TmphEnumCast<TValueType, byte>.ToInt(value));
        }

        /// <summary>
        ///     枚举值序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举值序列化</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void enumSByteMemberMap<TValueType>(TValueType value)
        {
            Stream.Unsafer.Write((int)TmphPub.TmphEnumCast<TValueType, sbyte>.ToInt(value));
        }

        /// <summary>
        ///     枚举值序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举值序列化</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void enumShortMemberMap<TValueType>(TValueType value)
        {
            Stream.Unsafer.Write((int)TmphPub.TmphEnumCast<TValueType, short>.ToInt(value));
        }

        /// <summary>
        ///     枚举值序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举值序列化</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void enumUShortMemberMap<TValueType>(TValueType value)
        {
            Stream.Unsafer.Write((uint)TmphPub.TmphEnumCast<TValueType, ushort>.ToInt(value));
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private void enumByteArrayType<TValueType>(TValueType[] array)
        {
            if (array.Length == 0) Stream.Write(0);
            else
            {
                var length = (array.Length + 7) & (int.MaxValue - 3);
                Stream.PrepLength(length);
                var write = Stream.CurrentData;
                *(int*)write = array.Length;
                write += sizeof(int);
                foreach (var value in array) *write++ = TmphPub.TmphEnumCast<TValueType, byte>.ToInt(value);
                Stream.Unsafer.AddLength(length);
                Stream.PrepLength();
            }
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private void enumByteArray<TValueType>(TValueType[] array)
        {
            if (array.Length == 0) Stream.Write(0);
            else
            {
                var length = (array.Length + (3 + sizeof(int) * 2)) & (int.MaxValue - 3);
                Stream.PrepLength(length);
                var write = Stream.CurrentData + sizeof(int);
                *(int*)write = array.Length;
                write += sizeof(int);
                foreach (var value in array) *write++ = TmphPub.TmphEnumCast<TValueType, byte>.ToInt(value);
                *(int*)Stream.CurrentData = length;
                Stream.Unsafer.AddLength(length);
                Stream.PrepLength();
            }
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void enumByteArrayMember<TValueType>(TValueType[] array)
        {
            if (array == null) Stream.Write(NullValue);
            else enumByteArray(array);
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void enumByteArrayMemberMap<TValueType>(TValueType[] array)
        {
            if (array == null) *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
            else enumByteArray(array);
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private void enumSByteArrayType<TValueType>(TValueType[] array)
        {
            if (array.Length == 0) Stream.Write(0);
            else
            {
                var length = (array.Length + 7) & (int.MaxValue - 3);
                Stream.PrepLength(length);
                var write = Stream.CurrentData;
                *(int*)write = array.Length;
                write += sizeof(int);
                foreach (var value in array) *(sbyte*)write++ = TmphPub.TmphEnumCast<TValueType, sbyte>.ToInt(value);
                Stream.Unsafer.AddLength(length);
                Stream.PrepLength();
            }
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private void enumSByteArray<TValueType>(TValueType[] array)
        {
            if (array.Length == 0) Stream.Write(0);
            else
            {
                var length = (array.Length + (3 + sizeof(int) * 2)) & (int.MaxValue - 3);
                Stream.PrepLength(length);
                var write = Stream.CurrentData + sizeof(int);
                *(int*)write = array.Length;
                write += sizeof(int);
                foreach (var value in array) *(sbyte*)write++ = TmphPub.TmphEnumCast<TValueType, sbyte>.ToInt(value);
                *(int*)Stream.CurrentData = length;
                Stream.Unsafer.AddLength(length);
                Stream.PrepLength();
            }
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void enumSByteArrayMember<TValueType>(TValueType[] array)
        {
            if (array == null) Stream.Write(NullValue);
            else enumSByteArray(array);
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void enumSByteArrayMemberMap<TValueType>(TValueType[] array)
        {
            if (array == null) *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
            else enumSByteArray(array);
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private void enumShortArrayType<TValueType>(TValueType[] array)
        {
            if (array.Length == 0) Stream.Write(0);
            else
            {
                var length = ((array.Length * sizeof(short)) + 7) & (int.MaxValue - 3);
                Stream.PrepLength(length);
                var write = Stream.CurrentData;
                *(int*)write = array.Length;
                write += sizeof(int);
                foreach (var value in array)
                {
                    *(short*)write = TmphPub.TmphEnumCast<TValueType, short>.ToInt(value);
                    write += sizeof(short);
                }
                Stream.Unsafer.AddLength(length);
                Stream.PrepLength();
            }
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private void enumShortArray<TValueType>(TValueType[] array)
        {
            if (array.Length == 0) Stream.Write(0);
            else
            {
                var length = ((array.Length * sizeof(short)) + (3 + sizeof(int) * 2)) & (int.MaxValue - 3);
                Stream.PrepLength(length);
                var write = Stream.CurrentData + sizeof(int);
                *(int*)write = array.Length;
                write += sizeof(int);
                foreach (var value in array)
                {
                    *(short*)write = TmphPub.TmphEnumCast<TValueType, short>.ToInt(value);
                    write += sizeof(short);
                }
                *(int*)Stream.CurrentData = length;
                Stream.Unsafer.AddLength(length);
                Stream.PrepLength();
            }
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void enumShortArrayMember<TValueType>(TValueType[] array)
        {
            if (array == null) Stream.Write(NullValue);
            else enumShortArray(array);
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void enumShortArrayMemberMap<TValueType>(TValueType[] array)
        {
            if (array == null) *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
            else enumShortArray(array);
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private void enumUShortArrayType<TValueType>(TValueType[] array)
        {
            if (array.Length == 0) Stream.Write(0);
            else
            {
                var length = ((array.Length * sizeof(ushort)) + 7) & (int.MaxValue - 3);
                Stream.PrepLength(length);
                var write = Stream.CurrentData;
                *(int*)write = array.Length;
                write += sizeof(int);
                foreach (var value in array)
                {
                    *(ushort*)write = TmphPub.TmphEnumCast<TValueType, ushort>.ToInt(value);
                    write += sizeof(ushort);
                }
                Stream.Unsafer.AddLength(length);
                Stream.PrepLength();
            }
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private void enumUShortArray<TValueType>(TValueType[] array)
        {
            if (array.Length == 0) Stream.Write(0);
            else
            {
                var length = ((array.Length * sizeof(ushort)) + (3 + sizeof(int) * 2)) & (int.MaxValue - 3);
                Stream.PrepLength(length);
                var write = Stream.CurrentData + sizeof(int);
                *(int*)write = array.Length;
                write += sizeof(int);
                foreach (var value in array)
                {
                    *(ushort*)write = TmphPub.TmphEnumCast<TValueType, ushort>.ToInt(value);
                    write += sizeof(ushort);
                }
                *(int*)Stream.CurrentData = length;
                Stream.Unsafer.AddLength(length);
                Stream.PrepLength();
            }
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void enumUShortArrayMember<TValueType>(TValueType[] array)
        {
            if (array == null) Stream.Write(NullValue);
            else enumUShortArray(array);
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void enumUShortArrayMemberMap<TValueType>(TValueType[] array)
        {
            if (array == null) *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
            else enumUShortArray(array);
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private void enumIntArrayType<TValueType>(TValueType[] array)
        {
            if (array.Length == 0) Stream.Write(0);
            else
            {
                var length = (array.Length + 1) * sizeof(int);
                Stream.PrepLength(length);
                var write = Stream.CurrentData;
                *(int*)write = array.Length;
                foreach (var value in array)
                    *(int*)(write += sizeof(int)) = TmphPub.TmphEnumCast<TValueType, int>.ToInt(value);
                Stream.Unsafer.AddLength(length);
                Stream.PrepLength();
            }
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private void enumIntArray<TValueType>(TValueType[] array)
        {
            if (array.Length == 0) Stream.Write(0);
            else
            {
                var length = (array.Length + 2) * sizeof(int);
                Stream.PrepLength(length);
                var write = Stream.CurrentData + sizeof(int);
                *(int*)write = array.Length;
                foreach (var value in array)
                    *(int*)(write += sizeof(int)) = TmphPub.TmphEnumCast<TValueType, int>.ToInt(value);
                *(int*)Stream.CurrentData = length;
                Stream.Unsafer.AddLength(length);
                Stream.PrepLength();
            }
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void enumIntArrayMember<TValueType>(TValueType[] array)
        {
            if (array == null) Stream.Write(NullValue);
            else enumIntArray(array);
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void enumIntArrayMemberMap<TValueType>(TValueType[] array)
        {
            if (array == null) *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
            else enumIntArray(array);
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private void enumUIntArrayType<TValueType>(TValueType[] array)
        {
            if (array.Length == 0) Stream.Write(0);
            else
            {
                var length = (array.Length + 1) * sizeof(uint);
                Stream.PrepLength(length);
                var write = Stream.CurrentData;
                *(int*)write = array.Length;
                foreach (var value in array)
                    *(uint*)(write += sizeof(uint)) = TmphPub.TmphEnumCast<TValueType, uint>.ToInt(value);
                Stream.Unsafer.AddLength(length);
                Stream.PrepLength();
            }
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private void enumUIntArray<TValueType>(TValueType[] array)
        {
            if (array.Length == 0) Stream.Write(0);
            else
            {
                var length = (array.Length + 2) * sizeof(uint);
                Stream.PrepLength(length);
                var write = Stream.CurrentData + sizeof(int);
                *(int*)write = array.Length;
                foreach (var value in array)
                    *(uint*)(write += sizeof(uint)) = TmphPub.TmphEnumCast<TValueType, uint>.ToInt(value);
                *(int*)Stream.CurrentData = length;
                Stream.Unsafer.AddLength(length);
                Stream.PrepLength();
            }
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void enumUIntArrayMember<TValueType>(TValueType[] array)
        {
            if (array == null) Stream.Write(NullValue);
            else enumUIntArray(array);
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void enumUIntArrayMemberMap<TValueType>(TValueType[] array)
        {
            if (array == null) *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
            else enumUIntArray(array);
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private void enumLongArrayType<TValueType>(TValueType[] array)
        {
            if (array.Length == 0) Stream.Write(0);
            else
            {
                var length = array.Length * sizeof(long) + sizeof(int);
                Stream.PrepLength(length);
                var write = Stream.CurrentData;
                *(int*)write = array.Length;
                write += sizeof(int);
                foreach (var value in array)
                {
                    *(long*)write = TmphPub.TmphEnumCast<TValueType, long>.ToInt(value);
                    write += sizeof(long);
                }
                Stream.Unsafer.AddLength(length);
                Stream.PrepLength();
            }
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private void enumLongArray<TValueType>(TValueType[] array)
        {
            if (array.Length == 0) Stream.Write(0);
            else
            {
                var length = array.Length * sizeof(long) + sizeof(int) * 2;
                Stream.PrepLength(length);
                var write = Stream.CurrentData + sizeof(int);
                *(int*)write = array.Length;
                write += sizeof(int);
                foreach (var value in array)
                {
                    *(long*)write = TmphPub.TmphEnumCast<TValueType, long>.ToInt(value);
                    write += sizeof(long);
                }
                *(int*)Stream.CurrentData = length;
                Stream.Unsafer.AddLength(length);
                Stream.PrepLength();
            }
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void enumLongArrayMember<TValueType>(TValueType[] array)
        {
            if (array == null) Stream.Write(NullValue);
            else enumLongArray(array);
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void enumLongArrayMemberMap<TValueType>(TValueType[] array)
        {
            if (array == null) *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
            else enumLongArray(array);
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private void enumULongArrayType<TValueType>(TValueType[] array)
        {
            if (array.Length == 0) Stream.Write(0);
            else
            {
                var length = array.Length * sizeof(ulong) + sizeof(int);
                Stream.PrepLength(length);
                var write = Stream.CurrentData;
                *(int*)write = array.Length;
                write += sizeof(int);
                foreach (var value in array)
                {
                    *(ulong*)write = TmphPub.TmphEnumCast<TValueType, ulong>.ToInt(value);
                    write += sizeof(ulong);
                }
                Stream.Unsafer.AddLength(length);
                Stream.PrepLength();
            }
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private void enumULongArray<TValueType>(TValueType[] array)
        {
            if (array.Length == 0) Stream.Write(0);
            else
            {
                var length = array.Length * sizeof(ulong) + sizeof(int) * 2;
                Stream.PrepLength(length);
                var write = Stream.CurrentData + sizeof(int);
                *(int*)write = array.Length;
                write += sizeof(int);
                foreach (var value in array)
                {
                    *(ulong*)write = TmphPub.TmphEnumCast<TValueType, ulong>.ToInt(value);
                    write += sizeof(ulong);
                }
                *(int*)Stream.CurrentData = length;
                Stream.Unsafer.AddLength(length);
                Stream.PrepLength();
            }
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void enumULongArrayMember<TValueType>(TValueType[] array)
        {
            if (array == null) Stream.Write(NullValue);
            else enumULongArray(array);
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void enumULongArrayMemberMap<TValueType>(TValueType[] array)
        {
            if (array == null) *(uint*)(Stream.CurrentData - sizeof(int)) |= 0x80000000U;
            else enumULongArray(array);
        }

        /// <summary>
        ///     序列化
        /// </summary>
        /// <typeparam name="TValueType">目标数据类型</typeparam>
        /// <param name="value">数据对象</param>
        /// <param name="TmphConfig">配置参数</param>
        /// <returns>序列化数据</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static byte[] Serialize<TValueType>(TValueType value, TmphConfig TmphConfig = null)
        {
            if (value == null) return BitConverter.GetBytes(NullValue);
            var serializer = TmphTypePool<TmphIndexSerializer>.Pop() ?? new TmphIndexSerializer();
            try
            {
                return serializer.serialize(value, TmphConfig ?? defaultConfig);
            }
            finally
            {
                serializer.free();
            }
        }

        /// <summary>
        ///     序列化
        /// </summary>
        /// <typeparam name="TValueType">目标数据类型</typeparam>
        /// <param name="value">数据对象</param>
        /// <param name="stream">序列化输出缓冲区</param>
        /// <param name="TmphConfig">配置参数</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static void Serialize<TValueType>(TValueType value, TmphUnmanagedStream stream, TmphConfig TmphConfig = null)
        {
            if (stream == null || value == null) TmphLog.Default.Throw(TmphLog.TmphExceptionType.Null);
            else
            {
                var serializer = TmphTypePool<TmphIndexSerializer>.Pop() ?? new TmphIndexSerializer();
                try
                {
                    serializer.serialize(value, stream, TmphConfig ?? defaultConfig);
                }
                finally
                {
                    serializer.free();
                }
            }
        }

        /// <summary>
        ///     获取基本类型转换函数
        /// </summary>
        /// <param name="type">基本类型</param>
        /// <returns>转换函数</returns>
        private static MethodInfo getSerializeMethod(Type type)
        {
            MethodInfo method;
            if (serializeMethods.TryGetValue(type, out method))
            {
                serializeMethods.Remove(type);
                return method;
            }
            return null;
        }

        /// <summary>
        ///     获取基本类型转换函数
        /// </summary>
        /// <param name="type">基本类型</param>
        /// <returns>转换函数</returns>
        private static MethodInfo getMemberSerializeMethod(Type type)
        {
            MethodInfo method;
            return memberSerializeMethods.TryGetValue(type, out method) ? method : null;
        }

        /// <summary>
        ///     获取基本类型转换函数
        /// </summary>
        /// <param name="type">基本类型</param>
        /// <returns>转换函数</returns>
        private static MethodInfo getMemberMapSerializeMethod(Type type)
        {
            MethodInfo method;
            return memberMapSerializeMethods.TryGetValue(type, out method) ? method : null;
        }

        /// <summary>
        ///     基本类型序列化函数
        /// </summary>
        internal sealed class TmphSerializeMethod : Attribute
        {
        }

        /// <summary>
        ///     基本类型序列化函数
        /// </summary>
        internal sealed class TmphMemberSerializeMethod : Attribute
        {
        }

        /// <summary>
        ///     基本类型序列化函数
        /// </summary>
        internal sealed class TmphMemberMapSerializeMethod : Attribute
        {
        }

        /// <summary>
        ///     字段信息
        /// </summary>
        internal new class TmphFieldInfo : TmphBinarySerializer.TmphFieldInfo
        {
            /// <summary>
            ///     固定类型字节数
            /// </summary>
            private static readonly Dictionary<Type, byte> memberMapFixedSizes;

            /// <summary>
            ///     固定分组排序字节数
            /// </summary>
            internal byte MemberMapFixedSize;

            static TmphFieldInfo()
            {
                memberMapFixedSizes = TmphDictionary.CreateOnly<Type, byte>();
                memberMapFixedSizes.Add(typeof(bool), sizeof(int));
                memberMapFixedSizes.Add(typeof(byte), sizeof(int));
                memberMapFixedSizes.Add(typeof(sbyte), sizeof(int));
                memberMapFixedSizes.Add(typeof(short), sizeof(int));
                memberMapFixedSizes.Add(typeof(ushort), sizeof(int));
                memberMapFixedSizes.Add(typeof(int), sizeof(int));
                memberMapFixedSizes.Add(typeof(uint), sizeof(uint));
                memberMapFixedSizes.Add(typeof(long), sizeof(long));
                memberMapFixedSizes.Add(typeof(ulong), sizeof(ulong));
                memberMapFixedSizes.Add(typeof(char), sizeof(int));
                memberMapFixedSizes.Add(typeof(DateTime), sizeof(long));
                memberMapFixedSizes.Add(typeof(float), sizeof(float));
                memberMapFixedSizes.Add(typeof(double), sizeof(double));
                memberMapFixedSizes.Add(typeof(decimal), sizeof(decimal));
                memberMapFixedSizes.Add(typeof(Guid), (byte)sizeof(Guid));
            }

            /// <summary>
            ///     字段信息
            /// </summary>
            /// <param name="field"></param>
            internal TmphFieldInfo(TmphFieldIndex field)
                : base(field)
            {
                if (Field.FieldType.IsEnum)
                    memberMapFixedSizes.TryGetValue(Field.FieldType.GetEnumUnderlyingType(), out MemberMapFixedSize);
                else
                    memberMapFixedSizes.TryGetValue(Field.FieldType.nullableType() ?? Field.FieldType,
                        out MemberMapFixedSize);
            }
        }

        /// <summary>
        ///     二进制数据序列化
        /// </summary>
        internal static class TmphTypeSerializer
        {
            /// <summary>
            ///     未知类型序列化调用函数信息集合
            /// </summary>
            private static TmphInterlocked.TmphDictionary<Type, MethodInfo> memberSerializers =
                new TmphInterlocked.TmphDictionary<Type, MethodInfo>(TmphDictionary.CreateOnly<Type, MethodInfo>());

            /// <summary>
            ///     未知类型序列化调用函数信息集合
            /// </summary>
            private static TmphInterlocked.TmphDictionary<Type, MethodInfo> memberMapSerializers =
                new TmphInterlocked.TmphDictionary<Type, MethodInfo>(TmphDictionary.CreateOnly<Type, MethodInfo>());

            /// <summary>
            ///     获取字段成员集合
            /// </summary>
            /// <param name="type"></param>
            /// <returns>字段成员集合</returns>
            public static TmphFields<TmphFieldInfo> GetFields(TmphFieldIndex[] fieldIndexs, out int memberCountVerify,
                out int memberMapFixedSize)
            {
                TmphSubArray<TmphFieldInfo> fixedFields = new TmphSubArray<TmphFieldInfo>(fieldIndexs.Length),
                    fields = new TmphSubArray<TmphFieldInfo>();
                var jsonFields = new TmphSubArray<TmphFieldIndex>();
                fields.UnsafeSet(fixedFields.array, fixedFields.array.length(), 0);
                var fixedSize = memberMapFixedSize = 0;
                foreach (var field in fieldIndexs)
                {
                    var type = field.Member.FieldType;
                    if (!type.IsPointer && (!type.IsArray || type.GetArrayRank() == 1) && !field.IsIgnore)
                    {
                        var attribute = field.GetAttribute<TmphBinarySerialize.TmphMember>(true, true);
                        if (attribute == null || attribute.IsSetup)
                        {
                            if (attribute != null && attribute.IsJson) jsonFields.Add(field);
                            else
                            {
                                var value = new TmphFieldInfo(field);
                                if (value.FixedSize == 0) fields.UnsafeAddExpand(value);
                                else
                                {
                                    fixedFields.Add(value);
                                    fixedSize += value.FixedSize;
                                    memberMapFixedSize += value.MemberMapFixedSize + sizeof(int);
                                }
                            }
                        }
                    }
                }
                memberCountVerify = fixedFields.Count + fields.Count + 0x40000000;
                return new TmphFields<TmphFieldInfo>
                {
                    FixedFields = fixedFields.Sort(TmphBinarySerializer.TmphFieldInfo.FixedSizeSort),
                    Fields = fields,
                    JsonFields = jsonFields,
                    FixedSize = fixedSize
                };
            }

            /// <summary>
            ///     获取字段成员集合
            /// </summary>
            /// <param name="type"></param>
            /// <returns>字段成员集合</returns>
            public static TmphSubArray<TmphMemberIndex> GetMembers(TmphFieldIndex[] fieldIndexs)
            {
                var fields = new TmphSubArray<TmphMemberIndex>(fieldIndexs.Length);
                foreach (var field in fieldIndexs)
                {
                    var type = field.Member.FieldType;
                    if (!type.IsPointer && (!type.IsArray || type.GetArrayRank() == 1) && !field.IsIgnore)
                    {
                        var attribute = field.GetAttribute<TmphBinarySerialize.TmphMember>(true, true);
                        if (attribute == null || attribute.IsSetup) fields.Add(field);
                    }
                }
                return fields;
            }

            /// <summary>
            ///     未知类型枚举序列化委托调用函数信息
            /// </summary>
            /// <param name="type">数组类型</param>
            /// <returns>未知类型序列化委托调用函数信息</returns>
            public static MethodInfo getMemberSerializer(Type type)
            {
                MethodInfo method;
                if (memberSerializers.TryGetValue(type, out method)) return method;
                if (type.IsArray)
                {
                    var elementType = type.GetElementType();
                    if (elementType.IsValueType)
                    {
                        if (elementType.IsEnum)
                        {
                            var TEnumType = Enum.GetUnderlyingType(elementType);
                            if (TEnumType == typeof(uint)) method = enumUIntArrayMemberMethod;
                            else if (TEnumType == typeof(byte)) method = enumByteArrayMemberMethod;
                            else if (TEnumType == typeof(ulong)) method = enumULongArrayMemberMethod;
                            else if (TEnumType == typeof(ushort)) method = enumUShortArrayMemberMethod;
                            else if (TEnumType == typeof(long)) method = enumLongArrayMemberMethod;
                            else if (TEnumType == typeof(short)) method = enumShortArrayMemberMethod;
                            else if (TEnumType == typeof(sbyte)) method = enumSByteArrayMemberMethod;
                            else method = enumIntArrayMemberMethod;
                            method = method.MakeGenericMethod(elementType);
                        }
                        else if (elementType.IsGenericType &&
                                 elementType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            method = nullableArrayMemberMethod.MakeGenericMethod(elementType.GetGenericArguments());
                        }
                        else method = structArrayMemberMethod.MakeGenericMethod(elementType);
                    }
                    else method = arrayMemberMethod.MakeGenericMethod(elementType);
                }
                else if (type.IsEnum)
                {
                    var TEnumType = Enum.GetUnderlyingType(type);
                    if (TEnumType == typeof(uint)) method = enumUIntMemberMethod;
                    else if (TEnumType == typeof(byte)) method = enumByteMemberMethod;
                    else if (TEnumType == typeof(ulong)) method = enumULongMemberMethod;
                    else if (TEnumType == typeof(ushort)) method = enumUShortMemberMethod;
                    else if (TEnumType == typeof(long)) method = enumLongMemberMethod;
                    else if (TEnumType == typeof(short)) method = enumShortMemberMethod;
                    else if (TEnumType == typeof(sbyte)) method = enumSByteMemberMethod;
                    else method = enumIntMemberMethod;
                    method = method.MakeGenericMethod(type);
                }
                else
                {
                    if (type.IsGenericType)
                    {
                        var genericType = type.GetGenericTypeDefinition();
                        if (genericType == typeof(Dictionary<,>) || genericType == typeof(SortedDictionary<,>) ||
                            genericType == typeof(SortedList<,>))
                        {
                            var parameterTypes = type.GetGenericArguments();
                            method = dictionaryMemberMethod.MakeGenericMethod(type, parameterTypes[0], parameterTypes[1]);
                        }
                        else if (genericType == typeof(Nullable<>))
                        {
                            method = nullableMemberSerializeMethod.MakeGenericMethod(type.GetGenericArguments());
                        }
                        else if (genericType == typeof(KeyValuePair<,>))
                        {
                            method = memberKeyValuePairSerializeMethod.MakeGenericMethod(type.GetGenericArguments());
                        }
                    }
                    if (method == null)
                    {
                        if (type.IsValueType) method = structSerializeMethod.MakeGenericMethod(type);
                        else method = classSerializeMethod.MakeGenericMethod(type);
                    }
                }
                memberSerializers.Set(type, method);
                return method;
            }

            /// <summary>
            ///     未知类型枚举序列化委托调用函数信息
            /// </summary>
            /// <param name="type">数组类型</param>
            /// <returns>未知类型序列化委托调用函数信息</returns>
            public static MethodInfo getMemberMapSerializer(Type type)
            {
                MethodInfo method;
                if (memberMapSerializers.TryGetValue(type, out method)) return method;
                if (type.IsArray)
                {
                    var elementType = type.GetElementType();
                    if (elementType.IsValueType)
                    {
                        if (elementType.IsEnum)
                        {
                            var TEnumType = Enum.GetUnderlyingType(elementType);
                            if (TEnumType == typeof(uint)) method = enumUIntArrayMemberMapMethod;
                            else if (TEnumType == typeof(byte)) method = enumByteArrayMemberMapMethod;
                            else if (TEnumType == typeof(ulong)) method = enumULongArrayMemberMapMethod;
                            else if (TEnumType == typeof(ushort)) method = enumUShortArrayMemberMapMethod;
                            else if (TEnumType == typeof(long)) method = enumLongArrayMemberMapMethod;
                            else if (TEnumType == typeof(short)) method = enumShortArrayMemberMapMethod;
                            else if (TEnumType == typeof(sbyte)) method = enumSByteArrayMemberMapMethod;
                            else method = enumIntArrayMemberMapMethod;
                            method = method.MakeGenericMethod(elementType);
                        }
                        else if (elementType.IsGenericType &&
                                 elementType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            method =
                                nullableArrayMemberMapMethod.MakeGenericMethod(elementType.GetGenericArguments());
                        }
                        else method = structArrayMemberMapMethod.MakeGenericMethod(elementType);
                    }
                    else method = arrayMemberMapMethod.MakeGenericMethod(elementType);
                }
                else if (type.IsEnum)
                {
                    var TEnumType = Enum.GetUnderlyingType(type);
                    if (TEnumType == typeof(uint)) method = enumUIntMemberMethod;
                    else if (TEnumType == typeof(byte)) method = enumByteMemberMapMethod;
                    else if (TEnumType == typeof(ulong)) method = enumULongMemberMethod;
                    else if (TEnumType == typeof(ushort)) method = enumUShortMemberMapMethod;
                    else if (TEnumType == typeof(long)) method = enumLongMemberMethod;
                    else if (TEnumType == typeof(short)) method = enumShortMemberMapMethod;
                    else if (TEnumType == typeof(sbyte)) method = enumSByteMemberMapMethod;
                    else method = enumIntMemberMethod;
                    method = method.MakeGenericMethod(type);
                }
                else
                {
                    if (type.IsGenericType)
                    {
                        var genericType = type.GetGenericTypeDefinition();
                        if (genericType == typeof(Dictionary<,>) || genericType == typeof(SortedDictionary<,>) ||
                            genericType == typeof(SortedList<,>))
                        {
                            var parameterTypes = type.GetGenericArguments();
                            method = memberMapDictionaryMethod.MakeGenericMethod(type, parameterTypes[0],
                                parameterTypes[1]);
                        }
                        else if (genericType == typeof(Nullable<>))
                        {
                            method = memberMapNullableSerializeMethod.MakeGenericMethod(type.GetGenericArguments());
                        }
                        else if (genericType == typeof(KeyValuePair<,>))
                        {
                            method = memberKeyValuePairSerializeMethod.MakeGenericMethod(type.GetGenericArguments());
                        }
                    }
                    if (method == null)
                    {
                        if (type.IsValueType) method = structSerializeMethod.MakeGenericMethod(type);
                        else method = memberMapClassSerializeMethod.MakeGenericMethod(type);
                    }
                }
                memberMapSerializers.Set(type, method);
                return method;
            }

            /// <summary>
            ///     动态函数
            /// </summary>
            public struct TmphMemberDynamicMethod
            {
                /// <summary>
                ///     动态函数
                /// </summary>
                private readonly DynamicMethod dynamicMethod;

                /// <summary>
                /// </summary>
                private readonly ILGenerator generator;

                /// <summary>
                ///     是否值类型
                /// </summary>
                private readonly bool isValueType;

                /// <summary>
                ///     动态函数
                /// </summary>
                /// <param name="type"></param>
                /// <param name="name">成员类型</param>
                public TmphMemberDynamicMethod(Type type)
                {
                    dynamicMethod = new DynamicMethod("indexSerializer", null, new[] { typeof(TmphIndexSerializer), type },
                        type, true);
                    generator = dynamicMethod.GetILGenerator();
                    isValueType = type.IsValueType;
                }

                /// <summary>
                ///     添加字段
                /// </summary>
                /// <param name="field">字段信息</param>
                public void Push(TmphFieldInfo field)
                {
                    generator.Emit(OpCodes.Ldarg_0);
                    if (isValueType) generator.Emit(OpCodes.Ldarga_S, 1);
                    else generator.Emit(OpCodes.Ldarg_1);
                    generator.Emit(OpCodes.Ldfld, field.Field);
                    var method = getMemberSerializeMethod(field.Field.FieldType) ??
                                 getMemberSerializer(field.Field.FieldType);
                    generator.Emit(method.IsFinal || !method.IsVirtual ? OpCodes.Call : OpCodes.Callvirt, method);
                }

                /// <summary>
                ///     创建成员转换委托
                /// </summary>
                /// <returns>成员转换委托</returns>
                public Delegate Create<delegateType>()
                {
                    generator.Emit(OpCodes.Ret);
                    return dynamicMethod.CreateDelegate(typeof(delegateType));
                }
            }

            /// <summary>
            ///     动态函数
            /// </summary>
            public struct TmphMemberMapDynamicMethod
            {
                /// <summary>
                ///     动态函数
                /// </summary>
                private readonly DynamicMethod dynamicMethod;

                /// <summary>
                /// </summary>
                private readonly ILGenerator generator;

                /// <summary>
                ///     是否值类型
                /// </summary>
                private readonly bool isValueType;

                /// <summary>
                ///     动态函数
                /// </summary>
                /// <param name="type"></param>
                /// <param name="name">成员类型</param>
                public TmphMemberMapDynamicMethod(Type type)
                {
                    dynamicMethod = new DynamicMethod("indexMemberMapSerializer", null,
                        new[] { typeof(TmphMemberMap), typeof(TmphIndexSerializer), type }, type, true);
                    generator = dynamicMethod.GetILGenerator();
                    isValueType = type.IsValueType;
                }

                /// <summary>
                ///     添加字段
                /// </summary>
                /// <param name="field">字段信息</param>
                public void Push(TmphFieldInfo field, bool isFixed)
                {
                    var end = generator.DefineLabel();
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldc_I4, field.MemberIndex);
                    generator.Emit(OpCodes.Callvirt, TmphPub.MemberMapIsMemberMethod);
                    generator.Emit(OpCodes.Brfalse_S, end);

                    generator.Emit(OpCodes.Ldarg_1);
                    generator.Emit(OpCodes.Ldfld, StreamField);
                    generator.Emit(OpCodes.Ldc_I4, field.MemberIndex);
                    generator.Emit(OpCodes.Callvirt,
                        isFixed ? TmphPub.UnmanagedStreamUnsafeWriteIntMethod : TmphPub.UnmanagedStreamWriteIntMethod);

                    generator.Emit(OpCodes.Ldarg_1);
                    if (isValueType) generator.Emit(OpCodes.Ldarga_S, 2);
                    else generator.Emit(OpCodes.Ldarg_2);
                    generator.Emit(OpCodes.Ldfld, field.Field);
                    var method = getMemberMapSerializeMethod(field.Field.FieldType) ??
                                 getMemberMapSerializer(field.Field.FieldType);
                    generator.Emit(method.IsFinal || !method.IsVirtual ? OpCodes.Call : OpCodes.Callvirt, method);

                    generator.MarkLabel(end);
                }

                /// <summary>
                ///     创建成员转换委托
                /// </summary>
                /// <returns>成员转换委托</returns>
                public Delegate Create<delegateType>()
                {
                    generator.Emit(OpCodes.Ret);
                    return dynamicMethod.CreateDelegate(typeof(delegateType));
                }
            }
        }

        /// <summary>
        ///     二进制数据序列化
        /// </summary>
        /// <typeparam name="TValueType">对象类型</typeparam>
        internal static class TmphTypeSerializer<TValueType>
        {
            /// <summary>
            ///     二进制数据序列化类型配置
            /// </summary>
            private static readonly TmphIndexSerialize attribute;

            /// <summary>
            ///     序列化委托
            /// </summary>
            internal static readonly Action<TmphIndexSerializer, TValueType> DefaultSerializer;

            /// <summary>
            ///     固定分组成员序列化
            /// </summary>
            private static readonly Action<TmphIndexSerializer, TValueType> fixedMemberSerializer;

            /// <summary>
            ///     固定分组成员位图序列化
            /// </summary>
            private static readonly Action<TmphMemberMap, TmphIndexSerializer, TValueType> fixedMemberMapSerializer;

            /// <summary>
            ///     成员序列化
            /// </summary>
            private static readonly Action<TmphIndexSerializer, TValueType> memberSerializer;

            /// <summary>
            ///     成员位图序列化
            /// </summary>
            private static readonly Action<TmphMemberMap, TmphIndexSerializer, TValueType> memberMapSerializer;

            /// <summary>
            ///     JSON混合序列化位图
            /// </summary>
            private static readonly TmphMemberMap jsonMemberMap;

            /// <summary>
            ///     JSON混合序列化成员索引集合
            /// </summary>
            private static readonly int[] jsonMemberIndexs;

            /// <summary>
            ///     序列化成员数量
            /// </summary>
            private static readonly int memberCountVerify;

            /// <summary>
            ///     固定分组字节数
            /// </summary>
            private static readonly int fixedSize;

            /// <summary>
            ///     固定分组填充字节数
            /// </summary>
            private static readonly int fixedFillSize;

            /// <summary>
            ///     成员位图模式固定分组字节数
            /// </summary>
            private static readonly int memberMapFixedSize;

            static TmphTypeSerializer()
            {
                Type type = typeof(TValueType), TAttributeType;
                var methodInfo = getSerializeMethod(type);
                attribute = type.customAttribute<TmphIndexSerialize>(out TAttributeType, true) ?? TmphIndexSerialize.Default;
                if (methodInfo != null)
                {
                    var dynamicMethod = new DynamicMethod("indexSerializer", typeof(void),
                        new[] { typeof(TmphIndexSerializer), type }, true);
                    dynamicMethod.InitLocals = true;
                    var generator = dynamicMethod.GetILGenerator();
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldarg_1);
                    generator.Emit(OpCodes.Call, methodInfo);
                    generator.Emit(OpCodes.Ret);
                    DefaultSerializer =
                        (Action<TmphIndexSerializer, TValueType>)
                            dynamicMethod.CreateDelegate(typeof(Action<TmphIndexSerializer, TValueType>));
                    return;
                }
                if (type.IsArray)
                {
                    if (type.GetArrayRank() == 1)
                    {
                        var elementType = type.GetElementType();
                        if (!elementType.IsPointer)
                        {
                            if (elementType.IsValueType)
                            {
                                if (elementType.IsEnum)
                                {
                                    var TEnumType = Enum.GetUnderlyingType(elementType);
                                    if (TEnumType == typeof(uint))
                                        methodInfo = enumUIntArrayTypeMethod.MakeGenericMethod(elementType);
                                    else if (TEnumType == typeof(byte))
                                        methodInfo = enumByteArrayTypeMethod.MakeGenericMethod(elementType);
                                    else if (TEnumType == typeof(ulong))
                                        methodInfo = enumULongArrayTypeMethod.MakeGenericMethod(elementType);
                                    else if (TEnumType == typeof(ushort))
                                        methodInfo = enumUShortArrayTypeMethod.MakeGenericMethod(elementType);
                                    else if (TEnumType == typeof(long))
                                        methodInfo =
                                            enumLongArrayTypeMethod.MakeGenericMethod(elementType);
                                    else if (TEnumType == typeof(short))
                                        methodInfo =
                                            enumShortArrayTypeMethod.MakeGenericMethod(elementType);
                                    else if (TEnumType == typeof(sbyte))
                                        methodInfo =
                                            enumSByteArrayTypeMethod.MakeGenericMethod(
                                                elementType);
                                    else
                                        methodInfo =
                                            enumIntArrayTypeMethod.MakeGenericMethod(elementType);
                                }
                                else if (elementType.IsGenericType &&
                                         elementType.GetGenericTypeDefinition() == typeof(Nullable<>))
                                {
                                    methodInfo =
                                        nullableArrayTypeMethod.MakeGenericMethod(elementType.GetGenericArguments());
                                }
                                else methodInfo = structArrayTypeMethod.MakeGenericMethod(elementType);
                            }
                            else methodInfo = arrayTypeMethod.MakeGenericMethod(elementType);
                            DefaultSerializer =
                                (Action<TmphIndexSerializer, TValueType>)
                                    Delegate.CreateDelegate(typeof(Action<TmphIndexSerializer, TValueType>), methodInfo);
                            return;
                        }
                    }
                    DefaultSerializer = toNull;
                    return;
                }
                if (type.IsEnum)
                {
                    var TEnumType = Enum.GetUnderlyingType(type);
                    if (TEnumType == typeof(uint)) DefaultSerializer = enumUInt;
                    else if (TEnumType == typeof(byte)) DefaultSerializer = enumByte;
                    else if (TEnumType == typeof(ulong)) DefaultSerializer = enumULong;
                    else if (TEnumType == typeof(ushort)) DefaultSerializer = enumUShort;
                    else if (TEnumType == typeof(long)) DefaultSerializer = enumLong;
                    else if (TEnumType == typeof(short)) DefaultSerializer = enumShort;
                    else if (TEnumType == typeof(sbyte)) DefaultSerializer = enumSByte;
                    else DefaultSerializer = enumInt;
                    return;
                }
                if (type.IsPointer || type.IsAbstract || type.IsInterface)
                {
                    DefaultSerializer = toNull;
                    return;
                }
                if (type.IsGenericType)
                {
                    var genericType = type.GetGenericTypeDefinition();
                    if (genericType == typeof(TmphSubArray<>))
                    {
                        DefaultSerializer =
                            (Action<TmphIndexSerializer, TValueType>)
                                Delegate.CreateDelegate(typeof(Action<TmphIndexSerializer, TValueType>),
                                    subArraySerializeMethod.MakeGenericMethod(type.GetGenericArguments()));
                        return;
                    }
                    if (genericType == typeof(Dictionary<,>) || genericType == typeof(SortedDictionary<,>) ||
                        genericType == typeof(SortedList<,>))
                    {
                        var parameterTypes = type.GetGenericArguments();
                        DefaultSerializer =
                            (Action<TmphIndexSerializer, TValueType>)
                                Delegate.CreateDelegate(typeof(Action<TmphIndexSerializer, TValueType>),
                                    dictionarySerializeMethod.MakeGenericMethod(type, parameterTypes[0],
                                        parameterTypes[1]));
                        return;
                    }
                    if (genericType == typeof(Nullable<>))
                    {
                        DefaultSerializer =
                            (Action<TmphIndexSerializer, TValueType>)
                                Delegate.CreateDelegate(typeof(Action<TmphIndexSerializer, TValueType>),
                                    nullableSerializeMethod.MakeGenericMethod(type.GetGenericArguments()));
                        return;
                    }
                    if (genericType == typeof(KeyValuePair<,>))
                    {
                        DefaultSerializer =
                            (Action<TmphIndexSerializer, TValueType>)
                                Delegate.CreateDelegate(typeof(Action<TmphIndexSerializer, TValueType>),
                                    keyValuePairSerializeMethod.MakeGenericMethod(type.GetGenericArguments()));
                        return;
                    }
                }
                ConstructorInfo constructorInfo = null;
                Type argumentType = null;
                foreach (var interfaceType in type.GetInterfaces())
                {
                    if (interfaceType.IsGenericType)
                    {
                        var genericType = interfaceType.GetGenericTypeDefinition();
                        if (genericType == typeof(ICollection<>))
                        {
                            var parameters = interfaceType.GetGenericArguments();
                            argumentType = parameters[0];
                            parameters[0] = argumentType.MakeArrayType();
                            constructorInfo =
                                type.GetConstructor(
                                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null,
                                    parameters, null);
                            if (constructorInfo != null) break;
                            parameters[0] = typeof(IList<>).MakeGenericType(argumentType);
                            constructorInfo =
                                type.GetConstructor(
                                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null,
                                    parameters, null);
                            if (constructorInfo != null) break;
                            parameters[0] = typeof(ICollection<>).MakeGenericType(argumentType);
                            constructorInfo =
                                type.GetConstructor(
                                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null,
                                    parameters, null);
                            if (constructorInfo != null) break;
                            parameters[0] = typeof(IEnumerable<>).MakeGenericType(argumentType);
                            constructorInfo =
                                type.GetConstructor(
                                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null,
                                    parameters, null);
                            if (constructorInfo != null) break;
                        }
                        else if (genericType == typeof(IDictionary<,>))
                        {
                            constructorInfo =
                                type.GetConstructor(
                                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null,
                                    new[] { interfaceType }, null);
                            if (constructorInfo != null)
                            {
                                var parameters = interfaceType.GetGenericArguments();
                                methodInfo = dictionaryMethod.MakeGenericMethod(type, parameters[0], parameters[1]);
                                DefaultSerializer =
                                    (Action<TmphIndexSerializer, TValueType>)
                                        Delegate.CreateDelegate(typeof(Action<TmphIndexSerializer, TValueType>),
                                            methodInfo);
                                return;
                            }
                        }
                    }
                }
                if (constructorInfo != null)
                {
                    methodInfo = collectionMethod.MakeGenericMethod(argumentType, type);
                    DefaultSerializer =
                        (Action<TmphIndexSerializer, TValueType>)
                            Delegate.CreateDelegate(typeof(Action<TmphIndexSerializer, TValueType>), methodInfo);
                    return;
                }
                if (TmphConstructor<TValueType>.New == null) DefaultSerializer = toNull;
                else
                {
                    if (!type.IsValueType && attribute != TmphIndexSerialize.Default && TAttributeType != type)
                    {
                        for (var baseType = type.BaseType; baseType != typeof(object); baseType = baseType.BaseType)
                        {
                            var baseAttribute = TmphTypeAttribute.GetAttribute<TmphIndexSerialize>(baseType, false, true);
                            if (baseAttribute != null)
                            {
                                if (baseAttribute.IsBaseType)
                                {
                                    DefaultSerializer =
                                        (Action<TmphIndexSerializer, TValueType>)
                                            Delegate.CreateDelegate(typeof(Action<TmphIndexSerializer, TValueType>),
                                                baseSerializeMethod.MakeGenericMethod(baseType, type));
                                    return;
                                }
                                break;
                            }
                        }
                    }
                    var fields =
                        TmphTypeSerializer.GetFields(TmphMemberIndexGroup<TValueType>.GetFields(attribute.MemberFilter),
                            out memberCountVerify, out memberMapFixedSize);
                    fixedFillSize = -fields.FixedSize & 3;
                    fixedSize = (fields.FixedSize + (sizeof(int) + 3)) & (int.MaxValue - 3);
                    var fixedDynamicMethod = new TmphTypeSerializer.TmphMemberDynamicMethod(type);
                    var fixedMemberMapDynamicMethod = new TmphTypeSerializer.TmphMemberMapDynamicMethod(type);
                    foreach (var member in fields.FixedFields)
                    {
                        fixedDynamicMethod.Push(member);
                        fixedMemberMapDynamicMethod.Push(member, true);
                    }
                    fixedMemberSerializer =
                        (Action<TmphIndexSerializer, TValueType>)
                            fixedDynamicMethod.Create<Action<TmphIndexSerializer, TValueType>>();
                    fixedMemberMapSerializer =
                        (Action<TmphMemberMap, TmphIndexSerializer, TValueType>)
                            fixedMemberMapDynamicMethod.Create<Action<TmphMemberMap, TmphIndexSerializer, TValueType>>();

                    var dynamicMethod = new TmphTypeSerializer.TmphMemberDynamicMethod(type);
                    var memberMapDynamicMethod = new TmphTypeSerializer.TmphMemberMapDynamicMethod(type);
                    foreach (var member in fields.Fields)
                    {
                        dynamicMethod.Push(member);
                        memberMapDynamicMethod.Push(member, false);
                    }
                    memberSerializer =
                        (Action<TmphIndexSerializer, TValueType>)
                            dynamicMethod.Create<Action<TmphIndexSerializer, TValueType>>();
                    memberMapSerializer =
                        (Action<TmphMemberMap, TmphIndexSerializer, TValueType>)
                            memberMapDynamicMethod.Create<Action<TmphMemberMap, TmphIndexSerializer, TValueType>>();

                    if (fields.JsonFields.Count != 0)
                    {
                        jsonMemberMap = TmphMemberMap<TValueType>.New();
                        jsonMemberIndexs = new int[fields.JsonFields.Count];
                        var index = 0;
                        foreach (var field in fields.JsonFields)
                            jsonMemberMap.SetMember(jsonMemberIndexs[index++] = field.MemberIndex);
                    }
                }
            }

            /// <summary>
            ///     对象序列化
            /// </summary>
            /// <param name="serializer">二进制数据序列化</param>
            /// <param name="value">数据对象</param>
            internal static void Serialize(TmphIndexSerializer serializer, TValueType value)
            {
                if (DefaultSerializer == null)
                {
                    var memberMap = serializer.SerializeMemberMap<TValueType>();
                    var stream = serializer.Stream;
                    if (memberMap == null)
                    {
                        stream.PrepLength(fixedSize);
                        stream.Unsafer.Write(memberCountVerify);
                        fixedMemberSerializer(serializer, value);
                        stream.Unsafer.AddLength(fixedFillSize);
                        stream.PrepLength();
                        memberSerializer(serializer, value);
                        if (jsonMemberMap == null)
                        {
                            if (attribute.IsJson) stream.Write(0);
                        }
                        else
                        {
                            var TmphBuffer = TmphUnmanagedPool.StreamBuffers.Get();
                            try
                            {
                                using (
                                    var jsonStream = serializer.ResetJsonStream(TmphBuffer.Data,
                                        TmphUnmanagedPool.StreamBuffers.Size))
                                {
                                    TmphJsonSerializer.Serialize(value, jsonStream, stream,
                                        serializer.getJsonConfig(jsonMemberMap));
                                }
                            }
                            finally
                            {
                                TmphUnmanagedPool.StreamBuffers.Push(ref TmphBuffer);
                            }
                        }
                    }
                    else
                    {
                        stream.PrepLength(memberMapFixedSize);
                        fixedMemberMapSerializer(memberMap, serializer, value);
                        stream.PrepLength();
                        memberMapSerializer(memberMap, serializer, value);
                        if (jsonMemberMap == null ||
                            (memberMap = serializer.getJsonMemberMap<TValueType>(memberMap, jsonMemberIndexs)) == null)
                        {
                            if (attribute.IsJson) stream.Write(0);
                        }
                        else
                        {
                            var TmphBuffer = TmphUnmanagedPool.StreamBuffers.Get();
                            try
                            {
                                using (
                                    var jsonStream = serializer.ResetJsonStream(TmphBuffer.Data,
                                        TmphUnmanagedPool.StreamBuffers.Size))
                                {
                                    TmphJsonSerializer.Serialize(value, jsonStream, stream,
                                        serializer.getJsonConfig(memberMap));
                                }
                            }
                            finally
                            {
                                TmphUnmanagedPool.StreamBuffers.Push(ref TmphBuffer);
                            }
                        }
                    }
                }
                else DefaultSerializer(serializer, value);
            }

            /// <summary>
            ///     对象序列化
            /// </summary>
            /// <param name="serializer">二进制数据序列化</param>
            /// <param name="value">数据对象</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            internal static void BaseSerialize<childType>(TmphIndexSerializer serializer, childType value)
                where childType : TValueType
            {
                Serialize(serializer, value);
            }

            /// <summary>
            ///     不支持对象转换null
            /// </summary>
            /// <param name="serializer">二进制数据序列化</param>
            /// <param name="value">数据对象</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private static void toNull(TmphIndexSerializer serializer, TValueType value)
            {
                serializer.Stream.Write(NullValue);
            }

            /// <summary>
            ///     枚举值序列化
            /// </summary>
            /// <param name="serializer">二进制数据序列化</param>
            /// <param name="array">枚举值序列化</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private static void enumByte(TmphIndexSerializer serializer, TValueType value)
            {
                serializer.Stream.Write((uint)TmphPub.TmphEnumCast<TValueType, byte>.ToInt(value));
            }

            /// <summary>
            ///     枚举值序列化
            /// </summary>
            /// <param name="serializer">二进制数据序列化</param>
            /// <param name="array">枚举值序列化</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private static void enumSByte(TmphIndexSerializer serializer, TValueType value)
            {
                serializer.Stream.Write((int)TmphPub.TmphEnumCast<TValueType, sbyte>.ToInt(value));
            }

            /// <summary>
            ///     枚举值序列化
            /// </summary>
            /// <param name="serializer">二进制数据序列化</param>
            /// <param name="array">枚举值序列化</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private static void enumShort(TmphIndexSerializer serializer, TValueType value)
            {
                serializer.Stream.Write((int)TmphPub.TmphEnumCast<TValueType, short>.ToInt(value));
            }

            /// <summary>
            ///     枚举值序列化
            /// </summary>
            /// <param name="serializer">二进制数据序列化</param>
            /// <param name="array">枚举值序列化</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private static void enumUShort(TmphIndexSerializer serializer, TValueType value)
            {
                serializer.Stream.Write((uint)TmphPub.TmphEnumCast<TValueType, ushort>.ToInt(value));
            }

            /// <summary>
            ///     枚举值序列化
            /// </summary>
            /// <param name="serializer">二进制数据序列化</param>
            /// <param name="array">枚举值序列化</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private static void enumInt(TmphIndexSerializer serializer, TValueType value)
            {
                serializer.Stream.Write(TmphPub.TmphEnumCast<TValueType, int>.ToInt(value));
            }

            /// <summary>
            ///     枚举值序列化
            /// </summary>
            /// <param name="serializer">二进制数据序列化</param>
            /// <param name="array">枚举值序列化</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private static void enumUInt(TmphIndexSerializer serializer, TValueType value)
            {
                serializer.Stream.Write(TmphPub.TmphEnumCast<TValueType, uint>.ToInt(value));
            }

            /// <summary>
            ///     枚举值序列化
            /// </summary>
            /// <param name="serializer">二进制数据序列化</param>
            /// <param name="array">枚举值序列化</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private static void enumLong(TmphIndexSerializer serializer, TValueType value)
            {
                serializer.Stream.Write(TmphPub.TmphEnumCast<TValueType, long>.ToInt(value));
            }

            /// <summary>
            ///     枚举值序列化
            /// </summary>
            /// <param name="serializer">二进制数据序列化</param>
            /// <param name="array">枚举值序列化</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private static void enumULong(TmphIndexSerializer serializer, TValueType value)
            {
                serializer.Stream.Write(TmphPub.TmphEnumCast<TValueType, ulong>.ToInt(value));
            }

            /// <summary>
            ///     获取字段成员集合
            /// </summary>
            /// <returns>字段成员集合</returns>
            public static TmphSubArray<TmphMemberIndex> GetMembers()
            {
                if (fixedMemberSerializer == null) return default(TmphSubArray<TmphMemberIndex>);
                return TmphTypeSerializer.GetMembers(TmphMemberIndexGroup<TValueType>.GetFields(attribute.MemberFilter));
            }
        }
    }
}