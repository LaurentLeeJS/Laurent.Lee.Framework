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