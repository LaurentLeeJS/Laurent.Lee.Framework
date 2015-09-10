/*
 *  Copyright 2015 Tony Lee

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

     http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
 */

using Laurent.Lee.CLB.Config;
using System;

namespace Laurent.Lee.CLB.Web
{
    /// <summary>
    ///     ajax相关操作
    /// </summary>
    public static class TmphAjax
    {
        /// <summary>
        ///     ajax生成串替代字符串,默认输入都必须过滤
        /// </summary>
        public const char Quote = TmphPub.NullChar;

        /// <summary>
        ///     ajax生成串替代字符串,默认输入都必须过滤
        /// </summary>
        public const string QuoteString = "\0";

        /// <summary>
        ///     视图类型名称
        /// </summary>
        public const char ViewClientType = '@';

        /// <summary>
        ///     视图类型成员标识
        /// </summary>
        public const char ViewClientMember = '.';

        /// <summary>
        ///     ajax时间后缀
        /// </summary>
        public const char DateEnd = ')';

        /// <summary>
        ///     ajax数组字符串
        /// </summary>
        public const string Array = "[]";

        /// <summary>
        ///     ajax空对象
        /// </summary>
        public const string Object = "{}";

        /// <summary>
        ///     ajax空值
        /// </summary>
        public const string Null = "null";

        ///// <summary>
        ///// 客户端格式化ajax串的函数
        ///// </summary>
        //public const string FormatAjax = ".FormatAjax()";
        /// <summary>
        ///     客户端格式化视图数组函数
        /// </summary>
        public const string FormatView = ".FormatView()";

        /// <summary>
        ///     客户端循环引用获取函数名称
        /// </summary>
        public const string SetLoopObject = TmphPub.LaurentLeeFramework + ".SetJsonLoop";

        /// <summary>
        ///     客户端循环引用获取函数名称
        /// </summary>
        public const string GetLoopObject = TmphPub.LaurentLeeFramework + ".GetJsonLoop";

        /// <summary>
        ///     最大值
        /// </summary>
        private const long maxValue = (1L << 52) - 1;

        /// <summary>
        ///     ajax时间前缀
        /// </summary>
        public static readonly string DateStart = "new Date(";

        ///// <summary>
        ///// 客户端视图类型复制函数
        ///// </summary>
        //public static readonly string ViewCopy = pub.Laurent.Lee.CLB + ".Copy";
        /// <summary>
        ///     十进制前缀"--0x"
        /// </summary>
        private static readonly ulong hexPrefix;

        /// <summary>
        ///     Json转换时间差
        /// </summary>
        public static readonly long JavascriptMinTimeTicks = TmphAppSetting.JavascriptMinTime.Ticks;

        static unsafe TmphAjax()
        {
            var hex = "--0x";
            fixed (char* hexFixed = hex) hexPrefix = *(ulong*)hexFixed;
        }

        /// <summary>
        ///     格式化ajax字符串
        /// </summary>
        /// <param name="jsStream">js字符流</param>
        /// <returns>格式化后ajax字符串</returns>
        public static unsafe string FormatJavascript(TmphCharStream jsStream)
        {
            var length = formatLength(jsStream);
            if (length == 0) return jsStream.Length == 0 ? string.Empty : formatQuote(jsStream);
            if ((length += jsStream.Length) <= jsStream.DataLength >> 1) return format(jsStream, length);
            var value = TmphString.FastAllocateString(length);
            fixed (char* valueFixed = value)
            {
                for (char* start = jsStream.Char, write = valueFixed, end = start + jsStream.Length;
                    start != end;
                    ++start)
                {
                    if (*start == Quote) *write++ = '"';
                    else if (*start == '\n')
                    {
                        *(int*)write = '\\' + ('n' << 16);
                        write += 2;
                    }
                    else if (*start == '\r')
                    {
                        *(int*)write = '\\' + ('r' << 16);
                        write += 2;
                    }
                    else if (*start == '"' || *start == '\\')
                    {
                        *write++ = '\\';
                        *write++ = *start;
                    }
                    else *write++ = *start;
                }
            }
            return value;
        }

        /// <summary>
        ///     ajax字符串长度
        /// </summary>
        /// <param name="jsStream">js字符流</param>
        /// <returns>ajax字符串长度</returns>
        private static unsafe int formatLength(TmphCharStream jsStream)
        {
            var length = 0;
            for (char* start = jsStream.Char, end = start + jsStream.Length; start != end; ++start)
            {
                if (*start == '\n' || *start == '\r' || *start == '"' || *start == '\\') ++length;
            }
            return length;
        }

        /// <summary>
        ///     格式化ajax字符串
        /// </summary>
        /// <param name="jsStream">js字符流</param>
        /// <returns>格式化后ajax字符串</returns>
        private static unsafe string formatQuote(TmphCharStream jsStream)
        {
            for (char* start = jsStream.Char, end = start + jsStream.Length; start != end; ++start)
            {
                if (*start == Quote) *start = '"';
            }
            return new string(jsStream.Char, 0, jsStream.Length);
        }

        /// <summary>
        ///     格式化ajax字符串
        /// </summary>
        /// <param name="jsStream">js字符流</param>
        /// <param name="length">格式化后ajax字符串长度</param>
        /// <returns>格式化后ajax字符串</returns>
        private static unsafe string format(TmphCharStream jsStream, int length)
        {
            char* start = jsStream.Char, write = start + length;
            for (var read = start + jsStream.Length; read != write;)
            {
                if (*--read == Quote) *--write = '"';
                else if (*read == '\n') *(int*)(write -= 2) = ('n' << 16) + '\\';
                else if (*read == '\r') *(int*)(write -= 2) = ('r' << 16) + '\\';
                else
                {
                    *--write = *read;
                    if (*read == '"' || *read == '\\') *--write = '\\';
                }
            }
            while (write != start) if (*--write == Quote) *write = '"';
            return new string(jsStream.Char, 0, length);
        }

