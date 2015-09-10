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

namespace Laurent.Lee.CLB.Emit
{
    /// <summary>
    ///     二进制数据序列化
    /// </summary>
    public abstract unsafe class TmphBinaryDeSerializer
    {
        /// <summary>
        ///     反序列化状态
        /// </summary>
        public enum TmphDeSerializeState : byte
        {
            /// <summary>
            ///     成功
            /// </summary>
            Success,

            /// <summary>
            ///     数据不可识别
            /// </summary>
            UnknownData,

            /// <summary>
            ///     成员位图检测失败
            /// </summary>
            MemberMap,

            /// <summary>
            ///     成员位图类型错误
            /// </summary>
            MemberMapType,

            /// <summary>
            ///     成员位图数量验证失败
            /// </summary>
            MemberMapVerify,

            /// <summary>
            ///     头部数据不匹配
            /// </summary>
            HeaderError,

            /// <summary>
            ///     结束验证错误
            /// </summary>
            EndVerify,

            /// <summary>
            ///     数据完整检测失败
            /// </summary>
            FullDataError,

            /// <summary>
            ///     没有命中历史对象
            /// </summary>
            NoPoint,

            /// <summary>
            ///     数据长度不足
            /// </summary>
            IndexOutOfRange,

            /// <summary>
            ///     不支持对象null解析检测失败
            /// </summary>
            NotNull,

            /// <summary>
            ///     成员索引检测失败
            /// </summary>
            MemberIndex,

            /// <summary>
            ///     JSON反序列化失败
            /// </summary>
            JsonError
        }

        /// <summary>
        ///     公共默认配置参数
        /// </summary>
        protected static readonly TmphConfig defaultConfig = new TmphConfig { IsDisposeMemberMap = true };

        /// <summary>
        ///     反序列化配置参数
        /// </summary>
        protected TmphConfig DeSerializeConfig;

        /// <summary>
        ///     序列化数据结束位置
        /// </summary>
        protected byte* end;

        /// <summary>
        ///     是否序列化成员位图
        /// </summary>
        protected bool isMemberMap;

        /// <summary>
        ///     成员位图
        /// </summary>
        protected TmphMemberMap MemberMap;

        /// <summary>
        ///     当前读取数据位置
        /// </summary>
        public byte* Read;

        /// <summary>
        ///     序列化数据起始位置
        /// </summary>
        protected byte* start;

        /// <summary>
        ///     反序列化状态
        /// </summary>
        protected TmphDeSerializeState state;

        /// <summary>
        ///     数据字节数组
        /// </summary>
        public byte[] TmphBuffer { get; protected set; }

        /// <summary>
        ///     检测反序列化状态
        /// </summary>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void checkState()
        {
            if (state == TmphDeSerializeState.Success)
            {
                if (DeSerializeConfig.IsFullData)
                {
                    if (Read != end) Error(TmphDeSerializeState.FullDataError);
                }
                else if (Read <= end)
                {
                    var length = *(int*)Read;
                    if (length == Read - start) DeSerializeConfig.DataLength = length + sizeof(int);
                    Error(TmphDeSerializeState.EndVerify);
                }
                else Error(TmphDeSerializeState.EndVerify);
            }
        }

        /// <summary>
        ///     释放资源
        /// </summary>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void free()
        {
            if (DeSerializeConfig.IsDisposeMemberMap)
            {
                if (MemberMap != null)
                {
                    MemberMap.Dispose();
                    MemberMap = null;
                }
            }
            else MemberMap = null;
        }

        /// <summary>
        ///     设置错误状态
        /// </summary>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public void Error(TmphDeSerializeState eDeSerializeState)
        {
            state = eDeSerializeState;
            if (DeSerializeConfig.IsLogError) TmphLog.Error.Add(eDeSerializeState.ToString(), true, false);
            if (DeSerializeConfig.IsThrowError) throw new Exception(eDeSerializeState.ToString());
        }

        /// <summary>
        ///     自定义序列化重置当前读取数据位置
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public bool VerifyRead(int size)
        {
            if ((Read += size) <= end) return true;
            Error(TmphDeSerializeState.IndexOutOfRange);
            return false;
        }

        /// <summary>
        ///     JSON反序列化
        /// </summary>
        /// <param name="value"></param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void parseJson<TValueType>(ref TValueType value)
        {
            var size = *(int*)Read;
            if (size == 0)
            {
                Read += sizeof(int);
                return;
            }
            if (size > 0 && (size & 1) == 0)
            {
                var start = Read;
                if ((Read += (size + (2 + sizeof(int))) & (int.MaxValue - 3)) <= end)
                {
                    if (!TmphJsonParser.Parse((char*)start, size >> 1, ref value)) Error(TmphDeSerializeState.JsonError);
                    return;
                }
            }
            Error(TmphDeSerializeState.IndexOutOfRange);
        }

