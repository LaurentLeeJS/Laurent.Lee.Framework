using System;
using System.Collections.Generic;

namespace Laurent.Lee.CLB
{
    /// <summary>
    ///     字典扩展操作
    /// </summary>
    public static class TmphDictionary
    {
        /// <summary>
        ///     创建字典
        /// </summary>
        /// <typeparam name="TKeyType">关键字类型</typeparam>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <returns>字典</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static Dictionary<TKeyType, TValueType> CreateOnly<TKeyType, TValueType>()
            where TKeyType : class
        {
            return new Dictionary<TKeyType, TValueType>();
        }

        /// <summary>
        ///     创建字典
        /// </summary>
        /// <typeparam name="TKeyType">关键字类型</typeparam>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="capacity">初始化容器尺寸</param>
        /// <returns>字典</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static Dictionary<TKeyType, TValueType> CreateOnly<TKeyType, TValueType>(int capacity)
            where TKeyType : class
        {
            return new Dictionary<TKeyType, TValueType>(capacity);
        }

        /// <summary>
        ///     创建字典
        /// </summary>
        /// <typeparam name="TKeyType">关键字类型</typeparam>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <returns>字典</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static Dictionary<TKeyType, TValueType> Create<TKeyType, TValueType>()
            where TKeyType : struct, IEquatable<TKeyType>
        {
#if MONO
            return new Dictionary<TKeyType, TValueType>(equalityComparer.comparer<TKeyType>.Default);
#else
            return new Dictionary<TKeyType, TValueType>();
#endif
        }

        /// <summary>
        ///     创建字典
        /// </summary>
        /// <typeparam name="TKeyType">关键字类型</typeparam>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="capacity">初始化容器尺寸</param>
        /// <returns>字典</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static Dictionary<TKeyType, TValueType> CreateAny<TKeyType, TValueType>(int capacity)
        {
#if MONO
            return new Dictionary<TKeyType, TValueType>(capacity, equalityComparer.comparer<TKeyType>.Default);
#else
            return new Dictionary<TKeyType, TValueType>(capacity);
#endif
        }

        /// <summary>
        ///     创建字典
        /// </summary>
        /// <typeparam name="TKeyType">关键字类型</typeparam>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <returns>字典</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static Dictionary<TKeyType, TValueType> CreateAny<TKeyType, TValueType>()
        {
#if MONO
            return new Dictionary<TKeyType, TValueType>(equalityComparer.comparer<TKeyType>.Default);
#else
            return new Dictionary<TKeyType, TValueType>();
#endif
        }

        /// <summary>
        ///     创建字典
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <returns>字典</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static Dictionary<short, TValueType> CreateShort<TValueType>()
        {
#if MONO
            return new Dictionary<short, TValueType>(equalityComparer.Short);
#else
            return new Dictionary<short, TValueType>();
#endif
        }

        /// <summary>
        ///     创建字典
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <returns>字典</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static Dictionary<int, TValueType> CreateInt<TValueType>()
        {
#if MONO
            return new Dictionary<int, TValueType>(equalityComparer.Int);
#else
            return new Dictionary<int, TValueType>();
#endif
        }

        /// <summary>
        ///     创建字典
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="capacity">初始化容器尺寸</param>
        /// <returns>字典</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static Dictionary<int, TValueType> CreateInt<TValueType>(int capacity)
        {
#if MONO
            return new Dictionary<int, TValueType>(capacity, equalityComparer.Int);
#else
            return new Dictionary<int, TValueType>(capacity);
#endif
        }

        /// <summary>
        ///     创建字典
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <returns>字典</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static Dictionary<ulong, TValueType> CreateULong<TValueType>()
        {
#if MONO
            return new Dictionary<ulong, TValueType>(equalityComparer.ULong);
#else
            return new Dictionary<ulong, TValueType>();
#endif
        }

        /// <summary>
        ///     创建字典
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <returns>字典</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static Dictionary<TmphUint128, TValueType> CreateUInt128<TValueType>()
        {
#if MONO
            return new Dictionary<uint128, TValueType>(equalityComparer.UInt128);
#else
            return new Dictionary<TmphUint128, TValueType>();
#endif
        }

        /// <summary>
        ///     创建字典
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <returns>字典</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static Dictionary<TmphPointer, TValueType> CreatePointer<TValueType>()
        {
#if MONO
            return new Dictionary<pointer, TValueType>(equalityComparer.Pointer);
#else
            return new Dictionary<TmphPointer, TValueType>();
#endif
        }

        //        /// <summary>
        //        /// 创建字典
        //        /// </summary>
        //        /// <typeparam name="TValueType">数据类型</typeparam>
        //        /// <returns>字典</returns>
        //        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        //        public static Dictionary<subString, TValueType> CreateSubString<TValueType>()
        //        {
        //#if MONO
        //            return new Dictionary<subString, TValueType>(equalityComparer.SubString);
        //#else
        //            return new Dictionary<subString, TValueType>();
        //#endif
        //        }
        /// <summary>
        ///     创建字典
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <returns>字典</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static Dictionary<TmphHashBytes, TValueType> CreateHashBytes<TValueType>()
        {
#if MONO
            return new Dictionary<hashBytes, TValueType>(equalityComparer.HashBytes);
#else
            return new Dictionary<TmphHashBytes, TValueType>();
#endif
        }

        /// <summary>
        ///     创建字典
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <returns>字典</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static Dictionary<TmphHashString, TValueType> CreateHashString<TValueType>()
        {
#if MONO
            return new Dictionary<hashBytes, TValueType>(equalityComparer.HashString);
#else
            return new Dictionary<TmphHashString, TValueType>();
#endif
        }
    }

    /// <summary>
    ///     字典扩展操作
    /// </summary>
    /// <typeparam name="TKeyType">关键字类型</typeparam>
    public static class TmphDictionary<TKeyType> where TKeyType : IEquatable<TKeyType>
    {
        /// <summary>
        ///     是否值类型
        /// </summary>
        private static readonly bool isValueType = typeof(TKeyType).IsValueType;

        /// <summary>
        ///     创建字典
        /// </summary>
        /// <typeparam name="TKeyType">关键字类型</typeparam>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <returns>字典</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static Dictionary<TKeyType, TValueType> Create<TValueType>()
        {
#if MONO
            if (isValueType) return new Dictionary<TKeyType, TValueType>(equalityComparer.comparer<TKeyType>.Default);
#endif
            return new Dictionary<TKeyType, TValueType>();
        }

        /// <summary>
        ///     创建字典
        /// </summary>
        /// <typeparam name="TKeyType">关键字类型</typeparam>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="capacity">初始化容器尺寸</param>
        /// <returns>字典</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static Dictionary<TKeyType, TValueType> Create<TValueType>(int capacity)
        {
#if MONO
            if (isValueType) return new Dictionary<TKeyType, TValueType>(capacity, equalityComparer.comparer<TKeyType>.Default);
#endif
            return new Dictionary<TKeyType, TValueType>(capacity);
        }
    }
}