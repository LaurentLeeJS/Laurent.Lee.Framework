/*
-------------------------------------------------- -----------------------------------------
The frame content is protected by copyright law. In order to facilitate individual learning,
allows to download the program source information, but does not allow individuals or a third
party for profit, the commercial use of the source information. Without consent,
does not allow any form (even if partial, or modified) database storage,
copy the source of information. If the source content provided by third parties,
which corresponds to the third party content is also protected by copyright.

If you are found to have infringed copyright behavior, please give me a hint. THX!

Here in particular it emphasized that the third party is not allowed to contact addresses
published in this "version copyright statement" to send advertising material.
I will take legal means to resist sending spam.
-------------------------------------------------- ----------------------------------------
The framework under the GNU agreement, Detail View GNU License.
If you think about this item affection join the development team,
Please contact me: LaurentLeeJS@gmail.com
-------------------------------------------------- ----------------------------------------
Laurent.Lee.Framework Coded by Laurent Lee
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
    ///     二进制数据序列化
    /// </summary>
    public sealed unsafe class TmphDataSerializer : TmphBinarySerializer
    {
        /// <summary>
        ///     真实类型
        /// </summary>
        public const int RealTypeValue = NullValue + 1;

        /// <summary>
        ///     对象序列化函数信息
        /// </summary>
        private static readonly MethodInfo structSerializeMethod = typeof(TmphDataSerializer).GetMethod(
            "structSerialize", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     字典序列化函数信息
        /// </summary>
        private static readonly MethodInfo dictionarySerializeMethod =
            typeof(TmphDataSerializer).GetMethod("dictionarySerialize", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     字典序列化函数信息
        /// </summary>
        private static readonly MethodInfo dictionaryMemberMethod =
            typeof(TmphDataSerializer).GetMethod("dictionaryMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     字典序列化函数信息
        /// </summary>
        private static readonly MethodInfo keyValuePairSerializeMethod =
            typeof(TmphDataSerializer).GetMethod("keyValuePairSerialize", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     数组序列化函数信息
        /// </summary>
        private static readonly MethodInfo subArraySerializeMethod =
            typeof(TmphDataSerializer).GetMethod("subArraySerialize", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     对象序列化函数信息
        /// </summary>
        private static readonly MethodInfo nullableSerializeMethod =
            typeof(TmphDataSerializer).GetMethod("nullableSerialize", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     对象序列化函数信息
        /// </summary>
        private static readonly MethodInfo nullableMemberSerializeMethod =
            typeof(TmphDataSerializer).GetMethod("nullableMemberSerialize", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     对象序列化函数信息
        /// </summary>
        private static readonly MethodInfo baseSerializeMethod = typeof(TmphDataSerializer).GetMethod("baseSerialize",
            BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     真实类型序列化函数信息
        /// </summary>
        private static readonly MethodInfo realTypeObjectMethod = typeof(TmphDataSerializer).GetMethod("realTypeObject",
            BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     集合序列化函数信息
        /// </summary>
        private static readonly MethodInfo structCollectionMethod =
            typeof(TmphDataSerializer).GetMethod("structCollection", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     集合序列化函数信息
        /// </summary>
        private static readonly MethodInfo classCollectionMethod = typeof(TmphDataSerializer).GetMethod(
            "classCollection", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     集合序列化函数信息
        /// </summary>
        private static readonly MethodInfo structDictionaryMethod =
            typeof(TmphDataSerializer).GetMethod("structDictionary", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     集合序列化函数信息
        /// </summary>
        private static readonly MethodInfo classDictionaryMethod = typeof(TmphDataSerializer).GetMethod(
            "classDictionary", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     集合序列化函数信息
        /// </summary>
        private static readonly MethodInfo structEnumByteCollectionMethod =
            typeof(TmphDataSerializer).GetMethod("structEnumByteCollection",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     集合序列化函数信息
        /// </summary>
        private static readonly MethodInfo classEnumByteCollectionMethod =
            typeof(TmphDataSerializer).GetMethod("classEnumByteCollection",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     集合序列化函数信息
        /// </summary>
        private static readonly MethodInfo structEnumSByteCollectionMethod =
            typeof(TmphDataSerializer).GetMethod("structEnumSByteCollection",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     集合序列化函数信息
        /// </summary>
        private static readonly MethodInfo classEnumSByteCollectionMethod =
            typeof(TmphDataSerializer).GetMethod("classEnumSByteCollection",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     集合序列化函数信息
        /// </summary>
        private static readonly MethodInfo structEnumShortCollectionMethod =
            typeof(TmphDataSerializer).GetMethod("structEnumShortCollection",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     集合序列化函数信息
        /// </summary>
        private static readonly MethodInfo classEnumShortCollectionMethod =
            typeof(TmphDataSerializer).GetMethod("classEnumShortCollection",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     集合序列化函数信息
        /// </summary>
        private static readonly MethodInfo structEnumUShortCollectionMethod =
            typeof(TmphDataSerializer).GetMethod("structEnumUShortCollection",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     集合序列化函数信息
        /// </summary>
        private static readonly MethodInfo classEnumUShortCollectionMethod =
            typeof(TmphDataSerializer).GetMethod("classEnumUShortCollection",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     集合序列化函数信息
        /// </summary>
        private static readonly MethodInfo structEnumIntCollectionMethod =
            typeof(TmphDataSerializer).GetMethod("structEnumIntCollection",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     集合序列化函数信息
        /// </summary>
        private static readonly MethodInfo classEnumIntCollectionMethod =
            typeof(TmphDataSerializer).GetMethod("classEnumIntCollection", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     集合序列化函数信息
        /// </summary>
        private static readonly MethodInfo structEnumUIntCollectionMethod =
            typeof(TmphDataSerializer).GetMethod("structEnumUIntCollection",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     集合序列化函数信息
        /// </summary>
        private static readonly MethodInfo classEnumUIntCollectionMethod =
            typeof(TmphDataSerializer).GetMethod("classEnumUIntCollection",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     集合序列化函数信息
        /// </summary>
        private static readonly MethodInfo structEnumLongCollectionMethod =
            typeof(TmphDataSerializer).GetMethod("structEnumLongCollection",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     集合序列化函数信息
        /// </summary>
        private static readonly MethodInfo classEnumLongCollectionMethod =
            typeof(TmphDataSerializer).GetMethod("classEnumLongCollection",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     集合序列化函数信息
        /// </summary>
        private static readonly MethodInfo structEnumULongCollectionMethod =
            typeof(TmphDataSerializer).GetMethod("structEnumULongCollection",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     集合序列化函数信息
        /// </summary>
        private static readonly MethodInfo classEnumULongCollectionMethod =
            typeof(TmphDataSerializer).GetMethod("classEnumULongCollection",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     枚举转换函数信息
        /// </summary>
        private static readonly MethodInfo enumByteMemberMethod = typeof(TmphDataSerializer).GetMethod("enumByteMember",
            BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     枚举转换函数信息
        /// </summary>
        private static readonly MethodInfo enumSByteMemberMethod = typeof(TmphDataSerializer).GetMethod(
            "enumSByteMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     枚举转换函数信息
        /// </summary>
        private static readonly MethodInfo enumShortMemberMethod = typeof(TmphDataSerializer).GetMethod(
            "enumShortMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     枚举转换函数信息
        /// </summary>
        private static readonly MethodInfo enumUShortMemberMethod =
            typeof(TmphDataSerializer).GetMethod("enumUShortMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     枚举转换函数信息
        /// </summary>
        private static readonly MethodInfo enumIntMemberMethod = typeof(TmphDataSerializer).GetMethod("enumIntMember",
            BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     枚举转换函数信息
        /// </summary>
        private static readonly MethodInfo enumUIntMemberMethod = typeof(TmphDataSerializer).GetMethod("enumUIntMember",
            BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     枚举转换函数信息
        /// </summary>
        private static readonly MethodInfo enumLongMemberMethod = typeof(TmphDataSerializer).GetMethod("enumLongMember",
            BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     枚举转换函数信息
        /// </summary>
        private static readonly MethodInfo enumULongMemberMethod = typeof(TmphDataSerializer).GetMethod(
            "enumULongMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组转换函数信息
        /// </summary>
        private static readonly MethodInfo enumByteArrayMethod = typeof(TmphDataSerializer).GetMethod("enumByteArray",
            BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组转换函数信息
        /// </summary>
        private static readonly MethodInfo enumByteArrayMemberMethod =
            typeof(TmphDataSerializer).GetMethod("enumByteArrayMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组转换函数信息
        /// </summary>
        private static readonly MethodInfo enumSByteArrayMethod = typeof(TmphDataSerializer).GetMethod("enumSByteArray",
            BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组转换函数信息
        /// </summary>
        private static readonly MethodInfo enumSByteArrayMemberMethod =
            typeof(TmphDataSerializer).GetMethod("enumSByteArrayMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组转换函数信息
        /// </summary>
        private static readonly MethodInfo enumShortArrayMethod = typeof(TmphDataSerializer).GetMethod("enumShortArray",
            BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组转换函数信息
        /// </summary>
        private static readonly MethodInfo enumShortArrayMemberMethod =
            typeof(TmphDataSerializer).GetMethod("enumShortArrayMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组转换函数信息
        /// </summary>
        private static readonly MethodInfo enumUShortArrayMethod = typeof(TmphDataSerializer).GetMethod(
            "enumUShortArray", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组转换函数信息
        /// </summary>
        private static readonly MethodInfo enumUShortArrayMemberMethod =
            typeof(TmphDataSerializer).GetMethod("enumUShortArrayMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组转换函数信息
        /// </summary>
        private static readonly MethodInfo enumIntArrayMethod = typeof(TmphDataSerializer).GetMethod("enumIntArray",
            BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组转换函数信息
        /// </summary>
        private static readonly MethodInfo enumIntArrayMemberMethod =
            typeof(TmphDataSerializer).GetMethod("enumIntArrayMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组转换函数信息
        /// </summary>
        private static readonly MethodInfo enumUIntArrayMethod = typeof(TmphDataSerializer).GetMethod("enumUIntArray",
            BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组转换函数信息
        /// </summary>
        private static readonly MethodInfo enumUIntArrayMemberMethod =
            typeof(TmphDataSerializer).GetMethod("enumUIntArrayMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组转换函数信息
        /// </summary>
        private static readonly MethodInfo enumLongArrayMethod = typeof(TmphDataSerializer).GetMethod("enumLongArray",
            BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组转换函数信息
        /// </summary>
        private static readonly MethodInfo enumLongArrayMemberMethod =
            typeof(TmphDataSerializer).GetMethod("enumLongArrayMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组转换函数信息
        /// </summary>
        private static readonly MethodInfo enumULongArrayMethod = typeof(TmphDataSerializer).GetMethod("enumULongArray",
            BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组转换函数信息
        /// </summary>
        private static readonly MethodInfo enumULongArrayMemberMethod =
            typeof(TmphDataSerializer).GetMethod("enumULongArrayMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组转换函数信息
        /// </summary>
        private static readonly MethodInfo nullableArrayMethod = typeof(TmphDataSerializer).GetMethod("nullableArray",
            BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组转换函数信息
        /// </summary>
        private static readonly MethodInfo nullableArrayMemberMethod =
            typeof(TmphDataSerializer).GetMethod("nullableArrayMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组转换函数信息
        /// </summary>
        private static readonly MethodInfo structArrayMethod = typeof(TmphDataSerializer).GetMethod("structArray",
            BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组转换函数信息
        /// </summary>
        private static readonly MethodInfo structArrayMemberMethod =
            typeof(TmphDataSerializer).GetMethod("structArrayMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组转换函数信息
        /// </summary>
        private static readonly MethodInfo arrayMethod = typeof(TmphDataSerializer).GetMethod("array",
            BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组转换函数信息
        /// </summary>
        private static readonly MethodInfo arrayMemberMethod = typeof(TmphDataSerializer).GetMethod("arrayMember",
            BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     序列化接口函数信息
        /// </summary>
        private static readonly MethodInfo structISerializeMethod =
            typeof(TmphDataSerializer).GetMethod("structISerialize", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     序列化接口函数信息
        /// </summary>
        private static readonly MethodInfo classISerializeMethod = typeof(TmphDataSerializer).GetMethod(
            "classISerialize", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     序列化接口函数信息
        /// </summary>
        private static readonly MethodInfo memberClassISerializeMethod =
            typeof(TmphDataSerializer).GetMethod("memberClassISerialize", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     引用类型成员序列化函数信息
        /// </summary>
        private static readonly MethodInfo memberClassSerializeMethod =
            typeof(TmphDataSerializer).GetMethod("MemberClassSerialize", BindingFlags.Instance | BindingFlags.Public);

        /// <summary>
        ///     公共默认配置参数
        /// </summary>
        private static readonly TmphConfig defaultConfig = new TmphConfig();

        /// <summary>
        ///     未知类型对象序列化
        /// </summary>
        private static TmphInterlocked.TmphDictionary<Type, Func<object, TmphConfig, byte[]>> objectSerializes =
            new TmphInterlocked.TmphDictionary<Type, Func<object, TmphConfig, byte[]>>(
                TmphDictionary.CreateOnly<Type, Func<object, TmphConfig, byte[]>>());

        /// <summary>
        ///     未知类型对象序列化
        /// </summary>
        private static readonly MethodInfo objectSerializeMethod = typeof(TmphDataSerializer).GetMethod(
            "objectSerialize", BindingFlags.Static | BindingFlags.NonPublic, null,
            new[] { typeof(object), typeof(TmphConfig) }, null);

        /// <summary>
        ///     序列化数据流字段信息
        /// </summary>
        private static readonly FieldInfo serializeStreamField = typeof(TmphDataSerializer).GetField("Stream",
            BindingFlags.Instance | BindingFlags.NonPublic);

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

        /// <summary>
        ///     是否检测数组引用
        /// </summary>
        private bool isReferenceArray;

        /// <summary>
        ///     是否支持循环引用处理
        /// </summary>
        private bool isReferenceMember;

        /// <summary>
        ///     历史对象指针位置
        /// </summary>
        private Dictionary<TmphObjectReference, int> points;

        /// <summary>
        ///     序列化配置参数
        /// </summary>
        private TmphConfig serializeConfig;

        static TmphDataSerializer()
        {
            serializeMethods = TmphDictionary.CreateOnly<Type, MethodInfo>();
            memberSerializeMethods = TmphDictionary.CreateOnly<Type, MethodInfo>();
            memberMapSerializeMethods = TmphDictionary.CreateOnly<Type, MethodInfo>();
            foreach (var method in typeof(TmphDataSerializer).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic))
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
            binarySerializerConfig = serializeConfig = TmphConfig;
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
            binarySerializerConfig = serializeConfig = TmphConfig;
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
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize<TValueType>(TValueType value)
        {
            isReferenceMember = TmphTypeSerializer<TValueType>.IsReferenceMember;
            if (points == null && isReferenceMember) points = TmphDictionary<TmphObjectReference>.Create<int>();
            isReferenceArray = true;
            memberMap = serializeConfig.MemberMap;
            streamStartIndex = Stream.OffsetLength;
            Stream.Write(serializeConfig.HeaderValue);
            TmphTypeSerializer<TValueType>.Serialize(this, value);
            Stream.Write(Stream.OffsetLength - streamStartIndex);
        }

        /// <summary>
        ///     对象序列化
        /// </summary>
        /// <typeparam name="TValueType">目标数据类型</typeparam>
        /// <param name="value">数据对象</param>
        /// <param name="TmphConfig">配置参数</param>
        /// <returns>序列化数据</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private byte[] codeSerialize<TValueType>(TValueType value, TmphConfig TmphConfig)
            where TValueType : Code.CSharp.TmphDataSerialize.ISerialize
        {
            binarySerializerConfig = serializeConfig = TmphConfig;
            var TmphBuffer = TmphUnmanagedPool.StreamBuffers.Get();
            try
            {
                Stream.Reset(TmphBuffer.Byte, TmphUnmanagedPool.StreamBuffers.Size);
                using (Stream)
                {
                    codeSerialize(value);
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
        private void codeSerialize<TValueType>(TValueType value, TmphUnmanagedStream stream, TmphConfig TmphConfig)
            where TValueType : Code.CSharp.TmphDataSerialize.ISerialize
        {
            binarySerializerConfig = serializeConfig = TmphConfig;
            Stream.From(stream);
            try
            {
                codeSerialize(value);
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
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void codeSerialize<TValueType>(TValueType value) where TValueType : Code.CSharp.TmphDataSerialize.ISerialize
        {
            isReferenceMember = TmphTypeSerializer<TValueType>.IsReferenceMember;
            if (points == null && isReferenceMember) points = TmphDictionary<TmphObjectReference>.Create<int>();
            isReferenceArray = true;
            memberMap = serializeConfig.MemberMap;
            streamStartIndex = Stream.OffsetLength;
            Stream.Write(serializeConfig.HeaderValue);
            value.Serialize(this);
            Stream.Write(Stream.OffsetLength - streamStartIndex);
        }

        /// <summary>
        ///     释放资源
        /// </summary>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private new void free()
        {
            base.free();
            if (points != null) points.Clear();
            TmphTypePool<TmphDataSerializer>.Push(this);
        }

        /// <summary>
        ///     添加历史对象
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public bool CheckPoint<TValueType>(TValueType value)
        {
            if (isReferenceMember)
            {
                int point;
                if (points.TryGetValue(new TmphObjectReference { Value = value }, out point))
                {
                    Stream.Write(-point);
                    return false;
                }
                points[new TmphObjectReference { Value = value }] = Stream.OffsetLength - streamStartIndex;
            }
            return true;
        }

        /// <summary>
        ///     添加历史对象
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private bool checkPoint<TValueType>(TValueType[] value)
        {
            if (value.Length == 0)
            {
                Stream.Write(0);
                isReferenceArray = true;
                return false;
            }
            if (isReferenceArray) return CheckPoint(value);
            return isReferenceArray = true;
        }

        /// <summary>
        ///     判断成员索引是否有效
        /// </summary>
        /// <param name="memberIndex"></param>
        /// <returns></returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public bool IsMemberMap(int memberIndex)
        {
            return currentMemberMap.IsMember(memberIndex);
        }

        /// <summary>
        ///     逻辑值序列化
        /// </summary>
        /// <param name="value">逻辑值</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(bool[] value)
        {
            if (checkPoint(value)) Serialize(Stream, value);
        }

        /// <summary>
        ///     逻辑值序列化
        /// </summary>
        /// <param name="value">逻辑值</param>
        [TmphMemberSerializeMethod]
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(bool[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }

        /// <summary>
        ///     逻辑值序列化
        /// </summary>
        /// <param name="value">逻辑值</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(bool?[] value)
        {
            if (checkPoint(value)) Serialize(Stream, value);
        }

        /// <summary>
        ///     逻辑值序列化
        /// </summary>
        /// <param name="value">逻辑值</param>
        [TmphMemberSerializeMethod]
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(bool?[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(byte[] value)
        {
            if (checkPoint(value)) Serialize(Stream, value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberSerializeMethod]
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(byte[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(byte?[] value)
        {
            if (checkPoint(value)) Serialize(Stream, value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberSerializeMethod]
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(byte?[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(sbyte[] value)
        {
            if (checkPoint(value)) Serialize(Stream, value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberSerializeMethod]
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(sbyte[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(sbyte?[] value)
        {
            if (checkPoint(value)) Serialize(Stream, value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberSerializeMethod]
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(sbyte?[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(short[] value)
        {
            if (checkPoint(value)) Serialize(Stream, value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberSerializeMethod]
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(short[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(short?[] value)
        {
            if (checkPoint(value)) Serialize(Stream, value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberSerializeMethod]
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(short?[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(ushort[] value)
        {
            if (checkPoint(value)) Serialize(Stream, value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberSerializeMethod]
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(ushort[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(ushort?[] value)
        {
            if (checkPoint(value)) Serialize(Stream, value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberSerializeMethod]
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(ushort?[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(int[] value)
        {
            if (checkPoint(value)) Serialize(Stream, value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberSerializeMethod]
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(int[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(int?[] value)
        {
            if (checkPoint(value)) Serialize(Stream, value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberSerializeMethod]
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(int?[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(uint[] value)
        {
            if (checkPoint(value)) Serialize(Stream, value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberSerializeMethod]
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(uint[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(uint?[] value)
        {
            if (checkPoint(value)) Serialize(Stream, value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberSerializeMethod]
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(uint?[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
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
            if (checkPoint(value)) Serialize(Stream, value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberSerializeMethod]
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(long[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(long?[] value)
        {
            if (checkPoint(value)) Serialize(Stream, value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberSerializeMethod]
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(long?[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(ulong[] value)
        {
            if (checkPoint(value)) Serialize(Stream, value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberSerializeMethod]
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(ulong[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(ulong?[] value)
        {
            if (checkPoint(value)) Serialize(Stream, value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberSerializeMethod]
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(ulong?[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(float[] value)
        {
            if (checkPoint(value)) Serialize(Stream, value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberSerializeMethod]
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(float[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(float?[] value)
        {
            if (checkPoint(value)) Serialize(Stream, value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberSerializeMethod]
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(float?[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(double[] value)
        {
            if (checkPoint(value)) Serialize(Stream, value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberSerializeMethod]
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(double[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(double?[] value)
        {
            if (checkPoint(value)) Serialize(Stream, value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberSerializeMethod]
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(double?[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(decimal[] value)
        {
            if (checkPoint(value)) Serialize(Stream, value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberSerializeMethod]
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(decimal[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(decimal?[] value)
        {
            if (checkPoint(value)) Serialize(Stream, value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberSerializeMethod]
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(decimal?[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }

        /// <summary>
        ///     字符序列化
        /// </summary>
        /// <param name="value">字符</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(char[] value)
        {
            if (checkPoint(value)) Serialize(Stream, value);
        }

        /// <summary>
        ///     字符序列化
        /// </summary>
        /// <param name="value">字符</param>
        [TmphMemberSerializeMethod]
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(char[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }

        /// <summary>
        ///     字符序列化
        /// </summary>
        /// <param name="value">字符</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(char?[] value)
        {
            if (checkPoint(value)) Serialize(Stream, value);
        }

        /// <summary>
        ///     字符序列化
        /// </summary>
        /// <param name="value">字符</param>
        [TmphMemberSerializeMethod]
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(char?[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }

        /// <summary>
        ///     时间序列化
        /// </summary>
        /// <param name="value">时间</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(DateTime[] value)
        {
            if (checkPoint(value)) Serialize(Stream, value);
        }

        /// <summary>
        ///     时间序列化
        /// </summary>
        /// <param name="value">时间</param>
        [TmphMemberSerializeMethod]
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(DateTime[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }

        /// <summary>
        ///     时间序列化
        /// </summary>
        /// <param name="value">时间</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(DateTime?[] value)
        {
            if (checkPoint(value)) Serialize(Stream, value);
        }

        /// <summary>
        ///     时间序列化
        /// </summary>
        /// <param name="value">时间</param>
        [TmphMemberSerializeMethod]
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(DateTime?[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }

        /// <summary>
        ///     Guid序列化
        /// </summary>
        /// <param name="value">Guid</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(Guid[] value)
        {
            if (checkPoint(value)) Serialize(Stream, value);
        }

        /// <summary>
        ///     Guid序列化
        /// </summary>
        /// <param name="value">Guid</param>
        [TmphMemberSerializeMethod]
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(Guid[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }

        /// <summary>
        ///     Guid序列化
        /// </summary>
        /// <param name="value">Guid</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(Guid?[] value)
        {
            if (checkPoint(value)) Serialize(Stream, value);
        }

        /// <summary>
        ///     Guid序列化
        /// </summary>
        /// <param name="value">Guid</param>
        [TmphMemberSerializeMethod]
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(Guid?[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
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
            else if (CheckPoint(value)) Serialize(Stream, value);
        }

        /// <summary>
        ///     字符串序列化
        /// </summary>
        /// <param name="value">字符串</param>
        [TmphMemberSerializeMethod]
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(string value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }

        /// <summary>
        ///     字符串序列化
        /// </summary>
        /// <param name="array">字符串数组</param>
        [TmphMemberSerializeMethod]
        [TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberSerialize(string[] array)
        {
            if (array == null) Stream.Write(NullValue);
            else serialize(array);
        }

        /// <summary>
        ///     字符串序列化
        /// </summary>
        /// <param name="array">字符串数组</param>
        [TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void serialize(string[] array)
        {
            if (checkPoint(array))
            {
                var arrayMap = new TmphArrayMap(Stream, array.Length, array.Length);
                foreach (var value in array) arrayMap.Next(value != null);
                arrayMap.End(Stream);
                foreach (var value in array)
                {
                    if (value != null)
                    {
                        if (value.Length == 0) Stream.Write(0);
                        else if (CheckPoint(value)) Serialize(Stream, value);
                    }
                }
            }
        }

        /// <summary>
        ///     对象序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="value">数据对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private static void structSerialize<TValueType>(TmphDataSerializer serializer, TValueType value)
            where TValueType : struct
        {
            TmphTypeSerializer<TValueType>.StructSerialize(serializer, value);
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
            if (CheckPoint(value))
            {
                var index = 0;
                var keys = new TKeyType[value.Count];
                var values = new TValueType[keys.Length];
                foreach (var keyValue in value)
                {
                    keys[index] = keyValue.Key;
                    values[index++] = keyValue.Value;
                }
                isReferenceArray = false;
                TmphTypeSerializer<TKeyType[]>.DefaultSerializer(this, keys);
                isReferenceArray = false;
                TmphTypeSerializer<TValueType[]>.DefaultSerializer(this, values);
            }
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
        private static void keyValuePairSerialize<TKeyType, TValueType>(TmphDataSerializer serializer,
            KeyValuePair<TKeyType, TValueType> value)
        {
            TmphTypeSerializer<TmphKeyValue<TKeyType, TValueType>>.MemberSerialize(serializer,
                new TmphKeyValue<TKeyType, TValueType>(value.Key, value.Value));
        }

        /// <summary>
        ///     数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="value">数据对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void subArraySerialize<TValueType>(TmphSubArray<TValueType> value)
        {
            var array = value.ToArray();
            isReferenceArray = false;
            TmphTypeSerializer<TValueType[]>.DefaultSerializer(this, array);
        }

        /// <summary>
        ///     对象序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="value">数据对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private static void nullableSerialize<TValueType>(TmphDataSerializer serializer, TValueType? value)
            where TValueType : struct
        {
            TmphTypeSerializer<TValueType>.StructSerialize(serializer, value.Value);
        }

        /// <summary>
        ///     对象序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="value">数据对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private static void nullableMemberSerialize<TValueType>(TmphDataSerializer serializer, TValueType? value)
            where TValueType : struct
        {
            if (value.HasValue) TmphTypeSerializer<TValueType>.StructSerialize(serializer, value.Value);
            else serializer.Stream.Write(NullValue);
        }

        /// <summary>
        ///     对象序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="value">数据对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private static void baseSerialize<TValueType, childType>(TmphDataSerializer serializer, childType value)
            where childType : TValueType
        {
            TmphTypeSerializer<TValueType>.BaseSerialize(serializer, value);
        }

        /// <summary>
        ///     真实类型序列化
        /// </summary>
        /// <param name="serializer"></param>
        /// <param name="value"></param>
        private static void realTypeObject<TValueType>(TmphDataSerializer serializer, object value)
        {
            TmphTypeSerializer<TValueType>.RealTypeObject(serializer, value);
        }

        /// <summary>
        ///     集合转换
        /// </summary>
        /// <param name="collection">对象集合</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void structCollection<TValueType, collectionType>(collectionType collection)
            where collectionType : ICollection<TValueType>
        {
            isReferenceArray = false;
            TmphTypeSerializer<TValueType[]>.DefaultSerializer(this, collection.GetArray());
        }

        /// <summary>
        ///     集合转换
        /// </summary>
        /// <param name="collection">对象集合</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void classCollection<TValueType, collectionType>(collectionType collection)
            where collectionType : ICollection<TValueType>
        {
            if (CheckPoint(collection)) structCollection<TValueType, collectionType>(collection);
        }

        /// <summary>
        ///     集合转换
        /// </summary>
        /// <param name="collection">对象集合</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void structDictionary<dictionaryType, TKeyType, TValueType>(dictionaryType dictionary)
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
            isReferenceArray = false;
            TmphTypeSerializer<TKeyType[]>.DefaultSerializer(this, keys);
            isReferenceArray = false;
            TmphTypeSerializer<TValueType[]>.DefaultSerializer(this, values);
        }

        /// <summary>
        ///     集合转换
        /// </summary>
        /// <param name="collection">对象集合</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void classDictionary<dictionaryType, TKeyType, TValueType>(dictionaryType dictionary)
            where dictionaryType : IDictionary<TKeyType, TValueType>
        {
            if (CheckPoint(dictionary)) structDictionary<dictionaryType, TKeyType, TValueType>(dictionary);
        }

        /// <summary>
        ///     枚举集合序列化
        /// </summary>
        /// <param name="collection">枚举集合序列化</param>
        private void structEnumByteCollection<TValueType, collectionType>(collectionType collection)
            where collectionType : ICollection<TValueType>
        {
            int count = collection.Count, length = (count + 7) & (int.MaxValue - 3);
            Stream.PrepLength(length);
            var write = Stream.CurrentData;
            *(int*)write = count;
            write += sizeof(int);
            foreach (var value in collection) *write++ = TmphPub.TmphEnumCast<TValueType, byte>.ToInt(value);
            Stream.Unsafer.AddLength(length);
            Stream.PrepLength();
        }

        /// <summary>
        ///     枚举集合序列化
        /// </summary>
        /// <param name="collection">枚举集合序列化</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void classEnumByteCollection<TValueType, collectionType>(collectionType collection)
            where collectionType : ICollection<TValueType>
        {
            if (CheckPoint(collection)) structEnumByteCollection<TValueType, collectionType>(collection);
        }

        /// <summary>
        ///     枚举集合序列化
        /// </summary>
        /// <param name="collection">枚举集合序列化</param>
        private void structEnumSByteCollection<TValueType, collectionType>(collectionType collection)
            where collectionType : ICollection<TValueType>
        {
            int count = collection.Count, length = (count + 7) & (int.MaxValue - 3);
            Stream.PrepLength(length);
            var write = Stream.CurrentData;
            *(int*)write = count;
            write += sizeof(int);
            foreach (var value in collection) *(sbyte*)write++ = TmphPub.TmphEnumCast<TValueType, sbyte>.ToInt(value);
            Stream.Unsafer.AddLength(length);
            Stream.PrepLength();
        }

        /// <summary>
        ///     枚举集合序列化
        /// </summary>
        /// <param name="collection">枚举集合序列化</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void classEnumSByteCollection<TValueType, collectionType>(collectionType collection)
            where collectionType : ICollection<TValueType>
        {
            if (CheckPoint(collection)) structEnumSByteCollection<TValueType, collectionType>(collection);
        }

        /// <summary>
        ///     枚举集合序列化
        /// </summary>
        /// <param name="collection">枚举集合序列化</param>
        private void structEnumShortCollection<TValueType, collectionType>(collectionType collection)
            where collectionType : ICollection<TValueType>
        {
            int count = collection.Count, length = ((count * sizeof(short)) + 7) & (int.MaxValue - 3);
            Stream.PrepLength(length);
            var write = Stream.CurrentData;
            *(int*)write = count;
            write += sizeof(int);
            foreach (var value in collection)
            {
                *(short*)write = TmphPub.TmphEnumCast<TValueType, short>.ToInt(value);
                write += sizeof(short);
            }
            Stream.Unsafer.AddLength(length);
            Stream.PrepLength();
        }

        /// <summary>
        ///     枚举集合序列化
        /// </summary>
        /// <param name="collection">枚举集合序列化</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void classEnumShortCollection<TValueType, collectionType>(collectionType collection)
            where collectionType : ICollection<TValueType>
        {
            if (CheckPoint(collection)) structEnumShortCollection<TValueType, collectionType>(collection);
        }

        /// <summary>
        ///     枚举集合序列化
        /// </summary>
        /// <param name="collection">枚举集合序列化</param>
        private void structEnumUShortCollection<TValueType, collectionType>(collectionType collection)
            where collectionType : ICollection<TValueType>
        {
            int count = collection.Count, length = ((count * sizeof(ushort)) + 7) & (int.MaxValue - 3);
            Stream.PrepLength(length);
            var write = Stream.CurrentData;
            *(int*)write = count;
            write += sizeof(int);
            foreach (var value in collection)
            {
                *(ushort*)write = TmphPub.TmphEnumCast<TValueType, ushort>.ToInt(value);
                write += sizeof(ushort);
            }
            Stream.Unsafer.AddLength(length);
            Stream.PrepLength();
        }

        /// <summary>
        ///     枚举集合序列化
        /// </summary>
        /// <param name="collection">枚举集合序列化</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void classEnumUShortCollection<TValueType, collectionType>(collectionType collection)
            where collectionType : ICollection<TValueType>
        {
            if (CheckPoint(collection)) structEnumUShortCollection<TValueType, collectionType>(collection);
        }

        /// <summary>
        ///     枚举集合序列化
        /// </summary>
        /// <param name="collection">枚举集合序列化</param>
        private void structEnumIntCollection<TValueType, collectionType>(collectionType collection)
            where collectionType : ICollection<TValueType>
        {
            int count = collection.Count, length = (count + 1) * sizeof(int);
            Stream.PrepLength(length);
            var write = Stream.CurrentData;
            *(int*)write = count;
            foreach (var value in collection)
                *(int*)(write += sizeof(int)) = TmphPub.TmphEnumCast<TValueType, int>.ToInt(value);
            Stream.Unsafer.AddLength(length);
            Stream.PrepLength();
        }

        /// <summary>
        ///     枚举集合序列化
        /// </summary>
        /// <param name="collection">枚举集合序列化</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void classEnumIntCollection<TValueType, collectionType>(collectionType collection)
            where collectionType : ICollection<TValueType>
        {
            if (CheckPoint(collection)) structEnumIntCollection<TValueType, collectionType>(collection);
        }

        /// <summary>
        ///     枚举集合序列化
        /// </summary>
        /// <param name="collection">枚举集合序列化</param>
        private void structEnumUIntCollection<TValueType, collectionType>(collectionType collection)
            where collectionType : ICollection<TValueType>
        {
            int count = collection.Count, length = (count + 1) * sizeof(uint);
            Stream.PrepLength(length);
            var write = Stream.CurrentData;
            *(int*)write = count;
            foreach (var value in collection)
                *(uint*)(write += sizeof(uint)) = TmphPub.TmphEnumCast<TValueType, uint>.ToInt(value);
            Stream.Unsafer.AddLength(length);
            Stream.PrepLength();
        }

        /// <summary>
        ///     枚举集合序列化
        /// </summary>
        /// <param name="collection">枚举集合序列化</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void classEnumUIntCollection<TValueType, collectionType>(collectionType collection)
            where collectionType : ICollection<TValueType>
        {
            if (CheckPoint(collection)) structEnumUIntCollection<TValueType, collectionType>(collection);
        }

        /// <summary>
        ///     枚举集合序列化
        /// </summary>
        /// <param name="collection">枚举集合序列化</param>
        private void structEnumLongCollection<TValueType, collectionType>(collectionType collection)
            where collectionType : ICollection<TValueType>
        {
            int count = collection.Count, length = count * sizeof(long) + sizeof(int);
            Stream.PrepLength(length);
            var write = Stream.CurrentData;
            *(int*)write = count;
            write += sizeof(int);
            foreach (var value in collection)
            {
                *(long*)write = TmphPub.TmphEnumCast<TValueType, long>.ToInt(value);
                write += sizeof(long);
            }
            Stream.Unsafer.AddLength(length);
            Stream.PrepLength();
        }

        /// <summary>
        ///     枚举集合序列化
        /// </summary>
        /// <param name="collection">枚举集合序列化</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void classEnumLongCollection<TValueType, collectionType>(collectionType collection)
            where collectionType : ICollection<TValueType>
        {
            if (CheckPoint(collection)) structEnumLongCollection<TValueType, collectionType>(collection);
        }

        /// <summary>
        ///     枚举集合序列化
        /// </summary>
        /// <param name="collection">枚举集合序列化</param>
        private void structEnumULongCollection<TValueType, collectionType>(collectionType collection)
            where collectionType : ICollection<TValueType>
        {
            int count = collection.Count, length = count * sizeof(ulong) + sizeof(int);
            Stream.PrepLength(length);
            var write = Stream.CurrentData;
            *(int*)write = count;
            write += sizeof(int);
            foreach (var value in collection)
            {
                *(ulong*)write = TmphPub.TmphEnumCast<TValueType, ulong>.ToInt(value);
                write += sizeof(ulong);
            }
            Stream.Unsafer.AddLength(length);
            Stream.PrepLength();
        }

        /// <summary>
        ///     枚举集合序列化
        /// </summary>
        /// <param name="collection">枚举集合序列化</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void classEnumULongCollection<TValueType, collectionType>(collectionType collection)
            where collectionType : ICollection<TValueType>
        {
            if (CheckPoint(collection)) structEnumULongCollection<TValueType, collectionType>(collection);
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private void enumByteArray<TValueType>(TValueType[] array)
        {
            if (checkPoint(array))
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
        private void enumSByteArray<TValueType>(TValueType[] array)
        {
            if (checkPoint(array))
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
        private void enumShortArray<TValueType>(TValueType[] array)
        {
            if (checkPoint(array))
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
        private void enumUShortArray<TValueType>(TValueType[] array)
        {
            if (checkPoint(array))
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
        private void enumIntArray<TValueType>(TValueType[] array)
        {
            if (checkPoint(array))
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
        private void enumUIntArray<TValueType>(TValueType[] array)
        {
            if (checkPoint(array))
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
        private void enumLongArray<TValueType>(TValueType[] array)
        {
            if (checkPoint(array))
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
        private void enumULongArray<TValueType>(TValueType[] array)
        {
            if (checkPoint(array))
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
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void enumULongArrayMember<TValueType>(TValueType[] array)
        {
            if (array == null) Stream.Write(NullValue);
            else enumULongArray(array);
        }

        /// <summary>
        ///     数组转换
        /// </summary>
        /// <param name="array">数组对象</param>
        private void nullableArray<TValueType>(TValueType[] array) where TValueType : struct
        {
            if (checkPoint(array))
            {
                var arrayMap = new TmphArrayMap(Stream, array.Length);
                foreach (TValueType? value in array) arrayMap.Next(value.HasValue);
                arrayMap.End(Stream);

                foreach (TValueType? value in array)
                {
                    if (value.HasValue) TmphTypeSerializer<TValueType>.StructSerialize(this, value.Value);
                }
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
        private void structArray<TValueType>(TValueType[] array)
        {
            if (checkPoint(array))
            {
                Stream.Write(array.Length);
                foreach (var value in array) TmphTypeSerializer<TValueType>.StructSerialize(this, value);
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
        private void array<TValueType>(TValueType[] array) where TValueType : class
        {
            if (checkPoint(array))
            {
                var arrayMap = new TmphArrayMap(Stream, array.Length);
                foreach (var value in array) arrayMap.Next(value != null);
                arrayMap.End(Stream);

                foreach (var value in array)
                {
                    if (value != null) TmphTypeSerializer<TValueType>.ClassSerialize(this, value);
                }
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
        ///     序列化接口
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="value"></param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void structISerialize<TValueType>(TValueType value)
            where TValueType : struct, Code.CSharp.TmphDataSerialize.ISerialize
        {
            value.Serialize(this);
        }

        /// <summary>
        ///     序列化接口
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="value"></param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void classISerialize<TValueType>(TValueType value)
            where TValueType : class, Code.CSharp.TmphDataSerialize.ISerialize
        {
            value.Serialize(this);
        }

        /// <summary>
        ///     序列化接口
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="value"></param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberClassISerialize<TValueType>(TValueType value)
            where TValueType : class, Code.CSharp.TmphDataSerialize.ISerialize
        {
            if (value == null) Stream.Write(NullValue);
            else value.Serialize(this);
        }

        /// <summary>
        ///     引用类型成员序列化
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="value"></param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public void MemberClassSerialize<TValueType>(TValueType value) where TValueType : class
        {
            if (value == null) Stream.Write(NullValue);
            else TmphTypeSerializer<TValueType>.ClassSerialize(this, value);
        }

        /// <summary>
        ///     未知类型序列化
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="value"></param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public void MemberNullableSerialize<TValueType>(TValueType? value) where TValueType : struct
        {
            TmphTypeSerializer<TValueType>.StructSerialize(this, value.Value);
        }

        /// <summary>
        ///     未知类型序列化
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="value"></param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public void MemberStructSerialize<TValueType>(TValueType value) where TValueType : struct
        {
            TmphTypeSerializer<TValueType>.StructSerialize(this, value);
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
            if (stream == null) TmphLog.Default.Throw(TmphLog.TmphExceptionType.Null);
            if (value == null) stream.Write(NullValue);
            else
            {
                var serializer = TmphTypePool<TmphDataSerializer>.Pop() ?? new TmphDataSerializer();
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
            var serializer = TmphTypePool<TmphDataSerializer>.Pop() ?? new TmphDataSerializer();
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
        public static void CodeSerialize<TValueType>(TValueType value, TmphUnmanagedStream stream, TmphConfig TmphConfig = null)
            where TValueType : Code.CSharp.TmphDataSerialize.ISerialize
        {
            if (stream == null) TmphLog.Default.Throw(TmphLog.TmphExceptionType.Null);
            if (value == null) stream.Write(NullValue);
            else
            {
                var serializer = TmphTypePool<TmphDataSerializer>.Pop() ?? new TmphDataSerializer();
                try
                {
                    serializer.codeSerialize(value, stream, TmphConfig ?? defaultConfig);
                }
                finally
                {
                    serializer.free();
                }
            }
        }

        /// <summary>
        ///     序列化
        /// </summary>
        /// <typeparam name="TValueType">目标数据类型</typeparam>
        /// <param name="value">数据对象</param>
        /// <param name="TmphConfig">配置参数</param>
        /// <returns>序列化数据</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static byte[] CodeSerialize<TValueType>(TValueType value, TmphConfig TmphConfig = null)
            where TValueType : Code.CSharp.TmphDataSerialize.ISerialize
        {
            if (value == null) return BitConverter.GetBytes(NullValue);
            var serializer = TmphTypePool<TmphDataSerializer>.Pop() ?? new TmphDataSerializer();
            try
            {
                return serializer.codeSerialize(value, TmphConfig ?? defaultConfig);
            }
            finally
            {
                serializer.free();
            }
        }

        /// <summary>
        ///     未知类型对象序列化
        /// </summary>
        /// <typeparam name="TValueType">目标数据类型</typeparam>
        /// <param name="value">数据对象</param>
        /// <param name="TmphConfig">配置参数</param>
        /// <returns>序列化数据</returns>
        private static byte[] objectSerialize<TValueType>(object value, TmphConfig TmphConfig)
        {
            var serializer = TmphTypePool<TmphDataSerializer>.Pop() ?? new TmphDataSerializer();
            try
            {
                return serializer.serialize((TValueType)value, TmphConfig ?? defaultConfig);
            }
            finally
            {
                serializer.free();
            }
        }

        /// <summary>
        ///     未知类型对象序列化
        /// </summary>
        /// <param name="value">数据对象</param>
        /// <param name="TmphConfig">配置参数</param>
        /// <returns>序列化数据</returns>
        public static byte[] ObjectSerialize(object value, TmphConfig TmphConfig = null)
        {
            if (value == null) return BitConverter.GetBytes(NullValue);
            var type = value.GetType();
            Func<object, TmphConfig, byte[]> serializer;
            if (!objectSerializes.TryGetValue(type, out serializer))
            {
                serializer =
                    (Func<object, TmphConfig, byte[]>)
                        Delegate.CreateDelegate(typeof(Func<object, TmphConfig, byte[]>),
                            objectSerializeMethod.MakeGenericMethod(type));
                objectSerializes.Set(type, serializer);
            }
            return serializer(value, TmphConfig);
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
        ///     配置参数
        /// </summary>
        public new sealed class TmphConfig : TmphBinarySerializer.TmphConfig
        {
            /// <summary>
            ///     是否检测引用类型对象的真实类型
            /// </summary>
            internal const int ObjectRealTypeValue = 2;

            /// <summary>
            ///     是否序列化成员位图
            /// </summary>
            public bool IsMemberMap;

            /// <summary>
            ///     是否检测引用类型对象的真实类型
            /// </summary>
            public bool IsRealType;

            /// <summary>
            ///     序列化头部数据
            /// </summary>
            internal override int HeaderValue
            {
                get
                {
                    var value = base.HeaderValue;
                    if (IsRealType) value += ObjectRealTypeValue;
                    return value;
                }
            }
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
            ///     真实类型序列化函数集合
            /// </summary>
            private static TmphInterlocked.TmphDictionary<Type, Action<TmphDataSerializer, object>> realSerializers =
                new TmphInterlocked.TmphDictionary<Type, Action<TmphDataSerializer, object>>(
                    TmphDictionary.CreateOnly<Type, Action<TmphDataSerializer, object>>());

            /// <summary>
            ///     是否支持循环引用处理集合
            /// </summary>
            private static TmphInterlocked.TmphDictionary<Type, bool> isReferenceMembers =
                new TmphInterlocked.TmphDictionary<Type, bool>(TmphDictionary.CreateOnly<Type, bool>());

            /// <summary>
            ///     是否支持循环引用处理函数信息
            /// </summary>
            private static readonly MethodInfo isReferenceMemberMethod =
                typeof(TmphTypeSerializer).GetMethod("isReferenceMember", BindingFlags.Static | BindingFlags.NonPublic);

            /// <summary>
            ///     获取字段成员集合
            /// </summary>
            /// <param name="type"></param>
            /// <returns>字段成员集合</returns>
            public static TmphFields<TmphFieldInfo> GetFields(TmphFieldIndex[] fieldIndexs, out int memberCountVerify)
            {
                TmphSubArray<TmphFieldInfo> fixedFields = new TmphSubArray<TmphFieldInfo>(fieldIndexs.Length),
                    fields = new TmphSubArray<TmphFieldInfo>();
                var jsonFields = new TmphSubArray<TmphFieldIndex>();
                fields.UnsafeSet(fixedFields.array, fixedFields.array.length(), 0);
                var fixedSize = 0;
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
                                }
                            }
                        }
                    }
                }
                memberCountVerify = fixedFields.Count + fields.Count + jsonFields.Count + 0x40000000;
                return new TmphFields<TmphFieldInfo>
                {
                    FixedFields = fixedFields.Sort(TmphFieldInfo.FixedSizeSort),
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
            ///     获取自定义序列化函数信息
            /// </summary>
            /// <param name="type"></param>
            /// <param name="isSerializer"></param>
            /// <returns></returns>
            public static MethodInfo GetCustom(Type type, bool isSerializer)
            {
                MethodInfo serializeMethod = null, deSerializeMethod = null;
                var refType = type.MakeByRefType();
                foreach (var method in TmphAttributeMethod.GetStatic(type))
                {
                    if (method.Method.ReturnType == typeof(void)
                        && method.GetAttribute<TmphDataSerialize.TmphCustom>(true) != null)
                    {
                        var parameters = method.Method.GetParameters();
                        if (parameters.Length == 2)
                        {
                            if (parameters[0].ParameterType == typeof(TmphDataSerializer))
                            {
                                if (parameters[1].ParameterType == type)
                                {
                                    if (deSerializeMethod != null)
                                        return isSerializer ? method.Method : deSerializeMethod;
                                    serializeMethod = method.Method;
                                }
                            }
                            else if (parameters[0].ParameterType == typeof(TmphDataDeSerializer))
                            {
                                if (parameters[1].ParameterType == refType)
                                {
                                    if (serializeMethod != null) return isSerializer ? serializeMethod : method.Method;
                                    deSerializeMethod = method.Method;
                                }
                            }
                        }
                    }
                }
                return null;
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
                            method = keyValuePairSerializeMethod.MakeGenericMethod(type.GetGenericArguments());
                        }
                    }
                    if (method == null)
                    {
                        if (typeof(Code.CSharp.TmphDataSerialize.ISerialize).IsAssignableFrom(type))
                        {
                            if (type.IsValueType) method = structISerializeMethod.MakeGenericMethod(type);
                            else method = memberClassISerializeMethod.MakeGenericMethod(type);
                        }
                        else if (type.IsValueType) method = structSerializeMethod.MakeGenericMethod(type);
                        else method = memberClassSerializeMethod.MakeGenericMethod(type);
                    }
                }
                memberSerializers.Set(type, method);
                return method;
            }

            /// <summary>
            ///     获取真实类型序列化函数
            /// </summary>
            /// <param name="type">数组类型</param>
            /// <returns>真实类型序列化函数</returns>
            public static Action<TmphDataSerializer, object> GetRealSerializer(Type type)
            {
                Action<TmphDataSerializer, object> method;
                if (realSerializers.TryGetValue(type, out method)) return method;
                method =
                    (Action<TmphDataSerializer, object>)
                        Delegate.CreateDelegate(typeof(Action<TmphDataSerializer, object>),
                            realTypeObjectMethod.MakeGenericMethod(type));
                realSerializers.Set(type, method);
                return method;
            }

            /// <summary>
            ///     是否支持循环引用处理
            /// </summary>
            /// <param name="type"></param>
            /// <returns></returns>
            public static bool IsReferenceMember(Type type)
            {
                bool isReferenceMember;
                if (isReferenceMembers.TryGetValue(type, out isReferenceMember)) return isReferenceMember;
                isReferenceMembers.Set(type,
                    isReferenceMember = (bool)isReferenceMemberMethod.MakeGenericMethod(type).Invoke(null, null));
                return isReferenceMember;
            }

            /// <summary>
            ///     是否支持循环引用处理
            /// </summary>
            /// <typeparam name="TValueType"></typeparam>
            /// <returns></returns>
            private static bool isReferenceMember<TValueType>()
            {
                return TmphTypeSerializer<TValueType>.IsReferenceMember;
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
                    dynamicMethod = new DynamicMethod("dataSerializer", null, new[] { typeof(TmphDataSerializer), type },
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
                    dynamicMethod = new DynamicMethod("dataMemberMapSerializer", null,
                        new[] { typeof(TmphMemberMap), typeof(TmphDataSerializer), type }, type, true);
                    generator = dynamicMethod.GetILGenerator();
                    isValueType = type.IsValueType;
                }

                /// <summary>
                ///     添加字段
                /// </summary>
                /// <param name="field">字段信息</param>
                public void Push(TmphFieldInfo field)
                {
                    var end = generator.DefineLabel();
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldc_I4, field.MemberIndex);
                    generator.Emit(OpCodes.Callvirt, TmphPub.MemberMapIsMemberMethod);
                    generator.Emit(OpCodes.Brfalse_S, end);

                    generator.Emit(OpCodes.Ldarg_1);
                    if (isValueType) generator.Emit(OpCodes.Ldarga_S, 2);
                    else generator.Emit(OpCodes.Ldarg_2);
                    generator.Emit(OpCodes.Ldfld, field.Field);
                    var method = getMemberMapSerializeMethod(field.Field.FieldType) ??
                                 getMemberSerializer(field.Field.FieldType);
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
            private static readonly TmphDataSerialize attribute;

            /// <summary>
            ///     序列化委托
            /// </summary>
            internal static readonly Action<TmphDataSerializer, TValueType> DefaultSerializer;

            /// <summary>
            ///     固定分组成员序列化
            /// </summary>
            private static readonly Action<TmphDataSerializer, TValueType> fixedMemberSerializer;

            /// <summary>
            ///     固定分组成员位图序列化
            /// </summary>
            private static readonly Action<TmphMemberMap, TmphDataSerializer, TValueType> fixedMemberMapSerializer;

            /// <summary>
            ///     成员序列化
            /// </summary>
            private static readonly Action<TmphDataSerializer, TValueType> memberSerializer;

            /// <summary>
            ///     成员位图序列化
            /// </summary>
            private static readonly Action<TmphMemberMap, TmphDataSerializer, TValueType> memberMapSerializer;

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
            ///     是否值类型
            /// </summary>
            private static readonly bool isValueType;

            /// <summary>
            ///     是否支持循环引用处理
            /// </summary>
            internal static readonly bool IsReferenceMember;

            static TmphTypeSerializer()
            {
                Type type = typeof(TValueType), TAttributeType;
                var methodInfo = getSerializeMethod(type);
                attribute = type.customAttribute<TmphDataSerialize>(out TAttributeType, true) ?? TmphDataSerialize.Default;
                if (methodInfo != null)
                {
                    var dynamicMethod = new DynamicMethod("dataSerializer", typeof(void),
                        new[] { typeof(TmphDataSerializer), type }, true);
                    dynamicMethod.InitLocals = true;
                    var generator = dynamicMethod.GetILGenerator();
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldarg_1);
                    generator.Emit(OpCodes.Call, methodInfo);
                    generator.Emit(OpCodes.Ret);
                    DefaultSerializer =
                        (Action<TmphDataSerializer, TValueType>)
                            dynamicMethod.CreateDelegate(typeof(Action<TmphDataSerializer, TValueType>));
                    isValueType = true;
                    IsReferenceMember = false;
                    return;
                }
                if (type.IsArray)
                {
                    isValueType = true;
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
                                    if (TEnumType == typeof(uint)) methodInfo = enumUIntArrayMethod;
                                    else if (TEnumType == typeof(byte)) methodInfo = enumByteArrayMethod;
                                    else if (TEnumType == typeof(ulong)) methodInfo = enumULongArrayMethod;
                                    else if (TEnumType == typeof(ushort)) methodInfo = enumUShortArrayMethod;
                                    else if (TEnumType == typeof(long)) methodInfo = enumLongArrayMethod;
                                    else if (TEnumType == typeof(short)) methodInfo = enumShortArrayMethod;
                                    else if (TEnumType == typeof(sbyte)) methodInfo = enumSByteArrayMethod;
                                    else methodInfo = enumIntArrayMethod;
                                    methodInfo = methodInfo.MakeGenericMethod(elementType);
                                    IsReferenceMember = false;
                                }
                                else if (elementType.IsGenericType &&
                                         elementType.GetGenericTypeDefinition() == typeof(Nullable<>))
                                {
                                    methodInfo =
                                        nullableArrayMethod.MakeGenericMethod(
                                            elementType = elementType.GetGenericArguments()[0]);
                                    IsReferenceMember = TmphTypeSerializer.IsReferenceMember(elementType);
                                }
                                else
                                {
                                    methodInfo = structArrayMethod.MakeGenericMethod(elementType);
                                    IsReferenceMember = TmphTypeSerializer.IsReferenceMember(elementType);
                                }
                            }
                            else
                            {
                                methodInfo = arrayMethod.MakeGenericMethod(elementType);
                                IsReferenceMember = TmphTypeSerializer.IsReferenceMember(elementType);
                            }
                            DefaultSerializer =
                                (Action<TmphDataSerializer, TValueType>)
                                    Delegate.CreateDelegate(typeof(Action<TmphDataSerializer, TValueType>), methodInfo);
                            return;
                        }
                    }
                    DefaultSerializer = toNull;
                    IsReferenceMember = false;
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
                    isValueType = true;
                    IsReferenceMember = false;
                    return;
                }
                if (type.IsPointer)
                {
                    DefaultSerializer = toNull;
                    IsReferenceMember = false;
                    isValueType = true;
                    return;
                }

                if (type.IsGenericType)
                {
                    var genericType = type.GetGenericTypeDefinition();
                    var parameterTypes = type.GetGenericArguments();
                    if (genericType == typeof(TmphSubArray<>))
                    {
                        DefaultSerializer =
                            (Action<TmphDataSerializer, TValueType>)
                                Delegate.CreateDelegate(typeof(Action<TmphDataSerializer, TValueType>),
                                    subArraySerializeMethod.MakeGenericMethod(parameterTypes));
                        IsReferenceMember = TmphTypeSerializer.IsReferenceMember(parameterTypes[0]);
                        isValueType = true;
                        return;
                    }
                    if (genericType == typeof(Dictionary<,>) || genericType == typeof(SortedDictionary<,>) ||
                        genericType == typeof(SortedList<,>))
                    {
                        DefaultSerializer =
                            (Action<TmphDataSerializer, TValueType>)
                                Delegate.CreateDelegate(typeof(Action<TmphDataSerializer, TValueType>),
                                    dictionarySerializeMethod.MakeGenericMethod(type, parameterTypes[0],
                                        parameterTypes[1]));
                        IsReferenceMember = TmphTypeSerializer.IsReferenceMember(parameterTypes[0]) ||
                                            TmphTypeSerializer.IsReferenceMember(parameterTypes[1]);
                        isValueType = true;
                        return;
                    }
                    if (genericType == typeof(Nullable<>))
                    {
                        DefaultSerializer =
                            (Action<TmphDataSerializer, TValueType>)
                                Delegate.CreateDelegate(typeof(Action<TmphDataSerializer, TValueType>),
                                    nullableSerializeMethod.MakeGenericMethod(parameterTypes));
                        IsReferenceMember = TmphTypeSerializer.IsReferenceMember(parameterTypes[0]);
                        isValueType = true;
                        return;
                    }
                    if (genericType == typeof(KeyValuePair<,>))
                    {
                        DefaultSerializer =
                            (Action<TmphDataSerializer, TValueType>)
                                Delegate.CreateDelegate(typeof(Action<TmphDataSerializer, TValueType>),
                                    keyValuePairSerializeMethod.MakeGenericMethod(parameterTypes));
                        IsReferenceMember = TmphTypeSerializer.IsReferenceMember(parameterTypes[0]) ||
                                            TmphTypeSerializer.IsReferenceMember(parameterTypes[1]);
                        isValueType = true;
                        return;
                    }
                }
                if ((methodInfo = TmphTypeSerializer.GetCustom(type, true)) != null)
                {
                    DefaultSerializer =
                        (Action<TmphDataSerializer, TValueType>)
                            Delegate.CreateDelegate(typeof(Action<TmphDataSerializer, TValueType>), methodInfo);
                    IsReferenceMember = attribute.IsReferenceMember;
                    isValueType = true;
                    return;
                }
                if (type.IsAbstract || type.IsInterface || TmphConstructor<TValueType>.New == null)
                {
                    DefaultSerializer = noConstructor;
                    isValueType = IsReferenceMember = true;
                    return;
                }
                ConstructorInfo constructorInfo = null;
                Type argumentType = null;
                IsReferenceMember = attribute.IsReferenceMember;
                foreach (var interfaceType in type.GetInterfaces())
                {
                    if (interfaceType.IsGenericType)
                    {
                        var genericType = interfaceType.GetGenericTypeDefinition();
                        if (genericType == typeof(ICollection<>))
                        {
                            var parameterTypes = interfaceType.GetGenericArguments();
                            argumentType = parameterTypes[0];
                            parameterTypes[0] = argumentType.MakeArrayType();
                            constructorInfo =
                                type.GetConstructor(
                                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null,
                                    parameterTypes, null);
                            if (constructorInfo != null) break;
                            parameterTypes[0] = typeof(IList<>).MakeGenericType(argumentType);
                            constructorInfo =
                                type.GetConstructor(
                                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null,
                                    parameterTypes, null);
                            if (constructorInfo != null) break;
                            parameterTypes[0] = typeof(ICollection<>).MakeGenericType(argumentType);
                            constructorInfo =
                                type.GetConstructor(
                                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null,
                                    parameterTypes, null);
                            if (constructorInfo != null) break;
                            parameterTypes[0] = typeof(IEnumerable<>).MakeGenericType(argumentType);
                            constructorInfo =
                                type.GetConstructor(
                                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null,
                                    parameterTypes, null);
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
                                methodInfo =
                                    (type.IsValueType ? structDictionaryMethod : classDictionaryMethod)
                                        .MakeGenericMethod(type, parameters[0], parameters[1]);
                                DefaultSerializer =
                                    (Action<TmphDataSerializer, TValueType>)
                                        Delegate.CreateDelegate(typeof(Action<TmphDataSerializer, TValueType>), methodInfo);
                                return;
                            }
                        }
                    }
                }
                if (constructorInfo != null)
                {
                    if (argumentType.IsValueType && argumentType.IsEnum)
                    {
                        var TEnumType = Enum.GetUnderlyingType(argumentType);
                        if (TEnumType == typeof(uint))
                            methodInfo = type.IsValueType
                                ? structEnumUIntCollectionMethod
                                : classEnumUIntCollectionMethod;
                        else if (TEnumType == typeof(byte))
                            methodInfo = type.IsValueType
                                ? structEnumByteCollectionMethod
                                : classEnumByteCollectionMethod;
                        else if (TEnumType == typeof(ulong))
                            methodInfo = type.IsValueType
                                ? structEnumULongCollectionMethod
                                : classEnumULongCollectionMethod;
                        else if (TEnumType == typeof(ushort))
                            methodInfo = type.IsValueType
                                ? structEnumUShortCollectionMethod
                                : classEnumUShortCollectionMethod;
                        else if (TEnumType == typeof(long))
                            methodInfo = type.IsValueType
                                ? structEnumLongCollectionMethod
                                : classEnumLongCollectionMethod;
                        else if (TEnumType == typeof(short))
                            methodInfo = type.IsValueType
                                ? structEnumShortCollectionMethod
                                : classEnumShortCollectionMethod;
                        else if (TEnumType == typeof(sbyte))
                            methodInfo = type.IsValueType
                                ? structEnumSByteCollectionMethod
                                : classEnumSByteCollectionMethod;
                        else
                            methodInfo = type.IsValueType
                                ? structEnumIntCollectionMethod
                                : classEnumIntCollectionMethod;
                        methodInfo = methodInfo.MakeGenericMethod(argumentType, type);
                    }
                    else
                        methodInfo =
                            (type.IsValueType ? structCollectionMethod : classCollectionMethod).MakeGenericMethod(
                                argumentType, type);
                    DefaultSerializer =
                        (Action<TmphDataSerializer, TValueType>)
                            Delegate.CreateDelegate(typeof(Action<TmphDataSerializer, TValueType>), methodInfo);
                    return;
                }
                if (typeof(Code.CSharp.TmphDataSerialize.ISerialize).IsAssignableFrom(type))
                {
                    methodInfo = type.IsValueType ? structISerializeMethod : classISerializeMethod;
                    DefaultSerializer =
                        (Action<TmphDataSerializer, TValueType>)
                            Delegate.CreateDelegate(typeof(Action<TmphDataSerializer, TValueType>),
                                methodInfo.MakeGenericMethod(type));
                    isValueType = true;
                }
                else
                {
                    if (type.IsValueType) isValueType = true;
                    else if (attribute != TmphDataSerialize.Default && TAttributeType != type)
                    {
                        for (var baseType = type.BaseType; baseType != typeof(object); baseType = baseType.BaseType)
                        {
                            var baseAttribute = TmphTypeAttribute.GetAttribute<TmphDataSerialize>(baseType, false, true);
                            if (baseAttribute != null)
                            {
                                if (baseAttribute.IsBaseType)
                                {
                                    methodInfo = baseSerializeMethod.MakeGenericMethod(baseType, type);
                                    DefaultSerializer =
                                        (Action<TmphDataSerializer, TValueType>)
                                            Delegate.CreateDelegate(typeof(Action<TmphDataSerializer, TValueType>),
                                                methodInfo);
                                    return;
                                }
                                break;
                            }
                        }
                    }
                    var fields =
                        TmphTypeSerializer.GetFields(TmphMemberIndexGroup<TValueType>.GetFields(attribute.MemberFilter),
                            out memberCountVerify);
                    fixedFillSize = -fields.FixedSize & 3;
                    fixedSize = (fields.FixedSize + (sizeof(int) + 3)) & (int.MaxValue - 3);
                    var fixedDynamicMethod = new TmphTypeSerializer.TmphMemberDynamicMethod(type);
                    var fixedMemberMapDynamicMethod = attribute.IsMemberMap
                        ? new TmphTypeSerializer.TmphMemberMapDynamicMethod(type)
                        : default(TmphTypeSerializer.TmphMemberMapDynamicMethod);
                    foreach (var member in fields.FixedFields)
                    {
                        fixedDynamicMethod.Push(member);
                        if (attribute.IsMemberMap) fixedMemberMapDynamicMethod.Push(member);
                    }
                    fixedMemberSerializer =
                        (Action<TmphDataSerializer, TValueType>)
                            fixedDynamicMethod.Create<Action<TmphDataSerializer, TValueType>>();
                    if (attribute.IsMemberMap)
                        fixedMemberMapSerializer =
                            (Action<TmphMemberMap, TmphDataSerializer, TValueType>)
                                fixedMemberMapDynamicMethod.Create<Action<TmphMemberMap, TmphDataSerializer, TValueType>>();

                    var dynamicMethod = new TmphTypeSerializer.TmphMemberDynamicMethod(type);
                    var memberMapDynamicMethod = attribute.IsMemberMap
                        ? new TmphTypeSerializer.TmphMemberMapDynamicMethod(type)
                        : default(TmphTypeSerializer.TmphMemberMapDynamicMethod);
                    foreach (var member in fields.Fields)
                    {
                        dynamicMethod.Push(member);
                        if (attribute.IsMemberMap) memberMapDynamicMethod.Push(member);
                    }
                    memberSerializer =
                        (Action<TmphDataSerializer, TValueType>)
                            dynamicMethod.Create<Action<TmphDataSerializer, TValueType>>();
                    if (attribute.IsMemberMap)
                        memberMapSerializer =
                            (Action<TmphMemberMap, TmphDataSerializer, TValueType>)
                                memberMapDynamicMethod.Create<Action<TmphMemberMap, TmphDataSerializer, TValueType>>();

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
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            internal static void Serialize(TmphDataSerializer serializer, TValueType value)
            {
                if (isValueType) StructSerialize(serializer, value);
                else ClassSerialize(serializer, value);
            }

            /// <summary>
            ///     对象序列化
            /// </summary>
            /// <param name="serializer">二进制数据序列化</param>
            /// <param name="value">数据对象</param>
            internal static void ClassSerialize(TmphDataSerializer serializer, TValueType value)
            {
                if (DefaultSerializer == null)
                {
                    if (serializer.CheckPoint(value))
                    {
                        if (serializer.serializeConfig.IsRealType)
                        {
                            var type = value.GetType();
                            if (type != typeof(TValueType))
                            {
                                if (serializer.CheckPoint(value))
                                {
                                    serializer.Stream.Write(RealTypeValue);
                                    TmphTypeSerializer.GetRealSerializer(type)(serializer, value);
                                }
                                return;
                            }
                        }
                        if (TmphConstructor<TValueType>.New == null) serializer.Stream.Write(NullValue);
                        else MemberSerialize(serializer, value);
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
            internal static void StructSerialize(TmphDataSerializer serializer, TValueType value)
            {
                if (DefaultSerializer == null) MemberSerialize(serializer, value);
                else DefaultSerializer(serializer, value);
            }

            /// <summary>
            ///     对象序列化
            /// </summary>
            /// <param name="serializer">二进制数据序列化</param>
            /// <param name="value">数据对象</param>
            internal static void MemberSerialize(TmphDataSerializer serializer, TValueType value)
            {
                var memberMap = attribute.IsMemberMap ? serializer.SerializeMemberMap<TValueType>() : null;
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
                    stream.PrepLength(fixedSize - sizeof(int));
                    var length = stream.OffsetLength;
                    fixedMemberMapSerializer(memberMap, serializer, value);
                    stream.Unsafer.AddLength((length - stream.OffsetLength) & 3);
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

            /// <summary>
            ///     真实类型序列化
            /// </summary>
            /// <param name="serializer"></param>
            /// <param name="value"></param>
            internal static void RealTypeObject(TmphDataSerializer serializer, object value)
            {
                if (isValueType)
                {
                    TmphTypeSerializer<TmphRemoteType>.StructSerialize(serializer, typeof(TValueType));
                    StructSerialize(serializer, (TValueType)value);
                }
                else
                {
                    if (TmphConstructor<TValueType>.New == null) serializer.Stream.Write(NullValue);
                    else
                    {
                        TmphTypeSerializer<TmphRemoteType>.StructSerialize(serializer, typeof(TValueType));
                        if (DefaultSerializer == null)
                        {
                            if (serializer.CheckPoint(value)) MemberSerialize(serializer, (TValueType)value);
                        }
                        else DefaultSerializer(serializer, (TValueType)value);
                    }
                }
            }

            /// <summary>
            ///     对象序列化
            /// </summary>
            /// <param name="serializer">二进制数据序列化</param>
            /// <param name="value">数据对象</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            internal static void BaseSerialize<childType>(TmphDataSerializer serializer, childType value)
                where childType : TValueType
            {
                if (serializer.CheckPoint(value)) StructSerialize(serializer, value);
            }

            /// <summary>
            ///     找不到构造函数
            /// </summary>
            /// <param name="serializer">二进制数据序列化</param>
            /// <param name="value">数据对象</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private static void noConstructor(TmphDataSerializer serializer, TValueType value)
            {
                if (serializer.CheckPoint(value))
                {
                    if (serializer.serializeConfig.IsRealType) serializer.Stream.Write(NullValue);
                    else
                    {
                        var type = value.GetType();
                        if (type == typeof(TValueType)) serializer.Stream.Write(NullValue);
                        else TmphTypeSerializer.GetRealSerializer(type)(serializer, value);
                    }
                }
            }

            /// <summary>
            ///     不支持对象转换null
            /// </summary>
            /// <param name="serializer">二进制数据序列化</param>
            /// <param name="value">数据对象</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private static void toNull(TmphDataSerializer serializer, TValueType value)
            {
                serializer.Stream.Write(NullValue);
            }

            /// <summary>
            ///     枚举值序列化
            /// </summary>
            /// <param name="serializer">二进制数据序列化</param>
            /// <param name="array">枚举值序列化</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private static void enumByte(TmphDataSerializer serializer, TValueType value)
            {
                serializer.Stream.Write((uint)TmphPub.TmphEnumCast<TValueType, byte>.ToInt(value));
            }

            /// <summary>
            ///     枚举值序列化
            /// </summary>
            /// <param name="serializer">二进制数据序列化</param>
            /// <param name="array">枚举值序列化</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private static void enumSByte(TmphDataSerializer serializer, TValueType value)
            {
                serializer.Stream.Write((int)TmphPub.TmphEnumCast<TValueType, sbyte>.ToInt(value));
            }

            /// <summary>
            ///     枚举值序列化
            /// </summary>
            /// <param name="serializer">二进制数据序列化</param>
            /// <param name="array">枚举值序列化</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private static void enumShort(TmphDataSerializer serializer, TValueType value)
            {
                serializer.Stream.Write((int)TmphPub.TmphEnumCast<TValueType, short>.ToInt(value));
            }

            /// <summary>
            ///     枚举值序列化
            /// </summary>
            /// <param name="serializer">二进制数据序列化</param>
            /// <param name="array">枚举值序列化</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private static void enumUShort(TmphDataSerializer serializer, TValueType value)
            {
                serializer.Stream.Write((uint)TmphPub.TmphEnumCast<TValueType, ushort>.ToInt(value));
            }

            /// <summary>
            ///     枚举值序列化
            /// </summary>
            /// <param name="serializer">二进制数据序列化</param>
            /// <param name="array">枚举值序列化</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private static void enumInt(TmphDataSerializer serializer, TValueType value)
            {
                serializer.Stream.Write(TmphPub.TmphEnumCast<TValueType, int>.ToInt(value));
            }

            /// <summary>
            ///     枚举值序列化
            /// </summary>
            /// <param name="serializer">二进制数据序列化</param>
            /// <param name="array">枚举值序列化</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private static void enumUInt(TmphDataSerializer serializer, TValueType value)
            {
                serializer.Stream.Write(TmphPub.TmphEnumCast<TValueType, uint>.ToInt(value));
            }

            /// <summary>
            ///     枚举值序列化
            /// </summary>
            /// <param name="serializer">二进制数据序列化</param>
            /// <param name="array">枚举值序列化</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private static void enumLong(TmphDataSerializer serializer, TValueType value)
            {
                serializer.Stream.Write(TmphPub.TmphEnumCast<TValueType, long>.ToInt(value));
            }

            /// <summary>
            ///     枚举值序列化
            /// </summary>
            /// <param name="serializer">二进制数据序列化</param>
            /// <param name="array">枚举值序列化</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private static void enumULong(TmphDataSerializer serializer, TValueType value)
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