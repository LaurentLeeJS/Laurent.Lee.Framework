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

namespace Laurent.Lee.CLB
{
    public sealed unsafe class TmphCharStream : TmphUnmanagedStream
    {
        public TmphCharStream(int length = DefaultLength) : base(length << 1)
        {
        }

        public TmphCharStream(char* data, int length) : base((byte*)data, length << 1)
        {
        }

        public TmphCharStream(TmphUnmanagedStreamBase stream) : base(stream)
        {
        }

        public TmphUnsafer Unsafer
        {
            get { return new TmphUnsafer { Stream = this }; }
        }

        public char* Char
        {
            get { return (char*)Data; }
        }

        public char* CurrentChar
        {
            get { return (char*)CurrentData; }
        }

        public int Length
        {
            get { return length >> 1; }
        }

        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public void PrepLength(int addLength)
        {
            PrepLength(addLength << 1);
        }

        /// <summary>
        ///     重置当前数据长度
        /// </summary>
        /// <param name="dataLength">当前数据长度</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public void SetLength(int dataLength)
        {
            setLength(dataLength << 1);
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="stream">数据</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public void Write(TmphCharStream stream)
        {
            write(stream);
        }

        /// <summary>
        ///     写字符串
        /// </summary>
        /// <param name="value">字符串</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal void WriteNotNull(string value)
        {
            var length = value.Length << 1;
            PrepLength(length);
            CLB.Unsafe.TmphString.Copy(value, Data + this.length);
            this.length += length;
        }

        /// <summary>
        ///     内存字符流(请自行确保数据可靠性)
        /// </summary>
        public struct TmphUnsafer
        {
            /// <summary>
            ///     内存字符流
            /// </summary>
            public TmphCharStream Stream;

            /// <summary>
            ///     增加数据流长度
            /// </summary>
            /// <param name="length">增加长度</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            public void AddLength(int length)
            {
                Stream.length += length << 1;
            }

            /// <summary>
            ///     设置数据流长度
            /// </summary>
            /// <param name="length">数据流长度</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            public void SetLength(int length)
            {
                Stream.length = length << 1;
            }

            /// <summary>
            ///     写数据
            /// </summary>
            /// <param name="value">数据</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            public void Write(char value)
            {
                *(char*)Stream.CurrentData = value;
                Stream.length += sizeof(char);
            }

            /// <summary>
            ///     写字符串
            /// </summary>
            /// <param name="start">字符串起始位置,不能为null</param>
            /// <param name="count">写入字符数，必须>=0</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            public void Write(char* start, int count)
            {
                TmphMemory.Copy(start, Stream.CurrentData, count <<= 1);
                Stream.length += count;
            }

            /// <summary>
            ///     写数据
            /// </summary>
            /// <param name="charStream">数据,不能为null</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            public void Write(TmphCharStream charStream)
            {
                Unsafe.TmphMemory.Copy(charStream.CurrentData, Stream.CurrentData, charStream.length);
                Stream.length += charStream.length;
            }
        }
    }
}