        /// <summary>
        ///     格式化ajax字符串
        /// </summary>
        /// <param name="jsStream">JS字符流</param>
        /// <param name="formatStream">格式化JSON字符流</param>
        internal static unsafe void FormatJavascript(TmphCharStream jsStream, TmphUnmanagedStream formatStream)
        {
            var length = formatLength(jsStream);
            if (length == 0)
            {
                if (jsStream.Length != 0) formatQuote(jsStream, formatStream);
                return;
            }
            length += jsStream.Length;
            formatStream.PrepLength(length <<= 1);
            for (char* start = jsStream.Char, write = (char*)(formatStream.CurrentData), end = start + jsStream.Length;
                start != end;
                ++start)
            {
                if (*start == Quote) *write++ = '"';
                else if (*start == '\n')
                {
                    *(int*)write = ('n' << 16) + '\\';
                    write += 2;
                }
                else if (*start == '\r')
                {
                    *(int*)write = ('r' << 16) + '\\';
                    write += 2;
                }
                else
                {
                    if (*start == '"' || *start == '\\') *write++ = '\\';
                    *write++ = *start;
                }
            }
            formatStream.Unsafer.AddLength(length);
        }

        /// <summary>
        ///     格式化ajax字符串
        /// </summary>
        /// <param name="jsStream">js字符流</param>
        /// <param name="formatStream">格式化JSON字符流</param>
        private static unsafe void formatQuote(TmphCharStream jsStream, TmphUnmanagedStream formatStream)
        {
            var length = jsStream.Length;
            char* start = jsStream.Char, end = start + length;
            formatStream.PrepLength(length <<= 1);
            for (var write = (char*)(formatStream.CurrentData); start != end; ++start)
                *write++ = *start == Quote ? '"' : *start;
            formatStream.Unsafer.AddLength(length);
        }

        /// <summary>
        ///     输出空对象
        /// </summary>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static unsafe void WriteObject(TmphCharStream jsonStream)
        {
            jsonStream.PrepLength(2);
            var data = (byte*)jsonStream.CurrentChar;
            *(char*)data = '{';
            *(char*)(data + sizeof(char)) = '}';
            jsonStream.Unsafer.AddLength(2);
        }

        /// <summary>
        ///     输出空数组
        /// </summary>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static unsafe void WriteArray(TmphCharStream jsonStream)
        {
            jsonStream.PrepLength(2);
            var data = (byte*)jsonStream.CurrentChar;
            *(char*)data = '[';
            *(char*)(data + sizeof(char)) = ']';
            jsonStream.Unsafer.AddLength(2);
        }

        /// <summary>
        ///     输出null值
        /// </summary>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static unsafe void WriteNull(TmphCharStream jsonStream)
        {
            jsonStream.PrepLength(4);
            var data = (byte*)jsonStream.CurrentChar;
            *(char*)data = 'n';
            *(char*)(data + sizeof(char)) = 'u';
            *(char*)(data + sizeof(char) * 2) = 'l';
            *(char*)(data + sizeof(char) * 3) = 'l';
            jsonStream.Unsafer.AddLength(4);
        }

        /// <summary>
        ///     输出非数字值
        /// </summary>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static unsafe void WriteNaN(TmphCharStream jsonStream)
        {
            jsonStream.PrepLength(3);
            var data = (byte*)jsonStream.CurrentChar;
            *(char*)data = 'N';
            *(char*)(data + sizeof(char)) = 'a';
            *(char*)(data + sizeof(char) * 2) = 'N';
            jsonStream.Unsafer.AddLength(3);
        }

