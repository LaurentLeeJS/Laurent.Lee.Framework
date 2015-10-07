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
using System.Timers;

namespace Laurent.Lee.CLB
{
    /// <summary>
    ///     日期相关操作
    /// </summary>
    public static unsafe class TmphDate
    {
        /// <summary>
        ///     32位除以60转乘法的乘数
        /// </summary>
        public const ulong Div60_32Mul = ((1L << Div60_32Shift) + 59) / 60;

        /// <summary>
        ///     32位除以60转乘法的位移
        /// </summary>
        public const int Div60_32Shift = 21 + 32;

        /// <summary>
        ///     16位除以60转乘法的乘数
        /// </summary>
        public const uint Div60_16Mul = ((1U << Div60_16Shift) + 59) / 60;

        /// <summary>
        ///     16位除以60转乘法的位移
        /// </summary>
        public const int Div60_16Shift = 21;

        /// <summary>
        ///     时间转换字符串字节长度
        /// </summary>
        public const int SqlMillisecondSize = 23;

        /// <summary>
        ///     时间转字节流长度
        /// </summary>
        internal const int ToByteLength = 29;

        /// <summary>
        ///     每毫秒计时周期数
        /// </summary>
        public static readonly long MillisecondTicks = new TimeSpan(0, 0, 0, 0, 1).Ticks;

        /// <summary>
        ///     每秒计时周期数
        /// </summary>
        public static readonly long SecondTicks = MillisecondTicks * 1000;

        /// <summary>
        ///     每分钟计时周期数
        /// </summary>
        public static readonly long MinutesTicks = SecondTicks * 60;

        /// <summary>
        ///     一天的计时周期数
        /// </summary>
        public static readonly long DayTiks = 24L * 60L * 60L * SecondTicks;

        /// <summary>
        ///     星期
        /// </summary>
        private static readonly TmphPointer weekData;

        /// <summary>
        ///     月份
        /// </summary>
        private static readonly TmphPointer monthData;

        /// <summary>
        ///     时间字节流缓冲区
        /// </summary>
        internal static readonly TmphMemoryPool ByteBuffers = TmphMemoryPool.GetPool(ToByteLength);

        static TmphDate()
        {
            var dataIndex = 0;
            var datas = TmphUnmanaged.Get(false, 7 * sizeof(int), 12 * sizeof(int));
            weekData = datas[dataIndex++];
            monthData = datas[dataIndex++];

            var write = weekData.Int;
            *write = 'S' + ('u' << 8) + ('n' << 16) + (',' << 24);
            *++write = 'M' + ('o' << 8) + ('n' << 16) + (',' << 24);
            *++write = 'T' + ('u' << 8) + ('e' << 16) + (',' << 24);
            *++write = 'W' + ('e' << 8) + ('d' << 16) + (',' << 24);
            *++write = 'T' + ('h' << 8) + ('u' << 16) + (',' << 24);
            *++write = 'F' + ('r' << 8) + ('i' << 16) + (',' << 24);
            *++write = 'S' + ('a' << 8) + ('t' << 16) + (',' << 24);

            write = monthData.Int;
            *write = 'J' + ('a' << 8) + ('n' << 16) + (' ' << 24);
            *++write = 'F' + ('e' << 8) + ('b' << 16) + (' ' << 24);
            *++write = 'M' + ('a' << 8) + ('r' << 16) + (' ' << 24);
            *++write = 'A' + ('p' << 8) + ('r' << 16) + (' ' << 24);
            *++write = 'M' + ('a' << 8) + ('y' << 16) + (' ' << 24);
            *++write = 'J' + ('u' << 8) + ('n' << 16) + (' ' << 24);
            *++write = 'J' + ('u' << 8) + ('l' << 16) + (' ' << 24);
            *++write = 'A' + ('u' << 8) + ('g' << 16) + (' ' << 24);
            *++write = 'S' + ('e' << 8) + ('p' << 16) + (' ' << 24);
            *++write = 'O' + ('c' << 8) + ('t' << 16) + (' ' << 24);
            *++write = 'N' + ('o' << 8) + ('v' << 16) + (' ' << 24);
            *++write = 'D' + ('e' << 8) + ('c' << 16) + (' ' << 24);
        }

        /// <summary>
        ///     精确到秒的时间
        /// </summary>
        public static DateTime NowSecond
        {
            get { return TmphNowTime.Now; }
        }

        /// <summary>
        ///     DateTime.Now
        /// </summary>
        public static DateTime Now
        {
            get { return TmphNowTime.Now = DateTime.Now; }
        }

        /// <summary>
        ///     时间更新间隔
        /// </summary>
        internal static int NowTimerInterval
        {
            get { return (int)TmphNowTime.Timer.Interval; }
        }

        /// <summary>
        ///     时间转换成字符串(精确到秒)
        /// </summary>
        /// <param name="time">时间</param>
        /// <returns>时间字符串</returns>
        public static string toString(this DateTime time)
        {
            var timeString = TmphString.FastAllocateString(19);
            fixed (char* timeFixed = timeString) toString(time, time.Ticks % DayTiks, timeFixed);
            return timeString;
        }

