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

using Laurent.Lee.CLB.Web;
using Laurent.Lee.CLB.Win32;

namespace Laurent.Lee.CLB.Unsafe
{
    /// <summary>
    ///     内存或字节数组(非安全,请自行确保数据可靠性)
    /// </summary>
    public static class TmphMemory
    {
        /// <summary>
        ///     字节流转16位整数
        /// </summary>
        /// <param name="values">字节数组,不能为null</param>
        /// <returns>整数值</returns>
        public static short GetShort(byte[] values)
        {
            return (short)(values[0] + (values[1] << 8));
        }

        /// <summary>
        ///     字节流转16位整数
        /// </summary>
        /// <param name="values">字节数组,不能为null</param>
        /// <param name="startIndex">起始位置</param>
        /// <returns>整数值</returns>
        public static short GetShort(byte[] values, int startIndex)
        {
            return (short)(values[startIndex] + (values[startIndex + 1] << 8));
        }

        /// <summary>
        ///     字节流转16位无符号整数
        /// </summary>
        /// <param name="values">字节数组,不能为null</param>
        /// <returns>无符号整数值</returns>
        public static ushort GetUShort(byte[] values)
        {
            return (ushort)(values[0] + (values[1] << 8));
        }

        /// <summary>
        ///     字节流转16位无符号整数
        /// </summary>
        /// <param name="values">字节数组,不能为null</param>
        /// <param name="startIndex">起始位置</param>
        /// <returns>无符号整数值</returns>
        public static ushort GetUShort(byte[] values, int startIndex)
        {
            return (ushort)(values[startIndex] + (values[startIndex + 1] << 8));
        }

        /// <summary>
        ///     字节流转32位整数
        /// </summary>
        /// <param name="values">字节数组,不能为null</param>
        /// <returns>整数值</returns>
        public static unsafe int GetInt(byte[] values)
        {
            fixed (byte* value = values) return *((int*)value);
        }

        /// <summary>
        ///     字节流转32位整数
        /// </summary>
        /// <param name="values">字节数组,不能为null</param>
        /// <param name="startIndex">起始位置</param>
        /// <returns>整数值</returns>
        public static unsafe int GetInt(byte[] values, int startIndex)
        {
            fixed (byte* value = values) return *((int*)(value + startIndex));
        }

        /// <summary>
        ///     字节流转32位无符号整数
        /// </summary>
        /// <param name="values">字节数组,不能为null</param>
        /// <returns>无符号整数值</returns>
        public static unsafe uint GetUInt(byte[] values)
        {
            fixed (byte* value = values) return *((uint*)value);
        }

        /// <summary>
        ///     字节流转32位无符号整数
        /// </summary>
        /// <param name="values">字节数组,不能为null</param>
        /// <param name="startIndex">起始位置</param>
        /// <returns>无符号整数值</returns>
        public static unsafe uint GetUInt(byte[] values, int startIndex)
        {
            fixed (byte* value = values) return *((uint*)(value + startIndex));
        }

        /// <summary>
        ///     字节流转32位无符号整数
        /// </summary>
        /// <param name="values">字节数组,不能为null</param>
        /// <param name="startIndex">起始位置</param>
        /// <returns>无符号整数值</returns>
        public static unsafe uint GetUIntBigEndian(byte[] values, int startIndex)
        {
            fixed (byte* value = values)
            {
                var start = value + startIndex;
                return ((uint)*start << 24) + ((uint)*(start + 1) << 16) + ((uint)*(start + 2) << 8) + *(start + 3);
            }
        }

        /// <summary>
        ///     字节流转64位整数
        /// </summary>
        /// <param name="values">字节数组,不能为null</param>
        /// <returns>整数值</returns>
        public static unsafe long GetLong(byte[] values)
        {
            fixed (byte* value = values) return *((long*)value);
        }

        /// <summary>
        ///     字节流转64位整数
        /// </summary>
        /// <param name="values">字节数组,不能为null</param>
        /// <param name="startIndex">起始位置</param>
        /// <returns>整数值</returns>
        public static unsafe long GetLong(byte[] values, int startIndex)
        {
            fixed (byte* value = values) return *((long*)(value + startIndex));
        }

