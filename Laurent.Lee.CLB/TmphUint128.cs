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

using Laurent.Lee.CLB.Emit;
using System;
using System.Runtime.InteropServices;

namespace Laurent.Lee.CLB
{
    [StructLayout(LayoutKind.Explicit)]
    public struct TmphUint128 : IEquatable<TmphUint128>
    {
        [FieldOffset(0)]
        private uint bit0;

        [FieldOffset(sizeof(uint))]
        private uint bit32;

        [FieldOffset(sizeof(ulong))]
        private uint bit64;

        [FieldOffset(sizeof(ulong) + sizeof(uint))]
        private uint bit96;

        [FieldOffset(0)]
        public ulong Low;

        [FieldOffset(sizeof(ulong))]
        public ulong High;

        public bool Equals(TmphUint128 other)
        {
            return ((Low ^ other.Low) | (High ^ other.High)) == 0;
        }

        public override int GetHashCode()
        {
            return (int)(bit0 ^ bit32 ^ bit64 ^ bit96);
        }

        public override bool Equals(object obj)
        {
            return Equals((TmphUint128)obj);
        }

        internal unsafe void ParseHex(byte* data)
        {
            var next = data + 8;
            bit96 = TmphNumber.ParseHex(data, next);
            bit64 = TmphNumber.ParseHex(next, data += 16);
            bit32 = TmphNumber.ParseHex(data, next += 16);
            bit0 = TmphNumber.ParseHex(next, data + 16);
        }

        internal unsafe byte[] ToHex()
        {
            var data = new byte[32];
            fixed (byte* dataFixed = data)
            {
                TmphNumber.ToHex(bit96, dataFixed);
                TmphNumber.ToHex(bit64, dataFixed + 8);
                TmphNumber.ToHex(bit32, dataFixed + 16);
                TmphNumber.ToHex(bit0, dataFixed + 24);
            }
            return data;
        }

        [TmphDataSerialize.TmphCustom]
        private static unsafe void serialize(TmphDataSerializer serializer, TmphUint128 value)
        {
            var stream = serializer.Stream;
            stream.PrepLength(sizeof(ulong) + sizeof(ulong));
            var write = stream.CurrentData;
            *(ulong*)write = value.Low;
            *(ulong*)(write + sizeof(ulong)) = value.High;
            stream.Unsafer.AddLength(sizeof(ulong) + sizeof(ulong));
        }

        /// <summary>
        ///     反序列化
        /// </summary>
        /// <param name="parentDeSerializer">对象反序列化器</param>
        [TmphDataSerialize.TmphCustom]
        private static unsafe void deSerialize(TmphDataDeSerializer deSerializer, ref TmphUint128 value)
        {
            if (deSerializer.VerifyRead(sizeof(ulong) + sizeof(ulong)))
            {
                var dataStart = deSerializer.Read;
                value.Low = *(ulong*)(dataStart - sizeof(ulong) * 2);
                value.High = *(ulong*)(dataStart - sizeof(ulong));
            }
        }
    }
}