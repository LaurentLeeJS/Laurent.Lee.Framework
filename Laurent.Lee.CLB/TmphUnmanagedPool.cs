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
using System.Collections.Generic;
using System.Reflection;

namespace Laurent.Lee.CLB
{
    /// <summary>
    ///     非托管内存池
    /// </summary>
    public abstract unsafe class TmphUnmanagedPool
    {
        /// <summary>
        ///     内存池
        /// </summary>
        private static readonly Dictionary<int, TmphUnmanagedPool> Pools;

        /// <summary>
        ///     内存池访问锁
        /// </summary>
        private static int _poolLock;

        /// <summary>
        ///     默认临时缓冲区
        /// </summary>
        public static readonly TmphUnmanagedPool TinyBuffers;

        /// <summary>
        ///     默认流缓冲区
        /// </summary>
        public static readonly TmphUnmanagedPool StreamBuffers;

        static TmphUnmanagedPool()
        {
            Pools = TmphDictionary.CreateInt<TmphUnmanagedPool>();
            Pools.Add(TmphUnmanagedStreamBase.DefaultLength,
                TinyBuffers =
                    TmphAppSetting.IsPoolDebug
                        ? (TmphUnmanagedPool)new TmphDebugPool(TmphUnmanagedStreamBase.DefaultLength)
                        : new TmphArrayPool(TmphUnmanagedStreamBase.DefaultLength));
            Pools.Add(TmphAppSetting.StreamBufferSize,
                StreamBuffers =
                    TmphAppSetting.IsPoolDebug
                        ? (TmphUnmanagedPool)new TmphDebugPool(TmphAppSetting.StreamBufferSize)
                        : new TmphArrayPool(TmphAppSetting.StreamBufferSize));
            if (TmphAppSetting.IsCheckMemory) TmphCheckMemory.Add(MethodBase.GetCurrentMethod().DeclaringType);
        }

        /// <summary>
        ///     缓冲区尺寸
        /// </summary>
        public int Size { get; protected set; }

        /// <summary>
        ///     非托管内存数量
        /// </summary>
        protected abstract int Count { get; }

        /// <summary>
        ///     获取所有非托管内存的数量
        /// </summary>
        /// <returns></returns>
        public static int TotalCount
        {
            get
            {
                var count = 0;
                TmphInterlocked.NoCheckCompareSetSleep0(ref _poolLock);
                foreach (var pool in Pools.Values) count += pool.Count;
                _poolLock = 0;
                return count;
            }
        }

        /// <summary>
        ///     清除缓冲区集合
        /// </summary>
        public void Clear()
        {
            ClearCache(0);
        }

        /// <summary>
        ///     清除缓冲区集合
        /// </summary>
        /// <param name="cacheCount">保留清除缓冲区数量</param>
        public void Clear(int cacheCount)
        {
            ClearCache(cacheCount <= 0 ? 0 : cacheCount);
        }

        /// <summary>
        ///     清除缓冲区集合
        /// </summary>
        /// <param name="count">保留清除缓冲区数量</param>
        protected abstract void ClearCache(int count);

        /// <summary>
        ///     获取缓冲区
        /// </summary>
        /// <returns>缓冲区,失败返回null</returns>
        public abstract TmphPointer TryGet();

        /// <summary>
        ///     获取缓冲区
        /// </summary>
        /// <param name="minSize">数据字节长度</param>
        /// <returns>缓冲区,失败返回null</returns>
        public TmphPointer TryGet(int minSize)
        {
            return minSize <= Size ? TryGet() : new TmphPointer();
        }

        /// <summary>
        ///     获取缓冲区
        /// </summary>
        /// <returns>缓冲区</returns>
        public TmphPointer Get()
        {
            var data = TryGet();
            return data.Data != null ? data : TmphUnmanaged.Get(Size, false);
        }

        /// <summary>
        ///     获取缓冲区
        /// </summary>
        /// <param name="minSize">数据字节长度</param>
        /// <returns>缓冲区</returns>
        public TmphPointer Get(int minSize)
        {
            return minSize <= Size ? Get() : TmphUnmanaged.Get(minSize, false);
        }

        /// <summary>
        ///     保存缓冲区
        /// </summary>
        /// <param name="TmphBuffer">缓冲区</param>
        public abstract void Push(ref TmphPointer TmphBuffer);

