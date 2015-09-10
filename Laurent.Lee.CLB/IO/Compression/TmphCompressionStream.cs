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

using System;
using System.IO;
using System.IO.Compression;

namespace Laurent.Lee.CLB.IO.Compression
{
    /// <summary>
    ///     压缩流处理
    /// </summary>
    public abstract class TmphStream
    {
        /// <summary>
        ///     GZip压缩流处理
        /// </summary>
        public static readonly TmphStream GZip = new TmphGZipStream();

        /// <summary>
        ///     deflate压缩流处理
        /// </summary>
        public static readonly TmphStream Deflate = new TmphDeflateStream();

        /// <summary>
        ///     获取压缩流
        /// </summary>
        /// <param name="dataStream">原始数据流</param>
        /// <returns>压缩流</returns>
        protected abstract Stream getStream(Stream dataStream);

        /// <summary>
        ///     获取解压缩流
        /// </summary>
        /// <param name="dataStream">压缩数据流</param>
        /// <returns>解压缩流</returns>
        protected abstract Stream getDecompressStream(Stream dataStream);

        /// <summary>
        ///     压缩数据
        /// </summary>
        /// <param name="data">原始数据</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">压缩字节数</param>
        /// <param name="seek">起始位置</param>
        /// <returns>压缩后的数据,失败返回null</returns>
        internal TmphSubArray<byte> GetCompressUnsafe(byte[] data, int startIndex, int count, int seek = 0,
            TmphMemoryPool memoryPool = null)
        {
            var length = count + seek;
            if (memoryPool == null)
            {
                using (var dataStream = new MemoryStream())
                {
                    if (seek != 0) dataStream.Seek(seek, SeekOrigin.Begin);
                    using (var compressStream = getStream(dataStream))
                    {
                        compressStream.Write(data, startIndex, count);
                    }
                    if (dataStream.Position < length)
                    {
                        return TmphSubArray<byte>.Unsafe(dataStream.GetBuffer(), seek, (int)dataStream.Position - seek);
                    }
                }
            }
            else
            {
                var TmphBuffer = memoryPool.Get();
                try
                {
                    using (var dataStream = TmphMemoryStream.Get(TmphBuffer))
                    {
                        if (seek != 0) dataStream.Seek(seek, SeekOrigin.Begin);
                        using (var compressStream = getStream(dataStream))
                        {
                            compressStream.Write(data, startIndex, count);
                        }
                        if (dataStream.Position < length)
                        {
                            var streamBuffer = dataStream.GetBuffer();
                            if (streamBuffer == TmphBuffer) TmphBuffer = null;
                            return TmphSubArray<byte>.Unsafe(streamBuffer, seek, (int)dataStream.Position - seek);
                        }
                    }
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
                finally
                {
                    memoryPool.Push(ref TmphBuffer);
                }
            }
            return default(TmphSubArray<byte>);
        }

        /// <summary>
        ///     压缩数据
        /// </summary>
        /// <param name="data">原始数据</param>
        /// <param name="seek">起始位置</param>
        /// <returns>压缩后的数据,失败返回null</returns>
        public TmphSubArray<byte> GetCompress(byte[] data, int seek = 0, TmphMemoryPool memoryPool = null)
        {
            if (seek < 0) TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
            if (data != null && data.Length != 0)
            {
                return GetCompressUnsafe(data, 0, data.Length, seek, memoryPool);
            }
            return default(TmphSubArray<byte>);
        }

        /// <summary>
        ///     压缩数据
        /// </summary>
        /// <param name="data">原始数据</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">压缩字节数</param>
        /// <param name="seek">起始位置</param>
        /// <returns>压缩后的数据,失败返回null</returns>
        public TmphSubArray<byte> GetCompress(byte[] data, int startIndex, int count, int seek = 0,
            TmphMemoryPool memoryPool = null)
        {
            if (seek >= 0)
            {
                if (count == 0) return TmphSubArray<byte>.Unsafe(TmphNullValue<byte>.Array, 0, 0);
                var range = new TmphArray.TmphRange(data.length(), startIndex, count);
                if (count == range.GetCount) return GetCompressUnsafe(data, startIndex, count, seek, memoryPool);
            }
            TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
            return default(TmphSubArray<byte>);
        }

        /// <summary>
        ///     解压缩数据
        /// </summary>
        /// <param name="compressData">压缩数据</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">解压缩字节数</param>
        /// <returns>解压缩后的数据</returns>
        internal TmphSubArray<byte> GetDeCompressUnsafe(byte[] compressData, int startIndex, int count,
            TmphMemoryPool memoryPool)
        {
            using (Stream memoryStream = new MemoryStream(compressData, startIndex, count))
            using (var compressStream = getDecompressStream(memoryStream))
            {
                return new TmphDeCompressor { CompressStream = compressStream }.Get(memoryPool);
            }
        }

        /// <summary>
        ///     解压缩数据
        /// </summary>
        /// <param name="stream">数据流</param>
        /// <returns>解压缩后的数据</returns>
        public TmphSubArray<byte> GetDeCompress(Stream stream, TmphMemoryPool memoryPool = null)
        {
            if (stream != null)
            {
                using (var compressStream = getDecompressStream(stream))
                {
                    return new TmphDeCompressor { CompressStream = compressStream }.Get(memoryPool);
                }
            }
            return default(TmphSubArray<byte>);
        }

        /// <summary>
        ///     解压缩数据
        /// </summary>
        /// <param name="compressData">压缩数据</param>
        /// <returns>解压缩后的数据</returns>
        public TmphSubArray<byte> GetDeCompress(byte[] compressData, TmphMemoryPool memoryPool = null)
        {
            if (compressData.length() > 0)
            {
                return GetDeCompressUnsafe(compressData, 0, compressData.Length, memoryPool);
            }
            return default(TmphSubArray<byte>);
        }

        /// <summary>
        ///     解压缩数据
        /// </summary>
        /// <param name="compressData">压缩数据</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">解压缩字节数</param>
        /// <returns>解压缩后的数据</returns>
        public TmphSubArray<byte> GetDeCompress(byte[] compressData, int startIndex, int count,
            TmphMemoryPool memoryPool = null)
        {
            if (count > 0)
            {
                var range = new TmphArray.TmphRange(compressData.length(), startIndex, count);
                if (count == range.GetCount)
                    return GetDeCompressUnsafe(compressData, range.SkipCount, count, memoryPool);
            }
            TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
            return default(TmphSubArray<byte>);
        }

        /// <summary>
        ///     解压器
        /// </summary>
        private struct TmphDeCompressor
        {
            /// <summary>
            ///     压缩输出流
            /// </summary>
            public Stream CompressStream;

            /// <summary>
            ///     输出数据流
            /// </summary>
            private TmphUnmanagedStream dataStream;

            /// <summary>
            ///     获取解压数据
            /// </summary>
            /// <returns>解压数据</returns>
            public unsafe TmphSubArray<byte> Get(TmphMemoryPool memoryPool)
            {
                if (memoryPool == null)
                {
                    var data = TmphUnmanagedPool.StreamBuffers.Get();
                    try
                    {
                        using (dataStream = new TmphUnmanagedStream(data.Byte, TmphUnmanagedPool.StreamBuffers.Size))
                        {
                            get();
                            return new TmphSubArray<byte>(dataStream.GetArray());
                        }
                    }
                    finally
                    {
                        TmphUnmanagedPool.StreamBuffers.Push(ref data);
                    }
                }
                else
                {
                    var data = memoryPool.Get();
                    try
                    {
                        fixed (byte* dataFixed = data)
                        {
                            using (dataStream = new TmphUnmanagedStream(dataFixed, data.Length))
                            {
                                get();
                                if (dataStream.Data == dataFixed)
                                {
                                    var TmphBuffer = data;
                                    data = null;
                                    return TmphSubArray<byte>.Unsafe(TmphBuffer, 0, dataStream.Length);
                                }
                                return new TmphSubArray<byte>(dataStream.GetArray());
                            }
                        }
                    }
                    finally
                    {
                        memoryPool.Push(ref data);
                    }
                }
            }

            /// <summary>
            ///     获取解压数据
            /// </summary>
            private unsafe void get()
            {
                var TmphBuffer = TmphMemoryPool.StreamBuffers.Get();
                try
                {
                    var bufferLength = TmphBuffer.Length;
                    fixed (byte* bufferFixed = TmphBuffer)
                    {
                        var length = CompressStream.Read(TmphBuffer, 0, bufferLength);
                        while (length != 0)
                        {
                            dataStream.Write(bufferFixed, length);
                            length = CompressStream.Read(TmphBuffer, 0, bufferLength);
                        }
                    }
                }
                finally
                {
                    TmphMemoryPool.StreamBuffers.Push(ref TmphBuffer);
                }
            }
        }

        /// <summary>
        ///     GZip压缩流处理
        /// </summary>
        private class TmphGZipStream : TmphStream
        {
            /// <summary>
            ///     获取压缩流
            /// </summary>
            /// <param name="dataStream">原始数据流</param>
            /// <returns>压缩流</returns>
            protected override Stream getStream(Stream dataStream)
            {
                return new GZipStream(dataStream, CompressionMode.Compress, true);
            }

            /// <summary>
            ///     获取解压缩流
            /// </summary>
            /// <param name="dataStream">压缩数据流</param>
            /// <returns>解压缩流</returns>
            protected override Stream getDecompressStream(Stream dataStream)
            {
                return new GZipStream(dataStream, CompressionMode.Decompress, false);
            }
        }

        /// <summary>
        ///     deflate压缩流处理
        /// </summary>
        private class TmphDeflateStream : TmphStream
        {
            /// <summary>
            ///     获取压缩流
            /// </summary>
            /// <param name="dataStream">原始数据流</param>
            /// <returns>压缩流</returns>
            protected override Stream getStream(Stream dataStream)
            {
                return new DeflateStream(dataStream, CompressionMode.Compress, true);
            }

            /// <summary>
            ///     获取解压缩流
            /// </summary>
            /// <param name="dataStream">压缩数据流</param>
            /// <returns>解压缩流</returns>
            protected override Stream getDecompressStream(Stream dataStream)
            {
                return new DeflateStream(dataStream, CompressionMode.Decompress, false);
            }
        }
    }
}