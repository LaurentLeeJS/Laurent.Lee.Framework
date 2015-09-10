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

using Laurent.Lee.CLB.Config;
using Laurent.Lee.CLB.IO;
using Laurent.Lee.CLB.IO.Compression;
using Laurent.Lee.CLB.Threading;
using System;
using System.IO;
using System.Threading;

namespace Laurent.Lee.CLB.MemoryDataBase
{
    /// <summary>
    ///     内存数据库物理层
    /// </summary>
    internal sealed class TmphPhysical : IDisposable
    {
        /// <summary>
        ///     数据错误
        /// </summary>
        private static readonly Exception dataException = new Exception("数据错误");

        /// <summary>
        ///     文件名
        /// </summary>
        private readonly string fileName;

        /// <summary>
        ///     文件路径
        /// </summary>
        private readonly string path;

        /// <summary>
        ///     缓冲区
        /// </summary>
        private byte[] TmphBuffer;

        /// <summary>
        ///     当前缓冲区索引位置
        /// </summary>
        internal int BufferIndex;

        /// <summary>
        ///     当前操作访问锁
        /// </summary>
        private int currentLock;

        /// <summary>
        ///     文件流写入器
        /// </summary>
        private TmphFileStreamWriter fileWriter;

        /// <summary>
        ///     是否已经释放资源
        /// </summary>
        private int isDisposed;

        /// <summary>
        ///     数据加载缓冲区
        /// </summary>
        private byte[] loadBuffer;

        /// <summary>
        ///     数据加载文件读取器
        /// </summary>
        private TmphFileReader loader;

        /// <summary>
        ///     内存池
        /// </summary>
        private TmphMemoryPool memoryPool;

        /// <summary>
        ///     内存数据库物理层
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="isDomainUnloadDispose"></param>
        public TmphPhysical(string fileName, bool isDomainUnloadDispose = true)
        {
            try
            {
                var file = new FileInfo(this.fileName = fileName + ".fmd");
                path = file.Directory.fullName();
                if (file.Exists)
                {
                    currentLock = 1;
                    loader = new TmphFileReader(this);
                }
                else
                    fileWriter = new TmphFileStreamWriter(path + this.fileName, FileMode.CreateNew, FileShare.Read,
                        FileOptions.None, true, null);
                if (isDomainUnloadDispose) TmphDomainUnload.Add(Dispose);
            }
            catch (Exception error)
            {
                LastException = error;
                currentLock = 2;
                Dispose();
            }
        }

        /// <summary>
        ///     数据加载文件读取器
        /// </summary>
        internal bool IsLoader
        {
            get { return loader != null; }
        }

        /// <summary>
        ///     最后产生的异常错误
        /// </summary>
        internal Exception LastException { get; private set; }

        /// <summary>
        ///     是否已经释放资源
        /// </summary>
        internal bool IsDisposed
        {
            get { return isDisposed != 0; }
        }

