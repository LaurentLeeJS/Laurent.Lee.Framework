using System;
using System.IO;
using System.Threading;

namespace Laurent.Lee.CLB.Threading
{
    /// <summary>
    /// 异步模拟async+await
    /// </summary>
    public static class TmphAsynchronous
    {
        /// <summary>
        /// 异步完成状态
        /// </summary>
        public class TmphCompleteState : IDisposable
        {
            /// <summary>
            /// 异常错误
            /// </summary>
            protected Exception error;

            /// <summary>
            /// 完成状态等待事件
            /// </summary>
            protected readonly EventWaitHandle completeWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset, null);

            /// <summary>
            /// 异步任务是否已完成
            /// </summary>
            protected bool isCompleted;

            /// <summary>
            /// 释放资源
            /// </summary>
            public void Dispose()
            {
                completeWaitHandle.Set();
                completeWaitHandle.Close();
            }

            /// <summary>
            /// 异步任务完成
            /// </summary>
            protected void completed()
            {
                if (!isCompleted)
                {
                    isCompleted = true;
                    completeWaitHandle.Set();
                }
            }

            /// <summary>
            /// 等待异步任务完成
            /// </summary>
            protected void wait()
            {
                if (!isCompleted) completeWaitHandle.WaitOne();
            }

            /// <summary>
            /// 等待异步任务完成,如果有异常则抛出
            /// </summary>
            protected void waitError()
            {
                wait();
                if (error != null) throw error;
            }
        }

        /// <summary>
        /// 数据流异步读
        /// </summary>
        public sealed class TmphStreamReader : TmphCompleteState
        {
            /// <summary>
            /// 数据流
            /// </summary>
            private Stream stream;

            /// <summary>
            /// 从流中读取的字节数,0表示结束
            /// </summary>
            private int readSize = -1;

            /// <summary>
            /// 数据流异步读
            /// </summary>
            /// <param name="stream">数据流</param>
            /// <param name="buffer">目标字节数组</param>
            /// <param name="offset">目标起始位置</param>
            /// <param name="count">读取数据字节数</param>
            /// <param name="state">绑定用户对象</param>
            public TmphStreamReader(Stream stream, byte[] buffer, int offset, int count, object state)
            {
                this.stream = stream;
                try
                {
                    stream.BeginRead(buffer, offset, count, onRead, state);
                }
                catch (Exception error)
                {
                    this.error = error;
                    completed();
                }
            }

            /// <summary>
            /// 异步流读取完成
            /// </summary>
            /// <param name="result">异步操作状态</param>
            private void onRead(IAsyncResult result)
            {
                try
                {
                    readSize = stream.EndRead(result);
                }
                catch (Exception error)
                {
                    this.error = error;
                }
                finally { completed(); }
            }

            /// <summary>
            /// 等待异步任务完成
            /// </summary>
            /// <returns>从流中读取的字节数,0表示结束,-1表示出错</returns>
            public int Wait()
            {
                wait();
                return readSize;
            }

            /// <summary>
            /// 等待异步任务完成,如果有异常则抛出
            /// </summary>
            /// <returns>从流中读取的字节数,0表示结束</returns>
            public int WaitError()
            {
                waitError();
                return readSize;
            }
        }

        /// <summary>
        /// 异步读
        /// </summary>
        /// <param name="stream">数据流</param>
        /// <param name="buffer">目标字节数组</param>
        /// <param name="offset">目标起始位置</param>
        /// <param name="count">读取数据字节数</param>
        /// <param name="state">绑定用户对象</param>
        /// <returns>异步任务</returns>
        public static TmphStreamReader readAsync(this Stream stream, byte[] buffer, int offset, int count, object state)
        {
            return new TmphStreamReader(stream, buffer, offset, count, state);
        }

        /// <summary>
        /// 数据流异步读
        /// </summary>
        public sealed class TmphStreamEndReader : TmphCompleteState
        {
            /// <summary>
            /// 数据流
            /// </summary>
            private Stream stream;

            /// <summary>
            /// 目标字节流
            /// </summary>
            private TmphMemoryStream memoryStream;

            /// <summary>
            /// 数据流异步读
            /// </summary>
            /// <param name="stream">数据流</param>
            /// <param name="state">绑定用户对象</param>
            /// <param name="memoryStream">目标字节流</param>
            public TmphStreamEndReader(Stream stream, object state, TmphMemoryStream memoryStream)
            {
                this.stream = stream;
                try
                {
                    this.memoryStream = memoryStream ?? new TmphMemoryStream();
                    stream.BeginRead(this.memoryStream.Array, this.memoryStream.Length, this.memoryStream.Array.Length - this.memoryStream.Length, onRead, state);
                }
                catch (Exception error)
                {
                    this.error = error;
                    completed();
                }
            }