        /// <summary>
        ///     不支持对象null解析检测
        /// </summary>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void checkNull()
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue) Read += sizeof(int);
            else Error(TmphDeSerializeState.NotNull);
        }

        /// <summary>
        ///     对象null值检测
        /// </summary>
        /// <returns></returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal int CheckNull()
        {
            if (*(int*)Read == TmphBinarySerializer.NullValue)
            {
                Read += sizeof(int);
                return 0;
            }
            return 1;
        }

        /// <summary>
        ///     检测成员数量
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public bool CheckMemberCount(int count)
        {
            if (*(int*)Read == count)
            {
                Read += sizeof(int);
                return true;
            }
            return false;
        }

        /// <summary>
        ///     成员位图反序列化
        /// </summary>
        /// <param name="fieldCount"></param>
        /// <returns></returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal ulong DeSerializeMemberMap(int fieldCount)
        {
            if (*(int*)Read == fieldCount)
            {
                var value = *(ulong*)(Read + sizeof(int));
                Read += sizeof(int) + sizeof(ulong);
                return value;
            }
            Error(TmphDeSerializeState.MemberMapVerify);
            return 0;
        }

        /// <summary>
        ///     成员位图反序列化
        /// </summary>
        /// <param name="map"></param>
        /// <param name="fieldCount"></param>
        /// <param name="size"></param>
        internal void DeSerializeMemberMap(byte* map, int fieldCount, int size)
        {
            if (*(int*)Read == fieldCount)
            {
                if (size <= (int)(end - (Read += sizeof(int))))
                {
                    for (var mapEnd = map + (size & (int.MaxValue - sizeof(ulong) + 1));
                        map != mapEnd;
                        map += sizeof(ulong), Read += sizeof(ulong))
                        *(ulong*)map = *(ulong*)Read;
                    if ((size & sizeof(int)) != 0)
                    {
                        *(uint*)map = *(uint*)Read;
                        Read += sizeof(uint);
                    }
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
            else Error(TmphDeSerializeState.MemberMapVerify);
        }

        /// <summary>
        ///     检测成员位图
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <returns></returns>
        public TmphMemberMap CheckMemberMap<TValueType>()
        {
            if ((*(uint*)Read & 0xc0000000U) == 0)
            {
                if (MemberMap == null)
                {
                    MemberMap = TmphMemberMap<TValueType>.New();
                    if (*Read == 0)
                    {
                        Read += sizeof(int);
                        return MemberMap;
                    }
                }
                else
                {
                    if (MemberMap.Type != TmphMemberMap<TValueType>.Type)
                    {
                        Error(TmphDeSerializeState.MemberMapType);
                        return null;
                    }
                    if (*Read == 0)
                    {
                        MemberMap.Clear();
                        Read += sizeof(int);
                        return MemberMap;
                    }
                }
                MemberMap.FieldDeSerialize(this);
                return state == TmphDeSerializeState.Success ? MemberMap : null;
            }
            Error(TmphDeSerializeState.MemberMap);
            return null;
        }

        /// <summary>
        ///     逻辑值反序列化
        /// </summary>
        /// <param name="value">逻辑值</param>
        [TmphIndexDeSerializer.TmphMemberMapDeSerializeMethod]
        [TmphIndexDeSerializer.TmphDeSerializeMethod]
        [TmphDataDeSerializer.TmphDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void deSerialize(ref bool value)
        {
            value = *(bool*)Read;
            Read += sizeof(int);
        }

        /// <summary>
        ///     逻辑值反序列化
        /// </summary>
        /// <param name="value">逻辑值</param>
        [TmphIndexDeSerializer.TmphMemberDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void memberDeSerialize(ref bool value)
        {
            value = *(bool*)Read++;
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal static byte* DeSerialize(byte* data, bool[] value)
        {
            var arrayMap = new TmphArrayMap(data);
            for (var index = 0; index != value.Length; ++index) value[index] = arrayMap.Next() != 0;
            return arrayMap.Read;
        }

        /// <summary>
        ///     逻辑值反序列化
        /// </summary>
        /// <param name="value">逻辑值</param>
        [TmphIndexDeSerializer.TmphDeSerializeMethod]
        [TmphDataDeSerializer.TmphDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void deSerialize(ref bool? value)
        {
            value = *(bool*)Read;
            Read += sizeof(int);
        }

        /// <summary>
        ///     逻辑值反序列化
        /// </summary>
        /// <param name="value">逻辑值</param>
        [TmphIndexDeSerializer.TmphMemberDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void memberDeSerialize(ref bool? value)
        {
            if (*Read == 0) value = null;
            else value = *Read == 2;
            ++Read;
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        internal static byte* DeSerialize(byte* data, bool?[] value)
        {
            var arrayMap = new TmphArrayMap(data, 2);
            for (var index = 0; index != value.Length; ++index) value[index] = arrayMap.NextBool();
            return arrayMap.Read;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexDeSerializer.TmphMemberMapDeSerializeMethod]
        [TmphIndexDeSerializer.TmphDeSerializeMethod]
        [TmphDataDeSerializer.TmphDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void deSerialize(ref byte value)
        {
            value = *Read;
            Read += sizeof(int);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexDeSerializer.TmphMemberDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void memberDeSerialize(ref byte value)
        {
            value = *Read++;
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal static byte* DeSerialize(byte* data, byte[] value)
        {
            Unsafe.TmphMemory.Copy(data, value, value.Length);
            return data + ((value.Length + 3) & (int.MaxValue - 3));
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexDeSerializer.TmphDeSerializeMethod]
        [TmphDataDeSerializer.TmphDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void deSerialize(ref TmphSubArray<byte> value)
        {
            var length = *(int*)Read;
            if (length == 0)
            {
                value.UnsafeSetLength(0);
                Read += sizeof(int);
            }
            else
            {
                if (((length + (3 + sizeof(int))) & (int.MaxValue - 3)) <= (int)(end - Read))
                {
                    var array = new byte[length];
                    Read = DeSerialize(Read + sizeof(int), array);
                    value.UnsafeSet(array, 0, length);
                }
                else Error(TmphDeSerializeState.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     反序列化
        /// </summary>
        /// <param name="value"></param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal void DeSerialize(ref TmphSubArray<byte> value)
        {
            var read = DeSerialize(Read, end, TmphBuffer, ref value);
            if (read == null) Error(TmphDeSerializeState.IndexOutOfRange);
            else Read = read;
        }

        /// <summary>
        ///     反序列化
        /// </summary>
        /// <param name="value"></param>
        internal static byte* DeSerialize(byte* read, byte* end, byte[] TmphBuffer, ref TmphSubArray<byte> value)
        {
            var length = *(int*)read;
            if (length > 0)
            {
                var start = read;
                if ((read += (length + (3 + sizeof(int))) & (int.MaxValue - 3)) <= end)
                {
                    fixed (byte* bufferFixed = TmphBuffer)
                    {
                        value.UnsafeSet(TmphBuffer, (int)(start - bufferFixed) + sizeof(int), length);
                        return read;
                    }
                }
            }
            else if (length == 0)
            {
                value.UnsafeSet(TmphNullValue<byte>.Array, 0, 0);
                return read + sizeof(int);
            }
            else if (length == TmphBinarySerializer.NullValue)
            {
                value.Null();
                return read + sizeof(int);
            }
            return null;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexDeSerializer.TmphDeSerializeMethod]
        [TmphDataDeSerializer.TmphDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void deSerialize(ref byte? value)
        {
            value = *Read;
            Read += sizeof(int);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexDeSerializer.TmphMemberDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void memberDeSerialize(ref byte? value)
        {
            if (*(Read + sizeof(byte)) == 0) value = *Read;
            else value = null;
            Read += sizeof(ushort);
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal static byte* DeSerialize(byte* data, byte?[] value)
        {
            var arrayMap = new TmphArrayMap(data);
            var start = (data += ((value.Length + 31) >> 5) << 2);
            for (var index = 0; index != value.Length; ++index)
            {
                if (arrayMap.Next() == 0) value[index] = null;
                else value[index] = *data++;
            }
            return data + ((int)(start - data) & 3);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexDeSerializer.TmphMemberMapDeSerializeMethod]
        [TmphIndexDeSerializer.TmphDeSerializeMethod]
        [TmphDataDeSerializer.TmphDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void deSerialize(ref sbyte value)
        {
            value = (sbyte)*(int*)Read;
            Read += sizeof(int);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexDeSerializer.TmphMemberDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void memberDeSerialize(ref sbyte value)
        {
            value = *(sbyte*)Read++;
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal static byte* DeSerialize(byte* data, sbyte[] value)
        {
            fixed (sbyte* valueFixed = value) Unsafe.TmphMemory.Copy(data, valueFixed, value.Length);
            return data + ((value.Length + 3) & (int.MaxValue - 3));
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexDeSerializer.TmphDeSerializeMethod]
        [TmphDataDeSerializer.TmphDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void deSerialize(ref sbyte? value)
        {
            value = (sbyte)*(int*)Read;
            Read += sizeof(int);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexDeSerializer.TmphMemberDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void memberDeSerialize(ref sbyte? value)
        {
            if (*(Read + sizeof(byte)) == 0) value = *(sbyte*)Read;
            else value = null;
            Read += sizeof(ushort);
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal static byte* DeSerialize(sbyte* data, sbyte?[] value)
        {
            var arrayMap = new TmphArrayMap((byte*)data);
            var start = (data += ((value.Length + 31) >> 5) << 2);
            for (var index = 0; index != value.Length; ++index)
            {
                if (arrayMap.Next() == 0) value[index] = null;
                else value[index] = *data++;
            }
            return (byte*)(data + ((int)(start - data) & 3));
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexDeSerializer.TmphMemberMapDeSerializeMethod]
        [TmphIndexDeSerializer.TmphDeSerializeMethod]
        [TmphDataDeSerializer.TmphDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void deSerialize(ref short value)
        {
            value = (short)*(int*)Read;
            Read += sizeof(int);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexDeSerializer.TmphMemberDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void memberDeSerialize(ref short value)
        {
            value = *(short*)Read;
            Read += sizeof(short);
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal static byte* DeSerialize(byte* data, short[] value)
        {
            var length = value.Length * sizeof(short);
            fixed (short* valueFixed = value) Unsafe.TmphMemory.Copy(data, valueFixed, length);
            return data + ((length + 3) & (int.MaxValue - 3));
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexDeSerializer.TmphDeSerializeMethod]
        [TmphDataDeSerializer.TmphDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void deSerialize(ref short? value)
        {
            value = (short)*(int*)Read;
            Read += sizeof(int);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexDeSerializer.TmphMemberDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void memberDeSerialize(ref short? value)
        {
            if (*(ushort*)(Read + sizeof(ushort)) == 0) value = *(short*)Read;
            else value = null;
            Read += sizeof(int);
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal static byte* DeSerialize(byte* data, short?[] value)
        {
            var arrayMap = new TmphArrayMap(data);
            short* read = (short*)(data + (((value.Length + 31) >> 5) << 2)), start = read;
            for (var index = 0; index != value.Length; ++index)
            {
                if (arrayMap.Next() == 0) value[index] = null;
                else value[index] = *read++;
            }
            return ((int)((byte*)read - (byte*)start) & 2) == 0 ? (byte*)read : (byte*)(read + 1);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexDeSerializer.TmphMemberMapDeSerializeMethod]
        [TmphIndexDeSerializer.TmphDeSerializeMethod]
        [TmphDataDeSerializer.TmphDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void deSerialize(ref ushort value)
        {
            value = *(ushort*)Read;
            Read += sizeof(int);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexDeSerializer.TmphMemberDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void memberDeSerialize(ref ushort value)
        {
            value = *(ushort*)Read;
            Read += sizeof(ushort);
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal static byte* DeSerialize(byte* data, ushort[] value)
        {
            var length = value.Length * sizeof(ushort);
            fixed (ushort* valueFixed = value) Unsafe.TmphMemory.Copy(data, valueFixed, length);
            return data + ((length + 3) & (int.MaxValue - 3));
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexDeSerializer.TmphDeSerializeMethod]
        [TmphDataDeSerializer.TmphDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void deSerialize(ref ushort? value)
        {
            value = *(ushort*)Read;
            Read += sizeof(int);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexDeSerializer.TmphMemberDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void memberDeSerialize(ref ushort? value)
        {
            if (*(ushort*)(Read + sizeof(ushort)) == 0) value = *(ushort*)Read;
            else value = null;
            Read += sizeof(int);
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal static byte* DeSerialize(byte* data, ushort?[] value)
        {
            var arrayMap = new TmphArrayMap(data);
            ushort* read = (ushort*)(data + (((value.Length + 31) >> 5) << 2)), start = read;
            for (var index = 0; index != value.Length; ++index)
            {
                if (arrayMap.Next() == 0) value[index] = null;
                else value[index] = *read++;
            }
            return ((int)((byte*)read - (byte*)start) & 2) == 0 ? (byte*)read : (byte*)(read + 1);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexDeSerializer.TmphDeSerializeMethod]
        [TmphIndexDeSerializer.TmphMemberDeSerializeMethod]
        [TmphIndexDeSerializer.TmphMemberMapDeSerializeMethod]
        [TmphDataDeSerializer.TmphDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void deSerialize(ref int value)
        {
            value = *(int*)Read;
            Read += sizeof(int);
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal static byte* DeSerialize(byte* data, int[] value)
        {
            var length = value.Length * sizeof(int);
            fixed (int* valueFixed = value) Unsafe.TmphMemory.Copy(data, valueFixed, length);
            return data + length;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexDeSerializer.TmphDeSerializeMethod]
        [TmphDataDeSerializer.TmphDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void deSerialize(ref int? value)
        {
            value = *(int*)Read;
            Read += sizeof(int);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexDeSerializer.TmphMemberDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void memberDeSerialize(ref int? value)
        {
            if (*(int*)Read == 0)
            {
                value = *(int*)(Read += sizeof(int));
                Read += sizeof(int);
            }
            else
            {
                Read += sizeof(int);
                value = null;
            }
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal static byte* DeSerialize(byte* data, int?[] value)
        {
            var arrayMap = new TmphArrayMap(data);
            var read = (int*)(data + (((value.Length + 31) >> 5) << 2));
            for (var index = 0; index != value.Length; ++index)
            {
                if (arrayMap.Next() == 0) value[index] = null;
                else value[index] = *read++;
            }
            return (byte*)read;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexDeSerializer.TmphDeSerializeMethod]
        [TmphIndexDeSerializer.TmphMemberDeSerializeMethod]
        [TmphIndexDeSerializer.TmphMemberMapDeSerializeMethod]
        [TmphDataDeSerializer.TmphDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void deSerialize(ref uint value)
        {
            value = *(uint*)Read;
            Read += sizeof(int);
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal static byte* DeSerialize(byte* data, uint[] value)
        {
            var length = value.Length * sizeof(uint);
            fixed (uint* valueFixed = value) Unsafe.TmphMemory.Copy(data, valueFixed, length);
            return data + length;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexDeSerializer.TmphDeSerializeMethod]
        [TmphDataDeSerializer.TmphDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void deSerialize(ref uint? value)
        {
            value = *(uint*)Read;
            Read += sizeof(int);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexDeSerializer.TmphMemberDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void memberDeSerialize(ref uint? value)
        {
            if (*(int*)Read == 0)
            {
                value = *(uint*)(Read += sizeof(int));
                Read += sizeof(uint);
            }
            else
            {
                Read += sizeof(int);
                value = null;
            }
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal static byte* DeSerialize(byte* data, uint?[] value)
        {
            var arrayMap = new TmphArrayMap(data);
            var read = (uint*)(data + (((value.Length + 31) >> 5) << 2));
            for (var index = 0; index != value.Length; ++index)
            {
                if (arrayMap.Next() == 0) value[index] = null;
                else value[index] = *read++;
            }
            return (byte*)read;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexDeSerializer.TmphDeSerializeMethod]
        [TmphIndexDeSerializer.TmphMemberDeSerializeMethod]
        [TmphIndexDeSerializer.TmphMemberMapDeSerializeMethod]
        [TmphDataDeSerializer.TmphDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void deSerialize(ref long value)
        {
            value = *(long*)Read;
            Read += sizeof(long);
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal static byte* DeSerialize(byte* data, long[] value)
        {
            var length = value.Length * sizeof(long);
            fixed (long* valueFixed = value) Unsafe.TmphMemory.Copy(data, valueFixed, length);
            return data + length;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexDeSerializer.TmphDeSerializeMethod]
        [TmphDataDeSerializer.TmphDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void deSerialize(ref long? value)
        {
            value = *(long*)Read;
            Read += sizeof(long);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexDeSerializer.TmphMemberDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void memberDeSerialize(ref long? value)
        {
            if (*(int*)Read == 0)
            {
                value = *(long*)(Read += sizeof(int));
                Read += sizeof(long);
            }
            else
            {
                Read += sizeof(int);
                value = null;
            }
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal static byte* DeSerialize(byte* data, long?[] value)
        {
            var arrayMap = new TmphArrayMap(data);
            var read = (long*)(data + (((value.Length + 31) >> 5) << 2));
            for (var index = 0; index != value.Length; ++index)
            {
                if (arrayMap.Next() == 0) value[index] = null;
                else value[index] = *read++;
            }
            return (byte*)read;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexDeSerializer.TmphDeSerializeMethod]
        [TmphIndexDeSerializer.TmphMemberDeSerializeMethod]
        [TmphIndexDeSerializer.TmphMemberMapDeSerializeMethod]
        [TmphDataDeSerializer.TmphDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void deSerialize(ref ulong value)
        {
            value = *(ulong*)Read;
            Read += sizeof(ulong);
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal static byte* DeSerialize(byte* data, ulong[] value)
        {
            var length = value.Length * sizeof(ulong);
            fixed (ulong* valueFixed = value) Unsafe.TmphMemory.Copy(data, valueFixed, length);
            return data + length;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexDeSerializer.TmphDeSerializeMethod]
        [TmphDataDeSerializer.TmphDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void deSerialize(ref ulong? value)
        {
            value = *(ulong*)Read;
            Read += sizeof(ulong);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexDeSerializer.TmphMemberDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void memberDeSerialize(ref ulong? value)
        {
            if (*(int*)Read == 0)
            {
                value = *(ulong*)(Read += sizeof(int));
                Read += sizeof(ulong);
            }
            else
            {
                Read += sizeof(int);
                value = null;
            }
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal static byte* DeSerialize(byte* data, ulong?[] value)
        {
            var arrayMap = new TmphArrayMap(data);
            var read = (ulong*)(data + (((value.Length + 31) >> 5) << 2));
            for (var index = 0; index != value.Length; ++index)
            {
                if (arrayMap.Next() == 0) value[index] = null;
                else value[index] = *read++;
            }
            return (byte*)read;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexDeSerializer.TmphDeSerializeMethod]
        [TmphIndexDeSerializer.TmphMemberDeSerializeMethod]
        [TmphIndexDeSerializer.TmphMemberMapDeSerializeMethod]
        [TmphDataDeSerializer.TmphDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void deSerialize(ref float value)
        {
            value = *(float*)Read;
            Read += sizeof(float);
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal static byte* DeSerialize(byte* data, float[] value)
        {
            var length = value.Length * sizeof(float);
            fixed (float* valueFixed = value) Unsafe.TmphMemory.Copy(data, valueFixed, length);
            return data + length;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexDeSerializer.TmphDeSerializeMethod]
        [TmphDataDeSerializer.TmphDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void deSerialize(ref float? value)
        {
            value = *(float*)Read;
            Read += sizeof(float);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexDeSerializer.TmphMemberDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void memberDeSerialize(ref float? value)
        {
            if (*(int*)Read == 0)
            {
                value = *(float*)(Read += sizeof(int));
                Read += sizeof(float);
            }
            else
            {
                Read += sizeof(int);
                value = null;
            }
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal static byte* DeSerialize(byte* data, float?[] value)
        {
            var arrayMap = new TmphArrayMap(data);
            var read = (float*)(data + (((value.Length + 31) >> 5) << 2));
            for (var index = 0; index != value.Length; ++index)
            {
                if (arrayMap.Next() == 0) value[index] = null;
                else value[index] = *read++;
            }
            return (byte*)read;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexDeSerializer.TmphDeSerializeMethod]
        [TmphIndexDeSerializer.TmphMemberDeSerializeMethod]
        [TmphIndexDeSerializer.TmphMemberMapDeSerializeMethod]
        [TmphDataDeSerializer.TmphDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void deSerialize(ref double value)
        {
            value = *(double*)Read;
            Read += sizeof(double);
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal static byte* DeSerialize(byte* data, double[] value)
        {
            var length = value.Length * sizeof(double);
            fixed (double* valueFixed = value) Unsafe.TmphMemory.Copy(data, valueFixed, length);
            return data + length;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexDeSerializer.TmphDeSerializeMethod]
        [TmphDataDeSerializer.TmphDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void deSerialize(ref double? value)
        {
            value = *(double*)Read;
            Read += sizeof(double);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexDeSerializer.TmphMemberDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void memberDeSerialize(ref double? value)
        {
            if (*(int*)Read == 0)
            {
                value = *(double*)(Read += sizeof(int));
                Read += sizeof(double);
            }
            else
            {
                Read += sizeof(int);
                value = null;
            }
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal static byte* DeSerialize(byte* data, double?[] value)
        {
            var arrayMap = new TmphArrayMap(data);
            var read = (double*)(data + (((value.Length + 31) >> 5) << 2));
            for (var index = 0; index != value.Length; ++index)
            {
                if (arrayMap.Next() == 0) value[index] = null;
                else value[index] = *read++;
            }
            return (byte*)read;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexDeSerializer.TmphDeSerializeMethod]
        [TmphIndexDeSerializer.TmphMemberDeSerializeMethod]
        [TmphIndexDeSerializer.TmphMemberMapDeSerializeMethod]
        [TmphDataDeSerializer.TmphDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void deSerialize(ref decimal value)
        {
            value = *(decimal*)Read;
            Read += sizeof(decimal);
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal static byte* DeSerialize(byte* data, decimal[] value)
        {
            var length = value.Length * sizeof(decimal);
            fixed (decimal* valueFixed = value) Unsafe.TmphMemory.Copy(data, valueFixed, length);
            return data + length;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexDeSerializer.TmphDeSerializeMethod]
        [TmphDataDeSerializer.TmphDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void deSerialize(ref decimal? value)
        {
            value = *(decimal*)Read;
            Read += sizeof(decimal);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexDeSerializer.TmphMemberDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void memberDeSerialize(ref decimal? value)
        {
            if (*(int*)Read == 0)
            {
                value = *(decimal*)(Read += sizeof(int));
                Read += sizeof(decimal);
            }
            else
            {
                Read += sizeof(int);
                value = null;
            }
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal static byte* DeSerialize(byte* data, decimal?[] value)
        {
            var arrayMap = new TmphArrayMap(data);
            var read = (decimal*)(data + (((value.Length + 31) >> 5) << 2));
            for (var index = 0; index != value.Length; ++index)
            {
                if (arrayMap.Next() == 0) value[index] = null;
                else value[index] = *read++;
            }
            return (byte*)read;
        }

        /// <summary>
        ///     字符反序列化
        /// </summary>
        /// <param name="value">字符</param>
        [TmphIndexDeSerializer.TmphMemberMapDeSerializeMethod]
        [TmphIndexDeSerializer.TmphDeSerializeMethod]
        [TmphDataDeSerializer.TmphDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void deSerialize(ref char value)
        {
            value = *(char*)Read;
            Read += sizeof(int);
        }

        /// <summary>
        ///     字符反序列化
        /// </summary>
        /// <param name="value">字符</param>
        [TmphIndexDeSerializer.TmphMemberDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void memberDeSerialize(ref char value)
        {
            value = *(char*)Read;
            Read += sizeof(char);
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal static byte* DeSerialize(byte* data, char[] value)
        {
            var length = value.Length * sizeof(char);
            fixed (char* valueFixed = value) Unsafe.TmphMemory.Copy(data, valueFixed, length);
            return data + ((length + 3) & (int.MaxValue - 3));
        }

        /// <summary>
        ///     字符反序列化
        /// </summary>
        /// <param name="value">字符</param>
        [TmphIndexDeSerializer.TmphDeSerializeMethod]
        [TmphDataDeSerializer.TmphDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void deSerialize(ref char? value)
        {
            value = *(char*)Read;
            Read += sizeof(int);
        }

        /// <summary>
        ///     字符反序列化
        /// </summary>
        /// <param name="value">字符</param>
        [TmphIndexDeSerializer.TmphMemberDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void memberDeSerialize(ref char? value)
        {
            if (*(ushort*)(Read + sizeof(char)) == 0) value = *(char*)Read;
            else value = null;
            Read += sizeof(int);
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal static byte* DeSerialize(byte* data, char?[] value)
        {
            var arrayMap = new TmphArrayMap(data);
            char* read = (char*)(data + (((value.Length + 31) >> 5) << 2)), start = read;
            for (var index = 0; index != value.Length; ++index)
            {
                if (arrayMap.Next() == 0) value[index] = null;
                else value[index] = *read++;
            }
            return ((int)((byte*)read - (byte*)start) & 2) == 0 ? (byte*)read : (byte*)(read + 1);
        }

        /// <summary>
        ///     时间反序列化
        /// </summary>
        /// <param name="value">时间</param>
        [TmphIndexDeSerializer.TmphDeSerializeMethod]
        [TmphIndexDeSerializer.TmphMemberDeSerializeMethod]
        [TmphIndexDeSerializer.TmphMemberMapDeSerializeMethod]
        [TmphDataDeSerializer.TmphDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void deSerialize(ref DateTime value)
        {
            value = *(DateTime*)Read;
            Read += sizeof(DateTime);
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal static byte* DeSerialize(byte* data, DateTime[] value)
        {
            var length = value.Length * sizeof(DateTime);
            fixed (DateTime* valueFixed = value) Unsafe.TmphMemory.Copy(data, valueFixed, length);
            return data + length;
        }

        /// <summary>
        ///     时间反序列化
        /// </summary>
        /// <param name="value">时间</param>
        [TmphIndexDeSerializer.TmphDeSerializeMethod]
        [TmphDataDeSerializer.TmphDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void deSerialize(ref DateTime? value)
        {
            value = *(DateTime*)Read;
            Read += sizeof(DateTime);
        }

        /// <summary>
        ///     时间反序列化
        /// </summary>
        /// <param name="value">时间</param>
        [TmphIndexDeSerializer.TmphMemberDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void memberDeSerialize(ref DateTime? value)
        {
            if (*(int*)Read == 0)
            {
                value = *(DateTime*)(Read += sizeof(int));
                Read += sizeof(DateTime);
            }
            else
            {
                Read += sizeof(int);
                value = null;
            }
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal static byte* DeSerialize(byte* data, DateTime?[] value)
        {
            var arrayMap = new TmphArrayMap(data);
            var read = (DateTime*)(data + (((value.Length + 31) >> 5) << 2));
            for (var index = 0; index != value.Length; ++index)
            {
                if (arrayMap.Next() == 0) value[index] = null;
                else value[index] = *read++;
            }
            return (byte*)read;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexDeSerializer.TmphDeSerializeMethod]
        [TmphIndexDeSerializer.TmphMemberDeSerializeMethod]
        [TmphIndexDeSerializer.TmphMemberMapDeSerializeMethod]
        [TmphDataDeSerializer.TmphDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void deSerialize(ref Guid value)
        {
            value = *(Guid*)Read;
            Read += sizeof(Guid);
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal static byte* DeSerialize(byte* data, Guid[] value)
        {
            var length = value.Length * sizeof(Guid);
            fixed (Guid* valueFixed = value) Unsafe.TmphMemory.Copy(data, valueFixed, length);
            return data + length;
        }

        /// <summary>
        ///     Guid反序列化
        /// </summary>
        /// <param name="value">Guid</param>
        [TmphIndexDeSerializer.TmphDeSerializeMethod]
        [TmphDataDeSerializer.TmphDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void deSerialize(ref Guid? value)
        {
            value = *(Guid*)Read;
            Read += sizeof(Guid);
        }

        /// <summary>
        ///     Guid反序列化
        /// </summary>
        /// <param name="value">Guid</param>
        [TmphIndexDeSerializer.TmphMemberDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void memberDeSerialize(ref Guid? value)
        {
            if (*(int*)Read == 0)
            {
                value = *(Guid*)(Read += sizeof(int));
                Read += sizeof(Guid);
            }
            else
            {
                Read += sizeof(int);
                value = null;
            }
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal static byte* DeSerialize(byte* data, Guid?[] value)
        {
            var arrayMap = new TmphArrayMap(data);
            var read = (Guid*)(data + (((value.Length + 31) >> 5) << 2));
            for (var index = 0; index != value.Length; ++index)
            {
                if (arrayMap.Next() == 0) value[index] = null;
                else value[index] = *read++;
            }
            return (byte*)read;
        }

        [TmphIndexDeSerializer.TmphDeSerializeMethod]
        [TmphDataDeSerializer.TmphDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberDeSerializeMethod]
        [TmphDataDeSerializer.TmphMemberMapDeSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void deSerialize(ref TmphSubString value)
        {
            string stringValue = null;
            if ((Read = DeSerialize(Read, end, ref stringValue)) == null) Error(TmphDeSerializeState.IndexOutOfRange);
            else value.UnsafeSet(stringValue, 0, stringValue.Length);
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="end">结束位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置,失败返回null</returns>
        internal static byte* DeSerialize(byte* data, byte* end, ref string value)
        {
            var length = *(int*)data;
            if ((length & 1) == 0)
            {
                if (length != 0)
                {
                    var dataLength = (length + (3 + sizeof(int))) & (int.MaxValue - 3);
                    if (dataLength <= end - data)
                    {
                        value = new string((char*)(data + sizeof(int)), 0, length >> 1);
                        return data + dataLength;
                    }
                }
                else
                {
                    value = string.Empty;
                    return data + sizeof(int);
                }
            }
            else
            {
                var dataLength = ((length >>= 1) + (3 + sizeof(int))) & (int.MaxValue - 3);
                if (dataLength <= end - data)
                {
                    fixed (char* valueFixed = (value = TmphString.FastAllocateString(length)))
                    {
                        var start = data + sizeof(int);
                        var write = valueFixed;
                        end = start + length;
                        do
                        {
                            *write++ = (char)*start++;
                        } while (start != end);
                    }
                    return data + dataLength;
                }
            }
            return null;
        }

        /// <summary>
        ///     枚举值序列化
        /// </summary>
        /// <param name="deSerializer">二进制数据反序列化</param>
        /// <param name="array">枚举值序列化</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void enumByte<TValueType>(ref TValueType value)
        {
            value = TmphPub.TmphEnumCast<TValueType, byte>.FromInt(*Read);
            Read += sizeof(int);
        }

        /// <summary>
        ///     枚举值序列化
        /// </summary>
        /// <param name="deSerializer">二进制数据反序列化</param>
        /// <param name="array">枚举值序列化</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void enumByteMember<TValueType>(ref TValueType value)
        {
            value = TmphPub.TmphEnumCast<TValueType, byte>.FromInt(*Read++);
        }

        /// <summary>
        ///     枚举值序列化
        /// </summary>
        /// <param name="deSerializer">二进制数据反序列化</param>
        /// <param name="array">枚举值序列化</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void enumSByte<TValueType>(ref TValueType value)
        {
            value = TmphPub.TmphEnumCast<TValueType, sbyte>.FromInt((sbyte)*(int*)Read);
            Read += sizeof(int);
        }

        /// <summary>
        ///     枚举值序列化
        /// </summary>
        /// <param name="deSerializer">二进制数据反序列化</param>
        /// <param name="array">枚举值序列化</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void enumSByteMember<TValueType>(ref TValueType value)
        {
            value = TmphPub.TmphEnumCast<TValueType, sbyte>.FromInt(*(sbyte*)Read++);
        }

        /// <summary>
        ///     枚举值序列化
        /// </summary>
        /// <param name="deSerializer">二进制数据反序列化</param>
        /// <param name="array">枚举值序列化</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void enumShort<TValueType>(ref TValueType value)
        {
            value = TmphPub.TmphEnumCast<TValueType, short>.FromInt((short)*(int*)Read);
            Read += sizeof(int);
        }

        /// <summary>
        ///     枚举值序列化
        /// </summary>
        /// <param name="deSerializer">二进制数据反序列化</param>
        /// <param name="array">枚举值序列化</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void enumShortMember<TValueType>(ref TValueType value)
        {
            value = TmphPub.TmphEnumCast<TValueType, short>.FromInt(*(short*)Read);
            Read += sizeof(short);
        }

        /// <summary>
        ///     枚举值序列化
        /// </summary>
        /// <param name="deSerializer">二进制数据反序列化</param>
        /// <param name="array">枚举值序列化</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void enumUShort<TValueType>(ref TValueType value)
        {
            value = TmphPub.TmphEnumCast<TValueType, ushort>.FromInt(*(ushort*)Read);
            Read += sizeof(int);
        }

        /// <summary>
        ///     枚举值序列化
        /// </summary>
        /// <param name="deSerializer">二进制数据反序列化</param>
        /// <param name="array">枚举值序列化</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void enumUShortMember<TValueType>(ref TValueType value)
        {
            value = TmphPub.TmphEnumCast<TValueType, ushort>.FromInt(*(ushort*)Read);
            Read += sizeof(ushort);
        }

        /// <summary>
        ///     枚举值序列化
        /// </summary>
        /// <param name="deSerializer">二进制数据反序列化</param>
        /// <param name="array">枚举值序列化</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void enumInt<TValueType>(ref TValueType value)
        {
            value = TmphPub.TmphEnumCast<TValueType, int>.FromInt(*(int*)Read);
            Read += sizeof(int);
        }

        /// <summary>
        ///     枚举值序列化
        /// </summary>
        /// <param name="deSerializer">二进制数据反序列化</param>
        /// <param name="array">枚举值序列化</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void enumUInt<TValueType>(ref TValueType value)
        {
            value = TmphPub.TmphEnumCast<TValueType, uint>.FromInt(*(uint*)Read);
            Read += sizeof(uint);
        }

        /// <summary>
        ///     枚举值序列化
        /// </summary>
        /// <param name="deSerializer">二进制数据反序列化</param>
        /// <param name="array">枚举值序列化</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void enumLong<TValueType>(ref TValueType value)
        {
            value = TmphPub.TmphEnumCast<TValueType, long>.FromInt(*(long*)Read);
            Read += sizeof(long);
        }

        /// <summary>
        ///     枚举值序列化
        /// </summary>
        /// <param name="deSerializer">二进制数据反序列化</param>
        /// <param name="array">枚举值序列化</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void enumULong<TValueType>(ref TValueType value)
        {
            value = TmphPub.TmphEnumCast<TValueType, ulong>.FromInt(*(ulong*)Read);
            Read += sizeof(ulong);
        }

        /// <summary>
        ///     数组位图
        /// </summary>
        internal struct TmphArrayMap
        {
            /// <summary>
            ///     当前位
            /// </summary>
            public uint Bit;

            /// <summary>
            ///     当前位图
            /// </summary>
            public uint Map;

            /// <summary>
            ///     当前读取位置
            /// </summary>
            public byte* Read;

            /// <summary>
            ///     数组位图
            /// </summary>
            /// <param name="read">当前读取位置</param>
            public TmphArrayMap(byte* read)
            {
                Read = read;
                Bit = 1;
                Map = 0;
            }

            /// <summary>
            ///     数组位图
            /// </summary>
            /// <param name="read">当前读取位置</param>
            /// <param name="bit">当前位</param>
            public TmphArrayMap(byte* read, uint bit)
            {
                Read = read;
                Bit = bit;
                Map = 0;
            }

            /// <summary>
            ///     获取位图数据
            /// </summary>
            /// <returns></returns>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            public uint Next()
            {
                if (Bit == 1)
                {
                    Map = *(uint*)Read;
                    Bit = 1U << 31;
                    Read += sizeof(uint);
                }
                else Bit >>= 1;
                return Map & Bit;
            }

            /// <summary>
            ///     获取位图数据
            /// </summary>
            /// <returns></returns>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            public bool? NextBool()
            {
                if (Bit == 2)
                {
                    Map = *(uint*)Read;
                    Bit = 1U << 31;
                    Read += sizeof(uint);
                }
                else Bit >>= 2;
                if ((Map & Bit) == 0) return null;
                return (Map & (Bit >> 1)) != 0;
            }
        }

        /// <summary>
        ///     配置参数
        /// </summary>
        public sealed class TmphConfig
        {
            /// <summary>
            ///     是否自动释放成员位图
            /// </summary>
            internal bool IsDisposeMemberMap;

            /// <summary>
            ///     数据是否完整
            /// </summary>
            public bool IsFullData = true;

            /// <summary>
            ///     是否输出错误日志
            /// </summary>
            public bool IsLogError = true;

            /// <summary>
            ///     是否抛出错误异常
            /// </summary>
            public bool IsThrowError;

            /// <summary>
            ///     成员位图
            /// </summary>
            public TmphMemberMap MemberMap;

            /// <summary>
            ///     反序列化状态
            /// </summary>
            public TmphDeSerializeState State;

            /// <summary>
            ///     数据长度
            /// </summary>
            public int DataLength { get; internal set; }
        }
    }
}