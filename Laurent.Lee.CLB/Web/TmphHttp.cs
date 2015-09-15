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

namespace Laurent.Lee.CLB.Web
{
    /// <summary>
    ///     HTTP参数及其相关操作
    /// </summary>
    public static class TmphHttp
    {
        /// <summary>
        ///     查询模式类别
        /// </summary>
        public enum TmphMethodType : byte
        {
            None = 0,

            /// <summary>
            ///     请求获取Request-URI所标识的资源
            /// </summary>
            GET,

            /// <summary>
            ///     在Request-URI所标识的资源后附加新的数据
            /// </summary>
            POST,

            /// <summary>
            ///     请求获取由Request-URI所标识的资源的响应消息报头
            /// </summary>
            HEAD,

            /// <summary>
            ///     请求服务器存储一个资源，并用Request-URI作为其标识
            /// </summary>
            PUT,

            /// <summary>
            ///     请求服务器删除Request-URI所标识的资源
            /// </summary>
            DELETE,

            /// <summary>
            ///     请求服务器回送收到的请求信息，主要用于测试或诊断
            /// </summary>
            TRACE,

            /// <summary>
            ///     保留将来使用
            /// </summary>
            CONNECT,

            /// <summary>
            ///     请求查询服务器的性能，或者查询与资源相关的选项和需求
            /// </summary>
            OPTIONS
        }

        /// <summary>
        ///     查询模式类型集合
        /// </summary>
        private static readonly TmphMethodType[] uniqueTypes;

        static unsafe TmphHttp()
        {
            uniqueTypes = new TmphMethodType[1 << 4];
            uint code;
            var methodBufferFixed = (byte*)&code;
            foreach (TmphMethodType method in Enum.GetValues(typeof(TmphMethodType)))
            {
                if (method != TmphMethodType.None)
                {
                    var methodString = method.ToString();
                    fixed (char* methodFixed = methodString)
                    {
                        byte* write = methodBufferFixed, end = methodBufferFixed;
                        if (methodString.Length >= sizeof(int)) end += sizeof(int);
                        else
                        {
                            code = 0x20202020U;
                            end += methodString.Length;
                        }
                        for (var read = methodFixed; write != end; *write++ = (byte)*read++) ;
                        uniqueTypes[((code >> 12) ^ code) & ((1U << 4) - 1)] = method;
                    }
                }
            }
        }

        /// <summary>
        ///     查询模式字节转枚举
        /// </summary>
        /// <param name="method">查询模式</param>
        /// <returns>查询模式枚举</returns>
        internal static unsafe TmphMethodType GetMethod(byte* method)
        {
            var code = *(uint*)method;
            return uniqueTypes[((code >> 12) ^ code) & ((1U << 4) - 1)];
        }
    }
}