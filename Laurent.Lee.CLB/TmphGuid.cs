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