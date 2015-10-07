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
    ///     二进制数据反序列化
    /// </summary>
    public sealed unsafe class TmphIndexDeSerializer : TmphBinaryDeSerializer
    {
        /// <summary>
        ///     检测是否匹配成员索引函数信息
        /// </summary>
        private static readonly MethodInfo isMemberIndexMethod = typeof(TmphIndexDeSerializer).GetMethod(
            "isMemberIndex", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     基类反序列化函数信息
        /// </summary>
        private static readonly MethodInfo baseSerializeMethod = typeof(TmphIndexDeSerializer).GetMethod(
            "baseSerialize", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     对象反序列化函数信息
        /// </summary>
        private static readonly MethodInfo structDeSerializeMethod =
            typeof(TmphIndexDeSerializer).GetMethod("structDeSerialize", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     对象反序列化函数信息
        /// </summary>
        private static readonly MethodInfo classDeSerializeMethod =
            typeof(TmphIndexDeSerializer).GetMethod("classDeSerialize", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     对象反序列化函数信息
        /// </summary>
        private static readonly MethodInfo memberMapClassDeSerializeMethod =
            typeof(TmphIndexDeSerializer).GetMethod("memberMapClassDeSerialize",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo structArrayMethod = typeof(TmphIndexDeSerializer).GetMethod(
            "structArrayType", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo structArrayMemberMethod =
            typeof(TmphIndexDeSerializer).GetMethod("structArrayMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo structArrayMemberMapMethod =
            typeof(TmphIndexDeSerializer).GetMethod("structArrayMemberMap",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo nullableArrayMethod =
            typeof(TmphIndexDeSerializer).GetMethod("nullableArrayType", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo nullableArrayMemberMethod =
            typeof(TmphIndexDeSerializer).GetMethod("nullableArrayMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo nullableArrayMemberMapMethod =
            typeof(TmphIndexDeSerializer).GetMethod("nullableArrayMemberMap",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo arrayMethod = typeof(TmphIndexDeSerializer).GetMethod("arrayType",
            BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo arrayMemberMethod = typeof(TmphIndexDeSerializer).GetMethod("arrayMember",
            BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo arrayMemberMapMethod =
            typeof(TmphIndexDeSerializer).GetMethod("arrayMemberMap", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     对象序列化函数信息
        /// </summary>
        private static readonly MethodInfo nullableDeSerializeMethod =
            typeof(TmphIndexDeSerializer).GetMethod("nullableDeSerialize", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     对象序列化函数信息
        /// </summary>
        private static readonly MethodInfo nullableMemberDeSerializeMethod =
            typeof(TmphIndexDeSerializer).GetMethod("nullableMemberDeSerialize",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     对象序列化函数信息
        /// </summary>
        private static readonly MethodInfo nullableMemberMapDeSerializeMethod =
            typeof(TmphIndexDeSerializer).GetMethod("nullableMemberMapDeSerialize",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组对象序列化函数信息
        /// </summary>
        private static readonly MethodInfo subArrayDeSerializeMethod =
            typeof(TmphIndexDeSerializer).GetMethod("subArrayDeSerialize", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     字典序列化函数信息
        /// </summary>
        private static readonly MethodInfo keyValuePairDeSerializeMethod =
            typeof(TmphIndexDeSerializer).GetMethod("keyValuePairDeSerialize",
                BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     字典序列化函数信息
        /// </summary>
        private static readonly MethodInfo memberKeyValuePairDeSerializeMethod =
            typeof(TmphIndexDeSerializer).GetMethod("memberKeyValuePairDeSerialize",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     字典序列化函数信息
        /// </summary>
        private static readonly MethodInfo dictionaryDeSerializeMethod =
            typeof(TmphIndexDeSerializer).GetMethod("dictionaryDeSerialize",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     字典序列化函数信息
        /// </summary>
        private static readonly MethodInfo dictionaryMemberMethod =
            typeof(TmphIndexDeSerializer).GetMethod("dictionaryMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     字典序列化函数信息
        /// </summary>
        private static readonly MethodInfo dictionaryMemberMapMethod =
            typeof(TmphIndexDeSerializer).GetMethod("dictionaryMemberMap", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     字典序列化函数信息
        /// </summary>
        private static readonly MethodInfo sortedDictionaryDeSerializeMethod =
            typeof(TmphIndexDeSerializer).GetMethod("sortedDictionaryDeSerialize",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     字典序列化函数信息
        /// </summary>
        private static readonly MethodInfo sortedDictionaryMemberMethod =
            typeof(TmphIndexDeSerializer).GetMethod("sortedDictionaryMember",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     字典序列化函数信息
        /// </summary>
        private static readonly MethodInfo sortedDictionaryMemberMapMethod =
            typeof(TmphIndexDeSerializer).GetMethod("sortedDictionaryMemberMap",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     字典序列化函数信息
        /// </summary>
        private static readonly MethodInfo sortedListDeSerializeMethod =
            typeof(TmphIndexDeSerializer).GetMethod("sortedListDeSerialize",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     字典序列化函数信息
        /// </summary>
        private static readonly MethodInfo sortedListMemberMethod =
            typeof(TmphIndexDeSerializer).GetMethod("sortedListMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     字典序列化函数信息
        /// </summary>
        private static readonly MethodInfo sortedListMemberMapMethod =
            typeof(TmphIndexDeSerializer).GetMethod("sortedListMemberMap", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组构造反序列化函数信息
        /// </summary>
        private static readonly MethodInfo arrayConstructorMethod =
            typeof(TmphIndexDeSerializer).GetMethod("arrayConstructor", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组构造反序列化函数信息
        /// </summary>
        private static readonly MethodInfo listConstructorMethod =
            typeof(TmphIndexDeSerializer).GetMethod("listConstructor", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组构造反序列化函数信息
        /// </summary>
        private static readonly MethodInfo collectionConstructorMethod =
            typeof(TmphIndexDeSerializer).GetMethod("collectionConstructor",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组构造反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumerableConstructorMethod =
            typeof(TmphIndexDeSerializer).GetMethod("enumerableConstructor",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组构造反序列化函数信息
        /// </summary>
        private static readonly MethodInfo dictionaryConstructorMethod =
            typeof(TmphIndexDeSerializer).GetMethod("dictionaryConstructor",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     枚举转换函数信息
        /// </summary>
        private static readonly MethodInfo enumByteMemberMethod =
            typeof(TmphIndexDeSerializer).GetMethod("enumByteMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     枚举转换函数信息
        /// </summary>
        private static readonly MethodInfo enumSByteMemberMethod =
            typeof(TmphIndexDeSerializer).GetMethod("enumSByteMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     枚举转换函数信息
        /// </summary>
        private static readonly MethodInfo enumShortMemberMethod =
            typeof(TmphIndexDeSerializer).GetMethod("enumShortMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     枚举转换函数信息
        /// </summary>
        private static readonly MethodInfo enumUShortMemberMethod =
            typeof(TmphIndexDeSerializer).GetMethod("enumUShortMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     枚举转换函数信息
        /// </summary>
        private static readonly MethodInfo enumByteMemberMapMethod = typeof(TmphIndexDeSerializer).GetMethod("enumByte",
            BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     枚举转换函数信息
        /// </summary>
        private static readonly MethodInfo enumSByteMemberMapMethod = typeof(TmphIndexDeSerializer).GetMethod(
            "enumSByte", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     枚举转换函数信息
        /// </summary>
        private static readonly MethodInfo enumShortMemberMapMethod = typeof(TmphIndexDeSerializer).GetMethod(
            "enumShort", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     枚举转换函数信息
        /// </summary>
        private static readonly MethodInfo enumUShortMemberMapMethod =
            typeof(TmphIndexDeSerializer).GetMethod("enumUShort", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     枚举转换函数信息
        /// </summary>
        private static readonly MethodInfo enumIntMethod = typeof(TmphIndexDeSerializer).GetMethod("enumInt",
            BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     枚举转换函数信息
        /// </summary>
        private static readonly MethodInfo enumUIntMethod = typeof(TmphIndexDeSerializer).GetMethod("enumUInt",
            BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     枚举转换函数信息
        /// </summary>
        private static readonly MethodInfo enumLongMethod = typeof(TmphIndexDeSerializer).GetMethod("enumLong",
            BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     枚举转换函数信息
        /// </summary>
        private static readonly MethodInfo enumULongMethod = typeof(TmphIndexDeSerializer).GetMethod("enumULong",
            BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumByteArrayMethod =
            typeof(TmphIndexDeSerializer).GetMethod("enumByteArrayType", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumByteArrayMemberMethod =
            typeof(TmphIndexDeSerializer).GetMethod("enumByteArrayMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumByteArrayMemberMapMethod =
            typeof(TmphIndexDeSerializer).GetMethod("enumByteArrayMemberMap",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumSByteArrayMethod =
            typeof(TmphIndexDeSerializer).GetMethod("enumSByteArrayType", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumSByteArrayMemberMethod =
            typeof(TmphIndexDeSerializer).GetMethod("enumSByteArrayMember",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumSByteArrayMemberMapMethod =
            typeof(TmphIndexDeSerializer).GetMethod("enumSByteArrayMemberMap",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumShortArrayMethod =
            typeof(TmphIndexDeSerializer).GetMethod("enumShortArrayType", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumShortArrayMemberMethod =
            typeof(TmphIndexDeSerializer).GetMethod("enumShortArrayMember",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumShortArrayMemberMapMethod =
            typeof(TmphIndexDeSerializer).GetMethod("enumShortArrayMemberMap",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumUShortArrayMethod =
            typeof(TmphIndexDeSerializer).GetMethod("enumUShortArrayType", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumUShortArrayMemberMethod =
            typeof(TmphIndexDeSerializer).GetMethod("enumUShortArrayMember",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumUShortArrayMemberMapMethod =
            typeof(TmphIndexDeSerializer).GetMethod("enumUShortArrayMemberMap",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumIntArrayMethod =
            typeof(TmphIndexDeSerializer).GetMethod("enumIntArrayType", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumIntArrayMemberMethod =
            typeof(TmphIndexDeSerializer).GetMethod("enumIntArrayMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumIntArrayMemberMapMethod =
            typeof(TmphIndexDeSerializer).GetMethod("enumIntArrayMemberMap",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumUIntArrayMethod =
            typeof(TmphIndexDeSerializer).GetMethod("enumUIntArrayType", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumUIntArrayMemberMethod =
            typeof(TmphIndexDeSerializer).GetMethod("enumUIntArrayMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumUIntArrayMemberMapMethod =
            typeof(TmphIndexDeSerializer).GetMethod("enumUIntArrayMemberMap",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumLongArrayMethod =
            typeof(TmphIndexDeSerializer).GetMethod("enumLongArrayType", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumLongArrayMemberMethod =
            typeof(TmphIndexDeSerializer).GetMethod("enumLongArrayMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumLongArrayMemberMapMethod =
            typeof(TmphIndexDeSerializer).GetMethod("enumLongArrayMemberMap",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumULongArrayMethod =
            typeof(TmphIndexDeSerializer).GetMethod("enumULongArrayType", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumULongArrayMemberMethod =
            typeof(TmphIndexDeSerializer).GetMethod("enumULongArrayMember",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumULongArrayMemberMapMethod =
            typeof(TmphIndexDeSerializer).GetMethod("enumULongArrayMemberMap",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     基本类型反序列化函数
        /// </summary>
        private static readonly Dictionary<Type, MethodInfo> deSerializeMethods;

        /// <summary>
        ///     基本类型转换函数
        /// </summary>
        private static readonly Dictionary<Type, MethodInfo> memberDeSerializeMethods;

        /// <summary>
        ///     基本类型转换函数
        /// </summary>
        private static readonly Dictionary<Type, MethodInfo> memberMapDeSerializeMethods;

        /// <summary>
        ///     当前数据字节数
        /// </summary>
        private int currentSize;

        static TmphIndexDeSerializer()
        {
            deSerializeMethods = TmphDictionary.CreateOnly<Type, MethodInfo>();
            memberDeSerializeMethods = TmphDictionary.CreateOnly<Type, MethodInfo>();
            memberMapDeSerializeMethods = TmphDictionary.CreateOnly<Type, MethodInfo>();
            foreach (
                var method in typeof(TmphIndexDeSerializer).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic))
            {
                Type TParameterType = null;
                if (method.CustomAttribute<TmphDeSerializeMethod>() != null)
                {
                    deSerializeMethods.Add(TParameterType = method.GetParameters()[0].ParameterType.GetElementType(),
                        method);
                }
                if (method.CustomAttribute<TmphMemberDeSerializeMethod>() != null)
                {
                    if (TParameterType == null) TParameterType = method.GetParameters()[0].ParameterType.GetElementType();
                    try
                    {
                        memberDeSerializeMethods.Add(TParameterType, method);
                    }
                    catch
                    {
                        TmphLog.Error.Real(TParameterType.fullName());
                    }
                }
                if (method.CustomAttribute<TmphMemberMapDeSerializeMethod>() != null)
                {
                    memberMapDeSerializeMethods.Add(
                        TParameterType ?? method.GetParameters()[0].ParameterType.GetElementType(), method);
                }
            }
        }

        /// <summary>
        ///     反序列化
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="value"></param>
        /// <param name="TmphConfig"></param>
        /// <returns></returns>
        private TmphDeSerializeState deSerialize<TValueType>(byte* start, byte* end, ref TValueType value, TmphConfig TmphConfig)
        {
            DeSerializeConfig = TmphConfig;
            this.start = start;
            Read = start + sizeof(int);
            this.end = end;
            isMemberMap = (*start & TmphBinarySerializer.TmphConfig.MemberMapValue) != 0;
            if (isMemberMap) MemberMap = TmphConfig.MemberMap;
            state = TmphDeSerializeState.Success;
            TmphTypeDeSerializer<TValueType>.DeSerialize(this, ref value);
            checkState();
            return TmphConfig.State = state;
        }

        /// <summary>
        ///     检测是否匹配成员索引
        /// </summary>
        /// <param name="memberIndex"></param>
        /// <returns></returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private bool isMemberIndex(int memberIndex)
        {
            if ((*(uint*)Read & int.MaxValue) == memberIndex)
            {
                Read += sizeof(int);
                return true;
            }
            Error(TmphDeSerializeState.MemberIndex);
            return false;
        }

        /// <summary>
        ///     数组反序列化
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="value"></param>
        /// <returns>数组长度</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private int deSerializeArray<TValueType>(ref TValueType[] value)
        {
            if (*(int*)Read != 0) return *(int*)Read;
            value = TmphNullValue<TValueType>.Array;
            Read += sizeof(int);
            return 0;
        }

        /// <summary>
        ///     反序列化数组
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="value"></param>
        /// <returns>当前数据字节长度</returns>
        private int deSerializeArrayMember<TValueType>(ref TValueType[] value)
        {
            if ((currentSize = *(int*)Read) == 0)
            {
                value = TmphNullValue<TValueType>.Array;
                Read += sizeof(int);
                return 0;
            }
            return *(int*)(Read + sizeof(int));
        }

        /// <summary>
        ///     逻辑值反序列化
        /// </summary>
        /// <param name="value">逻辑值</param>
        [TmphDeSerializeMethod]
        private void deSerialize(ref bool[] value)
        {
            var length = deSerializeArray(ref value);
            if (length != 0)
            {
                if ((((length + (31 + 32)) >> 5) << 2) <= (int)(end - Read))
                {
                    value = new bool[length];
                    Read = DeSerialize(Read + sizeof(int), value);
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     逻辑值反序列化
        /// </summary>
        /// <param name="value">逻辑值</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void deSerializeBoolArray(ref bool[] value)
        {
            var length = deSerializeArrayMember(ref value);
            if (length != 0)
            {
                if ((((length + (31 + 64)) >> 5) << 2) == currentSize && (uint)currentSize <= (uint)(int)(end - Read))
                {
                    value = new bool[length];
                    Read = DeSerialize(Read + sizeof(int) * 2, value);
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     逻辑值反序列化
        /// </summary>
        /// <param name="value">逻辑值</param>
        [TmphMemberDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref bool[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerializeBoolArray(ref value);
        }

        /// <summary>
        ///     逻辑值反序列化
        /// </summary>
        /// <param name="value">逻辑值</param>
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapDeSerialize(ref bool[] value)
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0) deSerializeBoolArray(ref value);
            else value = null;
        }

        /// <summary>
        ///     逻辑值反序列化
        /// </summary>
        /// <param name="value">逻辑值</param>
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapDeSerialize(ref bool? value)
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0)
            {
                value = *(bool*)Read;
                Read += sizeof(int);
            }
            else value = null;
        }

        /// <summary>
        ///     逻辑值反序列化
        /// </summary>
        /// <param name="value">逻辑值</param>
        [TmphDeSerializeMethod]
        private void deSerialize(ref bool?[] value)
        {
            var length = deSerializeArray(ref value);
            if (length != 0)
            {
                if ((((length + (31 + 32)) >> 5) << 2) <= (int)(end - Read))
                {
                    value = new bool?[length >> 1];
                    Read = DeSerialize(Read + sizeof(int), value);
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     逻辑值反序列化
        /// </summary>
        /// <param name="value">逻辑值</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void deSerializeBoolArray(ref bool?[] value)
        {
            var length = deSerializeArrayMember(ref value);
            if (length != 0)
            {
                if ((((length + (31 + 64)) >> 5) << 2) == currentSize && (uint)currentSize <= (uint)(int)(end - Read))
                {
                    value = new bool?[length >> 1];
                    Read = DeSerialize(Read + sizeof(int) * 2, value);
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     逻辑值反序列化
        /// </summary>
        /// <param name="value">逻辑值</param>
        [TmphMemberDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref bool?[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerializeBoolArray(ref value);
        }

        /// <summary>
        ///     逻辑值反序列化
        /// </summary>
        /// <param name="value">逻辑值</param>
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapDeSerialize(ref bool?[] value)
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0) deSerializeBoolArray(ref value);
            else value = null;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphDeSerializeMethod]
        private void deSerialize(ref byte[] value)
        {
            var length = deSerializeArray(ref value);
            if (length != 0)
            {
                if (((length + (3 + sizeof(int))) & (int.MaxValue - 3)) <= (int)(end - Read))
                {
                    value = new byte[length];
                    Read = DeSerialize(Read + sizeof(int), value);
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        private void deSerializeByteArray(ref byte[] value)
        {
            var length = deSerializeArrayMember(ref value);
            if (length != 0)
            {
                if (((length + (3 + sizeof(int) * 2)) & (int.MaxValue - 3)) == currentSize &&
                    (uint)currentSize <= (uint)(int)(end - Read))
                {
                    value = new byte[length];
                    Read = DeSerialize(Read + sizeof(int) * 2, value);
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref byte[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerializeByteArray(ref value);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapDeSerialize(ref byte[] value)
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0) deSerializeByteArray(ref value);
            else value = null;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapDeSerializeMethod]
        [TmphMemberDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref TmphSubArray<byte> value)
        {
            if ((currentSize = *(int*)Read) == 0)
            {
                value.UnsafeSetLength(0);
                Read += sizeof(int);
            }
            else
            {
                var length = *(int*)(Read + sizeof(int));
                if (length != 0)
                {
                    if (((length + (3 + sizeof(int) * 2)) & (int.MaxValue - 3)) == currentSize &&
                        (uint)currentSize <= (uint)(int)(end - Read))
                    {
                        var array = new byte[length];
                        Read = DeSerialize(Read + sizeof(int) * 2, array);
                        value.UnsafeSet(array, 0, length);
                    }
                    else Error(TmphDeSerializeState.IndexOutOfRange);
                }
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapDeSerialize(ref byte? value)
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0)
            {
                value = *Read;
                Read += sizeof(int);
            }
            else value = null;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphDeSerializeMethod]
        private void deSerialize(ref byte?[] value)
        {
            var length = deSerializeArray(ref value);
            if (length != 0)
            {
                if ((((length + (31 + 32)) >> 5) << 2) <= (int)(end - Read))
                {
                    value = new byte?[length];
                    Read = DeSerialize(Read + sizeof(int), value);
                    if (Read <= end) return;
                }
                Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        private void deSerializeByteArray(ref byte?[] value)
        {
            var length = deSerializeArrayMember(ref value);
            if (length != 0)
            {
                var end = Read + currentSize;
                if ((((length + (31 + 64)) >> 5) << 2) <= currentSize && end <= this.end)
                {
                    value = new byte?[length];
                    Read = DeSerialize(Read + sizeof(int) * 2, value);
                    if (Read == end) return;
                }
                Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref byte?[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerializeByteArray(ref value);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapDeSerialize(ref byte?[] value)
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0) deSerializeByteArray(ref value);
            else value = null;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphDeSerializeMethod]
        private void deSerialize(ref sbyte[] value)
        {
            var length = deSerializeArray(ref value);
            if (length != 0)
            {
                if (((length + (3 + sizeof(int))) & (int.MaxValue - 3)) <= (int)(end - Read))
                {
                    value = new sbyte[length];
                    Read = DeSerialize(Read + sizeof(int), value);
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        private void deSerializeSByteArray(ref sbyte[] value)
        {
            var length = deSerializeArrayMember(ref value);
            if (length != 0)
            {
                if (((length + (3 + sizeof(int) * 2)) & (int.MaxValue - 3)) == currentSize &&
                    (uint)currentSize <= (uint)(int)(end - Read))
                {
                    value = new sbyte[length];
                    Read = DeSerialize(Read + sizeof(int) * 2, value);
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref sbyte[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerializeSByteArray(ref value);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapDeSerialize(ref sbyte[] value)
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0) deSerializeSByteArray(ref value);
            else value = null;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapDeSerialize(ref sbyte? value)
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0)
            {
                value = (sbyte)*(int*)Read;
                Read += sizeof(int);
            }
            else value = null;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphDeSerializeMethod]
        private void deSerialize(ref sbyte?[] value)
        {
            var length = deSerializeArray(ref value);
            if (length != 0)
            {
                if ((((length + (31 + 32)) >> 5) << 2) <= (int)(end - Read))
                {
                    value = new sbyte?[length];
                    Read = DeSerialize((sbyte*)Read + sizeof(int), value);
                    if (Read <= end) return;
                }
                Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        private void deSerializeSByteArray(ref sbyte?[] value)
        {
            var length = deSerializeArrayMember(ref value);
            if (length != 0)
            {
                var end = Read + currentSize;
                if ((((length + (31 + 64)) >> 5) << 2) <= currentSize && end <= this.end)
                {
                    value = new sbyte?[length];
                    Read = DeSerialize((sbyte*)Read + sizeof(int) * 2, value);
                    if (Read == end) return;
                }
                Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref sbyte?[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerializeSByteArray(ref value);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapDeSerialize(ref sbyte?[] value)
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0) deSerializeSByteArray(ref value);
            else value = null;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphDeSerializeMethod]
        private void deSerialize(ref short[] value)
        {
            var length = deSerializeArray(ref value);
            if (length != 0)
            {
                if ((((length * sizeof(short)) + (3 + sizeof(int))) & (int.MaxValue - 3)) <= (int)(end - Read))
                {
                    value = new short[length];
                    Read = DeSerialize(Read + sizeof(int), value);
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        private void deSerializeShortArray(ref short[] value)
        {
            var length = deSerializeArrayMember(ref value);
            if (length != 0)
            {
                if ((((length * sizeof(short)) + (3 + sizeof(int) * 2)) & (int.MaxValue - 3)) == currentSize &&
                    (uint)currentSize <= (uint)(int)(end - Read))
                {
                    value = new short[length];
                    Read = DeSerialize(Read + sizeof(int) * 2, value);
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref short[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerializeShortArray(ref value);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapDeSerialize(ref short[] value)
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0) deSerializeShortArray(ref value);
            else value = null;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapDeSerialize(ref short? value)
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0)
            {
                value = (short)*(int*)Read;
                Read += sizeof(int);
            }
            else value = null;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphDeSerializeMethod]
        private void deSerialize(ref short?[] value)
        {
            var length = deSerializeArray(ref value);
            if (length != 0)
            {
                if ((((length + (31 + 32)) >> 5) << 2) <= (int)(end - Read))
                {
                    value = new short?[length];
                    Read = DeSerialize(Read + sizeof(int), value);
                    if (Read <= end) return;
                }
                Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        private void deSerializeShortArray(ref short?[] value)
        {
            var length = deSerializeArrayMember(ref value);
            if (length != 0)
            {
                var end = Read + currentSize;
                if ((((length + (31 + 64)) >> 5) << 2) <= currentSize && end <= this.end)
                {
                    value = new short?[length];
                    Read = DeSerialize(Read + sizeof(int) * 2, value);
                    if (Read == end) return;
                }
                Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref short?[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerializeShortArray(ref value);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapDeSerialize(ref short?[] value)
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0) deSerializeShortArray(ref value);
            else value = null;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphDeSerializeMethod]
        private void deSerialize(ref ushort[] value)
        {
            var length = deSerializeArray(ref value);
            if (length != 0)
            {
                if (((length * sizeof(ushort) + (3 + sizeof(int))) & (int.MaxValue - 3)) <= (int)(end - Read))
                {
                    value = new ushort[length];
                    Read = DeSerialize(Read + sizeof(int), value);
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        private void deSerializeUShortArray(ref ushort[] value)
        {
            var length = deSerializeArrayMember(ref value);
            if (length != 0)
            {
                if ((((length * sizeof(short)) + (3 + sizeof(int) * 2)) & (int.MaxValue - 3)) == currentSize &&
                    (uint)currentSize <= (uint)(int)(end - Read))
                {
                    value = new ushort[length];
                    Read = DeSerialize(Read + sizeof(int) * 2, value);
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref ushort[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerializeUShortArray(ref value);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapDeSerialize(ref ushort[] value)
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0) deSerializeUShortArray(ref value);
            else value = null;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapDeSerialize(ref ushort? value)
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0)
            {
                value = *(ushort*)Read;
                Read += sizeof(int);
            }
            else value = null;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphDeSerializeMethod]
        private void deSerialize(ref ushort?[] value)
        {
            var length = deSerializeArray(ref value);
            if (length != 0)
            {
                if ((((length + (31 + 32)) >> 5) << 2) <= (int)(end - Read))
                {
                    value = new ushort?[length];
                    Read = DeSerialize(Read + sizeof(int), value);
                    if (Read <= end) return;
                }
                Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        private void deSerializeUShortArray(ref ushort?[] value)
        {
            var length = deSerializeArrayMember(ref value);
            if (length != 0)
            {
                var end = Read + currentSize;
                if ((((length + (31 + 64)) >> 5) << 2) <= currentSize && end <= this.end)
                {
                    value = new ushort?[length];
                    Read = DeSerialize(Read + sizeof(int) * 2, value);
                    if (Read == end) return;
                }
                Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref ushort?[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerializeUShortArray(ref value);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapDeSerialize(ref ushort?[] value)
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0) deSerializeUShortArray(ref value);
            else value = null;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphDeSerializeMethod]
        private void deSerialize(ref int[] value)
        {
            var length = deSerializeArray(ref value);
            if (length != 0)
            {
                if ((length + 1) * sizeof(int) <= (int)(end - Read))
                {
                    value = new int[length];
                    Read = DeSerialize(Read + sizeof(int), value);
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        private void deSerializeIntArray(ref int[] value)
        {
            var length = deSerializeArrayMember(ref value);
            if (length != 0)
            {
                if ((length + 2) * sizeof(int) == currentSize && (uint)currentSize <= (uint)(int)(end - Read))
                {
                    value = new int[length];
                    Read = DeSerialize(Read + sizeof(int) * 2, value);
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref int[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerializeIntArray(ref value);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapDeSerialize(ref int[] value)
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0) deSerializeIntArray(ref value);
            else value = null;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapDeSerialize(ref int? value)
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0)
            {
                value = *(int*)Read;
                Read += sizeof(int);
            }
            else value = null;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphDeSerializeMethod]
        private void deSerialize(ref int?[] value)
        {
            var length = deSerializeArray(ref value);
            if (length != 0)
            {
                if ((((length + (31 + 32)) >> 5) << 2) <= (int)(end - Read))
                {
                    value = new int?[length];
                    Read = DeSerialize(Read + sizeof(int), value);
                    if (Read <= end) return;
                }
                Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        private void deSerializeIntArray(ref int?[] value)
        {
            var length = deSerializeArrayMember(ref value);
            if (length != 0)
            {
                var end = Read + currentSize;
                if ((((length + (31 + 64)) >> 5) << 2) <= currentSize && end <= this.end)
                {
                    value = new int?[length];
                    Read = DeSerialize(Read + sizeof(int) * 2, value);
                    if (Read == end) return;
                }
                Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref int?[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerializeIntArray(ref value);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapDeSerialize(ref int?[] value)
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0) deSerializeIntArray(ref value);
            else value = null;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphDeSerializeMethod]
        private void deSerialize(ref uint[] value)
        {
            var length = deSerializeArray(ref value);
            if (length != 0)
            {
                if ((length + 1) * sizeof(int) <= (int)(end - Read))
                {
                    value = new uint[length];
                    Read = DeSerialize(Read + sizeof(int), value);
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        private void deSerializeUIntArray(ref uint[] value)
        {
            var length = deSerializeArrayMember(ref value);
            if (length != 0)
            {
                if ((length + 2) * sizeof(int) == currentSize && (uint)currentSize <= (uint)(int)(end - Read))
                {
                    value = new uint[length];
                    Read = DeSerialize(Read + sizeof(int) * 2, value);
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref uint[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerializeUIntArray(ref value);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapDeSerialize(ref uint[] value)
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0) deSerializeUIntArray(ref value);
            else value = null;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapDeSerialize(ref uint? value)
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0)
            {
                value = *(uint*)Read;
                Read += sizeof(int);
            }
            else value = null;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphDeSerializeMethod]
        private void deSerialize(ref uint?[] value)
        {
            var length = deSerializeArray(ref value);
            if (length != 0)
            {
                if ((((length + (31 + 32)) >> 5) << 2) <= (int)(end - Read))
                {
                    value = new uint?[length];
                    Read = DeSerialize(Read + sizeof(int), value);
                    if (Read <= end) return;
                }
                Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        private void deSerializeUIntArray(ref uint?[] value)
        {
            var length = deSerializeArrayMember(ref value);
            if (length != 0)
            {
                var end = Read + currentSize;
                if ((((length + (31 + 64)) >> 5) << 2) <= currentSize && end <= this.end)
                {
                    value = new uint?[length];
                    Read = DeSerialize(Read + sizeof(int) * 2, value);
                    if (Read == end) return;
                }
                Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref uint?[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerializeUIntArray(ref value);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapDeSerialize(ref uint?[] value)
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0) deSerializeUIntArray(ref value);
            else value = null;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphDeSerializeMethod]
        private void deSerialize(ref long[] value)
        {
            var length = deSerializeArray(ref value);
            if (length != 0)
            {
                if (length * sizeof(long) + sizeof(int) <= (int)(end - Read))
                {
                    value = new long[length];
                    Read = DeSerialize(Read + sizeof(int), value);
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        private void deSerializeLongArray(ref long[] value)
        {
            var length = deSerializeArrayMember(ref value);
            if (length != 0)
            {
                if ((length * sizeof(long) + sizeof(int) * 2) == currentSize &&
                    (uint)currentSize <= (uint)(int)(end - Read))
                {
                    value = new long[length];
                    Read = DeSerialize(Read + sizeof(int) * 2, value);
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref long[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerializeLongArray(ref value);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapDeSerialize(ref long[] value)
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0) deSerializeLongArray(ref value);
            else value = null;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapDeSerialize(ref long? value)
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0)
            {
                value = *(long*)Read;
                Read += sizeof(long);
            }
            else value = null;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphDeSerializeMethod]
        private void deSerialize(ref long?[] value)
        {
            var length = deSerializeArray(ref value);
            if (length != 0)
            {
                if ((((length + (31 + 32)) >> 5) << 2) <= (int)(end - Read))
                {
                    value = new long?[length];
                    Read = DeSerialize(Read + sizeof(int), value);
                    if (Read <= end) return;
                }
                Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        private void deSerializeLongArray(ref long?[] value)
        {
            var length = deSerializeArrayMember(ref value);
            if (length != 0)
            {
                var end = Read + currentSize;
                if ((((length + (31 + 64)) >> 5) << 2) <= currentSize && end <= this.end)
                {
                    value = new long?[length];
                    Read = DeSerialize(Read + sizeof(int) * 2, value);
                    if (Read == end) return;
                }
                Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref long?[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerializeLongArray(ref value);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapDeSerialize(ref long?[] value)
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0) deSerializeLongArray(ref value);
            else value = null;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphDeSerializeMethod]
        private void deSerialize(ref ulong[] value)
        {
            var length = deSerializeArray(ref value);
            if (length != 0)
            {
                if (length * sizeof(ulong) + sizeof(int) <= (int)(end - Read))
                {
                    value = new ulong[length];
                    Read = DeSerialize(Read + sizeof(int), value);
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        private void deSerializeULongArray(ref ulong[] value)
        {
            var length = deSerializeArrayMember(ref value);
            if (length != 0)
            {
                if ((length * sizeof(ulong) + sizeof(int) * 2) == currentSize &&
                    (uint)currentSize <= (uint)(int)(end - Read))
                {
                    value = new ulong[length];
                    Read = DeSerialize(Read + sizeof(int) * 2, value);
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref ulong[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerializeULongArray(ref value);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapDeSerialize(ref ulong[] value)
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0) deSerializeULongArray(ref value);
            else value = null;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapDeSerialize(ref ulong? value)
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0)
            {
                value = *(ulong*)Read;
                Read += sizeof(ulong);
            }
            else value = null;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphDeSerializeMethod]
        private void deSerialize(ref ulong?[] value)
        {
            var length = deSerializeArray(ref value);
            if (length != 0)
            {
                if ((((length + (31 + 32)) >> 5) << 2) <= (int)(end - Read))
                {
                    value = new ulong?[length];
                    Read = DeSerialize(Read + sizeof(int), value);
                    if (Read <= end) return;
                }
                Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        private void deSerializeULongArray(ref ulong?[] value)
        {
            var length = deSerializeArrayMember(ref value);
            if (length != 0)
            {
                var end = Read + currentSize;
                if ((((length + (31 + 64)) >> 5) << 2) <= currentSize && end <= this.end)
                {
                    value = new ulong?[length];
                    Read = DeSerialize(Read + sizeof(int) * 2, value);
                    if (Read == end) return;
                }
                Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref ulong?[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerializeULongArray(ref value);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapDeSerialize(ref ulong?[] value)
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0) deSerializeULongArray(ref value);
            else value = null;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphDeSerializeMethod]
        private void deSerialize(ref float[] value)
        {
            var length = deSerializeArray(ref value);
            if (length != 0)
            {
                if (length * sizeof(float) + sizeof(int) <= (int)(end - Read))
                {
                    value = new float[length];
                    Read = DeSerialize(Read + sizeof(int), value);
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        private void deSerializeFloatArray(ref float[] value)
        {
            var length = deSerializeArrayMember(ref value);
            if (length != 0)
            {
                if ((length * sizeof(float) + sizeof(int) * 2) == currentSize &&
                    (uint)currentSize <= (uint)(int)(end - Read))
                {
                    value = new float[length];
                    Read = DeSerialize(Read + sizeof(int) * 2, value);
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref float[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerializeFloatArray(ref value);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapDeSerialize(ref float[] value)
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0) deSerializeFloatArray(ref value);
            else value = null;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapDeSerialize(ref float? value)
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0)
            {
                value = *(float*)Read;
                Read += sizeof(float);
            }
            else value = null;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphDeSerializeMethod]
        private void deSerialize(ref float?[] value)
        {
            var length = deSerializeArray(ref value);
            if (length != 0)
            {
                if ((((length + (31 + 32)) >> 5) << 2) <= (int)(end - Read))
                {
                    value = new float?[length];
                    Read = DeSerialize(Read + sizeof(int), value);
                    if (Read <= end) return;
                }
                Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        private void deSerializeFloatArray(ref float?[] value)
        {
            var length = deSerializeArrayMember(ref value);
            if (length != 0)
            {
                var end = Read + currentSize;
                if ((((length + (31 + 64)) >> 5) << 2) <= currentSize && end <= this.end)
                {
                    value = new float?[length];
                    Read = DeSerialize(Read + sizeof(int) * 2, value);
                    if (Read == end) return;
                }
                Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref float?[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerializeFloatArray(ref value);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapDeSerialize(ref float?[] value)
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0) deSerializeFloatArray(ref value);
            else value = null;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphDeSerializeMethod]
        private void deSerialize(ref double[] value)
        {
            var length = deSerializeArray(ref value);
            if (length != 0)
            {
                if (length * sizeof(double) + sizeof(int) <= (int)(end - Read))
                {
                    value = new double[length];
                    Read = DeSerialize(Read + sizeof(int), value);
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        private void deSerializeDoubleArray(ref double[] value)
        {
            var length = deSerializeArrayMember(ref value);
            if (length != 0)
            {
                if ((length * sizeof(double) + sizeof(int) * 2) == currentSize &&
                    (uint)currentSize <= (uint)(int)(end - Read))
                {
                    value = new double[length];
                    Read = DeSerialize(Read + sizeof(int) * 2, value);
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref double[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerializeDoubleArray(ref value);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapDeSerialize(ref double[] value)
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0) deSerializeDoubleArray(ref value);
            else value = null;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapDeSerialize(ref double? value)
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0)
            {
                value = *(double*)Read;
                Read += sizeof(double);
            }
            else value = null;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphDeSerializeMethod]
        private void deSerialize(ref double?[] value)
        {
            var length = deSerializeArray(ref value);
            if (length != 0)
            {
                if ((((length + (31 + 32)) >> 5) << 2) <= (int)(end - Read))
                {
                    value = new double?[length];
                    Read = DeSerialize(Read + sizeof(int), value);
                    if (Read <= end) return;
                }
                Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        private void deSerializeDoubleArray(ref double?[] value)
        {
            var length = deSerializeArrayMember(ref value);
            if (length != 0)
            {
                var end = Read + currentSize;
                if ((((length + (31 + 64)) >> 5) << 2) <= currentSize && end <= this.end)
                {
                    value = new double?[length];
                    Read = DeSerialize(Read + sizeof(int) * 2, value);
                    if (Read == end) return;
                }
                Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref double?[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerializeDoubleArray(ref value);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapDeSerialize(ref double?[] value)
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0) deSerializeDoubleArray(ref value);
            else value = null;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphDeSerializeMethod]
        private void deSerialize(ref decimal[] value)
        {
            var length = deSerializeArray(ref value);
            if (length != 0)
            {
                if (length * sizeof(decimal) + sizeof(int) <= (int)(end - Read))
                {
                    value = new decimal[length];
                    Read = DeSerialize(Read + sizeof(int), value);
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        private void deSerializeDecimalArray(ref decimal[] value)
        {
            var length = deSerializeArrayMember(ref value);
            if (length != 0)
            {
                if ((length * sizeof(decimal) + sizeof(int) * 2) == currentSize &&
                    (uint)currentSize <= (uint)(int)(end - Read))
                {
                    value = new decimal[length];
                    Read = DeSerialize(Read + sizeof(int) * 2, value);
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref decimal[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerializeDecimalArray(ref value);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapDeSerialize(ref decimal[] value)
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0) deSerializeDecimalArray(ref value);
            else value = null;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapDeSerialize(ref decimal? value)
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0)
            {
                value = *(decimal*)Read;
                Read += sizeof(decimal);
            }
            else value = null;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphDeSerializeMethod]
        private void deSerialize(ref decimal?[] value)
        {
            var length = deSerializeArray(ref value);
            if (length != 0)
            {
                if ((((length + (31 + 32)) >> 5) << 2) <= (int)(end - Read))
                {
                    value = new decimal?[length];
                    Read = DeSerialize(Read + sizeof(int), value);
                    if (Read <= end) return;
                }
                Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        private void deSerializeDecimalArray(ref decimal?[] value)
        {
            var length = deSerializeArrayMember(ref value);
            if (length != 0)
            {
                var end = Read + currentSize;
                if ((((length + (31 + 64)) >> 5) << 2) <= currentSize && end <= this.end)
                {
                    value = new decimal?[length];
                    Read = DeSerialize(Read + sizeof(int) * 2, value);
                    if (Read == end) return;
                }
                Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref decimal?[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerializeDecimalArray(ref value);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapDeSerialize(ref decimal?[] value)
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0) deSerializeDecimalArray(ref value);
            else value = null;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphDeSerializeMethod]
        private void deSerialize(ref char[] value)
        {
            var length = deSerializeArray(ref value);
            if (length != 0)
            {
                if (((length * sizeof(char) + (3 + sizeof(int))) & (int.MaxValue - 3)) <= (int)(end - Read))
                {
                    value = new char[length];
                    Read = DeSerialize(Read + sizeof(int), value);
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        private void deSerializeCharArray(ref char[] value)
        {
            var length = deSerializeArrayMember(ref value);
            if (length != 0)
            {
                if ((((length * sizeof(char)) + (3 + sizeof(int) * 2)) & (int.MaxValue - 3)) == currentSize &&
                    (uint)currentSize <= (uint)(int)(end - Read))
                {
                    value = new char[length];
                    Read = DeSerialize(Read + sizeof(int) * 2, value);
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref char[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerializeCharArray(ref value);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapDeSerialize(ref char[] value)
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0) deSerializeCharArray(ref value);
            else value = null;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapDeSerialize(ref char? value)
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0)
            {
                value = *(char*)Read;
                Read += sizeof(int);
            }
            else value = null;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphDeSerializeMethod]
        private void deSerialize(ref char?[] value)
        {
            var length = deSerializeArray(ref value);
            if (length != 0)
            {
                if ((((length + (31 + 32)) >> 5) << 2) <= (int)(end - Read))
                {
                    value = new char?[length];
                    Read = DeSerialize(Read + sizeof(int), value);
                    if (Read <= end) return;
                }
                Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        private void deSerializeCharArray(ref char?[] value)
        {
            var length = deSerializeArrayMember(ref value);
            if (length != 0)
            {
                var end = Read + currentSize;
                if ((((length + (31 + 64)) >> 5) << 2) <= currentSize && end <= this.end)
                {
                    value = new char?[length];
                    Read = DeSerialize(Read + sizeof(int) * 2, value);
                    if (Read == end) return;
                }
                Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref char?[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerializeCharArray(ref value);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapDeSerialize(ref char?[] value)
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0) deSerializeCharArray(ref value);
            else value = null;
        }

        /// <summary>
        ///     时间反序列化
        /// </summary>
        /// <param name="value">时间</param>
        [TmphDeSerializeMethod]
        private void deSerialize(ref DateTime[] value)
        {
            var length = deSerializeArray(ref value);
            if (length != 0)
            {
                if (length * sizeof(DateTime) + sizeof(int) <= (int)(end - Read))
                {
                    value = new DateTime[length];
                    Read = DeSerialize(Read + sizeof(int), value);
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     时间反序列化
        /// </summary>
        /// <param name="value">时间</param>
        private void deSerializeDateTimeArray(ref DateTime[] value)
        {
            var length = deSerializeArrayMember(ref value);
            if (length != 0)
            {
                if ((length * sizeof(DateTime) + sizeof(int) * 2) == currentSize &&
                    (uint)currentSize <= (uint)(int)(end - Read))
                {
                    value = new DateTime[length];
                    Read = DeSerialize(Read + sizeof(int) * 2, value);
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     时间反序列化
        /// </summary>
        /// <param name="value">时间</param>
        [TmphMemberDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref DateTime[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerializeDateTimeArray(ref value);
        }

        /// <summary>
        ///     时间反序列化
        /// </summary>
        /// <param name="value">时间</param>
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapDeSerialize(ref DateTime[] value)
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0) deSerializeDateTimeArray(ref value);
            else value = null;
        }

        /// <summary>
        ///     时间反序列化
        /// </summary>
        /// <param name="value">时间</param>
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapDeSerialize(ref DateTime? value)
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0)
            {
                value = *(DateTime*)Read;
                Read += sizeof(DateTime);
            }
            else value = null;
        }

        /// <summary>
        ///     时间反序列化
        /// </summary>
        /// <param name="value">时间</param>
        [TmphDeSerializeMethod]
        private void deSerialize(ref DateTime?[] value)
        {
            var length = deSerializeArray(ref value);
            if (length != 0)
            {
                if ((((length + (31 + 32)) >> 5) << 2) <= (int)(end - Read))
                {
                    value = new DateTime?[length];
                    Read = DeSerialize(Read + sizeof(int), value);
                    if (Read <= end) return;
                }
                Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     时间反序列化
        /// </summary>
        /// <param name="value">时间</param>
        private void deSerializeDateTimeArray(ref DateTime?[] value)
        {
            var length = deSerializeArrayMember(ref value);
            if (length != 0)
            {
                var end = Read + currentSize;
                if ((((length + (31 + 64)) >> 5) << 2) <= currentSize && end <= this.end)
                {
                    value = new DateTime?[length];
                    Read = DeSerialize(Read + sizeof(int) * 2, value);
                    if (Read == end) return;
                }
                Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     时间反序列化
        /// </summary>
        /// <param name="value">时间</param>
        [TmphMemberDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref DateTime?[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerializeDateTimeArray(ref value);
        }

        /// <summary>
        ///     时间反序列化
        /// </summary>
        /// <param name="value">时间</param>
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapDeSerialize(ref DateTime?[] value)
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0) deSerializeDateTimeArray(ref value);
            else value = null;
        }

        /// <summary>
        ///     Guid反序列化
        /// </summary>
        /// <param name="value">Guid</param>
        [TmphDeSerializeMethod]
        private void deSerialize(ref Guid[] value)
        {
            var length = deSerializeArray(ref value);
            if (length != 0)
            {
                if (length * sizeof(Guid) + sizeof(int) <= (int)(end - Read))
                {
                    value = new Guid[length];
                    Read = DeSerialize(Read + sizeof(int), value);
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     Guid反序列化
        /// </summary>
        /// <param name="value">Guid</param>
        private void deSerializeGuidArray(ref Guid[] value)
        {
            var length = deSerializeArrayMember(ref value);
            if (length != 0)
            {
                if ((length * sizeof(Guid) + sizeof(int) * 2) == currentSize &&
                    (uint)currentSize <= (uint)(int)(end - Read))
                {
                    value = new Guid[length];
                    Read = DeSerialize(Read + sizeof(int) * 2, value);
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     Guid反序列化
        /// </summary>
        /// <param name="value">Guid</param>
        [TmphMemberDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref Guid[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerializeGuidArray(ref value);
        }

        /// <summary>
        ///     Guid反序列化
        /// </summary>
        /// <param name="value">Guid</param>
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapDeSerialize(ref Guid[] value)
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0) deSerializeGuidArray(ref value);
            else value = null;
        }

        /// <summary>
        ///     Guid反序列化
        /// </summary>
        /// <param name="value">Guid</param>
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapDeSerialize(ref Guid? value)
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0)
            {
                value = *(Guid*)Read;
                Read += sizeof(Guid);
            }
            else value = null;
        }

        /// <summary>
        ///     Guid反序列化
        /// </summary>
        /// <param name="value">Guid</param>
        [TmphDeSerializeMethod]
        private void deSerialize(ref Guid?[] value)
        {
            var length = deSerializeArray(ref value);
            if (length != 0)
            {
                if ((((length + (31 + 32)) >> 5) << 2) <= (int)(end - Read))
                {
                    value = new Guid?[length];
                    Read = DeSerialize(Read + sizeof(int), value);
                    if (Read <= end) return;
                }
                Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     Guid反序列化
        /// </summary>
        /// <param name="value">Guid</param>
        private void deSerializeGuidArray(ref Guid?[] value)
        {
            var length = deSerializeArrayMember(ref value);
            if (length != 0)
            {
                var end = Read + currentSize;
                if ((((length + (31 + 64)) >> 5) << 2) <= currentSize && end <= this.end)
                {
                    value = new Guid?[length];
                    Read = DeSerialize(Read + sizeof(int) * 2, value);
                    if (Read == end) return;
                }
                Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     Guid反序列化
        /// </summary>
        /// <param name="value">Guid</param>
        [TmphMemberDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref Guid?[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerializeGuidArray(ref value);
        }

        /// <summary>
        ///     Guid反序列化
        /// </summary>
        /// <param name="value">Guid</param>
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapDeSerialize(ref Guid?[] value)
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0) deSerializeGuidArray(ref value);
            else value = null;
        }

        /// <summary>
        ///     字符串反序列化
        /// </summary>
        /// <param name="value">字符串</param>
        [TmphDeSerializeMethod]
        private void deSerialize(ref string value)
        {
            var length = *(int*)Read;
            if ((length & 1) == 0)
            {
                if (length != 0)
                {
                    var dataLength = (length + (3 + sizeof(int))) & (int.MaxValue - 3);
                    if (dataLength <= (int)(end - Read))
                    {
                        value = new string((char*)(Read + sizeof(int)), 0, length >> 1);
                        Read += dataLength;
                    }
                    else Error(TmphDeSerializeState.IndexOutOfRange);
                }
                else
                {
                    value = string.Empty;
                    Read += sizeof(int);
                }
            }
            else
            {
                var dataLength = ((length >>= 1) + (3 + sizeof(int))) & (int.MaxValue - 3);
                if (dataLength <= (int)(end - Read))
                {
                    value = TmphString.FastAllocateString(length);
                    fixed (char* valueFixed = value)
                    {
                        var write = valueFixed;
                        byte* readStart = Read + sizeof(int), readEnd = readStart + length;
                        do
                        {
                            *write++ = (char)*readStart++;
                        } while (readStart != readEnd);
                    }
                    Read += dataLength;
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     字符串反序列化
        /// </summary>
        /// <param name="value">字符串</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void deSerializeString(ref string value)
        {
            if ((currentSize = *(int*)Read) == 0)
            {
                value = string.Empty;
                Read += sizeof(int);
                return;
            }
            var end = Read + currentSize;
            if ((Read = DeSerialize(Read + sizeof(int), this.end, ref value)) != end)
                Error(TmphDeSerializeState.IndexOutOfRange);
        }

        /// <summary>
        ///     字符串反序列化
        /// </summary>
        /// <param name="value">字符串</param>
        [TmphMemberDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref string value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerializeString(ref value);
        }

        /// <summary>
        ///     字符串反序列化
        /// </summary>
        /// <param name="value">字符串</param>
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapDeSerialize(ref string value)
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0) deSerializeString(ref value);
            else value = null;
        }

        /// <summary>
        ///     字符串反序列化
        /// </summary>
        /// <param name="value">字符串</param>
        [TmphMemberDeSerializeMethod]
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref TmphSubString value)
        {
            if ((currentSize = *(int*)Read) == 0)
            {
                value = string.Empty;
                Read += sizeof(int);
                return;
            }
            var end = Read + currentSize;
            string stringValue = null;
            if ((Read = DeSerialize(Read + sizeof(int), this.end, ref stringValue)) == end)
                value.UnsafeSet(stringValue, 0, stringValue.Length);
            else Error(TmphDeSerializeState.IndexOutOfRange);
        }

        /// <summary>
        ///     字符串反序列化
        /// </summary>
        /// <param name="value">字符串</param>
        [TmphDeSerializeMethod]
        private void deSerialize(ref string[] value)
        {
            var length = deSerializeArray(ref value);
            if (length != 0)
            {
                var mapLength = ((length + (31 + 32)) >> 5) << 2;
                if (mapLength <= (int)(end - Read))
                {
                    value = new string[length];
                    var arrayMap = new TmphArrayMap(Read + sizeof(int));
                    Read += mapLength;
                    for (var index = 0; index != value.Length; ++index)
                    {
                        if (arrayMap.Next() == 0) value[index] = null;
                        else
                        {
                            deSerialize(ref value[index]);
                            if (state != TmphDeSerializeState.Success) return;
                        }
                    }
                    if (Read <= end) return;
                }
                Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     字符串反序列化
        /// </summary>
        /// <param name="value">字符串</param>
        private void deSerializeStringArray(ref string[] value)
        {
            var length = deSerializeArrayMember(ref value);
            if (length != 0)
            {
                var end = Read + currentSize;
                var mapLength = ((length + (31 + 64)) >> 5) << 2;
                if (mapLength <= currentSize && end <= this.end)
                {
                    value = new string[length];
                    var arrayMap = new TmphArrayMap(Read + sizeof(int) * 2);
                    Read += mapLength;
                    for (var index = 0; index != value.Length; ++index)
                    {
                        if (arrayMap.Next() == 0) value[index] = null;
                        else
                        {
                            deSerialize(ref value[index]);
                            if (state != TmphDeSerializeState.Success) return;
                        }
                    }
                    if (Read == end) return;
                }
                Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     字符串反序列化
        /// </summary>
        /// <param name="value">字符串</param>
        [TmphMemberDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref string[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerializeStringArray(ref value);
        }

        /// <summary>
        ///     字符串反序列化
        /// </summary>
        /// <param name="value">字符串</param>
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapDeSerialize(ref string[] value)
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0) deSerializeStringArray(ref value);
            else value = null;
        }

        /// <summary>
        ///     基类反序列化
        /// </summary>
        /// <param name="deSerializer">二进制数据反序列化</param>
        /// <param name="value">数据对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private static void baseSerialize<TValueType, childType>(TmphIndexDeSerializer deSerializer, ref childType value)
            where childType : TValueType
        {
            TmphTypeDeSerializer<TValueType>.BaseSerialize(deSerializer, ref value);
        }

        /// <summary>
        ///     对象反序列化
        /// </summary>
        /// <param name="value">数据对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void structDeSerialize<TValueType>(ref TValueType value)
        {
            var end = Read + *(int*)Read;
            if (end <= this.end)
            {
                Read += sizeof(int);
                TmphTypeDeSerializer<TValueType>.DeSerialize(this, ref value);
                if (Read == end) return;
            }
            Error(TmphDeSerializeState.IndexOutOfRange);
        }

        /// <summary>
        ///     对象反序列化
        /// </summary>
        /// <param name="value">数据对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void classDeSerialize<TValueType>(ref TValueType value) where TValueType : class
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else structDeSerialize(ref value);
        }

        /// <summary>
        ///     对象反序列化
        /// </summary>
        /// <param name="value">数据对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberMapClassDeSerialize<TValueType>(ref TValueType value) where TValueType : class
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0) structDeSerialize(ref value);
            else value = null;
        }

        /// <summary>
        ///     数组转换
        /// </summary>
        /// <param name="array">数组对象</param>
        private void structArrayType<TValueType>(ref TValueType[] array)
        {
            var length = deSerializeArray(ref array);
            if (length != 0)
            {
                if ((length + 1) * sizeof(int) <= (int)(end - Read))
                {
                    array = new TValueType[length];
                    Read += sizeof(int);
                    for (var index = 0; index != array.Length; ++index)
                    {
                        TmphTypeDeSerializer<TValueType>.StructDeSerialize(this, ref array[index]);
                        if (state != TmphDeSerializeState.Success) return;
                    }
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数组转换
        /// </summary>
        /// <param name="array">数组对象</param>
        private void structArray<TValueType>(ref TValueType[] array)
        {
            var length = deSerializeArrayMember(ref array);
            if (length != 0)
            {
                var end = Read + currentSize;
                if ((length + 2) * sizeof(int) <= currentSize && end <= this.end)
                {
                    array = new TValueType[length];
                    Read += sizeof(int) * 2;
                    for (var index = 0; index != array.Length; ++index)
                    {
                        TmphTypeDeSerializer<TValueType>.StructDeSerialize(this, ref array[index]);
                        if (state != TmphDeSerializeState.Success) return;
                    }
                    if (Read == end) return;
                }
                Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数组转换
        /// </summary>
        /// <param name="array">数组对象</param>
        private void structArrayMember<TValueType>(ref TValueType[] array)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                array = null;
            }
            else structArray(ref array);
        }

        /// <summary>
        ///     数组转换
        /// </summary>
        /// <param name="array">数组对象</param>
        private void structArrayMemberMap<TValueType>(ref TValueType[] array)
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0) structArray(ref array);
            else array = null;
        }

        /// <summary>
        ///     数组转换
        /// </summary>
        /// <param name="array">数组对象</param>
        private void nullableArrayType<TValueType>(ref TValueType[] array) where TValueType : struct
        {
            var length = deSerializeArray(ref array);
            if (length != 0)
            {
                var mapLength = ((length + (31 + 32)) >> 5) << 2;
                if (mapLength <= (int)(end - Read))
                {
                    array = new TValueType[length];
                    var arrayMap = new TmphArrayMap(Read + sizeof(int));
                    Read += mapLength;
                    for (var index = 0; index != array.Length; ++index)
                    {
                        if (arrayMap.Next() == 0) array[index] = default(TValueType);
                        else
                        {
                            var value = default(TValueType);
                            TmphTypeDeSerializer<TValueType>.StructDeSerialize(this, ref value);
                            if (state != TmphDeSerializeState.Success) return;
                            array[index] = value;
                        }
                    }
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数组转换
        /// </summary>
        /// <param name="array">数组对象</param>
        private void nullableArray<TValueType>(ref TValueType[] array) where TValueType : struct
        {
            var length = deSerializeArrayMember(ref array);
            if (length != 0)
            {
                var end = Read + currentSize;
                var mapLength = ((length + (31 + 64)) >> 5) << 2;
                if (mapLength <= currentSize && end <= this.end)
                {
                    array = new TValueType[length];
                    var arrayMap = new TmphArrayMap(Read + sizeof(int) * 2);
                    Read += mapLength;
                    for (var index = 0; index != array.Length; ++index)
                    {
                        if (arrayMap.Next() == 0) array[index] = default(TValueType);
                        else
                        {
                            var value = default(TValueType);
                            TmphTypeDeSerializer<TValueType>.StructDeSerialize(this, ref value);
                            if (state != TmphDeSerializeState.Success) return;
                            array[index] = value;
                        }
                    }
                    if (Read == end) return;
                }
                Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数组转换
        /// </summary>
        /// <param name="array">数组对象</param>
        private void nullableArrayMember<TValueType>(ref TValueType[] array) where TValueType : struct
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                array = null;
            }
            else nullableArray(ref array);
        }

        /// <summary>
        ///     数组转换
        /// </summary>
        /// <param name="array">数组对象</param>
        private void nullableArrayMemberMap<TValueType>(ref TValueType[] array) where TValueType : struct
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0) nullableArray(ref array);
            else array = null;
        }

        /// <summary>
        ///     数组转换
        /// </summary>
        /// <param name="array">数组对象</param>
        private void arrayType<TValueType>(ref TValueType[] array) where TValueType : class
        {
            var length = deSerializeArray(ref array);
            if (length != 0)
            {
                var mapLength = ((length + (31 + 32)) >> 5) << 2;
                if (mapLength <= (int)(end - Read))
                {
                    array = new TValueType[length];
                    var arrayMap = new TmphArrayMap(Read + sizeof(int));
                    Read += mapLength;
                    for (var index = 0; index != array.Length; ++index)
                    {
                        if (arrayMap.Next() == 0) array[index] = null;
                        else
                        {
                            TmphTypeDeSerializer<TValueType>.DeSerialize(this, ref array[index]);
                            if (state != TmphDeSerializeState.Success) return;
                        }
                    }
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数组转换
        /// </summary>
        /// <param name="array">数组对象</param>
        private void array<TValueType>(ref TValueType[] array) where TValueType : class
        {
            var length = deSerializeArrayMember(ref array);
            if (length != 0)
            {
                var end = Read + currentSize;
                var mapLength = (((length + (31 + 64)) >> 5) << 2);
                if (mapLength <= currentSize && end <= this.end)
                {
                    array = new TValueType[length];
                    var arrayMap = new TmphArrayMap(Read + sizeof(int) * 2);
                    Read += mapLength;
                    for (var index = 0; index != array.Length; ++index)
                    {
                        if (arrayMap.Next() == 0) array[index] = null;
                        else
                        {
                            TmphTypeDeSerializer<TValueType>.DeSerialize(this, ref array[index]);
                            if (state != TmphDeSerializeState.Success) return;
                        }
                    }
                    if (Read == end) return;
                }
                Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数组转换
        /// </summary>
        /// <param name="array">数组对象</param>
        private void arrayMember<TValueType>(ref TValueType[] array) where TValueType : class
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                array = null;
            }
            else this.array(ref array);
        }

        /// <summary>
        ///     数组转换
        /// </summary>
        /// <param name="array">数组对象</param>
        private void arrayMemberMap<TValueType>(ref TValueType[] array) where TValueType : class
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0) this.array(ref array);
            else array = null;
        }

        /// <summary>
        ///     对象序列化
        /// </summary>
        /// <param name="value">数据对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void nullableDeSerialize<TValueType>(ref TValueType? value) where TValueType : struct
        {
            var newValue = value.HasValue ? value.Value : default(TValueType);
            TmphTypeDeSerializer<TValueType>.StructDeSerialize(this, ref newValue);
            value = newValue;
        }

        /// <summary>
        ///     对象序列化
        /// </summary>
        /// <param name="value">数据对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void nullableMemberDeSerialize<TValueType>(ref TValueType? value) where TValueType : struct
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else
            {
                var newValue = value.HasValue ? value.Value : default(TValueType);
                structDeSerialize(ref newValue);
                value = newValue;
            }
        }

        /// <summary>
        ///     对象序列化
        /// </summary>
        /// <param name="value">数据对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void nullableMemberMapDeSerialize<TValueType>(ref TValueType? value) where TValueType : struct
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0)
            {
                var newValue = value.HasValue ? value.Value : default(TValueType);
                structDeSerialize(ref newValue);
                value = newValue;
            }
            else value = null;
        }

        /// <summary>
        ///     数组对象序列化
        /// </summary>
        /// <param name="value">数据对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void subArrayDeSerialize<TValueType>(ref TmphSubArray<TValueType> value)
        {
            TValueType[] array = null;
            TmphTypeDeSerializer<TValueType[]>.DefaultDeSerializer(this, ref array);
            value.UnsafeSet(array, 0, array.Length);
        }

        /// <summary>
        ///     字典序列化
        /// </summary>
        /// <param name="value">数据对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private static void keyValuePairDeSerialize<TKeyType, TValueType>(TmphIndexDeSerializer deSerializer,
            ref KeyValuePair<TKeyType, TValueType> value)
        {
            var keyValue = new TmphKeyValue<TKeyType, TValueType>(value.Key, value.Value);
            TmphTypeDeSerializer<TmphKeyValue<TKeyType, TValueType>>.MemberDeSerialize(deSerializer, ref keyValue);
            value = new KeyValuePair<TKeyType, TValueType>(keyValue.Key, keyValue.Value);
        }

        /// <summary>
        ///     字典序列化
        /// </summary>
        /// <param name="value">数据对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberKeyValuePairDeSerialize<TKeyType, TValueType>(ref KeyValuePair<TKeyType, TValueType> value)
        {
            var keyValue = new TmphKeyValue<TKeyType, TValueType>(value.Key, value.Value);
            structDeSerialize(ref keyValue);
            value = new KeyValuePair<TKeyType, TValueType>(keyValue.Key, keyValue.Value);
        }

        /// <summary>
        ///     字典序列化
        /// </summary>
        /// <param name="value">数据对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void dictionaryArrayDeSerialize<TKeyType, TValueType>(IDictionary<TKeyType, TValueType> value)
        {
            TKeyType[] keys = null;
            TmphTypeDeSerializer<TKeyType[]>.DefaultDeSerializer(this, ref keys);
            if (state == TmphDeSerializeState.Success)
            {
                TValueType[] values = null;
                TmphTypeDeSerializer<TValueType[]>.DefaultDeSerializer(this, ref values);
                if (state == TmphDeSerializeState.Success)
                {
                    var index = 0;
                    foreach (var nextValue in values) value.Add(keys[index++], nextValue);
                }
            }
        }

        /// <summary>
        ///     字典序列化
        /// </summary>
        /// <param name="value">数据对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void dictionaryArrayMember<TKeyType, TValueType>(IDictionary<TKeyType, TValueType> value)
        {
            var end = Read + *(int*)Read;
            if (end <= this.end)
            {
                Read += sizeof(int);
                TKeyType[] keys = null;
                TmphTypeDeSerializer<TKeyType[]>.DefaultDeSerializer(this, ref keys);
                if (state != TmphDeSerializeState.Success) return;
                TValueType[] values = null;
                TmphTypeDeSerializer<TValueType[]>.DefaultDeSerializer(this, ref values);
                if (state != TmphDeSerializeState.Success) return;
                if (Read == end)
                {
                    var index = 0;
                    foreach (var nextValue in values) value.Add(keys[index++], nextValue);
                    return;
                }
            }
            Error(TmphDeSerializeState.IndexOutOfRange);
        }

        /// <summary>
        ///     字典序列化
        /// </summary>
        /// <param name="value">数据对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void dictionaryDeSerialize<TKeyType, TValueType>(ref Dictionary<TKeyType, TValueType> value)
        {
            dictionaryArrayDeSerialize(value = TmphDictionary.CreateAny<TKeyType, TValueType>());
        }

        /// <summary>
        ///     字典序列化
        /// </summary>
        /// <param name="value">数据对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void dictionaryMember<TKeyType, TValueType>(ref Dictionary<TKeyType, TValueType> value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else dictionaryArrayMember(value = TmphDictionary.CreateAny<TKeyType, TValueType>());
        }

        /// <summary>
        ///     字典序列化
        /// </summary>
        /// <param name="value">数据对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void dictionaryMemberMap<TKeyType, TValueType>(ref Dictionary<TKeyType, TValueType> value)
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0)
                dictionaryArrayMember(value = TmphDictionary.CreateAny<TKeyType, TValueType>());
            else value = null;
        }

        /// <summary>
        ///     字典序列化
        /// </summary>
        /// <param name="value">数据对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void sortedDictionaryDeSerialize<TKeyType, TValueType>(ref SortedDictionary<TKeyType, TValueType> value)
        {
            dictionaryArrayDeSerialize(value = new SortedDictionary<TKeyType, TValueType>());
        }

        /// <summary>
        ///     字典序列化
        /// </summary>
        /// <param name="value">数据对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void sortedDictionaryMember<TKeyType, TValueType>(ref SortedDictionary<TKeyType, TValueType> value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else dictionaryArrayMember(value = new SortedDictionary<TKeyType, TValueType>());
        }

        /// <summary>
        ///     字典序列化
        /// </summary>
        /// <param name="value">数据对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void sortedDictionaryMemberMap<TKeyType, TValueType>(ref SortedDictionary<TKeyType, TValueType> value)
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0)
                dictionaryArrayMember(value = new SortedDictionary<TKeyType, TValueType>());
            else value = null;
        }

        /// <summary>
        ///     字典序列化
        /// </summary>
        /// <param name="value">数据对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void sortedListDeSerialize<TKeyType, TValueType>(ref SortedList<TKeyType, TValueType> value)
        {
            dictionaryArrayDeSerialize(value = new SortedList<TKeyType, TValueType>());
        }

        /// <summary>
        ///     字典序列化
        /// </summary>
        /// <param name="value">数据对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void sortedListMember<TKeyType, TValueType>(ref SortedList<TKeyType, TValueType> value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else dictionaryArrayMember(value = new SortedList<TKeyType, TValueType>());
        }

        /// <summary>
        ///     字典序列化
        /// </summary>
        /// <param name="value">数据对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void sortedListMemberMap<TKeyType, TValueType>(ref SortedList<TKeyType, TValueType> value)
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0)
                dictionaryArrayMember(value = new SortedList<TKeyType, TValueType>());
            else value = null;
        }

        /// <summary>
        ///     集合构造函数反序列化
        /// </summary>
        /// <param name="value">目标数据</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void arrayConstructor<TValueType, argumentType>(ref TValueType value)
        {
            argumentType[] values = null;
            TmphTypeDeSerializer<argumentType[]>.DefaultDeSerializer(this, ref values);
            if (state == TmphDeSerializeState.Success)
            {
                value = TmphPub.TmphArrayConstructor<TValueType, argumentType>.Constructor(values);
            }
        }

        /// <summary>
        ///     集合构造函数反序列化
        /// </summary>
        /// <param name="value">目标数据</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void listConstructor<TValueType, argumentType>(ref TValueType value)
        {
            argumentType[] values = null;
            TmphTypeDeSerializer<argumentType[]>.DefaultDeSerializer(this, ref values);
            if (state == TmphDeSerializeState.Success)
            {
                value = TmphPub.TmphListConstructor<TValueType, argumentType>.Constructor(values);
            }
        }

        /// <summary>
        ///     集合构造函数反序列化
        /// </summary>
        /// <param name="value">目标数据</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void collectionConstructor<TValueType, argumentType>(ref TValueType value)
        {
            argumentType[] values = null;
            TmphTypeDeSerializer<argumentType[]>.DefaultDeSerializer(this, ref values);
            if (state == TmphDeSerializeState.Success)
            {
                value = TmphPub.TmphListConstructor<TValueType, argumentType>.Constructor(values);
            }
        }

        /// <summary>
        ///     集合构造函数反序列化
        /// </summary>
        /// <param name="value">目标数据</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void enumerableConstructor<TValueType, argumentType>(ref TValueType value)
        {
            argumentType[] values = null;
            TmphTypeDeSerializer<argumentType[]>.DefaultDeSerializer(this, ref values);
            if (state == TmphDeSerializeState.Success)
            {
                value = TmphPub.TmphListConstructor<TValueType, argumentType>.Constructor(values);
            }
        }

        /// <summary>
        ///     集合构造函数反序列化
        /// </summary>
        /// <param name="dictionary">目标数据</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void dictionaryConstructor<dictionaryType, TKeyType, TValueType, argumentType>(
            ref dictionaryType dictionary) where dictionaryType : IDictionary<TKeyType, TValueType>
        {
            TKeyType[] keys = null;
            TmphTypeDeSerializer<TKeyType[]>.DefaultDeSerializer(this, ref keys);
            if (state == TmphDeSerializeState.Success)
            {
                TValueType[] values = null;
                TmphTypeDeSerializer<TValueType[]>.DefaultDeSerializer(this, ref values);
                if (state == TmphDeSerializeState.Success)
                {
                    var index = 0;
                    dictionary = TmphConstructor<dictionaryType>.New();
                    foreach (var value in values) dictionary.Add(keys[index++], value);
                }
            }
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private void enumByteArrayType<TValueType>(ref TValueType[] array)
        {
            var length = deSerializeArray(ref array);
            if (length != 0)
            {
                var dataLength = (length + (3 + sizeof(int))) & (int.MaxValue - 3);
                if (dataLength <= (int)(end - Read))
                {
                    array = new TValueType[length];
                    var data = Read + sizeof(int);
                    for (var index = 0;
                        index != array.Length;
                        array[index++] = TmphPub.TmphEnumCast<TValueType, byte>.FromInt(*data++))
                        ;
                    Read += dataLength;
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private void enumByteArray<TValueType>(ref TValueType[] array)
        {
            var length = deSerializeArrayMember(ref array);
            if (length != 0)
            {
                var end = Read + currentSize;
                if (((length + (3 + sizeof(int) * 2)) & (int.MaxValue - 3)) == currentSize && end <= this.end)
                {
                    array = new TValueType[length];
                    var data = Read + sizeof(int) * 2;
                    for (var index = 0;
                        index != array.Length;
                        array[index++] = TmphPub.TmphEnumCast<TValueType, byte>.FromInt(*data++))
                        ;
                    Read = end;
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private void enumByteArrayMember<TValueType>(ref TValueType[] array)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                array = null;
            }
            else enumByteArray(ref array);
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private void enumByteArrayMemberMap<TValueType>(ref TValueType[] array)
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0) enumByteArray(ref array);
            else array = null;
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private void enumSByteArrayType<TValueType>(ref TValueType[] array)
        {
            var length = deSerializeArray(ref array);
            if (length != 0)
            {
                var dataLength = (length + (3 + sizeof(int))) & (int.MaxValue - 3);
                if (dataLength <= (int)(end - Read))
                {
                    array = new TValueType[length];
                    var data = Read + sizeof(int);
                    for (var index = 0;
                        index != array.Length;
                        array[index++] = TmphPub.TmphEnumCast<TValueType, sbyte>.FromInt((sbyte)*data++))
                        ;
                    Read += dataLength;
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private void enumSByteArray<TValueType>(ref TValueType[] array)
        {
            var length = deSerializeArrayMember(ref array);
            if (length != 0)
            {
                var end = Read + currentSize;
                if (((length + (3 + sizeof(int) * 2)) & (int.MaxValue - 3)) == currentSize && end <= this.end)
                {
                    array = new TValueType[length];
                    var data = Read + sizeof(int) * 2;
                    for (var index = 0;
                        index != array.Length;
                        array[index++] = TmphPub.TmphEnumCast<TValueType, sbyte>.FromInt((sbyte)*data++))
                        ;
                    Read = end;
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private void enumSByteArrayMember<TValueType>(ref TValueType[] array)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                array = null;
            }
            else enumSByteArray(ref array);
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private void enumSByteArrayMemberMap<TValueType>(ref TValueType[] array)
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0) enumSByteArray(ref array);
            else array = null;
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private void enumShortArrayType<TValueType>(ref TValueType[] array)
        {
            var length = deSerializeArray(ref array);
            if (length != 0)
            {
                var dataLength = ((length << 1) + (3 + sizeof(int))) & (int.MaxValue - 3);
                if (dataLength <= (int)(end - Read))
                {
                    array = new TValueType[length];
                    var data = (short*)(Read + sizeof(int));
                    for (var index = 0;
                        index != array.Length;
                        array[index++] = TmphPub.TmphEnumCast<TValueType, short>.FromInt(*data++))
                        ;
                    Read += dataLength;
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private void enumShortArray<TValueType>(ref TValueType[] array)
        {
            var length = deSerializeArrayMember(ref array);
            if (length != 0)
            {
                var end = Read + currentSize;
                if ((((length << 1) + (3 + sizeof(int) * 2)) & (int.MaxValue - 3)) == currentSize && end <= this.end)
                {
                    array = new TValueType[length];
                    var data = (short*)(Read + sizeof(int) * 2);
                    for (var index = 0;
                        index != array.Length;
                        array[index++] = TmphPub.TmphEnumCast<TValueType, short>.FromInt(*data++))
                        ;
                    Read = end;
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private void enumShortArrayMember<TValueType>(ref TValueType[] array)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                array = null;
            }
            else enumShortArray(ref array);
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private void enumShortArrayMemberMap<TValueType>(ref TValueType[] array)
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0) enumShortArray(ref array);
            else array = null;
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private void enumUShortArrayType<TValueType>(ref TValueType[] array)
        {
            var length = deSerializeArray(ref array);
            if (length != 0)
            {
                var dataLength = ((length << 1) + (3 + sizeof(int))) & (int.MaxValue - 3);
                if (dataLength <= (int)(end - Read))
                {
                    array = new TValueType[length];
                    var data = (ushort*)(Read + sizeof(int));
                    for (var index = 0;
                        index != array.Length;
                        array[index++] = TmphPub.TmphEnumCast<TValueType, ushort>.FromInt(*data++))
                        ;
                    Read += dataLength;
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private void enumUShortArray<TValueType>(ref TValueType[] array)
        {
            var length = deSerializeArrayMember(ref array);
            if (length != 0)
            {
                var end = Read + currentSize;
                if ((((length << 1) + (3 + sizeof(int) * 2)) & (int.MaxValue - 3)) == currentSize && end <= this.end)
                {
                    array = new TValueType[length];
                    var data = (ushort*)(Read + sizeof(int) * 2);
                    for (var index = 0;
                        index != array.Length;
                        array[index++] = TmphPub.TmphEnumCast<TValueType, ushort>.FromInt(*data++))
                        ;
                    Read = end;
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private void enumUShortArrayMember<TValueType>(ref TValueType[] array)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                array = null;
            }
            else enumUShortArray(ref array);
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private void enumUShortArrayMemberMap<TValueType>(ref TValueType[] array)
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0) enumUShortArray(ref array);
            else array = null;
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private void enumIntArrayType<TValueType>(ref TValueType[] array)
        {
            var length = deSerializeArray(ref array);
            if (length != 0)
            {
                if ((length + 1) * sizeof(int) <= (int)(end - Read))
                {
                    array = new TValueType[length];
                    Read += sizeof(int);
                    for (var index = 0; index != array.Length; Read += sizeof(int))
                        array[index++] = TmphPub.TmphEnumCast<TValueType, int>.FromInt(*(int*)Read);
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private void enumIntArray<TValueType>(ref TValueType[] array)
        {
            var length = deSerializeArrayMember(ref array);
            if (length != 0)
            {
                var end = Read + currentSize;
                if ((length + 2) * sizeof(int) == currentSize && end <= this.end)
                {
                    array = new TValueType[length];
                    Read += sizeof(int) * 2;
                    for (var index = 0; index != array.Length; Read += sizeof(int))
                        array[index++] = TmphPub.TmphEnumCast<TValueType, int>.FromInt(*(int*)Read);
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private void enumIntArrayMember<TValueType>(ref TValueType[] array)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                array = null;
            }
            else enumIntArray(ref array);
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private void enumIntArrayMemberMap<TValueType>(ref TValueType[] array)
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0) enumIntArray(ref array);
            else array = null;
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private void enumUIntArrayType<TValueType>(ref TValueType[] array)
        {
            var length = deSerializeArray(ref array);
            if (length != 0)
            {
                if ((length + 1) * sizeof(int) <= (int)(end - Read))
                {
                    array = new TValueType[length];
                    Read += sizeof(int);
                    for (var index = 0; index != array.Length; Read += sizeof(uint))
                        array[index++] = TmphPub.TmphEnumCast<TValueType, uint>.FromInt(*(uint*)Read);
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private void enumUIntArray<TValueType>(ref TValueType[] array)
        {
            var length = deSerializeArrayMember(ref array);
            if (length != 0)
            {
                var end = Read + currentSize;
                if ((length + 2) * sizeof(int) == currentSize && end <= this.end)
                {
                    array = new TValueType[length];
                    Read += sizeof(int) * 2;
                    for (var index = 0; index != array.Length; Read += sizeof(uint))
                        array[index++] = TmphPub.TmphEnumCast<TValueType, uint>.FromInt(*(uint*)Read);
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private void enumUIntArrayMember<TValueType>(ref TValueType[] array)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                array = null;
            }
            else enumUIntArray(ref array);
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private void enumUIntArrayMemberMap<TValueType>(ref TValueType[] array)
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0) enumUIntArray(ref array);
            else array = null;
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private void enumLongArrayType<TValueType>(ref TValueType[] array)
        {
            var length = deSerializeArray(ref array);
            if (length != 0)
            {
                if (length * sizeof(long) + sizeof(int) <= (int)(end - Read))
                {
                    array = new TValueType[length];
                    Read += sizeof(int);
                    for (var index = 0; index != array.Length; Read += sizeof(long))
                        array[index++] = TmphPub.TmphEnumCast<TValueType, long>.FromInt(*(long*)Read);
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private void enumLongArray<TValueType>(ref TValueType[] array)
        {
            var length = deSerializeArrayMember(ref array);
            if (length != 0)
            {
                var end = Read + currentSize;
                if (length * sizeof(long) + sizeof(int) * 2 == currentSize && end <= this.end)
                {
                    array = new TValueType[length];
                    Read += sizeof(int) * 2;
                    for (var index = 0; index != array.Length; Read += sizeof(long))
                        array[index++] = TmphPub.TmphEnumCast<TValueType, long>.FromInt(*(long*)Read);
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private void enumLongArrayMember<TValueType>(ref TValueType[] array)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                array = null;
            }
            else enumLongArray(ref array);
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private void enumLongArrayMemberMap<TValueType>(ref TValueType[] array)
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0) enumLongArray(ref array);
            else array = null;
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private void enumULongArrayType<TValueType>(ref TValueType[] array)
        {
            var length = deSerializeArray(ref array);
            if (length != 0)
            {
                if (length * sizeof(ulong) + sizeof(int) <= (int)(end - Read))
                {
                    array = new TValueType[length];
                    Read += sizeof(int);
                    for (var index = 0; index != array.Length; Read += sizeof(ulong))
                        array[index++] = TmphPub.TmphEnumCast<TValueType, ulong>.FromInt(*(ulong*)Read);
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private void enumULongArray<TValueType>(ref TValueType[] array)
        {
            var length = deSerializeArrayMember(ref array);
            if (length != 0)
            {
                var end = Read + currentSize;
                if (length * sizeof(ulong) + sizeof(int) * 2 == currentSize && end <= this.end)
                {
                    array = new TValueType[length];
                    Read += sizeof(int) * 2;
                    for (var index = 0; index != array.Length; Read += sizeof(ulong))
                        array[index++] = TmphPub.TmphEnumCast<TValueType, ulong>.FromInt(*(ulong*)Read);
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private void enumULongArrayMember<TValueType>(ref TValueType[] array)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                array = null;
            }
            else enumULongArray(ref array);
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private void enumULongArrayMemberMap<TValueType>(ref TValueType[] array)
        {
            if ((*(uint*)(Read - sizeof(int)) & 0x80000000U) == 0) enumULongArray(ref array);
            else array = null;
        }

        /// <summary>
        ///     反序列化
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="data"></param>
        /// <param name="TmphConfig"></param>
        /// <returns></returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static TValueType DeSerialize<TValueType>(byte[] data, TmphConfig TmphConfig = null)
        {
            if (data != null)
            {
                fixed (byte* dataFixed = data) return DeSerialize<TValueType>(dataFixed, data.Length, TmphConfig);
            }
            if (TmphConfig != null) TmphConfig.State = TmphDeSerializeState.UnknownData;
            return default(TValueType);
        }

        /// <summary>
        ///     反序列化
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="data"></param>
        /// <param name="TmphConfig"></param>
        /// <returns></returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static TValueType DeSerialize<TValueType>(TmphSubArray<byte> data, TmphConfig TmphConfig = null)
        {
            if (data.Count != 0)
            {
                fixed (byte* dataFixed = data.array)
                    return DeSerialize<TValueType>(dataFixed + data.StartIndex, data.Count, TmphConfig);
            }
            if (TmphConfig != null) TmphConfig.State = TmphDeSerializeState.UnknownData;
            return default(TValueType);
        }

        /// <summary>
        ///     反序列化
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="data"></param>
        /// <param name="startIndex"></param>
        /// <param name="TmphConfig"></param>
        /// <returns></returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static TValueType DeSerialize<TValueType>(TmphUnmanagedStream data, int startIndex = 0,
            TmphConfig TmphConfig = null)
        {
            if (data != null && startIndex >= 0)
            {
                return DeSerialize<TValueType>(data.Data + startIndex, data.Length - startIndex, TmphConfig);
            }
            if (TmphConfig != null) TmphConfig.State = TmphDeSerializeState.UnknownData;
            return default(TValueType);
        }

        /// <summary>
        ///     反序列化
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="data"></param>
        /// <param name="size"></param>
        /// <param name="TmphConfig"></param>
        /// <returns></returns>
        public static TValueType DeSerialize<TValueType>(byte* data, int size, TmphConfig TmphConfig = null)
        {
            if (TmphConfig == null) TmphConfig = defaultConfig;
            var length = size - sizeof(int);
            if (length >= 0 && data != null)
            {
                if (TmphConfig.IsFullData)
                {
                    if ((size & 3) == 0)
                    {
                        if (length != 0)
                        {
                            var end = data + length;
                            if (*(int*)end == length)
                            {
                                if ((*(uint*)data & TmphBinarySerializer.TmphConfig.HeaderMapAndValue) ==
                                    TmphBinarySerializer.TmphConfig.HeaderMapValue)
                                {
                                    var value = default(TValueType);
                                    var deSerializer = TmphTypePool<TmphIndexDeSerializer>.Pop() ??
                                                       new TmphIndexDeSerializer();
                                    try
                                    {
                                        return deSerializer.deSerialize(data, end, ref value, TmphConfig) ==
                                               TmphDeSerializeState.Success
                                            ? value
                                            : default(TValueType);
                                    }
                                    finally
                                    {
                                        deSerializer.free();
                                    }
                                }
                                TmphConfig.State = TmphDeSerializeState.HeaderError;
                                return default(TValueType);
                            }
                            TmphConfig.State = TmphDeSerializeState.EndVerify;
                            return default(TValueType);
                        }
                        if (*(int*)data == TmphBinarySerializer.NullValue)
                        {
                            TmphConfig.State = TmphDeSerializeState.Success;
                            return default(TValueType);
                        }
                    }
                }
                else
                {
                    if ((*(uint*)data & TmphBinarySerializer.TmphConfig.HeaderMapAndValue) ==
                        TmphBinarySerializer.TmphConfig.HeaderMapValue)
                    {
                        var value = default(TValueType);
                        var deSerializer = TmphTypePool<TmphIndexDeSerializer>.Pop() ?? new TmphIndexDeSerializer();
                        try
                        {
                            return deSerializer.deSerialize(data, data + length, ref value, TmphConfig) ==
                                   TmphDeSerializeState.Success
                                ? value
                                : default(TValueType);
                        }
                        finally
                        {
                            deSerializer.free();
                        }
                    }
                    if (*(int*)data == TmphBinarySerializer.NullValue)
                    {
                        TmphConfig.State = TmphDeSerializeState.Success;
                        TmphConfig.DataLength = sizeof(int);
                        return default(TValueType);
                    }
                    TmphConfig.State = TmphDeSerializeState.HeaderError;
                    return default(TValueType);
                }
            }
            TmphConfig.State = TmphDeSerializeState.UnknownData;
            return default(TValueType);
        }

        /// <summary>
        ///     反序列化
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="data"></param>
        /// <param name="value"></param>
        /// <param name="TmphConfig"></param>
        /// <returns>是否成功</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static bool DeSerialize<TValueType>(byte[] data, ref TValueType value, TmphConfig TmphConfig = null)
        {
            if (data != null)
            {
                fixed (byte* dataFixed = data) return DeSerialize(dataFixed, data.Length, ref value, TmphConfig);
            }
            if (TmphConfig != null) TmphConfig.State = TmphDeSerializeState.UnknownData;
            return false;
        }

        /// <summary>
        ///     反序列化
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="data"></param>
        /// <param name="value"></param>
        /// <param name="TmphConfig"></param>
        /// <returns>是否成功</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static bool DeSerialize<TValueType>(TmphSubArray<byte> data, ref TValueType value, TmphConfig TmphConfig = null)
        {
            if (data.Count != 0)
            {
                fixed (byte* dataFixed = data.array)
                    return DeSerialize(dataFixed + data.StartIndex, data.Count, ref value, TmphConfig);
            }
            if (TmphConfig != null) TmphConfig.State = TmphDeSerializeState.UnknownData;
            return false;
        }

        /// <summary>
        ///     反序列化
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="data"></param>
        /// <param name="startIndex"></param>
        /// <param name="value"></param>
        /// <param name="TmphConfig"></param>
        /// <returns>是否成功</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static bool DeSerialize<TValueType>(TmphUnmanagedStream data, ref TValueType value, int startIndex = 0,
            TmphConfig TmphConfig = null)
        {
            if (data != null && startIndex >= 0)
            {
                return DeSerialize(data.Data + startIndex, data.Length - startIndex, ref value, TmphConfig);
            }
            if (TmphConfig != null) TmphConfig.State = TmphDeSerializeState.UnknownData;
            return false;
        }

        /// <summary>
        ///     反序列化
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="data"></param>
        /// <param name="size"></param>
        /// <param name="value"></param>
        /// <param name="TmphConfig"></param>
        /// <returns>是否成功</returns>
        public static bool DeSerialize<TValueType>(byte* data, int size, ref TValueType value, TmphConfig TmphConfig = null)
        {
            if (TmphConfig == null) TmphConfig = defaultConfig;
            var length = size - sizeof(int);
            if (length >= 0)
            {
                if (TmphConfig.IsFullData)
                {
                    if ((size & 3) == 0)
                    {
                        if (length != 0)
                        {
                            var end = data + length;
                            if (*(int*)end == length)
                            {
                                if ((*(uint*)data & TmphBinarySerializer.TmphConfig.HeaderMapAndValue) ==
                                    TmphBinarySerializer.TmphConfig.HeaderMapValue)
                                {
                                    var deSerializer = TmphTypePool<TmphIndexDeSerializer>.Pop() ??
                                                       new TmphIndexDeSerializer();
                                    try
                                    {
                                        return deSerializer.deSerialize(data, end, ref value, TmphConfig) ==
                                               TmphDeSerializeState.Success;
                                    }
                                    finally
                                    {
                                        deSerializer.free();
                                    }
                                }
                                TmphConfig.State = TmphDeSerializeState.HeaderError;
                                return false;
                            }
                            TmphConfig.State = TmphDeSerializeState.EndVerify;
                            return false;
                        }
                        if (*(int*)data == TmphBinarySerializer.NullValue)
                        {
                            TmphConfig.State = TmphDeSerializeState.Success;
                            value = default(TValueType);
                            return true;
                        }
                    }
                }
                else
                {
                    if ((*(uint*)data & TmphBinarySerializer.TmphConfig.HeaderMapAndValue) ==
                        TmphBinarySerializer.TmphConfig.HeaderMapValue)
                    {
                        var deSerializer = TmphTypePool<TmphIndexDeSerializer>.Pop() ?? new TmphIndexDeSerializer();
                        try
                        {
                            return deSerializer.deSerialize(data, data + length, ref value, TmphConfig) ==
                                   TmphDeSerializeState.Success;
                        }
                        finally
                        {
                            deSerializer.free();
                        }
                    }
                    if (*(int*)data == TmphBinarySerializer.NullValue)
                    {
                        TmphConfig.State = TmphDeSerializeState.Success;
                        TmphConfig.DataLength = sizeof(int);
                        value = default(TValueType);
                        return true;
                    }
                    TmphConfig.State = TmphDeSerializeState.HeaderError;
                    return false;
                }
            }
            TmphConfig.State = TmphDeSerializeState.UnknownData;
            return false;
        }

        /// <summary>
        ///     获取基本类型反序列化函数
        /// </summary>
        /// <param name="type">基本类型</param>
        /// <returns>反序列化函数</returns>
        private static MethodInfo getDeSerializeMethod(Type type)
        {
            MethodInfo method;
            if (deSerializeMethods.TryGetValue(type, out method))
            {
                deSerializeMethods.Remove(type);
                return method;
            }
            return null;
        }

        /// <summary>
        ///     获取基本类型转换函数
        /// </summary>
        /// <param name="type">基本类型</param>
        /// <returns>转换函数</returns>
        private static MethodInfo getMemberDeSerializeMethod(Type type)
        {
            MethodInfo method;
            return memberDeSerializeMethods.TryGetValue(type, out method) ? method : null;
        }

        /// <summary>
        ///     获取基本类型转换函数
        /// </summary>
        /// <param name="type">基本类型</param>
        /// <returns>转换函数</returns>
        private static MethodInfo getMemberMapDeSerializeMethod(Type type)
        {
            MethodInfo method;
            return memberMapDeSerializeMethods.TryGetValue(type, out method) ? method : null;
        }

        /// <summary>
        ///     基本类型反序列化函数
        /// </summary>
        internal sealed class TmphDeSerializeMethod : Attribute
        {
        }

        /// <summary>
        ///     基本类型反序列化函数
        /// </summary>
        internal sealed class TmphMemberDeSerializeMethod : Attribute
        {
        }

        /// <summary>
        ///     基本类型反序列化函数
        /// </summary>
        internal sealed class TmphMemberMapDeSerializeMethod : Attribute
        {
        }

        /// <summary>
        ///     二进制数据反序列化
        /// </summary>
        internal static class TmphTypeDeSerializer
        {
            /// <summary>
            ///     未知类型反序列化调用函数信息集合
            /// </summary>
            private static TmphInterlocked.TmphDictionary<Type, MethodInfo> memberDeSerializers =
                new TmphInterlocked.TmphDictionary<Type, MethodInfo>(TmphDictionary.CreateOnly<Type, MethodInfo>());

            /// <summary>
            ///     未知类型反序列化调用函数信息集合
            /// </summary>
            private static TmphInterlocked.TmphDictionary<Type, MethodInfo> memberMapDeSerializers =
                new TmphInterlocked.TmphDictionary<Type, MethodInfo>(TmphDictionary.CreateOnly<Type, MethodInfo>());

            /// <summary>
            ///     未知类型枚举反序列化委托调用函数信息
            /// </summary>
            /// <param name="type">数组类型</param>
            /// <returns>未知类型反序列化委托调用函数信息</returns>
            private static MethodInfo getMemberDeSerializer(Type type)
            {
                MethodInfo method;
                if (memberDeSerializers.TryGetValue(type, out method)) return method;
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
                    if (TEnumType == typeof(uint)) method = enumUIntMethod;
                    else if (TEnumType == typeof(byte)) method = enumByteMemberMethod;
                    else if (TEnumType == typeof(ulong)) method = enumULongMethod;
                    else if (TEnumType == typeof(ushort)) method = enumUShortMemberMethod;
                    else if (TEnumType == typeof(long)) method = enumLongMethod;
                    else if (TEnumType == typeof(short)) method = enumShortMemberMethod;
                    else if (TEnumType == typeof(sbyte)) method = enumSByteMemberMethod;
                    else method = enumIntMethod;
                    method = method.MakeGenericMethod(type);
                }
                else
                {
                    if (type.IsGenericType)
                    {
                        var genericType = type.GetGenericTypeDefinition();
                        if (genericType == typeof(Dictionary<,>))
                        {
                            method = dictionaryMemberMethod.MakeGenericMethod(type.GetGenericArguments());
                        }
                        else if (genericType == typeof(Nullable<>))
                        {
                            method = nullableMemberDeSerializeMethod.MakeGenericMethod(type.GetGenericArguments());
                        }
                        else if (genericType == typeof(KeyValuePair<,>))
                        {
                            method = memberKeyValuePairDeSerializeMethod.MakeGenericMethod(type.GetGenericArguments());
                        }
                        else if (genericType == typeof(SortedDictionary<,>))
                        {
                            method = sortedDictionaryMemberMethod.MakeGenericMethod(type.GetGenericArguments());
                        }
                        else if (genericType == typeof(SortedList<,>))
                        {
                            method = sortedListMemberMethod.MakeGenericMethod(type.GetGenericArguments());
                        }
                    }
                    if (method == null)
                    {
                        if (type.IsValueType) method = structDeSerializeMethod.MakeGenericMethod(type);
                        else method = classDeSerializeMethod.MakeGenericMethod(type);
                    }
                }
                memberDeSerializers.Set(type, method);
                return method;
            }

            /// <summary>
            ///     未知类型枚举反序列化委托调用函数信息
            /// </summary>
            /// <param name="type">数组类型</param>
            /// <returns>未知类型反序列化委托调用函数信息</returns>
            private static MethodInfo getMemberMapDeSerializer(Type type)
            {
                MethodInfo method;
                if (memberMapDeSerializers.TryGetValue(type, out method)) return method;
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
                    if (TEnumType == typeof(uint)) method = enumUIntMethod;
                    else if (TEnumType == typeof(byte)) method = enumByteMemberMapMethod;
                    else if (TEnumType == typeof(ulong)) method = enumULongMethod;
                    else if (TEnumType == typeof(ushort)) method = enumUShortMemberMapMethod;
                    else if (TEnumType == typeof(long)) method = enumLongMethod;
                    else if (TEnumType == typeof(short)) method = enumShortMemberMapMethod;
                    else if (TEnumType == typeof(sbyte)) method = enumSByteMemberMapMethod;
                    else method = enumIntMethod;
                    method = method.MakeGenericMethod(type);
                }
                else
                {
                    if (type.IsGenericType)
                    {
                        var genericType = type.GetGenericTypeDefinition();
                        if (genericType == typeof(Dictionary<,>))
                        {
                            method = dictionaryMemberMapMethod.MakeGenericMethod(type.GetGenericArguments());
                        }
                        else if (genericType == typeof(Nullable<>))
                        {
                            method = nullableMemberMapDeSerializeMethod.MakeGenericMethod(type.GetGenericArguments());
                        }
                        else if (genericType == typeof(KeyValuePair<,>))
                        {
                            method = memberKeyValuePairDeSerializeMethod.MakeGenericMethod(type.GetGenericArguments());
                        }
                        else if (genericType == typeof(SortedDictionary<,>))
                        {
                            method = sortedDictionaryMemberMapMethod.MakeGenericMethod(type.GetGenericArguments());
                        }
                        else if (genericType == typeof(SortedList<,>))
                        {
                            method = sortedListMemberMapMethod.MakeGenericMethod(type.GetGenericArguments());
                        }
                    }
                    if (method == null)
                    {
                        if (type.IsValueType) method = structDeSerializeMethod.MakeGenericMethod(type);
                        else method = memberMapClassDeSerializeMethod.MakeGenericMethod(type);
                    }
                }
                memberMapDeSerializers.Set(type, method);
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
                    dynamicMethod = new DynamicMethod("indexDeSerializer", null,
                        new[] { typeof(TmphIndexDeSerializer), type.MakeByRefType() }, type, true);
                    generator = dynamicMethod.GetILGenerator();
                    isValueType = type.IsValueType;
                }

                /// <summary>
                ///     添加字段
                /// </summary>
                /// <param name="field">字段信息</param>
                public void Push(TmphBinarySerializer.TmphFieldInfo field)
                {
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldarg_1);
                    if (!isValueType) generator.Emit(OpCodes.Ldind_Ref);
                    generator.Emit(OpCodes.Ldflda, field.Field);
                    var method = getMemberDeSerializeMethod(field.Field.FieldType) ??
                                 getMemberDeSerializer(field.Field.FieldType);
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
                ///     返回结束反序列化
                /// </summary>
                private readonly Label returnLabel;

                /// <summary>
                ///     动态函数
                /// </summary>
                /// <param name="type"></param>
                /// <param name="name">成员类型</param>
                public TmphMemberMapDynamicMethod(Type type)
                {
                    dynamicMethod = new DynamicMethod("indexMemberMapDeSerializer", null,
                        new[] { typeof(TmphMemberMap), typeof(TmphIndexDeSerializer), type.MakeByRefType() }, type, true);
                    generator = dynamicMethod.GetILGenerator();
                    returnLabel = generator.DefineLabel();
                    isValueType = type.IsValueType;
                }

                /// <summary>
                ///     添加字段
                /// </summary>
                /// <param name="field">字段信息</param>
                public void Push(TmphBinarySerializer.TmphFieldInfo field)
                {
                    var end = generator.DefineLabel();
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldc_I4, field.MemberIndex);
                    generator.Emit(OpCodes.Callvirt, TmphPub.MemberMapIsMemberMethod);
                    generator.Emit(OpCodes.Brfalse_S, end);

                    generator.Emit(OpCodes.Ldarg_1);
                    generator.Emit(OpCodes.Ldc_I4, field.MemberIndex);
                    generator.Emit(OpCodes.Call, isMemberIndexMethod);
                    generator.Emit(OpCodes.Brfalse, returnLabel);

                    generator.Emit(OpCodes.Ldarg_1);
                    generator.Emit(OpCodes.Ldarg_2);
                    if (!isValueType) generator.Emit(OpCodes.Ldind_Ref);
                    generator.Emit(OpCodes.Ldflda, field.Field);
                    var method = getMemberMapDeSerializeMethod(field.Field.FieldType) ??
                                 getMemberMapDeSerializer(field.Field.FieldType);
                    generator.Emit(method.IsFinal || !method.IsVirtual ? OpCodes.Call : OpCodes.Callvirt, method);

                    generator.MarkLabel(end);
                }

                /// <summary>
                ///     创建成员转换委托
                /// </summary>
                /// <returns>成员转换委托</returns>
                public Delegate Create<delegateType>()
                {
                    generator.MarkLabel(returnLabel);
                    generator.Emit(OpCodes.Ret);
                    return dynamicMethod.CreateDelegate(typeof(delegateType));
                }
            }
        }

        /// <summary>
        ///     二进制数据反序列化
        /// </summary>
        /// <typeparam name="TValueType">对象类型</typeparam>
        internal static class TmphTypeDeSerializer<TValueType>
        {
            /// <summary>
            ///     二进制数据序列化类型配置
            /// </summary>
            private static readonly TmphIndexSerialize attribute;

            /// <summary>
            ///     反序列化委托
            /// </summary>
            internal static readonly TmphDeSerialize DefaultDeSerializer;

            /// <summary>
            ///     固定分组成员序列化
            /// </summary>
            private static readonly TmphDeSerialize fixedMemberDeSerializer;

            /// <summary>
            ///     固定分组成员位图序列化
            /// </summary>
            private static readonly TmphMemberMapDeSerialize fixedMemberMapDeSerializer;

            /// <summary>
            ///     成员序列化
            /// </summary>
            private static readonly TmphDeSerialize memberDeSerializer;

            /// <summary>
            ///     成员位图序列化
            /// </summary>
            private static readonly TmphMemberMapDeSerialize memberMapDeSerializer;

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

            /// <summary>
            ///     是否值类型
            /// </summary>
            private static readonly bool isValueType;

            static TmphTypeDeSerializer()
            {
                Type type = typeof(TValueType), TAttributeType;
                var methodInfo = getDeSerializeMethod(type);
                attribute = type.customAttribute<TmphIndexSerialize>(out TAttributeType, true) ?? TmphIndexSerialize.Default;
                if (methodInfo != null)
                {
                    var dynamicMethod = new DynamicMethod("indexDeSerializer", typeof(void),
                        new[] { typeof(TmphIndexDeSerializer), type.MakeByRefType() }, true);
                    dynamicMethod.InitLocals = true;
                    var generator = dynamicMethod.GetILGenerator();
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldarg_1);
                    generator.Emit(OpCodes.Call, methodInfo);
                    generator.Emit(OpCodes.Ret);
                    DefaultDeSerializer = (TmphDeSerialize)dynamicMethod.CreateDelegate(typeof(TmphDeSerialize));
                    isValueType = true;
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
                                    if (TEnumType == typeof(uint))
                                        methodInfo = enumUIntArrayMethod.MakeGenericMethod(elementType);
                                    else if (TEnumType == typeof(byte))
                                        methodInfo = enumByteArrayMethod.MakeGenericMethod(elementType);
                                    else if (TEnumType == typeof(ulong))
                                        methodInfo = enumULongArrayMethod.MakeGenericMethod(elementType);
                                    else if (TEnumType == typeof(ushort))
                                        methodInfo = enumUShortArrayMethod.MakeGenericMethod(elementType);
                                    else if (TEnumType == typeof(long))
                                        methodInfo = enumLongArrayMethod.MakeGenericMethod(elementType);
                                    else if (TEnumType == typeof(short))
                                        methodInfo =
                                            enumShortArrayMethod.MakeGenericMethod(elementType);
                                    else if (TEnumType == typeof(sbyte))
                                        methodInfo =
                                            enumSByteArrayMethod.MakeGenericMethod(elementType);
                                    else
                                        methodInfo =
                                            enumIntArrayMethod.MakeGenericMethod(elementType);
                                }
                                else if (elementType.IsGenericType &&
                                         elementType.GetGenericTypeDefinition() == typeof(Nullable<>))
                                {
                                    methodInfo =
                                        nullableArrayMethod.MakeGenericMethod(elementType.GetGenericArguments());
                                }
                                else methodInfo = structArrayMethod.MakeGenericMethod(elementType);
                            }
                            else methodInfo = arrayMethod.MakeGenericMethod(elementType);
                            DefaultDeSerializer =
                                (TmphDeSerialize)Delegate.CreateDelegate(typeof(TmphDeSerialize), methodInfo);
                            return;
                        }
                    }
                    DefaultDeSerializer = fromNull;
                    return;
                }
                if (type.IsEnum)
                {
                    var TEnumType = Enum.GetUnderlyingType(type);
                    if (TEnumType == typeof(uint)) DefaultDeSerializer = enumUInt;
                    else if (TEnumType == typeof(byte)) DefaultDeSerializer = enumByte;
                    else if (TEnumType == typeof(ulong)) DefaultDeSerializer = enumULong;
                    else if (TEnumType == typeof(ushort)) DefaultDeSerializer = enumUShort;
                    else if (TEnumType == typeof(long)) DefaultDeSerializer = enumLong;
                    else if (TEnumType == typeof(short)) DefaultDeSerializer = enumShort;
                    else if (TEnumType == typeof(sbyte)) DefaultDeSerializer = enumSByte;
                    else DefaultDeSerializer = enumInt;
                    isValueType = true;
                    return;
                }
                if (type.IsPointer || type.IsAbstract || type.IsInterface)
                {
                    DefaultDeSerializer = fromNull;
                    return;
                }
                if (type.IsGenericType)
                {
                    var genericType = type.GetGenericTypeDefinition();
                    if (genericType == typeof(TmphSubArray<>))
                    {
                        DefaultDeSerializer =
                            (TmphDeSerialize)
                                Delegate.CreateDelegate(typeof(TmphDeSerialize),
                                    subArrayDeSerializeMethod.MakeGenericMethod(type.GetGenericArguments()));
                        isValueType = true;
                        return;
                    }
                    if (genericType == typeof(Dictionary<,>))
                    {
                        DefaultDeSerializer =
                            (TmphDeSerialize)
                                Delegate.CreateDelegate(typeof(TmphDeSerialize),
                                    dictionaryDeSerializeMethod.MakeGenericMethod(type.GetGenericArguments()));
                        isValueType = true;
                        return;
                    }
                    if (genericType == typeof(TmphNullValue<>))
                    {
                        DefaultDeSerializer =
                            (TmphDeSerialize)
                                Delegate.CreateDelegate(typeof(TmphDeSerialize),
                                    nullableDeSerializeMethod.MakeGenericMethod(type.GetGenericArguments()));
                        isValueType = true;
                        return;
                    }
                    if (genericType == typeof(KeyValuePair<,>))
                    {
                        DefaultDeSerializer =
                            (TmphDeSerialize)
                                Delegate.CreateDelegate(typeof(TmphDeSerialize),
                                    keyValuePairDeSerializeMethod.MakeGenericMethod(type.GetGenericArguments()));
                        isValueType = true;
                        return;
                    }
                    if (genericType == typeof(SortedDictionary<,>))
                    {
                        DefaultDeSerializer =
                            (TmphDeSerialize)
                                Delegate.CreateDelegate(typeof(TmphDeSerialize),
                                    sortedDictionaryDeSerializeMethod.MakeGenericMethod(type.GetGenericArguments()));
                        isValueType = true;
                        return;
                    }
                    if (genericType == typeof(SortedList<,>))
                    {
                        DefaultDeSerializer =
                            (TmphDeSerialize)
                                Delegate.CreateDelegate(typeof(TmphDeSerialize),
                                    sortedListDeSerializeMethod.MakeGenericMethod(type.GetGenericArguments()));
                        isValueType = true;
                        return;
                    }
                }
                foreach (var interfaceType in type.GetInterfaces())
                {
                    if (interfaceType.IsGenericType)
                    {
                        var genericType = interfaceType.GetGenericTypeDefinition();
                        if (genericType == typeof(ICollection<>))
                        {
                            var parameters = interfaceType.GetGenericArguments();
                            var argumentType = parameters[0];
                            parameters[0] = argumentType.MakeArrayType();
                            var constructorInfo =
                                type.GetConstructor(
                                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null,
                                    parameters, null);
                            if (constructorInfo != null)
                            {
                                methodInfo = arrayConstructorMethod.MakeGenericMethod(type, argumentType);
                                break;
                            }
                            parameters[0] = typeof(IList<>).MakeGenericType(argumentType);
                            constructorInfo =
                                type.GetConstructor(
                                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null,
                                    parameters, null);
                            if (constructorInfo != null)
                            {
                                methodInfo = listConstructorMethod.MakeGenericMethod(type, argumentType);
                                break;
                            }
                            parameters[0] = typeof(ICollection<>).MakeGenericType(argumentType);
                            constructorInfo =
                                type.GetConstructor(
                                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null,
                                    parameters, null);
                            if (constructorInfo != null)
                            {
                                methodInfo = collectionConstructorMethod.MakeGenericMethod(type, argumentType);
                                break;
                            }
                            parameters[0] = typeof(IEnumerable<>).MakeGenericType(argumentType);
                            constructorInfo =
                                type.GetConstructor(
                                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null,
                                    parameters, null);
                            if (constructorInfo != null)
                            {
                                methodInfo = enumerableConstructorMethod.MakeGenericMethod(type, argumentType);
                                break;
                            }
                        }
                        else if (genericType == typeof(IDictionary<,>))
                        {
                            var constructorInfo =
                                type.GetConstructor(
                                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null,
                                    new[] { interfaceType }, null);
                            if (constructorInfo != null)
                            {
                                var parameters = interfaceType.GetGenericArguments();
                                methodInfo = dictionaryConstructorMethod.MakeGenericMethod(type, parameters[0],
                                    parameters[1]);
                                break;
                            }
                        }
                    }
                }
                if (methodInfo != null)
                {
                    DefaultDeSerializer = (TmphDeSerialize)Delegate.CreateDelegate(typeof(TmphDeSerialize), methodInfo);
                    return;
                }
                if (TmphConstructor<TValueType>.New == null) DefaultDeSerializer = fromNull;
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
                                    methodInfo = baseSerializeMethod.MakeGenericMethod(baseType, type);
                                    DefaultDeSerializer =
                                        (TmphDeSerialize)Delegate.CreateDelegate(typeof(TmphDeSerialize), methodInfo);
                                    return;
                                }
                                break;
                            }
                        }
                    }
                    var fields =
                        TmphIndexSerializer.TmphTypeSerializer.GetFields(
                            TmphMemberIndexGroup<TValueType>.GetFields(attribute.MemberFilter), out memberCountVerify,
                            out memberMapFixedSize);
                    fixedFillSize = -fields.FixedSize & 3;
                    fixedSize = (fields.FixedSize + (sizeof(int) + 3)) & (int.MaxValue - 3);
                    var fixedDynamicMethod = new TmphTypeDeSerializer.TmphMemberDynamicMethod(type);
                    var fixedMemberMapDynamicMethod = new TmphTypeDeSerializer.TmphMemberMapDynamicMethod(type);
                    foreach (var member in fields.FixedFields)
                    {
                        fixedDynamicMethod.Push(member);
                        fixedMemberMapDynamicMethod.Push(member);
                    }
                    fixedMemberDeSerializer = (TmphDeSerialize)fixedDynamicMethod.Create<TmphDeSerialize>();
                    fixedMemberMapDeSerializer =
                        (TmphMemberMapDeSerialize)fixedMemberMapDynamicMethod.Create<TmphMemberMapDeSerialize>();

                    var dynamicMethod = new TmphTypeDeSerializer.TmphMemberDynamicMethod(type);
                    var memberMapDynamicMethod = new TmphTypeDeSerializer.TmphMemberMapDynamicMethod(type);
                    foreach (var member in fields.Fields)
                    {
                        dynamicMethod.Push(member);
                        memberMapDynamicMethod.Push(member);
                    }
                    memberDeSerializer = (TmphDeSerialize)dynamicMethod.Create<TmphDeSerialize>();
                    memberMapDeSerializer =
                        (TmphMemberMapDeSerialize)memberMapDynamicMethod.Create<TmphMemberMapDeSerialize>();

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
            ///     对象反序列化
            /// </summary>
            /// <param name="deSerializer">二进制数据反序列化</param>
            /// <param name="value">数据对象</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            internal static void DeSerialize(TmphIndexDeSerializer deSerializer, ref TValueType value)
            {
                if (DefaultDeSerializer == null)
                {
                    if (value == null) value = TmphConstructor<TValueType>.New();
                    MemberDeSerialize(deSerializer, ref value);
                }
                else DefaultDeSerializer(deSerializer, ref value);
            }

            /// <summary>
            ///     对象序列化
            /// </summary>
            /// <param name="deSerializer">二进制数据反序列化</param>
            /// <param name="value">数据对象</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            internal static void StructDeSerialize(TmphIndexDeSerializer deSerializer, ref TValueType value)
            {
                if (DefaultDeSerializer == null) MemberDeSerialize(deSerializer, ref value);
                else DefaultDeSerializer(deSerializer, ref value);
            }

            /// <summary>
            ///     对象序列化
            /// </summary>
            /// <param name="deSerializer">二进制数据反序列化</param>
            /// <param name="value">数据对象</param>
            internal static void MemberDeSerialize(TmphIndexDeSerializer deSerializer, ref TValueType value)
            {
                if (deSerializer.CheckMemberCount(memberCountVerify))
                {
                    fixedMemberDeSerializer(deSerializer, ref value);
                    deSerializer.Read += fixedFillSize;
                    memberDeSerializer(deSerializer, ref value);
                    if (attribute.IsJson || jsonMemberMap != null) deSerializer.parseJson(ref value);
                }
                else
                {
                    var memberMap = deSerializer.CheckMemberMap<TValueType>();
                    if (memberMap != null)
                    {
                        var start = deSerializer.Read;
                        fixedMemberMapDeSerializer(memberMap, deSerializer, ref value);
                        deSerializer.Read += (int)(start - deSerializer.Read) & 3;
                        memberMapDeSerializer(memberMap, deSerializer, ref value);
                        if (attribute.IsJson) deSerializer.parseJson(ref value);
                        else if (jsonMemberMap != null)
                        {
                            foreach (var memberIndex in jsonMemberIndexs)
                            {
                                if (memberMap.IsMember(memberIndex))
                                {
                                    deSerializer.parseJson(ref value);
                                    return;
                                }
                            }
                        }
                    }
                }
            }

            /// <summary>
            ///     对象反序列化
            /// </summary>
            /// <param name="deSerializer">二进制数据反序列化</param>
            /// <param name="value">数据对象</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            internal static void BaseSerialize<childType>(TmphIndexDeSerializer deSerializer, ref childType value)
                where childType : TValueType
            {
                if (value == null) value = TmphConstructor<childType>.New();
                TValueType newValue = value;
                StructDeSerialize(deSerializer, ref newValue);
            }

            /// <summary>
            ///     不支持对象转换null
            /// </summary>
            /// <param name="deSerializer">二进制数据反序列化</param>
            /// <param name="value">数据对象</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private static void fromNull(TmphIndexDeSerializer deSerializer, ref TValueType value)
            {
                deSerializer.checkNull();
                value = default(TValueType);
            }

            /// <summary>
            ///     枚举值序列化
            /// </summary>
            /// <param name="deSerializer">二进制数据反序列化</param>
            /// <param name="array">枚举值序列化</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private static void enumByte(TmphIndexDeSerializer deSerializer, ref TValueType value)
            {
                deSerializer.enumByte(ref value);
            }

            /// <summary>
            ///     枚举值序列化
            /// </summary>
            /// <param name="deSerializer">二进制数据反序列化</param>
            /// <param name="array">枚举值序列化</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private static void enumSByte(TmphIndexDeSerializer deSerializer, ref TValueType value)
            {
                deSerializer.enumSByte(ref value);
            }

            /// <summary>
            ///     枚举值序列化
            /// </summary>
            /// <param name="deSerializer">二进制数据反序列化</param>
            /// <param name="array">枚举值序列化</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private static void enumShort(TmphIndexDeSerializer deSerializer, ref TValueType value)
            {
                deSerializer.enumShort(ref value);
            }

            /// <summary>
            ///     枚举值序列化
            /// </summary>
            /// <param name="deSerializer">二进制数据反序列化</param>
            /// <param name="array">枚举值序列化</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private static void enumUShort(TmphIndexDeSerializer deSerializer, ref TValueType value)
            {
                deSerializer.enumUShort(ref value);
            }

            /// <summary>
            ///     枚举值序列化
            /// </summary>
            /// <param name="deSerializer">二进制数据反序列化</param>
            /// <param name="array">枚举值序列化</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private static void enumInt(TmphIndexDeSerializer deSerializer, ref TValueType value)
            {
                deSerializer.enumInt(ref value);
            }

            /// <summary>
            ///     枚举值序列化
            /// </summary>
            /// <param name="deSerializer">二进制数据反序列化</param>
            /// <param name="array">枚举值序列化</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private static void enumUInt(TmphIndexDeSerializer deSerializer, ref TValueType value)
            {
                deSerializer.enumUInt(ref value);
            }

            /// <summary>
            ///     枚举值序列化
            /// </summary>
            /// <param name="deSerializer">二进制数据反序列化</param>
            /// <param name="array">枚举值序列化</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private static void enumLong(TmphIndexDeSerializer deSerializer, ref TValueType value)
            {
                deSerializer.enumLong(ref value);
            }

            /// <summary>
            ///     枚举值序列化
            /// </summary>
            /// <param name="deSerializer">二进制数据反序列化</param>
            /// <param name="array">枚举值序列化</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private static void enumULong(TmphIndexDeSerializer deSerializer, ref TValueType value)
            {
                deSerializer.enumULong(ref value);
            }

            /// <summary>
            ///     二进制数据反序列化委托
            /// </summary>
            /// <param name="deSerializer">二进制数据反序列化</param>
            /// <param name="value">目标数据</param>
            internal delegate void TmphDeSerialize(TmphIndexDeSerializer deSerializer, ref TValueType value);

            /// <summary>
            ///     二进制数据反序列化委托
            /// </summary>
            /// <param name="memberMap">成员位图</param>
            /// <param name="deSerializer">二进制数据反序列化</param>
            /// <param name="value">目标数据</param>
            private delegate void TmphMemberMapDeSerialize(
                TmphMemberMap memberMap, TmphIndexDeSerializer deSerializer, ref TValueType value);
        }
    }
}