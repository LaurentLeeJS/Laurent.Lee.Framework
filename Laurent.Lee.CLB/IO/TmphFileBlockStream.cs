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

using Laurent.Lee.CLB.Code.CSharp;
using Laurent.Lee.CLB.Emit;
using Laurent.Lee.CLB.Threading;
using System;
using System.IO;

namespace Laurent.Lee.CLB.IO
{
    /// <summary>
    ///     文件分块写入流
    /// </summary>
    public sealed class TmphFileBlockStream : TmphFileStreamWriter
    {
        /// <summary>
        ///     等待缓存写入
        /// </summary>
        private readonly Action<TmphReader> waitHandle;

        /// <summary>
        ///     文件读取
        /// </summary>
        private TmphReader currentReader;

        /// <summary>
        ///     文件读取流
        /// </summary>
        private FileStream fileReader;

        /// <summary>
        ///     文件读取访问锁
        /// </summary>
        private int readerLock;

        /// <summary>
        ///     文件分块写入流
        /// </summary>
        /// <param name="fileName">文件全名</param>
        /// <param name="fileOption">附加选项</param>
        public TmphFileBlockStream(string fileName, FileOptions fileOption = FileOptions.None)
            : base(fileName, File.Exists(fileName) ? FileMode.Open : FileMode.CreateNew, FileShare.Read, fileOption)
        {
            fileReader = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.Read, bufferLength,
                fileOption);
            waitHandle = wait;
        }

        /// <summary>
        ///     设置文件读取
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private bool set(TmphReader reader)
        {
            TmphInterlocked.NoCheckCompareSetSleep0(ref readerLock);
            if (currentReader == null)
            {
                currentReader = reader;
                readerLock = 0;
                reader.FileStream = this;
                return true;
            }
            currentReader.Next = reader;
            readerLock = 0;
            return false;
        }

        /// <summary>
        ///     读取下一个文件数据
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private TmphReader next(TmphReader reader)
        {
            TmphInterlocked.NoCheckCompareSetSleep0(ref readerLock);
            var nextReader = reader.Next;
            if (nextReader == null) currentReader = null;
            else if ((reader.Next = nextReader.Next) == null) currentReader = reader;
            readerLock = 0;
            return nextReader;
        }

