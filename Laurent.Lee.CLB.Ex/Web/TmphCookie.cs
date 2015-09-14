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

namespace Laurent.Lee.CLB.Web
{
    /// <summary>
    /// COOKIE处理类
    /// </summary>
    public unsafe static class TmphCookie
    {
        /// <summary>
        /// 需要格式化的cookie名称字符集合
        /// </summary>
        public const string FormatCookieNameChars = ",; -\n\r\t";

        /// <summary>
        /// 需要格式化的cookie名称字符位图
        /// </summary>
        private static readonly TmphString.TmphAsciiMap formatCookieNameCharMap = new TmphString.TmphAsciiMap(TmphUnmanaged.Get(TmphString.TmphAsciiMap.MapBytes, true), FormatCookieNameChars, true);

        /// <summary>
        /// 最大cookie名称长度
        /// </summary>
        public const int MaxCookieNameLength = 256;

        /// <summary>
        /// 格式化Cookie名称
        /// </summary>
        /// <param name="name">Cookie名称</param>
        /// <returns>格式化后Cookie名称</returns>
        public unsafe static string FormatCookieName(string name)
        {
            if (name.Length() == 0) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            if (name.Length > MaxCookieNameLength) TmphLog.Error.Throw(null, "cookie名称超过限定 " + ((uint)name.Length).toString(), false);
            fixed (char* nameFixed = name)
            {
                char* endName = nameFixed + name.Length;
                int count = Unsafe.TmphString.AsciiCount(nameFixed, endName, formatCookieNameCharMap.Map, FormatCookieNameChars[0]);
                if (*nameFixed == '$') ++count;
                if (count != 0)
                {
                    string newName = Laurent.Lee.CLB.TmphString.FastAllocateString(count = name.Length + (count << 1));
                    fixed (char* newNameFixed = newName)
                    {
                        char* nextCookieName = newNameFixed;
                        if (*nameFixed == '$')
                        {
                            nextCookieName += 2;
                            *(uint*)newNameFixed = '%' + ('2' << 16);
                            *nextCookieName = '4';
                        }
                        else *nextCookieName = *nameFixed;
                        char* nextName = nameFixed;
                        TmphString.TmphAsciiMap formatMap = formatCookieNameCharMap;
                        while (++nextName != endName)
                        {
                            int nextValue = (int)*nextName;
                            if (formatMap.Get(nextValue))
                            {
                                int highValue = nextValue >> 4;
                                *++nextCookieName = '%';
                                *++nextCookieName = (char)(highValue < 10 ? highValue + '0' : (highValue + ('0' + 'A' - '9' - 1)));
                                nextValue &= 15;
                                *++nextCookieName = (char)(nextValue < 10 ? nextValue + '0' : (nextValue + ('0' + 'A' - '9' - 1)));
                            }
                            else *++nextCookieName = (char)nextValue;
                        }
                        return newName;
                    }
                }
            }
            return name;
        }

        /// <summary>
        /// 格式化Cookie值
        /// </summary>
        /// <param name="value">Cookie值</param>
        /// <returns>格式化后的Cookie值</returns>
        public static string FormatCookieValue(string value)
        {
            return value == null ? string.Empty : value.Replace(",", "%2C").Replace(";", "%3B");
        }
    }
}