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