using Laurent.Lee.CLB.Threading;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Laurent.Lee.CLB
{
    /// <summary>
    /// 快速字符串池
    /// </summary>
    public abstract class TmphStringPool
    {
        /// <summary>
        /// 缓冲区集合访问锁
        /// </summary>
        protected int bufferLock;

        /// <summary>
        /// 缓冲区尺寸(单位字符)
        /// </summary>
        public int Size { get; protected set; }

        /// <summary>
        /// 清除缓冲区集合
        /// </summary>
        public void Clear()
        {
            clear(0);
        }

        /// <summary>
        /// 清除缓冲区集合
        /// </summary>
        /// <param name="count">保留清除缓冲区数量</param>
        public void Clear(int count)
        {
            clear(count <= 0 ? 0 : count);
        }

        /// <summary>
        /// 清除缓冲区集合
        /// </summary>
        /// <param name="count">保留清除缓冲区数量</param>
        protected abstract void clear(int count);

        /// <summary>
        /// 获取缓冲区
        /// </summary>
        /// <returns>缓冲区,失败返回null</returns>
        public abstract string TryGet();

        /// <summary>
        /// 获取缓冲区
        /// </summary>
        /// <param name="minSize">字符串长度</param>
        /// <returns>缓冲区,失败返回null</returns>
        public string TryGet(int minSize)
        {
            return minSize <= Size ? TryGet() : null;
        }

        /// <summary>
        /// 获取缓冲区
        /// </summary>
        /// <returns>缓冲区</returns>
        public string Get()
        {
            string value = TryGet();
            return value ?? TmphString.FastAllocateString(Size);
        }

        /// <summary>
        /// 获取缓冲区
        /// </summary>
        /// <param name="minSize">数据字节长度</param>
        /// <returns>缓冲区</returns>
        public string Get(int minSize)
        {
            return minSize <= Size ? Get() : TmphString.FastAllocateString(minSize);
        }

        /// <summary>
        /// 保存缓冲区
        /// </summary>
        /// <param name="TmphBuffer">缓冲区</param>
        public abstract void Push(ref string TmphBuffer);

        /// <summary>
        /// 数组模式内存池
        /// </summary>
        private sealed class TmphArrayPool : TmphStringPool
        {
            /// <summary>
            /// 缓冲区
            /// </summary>
            private struct TmphBuffer
            {
                /// <summary>
                /// 缓冲区
                /// </summary>
                public string tmphBuffer;

                /// <summary>
                /// 释放缓冲区
                /// </summary>
                /// <returns>缓冲区</returns>
                public string Free()
                {
                    string buffer = tmphBuffer;
                    tmphBuffer = null;
                    return buffer;
                }
            }

            /// <summary>
            /// 缓冲区集合
            /// </summary>
            private TmphBuffer[] buffers;

            /// <summary>
            /// 缓冲区集合更新访问锁
            /// </summary>
            private readonly object newBufferLock = new object();

            /// <summary>
            /// 缓冲区数量
            /// </summary>
            private int count;

            /// <summary>
            /// 内存池
            /// </summary>
            /// <param name="size">缓冲区尺寸</param>
            public TmphArrayPool(int size)
            {
                buffers = new TmphBuffer[Config.TmphAppSetting.PoolSize];
                Size = size;
            }

            /// <summary>
            /// 清除缓冲区集合
            /// </summary>
            /// <param name="count">保留清除缓冲区数量</param>
            protected override void clear(int count)
            {
                TmphInterlocked.CompareSetSleep0(ref bufferLock);
                int length = this.count - count;
                if (length > 0) Array.Clear(buffers, this.count = count, length);
                bufferLock = 0;
            }

            /// <summary>
            /// 获取缓冲区
            /// </summary>
            /// <returns>缓冲区,失败返回null</returns>
            public override string TryGet()
            {
                TmphInterlocked.CompareSetSleep0(ref bufferLock);
                if (count == 0)
                {
                    bufferLock = 0;
                    return null;
                }
                string TmphBuffer = buffers[--count].Free();
                bufferLock = 0;
                return TmphBuffer;
            }

            /// <summary>
            /// 保存缓冲区
            /// </summary>
            /// <param name="TmphBuffer">缓冲区</param>
            public override void Push(ref string TmphBuffer)
            {
                string value = Interlocked.Exchange(ref TmphBuffer, null);
                if (value != null && value.Length == Size)
                {
                    PUSH:
                    TmphInterlocked.CompareSetSleep0(ref bufferLock);
                    if (count == buffers.Length)
                    {
                        int length = count;
                        bufferLock = 0;
                        Monitor.Enter(newBufferLock);
                        if (length == buffers.Length)
                        {
                            try
                            {
                                TmphBuffer[] newBuffers = new TmphBuffer[length << 1];
                                TmphInterlocked.CompareSetSleep0(ref bufferLock);
                                Array.Copy(buffers, 0, newBuffers, 0, count);
                                newBuffers[count].tmphBuffer = value;
                                buffers = newBuffers;
                                ++count;
                                bufferLock = 0;
                            }
                            finally { Monitor.Exit(newBufferLock); }
                            return;
                        }
                        Monitor.Exit(newBufferLock);
                        goto PUSH;
                    }
                    buffers[count++].tmphBuffer = value;
                    bufferLock = 0;
                }
            }
        }

        /// <summary>
        /// 纠错模式内存池
        /// </summary>
        private sealed class TmphDebugPool : TmphStringPool
        {
            /// <summary>
            /// 缓冲区
            /// </summary>
            private struct TmphBuffer : IEquatable<TmphBuffer>
            {
                /// <summary>
                /// 缓冲区
                /// </summary>
                public string tmphBuffer;

                /// <summary>
                /// 判断缓冲区是否同一个实例
                /// </summary>
                /// <param name="other"></param>
                /// <returns></returns>
                public bool Equals(TmphBuffer other)
                {
                    return object.ReferenceEquals(tmphBuffer, other.tmphBuffer);
                }

                /// <summary>
                ///
                /// </summary>
                /// <returns></returns>
                public override int GetHashCode()
                {
                    return tmphBuffer.GetHashCode();
                }

                /// <summary>
                ///
                /// </summary>
                /// <param name="obj"></param>
                /// <returns></returns>
                public override bool Equals(object obj)
                {
                    return Equals((TmphBuffer)obj);
                }

                /// <summary>
                /// 释放缓冲区
                /// </summary>
                /// <returns>缓冲区</returns>
                public string Free()
                {
                    string buffer = tmphBuffer;
                    buffer = null;
                    return buffer;
                }
            }

            /// <summary>
            /// 缓冲区集合
            /// </summary>
            private HashSet<TmphBuffer> buffers;

            /// <summary>
            /// 当前最大缓冲区数量
            /// </summary>
            private int maxCount = TmphUnmanagedStreamBase.DefaultLength;

            /// <summary>
            /// 内存池
            /// </summary>
            /// <param name="size">缓冲区尺寸</param>
            public TmphDebugPool(int size)
            {
                buffers = new HashSet<TmphBuffer>();
                Size = size;
            }

            /// <summary>
            /// 清除缓冲区集合
            /// </summary>
            /// <param name="count">保留清除缓冲区数量</param>
            protected override void clear(int count)
            {
                TmphInterlocked.CompareSetSleep0(ref bufferLock);
                buffers.Clear();
                bufferLock = 0;
            }

            /// <summary>
            /// 获取缓冲区
            /// </summary>
            /// <returns>缓冲区,失败返回null</returns>
            public override string TryGet()
            {
                TmphBuffer TmphBuffer = new TmphBuffer();
                TmphInterlocked.CompareSetSleep0(ref bufferLock);
                foreach (TmphBuffer data in buffers)
                {
                    TmphBuffer = data;
                    break;
                }
                if (TmphBuffer.tmphBuffer != null) buffers.Remove(TmphBuffer);
                bufferLock = 0;
                return TmphBuffer.tmphBuffer;
            }

            /// <summary>
            /// 保存缓冲区
            /// </summary>
            /// <param name="TmphBuffer">缓冲区</param>
            public override void Push(ref string TmphBuffer)
            {
                string value = Interlocked.Exchange(ref TmphBuffer, null);
                if (value != null && value.Length == Size)
                {
                    bool isAdd, isMax = false;
                    TmphInterlocked.CompareSetSleep0(ref bufferLock);
                    try
                    {
                        if ((isAdd = buffers.Add(new TmphBuffer { tmphBuffer = value })) && (isMax = buffers.Count > maxCount))
                        {
                            maxCount <<= 1;
                        }
                    }
                    finally { bufferLock = 0; }
                    if (isAdd)
                    {
                        if (isMax)
                        {
                            TmphLog.Default.Add("快速字符串池 string(" + Size.toString() + ")[" + buffers.Count.toString() + "]", false, false);
                        }
                    }
                    else TmphLog.Default.Add("快速字符串池释放冲突 " + Size.toString(), true, false);
                }
            }
        }

        /// <summary>
        /// 内存池
        /// </summary>
        private static readonly Dictionary<int, TmphStringPool> pools;

        /// <summary>
        /// 内存池访问锁
        /// </summary>
        private static int poolLock;

        /// <summary>
        /// 获取内存池[反射引用于 Laurent.Lee.CLB.stringPoolProxy]
        /// </summary>
        /// <param name="size">缓冲区尺寸</param>
        /// <returns>内存池</returns>
        internal static TmphStringPool GetPool(int size)
        {
            if (size <= 0) TmphLog.Default.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
            TmphStringPool pool;
            TmphInterlocked.CompareSetSleep0(ref poolLock);
            if (pools.TryGetValue(size, out pool)) poolLock = 0;
            else
            {
                try
                {
                    pools.Add(size, pool = Config.TmphAppSetting.IsPoolDebug ? (TmphStringPool)new TmphDebugPool(size) : new TmphArrayPool(size));
                }
                finally { poolLock = 0; }
            }
            return pool;
        }

#if MONO
        /// <summary>
        /// 清除内存池
        /// </summary>
        public static void ClearPool() { ClearPool(0); }
        /// <summary>
        /// 清除内存池
        /// </summary>
        /// <param name="count">保留清除缓冲区数量</param>
        public static void ClearPool(int count)
#else

        /// <summary>
        /// 清除内存池
        /// </summary>
        /// <param name="count">保留清除缓冲区数量</param>
        public static void ClearPool(int count = 0)
#endif
        {
            if (count <= 0) count = 0;
            TmphInterlocked.CompareSetSleep0(ref poolLock);
            foreach (TmphStringPool pool in pools.Values) pool.clear(count);
            poolLock = 0;
        }

        ///// <summary>
        ///// 默认临时缓冲区
        ///// </summary>
        //public static readonly stringPool TinyBuffers;
        ///// <summary>
        ///// 默认流缓冲区
        ///// </summary>
        //public static readonly stringPool StreamBuffers;
        ///// <summary>
        ///// 获取临时缓冲区
        ///// </summary>
        ///// <param name="length">缓冲区字节长度</param>
        ///// <returns>临时缓冲区</returns>
        //public static stringPool GetDefaultPool(int length)
        //{
        //    return length <= unmanagedStreamBase.DefaultLength ? TinyBuffers : StreamBuffers;
        //}

        static TmphStringPool()
        {
#if MONO
            pools = new Dictionary<int, stringPool>(equalityComparer.Int);
#else
            pools = new Dictionary<int, TmphStringPool>();
#endif
            //pools.Add(unmanagedStreamBase.DefaultLength, TinyBuffers = TmphConfig.appSetting.IsPoolDebug ? (stringPool)new TmphDebugPool(unmanagedStreamBase.DefaultLength) : new TmphArrayPool(unmanagedStreamBase.DefaultLength));
            //pools.Add(TmphConfig.appSetting.StreamBufferSize, StreamBuffers = TmphConfig.appSetting.IsPoolDebug ? (stringPool)new TmphDebugPool(TmphConfig.appSetting.StreamBufferSize) : new TmphArrayPool(TmphConfig.appSetting.StreamBufferSize));
        }
    }
}