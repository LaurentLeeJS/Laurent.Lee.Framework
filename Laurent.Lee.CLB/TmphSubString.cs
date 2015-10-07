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

using Laurent.Lee.CLB.Algorithm;
using System;

namespace Laurent.Lee.CLB
{
    public unsafe struct TmphSubString : IEquatable<TmphSubString>, IEquatable<string>
    {
        private static readonly TmphPointer trimMap = new TmphString.TmphAsciiMap(TmphUnmanaged.Get(TmphString.TmphAsciiMap.MapBytes, true), " \t\r", true).Pointer;
        private int length;
        private int startIndex;
        internal string value;

        public TmphSubString(string value)
        {
            this.value = value;
            startIndex = 0;
            length = value.Length();
        }

        public TmphSubString(string value, int startIndex)
        {
            if (value == null)
                TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            length = value.Length - (this.startIndex = startIndex);
            if (length < 0 || startIndex < 0)
                TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
            this.value = length != 0 ? value : string.Empty;
        }

        public TmphSubString(string value, int startIndex, int length)
        {
            if (value == null)
                TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            var range = new TmphArray.TmphRange(value.Length, startIndex, length);
            if (range.GetCount != length)
                TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
            if (range.GetCount != 0)
            {
                this.value = value;
                this.startIndex = range.SkipCount;
                this.length = range.GetCount;
            }
            else
            {
                this.value = string.Empty;
                this.startIndex = this.length = 0;
            }
        }

        public string Value
        {
            get { return value; }
        }

        public int StartIndex
        {
            get { return startIndex; }
        }

        public int Length
        {
            get { return length; }
        }

        public char this[int index]
        {
            get { return value[index + startIndex]; }
        }

        public bool Equals(TmphSubString other)
        {
            if (value == null)
                return other == null;
            if (length == other.length)
            {
                if (value == other.value && startIndex == other.startIndex)
                    return true;
                fixed (char* valueFixed = value, otherFixed = other.value)
                {
                    return CLB.Unsafe.TmphMemory.Equal(valueFixed + startIndex, otherFixed + other.startIndex, length << 1);
                }
            }
            return false;
        }

        public bool Equals(string other)
        {
            if (value == null)
                return other == null;
            if (other != null && length == other.Length)
            {
                if (value == other && startIndex == 0)
                    return true;
                fixed (char* valueFixed = value, otherFixed = other)
                {
                    return CLB.Unsafe.TmphMemory.Equal(valueFixed + startIndex, otherFixed, length << 1);
                }
            }
            return false;
        }

        public static implicit operator TmphSubString(string value)
        {
            return value != null ? Unsafe(value, 0, value.Length) : default(TmphSubString);
        }

        public static implicit operator string (TmphSubString value)
        {
            return value.ToString();
        }

        public void Null()
        {
            value = null;
            startIndex = length = 0;
        }

        public int IndexOf(char value)
        {
            if (length != 0)
            {
                fixed (char* valueFixed = this.value)
                {
                    char* start = valueFixed + startIndex, find = CLB.Unsafe.TmphString.Find(start, start + length, value);
                    if (find != null)
                        return (int)(find - start);
                }
            }
            return -1;
        }

        public TmphSubString Substring(int startIndex)
        {
            return new TmphSubString(value, this.startIndex + startIndex, length - startIndex);
        }

        public TmphSubString Substring(int startIndex, int length)
        {
            return new TmphSubString(value, this.startIndex + startIndex, length);
        }

        public TmphSubString Trim()
        {
            if (length != 0)
            {
                fixed (char* valueFixed = value)
                {
                    char* start = valueFixed + startIndex, end = start + length;
                    start = CLB.Unsafe.TmphString.findNotAscii(start, end, trimMap.Byte);
                    if (start == null)
                        return new TmphSubString(string.Empty);
                    end = CLB.Unsafe.TmphString.findLastNotAscii(start, end, trimMap.Byte);
                    if (end == null)
                        return new TmphSubString(string.Empty);
                    return Unsafe(value, (int)(start - valueFixed), (int)(end - start));
                }
            }
            return this;
        }

        public TmphSubArray<TmphSubString> Split(char split)
        {
            return value.Split(startIndex, length, split);
        }

        public bool StartsWith(string value)
        {
            if (value == null)
                TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            if (length >= value.Length)
            {
                fixed (char* valueFixed = this.value, cmpFixed = value)
                {
                    return CLB.Unsafe.TmphMemory.Equal(valueFixed + startIndex, cmpFixed, value.Length << 1);
                }
            }
            return false;
        }

        public override int GetHashCode()
        {
            if (length == 0) return 0;
            fixed (char* valueFixed = value)
                return TmphHashCode.GetHashCode32(valueFixed + startIndex, length << 1);
        }

        public override bool Equals(object obj)
        {
            return Equals((TmphSubString)obj);
        }

        public override string ToString()
        {
            if (value != null)
            {
                if (startIndex == 0 && length == value.Length)
                    return value;
                fixed (char* valueFixed = value)
                    return new string(valueFixed, startIndex, length);
            }
            return null;
        }

        internal void UnsafeSet(string value, int startIndex, int length)
        {
            this.value = value;
            this.startIndex = startIndex;
            this.length = length;
        }

        internal void UnsafeSetLength(int length)
        {
            this.length = length;
        }

        internal void UnsafeSet(int startIndex, int length)
        {
            this.startIndex = startIndex;
            this.length = length;
        }

        public static TmphSubString Unsafe(string value, int startIndex)
        {
            return new TmphSubString { value = value, startIndex = startIndex, length = value.Length - startIndex };
        }

        public static TmphSubString Unsafe(string value, int startIndex, int length)
        {
            return new TmphSubString { value = value, startIndex = startIndex, length = length };
        }
    }
}