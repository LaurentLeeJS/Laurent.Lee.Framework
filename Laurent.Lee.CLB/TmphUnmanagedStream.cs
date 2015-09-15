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

using System;

namespace Laurent.Lee.CLB
{
    public abstract unsafe class TmphUnmanagedStreamBase : IDisposable
    {
        public const int DefaultLength = 256;
        public int length;

        protected TmphUnmanagedStreamBase(int length)
        {
            Data = TmphUnmanaged.Get(DataLength = length > 0 ? length : DefaultLength, false).Byte;
            IsUnmanaged = true;
        }

        protected TmphUnmanagedStreamBase(byte* data, int dataLength)
        {
            if (data == null || dataLength <= 0) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            Data = data;
            DataLength = dataLength;
        }

        protected internal TmphUnmanagedStreamBase(TmphUnmanagedStreamBase stream)
        {
            Data = stream.Data;
            DataLength = stream.DataLength;
            length = stream.length;
            IsUnmanaged = stream.IsUnmanaged;
            stream.IsUnmanaged = false;
        }

        public byte* Data { get; private set; }

        public byte* CurrentData
        {
            get { return Data + length; }
        }

        internal int DataLength { get; private set; }

        internal bool IsUnmanaged { get; private set; }

        public virtual void Dispose()
        {
            Close();
        }

        public virtual void Close()
        {
            if (IsUnmanaged)
            {
                TmphUnmanaged.Free(Data);
                IsUnmanaged = false;
            }
            DataLength = length = 0;
            Data = null;
        }

        public virtual void Clear()
        {
            length = 0;
        }

        protected void SetStreamLength(int length)
        {
            if (length < DefaultLength) length = DefaultLength;
            var newData = TmphUnmanaged.Get(length, false).Byte;
            Unsafe.TmphMemory.Copy(Data, newData, this.length);
            if (IsUnmanaged) TmphUnmanaged.Free(Data);
            Data = newData;
            DataLength = length;
            IsUnmanaged = true;
        }

        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void PrepLength(int length)
        {
            var newLength = length + this.length;
            if (newLength > DataLength) SetStreamLength(Math.Max(newLength, DataLength << 1));
        }

        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void setLength(int length)
        {
            if (length > 0)
            {
                if (length > DataLength) SetStreamLength(length);
                this.length = length;
            }
            else if (length == 0) this.length = 0;
            else TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
        }

        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public void Write(char value)
        {
            PrepLength(sizeof(char));
            *(char*)(Data + length) = value;
            length += sizeof(char);
        }

        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        protected void write(TmphUnmanagedStreamBase stream)
        {
            if (stream != null)
            {
                PrepLength(stream.length);
                Unsafe.TmphMemory.Copy(stream.Data, Data + length, stream.length);
                length += stream.length;
            }
        }

        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public void Write(string value)
        {
            if (value != null)
            {
                var length = value.Length << 1;
                PrepLength(length);
                Unsafe.TmphString.Copy(value, Data + this.length);
                this.length += length;
            }
        }

        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public void Write(TmphSubString value)
        {
            if (value.Length != 0)
            {
                var length = value.Length << 1;
                PrepLength(length);
                fixed (char* valueFixed = value.value)
                    Unsafe.TmphMemory.Copy(valueFixed + value.StartIndex, Data + this.length, length);
                this.length += length;
            }
        }

        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public void Write(char* start, int count)
        {
            if (start != null)
            {
                var length = count << 1;
                PrepLength(length);
                TmphMemory.Copy(start, Data + this.length, length);
                this.length += length;
            }
        }

        public void Write(string value, int index, int count)
        {
            var range = new TmphArray.TmphRange(value.Length(), index, count);
            if (range.GetCount == count)
            {
                PrepLength(count <<= 1);
                fixed (char* valueFixed = value)
                {
                    Unsafe.TmphMemory.Copy(valueFixed + index, Data + length, count);
                }
                length += count;
            }
            else if (count != 0) TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
        }

        public void Write(params string[] values)
        {
            if (values != null)
            {
                var length = 0;
                foreach (var value in values)
                {
                    if (value != null) length += value.Length;
                }
                PrepLength(length <<= 1);
                var write = Data + this.length;
                foreach (var value in values)
                {
                    if (value != null)
                    {
                        Unsafe.TmphString.Copy(value, write);
                        write += value.Length << 1;
                    }
                }
                this.length += length;
            }
        }

        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public override string ToString()
        {
            return new string((char*)Data, 0, length >> 1);
        }

