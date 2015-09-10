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