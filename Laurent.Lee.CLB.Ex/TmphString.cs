using System;
using System.Text;

namespace Laurent.Lee.CLB
{
    public unsafe static class TmphStringExpand
    {
        public unsafe static char* Find(char* start, char* end, char value)
        {
            return start != null && end > start ? Unsafe.TmphString.Find(start, end, value) : null;
        }

        public unsafe static char* Find(char* start, char* end, Laurent.Lee.CLB.TmphString.TmphAsciiMap valueMap, char value)
        {
            return start != null && end > start ? Unsafe.TmphString.Find(start, end, valueMap, value) : null;
        }

        public unsafe static TmphSubArray<int> splitIntNoCheck(this string ints, char split)
        {
            TmphSubArray<int> values = new TmphSubArray<int>();
            if (ints != null && ints.Length != 0)
            {
                fixed (char* intPoint = ints)
                {
                    int intValue = 0;
                    for (char* next = intPoint, end = intPoint + ints.Length; next != end; ++next)
                    {
                        if (*next == split)
                        {
                            values.Add(intValue);
                            intValue = 0;
                        }
                        else
                        {
                            intValue *= 10;
                            intValue += *next;
                            intValue -= '0';
                        }
                    }
                    values.Add(intValue);
                }
            }
            return values;
        }

        public static byte[] getBytes(this string value, Encoding encoding)
        {
            return encoding != Encoding.ASCII ? encoding.GetBytes(value) : value.GetBytes();
        }

        public unsafe static string deSerialize(this byte[] data, int index, int length)
        {
            return toString(data, index, length, Encoding.ASCII);
        }

        public unsafe static string toString(this byte[] data, int index, int length, Encoding encoding)
        {
            if (encoding != Encoding.ASCII) return encoding.GetString(data, index, length);
            TmphArray.TmphRange range = new TmphArray.TmphRange(data.length(), index, length);
            if (range.GetCount == length)
            {
                if (range.GetCount != 0)
                {
                    fixed (byte* dataFixed = data) return Laurent.Lee.CLB.TmphString.DeSerialize(dataFixed + range.SkipCount, -range.GetCount);
                }
                return string.Empty;
            }
            TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
            return null;
        }

        public unsafe static byte[] getLowerBytes(this string value)
        {
            if (value == null) return null;
            if (value.Length == 0) return TmphNullValue<byte>.Array;
            byte[] newValue = new byte[value.Length];
            fixed (char* valueFixed = value)
            fixed (byte* newValueFixed = newValue)
            {
                Unsafe.TmphString.ToLower(valueFixed, valueFixed + value.Length, newValueFixed);
            }
            return newValue;
        }

        public unsafe static void ToLower(char* start, char* end)
        {
            if (start != null && end > start) Unsafe.TmphString.ToLower(start, end);
        }

        public unsafe static string toUpper(this string value)
        {
            if (value != null)
            {
                fixed (char* valueFixed = value) Unsafe.TmphString.ToUpper(valueFixed, valueFixed + value.Length);
            }
            return value;
        }

        public unsafe static void ToUpper(char* start, char* end)
        {
            if (start != null && end > start) Unsafe.TmphString.ToUpper(start, end);
        }

        public static unsafe int cmp(this string left, string right)
        {
            if (left != null && right != null)
            {
                int length = left.Length <= right.Length ? left.Length : right.Length;
                for (int index = 0, endIndex = Math.Min(length, 4); index != endIndex; ++index)
                {
                    int value = left[index] - right[index];
                    if (value != 0) return value;
                }
                if ((length -= 4) > 0)
                {
                    fixed (char* leftFixed = left, rightFixed = right)
                    {
                        int value = Cmp(leftFixed + 4, rightFixed + 4, length);
                        if (value != 0) return value;
                    }
                }
                return left.Length - right.Length;
            }
            if (left == right) return 0;
            return left != null ? 1 : -1;
        }

        private static unsafe int Cmp(char* left, char* right, int length)
        {
            while (length >= 8)
            {
                if (((*(uint*)left ^ *(uint*)right) | (*(uint*)(left + 4) ^ *(uint*)(right + 4))
                    | (*(uint*)(left + 8) ^ *(uint*)(right + 8)) | (*(uint*)(left + 12) ^ *(uint*)(right + 12))) != 0)
                {
                    if (((*(uint*)left ^ *(uint*)right) | (*(uint*)(left + 4) ^ *(uint*)(right + 4))) == 0)
                    {
                        left += 8;
                        right += 8;
                    }
                    if (*(uint*)left == *(uint*)right)
                    {
                        left += 4;
                        right += 4;
                    }
                    int value = (int)*(ushort*)left - *(ushort*)right;
                    return value == 0 ? (int)*(ushort*)(left += 2) - *(ushort*)(right += 2) : value;
                }
                length -= 8;
                left += 16;
                right += 16;
            }
            if ((length & 4) != 0)
            {
                if (((*(uint*)left ^ *(uint*)right) | (*(uint*)(left + 4) ^ *(uint*)(right + 4))) != 0)
                {
                    if ((*(uint*)left ^ *(uint*)right) == 0)
                    {
                        left += 4;
                        right += 4;
                    }
                    int value = (int)*(ushort*)left - *(ushort*)right;
                    return value == 0 ? (int)*(ushort*)(left += 2) - *(ushort*)(right += 2) : value;
                }
                left += 8;
                right += 8;
            }
            if ((length & 2) != 0)
            {
                int TmphCode = (int)*(ushort*)left - *(ushort*)right;
                if (TmphCode != 0) return TmphCode;
                TmphCode = (int)*(ushort*)(left + 2) - *(ushort*)(right + 2);
                if (TmphCode != 0) return TmphCode;
                left += 4;
                right += 4;
            }
            return (length & 1) == 0 ? 0 : ((int)*(ushort*)left - *(ushort*)right);
        }

