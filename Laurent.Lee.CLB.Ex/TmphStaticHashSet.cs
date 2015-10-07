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