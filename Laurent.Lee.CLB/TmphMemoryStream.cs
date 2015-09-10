using System;
using System.IO;
using System.Text;

namespace Laurent.Lee.CLB
{
    /// <summary>
    ///     内存数据流
    /// </summary>
    public sealed class TmphMemoryStream : IDisposable
    {
        /// <summary>
        ///     默认容器初始尺寸
        /// </summary>
        public const int DefaultLength = 256;

        /// <summary>
        ///     内存流扩展设置
        /// </summary>
        private static readonly Action<MemoryStream, bool> memoryStreamExpandable =
            Emit.TmphPub.SetField<MemoryStream, bool>("_expandable");

        /// <summary>
        ///     数据
        /// </summary>
        private byte[] array;

        /// <summary>
        ///     内存数据流
        /// </summary>
        /// <param name="length">容器初始尺寸</param>
        public TmphMemoryStream(int length = DefaultLength)
        {
            array = new byte[length > 0 ? length : DefaultLength];
        }

        /// <summary>
        ///     非安全访问内存数据流
        /// </summary>
        /// <returns>非安全访问内存数据流</returns>
        public TmphUnsafer Unsafer
        {
            get { return new TmphUnsafer { Stream = this }; }
        }

        /// <summary>
        ///     数据
        /// </summary>
        public byte[] Array
        {
            get { return array; }
            internal set { array = value; }
        }

        /// <summary>
        ///     当前数据长度
        /// </summary>
        public int Length { get; private set; }

        /// <summary>
        ///     释放数据容器
        /// </summary>
        public void Dispose()
        {
            Close();
        }

        /// <summary>
        ///     隐式转换为MemoryStream
        /// </summary>
        /// <param name="stream">内存数据流</param>
        /// <returns>MemoryStream</returns>
        public static implicit operator MemoryStream(TmphMemoryStream stream)
        {
            if (stream.array.length() == 0) return new MemoryStream();
            var memoryStream = Get(stream.array);
            memoryStream.Seek(stream.Length, SeekOrigin.Begin);
            return memoryStream;
        }

        /// <summary>
        ///     释放数据容器
        /// </summary>
        public void Close()
        {
            array = null;
        }

        /// <summary>
        ///     清空数据
        /// </summary>
        public void Clear()
        {
            Length = 0;
        }

        /// <summary>
        ///     设置容器尺寸
        /// </summary>
        /// <param name="length">容器尺寸</param>
        private void setArrayLength(int length)
        {
            var data = new byte[length];
            Buffer.BlockCopy(array, 0, data, 0, Length);
            array = data;
        }

        /// <summary>
        ///     设置容器尺寸
        /// </summary>
        /// <param name="length">容器尺寸</param>
        public void SetArrayLength(int length)
        {
            if ((length += Length) > array.Length) setArrayLength(length);
        }

        /// <summary>
        ///     预增数据流长度
        /// </summary>
        /// <param name="length">增加长度</param>
        /// <returns>是否需要增加容器尺寸</returns>
        public bool PrepLength(int length)
        {
            var newLength = length + Length;
            if (newLength > array.Length)
            {
                setArrayLength(length > Length ? newLength : (Length << 1));
                return true;
            }
            return false;
        }

        /// <summary>
        ///     预增数据流长度
        /// </summary>
        /// <param name="length">增加长度</param>
        private void prepLength(int length)
        {
            var newLength = length + Length;
            if (newLength > array.Length) setArrayLength(length > Length ? newLength : (Length << 1));
        }

        /// <summary>
        ///     检测是否需要预增数据容器尺寸
        /// </summary>
        /// <param name="length">增加数据流长度</param>
        /// <returns>是否需要预增数据容器尺寸</returns>
        public bool Check(int length)
        {
            return Length + length <= array.Length;
        }

