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
    ///     域名参数及其相关操作
    /// </summary>
    public static class TmphDomain
    {
        /// <summary>
        ///     顶级域名集合
        /// </summary>
        private static readonly TmphUniqueHashSet<TmphTopDomain> TopDomains =
            new TmphUniqueHashSet<TmphTopDomain>(
                new TmphTopDomain[]
                {
                    "arpa", "com", "edu", "gov", "int", "mil", "net", "org", "biz", "name", "info", "pro", "museum", "aero",
                    "coop"
                }, 30);

        /// <summary>
        ///     根据URL地址获取主域名
        /// </summary>
        /// <param name="url">URL地址</param>
        /// <returns>主域名</returns>
        public static unsafe TmphSubArray<byte> GetMainDomainByUrl(TmphSubArray<byte> url)
        {
            if (url.Count != 0)
            {
                fixed (byte* urlFixed = url.Array)
                {
                    var urlStart = urlFixed + url.StartIndex;
                    return
                        new TmphDomainParser { Data = url.Array, DataFixed = urlFixed, DataEnd = urlStart + url.Count }
                            .GetMainDomainByUrl(urlStart);
                }
            }
            return default(TmphSubArray<byte>);
        }

        /// <summary>
        ///     根据域名获取主域名
        /// </summary>
        /// <param name="domain">域名</param>
        /// <returns>主域名</returns>
        public static unsafe TmphSubArray<byte> GetMainDomain(TmphSubArray<byte> domain)
        {
            if (domain.Count != 0)
            {
                fixed (byte* domainFixed = domain.Array)
                {
                    var domainStart = domainFixed + domain.StartIndex;
                    return
                        new TmphDomainParser
                        {
                            Data = domain.Array,
                            DataFixed = domainFixed,
                            DataEnd = domainStart + domain.Count
                        }.GetMainDomain(domainStart);
                }
            }
            return default(TmphSubArray<byte>);
        }

        /// <summary>
        ///     顶级域名唯一哈希
        /// </summary>
        private struct TmphTopDomain : IEquatable<TmphTopDomain>
        {
            /// <summary>
            ///     顶级域名
            /// </summary>
            public TmphSubArray<byte> Name;

            /// <summary>
            ///     判断是否相等
            /// </summary>
            /// <param name="other">待匹配数据</param>
            /// <returns>是否相等</returns>
            public bool Equals(TmphTopDomain other)
            {
                return Name.equal(other.Name);
            }

            /// <summary>
            ///     隐式转换
            /// </summary>
            /// <param name="name">顶级域名</param>
            /// <returns>顶级域名唯一哈希</returns>
            public static implicit operator TmphTopDomain(string name)
            {
                return TmphSubArray<byte>.Unsafe(name.GetBytes(), 0, name.Length);
            }

            /// <summary>
            ///     隐式转换
            /// </summary>
            /// <param name="name">顶级域名</param>
            /// <returns>顶级域名唯一哈希</returns>
            public static implicit operator TmphTopDomain(TmphSubArray<byte> name)
            {
                return new TmphTopDomain { Name = name };
            }

            /// <summary>
            ///     获取哈希值
            /// </summary>
            /// <returns>哈希值</returns>
            public override int GetHashCode()
            {
                if (Name.Count < 3) return 0;
                var key = Name.Array;
                var code = (uint)(key[Name.StartIndex] << 8) + key[Name.StartIndex + 2];
                return (int)(((code >> 4) ^ code) & ((1U << 5) - 1));
            }

            /// <summary>
            ///     判断是否相等
            /// </summary>
            /// <param name="obj">待匹配数据</param>
            /// <returns>是否相等</returns>
            public override bool Equals(object obj)
            {
                return Equals((TmphTopDomain)obj);
            }
        }

        /// <summary>
        ///     域名分解器
        /// </summary>
        private unsafe struct TmphDomainParser
        {
            /// <summary>
            ///     域名数据
            /// </summary>
            public byte[] Data;

            /// <summary>
            ///     域名数据结束
            /// </summary>
            public byte* DataEnd;

            /// <summary>
            ///     域名数据
            /// </summary>
            public byte* DataFixed;

            /// <summary>
            ///     根据URL地址获取主域名
            /// </summary>
            /// <param name="start">URL起始位置</param>
            /// <returns>主域名</returns>
            public TmphSubArray<byte> GetMainDomainByUrl(byte* start)
            {
                byte* end = DataEnd, domain = Unsafe.TmphMemory.Find(start, end, (byte)':');
                if (domain != null && *(short*)++domain == '/' + ('/' << 8) && (domain += sizeof(short)) < end)
                {
                    var next = Unsafe.TmphMemory.Find(domain, end, (byte)'/');
                    if (next == null) return GetMainDomain(domain, end);
                    if (domain != next) return GetMainDomain(domain, next);
                }
                return default(TmphSubArray<byte>);
            }

            /// <summary>
            ///     获取主域名
            /// </summary>
            /// <param name="start">域名起始位置</param>
            /// <returns>主域名</returns>
            public TmphSubArray<byte> GetMainDomain(byte* start)
            {
                return GetMainDomain(start, DataEnd);
            }

            /// <summary>
            ///     获取主域名
            /// </summary>
            /// <param name="domain">域名起始位置</param>
            /// <param name="end">域名结束位置</param>
            /// <returns>主域名</returns>
            private TmphSubArray<byte> GetMainDomain(byte* domain, byte* end)
            {
                var next = Unsafe.TmphMemory.Find(domain, end, (byte)':');
                if (next != null) end = next;
                if (domain != end)
                {
                    for (next = domain; next != end; ++next)
                    {
                        if (((uint)(*next - '0') >= 10 && *next != '.'))
                        {
                            var dot1 = Unsafe.TmphMemory.FindLast(domain, end, (byte)'.');
                            if (dot1 != null && domain != dot1)
                            {
                                var dot2 = Unsafe.TmphMemory.FindLast(domain, dot1, (byte)'.');
                                if (dot2 != null)
                                {
                                    if (TopDomains.Contains(TmphSubArray<byte>.Unsafe(Data, (int)(dot1 - DataFixed) + 1,
                                        (int)(end - dot1) - 1))
                                        ||
                                        !TopDomains.Contains(TmphSubArray<byte>.Unsafe(Data, (int)(dot2 - DataFixed) + 1,
                                            (int)(dot1 - dot2) - 1)))
                                    {
                                        domain = dot2 + 1;
                                    }
                                    else if (domain != dot2 &&
                                             (dot1 = Unsafe.TmphMemory.FindLast(domain, dot2, (byte)'.')) != null)
                                    {
                                        domain = dot1 + 1;
                                    }
                                }
                            }
                            break;
                        }
                    }
                    return TmphSubArray<byte>.Unsafe(Data, (int)(domain - DataFixed), (int)(end - domain));
                }
                return default(TmphSubArray<byte>);
            }
        }
    }
}