        /// <summary>
        ///     字节流转64位无符号整数
        /// </summary>
        /// <param name="values">字节数组,不能为null</param>
        /// <returns>无符号整数值</returns>
        public static unsafe ulong GetULong(byte[] values)
        {
            fixed (byte* value = values) return *((ulong*)value);
        }

        /// <summary>
        ///     字节流转64位无符号整数
        /// </summary>
        /// <param name="values">字节数组,不能为null</param>
        /// <param name="startIndex">起始位置</param>
        /// <returns>无符号整数值</returns>
        public static unsafe ulong GetULong(byte[] values, int startIndex)
        {
            fixed (byte* value = values) return *((ulong*)(value + startIndex));
        }

        /// <summary>
        ///     数据清0
        /// </summary>
        /// <param name="data"></param>
        /// <param name="count">必须保证是4的倍数</param>
        public static unsafe void Clear32(byte* data, int count)
        {
            for (var end = data + count; data != end; data += sizeof(int)) *(int*)data = 0;
        }

        /// <summary>
        ///     填充整数
        /// </summary>
        /// <param name="src">串起始地址,不能为null</param>
        /// <param name="value">整数值</param>
        /// <param name="length">整数数量,大于等于0</param>
        public static unsafe void Fill(void* src, ulong value, int count)
        {
            var shift = (int)src & (sizeof(ulong) - 1);
            if (shift == 0) fill((byte*)src, value, count);
            else if (count > 0)
            {
                *(ulong*)src = value;
                fill((byte*)src + (sizeof(ulong) - shift), (value >> (shift <<= 3)) | (value << (64 - shift)), --count);
                *((ulong*)src + count) = value;
            }
        }

        /// <summary>
        ///     填充整数
        /// </summary>
        /// <param name="src">串起始地址,不能为null</param>
        /// <param name="value">整数值</param>
        /// <param name="length">整数数量,大于等于0</param>
        public static unsafe void Fill(void* src, uint value, int count)
        {
            var data = (uint*)src;
            if ((count & 1) != 0)
            {
                *data = value;
                --count;
                ++data;
            }
            Fill(data, value | ((ulong)value << 32), count >> 1);
        }

        /// <summary>
        ///     填充字节
        /// </summary>
        /// <param name="src">串起始地址,不能为null</param>
        /// <param name="value">字节值</param>
        /// <param name="length">字节数量,大于等于0</param>
        public static unsafe void Fill(void* src, byte value, int length)
        {
            var data = (byte*)src;
            ulong cpuValue = (uint)((value << 8) | value);
            var shift = (int)data & (sizeof(ulong) - 1);
            cpuValue |= cpuValue << 16;
            if (shift != 0)
            {
                shift = sizeof(ulong) - shift;
                if (shift > length) shift = length;
                if ((shift & 1) != 0) *data++ = value;
                if ((shift & 2) != 0)
                {
                    *((short*)data) = (short)cpuValue;
                    data += 2;
                }
                if ((shift & 4) != 0)
                {
                    *((uint*)data) = (uint)cpuValue;
                    data += 4;
                }
                length -= shift;
            }
            fill(data, cpuValue | (cpuValue << 32), length >> 3);
            data += length & (int.MaxValue - (sizeof(ulong) - 1));
            if ((length & 4) != 0)
            {
                *((uint*)data) = (uint)cpuValue;
                data += 4;
            }
            if ((length & 2) != 0)
            {
                *((short*)data) = (short)cpuValue;
                data += 2;
            }
            if ((length & 1) != 0) *data = value;
        }

        /// <summary>
        ///     填充整数(用TmphBuffer.BlockCopy可能比指针快)
        /// </summary>
        /// <param name="src">串起始地址,不能为null</param>
        /// <param name="value">整数值</param>
        /// <param name="length">整数数量,大于等于0</param>
        private static unsafe void fill(byte* src, ulong value, int count)
        {
            for (var index = count >> 2; index != 0; --index)
            {
                *((ulong*)src) = *((ulong*)(src + sizeof(ulong)))
                    = *((ulong*)(src + sizeof(ulong) * 2)) = *((ulong*)(src + sizeof(ulong) * 3)) = value;
                src += sizeof(ulong) * 4;
            }
            if ((count & 2) != 0)
            {
                *((ulong*)src) = *((ulong*)(src + sizeof(ulong))) = value;
                src += sizeof(ulong) * 2;
            }
            if ((count & 1) != 0) *((ulong*)src) = value;
        }