        /// <summary>
        ///     获取内存池[反射引用于 Laurent.Lee.CLB.unmanagedPoolProxy]
        /// </summary>
        /// <param name="size">缓冲区尺寸</param>
        /// <returns>内存池</returns>
        internal static TmphUnmanagedPool GetPool(int size)
        {
            if (size <= 0) TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
            TmphUnmanagedPool pool;
            TmphInterlocked.NoCheckCompareSetSleep0(ref _poolLock);
            if (Pools.TryGetValue(size, out pool)) _poolLock = 0;
            else
            {
                try
                {
                    Pools.Add(size,
                        pool =
                            TmphAppSetting.IsPoolDebug ? (TmphUnmanagedPool)new TmphDebugPool(size) : new TmphArrayPool(size));
                }
                finally
                {
                    _poolLock = 0;
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
            TmphInterlocked.NoCheckCompareSetSleep0(ref _poolLock);
            foreach (var pool in Pools.Values) pool.ClearCache(count);
            _poolLock = 0;
        }

        /// <summary>
        ///     获取临时缓冲区
        /// </summary>
        /// <param name="length">缓冲区字节长度</param>
        /// <returns>临时缓冲区</returns>
        public static TmphUnmanagedPool GetDefaultPool(int length)
        {
            return length <= TmphUnmanagedStreamBase.DefaultLength ? TinyBuffers : StreamBuffers;
        }

        ///// <summary>
        ///// 保存缓冲区
        ///// </summary>
        ///// <param name="data">缓冲区</param>
        //public abstract void Push(byte* data);
        /// <summary>
        ///     数组模式内存池
        /// </summary>
        private sealed class TmphArrayPool : TmphUnmanagedPool
        {
            /// <summary>
            ///     缓冲区集合
            /// </summary>
            private TmphArrayPool<TmphPointer> _pool;

            /// <summary>
            ///     内存池
            /// </summary>
            /// <param name="size">缓冲区尺寸</param>
            public TmphArrayPool(int size)
            {
                _pool = TmphArrayPool<TmphPointer>.Create();
                Size = size;
            }

            /// <summary>
            ///     非托管内存数量
            /// </summary>
            protected override int Count
            {
                get { return _pool.Count; }
            }

            /// <summary>
            ///     清除缓冲区集合
            /// </summary>
            /// <param name="cacheCount">保留清除缓冲区数量</param>
            protected override void ClearCache(int cacheCount)
            {
                foreach (var pointer in _pool.GetClear(cacheCount, false))
                {
                    try
                    {
                        TmphUnmanaged.Free(pointer.Data);
                    }
                    catch (Exception error)
                    {
                        TmphLog.Error.Add(error, null, false);
                    }
                }
            }

            /// <summary>
            ///     获取缓冲区
            /// </summary>
            /// <returns>缓冲区,失败返回null</returns>
            public override TmphPointer TryGet()
            {
                var value = default(TmphPointer);
                _pool.TryGet(ref value);
                return value;
            }

            /// <summary>
            ///     保存缓冲区
            /// </summary>
            /// <param name="TmphBuffer">缓冲区</param>
            public override void Push(ref TmphPointer TmphBuffer)
            {
                var data = TmphBuffer.Data;
                TmphBuffer.Data = null;
                if (data != null) _pool.Push(new TmphPointer { Data = data });
            }
        }

        /// <summary>
        ///     纠错模式内存池
        /// </summary>
        private sealed class TmphDebugPool : TmphUnmanagedPool
        {
            /// <summary>
            ///     缓冲区集合
            /// </summary>
            private readonly HashSet<TmphPointer> _buffers;

            /// <summary>
            ///     缓冲区集合访问锁
            /// </summary>
            private int _bufferLock;

            /// <summary>
            ///     当前最大缓冲区数量
            /// </summary>
            private int _maxCount = TmphAppSetting.PoolSize;

            /// <summary>
            ///     内存池
            /// </summary>
            /// <param name="size">缓冲区尺寸</param>
            public TmphDebugPool(int size)
            {
                _buffers = TmphHashSet.CreatePointer();
                Size = size;
            }

            /// <summary>
            ///     非托管内存数量
            /// </summary>
            protected override int Count
            {
                get { return _buffers.Count; }
            }

            /// <summary>
            ///     清除缓冲区集合
            /// </summary>
            /// <param name="cacheCount">保留清除缓冲区数量</param>
            protected override void ClearCache(int cacheCount)
            {
                TmphInterlocked.NoCheckCompareSetSleep0(ref _bufferLock);
                TmphPointer[] removeBuffers;
                try
                {
                    removeBuffers = _buffers.GetArray();
                    _buffers.Clear();
                }
                finally
                {
                    _bufferLock = 0;
                }
                foreach (var pointer in removeBuffers)
                {
                    try
                    {
                        TmphUnmanaged.Free(pointer.Data);
                    }
                    catch (Exception error)
                    {
                        TmphLog.Error.Add(error, null, false);
                    }
                }
            }

            /// <summary>
            ///     获取缓冲区
            /// </summary>
            /// <returns>缓冲区,失败返回null</returns>
            public override TmphPointer TryGet()
            {
                var TmphBuffer = new TmphPointer();
                TmphInterlocked.NoCheckCompareSetSleep0(ref _bufferLock);
                foreach (var data in _buffers)
                {
                    TmphBuffer = data;
                    break;
                }
                if (TmphBuffer.Data != null) _buffers.Remove(TmphBuffer);
                _bufferLock = 0;
                return TmphBuffer;
            }

            /// <summary>
            ///     保存缓冲区
            /// </summary>
            /// <param name="TmphBuffer">缓冲区</param>
            public override void Push(ref TmphPointer TmphBuffer)
            {
                var data = TmphBuffer.Data;
                TmphBuffer.Data = null;
                if (data != null)
                {
                    bool isAdd, isMax = false;
                    TmphInterlocked.NoCheckCompareSetSleep0(ref _bufferLock);
                    try
                    {
                        if ((isAdd = _buffers.Add(new TmphPointer { Data = data })) && (isMax = _buffers.Count > _maxCount))
                        {
                            _maxCount <<= 1;
                        }
                    }
                    finally
                    {
                        _bufferLock = 0;
                    }
                    if (isAdd)
                    {
                        if (isMax)
                        {
                            TmphLog.Default.Add(
                                "非托管内存池扩展实例数量 byte*(" + Size.toString() + ")[" + _buffers.Count.toString() + "]");
                        }
                    }
                    else TmphLog.Error.Add("内存池释放冲突 " + Size.toString(), true);
                }
            }
        }
    }
}