        /// <summary>
        ///     数字转换成字符串
        /// </summary>
        /// <param name="value">数字值</param>
        /// <param name="chars">输出字符地址</param>
        /// <returns>输出起始位置+输出长度</returns>
        private static unsafe TmphKeyValue<int, int> toString(long value, char* chars)
        {
            var startIndex = 1;
            if (value < 0)
            {
                value = -value;
                startIndex = 0;
            }
            if (value < 10000)
            {
                var value32 = (int)value;
                var nextChar = chars + 4;
                if (value32 < 10) *nextChar = (char)(value32 + '0');
                else
                {
                    var div = (value32 * (int)TmphNumber.Div10_16Mul) >> TmphNumber.Div10_16Shift;
                    *nextChar = (char)((value32 - div * 10) + '0');
                    if (div < 10) *--nextChar = (char)(div + '0');
                    else
                    {
                        value32 = (div * (int)TmphNumber.Div10_16Mul) >> TmphNumber.Div10_16Shift;
                        *--nextChar = (char)((div - value32 * 10) + '0');
                        if (value32 < 10) *--nextChar = (char)(value32 + '0');
                        else
                        {
                            div = (value32 * (int)TmphNumber.Div10_16Mul) >> TmphNumber.Div10_16Shift;
                            *--nextChar = (char)((value32 - div * 10) + '0');
                            *--nextChar = (char)(div + '0');
                        }
                    }
                }
                *--nextChar = '-';
                nextChar += startIndex;
                return new TmphKeyValue<int, int>((int)(nextChar - chars), 5 - (int)(nextChar - chars));
            }
            else
            {
                var nextChar = chars + 19;
                var nextValue = value & 15;
                *nextChar = (char)(nextValue < 10 ? nextValue + '0' : (nextValue + ('0' + 'A' - '9' - 1)));
                nextValue = (value >>= 4) & 15;
                *--nextChar = (char)(nextValue < 10 ? nextValue + '0' : (nextValue + ('0' + 'A' - '9' - 1)));
                nextValue = (value >>= 4) & 15;
                *--nextChar = (char)(nextValue < 10 ? nextValue + '0' : (nextValue + ('0' + 'A' - '9' - 1)));
                nextValue = (value >>= 4) & 15;
                *--nextChar = (char)(nextValue < 10 ? nextValue + '0' : (nextValue + ('0' + 'A' - '9' - 1)));
                if (value >= 0x1000000000L)
                {
                    nextValue = (value >>= 4) & 15;
                    *--nextChar = (char)(nextValue < 10 ? nextValue + '0' : (nextValue + ('0' + 'A' - '9' - 1)));
                    nextValue = (value >>= 4) & 15;
                    *--nextChar = (char)(nextValue < 10 ? nextValue + '0' : (nextValue + ('0' + 'A' - '9' - 1)));
                    nextValue = (value >>= 4) & 15;
                    *--nextChar = (char)(nextValue < 10 ? nextValue + '0' : (nextValue + ('0' + 'A' - '9' - 1)));
                    nextValue = (value >>= 4) & 15;
                    *--nextChar = (char)(nextValue < 10 ? nextValue + '0' : (nextValue + ('0' + 'A' - '9' - 1)));
                }
                uint value32 = (uint)(value >> 4), nextValue32;
                if (value32 >= 0x10000)
                {
                    nextValue32 = value32 & 15;
                    *--nextChar = (char)(nextValue32 < 10 ? nextValue32 + '0' : (nextValue32 + ('0' + 'A' - '9' - 1)));
                    nextValue32 = (value32 >>= 4) & 15;
                    *--nextChar = (char)(nextValue32 < 10 ? nextValue32 + '0' : (nextValue32 + ('0' + 'A' - '9' - 1)));
                    nextValue32 = (value32 >>= 4) & 15;
                    *--nextChar = (char)(nextValue32 < 10 ? nextValue32 + '0' : (nextValue32 + ('0' + 'A' - '9' - 1)));
                    nextValue32 = (value32 >>= 4) & 15;
                    *--nextChar = (char)(nextValue32 < 10 ? nextValue32 + '0' : (nextValue32 + ('0' + 'A' - '9' - 1)));
                    value32 >>= 4;
                }
                if (value32 >= 0x100)
                {
                    nextValue32 = value32 & 15;
                    *--nextChar = (char)(nextValue32 < 10 ? nextValue32 + '0' : (nextValue32 + ('0' + 'A' - '9' - 1)));
                    nextValue32 = (value32 >>= 4) & 15;
                    *--nextChar = (char)(nextValue32 < 10 ? nextValue32 + '0' : (nextValue32 + ('0' + 'A' - '9' - 1)));
                    value32 >>= 4;
                }
                if (value32 >= 0x10)
                {
                    nextValue32 = value32 & 15;
                    *--nextChar = (char)(nextValue32 < 10 ? nextValue32 + '0' : (nextValue32 + ('0' + 'A' - '9' - 1)));
                    value32 >>= 4;
                }
                *--nextChar = (char)(value32 < 10 ? value32 + '0' : (value32 + ('0' + 'A' - '9' - 1)));
                *(ulong*)(nextChar -= 4) = hexPrefix;
                nextChar += ++startIndex;
                return new TmphKeyValue<int, int>((int)(nextChar - chars), 20 - (int)(nextChar - chars));
            }
        }

        /// <summary>
        ///     数字转换成字符串
        /// </summary>
        /// <param name="value">数字值</param>
        /// <param name="jsonStream">JSON输出流</param>
        /// <param name="isMaxToString">超出最大有效精度是否转换成字符串</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static unsafe void ToString(long value, TmphCharStream jsonStream, bool isMaxToString = true)
        {
            if ((ulong)(value + maxValue) <= maxValue << 1 || !isMaxToString)
            {
                char* chars = stackalloc char[20];
                var index = toString(value, chars);
                jsonStream.Write(chars + index.Key, index.Value);
            }
            else
            {
                var unsafeStraem = jsonStream.Unsafer;
                jsonStream.PrepLength(24);
                unsafeStraem.Write(Quote);
                TmphNumber.ToString(value, jsonStream);
                unsafeStraem.Write(Quote);
            }
        }