        /// <summary>
        ///     复制字节数组
        /// </summary>
        /// <param name="source">原字节数组,不能为null</param>
        /// <param name="destination">目标串起始地址,不能为null</param>
        /// <param name="length">字节长度,大于等于0</param>
        public static unsafe void Copy(byte[] source, void* destination, int length)
        {
            fixed (byte* data = source) Copy(data, destination, length);
        }

        /// <summary>
        ///     复制字节数组
        /// </summary>
        /// <param name="source">原字节起始地址,不能为null</param>
        /// <param name="destination">目标串数组,不能为null</param>
        /// <param name="length">字节长度,大于等于0</param>
        public static unsafe void Copy(void* source, byte[] destination, int length)
        {
            fixed (byte* data = destination) Copy(source, data, length);
        }

        /// <summary>
        ///     复制字节数组
        /// </summary>
        /// <param name="source">原串起始地址,不能为null</param>
        /// <param name="destination">目标串起始地址,不能为null</param>
        /// <param name="length">字节长度,大于等于0</param>
        public static unsafe void Copy(void* source, void* destination, int length)
        {
#if MONO
            int shift = (int)destination & (sizeof(ulong) - 1);
            if (shift == 0) copy((byte*)source, (byte*)destination, length);
            else
            {
                shift = sizeof(ulong) - shift;
                if (shift > length) shift = length;
                if ((shift & 1) != 0)
                {
                    *(byte*)destination = *(byte*)source;
                    if ((shift & 2) != 0)
                    {
                        *(ushort*)((byte*)destination + 1) = *(ushort*)((byte*)source + 1);
                        if ((shift & 4) != 0) *(uint*)((byte*)destination + 3) = *(uint*)((byte*)source + 3);
                    }
                    else if ((shift & 4) != 0) *(uint*)((byte*)destination + 1) = *(uint*)((byte*)source + 1);
                }
                else if ((shift & 2) != 0)
                {
                    *(ushort*)destination = *(ushort*)source;
                    if ((shift & 4) != 0) *(uint*)((byte*)destination + 2) = *(uint*)((byte*)source + 2);
                }
                else if ((shift & 4) != 0) *(uint*)destination = *(uint*)source;
                copy((byte*)source + shift, (byte*)destination + shift, length -= (int)shift);
            }
#else
            TmphKernel32.RtlMoveMemory(destination, source, length);
#endif
        }

        /// <summary>
        ///     复制字节数组
        /// </summary>
        /// <param name="destination">目标字节数组,不能为null</param>
        /// <param name="source">原串起始地址,不能为null</param>
        /// <param name="length">字节长度,大于等于0</param>
        public static unsafe byte[] Copy(void* source, int length)
        {
            var data = new byte[length];
#if MONO
            fixed (byte* dataFixed = data) copy((byte*)source, dataFixed, length);
#else
            fixed (byte* dataFixed = data) TmphKernel32.RtlMoveMemory(dataFixed, (byte*)source, length);
#endif
            return data;
        }

