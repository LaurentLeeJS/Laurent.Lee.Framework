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
using System.Threading;

namespace Laurent.Lee.CLB
{
    /// <summary>
    ///     类型对象池
    /// </summary>
    public static class TmphTypePool
    {
        /// <summary>
        ///     类型对象池操作集合
        /// </summary>
        private static readonly Dictionary<Type, TmphPool> Pools = TmphDictionary.CreateOnly<Type, TmphPool>();

        /// <summary>
        ///     类型对象池操作集合访问锁
        /// </summary>
        private static int _poolLock;

        static TmphTypePool()
        {
            if (TmphAppSetting.IsCheckMemory) TmphCheckMemory.Add(MethodBase.GetCurrentMethod().DeclaringType);
        }

        /// <summary>
        ///     添加类型对象池
        /// </summary>
        /// <param name="type">对象类型</param>
        /// <param name="pool">类型对象池</param>
        internal static void Add(Type type, TmphPool pool)
        {
            TmphInterlocked.NoCheckCompareSetSleep0(ref _poolLock);
            try
            {
                Pools.Add(type, pool);
            }
            finally
            {
                _poolLock = 0;
            }
        }

        /// <summary>
        ///     清除类型对象池
        /// </summary>
        /// <param name="count">保留对象数量</param>
        public static void ClearPool(int count = 0)
        {
            if (count <= 0) count = 0;
            TmphInterlocked.NoCheckCompareSetSleep0(ref _poolLock);
            foreach (var pool in Pools.Values) pool.Clear(count);
            _poolLock = 0;
        }

        /// <summary>
        ///     对象池操作
        /// </summary>
        internal struct TmphPool
        {
            /// <summary>
            ///     清除对象池+保留对象数量
            /// </summary>
            public Action<int> Clear;
        }
    }

    /// <summary>
    ///     类型对象池
    /// </summary>
    /// <typeparam name="TValueType">对象类型</typeparam>
    public static class TmphTypePool<TValueType> where TValueType : class
    {
        /// <summary>
        ///     类型对象池
        /// </summary>
        private static TmphObjectPool<TValueType> _pool;

        /// <summary>
        ///     类型对象池
        /// </summary>
        private static readonly HashSet<TValueType> TmphDebugPool;

        /// <summary>
        ///     类型对象池访问锁
        /// </summary>
        private static int _poolLock;

        /// <summary>
        ///     类型对象池对象数量
        /// </summary>
        private static int _poolCount;

        static TmphTypePool()
        {
            var type = typeof(TValueType);
            if (TmphAppSetting.IsPoolDebug)
            {
                TmphDebugPool = TmphHashSet.CreateOnly<TValueType>();
                TmphTypePool.Add(type, new TmphTypePool.TmphPool { Clear = ClearDebug });
                _poolCount = TmphAppSetting.PoolSize;
            }
            else
            {
                _pool = TmphObjectPool<TValueType>.Create();
                TmphTypePool.Add(type, new TmphTypePool.TmphPool { Clear = ClearPool });
            }
        }

        /// <summary>
        ///     添加类型对象
        /// </summary>
        /// <param name="value">类型对象</param>
        public static void Push(TValueType value)
        {
            if (value != null) PushObject(value);
        }

        /// <summary>
        ///     添加类型对象
        /// </summary>
        /// <param name="value">类型对象</param>
        public static void Push(ref TValueType value)
        {
            var pushValue = Interlocked.Exchange(ref value, null);
            if (pushValue != null) PushObject(pushValue);
        }

        /// <summary>
        ///     添加类型对象
        /// </summary>
        /// <param name="value">类型对象</param>
        private static void PushObject(TValueType value)
        {
            if (TmphAppSetting.IsPoolDebug)
            {
                bool isAdd, isMax = false;
                TmphInterlocked.NoCheckCompareSetSleep0(ref _poolLock);
                try
                {
                    if ((isAdd = TmphDebugPool.Add(value)) && (isMax = TmphDebugPool.Count > _poolCount))
                    {
                        _poolCount <<= 1;
                    }
                }
                finally
                {
                    _poolLock = 0;
                }
                if (isAdd)
                {
                    if (isMax)
                    {
                        TmphLog.Default.Add("类型对象池扩展实例数量 " + typeof(TValueType).FullName + "[" + TmphDebugPool.Count.toString() + "]");
                    }
                }
                else TmphLog.Error.Add("对象池释放冲突 " + typeof(TValueType).FullName, true);
            }
            else _pool.Push(value);
        }

