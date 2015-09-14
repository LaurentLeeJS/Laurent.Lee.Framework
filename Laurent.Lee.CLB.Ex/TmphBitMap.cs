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

namespace Laurent.Lee.CLB
{
    /// <summary>
    /// 位图
    /// </summary>
    public struct TmphBitMap
    {
        /// <summary>
        /// 非安全访问位图(请自行确保数据可靠性)
        /// </summary>
        public struct TmphUnsafer
        {
            /// <summary>
            /// 位图
            /// </summary>
            private byte[] map;

            /// <summary>
            /// 位图字节数组
            /// </summary>
            public byte[] Map
            {
                get { return map; }
            }

            /// <summary>
            /// 非安全访问位图
            /// </summary>
            /// <param name="map">位图</param>
            public TmphUnsafer(TmphBitMap map)
            {
                this.map = map.map;
            }

            /// <summary>
            /// 设置占位
            /// </summary>
            /// <param name="bit">位值</param>
            public void Set(int bit)
            {
                map[bit >> 3] |= (byte)(1 << (int)(bit &= 7));
            }

            /// <summary>
            /// 清除占位
            /// </summary>
            /// <param name="bit">位值</param>
            public void Clear(int bit)
            {
                map[bit >> 3] &= (byte)(0xff - (1 << (int)(bit &= 7)));
            }

            /// <summary>
            /// 获取占位状态
            /// </summary>
            /// <param name="bit">位值</param>
            /// <returns>是否已占位</returns>
            public bool Get(int bit)
            {
                return (map[bit >> 3] & (byte)(1 << (int)(bit &= 7))) != 0;
            }

            /// <summary>
            /// 设置占位段
            /// </summary>
            /// <param name="start">位值</param>
            /// <param name="count">段长</param>
            public unsafe void Set(int start, int count)
            {
                Laurent.Lee.CLB.Unsafe.TmphMemory.FillBits(map, start, count);
            }

            /// <summary>
            /// 清除占位段
            /// </summary>
            /// <param name="start">位值</param>
            /// <param name="count">段长</param>
            public unsafe void Clear(int start, int count)
            {
                Laurent.Lee.CLB.Unsafe.TmphMemory.ClearBits(map, start, count);
            }
        }

        /// <summary>
        /// 非安全访问位图
        /// </summary>
        public TmphUnsafer Unsafer
        {
            get
            {
                return new TmphUnsafer(this);
            }
        }

        /// <summary>
        /// 位图字节数组
        /// </summary>
        private byte[] map;

        /// <summary>
        /// 最大值
        /// </summary>
        private uint size;

        /// <summary>
        /// 位图
        /// </summary>
        /// <param name="size">位图尺寸</param>
        public TmphBitMap(int size)
        {
            this.size = size <= 0 ? 0 : ((uint)size + 7) >> 3;
            map = this.size > 0 ? new byte[this.size] : TmphNullValue<byte>.Array;
        }

        /// <summary>
        /// 设置占位
        /// </summary>
        /// <param name="bit">位值</param>
        public void Set(int bit)
        {
            if ((uint)bit < size) map[bit >> 3] |= (byte)(1 << (int)(bit &= 7));
        }

        /// <summary>
        /// 清除占位
        /// </summary>
        /// <param name="bit">位值</param>
        public void Clear(int bit)
        {
            if ((uint)bit < size) map[bit >> 3] &= (byte)(0xff - (1 << (int)(bit &= 7)));
        }

        /// <summary>
        /// 获取占位状态
        /// </summary>
        /// <param name="bit">位值</param>
        /// <returns>是否已占位</returns>
        public bool Get(int bit)
        {
            return (uint)bit < size && (map[bit >> 3] & (byte)(1 << (int)(bit &= 7))) != 0;
        }

        /// <summary>
        /// 设置占位段
        /// </summary>
        /// <param name="start">位值</param>
        /// <param name="count">段长</param>
        public unsafe void Set(int start, int count)
        {
            if (start < 0)
            {
                count += start;
                start = 0;
            }
            if ((uint)start < size && count > 0)
            {
                if ((uint)start + count > size) count = (int)size - start;
                Laurent.Lee.CLB.Unsafe.TmphMemory.FillBits(map, start, count);
            }
        }

        /// <summary>
        /// 清除占位段
        /// </summary>
        /// <param name="start">位值</param>
        /// <param name="count">段长</param>
        public unsafe void Clear(int start, int count)
        {
            if (start < 0)
            {
                count += start;
                start = 0;
            }
            if ((uint)start < size && count > 0)
            {
                if ((uint)start + count > size) count = (int)size - start;
                Laurent.Lee.CLB.Unsafe.TmphMemory.ClearBits(map, start, count);
            }
        }
    }

