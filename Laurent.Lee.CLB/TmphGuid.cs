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
using System.Runtime.InteropServices;

namespace Laurent.Lee.CLB
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct TmphGuid
    {
        [FieldOffset(0)]
        public Guid Value;

        [FieldOffset(0)]
        internal byte Byte0;

        [FieldOffset(1)]
        internal byte Byte1;

        [FieldOffset(2)]
        internal byte Byte2;

        [FieldOffset(3)]
        internal byte Byte3;

        [FieldOffset(4)]
        internal byte Byte4;

        [FieldOffset(5)]
        internal byte Byte5;

        [FieldOffset(4)]
        internal ushort Byte45;

        [FieldOffset(6)]
        internal byte Byte6;

        [FieldOffset(7)]
        internal byte Byte7;

        [FieldOffset(6)]
        internal ushort Byte67;

        [FieldOffset(8)]
        internal byte Byte8;

        [FieldOffset(9)]
        internal byte Byte9;

        [FieldOffset(10)]
        internal byte Byte10;

        [FieldOffset(11)]
        internal byte Byte11;

        [FieldOffset(12)]
        internal byte Byte12;

        [FieldOffset(13)]
        internal byte Byte13;

        [FieldOffset(14)]
        internal byte Byte14;

        [FieldOffset(15)]
        internal byte Byte15;

        public static unsafe byte[] ToByteArray(Guid guid)
        {
            var newGuid = new TmphGuid { Value = guid };
            var data = new byte[16];
            Unsafe.TmphMemory.Copy(&newGuid, data, 16);
            return data;
        }
    }
}