        /// <summary>
        ///     获取类型对象
        /// </summary>
        /// <returns>类型对象</returns>
        public static TValueType Pop()
        {
            if (TmphAppSetting.IsPoolDebug)
            {
                TValueType value = null;
                TmphInterlocked.NoCheckCompareSetSleep0(ref _poolLock);
                foreach (var poolValue in TmphDebugPool)
                {
                    value = poolValue;
                    break;
                }
                if (value != null) TmphDebugPool.Remove(value);
                _poolLock = 0;
                return value;
            }
            return _pool.Pop();
        }

        /// <summary>
        ///     对象数量
        /// </summary>
        /// <returns>对象数量</returns>
        public static int Count()
        {
            return TmphAppSetting.IsPoolDebug ? TmphDebugPool.Count : _pool.Count;
        }

        /// <summary>
        ///     清除对象池
        /// </summary>
        /// <param name="count">保留对象数量</param>
        internal static void Clear(int count = 0)
        {
            if (TmphAppSetting.IsPoolDebug) ClearDebug(0);
            else ClearPool(0);
        }

        /// <summary>
        ///     清除对象池
        /// </summary>
        /// <param name="count">保留对象数量</param>
        private static void ClearPool(int count)
        {
            _pool.Clear(count);
        }

        /// <summary>
        ///     清除对象池
        /// </summary>
        /// <param name="count">保留对象数量</param>
        private static void ClearDebug(int count)
        {
            ClearDebug(TmphDebugPool, ref _poolLock, count);
        }

        /// <summary>
        ///     清除对象池
        /// </summary>
        /// <param name="pool">类型对象池</param>
        /// <param name="poolLock">类型对象池访问锁</param>
        /// <param name="count">保留对象数量</param>
        internal static void ClearDebug(HashSet<TValueType> pool, ref int poolLock, int count)
        {
            TmphInterlocked.NoCheckCompareSetSleep0(ref poolLock);
            var removeCount = pool.Count - count;
            if (removeCount > 0)
            {
                TValueType[] removeValues;
                try
                {
                    removeValues = new TValueType[removeCount];
                    foreach (var value in pool)
                    {
                        removeValues[--removeCount] = value;
                        if (removeCount == 0) break;
                    }
                    foreach (var value in removeValues) pool.Remove(value);
                }
                finally
                {
                    poolLock = 0;
                }
                var dispose = TmphObjectPool<TValueType>.Dispose;
                if (dispose != null)
                {
                    foreach (var value in removeValues)
                    {
                        try
                        {
                            dispose(value);
                        }
                        catch (Exception error)
                        {
                            TmphLog.Default.Add(error, null, false);
                        }
                    }
                }
            }
            else poolLock = 0;
        }
    }

    /// <summary>
    ///     类型对象池
    /// </summary>
    /// <typeparam name="TMarkType">标识类型</typeparam>
    /// <typeparam name="TValueType">对象类型</typeparam>
    public abstract class TmphTypePool<TMarkType, TValueType> where TValueType : class
    {
        /// <summary>
        ///     类型对象池
        /// </summary>
        public static TmphTypePool<TMarkType, TValueType> Default = TmphAppSetting.IsPoolDebug
            ? (TmphTypePool<TMarkType, TValueType>)new TmphDebugPool()
            : new TmphArrayPool();

        /// <summary>
        ///     添加类型对象
        /// </summary>
        /// <param name="value">类型对象</param>
        public void Push(TValueType value)
        {
            if (value != null) PushObject(value);
        }

        /// <summary>
        ///     添加类型对象
        /// </summary>
        /// <param name="value">类型对象</param>
        public void Push(ref TValueType value)
        {
            var pushValue = Interlocked.Exchange(ref value, null);
            if (pushValue != null) PushObject(pushValue);
        }