        /// <summary>
        ///     数字转换成字符串
        /// </summary>
        /// <param name="value">数字值</param>
        /// <returns>数字字符串</returns>
        private static unsafe TmphKeyValue<int, int> toString(ulong value, char* chars)
        {
            if (value < 10000)
            {
                var value32 = (int)value;
                var nextChar = chars + 4;
                if (value32 < 10) *nextChar = (char)(value32 + '0');
                else
                {
                    var div = (value32 * (int)TmphNumber.Div10_16Mul) >> TmphNumber.Div10_16Shift;
                    *nextChar = (char)((value32 - div * 10) + '0');
                    if (div < 10) *--nextChar = (char)(div + '0');
                    else
                    {
                        value32 = (div * (int)TmphNumber.Div10_16Mul) >> TmphNumber.Div10_16Shift;
                        *--nextChar = (char)((div - value32 * 10) + '0');
                        if (value32 < 10) *--nextChar = (char)(value32 + '0');
                        else
                        {
                            div = (value32 * (int)TmphNumber.Div10_16Mul) >> TmphNumber.Div10_16Shift;
                            *--nextChar = (char)((value32 - div * 10) + '0');
                            *--nextChar = (char)(div + '0');
                        }
                    }
                }
                return new TmphKeyValue<int, int>((int)(nextChar - chars), 5 - (int)(nextChar - chars));
            }
            else
            {
                var nextChar = chars + 19;
                var nextValue = value & 15;
                *nextChar = (char)(nextValue < 10 ? nextValue + '0' : (nextValue + ('0' + 'A' - '9' - 1)));
                nextValue = (value >>= 4) & 15;
                *--nextChar = (char)(nextValue < 10 ? nextValue + '0' : (nextValue + ('0' + 'A' - '9' - 1)));
                nextValue = (value >>= 4) & 15;
                *--nextChar = (char)(nextValue < 10 ? nextValue + '0' : (nextValue + ('0' + 'A' - '9' - 1)));
                nextValue = (value >>= 4) & 15;
                *--nextChar = (char)(nextValue < 10 ? nextValue + '0' : (nextValue + ('0' + 'A' - '9' - 1)));
                if (value >= 0x1000000000L)
                {
                    nextValue = (value >>= 4) & 15;
                    *--nextChar = (char)(nextValue < 10 ? nextValue + '0' : (nextValue + ('0' + 'A' - '9' - 1)));
                    nextValue = (value >>= 4) & 15;
                    *--nextChar = (char)(nextValue < 10 ? nextValue + '0' : (nextValue + ('0' + 'A' - '9' - 1)));
                    nextValue = (value >>= 4) & 15;
                    *--nextChar = (char)(nextValue < 10 ? nextValue + '0' : (nextValue + ('0' + 'A' - '9' - 1)));
                    nextValue = (value >>= 4) & 15;
                    *--nextChar = (char)(nextValue < 10 ? nextValue + '0' : (nextValue + ('0' + 'A' - '9' - 1)));
                }
                uint value32 = (uint)(value >> 4), nextValue32;
                if (value32 >= 0x10000)
                {
                    nextValue32 = value32 & 15;
                    *--nextChar = (char)(nextValue32 < 10 ? nextValue32 + '0' : (nextValue32 + ('0' + 'A' - '9' - 1)));
                    nextValue32 = (value32 >>= 4) & 15;
                    *--nextChar = (char)(nextValue32 < 10 ? nextValue32 + '0' : (nextValue32 + ('0' + 'A' - '9' - 1)));
                    nextValue32 = (value32 >>= 4) & 15;
                    *--nextChar = (char)(nextValue32 < 10 ? nextValue32 + '0' : (nextValue32 + ('0' + 'A' - '9' - 1)));
                    nextValue32 = (value32 >>= 4) & 15;
                    *--nextChar = (char)(nextValue32 < 10 ? nextValue32 + '0' : (nextValue32 + ('0' + 'A' - '9' - 1)));
                    value32 >>= 4;
                }
                if (value32 >= 0x100)
                {
                    nextValue32 = value32 & 15;
                    *--nextChar = (char)(nextValue32 < 10 ? nextValue32 + '0' : (nextValue32 + ('0' + 'A' - '9' - 1)));
                    nextValue32 = (value32 >>= 4) & 15;
                    *--nextChar = (char)(nextValue32 < 10 ? nextValue32 + '0' : (nextValue32 + ('0' + 'A' - '9' - 1)));
                    value32 >>= 4;
                }
                if (value32 >= 0x10)
                {
                    nextValue32 = value32 & 15;
                    *--nextChar = (char)(nextValue32 < 10 ? nextValue32 + '0' : (nextValue32 + ('0' + 'A' - '9' - 1)));
                    value32 >>= 4;
                }
                *--nextChar = (char)(value32 < 10 ? value32 + '0' : (value32 + ('0' + 'A' - '9' - 1)));
                *(uint*)(nextChar -= 2) = (uint)(hexPrefix >> 32);
                return new TmphKeyValue<int, int>((int)(nextChar - chars), 20 - (int)(nextChar - chars));
            }
        }

        /// <summary>
        ///     数字转换成字符串
        /// </summary>
        /// <param name="value">数字值</param>
        /// <param name="jsonStream">JSON输出流</param>
        /// <param name="isMaxToString">超出最大有效精度是否转换成字符串</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static unsafe void ToString(ulong value, TmphCharStream jsonStream, bool isMaxToString = true)
        {
            if (value <= maxValue || !isMaxToString)
            {
                char* chars = stackalloc char[20];
                var index = toString(value, chars);
                jsonStream.Write(chars + index.Key, index.Value);
            }
            else
            {
                var unsafeStraem = jsonStream.Unsafer;
                jsonStream.PrepLength(22);
                unsafeStraem.Write(Quote);
                TmphNumber.ToString(value, jsonStream);
                unsafeStraem.Write(Quote);
            }
        }

        /// <summary>
        ///     数字转换成字符串
        /// </summary>
        /// <param name="value">数字值</param>
        /// <returns>数字字符串</returns>
        private static unsafe TmphKeyValue<int, int> toString(int value, char* chars)
        {
            var startIndex = 1;
            if (value < 0)
            {
                value = -value;
                startIndex = 0;
            }
            if (value < 10000)
            {
                var nextChar = chars + 4;
                if (value < 10) *nextChar = (char)(value + '0');
                else
                {
                    var div = (value * (int)TmphNumber.Div10_16Mul) >> TmphNumber.Div10_16Shift;
                    *nextChar = (char)((value - div * 10) + '0');
                    if (div < 10) *--nextChar = (char)(div + '0');
                    else
                    {
                        value = (div * (int)TmphNumber.Div10_16Mul) >> TmphNumber.Div10_16Shift;
                        *--nextChar = (char)((div - value * 10) + '0');
                        if (value < 10) *--nextChar = (char)(value + '0');
                        else
                        {
                            div = (value * (int)TmphNumber.Div10_16Mul) >> TmphNumber.Div10_16Shift;
                            *--nextChar = (char)((value - div * 10) + '0');
                            *--nextChar = (char)(div + '0');
                        }
                    }
                }
                *--nextChar = '-';
                nextChar += startIndex;
                return new TmphKeyValue<int, int>((int)(nextChar - chars), 5 - (int)(nextChar - chars));
            }
            else
            {
                var nextChar = chars + 11;
                var nextValue = value & 15;
                *nextChar = (char)(nextValue < 10 ? nextValue + '0' : (nextValue + ('0' + 'A' - '9' - 1)));
                nextValue = (value >>= 4) & 15;
                *--nextChar = (char)(nextValue < 10 ? nextValue + '0' : (nextValue + ('0' + 'A' - '9' - 1)));
                nextValue = (value >>= 4) & 15;
                *--nextChar = (char)(nextValue < 10 ? nextValue + '0' : (nextValue + ('0' + 'A' - '9' - 1)));
                nextValue = (value >>= 4) & 15;
                *--nextChar = (char)(nextValue < 10 ? nextValue + '0' : (nextValue + ('0' + 'A' - '9' - 1)));
                if (value >= 0x1000)
                {
                    nextValue = (value >>= 4) & 15;
                    *--nextChar = (char)(nextValue < 10 ? nextValue + '0' : (nextValue + ('0' + 'A' - '9' - 1)));
                    nextValue = (value >>= 4) & 15;
                    *--nextChar = (char)(nextValue < 10 ? nextValue + '0' : (nextValue + ('0' + 'A' - '9' - 1)));
                }
                if (value >= 0x100)
                {
                    nextValue = (value >>= 4) & 15;
                    *--nextChar = (char)(nextValue < 10 ? nextValue + '0' : (nextValue + ('0' + 'A' - '9' - 1)));
                }
                *--nextChar = (char)((value >>= 4) < 10 ? value + '0' : (value + ('0' + 'A' - '9' - 1)));
                *(ulong*)(nextChar -= 4) = hexPrefix;
                nextChar += ++startIndex;
                return new TmphKeyValue<int, int>((int)(nextChar - chars), 12 - (int)(nextChar - chars));
            }
        }

