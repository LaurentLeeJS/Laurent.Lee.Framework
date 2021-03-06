﻿/*
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
    /// <summary>
    ///     指针(因为指针无法静态初始化)
    /// </summary>
    public unsafe struct TmphPointer : IEquatable<TmphPointer>
    {
        /// <summary>
        ///     指针
        /// </summary>
        public void* Data;

        /// <summary>
        ///     字节指针
        /// </summary>
        public byte* Byte
        {
            get { return (byte*)Data; }
        }

        /// <summary>
        ///     字节指针
        /// </summary>
        public sbyte* SByte
        {
            get { return (sbyte*)Data; }
        }

        /// <summary>
        ///     字符指针
        /// </summary>
        public char* Char
        {
            get { return (char*)Data; }
        }

        /// <summary>
        ///     整形指针
        /// </summary>
        public short* Short
        {
            get { return (short*)Data; }
        }

        /// <summary>
        ///     整形指针
        /// </summary>
        public ushort* UShort
        {
            get { return (ushort*)Data; }
        }

        /// <summary>
        ///     整形指针
        /// </summary>
        public int* Int
        {
            get { return (int*)Data; }
        }

        /// <summary>
        ///     整形指针
        /// </summary>
        public uint* UInt
        {
            get { return (uint*)Data; }
        }

        /// <summary>
        ///     整形指针
        /// </summary>
        public long* Long
        {
            get { return (long*)Data; }
        }

        /// <summary>
        ///     整形指针
        /// </summary>
        public ulong* ULong
        {
            get { return (ulong*)Data; }
        }

        /// <summary>
        ///     浮点指针
        /// </summary>
        public float* Float
        {
            get { return (float*)Data; }
        }

        /// <summary>
        ///     双精度浮点指针
        /// </summary>
        public double* Double
        {
            get { return (double*)Data; }
        }

        /// <summary>
        ///     日期指针
        /// </summary>
        public DateTime* DateTime
        {
            get { return (DateTime*)Data; }
        }

        /// <summary>
        ///     指针比较
        /// </summary>
        /// <param name="other">待比较指针</param>
        /// <returns>指针是否相等</returns>
        public bool Equals(TmphPointer other)
        {
            return Data == other.Data;
        }

        /// <summary>
        ///     HASH值
        /// </summary>
        /// <returns>HASH值</returns>
        public override int GetHashCode()
        {
            if (TmphPub.MemoryBits == 64)
            {
                return (int)((long)Data >> 3) ^ (int)((long)Data >> 35);
            }
            return (int)Data >> 2;
        }

        /// <summary>
        ///     指针比较
        /// </summary>
        /// <param name="obj">待比较指针</param>
        /// <returns>指针是否相等</returns>
        public override bool Equals(object obj)
        {
            return Equals((TmphPointer)obj);
        }
    }
}