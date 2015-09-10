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