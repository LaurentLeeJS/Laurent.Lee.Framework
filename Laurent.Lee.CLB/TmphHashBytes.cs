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
    /// <summary>
    ///     用于HASH的字节数组
    /// </summary>
    public struct TmphHashBytes : IEquatable<TmphHashBytes>, IEquatable<TmphSubArray<byte>>, IEquatable<byte[]>
    {
        /// <summary>
        ///     字节数组
        /// </summary>
        private TmphSubArray<byte> data;

        /// <summary>
        ///     HASH值
        /// </summary>
        private int hashCode;

        /// <summary>
        ///     字节数组HASH
        /// </summary>
        /// <param name="data">字节数组</param>
        public unsafe TmphHashBytes(TmphSubArray<byte> data)
        {
            this.data = data;
            if (data.Count == 0) hashCode = 0;
            else
            {
                fixed (byte* dataFixed = data.Array)
                {
                    hashCode = Algorithm.TmphHashCode.GetHashCode32(dataFixed + data.StartIndex, data.Count) ^ TmphRandom.Hash;
                }
            }
        }

        /// <summary>
        ///     数组长度
        /// </summary>
        public int Length
        {
            get { return data.Count; }
        }

        /// <summary>
        ///     比较字节数组是否相等
        /// </summary>
        /// <param name="other">用于HASH的字节数组</param>
        /// <returns>是否相等</returns>
        public unsafe bool Equals(byte[] other)
        {
            if (data.Array == null) return other == null;
            if (other != null && data.Count == other.Length)
            {
                if (data.Array == other && data.StartIndex == 0) return true;
                fixed (byte* dataFixed = data.Array)
                    return Unsafe.TmphMemory.Equal(other, dataFixed + data.StartIndex, data.Count);
            }
            return false;
        }

        /// <summary>
        ///     比较字节数组是否相等
        /// </summary>
        /// <param name="other">用于HASH的字节数组</param>
        /// <returns>是否相等</returns>
        public unsafe bool Equals(TmphSubArray<byte> other)
        {
            if (data.Array == null) return other.Array == null;
            if (other.Array != null && data.Count == other.Count)
            {
                if (data.Array == other.array && data.StartIndex == other.StartIndex) return true;
                fixed (byte* dataFixed = data.Array, otherDataFixed = other.Array)
                {
                    return Unsafe.TmphMemory.Equal(dataFixed + data.StartIndex, otherDataFixed + other.StartIndex,
                        data.Count);
                }
            }
            return false;
        }

        /// <summary>
        ///     比较字节数组是否相等
        /// </summary>
        /// <param name="other">用于HASH的字节数组</param>
        /// <returns>是否相等</returns>
        public unsafe bool Equals(TmphHashBytes other)
        {
            if (data.Array == null) return other.data.Array == null;
            if (other.data.Array != null && ((hashCode ^ other.hashCode) | (data.Count ^ other.data.Count)) == 0)
            {
                if (data.Array == other.data.array && data.StartIndex == other.data.StartIndex) return true;
                fixed (byte* dataFixed = data.Array, otherDataFixed = other.data.Array)
                {
                    return Unsafe.TmphMemory.Equal(dataFixed + data.StartIndex, otherDataFixed + other.data.StartIndex,
                        data.Count);
                }
            }
            return false;
        }

        /// <summary>
        ///     HASH字节数组隐式转换
        /// </summary>
        /// <param name="data">字节数组</param>
        /// <returns>HASH字节数组</returns>
        public static implicit operator TmphHashBytes(TmphSubArray<byte> data)
        {
            return new TmphHashBytes(data);
        }

        /// <summary>
        ///     HASH字节数组隐式转换
        /// </summary>
        /// <param name="data">字节数组</param>
        /// <returns>HASH字节数组</returns>
        public static implicit operator TmphHashBytes(byte[] data)
        {
            return
                new TmphHashBytes(data != null ? TmphSubArray<byte>.Unsafe(data, 0, data.Length) : default(TmphSubArray<byte>));
        }

        /// <summary>
        ///     HASH字节数组隐式转换
        /// </summary>
        /// <param name="value">HASH字节数组</param>
        /// <returns>字节数组</returns>
        public static implicit operator TmphSubArray<byte>(TmphHashBytes value)
        {
            return value.data;
        }

        /// <summary>
        ///     获取HASH值
        /// </summary>
        /// <returns>HASH值</returns>
        public override int GetHashCode()
        {
            return hashCode;
        }

        /// <summary>
        ///     比较字节数组是否相等
        /// </summary>
        /// <param name="other">字节数组HASH</param>
        /// <returns>是否相等</returns>
        public override bool Equals(object other)
        {
            return Equals((TmphHashBytes)other);
            //return other != null && other.GetType() == typeof(hashBytes) && Equals((hashBytes)other);
        }

        /// <summary>
        ///     复制HASH字节数组
        /// </summary>
        /// <returns>HASH字节数组</returns>
        public TmphHashBytes Copy()
        {
            if (this.data.Count == 0)
            {
                return new TmphHashBytes
                {
                    data = TmphSubArray<byte>.Unsafe(TmphNullValue<byte>.Array, 0, 0),
                    hashCode = hashCode
                };
            }
            var data = new byte[this.data.Count];
            Buffer.BlockCopy(this.data.array, this.data.StartIndex, data, 0, data.Length);
            return new TmphHashBytes { data = TmphSubArray<byte>.Unsafe(data, 0, data.Length), hashCode = hashCode };
        }
    }
}