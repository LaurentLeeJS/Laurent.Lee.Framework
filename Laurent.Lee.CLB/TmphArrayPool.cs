using Laurent.Lee.CLB.Config;
using Laurent.Lee.CLB.Threading;
using System.Threading;

namespace Laurent.Lee.CLB
{
    public struct TmphArrayPool<TValueType> 
    {
        private int arrayLock;
        private object newLock;
        internal TValueType[] Array;
        internal int Count;

        public void Push(TValueType value)
        {
            PUSH:
            TmphInterlocked.NoCheckCompareSetSleep0(ref arrayLock);
            if (Count == Array.Length)
            {
                var length = Count;
                arrayLock = 0;
                Monitor.Enter(newLock);
                if (length == Array.Length)
                {
                    try
                    {
                        var newArray = new TValueType[length << 1];
                        TmphInterlocked.NoCheckCompareSetSleep0(ref arrayLock);
                        System.Array.Copy(Array, 0, newArray, 0, Count);
                        newArray[Count] = value;
                        Array = newArray;
                        ++Count;
                        arrayLock = 0;
                    }
                    finally
                    {
                        Monitor.Exit(newLock);
                    }
                    return;
                }
                Monitor.Exit(newLock);
                goto PUSH;
            }
            Array[Count++] = value;
            arrayLock = 0;
        }

        public bool TryGet(ref TValueType value)
        {
            TmphInterlocked.NoCheckCompareSetSleep0(ref arrayLock);
            if (Count == 0)
            {
                arrayLock = 0;
                return false;
            }
            value = Array[--Count];
            arrayLock = 0;
            return true;
        }

        internal void Clear(int count, bool isClear)
        {
            TmphInterlocked.NoCheckCompareSetSleep0(ref arrayLock);
            var length = Count - count;
            if (length > 0)
            {
                if (isClear) System.Array.Clear(Array, count, length);
                Count = count;
            }
            arrayLock = 0;
        }

        internal TValueType[] GetClear(int count, bool isClear)
        {
            TmphInterlocked.NoCheckCompareSetSleep0(ref arrayLock);
            var length = Count - count;
            if (length > 0)
            {
                TValueType[] removeBuffers;
                try
                {
                    removeBuffers = new TValueType[length];
                    System.Array.Copy(Array, Count = count, removeBuffers, 0, length);
                    if (isClear) System.Array.Clear(Array, count, length);
                }
                finally
                {
                    arrayLock = 0;
                }
                return removeBuffers;
            }
            arrayLock = 0;
            return TmphNullValue<TValueType>.Array;
        }

        public static TmphArrayPool<TValueType> Create()
        {
            return CreatePool(TmphAppSetting.PoolSize);
        }

        public static TmphArrayPool<TValueType> Create(int size)
        {
            return CreatePool(size <= 0 ? TmphAppSetting.PoolSize : size);
        }

        private static TmphArrayPool<TValueType> CreatePool(int size)
        {
            return new TmphArrayPool<TValueType> { Array = new TValueType[size], newLock = new object() };
        }
    }
}