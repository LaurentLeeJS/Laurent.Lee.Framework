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

using Laurent.Lee.CLB.Threading;
using System;
using System.Reflection;
using System.Threading;

namespace Laurent.Lee.CLB
{
    /// <summary>
    ///     随机数
    /// </summary>
    public sealed unsafe class TmphRandom
    {
        /// <summary>
        ///     默认随机数
        /// </summary>
        public static TmphRandom Default = new TmphRandom();

        /// <summary>
        ///     随机Hash值
        /// </summary>
        internal static readonly int Hash = Default.Next();

        /// <summary>
        ///     安全种子
        /// </summary>
        private readonly uint* secureSeeds;

        /// <summary>
        ///     公用种子
        /// </summary>
        private readonly uint* seeds;

        /// <summary>
        ///     随机位缓存数量
        /// </summary>
        private int bitCount;

        /// <summary>
        ///     随机位缓存
        /// </summary>
        private uint bits;

        /// <summary>
        ///     字节缓存数量
        /// </summary>
        private int byteCount;

        /// <summary>
        ///     字节缓存访问锁
        /// </summary>
        private int byteLock;

        /// <summary>
        ///     字节缓存
        /// </summary>
        private ulong bytes;

        /// <summary>
        ///     32位种子位置
        /// </summary>
        private int current;

        /// <summary>
        ///     64位种子位置
        /// </summary>
        private int current64;

        /// <summary>
        ///     64位种子位置访问锁
        /// </summary>
        private int currentLock;

        /// <summary>
        ///     双字节缓存数量
        /// </summary>
        private int ushortCount;

        /// <summary>
        ///     双字节缓存访问锁
        /// </summary>
        private int ushortLock;

        /// <summary>
        ///     双字节缓存
        /// </summary>
        private ulong ushorts;

        /// <summary>
        ///     随机数
        /// </summary>
        private TmphRandom()
        {
            secureSeeds = TmphUnmanaged.Get(64 * sizeof(uint) + 5 * 11 * sizeof(uint), false).UInt;
            seeds = secureSeeds + 64;
            current64 = 5 * 11 - 2;
            var tick = (ulong)TmphPub.StartTime.Ticks ^ (ulong)Environment.TickCount ^ ((ulong)TmphPub.Identity32 << 8) ^
                       ((ulong)TmphDate.NowTimerInterval << 24);
            var isSeedArray = 0;
            var seedField = typeof(Random).GetField("SeedArray", BindingFlags.Instance | BindingFlags.NonPublic);
            if (seedField != null)
            {
                var seedArray = seedField.GetValue(new Random()) as int[];
                if (seedArray != null && seedArray.Length == 5 * 11 + 1)
                {
                    tick *= 0xb163dUL;
                    fixed (int* seedFixed = seedArray)
                    {
                        for (uint* write = seeds, end = seeds + 5 * 11, read = (uint*)seedFixed;
                            write != end;
                            tick >>= 1)
                        {
                            *write++ = *++read ^ (((uint)tick & 1U) << 31);
                        }
                    }
                    isSeedArray = 1;
                }
            }
            if (isSeedArray == 0)
            {
                TmphLog.Default.Add("系统随机数种子获取失败", false, false);
                for (uint* start = seeds, end = start + 5 * 11; start != end; ++start)
                {
                    *start = (uint)tick ^ (uint)(tick >> 32);
                    tick *= 0xb163dUL;
                    tick += tick >> 32;
                }
            }
            for (var start = (ulong*)secureSeeds; start != seeds; *start++ = NextULong()) ;
            bits = (uint)Next();
            bitCount = 32;
        }

        /// <summary>
        ///     获取随机种子位置
        /// </summary>
        /// <returns></returns>
        private int nextIndex()
        {
            var index = Interlocked.Increment(ref current);
            if (index >= 5 * 11)
            {
                var cacheIndex = index;
                do
                {
                    index -= 5 * 11;
                } while (index >= 5 * 11);
                Interlocked.CompareExchange(ref current, index, cacheIndex);
            }
            return index;
        }

        /// <summary>
        ///     获取下一个随机数
        /// </summary>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public int Next()
        {
            var index = nextIndex();
            var seed = seeds + index;
            if (index < (5 * 11 - 3 * 7)) return (int)(*seed -= *(seed + 3 * 7));
            return (int)(*seed ^= *(seed - (5 * 11 - 3 * 7)));
        }

        /// <summary>
        ///     获取下一个随机数
        /// </summary>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public float NextFloat()
        {
            var index = nextIndex();
            var seed = seeds + index;
            if (index < (5 * 11 - 3 * 7)) *seed -= *(seed + 3 * 7);
            else *seed ^= *(seed - (5 * 11 - 3 * 7));
            return *(float*)seed;
        }

        /// <summary>
        ///     获取下一个随机数
        /// </summary>
        /// <param name="mod">求余取模数</param>
        /// <returns></returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public int Next(int mod)
        {
            if (mod <= 1) return 0;
            var value = Next() % mod;
            return value >= 0 ? value : (value + mod);
        }