        /// <summary>
        ///     释放资源
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.Increment(ref isDisposed) == 1)
            {
                TmphDomainUnload.Remove(Dispose, false);
                if (currentLock != 2)
                {
                    while (Interlocked.CompareExchange(ref currentLock, 2, 0) == 1) Thread.Sleep(0);
                }
                TmphPub.Dispose(ref loader);
                if (fileWriter != null)
                {
                    try
                    {
                        flush();
                        fileWriter.Flush(true);
                    }
                    catch (Exception error)
                    {
                        LastException = error;
                    }
                    finally
                    {
                        TmphPub.Dispose(ref fileWriter);
                    }
                }
                if (memoryPool != null)
                {
                    memoryPool.Push(ref TmphBuffer);
                    memoryPool.Push(ref loadBuffer);
                }
                if (LastException != null) TmphLog.Error.Add(LastException, fileName, false);
            }
        }

        /// <summary>
        ///     设置缓冲区
        /// </summary>
        /// <param name="bufferSize"></param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void setLoadBuffer(int bufferSize)
        {
            setBuffer(bufferSize);
            loadBuffer = memoryPool.Get();
        }

        /// <summary>
        ///     设置缓冲区
        /// </summary>
        /// <param name="TmphBuffer"></param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void setLoadBuffer(byte[] TmphBuffer)
        {
            setLoadBuffer();
            loadBuffer = TmphBuffer;
        }

        /// <summary>
        ///     设置缓冲区
        /// </summary>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void setLoadBuffer()
        {
            memoryPool.Push(ref loadBuffer);
        }

        /// <summary>
        ///     获取缓冲区
        /// </summary>
        /// <returns></returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private byte[] getLoadBuffer()
        {
            if (loadBuffer == null) return loadBuffer = memoryPool.Get();
            return loadBuffer;
        }

        /// <summary>
        ///     设置缓冲区
        /// </summary>
        /// <param name="bufferSize"></param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void setBuffer(int bufferSize)
        {
            TmphBuffer = (memoryPool = TmphMemoryPool.GetPool(bufferSize)).Get();
            BufferIndex = sizeof(int);
        }

        /// <summary>
        ///     数据加载错误
        /// </summary>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void loadError()
        {
            LastException = dataException;
            Dispose();
        }

        /// <summary>
        ///     数据错误
        /// </summary>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void dataError()
        {
            if (Interlocked.CompareExchange(ref currentLock, 1, 0) == 0)
            {
                LastException = dataException;
                currentLock = 2;
                TmphTask.Tiny.Add(Dispose);
            }
        }

        /// <summary>
        ///     检测当前操作
        /// </summary>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void checkCurrent()
        {
            if (LastException == null) currentLock = 0;
            else
            {
                currentLock = 2;
                TmphTask.Tiny.Add(Dispose);
            }
        }

        /// <summary>
        ///     读取数据
        /// </summary>
        /// <returns></returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public TmphSubArray<byte> LoadHeader()
        {
            return loader.ReadHeader();
        }

        /// <summary>
        ///     读取数据
        /// </summary>
        /// <returns></returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public TmphSubArray<byte> Load()
        {
            return loader.Read();
        }

        /// <summary>
        ///     读取数据完毕
        /// </summary>
        /// <param name="isLoaded">是否成功</param>
        /// <returns></returns>
        public bool Loaded(bool isLoaded)
        {
            if (currentLock == 1)
            {
                if (isLoaded)
                {
                    try
                    {
                        TmphPub.Dispose(ref loader);
                        memoryPool.Push(ref loadBuffer);
                        fileWriter = new TmphFileStreamWriter(path + fileName, FileMode.Open, FileShare.Read,
                            FileOptions.None, true, null);
                        return true;
                    }
                    catch (Exception error)
                    {
                        LastException = error;
                    }
                    finally
                    {
                        currentLock = 0;
                    }
                }
                else currentLock = 0;
            }
            Dispose();
            return false;
        }

        /// <summary>
        ///     创建文件头
        /// </summary>
        /// <param name="data"></param>
        /// <returns>是否创建成功</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public unsafe bool Create(TmphSubArray<byte> data)
        {
            fixed (byte* dataFixed = data.array) return Create(dataFixed + data.StartIndex, data.Count);
        }

        /// <summary>
        ///     创建文件头
        /// </summary>
        /// <param name="data"></param>
        /// <param name="size"></param>
        /// <returns>是否创建成功</returns>
        public unsafe bool Create(byte* data, int size)
        {
            if (size >= sizeof(int) * 3 &&
                ((*(int*)data ^ Emit.TmphPub.PuzzleValue) | (*(int*)(data + sizeof(int)) ^ size) | (size & 3)) == 0)
            {
                var bufferSize = *(int*)(data + sizeof(int) * 2);
                if ((bufferSize & (bufferSize - 1)) == 0 && bufferSize >= TmphMemoryDatabase.MinPhysicalBufferSize &&
                    bufferSize >= size)
                {
                    if (Interlocked.CompareExchange(ref currentLock, 1, 0) == 0)
                    {
                        try
                        {
                            if (fileWriter.UnsafeWrite(data, size) >= 0) setBuffer(bufferSize);
                            else LastException = fileWriter.LastException;
                        }
                        catch (Exception error)
                        {
                            LastException = error;
                        }
                        finally
                        {
                            checkCurrent();
                        }
                        return LastException == null;
                    }
                    return false;
                }
            }
            dataError();
            return false;
        }

        /// <summary>
        ///     添加数据
        /// </summary>
        /// <param name="data"></param>
        /// <param name="size"></param>
        /// <returns>成功状态，2表示成功需要等待缓存写入</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public unsafe int Append(TmphSubArray<byte> data)
        {
            fixed (byte* dataFixed = data.array) return Append(dataFixed + data.StartIndex, data.Count);
        }

        /// <summary>
        ///     添加数据
        /// </summary>
        /// <param name="data"></param>
        /// <param name="size"></param>
        private unsafe void append(byte* data, int size)
        {
            fixed (byte* bufferFixed = TmphBuffer)
            {
                do
                {
                    var start = data;
                    var count = TmphBuffer.Length - BufferIndex;
                    while (count >= *(int*)data)
                    {
                        size -= *(int*)data;
                        count -= *(int*)data;
                        data += *(int*)data;
                        if (size == 0)
                        {
                            Unsafe.TmphMemory.Copy(start, bufferFixed + BufferIndex, count = (int)(data - start));
                            BufferIndex += count;
                            return;
                        }
                        if (*(uint*)data > (uint)size || (*(int*)data & 3) != 0)
                        {
                            dataError();
                            return;
                        }
                    }
                    Unsafe.TmphMemory.Copy(start, bufferFixed + BufferIndex, count = (int)(data - start));
                    if ((BufferIndex += count) != sizeof(int))
                    {
                        flush(bufferFixed);
                        if (LastException != null) return;
                    }
                    while (*(int*)data >= TmphBuffer.Length)
                    {
                        var currentBuffer = new byte[(count = *(int*)data) + sizeof(int)];
                        fixed (byte* currentBufferFixed = currentBuffer)
                        {
                            Unsafe.TmphMemory.Copy(data, currentBufferFixed + sizeof(int), count);
                            size -= count;
                            data += count;
                            var compressData = TmphStream.Deflate.GetCompressUnsafe(currentBuffer, sizeof(int), count,
                                sizeof(int), memoryPool);
                            if (compressData.array == null)
                            {
                                *(int*)currentBufferFixed = count + sizeof(int);
                                if (fileWriter.UnsafeWrite(currentBuffer) < 0)
                                {
                                    LastException = fileWriter.LastException;
                                    return;
                                }
                            }
                            else
                            {
                                fixed (byte* dataFixed = compressData.array)
                                {
                                    count = compressData.Count + sizeof(int);
                                    Unsafe.TmphMemory.Copy(dataFixed + sizeof(int), currentBufferFixed + sizeof(int),
                                        compressData.Count);
                                    memoryPool.Push(ref compressData.array);
                                    *(int*)currentBufferFixed = -count;
                                    if (fileWriter.UnsafeWrite(currentBuffer, 0, count + (-count & 3)) < 0)
                                    {
                                        LastException = fileWriter.LastException;
                                        return;
                                    }
                                }
                            }
                        }
                        if (size == 0) return;
                        if (*(uint*)data > (uint)size || (*(int*)data & 3) != 0)
                        {
                            dataError();
                            return;
                        }
                    }
                } while (true);
            }
        }

        /// <summary>
        ///     添加数据
        /// </summary>
        /// <param name="data"></param>
        /// <param name="size"></param>
        /// <returns>成功状态，2表示成功需要等待缓存写入</returns>
        public unsafe int Append(byte* data, int size)
        {
            if (size >= sizeof(int) && *(uint*)data <= (uint)size && ((*(int*)data | size) & 3) == 0)
            {
                if (Interlocked.CompareExchange(ref currentLock, 1, 0) == 0)
                {
                    try
                    {
                        append(data, size);
                    }
                    catch (Exception error)
                    {
                        LastException = error;
                    }
                    finally
                    {
                        checkCurrent();
                    }
                    if (LastException == null)
                    {
                        var file = fileWriter;
                        if (file != null && file.IsWaitBuffer) return 2;
                        return 1;
                    }
                }
            }
            else dataError();
            return 0;
        }

        /// <summary>
        ///     本地数据库获取缓冲区
        /// </summary>
        /// <returns></returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal byte[] LocalBuffer()
        {
            if (currentLock == 0)
            {
                if (TmphBuffer.Length - BufferIndex < sizeof(int) * 3) Flush();
                if (LastException == null) return TmphBuffer;
            }
            return null;
        }

        /// <summary>
        ///     刷新缓存区
        /// </summary>
        /// <param name="bufferFixed"></param>
        private unsafe void flush(byte* bufferFixed)
        {
            var compressData = TmphStream.Deflate.GetCompressUnsafe(TmphBuffer, sizeof(int), BufferIndex - sizeof(int),
                sizeof(int), memoryPool);
            if (compressData.array == null) fileWriter.UnsafeWrite(bufferFixed, *(int*)bufferFixed = BufferIndex);
            else
            {
                fixed (byte* dataFixed = compressData.array)
                {
                    var count = compressData.Count + sizeof(int);
                    Unsafe.TmphMemory.Copy(dataFixed + sizeof(int), bufferFixed + sizeof(int), compressData.Count);
                    *(int*)dataFixed = -count;
                    fileWriter.UnsafeWrite(dataFixed, count + (-count & 3));
                }
                memoryPool.Push(ref compressData.array);
            }
            LastException = fileWriter.LastException;
            BufferIndex = sizeof(int);
        }

        /// <summary>
        ///     刷新缓存区
        /// </summary>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private unsafe void flush()
        {
            if (BufferIndex != sizeof(int))
            {
                fixed (byte* bufferFixed = TmphBuffer) flush(bufferFixed);
            }
        }

        /// <summary>
        ///     等待缓存写入
        /// </summary>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public void WaitBuffer()
        {
            fileWriter.WaitWriteBuffer();
        }

        /// <summary>
        ///     刷新缓存区
        /// </summary>
        /// <returns>是否操作成功</returns>
        public bool Flush()
        {
            if (Interlocked.CompareExchange(ref currentLock, 1, 0) == 0)
            {
                try
                {
                    flush();
                }
                catch (Exception error)
                {
                    LastException = error;
                }
                finally
                {
                    checkCurrent();
                }
                return LastException == null;
            }
            return false;
        }

        /// <summary>
        ///     刷新写入文件缓存区
        /// </summary>
        /// <param name="isWriteFile">是否写入文件</param>
        /// <returns>是否操作成功</returns>
        public bool FlushFile(bool isWriteFile)
        {
            try
            {
                fileWriter.Flush(true);
                return true;
            }
            catch (Exception error)
            {
                LastException = error;
                var value = Interlocked.CompareExchange(ref currentLock, 2, 0);
                while (value == 1)
                {
                    Thread.Sleep(0);
                    value = Interlocked.CompareExchange(ref currentLock, 2, 0);
                }
                if (value == 0) Dispose();
            }
            return false;
        }

        /// <summary>
        ///     文件读取器
        /// </summary>
        internal sealed class TmphFileReader : IDisposable
        {
            /// <summary>
            ///     客户端读取数据等待事件
            /// </summary>
            private readonly EventWaitHandle clientHandle;

            /// <summary>
            ///     服务器端读取文件等待事件
            /// </summary>
            private readonly EventWaitHandle waitHandle;

            /// <summary>
            ///     缓存区访问锁
            /// </summary>
            private int bufferLock;

            /// <summary>
            ///     读取数据结果
            /// </summary>
            private TmphSubArray<byte> data;

            /// <summary>
            ///     文件流
            /// </summary>
            private FileStream fileStream;

            /// <summary>
            ///     是否释放资源
            /// </summary>
            private int isDisposed;

            /// <summary>
            ///     是否等待数据缓冲区
            /// </summary>
            private int isWaitBuffer;

            /// <summary>
            ///     内存数据库物理层
            /// </summary>
            private TmphPhysical physical;

            /// <summary>
            ///     文件长度
            /// </summary>
            private long size;

            /// <summary>
            ///     文件读取器
            /// </summary>
            /// <param name="fileName">文件名称</param>
            public TmphFileReader(TmphPhysical physical)
            {
                this.physical = physical;
                fileStream = new FileStream(physical.path + physical.fileName, FileMode.Open, FileAccess.Read,
                    FileShare.ReadWrite, TmphAppSetting.StreamBufferSize, FileOptions.SequentialScan);
                size = fileStream.Length;
                waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
                clientHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
                if ((size & 3) == 0 && size >= sizeof(int) * 3) TmphThreadPool.TinyPool.FastStart(read, null, null);
                else
                {
                    Dispose();
                    physical.dataError();
                }
            }

            /// <summary>
            ///     释放资源
            /// </summary>
            public void Dispose()
            {
                if (Interlocked.CompareExchange(ref isDisposed, 1, 0) == 0)
                {
                    waitHandle.Set();
                    clientHandle.Set();
                    waitHandle.Close();
                    clientHandle.Close();
                    TmphPub.Dispose(ref fileStream);
                }
            }

            /// <summary>
            ///     读取数据
            /// </summary>
            /// <returns>读取的数据</returns>
            public TmphSubArray<byte> ReadHeader()
            {
                if (isDisposed == 0) waitHandle.WaitOne();
                return data;
            }

            /// <summary>
            ///     读取数据
            /// </summary>
            /// <returns>读取的数据</returns>
            public TmphSubArray<byte> Read()
            {
                if (isDisposed == 0)
                {
                    data.Null();
                    TmphInterlocked.NoCheckCompareSetSleep0(ref bufferLock);
                    if (isWaitBuffer == 0) bufferLock = 0;
                    else
                    {
                        isWaitBuffer = 0;
                        bufferLock = 0;
                        clientHandle.Set();
                    }
                    waitHandle.WaitOne();
                }
                return data;
            }

            /// <summary>
            ///     等待缓存区
            /// </summary>
            private void waitBuffer()
            {
                if (data.array != null)
                {
                    Thread.Sleep(0);
                    TmphInterlocked.NoCheckCompareSetSleep0(ref bufferLock);
                    if (data.array == null) bufferLock = 0;
                    else
                    {
                        isWaitBuffer = 1;
                        bufferLock = 0;
                        clientHandle.WaitOne();
                    }
                }
            }

            /// <summary>
            ///     读取文件线程
            /// </summary>
            private unsafe void read()
            {
                int startIndex = 0, nextCount = sizeof(int) * 3;
                try
                {
                    var headerBuffer = new byte[sizeof(int) * 3];
                    if (fileStream.Read(headerBuffer, 0, sizeof(int) * 3) != sizeof(int) * 3) return;
                    fixed (byte* bufferFixed = headerBuffer)
                    {
                        if (*(int*)bufferFixed != Emit.TmphPub.PuzzleValue) return;
                        var headerSize = *(int*)(bufferFixed + sizeof(int));
                        if (headerSize > size || headerSize < sizeof(int) * 3 || (headerSize & 3) != 0) return;
                        var bufferSize = *(int*)(bufferFixed + sizeof(int) * 2);
                        if ((bufferSize & (bufferSize - 1)) != 0 || (bufferSize >> 12) == 0 || headerSize > bufferSize)
                            return;
                        size -= headerSize;
                        physical.setLoadBuffer(bufferSize);
                        if (fileStream.Read(physical.loadBuffer, sizeof(int), headerSize -= sizeof(int) * 3) !=
                            headerSize)
                            return;
                        fixed (byte* loaderBufferFixed = physical.loadBuffer) *(int*)loaderBufferFixed = bufferSize;
                        data.UnsafeSet(physical.loadBuffer, 0, headerSize + sizeof(int));
                        waitHandle.Set();
                        nextCount = 0;
                    }
                    byte[] TmphBuffer = physical.TmphBuffer, bigBuffer = TmphNullValue<byte>.Array;
                    var isBigBuffer = 0;
                    fixed (byte* bufferFixed = TmphBuffer)
                    {
                        while (isDisposed == 0 && (size | (uint)nextCount) != 0)
                        {
                            if (nextCount == 0)
                            {
                                if (
                                    fileStream.Read(TmphBuffer, startIndex = 0,
                                        nextCount = (int)Math.Min(TmphBuffer.Length, size)) != nextCount)
                                    return;
                                size -= nextCount;
                            }
                            var dataStart = bufferFixed + startIndex;
                            int dataSize = *(int*)dataStart, bufferSize;
                            if (dataSize < 0)
                            {
                                bufferSize = dataSize & 3;
                                bufferSize += (dataSize = -dataSize);
                            }
                            else
                            {
                                if ((dataSize & 3) != 0) return;
                                bufferSize = dataSize;
                            }
                            if (bufferSize > TmphBuffer.Length)
                            {
                                var count = bufferSize - nextCount;
                                if ((size -= count) < 0) return;
                                if (bufferSize > bigBuffer.Length)
                                {
                                    var bigSize = (uint)(bigBuffer.Length == 0 ? TmphBuffer.Length : bigBuffer.Length);
                                    while (bigSize < bufferSize) bigSize <<= 1;
                                    if (bigSize == 0x80000000U) return;
                                    bigBuffer = new byte[bigSize];
                                    isBigBuffer = 0;
                                }
                                if (isBigBuffer == 0)
                                {
                                    Buffer.BlockCopy(TmphBuffer, startIndex, bigBuffer, 0, nextCount);
                                    if (fileStream.Read(bigBuffer, nextCount, count) != count) return;
                                    if (*(int*)dataStart < 0)
                                    {
                                        var newBuffer = TmphStream.Deflate.GetDeCompressUnsafe(bigBuffer, sizeof(int),
                                            dataSize - sizeof(int), physical.memoryPool);
                                        waitBuffer();
                                        physical.setLoadBuffer(newBuffer.array);
                                        data = newBuffer;
                                    }
                                    else
                                    {
                                        waitBuffer();
                                        physical.setLoadBuffer();
                                        data.UnsafeSet(bigBuffer, sizeof(int), dataSize - sizeof(int));
                                        isBigBuffer = 1;
                                    }
                                }
                                else
                                {
                                    waitBuffer();
                                    Buffer.BlockCopy(TmphBuffer, startIndex, bigBuffer, 0, nextCount);
                                    if (fileStream.Read(bigBuffer, nextCount, count) != count) return;
                                    if (*(int*)dataStart < 0)
                                    {
                                        var newBuffer = TmphStream.Deflate.GetDeCompressUnsafe(bigBuffer, sizeof(int),
                                            dataSize - sizeof(int), physical.memoryPool);
                                        physical.setLoadBuffer(newBuffer.array);
                                        data = newBuffer;
                                        isBigBuffer = 0;
                                    }
                                    else
                                    {
                                        physical.setLoadBuffer();
                                        data.UnsafeSet(bigBuffer, sizeof(int), dataSize - sizeof(int));
                                    }
                                }
                                waitHandle.Set();
                                nextCount = 0;
                            }
                            else
                            {
                                var count = bufferSize - nextCount;
                                if (count > 0)
                                {
                                    if (size < count) return;
                                    Buffer.BlockCopy(TmphBuffer, startIndex, TmphBuffer, 0, nextCount);
                                    if (
                                        fileStream.Read(TmphBuffer, nextCount,
                                            count = (int)Math.Min(TmphBuffer.Length - nextCount, size)) != count)
                                        return;
                                    dataStart = bufferFixed;
                                    startIndex = 0;
                                    size -= count;
                                    nextCount += count;
                                }
                                if (*(int*)dataStart < 0)
                                {
                                    var newBuffer = TmphStream.Deflate.GetDeCompressUnsafe(TmphBuffer,
                                        startIndex + sizeof(int), bufferSize - sizeof(int), physical.memoryPool);
                                    waitBuffer();
                                    physical.setLoadBuffer(newBuffer.array);
                                    data = newBuffer;
                                    isBigBuffer = 0;
                                }
                                else
                                {
                                    waitBuffer();
                                    byte[] newBuffer;
                                    if (isBigBuffer == 0)
                                    {
                                        if (bigBuffer.Length == 0) newBuffer = physical.getLoadBuffer();
                                        else
                                        {
                                            newBuffer = bigBuffer;
                                            isBigBuffer = 1;
                                        }
                                    }
                                    else
                                    {
                                        newBuffer = physical.getLoadBuffer();
                                        isBigBuffer = 0;
                                    }
                                    Buffer.BlockCopy(TmphBuffer, startIndex + sizeof(int), newBuffer, 0,
                                        bufferSize - sizeof(int));
                                    data.UnsafeSet(newBuffer, 0, bufferSize - sizeof(int));
                                }
                                waitHandle.Set();
                                nextCount -= bufferSize;
                                startIndex += bufferSize;
                            }
                        }
                    }
                    if (isDisposed == 0)
                    {
                        waitBuffer();
                        data.UnsafeSet(physical.TmphBuffer, 0, 0);
                    }
                }
                finally
                {
                    if (nextCount == 0) Dispose();
                    else
                    {
                        data.Null();
                        Dispose();
                        TmphPub.Dispose(ref physical);
                    }
                }
            }
        }
    }
}