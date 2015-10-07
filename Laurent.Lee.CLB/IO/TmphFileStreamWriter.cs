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

using Laurent.Lee.CLB.Config;
using Laurent.Lee.CLB.Threading;
using System;
using System.IO;
using System.Text;
using System.Threading;

namespace Laurent.Lee.CLB.IO
{
    /// <summary>
    ///     文件流写入器
    /// </summary>
    public class TmphFileStreamWriter : IDisposable
    {
        /// <summary>
        ///     最大文件缓存集合字节数
        /// </summary>
        private const int maxBufferSize = 1 << 20;

        /// <summary>
        ///     文件编码
        /// </summary>
        private readonly Encoding encoding;

        /// <summary>
        ///     附加选项
        /// </summary>
        private readonly FileOptions fileOption;

        /// <summary>
        ///     文件共享方式
        /// </summary>
        private readonly FileShare fileShare;

        /// <summary>
        ///     缓存刷新等待事件
        /// </summary>
        private readonly EventWaitHandle flushWait;

        /// <summary>
        ///     是否写日志
        /// </summary>
        private readonly bool isLog;

        /// <summary>
        ///     内存池
        /// </summary>
        private readonly TmphMemoryPool memoryPool;

        /// <summary>
        ///     写入文件
        /// </summary>
        private readonly Action writeFileHandle;

        /// <summary>
        ///     文件写入缓冲区
        /// </summary>
        private byte[] TmphBuffer;

        /// <summary>
        ///     待写入文件缓存集合位置索引
        /// </summary>
        protected long bufferIndex;

        /// <summary>
        ///     文件写入缓冲字节长度
        /// </summary>
        protected int bufferLength;

        /// <summary>
        ///     缓存操作锁
        /// </summary>
        protected int bufferLock;

        /// <summary>
        ///     待写入文件缓存集合
        /// </summary>
        protected TmphList<TmphMemoryPool.TmphPushSubArray> buffers = new TmphList<TmphMemoryPool.TmphPushSubArray>(sizeof(int));

        /// <summary>
        ///     未写入文件缓存集合字节数
        /// </summary>
        private long bufferSize;

        /// <summary>
        ///     刷新检测
        /// </summary>
        private Action checkFlushHandle;

        /// <summary>
        ///     刷新检测周期
        /// </summary>
        private long checkFlushTicks = TmphDate.SecondTicks << 1;

        /// <summary>
        ///     刷新检测时间
        /// </summary>
        private DateTime checkFlushTime;

        /// <summary>
        ///     正在写入文件缓存集合
        /// </summary>
        private TmphList<TmphMemoryPool.TmphPushSubArray> currentBuffers = new TmphList<TmphMemoryPool.TmphPushSubArray>(sizeof(int));

        /// <summary>
        ///     当前写入位置
        /// </summary>
        private int currentIndex;

        /// <summary>
        ///     当前写入缓存后的文件长度
        /// </summary>
        protected long fileBufferLength;

        /// <summary>
        ///     文件有效长度(已经写入)
        /// </summary>
        protected long fileLength;

        /// <summary>
        ///     文件流
        /// </summary>
        private FileStream fileStream;

        /// <summary>
        ///     缓存刷新等待数量
        /// </summary>
        protected int flushCount;

        /// <summary>
        ///     是否正在检测刷新
        /// </summary>
        private int isCheckFlush;

        /// <summary>
        ///     最后一个数据是否数据复制缓冲区
        /// </summary>
        private byte isCopyBuffer;

        /// <summary>
        ///     是否已经释放资源
        /// </summary>
        protected byte isDisposed;

        /// <summary>
        ///     是否正在刷新
        /// </summary>
        private byte isFlush;

        /// <summary>
        ///     是否正在写文件
        /// </summary>
        private byte isWritting;

        /// <summary>
        ///     文件写入缓冲起始位置
        /// </summary>
        private int startIndex;

