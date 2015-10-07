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
using System.Reflection;
using System.Threading;

namespace Laurent.Lee.CLB
{
    public struct TmphObjectPool<TValueType> where TValueType : class
    {
        internal static readonly Action<TValueType> Dispose;
        private TmphArray.TmphValue<TValueType>[] array;
        private int arrayLock;
        internal int Count;
        private object newLock;

        static TmphObjectPool()
        {
            var type = typeof(TValueType);
            if (typeof(IDisposable).IsAssignableFrom(type))
            {
                Dispose = (Action<TValueType>)Delegate.CreateDelegate(typeof(Action<TValueType>), type.GetMethod("Dispose", BindingFlags.Public | BindingFlags.Instance, null, TmphNullValue<Type>.Array, null));
            }
        }

        public void Push(TValueType value)
        {
            PUSH:
            TmphInterlocked.NoCheckCompareSetSleep0(ref arrayLock);
            if (Count == array.Length)
            {
                var length = Count;
                arrayLock = 0;
                Monitor.Enter(newLock);
                if (length == array.Length)
                {
                    try
                    {
                        var newArray = new TmphArray.TmphValue<TValueType>[length << 1];
                        TmphInterlocked.NoCheckCompareSetSleep0(ref arrayLock);
                        Array.Copy(array, 0, newArray, 0, Count);
                        newArray[Count].Value = value;
                        array = newArray;
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
            array[Count++].Value = value;
            arrayLock = 0;
        }

        public TValueType Pop()
        {
            TmphInterlocked.NoCheckCompareSetSleep0(ref arrayLock);
            if (Count == 0)
            {
                arrayLock = 0;
                return null;
            }
            var value = array[--Count].Free();
            arrayLock = 0;
            return value;
        }

        internal void Clear(int count)
        {
            if (Dispose == null)
            {
                TmphInterlocked.NoCheckCompareSetSleep0(ref arrayLock);
                var length = Count - count;
                if (length > 0)
                {
                    Array.Clear(array, count, length);
                    Count = count;
                }
                arrayLock = 0;
            }
            else
            {
                foreach (var value in GetClear(count))
                {
                    try
                    {
                        Dispose(value.Value);
                    }
                    catch (Exception error)
                    {
                        TmphLog.Default.Add(error, null, false);
                    }
                }
            }
        }

        internal TmphArray.TmphValue<TValueType>[] GetClear(int count)
        {
            TmphInterlocked.NoCheckCompareSetSleep0(ref arrayLock);
            var length = Count - count;
            if (length > 0)
            {
                TmphArray.TmphValue<TValueType>[] removeBuffers;
                try
                {
                    removeBuffers = new TmphArray.TmphValue<TValueType>[length];
                    Array.Copy(array, Count = count, removeBuffers, 0, length);
                    Array.Clear(array, count, length);
                }
                finally
                {
                    arrayLock = 0;
                }
                return removeBuffers;
            }
            arrayLock = 0;
            return TmphNullValue<TmphArray.TmphValue<TValueType>>.Array;
        }

        public static TmphObjectPool<TValueType> Create()
        {
            return new TmphObjectPool<TValueType>
            {
                array = new TmphArray.TmphValue<TValueType>[TmphAppSetting.PoolSize],
                newLock = new object()
            };
        }

        public static TmphObjectPool<TValueType> Create(int size)
        {
            return new TmphObjectPool<TValueType>
            {
                array = new TmphArray.TmphValue<TValueType>[size <= 0 ? TmphAppSetting.PoolSize : size],
                newLock = new object()
            };
        }
    }
}