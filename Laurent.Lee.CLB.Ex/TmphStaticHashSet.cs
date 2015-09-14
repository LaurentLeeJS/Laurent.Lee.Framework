using System;
using System.Collections.Generic;

namespace Laurent.Lee.CLB
{
    public sealed class TmphStaticHashSet<TValueType> : TmphStaticHash<TValueType> where TValueType : IEquatable<TValueType>
    {
        public unsafe TmphStaticHashSet(TValueType[] values)
        {
            if (values.length() != 0)
            {
                TmphUnmanagedPool pool = CLB.TmphUnmanagedPool.GetDefaultPool(values.Length * sizeof(int));
                TmphPointer data = pool.Get(values.Length * sizeof(int));
                try
                {
                    getValues(values, data.Int);
                }
                finally { pool.Push(ref data); }
            }
            else
            {
                Indexs = DefaultIndexs;
                Array = TmphNullValue<TValueType>.Array;
            }
        }

        public TmphStaticHashSet(IEnumerable<TValueType> values) : this(values.getArray())
        {
        }

        public TmphStaticHashSet(ICollection<TValueType> values) : this(values.getArray())
        {
        }

        public TmphStaticHashSet(TmphList<TValueType> values) : this(values.toArray())
        {
        }

        public TmphStaticHashSet(TmphCollection<TValueType> values) : this(values.toArray())
        {
        }

        private unsafe void getValues(TValueType[] values, int* hashs)
        {
            int* hash = hashs;
            foreach (TValueType value in values) *hash++ = GetHashCode(value);
            Array = base.GetValues(values, hashs);
        }

        public bool Contains(TValueType value)
        {
            for (TmphRange range = Indexs[GetHashCode(value) & IndexAnd]; range.StartIndex != range.EndIndex; ++range.StartIndex)
            {
                if (Array[range.StartIndex].Equals(value)) return true;
            }
            return false;
        }

        public bool Remove(TValueType value)
        {
            int index = (GetHashCode(value) & IndexAnd);
            for (TmphRange range = Indexs[index]; range.StartIndex != range.EndIndex; ++range.StartIndex)
            {
                TValueType nextValue = Array[range.StartIndex];
                if (value.Equals(nextValue))
                {
                    Indexs[index].EndIndex = --range.EndIndex;
                    Array[range.StartIndex] = Array[range.EndIndex];
                    return true;
                }
            }
            return false;
        }
    }
}