        /// <summary>
        ///     文件流写入器
        /// </summary>
        /// <param name="fileName">文件全名</param>
        /// <param name="mode">打开方式</param>
        /// <param name="fileShare">共享访问方式</param>
        /// <param name="fileOption">附加选项</param>
        /// <param name="encoding">文件编码</param>
        public TmphFileStreamWriter(string fileName, FileMode mode = FileMode.CreateNew,
            FileShare fileShare = FileShare.None, FileOptions fileOption = FileOptions.None, bool isLog = true,
            Encoding encoding = null)
        {
            if (fileName.Length() == 0) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            FileName = fileName;
            this.isLog = isLog;
            this.fileShare = fileShare;
            this.fileOption = fileOption;
            this.encoding = encoding;
            memoryPool = TmphMemoryPool.GetPool(bufferLength = (int)TmphFile.BytesPerCluster(fileName));
            TmphBuffer = memoryPool.Get();
            writeFileHandle = writeFile;
            open(mode);
            flushWait = new EventWaitHandle(true, EventResetMode.ManualReset);
        }

        /// <summary>
        ///     文件名称
        /// </summary>
        public string FileName { get; private set; }

        /// <summary>
        ///     缓存刷新检测秒数
        /// </summary>
        public uint CheckFlushSecond
        {
            set { checkFlushTicks = value * TmphDate.SecondTicks; }
        }

        /// <summary>
        ///     缓存刷新检测毫秒数
        /// </summary>
        public uint CheckFlushMillisecond
        {
            set { checkFlushTicks = value * TmphDate.MillisecondTicks; }
        }

        /// <summary>
        ///     文件流长度
        /// </summary>
        public long FileSize
        {
            get
            {
                var fileStream = this.fileStream;
                return fileStream != null ? fileStream.Length : -1;
            }
        }

        /// <summary>
        ///     是否需要等待写入缓存
        /// </summary>
        public bool IsWaitBuffer
        {
            get { return bufferSize > maxBufferSize; }
        }

        /// <summary>
        ///     最后一次异常错误
        /// </summary>
        public Exception LastException { get; private set; }

        /// <summary>
        ///     释放资源
        /// </summary>
        public void Dispose()
        {
            TmphInterlocked.NoCheckCompareSetSleep0(ref bufferLock);
            if (isDisposed == 0)
            {
                isDisposed = 1;
                bufferLock = 0;
                while (LastException == null)
                {
                    Interlocked.Increment(ref flushCount);
                    flushWait.WaitOne();
                    Interlocked.Decrement(ref flushCount);
                    if (LastException == null)
                    {
                        TmphInterlocked.NoCheckCompareSetSleep0(ref bufferLock);
                        if (fileBufferLength == fileLength)
                        {
                            bufferLock = 0;
                            break;
                        }
                        bufferLock = 0;
                    }
                }
                dispose();
                TmphPub.Dispose(ref fileStream);
                TmphInterlocked.NoCheckCompareSetSleep0(ref bufferLock);
                buffers.Null();
                currentBuffers.Null();
                bufferLock = 0;
                memoryPool.Push(ref TmphBuffer);
                flushWait.Set();
                flushWait.Close();
            }
            else bufferLock = 0;
        }

        /// <summary>
        ///     打开文件
        /// </summary>
        /// <param name="mode">打开方式</param>
        private unsafe void open(FileMode mode)
        {
            startIndex = currentIndex = 0;
            fileStream = new FileStream(FileName, mode, FileAccess.Write, fileShare, bufferLength, fileOption);
            fileLength = fileBufferLength = bufferIndex = fileStream.Length;
            if (fileLength != 0)
            {
                fileStream.Seek(0, SeekOrigin.End);
                startIndex = currentIndex = (int)(fileLength % bufferLength);
            }
            else if (encoding != null)
            {
                var bom = TmphFile.GetBom(encoding);
                if ((currentIndex = bom.Length) != 0)
                {
                    bufferIndex = (fileBufferLength += currentIndex);
                    fixed (byte* bufferFixed = TmphBuffer) *(uint*)bufferFixed = bom.Bom;
                }
            }
        }