        internal virtual void Reset(byte* data, int length)
        {
            if (IsUnmanaged)
            {
                TmphUnmanaged.Free(Data);
                IsUnmanaged = false;
            }
            Data = data;
            DataLength = length;
            this.length = 0;
        }

        internal virtual void From(TmphUnmanagedStreamBase stream)
        {
            IsUnmanaged = stream.IsUnmanaged;
            Data = stream.Data;
            DataLength = stream.DataLength;
            length = stream.length;
            stream.IsUnmanaged = false;
        }

        internal TmphCharStream ToCharStream()
        {
            if ((length & 1) != 0) TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
            DataLength &= (int.MaxValue - 1);
            return new TmphCharStream(this);
        }
    }

    /// <summary>
    ///     内存数据流(请自行确保数据可靠性)
    /// </summary>
    public struct TmphUnsafer
    {
        /// <summary>
        ///     内存数据流
        /// </summary>
        public TmphUnmanagedStream Stream;

        /// <summary>
        ///     增加数据流长度
        /// </summary>
        /// <param name="length">增加长度</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public void AddSerializeLength(int length)
        {
            Stream.length += length + (-length & 3);
            //if (Stream.length > Stream.DataLength) log.Error.ThrowReal(Stream.length.toString() + " > " + Stream.DataLength.toString(), true, false);
        }

        /// <summary>
        ///     增加数据流长度
        /// </summary>
        /// <param name="length">增加长度</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public unsafe void AddLength(int length)
        {
            Stream.length += length;
            //if (Stream.length > Stream.DataLength) log.Error.ThrowReal(Stream.length.toString() + " > " + Stream.DataLength.toString(), true, false);
        }