        /// <summary>
        ///     复制字符数组
        /// </summary>
        /// <param name="source">原串起始地址,不能为null</param>
        /// <param name="destination">目标串起始地址,不能为null</param>
        /// <param name="count">字符数量,大于等于0</param>
        public static unsafe void Copy(char* source, char* destination, int count)
        {
            Copy(source, (void*)destination, count << 1);
        }

#if MONO
    /// <summary>
    /// 复制字节数组
    /// </summary>
    /// <param name="source">原串起始地址,不能为null</param>
    /// <param name="destination">目标串起始地址,不能为null</param>
    /// <param name="length">字节长度,大于等于0</param>
        unsafe static void copy(byte* source, byte* destination, int length)
        {
            if (length >= sizeof(ulong) * 4)
            {
                do
                {
                    *((ulong*)destination) = *((ulong*)source);
                    *((ulong*)(destination + sizeof(ulong))) = *((ulong*)(source + sizeof(ulong)));
                    *((ulong*)(destination + sizeof(ulong) * 2)) = *((ulong*)(source + sizeof(ulong) * 2));
                    *((ulong*)(destination + sizeof(ulong) * 3)) = *((ulong*)(source + sizeof(ulong) * 3));
                    destination += sizeof(ulong) * 4;
                    source += sizeof(ulong) * 4;
                }
                while ((length -= sizeof(ulong) * 4) >= sizeof(ulong) * 4);
            }
            if ((length & (sizeof(ulong) * 2)) != 0)
            {
                *((ulong*)destination) = *((ulong*)source);
                *((ulong*)(destination + sizeof(ulong))) = *((ulong*)(source + sizeof(ulong)));
                destination += sizeof(ulong) * 2;
                source += sizeof(ulong) * 2;
            }
            if ((length & sizeof(ulong)) != 0)
            {
                *((ulong*)destination) = *((ulong*)source);
                destination += sizeof(ulong);
                source += sizeof(ulong);
            }
            if ((length & sizeof(uint)) != 0)
            {
                *((uint*)destination) = *((uint*)source);
                destination += sizeof(uint);
                source += sizeof(uint);
            }
            if ((length & 2) != 0)
            {
                *((ushort*)destination) = *((ushort*)source);
                destination += 2;
                source += 2;
            }
            if ((length & 1) != 0) *destination = *source;
        }
#endif

        /// <summary>
        ///     复制字节数组
        /// </summary>
        /// <param name="source">原串起始地址,不能为null</param>
        /// <param name="destination">目标串起始地址,不能为null</param>
        /// <param name="length">字节长度,大于等于0</param>
        public static unsafe void CopyDesc(void* source, void* destination, int length)
        {
            source = (byte*)source + length;
            destination = (byte*)destination + length;
            var shift = (int)destination & (sizeof(ulong) - 1);
            if (shift == 0) copyDesc((byte*)source, (byte*)destination, length);
            else
            {
                if (shift > length) shift = length;
                if ((shift & 1) != 0)
                {
                    source = (byte*)source - 1;
                    destination = (byte*)destination - 1;
                    *(byte*)destination = *(byte*)source;
                }
                if ((shift & 2) != 0)
                {
                    source = (byte*)source - sizeof(ushort);
                    destination = (byte*)destination - sizeof(ushort);
                    *(ushort*)destination = *(ushort*)source;
                }
                if ((shift & 4) != 0)
                {
                    source = (byte*)source - sizeof(uint);
                    destination = (byte*)destination - sizeof(uint);
                    *(uint*)destination = *(uint*)source;
                }
                copyDesc((byte*)source, (byte*)destination, length -= shift);
            }
        }

        /// <summary>
        ///     复制字节数组
        /// </summary>
        /// <param name="source">原串起始地址,不能为null</param>
        /// <param name="destination">目标串起始地址,不能为null</param>
        /// <param name="length">字节长度,大于等于0</param>
        private static unsafe void copyDesc(byte* source, byte* destination, int length)
        {
            if (length >= sizeof(ulong) * 4)
            {
                do
                {
                    destination -= sizeof(ulong) * 4;
                    source -= sizeof(ulong) * 4;
                    *((ulong*)(destination + sizeof(ulong) * 3)) = *((ulong*)(source + sizeof(ulong) * 3));
                    *((ulong*)(destination + sizeof(ulong) * 2)) = *((ulong*)(source + sizeof(ulong) * 2));
                    *((ulong*)(destination + sizeof(ulong))) = *((ulong*)(source + sizeof(ulong)));
                    *((ulong*)destination) = *((ulong*)source);
                } while ((length -= sizeof(ulong) * 4) >= sizeof(ulong) * 4);
            }
            if ((length & (sizeof(ulong) * 2)) != 0)
            {
                destination -= sizeof(ulong) * 2;
                source -= sizeof(ulong) * 2;
                *((ulong*)(destination + sizeof(ulong))) = *((ulong*)(source + sizeof(ulong)));
                *((ulong*)destination) = *((ulong*)source);
            }
            if ((length & sizeof(ulong)) != 0)
            {
                destination -= sizeof(ulong);
                source -= sizeof(ulong);
                *((ulong*)destination) = *((ulong*)source);
            }
            if ((length & 4) != 0)
            {
                destination -= 4;
                source -= 4;
                *((uint*)destination) = *((uint*)source);
            }
            if ((length & 2) != 0)
            {
                destination -= 2;
                source -= 2;
                *((ushort*)destination) = *((ushort*)source);
            }
            if ((length & 1) != 0) *--destination = *--source;
        }