        /// <summary>
        ///     获取下一个随机位
        /// </summary>
        /// <returns></returns>
        public uint NextBit()
        {
            var count = Interlocked.Decrement(ref bitCount);
            while (count < 0)
            {
                Thread.Sleep(0);
                count = Interlocked.Decrement(ref bitCount);
            }
            if (count == 0)
            {
                var value = bits & 1;
                bits = (uint)Next();
                bitCount = 32;
                return value;
            }
            return bits & (1U << count);
        }

        /// <summary>
        ///     获取下一个随机字节
        /// </summary>
        /// <returns></returns>
        public byte NextByte()
        {
            START:
            TmphInterlocked.NoCheckCompareSetSleep0(ref byteLock);
            if (byteCount == 0)
            {
                byteCount = -1;
                byteLock = 0;
                var value = (byte)(bytes = NextULong());
                bytes >>= 8;
                TmphInterlocked.NoCheckCompareSetSleep0(ref byteLock);
                byteCount = 7;
                byteLock = 0;
                return value;
            }
            if (byteCount > 0)
            {
                var value = (byte)bytes;
                --byteCount;
                bytes >>= 8;
                byteLock = 0;
                return value;
            }
            byteLock = 0;
            Thread.Sleep(0);
            goto START;
        }

        /// <summary>
        ///     获取下一个随机双字节
        /// </summary>
        /// <returns></returns>
        public ushort NextUShort()
        {
            START:
            TmphInterlocked.NoCheckCompareSetSleep0(ref ushortLock);
            if (ushortCount == 0)
            {
                ushortLock = 0;
                var value = (ushort)(ushorts = NextULong());
                ushorts >>= 16;
                TmphInterlocked.NoCheckCompareSetSleep0(ref ushortLock);
                ushortCount = 3;
                ushortLock = 0;
                return value;
            }
            if (ushortCount > 0)
            {
                var value = (ushort)ushorts;
                --ushortCount;
                ushorts >>= 16;
                ushortLock = 0;
                return value;
            }
            ushortLock = 0;
            Thread.Sleep(0);
            goto START;
        }

        /// <summary>
        ///     获取随机种子位置
        /// </summary>
        /// <returns></returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private int nextIndex64()
        {
            TmphInterlocked.NoCheckCompareSetSleep0(ref currentLock);
            var index = current64;
            if ((current64 -= 2) < 0) current64 = (5 * 11 - 4) - current64;
            currentLock = 0;
            return index;
        }

        /// <summary>
        ///     获取下一个随机数
        /// </summary>
        public ulong NextULong()
        {
            var index = nextIndex64();
            var seed = seeds + index;
            if (index < (5 * 11 - 3 * 7 - 1)) return *(ulong*)seed -= *(ulong*)(seed + 3 * 7);
            if (index == (5 * 11 - 3 * 7 - 1)) return *(ulong*)seed -= *(ulong*)seeds;
            return *(ulong*)seed ^= *(ulong*)(seed - (5 * 11 - 3 * 7));
        }

        /// <summary>
        ///     获取下一个随机数
        /// </summary>
        public double NextDouble()
        {
            var index = nextIndex64();
            var seed = seeds + index;
            if (index < (5 * 11 - 3 * 7 - 1)) *(ulong*)seed -= *(ulong*)(seed + 3 * 7);
            else if (index == (5 * 11 - 3 * 7 - 1)) *(ulong*)seed -= *(ulong*)seeds;
            else *(ulong*)seed ^= *(ulong*)(seed - (5 * 11 - 3 * 7));
            return *(double*)seed;
        }

        /// <summary>
        ///     获取下一个随机数
        /// </summary>
        public int SecureNext()
        {
            int seed = Next(), leftIndex = seed & 63, rightIndex = (seed >> 6) & 63;
            if (leftIndex == rightIndex) return (int)((secureSeeds[leftIndex] ^= (uint)seed) - (uint)seed);
            if ((seed & (1 << ((seed >> 12) & 31))) == 0)
                return (int)((secureSeeds[leftIndex] -= secureSeeds[rightIndex]) ^ (uint)seed);
            return (int)((secureSeeds[leftIndex] ^= secureSeeds[rightIndex]) - (uint)seed);
        }

        /// <summary>
        ///     获取下一个随机数
        /// </summary>
        public ulong SecureNextULong()
        {
            var seed = NextULong();
            int leftIndex = (int)(uint)seed & 63, rightIndex = (int)((uint)seed >> 6) & 63;
            if (leftIndex == 63) leftIndex = 62;
            if (rightIndex == 63) rightIndex = 62;
            if (leftIndex == rightIndex) return (*(ulong*)(secureSeeds + leftIndex) ^= seed) - seed;
            if (((uint)seed & (1U << ((int)((uint)seed >> 12) & 31))) == 0)
                return (*(ulong*)(secureSeeds + leftIndex) -= *(ulong*)(secureSeeds + rightIndex)) ^ seed;
            return (*(ulong*)(secureSeeds + leftIndex) ^= *(ulong*)(secureSeeds + rightIndex)) - seed;
        }

        /// <summary>
        ///     获取下一个非0随机数
        /// </summary>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public ulong SecureNextULongNotZero()
        {
            var value = SecureNextULong();
            while (value == 0) value = SecureNextULong();
            return value;
        }
    }
}