        public static bool Trim(ref string value)
        {
            return value != null && (value = value.Trim()).Length != 0;
        }

        public unsafe static string getLeft(this string value, int count)
        {
            if (count <= 0) return string.Empty;
            if (value.Length << 1 > count)
            {
                fixed (char* valueFixed = value)
                {
                    char* start = valueFixed;
                    for (char* end = valueFixed + value.Length; start != end; ++start)
                    {
                        if (*start > 0xff)
                        {
                            if ((count -= 2) <= 0)
                            {
                                if (count == 0) ++start;
                                break;
                            }
                        }
                        else if (--count == 0)
                        {
                            ++start;
                            break;
                        }
                    }
                    count = (int)(start - valueFixed);
                }
                return count != value.Length ? value.Substring(0, count) : value;
            }
            return value;
        }

        public unsafe static string toHalf(this string value)
        {
            if (value != null)
            {
                fixed (char* valueFixed = value)
                {
                    for (char* start = valueFixed, end = valueFixed + value.Length; start != end; ++start)
                    {
                        int TmphCode = *start;
                        if ((uint)(TmphCode - 0xff01) <= 0xff5e - 0xff01) *start = (char)(TmphCode - 0xfee0);
                        else
                        {
                            switch (TmphCode)
                            {
                                case 0x2019:
                                case 0x2018:
                                    *start = '\''; break;
                                case 0x201c:
                                case 0x201d:
                                    *start = '"'; break;
                                case 0x3002:
                                    *start = '.'; break;
                                case 0xb7:
                                    *start = '@'; break;
                                    //default:
                                    //    if ((uint)(TmphCode - 0x2160) < 9) *start = (char)(TmphCode - 0x212f);
                                    //    break;
                            }
                        }
                    }
                }
            }
            return value;
        }

        public const string RegexEscape = @"^[-]{}()\|/?*+.$";
        private static readonly TmphPointer regexEscapeMap = new Laurent.Lee.CLB.TmphString.TmphAsciiMap(TmphUnmanaged.Get(TmphString.TmphAsciiMap.MapBytes, true), RegexEscape, true).Pointer;

        public unsafe static string toRegex(this string value)
        {
            if (value == null || value.Length == 0)
            {
                fixed (char* valueFixed = value)
                {
                    char* end = valueFixed + value.Length;
                    int count = Unsafe.TmphString.AsciiCount(valueFixed, end, regexEscapeMap.Byte, RegexEscape[0]);
                    if (count != 0)
                    {
                        TmphFixedMap map = new TmphFixedMap(regexEscapeMap);
                        value = Laurent.Lee.CLB.TmphString.FastAllocateString(count += value.Length);
                        fixed (char* writeFixed = value)
                        {
                            for (char* start = valueFixed, write = writeFixed; start != end; *write++ = *start++)
                            {
                                if ((*start & 0xff80) == 0 && map.Get(*start)) *write++ = '\\';
                            }
                        }
                    }
                }
            }
            return value;
        }

        public static unsafe string pinyinToLetter(this string value)
        {
            if (value != null)
            {
                fixed (char* valueFixed = value)
                {
                    for (char* start = valueFixed, end = valueFixed + value.Length, pinyinFixed = pinyins.Char; start != end; ++start)
                    {
                        if ((uint)(*start - 224) <= (476 - 224)) *start = pinyinFixed[*start - 224];
                    }
                }
            }
            return value;
        }

        private static TmphPointer pinyins;

        static TmphStringExpand()
        {
            pinyins = TmphUnmanaged.Get((476 - 224 + 1) * sizeof(char), true);
            char* pinyinData = pinyins.Char;
            pinyinData['ā' - 224] = 'a';
            pinyinData['á' - 224] = 'a';
            pinyinData['ǎ' - 224] = 'a';
            pinyinData['à' - 224] = 'a';
            pinyinData['ē' - 224] = 'e';
            pinyinData['é' - 224] = 'e';
            pinyinData['ě' - 224] = 'e';
            pinyinData['è' - 224] = 'e';
            pinyinData['ī' - 224] = 'i';
            pinyinData['í' - 224] = 'i';
            pinyinData['ǐ' - 224] = 'i';
            pinyinData['ì' - 224] = 'i';
            pinyinData['ō' - 224] = 'o';
            pinyinData['ó' - 224] = 'o';
            pinyinData['ǒ' - 224] = 'o';
            pinyinData['ò' - 224] = 'o';
            pinyinData['ū' - 224] = 'u';
            pinyinData['ú' - 224] = 'u';
            pinyinData['ǔ' - 224] = 'u';
            pinyinData['ù' - 224] = 'u';
            pinyinData['ǘ' - 224] = 'v';
            pinyinData['ǚ' - 224] = 'v';
            pinyinData['ǜ' - 224] = 'v';
        }
    }
}