        /// <summary>
        ///     填充二进制位
        /// </summary>
        /// <param name="data">数据起始位置,不能为null</param>
        /// <param name="start">起始二进制位,不能越界</param>
        /// <param name="count">二进制位数量,不能越界</param>
        public static unsafe void FillBits(byte[] data, int start, int count)
        {
            fixed (byte* dataFixed = data) FillBits(dataFixed, start, count);
        }

        /// <summary>
        ///     填充二进制位
        /// </summary>
        /// <param name="data">数据起始位置,不能为null</param>
        /// <param name="start">起始二进制位,不能越界</param>
        /// <param name="count">二进制位数量,不能越界</param>
        public static unsafe void FillBits(byte* data, int start, int count)
        {
            data += (start >> 6) << 3;
            if ((start &= ((sizeof(ulong) << 3) - 1)) != 0)
            {
                var high = (sizeof(ulong) << 3) - start;
                if ((count -= high) >= 0)
                {
                    *(ulong*)data |= ulong.MaxValue << start;
                    data += sizeof(ulong);
                }
                else
                {
                    *(ulong*)data |= (ulong.MaxValue >> (start - count)) << start;
                    return;
                }
            }
            Fill(data, ulong.MaxValue, start = count >> 6);
            if ((count = -count & ((sizeof(ulong) << 3) - 1)) != 0)
                *(ulong*)(data + (start << 3)) |= ulong.MaxValue >> count;
        }

        /// <summary>
        ///     清除二进制位
        /// </summary>
        /// <param name="data">数据起始位置,不能为null</param>
        /// <param name="start">起始二进制位,不能越界</param>
        /// <param name="count">二进制位数量,不能越界</param>
        public static unsafe void ClearBits(byte[] data, int start, int count)
        {
            fixed (byte* dataFixed = data) ClearBits(dataFixed, start, count);
        }

        /// <summary>
        ///     清除二进制位
        /// </summary>
        /// <param name="data">数据起始位置,不能为null</param>
        /// <param name="start">起始二进制位,不能越界</param>
        /// <param name="count">二进制位数量,不能越界</param>
        public static unsafe void ClearBits(byte* data, int start, int count)
        {
            data += (start >> 6) << 3;
            if ((start &= ((sizeof(ulong) << 3) - 1)) != 0)
            {
                var high = (sizeof(ulong) << 3) - start;
                if ((count -= high) >= 0)
                {
                    *(ulong*)data &= ulong.MaxValue >> high;
                    data += sizeof(ulong);
                }
                else
                {
                    *(ulong*)data &= ((ulong.MaxValue >> (start - count)) << start) ^ ulong.MaxValue;
                    return;
                }
            }
            Fill(data, 0UL, start = count >> 6);
            if ((count &= ((sizeof(ulong) << 3) - 1)) != 0) *(ulong*)(data + (start << 3)) &= ulong.MaxValue << count;
        }

        /// <summary>
        ///     字节数组比较
        /// </summary>
        /// <param name="left">不能为null</param>
        /// <param name="right">不能为null</param>
        /// <returns>是否相等</returns>
        public static bool Equal(byte[] left, byte[] right)
        {
            return left.Length == right.Length && Equal(left, right, left.Length);
        }

        /// <summary>
        ///     字节数组比较
        /// </summary>
        /// <param name="left">不能为null</param>
        /// <param name="right">不能为null</param>
        /// <param name="count">比较字节数,必须大于等于0</param>
        /// <returns>是否相等</returns>
        public static unsafe bool Equal(byte[] left, byte[] right, int count)
        {
            fixed (byte* leftFixed = left, rightFixed = right) return equal(leftFixed, rightFixed, count);
        }

