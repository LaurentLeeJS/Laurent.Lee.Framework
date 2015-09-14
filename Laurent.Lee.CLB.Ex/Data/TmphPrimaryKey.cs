using System;

namespace Laurent.Lee.CLB.Data
{
    /// <summary>
    ///     双关键字
    /// </summary>
    /// <typeparam name="TKeyType1">关键字类型1</typeparam>
    /// <typeparam name="TKeyType2">关键字类型2</typeparam>
    public struct TmphPrimaryKey<TKeyType1, TKeyType2> : IEquatable<TmphPrimaryKey<TKeyType1, TKeyType2>>,
        IComparable<TmphPrimaryKey<TKeyType1, TKeyType2>>
        where TKeyType1 : IEquatable<TKeyType1>, IComparable<TKeyType1>
        where TKeyType2 : IEquatable<TKeyType2>, IComparable<TKeyType2>
    {
        /// <summary>
        ///     关键字1
        /// </summary>
        public TKeyType1 Key1;

        /// <summary>
        ///     关键字2
        /// </summary>
        public TKeyType2 Key2;

        /// <summary>
        ///     关键字大小
        /// </summary>
        /// <param name="other">关键字</param>
        /// <returns>比较结果</returns>
        public int CompareTo(TmphPrimaryKey<TKeyType1, TKeyType2> other)
        {
            var cmp = Key1.CompareTo(other.Key1);
            return cmp != 0 ? cmp : Key2.CompareTo(other.Key2);
        }

        /// <summary>
        ///     关键字比较
        /// </summary>
        /// <param name="other">关键字</param>
        /// <returns>是否相等</returns>
        public bool Equals(TmphPrimaryKey<TKeyType1, TKeyType2> other)
        {
            return Key1.Equals(other.Key1) && Key2.Equals(other.Key2);
        }

        /// <summary>
        ///     哈希编码
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Key1.GetHashCode() ^ Key2.GetHashCode();
        }