        /// <summary>
        ///     数字转换成字符串
        /// </summary>
        /// <param name="value">数字值</param>
        /// <param name="jsonStream">JSON输出流</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static unsafe void ToString(int value, TmphCharStream jsonStream)
        {
            char* chars = stackalloc char[12];
            var index = toString(value, chars);
            jsonStream.Write(chars + index.Key, index.Value);
        }

        /// <summary>
        ///     数字转换成字符串
        /// </summary>
        /// <param name="value">数字值</param>
        /// <returns>数字字符串</returns>
        private static unsafe TmphKeyValue<int, int> toString(uint value, char* chars)
        {
            if (value < 10000)
            {
                var nextChar = chars + 4;
                if (value < 10) *nextChar = (char)(value + '0');
                else
                {
                    var div = (value * TmphNumber.Div10_16Mul) >> TmphNumber.Div10_16Shift;
                    *nextChar = (char)((value - div * 10) + '0');
                    if (div < 10) *--nextChar = (char)(div + '0');
                    else
                    {
                        value = (div * TmphNumber.Div10_16Mul) >> TmphNumber.Div10_16Shift;
                        *--nextChar = (char)((div - value * 10) + '0');
                        if (value < 10) *--nextChar = (char)(value + '0');
                        else
                        {
                            div = (value * TmphNumber.Div10_16Mul) >> TmphNumber.Div10_16Shift;
                            *--nextChar = (char)((value - div * 10) + '0');
                            *--nextChar = (char)(div + '0');
                        }
                    }
                }
                return new TmphKeyValue<int, int>((int)(nextChar - chars), 5 - (int)(nextChar - chars));
            }
            else
            {
                var nextChar = chars + 11;
                var nextValue = value & 15;
                *nextChar = (char)(nextValue < 10 ? nextValue + '0' : (nextValue + ('0' + 'A' - '9' - 1)));
                nextValue = (value >>= 4) & 15;
                *--nextChar = (char)(nextValue < 10 ? nextValue + '0' : (nextValue + ('0' + 'A' - '9' - 1)));
                nextValue = (value >>= 4) & 15;
                *--nextChar = (char)(nextValue < 10 ? nextValue + '0' : (nextValue + ('0' + 'A' - '9' - 1)));
                nextValue = (value >>= 4) & 15;
                *--nextChar = (char)(nextValue < 10 ? nextValue + '0' : (nextValue + ('0' + 'A' - '9' - 1)));
                if (value >= 0x1000)
                {
                    nextValue = (value >>= 4) & 15;
                    *--nextChar = (char)(nextValue < 10 ? nextValue + '0' : (nextValue + ('0' + 'A' - '9' - 1)));
                    nextValue = (value >>= 4) & 15;
                    *--nextChar = (char)(nextValue < 10 ? nextValue + '0' : (nextValue + ('0' + 'A' - '9' - 1)));
                }
                if (value >= 0x100)
                {
                    nextValue = (value >>= 4) & 15;
                    *--nextChar = (char)(nextValue < 10 ? nextValue + '0' : (nextValue + ('0' + 'A' - '9' - 1)));
                }
                *--nextChar = (char)((value >>= 4) < 10 ? value + '0' : (value + ('0' + 'A' - '9' - 1)));
                *(uint*)(nextChar -= 2) = (uint)(hexPrefix >> 32);
                return new TmphKeyValue<int, int>((int)(nextChar - chars), 12 - (int)(nextChar - chars));
            }
        }

        /// <summary>
        ///     数字转换成字符串
        /// </summary>
        /// <param name="value">数字值</param>
        /// <param name="jsonStream">JSON输出流</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static unsafe void ToString(uint value, TmphCharStream jsonStream)
        {
            char* chars = stackalloc char[12];
            var index = toString(value, chars);
            jsonStream.Write(chars + index.Key, index.Value);
        }