        /// <summary>
        ///     字节数组比较
        /// </summary>
        /// <param name="left">不能为null</param>
        /// <param name="right">不能为null</param>
        /// <param name="count">比较字节数,必须大于等于0</param>
        /// <returns>是否相等</returns>
        public static unsafe bool Equal(byte[] left, void* right, int count)
        {
            fixed (byte* leftFixed = left) return equal(leftFixed, (byte*)right, count);
        }

        /// </summary>
        /// 字节数组比较
        /// </summary>
        /// <param name="left">不能为null</param>
        /// <param name="right">不能为null</param>
        /// <param name="count">比较字节数,必须大于等于0</param>
        /// <returns>是否相等</returns>
        public static unsafe bool Equal(void* left, void* right, int count)
        {
            var shift = (int)left & (sizeof(ulong) - 1);
            if (count > shift)
            {
                return equal((byte*)left, (byte*)right, shift) &&
                       equal((byte*)left + shift, (byte*)right + shift, count - shift);
            }
            return equal((byte*)left, (byte*)right, count);
        }

        /// </summary>
        /// 字节数组比较
        /// </summary>
        /// <param name="left">不能为null</param>
        /// <param name="right">不能为null</param>
        /// <param name="count">比较字节数</param>
        /// <returns>是否相等</returns>
        private static unsafe bool equal(byte* left, byte* right, int count)
        {
            while (count >= sizeof(ulong) * 4)
            {
                if (((*((ulong*)left) ^ *((ulong*)right)) |
                     (*((ulong*)(left + sizeof(ulong))) ^ *((ulong*)(right + sizeof(ulong)))) |
                     (*((ulong*)(left + sizeof(ulong) * 2)) ^ *((ulong*)(right + sizeof(ulong) * 2))) |
                     (*((ulong*)(left + sizeof(ulong) * 3)) ^ *((ulong*)(right + sizeof(ulong) * 3)))) != 0)
                    return false;
                left += sizeof(ulong) * 4;
                right += sizeof(ulong) * 4;
                count -= sizeof(ulong) * 4;
            }
            if (count < sizeof(ulong) * 4)
            {
                ulong isEqual = 0;
                if ((count & (sizeof(ulong) * 2)) != 0)
                {
                    isEqual |= (*((ulong*)left) ^ *((ulong*)right))
                               | (*((ulong*)(left + sizeof(ulong))) ^ *((ulong*)(right + sizeof(ulong))));
                    left += sizeof(ulong) * 2;
                    right += sizeof(ulong) * 2;
                }
                if ((count & sizeof(ulong)) != 0)
                {
                    isEqual |= *((ulong*)left) ^ *((ulong*)right);
                    left += sizeof(ulong);
                    right += sizeof(ulong);
                }
                if ((count &= (sizeof(ulong) - 1)) != 0)
                {
                    count <<= 3;
                    isEqual |= (*((ulong*)left) ^ *((ulong*)right)) << (64 - count);
                }
                return isEqual == 0;
            }
            return true;
        }

        /// <summary>
        ///     字节数组比较(忽略大小写)
        /// </summary>
        /// <param name="left">不能为null</param>
        /// <param name="right">不能为null</param>
        /// <param name="count">字符数量,大于等于0</param>
        /// <returns>是否相等</returns>
        public static unsafe bool EqualCase(byte* left, byte* right, int count)
        {
            for (var end = left + count; left != end; ++left, ++right)
            {
                if (*left != *right)
                {
                    if ((*left | 0x20) != (*right | 0x20) || (uint)(*left - 'a') >= 26) return false;
                }
            }
            return true;
        }

        /// <summary>
        ///     查找字节
        /// </summary>
        /// <param name="data">字节数组,不能为null</param>
        /// <param name="value">字节值</param>
        /// <returns>字节位置,失败为-1</returns>
        public static unsafe int IndexOf(byte[] data, byte value)
        {
            fixed (byte* dataFixed = data)
            {
                var valueData = Find(dataFixed, dataFixed + data.Length, value);
                return valueData != null ? (int)(valueData - dataFixed) : -1;
            }
        }