        /// <summary>
        ///     关键字比较
        /// </summary>
        /// <param name="obj">关键字</param>
        /// <returns>是否相等</returns>
        public override bool Equals(object obj)
        {
            return Equals((TmphPrimaryKey<TKeyType1, TKeyType2>)obj);
        }
    }

    /// <summary>
    ///     3关键字
    /// </summary>
    /// <typeparam name="TKeyType1">关键字类型1</typeparam>
    /// <typeparam name="TKeyType2">关键字类型2</typeparam>
    /// <typeparam name="TKeyType3">关键字类型3</typeparam>
    public struct TmphPrimaryKey<TKeyType1, TKeyType2, TKeyType3> :
        IEquatable<TmphPrimaryKey<TKeyType1, TKeyType2, TKeyType3>>,
        IComparable<TmphPrimaryKey<TKeyType1, TKeyType2, TKeyType3>>
        where TKeyType1 : IEquatable<TKeyType1>, IComparable<TKeyType1>
        where TKeyType2 : IEquatable<TKeyType2>, IComparable<TKeyType2>
        where TKeyType3 : IEquatable<TKeyType3>, IComparable<TKeyType3>
    {
        /// <summary>
        ///     关键字1
        /// </summary>
        public TKeyType1 Key1;

        /// <summary>
        ///     关键字2
        /// </summary>
        public TKeyType2 Key2;

        /// <summary>
        ///     关键字3
        /// </summary>
        public TKeyType3 Key3;

        /// <summary>
        ///     关键字大小
        /// </summary>
        /// <param name="other">关键字</param>
        /// <returns>比较结果</returns>
        public int CompareTo(TmphPrimaryKey<TKeyType1, TKeyType2, TKeyType3> other)
        {
            var cmp = Key1.CompareTo(other.Key1);
            if (cmp == 0)
            {
                cmp = Key2.CompareTo(other.Key2);
                return cmp != 0 ? cmp : Key3.CompareTo(other.Key3);
            }
            return cmp;
        }

        /// <summary>
        ///     关键字比较
        /// </summary>
        /// <param name="other">关键字</param>
        /// <returns>是否相等</returns>
        public bool Equals(TmphPrimaryKey<TKeyType1, TKeyType2, TKeyType3> other)
        {
            return Key1.Equals(other.Key1) && Key2.Equals(other.Key2) && Key3.Equals(other.Key3);
        }

        /// <summary>
        ///     哈希编码
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Key1.GetHashCode() ^ Key2.GetHashCode() ^ Key3.GetHashCode();
        }

        /// <summary>
        ///     关键字比较
        /// </summary>
        /// <param name="obj">关键字</param>
        /// <returns>是否相等</returns>
        public override bool Equals(object obj)
        {
            return Equals((TmphPrimaryKey<TKeyType1, TKeyType2, TKeyType3>)obj);
        }
    }

    /// <summary>
    ///     4关键字
    /// </summary>
    /// <typeparam name="TKeyType1">关键字类型1</typeparam>
    /// <typeparam name="TKeyType2">关键字类型2</typeparam>
    /// <typeparam name="TKeyType3">关键字类型3</typeparam>
    /// <typeparam name="TKeyType4">关键字类型4</typeparam>
    public struct TmphPrimaryKey<TKeyType1, TKeyType2, TKeyType3, TKeyType4> :
        IEquatable<TmphPrimaryKey<TKeyType1, TKeyType2, TKeyType3, TKeyType4>>,
        IComparable<TmphPrimaryKey<TKeyType1, TKeyType2, TKeyType3, TKeyType4>>
        where TKeyType1 : IEquatable<TKeyType1>, IComparable<TKeyType1>
        where TKeyType2 : IEquatable<TKeyType2>, IComparable<TKeyType2>
        where TKeyType3 : IEquatable<TKeyType3>, IComparable<TKeyType3>
        where TKeyType4 : IEquatable<TKeyType4>, IComparable<TKeyType4>
    {
        /// <summary>
        ///     关键字1
        /// </summary>
        public TKeyType1 Key1;

        /// <summary>
        ///     关键字2
        /// </summary>
        public TKeyType2 Key2;

        /// <summary>
        ///     关键字3
        /// </summary>
        public TKeyType3 Key3;

        /// <summary>
        ///     关键字4
        /// </summary>
        public TKeyType4 Key4;

        /// <summary>
        ///     关键字大小
        /// </summary>
        /// <param name="other">关键字</param>
        /// <returns>比较结果</returns>
        public int CompareTo(TmphPrimaryKey<TKeyType1, TKeyType2, TKeyType3, TKeyType4> other)
        {
            var cmp = Key1.CompareTo(other.Key1);
            if (cmp == 0)
            {
                cmp = Key2.CompareTo(other.Key2);
                if (cmp == 0)
                {
                    cmp = Key3.CompareTo(other.Key3);
                    return cmp != 0 ? cmp : Key4.CompareTo(other.Key4);
                }
            }
            return cmp;
        }

        /// <summary>
        ///     关键字比较
        /// </summary>
        /// <param name="other">关键字</param>
        /// <returns>是否相等</returns>
        public bool Equals(TmphPrimaryKey<TKeyType1, TKeyType2, TKeyType3, TKeyType4> other)
        {
            return Key1.Equals(other.Key1) && Key2.Equals(other.Key2) && Key3.Equals(other.Key3) &&
                   Key4.Equals(other.Key4);
        }

        /// <summary>
        ///     哈希编码
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Key1.GetHashCode() ^ Key2.GetHashCode() ^ Key3.GetHashCode() ^ Key4.GetHashCode();
        }

        /// <summary>
        ///     关键字比较
        /// </summary>
        /// <param name="obj">关键字</param>
        /// <returns>是否相等</returns>
        public override bool Equals(object obj)
        {
            return Equals((TmphPrimaryKey<TKeyType1, TKeyType2, TKeyType3, TKeyType4>)obj);
        }
    }
}