        /// <summary>
        ///     数字转换成字符串
        /// </summary>
        /// <param name="value">数字值</param>
        /// <returns>数字字符串</returns>
        private static unsafe TmphKeyValue<int, int> toString(short value16, char* chars)
        {
            int startIndex = 1, value = value16;
            if (value < 0)
            {
                value = -value;
                startIndex = 0;
            }
            if (value < 10000)
            {
                var nextChar = chars + 4;
                if (value < 10) *nextChar = (char)(value + '0');
                else
                {
                    var div = (value * (int)TmphNumber.Div10_16Mul) >> TmphNumber.Div10_16Shift;
                    *nextChar = (char)((value - div * 10) + '0');
                    if (div < 10) *--nextChar = (char)(div + '0');
                    else
                    {
                        value = (div * (int)TmphNumber.Div10_16Mul) >> TmphNumber.Div10_16Shift;
                        *--nextChar = (char)((div - value * 10) + '0');
                        if (value < 10) *--nextChar = (char)(value + '0');
                        else
                        {
                            div = (value * (int)TmphNumber.Div10_16Mul) >> TmphNumber.Div10_16Shift;
                            *--nextChar = (char)((value - div * 10) + '0');
                            *--nextChar = (char)(div + '0');
                        }
                    }
                }
                *--nextChar = '-';
                nextChar += startIndex;
                return new TmphKeyValue<int, int>((int)(nextChar - chars), 5 - (int)(nextChar - chars));
            }
            else
            {
                var nextChar = chars + 7;
                var nextValue = value & 15;
                *nextChar = (char)(nextValue < 10 ? nextValue + '0' : (nextValue + ('0' + 'A' - '9' - 1)));
                nextValue = (value >>= 4) & 15;
                *--nextChar = (char)(nextValue < 10 ? nextValue + '0' : (nextValue + ('0' + 'A' - '9' - 1)));
                nextValue = (value >>= 4) & 15;
                *--nextChar = (char)(nextValue < 10 ? nextValue + '0' : (nextValue + ('0' + 'A' - '9' - 1)));
                *--nextChar = (char)((value >> 4) + '0');
                *(ulong*)(nextChar -= 4) = hexPrefix;
                nextChar += ++startIndex;
                return new TmphKeyValue<int, int>((int)(nextChar - chars), 8 - (int)(nextChar - chars));
            }
        }

        /// <summary>
        ///     数字转换成字符串
        /// </summary>
        /// <param name="value">数字值</param>
        /// <param name="jsonStream">JSON输出流</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static unsafe void ToString(short value, TmphCharStream jsonStream)
        {
            char* chars = stackalloc char[8];
            var index = toString(value, chars);
            jsonStream.Write(chars + index.Key, index.Value);
        }

        /// <summary>
        ///     数字转换成字符串
        /// </summary>
        /// <param name="value">数字值</param>
        /// <returns>数字字符串</returns>
        private static unsafe TmphKeyValue<int, int> toString(ushort value16, char* chars)
        {
            int value = value16;
            if (value < 10000)
            {
                var nextChar = chars + 4;
                if (value < 10) *nextChar = (char)(value + '0');
                else
                {
                    var div = (value * (int)TmphNumber.Div10_16Mul) >> TmphNumber.Div10_16Shift;
                    *nextChar = (char)((value - div * 10) + '0');
                    if (div < 10) *--nextChar = (char)(div + '0');
                    else
                    {
                        value = (div * (int)TmphNumber.Div10_16Mul) >> TmphNumber.Div10_16Shift;
                        *--nextChar = (char)((div - value * 10) + '0');
                        if (value < 10) *--nextChar = (char)(value + '0');
                        else
                        {
                            div = (value * (int)TmphNumber.Div10_16Mul) >> TmphNumber.Div10_16Shift;
                            *--nextChar = (char)((value - div * 10) + '0');
                            *--nextChar = (char)(div + '0');
                        }
                    }
                }
                return new TmphKeyValue<int, int>((int)(nextChar - chars), 5 - (int)(nextChar - chars));
            }
            else
            {
                var nextChar = chars + 7;
                var nextValue = value & 15;
                *nextChar = (char)(nextValue < 10 ? nextValue + '0' : (nextValue + ('0' + 'A' - '9' - 1)));
                nextValue = (value >>= 4) & 15;
                *--nextChar = (char)(nextValue < 10 ? nextValue + '0' : (nextValue + ('0' + 'A' - '9' - 1)));
                nextValue = (value >>= 4) & 15;
                *--nextChar = (char)(nextValue < 10 ? nextValue + '0' : (nextValue + ('0' + 'A' - '9' - 1)));
                nextValue = value >> 4;
                *--nextChar = (char)(nextValue < 10 ? nextValue + '0' : (nextValue + ('0' + 'A' - '9' - 1)));
                *(uint*)(nextChar -= 2) = (uint)(hexPrefix >> 32);
                return new TmphKeyValue<int, int>((int)(nextChar - chars), 8 - (int)(nextChar - chars));
            }
        }

        /// <summary>
        ///     数字转换成字符串
        /// </summary>
        /// <param name="value">数字值</param>
        /// <param name="jsonStream">JSON输出流</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static unsafe void ToString(ushort value, TmphCharStream jsonStream)
        {
            char* chars = stackalloc char[8];
            var index = toString(value, chars);
            jsonStream.Write(chars + index.Key, index.Value);
        }

        /// <summary>
        ///     数字转换成字符串
        /// </summary>
        /// <param name="value">数字值</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private static unsafe void toString(sbyte value, char* chars)
        {
            *(ulong*)chars = hexPrefix;
            *(chars + 4) = (char)((value >> 4) + '0');
            *(chars + 5) = (char)((value &= 15) < 10 ? value + '0' : (value + ('0' + 'A' - '9' - 1)));
        }

        /// <summary>
        ///     数字转换成字符串
        /// </summary>
        /// <param name="value">数字值</param>
        /// <param name="jsonStream">JSON输出流</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static unsafe void ToString(sbyte value, TmphCharStream jsonStream)
        {
            if (value == 0) jsonStream.Write('0');
            else
            {
                char* chars = stackalloc char[6];
                if (value < 0)
                {
                    if (value == -128)
                    {
                        jsonStream.PrepLength(4);
                        var unsafeStream = jsonStream.Unsafer;
                        unsafeStream.Write('-');
                        unsafeStream.Write('1');
                        unsafeStream.Write('2');
                        unsafeStream.Write('8');
                    }
                    else
                    {
                        toString((sbyte)-value, chars);
                        jsonStream.Write(chars + 1, 5);
                    }
                }
                else
                {
                    toString(value, chars);
                    jsonStream.Write(chars + 2, 4);
                }
            }
        }