        /// <summary>
        ///     时间转换成字符串(精确到秒)
        /// </summary>
        /// <param name="time">时间</param>
        /// <param name="dayTiks">当天的计时周期数</param>
        /// <param name="chars">时间字符串</param>
        private static void toString(DateTime time, long dayTiks, char* chars)
        {
            int data0 = time.Year, data1 = (data0 * (int)TmphNumber.Div10_16Mul) >> TmphNumber.Div10_16Shift;
            data0 -= data1 * 10;
            var data2 = (data1 * (int)TmphNumber.Div10_16Mul) >> TmphNumber.Div10_16Shift;
            data1 -= data2 * 10;
            var data3 = (data2 * (int)TmphNumber.Div10_16Mul) >> TmphNumber.Div10_16Shift;
            data2 -= data3 * 10;
            data0 += '0';
            data1 += '0';
            data0 <<= 16;
            *(int*)(chars + 2) = (data1 += data0);
            data2 += '0';
            data3 += '0';
            data2 <<= 16;
            *(int*)chars = (data3 += data2);

            data0 = time.Month;
            data1 = (data0 + 6) >> 4;
            data0 -= data1 * 10;
            data1 += '0';
            data0 += '0';
            data1 <<= 16;
            *(int*)(chars + 6) = (data0 += ('/' << 16));
            *(int*)(chars + 4) = (data1 += '/');

            data0 = time.Day;
            data1 = (data0 * (int)TmphNumber.Div10_16Mul) >> TmphNumber.Div10_16Shift;
            data0 -= data1 * 10;
            data1 += '0';
            data0 += '0';
            data0 <<= 16;
            *(int*)(chars + 8) = (data1 += data0);

            data3 = (int)(dayTiks / (1000 * 10000));
            data2 = (int)(((ulong)data3 * Div60_32Mul) >> Div60_32Shift);
            data3 -= data2 * 60;
            data0 = (data2 * (int)Div60_16Mul) >> Div60_16Shift;
            data2 -= data0 * 60;

            data1 = (data0 * (int)TmphNumber.Div10_16Mul) >> TmphNumber.Div10_16Shift;
            data0 -= data1 * 10;
            data1 += '0';
            data0 += '0';
            data1 <<= 16;
            *(int*)(chars + 12) = (data0 += (':' << 16));
            *(int*)(chars + 10) = (data1 += ' ');

            data1 = (data2 * (int)TmphNumber.Div10_16Mul) >> TmphNumber.Div10_16Shift;
            data2 -= data1 * 10;
            data1 += '0';
            data2 += '0';
            data2 <<= 16;
            *(int*)(chars + 14) = (data1 += data2);

            data1 = (data3 * (int)TmphNumber.Div10_16Mul) >> TmphNumber.Div10_16Shift;
            data3 -= data1 * 10;
            data1 += '0';
            data3 += '0';
            data1 <<= 16;
            *(chars + 18) = (char)data3;
            *(int*)(chars + 16) = (data1 += ':');
        }

        /// <summary>
        ///     时间转换成字符串(精确到毫秒)
        /// </summary>
        /// <param name="time">时间</param>
        /// <param name="charStream">字符流</param>
        internal static void ToSqlMillisecond(DateTime time, TmphCharStream charStream)
        {
            toSqlMillisecond(time, charStream.Char + charStream.Length);
            charStream.Unsafer.AddLength(SqlMillisecondSize);
        }

        /// <summary>
        ///     时间转换成字符串(精确到毫秒)
        /// </summary>
        /// <param name="time">时间</param>
        /// <param name="chars">时间字符串</param>
        private static void toSqlMillisecond(DateTime time, char* chars)
        {
            var dayTiks = time.Ticks % DayTiks;
            toString(time, dayTiks, chars);
            chars[19] = '.';
            var data0 = (int)(((ulong)(dayTiks % (1000 * 10000)) * TmphNumber.Div10000Mul) >> TmphNumber.Div10000Shift);
            var data1 = (data0 * (int)TmphNumber.Div10_16Mul) >> TmphNumber.Div10_16Shift;
            data0 -= data1 * 10;
            var data2 = (data1 * (int)TmphNumber.Div10_16Mul) >> TmphNumber.Div10_16Shift;
            data1 -= data2 * 10;
            data0 += '0';
            data1 += '0';
            data2 += '0';
            data1 <<= 16;
            chars[22] = (char)data0;
            *(int*)(chars + 20) = (data2 += data1);
        }

        /// <summary>
        ///     日期转换成整数表示
        /// </summary>
        /// <param name="date">日期</param>
        /// <returns>整数表示</returns>
        public static int toInt(this DateTime date)
        {
            return date != default(DateTime) ? (date.Year << 9) + (date.Month << 5) + date.Day : 0;
        }