        ///// <summary>
        ///// 写入数据
        ///// </summary>
        ///// <param name="value">数据</param>
        ///// <returns>写入位置,失败返回-1</returns>
        //public long Write(string value)
        //{
        //    return value.length() != 0 ? UnsafeWrite(value) : 0;
        //}
        /// <summary>
        ///     写入数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="count">写入字节数</param>
        /// <returns>写入位置,失败返回-1</returns>
        public long Write(byte[] data, int count)
        {
            if (count > data.length() || count < 0) TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
            return count != 0
                ? UnsafeWrite(new TmphMemoryPool.TmphPushSubArray { Value = TmphSubArray<byte>.Unsafe(data, 0, count) })
                : 0;
        }

        /// <summary>
        ///     写入数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="index">起始位置</param>
        /// <param name="count">写入字节数</param>
        /// <returns>写入位置,失败返回-1</returns>
        public long Write(byte[] data, int index, int count)
        {
            if (index + count > data.length() || index < 0 || count < 0)
                TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
            return count != 0
                ? UnsafeWrite(new TmphMemoryPool.TmphPushSubArray { Value = TmphSubArray<byte>.Unsafe(data, index, count) })
                : 0;
        }

        /// <summary>
        ///     字符串转换成字节数组
        /// </summary>
        /// <param name="value">字符串</param>
        /// <returns>字节数组+缓冲区入池调用</returns>
        internal unsafe TmphMemoryPool.TmphPushSubArray GetBytes(string value)
        {
            var encoding = this.encoding ?? TmphAppSetting.Encoding;
            var length = encoding.CodePage == Encoding.Unicode.CodePage
                ? value.Length << 1
                : encoding.GetByteCount(value);
            var pool = TmphMemoryPool.GetDefaultPool(length);
            var data = pool.Get(length);
            if (encoding.CodePage == Encoding.Unicode.CodePage)
            {
                fixed (byte* dataFixed = data) Unsafe.TmphString.Copy(value, dataFixed);
            }
            else encoding.GetBytes(value, 0, value.Length, data, 0);
            return new TmphMemoryPool.TmphPushSubArray
            {
                Value = TmphSubArray<byte>.Unsafe(data, 0, length),
                PushPool = pool.PushHandle
            };
        }

        ///// <summary>
        ///// 写入数据
        ///// </summary>
        ///// <param name="value">数据</param>
        ///// <returns>写入位置,失败返回-1</returns>
        //internal unsafe long UnsafeWrite(string value)
        //{
        //    return UnsafeWrite(GetBytes(value));
        //}
        /// <summary>
        ///     写入数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns>写入位置,失败返回-1</returns>
        internal long UnsafeWrite(byte[] data)
        {
            return UnsafeWrite(new TmphMemoryPool.TmphPushSubArray { Value = TmphSubArray<byte>.Unsafe(data, 0, data.Length) });
        }

        /// <summary>
        ///     写入数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="index">起始位置</param>
        /// <param name="count">写入字节数</param>
        /// <returns>写入位置,失败返回-1</returns>
        internal long UnsafeWrite(byte[] data, int index, int count)
        {
            return UnsafeWrite(new TmphMemoryPool.TmphPushSubArray { Value = TmphSubArray<byte>.Unsafe(data, index, count) });
        }