        /// <summary>
        ///     数字转换成字符串
        /// </summary>
        /// <param name="value">数字值</param>
        /// <param name="jsonStream">JSON输出流</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static unsafe void ToString(byte value, TmphCharStream jsonStream)
        {
            if (value == 0) jsonStream.Write('0');
            else
            {
                jsonStream.PrepLength(4);
                var data = (byte*)jsonStream.CurrentChar;
                var nextValue = value >> 4;
                *(char*)data = '0';
                *(char*)(data + sizeof(char)) = 'x';
                *(char*)(data + sizeof(char) * 2) =
                    (char)(nextValue < 10 ? nextValue + '0' : (nextValue + ('0' + 'A' - '9' - 1)));
                *(char*)(data + sizeof(char) * 3) =
                    (char)((value &= 15) < 10 ? value + '0' : (value + ('0' + 'A' - '9' - 1)));
                jsonStream.Unsafer.AddLength(4);
            }
        }

        /// <summary>
        ///     数字转换成字符串
        /// </summary>
        /// <param name="value">数字值</param>
        /// <param name="jsonStream">JSON输出流</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static void ToString(bool value, TmphCharStream jsonStream)
        {
            jsonStream.Write(value ? '1' : '0');
        }

        /// <summary>
        ///     单精度浮点数转字符串
        /// </summary>
        /// <param name="value">单精度浮点数</param>
        /// <param name="jsonStream">JSON输出流</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static void ToString(float value, TmphCharStream jsonStream)
        {
            jsonStream.WriteNotNull(value.ToString());
        }

        /// <summary>
        ///     双精度浮点数转字符串
        /// </summary>
        /// <param name="value">双精度浮点数</param>
        /// <param name="jsonStream">JSON输出流</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static void ToString(double value, TmphCharStream jsonStream)
        {
            jsonStream.WriteNotNull(value.ToString());
        }

        /// <summary>
        ///     十进制数转字符串
        /// </summary>
        /// <param name="value">十进制数</param>
        /// <param name="jsonStream">JSON输出流</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static void ToString(decimal value, TmphCharStream jsonStream)
        {
            jsonStream.WriteNotNull(value.ToString());
        }

        /// <summary>
        ///     时间转字符串
        /// </summary>
        /// <param name="time">时间</param>
        /// <param name="jsonStream">JSON输出流</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static void ToString(DateTime time, TmphCharStream jsonStream)
        {
            if (time != DateTime.MinValue) ToStringNotNull(time, jsonStream);
            else WriteNull(jsonStream);
        }

        /// <summary>
        ///     时间转字符串
        /// </summary>
        /// <param name="time">时间</param>
        /// <param name="jsonStream">JSON输出流</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static unsafe void ToStringNotNull(DateTime time, TmphCharStream jsonStream)
        {
            char* chars = stackalloc char[20];
            var index = toString((time.Ticks - JavascriptMinTimeTicks) / TmphDate.MillisecondTicks, chars);
            jsonStream.PrepLength(index.Value + DateStart.Length + 1);
            var unsafeStraem = jsonStream.Unsafer;
            fixed (char* dataFixed = DateStart) unsafeStraem.Write(dataFixed, DateStart.Length);
            unsafeStraem.Write(chars + index.Key, index.Value);
            unsafeStraem.Write(DateEnd);
        }