        /// <summary>
        ///     添加类型对象
        /// </summary>
        /// <param name="value">类型对象</param>
        protected abstract void PushObject(TValueType value);

        /// <summary>
        ///     获取类型对象
        /// </summary>
        /// <returns>类型对象</returns>
        public abstract TValueType Pop();

        /// <summary>
        ///     数组模式对象池
        /// </summary>
        private sealed class TmphArrayPool : TmphTypePool<TMarkType, TValueType>
        {
            /// <summary>
            ///     类型对象池
            /// </summary>
            private TmphObjectPool<TValueType> _pool;

            /// <summary>
            ///     数组模式对象池
            /// </summary>
            public TmphArrayPool()
            {
                _pool = TmphObjectPool<TValueType>.Create();
                TmphTypePool.Add(typeof(TmphTypePool<TMarkType, TValueType>), new TmphTypePool.TmphPool { Clear = Clear });
            }

            /// <summary>
            ///     添加类型对象
            /// </summary>
            /// <param name="value">类型对象</param>
            protected override void PushObject(TValueType value)
            {
                _pool.Push(value);
            }

            /// <summary>
            ///     清除对象池
            /// </summary>
            /// <param name="count">保留对象数量</param>
            private void Clear(int count)
            {
                _pool.Clear(count);
            }

            /// <summary>
            ///     获取类型对象
            /// </summary>
            /// <returns>类型对象</returns>
            public override TValueType Pop()
            {
                return _pool.Pop();
            }
        }

        /// <summary>
        ///     纠错模式对象池
        /// </summary>
        private sealed class TmphDebugPool : TmphTypePool<TMarkType, TValueType>
        {
            /// <summary>
            ///     类型对象池
            /// </summary>
            private readonly HashSet<TValueType> _pool;

            /// <summary>
            ///     当前最大缓冲区数量
            /// </summary>
            private int _maxCount = TmphAppSetting.PoolSize;

            /// <summary>
            ///     类型对象池访问锁
            /// </summary>
            private int _poolLock;

            /// <summary>
            ///     纠错模式对象池
            /// </summary>
            public TmphDebugPool()
            {
                _pool = TmphHashSet.CreateOnly<TValueType>();
                TmphTypePool.Add(typeof(TmphTypePool<TMarkType, TValueType>), new TmphTypePool.TmphPool { Clear = Clear });
            }

            /// <summary>
            ///     添加类型对象
            /// </summary>
            /// <param name="value">类型对象</param>
            protected override void PushObject(TValueType value)
            {
                bool isAdd, isMax = false;
                TmphInterlocked.NoCheckCompareSetSleep0(ref _poolLock);
                try
                {
                    if ((isAdd = _pool.Add(value)) && (isMax = _pool.Count > _maxCount))
                    {
                        _maxCount <<= 1;
                    }
                }
                finally
                {
                    _poolLock = 0;
                }
                if (isAdd)
                {
                    if (isMax)
                    {
                        TmphLog.Default.Add(
                            "类型对象池扩展实例数量 " + typeof(TValueType).FullName + "[" + _pool.Count.toString() + "]@" +
                            typeof(TMarkType).FullName);
                    }
                }
                else
                    TmphLog.Error.Add("对象池释放冲突 " + typeof(TMarkType).FullName + " -> " + typeof(TValueType).FullName,
                        true);
            }

            /// <summary>
            ///     清除对象池
            /// </summary>
            /// <param name="count">保留对象数量</param>
            private void Clear(int count)
            {
                TmphTypePool<TValueType>.ClearDebug(_pool, ref _poolLock, count);
            }

            /// <summary>
            ///     获取类型对象
            /// </summary>
            /// <returns>类型对象</returns>
            public override TValueType Pop()
            {
                TValueType value = null;
                TmphInterlocked.NoCheckCompareSetSleep0(ref _poolLock);
                foreach (var poolValue in _pool)
                {
                    value = poolValue;
                    break;
                }
                if (value != null) _pool.Remove(value);
                _poolLock = 0;
                return value;
            }
        }
    }
}