        /// <summary>
        ///     写入数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns>写入位置,失败返回-1</returns>
        internal long UnsafeWrite(TmphMemoryPool.TmphPushSubArray data)
        {
            flushWait.Reset();
            var dataArray = data.Value;
            TmphInterlocked.NoCheckCompareSetSleep0(ref bufferLock);
            if (isDisposed == 0)
            {
                var fileBufferLength = this.fileBufferLength;
                this.fileBufferLength += dataArray.Count;
                if (isWritting == 0)
                {
                    var length = currentIndex + dataArray.Count;
                    if (length < bufferLength && flushCount == 0)
                    {
                        Buffer.BlockCopy(dataArray.array, dataArray.StartIndex, TmphBuffer, currentIndex, dataArray.Count);
                        checkFlushTime = TmphDate.NowSecond.AddTicks(checkFlushTicks);
                        currentIndex = length;
                        bufferIndex = this.fileBufferLength;
                        bufferLock = 0;
                        data.Push();
                        setCheckFlush();
                    }
                    else
                    {
                        buffers.array[0] = data;
                        buffers.Unsafer.AddLength(1);
                        bufferSize += dataArray.Count;
                        isFlush = 0;
                        isWritting = 1;
                        isCopyBuffer = 0;
                        bufferLock = 0;
                        TmphThreadPool.TinyPool.FastStart(writeFileHandle, null, null);
                    }
                }
                else
                {
                    try
                    {
                        buffers.Add(data);
                        bufferSize += dataArray.Count;
                        isCopyBuffer = 0;
                    }
                    finally
                    {
                        bufferLock = 0;
                    }
                }
                return fileBufferLength;
            }
            bufferLock = 0;
            data.Push();
            return -1;
        }

