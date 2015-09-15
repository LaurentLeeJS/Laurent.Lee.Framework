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

namespace Laurent.Lee.CLB
{
    public unsafe struct TmphFixedMap
    {
        private readonly byte* map;

        public TmphFixedMap(void* map)
        {
            this.map = (byte*)map;
        }

        public TmphFixedMap(TmphPointer map)
        {
            this.map = map.Byte;
        }

        public TmphFixedMap(void* map, int length, byte value = 0)
        {
            this.map = (byte*)map;
            TmphMemory.Fill(map, value, length);
        }

        public TmphUnsafer Unsafer
        {
            get { return new TmphUnsafer(map); }
        }

        public byte* Map
        {
            get { return map; }
        }

        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public void Set(int bit)
        {
            map[bit >> 3] |= (byte)(1 << (bit & 7));
        }

        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public bool Get(int bit)
        {
            return (map[bit >> 3] & (1 << (bit & 7))) != 0;
        }

        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public bool Get(char bit)
        {
            return (map[bit >> 3] & (1 << (bit & 7))) != 0;
        }

        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public void Set(int start, int count)
        {
            if (start < 0)
            {
                count += start;
                start = 0;
            }
            if (count > 0) CLB.Unsafe.TmphMemory.FillBits(map, start, count);
        }

        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public void Clear(int start, int count)
        {
            if (start < 0)
            {
                count += start;
                start = 0;
            }
            if (count > 0) CLB.Unsafe.TmphMemory.ClearBits(map, start, count);
        }

        public struct TmphUnsafer
        {
            private readonly byte* map;

            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            public TmphUnsafer(byte* map)
            {
                this.map = map;
            }

            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            public void Set(int bit)
            {
                map[bit >> 3] |= (byte)(1 << (bit &= 7));
            }

            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            public bool Get(int bit)
            {
                return (map[bit >> 3] & (byte)(1 << (bit &= 7))) != 0;
            }

            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            public void Set(int start, int count)
            {
                CLB.Unsafe.TmphMemory.FillBits(map, start, count);
            }

            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            public void Clear(int start, int count)
            {
                CLB.Unsafe.TmphMemory.ClearBits(map, start, count);
            }
        }
    }

    public unsafe struct TmphFixedMap<TEnumType> where TEnumType : IConvertible
    {
        private readonly byte* map;
        private readonly uint size;

        public TmphFixedMap(byte* map, uint size)
        {
            if (map == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            this.map = map;
            this.size = size;
        }

        public TmphFixedMap.TmphUnsafer Unsafer
        {
            get { return new TmphFixedMap.TmphUnsafer(map); }
        }

        public byte* Map
        {
            get { return map; }
        }

        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public void Set(int bit)
        {
            if ((uint)bit < size) map[bit >> 3] |= (byte)(1 << (bit &= 7));
        }

        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public void Set(TEnumType value)
        {
            Set(value.ToInt32(null));
        }

        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public void Clear(int bit)
        {
            if ((uint)bit < size) map[bit >> 3] &= (byte)(0xff - (1 << (bit &= 7)));
        }

        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public void Clear(TEnumType value)
        {
            Clear(value.ToInt32(null));
        }

        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public bool Get(int bit)
        {
            return (uint)bit < size && (map[bit >> 3] & (byte)(1 << (bit &= 7))) != 0;
        }

        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public bool Get(TEnumType value)
        {
            return Get(value.ToInt32(null));
        }
    }
}