        /// <summary>
        ///     Guid转换成字符串
        /// </summary>
        /// <param name="Guid">Guid</param>
        /// <param name="jsonStream">JSON输出流</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static unsafe void ToString(Guid value, TmphCharStream jsonStream)
        {
            var guid = new TmphGuid { Value = value };
            jsonStream.PrepLength(38);
            var data = (byte*)jsonStream.CurrentChar;
            int high = guid.Byte3 >> 4, low = guid.Byte3 & 15;
            *(char*)data = Quote;
            *(char*)(data + sizeof(char)) = (char)(high < 10 ? high + '0' : (high + ('0' + 'A' - '9' - 1)));
            *(char*)(data + sizeof(char) * 2) = (char)(low < 10 ? low + '0' : (low + ('0' + 'A' - '9' - 1)));
            high = guid.Byte2 >> 4;
            low = guid.Byte2 & 15;
            *(char*)(data + sizeof(char) * 3) = (char)(high < 10 ? high + '0' : (high + ('0' + 'A' - '9' - 1)));
            *(char*)(data + sizeof(char) * 4) = (char)(low < 10 ? low + '0' : (low + ('0' + 'A' - '9' - 1)));
            high = guid.Byte1 >> 4;
            low = guid.Byte1 & 15;
            *(char*)(data + sizeof(char) * 5) = (char)(high < 10 ? high + '0' : (high + ('0' + 'A' - '9' - 1)));
            *(char*)(data + sizeof(char) * 6) = (char)(low < 10 ? low + '0' : (low + ('0' + 'A' - '9' - 1)));
            high = guid.Byte0 >> 4;
            low = guid.Byte0 & 15;
            *(char*)(data + sizeof(char) * 7) = (char)(high < 10 ? high + '0' : (high + ('0' + 'A' - '9' - 1)));
            *(char*)(data + sizeof(char) * 8) = (char)(low < 10 ? low + '0' : (low + ('0' + 'A' - '9' - 1)));
            *(char*)(data + sizeof(char) * 9) = '-';
            high = guid.Byte5 >> 4;
            low = guid.Byte5 & 15;
            *(char*)(data + sizeof(char) * 10) = (char)(high < 10 ? high + '0' : (high + ('0' + 'A' - '9' - 1)));
            *(char*)(data + sizeof(char) * 11) = (char)(low < 10 ? low + '0' : (low + ('0' + 'A' - '9' - 1)));
            high = guid.Byte4 >> 4;
            low = guid.Byte4 & 15;
            *(char*)(data + sizeof(char) * 12) = (char)(high < 10 ? high + '0' : (high + ('0' + 'A' - '9' - 1)));
            *(char*)(data + sizeof(char) * 13) = (char)(low < 10 ? low + '0' : (low + ('0' + 'A' - '9' - 1)));
            *(char*)(data + sizeof(char) * 14) = '-';
            high = guid.Byte7 >> 4;
            low = guid.Byte7 & 15;
            *(char*)(data + sizeof(char) * 15) = (char)(high < 10 ? high + '0' : (high + ('0' + 'A' - '9' - 1)));
            *(char*)(data + sizeof(char) * 16) = (char)(low < 10 ? low + '0' : (low + ('0' + 'A' - '9' - 1)));
            high = guid.Byte6 >> 4;
            low = guid.Byte6 & 15;
            *(char*)(data + sizeof(char) * 17) = (char)(high < 10 ? high + '0' : (high + ('0' + 'A' - '9' - 1)));
            *(char*)(data + sizeof(char) * 18) = (char)(low < 10 ? low + '0' : (low + ('0' + 'A' - '9' - 1)));
            *(char*)(data + sizeof(char) * 19) = '-';
            high = guid.Byte8 >> 4;
            low = guid.Byte8 & 15;
            *(char*)(data + sizeof(char) * 20) = (char)(high < 10 ? high + '0' : (high + ('0' + 'A' - '9' - 1)));
            *(char*)(data + sizeof(char) * 21) = (char)(low < 10 ? low + '0' : (low + ('0' + 'A' - '9' - 1)));
            high = guid.Byte9 >> 4;
            low = guid.Byte9 & 15;
            *(char*)(data + sizeof(char) * 22) = (char)(high < 10 ? high + '0' : (high + ('0' + 'A' - '9' - 1)));
            *(char*)(data + sizeof(char) * 23) = (char)(low < 10 ? low + '0' : (low + ('0' + 'A' - '9' - 1)));
            *(char*)(data + sizeof(char) * 24) = '-';
            high = guid.Byte10 >> 4;
            low = guid.Byte10 & 15;
            *(char*)(data + sizeof(char) * 25) = (char)(high < 10 ? high + '0' : (high + ('0' + 'A' - '9' - 1)));
            *(char*)(data + sizeof(char) * 26) = (char)(low < 10 ? low + '0' : (low + ('0' + 'A' - '9' - 1)));
            high = guid.Byte11 >> 4;
            low = guid.Byte11 & 15;
            *(char*)(data + sizeof(char) * 27) = (char)(high < 10 ? high + '0' : (high + ('0' + 'A' - '9' - 1)));
            *(char*)(data + sizeof(char) * 28) = (char)(low < 10 ? low + '0' : (low + ('0' + 'A' - '9' - 1)));
            high = guid.Byte12 >> 4;
            low = guid.Byte12 & 15;
            *(char*)(data + sizeof(char) * 29) = (char)(high < 10 ? high + '0' : (high + ('0' + 'A' - '9' - 1)));
            *(char*)(data + sizeof(char) * 30) = (char)(low < 10 ? low + '0' : (low + ('0' + 'A' - '9' - 1)));
            high = guid.Byte13 >> 4;
            low = guid.Byte13 & 15;
            *(char*)(data + sizeof(char) * 31) = (char)(high < 10 ? high + '0' : (high + ('0' + 'A' - '9' - 1)));
            *(char*)(data + sizeof(char) * 32) = (char)(low < 10 ? low + '0' : (low + ('0' + 'A' - '9' - 1)));
            high = guid.Byte14 >> 4;
            low = guid.Byte14 & 15;
            *(char*)(data + sizeof(char) * 33) = (char)(high < 10 ? high + '0' : (high + ('0' + 'A' - '9' - 1)));
            *(char*)(data + sizeof(char) * 34) = (char)(low < 10 ? low + '0' : (low + ('0' + 'A' - '9' - 1)));
            high = guid.Byte15 >> 4;
            low = guid.Byte15 & 15;
            *(char*)(data + sizeof(char) * 35) = (char)(high < 10 ? high + '0' : (high + ('0' + 'A' - '9' - 1)));
            *(char*)(data + sizeof(char) * 36) = (char)(low < 10 ? low + '0' : (low + ('0' + 'A' - '9' - 1)));
            *(char*)(data + sizeof(char) * 37) = Quote;
            jsonStream.Unsafer.AddLength(38);
        }

        /// <summary>
        ///     字符
        /// </summary>
        /// <param name="value">字符</param>
        /// <param name="jsonStream">JSON输出流</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static void ToString(char value, TmphCharStream jsonStream)
        {
            jsonStream.PrepLength(3);
            var unsafeStream = jsonStream.Unsafer;
            unsafeStream.Write(Quote);
            unsafeStream.Write(value);
            unsafeStream.Write(Quote);
        }

        /// <summary>
        ///     字符串
        /// </summary>
        /// <param name="value">字符串</param>
        /// <param name="jsonStream">JSON输出流</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static void ToString(string value, TmphCharStream jsonStream)
        {
            jsonStream.PrepLength(value.Length() + 2);
            jsonStream.Unsafer.Write(Quote);
            jsonStream.Write(value);
            jsonStream.Unsafer.Write(Quote);
        }

        /// <summary>
        ///     字符串
        /// </summary>
        /// <param name="value">字符串</param>
        /// <param name="jsonStream">JSON输出流</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static void ToString(TmphSubString value, TmphCharStream jsonStream)
        {
            if (value.value == null) WriteNull(jsonStream);
            else
            {
                jsonStream.PrepLength(value.Length + 2);
                jsonStream.Unsafer.Write(Quote);
                jsonStream.Write(value);
                jsonStream.Unsafer.Write(Quote);
            }
        }
    }
}