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

namespace Laurent.Lee.CLB.Algorithm
{
    public static class TmphHashCode
    {
        public static unsafe int GetHashCode32(byte[] data)
        {
            if (data != null && data.Length != 0)
            {
                fixed (byte* fixedData = data)
                    return GetHashCode32(fixedData, data.Length);
            }
            return 0;
        }

        internal static unsafe int GetHashCode32(void* data, int length)
        {
            if (TmphPub.MemoryBits == 64)
            {
                long value = GetHashCode64((byte*)data, length);
                return (int)(value ^ (value >> 32));
            }
            if (length >= sizeof(uint))
            {
                uint value = *(uint*)data;
                byte* start = (byte*)data;
                for (byte* end = start + (length & (int.MaxValue - sizeof(uint) + 1)); (start += sizeof(uint)) != end; value ^= *(uint*)start)
                    value = (value << 13) | (value >> 19);
                if ((length & (sizeof(uint) - 1)) != 0)
                {
                    value = (value << 13) | (value >> 19);
                    value ^= *(uint*)start << ((sizeof(uint) - (length & (sizeof(uint) - 1))) << 3);
                }
                return (int)value ^ length;
            }
            return (int)(*(uint*)data << ((sizeof(uint) - length) << 3)) ^ length;
        }

        public static unsafe long GetHashCode64(byte[] data)
        {
            if (data != null && data.Length != 0)
            {
                fixed (byte* fixedData = data)
                    return GetHashCode64(fixedData, data.Length);
            }
            return 0;
        }

        private static unsafe long GetHashCode64(byte* start, int length)
        {
            if (length >= sizeof(ulong))
            {
                ulong value = *(ulong*)start;
                for (byte* end = start + (length & (int.MaxValue - sizeof(ulong) + 1));
                    (start += sizeof(ulong)) != end;
                    value ^= *(ulong*)start)
                {
                    value = (value << 53) | (value >> 11);
                }
                if ((length & (sizeof(ulong) - 1)) != 0)
                {
                    value = (value << 53) | (value >> 11);
                    value ^= *(ulong*)start << ((sizeof(ulong) - (length & (sizeof(ulong) - 1))) << 3);
                }
                return (long)value ^ ((long)length << 19);
            }
            return (long)(*(ulong*)start << ((sizeof(ulong) - length) << 3)) ^ ((long)length << 19);
        }
    }
}