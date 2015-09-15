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

namespace Laurent.Lee.CLB
{
    /// <summary>
    ///     内存或字节数组
    /// </summary>
    public static class TmphMemory
    {
        /// <summary>
        ///     填充字节
        /// </summary>
        /// <param name="src">串起始地址</param>
        /// <param name="value">字节值</param>
        /// <param name="length">字节数量</param>
        public static unsafe void Fill(void* src, byte value, int length)
        {
            if (src != null && length > 0) Unsafe.TmphMemory.Fill(src, value, length);
        }

        /// <summary>
        ///     复制字符数组
        /// </summary>
        /// <param name="source">原串起始地址</param>
        /// <param name="destination">目标串起始地址</param>
        /// <param name="count">字符数量</param>
        public static unsafe void Copy(char* source, char* destination, int count)
        {
            if (source != null && destination != null && count > 0)
                Unsafe.TmphMemory.Copy(source, (void*)destination, count << 1);
        }

        /// <summary>
        ///     复制字节数组
        /// </summary>
        /// <param name="source">原串起始地址</param>
        /// <param name="destination">目标串起始地址</param>
        /// <param name="length">字节长度</param>
        public static unsafe void Copy(void* source, void* destination, int length)
        {
            if (source != null && destination != null && length > 0) Unsafe.TmphMemory.Copy(source, destination, length);
        }

        /// <summary>
        ///     字节数组比较
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>是否相等</returns>
        public static bool equal(this byte[] left, byte[] right)
        {
            if (left == null) return right == null;
            return right != null && (left.Equals(right) || Unsafe.TmphMemory.Equal(left, right));
        }

        /// <summary>
        ///     字节数组比较
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>是否相等</returns>
        public static unsafe bool equal(this TmphSubArray<byte> left, TmphSubArray<byte> right)
        {
            if (left.Array == null) return right.Array == null;
            if (left.Count == right.Count)
            {
                fixed (byte* leftFixed = left.Array, rightFixed = right.Array)
                {
                    return Unsafe.TmphMemory.Equal(leftFixed + left.StartIndex, rightFixed + right.StartIndex, left.Count);
                }
            }
            return false;
        }

        /// <summary>
        ///     字节数组比较
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="count">比较字节数</param>
        /// <returns>是否相等</returns>
        public static bool equal(this byte[] left, byte[] right, int count)
        {
            if (left == null) return right == null;
            return right != null && left.Length >= count && right.Length >= count && count >= 0 &&
                   Unsafe.TmphMemory.Equal(left, right, count);
        }

        /// <summary>
        ///     字节数组比较
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="count">比较字节数</param>
        /// <returns>是否相等</returns>
        public static unsafe bool equal(this byte[] left, void* right, int count)
        {
            if (left == null) return right == null;
            return right != null && left.Length >= count && count >= 0 && Unsafe.TmphMemory.Equal(left, right, count);
        }

        /// 字节数组比较
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="count">比较字节数</param>
        /// <returns>是否相等</returns>
        public static unsafe bool Equal(void* left, void* right, int count)
        {
            if (left != null && right != null)
            {
                return count > 0 ? Unsafe.TmphMemory.Equal(left, right, count) : count == 0;
            }
            return left == null && right == null;
        }

        /// <summary>
        ///     查找字节
        /// </summary>
        /// <param name="data">字节数组</param>
        /// <param name="value">字节值</param>
        /// <returns>字节位置,失败为-1</returns>
        public static int indexOf(this byte[] data, byte value)
        {
            return data != null && data.Length > 0 ? Unsafe.TmphMemory.IndexOf(data, value) : -1;
        }

        /// <summary>
        ///     查找字节
        /// </summary>
        /// <param name="start">起始位置</param>
        /// <param name="end">结束位置</param>
        /// <param name="value">字节值</param>
        /// <returns>字节位置</returns>
        public static unsafe byte* Find(void* start, void* end, byte value)
        {
            return start != null && end > start ? Unsafe.TmphMemory.Find((byte*)start, (byte*)end, value) : null;
        }

        /// <summary>
        ///     字节替换
        /// </summary>
        /// <param name="value">字节数组</param>
        /// <param name="oldData">原字节</param>
        /// <param name="newData">目标字节</param>
        /// <returns>字节数组</returns>
        public static void replace(this byte[] value, byte oldData, byte newData)
        {
            if (value != null && value.Length != 0) Unsafe.TmphMemory.Replace(value, oldData, newData);
        }

        /// <summary>
        ///     大写转小写
        /// </summary>
        public static unsafe byte[] toLower(this byte[] value)
        {
            if (value != null)
            {
                fixed (byte* valueFixed = value) Unsafe.TmphMemory.ToLower(valueFixed, valueFixed + value.Length);
            }
            return value;
        }

        /// <summary>
        ///     大写转小写
        /// </summary>
        public static unsafe byte[] getToLower(this byte[] value)
        {
            if (value.length() != 0)
            {
                var newValue = new byte[value.Length];
                fixed (byte* valueFixed = value, newValueFixed = newValue)
                    Unsafe.TmphMemory.ToLower(valueFixed, valueFixed + value.Length, newValueFixed);
                return newValue;
            }
            return value;
        }

        /// <summary>
        ///     转16进制字符串(小写字母)
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns>16进制字符串</returns>
        public static unsafe string toLowerHex(this byte[] data)
        {
            if (data.length() != 0)
            {
                var value = TmphString.FastAllocateString(data.Length << 1);
                fixed (byte* dataFixed = data)
                fixed (char* valueFixed = value)
                {
                    var write = valueFixed;
                    for (byte* start = dataFixed, end = dataFixed + data.Length; start != end; ++start)
                    {
                        var code = *start >> 4;
                        *write++ = (char)(code < 10 ? code + '0' : (code + ('0' + 'a' - '9' - 1)));
                        code = *start & 0xf;
                        *write++ = (char)(code < 10 ? code + '0' : (code + ('0' + 'a' - '9' - 1)));
                    }
                }
                return value;
            }
            return data != null ? string.Empty : null;
        }
    }
}