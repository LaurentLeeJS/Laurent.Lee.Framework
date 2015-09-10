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
    ///     二进制数据反序列化
    /// </summary>
    public sealed unsafe class TmphDataDeSerializer : TmphBinaryDeSerializer
    {
        /// <summary>
        ///     对象序列化函数信息
        /// </summary>
        private static readonly MethodInfo structDeSerializeMethod =
            typeof(TmphDataDeSerializer).GetMethod("structDeSerialize", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     字典序列化函数信息
        /// </summary>
        private static readonly MethodInfo keyValuePairDeSerializeMethod =
            typeof(TmphDataDeSerializer).GetMethod("keyValuePairDeSerialize",
                BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     字典序列化函数信息
        /// </summary>
        private static readonly MethodInfo dictionaryDeSerializeMethod =
            typeof(TmphDataDeSerializer).GetMethod("dictionaryDeSerialize",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     字典序列化函数信息
        /// </summary>
        private static readonly MethodInfo dictionaryMemberMethod =
            typeof(TmphDataDeSerializer).GetMethod("dictionaryMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     字典序列化函数信息
        /// </summary>
        private static readonly MethodInfo sortedDictionaryDeSerializeMethod =
            typeof(TmphDataDeSerializer).GetMethod("sortedDictionaryDeSerialize",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     字典序列化函数信息
        /// </summary>
        private static readonly MethodInfo sortedDictionaryMemberMethod =
            typeof(TmphDataDeSerializer).GetMethod("sortedDictionaryMember",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     字典序列化函数信息
        /// </summary>
        private static readonly MethodInfo sortedListDeSerializeMethod =
            typeof(TmphDataDeSerializer).GetMethod("sortedListDeSerialize",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     字典序列化函数信息
        /// </summary>
        private static readonly MethodInfo sortedListMemberMethod =
            typeof(TmphDataDeSerializer).GetMethod("sortedListMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     对象序列化函数信息
        /// </summary>
        private static readonly MethodInfo nullableDeSerializeMethod =
            typeof(TmphDataDeSerializer).GetMethod("nullableDeSerialize", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组对象序列化函数信息
        /// </summary>
        private static readonly MethodInfo subArrayDeSerializeMethod =
            typeof(TmphDataDeSerializer).GetMethod("subArrayDeSerialize", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     对象序列化函数信息
        /// </summary>
        private static readonly MethodInfo nullableMemberDeSerializeMethod =
            typeof(TmphDataDeSerializer).GetMethod("nullableMemberDeSerialize",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     基类反序列化函数信息
        /// </summary>
        private static readonly MethodInfo baseSerializeMethod = typeof(TmphDataDeSerializer).GetMethod("baseSerialize",
            BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     基类反序列化函数信息
        /// </summary>
        private static readonly MethodInfo realTypeObjectMethod = typeof(TmphDataDeSerializer).GetMethod(
            "realTypeObject", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     集合反序列化函数信息
        /// </summary>
        private static readonly MethodInfo structCollectionMethod =
            typeof(TmphDataDeSerializer).GetMethod("structCollection", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     集合反序列化函数信息
        /// </summary>
        private static readonly MethodInfo classCollectionMethod =
            typeof(TmphDataDeSerializer).GetMethod("classCollection", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     集合反序列化函数信息
        /// </summary>
        private static readonly MethodInfo structDictionaryDeSerializeMethod =
            typeof(TmphDataDeSerializer).GetMethod("structDictionaryDeSerialize",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     集合反序列化函数信息
        /// </summary>
        private static readonly MethodInfo classDictionaryDeSerializeMethod =
            typeof(TmphDataDeSerializer).GetMethod("classDictionaryDeSerialize",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     枚举反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumByteMemberMethod = typeof(TmphDataDeSerializer).GetMethod(
            "enumByteMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     枚举反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumSByteMemberMethod =
            typeof(TmphDataDeSerializer).GetMethod("enumSByteMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     枚举反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumShortMemberMethod =
            typeof(TmphDataDeSerializer).GetMethod("enumShortMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     枚举反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumUShortMemberMethod =
            typeof(TmphDataDeSerializer).GetMethod("enumUShortMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     枚举反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumIntMethod = typeof(TmphDataDeSerializer).GetMethod("enumInt",
            BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     枚举反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumUIntMethod = typeof(TmphDataDeSerializer).GetMethod("enumUInt",
            BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     枚举反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumLongMethod = typeof(TmphDataDeSerializer).GetMethod("enumLong",
            BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     枚举反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumULongMethod = typeof(TmphDataDeSerializer).GetMethod("enumULong",
            BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumByteArrayMethod = typeof(TmphDataDeSerializer).GetMethod("enumByteArray",
            BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumByteArrayMemberMethod =
            typeof(TmphDataDeSerializer).GetMethod("enumByteArrayMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumSByteArrayMethod = typeof(TmphDataDeSerializer).GetMethod(
            "enumSByteArray", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumSByteArrayMemberMethod =
            typeof(TmphDataDeSerializer).GetMethod("enumSByteArrayMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumShortArrayMethod = typeof(TmphDataDeSerializer).GetMethod(
            "enumShortArray", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumShortArrayMemberMethod =
            typeof(TmphDataDeSerializer).GetMethod("enumShortArrayMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumUShortArrayMethod =
            typeof(TmphDataDeSerializer).GetMethod("enumUShortArray", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumUShortArrayMemberMethod =
            typeof(TmphDataDeSerializer).GetMethod("enumUShortArrayMember",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumIntArrayMethod = typeof(TmphDataDeSerializer).GetMethod("enumIntArray",
            BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumIntArrayMemberMethod =
            typeof(TmphDataDeSerializer).GetMethod("enumIntArrayMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumUIntArrayMethod = typeof(TmphDataDeSerializer).GetMethod("enumUIntArray",
            BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumUIntArrayMemberMethod =
            typeof(TmphDataDeSerializer).GetMethod("enumUIntArrayMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumLongArrayMethod = typeof(TmphDataDeSerializer).GetMethod("enumLongArray",
            BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumLongArrayMemberMethod =
            typeof(TmphDataDeSerializer).GetMethod("enumLongArrayMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumULongArrayMethod = typeof(TmphDataDeSerializer).GetMethod(
            "enumULongArray", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumULongArrayMemberMethod =
            typeof(TmphDataDeSerializer).GetMethod("enumULongArrayMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo nullableArrayMethod = typeof(TmphDataDeSerializer).GetMethod("nullableArray",
            BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo nullableArrayMemberMethod =
            typeof(TmphDataDeSerializer).GetMethod("nullableArrayMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo structArrayMethod = typeof(TmphDataDeSerializer).GetMethod("structArray",
            BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo structArrayMemberMethod =
            typeof(TmphDataDeSerializer).GetMethod("structArrayMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo arrayMethod = typeof(TmphDataDeSerializer).GetMethod("array",
            BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo arrayMemberMethod = typeof(TmphDataDeSerializer).GetMethod("arrayMember",
            BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     序列化接口函数信息
        /// </summary>
        private static readonly MethodInfo structISerializeMethod =
            typeof(TmphDataDeSerializer).GetMethod("structISerialize", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     序列化接口函数信息
        /// </summary>
        private static readonly MethodInfo classISerializeMethod =
            typeof(TmphDataDeSerializer).GetMethod("classISerialize", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     序列化接口函数信息
        /// </summary>
        private static readonly MethodInfo memberClassISerializeMethod =
            typeof(TmphDataDeSerializer).GetMethod("memberClassISerialize",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     序列化接口函数信息
        /// </summary>
        private static readonly MethodInfo memberClassDeSerializeMethod =
            typeof(TmphDataDeSerializer).GetMethod("memberClassDeSerialize",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     反序列化
        /// </summary>
        private static TmphInterlocked.TmphDictionary<Type, Func<TmphSubArray<byte>, TmphConfig, object>> deSerializeTypes =
            new TmphInterlocked.TmphDictionary<Type, Func<TmphSubArray<byte>, TmphConfig, object>>(
                TmphDictionary.CreateOnly<Type, Func<TmphSubArray<byte>, TmphConfig, object>>());

        /// <summary>
        ///     反序列化函数信息
        /// </summary>
        private static readonly MethodInfo deSerializeTypeMethod =
            typeof(TmphDataDeSerializer).GetMethod("deSerializeType", BindingFlags.Static | BindingFlags.NonPublic, null,
                new[] { typeof(TmphSubArray<byte>), typeof(TmphConfig) }, null);

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
        ///     是否检测引用类型对象的真实类型
        /// </summary>
        private bool isObjectRealType;

        /// <summary>
        ///     是否检测数组引用
        /// </summary>
        private bool isReferenceArray;

        /// <summary>
        ///     是否检测相同的引用成员
        /// </summary>
        private bool isReferenceMember;

        /// <summary>
        ///     历史对象指针位置
        /// </summary>
        private Dictionary<int, object> points;

        static TmphDataDeSerializer()
        {
            deSerializeMethods = TmphDictionary.CreateOnly<Type, MethodInfo>();
            memberDeSerializeMethods = TmphDictionary.CreateOnly<Type, MethodInfo>();
            memberMapDeSerializeMethods = TmphDictionary.CreateOnly<Type, MethodInfo>();
            foreach (
                var method in typeof(TmphDataDeSerializer).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic))
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
                    memberDeSerializeMethods.Add(TParameterType, method);
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
        private TmphDeSerializeState deSerialize<TValueType>(byte[] data, byte* start, byte* end, ref TValueType value,
            TmphConfig TmphConfig)
        {
            DeSerializeConfig = TmphConfig;
            TmphBuffer = data;
            this.start = start;
            Read = start + sizeof(int);
            this.end = end;
            if ((*start & TmphBinarySerializer.TmphConfig.MemberMapValue) == 0) isMemberMap = false;
            else
            {
                isMemberMap = true;
                MemberMap = TmphConfig.MemberMap;
            }
            isObjectRealType = (*start & TmphDataSerializer.TmphConfig.ObjectRealTypeValue) != 0;
            isReferenceMember = TmphTypeDeSerializer<TValueType>.IsReferenceMember;
            if (points == null && isReferenceMember) points = TmphDictionary.CreateInt<object>();
            isReferenceArray = true;
            state = TmphDeSerializeState.Success;
            TmphTypeDeSerializer<TValueType>.DeSerialize(this, ref value);
            checkState();
            return TmphConfig.State = state;
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
        private TmphDeSerializeState codeDeSerialize<TValueType>(byte[] data, byte* start, byte* end, ref TValueType value,
            TmphConfig TmphConfig) where TValueType : Code.CSharp.TmphDataSerialize.ISerialize
        {
            DeSerializeConfig = TmphConfig;
            TmphBuffer = data;
            this.start = start;
            Read = start + sizeof(int);
            this.end = end;
            if ((*start & TmphBinarySerializer.TmphConfig.MemberMapValue) == 0) isMemberMap = false;
            else
            {
                isMemberMap = true;
                MemberMap = TmphConfig.MemberMap;
            }
            isObjectRealType = (*start & TmphDataSerializer.TmphConfig.ObjectRealTypeValue) != 0;
            isReferenceMember = TmphTypeDeSerializer<TValueType>.IsReferenceMember;
            if (points == null && isReferenceMember) points = TmphDictionary.CreateInt<object>();
            if (value == null) value = TmphConstructor<TValueType>.New();
            isReferenceArray = true;
            state = TmphDeSerializeState.Success;
            value.DeSerialize(this);
            checkState();
            return TmphConfig.State = state;
        }

        /// <summary>
        ///     释放资源
        /// </summary>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private new void free()
        {
            base.free();
            if (points != null) points.Clear();
            TmphTypePool<TmphDataDeSerializer>.Push(this);
        }

        /// <summary>
        ///     获取历史对象
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private bool checkPoint<TValueType>(ref TValueType value)
        {
            if (isReferenceMember && *(int*)Read < 0)
            {
                object pointValue;
                if (points.TryGetValue(*(int*)Read, out pointValue))
                {
                    value = (TValueType)pointValue;
                    Read += sizeof(int);
                    return false;
                }
                if (*(int*)Read != TmphDataSerializer.RealTypeValue)
                {
                    Error(TmphDeSerializeState.NoPoint);
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        ///     添加历史对象
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="value"></param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void addPoint<TValueType>(ref TValueType value)
        {
            if (value == null) value = TmphConstructor<TValueType>.New();
            if (isReferenceMember) points.Add((int)(start - Read), value);
        }

        /// <summary>
        ///     添加历史对象
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="value"></param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public void AddPoint<TValueType>(TValueType value)
        {
            if (isReferenceMember) points.Add((int)(start - Read), value);
        }

        /// <summary>
        ///     是否真实类型处理
        /// </summary>
        /// <returns></returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private bool isRealType()
        {
            if (isObjectRealType && *(int*)Read == TmphDataSerializer.RealTypeValue)
            {
                Read += sizeof(int);
                return true;
            }
            return false;
        }

        /// <summary>
        ///     判断成员索引是否有效
        /// </summary>
        /// <param name="memberIndex"></param>
        /// <returns></returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public bool IsMemberMap(int memberIndex)
        {
            return MemberMap.IsMember(memberIndex);
        }

        /// <summary>
        ///     创建数组
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="array"></param>
        /// <param name="length"></param>
        private void createArray<TValueType>(ref TValueType[] array, int length)
        {
            array = new TValueType[length];
            if (isReferenceArray)
            {
                if (isReferenceMember) points.Add((int)(start - Read), array);
            }
            else isReferenceArray = true;
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
            if (isReferenceArray && !checkPoint(ref value)) return 0;
            if (*(int*)Read != 0) return *(int*)Read;
            isReferenceArray = true;
            value = TmphNullValue<TValueType>.Array;
            Read += sizeof(int);
            return 0;
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
                    createArray(ref value, length);
                    Read = DeSerialize(Read + sizeof(int), value);
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     逻辑值反序列化
        /// </summary>
        /// <param name="value">逻辑值</param>
        [TmphMemberDeSerializeMethod]
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref bool[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerialize(ref value);
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
                    createArray(ref value, length >> 1);
                    Read = DeSerialize(Read + sizeof(int), value);
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     逻辑值反序列化
        /// </summary>
        /// <param name="value">逻辑值</param>
        [TmphMemberDeSerializeMethod]
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref bool?[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerialize(ref value);
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
                    createArray(ref value, length);
                    Read = DeSerialize(Read + sizeof(int), value);
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberDeSerializeMethod]
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref byte[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerialize(ref value);
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
                    createArray(ref value, length);
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
        [TmphMemberDeSerializeMethod]
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref byte?[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerialize(ref value);
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
                    createArray(ref value, length);
                    Read = DeSerialize(Read + sizeof(int), value);
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberDeSerializeMethod]
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref sbyte[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerialize(ref value);
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
                    createArray(ref value, length);
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
        [TmphMemberDeSerializeMethod]
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref sbyte?[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerialize(ref value);
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
                    createArray(ref value, length);
                    Read = DeSerialize(Read + sizeof(int), value);
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberDeSerializeMethod]
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref short[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerialize(ref value);
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
                    createArray(ref value, length);
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
        [TmphMemberDeSerializeMethod]
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref short?[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerialize(ref value);
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
                    createArray(ref value, length);
                    Read = DeSerialize(Read + sizeof(int), value);
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberDeSerializeMethod]
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref ushort[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerialize(ref value);
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
                    createArray(ref value, length);
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
        [TmphMemberDeSerializeMethod]
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref ushort?[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerialize(ref value);
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
                    createArray(ref value, length);
                    Read = DeSerialize(Read + sizeof(int), value);
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberDeSerializeMethod]
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref int[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerialize(ref value);
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
                    createArray(ref value, length);
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
        [TmphMemberDeSerializeMethod]
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref int?[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerialize(ref value);
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
                    createArray(ref value, length);
                    Read = DeSerialize(Read + sizeof(int), value);
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberDeSerializeMethod]
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref uint[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerialize(ref value);
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
                    createArray(ref value, length);
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
        [TmphMemberDeSerializeMethod]
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref uint?[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerialize(ref value);
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
                    createArray(ref value, length);
                    Read = DeSerialize(Read + sizeof(int), value);
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberDeSerializeMethod]
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref long[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerialize(ref value);
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
                    createArray(ref value, length);
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
        [TmphMemberDeSerializeMethod]
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref long?[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerialize(ref value);
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
                    createArray(ref value, length);
                    Read = DeSerialize(Read + sizeof(int), value);
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberDeSerializeMethod]
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref ulong[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerialize(ref value);
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
                    createArray(ref value, length);
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
        [TmphMemberDeSerializeMethod]
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref ulong?[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerialize(ref value);
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
                    createArray(ref value, length);
                    Read = DeSerialize(Read + sizeof(int), value);
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberDeSerializeMethod]
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref float[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerialize(ref value);
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
                    createArray(ref value, length);
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
        [TmphMemberDeSerializeMethod]
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref float?[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerialize(ref value);
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
                    createArray(ref value, length);
                    Read = DeSerialize(Read + sizeof(int), value);
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberDeSerializeMethod]
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref double[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerialize(ref value);
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
                    createArray(ref value, length);
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
        [TmphMemberDeSerializeMethod]
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref double?[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerialize(ref value);
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
                    createArray(ref value, length);
                    Read = DeSerialize(Read + sizeof(int), value);
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphMemberDeSerializeMethod]
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref decimal[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerialize(ref value);
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
                    createArray(ref value, length);
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
        [TmphMemberDeSerializeMethod]
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref decimal?[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerialize(ref value);
        }

        /// <summary>
        ///     字符反序列化
        /// </summary>
        /// <param name="value">字符</param>
        [TmphDeSerializeMethod]
        private void deSerialize(ref char[] value)
        {
            var length = deSerializeArray(ref value);
            if (length != 0)
            {
                if (((length * sizeof(char) + (3 + sizeof(int))) & (int.MaxValue - 3)) <= (int)(end - Read))
                {
                    createArray(ref value, length);
                    Read = DeSerialize(Read + sizeof(int), value);
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     字符反序列化
        /// </summary>
        /// <param name="value">字符</param>
        [TmphMemberDeSerializeMethod]
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref char[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerialize(ref value);
        }

        /// <summary>
        ///     字符反序列化
        /// </summary>
        /// <param name="value">字符</param>
        [TmphDeSerializeMethod]
        private void deSerialize(ref char?[] value)
        {
            var length = deSerializeArray(ref value);
            if (length != 0)
            {
                if ((((length + (31 + 32)) >> 5) << 2) <= (int)(end - Read))
                {
                    createArray(ref value, length);
                    Read = DeSerialize(Read + sizeof(int), value);
                    if (Read <= end) return;
                }
                Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     字符反序列化
        /// </summary>
        /// <param name="value">字符</param>
        [TmphMemberDeSerializeMethod]
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref char?[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerialize(ref value);
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
                    createArray(ref value, length);
                    Read = DeSerialize(Read + sizeof(int), value);
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     时间反序列化
        /// </summary>
        /// <param name="value">时间</param>
        [TmphMemberDeSerializeMethod]
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref DateTime[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerialize(ref value);
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
                    createArray(ref value, length);
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
        [TmphMemberDeSerializeMethod]
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref DateTime?[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerialize(ref value);
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
                    createArray(ref value, length);
                    Read = DeSerialize(Read + sizeof(int), value);
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     Guid反序列化
        /// </summary>
        /// <param name="value">Guid</param>
        [TmphMemberDeSerializeMethod]
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref Guid[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerialize(ref value);
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
                    createArray(ref value, length);
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
        [TmphMemberDeSerializeMethod]
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref Guid?[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerialize(ref value);
        }

        /// <summary>
        ///     字符串反序列化
        /// </summary>
        /// <param name="value">字符串</param>
        [TmphDeSerializeMethod]
        private void deSerialize(ref string value)
        {
            if (checkPoint(ref value))
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
                            if (isReferenceMember) points.Add((int)(start - Read), value);
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
                        if (isReferenceMember) points.Add((int)(start - Read), value);
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
        }

        /// <summary>
        ///     字符串反序列化
        /// </summary>
        /// <param name="value">字符串</param>
        [TmphMemberDeSerializeMethod]
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref string value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerialize(ref value);
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
                    createArray(ref value, length);
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
        [TmphMemberDeSerializeMethod]
        [TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberDeSerialize(ref string[] value)
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else deSerialize(ref value);
        }

        /// <summary>
        ///     对象序列化
        /// </summary>
        /// <param name="deSerializer">二进制数据反序列化</param>
        /// <param name="value">数据对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private static void structDeSerialize<TValueType>(TmphDataDeSerializer deSerializer, ref TValueType value)
            where TValueType : struct
        {
            TmphTypeDeSerializer<TValueType>.StructDeSerialize(deSerializer, ref value);
        }

        /// <summary>
        ///     字典序列化
        /// </summary>
        /// <param name="value">数据对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private static void keyValuePairDeSerialize<TKeyType, TValueType>(TmphDataDeSerializer deSerializer,
            ref KeyValuePair<TKeyType, TValueType> value)
        {
            var keyValue = default(TmphKeyValue<TKeyType, TValueType>);
            TmphTypeDeSerializer<TmphKeyValue<TKeyType, TValueType>>.MemberDeSerialize(deSerializer, ref keyValue);
            value = new KeyValuePair<TKeyType, TValueType>(keyValue.Key, keyValue.Value);
        }

        /// <summary>
        ///     字典序列化
        /// </summary>
        /// <param name="value">数据对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void dictionaryArrayDeSerialize<TKeyType, TValueType>(IDictionary<TKeyType, TValueType> value)
        {
            if (isReferenceMember) points.Add((int)(start - Read), value);
            TKeyType[] keys = null;
            isReferenceArray = false;
            TmphTypeDeSerializer<TKeyType[]>.DefaultDeSerializer(this, ref keys);
            if (state == TmphDeSerializeState.Success)
            {
                TValueType[] values = null;
                isReferenceArray = false;
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
            else dictionaryDeSerialize(ref value);
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
            else sortedDictionaryDeSerialize(ref value);
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
            else sortedListDeSerialize(ref value);
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
        ///     数组对象序列化
        /// </summary>
        /// <param name="value">数据对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void subArrayDeSerialize<TValueType>(ref TmphSubArray<TValueType> value)
        {
            TValueType[] array = null;
            isReferenceArray = false;
            TmphTypeDeSerializer<TValueType[]>.DefaultDeSerializer(this, ref array);
            value.UnsafeSet(array, 0, array.Length);
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
            else nullableDeSerialize(ref value);
        }

        /// <summary>
        ///     基类反序列化
        /// </summary>
        /// <param name="deSerializer">二进制数据反序列化</param>
        /// <param name="value">数据对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private static void baseSerialize<TValueType, childType>(TmphDataDeSerializer deSerializer, ref childType value)
            where childType : TValueType
        {
            TmphTypeDeSerializer<TValueType>.BaseDeSerialize(deSerializer, ref value);
        }

        /// <summary>
        ///     真实类型反序列化
        /// </summary>
        /// <param name="deSerializer">二进制数据反序列化</param>
        /// <param name="objectValue"></param>
        /// <returns></returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private static object realTypeObject<TValueType>(TmphDataDeSerializer deSerializer, object objectValue)
        {
            var value = (TValueType)objectValue;
            TmphTypeDeSerializer<TValueType>.RealType(deSerializer, ref value);
            return value;
        }

        /// <summary>
        ///     集合构造函数解析
        /// </summary>
        /// <param name="deSerializer">二进制数据反序列化</param>
        /// <param name="value">目标数据</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void collection<TValueType, argumentType>(ref TValueType value)
            where TValueType : ICollection<argumentType>
        {
            argumentType[] values = null;
            isReferenceArray = false;
            TmphTypeDeSerializer<argumentType[]>.DefaultDeSerializer(this, ref values);
            if (state == TmphDeSerializeState.Success)
            {
                foreach (var nextValue in values) value.Add(nextValue);
            }
        }

        /// <summary>
        ///     集合构造函数解析
        /// </summary>
        /// <param name="deSerializer">二进制数据反序列化</param>
        /// <param name="value">目标数据</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void structCollection<TValueType, argumentType>(ref TValueType value)
            where TValueType : ICollection<argumentType>
        {
            value = TmphConstructor<TValueType>.New();
            collection<TValueType, argumentType>(ref value);
        }

        /// <summary>
        ///     集合构造函数解析
        /// </summary>
        /// <param name="deSerializer">二进制数据反序列化</param>
        /// <param name="value">目标数据</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void classCollection<TValueType, argumentType>(ref TValueType value)
            where TValueType : ICollection<argumentType>
        {
            if (checkPoint(ref value))
            {
                value = TmphConstructor<TValueType>.New();
                if (isReferenceMember) points.Add((int)(start - Read), value);
                collection<TValueType, argumentType>(ref value);
            }
        }

        /// <summary>
        ///     集合构造函数解析
        /// </summary>
        /// <param name="deSerializer">二进制数据反序列化</param>
        /// <param name="value">目标数据</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void dictionaryConstructorDeSerialize<dictionaryType, TKeyType, TValueType>(ref dictionaryType value)
            where dictionaryType : IDictionary<TKeyType, TValueType>
        {
            TKeyType[] keys = null;
            isReferenceArray = false;
            TmphTypeDeSerializer<TKeyType[]>.DefaultDeSerializer(this, ref keys);
            if (state == TmphDeSerializeState.Success)
            {
                TValueType[] values = null;
                isReferenceArray = false;
                TmphTypeDeSerializer<TValueType[]>.DefaultDeSerializer(this, ref values);
                if (state == TmphDeSerializeState.Success)
                {
                    var index = 0;
                    foreach (var nextValue in values) value.Add(keys[index++], nextValue);
                }
            }
        }

        /// <summary>
        ///     集合构造函数解析
        /// </summary>
        /// <param name="deSerializer">二进制数据反序列化</param>
        /// <param name="value">目标数据</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void structDictionaryDeSerialize<dictionaryType, TKeyType, TValueType>(ref dictionaryType value)
            where dictionaryType : IDictionary<TKeyType, TValueType>
        {
            value = TmphConstructor<dictionaryType>.New();
            dictionaryConstructorDeSerialize<dictionaryType, TKeyType, TValueType>(ref value);
        }

        /// <summary>
        ///     集合构造函数解析
        /// </summary>
        /// <param name="deSerializer">二进制数据反序列化</param>
        /// <param name="value">目标数据</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void classDictionaryDeSerialize<dictionaryType, TKeyType, TValueType>(ref dictionaryType value)
            where dictionaryType : IDictionary<TKeyType, TValueType>
        {
            if (checkPoint(ref value))
            {
                value = TmphConstructor<dictionaryType>.New();
                if (isReferenceMember) points.Add((int)(start - Read), value);
                dictionaryConstructorDeSerialize<dictionaryType, TKeyType, TValueType>(ref value);
            }
        }

        /// <summary>
        ///     枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private void enumByteArray<TValueType>(ref TValueType[] array)
        {
            var length = deSerializeArray(ref array);
            if (length != 0)
            {
                var dataLength = (length + (3 + sizeof(int))) & (int.MaxValue - 3);
                if (dataLength <= (int)(end - Read))
                {
                    createArray(ref array, length);
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
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
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
        private void enumSByteArray<TValueType>(ref TValueType[] array)
        {
            var length = deSerializeArray(ref array);
            if (length != 0)
            {
                var dataLength = (length + (3 + sizeof(int))) & (int.MaxValue - 3);
                if (dataLength <= (int)(end - Read))
                {
                    createArray(ref array, length);
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
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
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
        private void enumShortArray<TValueType>(ref TValueType[] array)
        {
            var length = deSerializeArray(ref array);
            if (length != 0)
            {
                var dataLength = ((length << 1) + (3 + sizeof(int))) & (int.MaxValue - 3);
                if (dataLength <= (int)(end - Read))
                {
                    createArray(ref array, length);
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
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
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
        private void enumUShortArray<TValueType>(ref TValueType[] array)
        {
            var length = deSerializeArray(ref array);
            if (length != 0)
            {
                var dataLength = ((length << 1) + (3 + sizeof(int))) & (int.MaxValue - 3);
                if (dataLength <= (int)(end - Read))
                {
                    createArray(ref array, length);
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
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
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
        private void enumIntArray<TValueType>(ref TValueType[] array)
        {
            var length = deSerializeArray(ref array);
            if (length != 0)
            {
                if ((length + 1) * sizeof(int) <= (int)(end - Read))
                {
                    createArray(ref array, length);
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
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
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
        private void enumUIntArray<TValueType>(ref TValueType[] array)
        {
            var length = deSerializeArray(ref array);
            if (length != 0)
            {
                if ((length + 1) * sizeof(int) <= (int)(end - Read))
                {
                    createArray(ref array, length);
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
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
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
        private void enumLongArray<TValueType>(ref TValueType[] array)
        {
            var length = deSerializeArray(ref array);
            if (length != 0)
            {
                if (length * sizeof(long) + sizeof(int) <= (int)(end - Read))
                {
                    createArray(ref array, length);
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
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
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
        private void enumULongArray<TValueType>(ref TValueType[] array)
        {
            var length = deSerializeArray(ref array);
            if (length != 0)
            {
                if (length * sizeof(ulong) + sizeof(int) <= (int)(end - Read))
                {
                    createArray(ref array, length);
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
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
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
        ///     数组转换
        /// </summary>
        /// <param name="array">数组对象</param>
        private void nullableArray<TValueType>(ref TValueType[] array) where TValueType : struct
        {
            var length = deSerializeArray(ref array);
            if (length != 0)
            {
                var mapLength = ((length + (31 + 32)) >> 5) << 2;
                if (mapLength <= (int)(end - Read))
                {
                    createArray(ref array, length);
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
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
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
        private void structArray<TValueType>(ref TValueType[] array)
        {
            var length = deSerializeArray(ref array);
            if (length != 0)
            {
                if ((length + 1) * sizeof(int) <= (int)(end - Read))
                {
                    createArray(ref array, length);
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
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
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
        private void array<TValueType>(ref TValueType[] array) where TValueType : class
        {
            var length = deSerializeArray(ref array);
            if (length != 0)
            {
                var mapLength = ((length + (31 + 32)) >> 5) << 2;
                if (mapLength <= (int)(end - Read))
                {
                    createArray(ref array, length);
                    var arrayMap = new TmphArrayMap(Read + sizeof(int));
                    Read += mapLength;
                    for (var index = 0; index != array.Length; ++index)
                    {
                        if (arrayMap.Next() == 0) array[index] = null;
                        else
                        {
                            TmphTypeDeSerializer<TValueType>.ClassDeSerialize(this, ref array[index]);
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
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
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
        ///     序列化接口
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="value"></param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void structISerialize<TValueType>(ref TValueType value)
            where TValueType : struct, Code.CSharp.TmphDataSerialize.ISerialize
        {
            value.DeSerialize(this);
        }

        /// <summary>
        ///     序列化接口
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="value"></param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void classISerialize<TValueType>(ref TValueType value)
            where TValueType : class, Code.CSharp.TmphDataSerialize.ISerialize
        {
            if (value == null) value = TmphConstructor<TValueType>.New();
            value.DeSerialize(this);
        }

        /// <summary>
        ///     序列化接口
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="value"></param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberClassISerialize<TValueType>(ref TValueType value)
            where TValueType : class, Code.CSharp.TmphDataSerialize.ISerialize
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else
            {
                if (value == null) value = TmphConstructor<TValueType>.New();
                value.DeSerialize(this);
            }
        }

        /// <summary>
        ///     引用类型成员反序列化
        /// </summary>
        /// <param name="value">数据对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public bool MemberClassDeSerialize<TValueType>(ref TValueType value) where TValueType : class
        {
            memberClassDeSerialize(ref value);
            return state == TmphDeSerializeState.Success;
        }

        /// <summary>
        ///     引用类型成员反序列化
        /// </summary>
        /// <param name="value">数据对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void memberClassDeSerialize<TValueType>(ref TValueType value) where TValueType : class
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else TmphTypeDeSerializer<TValueType>.ClassDeSerialize(this, ref value);
        }

        /// <summary>
        ///     未知类型反序列化
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="value"></param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public bool MemberStructDeSerialize<TValueType>(ref TValueType value)
        {
            TmphTypeDeSerializer<TValueType>.StructDeSerialize(this, ref value);
            return state == TmphDeSerializeState.Success;
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
                fixed (byte* dataFixed = data) return deSerialize<TValueType>(data, dataFixed, data.Length, TmphConfig);
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
                    return deSerialize<TValueType>(data.array, dataFixed + data.StartIndex, data.Count, TmphConfig);
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
                return deSerialize<TValueType>(null, data.Data + startIndex, data.Length - startIndex, TmphConfig);
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
            return deSerialize<TValueType>(null, data, size, TmphConfig);
        }

        /// <summary>
        ///     反序列化
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="data"></param>
        /// <param name="size"></param>
        /// <param name="TmphConfig"></param>
        /// <returns></returns>
        private static TValueType deSerialize<TValueType>(byte[] TmphBuffer, byte* data, int size, TmphConfig TmphConfig)
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
                                    var deSerializer = TmphTypePool<TmphDataDeSerializer>.Pop() ?? new TmphDataDeSerializer();
                                    try
                                    {
                                        return deSerializer.deSerialize(TmphBuffer, data, end, ref value, TmphConfig) ==
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
                        var deSerializer = TmphTypePool<TmphDataDeSerializer>.Pop() ?? new TmphDataDeSerializer();
                        try
                        {
                            return deSerializer.deSerialize(TmphBuffer, data, data + length, ref value, TmphConfig) ==
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
                fixed (byte* dataFixed = data) return deSerialize(data, dataFixed, data.Length, ref value, TmphConfig);
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
                    return deSerialize(data.array, dataFixed + data.StartIndex, data.Count, ref value, TmphConfig);
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
                return deSerialize(null, data.Data + startIndex, data.Length - startIndex, ref value, TmphConfig);
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
            return deSerialize(null, data, size, ref value, TmphConfig);
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
        private static bool deSerialize<TValueType>(byte[] TmphBuffer, byte* data, int size, ref TValueType value,
            TmphConfig TmphConfig)
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
                                    var deSerializer = TmphTypePool<TmphDataDeSerializer>.Pop() ?? new TmphDataDeSerializer();
                                    try
                                    {
                                        return deSerializer.deSerialize(TmphBuffer, data, end, ref value, TmphConfig) ==
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
                        var deSerializer = TmphTypePool<TmphDataDeSerializer>.Pop() ?? new TmphDataDeSerializer();
                        try
                        {
                            return deSerializer.deSerialize(TmphBuffer, data, data + length, ref value, TmphConfig) ==
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
        ///     反序列化
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="data"></param>
        /// <param name="value"></param>
        /// <param name="TmphConfig"></param>
        /// <returns>是否成功</returns>
        public static bool CodeDeSerialize<TValueType>(byte[] data, ref TValueType value, TmphConfig TmphConfig = null)
            where TValueType : Code.CSharp.TmphDataSerialize.ISerialize
        {
            return CodeDeSerialize(TmphSubArray<byte>.Unsafe(data, 0, data.Length), ref value, TmphConfig);
        }

        /// <summary>
        ///     反序列化
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="data"></param>
        /// <param name="value"></param>
        /// <param name="TmphConfig"></param>
        /// <returns>是否成功</returns>
        public static bool CodeDeSerialize<TValueType>(TmphSubArray<byte> data, ref TValueType value, TmphConfig TmphConfig = null)
            where TValueType : Code.CSharp.TmphDataSerialize.ISerialize
        {
            if (TmphConfig == null) TmphConfig = defaultConfig;
            var length = data.Count - sizeof(int);
            if (length >= 0)
            {
                if (TmphConfig.IsFullData)
                {
                    if ((data.Count & 3) == 0)
                    {
                        fixed (byte* dataFixed = data.array)
                        {
                            var start = dataFixed + data.StartIndex;
                            if (length != 0)
                            {
                                var end = start + length;
                                if (*(int*)end == length)
                                {
                                    if ((*(uint*)start & TmphBinarySerializer.TmphConfig.HeaderMapAndValue) ==
                                        TmphBinarySerializer.TmphConfig.HeaderMapValue)
                                    {
                                        var deSerializer = TmphTypePool<TmphDataDeSerializer>.Pop() ??
                                                           new TmphDataDeSerializer();
                                        try
                                        {
                                            return
                                                deSerializer.codeDeSerialize(data.array, start, end, ref value, TmphConfig) ==
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
                            if (*(int*)start == TmphBinarySerializer.NullValue)
                            {
                                TmphConfig.State = TmphDeSerializeState.Success;
                                value = default(TValueType);
                                return true;
                            }
                        }
                    }
                }
                else
                {
                    fixed (byte* dataFixed = data.array)
                    {
                        var start = dataFixed + data.StartIndex;
                        if ((*(uint*)start & TmphBinarySerializer.TmphConfig.HeaderMapAndValue) ==
                            TmphBinarySerializer.TmphConfig.HeaderMapValue)
                        {
                            var deSerializer = TmphTypePool<TmphDataDeSerializer>.Pop() ?? new TmphDataDeSerializer();
                            try
                            {
                                return
                                    deSerializer.codeDeSerialize(data.array, start, start + length, ref value, TmphConfig) ==
                                    TmphDeSerializeState.Success;
                            }
                            finally
                            {
                                deSerializer.free();
                            }
                        }
                        if (*(int*)start == TmphBinarySerializer.NullValue)
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
            }
            TmphConfig.State = TmphDeSerializeState.UnknownData;
            return false;
        }

        /// <summary>
        ///     反序列化
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="data"></param>
        /// <param name="TmphConfig"></param>
        /// <returns></returns>
        private static object deSerializeType<TValueType>(TmphSubArray<byte> data, TmphConfig TmphConfig)
        {
            return DeSerialize<TValueType>(data, TmphConfig);
        }

        /// <summary>
        ///     反序列化
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="type"></param>
        /// <param name="data"></param>
        /// <param name="TmphConfig"></param>
        /// <returns></returns>
        private static object DeSerializeType<TValueType>(Type type, TmphSubArray<byte> data, TmphConfig TmphConfig = null)
        {
            if (type == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            Func<TmphSubArray<byte>, TmphConfig, object> parse;
            if (!deSerializeTypes.TryGetValue(type, out parse))
            {
                parse =
                    (Func<TmphSubArray<byte>, TmphConfig, object>)
                        Delegate.CreateDelegate(typeof(Func<TmphSubArray<byte>, TmphConfig, object>),
                            deSerializeTypeMethod.MakeGenericMethod(type));
                deSerializeTypes.Set(type, parse);
            }
            return parse(data, TmphConfig);
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
            ///     真实类型序列化函数集合
            /// </summary>
            private static TmphInterlocked.TmphDictionary<Type, Func<TmphDataDeSerializer, object, object>> realDeSerializers =
                new TmphInterlocked.TmphDictionary<Type, Func<TmphDataDeSerializer, object, object>>(
                    TmphDictionary.CreateOnly<Type, Func<TmphDataDeSerializer, object, object>>());

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
                            method = keyValuePairDeSerializeMethod.MakeGenericMethod(type.GetGenericArguments());
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
                        if (typeof(Code.CSharp.TmphDataSerialize.ISerialize).IsAssignableFrom(type))
                        {
                            if (type.IsValueType) method = structISerializeMethod.MakeGenericMethod(type);
                            else method = memberClassISerializeMethod.MakeGenericMethod(type);
                        }
                        else if (type.IsValueType) method = structDeSerializeMethod.MakeGenericMethod(type);
                        else method = memberClassDeSerializeMethod.MakeGenericMethod(type);
                    }
                }
                memberDeSerializers.Set(type, method);
                return method;
            }

            /// <summary>
            ///     获取真实类型序列化函数
            /// </summary>
            /// <param name="type">数组类型</param>
            /// <returns>真实类型序列化函数</returns>
            public static Func<TmphDataDeSerializer, object, object> GetRealDeSerializer(Type type)
            {
                Func<TmphDataDeSerializer, object, object> method;
                if (realDeSerializers.TryGetValue(type, out method)) return method;
                method =
                    (Func<TmphDataDeSerializer, object, object>)
                        Delegate.CreateDelegate(typeof(Func<TmphDataDeSerializer, object, object>),
                            realTypeObjectMethod.MakeGenericMethod(type));
                realDeSerializers.Set(type, method);
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
                    dynamicMethod = new DynamicMethod("dataDeSerializer", null,
                        new[] { typeof(TmphDataDeSerializer), type.MakeByRefType() }, type, true);
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
                ///     动态函数
                /// </summary>
                /// <param name="type"></param>
                /// <param name="name">成员类型</param>
                public TmphMemberMapDynamicMethod(Type type)
                {
                    dynamicMethod = new DynamicMethod("dataMemberMapDeSerializer", null,
                        new[] { typeof(TmphMemberMap), typeof(TmphDataDeSerializer), type.MakeByRefType() }, type, true);
                    generator = dynamicMethod.GetILGenerator();
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
                    generator.Emit(OpCodes.Ldarg_2);
                    if (!isValueType) generator.Emit(OpCodes.Ldind_Ref);
                    generator.Emit(OpCodes.Ldflda, field.Field);
                    var method = getMemberMapDeSerializeMethod(field.Field.FieldType) ??
                                 getMemberDeSerializer(field.Field.FieldType);
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
        ///     二进制数据反序列化
        /// </summary>
        /// <typeparam name="TValueType">对象类型</typeparam>
        internal static class TmphTypeDeSerializer<TValueType>
        {
            /// <summary>
            ///     二进制数据序列化类型配置
            /// </summary>
            private static readonly TmphDataSerialize attribute;

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
            ///     固定分组填充字节数
            /// </summary>
            private static readonly int fixedFillSize;

            /// <summary>
            ///     序列化成员数量
            /// </summary>
            private static readonly int memberCountVerify;

            /// <summary>
            ///     是否值类型
            /// </summary>
            private static readonly bool isValueType;

            /// <summary>
            ///     是否支持循环引用处理
            /// </summary>
            internal static readonly bool IsReferenceMember;

            static TmphTypeDeSerializer()
            {
                Type type = typeof(TValueType), TAttributeType;
                var methodInfo = getDeSerializeMethod(type);
                attribute = type.customAttribute<TmphDataSerialize>(out TAttributeType, true) ?? TmphDataSerialize.Default;
                if (methodInfo != null)
                {
                    var dynamicMethod = new DynamicMethod("dataDeSerializer", typeof(void),
                        new[] { typeof(TmphDataDeSerializer), type.MakeByRefType() }, true);
                    dynamicMethod.InitLocals = true;
                    var generator = dynamicMethod.GetILGenerator();
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldarg_1);
                    generator.Emit(OpCodes.Call, methodInfo);
                    generator.Emit(OpCodes.Ret);
                    DefaultDeSerializer = (TmphDeSerialize)dynamicMethod.CreateDelegate(typeof(TmphDeSerialize));
                    IsReferenceMember = false;
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
                                    IsReferenceMember = false;
                                }
                                else if (elementType.IsGenericType &&
                                         elementType.GetGenericTypeDefinition() == typeof(Nullable<>))
                                {
                                    methodInfo =
                                        nullableArrayMethod.MakeGenericMethod(
                                            elementType = elementType.GetGenericArguments()[0]);
                                    IsReferenceMember =
                                        TmphDataSerializer.TmphTypeSerializer.IsReferenceMember(elementType);
                                }
                                else
                                {
                                    methodInfo = structArrayMethod.MakeGenericMethod(elementType);
                                    IsReferenceMember =
                                        TmphDataSerializer.TmphTypeSerializer.IsReferenceMember(elementType);
                                }
                            }
                            else
                            {
                                methodInfo = arrayMethod.MakeGenericMethod(elementType);
                                IsReferenceMember = TmphDataSerializer.TmphTypeSerializer.IsReferenceMember(elementType);
                            }
                            DefaultDeSerializer =
                                (TmphDeSerialize)Delegate.CreateDelegate(typeof(TmphDeSerialize), methodInfo);
                            return;
                        }
                    }
                    DefaultDeSerializer = fromNull;
                    IsReferenceMember = false;
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
                    IsReferenceMember = false;
                    isValueType = true;
                    return;
                }
                if (type.IsPointer)
                {
                    DefaultDeSerializer = fromNull;
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
                        DefaultDeSerializer =
                            (TmphDeSerialize)
                                Delegate.CreateDelegate(typeof(TmphDeSerialize),
                                    subArrayDeSerializeMethod.MakeGenericMethod(type.GetGenericArguments()));
                        IsReferenceMember = TmphDataSerializer.TmphTypeSerializer.IsReferenceMember(parameterTypes[0]);
                        isValueType = true;
                        return;
                    }
                    if (genericType == typeof(Dictionary<,>))
                    {
                        DefaultDeSerializer =
                            (TmphDeSerialize)
                                Delegate.CreateDelegate(typeof(TmphDeSerialize),
                                    dictionaryDeSerializeMethod.MakeGenericMethod(type.GetGenericArguments()));
                        IsReferenceMember = TmphDataSerializer.TmphTypeSerializer.IsReferenceMember(parameterTypes[0]) ||
                                            TmphDataSerializer.TmphTypeSerializer.IsReferenceMember(parameterTypes[1]);
                        isValueType = true;
                        return;
                    }
                    if (genericType == typeof(TmphNullValue<>))
                    {
                        DefaultDeSerializer =
                            (TmphDeSerialize)
                                Delegate.CreateDelegate(typeof(TmphDeSerialize),
                                    nullableDeSerializeMethod.MakeGenericMethod(type.GetGenericArguments()));
                        IsReferenceMember = TmphDataSerializer.TmphTypeSerializer.IsReferenceMember(parameterTypes[0]);
                        isValueType = true;
                        return;
                    }
                    if (genericType == typeof(KeyValuePair<,>))
                    {
                        DefaultDeSerializer =
                            (TmphDeSerialize)
                                Delegate.CreateDelegate(typeof(TmphDeSerialize),
                                    keyValuePairDeSerializeMethod.MakeGenericMethod(type.GetGenericArguments()));
                        IsReferenceMember = TmphDataSerializer.TmphTypeSerializer.IsReferenceMember(parameterTypes[0]) ||
                                            TmphDataSerializer.TmphTypeSerializer.IsReferenceMember(parameterTypes[1]);
                        isValueType = true;
                        return;
                    }
                    if (genericType == typeof(SortedDictionary<,>))
                    {
                        DefaultDeSerializer =
                            (TmphDeSerialize)
                                Delegate.CreateDelegate(typeof(TmphDeSerialize),
                                    sortedDictionaryDeSerializeMethod.MakeGenericMethod(type.GetGenericArguments()));
                        IsReferenceMember = TmphDataSerializer.TmphTypeSerializer.IsReferenceMember(parameterTypes[0]) ||
                                            TmphDataSerializer.TmphTypeSerializer.IsReferenceMember(parameterTypes[1]);
                        isValueType = true;
                        return;
                    }
                    if (genericType == typeof(SortedList<,>))
                    {
                        DefaultDeSerializer =
                            (TmphDeSerialize)
                                Delegate.CreateDelegate(typeof(TmphDeSerialize),
                                    sortedListDeSerializeMethod.MakeGenericMethod(type.GetGenericArguments()));
                        IsReferenceMember = TmphDataSerializer.TmphTypeSerializer.IsReferenceMember(parameterTypes[0]) ||
                                            TmphDataSerializer.TmphTypeSerializer.IsReferenceMember(parameterTypes[1]);
                        isValueType = true;
                        return;
                    }
                }
                if ((methodInfo = TmphDataSerializer.TmphTypeSerializer.GetCustom(type, false)) != null)
                {
                    DefaultDeSerializer = (TmphDeSerialize)Delegate.CreateDelegate(typeof(TmphDeSerialize), methodInfo);
                    IsReferenceMember = attribute.IsReferenceMember;
                    isValueType = true;
                    return;
                }
                if (type.IsAbstract || type.IsInterface || TmphConstructor<TValueType>.New == null)
                {
                    DefaultDeSerializer = noConstructor;
                    isValueType = IsReferenceMember = true;
                    return;
                }
                IsReferenceMember = attribute.IsReferenceMember;
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
                                methodInfo =
                                    (type.IsValueType ? structCollectionMethod : classCollectionMethod)
                                        .MakeGenericMethod(type, argumentType);
                                break;
                            }
                            parameters[0] = typeof(IList<>).MakeGenericType(argumentType);
                            constructorInfo =
                                type.GetConstructor(
                                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null,
                                    parameters, null);
                            if (constructorInfo != null)
                            {
                                methodInfo =
                                    (type.IsValueType ? structCollectionMethod : classCollectionMethod)
                                        .MakeGenericMethod(type, argumentType);
                                break;
                            }
                            parameters[0] = typeof(ICollection<>).MakeGenericType(argumentType);
                            constructorInfo =
                                type.GetConstructor(
                                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null,
                                    parameters, null);
                            if (constructorInfo != null)
                            {
                                methodInfo =
                                    (type.IsValueType ? structCollectionMethod : classCollectionMethod)
                                        .MakeGenericMethod(type, argumentType);
                                break;
                            }
                            parameters[0] = typeof(IEnumerable<>).MakeGenericType(argumentType);
                            constructorInfo =
                                type.GetConstructor(
                                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null,
                                    parameters, null);
                            if (constructorInfo != null)
                            {
                                methodInfo =
                                    (type.IsValueType ? structCollectionMethod : classCollectionMethod)
                                        .MakeGenericMethod(type, argumentType);
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
                                methodInfo =
                                    (type.IsValueType
                                        ? structDictionaryDeSerializeMethod
                                        : classDictionaryDeSerializeMethod).MakeGenericMethod(type, parameters[0],
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
                if (typeof(Code.CSharp.TmphDataSerialize.ISerialize).IsAssignableFrom(type))
                {
                    methodInfo =
                        (type.IsValueType ? structISerializeMethod : classISerializeMethod).MakeGenericMethod(type);
                    DefaultDeSerializer = (TmphDeSerialize)Delegate.CreateDelegate(typeof(TmphDeSerialize), methodInfo);
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
                                    DefaultDeSerializer =
                                        (TmphDeSerialize)Delegate.CreateDelegate(typeof(TmphDeSerialize), methodInfo);
                                    return;
                                }
                                break;
                            }
                        }
                    }
                    var fields =
                        TmphDataSerializer.TmphTypeSerializer.GetFields(
                            TmphMemberIndexGroup<TValueType>.GetFields(attribute.MemberFilter), out memberCountVerify);
                    fixedFillSize = -fields.FixedSize & 3;
                    var fixedDynamicMethod = new TmphTypeDeSerializer.TmphMemberDynamicMethod(type);
                    var fixedMemberMapDynamicMethod = attribute.IsMemberMap
                        ? new TmphTypeDeSerializer.TmphMemberMapDynamicMethod(type)
                        : default(TmphTypeDeSerializer.TmphMemberMapDynamicMethod);
                    foreach (var member in fields.FixedFields)
                    {
                        fixedDynamicMethod.Push(member);
                        if (attribute.IsMemberMap) fixedMemberMapDynamicMethod.Push(member);
                    }
                    fixedMemberDeSerializer = (TmphDeSerialize)fixedDynamicMethod.Create<TmphDeSerialize>();
                    if (attribute.IsMemberMap)
                        fixedMemberMapDeSerializer =
                            (TmphMemberMapDeSerialize)fixedMemberMapDynamicMethod.Create<TmphMemberMapDeSerialize>();

                    var dynamicMethod = new TmphTypeDeSerializer.TmphMemberDynamicMethod(type);
                    var memberMapDynamicMethod = attribute.IsMemberMap
                        ? new TmphTypeDeSerializer.TmphMemberMapDynamicMethod(type)
                        : default(TmphTypeDeSerializer.TmphMemberMapDynamicMethod);
                    foreach (var member in fields.Fields)
                    {
                        dynamicMethod.Push(member);
                        if (attribute.IsMemberMap) memberMapDynamicMethod.Push(member);
                    }
                    memberDeSerializer = (TmphDeSerialize)dynamicMethod.Create<TmphDeSerialize>();
                    if (attribute.IsMemberMap)
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
            internal static void DeSerialize(TmphDataDeSerializer deSerializer, ref TValueType value)
            {
                if (isValueType) StructDeSerialize(deSerializer, ref value);
                else ClassDeSerialize(deSerializer, ref value);
            }

            /// <summary>
            ///     对象序列化
            /// </summary>
            /// <param name="deSerializer">二进制数据反序列化</param>
            /// <param name="value">数据对象</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            internal static void StructDeSerialize(TmphDataDeSerializer deSerializer, ref TValueType value)
            {
                if (DefaultDeSerializer == null) MemberDeSerialize(deSerializer, ref value);
                else DefaultDeSerializer(deSerializer, ref value);
            }

            /// <summary>
            ///     对象序列化
            /// </summary>
            /// <param name="deSerializer">二进制数据反序列化</param>
            /// <param name="value">数据对象</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            internal static void ClassDeSerialize(TmphDataDeSerializer deSerializer, ref TValueType value)
            {
                if (deSerializer.checkPoint(ref value))
                //if (!attribute.IsAttribute || deSerializer.checkPoint(ref value))
                {
                    if (deSerializer.isRealType()) realType(deSerializer, ref value);
                    else classDeSerialize(deSerializer, ref value);
                }
            }

            /// <summary>
            ///     对象序列化
            /// </summary>
            /// <param name="deSerializer">二进制数据反序列化</param>
            /// <param name="value">数据对象</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private static void classDeSerialize(TmphDataDeSerializer deSerializer, ref TValueType value)
            {
                if (DefaultDeSerializer == null)
                {
                    deSerializer.addPoint(ref value);
                    MemberDeSerialize(deSerializer, ref value);
                }
                else DefaultDeSerializer(deSerializer, ref value);
            }

            /// <summary>
            ///     对象序列化
            /// </summary>
            /// <param name="deSerializer">二进制数据反序列化</param>
            /// <param name="value">数据对象</param>
            internal static void MemberDeSerialize(TmphDataDeSerializer deSerializer, ref TValueType value)
            {
                if (deSerializer.CheckMemberCount(memberCountVerify))
                {
                    fixedMemberDeSerializer(deSerializer, ref value);
                    deSerializer.Read += fixedFillSize;
                    memberDeSerializer(deSerializer, ref value);
                    if (attribute.IsJson || jsonMemberMap != null) deSerializer.parseJson(ref value);
                }
                else if (attribute.IsMemberMap)
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
                else deSerializer.Error(TmphDeSerializeState.MemberMap);
            }

            /// <summary>
            ///     真实类型反序列化
            /// </summary>
            /// <param name="deSerializer">二进制数据反序列化</param>
            /// <param name="objectValue"></param>
            /// <returns></returns>
            internal static void RealType(TmphDataDeSerializer deSerializer, ref TValueType value)
            {
                if (isValueType) StructDeSerialize(deSerializer, ref value);
                else classDeSerialize(deSerializer, ref value);
            }

            /// <summary>
            ///     对象反序列化
            /// </summary>
            /// <param name="deSerializer">二进制数据反序列化</param>
            /// <param name="value">数据对象</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            internal static void BaseDeSerialize<childType>(TmphDataDeSerializer deSerializer, ref childType value)
                where childType : TValueType
            {
                if (value == null) value = TmphConstructor<childType>.New();
                TValueType newValue = value;
                classDeSerialize(deSerializer, ref newValue);
            }

            /// <summary>
            ///     找不到构造函数
            /// </summary>
            /// <param name="deSerializer">二进制数据反序列化</param>
            /// <param name="value">数据对象</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private static void noConstructor(TmphDataDeSerializer deSerializer, ref TValueType value)
            {
                if (deSerializer.isObjectRealType) deSerializer.Error(TmphDeSerializeState.NotNull);
                else realType(deSerializer, ref value);
            }

            /// <summary>
            ///     真实类型
            /// </summary>
            /// <param name="deSerializer">二进制数据反序列化</param>
            /// <param name="value">数据对象</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private static void realType(TmphDataDeSerializer deSerializer, ref TValueType value)
            {
                var remoteType = default(TmphRemoteType);
                TmphTypeDeSerializer<TmphRemoteType>.StructDeSerialize(deSerializer, ref remoteType);
                if (deSerializer.state == TmphDeSerializeState.Success)
                {
                    var type = remoteType.Type;
                    if (value == null || type.IsValueType)
                    {
                        value = (TValueType)TmphTypeDeSerializer.GetRealDeSerializer(type)(deSerializer, value);
                    }
                    else TmphTypeDeSerializer.GetRealDeSerializer(type)(deSerializer, value);
                }
            }

            /// <summary>
            ///     不支持对象转换null
            /// </summary>
            /// <param name="deSerializer">二进制数据反序列化</param>
            /// <param name="value">数据对象</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private static void fromNull(TmphDataDeSerializer deSerializer, ref TValueType value)
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
            private static void enumByte(TmphDataDeSerializer deSerializer, ref TValueType value)
            {
                deSerializer.enumByte(ref value);
            }

            /// <summary>
            ///     枚举值序列化
            /// </summary>
            /// <param name="deSerializer">二进制数据反序列化</param>
            /// <param name="array">枚举值序列化</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private static void enumSByte(TmphDataDeSerializer deSerializer, ref TValueType value)
            {
                deSerializer.enumSByte(ref value);
            }

            /// <summary>
            ///     枚举值序列化
            /// </summary>
            /// <param name="deSerializer">二进制数据反序列化</param>
            /// <param name="array">枚举值序列化</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private static void enumShort(TmphDataDeSerializer deSerializer, ref TValueType value)
            {
                deSerializer.enumShort(ref value);
            }

            /// <summary>
            ///     枚举值序列化
            /// </summary>
            /// <param name="deSerializer">二进制数据反序列化</param>
            /// <param name="array">枚举值序列化</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private static void enumUShort(TmphDataDeSerializer deSerializer, ref TValueType value)
            {
                deSerializer.enumUShort(ref value);
            }

            /// <summary>
            ///     枚举值序列化
            /// </summary>
            /// <param name="deSerializer">二进制数据反序列化</param>
            /// <param name="array">枚举值序列化</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private static void enumInt(TmphDataDeSerializer deSerializer, ref TValueType value)
            {
                deSerializer.enumInt(ref value);
            }

            /// <summary>
            ///     枚举值序列化
            /// </summary>
            /// <param name="deSerializer">二进制数据反序列化</param>
            /// <param name="array">枚举值序列化</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private static void enumUInt(TmphDataDeSerializer deSerializer, ref TValueType value)
            {
                deSerializer.enumUInt(ref value);
            }

            /// <summary>
            ///     枚举值序列化
            /// </summary>
            /// <param name="deSerializer">二进制数据反序列化</param>
            /// <param name="array">枚举值序列化</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private static void enumLong(TmphDataDeSerializer deSerializer, ref TValueType value)
            {
                deSerializer.enumLong(ref value);
            }

            /// <summary>
            ///     枚举值序列化
            /// </summary>
            /// <param name="deSerializer">二进制数据反序列化</param>
            /// <param name="array">枚举值序列化</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private static void enumULong(TmphDataDeSerializer deSerializer, ref TValueType value)
            {
                deSerializer.enumULong(ref value);
            }

            /// <summary>
            ///     二进制数据反序列化委托
            /// </summary>
            /// <param name="deSerializer">二进制数据反序列化</param>
            /// <param name="value">目标数据</param>
            internal delegate void TmphDeSerialize(TmphDataDeSerializer deSerializer, ref TValueType value);

            /// <summary>
            ///     二进制数据反序列化委托
            /// </summary>
            /// <param name="memberMap">成员位图</param>
            /// <param name="deSerializer">二进制数据反序列化</param>
            /// <param name="value">目标数据</param>
            private delegate void TmphMemberMapDeSerialize(
                TmphMemberMap memberMap, TmphDataDeSerializer deSerializer, ref TValueType value);
        }
    }
}