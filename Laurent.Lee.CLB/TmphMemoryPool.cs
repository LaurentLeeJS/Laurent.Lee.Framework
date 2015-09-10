using Laurent.Lee.CLB.Config;
using Laurent.Lee.CLB.Threading;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace Laurent.Lee.CLB
{
    /// <summary>
    ///     内存池
    /// </summary>
    public abstract class TmphMemoryPool
    {
        /// <summary>
        ///     内存池
        /// </summary>
        private static readonly Dictionary<int, TmphMemoryPool> pools;

        /// <summary>
        ///     内存池访问锁
        /// </summary>
        private static int poolLock;

        /// <summary>
        ///     默认临时缓冲区
        /// </summary>
        public static readonly TmphMemoryPool TinyBuffers;

        /// <summary>
        ///     默认流缓冲区
        /// </summary>
        public static readonly TmphMemoryPool StreamBuffers;

        static TmphMemoryPool()
        {
            pools = TmphDictionary.CreateInt<TmphMemoryPool>();
            pools.Add(TmphUnmanagedStreamBase.DefaultLength,
                TinyBuffers =
                    Config.TmphAppSetting.IsPoolDebug
                        ? (TmphMemoryPool)new TmphDebugPool(TmphUnmanagedStreamBase.DefaultLength)
                        : new TmphArrayPool(TmphUnmanagedStreamBase.DefaultLength));
            pools.Add(Config.TmphAppSetting.StreamBufferSize,
                StreamBuffers =
                    Config.TmphAppSetting.IsPoolDebug
                        ? (TmphMemoryPool)new TmphDebugPool(Config.TmphAppSetting.StreamBufferSize)
                        : new TmphArrayPool(Config.TmphAppSetting.StreamBufferSize));
            if (TmphAppSetting.IsCheckMemory) TmphCheckMemory.Add(MethodBase.GetCurrentMethod().DeclaringType);
        }

        /// <summary>
        ///     缓冲区尺寸
        /// </summary>
        public int Size { get; protected set; }

        /// <summary>
        ///     保存缓冲区调用委托
        /// </summary>
        public TmphPushPool<byte[]> PushHandle { get; protected set; }

        /// <summary>
        ///     保存缓冲区调用委托
        /// </summary>
        public Action<TmphSubArray<byte>> PushSubArray { get; protected set; }

        /// <summary>
        ///     清除缓冲区集合
        /// </summary>
        public void Clear()
        {
            clear(0);
        }

        /// <summary>
        ///     清除缓冲区集合
        /// </summary>
        /// <param name="count">保留清除缓冲区数量</param>
        public void Clear(int count)
        {
            clear(count <= 0 ? 0 : count);
        }

        /// <summary>
        ///     清除缓冲区集合
        /// </summary>
        /// <param name="count">保留清除缓冲区数量</param>
        protected abstract void clear(int count);

        /// <summary>
        ///     获取缓冲区
        /// </summary>
        /// <returns>缓冲区,失败返回null</returns>
        public abstract byte[] TryGet();

        /// <summary>
        ///     获取缓冲区
        /// </summary>
        /// <param name="minSize">数据字节长度</param>
        /// <returns>缓冲区,失败返回null</returns>
        public byte[] TryGet(int minSize)
        {
            return minSize <= Size ? TryGet() : null;
        }

        /// <summary>
        ///     获取缓冲区
        /// </summary>
        /// <returns>缓冲区</returns>
        public byte[] Get()
        {
            var data = TryGet();
            return data ?? new byte[Size];
        }

        /// <summary>
        ///     获取缓冲区
        /// </summary>
        /// <param name="isNew">是否新建缓冲区</param>
        /// <returns>缓冲区</returns>
        public byte[] Get(out bool isNew)
        {
            var data = TryGet();
            if (data == null)
            {
                isNew = true;
                return new byte[Size];
            }
            isNew = false;
            return data;
        }

        /// <summary>
        ///     获取缓冲区
        /// </summary>
        /// <param name="minSize">数据字节长度</param>
        /// <returns>缓冲区</returns>
        public byte[] Get(int minSize)
        {
            return minSize <= Size ? Get() : new byte[minSize];
        }

        /// <summary>
        ///     获取缓冲区
        /// </summary>
        /// <param name="minSize">数据字节长度</param>
        /// <param name="isNew">是否新建缓冲区</param>
        /// <returns>缓冲区</returns>
        public byte[] Get(int minSize, out bool isNew)
        {
            if (minSize <= Size) return Get(out isNew);
            isNew = true;
            return new byte[minSize];
        }

        /// <summary>
        ///     保存缓冲区
        /// </summary>
        /// <param name="TmphBuffer">缓冲区</param>
        public abstract void Push(ref byte[] TmphBuffer);

        /// <summary>
        ///     保存缓冲区
        /// </summary>
        /// <param name="TmphBuffer"></param>
        public void Push(TmphSubArray<byte> TmphBuffer)
        {
            TmphBuffer.UnsafeSet(0, 0);
            Push(ref TmphBuffer.array);
        }

        /// <summary>
        ///     获取内存池[反射引用于 Laurent.Lee.CLB.memoryPoolProxy]
        /// </summary>
        /// <param name="size">缓冲区尺寸</param>
        /// <returns>内存池</returns>
        internal static TmphMemoryPool GetPool(int size)
        {
            if (size <= 0) TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
            TmphMemoryPool pool;
            TmphInterlocked.NoCheckCompareSetSleep0(ref poolLock);
            if (pools.TryGetValue(size, out pool)) poolLock = 0;
            else
            {
                try
                {
                    pools.Add(size,
                        pool =
                            Config.TmphAppSetting.IsPoolDebug
                                ? (TmphMemoryPool)new TmphDebugPool(size)
                                : new TmphArrayPool(size));
                }
                finally
                {
                    poolLock = 0;
                }
            }
            return pool;
        }

        /// <summary>
        ///     清除内存池
        /// </summary>
        /// <param name="count">保留清除缓冲区数量</param>
        public static void ClearPool(int count = 0)
        {
            if (count <= 0) count = 0;
            TmphInterlocked.NoCheckCompareSetSleep0(ref poolLock);
            foreach (var pool in pools.Values) pool.clear(count);
            poolLock = 0;
        }

        /// <summary>
        ///     获取临时缓冲区
        /// </summary>
        /// <param name="length">缓冲区字节长度</param>
        /// <returns>临时缓冲区</returns>
        public static TmphMemoryPool GetDefaultPool(int length)
        {
            return length <= TmphUnmanagedStreamBase.DefaultLength ? TinyBuffers : StreamBuffers;
        }

        /// <summary>
        ///     缓冲数组子串
        /// </summary>
        public struct TmphPushSubArray
        {
            /// <summary>
            ///     数组子串入池处理
            /// </summary>
            internal TmphPushPool<byte[]> PushPool;

            /// <summary>
            ///     数组子串
            /// </summary>
            internal TmphSubArray<byte> Value;

            /// <summary>
            ///     缓冲数组子串
            /// </summary>
            /// <param name="value">数组子串</param>
            /// <param name="pushPool">数组子串入池处理</param>
            public TmphPushSubArray(TmphSubArray<byte> value, TmphPushPool<byte[]> pushPool)
            {
                Value = value;
                PushPool = pushPool;
            }

            /// <summary>
            ///     数组子串
            /// </summary>
            public TmphSubArray<byte> SubArray
            {
                get { return Value; }
            }

            /// <summary>
            ///     数组
            /// </summary>
            public byte[] Array
            {
                get { return Value.Array; }
            }

            /// <summary>
            ///     数组子串入池处理
            /// </summary>
            public void Push()
            {
                if (PushPool == null) Value.array = null;
                else
                {
                    try
                    {
                        PushPool(ref Value.array);
                    }
                    catch (Exception error)
                    {
                        TmphLog.Error.Add(error, null, false);
                    }
                }
            }
        }

        /// <summary>
        ///     数组模式内存池
        /// </summary>
        private sealed class TmphArrayPool : TmphMemoryPool
        {
            /// <summary>
            ///     缓冲区集合
            /// </summary>
            private TmphObjectPool<byte[]> pool;

            /// <summary>
            ///     内存池
            /// </summary>
            /// <param name="size">缓冲区尺寸</param>
            public TmphArrayPool(int size)
            {
                pool = TmphObjectPool<byte[]>.Create();
                Size = size;
                PushHandle = Push;
                PushSubArray = Push;
            }

            /// <summary>
            ///     清除缓冲区集合
            /// </summary>
            /// <param name="count">保留清除缓冲区数量</param>
            protected override void clear(int count)
            {
                pool.Clear(count);
            }

            /// <summary>
            ///     获取缓冲区
            /// </summary>
            /// <returns>缓冲区,失败返回null</returns>
            public override byte[] TryGet()
            {
                return pool.Pop();
            }

            /// <summary>
            ///     保存缓冲区
            /// </summary>
            /// <param name="TmphBuffer">缓冲区</param>
            public override void Push(ref byte[] TmphBuffer)
            {
                var data = Interlocked.Exchange(ref TmphBuffer, null);
                if (data != null && data.Length == Size) pool.Push(data);
            }
        }

        /// <summary>
        ///     纠错模式内存池
        /// </summary>
        private sealed class TmphDebugPool : TmphMemoryPool
        {
            /// <summary>
            ///     缓冲区集合
            /// </summary>
            private readonly HashSet<byte[]> buffers;

            /// <summary>
            ///     缓冲区集合访问锁
            /// </summary>
            private int bufferLock;

            /// <summary>
            ///     当前最大缓冲区数量
            /// </summary>
            private int maxCount = TmphAppSetting.PoolSize;

            /// <summary>
            ///     内存池
            /// </summary>
            /// <param name="size">缓冲区尺寸</param>
            public TmphDebugPool(int size)
            {
                buffers = TmphHashSet.CreateOnly<byte[]>();
                Size = size;
                PushHandle = Push;
                PushSubArray = Push;
            }

            /// <summary>
            ///     清除缓冲区集合
            /// </summary>
            /// <param name="count">保留清除缓冲区数量</param>
            protected override void clear(int count)
            {
                TmphInterlocked.NoCheckCompareSetSleep0(ref bufferLock);
                buffers.Clear();
                bufferLock = 0;
            }

            /// <summary>
            ///     获取缓冲区
            /// </summary>
            /// <returns>缓冲区,失败返回null</returns>
            public override byte[] TryGet()
            {
                byte[] TmphBuffer = null;
                TmphInterlocked.NoCheckCompareSetSleep0(ref bufferLock);
                foreach (var data in buffers)
                {
                    TmphBuffer = data;
                    break;
                }
                if (TmphBuffer != null) buffers.Remove(TmphBuffer);
                bufferLock = 0;
                return TmphBuffer;
            }

            /// <summary>
            ///     保存缓冲区
            /// </summary>
            /// <param name="TmphBuffer">缓冲区</param>
            public override void Push(ref byte[] TmphBuffer)
            {
                var data = Interlocked.Exchange(ref TmphBuffer, null);
                if (data != null && data.Length == Size)
                {
                    bool isAdd, isMax = false;
                    TmphInterlocked.NoCheckCompareSetSleep0(ref bufferLock);
                    try
                    {
                        if ((isAdd = buffers.Add(data)) && (isMax = buffers.Count > maxCount))
                        {
                            maxCount <<= 1;
                        }
                    }
                    finally
                    {
                        bufferLock = 0;
                    }
                    if (isAdd)
                    {
                        if (isMax)
                        {
                            TmphLog.Default.Add(
                                "内存池扩展实例数量 byte[" + buffers.Count.toString() + "][" + Size.toString() + "]", false,
                                false);
                        }
                    }
                    else TmphLog.Error.Add("内存池释放冲突 " + Size.toString(), true, false);
                }
            }
        }
    }
}