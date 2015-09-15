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
using Laurent.Lee.CLB.Config;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Laurent.Lee.CLB.Emit
{
    /// <summary>
    ///     二进制数据序列化
    /// </summary>
    public abstract unsafe class TmphBinarySerializer : IDisposable
    {
        /// <summary>
        ///     空对象
        /// </summary>
        public const int NullValue = int.MinValue;

        /// <summary>
        ///     序列化输出缓冲区字段信息
        /// </summary>
        internal static readonly FieldInfo StreamField = typeof(TmphBinarySerializer).GetField("Stream",
            BindingFlags.Instance | BindingFlags.Public);

        /// <summary>
        ///     序列化输出缓冲区
        /// </summary>
        public readonly TmphUnmanagedStream Stream = new TmphUnmanagedStream((byte*)TmphPub.PuzzleValue, 1);

        /// <summary>
        ///     序列化配置参数
        /// </summary>
        protected TmphConfig binarySerializerConfig;

        /// <summary>
        ///     成员位图
        /// </summary>
        protected TmphMemberMap currentMemberMap;

        /// <summary>
        ///     JSON序列化配置参数
        /// </summary>
        private TmphJsonSerializer.TmphConfig jsonConfig;

        /// <summary>
        ///     JSON序列化成员位图
        /// </summary>
        private TmphMemberMap jsonMemberMap;

        /// <summary>
        ///     JSON序列化输出缓冲区
        /// </summary>
        private TmphCharStream jsonStream;

        /// <summary>
        ///     成员位图
        /// </summary>
        protected TmphMemberMap memberMap;

        /// </summary>
        /// 数据流起始位置
        /// </summary>
        protected int streamStartIndex;

        /// <summary>
        ///     释放资源
        /// </summary>
        public void Dispose()
        {
            CLB.TmphPub.Dispose(ref jsonMemberMap);
        }

        /// <summary>
        ///     释放资源
        /// </summary>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void free()
        {
            memberMap = currentMemberMap = null;
        }

        /// <summary>
        ///     获取JSON序列化输出缓冲区
        /// </summary>
        /// <param name="data"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal TmphCharStream ResetJsonStream(void* data, int size)
        {
            if (jsonStream == null) return jsonStream = new TmphCharStream((char*)data, size >> 1);
            jsonStream.Reset((byte*)data, size);
            return jsonStream;
        }

        /// <summary>
        ///     获取JSON序列化配置参数
        /// </summary>
        /// <param name="memberMap"></param>
        /// <returns></returns>
        protected TmphJsonSerializer.TmphConfig getJsonConfig(TmphMemberMap memberMap)
        {
            if (jsonConfig == null) jsonConfig = new TmphJsonSerializer.TmphConfig { CheckLoopDepth = TmphAppSetting.JsonDepth };
            jsonConfig.MemberMap = memberMap;
            return jsonConfig;
        }

        /// <summary>
        ///     获取JSON成员位图
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="memberMap"></param>
        /// <param name="memberIndexs"></param>
        /// <returns></returns>
        protected TmphMemberMap getJsonMemberMap<TValueType>(TmphMemberMap memberMap, int[] memberIndexs)
        {
            var count = 0;
            foreach (var memberIndex in memberIndexs)
            {
                if (memberMap.IsMember(memberIndex))
                {
                    if (count == 0 && jsonMemberMap == null) jsonMemberMap = TmphMemberMap<TValueType>.Empty();
                    jsonMemberMap.SetMember(memberIndex);
                    ++count;
                }
            }
            return count == 0 ? null : jsonMemberMap;
        }

        /// <summary>
        ///     序列化成员位图
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <returns></returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public TmphMemberMap SerializeMemberMap<TValueType>()
        {
            if (memberMap != null)
            {
                currentMemberMap = memberMap;
                memberMap = null;
                if (currentMemberMap.Type == TmphMemberMap<TValueType>.Type)
                {
                    currentMemberMap.FieldSerialize(Stream);
                    return currentMemberMap;
                }
                if (binarySerializerConfig.IsMemberMapErrorLog) TmphLog.Error.Add("二进制序列化成员位图类型匹配失败", true, true);
            }
            return null;
        }

        /// <summary>
        ///     逻辑值序列化
        /// </summary>
        /// <param name="value">逻辑值</param>
        [TmphIndexSerializer.TmphSerializeMethod]
        [TmphDataSerializer.TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void serialize(bool value)
        {
            Stream.Write(value ? 1 : 0);
        }

        /// <summary>
        ///     逻辑值序列化
        /// </summary>
        /// <param name="value">逻辑值</param>
        [TmphIndexSerializer.TmphMemberSerializeMethod]
        [TmphDataSerializer.TmphMemberSerializeMethod]
        [TmphDataSerializer.TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void memberSerialize(bool value)
        {
            Stream.Unsafer.Write(value);
        }

        /// <summary>
        ///     预增数据流长度并写入长度与数据(4字节对齐)
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="data">数据,不能为null</param>
        public static void Serialize(TmphUnmanagedStream stream, bool[] array)
        {
            var arrayMap = new TmphArrayMap(stream, array.Length);
            foreach (var value in array) arrayMap.Next(value);
            arrayMap.End(stream);
        }

        /// <summary>
        ///     逻辑值序列化
        /// </summary>
        /// <param name="value">逻辑值</param>
        [TmphIndexSerializer.TmphSerializeMethod]
        [TmphDataSerializer.TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void serialize(bool? value)
        {
            Stream.Write((bool)value ? 1 : 0);
        }

        /// <summary>
        ///     逻辑值序列化
        /// </summary>
        /// <param name="value">逻辑值</param>
        [TmphIndexSerializer.TmphMemberSerializeMethod]
        [TmphDataSerializer.TmphMemberSerializeMethod]
        [TmphDataSerializer.TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void memberSerialize(bool? value)
        {
            if (value.HasValue) Stream.Unsafer.Write((bool)value ? (byte)2 : (byte)1);
            else Stream.Unsafer.Write((byte)0);
        }

        /// <summary>
        ///     预增数据流长度并写入长度与数据(4字节对齐)
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="data">数据,不能为null</param>
        internal static void Serialize(TmphUnmanagedStream stream, bool?[] array)
        {
            var arrayMap = new TmphArrayMap(stream, array.Length << 1);
            foreach (var value in array) arrayMap.Next(value);
            arrayMap.End(stream);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexSerializer.TmphSerializeMethod]
        [TmphDataSerializer.TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void serialize(byte value)
        {
            Stream.Write((uint)value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexSerializer.TmphMemberSerializeMethod]
        [TmphDataSerializer.TmphMemberSerializeMethod]
        [TmphDataSerializer.TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void memberSerialize(byte value)
        {
            Stream.Unsafer.Write(value);
        }

        /// <summary>
        ///     预增数据流长度并写入长度与数据(4字节对齐)
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="data">数据,不能为null</param>
        /// <param name="length">数据数量</param>
        /// <param name="size">单个数据字节数</param>
        public static void Serialize(TmphUnmanagedStream stream, void* data, int arrayLength, int size)
        {
            int dataSize = arrayLength * size, length = (dataSize + (3 + sizeof(int))) & (int.MaxValue - 3);
            stream.PrepLength(length);
            var write = stream.CurrentData;
            *(int*)write = arrayLength;
            Unsafe.TmphMemory.Copy(data, write + sizeof(int), dataSize);
            stream.Unsafer.AddLength(length);
            stream.PrepLength();
        }

        /// <summary>
        ///     预增数据流长度并写入长度与数据(4字节对齐)
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="data">数据,不能为null</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal static void Serialize(TmphUnmanagedStream stream, byte[] data)
        {
            fixed (byte* dataFixed = data) Serialize(stream, dataFixed, data.Length, 1);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexSerializer.TmphSerializeMethod]
        [TmphDataSerializer.TmphSerializeMethod]
        [TmphDataSerializer.TmphMemberSerializeMethod]
        [TmphDataSerializer.TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void serialize(TmphSubArray<byte> value)
        {
            if (value.Count == 0) Stream.Write(0);
            else Serialize(Stream, value);
        }

        /// <summary>
        ///     预增数据流长度并序列化数据(4字节对齐)
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="value">数据,不能为null</param>
        internal static void Serialize(TmphUnmanagedStream stream, TmphSubArray<byte> data)
        {
            var length = sizeof(int) + ((data.Count + 3) & (int.MaxValue - 3));
            stream.PrepLength(length);
            var write = stream.CurrentData;
            *(int*)write = data.Count;
            fixed (byte* dataFixed = data.Array)
                Unsafe.TmphMemory.Copy(dataFixed + data.StartIndex, write + sizeof(int), data.Count);
            stream.Unsafer.AddLength(length);
            stream.PrepLength();
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexSerializer.TmphSerializeMethod]
        [TmphDataSerializer.TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void serialize(byte? value)
        {
            Stream.Write((uint)(byte)value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexSerializer.TmphMemberSerializeMethod]
        [TmphDataSerializer.TmphMemberSerializeMethod]
        [TmphDataSerializer.TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void memberSerialize(byte? value)
        {
            if (value.HasValue) Stream.Unsafer.Write((ushort)(byte)value);
            else Stream.Unsafer.Write(short.MinValue);
        }

        /// <summary>
        ///     序列化可空数组
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="array">数组数据</param>
        internal static void Serialize(TmphUnmanagedStream stream, byte?[] array)
        {
            var arrayMap = new TmphArrayMap(stream, array.Length, array.Length);
            var write = stream.CurrentData;
            foreach (var value in array)
            {
                if (value.HasValue)
                {
                    arrayMap.Next(true);
                    *write++ = (byte)value;
                }
                else arrayMap.Next(false);
            }
            stream.Unsafer.AddLength(((int)(write - stream.CurrentData) + 3) & (int.MaxValue - 3));
            arrayMap.End(stream);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexSerializer.TmphSerializeMethod]
        [TmphDataSerializer.TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void serialize(sbyte value)
        {
            Stream.Write((int)value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexSerializer.TmphMemberSerializeMethod]
        [TmphDataSerializer.TmphMemberSerializeMethod]
        [TmphDataSerializer.TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void memberSerialize(sbyte value)
        {
            Stream.Unsafer.Write(value);
        }

        /// <summary>
        ///     预增数据流长度并写入长度与数据(4字节对齐)
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="data">数据,不能为null</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal static void Serialize(TmphUnmanagedStream stream, sbyte[] data)
        {
            fixed (sbyte* dataFixed = data) Serialize(stream, dataFixed, data.Length, 1);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexSerializer.TmphSerializeMethod]
        [TmphDataSerializer.TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void serialize(sbyte? value)
        {
            Stream.Write((int)(sbyte)value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexSerializer.TmphMemberSerializeMethod]
        [TmphDataSerializer.TmphMemberSerializeMethod]
        [TmphDataSerializer.TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void memberSerialize(sbyte? value)
        {
            if (value.HasValue) Stream.Unsafer.Write((ushort)(byte)(sbyte)value);
            else Stream.Unsafer.Write(short.MinValue);
        }

        /// <summary>
        ///     序列化可空数组
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="array">数组数据</param>
        internal static void Serialize(TmphUnmanagedStream stream, sbyte?[] array)
        {
            var arrayMap = new TmphArrayMap(stream, array.Length, array.Length);
            var write = (sbyte*)stream.CurrentData;
            foreach (var value in array)
            {
                if (value.HasValue)
                {
                    arrayMap.Next(true);
                    *write++ = (sbyte)value;
                }
                else arrayMap.Next(false);
            }
            stream.Unsafer.AddLength(((int)(write - (sbyte*)stream.CurrentData) + 3) & (int.MaxValue - 3));
            arrayMap.End(stream);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexSerializer.TmphSerializeMethod]
        [TmphDataSerializer.TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void serialize(short value)
        {
            Stream.Write((int)value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexSerializer.TmphMemberSerializeMethod]
        [TmphDataSerializer.TmphMemberSerializeMethod]
        [TmphDataSerializer.TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void memberSerialize(short value)
        {
            Stream.Unsafer.Write(value);
        }

        /// <summary>
        ///     预增数据流长度并写入长度与数据(4字节对齐)
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="data">数据,不能为null</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal static void Serialize(TmphUnmanagedStream stream, short[] data)
        {
            fixed (short* dataFixed = data) Serialize(stream, dataFixed, data.Length, sizeof(short));
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexSerializer.TmphSerializeMethod]
        [TmphDataSerializer.TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void serialize(short? value)
        {
            Stream.Write((int)(short)value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexSerializer.TmphMemberSerializeMethod]
        [TmphDataSerializer.TmphMemberSerializeMethod]
        [TmphDataSerializer.TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void memberSerialize(short? value)
        {
            if (value.HasValue) Stream.Unsafer.Write((uint)(ushort)(short)value);
            else Stream.Unsafer.Write(NullValue);
        }

        /// <summary>
        ///     序列化可空数组
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="array">数组数据</param>
        internal static void Serialize(TmphUnmanagedStream stream, short?[] array)
        {
            var arrayMap = new TmphArrayMap(stream, array.Length, array.Length * sizeof(short));
            var write = (short*)stream.CurrentData;
            foreach (var value in array)
            {
                if (value.HasValue)
                {
                    arrayMap.Next(true);
                    *write++ = (short)value;
                }
                else arrayMap.Next(false);
            }
            stream.Unsafer.AddLength(((int)((byte*)write - stream.CurrentData) + 3) & (int.MaxValue - 3));
            arrayMap.End(stream);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexSerializer.TmphSerializeMethod]
        [TmphDataSerializer.TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void serialize(ushort value)
        {
            Stream.Write((uint)value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexSerializer.TmphMemberSerializeMethod]
        [TmphDataSerializer.TmphMemberSerializeMethod]
        [TmphDataSerializer.TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void memberSerialize(ushort value)
        {
            Stream.Unsafer.Write(value);
        }

        /// <summary>
        ///     预增数据流长度并写入长度与数据(4字节对齐)
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="data">数据,不能为null</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal static void Serialize(TmphUnmanagedStream stream, ushort[] data)
        {
            fixed (ushort* dataFixed = data) Serialize(stream, dataFixed, data.Length, sizeof(ushort));
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexSerializer.TmphSerializeMethod]
        [TmphDataSerializer.TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void serialize(ushort? value)
        {
            Stream.Write((uint)(ushort)value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexSerializer.TmphMemberSerializeMethod]
        [TmphDataSerializer.TmphMemberSerializeMethod]
        [TmphDataSerializer.TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void memberSerialize(ushort? value)
        {
            if (value.HasValue) Stream.Unsafer.Write((uint)(ushort)value);
            else Stream.Unsafer.Write(NullValue);
        }

        /// <summary>
        ///     序列化可空数组
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="array">数组数据</param>
        internal static void Serialize(TmphUnmanagedStream stream, ushort?[] array)
        {
            var arrayMap = new TmphArrayMap(stream, array.Length, array.Length * sizeof(ushort));
            var write = (ushort*)stream.CurrentData;
            foreach (var value in array)
            {
                if (value.HasValue)
                {
                    arrayMap.Next(true);
                    *write++ = (ushort)value;
                }
                else arrayMap.Next(false);
            }
            stream.Unsafer.AddLength(((int)((byte*)write - stream.CurrentData) + 3) & (int.MaxValue - 3));
            arrayMap.End(stream);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexSerializer.TmphSerializeMethod]
        [TmphDataSerializer.TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void serialize(int value)
        {
            Stream.Write(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexSerializer.TmphMemberSerializeMethod]
        [TmphIndexSerializer.TmphMemberMapSerializeMethod]
        [TmphDataSerializer.TmphMemberSerializeMethod]
        [TmphDataSerializer.TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void memberSerialize(int value)
        {
            Stream.Unsafer.Write(value);
        }

        /// <summary>
        ///     预增数据流长度并写入长度与数据(4字节对齐)
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="data">数据,不能为null</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal static void Serialize(TmphUnmanagedStream stream, int[] data)
        {
            fixed (int* dataFixed = data) Serialize(stream, dataFixed, data.Length, sizeof(int));
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexSerializer.TmphSerializeMethod]
        [TmphDataSerializer.TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void serialize(int? value)
        {
            Stream.Write((int)value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexSerializer.TmphMemberSerializeMethod]
        [TmphDataSerializer.TmphMemberSerializeMethod]
        [TmphDataSerializer.TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void memberSerialize(int? value)
        {
            if (value.HasValue)
            {
                var data = Stream.CurrentData;
                *(int*)data = 0;
                *(int*)(data + sizeof(int)) = (int)value;
                Stream.Unsafer.AddLength(sizeof(int) + sizeof(int));
            }
            else Stream.Unsafer.Write(NullValue);
        }

        /// <summary>
        ///     序列化可空数组
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="array">数组数据</param>
        internal static void Serialize(TmphUnmanagedStream stream, int?[] array)
        {
            var arrayMap = new TmphArrayMap(stream, array.Length, array.Length * sizeof(int));
            var write = (int*)stream.CurrentData;
            foreach (var value in array)
            {
                if (value.HasValue)
                {
                    arrayMap.Next(true);
                    *write++ = (int)value;
                }
                else arrayMap.Next(false);
            }
            stream.Unsafer.AddLength((int)((byte*)write - stream.CurrentData));
            arrayMap.End(stream);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexSerializer.TmphSerializeMethod]
        [TmphDataSerializer.TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void serialize(uint value)
        {
            Stream.Write(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexSerializer.TmphMemberSerializeMethod]
        [TmphIndexSerializer.TmphMemberMapSerializeMethod]
        [TmphDataSerializer.TmphMemberSerializeMethod]
        [TmphDataSerializer.TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void memberSerialize(uint value)
        {
            Stream.Unsafer.Write(value);
        }

        /// <summary>
        ///     预增数据流长度并写入长度与数据(4字节对齐)
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="data">数据,不能为null</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal static void Serialize(TmphUnmanagedStream stream, uint[] data)
        {
            fixed (uint* dataFixed = data) Serialize(stream, dataFixed, data.Length, sizeof(uint));
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexSerializer.TmphSerializeMethod]
        [TmphDataSerializer.TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void serialize(uint? value)
        {
            Stream.Write((uint)value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexSerializer.TmphMemberSerializeMethod]
        [TmphDataSerializer.TmphMemberSerializeMethod]
        [TmphDataSerializer.TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void memberSerialize(uint? value)
        {
            if (value.HasValue)
            {
                var data = Stream.CurrentData;
                *(int*)data = 0;
                *(uint*)(data + sizeof(int)) = (uint)value;
                Stream.Unsafer.AddLength(sizeof(uint) + sizeof(int));
            }
            else Stream.Unsafer.Write(NullValue);
        }

        /// <summary>
        ///     序列化可空数组
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="array">数组数据</param>
        internal static void Serialize(TmphUnmanagedStream stream, uint?[] array)
        {
            var arrayMap = new TmphArrayMap(stream, array.Length, array.Length * sizeof(uint));
            var write = (uint*)stream.CurrentData;
            foreach (var value in array)
            {
                if (value.HasValue)
                {
                    arrayMap.Next(true);
                    *write++ = (uint)value;
                }
                else arrayMap.Next(false);
            }
            stream.Unsafer.AddLength((int)((byte*)write - stream.CurrentData));
            arrayMap.End(stream);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexSerializer.TmphSerializeMethod]
        [TmphDataSerializer.TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void serialize(long value)
        {
            Stream.Write(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexSerializer.TmphMemberSerializeMethod]
        [TmphIndexSerializer.TmphMemberMapSerializeMethod]
        [TmphDataSerializer.TmphMemberSerializeMethod]
        [TmphDataSerializer.TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void memberSerialize(long value)
        {
            Stream.Unsafer.Write(value);
        }

        /// <summary>
        ///     预增数据流长度并写入长度与数据(4字节对齐)
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="data">数据,不能为null</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal static void Serialize(TmphUnmanagedStream stream, long[] data)
        {
            fixed (long* dataFixed = data) Serialize(stream, dataFixed, data.Length, sizeof(long));
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexSerializer.TmphSerializeMethod]
        [TmphDataSerializer.TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void serialize(long? value)
        {
            Stream.Write((long)value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexSerializer.TmphMemberSerializeMethod]
        [TmphDataSerializer.TmphMemberSerializeMethod]
        [TmphDataSerializer.TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void memberSerialize(long? value)
        {
            if (value.HasValue)
            {
                var data = Stream.CurrentData;
                *(int*)data = 0;
                *(long*)(data + sizeof(int)) = (long)value;
                Stream.Unsafer.AddLength(sizeof(long) + sizeof(int));
            }
            else Stream.Unsafer.Write(NullValue);
        }

        /// <summary>
        ///     序列化可空数组
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="array">数组数据</param>
        internal static void Serialize(TmphUnmanagedStream stream, long?[] array)
        {
            var arrayMap = new TmphArrayMap(stream, array.Length, array.Length * sizeof(long));
            var write = (long*)stream.CurrentData;
            foreach (var value in array)
            {
                if (value.HasValue)
                {
                    arrayMap.Next(true);
                    *write++ = (long)value;
                }
                else arrayMap.Next(false);
            }
            stream.Unsafer.AddLength((int)((byte*)write - stream.CurrentData));
            arrayMap.End(stream);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexSerializer.TmphSerializeMethod]
        [TmphDataSerializer.TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void serialize(ulong value)
        {
            Stream.Write(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexSerializer.TmphMemberSerializeMethod]
        [TmphIndexSerializer.TmphMemberMapSerializeMethod]
        [TmphDataSerializer.TmphMemberSerializeMethod]
        [TmphDataSerializer.TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void memberSerialize(ulong value)
        {
            Stream.Unsafer.Write(value);
        }

        /// <summary>
        ///     预增数据流长度并写入长度与数据(4字节对齐)
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="data">数据,不能为null</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal static void Serialize(TmphUnmanagedStream stream, ulong[] data)
        {
            fixed (ulong* dataFixed = data) Serialize(stream, dataFixed, data.Length, sizeof(ulong));
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexSerializer.TmphSerializeMethod]
        [TmphDataSerializer.TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void serialize(ulong? value)
        {
            Stream.Write((ulong)value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexSerializer.TmphMemberSerializeMethod]
        [TmphDataSerializer.TmphMemberSerializeMethod]
        [TmphDataSerializer.TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void memberSerialize(ulong? value)
        {
            if (value.HasValue)
            {
                var data = Stream.CurrentData;
                *(int*)data = 0;
                *(ulong*)(data + sizeof(int)) = (ulong)value;
                Stream.Unsafer.AddLength(sizeof(ulong) + sizeof(int));
            }
            else Stream.Unsafer.Write(NullValue);
        }

        /// <summary>
        ///     序列化可空数组
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="array">数组数据</param>
        internal static void Serialize(TmphUnmanagedStream stream, ulong?[] array)
        {
            var arrayMap = new TmphArrayMap(stream, array.Length, array.Length * sizeof(ulong));
            var write = (ulong*)stream.CurrentData;
            foreach (var value in array)
            {
                if (value.HasValue)
                {
                    arrayMap.Next(true);
                    *write++ = (ulong)value;
                }
                else arrayMap.Next(false);
            }
            stream.Unsafer.AddLength((int)((byte*)write - stream.CurrentData));
            arrayMap.End(stream);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexSerializer.TmphSerializeMethod]
        [TmphDataSerializer.TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void serialize(float value)
        {
            Stream.Write(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexSerializer.TmphMemberSerializeMethod]
        [TmphIndexSerializer.TmphMemberMapSerializeMethod]
        [TmphDataSerializer.TmphMemberSerializeMethod]
        [TmphDataSerializer.TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void memberSerialize(float value)
        {
            Stream.Unsafer.Write(value);
        }

        /// <summary>
        ///     预增数据流长度并写入长度与数据(4字节对齐)
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="data">数据,不能为null</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal static void Serialize(TmphUnmanagedStream stream, float[] data)
        {
            fixed (float* dataFixed = data) Serialize(stream, dataFixed, data.Length, sizeof(float));
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexSerializer.TmphSerializeMethod]
        [TmphDataSerializer.TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void serialize(float? value)
        {
            Stream.Write((float)value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexSerializer.TmphMemberSerializeMethod]
        [TmphDataSerializer.TmphMemberSerializeMethod]
        [TmphDataSerializer.TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void memberSerialize(float? value)
        {
            if (value.HasValue)
            {
                var data = Stream.CurrentData;
                *(int*)data = 0;
                *(float*)(data + sizeof(int)) = (float)value;
                Stream.Unsafer.AddLength(sizeof(float) + sizeof(int));
            }
            else Stream.Unsafer.Write(NullValue);
        }

        /// <summary>
        ///     序列化可空数组
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="array">数组数据</param>
        internal static void Serialize(TmphUnmanagedStream stream, float?[] array)
        {
            var arrayMap = new TmphArrayMap(stream, array.Length, array.Length * sizeof(float));
            var write = (float*)stream.CurrentData;
            foreach (var value in array)
            {
                if (value.HasValue)
                {
                    arrayMap.Next(true);
                    *write++ = (float)value;
                }
                else arrayMap.Next(false);
            }
            stream.Unsafer.AddLength((int)((byte*)write - stream.CurrentData));
            arrayMap.End(stream);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexSerializer.TmphSerializeMethod]
        [TmphDataSerializer.TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void serialize(double value)
        {
            Stream.Write(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexSerializer.TmphMemberSerializeMethod]
        [TmphIndexSerializer.TmphMemberMapSerializeMethod]
        [TmphDataSerializer.TmphMemberSerializeMethod]
        [TmphDataSerializer.TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void memberSerialize(double value)
        {
            Stream.Unsafer.Write(value);
        }

        /// <summary>
        ///     预增数据流长度并写入长度与数据(4字节对齐)
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="data">数据,不能为null</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal static void Serialize(TmphUnmanagedStream stream, double[] data)
        {
            fixed (double* dataFixed = data) Serialize(stream, dataFixed, data.Length, sizeof(double));
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexSerializer.TmphSerializeMethod]
        [TmphDataSerializer.TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void serialize(double? value)
        {
            Stream.Write((double)value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexSerializer.TmphMemberSerializeMethod]
        [TmphDataSerializer.TmphMemberSerializeMethod]
        [TmphDataSerializer.TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void memberSerialize(double? value)
        {
            if (value.HasValue)
            {
                var data = Stream.CurrentData;
                *(int*)data = 0;
                *(double*)(data + sizeof(int)) = (double)value;
                Stream.Unsafer.AddLength(sizeof(double) + sizeof(int));
            }
            else Stream.Unsafer.Write(NullValue);
        }

        /// <summary>
        ///     序列化可空数组
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="array">数组数据</param>
        internal static void Serialize(TmphUnmanagedStream stream, double?[] array)
        {
            var arrayMap = new TmphArrayMap(stream, array.Length, array.Length * sizeof(double));
            var write = (double*)stream.CurrentData;
            foreach (var value in array)
            {
                if (value.HasValue)
                {
                    arrayMap.Next(true);
                    *write++ = (double)value;
                }
                else arrayMap.Next(false);
            }
            stream.Unsafer.AddLength((int)((byte*)write - stream.CurrentData));
            arrayMap.End(stream);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexSerializer.TmphSerializeMethod]
        [TmphDataSerializer.TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void serialize(decimal value)
        {
            Stream.Write(value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexSerializer.TmphMemberSerializeMethod]
        [TmphIndexSerializer.TmphMemberMapSerializeMethod]
        [TmphDataSerializer.TmphMemberSerializeMethod]
        [TmphDataSerializer.TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void memberSerialize(decimal value)
        {
            Stream.Unsafer.Write(value);
        }

        /// <summary>
        ///     预增数据流长度并写入长度与数据(4字节对齐)
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="data">数据,不能为null</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static void Serialize(TmphUnmanagedStream stream, decimal[] data)
        {
            fixed (decimal* dataFixed = data) Serialize(stream, dataFixed, data.Length, sizeof(decimal));
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexSerializer.TmphSerializeMethod]
        [TmphDataSerializer.TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void serialize(decimal? value)
        {
            Stream.Write((decimal)value);
        }

        /// <summary>
        ///     数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [TmphIndexSerializer.TmphMemberSerializeMethod]
        [TmphDataSerializer.TmphMemberSerializeMethod]
        [TmphDataSerializer.TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void memberSerialize(decimal? value)
        {
            if (value.HasValue)
            {
                var data = Stream.CurrentData;
                *(int*)data = 0;
                *(decimal*)(data + sizeof(int)) = (decimal)value;
                Stream.Unsafer.AddLength(sizeof(decimal) + sizeof(int));
            }
            else Stream.Unsafer.Write(NullValue);
        }

        /// <summary>
        ///     序列化可空数组
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="array">数组数据</param>
        internal static void Serialize(TmphUnmanagedStream stream, decimal?[] array)
        {
            var arrayMap = new TmphArrayMap(stream, array.Length, array.Length * sizeof(decimal));
            var write = (decimal*)stream.CurrentData;
            foreach (var value in array)
            {
                if (value.HasValue)
                {
                    arrayMap.Next(true);
                    *write++ = (decimal)value;
                }
                else arrayMap.Next(false);
            }
            stream.Unsafer.AddLength((int)((byte*)write - stream.CurrentData));
            arrayMap.End(stream);
        }

        /// <summary>
        ///     字符序列化
        /// </summary>
        /// <param name="value">字符</param>
        [TmphIndexSerializer.TmphSerializeMethod]
        [TmphDataSerializer.TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void serialize(char value)
        {
            Stream.Write((uint)value);
        }

        /// <summary>
        ///     字符序列化
        /// </summary>
        /// <param name="value">字符</param>
        [TmphIndexSerializer.TmphMemberSerializeMethod]
        [TmphDataSerializer.TmphMemberSerializeMethod]
        [TmphDataSerializer.TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void memberSerialize(char value)
        {
            Stream.Unsafer.Write(value);
        }

        /// <summary>
        ///     预增数据流长度并写入长度与数据(4字节对齐)
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="data">数据,不能为null</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal static void Serialize(TmphUnmanagedStream stream, char[] data)
        {
            fixed (char* dataFixed = data) Serialize(stream, dataFixed, data.Length, sizeof(char));
        }

        /// <summary>
        ///     字符序列化
        /// </summary>
        /// <param name="value">字符</param>
        [TmphIndexSerializer.TmphSerializeMethod]
        [TmphDataSerializer.TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void serialize(char? value)
        {
            Stream.Write((uint)(char)value);
        }

        /// <summary>
        ///     字符序列化
        /// </summary>
        /// <param name="value">字符</param>
        [TmphIndexSerializer.TmphMemberSerializeMethod]
        [TmphDataSerializer.TmphMemberSerializeMethod]
        [TmphDataSerializer.TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void memberSerialize(char? value)
        {
            if (value.HasValue) Stream.Unsafer.Write((uint)(char)value);
            else Stream.Unsafer.Write(NullValue);
        }

        /// <summary>
        ///     序列化可空数组
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="array">数组数据</param>
        internal static void Serialize(TmphUnmanagedStream stream, char?[] array)
        {
            var arrayMap = new TmphArrayMap(stream, array.Length, array.Length * sizeof(char));
            var write = (char*)stream.CurrentData;
            foreach (var value in array)
            {
                if (value.HasValue)
                {
                    arrayMap.Next(true);
                    *write++ = (char)value;
                }
                else arrayMap.Next(false);
            }
            stream.Unsafer.AddLength(((int)((byte*)write - stream.CurrentData) + 3) & (int.MaxValue - 3));
            arrayMap.End(stream);
        }

        /// <summary>
        ///     时间序列化
        /// </summary>
        /// <param name="value">时间</param>
        [TmphIndexSerializer.TmphSerializeMethod]
        [TmphDataSerializer.TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void serialize(DateTime value)
        {
            Stream.Write(value);
        }

        /// <summary>
        ///     时间序列化
        /// </summary>
        /// <param name="value">时间</param>
        [TmphIndexSerializer.TmphMemberSerializeMethod]
        [TmphIndexSerializer.TmphMemberMapSerializeMethod]
        [TmphDataSerializer.TmphMemberSerializeMethod]
        [TmphDataSerializer.TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void memberSerialize(DateTime value)
        {
            Stream.Unsafer.Write(value);
        }

        /// <summary>
        ///     预增数据流长度并写入长度与数据(4字节对齐)
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="data">数据,不能为null</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static void Serialize(TmphUnmanagedStream stream, DateTime[] data)
        {
            fixed (DateTime* dataFixed = data) Serialize(stream, dataFixed, data.Length, sizeof(DateTime));
        }

        /// <summary>
        ///     时间序列化
        /// </summary>
        /// <param name="value">时间</param>
        [TmphIndexSerializer.TmphSerializeMethod]
        [TmphDataSerializer.TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void serialize(DateTime? value)
        {
            Stream.Write((DateTime)value);
        }

        /// <summary>
        ///     时间序列化
        /// </summary>
        /// <param name="value">时间</param>
        [TmphIndexSerializer.TmphMemberSerializeMethod]
        [TmphDataSerializer.TmphMemberSerializeMethod]
        [TmphDataSerializer.TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void memberSerialize(DateTime? value)
        {
            if (value.HasValue)
            {
                var data = Stream.CurrentData;
                *(int*)data = 0;
                *(DateTime*)(data + sizeof(int)) = (DateTime)value;
                Stream.Unsafer.AddLength(sizeof(DateTime) + sizeof(int));
            }
            else Stream.Unsafer.Write(NullValue);
        }

        /// <summary>
        ///     序列化可空数组
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="array">数组数据</param>
        internal static void Serialize(TmphUnmanagedStream stream, DateTime?[] array)
        {
            var arrayMap = new TmphArrayMap(stream, array.Length, array.Length * sizeof(DateTime));
            var write = (DateTime*)stream.CurrentData;
            foreach (var value in array)
            {
                if (value.HasValue)
                {
                    arrayMap.Next(true);
                    *write++ = (DateTime)value;
                }
                else arrayMap.Next(false);
            }
            stream.Unsafer.AddLength((int)((byte*)write - stream.CurrentData));
            arrayMap.End(stream);
        }

        /// <summary>
        ///     Guid序列化
        /// </summary>
        /// <param name="value">Guid</param>
        [TmphIndexSerializer.TmphSerializeMethod]
        [TmphDataSerializer.TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void serialize(Guid value)
        {
            Stream.Write(value);
        }

        /// <summary>
        ///     Guid序列化
        /// </summary>
        /// <param name="value">Guid</param>
        [TmphIndexSerializer.TmphMemberSerializeMethod]
        [TmphIndexSerializer.TmphMemberMapSerializeMethod]
        [TmphDataSerializer.TmphMemberSerializeMethod]
        [TmphDataSerializer.TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void memberSerialize(Guid value)
        {
            Stream.Unsafer.Write(value);
        }

        /// <summary>
        ///     预增数据流长度并写入长度与数据(4字节对齐)
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="data">数据,不能为null</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static void Serialize(TmphUnmanagedStream stream, Guid[] data)
        {
            fixed (Guid* dataFixed = data) Serialize(stream, dataFixed, data.Length, sizeof(Guid));
        }

        /// <summary>
        ///     Guid序列化
        /// </summary>
        /// <param name="value">Guid</param>
        [TmphIndexSerializer.TmphSerializeMethod]
        [TmphDataSerializer.TmphSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void serialize(Guid? value)
        {
            Stream.Write((Guid)value);
        }

        /// <summary>
        ///     Guid序列化
        /// </summary>
        /// <param name="value">Guid</param>
        [TmphIndexSerializer.TmphMemberSerializeMethod]
        [TmphDataSerializer.TmphMemberSerializeMethod]
        [TmphDataSerializer.TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void memberSerialize(Guid? value)
        {
            if (value.HasValue)
            {
                var data = Stream.CurrentData;
                *(int*)data = 0;
                *(Guid*)(data + sizeof(int)) = (Guid)value;
                Stream.Unsafer.AddLength(sizeof(Guid) + sizeof(int));
            }
            else Stream.Unsafer.Write(NullValue);
        }

        /// <summary>
        ///     序列化可空数组
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="array">数组数据</param>
        internal static void Serialize(TmphUnmanagedStream stream, Guid?[] array)
        {
            var arrayMap = new TmphArrayMap(stream, array.Length, array.Length * sizeof(Guid));
            var write = (Guid*)stream.CurrentData;
            foreach (var value in array)
            {
                if (value.HasValue)
                {
                    arrayMap.Next(true);
                    *write++ = (Guid)value;
                }
                else arrayMap.Next(false);
            }
            stream.Unsafer.AddLength((int)((byte*)write - stream.CurrentData));
            arrayMap.End(stream);
        }

        /// <summary>
        ///     字符串序列化
        /// </summary>
        /// <param name="valueFixed"></param>
        /// <param name="stream"></param>
        /// <param name="stringLength"></param>
        private static void serialize(char* valueFixed, TmphUnmanagedStream stream, int stringLength)
        {
            char* start = valueFixed, end = valueFixed + stringLength;
            do
            {
                if ((*start & 0xff00) != 0)
                {
                    var length = ((stringLength <<= 1) + (3 + sizeof(int))) & (int.MaxValue - 3);
                    stream.PrepLength(length);
                    start = (char*)stream.CurrentData;
                    Unsafe.TmphMemory.Copy(valueFixed, (byte*)start + sizeof(int), *(int*)start = stringLength);
                    stream.Unsafer.AddLength(length);
                    stream.PrepLength();
                    return;
                }
            } while (++start != end);
            {
                var length = (stringLength + (3 + sizeof(int))) & (int.MaxValue - 3);
                stream.PrepLength(length);
                var write = stream.CurrentData;
                *(int*)write = (stringLength << 1) + 1;
                write += sizeof(int);
                do
                {
                    *write++ = (byte)*valueFixed++;
                } while (valueFixed != end);
                stream.Unsafer.AddLength(length);
                stream.PrepLength();
            }
        }

        /// <summary>
        ///     预增数据流长度并序列化字符串(4字节对齐)
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="value">字符串,不能为null,长度不能为0</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal static void Serialize(TmphUnmanagedStream stream, string value)
        {
            //if (value.Length != 0)
            //{
            fixed (char* valueFixed = value) serialize(valueFixed, stream, value.Length);
            //}
            //else stream.Write(0);
        }

        /// <summary>
        ///     字符串序列化
        /// </summary>
        /// <param name="value">字符串</param>
        [TmphIndexSerializer.TmphSerializeMethod]
        [TmphDataSerializer.TmphSerializeMethod]
        [TmphDataSerializer.TmphMemberSerializeMethod]
        [TmphDataSerializer.TmphMemberMapSerializeMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void serialize(TmphSubString value)
        {
            Serialize(Stream, value);
        }

        /// <summary>
        ///     预增数据流长度并序列化字符串(4字节对齐)
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="value">字符串,不能为null</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal static void Serialize(TmphUnmanagedStream stream, TmphSubString value)
        {
            if (value.Length == 0) stream.Write(0);
            else
            {
                fixed (char* valueFixed = value.value) serialize(valueFixed + value.StartIndex, stream, value.Length);
            }
        }

        /// <summary>
        ///     枚举值序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举值序列化</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void enumByteMember<TValueType>(TValueType value)
        {
            Stream.Unsafer.Write(TmphPub.TmphEnumCast<TValueType, byte>.ToInt(value));
        }

        /// <summary>
        ///     枚举值序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举值序列化</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void enumSByteMember<TValueType>(TValueType value)
        {
            Stream.Unsafer.Write(TmphPub.TmphEnumCast<TValueType, sbyte>.ToInt(value));
        }

        /// <summary>
        ///     枚举值序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举值序列化</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void enumShortMember<TValueType>(TValueType value)
        {
            Stream.Unsafer.Write(TmphPub.TmphEnumCast<TValueType, short>.ToInt(value));
        }

        /// <summary>
        ///     枚举值序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举值序列化</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void enumUShortMember<TValueType>(TValueType value)
        {
            Stream.Unsafer.Write(TmphPub.TmphEnumCast<TValueType, ushort>.ToInt(value));
        }

        /// <summary>
        ///     枚举值序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举值序列化</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void enumIntMember<TValueType>(TValueType value)
        {
            Stream.Unsafer.Write(TmphPub.TmphEnumCast<TValueType, int>.ToInt(value));
        }

        /// <summary>
        ///     枚举值序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举值序列化</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void enumUIntMember<TValueType>(TValueType value)
        {
            Stream.Unsafer.Write(TmphPub.TmphEnumCast<TValueType, uint>.ToInt(value));
        }

        /// <summary>
        ///     枚举值序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举值序列化</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void enumLongMember<TValueType>(TValueType value)
        {
            Stream.Unsafer.Write(TmphPub.TmphEnumCast<TValueType, long>.ToInt(value));
        }

        /// <summary>
        ///     枚举值序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举值序列化</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void enumULongMember<TValueType>(TValueType value)
        {
            Stream.Unsafer.Write(TmphPub.TmphEnumCast<TValueType, ulong>.ToInt(value));
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
            ///     当前写入位置
            /// </summary>
            public byte* Write;

            /// <summary>
            ///     数组位图
            /// </summary>
            /// <param name="stream">序列化数据流</param>
            /// <param name="arrayLength">数组长度</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            public TmphArrayMap(TmphUnmanagedStream stream, int arrayLength)
            {
                var length = ((arrayLength + (31 + 32)) >> 5) << 2;
                Bit = 1U << 31;
                stream.PrepLength(length);
                Write = stream.CurrentData;
                Map = 0;
                *(int*)Write = arrayLength;
                stream.Unsafer.AddLength(length);
            }

            /// <summary>
            ///     数组位图
            /// </summary>
            /// <param name="stream">序列化数据流</param>
            /// <param name="arrayLength">数组长度</param>
            /// <param name="prepLength">附加长度</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            public TmphArrayMap(TmphUnmanagedStream stream, int arrayLength, int prepLength)
            {
                var length = ((arrayLength + (31 + 32)) >> 5) << 2;
                Bit = 1U << 31;
                stream.PrepLength(length + prepLength);
                Write = stream.CurrentData;
                Map = 0;
                *(int*)Write = arrayLength;
                stream.Unsafer.AddLength(length);
            }

            /// <summary>
            ///     添加数据
            /// </summary>
            /// <param name="value">是否写位图</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            public void Next(bool value)
            {
                if (value) Map |= Bit;
                if (Bit == 1)
                {
                    *(uint*)(Write += sizeof(int)) = Map;
                    Bit = 1U << 31;
                    Map = 0;
                }
                else Bit >>= 1;
            }

            ///// <summary>
            ///// 添加数据
            ///// </summary>
            ///// <param name="value">是否写位图</param>
            //[TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            //public void NextNot(bool value)
            //{
            //    if (!value) Map |= Bit;
            //    if (Bit == 1)
            //    {
            //        *(uint*)(Write += sizeof(int)) = Map;
            //        Bit = 1U << 31;
            //        Map = 0;
            //    }
            //    else Bit >>= 1;
            //}
            /// <summary>
            ///     添加数据
            /// </summary>
            /// <param name="value">是否写位图</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            public void Next(bool? value)
            {
                if (value.HasValue)
                {
                    Map |= Bit;
                    if ((bool)value) Map |= (Bit >> 1);
                }
                if (Bit == 2)
                {
                    *(uint*)(Write += sizeof(int)) = Map;
                    Bit = 1U << 31;
                    Map = 0;
                }
                else Bit >>= 2;
            }

            /// <summary>
            ///     位图写入结束
            /// </summary>
            /// <param name="stream">序列化数据流</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            public void End(TmphUnmanagedStream stream)
            {
                if (Bit != 1U << 31) *(uint*)(Write + sizeof(int)) = Map;
                stream.PrepLength();
            }
        }

        /// <summary>
        ///     配置参数
        /// </summary>
        public class TmphConfig
        {
            /// <summary>
            ///     序列化头部数据
            /// </summary>
            internal const uint HeaderMapValue = 0x51031000U;

            /// <summary>
            ///     序列化头部数据
            /// </summary>
            internal const uint HeaderMapAndValue = 0xffffff00U;

            /// <summary>
            ///     是否序列化成员位图
            /// </summary>
            internal const int MemberMapValue = 1;

            /// <summary>
            ///     成员位图类型不匹配是否输出错误信息
            /// </summary>
            public bool IsMemberMapErrorLog = true;

            /// <summary>
            ///     成员位图
            /// </summary>
            public TmphMemberMap MemberMap;

            /// <summary>
            ///     序列化头部数据
            /// </summary>
            internal virtual int HeaderValue
            {
                get
                {
                    var value = (int)HeaderMapValue;
                    if (MemberMap != null) value += MemberMapValue;
                    return value;
                }
            }
        }

        /// <summary>
        ///     字段信息
        /// </summary>
        internal class TmphFieldInfo
        {
            /// <summary>
            ///     固定类型字节数
            /// </summary>
            private static readonly Dictionary<Type, byte> fixedSizes;

            /// <summary>
            ///     字段信息
            /// </summary>
            public FieldInfo Field;

            /// <summary>
            ///     固定分组排序字节数
            /// </summary>
            internal byte FixedSize;

            /// <summary>
            ///     成员索引
            /// </summary>
            public int MemberIndex;

            static TmphFieldInfo()
            {
                fixedSizes = TmphDictionary.CreateOnly<Type, byte>();
                fixedSizes.Add(typeof(bool), sizeof(bool));
                fixedSizes.Add(typeof(byte), sizeof(byte));
                fixedSizes.Add(typeof(sbyte), sizeof(sbyte));
                fixedSizes.Add(typeof(short), sizeof(short));
                fixedSizes.Add(typeof(ushort), sizeof(ushort));
                fixedSizes.Add(typeof(int), sizeof(int));
                fixedSizes.Add(typeof(uint), sizeof(uint));
                fixedSizes.Add(typeof(long), sizeof(long));
                fixedSizes.Add(typeof(ulong), sizeof(ulong));
                fixedSizes.Add(typeof(char), sizeof(char));
                fixedSizes.Add(typeof(DateTime), sizeof(long));
                fixedSizes.Add(typeof(float), sizeof(float));
                fixedSizes.Add(typeof(double), sizeof(double));
                fixedSizes.Add(typeof(decimal), sizeof(decimal));
                fixedSizes.Add(typeof(Guid), (byte)sizeof(Guid));
                fixedSizes.Add(typeof(bool?), sizeof(byte));
                fixedSizes.Add(typeof(byte?), sizeof(ushort));
                fixedSizes.Add(typeof(sbyte?), sizeof(ushort));
                fixedSizes.Add(typeof(short?), sizeof(uint));
                fixedSizes.Add(typeof(ushort?), sizeof(uint));
                fixedSizes.Add(typeof(int?), sizeof(int) + sizeof(int));
                fixedSizes.Add(typeof(uint?), sizeof(uint) + sizeof(int));
                fixedSizes.Add(typeof(long?), sizeof(long) + sizeof(int));
                fixedSizes.Add(typeof(ulong?), sizeof(ulong) + sizeof(int));
                fixedSizes.Add(typeof(char?), sizeof(uint));
                fixedSizes.Add(typeof(DateTime?), sizeof(long) + sizeof(int));
                fixedSizes.Add(typeof(float?), sizeof(float) + sizeof(int));
                fixedSizes.Add(typeof(double?), sizeof(double) + sizeof(int));
                fixedSizes.Add(typeof(decimal?), sizeof(decimal) + sizeof(int));
                fixedSizes.Add(typeof(Guid?), (byte)(sizeof(Guid) + sizeof(int)));
            }

            /// <summary>
            ///     字段信息
            /// </summary>
            /// <param name="field"></param>
            internal TmphFieldInfo(TmphFieldIndex field)
            {
                Field = field.Member;
                MemberIndex = field.MemberIndex;
                if (Field.FieldType.IsEnum)
                    fixedSizes.TryGetValue(Field.FieldType.GetEnumUnderlyingType(), out FixedSize);
                else fixedSizes.TryGetValue(Field.FieldType, out FixedSize);
            }

            /// <summary>
            /// </summary>
            /// <param name="left"></param>
            /// <param name="right"></param>
            /// <returns></returns>
            internal static int FixedSizeSort(TmphFieldInfo left, TmphFieldInfo right)
            {
                return (int)(right.FixedSize & (0U - right.FixedSize)) - (int)(left.FixedSize & (0U - left.FixedSize));
            }
        }

        /// <summary>
        ///     字段集合信息
        /// </summary>
        /// <typeparam name="fieldType"></typeparam>
        internal struct TmphFields<fieldType> where fieldType : TmphFieldInfo
        {
            /// <summary>
            ///     非固定序列化字段
            /// </summary>
            public TmphSubArray<fieldType> Fields;

            /// <summary>
            ///     固定序列化字段
            /// </summary>
            public TmphSubArray<fieldType> FixedFields;

            /// <summary>
            ///     固定序列化字段字节数
            /// </summary>
            public int FixedSize;

            /// <summary>
            ///     JSON混合序列化字段
            /// </summary>
            public TmphSubArray<TmphFieldIndex> JsonFields;
        }
    }
}