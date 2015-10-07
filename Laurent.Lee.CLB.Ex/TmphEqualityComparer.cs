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

using Laurent.Lee.CLB.Reflection;
using System;
using System.Collections.Generic;

namespace Laurent.Lee.CLB
{
    public static class TmphEqualityComparer
    {
        private sealed class TmphByteComparer : IEqualityComparer<byte>
        {
            public bool Equals(byte x, byte y)
            {
                return x == y;
            }

            public int GetHashCode(byte obj)
            {
                return obj;
            }
        }

        public static readonly IEqualityComparer<byte> Byte;

        private sealed class TmphShortComparer : IEqualityComparer<short>
        {
            public bool Equals(short x, short y)
            {
                return x == y;
            }

            public int GetHashCode(short obj)
            {
                return obj;
            }
        }

        public static readonly IEqualityComparer<short> Short;

        private sealed class intComparer : IEqualityComparer<int>
        {
            public bool Equals(int x, int y)
            {
                return x == y;
            }

            public int GetHashCode(int obj)
            {
                return obj;
            }
        }

        public static readonly IEqualityComparer<int> Int;

        private sealed class TmphUlongComparer : IEqualityComparer<ulong>
        {
            public bool Equals(ulong x, ulong y)
            {
                return x == y;
            }

            public int GetHashCode(ulong obj)
            {
                return (int)((uint)obj ^ (uint)(obj >> 32));
            }
        }

        public static readonly IEqualityComparer<ulong> ULong;

        private sealed class TmphCharComparer : IEqualityComparer<char>
        {
            public bool Equals(char x, char y)
            {
                return x == y;
            }

            /// <summary>
            public int GetHashCode(char obj) { return obj; }
        }

        public static readonly IEqualityComparer<char> Char;

        private unsafe sealed class TmphPointerComparer : IEqualityComparer<TmphPointer>
        {
            public bool Equals(TmphPointer x, TmphPointer y)
            {
                return x.Data == y.Data;
            }

            public int GetHashCode(TmphPointer obj)
            {
                return obj.GetHashCode();
            }
        }

        public static readonly IEqualityComparer<TmphPointer> Pointer;

        //private unsafe sealed class subStringComparer : IEqualityComparer<subString>
        //{
        //    public bool Equals(subString x, subString y) { return x.Equals(y); }
        //    /// <summary>
        //    ///
        //    /// </summary>
        //    /// <param name="obj"></param>
        //    /// <returns></returns>
        //    public int GetHashCode(subString obj) { return obj.GetHashCode(); }
        //}
        //public static readonly IEqualityComparer<subString> SubString;
        private unsafe sealed class TmphHashBytesComparer : IEqualityComparer<TmphHashBytes>
        {
            public bool Equals(TmphHashBytes x, TmphHashBytes y)
            {
                return x.Equals(y);
            }

            public int GetHashCode(TmphHashBytes obj)
            {
                return obj.GetHashCode();
            }
        }

        public static readonly IEqualityComparer<TmphHashBytes> HashBytes;

        private unsafe sealed class TmphHashStringComparer : IEqualityComparer<TmphHashString>
        {
            public bool Equals(TmphHashString x, TmphHashString y)
            {
                return x.Equals(y);
            }

            public int GetHashCode(TmphHashString obj)
            {
                return obj.GetHashCode();
            }
        }

        public static readonly IEqualityComparer<TmphHashString> HashString;

        private sealed class TmphEquatable<TValueType> : IEqualityComparer<TValueType> where TValueType : IEquatable<TValueType>
        {
            public bool Equals(TValueType x, TValueType y)
            {
                return x == null ? y == null : x.Equals(y);
            }

            public int GetHashCode(TValueType obj)
            {
                return obj != null ? obj.GetHashCode() : 0;
            }
        }

        private sealed class TmphComparable<TValueType> : IEqualityComparer<TValueType> where TValueType : IComparable<TValueType>
        {
            public bool Equals(TValueType x, TValueType y)
            {
                return x == null ? y == null : x.CompareTo(y) == 0;
            }

            public int GetHashCode(TValueType obj)
            {
                return obj != null ? obj.GetHashCode() : 0;
            }
        }

        public static class TmphComparer<TValueType>
        {
            public static readonly IEqualityComparer<TValueType> Default;

            private sealed class TmphUnknownComparer : IEqualityComparer<TValueType>
            {
                public bool Equals(TValueType x, TValueType y)
                {
                    return x == null ? y == null : x.Equals(y);
                }

                public int GetHashCode(TValueType obj)
                {
                    return obj != null ? obj.GetHashCode() : 0;
                }
            }

            private sealed class TmphUnknownNotNullComparer : IEqualityComparer<TValueType>
            {
                public bool Equals(TValueType x, TValueType y)
                {
                    return x.Equals(y);
                }

                public int GetHashCode(TValueType obj)
                {
                    return obj.GetHashCode();
                }
            }

            static TmphComparer()
            {
                Type type = typeof(TValueType);
                object comparer;
                if (!comparers.TryGetValue(type, out comparer))
                {
                    if (typeof(IEquatable<TValueType>).IsAssignableFrom(type))
                    {
                        Default = (IEqualityComparer<TValueType>)Activator.CreateInstance(typeof(TmphEquatable<>).MakeGenericType(type));
                    }
                    else if (typeof(IComparable<TValueType>).IsAssignableFrom(type))
                    {
                        Default = (IEqualityComparer<TValueType>)Activator.CreateInstance(typeof(TmphComparable<>).MakeGenericType(type));
                    }
                    else Default = type.isStruct() ? (IEqualityComparer<TValueType>)new TmphUnknownNotNullComparer() : new TmphUnknownComparer();
                }
                else Default = (IEqualityComparer<TValueType>)comparer;
            }
        }

        private static readonly Dictionary<Type, object> comparers;

        static TmphEqualityComparer()
        {
            comparers = TmphDictionary.CreateOnly<Type, object>();
            comparers.Add(typeof(byte), Byte = new TmphByteComparer());
            comparers.Add(typeof(short), Short = new TmphShortComparer());
            comparers.Add(typeof(int), Int = new intComparer());
            comparers.Add(typeof(char), Char = new TmphCharComparer());
            comparers.Add(typeof(ulong), ULong = new TmphUlongComparer());
            comparers.Add(typeof(TmphPointer), Pointer = new TmphPointerComparer());
            //comparers.Add(typeof(TmphSubString), SubString = new subStringComparer());
            comparers.Add(typeof(TmphHashBytes), HashBytes = new TmphHashBytesComparer());
            comparers.Add(typeof(TmphHashString), HashString = new TmphHashStringComparer());
        }
    }
}