        /// <summary>
        ///     读取文件分块数据//showjim+cache
        /// </summary>
        /// <param name="index">文件分块数据位置</param>
        /// <param name="size">文件分块字节大小</param>
        /// <param name="onReaded"></param>
        internal unsafe void Read(TmphIndex index,
            Func<TmphAsynchronousMethod.TmphReturnValue<TmphTcpBase.TmphSubByteArrayEvent>, bool> onReaded)
        {
            if (onReaded == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            var endIndex = index.EndIndex;
            if (index.Size > 0 && ((int)index.Index & 3) == 0 && endIndex <= fileBufferLength)
            {
                if (endIndex <= fileLength)
                {
                    var reader = TmphReader.Get(index, onReaded);
                    if (reader != null)
                    {
                        if (set(reader)) TmphThreadPool.TinyPool.FastStart(reader.ReadHandle, null, null);
                        return;
                    }
                }
                else
                {
                    TmphMemoryPool memoryPool = null;
                    byte[] TmphBuffer = null;
                    var copyedSize = int.MinValue;
                    TmphInterlocked.NoCheckCompareSetSleep0(ref bufferLock);
                    if (isDisposed == 0)
                    {
                        if (index.Index >= bufferIndex)
                        {
                            index.Index -= bufferIndex;
                            try
                            {
                                TmphBuffer = (memoryPool = TmphMemoryPool.GetPool(index.Size)).Get(index.Size);
                                foreach (var nextData in buffers.array)
                                {
                                    var data = nextData.SubArray;
                                    if (index.Index != 0)
                                    {
                                        if (index.Index >= data.Count)
                                        {
                                            index.Index -= data.Count;
                                            continue;
                                        }
                                        data.UnsafeSet(data.StartIndex + (int)index.Index,
                                            data.Count - (int)index.Index);
                                        index.Index = 0;
                                    }
                                    if (copyedSize < 0)
                                    {
                                        fixed (byte* dataFixed = data.array)
                                        {
                                            if (*(int*)(dataFixed + data.StartIndex) != index.Size) break;
                                        }
                                        if ((copyedSize = data.Count - sizeof(int)) == 0) continue;
                                        data.UnsafeSet(data.StartIndex + sizeof(int), copyedSize);
                                        copyedSize = 0;
                                    }
                                    var copySize = index.Size - copyedSize;
                                    if (data.Count >= copySize)
                                    {
                                        Buffer.BlockCopy(data.array, data.StartIndex, TmphBuffer, copyedSize, copySize);
                                        copyedSize = index.Size;
                                        break;
                                    }
                                    Buffer.BlockCopy(data.array, data.StartIndex, TmphBuffer, copyedSize, copySize);
                                    copyedSize += copySize;
                                }
                            }
                            catch (Exception error)
                            {
                                TmphLog.Default.Add(error, null, false);
                            }
                            finally
                            {
                                bufferLock = 0;
                            }
                            if (copyedSize == index.Size)
                            {
                                onReaded(new TmphTcpBase.TmphSubByteArrayEvent
                                {
                                    TmphBuffer = TmphSubArray<byte>.Unsafe(TmphBuffer, 0, index.Size),
                                    Event = memoryPool.PushSubArray
                                });
                                return;
                            }
                        }
                        else
                        {
                            bufferLock = 0;
                            var reader = TmphReader.Get(index, onReaded);
                            if (reader != null)
                            {
                                TmphThreadPool.TinyPool.FastStart(waitHandle, reader, null, null);
                                return;
                            }
                        }
                    }
                    else bufferLock = 0;
                }
            }
            onReaded(default(TmphTcpBase.TmphSubByteArrayEvent));
        }

        /// <summary>
        ///     等待缓存写入
        /// </summary>
        /// <param name="reader"></param>
        private void wait(TmphReader reader)
        {
            var endIndex = reader.EndIndex;
            if (endIndex <= fileLength)
            {
                if (set(reader)) reader.Read();
                return;
            }
            flush(true);
            if (isDisposed == 0)
            {
                if (set(reader)) reader.Read();
            }
            else reader.Cancel();
        }

        /// <summary>
        ///     释放资源
        /// </summary>
        protected override void dispose()
        {
            TmphPub.Dispose(ref fileReader);
        }

        /// <summary>
        ///     文件索引
        /// </summary>
        [TmphSqlColumn]
        public struct TmphIndex
        {
            /// <summary>
            ///     位置索引
            /// </summary>
            public long Index;

            /// <summary>
            ///     数据大小
            /// </summary>
            public int Size;

            /// <summary>
            ///     文件分块结束位置
            /// </summary>
            public long EndIndex
            {
                get { return Index + (Size + sizeof(int)); }
            }

            /// <summary>
            ///     清空数据
            /// </summary>
            public void Null()
            {
                Index = 0;
                Size = 0;
            }

            /// <summary>
            ///     重置文件索引
            /// </summary>
            /// <param name="index"></param>
            /// <param name="size"></param>
            /// <returns></returns>
            internal int ReSet(long index, int size)
            {
                if (Index == index)
                {
                    if (Size == size) return 1;
                }
                else Index = index;
                Size = size;
                return 0;
            }
        }

        /// <summary>
        ///     文件读取
        /// </summary>
        private sealed class TmphReader
        {
            /// <summary>
            ///     文件数据缓冲区
            /// </summary>
            private byte[] TmphBuffer;

            /// <summary>
            ///     文件分块写入流
            /// </summary>
            public TmphFileBlockStream FileStream;

            /// <summary>
            ///     读取文件位置
            /// </summary>
            private TmphIndex index;

            /// <summary>
            ///     内存池
            /// </summary>
            private TmphMemoryPool memoryPool;

            /// <summary>
            ///     下一个文件读取
            /// </summary>
            public TmphReader Next;

            /// <summary>
            ///     读取文件回调函数
            /// </summary>
            private Func<TmphAsynchronousMethod.TmphReturnValue<TmphTcpBase.TmphSubByteArrayEvent>, bool> onReaded;

            /// <summary>
            ///     文件读取
            /// </summary>
            private TmphReader()
            {
                ReadHandle = Read;
            }

            /// <summary>
            ///     开始读取文件
            /// </summary>
            public Action ReadHandle { get; private set; }

            /// <summary>
            ///     文件分块结束位置
            /// </summary>
            public long EndIndex
            {
                get { return index.EndIndex; }
            }

            /// <summary>
            ///     开始读取文件
            /// </summary>
            public unsafe void Read()
            {
                do
                {
                    var readSize = index.Size + sizeof(int);
                    try
                    {
                        if (FileStream.isDisposed == 0)
                        {
                            TmphBuffer = (memoryPool = TmphMemoryPool.GetPool(readSize)).Get();
                            var fileReader = FileStream.fileReader;
                            var offset = fileReader.Position - index.Index;
                            if (offset >= 0 || -offset < index.Index) fileReader.Seek(offset, SeekOrigin.Current);
                            else fileReader.Seek(index.Index, SeekOrigin.Begin);
                            if (fileReader.Read(TmphBuffer, 0, readSize) == readSize)
                            {
                                fixed (byte* bufferFixed = TmphBuffer)
                                {
                                    if (*(int*)bufferFixed == index.Size) readSize = index.Size;
                                    else
                                        TmphLog.Default.Add(
                                            FileStream.FileName + " index[" + index.Index.toString() + "] size[" +
                                            (*(int*)bufferFixed).toString() + "]<>" + index.Size.toString(), false,
                                            false);
                                }
                            }
                            else readSize = 0;
                        }
                    }
                    catch (Exception error)
                    {
                        TmphLog.Default.Add(error, null, false);
                    }
                    var onReaded = this.onReaded;
                    if (readSize == index.Size)
                    {
                        if (
                            onReaded(new TmphTcpBase.TmphSubByteArrayEvent
                            {
                                TmphBuffer = TmphSubArray<byte>.Unsafe(TmphBuffer, sizeof(int), index.Size),
                                Event = memoryPool.PushSubArray
                            }))
                            TmphBuffer = null;
                        else memoryPool.Push(ref TmphBuffer);
                    }
                    else
                    {
                        onReaded(default(TmphTcpBase.TmphSubByteArrayEvent));
                        if (memoryPool != null) memoryPool.Push(ref TmphBuffer);
                    }
                    var next = FileStream.next(this);
                    if (next == null)
                    {
                        FileStream = null;
                        onReaded = null;
                        memoryPool = null;
                        TmphTypePool<TmphReader>.Push(this);
                        return;
                    }
                    onReaded = next.onReaded;
                    index = next.index;
                    next.onReaded = null;
                    TmphTypePool<TmphReader>.Push(next);
                } while (true);
            }

            /// <summary>
            ///     取消文件读取
            /// </summary>
            public void Cancel()
            {
                var onReaded = this.onReaded;
                this.onReaded = null;
                TmphTypePool<TmphReader>.Push(this);
                onReaded(default(TmphTcpBase.TmphSubByteArrayEvent));
            }

            /// <summary>
            ///     文件读取
            /// </summary>
            /// <param name="index">读取文件位置</param>
            /// <param name="onReaded">读取文件回调函数</param>
            /// <returns></returns>
            public static TmphReader Get(TmphIndex index,
                Func<TmphAsynchronousMethod.TmphReturnValue<TmphTcpBase.TmphSubByteArrayEvent>, bool> onReaded)
            {
                var reader = TmphTypePool<TmphReader>.Pop();
                if (reader == null)
                {
                    try
                    {
                        reader = new TmphReader();
                    }
                    catch
                    {
                        return null;
                    }
                }
                reader.onReaded = onReaded;
                reader.index = index;
                return reader;
            }
        }
    }
}