        /// <summary>
        ///     设置数据流长度
        /// </summary>
        /// <param name="length">数据流长度</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public unsafe void SetLength(int length)
        {
            Stream.length = length;
            //if (Stream.length > Stream.DataLength) log.Error.ThrowReal(Stream.length.toString() + " > " + Stream.DataLength.toString(), true, false);
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="value">数据</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public unsafe void Write(bool value)
        {
            Stream.Data[Stream.length++] = (byte)(value ? 1 : 0);
            //if (Stream.length > Stream.DataLength) log.Error.ThrowReal(Stream.length.toString() + " > " + Stream.DataLength.toString(), true, false);
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="value">数据</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public unsafe void Write(byte value)
        {
            Stream.Data[Stream.length++] = value;
            //if (Stream.length > Stream.DataLength) log.Error.ThrowReal(Stream.length.toString() + " > " + Stream.DataLength.toString(), true, false);
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="value">数据</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public unsafe void Write(sbyte value)
        {
            Stream.Data[Stream.length++] = (byte)value;
            //if (Stream.length > Stream.DataLength) log.Error.ThrowReal(Stream.length.toString() + " > " + Stream.DataLength.toString(), true, false);
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="value">数据</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public unsafe void Write(short value)
        {
            *(short*)Stream.CurrentData = value;
            Stream.length += sizeof(short);
            //if (Stream.length > Stream.DataLength) log.Error.ThrowReal(Stream.length.toString() + " > " + Stream.DataLength.toString(), true, false);
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="value">数据</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public unsafe void Write(ushort value)
        {
            *(ushort*)Stream.CurrentData = value;
            Stream.length += sizeof(ushort);
            //if (Stream.length > Stream.DataLength) log.Error.ThrowReal(Stream.length.toString() + " > " + Stream.DataLength.toString(), true, false);
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="value">数据</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public unsafe void Write(char value)
        {
            *(char*)Stream.CurrentData = value;
            Stream.length += sizeof(char);
            //if (Stream.length > Stream.DataLength) log.Error.ThrowReal(Stream.length.toString() + " > " + Stream.DataLength.toString(), true, false);
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="value">数据</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public unsafe void Write(int value)
        {
            Stream.UnsafeWrite(value);
            //if (Stream.length > Stream.DataLength) log.Error.ThrowReal(Stream.length.toString() + " > " + Stream.DataLength.toString(), true, false);
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="value">数据</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public unsafe void Write(uint value)
        {
            *(uint*)Stream.CurrentData = value;
            Stream.length += sizeof(uint);
            //if (Stream.length > Stream.DataLength) log.Error.ThrowReal(Stream.length.toString() + " > " + Stream.DataLength.toString(), true, false);
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="value">数据</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public unsafe void Write(long value)
        {
            *(long*)Stream.CurrentData = value;
            Stream.length += sizeof(long);
            //if (Stream.length > Stream.DataLength) log.Error.ThrowReal(Stream.length.toString() + " > " + Stream.DataLength.toString(), true, false);
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="value">数据</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public unsafe void Write(ulong value)
        {
            *(ulong*)Stream.CurrentData = value;
            Stream.length += sizeof(ulong);
            //if (Stream.length > Stream.DataLength) log.Error.ThrowReal(Stream.length.toString() + " > " + Stream.DataLength.toString(), true, false);
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="value">数据</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public unsafe void Write(DateTime value)
        {
            *(long*)Stream.CurrentData = value.Ticks;
            Stream.length += sizeof(long);
            //if (Stream.length > Stream.DataLength) log.Error.ThrowReal(Stream.length.toString() + " > " + Stream.DataLength.toString(), true, false);
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="value">数据</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public unsafe void Write(float value)
        {
            *(float*)Stream.CurrentData = value;
            Stream.length += sizeof(float);
            //if (Stream.length > Stream.DataLength) log.Error.ThrowReal(Stream.length.toString() + " > " + Stream.DataLength.toString(), true, false);
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="value">数据</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public unsafe void Write(double value)
        {
            *(double*)Stream.CurrentData = value;
            Stream.length += sizeof(double);
            //if (Stream.length > Stream.DataLength) log.Error.ThrowReal(Stream.length.toString() + " > " + Stream.DataLength.toString(), true, false);
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="value">数据</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public unsafe void Write(decimal value)
        {
            *(decimal*)Stream.CurrentData = value;
            Stream.length += sizeof(decimal);
            //if (Stream.length > Stream.DataLength) log.Error.ThrowReal(Stream.length.toString() + " > " + Stream.DataLength.toString(), true, false);
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="value">数据</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public unsafe void Write(Guid value)
        {
            *(Guid*)Stream.CurrentData = value;
            Stream.length += sizeof(Guid);
            //if (Stream.length > Stream.DataLength) log.Error.ThrowReal(Stream.length.toString() + " > " + Stream.DataLength.toString(), true, false);
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="data">数据,不能为null</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public unsafe void Write(byte[] data)
        {
            CLB.Unsafe.TmphMemory.Copy(data, Stream.CurrentData, data.Length);
            Stream.length += data.Length;
            //if (Stream.length > Stream.DataLength) log.Error.ThrowReal(Stream.length.toString() + " > " + Stream.DataLength.toString(), true, false);
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="stream">数据,不能为null</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public unsafe void Write(TmphUnmanagedStream stream)
        {
            CLB.Unsafe.TmphMemory.Copy(stream.Data, Stream.CurrentData, stream.length);
            Stream.length += stream.length;
            //if (Stream.length > Stream.DataLength) log.Error.ThrowReal(Stream.length.toString() + " > " + Stream.DataLength.toString(), true, false);
        }
    }

    public unsafe class TmphUnmanagedStream : TmphUnmanagedStreamBase
    {
        protected int Offset;

        public TmphUnmanagedStream(int length = DefaultLength) : base(length)
        {
        }

        public TmphUnmanagedStream(byte* data, int dataLength) : base(data, dataLength)
        {
        }

        internal TmphUnmanagedStream(TmphUnmanagedStreamBase stream) : base(stream)
        {
        }

        public TmphUnsafer Unsafer
        {
            get { return new TmphUnsafer { Stream = this }; }
        }

        public int OffsetLength
        {
            get { return Offset + length; }
        }

        public int Length
        {
            get { return length; }
        }

        public virtual void PrepLength(int length)
        {
            PrepLength(length);
        }

        public virtual void PrepLength()
        {
        }

        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public void SetLength(int length)
        {
            setLength(length);
        }

        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public void Write(bool value)
        {
            if (length == DataLength) SetStreamLength(length << 1);
            Data[length++] = (byte)(value ? 1 : 0);
        }

        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public void Write(byte value)
        {
            if (length == DataLength) SetStreamLength(length << 1);
            Data[length++] = value;
        }

        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public void Write(sbyte value)
        {
            if (length == DataLength) SetStreamLength(length << 1);
            Data[length++] = (byte)value;
        }

        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public void Write(short value)
        {
            PrepLength(sizeof(short));
            *(short*)(Data + length) = value;
            length += sizeof(short);
        }

        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public void Write(ushort value)
        {
            PrepLength(sizeof(ushort));
            *(ushort*)(Data + length) = value;
            length += sizeof(ushort);
        }

        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public void Write(int value)
        {
            PrepLength(sizeof(int));
            *(int*)(Data + length) = value;
            length += sizeof(int);
        }

        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal void UnsafeWrite(int value)
        {
            *(int*)(Data + length) = value;
            length += sizeof(int);
        }

        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public void Write(uint value)
        {
            PrepLength(sizeof(uint));
            *(uint*)(Data + length) = value;
            length += sizeof(uint);
        }

        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public void Write(long value)
        {
            PrepLength(sizeof(long));
            *(long*)(Data + length) = value;
            length += sizeof(long);
        }

        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public void Write(ulong value)
        {
            PrepLength(sizeof(ulong));
            *(ulong*)(Data + length) = value;
            length += sizeof(ulong);
        }

        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public void Write(DateTime value)
        {
            PrepLength(sizeof(long));
            *(long*)(Data + length) = value.Ticks;
            length += sizeof(long);
        }

        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public void Write(float value)
        {
            PrepLength(sizeof(float));
            *(float*)(Data + length) = value;
            length += sizeof(float);
        }

        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public void Write(double value)
        {
            PrepLength(sizeof(double));
            *(double*)(Data + length) = value;
            length += sizeof(double);
        }

        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public void Write(decimal value)
        {
            PrepLength(sizeof(decimal));
            *(decimal*)(Data + length) = value;
            length += sizeof(decimal);
        }

        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public void Write(Guid value)
        {
            PrepLength(sizeof(Guid));
            *(Guid*)(Data + length) = value;
            length += sizeof(Guid);
        }

        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public void Write(byte[] data)
        {
            if (data != null)
            {
                PrepLength(data.Length);
                Unsafe.TmphMemory.Copy(data, Data + length, data.Length);
                length += data.Length;
            }
        }

        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public void Write(TmphUnmanagedStream stream)
        {
            write(stream);
        }

        public void Write(byte[] data, int index, int count)
        {
            var range = new TmphArray.TmphRange(data.length(), index, count);
            if (range.GetCount == count)
            {
                PrepLength(count);
                fixed (byte* dataFixed = data)
                {
                    Unsafe.TmphMemory.Copy(dataFixed + range.SkipCount, Data + length, count);
                }
                length += count;
            }
            else if (count != 0) TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
        }

        public void Write(TmphSubArray<byte> data)
        {
            var count = data.Count;
            if (count != 0)
            {
                PrepLength(count);
                fixed (byte* dataFixed = data.Array)
                {
                    Unsafe.TmphMemory.Copy(dataFixed + data.StartIndex, Data + length, count);
                }
                length += count;
            }
        }

        public void Write(byte* value, int length)
        {
            if (value != null)
            {
                PrepLength(length);
                Unsafe.TmphMemory.Copy(value, Data + this.length, length);
                this.length += length;
            }
        }

        public byte[] GetArray()
        {
            if (length == 0) return TmphNullValue<byte>.Array;
            var data = new byte[length];
            Unsafe.TmphMemory.Copy(Data, data, length);
            return data;
        }

        internal byte[] GetArray(int copyIndex)
        {
            var data = new byte[length];
            fixed (byte* dataFixed = data)
                Unsafe.TmphMemory.Copy(Data + copyIndex, dataFixed + copyIndex, length - copyIndex);
            return data;
        }

        public byte[] GetSizeArray(int minSize)
        {
            var data = new byte[length < minSize ? minSize : length];
            Unsafe.TmphMemory.Copy(Data, data, length);
            return data;
        }

        internal byte[] GetSizeArray(int copyIndex, int minSize)
        {
            var data = new byte[length < minSize ? minSize : length];
            fixed (byte* dataFixed = data)
                Unsafe.TmphMemory.Copy(Data + copyIndex, dataFixed + copyIndex, length - copyIndex);
            return data;
        }

        internal override void Reset(byte* data, int length)
        {
            base.Reset(data, length);
            Offset = 0;
        }

        internal override void From(TmphUnmanagedStreamBase stream)
        {
            base.From(stream);
            Offset = 0;
        }
    }
}