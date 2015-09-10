using System.Collections.Generic;

namespace Laurent.Lee.CLB
{
    /// <summary>
    ///     HASH表扩展操作
    /// </summary>
    public static class TmphHashSet
    {
        /// <summary>
        ///     创建HASH表
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <returns>HASH表</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static HashSet<TValueType> CreateOnly<TValueType>() where TValueType : class
        {
            return new HashSet<TValueType>();
        }

        /// <summary>
        ///     创建HASH表
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <returns>HASH表</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static HashSet<TmphPointer> CreatePointer()
        {
#if MONO
            return new HashSet<pointer>(equalityComparer.Pointer);
#else
            return new HashSet<TmphPointer>();
#endif
        }

        //        /// <summary>
        //        /// 创建HASH表
        //        /// </summary>
        //        /// <typeparam name="TValueType">数据类型</typeparam>
        //        /// <returns>HASH表</returns>
        //        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        //        public static HashSet<subString> CreateSubString()
        //        {
        //#if MONO
        //            return new HashSet<subString>(equalityComparer.SubString);
        //#else
        //            return new HashSet<subString>();
        //#endif
        //        }
        /// <summary>
        ///     创建HASH表
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <returns>HASH表</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static HashSet<TmphHashString> CreateHashString()
        {
#if MONO
            return new HashSet<hashString>(equalityComparer.HashString);
#else
            return new HashSet<TmphHashString>();
#endif
        }
    }

    /// <summary>
    ///     HASH表扩展操作
    /// </summary>
    /// <typeparam name="TValueType">数据类型</typeparam>
    public static class TmphHashSet<TValueType>
    {
        /// <summary>
        ///     是否值类型
        /// </summary>
        private static readonly bool isValueType = typeof(TValueType).IsValueType;

        /// <summary>
        ///     创建字典
        /// </summary>
        /// <typeparam name="TKeyType">关键字类型</typeparam>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <returns>字典</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static HashSet<TValueType> Create()
        {
#if MONO
            if (isValueType) return new HashSet<TKeyType, TValueType>(equalityComparer.comparer<TValueType>.Default);
#endif
            return new HashSet<TValueType>();
        }
    }
}