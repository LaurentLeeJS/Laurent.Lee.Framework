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
    ///     字符串Hash
    /// </summary>
    public struct TmphHashString : IEquatable<TmphHashString>, IEquatable<TmphSubString>, IEquatable<string>
    {
        /// <summary>
        ///     哈希值
        /// </summary>
        private int hashCode;

        /// <summary>
        ///     字符串
        /// </summary>
        private TmphSubString value;

        /// <summary>
        ///     字符串Hash
        /// </summary>
        /// <param name="value"></param>
        public TmphHashString(TmphSubString value)
        {
            this.value = value;
            hashCode = value == null ? 0 : (value.GetHashCode() ^ TmphRandom.Hash);
        }

        /// <summary>
        ///     字符串长度
        /// </summary>
        internal int Length
        {
            get { return value.Length; }
        }

        /// <summary>
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(TmphSubString other)
        {
            return value.Equals(other);
        }

        /// <summary>
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(string other)
        {
            return value.Equals(other);
        }

        /// <summary>
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(TmphHashString other)
        {
            return hashCode == other.hashCode && value.Equals(other.value);
        }

        /// <summary>
        ///     清空数据
        /// </summary>
        internal void Null()
        {
            value.Null();
            hashCode = 0;
        }

        /// <summary>
        ///     隐式转换
        /// </summary>
        /// <param name="value">字符串</param>
        /// <returns>字符串</returns>
        public static implicit operator TmphHashString(string value)
        {
            return new TmphHashString(value);
        }

        /// <summary>
        ///     隐式转换
        /// </summary>
        /// <param name="value">字符串</param>
        /// <returns>字符串</returns>
        public static implicit operator TmphHashString(TmphSubString value)
        {
            return new TmphHashString(value);
        }

        /// <summary>
        ///     隐式转换
        /// </summary>
        /// <param name="value">字符串</param>
        /// <returns>字符串</returns>
        public static implicit operator TmphSubString(TmphHashString value)
        {
            return value.value;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return hashCode;
        }

        /// <summary>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return Equals((TmphHashString)obj);
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return value == null ? null : value.ToString();
        }
    }
}