        /// <summary>
        ///     查找字节,数据长度不能为0
        /// </summary>
        /// <param name="start">起始位置,不能为null</param>
        /// <param name="end">结束位置,不能为null</param>
        /// <param name="value">字节值</param>
        /// <returns>字节位置,失败为null</returns>
        public static unsafe byte* Find(byte* start, byte* end, byte value)
        {
            var oldValue = *--end;
            for (*end = value; *start != value; ++start) ;
            *end = oldValue;
            return start != end || oldValue == value ? start : null;
        }

        /// <summary>
        ///     查找最后一个字节,数据长度不能为0
        /// </summary>
        /// <param name="start">起始位置,不能为null</param>
        /// <param name="end">结束位置,不能为null</param>
        /// <param name="value">字节值</param>
        /// <returns>字节位置,失败为null</returns>
        public static unsafe byte* FindLast(byte* start, byte* end, byte value)
        {
            var oldValue = *start;
            *start = value;
            while (*--end != value) ;
            *start = oldValue;
            return start != end || oldValue == value ? end : null;
        }

        /// <summary>
        ///     字节替换
        /// </summary>
        /// <param name="value">字节数组,长度不能为0</param>
        /// <param name="oldData">原字节</param>
        /// <param name="newChar">目标字节</param>
        /// <returns>字节数组</returns>
        public static unsafe void Replace(byte[] value, byte oldData, byte newData)
        {
            fixed (byte* valueFixed = value)
            {
                byte* start = valueFixed, end = valueFixed + value.Length;
                var endValue = *--end;
                *end = oldData;
                do
                {
                    while (*start != oldData) ++start;
                    *start = newData;
                } while (start++ != end);
                if (endValue != oldData) *end = endValue;
            }
        }

        /// <summary>
        ///     大写转小写
        /// </summary>
        /// <param name="start">起始位置,不能为null</param>
        /// <param name="end">结束位置,不能为null</param>
        public static unsafe void ToLower(byte* start, byte* end)
        {
            while (start != end)
            {
                if ((uint)(*start - 'A') < 26) *start |= 0x20;
                ++start;
            }
        }

        /// <summary>
        ///     大写转小写
        /// </summary>
        /// <param name="start">起始位置,不能为null</param>
        /// <param name="end">结束位置,不能为null</param>
        /// <param name="write">写入位置,不能为null</param>
        public static unsafe void ToLower(byte* start, byte* end, byte* write)
        {
            while (start != end)
            {
                if ((uint)(*start - 'A') < 26) *write++ = (byte)(*start++ | 0x20);
                else *write++ = *start++;
            }
        }

        /// <summary>
        ///     大写转小写
        /// </summary>
        /// <param name="start">起始位置,不能为null</param>
        /// <param name="end">结束位置,不能为null</param>
        /// <param name="write">写入位置,不能为null</param>
        public static unsafe void ToLower(byte* start, byte* end, char* write)
        {
            while (start != end)
            {
                if ((uint)(*start - 'A') < 26) *write++ = (char)(*start++ | 0x20);
                else *write++ = (char)*start++;
            }
        }

        /// <summary>
        ///     字节流转换成JSON字符串
        /// </summary>
        /// <param name="jsonStream">JSON输出流</param>
        /// <param name="TmphBuffer">字节流数组</param>
        public static unsafe void ToJson(TmphCharStream jsonStream, TmphSubArray<byte> TmphBuffer)
        {
            if (TmphBuffer.Array == null) TmphAjax.WriteNull(jsonStream);
            else if (TmphBuffer.Count == 0) TmphAjax.WriteArray(jsonStream);
            else
            {
                fixed (byte* bufferFixed = TmphBuffer.Array)
                {
                    var start = bufferFixed + TmphBuffer.StartIndex;
                    ToJson(jsonStream, start, start + TmphBuffer.Count);
                }
            }
        }

        /// <summary>
        ///     字节流转换成JSON字符串
        /// </summary>
        /// <param name="jsonStream">JSON输出流,不能为null</param>
        /// <param name="start">起始位置,不能为null</param>
        /// <param name="end">结束位置,长度必须大于0</param>
        public static unsafe void ToJson(TmphCharStream jsonStream, byte* start, byte* end)
        {
            jsonStream.Write('[');
            for (TmphAjax.ToString(*start, jsonStream); ++start != end; TmphAjax.ToString(*start, jsonStream))
                jsonStream.Write(',');
            jsonStream.Write(']');
        }
    }
}