    /// <summary>
    /// 枚举位图
    /// </summary>
    /// <typeparam name="TEnumType">枚举类型</typeparam>
    public struct TmphBitMap<TEnumType> where TEnumType : IConvertible
    {
        /// <summary>
        /// 最大值
        /// </summary>
        private static readonly uint size = (uint)Laurent.Lee.CLB.TmphEnum.GetMaxValue<TEnumType>(-1) + 1;

        /// <summary>
        /// 非安全访问枚举位图(请自行确保数据可靠性)
        /// </summary>
        public struct TmphUnsafer
        {
            /// <summary>
            /// 枚举位图
            /// </summary>
            private TmphBitMap<TEnumType> map;

            /// <summary>
            /// 位图字节数组
            /// </summary>
            public byte[] Map
            {
                get { return map.map; }
            }

            /// <summary>
            /// 非安全访问枚举位图
            /// </summary>
            /// <param name="map">枚举位图</param>
            public TmphUnsafer(TmphBitMap<TEnumType> map)
            {
                this.map = map;
            }

            /// <summary>
            /// 设置占位
            /// </summary>
            /// <param name="bit">位值</param>
            public void Set(int bit)
            {
                map.map[bit >> 3] |= (byte)(1 << (int)(bit &= 7));
            }

            /// <summary>
            /// 设置占位
            /// </summary>
            /// <param name="value">位值</param>
            public void Set(TEnumType value)
            {
                Set(value.ToInt32(null));
            }

            /// <summary>
            /// 清除占位
            /// </summary>
            /// <param name="bit">位值</param>
            public void Clear(int bit)
            {
                map.map[bit >> 3] &= (byte)(0xff - (1 << (int)(bit &= 7)));
            }

            /// <summary>
            /// 设置占位
            /// </summary>
            /// <param name="value">位值</param>
            public void Clear(TEnumType value)
            {
                Clear(value.ToInt32(null));
            }

            /// <summary>
            /// 获取占位状态
            /// </summary>
            /// <param name="bit">位值</param>
            /// <returns>是否已占位</returns>
            public bool Get(int bit)
            {
                return (map.map[bit >> 3] & (byte)(1 << (int)(bit &= 7))) != 0;
            }

            /// <summary>
            /// 获取占位状态
            /// </summary>
            /// <param name="bit">位值</param>
            /// <returns>是否已占位</returns>
            public bool Get(TEnumType value)
            {
                return Get(value.ToInt32(null));
            }
        }

        /// <summary>
        /// 非安全访问枚举位图
        /// </summary>
        public TmphUnsafer Unsafer
        {
            get { return new TmphUnsafer(this); }
        }

        /// <summary>
        /// 位图字节数组
        /// </summary>
        private byte[] map;

        /// <summary>
        /// 枚举位图
        /// </summary>
        /// <param name="map">位图字节数组</param>
        public TmphBitMap(byte[] map)
        {
            this.map = map ?? new byte[(size + 7) >> 3];
        }

        /// <summary>
        /// 设置占位
        /// </summary>
        /// <param name="bit">位值</param>
        public void Set(int bit)
        {
            if ((uint)bit < size) map[bit >> 3] |= (byte)(1 << (int)(bit &= 7));
        }

        /// <summary>
        /// 设置占位
        /// </summary>
        /// <param name="value">位值</param>
        public void Set(TEnumType value)
        {
            Set(value.ToInt32(null));
        }

        /// <summary>
        /// 清除占位
        /// </summary>
        /// <param name="bit">位值</param>
        public void Clear(int bit)
        {
            if ((uint)bit < size) map[bit >> 3] &= (byte)(0xff - (1 << (int)(bit &= 7)));
        }

        /// <summary>
        /// 设置占位
        /// </summary>
        /// <param name="value">位值</param>
        public void Clear(TEnumType value)
        {
            Clear(value.ToInt32(null));
        }

        /// <summary>
        /// 获取占位状态
        /// </summary>
        /// <param name="bit">位值</param>
        /// <returns>是否已占位</returns>
        public bool Get(int bit)
        {
            return (uint)bit < size && (map[bit >> 3] & (byte)(1 << (int)(bit &= 7))) != 0;
        }

        /// <summary>
        /// 获取占位状态
        /// </summary>
        /// <param name="bit">位值</param>
        /// <returns>是否已占位</returns>
        public bool Get(TEnumType value)
        {
            return Get(value.ToInt32(null));
        }
    }
}