        /// <summary>
        ///     当前日期转换成整数表示
        /// </summary>
        /// <returns>整数表示</returns>
        public static int ToInt()
        {
            var date = TmphNowTime.Now;
            return (date.Year << 9) + (date.Month << 5) + date.Day;
        }

        /// <summary>
        ///     整数表示转换成日期
        /// </summary>
        /// <param name="dateInt">整数表示</param>
        /// <returns>日期</returns>
        public static DateTime GetDate(int dateInt)
        {
            var date = default(DateTime);
            try
            {
                int year = dateInt >> 9, month = (dateInt >> 5) & 15, day = dateInt & 31;
                if (year >= 1900 && year < 10000 && month >= 1 && month <= 12 && day >= 1 && day <= 31)
                {
                    date = new DateTime(year, month, day);
                }
            }
            catch
            {
            }
            return date;
        }

        /// <summary>
        ///     时间转字节流
        /// </summary>
        /// <param name="date">时间</param>
        /// <returns>字节流</returns>
        public static byte[] toBytes(this DateTime date)
        {
            var data = ByteBuffers.Get();
            fixed (byte* fixedData = data) ToBytes(date, fixedData);
            return data;
        }

        /// <summary>
        ///     时间转字节流
        /// </summary>
        /// <param name="date">时间</param>
        /// <param name="data">写入数据起始位置</param>
        internal static void ToBytes(DateTime date, byte* data)
        {
            *(int*)data = weekData.Int[(int)date.DayOfWeek];
            int value = date.Day, value10 = (value * (int)TmphNumber.Div10_16Mul) >> TmphNumber.Div10_16Shift;
            *(int*)(data + sizeof(int)) = (' ' + (value10 << 8) + ((value - value10 * 10) << 16) + (' ' << 24)) |
                                            0x303000;
            value = date.Year;
            *(int*)(data + sizeof(int) * 2) = monthData.Int[date.Month - 1];
            value10 = (value * (int)TmphNumber.Div10_16Mul) >> TmphNumber.Div10_16Shift;
            var value100 = (value10 * (int)TmphNumber.Div10_16Mul) >> TmphNumber.Div10_16Shift;
            var value1000 = (value100 * (int)TmphNumber.Div10_16Mul) >> TmphNumber.Div10_16Shift;
            *(int*)(data + sizeof(int) * 3) = (value1000 + ((value100 - value1000 * 10) << 8) +
                                               ((value10 - value100 * 10) << 16) + ((value - value10 * 10) << 24)) |
                                              0x30303030;

            value100 = (int)(date.Ticks % DayTiks / (1000 * 10000));
            value1000 = (int)(((ulong)value100 * Div60_32Mul) >> Div60_32Shift);
            value100 -= value1000 * 60;
            value = (value1000 * (int)Div60_16Mul) >> Div60_16Shift;
            value1000 -= value * 60;

            value10 = (value * (int)TmphNumber.Div10_16Mul) >> TmphNumber.Div10_16Shift;
            *(int*)(data + sizeof(int) * 4) = (' ' + (value10 << 8) + ((value - value10 * 10) << 16) + (':' << 24)) |
                                              0x303000;
            value10 = (value1000 * (int)TmphNumber.Div10_16Mul) >> TmphNumber.Div10_16Shift;
            value = (value100 * (int)TmphNumber.Div10_16Mul) >> TmphNumber.Div10_16Shift;
            *(int*)(data + sizeof(int) * 5) = (value10 + ((value1000 - value10 * 10) << 8) + (':' << 16) + (value << 24)) |
                                              0x30003030;
            *(int*)(data + sizeof(int) * 6) = ((value100 - value * 10) + '0') + (' ' << 8) + ('G' << 16) + ('M' << 24);
            *(data + sizeof(int) * 7) = (byte)'T';
        }

        /// <summary>
        ///     复制时间字节流
        /// </summary>
        /// <param name="dateTime">时间字节流</param>
        /// <returns>时间字节流</returns>
        internal static byte[] CopyBytes(byte[] dateTime)
        {
            if (dateTime == null) return null;
            var data = ByteBuffers.Get(dateTime.Length);
            fixed (byte* dataFixed = data) Unsafe.TmphMemory.Copy(dateTime, dataFixed, dateTime.Length);
            return data;
        }

        /// <summary>
        ///     精确到秒的时间
        /// </summary>
        private static class TmphNowTime
        {
            /// <summary>
            ///     精确到秒的时间
            /// </summary>
            public static DateTime Now;

            /// <summary>
            ///     刷新时间的定时器
            /// </summary>
            public static readonly Timer Timer;

            static TmphNowTime()
            {
                Now = DateTime.Now;
                Timer = new Timer(1000);
                Timer.Elapsed += refreshTime;
                Timer.AutoReset = false;
                Timer.Start();
            }

            /// <summary>
            ///     刷新时间
            /// </summary>
            private static void refreshTime(object sender, ElapsedEventArgs e)
            {
                Now = DateTime.Now;
                Timer.Interval = 1000 - Now.Millisecond;
                Timer.Start();
            }
        }
    }
}