            /// <summary>
            /// 异步流读取完成
            /// </summary>
            /// <param name="result">异步操作状态</param>
            private void onRead(IAsyncResult result)
            {
                try
                {
                    int read = stream.EndRead(result);
                    if (read > 0)
                    {
                        memoryStream.Unsafer.AddLength(read);
                        if (memoryStream.Array.Length == memoryStream.Length) memoryStream.PrepLength(memoryStream.Length);
                        stream.BeginRead(memoryStream.Array, memoryStream.Length, memoryStream.Array.Length - memoryStream.Length, onRead, result.AsyncState);
                    }
                    else if (read == 0) completed();
                }
                catch (Exception error)
                {
                    this.error = error;
                    completed();
                }
            }

            /// <summary>
            /// 等待异步任务完成
            /// </summary>
            /// <returns>目标字节流</returns>
            public TmphMemoryStream Wait()
            {
                wait();
                return memoryStream;
            }

            /// <summary>
            /// 等待异步任务完成,如果有异常则抛出
            /// </summary>
            /// <returns>目标字节流</returns>
            public TmphMemoryStream WaitError()
            {
                waitError();
                return memoryStream;
            }
        }

        /// <summary>
        /// 异步读
        /// </summary>
        /// <param name="stream">数据流</param>
        /// <param name="state">绑定用户对象</param>
        /// <param name="memoryStream">目标字节流</param>
        /// <returns>异步任务</returns>
        public static TmphStreamEndReader readToEndAsync(this Stream stream, object state, TmphMemoryStream memoryStream)
        {
            return new TmphStreamEndReader(stream, state, memoryStream);
        }

        /// <summary>
        /// 数据流异步写
        /// </summary>
        public sealed class TmphStreamWriter : TmphCompleteState
        {
            /// <summary>
            /// 数据流
            /// </summary>
            private Stream stream;

            /// <summary>
            /// 数据流异步写
            /// </summary>
            /// <param name="stream">数据流</param>
            /// <param name="buffer">目标字节数组</param>
            /// <param name="offset">目标起始位置</param>
            /// <param name="count">读取数据字节数</param>
            /// <param name="state">绑定用户对象</param>
            public TmphStreamWriter(Stream stream, byte[] buffer, int offset, int count, object state)
            {
                this.stream = stream;
                try
                {
                    stream.BeginWrite(buffer, offset, count, onWrite, state);
                }
                catch (Exception error)
                {
                    this.error = error;
                    completed();
                }
            }

            /// <summary>
            /// 异步流写入完成
            /// </summary>
            /// <param name="result">异步操作状态</param>
            private void onWrite(IAsyncResult result)
            {
                try
                {
                    stream.EndWrite(result);
                }
                catch (Exception error)
                {
                    this.error = error;
                }
                finally { completed(); }
            }

            /// <summary>
            /// 等待异步任务完成
            /// </summary>
            /// <returns>异步任务是否成功</returns>
            public bool Wait()
            {
                wait();
                return error == null;
            }

            /// <summary>
            /// 等待异步任务完成,如果有异常则抛出
            /// </summary>
            public void WaitError()
            {
                waitError();
            }
        }

        /// <summary>
        /// 异步写
        /// </summary>
        /// <param name="stream">数据流</param>
        /// <param name="buffer">源字节数组</param>
        /// <param name="offset">源起始位置</param>
        /// <param name="count">写入数据字节数</param>
        /// <param name="state">绑定用户对象</param>
        /// <returns>异步任务</returns>
        public static TmphStreamWriter writeAsync(this Stream stream, byte[] buffer, int offset, int count, object state)
        {
            return new TmphStreamWriter(stream, buffer, offset, count, state);
        }

        /// <summary>
        /// 异步写
        /// </summary>
        /// <param name="stream">数据流</param>
        /// <param name="buffer">源字节数组</param>
        /// <param name="offset">源起始位置</param>
        /// <param name="count">写入数据字节数</param>
        /// <param name="state">绑定用户对象</param>
        /// <returns>异步任务</returns>
        public static TmphStreamWriter writeAsync(this Stream stream, byte[] buffer, object state)
        {
            return new TmphStreamWriter(stream, buffer, 0, buffer.length(), state);
        }
    }
}