        ///// <summary>
        ///// 复制数据并写入
        ///// </summary>
        ///// <param name="data">数据</param>
        ///// <returns>写入位置,失败返回-1</returns>
        //internal long UnsafeWriteCopy(subArray<byte> data)
        //{
        //    byte isWriteBuffer = 0, isWrite = 0;
        //    THInterlocked.NoCheckCompareSetSleep0(ref bufferLock);
        //    long fileBufferLength = this.fileBufferLength;
        //    byte isDisposed = this.isDisposed;
        //    if (isDisposed == 0)
        //    {
        //        this.fileBufferLength += data.Count;
        //        if (isWritting == 0)
        //        {
        //            int length = currentIndex + data.Count;
        //            if (length < bufferLength)
        //            {
        //                TmphBuffer.BlockCopy(data.Array, data.StartIndex, TmphBuffer, currentIndex, data.Count);
        //                currentIndex = length;
        //                bufferLock = 0;
        //                isWriteBuffer = 1;
        //            }
        //            else
        //            {
        //                try
        //                {
        //                    copy(data.array, data.StartIndex, data.Count);
        //                    bufferSize += data.Count;
        //                    isWritting = 1;
        //                }
        //                finally { bufferLock = 0; }
        //                isWrite = 1;
        //            }
        //        }
        //        else
        //        {
        //            try
        //            {
        //                copy(data.array, data.StartIndex, data.Count);
        //                bufferSize += data.Count;
        //            }
        //            finally { bufferLock = 0; }
        //        }
        //    }
        //    else bufferLock = 0;
        //    if (isDisposed == 0)
        //    {
        //        if (isWrite == 0)
        //        {
        //            if (isWriteBuffer != 0) setCheckFlush();
        //        }
        //        else threadPool.TinyPool.FastStart(writeFileHandle, null, null);
        //        return fileBufferLength;
        //    }
        //    return -1;
        //}
        ///// <summary>
        ///// 复制数据到缓冲区
        ///// </summary>
        ///// <param name="data">数据</param>
        ///// <param name="startIndex">数据其实位置</param>
        ///// <param name="length">数据长度</param>
        //private void copy(byte[] data, int startIndex, int length)
        //{
        //    if (isCopyBuffer != 0)
        //    {
        //        memoryPool.pushSubArray[] bufferArray = buffers.array;
        //        int bufferIndex = buffers.Count - 1;
        //        subArray<byte> copyBuffer = bufferArray[bufferIndex].Value;
        //        int freeLength = copyBuffer.FreeLength;
        //        if (length <= freeLength)
        //        {
        //            TmphBuffer.BlockCopy(data, startIndex, copyBuffer.array, copyBuffer.EndIndex, length);
        //            bufferArray[bufferIndex].Value.UnsafeSetLength(copyBuffer.Count + length);
        //            if (length == freeLength) isCopyBuffer = 0;
        //            return;
        //        }
        //        TmphBuffer.BlockCopy(data, startIndex, copyBuffer.array, copyBuffer.EndIndex, freeLength);
        //        bufferArray[bufferIndex].Value.UnsafeSetLength(copyBuffer.array.Length);
        //        startIndex += freeLength;
        //        length -= freeLength;
        //    }
        //    do
        //    {
        //        byte[] TmphBuffer = memoryPool.TryGet();
        //        if (TmphBuffer == null)
        //        {
        //            if (length <= memoryPool.Size)
        //            {
        //                TmphBuffer.BlockCopy(data, startIndex, TmphBuffer = memoryPool.Get(), 0, length);
        //                isCopyBuffer = length == TmphBuffer.Length ? (byte)0 : (byte)1;
        //                buffers.Add(new memoryPool.pushSubArray { Value = subArray<byte>.Unsafe(TmphBuffer, 0, length), PushPool = memoryPool.PushHandle });
        //                return;
        //            }
        //            TmphBuffer.BlockCopy(data, startIndex, TmphBuffer = new byte[length], 0, length);
        //            buffers.Add(new memoryPool.pushSubArray { Value = subArray<byte>.Unsafe(TmphBuffer, 0, length) });
        //            isCopyBuffer = 0;
        //            return;
        //        }
        //        if (length <= TmphBuffer.Length)
        //        {
        //            TmphBuffer.BlockCopy(data, startIndex, TmphBuffer, 0, length);
        //            isCopyBuffer = length == TmphBuffer.Length ? (byte)0 : (byte)1;
        //            buffers.Add(new memoryPool.pushSubArray { Value = subArray<byte>.Unsafe(TmphBuffer, 0, length), PushPool = memoryPool.PushHandle });
        //            return;
        //        }
        //        TmphBuffer.BlockCopy(data, startIndex, TmphBuffer, 0, TmphBuffer.Length);
        //        startIndex += TmphBuffer.Length;
        //        length -= TmphBuffer.Length;
        //        buffers.Add(new memoryPool.pushSubArray { Value = subArray<byte>.Unsafe(TmphBuffer, 0, TmphBuffer.Length), PushPool = memoryPool.PushHandle });
        //    }
        //    while (true);
        //}
        /// <summary>
        ///     写入数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="dataLength">数据长度</param>
        /// <returns>写入位置,失败返回-1</returns>
        internal unsafe long UnsafeWrite(byte* data, int dataLength)
        {
            flushWait.Reset();
            Exception exception = null;
            fixed (byte* bufferFixed = TmphBuffer)
            {
                TmphInterlocked.NoCheckCompareSetSleep0(ref bufferLock);
                if (isDisposed == 0)
                {
                    var fileBufferLength = this.fileBufferLength;
                    this.fileBufferLength += dataLength;
                    if (isWritting == 0)
                    {
                        var length = currentIndex + dataLength;
                        if (length < bufferLength && flushCount == 0)
                        {
                            Unsafe.TmphMemory.Copy(data, bufferFixed + currentIndex, dataLength);
                            checkFlushTime = TmphDate.NowSecond.AddTicks(checkFlushTicks);
                            currentIndex = length;
                            bufferIndex = this.fileBufferLength;
                            bufferLock = 0;
                            setCheckFlush();
                        }
                        else
                        {
                            try
                            {
                                copy(data, dataLength);
                                bufferSize += dataLength;
                                isFlush = 0;
                                isWritting = 1;
                            }
                            catch (Exception error)
                            {
                                exception = error;
                            }
                            finally
                            {
                                bufferLock = 0;
                            }
                            if (exception == null) TmphThreadPool.TinyPool.FastStart(writeFileHandle, null, null);
                            else
                            {
                                error(exception);
                                return -1;
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            copy(data, dataLength);
                            bufferSize += dataLength;
                        }
                        catch (Exception error)
                        {
                            exception = error;
                        }
                        finally
                        {
                            bufferLock = 0;
                        }
                        if (exception != null)
                        {
                            error(exception);
                            return -1;
                        }
                    }
                    return fileBufferLength;
                }
                bufferLock = 0;
            }
            return -1;
        }

        /// <summary>
        ///     复制数据到缓冲区
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="length">数据长度</param>
        private unsafe void copy(byte* data, int length)
        {
            if (isCopyBuffer != 0)
            {
                var bufferArray = buffers.array;
                var bufferIndex = buffers.Count - 1;
                var copyBuffer = bufferArray[bufferIndex].Value;
                var freeLength = copyBuffer.FreeLength;
                if (length <= freeLength)
                {
                    fixed (byte* bufferFixed = copyBuffer.array)
                        Unsafe.TmphMemory.Copy(data, bufferFixed + copyBuffer.EndIndex, length);
                    bufferArray[bufferIndex].Value.UnsafeSetLength(copyBuffer.Count + length);
                    if (length == freeLength) isCopyBuffer = 0;
                    return;
                }
                fixed (byte* bufferFixed = copyBuffer.array)
                    Unsafe.TmphMemory.Copy(data, bufferFixed + copyBuffer.EndIndex, freeLength);
                bufferArray[bufferIndex].Value.UnsafeSetLength(copyBuffer.array.Length);
                data += freeLength;
                length -= freeLength;
            }
            do
            {
                var TmphBuffer = memoryPool.TryGet();
                if (TmphBuffer == null)
                {
                    if (length <= memoryPool.Size)
                    {
                        Unsafe.TmphMemory.Copy(data, TmphBuffer = memoryPool.Get(), length);
                        isCopyBuffer = length == TmphBuffer.Length ? (byte)0 : (byte)1;
                        buffers.Add(new TmphMemoryPool.TmphPushSubArray
                        {
                            Value = TmphSubArray<byte>.Unsafe(TmphBuffer, 0, length),
                            PushPool = memoryPool.PushHandle
                        });
                        return;
                    }
                    Unsafe.TmphMemory.Copy(data, TmphBuffer = new byte[length], length);
                    buffers.Add(new TmphMemoryPool.TmphPushSubArray { Value = TmphSubArray<byte>.Unsafe(TmphBuffer, 0, length) });
                    isCopyBuffer = 0;
                    return;
                }
                if (length <= TmphBuffer.Length)
                {
                    Unsafe.TmphMemory.Copy(data, TmphBuffer, length);
                    isCopyBuffer = length == TmphBuffer.Length ? (byte)0 : (byte)1;
                    buffers.Add(new TmphMemoryPool.TmphPushSubArray
                    {
                        Value = TmphSubArray<byte>.Unsafe(TmphBuffer, 0, length),
                        PushPool = memoryPool.PushHandle
                    });
                    return;
                }
                Unsafe.TmphMemory.Copy(data, TmphBuffer, TmphBuffer.Length);
                data += TmphBuffer.Length;
                length -= TmphBuffer.Length;
                buffers.Add(new TmphMemoryPool.TmphPushSubArray
                {
                    Value = TmphSubArray<byte>.Unsafe(TmphBuffer, 0, TmphBuffer.Length),
                    PushPool = memoryPool.PushHandle
                });
            } while (true);
        }

        /// <summary>
        ///     写入文件数据
        /// </summary>
        private void writeFile()
        {
            try
            {
                do
                {
                    TmphInterlocked.NoCheckCompareSetSleep0(ref bufferLock);
                    var bufferCount = buffers.Count;
                    if (bufferCount == 0)
                    {
                        if ((flushCount | isFlush) == 0)
                        {
                            checkFlushTime = TmphDate.NowSecond.AddTicks(checkFlushTicks);
                            isWritting = 0;
                            if (currentIndex == startIndex) bufferLock = 0;
                            else
                            {
                                bufferLock = 0;
                                setCheckFlush();
                            }
                            break;
                        }
                        isFlush = 0;
                        var writeSize = currentIndex - startIndex;
                        if (writeSize == 0)
                        {
                            bufferLock = 0;
                            if (buffers.Count == 0)
                            {
                                fileStream.Flush();
                                if (buffers.Count == 0) flushWait.Set();
                            }
                            continue;
                        }
                        bufferLock = 0;
                        fileStream.Write(TmphBuffer, startIndex, writeSize);
                        TmphInterlocked.NoCheckCompareSetSleep0(ref bufferLock);
                        fileLength += writeSize;
                        bufferLock = 0;
                        startIndex = currentIndex;
                        if (buffers.Count == 0)
                        {
                            fileStream.Flush();
                            if (buffers.Count == 0) flushWait.Set();
                        }
                        continue;
                    }
                    var datas = buffers;
                    isCopyBuffer = 0;
                    buffers = currentBuffers;
                    bufferIndex = fileBufferLength;
                    currentBuffers = datas;
                    bufferLock = 0;
                    foreach (var data in datas.array)
                    {
                        int dataSize = data.Value.Count, writeSize = writeFile(data.Value);
                        TmphInterlocked.NoCheckCompareSetSleep0(ref bufferLock);
                        fileLength += writeSize;
                        bufferSize -= dataSize;
                        bufferLock = 0;
                        data.Push();
                        if (--bufferCount == 0) break;
                    }
                    Array.Clear(datas.array, 0, datas.Count);
                    datas.Empty();
                    if (isCopyBuffer != 0 && buffers.Count == 0) Thread.Sleep(0);
                } while (true);
            }
            catch (Exception error)
            {
                this.error(error);
            }
        }

        /// <summary>
        ///     写入文件数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns>写入文件字节数</returns>
        private int writeFile(TmphSubArray<byte> data)
        {
            int count = data.Count, length = currentIndex + count;
            if (length < bufferLength)
            {
                Buffer.BlockCopy(data.Array, data.StartIndex, TmphBuffer, currentIndex, count);
                currentIndex = length;
                return 0;
            }
            var dataArray = data.Array;
            var index = data.StartIndex;
            length = bufferLength - currentIndex;
            if (currentIndex == startIndex)
            {
                fileStream.Write(dataArray, index, length += ((count - length) / bufferLength) * bufferLength);
                index += length;
                count -= length;
            }
            else
            {
                Buffer.BlockCopy(dataArray, index, TmphBuffer, currentIndex, length);
                index += length;
                count -= length;
                fileStream.Write(TmphBuffer, startIndex, length = bufferLength - startIndex);
                var size = count / bufferLength;
                if (size != 0)
                {
                    fileStream.Write(dataArray, index, size *= bufferLength);
                    index += size;
                    count -= size;
                    length += size;
                }
            }
            Buffer.BlockCopy(dataArray, index, TmphBuffer, startIndex = 0, currentIndex = count);
            return length;
        }

        ///// <summary>
        ///// 同步写入文件
        ///// </summary>
        ///// <param name="fileName">文件名</param>
        ///// <param name="startIndex">读取文件起始位置</param>
        //internal void WriteFile(string fileName, int startIndex)
        //{
        //    if (File.Exists(fileName))
        //    {
        //        try
        //        {
        //            using (FileStream readFileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, bufferLength, FileOptions.SequentialScan))
        //            {
        //                readFileStream.Seek(startIndex, SeekOrigin.Begin);
        //                int length = readFileStream.Read(TmphBuffer, currentIndex, bufferLength - currentIndex);
        //                fileBufferLength += length;
        //                if ((currentIndex += length) == bufferLength)
        //                {
        //                    fileStream.Write(TmphBuffer, this.startIndex, length = bufferLength - this.startIndex);
        //                    this.startIndex = 0;
        //                    fileLength += length;
        //                    do
        //                    {
        //                        currentIndex = readFileStream.Read(TmphBuffer, 0, bufferLength);
        //                        fileBufferLength += currentIndex;
        //                        if (currentIndex == bufferLength)
        //                        {
        //                            fileStream.Write(TmphBuffer, 0, bufferLength);
        //                            fileLength += currentIndex;
        //                        }
        //                        else break;
        //                    }
        //                    while (true);
        //                }
        //            }
        //        }
        //        catch (Exception error)
        //        {
        //            this.error(error);
        //        }
        //    }
        //}
        /// <summary>
        ///     等待缓存写入
        /// </summary>
        /// <param name="isDiskFile">是否写入到磁盘文件</param>
        /// <returns>最后一次异常错误</returns>
        public Exception Flush(bool isDiskFile)
        {
            flush(true);
            if (isDiskFile)
            {
                var fileStream = this.fileStream;
                if (fileStream != null) fileStream.Flush(true);
            }
            return LastException;
        }

        /// <summary>
        ///     设置刷新检测
        /// </summary>
        private void setCheckFlush()
        {
            if (Interlocked.CompareExchange(ref isCheckFlush, 1, 0) == 0)
            {
                if (checkFlushHandle == null) checkFlushHandle = checkFlush;
                TmphTimerTask.Default.Add(checkFlushHandle, checkFlushTime, null);
            }
        }

        /// <summary>
        ///     刷新检测
        /// </summary>
        private void checkFlush()
        {
            if (isWritting == 0)
            {
                if (checkFlushTime <= TmphDate.NowSecond)
                {
                    try
                    {
                        flush(false);
                    }
                    finally
                    {
                        isCheckFlush = 0;
                    }
                }
                else TmphTimerTask.Default.Add(checkFlushHandle, checkFlushTime, null);
            }
            else isCheckFlush = 0;
        }

        /// <summary>
        ///     等待缓存写入
        /// </summary>
        /// <param name="isWait"></param>
        protected void flush(bool isWait)
        {
            if (LastException == null)
            {
                TmphInterlocked.NoCheckCompareSetSleep0(ref bufferLock);
                if (isWritting == 0 && fileBufferLength != fileLength)
                {
                    isWritting = isFlush = 1;
                    bufferLock = 0;
                    writeFile();
                    if (isWait)
                    {
                        Interlocked.Increment(ref flushCount);
                        flushWait.WaitOne();
                        Interlocked.Decrement(ref flushCount);
                    }
                }
                else bufferLock = 0;
            }
        }

        /// <summary>
        ///     等待缓存写入
        /// </summary>
        public void WaitWriteBuffer()
        {
            if (bufferSize > maxBufferSize)
            {
                Thread.Sleep(0);
                while (bufferSize > maxBufferSize) Thread.Sleep(1);
            }
        }

        /// <summary>
        ///     写文件错误
        /// </summary>
        /// <param name="error">错误异常</param>
        private void error(Exception error)
        {
            if (isLog) TmphLog.Default.Add(error, null, false);
            LastException = error;
            TmphInterlocked.NoCheckCompareSetSleep0(ref bufferLock);
            var isDisposed = this.isDisposed;
            currentIndex = startIndex;
            isWritting = 0;
            fileBufferLength = fileLength = 0;
            this.isDisposed = 1;
            bufferLock = 0;
            dispose();
            TmphPub.Dispose(ref fileStream);
            TmphList<TmphMemoryPool.TmphPushSubArray> buffers = null;
            Interlocked.Exchange(ref buffers, this.buffers);
            try
            {
                if (buffers != null && buffers.Count != 0)
                {
                    var dataArray = buffers.array;
                    for (var index = buffers.Count; index != 0; dataArray[--index].Push()) ;
                }
            }
            finally
            {
                currentBuffers = null;
                memoryPool.Push(ref TmphBuffer);
                if (isDisposed == 0)
                {
                    flushWait.Set();
                    flushWait.Close();
                }
            }
        }

        /// <summary>
        ///     释放资源
        /// </summary>
        protected virtual void dispose()
        {
        }
    }
}