        /// <summary>
        ///     重置当前数据长度
        /// </summary>
        /// <param name="length">当前数据长度</param>
        public void SetLength(int length)
        {
            if (length > 0)
            {
                if (length > array.Length) setArrayLength(length);
                Length = length;
            }
            else if (length == 0) Length = 0;
            else TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="value">数据</param>
        public void Write(bool value)
        {
            if (Length == array.Length) setArrayLength(Length << 1);
            array[Length++] = (byte)(value ? 1 : 0);
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="value">数据</param>
        public void Write(byte value)
        {
            if (Length == array.Length) setArrayLength(Length << 1);
            array[Length++] = value;
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="value">数据</param>
        public void Write(sbyte value)
        {
            if (Length == array.Length) setArrayLength(Length << 1);
            array[Length++] = (byte)value;
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="value">数据</param>
        public void Write(short value)
        {
            if (Length == array.Length) setArrayLength(Length << 1);
            array[Length++] = (byte)value;
            if (Length == array.Length) setArrayLength(Length << 1);
            array[Length++] = (byte)((ushort)value >> 8);
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="value">数据</param>
        public void Write(ushort value)
        {
            if (Length == array.Length) setArrayLength(Length << 1);
            array[Length++] = (byte)value;
            if (Length == array.Length) setArrayLength(Length << 1);
            array[Length++] = (byte)(value >> 8);
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="value">数据</param>
        public void Write(char value)
        {
            if (Length == array.Length) setArrayLength(Length << 1);
            array[Length++] = (byte)value;
            if (Length == array.Length) setArrayLength(Length << 1);
            array[Length++] = (byte)(value >> 8);
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="value">数据</param>
        public unsafe void Write(int value)
        {
            prepLength(sizeof(int));
            fixed (byte* dataFixed = array) *(int*)(dataFixed + Length) = value;
            Length += sizeof(int);
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="value">数据</param>
        public unsafe void Write(uint value)
        {
            prepLength(sizeof(uint));
            fixed (byte* dataFixed = array) *(uint*)(dataFixed + Length) = value;
            Length += sizeof(uint);
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="value">数据</param>
        public unsafe void Write(long value)
        {
            prepLength(sizeof(long));
            fixed (byte* dataFixed = array) *(long*)(dataFixed + Length) = value;
            Length += sizeof(long);
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="value">数据</param>
        public unsafe void Write(ulong value)
        {
            prepLength(sizeof(ulong));
            fixed (byte* dataFixed = array) *(ulong*)(dataFixed + Length) = value;
            Length += sizeof(ulong);
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="value">数据</param>
        public unsafe void Write(DateTime value)
        {
            prepLength(sizeof(long));
            fixed (byte* dataFixed = array) *(long*)(dataFixed + Length) = value.Ticks;
            Length += sizeof(long);
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="value">数据</param>
        public unsafe void Write(float value)
        {
            prepLength(sizeof(float));
            fixed (byte* dataFixed = array) *(float*)(dataFixed + Length) = value;
            Length += sizeof(float);
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="value">数据</param>
        public unsafe void Write(double value)
        {
            prepLength(sizeof(double));
            fixed (byte* dataFixed = array) *(double*)(dataFixed + Length) = value;
            Length += sizeof(double);
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="value">数据</param>
        public unsafe void Write(decimal value)
        {
            prepLength(sizeof(decimal));
            fixed (byte* dataFixed = array) *(decimal*)(dataFixed + Length) = value;
            Length += sizeof(decimal);
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="value">数据</param>
        public unsafe void Write(Guid value)
        {
            prepLength(sizeof(Guid));
            fixed (byte* dataFixed = array) *(Guid*)(dataFixed + Length) = value;
            Length += sizeof(Guid);
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="value">数据</param>
        public void Write(byte[] data)
        {
            if (data != null)
            {
                prepLength(data.Length);
                Buffer.BlockCopy(data, 0, array, Length, data.Length);
                Length += data.Length;
            }
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="stream">数据</param>
        public void Write(TmphMemoryStream stream)
        {
            if (stream != null)
            {
                prepLength(stream.Length);
                Buffer.BlockCopy(stream.array, 0, array, Length, stream.Length);
                Length += stream.Length;
            }
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="index">起始位置</param>
        /// <param name="count">写入字节数</param>
        public void Write(byte[] data, int index, int count)
        {
            var range = new TmphArray.TmphRange(data.length(), index, count);
            if (range.GetCount == count)
            {
                prepLength(count);
                Buffer.BlockCopy(data, range.SkipCount, array, Length, count);
                Length += count;
            }
            else if (count != 0) TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="data">数据</param>
        public void Write(TmphSubArray<byte> data)
        {
            var count = data.Count;
            if (count != 0)
            {
                prepLength(count);
                Buffer.BlockCopy(data.Array, data.StartIndex, array, Length, count);
                Length += count;
            }
        }

        /// <summary>
        ///     写字符串
        /// </summary>
        /// <param name="value">字符串</param>
        public unsafe void Write(string value)
        {
            if (value != null)
            {
                var length = value.Length << 1;
                prepLength(length);
                fixed (byte* dataFixed = array) CLB.Unsafe.TmphString.Copy(value, dataFixed + Length);
                Length += length;
            }
        }

        /// <summary>
        ///     写字符串
        /// </summary>
        /// <param name="start">字符串起始位置</param>
        /// <param name="count">写入字符数</param>
        public unsafe void Write(char* start, int count)
        {
            if (start != null)
            {
                var length = count << 1;
                prepLength(length);
                fixed (byte* dataFixed = array) TmphMemory.Copy(start, dataFixed + Length, length);
                Length += length;
            }
        }

        /// <summary>
        ///     写字符串
        /// </summary>
        /// <param name="value">字符串</param>
        /// <param name="index">起始位置</param>
        /// <param name="count">写入字符数</param>
        public unsafe void Write(string value, int index, int count)
        {
            var range = new TmphArray.TmphRange(value.Length(), index, count);
            if (range.GetCount == count)
            {
                prepLength(count <<= 1);
                fixed (byte* dataFixed = array)
                fixed (char* valueFixed = value)
                {
                    CLB.Unsafe.TmphMemory.Copy(valueFixed + index, dataFixed + Length, count);
                }
                Length += count;
            }
            else if (count != 0) TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
        }

        /// <summary>
        ///     写字符串集合
        /// </summary>
        /// <param name="values">字符串集合</param>
        public unsafe void Write(params string[] values)
        {
            if (values != null)
            {
                var length = 0;
                foreach (var value in values)
                {
                    if (value != null) length += value.Length;
                }
                prepLength(length <<= 1);
                fixed (byte* dataFixed = array)
                {
                    var write = dataFixed + Length;
                    foreach (var value in values)
                    {
                        if (value != null)
                        {
                            CLB.Unsafe.TmphString.Copy(value, write);
                            write += value.Length << 1;
                        }
                    }
                }
                Length += length;
            }
        }

        /// <summary>
        ///     转换成字节数组
        /// </summary>
        /// <returns>字节数组</returns>
        public byte[] GetArray()
        {
            if (Length == 0) return TmphNullValue<byte>.Array;
            var data = new byte[Length];
            Buffer.BlockCopy(array, 0, data, 0, Length);
            return data;
        }

        ///// <summary>
        ///// 转换成字节数组
        ///// </summary>
        ///// <param name="index">起始位置</param>
        ///// <param name="count">字节数</param>
        ///// <returns>字节数组</returns>
        //public byte[] GetArray(int index, int count)
        //{
        //    array.range range = new array.range(Length, index, count);
        //    if (count == range.GetCount)
        //    {
        //        byte[] data = new byte[count];
        //        TmphBuffer.BlockCopy(data, index, data, 0, count);
        //        return data;
        //    }
        //    else if (count == 0) return null;
        //    log.Default.Throw(log.exceptionType.IndexOutOfRange);
        //    return null;
        //}
        /// <summary>
        ///     转换成字节数组
        /// </summary>
        /// <returns>字节数组</returns>
        public byte[] ToArray()
        {
            return Length != array.Length ? GetArray() : array;
        }

        /// <summary>
        ///     转换成字符串
        /// </summary>
        /// <returns>字符串</returns>
        public override unsafe string ToString()
        {
            fixed (byte* dataFixed = array) return new string((char*)dataFixed, 0, Length >> 1);
        }

        /// <summary>
        ///     转换成字符串
        /// </summary>
        /// <param name="encoding">编码</param>
        /// <returns>字符串</returns>
        public string ToString(Encoding encoding)
        {
            return encoding.GetString(array, 0, Length);
        }

        /// <summary>
        ///     内存流转换
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns>内存流</returns>
        internal static MemoryStream Get(byte[] data)
        {
            var memoryStream = new MemoryStream(data, 0, data.Length, true, true);
            memoryStreamExpandable(memoryStream, true);
            return memoryStream;
        }

        /// <summary>
        ///     内存数据流(请自行确保数据可靠性)
        /// </summary>
        public struct TmphUnsafer
        {
            /// <summary>
            ///     内存数据流
            /// </summary>
            public TmphMemoryStream Stream;

            /// <summary>
            ///     增加数据流长度
            /// </summary>
            /// <param name="length">增加长度</param>
            public void AddLength(int length)
            {
                Stream.Length += length;
            }

            /// <summary>
            ///     设置数据流长度
            /// </summary>
            /// <param name="length">数据流长度</param>
            public void SetLength(int length)
            {
                Stream.Length = length;
            }

            /// <summary>
            ///     写数据
            /// </summary>
            /// <param name="value">数据</param>
            public void Write(bool value)
            {
                Stream.array[Stream.Length++] = (byte)(value ? 1 : 0);
            }

            /// <summary>
            ///     写数据
            /// </summary>
            /// <param name="value">数据</param>
            public void Write(byte value)
            {
                Stream.array[Stream.Length++] = value;
            }

            /// <summary>
            ///     写数据
            /// </summary>
            /// <param name="value">数据</param>
            public void Write(sbyte value)
            {
                Stream.array[Stream.Length++] = (byte)value;
            }

            /// <summary>
            ///     写数据
            /// </summary>
            /// <param name="value">数据</param>
            public void Write(short value)
            {
                Stream.array[Stream.Length++] = (byte)value;
                Stream.array[Stream.Length++] = (byte)((ushort)value >> 8);
            }

            /// <summary>
            ///     写数据
            /// </summary>
            /// <param name="value">数据</param>
            public void Write(ushort value)
            {
                Stream.array[Stream.Length++] = (byte)value;
                Stream.array[Stream.Length++] = (byte)(value >> 8);
            }

            /// <summary>
            ///     写数据
            /// </summary>
            /// <param name="value">数据</param>
            public void Write(char value)
            {
                Stream.array[Stream.Length++] = (byte)value;
                Stream.array[Stream.Length++] = (byte)(value >> 8);
            }

            /// <summary>
            ///     写数据
            /// </summary>
            /// <param name="value">数据</param>
            public unsafe void Write(int value)
            {
                fixed (byte* dataFixed = Stream.array) *(int*)(dataFixed + Stream.Length) = value;
                Stream.Length += sizeof(int);
            }

            /// <summary>
            ///     写数据
            /// </summary>
            /// <param name="value">数据</param>
            public unsafe void Write(uint value)
            {
                fixed (byte* dataFixed = Stream.array) *(uint*)(dataFixed + Stream.Length) = value;
                Stream.Length += sizeof(uint);
            }

            /// <summary>
            ///     写数据
            /// </summary>
            /// <param name="value">数据</param>
            public unsafe void Write(long value)
            {
                fixed (byte* dataFixed = Stream.array) *(long*)(dataFixed + Stream.Length) = value;
                Stream.Length += sizeof(long);
            }

            /// <summary>
            ///     写数据
            /// </summary>
            /// <param name="value">数据</param>
            public unsafe void Write(ulong value)
            {
                fixed (byte* dataFixed = Stream.array) *(ulong*)(dataFixed + Stream.Length) = value;
                Stream.Length += sizeof(ulong);
            }

            /// <summary>
            ///     写数据
            /// </summary>
            /// <param name="value">数据</param>
            public unsafe void Write(DateTime value)
            {
                fixed (byte* dataFixed = Stream.array) *(long*)(dataFixed + Stream.Length) = value.Ticks;
                Stream.Length += sizeof(long);
            }

            /// <summary>
            ///     写数据
            /// </summary>
            /// <param name="value">数据</param>
            public unsafe void Write(float value)
            {
                fixed (byte* dataFixed = Stream.array) *(float*)(dataFixed + Stream.Length) = value;
                Stream.Length += sizeof(float);
            }

            /// <summary>
            ///     写数据
            /// </summary>
            /// <param name="value">数据</param>
            public unsafe void Write(double value)
            {
                fixed (byte* dataFixed = Stream.array) *(double*)(dataFixed + Stream.Length) = value;
                Stream.Length += sizeof(double);
            }

            /// <summary>
            ///     写数据
            /// </summary>
            /// <param name="value">数据</param>
            public unsafe void Write(decimal value)
            {
                fixed (byte* dataFixed = Stream.array) *(decimal*)(dataFixed + Stream.Length) = value;
                Stream.Length += sizeof(decimal);
            }

            /// <summary>
            ///     写数据
            /// </summary>
            /// <param name="value">数据</param>
            public unsafe void Write(Guid value)
            {
                fixed (byte* dataFixed = Stream.array) *(Guid*)(dataFixed + Stream.Length) = value;
                Stream.Length += sizeof(Guid);
            }

            /// <summary>
            ///     写数据
            /// </summary>
            /// <param name="value">数据,不能为null</param>
            public void Write(byte[] data)
            {
                Buffer.BlockCopy(data, 0, Stream.array, Stream.Length, data.Length);
                Stream.Length += data.Length;
            }

            /// <summary>
            ///     写数据
            /// </summary>
            /// <param name="stream">数据,不能为null</param>
            public void Write(TmphMemoryStream stream)
            {
                Buffer.BlockCopy(stream.array, 0, Stream.array, Stream.Length, stream.Length);
                Stream.Length += stream.Length;
            }

            /// <summary>
            ///     预增数据流长度并序列化字符串(4字节对齐)
            /// </summary>
            /// <param name="value">字符串,不能为null</param>
            public unsafe void PrepSerialize(string value)
            {
                var length = sizeof(int) + (((value.Length + 1) & (int.MaxValue - 1)) << 1);
                Stream.PrepLength(length);
                fixed (byte* dataFixed = Stream.array)
                {
                    var write = dataFixed + Stream.Length;
                    if (value.Length != 0)
                    {
                        fixed (char* valueFixed = value)
                        {
                            var isAscii = true;
                            for (char* start = valueFixed, end = valueFixed + value.Length; start != end; ++start)
                            {
                                if ((*start & 0xff00) != 0)
                                {
                                    isAscii = false;
                                    break;
                                }
                            }
                            var stringLength = value.Length << 1;
                            if (isAscii)
                            {
                                *(int*)write = stringLength + 1;
                                write += sizeof(int);
                                for (char* start = valueFixed, end = valueFixed + value.Length;
                                    start != end;
                                    *write++ = (byte)*start++)
                                    ;
                                Stream.Length += sizeof(int) + ((value.Length + 3) & (int.MaxValue - 3));
                            }
                            else
                            {
                                *(int*)write = stringLength;
                                CLB.Unsafe.TmphMemory.Copy(valueFixed, write += sizeof(int), stringLength);
                                Stream.Length += length;
                            }
                        }
                    }
                    else
                    {
                        *(int*)write = 0;
                        Stream.Length += sizeof(int);
                    }
                }
            }

            /// <summary>
            ///     预增数据流长度并写入长度与数据(4字节对齐)
            /// </summary>
            /// <param name="data">数据,不能为null</param>
            public unsafe void PrepSerialize(byte[] data)
            {
                var length = sizeof(int) + ((data.Length + 3) & (int.MaxValue - 3));
                Stream.PrepLength(length);
                fixed (byte* dataFixed = Stream.array) *(int*)(dataFixed + Stream.Length) = data.Length;
                Buffer.BlockCopy(data, 0, Stream.array, Stream.Length + sizeof(int), data.Length);
                Stream.Length += length;
            }

            /// <summary>
            ///     预增数据流长度并写入长度与数据(4字节对齐)
            /// </summary>
            /// <param name="data">数据,不能为null</param>
            public unsafe void PrepSerialize(bool[] array)
            {
                int mapLength = ((array.Length + 31) >> 5) << 2, prepLength = sizeof(int) + mapLength;
                Stream.PrepLength(prepLength);
                fixed (byte* dataFixed = Stream.array)
                {
                    var write = dataFixed + Stream.Length;
                    *(int*)write = array.Length;
                    var valueMap = new TmphFixedMap(write += sizeof(int));
                    mapLength = 0;
                    foreach (var value in array)
                    {
                        if (value) valueMap.Set(mapLength);
                        ++mapLength;
                    }
                }
                Stream.Length += prepLength;
            }

            /// <summary>
            ///     预增数据流长度并写入长度与数据(4字节对齐)
            /// </summary>
            /// <param name="data">数据,不能为null</param>
            public unsafe void PrepSerialize(bool?[] array)
            {
                var mapLength = ((array.Length + 31) >> 5) << 2;
                var prepLength = sizeof(int) + mapLength + mapLength;
                Stream.PrepLength(prepLength);
                fixed (byte* dataFixed = Stream.array)
                {
                    var write = dataFixed + Stream.Length;
                    *(int*)write = array.Length;
                    var nullMap = new TmphFixedMap(write += sizeof(int));
                    var valueMap = new TmphFixedMap(write += mapLength);
                    mapLength = 0;
                    foreach (var value in array)
                    {
                        if (value == null) nullMap.Set(mapLength);
                        else if ((bool)value) valueMap.Set(mapLength);
                        ++mapLength;
                    }
                }
                Stream.Length += prepLength;
            }

            /// <summary>
            ///     预增数据流长度并写入长度与数据(4字节对齐)
            /// </summary>
            /// <param name="data">数据,不能为null</param>
            public unsafe void PrepSerialize(DateTime[] data)
            {
                fixed (DateTime* dataFixed = data) prepSerialize(dataFixed, data.Length, sizeof(DateTime));
            }

            /// <summary>
            ///     预增数据流长度并写入长度与数据(4字节对齐)
            /// </summary>
            /// <param name="data">数据,不能为null</param>
            public unsafe void PrepSerialize(decimal[] data)
            {
                fixed (decimal* dataFixed = data) prepSerialize(dataFixed, data.Length, sizeof(decimal));
            }

            /// <summary>
            ///     预增数据流长度并写入长度与数据(4字节对齐)
            /// </summary>
            /// <param name="data">数据,不能为null</param>
            public unsafe void PrepSerialize(Guid[] data)
            {
                fixed (Guid* dataFixed = data) prepSerialize(dataFixed, data.Length, sizeof(Guid));
            }

            /// <summary>
            ///     预增数据流长度并写入长度与数据(4字节对齐)
            /// </summary>
            /// <param name="data">数据</param>
            /// <param name="count">数据数量</param>
            /// <param name="size">数据字节数</param>
            private unsafe void prepSerialize(void* data, int count, int size)
            {
                int dataSize = count * size, length = sizeof(int) + ((dataSize + 3) & (int.MaxValue - 3));
                Stream.PrepLength(length);
                fixed (byte* dataFixed = Stream.array)
                {
                    var write = dataFixed + Stream.Length;
                    *(int*)write = count;
                    CLB.Unsafe.TmphMemory.Copy(data, write + sizeof(int), dataSize);
                }
                Stream.Length += length;
            }

            /// <summary>
            ///     预增数据流长度并写入长度与数据(4字节对齐)
            /// </summary>
            /// <typeparam name="TValueType">数据类型</typeparam>
            /// <param name="data">数据,不能为null</param>
            /// <param name="size">单个数据字节数</param>
            public unsafe void PrepSerialize<TValueType>(TValueType[] data, int size)
            {
                int dataSize = data.Length * size, length = sizeof(int) + ((dataSize + 3) & (int.MaxValue - 3));
                Stream.PrepLength(length);
                fixed (byte* dataFixed = Stream.array) *(int*)(dataFixed + Stream.Length) = data.Length;
                Buffer.BlockCopy(data, 0, Stream.array, Stream.Length + sizeof(int), dataSize);
                Stream.Length += length;
            }
        }
    }
}